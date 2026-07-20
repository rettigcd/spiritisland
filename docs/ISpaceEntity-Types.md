# `ISpaceEntity` Type Catalog

`ISpaceEntity` (`SpiritIsland/Tokens/Interfaces/ISpaceEntity.cs`) is the marker interface for "anything that can be stored on a `Space`" — both visible game tokens (`IToken`) and invisible rule-modifying "mods" (`BaseModEntity`, plus several hook interfaces that extend `ISpaceEntity` directly: `IConfigRavages`, `ISkipBuilds`, `ISkipRavages`, `ISkipExploreTo`, `ISkipExploreFrom`, `IRunBeforeInvaderPhase`, `IRunAfterInvaderPhase`, `ICleanupSpaceWhenTimePasses`).

This catalog lists every concrete class in the solution that implements `ISpaceEntity`, directly or transitively, and rates how hard it would be to serialize an instance to/from JSON. It exists to support the "Save state by serializing it" goal noted in `todo.txt`.

## Methodology & caveats

Compiled via a project-by-project source sweep (grep for all `ISpaceEntity`-family base types/interfaces, then read each matching file), with spot-checks against the source for accuracy. Treat this as a strong starting point, not gospel — re-verify a class against current source before depending on its rating for actual serialization design, since the codebase will keep moving.

Excluded: abstract/interface types themselves (e.g. `DynamicToken`, `DefendToken`, `SkipBuild_Custom`, `SkipExploreTo_Custom`, `AdjustDamageFromAttackers` are all abstract bases — only their concrete subclasses are listed), hook-interface implementers whose only qualifying base is a class already listed here (e.g. `SpiritPresenceToken`/`Incarna` subclasses are listed once, not re-derived), and `SpiritIsland.Tests` — test-only `ISpaceEntity` types (e.g. `ActionScopeTracker`) are never part of real game state and won't be serialized, so they're out of scope for this catalog entirely.

## Complexity rubric

| Rating | Meaning |
|---|---|
| **Low** | Parameterless, or constructor takes only primitives/enums/strings. All state is simple owned data — no references to other live game objects, no delegates. |
| **Medium** | Constructor/state references other stable, identifiable domain objects (a `Spirit`, a `TargetSpaceCtx`, a static token-class singleton like `Token.Beast`/`Human.Dahan`, a `HumanToken` value, a `CountDictionary` of simple keys). Serializable via a reference/lookup scheme; no closures. |
| **High** | Constructor or fields capture a delegate (`Func<>`/`Action<>`/lambda closure), self-register as an event handler, or hold a live/mutable object graph (`GameState`, `ActionScope`, `Space`/`Board` collections, another entity) that isn't naturally serializable as data without a redesign. |

**Exception — `ITrackMySpaces` caches:** `CountDictionary<Space>` fields populated via `ITrackMySpaces.TrackAdjust()` (e.g. `SpiritPresenceToken._spaceCounts`, `Incarna._spaceCounts`) are a derived reverse-index, not owned state — the primary data already lives in `Tokens_ForIsland`'s per-space counts. `Tokens_ForIsland.MyMemento.Restore()` already sketches the intended fix (commented out): call `ITrackMySpaces.Clear()` then replay `TrackAdjust()` for every space as the rest of the state loads. Because of that, a type isn't rated High on the strength of an `ITrackMySpaces` cache alone — only when it *also* holds something else non-serializable (a delegate, a direct `Space`/`Board` reference outside this mechanism, a nested entity, etc.).

## Summary

The goal: drive **Not Yet Serializable** to 0. "Serialized" means the type implements
`ISerializableSpaceEntity` and is registered with `SpaceEntitySerialization` (verified against
source, not just this doc, via `grep -rn "SpaceEntitySerialization.Register("` across all projects).

| Project | Not Yet Serializable | Serialized | Total |
|---|---:|---:|---:|
| SpiritIsland (core) | 0 | 20 | 20 |
| SpiritIsland.Basegame | 0 | 17 | 17 |
| SpiritIsland.BranchAndClaw | 0 | 7 | 7 |
| SpiritIsland.FeatherAndFlame | 0 | 3 | 3 |
| SpiritIsland.Horizons | 0 | 2 | 2 |
| SpiritIsland.JaggedEarth | 0 | 34 | 34 |
| SpiritIsland.NatureIncarnate | 0 | 20 | 20 |
| **Total** | **0** | **103** | **103** |

The complexity rubric below still applies to what's left (it's a reasonable proxy for how much work
each remaining type needs), but it's no longer the headline metric - the "Remaining types" section
further down lists only the types still not serializable; everything already serialized has been
dropped from the detailed listing (see git history / `docs/` history for how each one was converted,
or grep `SpaceEntitySerialization.Register` in source for the current, authoritative list).

