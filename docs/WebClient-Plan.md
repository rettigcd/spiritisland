# Plan: web play, solo first, then multiplayer

The end goal is multiplayer Spirit Island in a browser. Multiplayer needs a UI to play-test with, so the first step is a solo web client, built so it can later become the multiplayer one. The solo client is built and playable on the `web` branch; this file keeps the design and what is still open. Finished work is not listed here, so the code and the git log are the record of it.

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

## View projection

`View/` builds the player view, the static view and each option's target (`GameViewBuilder`, `StaticViewBuilder`, `OptionTargets`). The rule that outlives that work: use `PlayerObservation` as the checklist for what a player may see. Anything it hides stays hidden, and the tests check that the names of face-down fear cards and the unflipped blight card never appear.

## Design Decisions

Choices made for the web UI, briefly. The code is the detail.

**The screen**
- **Everything needed to act fits on one screen.** The board, the decision, the spirit panel and the status line are on screen together. Anything that does not help with the decision stays off screen until asked for and opens on top of the game: the log and a spirit's special rules are modal dialogs. The hamburger menu in the status line holds Games, Replay game, Land backgrounds and Log. A new element on the main screen has to be something the player acts on or needs in order to act. Where the screen is too small the panels scroll inside themselves, not the page. It is checked at about 1400 x 900 and zoomed out.
- **Nothing moves when something appears.** The growth list keeps one height, the elements row is always there, and the hint above the board has its own strip. A hint, button or label that shows up while dragging is drawn out of the flow, so the layout does not jump.
- **Board left, spirit panel right**, with a drag handle to resize the right column (the width is remembered).
- **Terrain pictures on the lands** (repeating SVG patterns), with a menu toggle for flat colours, remembered in `localStorage`.
- **Status bar:** round, phase, fear, blight, the Build and Ravage cards and the deck count. There is no Explore slot: that card is flipped and moved to Build within the Invader Phase, and the engine has already drawn the next one, so showing it would mislead. The seed is shown, and "Replay game" starts the same spirit, board, adversary and seed again (the first game stays saved).

**The spirit panel** follows the MAUI app, not the older desktop app.
- **Presence tracks:** each slot is its picture with a presence token on it, covering about half of it, and the slot's text is its tooltip. To place presence, click or drag the token, not the slot. Labels are "Energy" and "Card plays: N", with no space of their own. Beside the energy track is a large coin with the energy now; beside the card-plays track is the pile of destroyed presence.
- **Growth** lists each group with a G1, G2... label and says how many to pick ("Growth (pick 2)"). The count and the note on its restrictions are worked out from the spirit's pick groups on the server.
- **Spirit badge** (the round MAUI picture) sits in the top right, behind the content. It is where a power that targets a spirit is dropped.
- **Cards:** a card's line is its cost coin (on the fast or slow coin picture, so it carries the speed), its name, then its elements. The elements sit almost touching, so two cards fit on a line.
- **Card areas:** In play, Hand, Draw and Discard are always on screen, full width, with the name as a faint watermark and the cards left-aligned. A click on the empty part of an area folds or opens it; an area with no cards stays folded and does nothing; a folded area shows only its count. An area opens by itself when the decision is about one of its cards (the player can still close it) and when a card is dropped on it. In play and Discard start folded, Hand open. Draw appears only while cards are being drawn.
- **Special rules** are a button after the spirit's name that opens a dialog.
- **Card and innate details** open in a popover: press and hold a finger still on one (about half a second), rest the mouse on it, or right-click it. It replaces the hover tooltips, which touch cannot use. Moving cancels it, so it never clashes with a drag, and lifting the finger closes it. Cards being drawn show their full text inline instead, since the text is what the choice is made on.

