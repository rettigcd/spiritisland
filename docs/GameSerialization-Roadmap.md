# Roadmap: Full-Game Serialization & Restore

Goal (from `todo.txt`): save the whole game to JSON and restore it, and eventually support undo-last-action
instead of undo-whole-round. This doc tracks what's still missing to get there. Completed work isn't
described here - when an item is closed, it's removed from this document rather than marked done; check
`git log`/the code itself for how a given area was implemented.

## What already exists

Two separate mechanisms are relevant, and it's easy to conflate them:

1. **`IHaveMemento`** - an in-memory object-graph snapshot/restore system, already built and in wide use.
   Nearly every stateful class in the engine (`GameState`, `Spirit`, `Fear`, `BlightCard`, `Island`,
   `PowerCardDeck`, `InvaderDeck`, `Tokens_ForIsland`, the hook action lists) implements it. This is what
   today's "undo whole round" almost certainly runs on. It proves *what state exists* - a `MyMemento`
   class's fields are a reliable inventory of what a type actually needs captured - but it doesn't
   produce JSON; it just clones live references in memory.
2. **`ISerializableSpaceEntity` / `SpaceEntitySerialization`** - the JSON serialization system built out
   over this project (see `docs/ISpaceEntity-Types.md`). Covers exactly one slice: the 103 `ISpaceEntity`
   types that get placed as token-dictionary keys on a `Space` or in island mods. This is done - 103/103.

Everything below is state that `IHaveMemento` already tracks in-memory, but that doesn't yet have a
complete JSON path. In each case, `IHaveMemento`'s `MyMemento` class (where one exists) is the best guide
to exactly what fields matter.

## Remaining gaps

None currently known. `Spirit.Mods` (`docs/ISpiritMod-Types.md` - all 22 catalogued types, Low/Medium/High)
and the mod-injected `IActionFactory`s that depended on it (`MarkedBeastMover`, `TheBehemothRises`,
`PourDownPower`'s `RepeatLandCardForCost`) were the last items - both closed via `IOwnedActionFactories`
(`SpiritIsland/Hooks/Spirit/IOwnedActionFactories.cs`), which lets `Spirit.SerializeActionFactory`/
`DeserializeActionFactory` resolve a Mods-owned action factory back to the exact live instance already
re-added by spirit/aspect construction replay, rather than a fresh, reference-distinct one.

## Design constraints (not gaps)

These aren't catalog items to fix - they're scope boundaries that should shape any restore driver's
design from the start.

- **Delegates & events**: `GameState.NewLogEntry` (event), `WinLossChecks` (`List<Action<GameState>>`),
  `Spirit.EnergyCollected` (`AsyncEvent<Spirit>`), `Spirit.SelectionMade` (event) - none of these are
  meaningfully "game state" to persist; they're wiring that gets re-established by replaying setup code
  (`AddStandardWinLossChecks()`, aspect `InitAspect` calls, etc.) rather than serialized directly. One
  confirmed exception (fixed, not a live gap): `France.Loss_SprawlingPlantations` used to close over a
  `_maxTownCount` computed from live board state inside `Init` - proof "replay is always safe" isn't
  universal without checking each case. `GameState.RestoreFromJson`'s ordering (replay `Init` on a normal
  board, *then* restore `Tokens` from JSON) happens to fix this one for free, since `Init` now always runs
  on a pristine board.
- **Mid-action state**: actions are async and interactive - `Spirit.Select(...)` suspends execution
  awaiting a UI decision, and there's no C#-native way to serialize a suspended `Task`/continuation.
  Practically, full JSON save/restore can only happen **at action boundaries** (between completed
  actions), never mid-action - any save/restore driver must guarantee it only runs when no `ActionScope`
  is actively awaiting player input.