**Twelfth pass — the "parameterless cluster" (31 types):** every remaining type whose entire state was
either nothing at all, or a mutable field with no constructor param backing it (`SwedenHeavyMining.MiningRush`,
`CountDestroyedTokens`/`CountDestroyedExplorers`'s `Count`). Same trivial pattern throughout: `const string Tag`,
`[ModuleInitializer]` registering `() => new TypeName()`, `ToJson => new JsonArray(Tag)` (or `new JsonArray(Tag, field)`
for the 3 with real mutable state). No new infrastructure, no design questions - purely mechanical, across
6 of the 7 projects (JaggedEarth: `HabsburgMakeTownsDurable`, `TrackBadRavageBlight`,
`Russia_Level1_HuntersBringHomeShelAndHide`, `Russia_Level2_SenseOfPendingDisasterMod`,
`DestroyerOfBeastsAndPresence`, `LandDamageBoost`, `Quicksand`, `RavageConfigToken`, `DamageNewInvaders`,
`StopCascade`, `PushPresenceInsteadOfDestroy`, `CountDestroyedTokens`, `CountDestroyedExplorers`; Basegame:
`SwedenHeavyMining`, `SimultaneousDefend`, `FearPushesInvaders`, `StopBuildInCityLands`,
`StopBuildWhereDahanOutnumber`, `StopBuildInDahanLands`, `StopExploreIn2DahanLands`; core: `SkipExploreFrom`,
`ReduceHealthByStrife`, `BlockBlightToken`, `LandDamage`; BranchAndClaw: `FranceFearPushesExplorers`,
`SkipLowestNumberedExplore`, `ReplaceRavageWithBuild`; NatureIncarnate: `AvariceRewardedMod`,
`DiseaseStopsRavageInMiningLands`, `BlightedInvadersSitOutRavage`; FeatherAndFlame: `ScotlandCoastalBlightCheckToken`).

`LandDamage` is the one case worth flagging: it's used as a stable singleton (`LandDamage.Token`) the same
way `Token.Beast` is, but isn't an `ITokenClass`, so it isn't resolved via `TokenClassRegistry` - `FromJson`
reconstructs a *fresh* instance instead. Same identity caveat as `AJoiningOfSwarmsAndFlocks`'s beast token:
fine for an isolated round-trip (no state to lose), not yet safe for a full board restore where the
singleton reference matters. (`FreezePresence`'s `TokenVariety` fields used to be cited as another example
of this same caveat - they no longer are, see the sixteenth pass below.)

BranchAndClaw and FeatherAndFlame are now fully serialized - every `ISpaceEntity` type in both projects
implements `ISerializableSpaceEntity`.

**Thirteenth pass — the "simple-primitive" cluster (4 types, all core):** the last non-special types
whose only state is 1-2 primitives or an enum, no domain references. `InvaderActionToken` (`string label`),
`SkipExploreTo` (`bool skipAll`), `SkipRavage` (`string label, UsageDuration duration` - enum captured as
`(int)duration`, matching `SkipBuild`'s existing convention), `SaveDahan` (`int maxPerAction, bool repeat`;
its constructor is private, reachable only via the `DestroyFewer` factory, but `FromJson` can still call it
directly since it's a member of the same class). `SaveDahan`'s per-instance GUID key (`_key`) isn't captured -
it only indexes into the current `ActionScope`, which isn't persisted across saves, so a freshly-generated
GUID on `FromJson` is equivalent. `InvaderActionToken` has the same singleton-identity caveat as `LandDamage`:
`DoExplore`/`DoBuild`/`DoRavage` are stable references used as dictionary keys elsewhere; `FromJson` builds a
fresh instance rather than resolving back to one of the 3 singletons - fine in isolation, not yet safe for a
full board restore.

**Fourteenth pass — `StopDahanDamageAndDestruction`:** a single `string _sourceName` field, no domain
references - it was originally miscategorized alongside the genuinely-blocked leftovers rather than the
simple-primitive cluster it actually belongs to. Basegame is now fully serialized.

**Fifteenth pass — the "special core types" (`TokenClassToken`, `TokenVariety`, `HumanToken`, `Incarna`):**
turned out to be 2 real conversions and 2 non-issues, not 4 conversions.

`TokenClassToken` and `HumanToken` deliberately do **not** implement `ISerializableSpaceEntity` - they're
already fully handled by mechanisms built earlier in this effort. `TokenClassToken` instances (`Token.Beast`,
etc.) are stable, process-wide singletons resolved by label via `TokenClassByLabel`/`TokenClassRegistry`,
which the polymorphic `IToken` resolver (`SerializeToken`/`DeserializeToken`) already checks for any
`ITokenClass`-typed token - converting it would have made things *worse*, replacing a resolution that
perfectly preserves the singleton with one that reconstructs a disconnected copy. `HumanToken` isn't
`ISpaceEntity` at all; it's an immutable value type with its own dedicated `SerializeHumanToken`/
`DeserializeHumanToken` pair (built in an earlier pass) precisely because it has no singleton identity to
preserve. Both were removed from the "not yet serializable" count as a result - they're not gaps, they were
just miscategorized as needing the same treatment as everything else.

`TokenVariety` and `Incarna` were the 2 genuine gaps. `TokenVariety` (`TokenClassToken original, string badge`)
implements neither `ITokenClass` nor `ISerializableSpaceEntity`, so a bare `TokenVariety` held in an `IToken`
field (e.g. `FreezePresence`'s `beast`/`badland` fields, built via `new TokenVariety(Token.Beast, badge)` in
`SettleIntoHuntingGrounds`) would throw inside `SerializeToken`. `ToJson` now captures `original.Label` +
`badge`; `FromJson` resolves `original` via `TokenClassByLabel` and reconstructs. This also unblocks
`FreezePresence`, previously flagged as blocked on exactly this gap - not converted in this pass, but no
longer structurally blocked.

`Incarna` implements `ITokenClass` too, but unlike `TokenClassToken` its instances are per-spirit (constructed
at spirit setup, not registered as `Token`/`Human` static fields), so `TokenClassRegistry` can never resolve
one by label - a latent bug if `SerializeToken` ever hit a real `Incarna` value. Since `SerializeToken` checks
`ISerializableSpaceEntity` *before* falling back to the generic `ITokenClass` branch, implementing it on
`Incarna` fixes that gap by taking priority. `ToJson` captures the owning spirit's index + `Empowered`;
`FromJson` resolves via `ctx.SpiritAt(idx).Presence.Incarna` - the *same* instance the spirit already owns,
not a fresh one, since `SpiritPresence.Incarna` is a stable per-spirit property. `_spaceCounts` (an
`ITrackMySpaces` cache) isn't captured, same exception as `SpiritPresenceToken`.

core is now fully serialized.

**Sixteenth pass — `FreezePresence`:** the conversion `TokenVariety` unblocked in the fifteenth pass.
`ToJson` captures `_name`, the owning spirit's index (via `_presence.Token.Self`, same pattern as
`PresenceCountDefend`/`DefendWherePresent`), and `beast`/`badland` via the polymorphic `IToken`
resolver (`ctx.SerializeToken`/`DeserializeToken`) - they're `TokenVariety` instances at runtime,
which only became serializable in the prior pass. Added a test that round-trips a real
`FreezePresence`, pulls the restored instance's own `beast` field back out via reflection (primary
constructor parameters compile to a `<name>P`-named backing field), and feeds it through
`ModifyRemovingAsync` to prove the reference-equality check inside `IsFrozen` still recognizes it -
a fresh unrelated `TokenVariety` with the same class/badge wouldn't have passed that check.

