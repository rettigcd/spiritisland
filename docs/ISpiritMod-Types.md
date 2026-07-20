# `ISpiritMod` Type Catalog

`ISpiritMod` (`SpiritIsland/Hooks/Spirit/ISpiritMod.cs`) is a marker interface, no members of its own.
An object becomes a valid `spirit.Mods.Add(...)` target either by implementing it directly, or
transitively through one of 6 hook interfaces that extend it: `IInitializeSpirit`,
`IModifyAvailableActions`, `IHandleActivatedActions`, `IHandleCardPlayed`,
`ICleanupSpiritWhenTimePasses`, `ICreateSacredSites`. (`IConfigureMyActions` and
`IEndWhenTimePasses` do **not** themselves extend `ISpiritMod` - a mod that implements one of those
always implements one of the 6 above as well, since that's the only way it type-checks as a `Mods.Add`
argument.)

`Spirit.Mods` (`ModBucket`, `SpiritIsland/SpiritParts/ModBucket.cs`) is a `List<ISpiritMod>` with no
`IHaveMemento` implementation at all - it isn't covered by today's in-memory undo. This catalog scoped
that JSON gap (see `docs/GameSerialization-Roadmap.md`) the same way `docs/ISpaceEntity-Types.md` scoped
the (also now-closed) `ISpaceEntity` gap - all 22 types found are now verified closed, see the Summary
below.

## Methodology & caveats

Compiled via a project-by-project source sweep (grep for every class implementing one of the 6
interfaces above, or `ISpiritMod` directly, then read each matching file), with spot-checks against
source for the more surprising findings. Treat this as a strong starting point, not gospel - re-verify
a class against current source before depending on its rating for actual implementation, since the
codebase will keep moving. Excluded: `SpiritIsland.Tests`, and `RunSlowCardsAsFastMod_EveryRound`
itself (abstract - only its 2 concrete subclasses are listed as real entries).

## Complexity rubric

| Rating | Meaning |
|---|---|
| **Low** | Deterministically reconstructed by replaying spirit/aspect construction (once that replay mechanism exists) - zero extra runtime state to capture. Either fully stateless, or its only field is the owning `Spirit` back-reference. |
| **Medium** | Reconstructed the same deterministic way, but also carries small extra mutable state (a `bool`/`int`/small list of stably-keyed items) that needs its own capture/restore hook - same shape as the `CustomStateToJson` hooks already built for `Spirit`/`SpiritPresence` subtypes. No reference-equality dependency. |
| **High** | Tracks "used"/consumed state via **reference-equality** against its own cached instance(s) inside `spirit.UsedActions`/`AllActions` (e.g. `x == this`, or `.Contains(_cachedField)`). Restoring this correctly requires `Spirit.Mods` to be reconstructed *before or alongside* `_usedActions`/`_availableActions`'s own restore, so both resolve to the exact same objects - an identity-ordering problem, not just an encoding one. |

Two cross-cutting tags apply on top of the rating:
- **Short-lived** (`IEndWhenTimePasses`) - auto-removed from `Mods` at the next round boundary, so a
  restore only needs a small "still active" fact, never long-term identity.
- **Ordering-risk** - installs *another* mod via `IInitializeSpirit.Initialize()`, which the engine
  guarantees fires exactly once (`InitSpirit`). A restore path can't safely call it again without
  double-installing; it must either replay it exactly once or reconstruct the post-`Initialize()` state
  directly (see `docs/GameSerialization-Roadmap.md`'s ordering/idempotency challenge).

## Summary

| Rating | Count |
|---|---:|
| Low | 7 (verified - see below) |
| Medium | 8 (verified - see below) |
| High | 7 (verified - see below) |
| **Total** | **22** |

All 22 catalogued types are now verified closed.

| Project | Low | Medium | High | Total |
|---|---:|---:|---:|---:|
| Basegame | 3 | 4 | 1 | 8 |
| FeatherAndFlame | 2 | 0 | 1 | 3 |
| JaggedEarth | 2 | 2 | 3 | 7 |
| NatureIncarnate | 1 | 1 | 2 | 4 |
| Horizons | 0 | 1 | 0 | 1 |
| BranchAndClaw | 0 | 0 | 0 | 0 |

BranchAndClaw adds no `Spirit.Mods` classes at all - its content (Blight Cards/Events) uses a different
mechanism. `Violence` (Basegame) duplicates the `IHandleCardPlayed` dispatch loop locally in its own
`SelectAndPlay1` (`SpiritIsland.Basegame/Spirits/Bringer/Aspects/Violence.cs:90`) rather than defining a
new mod class - not counted here. `TheBehemothRises` (NatureIncarnate) is the reference-equality *target*
cached inside `UnrelentingStrides` below, but isn't itself an `ISpiritMod` - also not counted, but relevant
to that entry's difficulty.

## Low - deterministic replay, no extra state - VERIFIED, closed

Confirmed empirically rather than just reasoned about: `SpiritIsland.Tests/Serialization/SpiritMods_LowTier_Tests.cs`
builds each spirit (with its aspect, where relevant) via `GameConfiguration.BuildGame()`, round-trips the
whole `GameState` through `ToJson`/`RestoreFromJson`, and confirms the mod is still present in `Mods`
afterward. No production code was needed - `GameState.RestoreFromJson`'s existing precondition (restore
onto a `GameState` built "the same way" as the original) already reconstructs `Mods` correctly for every
type in this tier, since `Mods` is populated at construction/`ModSpirit` time and `Spirit.RestoreFromJson`
never touches it.

