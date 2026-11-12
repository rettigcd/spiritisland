# Spirit Island AI Player & Strategy Discovery — Claude Code Handoff

**Status:** Project discovery / implementation planning  
**Audience:** Claude Code working in the existing Spirit Island engine repository  
**Purpose:** Transfer confirmed context, objectives, design proposals, and next steps.  
**Important:** This document describes discussions with the project owner. It is **not** evidence that any proposed APIs, types, or MCP tools currently exist. Inspect the repository before implementation.

## 1. Executive summary

The owner has built a functioning game engine for the cooperative board game **Spirit Island**. The engine currently powers a human-facing UI. At every decision point, the engine determines the **valid options**, presents them to the UI, accepts a selection, executes the appropriate game logic, and advances to the next decision point. The engine can **save and restore the full game state, including random-number-generator state**.

The owner wants to add an **AI player**, initially via a **Model Context Protocol (MCP) server** so an LLM (including Claude Code) can configure a game, inspect the player-visible state and legal choices, choose an option, and continue until the game ends. The longer-term ambition is **strategy discovery**: repeat games hundreds or thousands of times, compare strategies under controlled conditions, branch from saved states, analyze outcomes, and discover effective, explainable approaches to play.

**Core architectural principle:** Keep the game engine authoritative for rules and legal actions. The AI chooses from engine-provided options; it does not implement game rules.

## 2. Confirmed existing capabilities

The owner explicitly confirmed:

1. A Spirit Island game engine exists and is used by a human UI.
2. The engine knows which options are valid at **every decision point** and presents those options.
3. A selection causes the engine to run and present the next set of options.
4. The current UI facilitates human option selection.
5. The UI has access to the **entire game state** and extracts the data it needs for display.
6. The internal game state contains **hidden information**, including face-down cards. The UI intentionally does not expose that information to the player.
7. The engine supports **saving and restoring complete game state, including RNG state**.
8. The owner wants AI-controlled game setup, explicitly including **Spirit, board, and random-number seed**.

**Not yet established:** repository layout, language/framework of this particular engine, public APIs, serialization format, headless execution support, concurrency/thread safety, multiple Spirits, Adversary support, exact game-end reporting, or whether a reusable player-view projection already exists. The owner is experienced with C#/.NET, but **do not assume this engine is written in .NET without inspecting the code**.

## 3. Goals and non-goals

### Immediate goal: one complete AI-played game

Provide an MCP interface that lets an LLM:

- Discover available setup choices.
- Create a new game with a specified Spirit, board, and seed.
- Observe only information a legitimate human player could know.
- Obtain the current legal choices.
- Select one of those choices by stable ID.
- Repeat until victory, defeat, or another terminal state.
- Retrieve the outcome and relevant statistics.

### Intermediate goal: reproducible experiments

- Play many games with recorded configurations and seeds.
- Persist action traces and outcomes.
- Compare different agents/strategies on common sets of seeds.
- Save and restore states for branching experiments.
- Distinguish known state from hidden state in all agent-facing observations.

### Long-term goal: strategy discovery

- Have an LLM formulate strategic hypotheses and explain them.
- Test strategies using automated simulations.
- Explore alternative actions from identical saved positions.
- Compare LLM decision-making, heuristic policies, and search methods (e.g., Monte Carlo lookahead / MCTS).
- Potentially train a learned policy using reinforcement learning later.
- Discover strategies for different Spirits, boards, Adversaries, and eventually custom Spirits.

### Non-goals for the first implementation

- Training a neural network.
- Rewriting game rules in the MCP server or LLM prompt.
- Building a distributed simulation platform.
- Assuming that repeated prompting automatically updates an LLM's model weights.
- Giving the agent access to face-down cards during normal play.

## 4. Recommended conceptual architecture

```text
                 Claude Code / LLM client
                           |
                       MCP tools
                           |
                 Spirit Island MCP adapter
                           |
             Player-facing observation/action API
                           |
                Existing Spirit Island engine
               (rules, state, choices, RNG)
                           |
                   Save / restore support

Separately, later:
 Experiment runner -> same engine API -> agents/policies -> metrics
```

The **MCP server is an adapter**, not the core game abstraction. Prefer extracting or reusing an engine-facing interface so other clients can drive the same game without MCP overhead. Preserve the existing human UI.

A useful conceptual interaction is:

```text
create_game(config)
  -> get_observation / get_legal_actions
  -> select_action(actionId)
  -> get_observation / get_legal_actions
  -> ...
  -> get_game_result
```