**Seventeenth pass — `InvadersSitOut`:** the user refactored it directly (`TerrifyingRampage.cs`) before
this pass started - the stored `Quota _quota` field is gone entirely, replaced with a hardcoded
`new Quota().AddGroup(2, Human.Invader)` built fresh inside `Config()` each time, the same "hardcoded,
no need to persist" pattern as the `SkipBuild_Custom` subclasses. That sidesteps the `Quota`-serialization
gap completely rather than solving it, leaving just a plain `Spirit` reference - the same shape as the very
first "Spirit-only cluster" converted at the start of this whole effort. `ToJson`/`FromJson` follow that
exact pattern: capture `ctx.IndexOf(_invaderPicker)`, resolve via `ctx.SpiritAt(idx)`.

NatureIncarnate is now fully serialized.

**Eighteenth pass — `IntensifyThroughUnderstanding` split in two:** the dual-registration blocker
(`Spirit.Mods` for Air/Water, island mods for everything else) turned out to have no real coupling
behind it - the two halves only ever touched shared external state (`_spirit.PreparedElementMgr`),
never each other's fields. Split into `IntensifyAirWater` (Air/Water/`IConfigureMyActions`/
`IInitializeSpirit` - stays in `Spirit.Mods`, not `ISpaceEntity`, same permanently-out-of-scope bucket
as `MarkedBeastMover`, since nothing restores `Spirit.Mods` today) and `IntensifyThroughUnderstanding`
(Moon/Sun/Fire/Plant/Animal/Earth - a pure island mod). `IntensifyAirWater.Initialize()` constructs and
registers the island-mod half once the game exists, replacing the single object's dual registration.
The island-mod half is now just a plain `Spirit` reference - the same shape as `ToDreamAThousandDeaths`
or any other single-`Spirit` type in the catalog - so it converts trivially.

Also folded in along the way: the `RoundScope`-backed `_usedCount` counter that gated the Air ability
was removed entirely (not just fixed for serialization). It turned out to be enforcing a stricter rule
than the card actually states ("max 1 of each Marker per Action") - capping Air to once *per round*
regardless of how many separate actions might occur, rather than once per action the way every other
element already worked. Each Air spent is inherently tied to exactly one card being made playable and
then played as one action, so `PreparedElements[Element.Air]` decrementing on use is the only cap
actually needed; no separate counter, no `RoundScope`, no round-reset hook required. See the
explanatory comment left in `IntensifyAirWater.cs` for the reasoning.

Every `ISpaceEntity` type in the whole solution is now serializable. **103 of 103.**

**Nineteenth pass — `FrightfulShadowsEludeDestruction` (not part of the 103 count):** a real gap the
original catalog sweep simply missed - it's `ISpaceEntity` via `SpiritPresenceToken`, but had zero
`[ModuleInitializer]` registration at all, no `ToJson` override, nothing. Never flagged in this doc,
never counted toward the 105/103 totals at any point. Would have thrown `KeyNotFoundException` if
`SpaceEntitySerialization.Deserialize` ever hit it, rather than losing data quietly. Fixed the same way
as `GatewayToken`/`PresenceCountDefend`: `SpiritPresenceToken`'s base `ToJson` only captures `Self`
(fine for subclasses with no extra state), but this one now has `UsedThisRound` too (added when its
`RoundScope` usage was removed), so it needs its own override - `ToJson` captures `Self` +
`UsedThisRound`, `FromJson` resolves via `ctx.SpiritAt(idx).Presence.Token` (the same instance the
spirit already owns) rather than constructing a fresh one.

