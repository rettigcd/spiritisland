# Multiplayer direction and the stepwise engine

Status of the stepwise engine and where it is heading. Nothing under "Deferred" is built; don't build it until asked.

## Long-term goal (not started)

Multi-player Spirit Island where each player's spirit builds an entire action remotely, then submits it to a central coordinator. The coordinator applies all of that action's decisions as one unit of work. If another spirit changed the board so the action is no longer possible, the whole action is rejected and the player re-submits a new one.

This implies two engine roles:
1. A client-side engine that walks a player through building one action and submits it all at once.
2. A coordinator engine that applies an action's decisions atomically.

**Constraint:** a server can't hold in-progress actions in memory. A suspended async action can't be serialized, so the server never keeps one alive waiting on a player. In-progress actions live on the client; the server only handles complete actions.

**Guidance:** keep action resolution deterministic and replayable, keep actions self-contained, and keep per-game state in the execution context (see the parallel-games test) rather than in process-wide statics.

## Done: stepwise SoloGame

`SoloGame` is no longer one long-running async process. Only actions run async; between actions the engine pauses at a menu, so a game can be saved to disk and shut down.

- An action is one growth, one power resolution, all card plays grouped, or a presence-track token action. Sub-decisions (target land, token) are steps inside an action.
- `AdvanceAsync()` returns a `PendingMenu` (`MenuKind` = Init, Growth, PlayCards, Fast, Slow, GameOver, Suspended). `ChooseAsync(int index)` resolves one pick (`SoloGame.Done = -1` ends Fast/Slow) and advances. Menus are recomputed from saved state, never stored.
- `Save()`/`Load(JsonObject)` carry the `GameState` JSON plus `RoundStep` and `InvaderCursor`; valid only at action boundaries. The target game must be built the same way as the saved one.
- Checkpoints: `LatestCheckpoint` is refreshed at every menu, at the start of `ChooseAsync`, and between Invader steps. Mid-action process death rolls back to the last menu.
- `PromptTimeout` (on `UserGateway`, forwarded by `Spirit` and `SoloGame`): a decision that waits too long throws `PromptTimeoutException`; the engine loads the checkpoint and returns `MenuKind.Suspended`. Resume is `Load` plus `AdvanceAsync`.
- `InvaderPhase.StepAsync(gs, cursor)` runs the Invader phase one step at a time (PreInvader, Fear card, slot, Advance, PostInvader). `InvaderPhase.ActAsync` loops over it.
- `SoloGameFile` writes/reads one JSON file (version, `GameConfiguration`, game save) atomically.
- `Start()`/`StartAsync()` remain as wrappers that loop `AdvanceAsync`/`ChooseAsync`; rewind still works.
- Maui suspends the current game to disk when backgrounded and resumes it from the main page.
- Games can run in parallel only in separate execution contexts (`Task.Run` per game, `ActionScope.Initialize` inside each), because `ActionScope` is `AsyncLocal`.

Known limits: a slot or pre/post action list is one step (a timeout inside it re-runs the step); log entries and rewind history aren't saved; a resumed game redoes the unfinished decisions of an interrupted action; Maui keeps one saved-game slot; the Maui lifecycle events are unverified on devices.

## Done: MCP server saves games to disk

Games are saved to disk (`GameStore`, one JSON file per game; `SPIRITISLAND_GAMES_DIR`), restored per tool call and dropped after, so many games can run and no engine task is kept alive. It does not use `AdvanceAsync`/`ChooseAsync` for the agent's sub-decisions: `AgentGame.Save` stores the last action-boundary checkpoint (`SoloGame.CheckpointVersion` says when it moved) plus the picks made since, and `Restore` replays them. That is the "decision oracle" replay idea below, in small. Option order can differ after a restore (presence sources are listed in hash order); picks go by option text.

## Save/restore design constraints

JSON save/restore of the whole `GameState` is complete. These rules shaped it and still apply to new code:

- **Only at action boundaries.** `Spirit.Select(...)` suspends an action while it waits for a decision, and a suspended `Task` can't be serialized. Save only when no action is awaiting input.
- **Wiring is replayed, not serialized.** Events and delegates (`GameState.NewLogEntry`, `WinLossChecks`, `Spirit.EnergyCollected`, `Spirit.SelectionMade`) aren't game state. They are re-established by building the game the normal way (spirit and aspect construction, `AddStandardWinLossChecks()`, adversary `Init`), and then the JSON is restored on top. That is why the target game must be built the same way as the saved one.
- **Check that replay is really safe.** `France.Loss_SprawlingPlantations` once captured a value computed from live board state during `Init`. `GameState.RestoreFromJson` runs `Init` on a pristine board and only then restores `Tokens`, which covers that case, but a new adversary or setup step that captures live state needs the same check.
- **Resolve identity, don't rebuild.** When the same object is referenced from two places (e.g. a blight card that is also a pre-invader-phase hook, or a mod's own action factory in `UsedActions`), the restore must point both at the one live instance (`ISerializationContext`, `IOwnedActionFactories`), not create a second copy.

## Deferred client-side options

Neither started.

**Decided (2026-11-15):** the stateless decision oracle (option 2) is the leading candidate for web-based play. Try WASM (option 1) only after the oracle has worked. The MCP server stays an agent-play tool; it will not be turned into a web server or REST backend, though the oracle can borrow its replay idea (`AgentGame.Save`/`Restore`). The plan for the oracle and a solo web client is [WebClient-Plan.md](WebClient-Plan.md); it replays picks the way `AgentGame` does instead of using an abandon-by-exception gateway.

1. **Engine as WebAssembly in the browser** (Blazor custom elements, or a .NET WASM module with JSExport/JSImport exposing `advance`/`choose`/`save`/`load`). Expected problems: blocking calls in `UserGateway` (`WaitForNext`/`Next`/`Current`/`_signal.WaitOne`) can't run on the single browser thread (the stepwise API avoids them); reflection and `[ModuleInitializer]` registries vs the trimmer (probably need trimming off); download size; the Maui UI doesn't carry over, so a web UI is needed. First step: a small spike, a WASM console project that references the engine and calls advance/choose.
2. **Stateless "decision oracle" on the server (lazy decision tree)**, instead of sending a full tree (combinatorially huge, one engine replay per node). The client sends `{checkpoint version, menu pick, choices so far}`; the server `Load`s the checkpoint, replays with a scripted gateway that answers from the list, captures the next prompt and options when the list runs out, abandons the run, and replies. Nothing is held between calls, and the final submit is the coordinator's replay. New piece needed: a scripted gateway that abandons by exception (like `PromptTimeout`). Needs deterministic replay. First step: a spike measuring per-prompt cost of typical powers. Trade-off vs WASM: a round trip per decision and server CPU, but no trimming or size risk.

## Other open items

- Test for Ravage/Build prompts timing out.
- Verify the Maui suspend events (`Window.Stopped`/`Destroying`) on each platform.