If the engine already exposes a combined decision object containing state and choices, **reuse it** rather than creating redundant APIs. The tool surface below is provisional and should be adapted to actual code.

## 5. Proposed MCP tool contract (not yet implemented)

| Tool | Responsibility | Notes |
| --- | --- | --- |
| `get_game_configuration_options` | List available Spirits, boards, and supported setup settings | Return identifiers the engine recognizes; do not invent combinations |
| `create_game` | Start game with Spirit, board, seed, optional settings | Return game ID and/or first decision |
| `get_observation` | Return current **player-visible** state | Filter hidden deck orders, face-down cards, etc. |
| `get_legal_actions` | Return the current valid selections | Stable, unambiguous action IDs; include human-readable descriptions |
| `select_action` | Apply one of the offered choices and advance | Validate against the current decision; return updated decision or status |
| `get_game_result` | Return terminal outcome and available metrics | Victory/defeat and other engine-supported data |
| `save_game` | Capture a full state snapshot | Optional in MVP if the engine's existing mechanism is easy to expose |
| `restore_game` | Restore a prior snapshot | Ensure RNG state is restored too |

### Example configuration (illustrative)

```json
{
  "spirit": "River Surges in Sunlight",
  "board": "A",
  "seed": 12345
}
```

The example is **not** an assertion about engine IDs or schema. Let the engine provide authoritative identifiers and validation. If multiple games are supported, include a `gameId` in relevant requests and responses. Design for eventual multiple game instances, but do not prematurely implement concurrency.

### Action-selection safeguards

- An action ID must identify exactly one offered action at the current decision point.
- Prefer including a decision version/token so stale choices can be rejected.
- Invalid selections should produce clear errors, not silently advance the game.
- Engine state mutation should be atomic with respect to each selection.
- Automatic/noninteractive phases should advance using existing engine logic, not LLM guesses.
- Keep MCP responses concise enough for repeated model use; provide targeted detail or summaries where needed.

## 6. Player observations vs. complete internal state

**Critical confirmed fact:** The engine state contains hidden information that the human UI does not show. Therefore, returning the raw state to the LLM would permit cheating.

Build or reuse a **player observation projection** that includes all and only the information available to the relevant human player. This should be independent of UI rendering logic where practical.

Examples of likely visible information (verify exact game rules and engine implementation):

- Current phase/turn and board state.
- Spirit tracks, Presence, Energy, available Power cards and elements.
- Revealed Invader information and public game counters.
- Available legal actions and their parameters.

Examples of likely hidden information:

- Unrevealed Invader cards and their order.
- Undrawn Power cards and their order.
- Other face-down information not legitimately revealed.

Use the game's rules to determine exceptions (e.g., legitimately revealed cards). For development, an explicitly named **perfect-information diagnostic mode** may be useful, but keep it separate from normal agent play and label results accordingly.

## 7. Determinism, seeds, and experimental validity

The engine can save and restore RNG state. This is a major advantage for repeatability and branch analysis.

- Persist game configuration, seed, engine/rules version, and ordered action history.
- Verify that restoring a snapshot and replaying the same actions produces the same result.
- For strategy comparisons, use the same set of starting seeds when feasible.
- Be aware that alternative choices may consume RNG differently, so identical seeds do **not** guarantee identical future random events.
- Replaying one known future is useful for debugging but does not measure expected value under uncertainty.
- For statistically meaningful lookahead, sample multiple plausible hidden states/random futures consistent with the player's observations; do not let a normal-playing agent optimize against an unrevealed fixed deck order.
- Reserve some seeds/configurations as **held-out evaluation** to detect overfitting.

## 8. Strategy discovery: recommended evolution

### Stage A — LLM player

The model repeatedly observes, chooses a legal option, and records outcomes. Test whether it can complete games and make coherent decisions. LLM tool calls may be costly if every tiny choice requires a separate round trip.

### Stage B — Baseline agents

Introduce agents such as random-legal-choice and simple heuristics to establish meaningful comparisons. A conceptual interface could be:

```csharp
// Illustration only: adapt language and types to the actual repository.
public interface IGameAgent
{
    Task<string> ChooseActionAsync(GameDecision decision);
}
```

Possible implementations: `HumanAgent`, `LlmAgent`, `RandomAgent`, `HeuristicAgent`, `SearchAgent`. Avoid imposing this exact interface if the existing architecture already provides a better seam.

### Stage C — Experiment runner

A separate runner, not the MCP server itself, should orchestrate batches, capture outcomes, and compare policies. An LLM can propose hypotheses and analyze aggregate evidence rather than making every micro-decision in every simulated game.

