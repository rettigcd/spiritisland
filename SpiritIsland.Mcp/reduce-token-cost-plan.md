# Plan: reduce the token cost of playing through the MCP server

Playing Spirit Island through the MCP tools used a lot of tokens. This file records what was measured, what has been done, and what to do next.

## How the cost builds up

Every tool result stays in the conversation, and the model re-reads the whole conversation on every turn. So the cost is roughly:

**number of tool calls (turns) × size of the growing context**, plus the model's reasoning on each turn.

Making individual responses smaller helps a little. Making fewer calls, and having less for the model to work out by hand, helps much more.

## Measurement

`ToolCallLog` ([Mcp/ToolCallLog.cs](Mcp/ToolCallLog.cs)) is a call-tool filter registered in [Program.cs](Program.cs). It writes one CSV row per tool call:

`timestamp, session, tool, requestChars, responseChars, approxTokens, isError, ms`

- The log goes to `tool-calls.csv` next to the executable (`bin/Debug/net10.0/`), unless the `SPIRITISLAND_TOOL_LOG` environment variable sets another path.
- Sizes count the text the model reads, not the wire JSON. On the wire every `"` is escaped as `"`, which would overstate the size.
- It does not count the model's own reasoning or the cost of re-reading the context. The number of calls is the best stand-in for those.
- For error results the size is approximate: it counts the exception message only.

Per-tool totals (PowerShell):

```powershell
Import-Csv .\bin\Debug\net10.0\tool-calls.csv | Group-Object tool | % { [pscustomobject]@{tool=$_.Name; calls=$_.Count; tokens=($_.Group | Measure-Object approxTokens -Sum).Sum} } | Sort-Object tokens -Desc
```

### Baseline: 3 rounds, Lightning's Swift Strike, board A, seed 1

| Tool | Calls | Response chars | Largest | ~Tokens |
|---|---|---|---|---|
| `select_action` | 35 | 13,458 | 1,573 | 3,804 |
| `get_observation` | 3 | 9,944 | 3,399 | 2,486 |
| `create_game` | 1 | 651 | 651 | 178 |
| **Total** | **39** | **24,053** | | **~6,500** |

Findings:

- **The number of calls is the problem, not the size of each response.** That's about 13 calls per round, so a full ~10-round game is around 130 turns over a growing context.
- **`get_observation` is the largest single response.** It was about 850 tokens at round 1 and grows as the board fills. It was called once per round here. PLAYING.md says to call it before every decision, which would make it the biggest cost.
- **Most of the model's reasoning went into predicting ravage outcomes by hand**, for example "A8 has an explorer and a town, so 3 damage, so blight unless Shatter removes the town." PLAYING.md says "Nothing predicts this for you."
- **9 of the 39 calls (about 23%) were mechanical:**
  - 4 decisions with only one option: the second half of a growth group (3) and a fear card with one target land (1).
  - 3 "Select Fast to resolve" prompts with only one power left besides "Done". The rule to always resolve played powers usually decides those, but not always (see below).
  - 2 "minor or major?" prompts.

### Resolve order is a real decision

When two or more powers are waiting in "Select Fast/Slow to resolve", the order can matter. For example:

- Harbingers of the Lightning gives 1 Fear only when it pushes Dahan into a land that still has a Town or City, so it has to resolve before Shatter Homesteads destroys that town.
- Portents of Disaster has to resolve before the power that destroys an Invader in its land.

So the server must never pick a resolve order.

Even the last remaining power, when nothing is left but it and "Done", is not always mechanical. Occasionally you don't want it to resolve (see Done, item 5). Counting those 3 calls as real decisions, only 6 of the 39 calls (about 15%) are always mechanical.

## Done

1. **Tool-call size logging.** `ToolCallLog`, described above.
2. **Auto-select single-option decisions.**
   - In `AgentGame.GetDecision()`, a decision with exactly one option is chosen automatically. It is recorded in `History`, so games still replay the same way.
   - The pick is reported as an alert of kind `auto` on the next decision the agent sees, in time order with the other alerts, for example `Auto-selected 'A6' (only option for 'Select space to Gather up to 1 Explorers')`.
   - The decision version only goes up for decisions the agent is shown.
   - `AutoSelectSingleOption = false` turns it off.
   - Alerts recorded after the last decision, including the game-over entry, are now returned in the `gameOver` response (`FinalAlerts`).
   - "Up to" effects can still be declined, because the prompt that follows offers "Done".
   - Tests: `SingleOptionDecisions_AreAutoSelected_AndReportedAsAlerts`, `AutoSelect_PlaysTheSameGameAsChoosingTheOnlyOptionByHand`.

3. **Smaller observation (`get_static_info`).**
   - `get_static_info` holds what rarely changes: setup and adversary rules, a legend for codes, victory conditions per Terror Level, terrain and adjacency, special rules, growth groups, presence track slots, innate tiers, and the full text of the starting cards.
   - `get_observation` keeps what changes. Track progress is a count of uncovered slots, hand/inPlay/discard show name, cost, speed and elements, `innateTierStatus` replaces per-tier `elementStatus`, and lands without tokens are left out.
   - `staticVersion` is a hash of the static content, carried in every observation, so it changes whenever anything static changes, logged or not.
   - Measured on Lightning, A, seed 1, round 1: the observation went from 3,399 to 1,250 characters (-63%). `get_static_info` is 3,559 characters once, so it pays for itself after 2 observations.
   - Tests: `StaticInfo_HoldsLayoutSpiritRulesAndStartingCards`, `StaticVersion_MatchesObservation_AndStaysTheSameWhilePlaying`, `StaticInfo_DoesNotRevealHiddenCards`.

