# `ISpaceEntity` Type Catalog

`ISpaceEntity` (`SpiritIsland/Tokens/Interfaces/ISpaceEntity.cs`) is the marker interface for "anything that can be stored on a `Space`" — both visible game tokens (`IToken`) and invisible rule-modifying "mods" (`BaseModEntity`, plus several hook interfaces that extend `ISpaceEntity` directly: `IConfigRavages`, `ISkipBuilds`, `ISkipRavages`, `ISkipExploreTo`, `ISkipExploreFrom`, `IRunBeforeInvaderPhase`, `IRunAfterInvaderPhase`, `ICleanupSpaceWhenTimePasses`).

This catalog lists every concrete class in the solution that implements `ISpaceEntity`, directly or transitively, and rates how hard it would be to serialize an instance to/from JSON. It exists to support the "Save state by serializing it" goal noted in `todo.txt`.

## Methodology & caveats

Compiled via a project-by-project source sweep (grep for all `ISpaceEntity`-family base types/interfaces, then read each matching file), with spot-checks against the source for accuracy. Treat this as a strong starting point, not gospel — re-verify a class against current source before depending on its rating for actual serialization design, since the codebase will keep moving.

Excluded: abstract/interface types themselves, and hook-interface implementers whose only qualifying base is a class already listed here (e.g. `SpiritPresenceToken`/`Incarna` subclasses are listed once, not re-derived).

## Complexity rubric

| Rating | Meaning |
|---|---|
| **Low** | Parameterless, or constructor takes only primitives/enums/strings. All state is simple owned data — no references to other live game objects, no delegates. |
| **Medium** | Constructor/state references other stable, identifiable domain objects (a `Spirit`, a static token-class singleton like `Token.Beast`/`Human.Dahan`, a `HumanToken` value, a `CountDictionary` of simple keys). Serializable via a reference/lookup scheme; no closures. |
| **High** | Constructor or fields capture a delegate (`Func<>`/`Action<>`/lambda closure), self-register as an event handler, or hold a live/mutable object graph (`GameState`, `TargetSpaceCtx`, `ActionScope`, `Space`/`Board` collections, another entity) that isn't naturally serializable as data without a redesign. |

## Summary

| Project | Low | Medium | High | Total |
|---|---:|---:|---:|---:|
| SpiritIsland (core) | 7 | 5 | 14 | 26 |
| SpiritIsland.Basegame | 4 | 4 | 1 | 9 |
| SpiritIsland.BranchAndClaw | 3 | 2 | 2 | 7 |
| SpiritIsland.FeatherAndFlame | 1 | 1 | 0 | 2 |
| SpiritIsland.Horizons | 0 | 2 | 0 | 2 |
| SpiritIsland.JaggedEarth | 11 | 12 | 7 | 30 |
| SpiritIsland.NatureIncarnate | 5 | 11 | 4 | 20 |
| SpiritIsland.Tests | 1 | 0 | 1 | 2 |
| **Total** | **32** | **37** | **29** | **98** |

The single biggest driver of **High** ratings is the token/presence hierarchy (`SpiritPresenceToken`, `Incarna`, and their subclasses) — they carry live `CountDictionary<Space>`/`CountDictionary<Board>` tracking caches that aren't plain data. The second biggest driver is mods that capture a `Func<>`/`Action<>` closure instead of plain configuration (common in one-off spirit powers built with lambdas).

## Full catalog

### SpiritIsland (core)

