# Plan: Make Growth phase step-wise and serializable

## Context

`Spirit.DoGrowth(GameState)` (`SpiritIsland/SpiritParts/Spirit.cs:106-109`) currently does the whole
Growth phase in one uninterrupted call: `new DoGrowthClass(this, gameState).Execute()` runs an internal
`while` loop that repeatedly selects and resolves growth actions, using three **private, transient fields**
on the throwaway `DoGrowthClass` instance (`_availableNewGrowthOptions`, `_growthOptions`,
`_shouldInitNewGrowthOption`) to track where it is mid-phase.

That's a problem for serialization: per `docs/GameSerialization-Roadmap.md`'s documented design constraint,
save/restore can only happen **at action boundaries**, never mid-decision, because a suspended
`Spirit.Select(...)` `Task` can't be serialized. Today's `Execute()` loop makes several `Select` calls
back-to-back inside one C# call - there's no boundary between them a save driver could land on, and even if
there were, the "which `GrowthGroup`s are used" state lives in a local variable, not on `Spirit` or
`GrowthTrack`, so it wouldn't survive a round-trip anyway.

Investigation confirmed:
- `GrowthTrack`/`PickGroups`/`GrowthGroup.Used` (added in the prior session) is **not currently serialized
  at all** - `Spirit.ToJson`'s doc comment (`Spirit.cs:516-520`) explicitly still calls `GrowthTrack`
  "deliberately NOT captured... re-running [aspect] setup reproduces them" - true for the `Groups`
  structure itself, no longer true for the `Used` flag, which is genuine per-round runtime state.
- All three of `DoGrowthClass`'s private fields are fully **derivable from state that's already
  persisted** (`Spirit.Energy`, `Spirit._availableActions` via `GetAvailableActions(Phase.Growth)`, and
  `GrowthTrack.RemainingOptions(Energy)`) - none of them need a new place to live. Specifically:
  - `_shouldInitNewGrowthOption` is always exactly `!GetAvailableActions(Phase.Growth).Any()` at the
    moment of selection (verified by tracing every place the old code set/read it).
  - `_availableNewGrowthOptions`/`_growthOptions` are always exactly
    `GrowthTrack.RemainingOptions(Energy)` - a pure function of already-serialized state.
- So the *only* genuinely new state to persist is `GrowthGroup.Used` itself. Once that's serialized, the
  whole `DoGrowthClass` helper class can be deleted - its logic collapses into two stateless methods
  directly on `Spirit`, one of which does exactly one select+resolve step.

## Approach

### 1. Serialize `GrowthTrack`'s `Used` progress

Add `ToJson`/`RestoreFromJson` to `GrowthTrack` (`SpiritIsland/Growth/GrowthTrack.cs`), matching the shape
`SpiritPresence.ToJson` already uses (`SpiritIsland/Presence/SpiritPresence.cs:250-260`): capture only the
mutable delta, not the immutable `Groups` structure (still reconstructed via aspect replay). Reuse the same
position-based identity already established for `GrowthAction` in `Spirit.SerializeGrowthAction`
(`Spirit.cs:698-708`, `pickGroups[p].Groups[g]`):

```csharp
public JsonArray ToJson( ISerializationContext ctx ) => new JsonArray(
    _pickGroups.SelectMany( (pg, p) => pg.Groups
        .Select( (g, gi) => (p, gi, g) )
        .Where( t => t.g.Used )
        .Select( t => (JsonNode)new JsonArray( t.p, t.gi ) ) )
    .ToArray() );

public void RestoreFromJson( JsonArray json, ISerializationContext ctx ) {
    foreach( var pg in _pickGroups )
        foreach( var g in pg.Groups )
            g.Used = false;
    foreach( JsonNode? n in json ) {
        var pair = (JsonArray)n!;
        _pickGroups[ pair[0]!.GetValue<int>() ].Groups[ pair[1]!.GetValue<int>() ].Used = true;
    }
}
```

Wire into `Spirit.ToJson`/`RestoreFromJson` (`Spirit.cs:576-615`) as a new `["GrowthUsed"]` entry.

Update the two doc comments that currently cite GrowthTrack as fully-deterministic/uncaptured
(`Spirit.cs:516-520` and `SpiritPresence.cs:239-245`) to note the `Used` flag is now the one exception.

### 2. Collapse `DoGrowthClass` into two stateless methods on `Spirit`

Delete `SpiritIsland/SpiritParts/DoGrowthClass.cs`. In `Spirit.cs`'s existing `#region Growth`
(around line 101), add:

