# Plan: Single test call site for Spirit.SelectAndResolveActions

## Context

`Spirit.SelectAndResolveActions(GameState)` (`SpiritIsland/SpiritParts/Spirit.cs:170-183`) is called directly
from 9 test files across 19 call sites (plus 3 production call sites in `SoloGame.cs`, which are untouched -
this is a test-only change). Same problem the earlier `DoGrowthForTestsAsync` wrapper solved for
`Spirit.DoGrowthAsync`: if `SelectAndResolveActions`'s signature ever changes, every one of those 19 spots
needs a matching edit. Routing all of them through one new test-project extension method means only that one
method needs to change.

Unlike the `DoGrowthForTestsAsync` follow-up (which was asked to mirror `DoGrowthAsync`'s body directly, for
a different reason - decoupling from `EndGrowth`), this request's stated goal is purely signature-change
isolation, so a thin delegating wrapper is the right shape here - no need to duplicate
`SelectAndResolveActions`'s loop body.

## Approach

Add to `SpiritIsland.Tests/Extensions/SpiritExtensions.cs` (same file/region as `DoGrowthForTestsAsync`,
`#region When`):

```csharp
/// <summary> Single call site for Spirit.SelectAndResolveActions from tests, so a future signature change
/// only touches this one spot. </summary>
internal static Task SelectAndResolveActionsForTest( this Spirit spirit, GameState gameState )
    => spirit.SelectAndResolveActions( gameState );
```

Then replace every test-project call to `spirit.SelectAndResolveActions(gs)` with
`spirit.SelectAndResolveActionsForTest(gs)` (mechanical rename, same arguments, same call shape - `.AwaitUser(...)`/`.ShouldComplete(...)` chains untouched) in:

- `SpiritIsland.Tests/Extensions/SpiritExtensions.cs:249` (inside `Testing_GrowAndResolve`, itself a test helper - also included so *every* test-project path funnels through the one call site)
- `SpiritIsland.Tests/Spirits/VitalStrength/GiftOfStrength_Tests.cs` (2 call sites)
- `SpiritIsland.Tests/Spirits/EmberEyed/EmberEyed_Tests.cs` (2 call sites)
- `SpiritIsland.Tests/Spirits/Breath/TerrorStalksTheLand_Tests.cs` (2 call sites)
- `SpiritIsland.Tests/Core/CommandBeast_Test.cs` (6 call sites)
- `SpiritIsland.Tests/Minor/SkyStretchesToShore_Tests.cs` (1 call site)
- `SpiritIsland.Tests/Spirits/RelentlessGaze/RelentlessGaze_Tests.cs` (1 call site)
- `SpiritIsland.Tests/Spirits/Ocean/OceanTerrain_Tests.cs` (3 call sites)
- `SpiritIsland.Tests/Spirits/Ocean/SwallowTheLandDwellers_Tests.cs` (1 call site)

`SoloGame.cs`'s 2 production call sites (lines 47, 102, 108 - `Spirit.SelectAndResolveActions(GameState)`)
are left as-is; this change is scoped to the test project only, matching what was asked.

## Verification

1. `dotnet build -f net10.0` - 0 warnings, 0 errors.
2. `dotnet test SpiritIsland.Tests -f net10.0` - full suite (944 tests currently) must still pass unchanged,
   since this is a pure call-site substitution with no behavior change.
3. `grep` for `\.SelectAndResolveActions\(` under `SpiritIsland.Tests/` afterward should show exactly one
   match - the new wrapper's own body.