| Type | Constructor Parameters | Complexity | Notes |
|---|---|---|---|
| `TokenClassToken` | `string label, char spaceAbrev, Img img, string? badge = null` | Medium | Used as a reference-identity singleton (`Token.Beast`, etc.); mutable `BonusTag` ref |
| `TokenVariety` | `TokenClassToken original, string badge` | Medium | Wraps a `TokenClassToken` reference plus a badge string |
| `HumanToken` | `HumanTokenClass tokenClass, int rawFullHealth = ...` | Medium | Immutable value type with `Equals`/`GetHashCode` override; refs a `HumanTokenClass` singleton |
| `SpiritPresenceToken` | `Spirit spirit` | High | `Spirit` ref plus live `CountDictionary<Space>`/`CountDictionary<Board>` tracking caches |
| `FollowingPresenceToken` | `Spirit spirit, ITokenClass leaderClass, string spaceAbreviation` | High | Base tracking caches + `ITokenClass` ref + `ActionScope`-scoped move list |
| `Incarna` | `Spirit _spirit, string _abrev, Img _notEmpowered, Img _empowered` | High | `Spirit` ref plus live `CountDictionary<Space>` tracking cache |
| `GatewayToken` | `SpiritPresenceToken presence, Space from, Space to` | High | Holds direct live `Space` object references (`from`/`to`) |
| `InvaderActionToken` | `string label` | Low | Single string; used as 3 static singletons (`DoExplore`/`DoBuild`/`DoRavage`) |
| `DynamicToken` | `Func<Space,int> getCount, TokenVariety dynamicToken` | High | Holds a delegate closure (`getCount`) plus a `TokenVariety` ref |
| `SkipAnyInvaderAction` | `string label, Spirit spirit, Func<Space,Task>? alternativeAction = null` | High | Optional delegate plus a `Spirit` reference |
| `SkipBuild` | `string label, UsageDuration duration, params ITokenClass[] stoppedTokenClasses` | Medium | Enum, string, array of stable `ITokenClass` singleton refs |
| `SkipBuild_Custom` | `string label, bool stopAll, Func<Space,bool> func` | High | Stores a `Func<Space,Task<bool>>` delegate field |
| `SkipExploreFrom` | none | Low | Stateless marker class |
| `SkipExploreTo` | `bool skipAll = false` | Low | Single bool flag only |
| `SkipExploreTo_Custom` | `bool stopAll, Func<Space,bool> func` | High | Stores a delegate field plus a bool |
| `SkipRavage` | `string label, UsageDuration duration = UsageDuration.SkipOneThisTurn` | Low | String label and enum duration only |
| `ReduceAttack` | `int reduce, params HumanTokenClass[] classesToReduce` | Medium | Int plus array of stable `HumanTokenClass` refs |
| `ReduceHealthByStrife` | none | Low | Stateless (behavior only, via `ActionScope.Current`) |
| `SaveDahan` | `Space space, int maxPerAction, int maxActionCount` (private) | High | `CountDictionary<ActionScope>` keyed by live/transient `ActionScope`, plus `Space` ref |
| `TokenRemovedHandlerAsync` | `Func<ITokenRemovedArgs,Task> handler` | High | Sole state is an async delegate |
| `TokenRemovedHandlerAsync_Persistent` | `Func<ITokenRemovedArgs,Task> _handler` | High | Sole state is an async delegate |
| `TokenRemovedHandler` | `Action<ITokenRemovedArgs> _handler` | High | Sole state is an `Action<>` delegate |
| `RavageBehavior` | none (cloned via `.Clone()`) | High | Two mutable delegate fields (`RavageSequence`, `GetDamageFromParticipatingAttackers`) |
| `BlockBlightToken` | none | Low | Stateless marker class |
| `LandDamage` | none | Low | Stateless singleton, no instance fields |
| `BeforeInvaderPhase` | `Func<GameState,Task> _func, bool _remove` | High | Delegate closure over `GameState` plus a bool flag |

### SpiritIsland.Basegame

| Type | Constructor Parameters | Complexity | Notes |
|---|---|---|---|
| `SwedenHeavyMining` | none | Low | Only a bool property; no refs/delegates |
| `ToDreamAThousandDeaths` | `Spirit spirit` | Medium | Holds a captured `Spirit` reference |
| `DreamingDamageLog` | none | Medium | List of `HumanToken` value transitions |
| `Drowning` | `Ocean ocean` | Medium | `Ocean` (Spirit) ref plus int accumulator fields |
| `SimultaneousDefend` | none | Low | Stateless marker token |
| `StopDahanDamageAndDestruction` | `string sourceName` | Low | Single string field only |
| `ConvertFearToDefense` | `string powerName, Spirit spirit` | Medium | String plus a `Spirit` reference |
| `FearPushesInvaders` | none | Low | No stored fields |
| `ChokeTheLandWithGreen` | `ASpreadOfRampantGreen _self` | High | Derives `SpiritPresenceToken`; inherits its live tracking caches |

