# Plan: web play, solo first, then multiplayer

The end goal is multiplayer Spirit Island in a browser. Multiplayer needs a UI to play-test with, so the first step is a solo web client, built so it can later become the multiplayer one. Nothing here is built yet.

## Goal: multiplayer

Each player builds an entire action, then submits it to a central coordinator. The coordinator applies all of that action's decisions as one unit of work. If another spirit changed the board so the action is no longer possible, the whole action is rejected and the player builds a new one.

Multiplayer will need its own engine, file formats and setup. None of that is designed yet.

## The server steps the player through an action

The web server, not the browser, walks a player through building an action. The browser only shows the current prompt, sends the player's pick, and keeps the list of picks made so far in the current action.

There are two server roles:

1. **Stepping (the decision oracle).** The browser sends `{ checkpoint version, picks so far }`. The server loads the checkpoint, replays the picks, and returns the next prompt and its options. Nothing is held between requests: when the reply is sent, the game instance is dropped.
2. **Coordinating.** When an action is complete, the server applies it as one unit of work. In multiplayer the checkpoint may have moved on since the player started building the action. That alone doesn't reject it: the server replays the picks against the newest checkpoint. If every pick is still valid there, the action is accepted. If any pick no longer applies, the whole action is rejected and the player rebuilds it.

In solo play the two roles collapse into one: the replay that completes an action also commits it. Nothing else can move the checkpoint, so a stale `checkpointVersion` is just an error (409), not something to retry. The solo plan below builds exactly that.

**Constraint:** the server never holds an in-progress action in memory. A suspended async action can't be serialized, so nothing is kept alive waiting on a player. The in-progress picks travel with the browser; the server only replays them.

**Guidance:**
- Keep action resolution deterministic and replayable, and keep actions self-contained.
- Keep per-game state in the execution context rather than in process-wide statics. `ActionScope` is `AsyncLocal`, so games run in parallel only in separate execution contexts (`Task.Run` per game, `ActionScope.Initialize` inside each; see the parallel-games test).
- Identify picks by option text, not index. Option order can differ after a restore (presence sources are listed in hash order).
- Save/restore rules for engine code are in the doc comment on `GameState.RestoreFromJson`.

## Solo web client: decisions already made

- **New project, not the MCP server.** The MCP server stays an agent-play tool. The web server is a separate ASP.NET project.
- **Oracle first, WASM later.** Try running the engine in the browser only after the oracle works.
- **Layout runs on the server.** `BoardLayout`, `SpaceLayout` and `ManageInternalPoints` (in `SpiritIsland/Island/Layout/`) stay in C#. The client gets plain coordinates and draws them. No geometry is ported to TypeScript.
- **Send a player view, not GameState.** The server sends a projection modelled on the MCP `PlayerObservation`, which already hides what a player can't see (invader deck order, face-down fear cards, unflipped blight card).
- **Projection code lives in the web server.** It goes in its own folder and namespace (`View/`), with no ASP.NET types, so it can move into a library if WASM ever needs it.
- **Copy, don't share, the replay code.** `OracleSession` copies the restore/replay pattern from `AgentGame`. The web project does not reference `SpiritIsland.Mcp`.
- **TypeScript types are generated from the C# records** (see "Generated types").
- **Save format:** the engine's `SoloGameFile` and `GameConfiguration`, which Maui also uses.
- **No auto-picking.** Every decision is shown to the player, including ones with a single option.
- **Client framework:** Preact.

## How the oracle works

The server stores only **action-boundary checkpoints**. Picks inside an action live in the browser until the action finishes. This matches the multiplayer constraint that the server never holds an in-progress action.

Each request carries `{ gameId, checkpointVersion, picks[] }`:

1. The server loads the game's checkpoint and checks that `checkpointVersion` matches. If not, it returns 409 with the current state, e.g. when another tab moved the game on.
2. It rebuilds the `SoloGame`, loads the checkpoint and starts it. The engine continues from the saved step.
3. It replays `picks` in order, matching each by option text. A pick that isn't among the current options gets a 400 response. That is also the anti-cheat check.
4. When the picks run out, it reads the pending decision and builds the view.
5. If the replay crossed an action boundary (`SoloGame.CheckpointVersion` moved), it writes the new checkpoint and tells the client which picks the new checkpoint has already absorbed. The client keeps only the rest.
6. It drops the game instance. The engine task left waiting on a decision holds no thread and is collected.

