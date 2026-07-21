# Plan: Step-wise Spirit.SelectAndResolveActions for SoloGame

## Context

`Spirit.SelectAndResolveActions(GameState gs)` (`SpiritIsland/SpiritParts/Spirit.cs:170-183`) currently runs
a phase's entire action-resolution loop in one call:

```csharp
public async Task SelectAndResolveActions( GameState gs ) {
    Phase phase = gs.Phase;
    Present present = phase switch{ Phase.Growth or Phase.Init => Present.Always, _ => Present.Done };
    IActionFactory[] options = GetAvailableActions(phase).ToArray();
    while( 0 < options.Length ) {
        IActionFactory? option = await Select(new A.TypedDecision<IActionFactory>("Select " + phase + " to resolve", options, present));
        if( option is null ) break;
        await ResolveActionAsync(option, phase);
        options = GetAvailableActions(phase).ToArray();
    }
}
```

`SoloGame.cs` calls it 3 times (`StartAsync` for `Phase.Init`, `Do1Round` for `Phase.Fast` and
`Phase.Slow`) - each call resolves every remaining action for that phase back-to-back. This is the same
"whole-phase-in-one-call" shape `DoGrowthAsync`/`DoGrowthClass` had before this session's Growth-phase
refactor, and that refactor's end state is the precedent to follow here directly: `DoGrowthAsync` was
removed from `Spirit` entirely once the granular step existed, `SoloGame.Do1Round` inlines the Growth loop
itself, and the test project's `DoGrowthForTestsAsync` independently re-implements that same loop rather
than delegating to a `Spirit`-level wrapper. `SelectAndResolveActions` gets the identical treatment:
`Spirit` keeps only the single-step primitive; `SoloGame` and the test extension method each own their own
loop over it.

One real difference from the Growth case: Growth uses `Present.Always` (forces a selection every time -
`SelectAlways` never returns null), so `SelectAndResolveNextGrowthAction()` never needed to signal "stop."
Fast/Slow/Init use `Present.Done` for non-Growth phases, so the user can explicitly decline ("Done") and
stop mid-phase even with options still available - the loop needs a signal for that, not just whether
`GetAvailableActions(phase)` is non-empty. So the new granular method returns `bool` (whether it resolved
something) rather than being `void` like the Growth one.

## Approach

### 1. `Spirit.cs` - replace `SelectAndResolveActions` with the granular step

Remove `SelectAndResolveActions(GameState gs)` entirely and replace it with:

```csharp
/// <summary>
/// Selects and resolves exactly one action for the phase in gs.Phase - safe to call repeatedly, one
/// decision at a time. Returns false once there's nothing left to do (no options remain, or the user
/// declined/chose "Done"), signaling the caller to stop looping.
/// </summary>
public async Task<bool> SelectAndResolveNextAction( GameState gs ) {
    Phase phase = gs.Phase;
    Present present = phase switch{ Phase.Growth or Phase.Init => Present.Always, _ => Present.Done };
    IActionFactory[] options = GetAvailableActions(phase).ToArray();
    if( options.Length == 0 ) return false;

    IActionFactory? option = await Select(new A.TypedDecision<IActionFactory>("Select " + phase + " to resolve", options, present));
    if( option is null ) return false;

    await ResolveActionAsync(option, phase);
    return true;
}
```

### 2. `SoloGame.cs` - own the loop directly

Replace each of the 3 `await Spirit.SelectAndResolveActions( GameState );` calls
(`StartAsync` line 47, `Do1Round` lines 102 and 108) with an inlined loop, mirroring how Growth is already
inlined directly in `Do1Round` (lines 92-96):

```csharp
while( await Spirit.SelectAndResolveNextAction( GameState ) ) { }
```

### 3. `SpiritIsland.Tests/Extensions/SpiritExtensions.cs` - give the test wrapper its own loop

`SelectAndResolveActionsForTest` (added last session as a thin delegating call-through) can no longer
delegate, since `Spirit.SelectAndResolveActions` won't exist. Change it to own the loop directly, the same
way `DoGrowthForTestsAsync` independently re-implements `DoGrowthAsync`'s former body:

```csharp
internal static async Task SelectAndResolveActionsForTest( this Spirit spirit, GameState gameState ) {
    while( await spirit.SelectAndResolveNextAction( gameState ) ) { }
}
```

No other test files change - every existing call site already goes through this one method (confirmed last
session: `SelectAndResolveActions` only appears in `Spirit.cs`, `SoloGame.cs`, and this one wrapper -
verified via a repo-wide search, so removal is fully scoped to these 3 files).

## Verification

1. `dotnet build -f net10.0` - 0 warnings, 0 errors.
2. `dotnet test SpiritIsland.Tests -f net10.0` - full suite (944 tests currently) must still pass unchanged;
   behavior is identical, just restructured, so no test assertions should need updating.
3. Confirm via `grep` that `SelectAndResolveActions` (the old name) no longer appears anywhere in source
   after the change - only `SelectAndResolveNextAction` and `SelectAndResolveActionsForTest` remain.
