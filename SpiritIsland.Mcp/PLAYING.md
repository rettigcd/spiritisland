# Playing Spirit Island through the MCP tools

Read `docs/rules.md` first and don't play from memory. This file adds what the engine and tools do, general lessons, and how specific spirits differ from the base rules. The tool descriptions cover each tool's fields; only what they don't say is here.

## Ground rules

- **Play only from what the tools show.** Don't read the game's source code for what a card, fear card or blight card does: a player only knows their own cards, revealed Invader cards, and what alerts report. If something you need isn't shown, report it as a gap. (Reading source is fine when building or fixing the tools.)
- **Resolve every power you played, and every innate that's active**, even if it does little. This doesn't mean "play as many cards as you can". Rare exception: skip a power whose effect is forced and hurts you (e.g. a forced Remove Blight another spirit may need, or a Stop Ravage on a land you need to ravage), and say why.
- **Log each game in [GAMELOG.md](GAMELOG.md):** per round, the growth group, the track each Presence came from, income and card plays, and what the round achieved. Don't draw conclusions from one game.
- If a prompt isn't understood, stop and ask the user instead of guessing.

## Each round

1. `get_static_info` once per game (layout, spirit rules, innate tiers, track slots, legend, starting cards). Again when the observation's `staticVersion` differs, or it's no longer in your context.
2. `get_observation` with `sinceId` = the `observationId` of the last observation you still have (only changes come back; leave it out if you no longer have that observation). Once per round, and after anything unexpected.
3. `predict_invader_phase`: what this round's Ravage, Build and Explore will do per land, and blight left on the card. Read `notIncluded`: face-down fear cards and unresolved powers aren't simulated, and `choicesMade` were the first option, not yours.
4. After growth, `get_power_targets` for every power's reachable lands.
5. Plan, then send the round with `select_actions`. Re-run the prediction after a Fast power changes the board if in doubt.

## Tools: what the descriptions don't say

- **Alert kinds:** `auto` = a decision with one option was chosen for you (you never see it). `skipped` = "Done" on a resolve prompt dropped powers that won't resolve this turn; if that wasn't deliberate, you lost them. `batch` = one per `select_actions` step, naming the prompt it answered. A fear card's name and effect arrive as a `fear-blight` alert in the same decision as its prompt (the prompt itself doesn't name it). After the game ends, the last alerts come with the `gameOver` response.
- **`select_actions`:** give `prompt` for any step whose option could answer another question (land names especially). It stops at the first step that doesn't fit; read the returned decision and continue. End a batch before a decision you can't plan yet (e.g. which drawn card to take). Steps that would be auto-selected count as done.
- **Observation diffs:** keys left out are unchanged, `null` means removed; arrays (e.g. `hand`) and each land under `spaces` come whole. Lands with no tokens are left out. Defend and Isolate show as tokens with a count.
- **Hidden information:** earned fear cards stay face down until the next Invader Phase. The blight card's name, effect and back-side count are hidden until it flips; when `blightRemainingOnCard` gets low, assume something bad happens at 0.
- `get_card_details` covers cards you hold, have in play or discard, or are offered; not the power decks.

## Engine and prompt mechanics

- **Growth:** pick ONE group per round, then everything in it (follow-up prompts like "Place Presence" or "Gain Power Card" belong to it). Within a group, the first Presence placed extends range for the next. The prompt after the card-play "Done" is the next round's growth.
- **Card play:** `Play power card ($E / N)` = energy and plays left. Pick cards, then "Done" (the list ends by itself when nothing is playable). Then "Select Fast to resolve" lists played powers and active innates; you choose the order. **"Done" there skips everything still listed.**
- **Resolve order matters** when one power sets up another (e.g. a power that gives fear for a Town in the land must resolve before the power that destroys the Town).
- "Up to" effects can be declined: the follow-up prompt offers "Done".
- Fear card prompts come at the start of the Invader Phase, for cards earned earlier.
- Drawing a major forces you to forget a card; a minor doesn't.
- The game is won the moment the condition is met, even mid-Invader Phase (a fear card removing the last Invader wins).

## Rules learned (beyond docs/rules.md)

- Slow powers resolve after the Invader Phase, so only Fast powers stop this round's ravage; a Slow push or kill affects next round.
- **Blight:** a ravage blights a land at 2+ damage (Explorer 1, Town 2, City 3). Adding blight to a land that has blight cascades to an adjacent land (you choose: pick one without blight or Presence), and blight destroys 1 Presence per spirit in the land.
- **Strife:** a strifed Invader deals no damage in Ravage. **Defend N** reduces total ravage damage by N.
- **Isolate:** the land is cut off from adjacent lands for exploring (no Explorer from neighbouring Towns, Cities or Ocean).
- **Explore** adds an Explorer only where a land holds or touches a Town, City or the Ocean, so killing Towns and Cities starves Explore. Coastal lands are those touching the Ocean.
- **Fear:** pool of 4 per player; a destroyed Town is 1 Fear, a City 2. Terror Level 2 (3 cards earned) wins with no Towns or Cities; Level 3 (6 cards) with no Cities; all fear cards earned wins outright. The Terror Level changes the moment a card is earned.
- **Range:** "N:ss" is range N from a sacred site (2+ Presence in a land, or what a special rule says); with no sacred site such a power has no targets. Other ranges count from any of your Presence.
- The Invader deck runs out around round 11 (Stage I-III), which loses. Plan to win before the Explore with no card.
- Once the blight card flips, its Blighted Island effect can change the rules (fewer blights to lose, Invaders weaker, blight destroying more Presence, losing Presence for big cards). Read the alert and re-plan.

