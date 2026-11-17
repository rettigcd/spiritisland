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
- **Layout runs on the server.** `BoardLayout`, `SpaceLayout` and `ManageInternalPoints` (in `SpiritIsland/Island/Layout/`) stay in C#. The client gets plain coordinates and draws them. No geometry is ported to TypeScript. The engine's y points up and a screen's points down (the desktop apps flip it in `PointMapper`), so the server flips every position it sends (`View/ScreenCoordinates`) and the client draws them as they are.
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
- **The Explore slot is not sent.** The Explore card is flipped in the Invader Phase's Explore step and moves to Build as the cards advance at the end of that phase, so a player first sees it as the Build card (the log says what was explored). The engine draws the next card into the Explore slot as soon as the cards advance, which would show it all through the next Growth, Fast and Slow phases, so `InvadersView` has no Explore list and `UnrevealedCount` counts that waiting card with the deck. The status bar shows the deck count instead.
- **The game's seed is sent** (`SetupView.Seed`, in the static view). The top bar shows it, and "Play again (same seed)" starts a new game with the same spirit, board, adversary and seed, which is the same game from the beginning (the first one stays saved).
- **Prompt details and log: done.** `DecisionView.Details` spells out the "Play power card ($E / N)" prompt (same wording match as the MCP server's `PromptDetails`). `OracleResponse.Log` is the most recent 300 lines of what happened: rounds and phases, what the invaders did, blight and fear, presence destroyed and the end of the game (Debug detail such as each token moved, what the player chose, and failures are left out). The oracle keeps no state, so completed lines are stored in the game record with each checkpoint, tagged by the checkpoint version they were raised under; the lines after the checkpoint (the action in progress) are made again each time the picks are replayed.
- **Full card text: done.** `CardView` also has range, target and instructions, and an `OptionView` that is a power card carries its `Card`, so the text can be read before choosing which card to gain.
- **Picture keys: done.** `TokenView.Image` is the engine's `Img` name (or "Presence:" and a spirit name). The client's `images.ts` maps those to the pictures in `client/public/img` (copied from `SpiritIsland.Utilities/images` by `npm run copy-images`; the copies are committed). A key with no picture is drawn as a labelled circle. Presence is the one red picture turned to a hue worked out from the spirit's name; the desktop apps instead build it from the spirit's artwork, which is a possible upgrade (about 3 MB of pictures).
- **Arrows: done, for moves.** Hovering a move option draws an arrow on the island from the token (or its land) to the destination, and outlines both lands. Other decisions that carry arrows in the desktop apps (`IHaveArrows`) are not drawn.
- **Moving tokens on the board: done.** A move option already says where it starts and where it ends, so once the player has picked a start every place it can go is known; the client needs nothing more from the server. Click a token (or a presence track slot, or the pile of destroyed presence in the spirit panel) to pick it up: the lands it can reach are highlighted, and clicking one sends the matching move. Dragging a token from the board and dropping it on a highlighted land does the same, and dropping anywhere else puts it down; Escape or Cancel also puts it down. What is sent is still the one option, so the request shape is unchanged. `moves.ts` groups a decision's moves by their start. Not done: dragging from the spirit panel onto the board (it is click then click), and touch screens have not been tried.
- **Actions together with their targets: done (pick or point at, not yet drag).** The engine can offer a power or growth action together with what it targets as one choice (`Spirit.CombineActionsAndTargets`, `TargetedAction`; off by default in the engine, on for every web game, set by `OracleSession` each time it builds or replays a game). `OptionTargets` reports it as kind "act": the card (or the growth group and action) plus `ToSpace`, a land it targets, or `ToSpirit`, a spirit it targets. A growth action targets the spirit growing, so every action has the same form. The client groups acts by their action like moves (`moves.ts`): pointing at a card, an innate's "Use" button or the power's entry in the decision panel highlights the lands (or the spirit) it could target; clicking picks it, and then clicking a highlighted land or the spirit's "Use it on this spirit" button sends the one option. An action with a single target is chosen when it is clicked. Powers that must ask something before they know where they can target (element thresholds that change the range) and Shroud of Silent Mist (it gathers presence while it targets) stay as two steps, so they arrive as kind "card" or "innate" as before.

## Engine problems found by playing every spirit

Playing every spirit found failures in the engine, not in the oracle. They are all fixed. `EverySpirit_Tests` plays each spirit for 100 random picks and fails on any engine error. Its `KnownFailures` list is empty. Add a spirit there only while a known engine bug stops it, and the test fails when a listed one is fixed so it gets removed. The oracle reports an engine failure as a 500 with the engine's message.

### Found by playing whole games in the browser

The browser client (and then a script over the API) played one game per spirit on board A, with seed 100 plus the spirit's position and always taking the first option, until the game ended. 31 of 37 spirits reached a Victory/Defeat screen; the other six found these engine bugs. Each has a fix and a test on the `engine-bugs` branch (to be merged into `main`), after which all 37 spirits play to the end:
- **Finder of Paths Unseen**: A Circuitous and Wending Journey set a source quota once per kind of token, and a selector takes only one ("Cannot set Quota twice").
- **Grinning Trickster**: Let's See What Happens started a spirit-power scope with `ActionScope.Start`, which is for non-spirit actions.
- **Relentless Gaze**: a repeated power is not targeted again, so powers that read their origin lands from the targetting details (Unbearable Gaze) crashed. The repeater now keeps the origins and sets those details.
- **Starlight Seeks Its Form**: `ReclaimN` had no serializer, so the one-off Reclaim action from Gather the Scattered Light of Stars could not be saved.
- **Vital Strength**: Earth's Vitality gave Defend 3 only when a sacred site formed, but Defend is cleared every round and the starting sacred site never got it, so losing a sacred site threw "Defend sync issue". It now re-grants Defend each round and at the start.
- **Ember-Eyed Behemoth** and **Relentless Gaze (Scorching Convergence)**: a move from a land to the same land was offered; it moves nothing, leaves the token where it was selected from, and a mover with no limit asked for it again forever. `TokenMover` no longer offers it.

Not changed, but looks wrong: Relentless Punishment adds the repeater when `Sources.Any(s => spirit.Presence.CountOn(s) >= 0)`, which is always true; the rule asks for at least 3 presence in the origin land.

Also open:
- **Every-spirit coverage is shallow.** `EverySpirit_Tests` stops each game after 100 picks, so it only finds failures reachable early. Late-game saves (many tokens, blight cascades) and games played to the end are untested, as is the oracle's cost late in a game (Phase 0 measured early game only).
- **Prompts that time out.** The oracle gives up after 30 s waiting for the engine to reach a decision (`Settle`). `todo.txt` lists Ravage and Build prompts timing out as needing tests; they may surface here as 500s.

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
- **Log**: off the main screen, opened from the menu as a modal dialog (see the design goal below).

### Design goal: everything needed to act fits on one screen

The player should be able to see all the information they need to make the current decision at once, without scrolling the page. That means the board, the decision and its options, the spirit's panel (tracks, growth, innates, hand) and the status line (round, phase, fear, blight, invader cards) are always on screen together.

Anything that does not help with the decision is kept off the screen until asked for, and shown on top of the game while it is open:
- **The log** is hidden by default. A menu item opens it as a modal dialog that the player closes when done.
- Other reference material gets the same treatment: the discard pile, special rules, card text, and the game's number and seed. The hamburger menu in the status line is where these open from.

Rules of thumb that follow from it:
- A new element on the main screen has to be something the player acts on or needs to see to act. If it only explains what happened, it goes off-screen.
- Where the screen is too small for everything, the panels scroll inside themselves (the right-hand column already does), and the page itself does not.
- The page is checked at a normal laptop window size (about 1400 x 900) and zoomed out, since browser zoom is how players fit more on screen.

### Design goal: every decision is something you drag

For many players it is more natural when everything they do in a UI is moving something physical, so every decision should, where it can, read as "drag this onto that". Moving tokens already works that way (drag a token onto a land). The goal is to carry it through every decision the game asks for, not only moving tokens. In particular **powers are dragged onto their targets**: a power card or an innate power dropped on a land it can target, or on a spirit it can target, depending on what the power targets. Other decisions follow the same idea where there is a natural thing and place for it, for example dragging a card you may gain onto your hand, or dragging a presence from a track onto the land it is placed on.

The engine and server are shaped for this already:
- A decision's options say *a thing together with where it goes*, never two separate decisions: a move is a token with its destination, and a power is an action with its target (`TargetedAction`, the "act" option kind). The client does not need to ask the server anything while the player drags, because everything a thing can be dropped on is known from the decision's options.
- Targets have one generic form. A power that targets a spirit is offered with each spirit it can target, even when there is only one (solo play), and a growth action targets the spirit growing, so a UI can treat every power and action the same way: drop it on its target. That is why spirit targets are kept in the options even though a solo player has no choice.
- Dropping something where it cannot go, or releasing it away from any target, puts it back (the same as tokens today).

What is built now is the click path (pick the action, then click its target) and highlighting what a power can target while the pointer is on it. Dragging a card or an innate power onto a land or a spirit, and dragging a growth action onto the spirit, is not built. When it is, the drop targets are the lands on the island and the spirit panels, and the option the pair names is what is sent. Decisions that are not yet a thing-and-where (a plain list of choices such as "Done", picking which card to gain, answering a yes or no) need looking at one by one: some have a natural thing and place to drag between, and some will stay as buttons.

Client state: `gameId`, `checkpointVersion`, `picks[]`, the cached `StaticView` and the last response. Everything else comes from the server.

Assets: copy the token, icon and track images from `SpiritIsland.Maui/Resources/Images` into `wwwroot/img`. Cards are drawn from data for now, with no card art.

## Phases

Each phase ends with something that runs.

### Phase 3: minimal client (built)
Vite + Preact + TypeScript in `SpiritIsland.Web/client/`, with types generated from the server's OpenAPI document, built into `wwwroot/` and served by `SpiritIsland.Web`. It has the new-game screen (and a list of saved games), the island SVG with highlighted options, the decision panel with Undo, a plain status bar and spirit panel (energy, elements, cards), and `localStorage` for picks.
- **Exit:** a whole game can be played in the browser, ugly but complete. Met: games were played from the new-game screen to a Victory/Defeat screen in a real browser (see "Found by playing whole games" above for the engine bugs that stopped six spirits).

Running it:
- `cd SpiritIsland.Web/client && npm install` once.
- **Built:** `npm run build` (regenerates the types, type-checks, writes `wwwroot/`), then `dotnet run --project SpiritIsland.Web` and open http://localhost:5000.
- **Hot reload:** `dotnet run --project SpiritIsland.Web --urls http://localhost:5080`, and in `client/` run `npm run dev`; Vite proxies `/api` to the server.
- Building `SpiritIsland.Web` writes `client/openapi.json`; `npm run gen` turns it into `client/src/api/types.ts`. Both are committed.

How the client works, in short:
- It keeps `gameId`, `checkpointVersion`, `picks[]`, the static view and the last response. `localStorage` holds the version and picks per game, so a refresh replays the action in progress.
- A pick is sent as `{ checkpointVersion, picks: [...picks, pick] }`. A 200 replaces the picks with the server's list; a 409 (stale) takes the server's state and drops the picks with a notice; a 400 shows the message and changes nothing.
- Undo sends the picks without the last one. Every option is a button in the decision panel; options that are a space or a token are also highlighted and clickable on the island.
- The island is drawn in the server's own units (about 1.5 wide), so strokes are in screen pixels (`vector-effect: non-scaling-stroke`).
- Tokens are coloured circles with their text for now; pictures come with Phase 4.

### Phase 4: full player panels (built)
The log, prompt details, the spirit panel (tracks, growth, innate powers, special rules, cards with their text), token pictures and move arrows. What is left from the original list is a fuller status bar and the other arrows.

### Phase 5: polish (pick as needed)
- **One screen.** Move the log into a modal dialog opened from the hamburger menu, and check the rest of the main screen against the "everything needed to act fits on one screen" goal (first candidates: the discard list and the special rules).
- **Tokens jumping.** Positions are computed fresh each request, so tokens may move when others come or go. If that's distracting, send ordered slot lists and port the slot-assignment part of `ManageInternalPoints` (~100 lines) to TypeScript.
- **Drag every decision.** Drag a power card or an innate power onto the land or spirit it targets, and a growth action onto the spirit; then go through the other decisions one by one and make each a drag where it has a natural thing and place (see "every decision is something you drag").
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