### Stage D — Branching search

From a saved state, evaluate several actions using rollouts. Start with Monte Carlo lookahead and a baseline rollout policy; consider MCTS later. The strength of the rollout policy and the treatment of hidden information strongly affect validity.

### Stage E — Learning (optional)

If desired, expose the environment through a conventional reinforcement-learning interface. This is distinct from an LLM remembering game summaries in a prompt; training a policy requires a separate learning process.

## 9. Suggested phased implementation plan

**Phase 0 — Repository reconnaissance**

1. Locate game state, decision/options, selection, progression, setup, and save/restore APIs.
2. Identify how the UI obtains state and hides face-down information.
3. Determine whether the engine can run without a UI event loop or user interaction.
4. Identify existing tests, fixtures, logging, and serialization support.
5. Propose a minimal integration design based on actual code. Do not refactor broadly before discussing findings.

**Phase 1 — Headless player adapter**

1. Implement or expose a clean way to initialize a game from config/seed.
2. Read the current legal decision.
3. Select an action and advance to the next decision or terminal state.
4. Expose a filtered observation.
5. Prove a simple automatic agent can finish at least one game.

**Phase 2 — MCP integration**

1. Expose setup discovery and game creation.
2. Expose observation, legal choices, and action selection.
3. Expose terminal outcome.
4. Add clear validation/errors and session/game identity if appropriate.
5. Test from Claude Code through a configured MCP connection.

**Phase 3 — Repeatability and telemetry**

1. Save configuration, seed, action trace, outcome, engine version.
2. Add replay/determinism tests.
3. Add basic batch execution outside MCP.
4. Establish metrics: win rate, game length, Blight, Fear, and any other relevant engine-supported measurements.

**Phase 4 — Strategy experiments**

1. Compare baseline and LLM policies over paired seed sets.
2. Introduce explicit, versioned strategy descriptions.
3. Let an LLM propose new hypotheses and interpret results.
4. Add snapshot branching and Monte Carlo evaluations where valuable.

## 10. Acceptance criteria for the first milestone

- Claude Code can discover supported game configurations through MCP.
- Claude Code can create a game specifying a Spirit, board, and seed.
- Claude Code receives legal choices and an adequate player-visible observation.
- Claude Code can select one choice and reliably advance the engine.
- Claude Code can repeat this interaction to a terminal game state without the human UI.
- The AI cannot see face-down cards or hidden deck order in normal mode.
- Invalid/stale action IDs are rejected safely.
- The original human UI remains functional.
- A saved/replayed game can be reproduced when the same choices and RNG state are used.

## 11. Questions to resolve by inspecting code / asking the owner

- What language, solution layout, and runtime does this engine actually use?
- Which classes currently represent game state, options, and action selection?
- Does a choice have a stable ID, or is it represented by callbacks/objects/UI bindings?
- Can the engine advance headlessly, including automatic phases?
- Is there already a player-visible projection, or must one be created?
- How are Spirit, board, seed, and optional settings represented?
- What exactly is included in save/restore, and how are snapshots persisted?
- Can games be instantiated independently? Is any mutable state global/static?
- How are terminal outcomes and performance metrics represented?
- What is the approximate number of selections per game and typical decision branching factor?

## 12. Instructions to Claude Code for the first session

**Do not immediately implement a large framework.** Start by examining the repository and reporting:

1. The existing game flow from UI-presented options to engine state transition.
2. The minimum code changes needed to drive that flow without the UI.
3. How to construct a player-visible observation without leaking hidden state.
4. How setup with Spirit, board, and seed currently works.
5. Whether existing save/restore supports deterministic replay.
6. A concrete MVP MCP tool contract tailored to the real code.
7. A small, testable implementation plan, with any questions requiring owner decisions.

After that review, seek approval for the design and implement the smallest working vertical slice: **create game → observe options → select option → advance**. Extend it to a full game before building batch experiments or reinforcement learning.

## 13. Suggested opening prompt for Claude Code

> Read `Spirit_Island_AI_Claude_Code_Handoff.md` as the project brief. Inspect this repository to understand the existing engine and UI integration. Confirm which assumptions in the brief are accurate. Do not implement anything yet. Propose the smallest architecture and phased plan for allowing an LLM to configure and play a full Spirit Island game through an MCP server, with player-visible observations only. Identify concrete classes/files to reuse, risks, and questions before making changes.

---

**Guiding objective:** Build a reproducible, rules-correct, non-cheating AI interface first; use it later to test and discover effective Spirit Island strategies.