### SpiritIsland.BranchAndClaw

| Type | Constructor Parameters | Complexity | Notes |
|---|---|---|---|
| `SlaveRebellion` | none | Low | No fields; pure behavior class |
| `SlowBlightMod` | `GameState gs` | Medium | Int threshold + `BlightTokenBinding` ref to a `FakeSpace` |
| `FranceFearPushesExplorers` | none | Low | No stored fields |
| `SkipLowestNumberedExplore` | none | High | Caches `Dictionary<Board,Space>` of live board/space refs |
| `ReplaceRavageWithBuild` | none | Low | No stored fields |
| `MistPusher` | `Spirit spirit` | Medium | Captures a `Spirit` reference only |
| `LandsHostileToHumanity` | `Spirit spirit` | High | Derives `SpiritPresenceToken`; inherits its live tracking caches |

### SpiritIsland.FeatherAndFlame

| Type | Constructor Parameters | Complexity | Notes |
|---|---|---|---|
| `ScotlandCoastalBlightCheckToken` | none | Low | No stored fields; ctor only logs |
| `MudToken` | `Spirit _self, int _count` | Medium | `Spirit` reference plus int multiplier |

### SpiritIsland.Horizons

| Type | Constructor Parameters | Complexity | Notes |
|---|---|---|---|
| `OneTimeDamageBoost` | `Spirit spirit, int damageBoost` | Medium | `Spirit` ref, int, and a GUID-based string key |
| `SkipRavageOrBuild` | `string label, Spirit spirit` | Medium | `Spirit` reference plus a label string |

### SpiritIsland.JaggedEarth

| Type | Constructor Parameters | Complexity | Notes |
|---|---|---|---|
| `HabsburgMakeTownsDurable` | none | Low | Stateless mod entity |
| `NeighborTownsCauseBonusLandDamage` | none | Low | Stateless; closure used locally only, not stored |
| `TrackBadRavageBlight` | none | Low | Stateless mod entity |
| `Russia_Level1_HuntersBringHomeShelAndHide` | none | Low | Only a static `FakeSpace`, no instance fields |
| `Russia_Level2_SenseOfPendingDisasterMod` | none | Low | Stateless mod entity |
| `StopRavageIfTooFewExplorers` | none | High | Holds a `Func<RavageBehavior,RavageData,Task>?` delegate field |
| `RecordBlightAdded` | none | Medium | Holds a `HashSet<Board>` of stable domain objects |
| `DestroyerOfBeastsAndPresence` | none | Low | Stateless mod entity |
| `LandDamageBoost` | none | Low | Stateless mod entity |
| `EachDahanAtRange1Defend1` | `Spirit self` | Medium | `Spirit` reference; owns a simple `TokenVariety` field |
| `InvadersSkip1Board` | none | Medium | Mutable `Board?` field references a stable domain object |
| `Quicksand` | none | Low | Stateless mod entity |
| `FreezePresence` | `string _name, SpiritPresence _presence, IToken beast, IToken badland` | Medium | Captures `SpiritPresence`/`IToken` domain refs and a string |
| `RavageConfigToken` | `Action<Space> _setup, Action<Space> _teardown` | High | Constructor captures two `Action` delegates |
| `DamageNewInvaders` | none | Low | Stateless mod entity |
| `StopCascade` | none | Low | Stateless mod entity |
| `ARealFlairForDiscord` | `Spirit spirit` | Medium | Captures a single `Spirit` reference |
| `CleaningUpMessesIsADrag` | `Spirit spirit` | Medium | Captures a single `Spirit` reference |
| `IntensifyThroughUnderstanding` | `ShiftingMemoryOfAges smoa` | Medium | Holds `Spirit`, `DefaultMoverFactory`, `IActionFactory[]` domain refs |
| `InvadersDontParticipateInRavage` | `CountDictionary<HumanToken> sitOuts` | Medium | `CountDictionary` keyed by `HumanToken` values |
| `PushPresenceInsteadOfDestroy` | none | Low | Directly implements `ISpaceEntity`; stateless |
| `PayEnergyToTakeFromBox` | `Spirit _spirit, int _cost` | Medium | `Spirit` reference plus a primitive int |
| `StopPresenceDestructionFromBlightOrEvents` | `Spirit _spirit` | Medium | Captures a single `Spirit` reference |
| `BreakThemselvesMod` | `Spirit _spirit, bool _shouldAddHalfInvaderDamage` | Medium | `Spirit` reference plus a primitive bool |
| `TerrorOfASlowlyUnfoldingPlague` | `Spirit _spirit` | Medium | Captures a single `Spirit` reference |
| `ASingleAluringLair` | `Spirit spirit` | High | Derives `Incarna`; inherits its live tracking cache |
| `EnthrallTheForeignExplorers` | `Spirit self` | High | Derives `SpiritPresenceToken`; inherits its live tracking caches |
| `AJoiningOfSwarmsAndFlocks` | `Spirit spirit` | High | Base tracking caches + owns a nested `ManyMindsBeast` instance |
| `ManyMindsBeast` | `AJoiningOfSwarmsAndFlocks presenceToken` | High | Circular back-reference to its owning presence token |
| `ObserveWorldMod` | `TargetSpaceCtx ctx` | High | Holds `HashSet<ActionScope>` (transient) plus a `Spirit` ref |

