# Plan: web-based solo play

Play a solo game in a browser. The server runs the engine as a stateless **decision oracle** (option 2 in [MultiplayerDirection.md](MultiplayerDirection.md)). The browser draws the board as SVG from geometry computed by the engine's existing layout code. Nothing here is built yet.

## Decisions already made

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
  staticVersion,            // client refetches /static when this changes
  view,                     // GameView
  decision,                 // DecisionView, or null when the game is over
  result                    // win/loss, when over
}
```

## Server: `SpiritIsland.Web`

An ASP.NET Core minimal API (`net10.0`). It references the engine and the expansion projects, like `SpiritIsland.Mcp` does, and serves the built client from `wwwroot/`.

| Endpoint | Purpose |
|---|---|
| `GET /api/options` | Spirits, boards, adversaries and levels to choose from |
| `POST /api/games` | Create a game; returns its id and the first response |
| `GET /api/games` | List saved games |
| `POST /api/games/{id}/decide` | The oracle call described above. `picks: []` just reads the current decision. |
| `GET /api/games/{id}/static` | The `StaticView` (geometry, setup, spirit tracks, innates) |

Folders:
- `Oracle/`: `GameFiles` (one JSON file per game, written with `SoloGameFile`, plus the checkpoint version) and `OracleSession` (load, replay, read decision, detect boundary; copied from `AgentGame` without its auto-select, alert buffering, observation diffs and batch steps).
- `View/`: the projection. It uses plain records and has no web types.
- `Endpoints/`: the HTTP layer. Request validation, JSON and error mapping happen only here.

Concurrency: one lock per game id. Each request runs its engine work inside `Task.Run` with `ActionScope.Initialize`, so parallel games don't share the ambient scope (see the parallel-games test).

## View projection (`View/`)

Typed C# records, serialized with `System.Text.Json` (camelCase). The client's TypeScript types are generated from them (see "Generated types").

**`StaticView`**: changes only when the island layout changes, versioned by a hash as `PlayerObservation.BuildStatic` does.
- Setup: spirit, boards, adversary and level.
- `WorldBounds`.
- For each space: id, label, terrain, `Corners`, `NameLocation` and `TokenSize` (from `ManageInternalPoints`).
- Spirit: presence tracks (each slot's energy, card plays, elements or action), growth options and innate tiers.

**`GameView`**: per response.
- Round, phase, fear (pool, terror level, cards remaining), blight (count, flipped card), invader slots (explore, build and ravage cards, discards, deck count).
- `spirits`: a list (solo has one entry), plus `viewerSpiritId`. Each has energy, elements, revealed track slots, hand, in play, discard, destroyed presence and growth groups used.
- For each space: its tokens, each with a stable id, an image key (token class plus count, damage and strife) and an `xy`. Positions come from a fresh `ManageInternalPoints(...).Init(space)`.

**`DecisionView`**:
- Prompt text and prompt details. Decisions with a single option are shown like any other; nothing is auto-picked.
- Options, each with `id` (option text, unique within a decision), `label` and a target: `{ kind: space | spaceToken | card | track | growth | other, ref }`. The `ref` points at a space id, token id or card name, so the client can highlight it on the board or in a panel.
- Recent log entries, for a log panel.

Use `PlayerObservation` as the checklist for what may be shown. Anything it hides stays hidden.

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

### Phase 0: measure the oracle's cost
The first step the multiplayer doc asks for. Write a test or small benchmark that loads a checkpoint and replays N picks for a few typical actions (growth, a multi-target power, an innate). Record milliseconds per request.
- **Exit:** a number. If a typical request costs well over ~100 ms, revisit before building UI (e.g. cache the most recent instance per game).

### Phase 1: oracle server with no UI
`SpiritIsland.Web` with the game files, `OracleSession` and endpoints, returning `decision` with plain options and a minimal `view`.
- **Tests:** create a game, then play the first rounds through `/decide` by taking the first option each time. Check that replaying the same picks gives the same decision, that a stale `checkpointVersion` gets 409, and that an invalid pick gets 400.
- **Exit:** a whole game can be played with `curl` or a test.

### Phase 2: view projection
`StaticView`, `GameView` and `DecisionView` with option targets. Port the hidden-information rules from `PlayerObservation`.
- **Tests:** the projection hides the invader deck order and face-down fear cards. Every token on every space has a position inside its polygon. Option targets resolve to existing space, token and card ids.

### Phase 3: minimal client
Vite + Preact + TypeScript, with types generated from the server's OpenAPI document. Includes the new-game screen, the island SVG with highlighted options, the decision panel with Undo, and `localStorage` for picks.
- **Exit:** a whole game can be played in the browser, ugly but complete.

### Phase 4: full player panels
The spirit panel with tracks, growth and innates. Cards, status bar, log and token images.

### Phase 5: polish (pick as needed)
- **Tokens jumping.** Positions are computed fresh each request, so tokens may move when others come or go. If that's distracting, send ordered slot lists and port the slot-assignment part of `ManageInternalPoints` (~100 lines) to TypeScript.
- Island rotation to fit the screen (Maui computes this from the corners).
- Response diffs (`ObservationHistory` pattern) if the payload size matters.
- Mobile layout.

## Multiplayer-ready choices

The end goal is multiplayer, which will need its own engine, file formats and setup. The solo web UI is the test bed for that work, so it must not bake in solo assumptions. These choices cost little now and avoid rewrites later:

1. **Spirits are a list.** `GameView.spirits` plus `viewerSpiritId` from the start. Panels take a spirit, not "the" spirit. Solo just has one.
2. **The decision panel is generic.** It renders whatever prompt and options arrive and doesn't walk `SoloGame`'s fixed menu sequence (Growth, PlayCards, Fast, Slow). Round and phase are shown in the status bar only. In multiplayer, players act at the same time within a phase.
3. **The island handles several boards.** The layout code already supports board orientation and `Island` takes several boards. Test a solo game on two boards early, if the engine allows it, to prove the SVG view copes.
4. **The client re-renders from any incoming view**, not only from the answer to its own request. Multiplayer needs server push (SSE or WebSockets) when another player acts; with this rule, push is a new event source rather than a rewrite.
5. **Storage and setup are swappable.** `GameFiles` and the new-game endpoint sit behind small interfaces in `Oracle/`, because both will change for multiplayer. Don't design the multiplayer formats now.
6. **Keep the oracle's request shape.** `{ checkpointVersion, picks[] }` with a 409 on a stale checkpoint is the same mechanism multiplayer needs to reject an action when another spirit changed the board. Don't add solo-only shortcuts to it.

These are about naming and shape, not features. Lobbies, accounts, push and multiplayer engine work stay out of scope.

## Out of scope

- Multiplayer and the coordinator.
- WASM.
- Accounts, authentication and hosting on the internet. The server is local only for now.
- Changes to the MCP server.