```csharp
public bool HasMoreGrowthActions =>
    GrowthTrack.RemainingOptions( Energy ).SelectMany( o => o.GrowthActionFactories )
        .Union( GetAvailableActions( Phase.Growth ) )
        .OfType<IHelpGrowActionFactory>()
        .Any();

/// <summary>
/// Selects and resolves exactly one Growth action - safe to call repeatedly, one decision at a time,
/// with a JSON save/restore in between each call (see docs/GameSerialization-Roadmap.md's mid-action
/// note - this is the one boundary-safe unit of work for the Growth phase). GrowthTrack.Reset() must
/// have already run once for the round (DoGrowth below does this for the common "run the whole phase"
/// case).
/// </summary>
public async Task SelectAndResolveNextGrowthAction() {
    bool isNewPick = !GetAvailableActions( Phase.Growth ).Any();
    GrowthGroup[] remainingOptions = GrowthTrack.RemainingOptions( Energy );
    IActionFactory[] consolidated = remainingOptions.SelectMany( o => o.GrowthActionFactories )
        .Union( GetAvailableActions( Phase.Growth ) ).ToArray();

    IActionFactory selectedAction = await SelectAlways( new A.GrowthDecision( "Select Growth", consolidated, Present.Always ) );

    if( isNewPick ) {
        GrowthGroup? option = remainingOptions.SingleOrDefault( o => o.GrowthActionFactories.Contains( selectedAction ) );
        if( option is not null ) {
            GrowthTrack.MarkAsUsed( option );
            foreach( IHelpGrowActionFactory action in option.GrowthActionFactories )
                _availableActions.Add( action );

            foreach( var autoAction in option.GrowthActionFactories.Where( x => x.AutoRun ) )
                if( autoAction != selectedAction )
                    await ResolveActionAsync( autoAction, Phase.Growth );
        }
        // option is null: selectedAction came from somewhere else, not the Growth track (e.g. Behemoth Rise)
    }

    await ResolveActionAsync( selectedAction, Phase.Growth );
}

public virtual async Task DoGrowth(GameState gameState) {
    GrowthTrack.Reset();
    while( HasMoreGrowthActions )
        await SelectAndResolveNextGrowthAction();
    await ApplyRevealedPresenceTrack();
}
```

`DoGrowth` keeps its existing signature/behavior (still the single call every current caller/test uses:
`SoloGame.Do1Round`, `EmberEyedBehemoth.DoGrowth`'s `base.DoGrowth(...)`, and every Growth test via
`spirit.DoGrowth(gs).AwaitUser(...)`) - it's now just a thin loop over the new granular method. Anything
that *does* want per-decision save points (a future step-by-step driver) can call
`HasMoreGrowthActions`/`SelectAndResolveNextGrowthAction()` directly instead, as long as it calls
`GrowthTrack.Reset()` once at the start of the round's Growth phase first.

No changes needed to `PickGroups`/`GrowthGroup` themselves (already refactored to expose `Used`) or to
`GrowthTrack.RemainingOptions`/`MarkAsUsed`/`Reset` (already correct, just called from a new location).

### 3. New test: Growth phase survives a mid-phase JSON round-trip

Add a test (likely `SpiritIsland.Tests/Serialization/`, following the `BuildAndRestore`-style pattern in
`SpiritMods_HighTier_Tests.cs`/`SpiritMods_MediumTier_Tests.cs`) that: builds a spirit with a
multi-pick-group `GrowthTrack`, calls `SelectAndResolveNextGrowthAction()` once (via `AwaitUser`), round-trips
the whole `GameState` through `ToJson`/`RestoreFromJson`, and confirms `HasMoreGrowthActions` and the
remaining available options are identical afterward. This is the concrete proof the new design achieves what
was asked: growth progress now survives a save between individual actions, not just between whole phases.

## Verification

1. `dotnet build -f net10.0` - 0 warnings, 0 errors (current baseline).
2. `dotnet test SpiritIsland.Tests -f net10.0` - full suite (943 tests today) must still pass, especially:
   - Every existing Growth test (`Volcano_Tests`, `Stone_Growth_Tests`, `RiverSurges_GrowthTests`,
     `OceanTerrain_Tests`, `Finder_Tests`, and anything using `SpiritExtensions.DoGrowth`/`AwaitUser`) -
     regression check that collapsing `DoGrowthClass` didn't change observable behavior.
   - `EmberEyedBehemoth`-related tests specifically, since its `DoGrowth` override and `TheBehemothRises`
     mid-decision injection (`GrowthAction`'s "may Behemoth Rise" comment) are the one documented edge case
     where recomputing fresh each call (rather than the old code's staleness) could differ - verify no
     regression.
3. New mid-phase round-trip test (above) passes.