## Strategy (all spirits)

- **Know the spirit through its unique cards and innate**: they usually combine to meet the innate's top tier. Drawn cards supplement them. Prefer drawing minors (no forced forget).
- **Prevent Builds early.** Each Build opens new lands to Explore. Kill or push the Explorers in next round's Build lands (`matchedLands.build`), pushing toward the coast so inland lands have nothing adjacent.
- **Each round, give each ravaged land that would blight a Fast answer:** Defend, strife, removing enough Invaders (destroying the Town often leaves one Explorer = 1 damage, no blight), or pushing them out. Lands that already have blight come first (cascade).
- **Dahan fight back:** gather Dahan into a land with a City or Town before it ravages; enough Dahan kill it in the counterattack.
- **Consolidate, then sweep:** push scattered Invaders into one land, then hit it with an area effect.
- **Win via the Terror Level** rather than clearing every Invader, unless the spirit sweeps well. Count Towns/Cities left against fear cards earned; time the last kill so its own Fear completes the card.
- **Presence placement:** put sacred sites next to where the threats will be a round ahead; keep the last City within reach. Presence in lands that will blight gets destroyed.
- **Growth:** unlock card plays early enough to reach the innate's tiers; energy beyond what you can spend is wasted. Some spirits have one growth sequence across rounds that beats all others: compare whole sequences on the same seeds (GAMELOG.md).
- Cards go to the discard after use: a key defensive card is only back after a reclaim, so plan reclaims around when you need it.
- Useful kinds of drawn cards: Fast Defend, strife, Fear generators, Fast damage to finish the last pieces, blight removal, Isolate.

## Spirits: how they differ from the base rules

Base: one growth group per round; Presence tracks for energy and card plays; sacred site = 2+ Presence in a land; Slow powers resolve after the Invader Phase; innate tiers are met by the elements of the cards you play. Read each spirit's `specialRules`, growth groups and innate tiers from `get_static_info`; add a section here only for what differs and has been learned in play.

### River Surges in Sunlight

- **Special rule:** Presence in a Wetland counts as a sacred site (`sacredSites` lists it with 1 Presence).
- Starts with 1 Presence, 1 energy and 1 card play; the card-play track has a "reclaim 1" slot.
- **Its four unique cards together meet Massive Flooding's top tier** (2 damage to each Invader in one land), for $3 but 4 card plays. So grow the card-play track first.
- **Opening that wins by round 5-6 without an adversary (the user's):** G2 (the two-Presence group) in rounds 1-3, Presence from the card-play track (in round 3 the second may come from energy), then G1 (Reclaim All) every round. Never G3. Round 1: River's Bounty to gather Dahan into the City's land (needs your Presence there), Wash Away to push Explorers out of Build lands toward the coast. Rounds 2-3: Flash Floods (Fast) snipes any Explorer about to Build; reclaim the card you need. From round 4: Wash Away consolidates, Massive Flooding tier 3 sweeps. Win at Terror Level 1 (no Invaders). Won on two seeds.
- **Wash Away's target depends on the round:** sometimes it clears an Explorer that would cause next round's Build, sometimes the Town/Explorer that was just built (e.g. before it ravages a land where you'd lose Presence to blight). Check the prediction and pick.
- **Chain pushes:** resolve Massive Flooding first to push an Explorer into the land you will target with Wash Away, then Wash Away pushes it again: the same Explorer moves two lands.
- Flash Floods does extra damage on a Coastal land; River's Bounty needs your Presence in its land (range 0).
- Little Fast damage and no Defend in the starting hand: Fast defence comes from drawn cards.

### Lightning's Swift Strike

- **Special rule (Swiftness of Lightning):** one Slow power may resolve as Fast per air element gained this turn. The resolve prompt shows the allowance: `Select Fast to resolve (Slow as Fast: 1 of 2 left)`. Slow powers left over still resolve in the Slow phase (after the Invader Phase). Spend the allowances on the Slow powers that must act before the ravage.
- **Its four unique cards give 4 fire and 4 air**: all four played (6 energy, 4 plays) meet Thundering Destruction's "destroy a City" tier, and the 4 air make all of them Fast.
- Its main tool is a Slow card with air that destroys a Town (range 2 from a sacred site), made Fast each round; take Reclaim All most rounds to get it back.
- Won at Terror Level 2: earn fear by destroying Towns, then line up the last City kill (a sacred site next to it a round ahead) on the turn its Fear completes the third fear card.