**Picking and dragging**
- **Every decision is something you drag, where there is a natural thing and place.** Everything a thing can be dropped on is known from the decision's options, so the client asks the server nothing while dragging. What is sent is always the one option, so the request shape did not change. The click path (pick it, then click where it goes) always works too, so a drag is never the only way. A press that moves under 5 pixels is a click.
- **What can be dragged now:** a token from the board onto a land; presence from a track slot or the destroyed pile onto a land (the picture follows the cursor); a power card, an innate power or a growth action onto a land or onto a spirit's badge (a copy of its button follows the cursor); a card from the Hand onto In play to play it; a drawn card from Draw onto the Hand; a card from the Discard onto the Hand to reclaim it; a card onto the Discard when the decision is to discard it. Where a card may be dropped comes from what choosing it does: each card option carries `Target.Use`, the engine's `CardUse` (Play, AddToHand, Reclaim, Discard, Forget, Repeat...), read from the pending card decision. A Forget area, red-edged, appears below the Discard only while the decision is to forget one of the spirit's cards, and that card is dropped on it. A use with no card area (Repeat, Gift, Impend, Accept) is click only, and a card's details popover says what choosing it does.
- **Targets have one generic form.** A power is offered together with each target it can have (`TargetedAction`, the "act" option kind; on for every web game, set by `OracleSession`), and a growth action targets the spirit growing. Spirit targets stay in the options even when a solo player has no choice, so the client treats every power the same way. An action with a single target is chosen as soon as it is clicked. Powers that must ask something before they know where they can target, and Shroud of Silent Mist, stay as two steps (kind "card" or "innate").
- **Where a power targets a spirit, the badge is the drop site**, and a small "Use here" button on the badge is the click alternative. Dropping where it cannot go puts the thing back.
- **Seeing the target while dragging:** the land under the pointer, when a drop there is allowed, gets a red wash and a thick red outline (drags that start in the spirit panel tell the island which land they are over); a spirit's badge gets a red ring. Lines and rings are made to show on any terrain picture: land borders are a light band with a dark line in it, one look marks everything that can be picked or dropped on: a dashed orange line means "you can" and a solid green one means "this is the one" (picked up, or under the pointer). It is used for the ring on a token, a presence track slot, a spirit's badge, a card area and the lands a token, presence or power can go to (an orange wash, a green one on the land under the pointer; drawn a few pixels inside the land with `insetPolygon`, the same idea as the MAUI app's `Polygons.InnerPoints`, so the borders do not change). The dashes on the badge and the track slots are 3 pixels thick so the dash pattern is long enough to read. A thin dark edge under the token ring appears only on sand, where orange is hard to see. With a picked power that targets spirits, a spirit's badge shows a red ring while the pointer is on it and can be clicked, as well as dropped on.
- **Highlights:** pointing at an action, or picking it, highlights the lands (and spirit badges) it can reach. Moves draw an arrow from the token to its destination. Other arrows the desktop apps draw (`IHaveArrows`) are not drawn.
- **Prompt details and log:** the "Play power card ($E / N)" prompt is spelled out (`DecisionView.Details`). The log is the last 300 lines of what happened (rounds, phases, invader actions, blight, fear, presence destroyed, the end). Completed lines are stored in the game record with each checkpoint and the action in progress is made again on each replay, because the oracle keeps no state.
- **Pictures:** `TokenView.Image` is the engine's `Img` name (or "Presence:" and a spirit name), mapped to `client/public/img` by `images.ts`; a key with no picture is a labelled circle. Presence is one red picture turned to a hue from the spirit's name, unless the spirit has its own picture. Picture names follow MAUI's resource naming. `npm run copy-images` copies from the engine's and MAUI's image folders, and the copies are committed. Cards are drawn from data, with no card art.

**Games kept in the browser**
- The server keeps every game file forever and has no idea who played it. The Saved games list shows only games played in this browser in the last 30 days, newest first (kept in `localStorage`; older ones are forgotten there, not deleted on the server). The last four distinct spirits played come first in the spirit list, marked "(recent)".
- **First-time hints** (`hints.tsx`) are short notes that float at the bottom of the screen, taking no room and no clicks, and stay until the player has done what they teach, then never show again (the done list is in `localStorage`). The first one: "Long-press a power to see its details." Add others the same way.
- Picks of the action in progress are kept per game in `localStorage`, so a refresh loses at most the current action.
- Every API call carries a shared access token (`X-Access-Token`; `AccessToken.Value`, "blue32" to start). The page is served without it, so the client asks for it when a call is turned down and keeps it in `localStorage`. It is one token for everyone, over plain HTTP, so it only keeps casual visitors out.

## Generated types

The C# records and endpoints are the single source of truth for the API:

1. The server describes its endpoints with ASP.NET's built-in OpenAPI support (`Microsoft.AspNetCore.OpenApi`). `Microsoft.Extensions.ApiDescription.Server` writes `openapi.json` at build time.
2. In the client, `openapi-typescript` turns `openapi.json` into `src/api/types.ts`, and `openapi-fetch` gives typed calls from it.
3. An npm script (`npm run gen`) regenerates the types. The client build runs it first, so a changed record breaks the TypeScript build instead of failing at runtime.

The generated file is committed, so the client builds without running the server.

Watch for: option targets and other union-like records. Model them as one record with a `kind` enum plus optional fields, which OpenAPI describes cleanly, rather than as a C# class hierarchy.

## Running it

- `cd SpiritIsland.Web/client && npm install` once.
- **Built:** `npm run build` (regenerates the types, type-checks, writes `wwwroot/`), then `dotnet run --project SpiritIsland.Web` and open http://localhost:5000.
- **Hot reload:** `dotnet run --project SpiritIsland.Web --urls http://localhost:5080`, and in `client/` run `npm run dev`; Vite proxies `/api` to the server.
- **From other devices on your network:** add `--lan` after `--`, as in `dotnet run --project SpiritIsland.Web -- --lan`. The server then listens on every network interface and prints the addresses to type on the other device. Windows asks to allow the program through its firewall the first time. Without `--lan` only this computer can connect.
- **From the internet:** add `--wan` (it includes `--lan`). The server asks the router, over UPnP (Mono.Nat), to forward its TCP port: leased for 1 hour, renewed every 30 minutes, removed when the server stops normally. It logs the outside address and tries it from the server as a check; the check can fail while forwarding works, because many routers do not loop back, so also try it from a phone with Wi-Fi off. If no router answers or it refuses, that is logged and the server carries on on the local network. It warns when the router's outside address is private or carrier-shared, and when the access token is still "blue32". It has been tried on a real router and works. The tests use a fake one (`WanPortForwarder`, `IRouter`); `MonoNatRouter` is the only part that talks UPnP.
- Building `SpiritIsland.Web` writes `client/openapi.json`; `npm run gen` turns it into `client/src/api/types.ts`. Both are committed.

## Still open

**Engine and oracle**
- **The engine fixes are on `web`, not `main`.** They reach `main` when `web` is merged.
- **Every-spirit coverage is shallow.** `EverySpirit_Tests` plays each spirit for 100 random picks and fails on any engine error (its `KnownFailures` list is empty; add a spirit there only while a known engine bug stops it). It only finds failures reachable early. Late-game saves (many tokens, blight cascades) and games played to the end are untested, as is the oracle's cost late in a game (`OracleCost_Tests` measured about 7 ms a request in rounds 1-4).
- **Prompts that time out.** The oracle gives up after 30 s waiting for the engine to reach a decision (`Settle`). `todo.txt` lists Ravage and Build prompts timing out as needing tests; they may surface as 500s.

**Dragging**
- **Spirit targets in multiplayer.** When several spirits are eligible, their badges should appear next to the player's own so one can be chosen. Only a single spirit exists in solo play, so this is untested.
- **The major and minor decks on screen**, so that gaining a card is a drag from the deck. Today the choice of deck is two buttons in the decision panel, and only the cards drawn show in the Draw area.
- **The other decisions**, one by one (a plain list of choices such as "Done", answering yes or no): some have a natural thing and place to drag between, some stay as buttons.
- **Not tried:** touch screens, and dragging an innate power, a growth action or the destroyed-presence pile in a real game (they share the code that was tried).

**Looks and information**
- **Game info:** adversary, level and its description (`AdversaryView` already has the name, level and rules; a longer description needs adding), opened from the menu as a dialog.
- **Counts as disks.** A stack shows as tokens on a red disk (5) and white disks (1), as in the physical game, instead of "x7". Try it on a busy land first, since it competes with the token for room.
- **Damage as marks.** A damaged city or town gets evenly spread red slashes instead of the remaining-health number.
- **Tokens jumping.** Positions are computed fresh each request, so tokens may move when others come or go. If that is distracting, send ordered slot lists and port the slot-assignment part of `ManageInternalPoints` (~100 lines) to TypeScript.
- **Island rotation** to fit the screen (MAUI computes it from the corners).
- **Mobile layout.**
- **Response diffs** (`ObservationHistory` pattern), if the payload size matters.

**Ideas to look at again later**
- A popup showing all the cards of a group (draw pile, hand, discard) as whole cards. It wants card artwork, which is not decided.
- A big and a small view of each of the cards, the spirit panel and the board, all always visible. This is a layout system rather than a feature; the folding card areas already cover part of it.

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
- Accounts and real authentication. The shared access token and `--wan` are a stopgap.
- Changes to the MCP server.