| Type | Project / file | Added via | Status |
|---|---|---|---|
| `RiversDomain` | Basegame - `Spirits/River/Mods/RiversDomain.cs` | `RiverSurges`'s own constructor | Verified |
| `TwoElementsForMajorCards` | Basegame - `Spirits/Lightning/Aspects/Immense.cs` (nested) | `Immense.ModSpirit` | Verified |
| `FinderOfPathsUnseen` | FeatherAndFlame - `Spirits/Finder/FinderOfPathsUnseen.cs` | its own constructor (`Mods.Add(this)` - the mod *is* the Spirit) | Verified |
| `WreakVengeananceForTheLandsCorruption` | JaggedEarth - `Spirits/Vengeance/Mods/WreakVengeananceForTheLandsCorruption.cs` | `VengeanceAsABurningPlague`'s own constructor | Verified |
| `FlyFastAsThought` | JaggedEarth - `Spirits/ManyMinds/Mods/FlyFastAsThought.cs` | `ManyMindsMoveAsOne`'s own constructor | Verified |
| `RelentlessPunishment` | NatureIncarnate - `Spirits/Sun/Mods/RelentlessPunishment.cs` | `RelentlessGazeOfTheSun`'s own constructor | Verified |
| `LocusOfTheSerpentsRegard` | FeatherAndFlame - `Spirits/Serpent/Mods/LocusOfTheSerpentsRegard.cs` | `Locus.ModSpirit` | Verified |

`LocusOfTheSerpentsRegard`'s test was initially blocked on an unrelated gap: `Locus.ModSpirit` also adds a
raw `SpiritAction` via `AddActionFactory(...).ToGrowth()`, and `SelfCmdRegistry` didn't support `SpiritAction`
(only `PlayCardForCost`). Fixed by converting the 3 solution-wide call sites that wrapped an anonymous
`new SpiritAction(title, delegate)` into named `ISerializableSelfCmd` subclasses (same pattern as
`PlayCardForCost` itself): `Locus.PlaceIncarnaAndFireEnergy`, `WarriorSpiritsRaidingParty.PlaceIncarna`,
`Lair.InitLair`. All 3 have their own round-trip test in `SpiritMods_LowTier_Tests.cs`.

Two small pre-existing bugs, unrelated to `Spirit.Mods`, surfaced and fixed while writing these tests:
`ThrivingCrops` (NatureIncarnate `BlightCard`) was missing its `BlightCardRegistry.Register` call entirely
(never caught before because no test exercised NatureIncarnate blight cards through a full
`GameState.ToJson` round trip); and 2 NatureIncarnate Fear cards fail an existing text-formatting check
(discovered only because fixing the first bug meant briefly routing NatureIncarnate through the shared
`TestGames.GameBuilder` - reverted in favor of a test-file-local `GameBuilder` instead, so that finding is
noted here rather than fixed, to avoid pulling unrelated content edits into this change).

## Medium - deterministic replay, plus small extra state - VERIFIED, closed

Closed via a new small mechanism - `ISerializableSpiritMod` (`SpiritIsland/Hooks/Spirit/ISerializableSpiritMod.cs`)
plus `SpiritModRegistry` (`SpiritIsland/Serialization/SpiritModRegistry.cs`), wired into
`Spirit.ToJson`/`RestoreFromJson` as a new `mods` element - same tag-dispatch shape as
`SelfCmdRegistry`/`TimePassesActionRegistry`, except a reader is an `Action<Spirit, JsonArray, ...>`
that mutates directly rather than a `Func` returning a fresh instance: most of these mods already exist
in `Mods` by the time it runs (added deterministically at construction/`ModSpirit` time, replayed for
free before `RestoreFromJson` is ever called), so the reader finds and mutates that same instance -
constructing a new one and `Mods.Add()`-ing it would create a duplicate. The 2 short-lived types
(added dynamically mid-game, not by spirit/aspect construction) are the exception - their reader
constructs a fresh instance and adds it, since nothing added it during replay.