### SpiritIsland.NatureIncarnate

| Type | Constructor Parameters | Complexity | Notes |
|---|---|---|---|
| `AdjustDamageFromAttackers` | `Func<RavageExchange,int> damageAdjustment` | High | Constructor captures a delegate field |
| `LandStrippedBare` | none | Low | No fields; base ctor takes literal strings only |
| `AvariceRewardedMod` | none | Low | State kept in `ActionScope`, not the instance |
| `DiseaseStopsRavageInMiningLands` | none | Low | No fields at all |
| `ExtraDamage` | none | Low | Stateless mod |
| `DahanSitOutRavage` | `Spirit picker, int countToSitOut` | Medium | `Spirit` reference plus a primitive int |
| `TerrorStalksTheLand` | `Spirit spirit` | Medium | Single `Spirit` reference field |
| `InvadersSitOut` | `Spirit invaderPicker, Quota quota` | Medium | `Spirit` ref plus a `Quota` data object |
| `DamageNewInvadersOnce` | `Spirit spirit` | Medium | `Spirit` reference plus a mutable bool |
| `WarnToken` | `Spirit spirit, int dahanToGoEarly, bool allSpaces = false` | Medium | `Spirit` ref plus primitives, mutable int field |
| `MyPowersDontDamageDahanThisRound` | `Spirit spirit, string source` | Medium | `Spirit` ref plus a string label |
| `DestroyPresenceInsteadOfAddingBlight` | `Spirit spirit, string source` | Medium | `Spirit` ref plus a string label |
| `SenselessRoaming` | `Spirit spirit` | Medium | Single `Spirit` reference field |
| `SpreadTumultAndDelusion` | `Spirit spirit` | Medium | Single `Spirit` reference field |
| `BlightedInvadersSitOutRavage` | none | Low | Pure logic mod, no fields |
| `RoilingWaters.Mod` | `Spirit spirit` | Medium | Single `Spirit` reference field |
| `SereneWaters.Mod` | `Spirit spirit` | Medium | Single `Spirit` reference field |
| `TriggerAfterNoRavageOrBuild` | `Spirit spirit, Func<TargetSpaceCtx,Task> triggeredAction` | High | Captures a delegate/closure field |
| `MarkedBeast` | `Spirit controlSpirit` | High | Self-subscribes to `Spirit.EnergyCollected` (event/delegate registration) |
| `ToweringRootsIncarna` | `Spirit spirit` | High | Derives `Incarna`; inherits its live tracking cache |

### SpiritIsland.Tests

| Type | Constructor Parameters | Complexity | Notes |
|---|---|---|---|
| `StopBlightEffects` | none | Low | No stored fields |
| `ActionScopeTracker` | none | High | Holds `HashSet<ActionScope>` of live mutable scope objects |