**Changelog vs. the original 98-type catalog:** 14 delegate-holding types were converted to abstract bases + named subclasses (or, for `BeforeInvaderPhase`, switched to holding an existing `IActOn<GameState>` reference instead of a `Func<>`). Net effect: `RavageBehavior` and the mods that chain its two delegate fields (`StopRavageIfTooFewExplorers`, `MarkedBeast`) were deliberately left untouched; everything else in that batch dropped out of **High** into **Low**/**Medium**, and `TokenRemovedHandlerAsync`/`TokenRemovedHandlerAsync_Persistent`/`TokenRemovedHandler` were deleted outright once their 8 call sites were migrated to dedicated subclasses.

**Second pass — `_boardCounts` removal + `ITrackMySpaces` cache exception:** `SpiritPresenceToken._boardCounts` was removed; `IsOnIsland`/`IsOn(Board)` are now derived from `_spaceCounts` on demand instead of maintaining a second tracking dictionary. Separately, once `ITrackMySpaces` caches were recognized as derived/rebuildable (see rubric exception above), `SpiritPresenceToken`, `FollowingPresenceToken`, `Incarna`, and the subclasses whose *only* High-triggering state was that inherited cache (`ChokeTheLandWithGreen`, `LandsHostileToHumanity`, `ASingleAluringLair`, `EnthrallTheForeignExplorers`, `ToweringRootsIncarna`) all dropped to **Medium**. `AJoiningOfSwarmsAndFlocks` stayed High at the time — its tracking cache was no longer the issue, but it still owned a nested `ManyMindsBeast` entity (fixed in the eleventh pass below).

**Third pass — `SaveDahan`:** `_byAction` (`CountDictionary<ActionScope>`) was replaced with `ActionScope`'s own ad-hoc dictionary for the per-action running count (same pattern as `OneTimeDamageBoost`), and the "only the first action counts" behavior became self-removal via `AtEndOfThisAction` instead of a distinct-actions counter. The stored `Space` field was then dropped too — the space is now taken from `args.From` at call time rather than held as state. Result: `SaveDahan` went from High straight to **Low**.

**Fourth pass — `GatewayToken`:** the two stored `Space` fields were replaced with their `SpaceSpec` identifiers, resolved back to a live `Space` on demand via `ActionScope.AccessTokens()` — the same "store the stable ID, not the live object" fix as `SaveDahan`. This works because `Space.Equals`/`==` already compares by `SpaceSpec`, and `Space` instances are cheap, recreated-on-access wrappers rather than stable singletons, so nothing was actually lost by not holding onto one. `GatewayToken` moved from High to **Medium** (still holds a `SpiritPresenceToken` reference).

**Fifth pass — `RavageBehavior`:** its two delegate fields, `GetDamageFromParticipatingAttackers` and `RavageSequence`, are both gone. `GetDamageFromParticipatingAttackers` became `List<IAdjustAttackerDamage> DamageAdjusters`, folded over the default calculation in registration order — its 3 customizers (`NeighborTownsCauseBonusLandDamage`, `AdjustDamageFromAttackers`'s subclasses, `ExtraDamage`) now just append themselves instead of capturing/wrapping the old delegate. `RavageSequence` became `List<IRavageSequenceStep> SequenceSteps`, built into a call chain fresh on each `Exec()` (each step gets handed `next` directly rather than having to save it) — its 2 customizers (`StopRavageIfTooFewExplorers`, and `InstrumentsOfTheirOwnRuin`'s new `DamageInvadersInAdjacentLands`) migrated cleanly: the "conditionally defer" case just calls or skips `next()`, the "fully replace" case just never calls it. `TargetSpaceCtx.ModifyRavage` was removed — it had exactly one caller, which no longer needs it. `RavageBehavior` moved from High to **Medium**; `StopRavageIfTooFewExplorers` moved from High to **Low** (no fields left at all).

**Sixth pass — `SkipLowestNumberedExplore`:** same `Space` → `SpaceSpec` swap as `GatewayToken`/`SaveDahan`. Its `Dictionary<Board,Space>` cache was memoized once per instance from live `GameState` and never mutated after, so the fix was purely mechanical — no design questions, no cross-action bookkeeping. Moved from High straight to **Low**.

**Seventh pass — `DealVengeanceDamageOnDestroy`:** same `Space` → `SpaceSpec` swap as `GatewayToken`/`SaveDahan`/`SkipLowestNumberedExplore`, applied to a list that's appended to *after* construction rather than a single field. Resolved via `ActionScope.Current.AccessTokens(spec)` right before filtering `HasInvaders()`. Added 3 dedicated tests (`VengeanceOfTheDead_Tests.cs`) — notably one that destroys the only invader in *target* land while a second invader sits in an *adjacent* land, forcing resolution to a `SpaceSpec` other than the one the mod was created with, which is the actual scenario this fix needed to get right. Moved from High to **Medium**.

**Eighth pass — `DoMistsOnDestroy` renamed to `FearOnTownCityDestroy`:** same `SaveDahan`-style fix — the stored `ActionScope originalScope` comparison is replaced with self-removal via `AtEndOfThisAction`, registered once at construction (the space is already known then, unlike `SaveDahan` which registered lazily on first trigger). Moved from High to **Medium**.

**Ninth pass — `ObserveWorldMod`:** same `ActionScope`-ad-hoc-dictionary trick as `SaveDahan`/`FearOnTownCityDestroy`, but this one genuinely needs multi-action memory (it applies across up to 3 qualifying actions, not just the one it was created in), so the self-removal shortcut didn't apply here — only the storage mechanism changed: `HashSet<ActionScope>` → a per-instance GUID-based key checked via `ActionScope.ContainsKey`/`SafeSet`. Moved from High to **Medium**.

**Tenth pass — `MarkedBeast`:** `IActionFactory` was extracted into a new `MarkedBeastMover` class, which replaces the `Spirit.EnergyCollected` delegate subscription with `Spirit.Mods` registration (`IModifyAvailableActions`, extends `ISpiritMod`) — the same established pattern several other spirit mods already use (e.g. `StrandedInTheShiftingMists`), checking `Spirit.UsedActions` directly instead of needing a "re-add myself every round" delegate callback. `MarkedBeast` no longer stores a `Spirit` reference at all (only passes it through to `MarkedBeastMover`'s constructor), and its `Space?` field became `SpaceSpec?` (same fix as `GatewayToken`/`SaveDahan`/etc., resolved via `ActionScope.AccessTokens()`). `MarkedBeast` dropped from High straight to **Low**. `MarkedBeastMover` itself isn't `ISpaceEntity` (it lives in `Spirit.Mods`, not on a `Space`), so it isn't part of this catalog.

**Eleventh pass — `AJoiningOfSwarmsAndFlocks` / `ManyMindsBeast`:** the last remaining circular reference. `ManyMindsBeast` held the `AJoiningOfSwarmsAndFlocks` that created it directly; `AJoiningOfSwarmsAndFlocks` held the `ManyMindsBeast` it created directly — neither could be constructed first from JSON. Broken by having `ManyMindsBeast` hold a `Spirit` reference instead and look up its owning presence token on demand via `(AJoiningOfSwarmsAndFlocks)spirit.Presence.Token` (always valid, since `ManyMindsMoveAsOne` always constructs its `SpiritPresence` with `AJoiningOfSwarmsAndFlocks` as the token). Both types moved from High to **Medium**.

**The catalog now has zero remaining High-complexity types.** All 106 `ISpaceEntity` types are Low or Medium.

## Serialization implementation (separate from the rating above)

The ratings above describe how *hard* a type would be to serialize; actually implementing it is a
separate, ongoing effort living in `SpiritIsland/Serialization/` (`ISerializationContext`,
`ISerializableSpaceEntity`, `SpaceEntitySerialization`). A type's row here doesn't change when it
gets implemented - only when its complexity changes.

**Trial (3 types):** `TerrorStalksTheLand` (bare `Spirit` ref), `MistsOfOblivion.FearOnTownCityDestroy`
(`TargetSpaceCtx` + mutable budget), `SkipBuild` (array of `ITokenClass` singletons via a new
label-keyed `TokenClassRegistry`) - proved the DTO-as-`JsonArray` + `[ModuleInitializer]`-registered
reader design end-to-end, with round-trip tests that check behavior, not just JSON shape.

**Spirit-only cluster (22 types):** every Medium type whose entire state is a `Spirit` reference (plus,
for a few, one extra mutable field) now implements `ISerializableSpaceEntity`:
`SpiritPresenceToken`, `ChokeTheLandWithGreen`, `LandsHostileToHumanity`, `EnthrallTheForeignExplorers`,
`AJoiningOfSwarmsAndFlocks`, `ManyMindsBeast`, `ASingleAluringLair`, `ToweringRootsIncarna`,
`ToDreamAThousandDeaths`, `DestroyNearbyPresenceOnDahanDestroyed`, `ResponsibilityToTheDeadMod`,
`MistPusher`, `ARealFlairForDiscord`, `CleaningUpMessesIsADrag`, `TerrorOfASlowlyUnfoldingPlague`,
`EachDahanAtRange1Defend1`, `StopPresenceDestructionFromBlightOrEvents`, `SenselessRoaming`,
`SpreadTumultAndDelusion`, `RoilingWaters.Mod`, `SereneWaters.Mod`, `DamageNewInvadersOnce`.

Two design points this batch settled:
- `SpiritPresenceToken.ToJson` is `public virtual`, tagging with `GetType().Name`. Subclasses that add
  no state beyond `Self` (`ChokeTheLandWithGreen`, `LandsHostileToHumanity`, `EnthrallTheForeignExplorers`,
  `AJoiningOfSwarmsAndFlocks`) only need their own `[ModuleInitializer]` reader registration, not their
  own `ToJson` override - one implementation covers the whole family.
- `Incarna.Empowered` is real mutable state (not derivable from the constructor), so `Incarna` itself
  wasn't given a shared `ToJson` the way `SpiritPresenceToken` was. Its two qualifying subclasses
  (`ASingleAluringLair`, `ToweringRootsIncarna`) each capture `Empowered` as an explicit extra array
  element instead. Same treatment for `DamageNewInvadersOnce`'s one-shot `_used` flag.

Several private-nested mod classes (`DestroyNearbyPresenceOnDahanDestroyed`, `ResponsibilityToTheDeadMod`,
`MistPusher`, `EachDahanAtRange1Defend1`, `RoilingWaters.Mod`, `SereneWaters.Mod`) had to become `public`
for the same reason as the trial's `TerrorStalksTheLand`/`FearOnTownCityDestroy`: no `InternalsVisibleTo`
between the game projects and `SpiritIsland.Tests`.

**Known limitation:** `AJoiningOfSwarmsAndFlocks.FromJson` reconstructs via `new AJoiningOfSwarmsAndFlocks(spirit)`,
which creates a *fresh* `ManyMindsBeast` rather than resolving whatever specific instance is already
tracked elsewhere on the board. Fine for an isolated round-trip test; a full board restore needs the
two-phase, id-based cross-reference wiring described earlier in this doc, not this shortcut.

**`TargetSpaceCtx` cluster (5 more types, 6 of 7 total):** `Add1FearForFirstDestroyedInvader`
(`PortentsOfDisaster`), `AddBeastAndDamageOnInvaderDestroyed` (`BloodDrawsPredators`), `ObserveWorldMod`,
`SkipAndDamageInvaders` (`InfestationOfVenomousSpiders`), `DealVengeanceDamageOnDestroy`
(`VengeanceOfTheDead`) - plus `FearOnTownCityDestroy` from the original trial. Same resolution mechanism
throughout: `ctx.SpaceSpec.Label` out, `SpaceSpecByLabel` + `TargetSpace(spirit, spec)` back in.

Two more extra-state cases, same treatment as `Incarna.Empowered`:
- `Add1FearForFirstDestroyedInvader`'s one-shot `_addFear` bool.
- `ObserveWorldMod`'s mutable `_tokenSummary` string. Its `_key` (a per-instance GUID used only as an
  `ActionScope`-scoped dedup guard) is *not* captured - a fresh GUID on deserialize is exactly as
  correct as the original, since that state was never meant to outlive the current action anyway.
  `ObserveWorldMod` also needed a new field, `readonly SpaceSpec _spaceSpec = ctx.SpaceSpec`, purely so
  serialization has something to read - the original code never stored the ctx's space beyond
  construction, since every runtime use gets its `Space` from event args instead.

`DealVengeanceDamageOnDestroy` introduced the first *array of `SpaceSpec`* case: `landsWeCanApplyTheDamageTo`
is a `List<SpaceSpec>` that `VengeanceOfTheDead.ActAsync` appends to *after* constructing the mod (if the
3-animal threshold is met). `ToJson` just serializes whatever the list currently holds; the round-trip test
specifically captures it after the append, to prove the extra land survives, not just the original one.

`SkipAndDamageInvaders` subclasses `SkipAnyInvaderAction` (not yet converted - it's one of the small
`Spirit`+`string` shape family types). No base-class changes were needed: `SkipAndDamageInvaders`
implements `ISerializableSpaceEntity` directly on itself, same as `MarkedBeast`-era pattern of a subclass
opting in independently of its base.

**Originally skipped, now solved: `TriggerAfterNoRavageOrBuild`** (`UnearthABeastOfWrathfulStone`). Its
`IToken beastToken` field is polymorphic - either `Token.Beast` (a stable `ITokenClass` singleton) or a
specific `MarkedBeast` instance (a per-game entity). Solved with two new default methods on
`ISerializationContext`:

```csharp
JsonArray SerializeToken( IToken token ) => token switch {
    ISerializableSpaceEntity entity => new JsonArray( "Entity", entity.ToJson( this ) ),
    ITokenClass tokenClass => new JsonArray( "Class", tokenClass.Label ),
    _ => throw new NotSupportedException( ... )
};

IToken DeserializeToken( JsonArray json ) => json[0]!.GetValue<string>() switch {
    "Class" => (IToken)TokenClassByLabel( json[1]!.GetValue<string>() ),
    "Entity" => (IToken)SpaceEntitySerialization.Deserialize( (JsonArray)json[1]!, this ),
    var kind => throw new NotSupportedException( ... )
};
```

`ISerializableSpaceEntity` is checked first: any per-game entity that opts in gets nested (nesting works
because `SpaceEntitySerialization.Serialize`/`Deserialize` don't care whether they're called at the top
level or from inside another type's `ToJson`/`FromJson`); `ITokenClass` is the fallback for stable
singletons. This unblocked `MarkedBeast` too, which needed one thing it didn't have: a way to reconstruct
itself. Its constructor takes a `Spirit` but never stored it (only `MarkedBeastMover` needed it), so a
`readonly Spirit _controlSpirit` field was added purely for `ToJson` to read - the same "extra field with
no runtime purpose" pattern as `ObserveWorldMod`'s `_spaceSpec`.

**New caveat surfaced by this:** `MarkedBeast.FromJson` calls the real constructor, which re-registers a
*fresh* `MarkedBeastMover` into `Spirit.Mods`. Fine for an isolated round-trip (a fresh `GameState` each
time), but for a full board restore, the original `MarkedBeastMover` mustn't also survive somewhere else -
same shape as the caveat already on `AJoiningOfSwarmsAndFlocks`'s beast token. Both point at the same
underlying gap: reconstructing-via-constructor is fine for standalone types, but anything that has side
effects on construction (registering into a collection, wiring a delegate) needs the two-phase, id-based
approach once whole-board restores are in scope, not "just call `new`."

**`Spirit`+`string` family (5 types):** `SkipAnyInvaderAction`, `ConvertFearToDefense` (`DreadApparitions`),
`SkipRavageOrBuild` (`SwelteringExhaustion`), `MyPowersDontDamageDahanThisRound`, `DestroyPresenceInsteadOfAddingBlight`
(both `ConsiderAHarmoniousNature`). All straightforward - a spirit index plus a plain string, no extra
mutable state. `SkipAnyInvaderAction` is directly instantiated in its own right (via `Skip1InvaderAction`),
not just subclassed by the already-converted `SkipAndDamageInvaders`, so it got its own registration too.

**`Spirit`+`int` family (4 types):** `MudToken`, `OneTimeDamageBoost` (`GiftOfFuriousMight`),
`PayEnergyToTakeFromBox`, `DahanSitOutRavage` (`RoilingBogAndSnaggingThorn`). Three were trivial; the
fourth surfaced a real trap:

`OneTimeDamageBoost.UsedThisRound` is backed by `GameState.Current.RoundScope[_key]`, where `_key` is a
per-instance GUID - the same shape as `ObserveWorldMod`'s dedup key, which we'd established doesn't need
capturing. But that reasoning only holds for *`ActionScope`*-scoped guards, which are meaningless once
the current action ends (and serialization only ever happens between actions). `RoundScope` persists
across the *whole round*, so `UsedThisRound` is real, meaningful state - if `_key` were regenerated fresh
on deserialize without transferring the value, a restored instance would silently forget it had already
granted its bonus this round, allowing it to fire twice. Fixed by reading/writing `UsedThisRound` through
its own property (already exposed, just made `public`) rather than trying to preserve `_key` itself - the
*value* transfers correctly to whatever key the new instance happens to get.

## The remaining ~22 bespoke types

The doc's earlier "~35 remaining" estimate was stale. The actual remainder was 22 types with no
shared shape - each needed either a small extension of an existing pattern or a genuinely new piece
of resolution infrastructure. 13 were converted; 5 were deliberately left unconverted; 4 turned out
not to need their own `ISerializableSpaceEntity` at all.

**New infrastructure added to `ISerializationContext`:**
- `BoardByName(string)` - `Board.Name` is already a stable, unique identifier (used for board
  selection UI elsewhere), so this needed no new concept, just a lookup.
- `SerializeHumanToken`/`DeserializeHumanToken` - `HumanToken` is an immutable value (not a
  singleton, not an entity with identity), so it's written as its own small array
  (`[classLabel, rawFullHealth, damage, strifeCount, attack, ravageOrder, ravageSide]`) rather than
  resolved by reference. Reconstructed by chaining `HumanToken`'s existing public "wither" methods
  (`AddDamage`, `HavingStrife`, `SetAttack`, `SetRavageOrder`, `SetRavageSide`) from a base token -
  its `Props`-based constructor is `protected`, so no changes to `HumanToken` itself were needed.

**Converted (13):** `BreakThemselvesMod`, `WarnToken`, `Drowning` (`Spirit`+primitives, one with a
real mutable accumulator); `ReduceAttack` (array of `HumanTokenClass`, same pattern as `SkipBuild`);
`FollowingPresenceToken` (a `SpiritPresenceToken` subclass with real extra state, so it overrides the
inherited `ToJson` rather than relying on it); `GatewayToken`, `PresenceCountDefend`,
`DefendWherePresent` (each resolves a `SpiritPresenceToken`/`SpiritPresence` field via
`presence.Token.Self` or `presence.CountOn`'s owning spirit, rather than nesting the presence
object's own serialization - since a spirit has exactly one presence, an index is enough, and it
avoids reconstructing a second instance that's different from the one the spirit itself tracks, the
same identity trap flagged for `AJoiningOfSwarmsAndFlocks`); `InvadersSkip1Board`, `RecordBlightAdded`
(the two consumers of `BoardByName`); `SlowBlightMod` (trivial - its only two fields are both
deterministically derived from `GameState`, so `ToJson` carries no data at all beyond the tag);
`InvadersDontParticipateInRavage`, `DreamingDamageLog` (the two consumers of `SerializeHumanToken`).

**Deliberately not converted (4 at the time; now 0 - see the fifteenth/sixteenth/seventeenth/eighteenth
passes above), each blocked on a real gap rather than a shortcut:**
- ~~`FreezePresence`~~ - **converted in the sixteenth pass.** Its `beast`/`badland` fields are
  `TokenVariety` instances that are *also* placed directly on many spaces by the same card - the real
  blocker was `TokenVariety` itself not being independently resolvable, not `FreezePresence`'s own
  shape. Fixed once `TokenVariety` became `ISerializableSpaceEntity` (fifteenth pass): `beast`/`badland`
  now round-trip through the polymorphic `IToken` resolver like any other `IToken` field.
- ~~`IntensifyThroughUnderstanding`~~ - **converted in the eighteenth pass**, by splitting it. It was
  registered in *two* different places (`Spirit.Mods` and `GameState`'s island mods) because it did two
  genuinely unrelated jobs; once split into `IntensifyAirWater` (stays in `Spirit.Mods`, out of scope,
  same as `MarkedBeastMover`) and a slimmed-down `IntensifyThroughUnderstanding` (pure island mod, just
  a `Spirit` reference), the remaining half converts the same as any other single-`Spirit` type.
- ~~`BeforeInvaderPhase`~~ - not converted, but not because of the gap described below either: the
  class itself was deleted entirely once the blight-card refactor (see the "Correction" section
  further down) left it with zero callers. Wrapped an arbitrary `IActOn<GameState> cmd`, used
  generically by `AtTheStartOfEachInvaderPhase()` for whatever command a card/adversary author handed
  it - no stable set of "kinds" to discriminate the way `SerializeToken` does for `IToken`, would have
  needed a much broader command registry to do properly, had it survived.
- ~~`InvadersSitOut`~~ - **converted in the seventeenth pass**, but not by solving `Quota` serialization.
  The user refactored it to stop holding a `Quota` field at all - the group it built (`2, Human.Invader`)
  was always the same hardcoded value, so it's now constructed fresh inside `Config()` instead of stored.
  `Quota`/`QuotaGroup`'s internals (`_quotaGroups`, per-group `_count`) are still entirely private with no
  public enumeration - that gap is sidestepped here, not closed; worth solving properly if a future
  `Quota`-holding type actually needs a *variable* quota captured.

**Not independently serialized, and don't need to be (2 - see correction below, this used to say 4):**
`TokenClassToken` is always a stable singleton, resolved via `TokenClassRegistry` like any other
`ITokenClass` - converting it would replace that with a disconnected reconstructed copy, strictly worse.
`HumanToken` doesn't implement `ISerializableSpaceEntity` - it's handled by the value-serialization
helpers above instead, since it's data, not an entity with its own identity.

## Correction: `TokenVariety` and `Incarna` *did* need independent serialization

The paragraph above originally also listed `TokenVariety` and `Incarna` as "don't need to be" - reasoning
that `TokenVariety` was always either a stable singleton or freshly reconstructed by whatever other type
built it, and that `Incarna` was "never directly instantiated". Both turned out to be wrong once
`FreezePresence` was looked at directly (fifteenth pass, see the Summary section above): `SettleIntoHuntingGrounds`
builds bare `TokenVariety` instances and places them on several spaces directly, independent of any owning
entity, and `Incarna` instances are placed on spaces via `Incarna.MoveTo` in their own right. Both now
implement `ISerializableSpaceEntity` - `TokenVariety` resolved by label, `Incarna` resolved back to the
exact instance its owning `Spirit` already holds via `ctx.SpiritAt(idx).Presence.Incarna`. `TokenClassToken`
and `HumanToken` remain correctly excluded.

## Correction: `RavageBehavior` is placed exactly like any other `ISpaceEntity`

An earlier version of this doc said `RavageBehavior` was "not clearly an independently-placed
`ISpaceEntity`... more a property of how a `Space` processes a ravage than a token sitting on it."
That was wrong. It's placed via the exact same `Init(mod, count)` every other mod uses - just
*lazily*, from `Space.RavageBehavior`'s getter, rather than eagerly by a card action:

```csharp
public RavageBehavior RavageBehavior {
    get {
        var mod = Keys.OfType<RavageBehavior>().FirstOrDefault();
        if(mod == null) {
            mod = RavageBehavior.GetDefault();   // clones a static default template
            Init( mod, 1 );                       // placed on the space right here
        }
        return mod;
    }
}
```

The first thing that touches `space.RavageBehavior` (a card registering a `SequenceStep`, or the
ravage itself executing) clones the static default and places the clone on that specific space. It
implements `IEndWhenTimePasses`, so it's wiped at round end along with every other round-scoped mod,
same as everything else - a space with no customization just re-clones a fresh default next time.

The real blocker was narrower than the original note implied: `SequenceSteps: List<IRavageSequenceStep>`
and `DamageAdjusters: List<IAdjustAttackerDamage>` hold *other mod objects*, and those needed the same
kind of polymorphic resolution `SerializeToken`/`DeserializeToken` already solved for `IToken` - just
for two different interfaces. Since every current implementer of both interfaces is a specific mod
instance (never a stable singleton the way `Token.Beast` is for `IToken`), `RavageBehavior.ToJson`
doesn't need a "Class vs. Entity" discriminator - it just requires `ISerializableSpaceEntity` directly
and nests each member's own `ToJson`:

```csharp
JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext ctx ) => new JsonArray(
    Tag,
    new JsonArray( SequenceSteps.Select( s => (JsonNode)SerializeMember( s, ctx ) ).ToArray() ),
    new JsonArray( DamageAdjusters.Select( a => (JsonNode)SerializeMember( a, ctx ) ).ToArray() ),
    AttackersDefend
);
```

All 6 current implementers were converted: `DamageInvadersInAdjacentLands` (`InstrumentsOfTheirOwnRuin`,
holds a `TargetSpaceCtx`, same resolution as the earlier `TargetSpaceCtx` cluster), `NeighborTownsCauseBonusLandDamage`,
`StopRavageIfTooFewExplorers`, `ExtraDamage` (all parameterless), and `AdjustDamageFromAttackers`'s two
concrete subclasses (`ReduceAttackBy6Mod`, `ReduceAttackByReceivedDamageMod`) - since both are
parameterless, `AdjustDamageFromAttackers` got the same shared, `GetType().Name`-tagged `ToJson` as
`SpiritPresenceToken`, so each subclass only needed its own `[ModuleInitializer]` registration.

`RavageBehavior` moved from "flagged, not converted" to fully serializable - **55 of 62** Medium types
are now converted. Its 6 list members are Low-complexity types outside that count (most were already
rated Low, not Medium, in the original catalog), but had to become `ISerializableSpaceEntity` too,
since `RavageBehavior` can't nest a member's `ToJson` unless the member has one.

## Correction: `IRunBeforeInvaderPhase` should never have extended `ISpaceEntity`

`SlaveRebellion` (below) was catalogued as a Low-complexity `ISpaceEntity` type, and offering the same
treatment to blight cards that implement `IRunBeforeInvaderPhase` (`DownwardSpiral` and 7 others, once
they stopped going through the generic `BeforeInvaderPhase` wrapper - see the blight-card refactor)
was the natural next suggestion. It was wrong. Neither is ever placed on a `Space`; both are registered
directly on `GameState` via `AddPreInvaderPhaseAction`, processed by `PreInvaderPhaseActionList`
(`SpiritIsland/Core/ActionList.cs`), which uses the unrelated `IHaveMemento` save/restore mechanism -
nothing about that path touches a `Space`'s token dictionary or needs `ISpaceEntity` at all.

The actual cause: `IRunBeforeInvaderPhase` (and, it turns out, `IRunAfterInvaderPhase` too, though it
wasn't touched here since its one known implementer, `TriggerAfterNoRavageOrBuild`, genuinely *is*
space-placed) declared `: ISpaceEntity` for no functional reason - nothing in `ActionList<T>`/
`PreInvaderPhaseActionList` requires it. That made every implementer satisfy `ISpaceEntity` by C# type
hierarchy regardless of whether it was ever stored on a `Space`, which is exactly the ambiguity
`ISpaceEntity`'s own doc comment ("marker that means it can be stored on a Space") was supposed to
prevent. Fixed by dropping `: ISpaceEntity` from `IRunBeforeInvaderPhase`. `SlaveRebellion` is removed
from the catalog below; the 8 blight cards were never added to it. Total dropped from 106 to **105**.

If a type is `IRunBeforeInvaderPhase`-only (no other reason to be `ISpaceEntity`), it's out of scope for
this effort, for the same reason: it's never stored on a `Space`, so there's no per-space serialization
problem to solve for it. `IRunAfterInvaderPhase`'s `: ISpaceEntity` was deliberately left alone rather
than "fixed" the same way, since it's plausible other implementers besides `TriggerAfterNoRavageOrBuild`
are genuinely space-placed and this doc hasn't audited all of them - worth the same check before
assuming it's fine, not worth assuming it's broken without checking.

## Remaining types (not yet serializable)

**None. Every `ISpaceEntity` type in the solution implements `ISerializableSpaceEntity` - 103 of 103.**
(Grep `SpaceEntitySerialization.Register` in source to confirm the current, authoritative count; this
includes `TokenClassToken` and `HumanToken`, which are "done" via alternate mechanisms rather than
`ISerializableSpaceEntity` itself - see the fifteenth pass above.) Two entries from the old "Full
catalog" were removed outright rather than converted: `BeforeInvaderPhase` (the class itself was
deleted once the blight-card refactor left it with zero callers) and `LandStrippedBare`
(`IRunBeforeInvaderPhase`-only, same as `SlaveRebellion` - see the correction above; never placed on a
`Space`, out of scope for this effort). `IntensifyAirWater` (split from `IntensifyThroughUnderstanding`
in the eighteenth pass) is deliberately not `ISpaceEntity` either - it lives in `Spirit.Mods`, which has
no restoration mechanism today, same as `MarkedBeastMover`.

### SpiritIsland (core) - 0 remaining

Every `ISpaceEntity` type in this project is already serializable.

### SpiritIsland.Basegame - 0 remaining

Every `ISpaceEntity` type in this project is already serializable.

### SpiritIsland.BranchAndClaw - 0 remaining

Every `ISpaceEntity` type in this project is already serializable.

### SpiritIsland.FeatherAndFlame - 0 remaining

Every `ISpaceEntity` type in this project is already serializable.

### SpiritIsland.Horizons - 0 remaining

Every `ISpaceEntity` type in this project is already serializable.

### SpiritIsland.JaggedEarth - 0 remaining

Every `ISpaceEntity` type in this project is already serializable.

### SpiritIsland.NatureIncarnate - 0 remaining

Every `ISpaceEntity` type in this project is already serializable.