This is the pattern `AgentGame.Save`/`Restore` already uses in the MCP server, minus the agent-specific parts (alert buffering, observation diffs, batch steps).

What you get for free:
- **Undo within an action.** The client drops its last pick and asks again.
- **Refresh-safe.** The client keeps its picks in `localStorage`. Losing them rolls back to the start of the action, never further.
- **Many games at once.** Nothing is held in memory between requests.

### Response shape

```
{
  checkpointVersion,        // current, after any boundary crossed during replay
  picks,                    // the picks still pending after that checkpoint (client replaces its list with this)
  staticVersion,            // client refetches GET /api/games/{id}/static when this changes
  view,                     // GameView
  decision,                 // DecisionView, or null when the game is over
  result                    // win/loss, when over
}
```

## View projection: what is still open

`View/` builds the player view, the static view and each option's target (see `GameViewBuilder`, `StaticViewBuilder`, `OptionTargets`). The rule that outlives that work: use `PlayerObservation` as the checklist for what a player may see. Anything it hides stays hidden, and the tests check the names of face-down fear cards and the unflipped blight card never appear.

Not in the view yet, for Phases 3 and 4:
- **Prompt details and recent log entries** on `DecisionView`, for the log panel (the MCP server's `PromptDetails` and `Alert`s are the model).
- **Full card text** (range, target, instructions) for the cards in hand and in play. `CardView` has name, cost, speed and elements only.
- **Picture keys.** `TokenView.Image` is the engine's `Img` name (or "Presence:" and a spirit name), and invader cards are their text. The client maps them to files copied from `SpiritIsland.Maui/Resources/Images`.
- **Arrows** for decisions that move something (`IHaveArrows`). A move option already says where from and where to.
- **Growth options** arrive as kind "action". The client matches them to the static view's growth groups by text.

## Engine problems that stop some spirits

Playing every spirit found failures in the engine, not in the oracle. `EverySpirit_Tests` lists them (`KnownFailures`), fails if a new one appears, and fails when a listed one is fixed so it gets removed. The oracle reports them as a 500 with the engine's message.
- **Not savable yet** (the oracle saves a checkpoint at every action boundary, so it meets these sooner than Maui or the MCP server): Finder of Paths Unseen (`OpenTheWays`), Fractured Days (`RepeatableSelfCmd`), Stone (`StubbornSolidityBehavior`), Sun Bright Whirlwind (`AdjustHealth`), Towering Roots ("Sequence contains no elements"), Volcano Looming High (key 'M' missing).
- **`Error SI01`** (an `ActionScope` disposed out of order during a Ravage): six spirits including River. It also happens in plain in-memory play on the same seeds, so it is an engine bug.
- **`NullReferenceException` in `Cmd.DestroyPresence`**, seen once in in-memory play (River, seed 35) and not yet in the list.

## Generated types

The C# records and endpoints are the single source of truth for the API:

1. The server describes its endpoints with ASP.NET's built-in OpenAPI support (`Microsoft.AspNetCore.OpenApi`). `Microsoft.Extensions.ApiDescription.Server` writes `openapi.json` at build time.
2. In the client, `openapi-typescript` turns `openapi.json` into `src/api/types.ts`, and `openapi-fetch` gives typed calls from it.
3. An npm script (`npm run gen`) regenerates the types. The client build runs it first, so a changed record breaks the TypeScript build instead of failing at runtime.

The generated file is committed, so the client builds without running the server.

Watch for: option targets and other union-like records. Model them as one record with a `kind` enum plus optional fields, which OpenAPI describes cleanly, rather than as a C# class hierarchy.

## Client

A Preact + TypeScript single-page app built with Vite into `SpiritIsland.Web/wwwroot`.

Views:
- **New-game screen**: pick the spirit, board, adversary and seed.
- **Island**: one `<svg viewBox>` set from `WorldBounds`, with a `<polygon>` per space and an `<image>` per token at its `xy`. SVG scaling replaces `PointMapper`. Spaces and tokens that are options are highlighted, and clicking one sends the pick.
- **Decision panel**: the prompt, buttons for options that aren't on the board, and Undo (drop the last pick).
- **Spirit panel**: energy, elements, presence tracks (revealed and clickable when they are options), growth and innates.
- **Cards**: hand, in play and discard. Cards that are options are highlighted.
- **Status bar**: round, phase, fear, blight, and the invader explore, build and ravage slots.
- **Log**.

Client state: `gameId`, `checkpointVersion`, `picks[]`, the cached `StaticView` and the last response. Everything else comes from the server.

Assets: copy the token, icon and track images from `SpiritIsland.Maui/Resources/Images` into `wwwroot/img`. Cards are drawn from data for now, with no card art.

## Phases

Each phase ends with something that runs.

### Phase 3: minimal client
Vite + Preact + TypeScript, with types generated from the server's OpenAPI document, built into `wwwroot/` and served by `SpiritIsland.Web`. Includes the new-game screen, the island SVG with highlighted options, the decision panel with Undo, and `localStorage` for picks.
- **Exit:** a whole game can be played in the browser, ugly but complete.

### Phase 4: full player panels
The spirit panel with tracks, growth and innates. Cards, status bar, log and token images.

### Phase 5: polish (pick as needed)
- **Tokens jumping.** Positions are computed fresh each request, so tokens may move when others come or go. If that's distracting, send ordered slot lists and port the slot-assignment part of `ManageInternalPoints` (~100 lines) to TypeScript.
- Island rotation to fit the screen (Maui computes this from the corners).
- Response diffs (`ObservationHistory` pattern) if the payload size matters.
- Mobile layout.

## Multiplayer-ready choices

The solo web UI is the test bed for multiplayer, so it must not bake in solo assumptions. These choices cost little now and avoid rewrites later:

1. **Spirits are a list.** `GameView.spirits` plus `viewerSpiritId` from the start. Panels take a spirit, not "the" spirit. Solo just has one.
2. **The decision panel is generic.** It renders whatever prompt and options arrive and doesn't walk `SoloGame`'s fixed menu sequence (Growth, PlayCards, Fast, Slow). Round and phase are shown in the status bar only. In multiplayer, players act at the same time within a phase.
3. **The island handles several boards.** The layout code already supports board orientation and `Island` takes several boards. Test a solo game on two boards early, if the engine allows it, to prove the SVG view copes.
4. **The client re-renders from any incoming view**, not only from the answer to its own request. Multiplayer needs server push (SSE or WebSockets) when another player acts; with this rule, push is a new event source rather than a rewrite.
5. **Storage and setup are swappable.** `GameFiles` and the new-game endpoint sit behind small interfaces in `Oracle/`, because both will change for multiplayer. Don't design the multiplayer formats now.
6. **Keep the oracle's request shape.** `{ checkpointVersion, picks[] }` is also what multiplayer needs: replaying the picks against the current checkpoint is how it decides whether an action still applies, and an invalid pick gets the same 400 rejection. Only the stale-checkpoint response differs (solo: 409; multiplayer: replay against the new checkpoint). Don't add solo-only shortcuts to the request.

These are about naming and shape, not features. Lobbies, accounts, push and multiplayer engine work stay out of scope.

## Later option: stepping in the browser (WASM)

Only if server-side stepping turns out too slow or too costly. A first measurement (`OracleCost_Tests`) found a median request of about 7 ms in rounds 1-4, so this isn't expected; re-measure with a stronger player's late-game state before ruling it out. The engine would run in the browser as WebAssembly (Blazor custom elements, or a .NET WASM module with JSExport/JSImport exposing `advance`/`choose`/`save`/`load`), stepping locally and sending only complete actions to the coordinator. The `View/` projection would move into a library so the browser can run it too.

Expected problems:
- Blocking calls in `UserGateway` (`WaitForNext`/`Next`/`Current`/`_signal.WaitOne`) can't run on the single browser thread; the stepwise API avoids them.
- Reflection and `[ModuleInitializer]` registries versus the trimmer (trimming probably has to be off).
- Download size.

First step, if it ever comes to this: a small spike, a WASM console project that references the engine and calls advance/choose.

## Out of scope for the solo client

- Multiplayer and the coordinator.
- WASM.
- Accounts, authentication and hosting on the internet. The server is local only for now.
- Changes to the MCP server.