Verified in `SpiritIsland.Tests/Serialization/SpiritMods_MediumTier_Tests.cs` - one test per type,
round-tripping a whole `GameState` and asserting the exact private field via reflection (several of
these mod classes are internal to their own project by design; reflection exercises the real state
without loosening production accessibility just for testability).

| Type | Project / file | Added via | Extra state | Status |
|---|---|---|---|---|
| `FrightfulShadowsEludeDestruction` | Basegame - `Spirits/Shadows/Mods/FrightfulShadowsEludeDestruction.cs` | `InitAspect`, only for the cleanup hook (it's really the Presence token) | `bool UsedThisRound` | Verified - already fully handled via its own pre-existing `SpaceEntitySerialization` registration; no new code needed |
| `ASingleAluringLair` | JaggedEarth - `Spirits/Lure/Mods/ASingleAluringLair.cs` | `Lair.ModSpirit` | already captured `Empowered` via its own `ISerializableSpaceEntity.ToJson` | Verified - found and fixed a real bug along the way: its registered reader constructed a *fresh* instance instead of reusing `spirit.Presence.Incarna`, the same double-instantiation class of bug fixed for `GatewayToken`/`ToDreamAThousandDeaths` |
| `ReachThroughEphemeralDistance` | Basegame - `Spirits/Shadows/Mods/ReachThroughEphemeralDistance.cs` | `InitAspect`, only for the cleanup hook (it's really `PowerRangeCalc`) | `bool _usedThisRound` (`_bonusTargets` is always empty at a valid save boundary - not captured) | Verified - also fixed a real bug: `DefaultRangeCalculator.ToJson`'s base implementation tagged it "Default", silently downgrading a Reach-aspected spirit's `PowerRangeCalc` on restore. Registers its own `RangeCalcRegistry` tag instead |
| `SwiftnessOfLightning` | Basegame - `Spirits/Lightning/Mods/SwiftnessOfLightning.cs` | `LightningsSwiftStrike`'s own constructor | plain `int _usedCount` (base class, exposed as `protected UsedCountForJson`) | Verified |
| `GatherPowerFromTheCoolAndDark` | JaggedEarth - `Spirits/Shroud/Mods/GatherPowerFromTheCoolAndDark.cs` | `ShroudOfSilentMist`'s own constructor, only for the cleanup hook (it's really `Draw`) | `bool _usedThisRound` | Verified |
| `Run1SlowNonMajorAsFast` | Basegame - `Spirits/Lightning/Mods/Run1Fast.cs` | `ExaltationOfTheStormWind`'s innate tier 1 | plain `int _usedCount`; **Short-lived** | Verified - reader constructs + `Mods.Add()`s a fresh instance, since replay never adds this one |
| `Run1SlowPushOrGatherAsFast` | Horizons - `Spirits/SunBright/Cards/GiftOfWindSpedSteps.cs` | `GiftOfWindSpedSteps.ActAsync` (any spirit, Fast card) | plain `int _usedCount` (base class); **Short-lived** | Verified - same shape as `Run1SlowNonMajorAsFast` |
| `EnableEmpoweredAbductMod` | NatureIncarnate - `Spirits/BreathOfDarkness/EmpoweredAbduct.cs` | `BreathOfDarknessDownYourSpine`'s own constructor | none on the mod itself | Verified - the mod needed no `ISerializableSpiritMod` at all (stateless, deterministic - genuinely Low tier). The real gap was one level down: `EmpoweredAbduct` (the `IActionFactory` it compares `spirit.UsedActions` against by reference) wasn't `ISerializableActionFactory` at all, and even if serialized naively would've resolved to a fresh instance instead of the shared `static readonly Singleton` the reference-equality check depends on. Fixed by making `EmpoweredAbduct` implement `ISerializableActionFactory`, resolving to `Singleton` |

The `RunSlowCardsAsFastMod_EveryRound` shared base (`SpiritIsland/RunSlowCardsAsFastMod_EveryRound.cs`)
now exposes `protected int UsedCountForJson` so its 3 concrete subclasses (`SwiftnessOfLightning` plus
the 2 short-lived ones above) can each implement `ISerializableSpiritMod` against their own tag without
duplicating the counter itself.

## High - reference-equality against a live instance - VERIFIED, closed

Closed via `IOwnedActionFactories` (`SpiritIsland/Hooks/Spirit/IOwnedActionFactories.cs`):
`Spirit.SerializeActionFactory`/`DeserializeActionFactory` now checks every `Mods` entry implementing it
*before* falling through to the ordinary `PowerCard`/`InnatePower`/`GrowthAction`/etc. cases, resolving
a cached factory back to the exact live instance the owning mod already holds (re-added for free by
spirit/aspect construction replay, same as every other tier) rather than a fresh, reference-distinct one
built from the factory's own serialized content. No reordering of `Spirit.RestoreFromJson`'s existing
steps was needed - `Mods` is already fully populated (by construction/`ModSpirit`, which ran during the
*earlier*, separate `BuildGame()` call the restore target came from) before `_usedActions`/
`_availableActions` ever deserialize, so `Mods.OfType<IOwnedActionFactories>()` always has an answer by
the time it's asked.

Verified in `SpiritIsland.Tests/Serialization/SpiritMods_HighTier_Tests.cs` - one round-trip test per
type, putting the owned factory into `_usedActions` (via reflection) before serializing and confirming
`UsedActions` still contains the exact same object afterward.

| Type | Project / file | Added via | Tracked via | Status |
|---|---|---|---|---|
| `ShadowsPartakeOfAmorphousSpace` | Basegame - `Spirits/Shadows/Mods/ShadowsPartakeOfAmorphousSpace.cs` | Shadows aspect `InitAspect` | `UsedActions.Count(x => x == _moveFast \|\| x == _moveSlow)` | Verified |
| `PourDownPower` | FeatherAndFlame - `Spirits/Downpour/PourDownPower.cs` | `DownpourDrenchesTheWorld`'s own constructor | `UsedCounts => UsedActions.Count(x => x == _a1 \|\| x == _a2)` | Verified - closes roadmap row 1's `RepeatLandCardForCost` |
| `MistsSteadilyDrift` | JaggedEarth - `Spirits/Shroud/Mods/MistsSteadilyDrift.cs` | `Stranded.ModSpirit` | `UsedActions.Count(x => x == _pushFast \|\| x == _pushSlow)` | Verified |
| `StrandedInTheShiftingMists` | JaggedEarth - `Spirits/Shroud/Mods/StrandedInTheShiftingMists.cs` | `Stranded.ModSpirit` (same aspect as above) | `UsedActions.Any(x => x == _isolate)` | Verified |
| `UnrelentingStrides` | NatureIncarnate - `Spirits/EmberEyed/UnrelentingStrides.cs` | `EmberEyedBehemoth`'s own constructor | `UsedActions.Count(x => x == _behemoth)` against `TheBehemothRises` | Verified - closes roadmap row 1's `TheBehemothRises` |
| `MarkedBeastMover` | NatureIncarnate - `Major/UnearthABeastOfWrathfulStone.cs` (nested) | `MarkedBeast`'s constructor (a board token, spawned from a Major Power's `ActAsync`) | `UsedActions.Any(x => x == this)` - self-reference | Verified - closes roadmap row 1's `MarkedBeastMover`. Unlike every other type here, `Mods` replay never recreates this one (it's spawned mid-game, not by spirit/aspect construction) - the *only* place it's reconstructed on restore is `MarkedBeast`'s own `SpaceEntitySerialization` reader (via `Tokens_ForIsland.FromJson`), which runs before `Spirit.RestoreFromJson`, so `Mods` already has it by the time `_usedActions` asks. The previously-flagged "double instantiation" risk didn't materialize in testing - `MarkedBeast` only ever sits on one space, so its reader only ever runs once |
| `IntensifyAirWater` | JaggedEarth - `Spirits/ShiftingMemory/Mods/IntensifyAirWater.cs` | `IntensifyThroughUnderstanding.InitAspect` | *(originally listed as tracking `_slowAsFast.Contains(factory)` against a cached array, plus an `IInitializeSpirit.Initialize()` ordering risk)* | Verified - **neither problem actually applies**, no code changed. `_slowAsFast` is rebuilt fresh every `Modify()` call and read only within that same synchronous cycle - it never survives to a valid save boundary (mid-decision states can't be saved at all, see roadmap's Design Constraints). And `RestoreFromJson` never calls `Initialize()` again - that only fires once, during the *earlier* normal `BuildGame()` the restore target was already built through. This was the catalog's original over-estimate; correcting it here |

`RepeatLandCardForCost`/`TheBehemothRises`/`MarkedBeastMover` were exactly the 3 types blocking roadmap
row 1 - now closed as a side effect of this tier.

## Recommended order of attack

Done - all 22 catalogued types across Low/Medium/High are verified closed. See `docs/GameSerialization-Roadmap.md`
for what (if anything) remains on the broader serialization effort.