4. **Batch actions (`select_actions`).**
   - `select_actions(version, actions)`: each action is `{"option": "...", "prompt": "optional start of the prompt"}`. Steps are applied in order, starting with the decision of that version (a stale version is rejected, as with `select_action`).
   - The batch stops early, without error, at the first step whose option isn't offered or whose `prompt` doesn't match the start of the actual prompt. That way an unexpected prompt, such as a fear card's, is never answered by mistake. The response includes `batch: {applied, requested, stoppedBecause}` and the decision where it stopped.
   - A step naming an option the server just auto-selected counts as done, so the model doesn't need to know which steps will be auto-picked.
   - The returned alerts cover every decision passed through, in order, with one `batch` alert per step: `Selected 'A8' for 'Shatter Homesteads: Target Space'`.
   - Measured on Lightning, A, seed 1: all of round 1 (growth, presence, 2 cards, both resolved with targets) took **1 call instead of 10**. The response was 2,460 characters.
   - Tests: `Batch_PlaysTheSameGameAsOneAtATime_IncludingStepsThatWereAutoSelected`, `Batch_ReportsEachStep_AndKeepsTheAlertsOfDecisionsItPassedThrough`, `Batch_StopsAtAPromptThatDoesNotMatch_WithoutAnsweringIt`, `Batch_StopsAtAnOptionThatIsNotOffered_AfterApplyingTheStepsBefore`, `Batch_RejectsAStaleVersion`.

5. **Warn when "Done" skips powers (no auto-resolve).**
   - The server can't resolve powers for the model: most powers start their own decisions (targets, what to push or gather), and occasionally skipping one is right, e.g. a forced Remove Blight another spirit may need, or a Stop Ravage on a land you need to ravage. So resolving stays the model's decision, and `select_actions` is how to save calls on it.
   - Instead, choosing "Done" on "Select Fast/Slow to resolve" adds a `skipped` alert naming the powers that won't resolve this turn. In the Fast phase, a power that can still resolve in the Slow phase (e.g. a Slow card offered early by Lightning's Swiftness) isn't listed. A deliberate skip still works; an accidental one is no longer silent. This doesn't save tokens; it helps the model notice a mistake.
   - Test: `Done_ThatDropsPlayedPowers_IsReportedAsSkipped_ButNotWhenTheyComeBackInTheSlowPhase`.

## Next steps, in order of payoff

### 1. `predict_invader_phase`

- Clone the state and run the coming Ravage, Build and Explore using only the revealed Invader cards. Return a short report, for example: `A4: ravage 5 damage vs 2 Dahan -> blight (cascade risk); A6: build town; blight on card 3 -> 1`.
- This uses only information a player could see, so it's fair.
- Choices inside the simulation, such as cascade targets, need a simple default, or the report should list the possible outcomes.
- This replaces the most expensive and most error-prone reasoning, and should also improve play.

### 2. A smaller observation, further

- `get_observation(sinceVersion)`: only the lands and fields that changed.
- Optionally a compact text format instead of JSON, for example `A4 Sands: 2E 1T 1B | D2 | P1`.

### 3. Targeting helper: `get_card_targets(card)`

- Return the lands a power can legally target, given range and sacred sites, so the model doesn't work it out by hand.

### 4. `evaluate_options` (later)

- For each option, branch from the saved state, play forward with a heuristic agent, and score the result.
- Each rollout must reshuffle the cards the player hasn't seen (fear deck, Invader deck, power decks). Otherwise the rollouts use hidden information.

## The biggest long-term saving

Keep the LLM out of routine moves:

- Add a `HeuristicAgent` next to [RandomAgent.cs](Agent/RandomAgent.cs), with a configurable policy: growth priorities, which Presence track to focus on, and which threats to defend first.
- The LLM writes and tunes that policy from game logs and [GAMELOG.md](GAMELOG.md). It steps in only at decisions the heuristic marks as close.
- Simulated games then cost no tokens, which fits the strategy-discovery goal in [mcp_server_info.md](../mcp_server_info.md).

Changes that need no code:

- Run routine play on a cheaper model (Sonnet or Haiku).
- Trim [PLAYING.md](PLAYING.md), which is loaded every session.

## Notes

- While Claude Code is running the MCP server, it locks `bin/Debug/net10.0`, so building there fails.
  - To rebuild the server: stop it (`/mcp` or restart Claude Code), build, then reconnect.
  - To run the tests while it's running, build to another folder: `dotnet test SpiritIsland.Tests -p:BaseOutputPath=<other dir>/`.
- After each change, measure again by playing the same 3 rounds (Lightning, A, seed 1) and comparing calls per round against the baseline above.
