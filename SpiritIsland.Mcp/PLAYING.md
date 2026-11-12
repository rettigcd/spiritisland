# Playing Spirit Island through the MCP tools

Notes for an agent playing via `create_game` / `get_observation` / `get_decision` / `select_action` / `get_card_details`. Read `docs/rules.md` first and don't play from memory.

**These notes were written while playing only Lightning's Swift Strike** on board A with no adversary (two losses: round 4, a blight cascade; round 10, Blighted Island). So the file has two parts:

- **Part 1 applies to every spirit:** the rules, the tools and general lessons.
- **Part 2 applies only to Lightning's Swift Strike.** It includes its special rule, its cards and what worked for it.

When you play a different spirit, don't carry Part 2 over. Read that spirit's `specialRules`, `innatePowers[].tiers` and growth options from the observation, and add a new section for it here once you've learned what applies. Anything in Part 1 that I only saw with Lightning is marked "(seen with Lightning)".

---

# Part 1: All spirits

## Before every decision

1. Call `get_observation`. Do it every round, and after anything unexpected. In the first game it was called twice and the game was lost by round 4.
2. Read the **Invader cards** in order: the card in `ravage` hits this turn's Invader Phase, `build` next, `explore` after. `matchedLands` lists the lands each card's terrain matches.
3. Work out which lands will take blight (2+ damage; Explorer 1, Town 2, City 3) and which already have blight (cascade, so two blights from one ravage). Nothing predicts this for you: read the tokens and apply the rules. Mods (strife, Defend, Isolate) can change the outcome.
4. Check `blightRemainingOnCard`. If a ravage can take it to 0 you lose, so that ravage must be stopped before the Invader Phase.
5. Read the `alerts` on the decision, which say what happened since your last choice.
6. Read your spirit's `specialRules` and the `tiers` of its `innatePowers`. They differ per spirit.
7. If a prompt isn't understood, stop and ask the user instead of guessing.

## What the tools return

**Decision** (`create_game`, `get_decision`, `select_action`): `version`, `prompt`, `options`, plus
- `alerts`: `{kind, text}` entries for what happened since the previous decision (your own choices are left out). Kinds follow the Maui log colors: `bad`, `invaders`, `rewind`, `phase`, `fear-blight`, `tokens`, `other`. This is where "N Fear generated", ravage/build/explore results, and a revealed fear card with its effect text show up. Repeating `get_decision` returns the same alerts.
- `optionDetails`: for card choices only (drawing a card, forgetting one): the card's cost, speed, elements, range, target and instructions, plus `use` (e.g. `AddToHand`). Not attached when choosing which hand card to play; use the observation's `hand` or `get_card_details`.
- `promptDetails`: spells out the `Play power card ($E / N)` prompt as `Energy: E. Card plays left: N.` (matched by regex in `PromptDetails.cs`; null for other prompts).

**Observation** (`get_observation`):
- `fear`: `terrorLevel`, `victoryCondition` for that level, `fearPool` / `fearPoolMax`, `facedownCardsRemaining`, `revealedCards` (the card's name is listed as soon as it's earned, even before it resolves in the Invader Phase; its effect text arrives in the alerts when it resolves).
- `blight`: `blightedIslandEffect` is what happens when the card flips (it's the same text before and after the flip), `blightRemainingOnCard`. The other side's blight count isn't visible until the card flips.
- `invaders`: the three slots, `discards`, `unrevealedCount` (reveals left; running out loses, see `docs/rules.md`), `matchedLands` per action (lands matching the card's terrain; an adversary can change what an action really does).
- `spirit`: `specialRules` (title and description), `elements` (gained so far this turn; empty until cards are paid), `presence` (tracks, `sacredSites`), `growth`, `hand`, `inPlay`, `discard` (name, cost, speed), and `innatePowers` with `tiers` (`threshold`, `effect`, `elementStatus`).
- `spaces`: terrain, adjacency, tokens. Defend and Isolate show as tokens with a count (e.g. `"Defend":2`, `"Isolate":1`; seen live while Roiling Bog and Snagging Thorn's effects were active, and gone by the next round). Strife should show as `^` after an Invader's text (from the code; not yet seen live).

**`get_card_details(name)`**: full details for a card in hand, in play, in the discard pile, or offered by the pending decision. Not for the power decks. Accepts the plain name or option text such as `Shatter Homesteads $2 (Slow)`.

All of these were confirmed in a live game: `alerts`, `optionDetails`, `promptDetails`, `matchedLands`, `sacredSites`, `discard`, `blightedIslandEffect`, `victoryCondition`, `specialRules`, innate `tiers`, `get_card_details`.

## Tool mechanics

- `select_action` needs the current `version` and the exact option text.
- **Growth**: each round you pick ONE growth group, then do everything in it (follow-up prompts such as "Place Presence(0)" or "Gain Power Card" are part of the same group). The prompt that follows "Done" on the card-play step is the *next round's* growth, not a loop. The group contents are per spirit; the `growth` field in the observation lists them.
- **Card play**: the prompt `Play power card ($E / N)` shows energy and remaining card plays (`promptDetails` spells it out). Select cards one by one, then "Done". Then "Select Fast to resolve" lets you choose the resolve order.
- Prompts appear for **fear card effects** (for example "Select space to Isolate target land", "Select space to Gather up to 1 Explorers", "Stop Invaders Ravage this Turn", "Remove up to 3 worth of Invaders", "Select space to Push 1 explorers or towns"). The prompt doesn't name the card, but the card's name and its effect at the current Terror Level arrive in the same decision's `alerts` (kind `fear-blight`, e.g. `Flee from Dangerous Lands : 1 : On Each Board: Push 1 Explorer/Town from a land with Badlands/Wilds/Dahan.`). They come at the start of the Invader Phase for cards earned during the Fast phase (or the previous turn).
- **Isolate** (fear card): the target land is disconnected from adjacent lands for exploring, so it gets no Explorer from adjacent Towns/Cities/Ocean.
- Blighted Island card (Disintegrating Ecosystem): after it flips, prompts "Destroy 1 Beast" then "Add 1 blight to a land with town/city" appear. Choose a land that is not going to cascade onto your presence.
- A major card draw forces a "forget a card" prompt; minor does not.

## Rules learned or confirmed

- Slow powers resolve **after** the Invader Phase, so only Fast powers stop a ravage. Some spirits can make Slow powers Fast (see their special rules).
- **Strife**: a Strifed Invader deals **no damage** in Ravage. Terror Turns to Madness adds Strife at Terror Level 2 (choice with 2 Fear) and Level 3. This was the best defence against a ravage with many Invaders.
- **Defend N** (Nature's Resilience: Defend 6, Fast) reduces total ravage damage. It stops blight when the total damage is N+1 or less.
- Blight Cascade: if a land already has blight, adding blight adds another to an adjacent land (you choose). Also destroys 1 presence per spirit in each land. Choose cascade targets with no blight and no presence.
- Fear: pool of 4 per player. A destroyed Town is 1 Fear, a City 2. Portents of Disaster gives 2 plus 1 more when an Invader is destroyed in its target land that turn, so resolve it **before** the destroying power.
- Terror Level 3 wins when there are **no Cities**. Terror Level 4 (all 9 fear cards earned) wins outright. Earning the whole bottom stack in one turn is possible but needs the exact Fear count.
- The Invader deck runs out: with Stage I-III (3+4+5 cards less setup) the game ends in a loss around round 11. Plan to win before the Explore that has no card.
- Range: "1:ss" means range 1 *starting from* a **Sacred Site** (2+ Presence in a land; see `presence.sacredSites`). With no sacred site such a power has no origin and so no targets at all, so build sacred sites early and check `sacredSites` before paying for one.
- Song of Sanctity pushes Explorers whenever the target land has any; it only removes blight if the land has no Explorers.
- Minor powers that helped defend (seen with Lightning, but they aren't spirit specific): Terror Turns to Madness (strife), Nature's Resilience (Defend 6), Renewing Rain (removes blight in Jungle/Sands), Portents of Disaster and Visions of Fiery Doom (Fear).

## Strategy (general)

- Each round, pair a Fast defensive effect (strife, Defend, destroying a Town) with each ravaged land that already has blight.
- Destroy a Town in a ravaged land to leave only an Explorer (1 damage = no blight).
- Draw minor cards when gaining a power; a major forces a forget.
- Earn fear aggressively: at Terror Level 2-3 the fear cards remove Invaders and can stop a ravage outright.
- **Plan the win around the Terror Level, not "clear the island".** Terror Level 1 needs every Invader gone, which is hard. Level 2 only needs no Towns or Cities, and Level 3 only no Cities. Earning 3 fear cards (12 Fear) reaches Level 2 and 6 cards reaches Level 3. Each round, count the Towns and Cities left and how many fear cards you've earned (`facedownCardsRemaining` shows 9 minus earned). If you're one fear card short of Level 2 and only Cities remain, line up a way to destroy the last City on a turn where that destruction's own Fear completes the card; the Terror Level changes immediately and the win is checked at once. Won game 3 in round 5 this way.
- Make sure a power can actually reach the last City before you need it: for "range 1 from a sacred site" powers, put 2 Presence on a land next to it a round ahead.

## Mistakes to avoid

- Not checking the observation between rounds (game 1).
- Playing slow powers after the ravage they should have prevented.
- Wasting turns when blight card remaining is low. Clear blight early with Renewing Rain / Song of Sanctity (lands with no Explorers) or Nature's Resilience with 2 water.
- Presence placement: put Presence where it extends range to the lands that will ravage. Don't leave a land (A2 in game 2) beyond range 2 of any sacred site while it accumulates a City and Towns.

## Open questions (all spirits)

- Whether strife shows as `^` on an Invader's token text (Defend and Isolate were confirmed as tokens).
- Whether "Place Presence" growth can be taken together with another group in one round (for Lightning the three groups looked mutually exclusive).

---

# Part 2: Lightning's Swift Strike only

Nothing in this part is known to apply to any other spirit.

## Special rule: Swiftness of Lightning

"For every Simple air you have, you may use 1 Slow Power as if it were fast." (The description is the game's own text.) In practice: 1 allowance per **air element gained this turn** (from the cards you play, including a card's own air).

- Count your allowance before choosing which Slow power to make Fast. The Lightning prompt shows it: `Select Fast to resolve (Slow as Fast: 1 of 2 left)`. This suffix is Lightning's own prompt, so other spirits won't show it.
- "Select Fast to resolve" lists *every* Slow power you have, even after the allowance is used up, and then they vanish from the list as allowances run out.
- A Slow power with no allowance left resolves in the **Slow phase**, after the Invader Phase. The Fast phase ends when no Fast powers (and no allowances) remain. Nothing is lost, but the Invader Phase happens before your Slow powers.
- In round 10 of game 2 the 2 allowances (Portents + Shatter air) were spent on Shatter Homesteads and Savage Mawbeasts, so Thundering Destruction waited for the Slow phase, and the Invader Phase blight ended the game before it. Spending one allowance on Thundering Destruction instead would have given the Fear for the last fear card. Decide which Slow powers matter before the Invader Phase and spend the allowances on those.
- Lightning's Boon grants 2 more Slow-as-Fast.

## Thundering Destruction (innate)

Tiers, also in `innatePowers[].tiers` (check `elementStatus` rather than counting by hand): 3 fire 2 air: destroy a Town; 4 fire 3 air: or a City; 5 fire 4 air 1 water: +1 Town/City; 5 fire 5 air 2 water: +1 more. Range 1 from a sacred site.

## Growth (Lightning's panel)

Three groups, pick one per round: **Reclaim All + Gain Power Card + Gain 1 Energy**; **Place Presence(2) + Place Presence(0)**; **Gain 3 Energy + Place Presence(1)**. The presence groups also uncover card-play and energy slots, so the third group can give extra card plays that same turn (round 8 of game 2 used the Place Presence(2)/(0) group for 4 plays).

## Cards and what they did

- **Shatter Homesteads** ($2, Slow, range 2 from a sacred site): 1 Fear and destroy a Town, which is 2 Fear in total. It has an air element, so it's usually the Slow power you make Fast. It was the workhorse; it's only back in hand after Reclaim All.
- **Raging Storm** ($3, 1 damage to each Invader in a land, range 1) kills Explorers; **Harbingers of the Lightning** ($0, push Dahan, 1 Fear if pushed into a land with a Town/City); **Lightning's Boon** ($1, Fast).
- Cards that gave air or Fear for Lightning's rule and the Fear count: Portents of Disaster, Delusions of Danger, Visions of Fiery Doom (needs 2 fire for its extra Fear), Savage Mawbeasts (1 Fear in Jungle/Wetland).

## Strategy that worked for Lightning

- Take **Reclaim All** growth most rounds (the free new minor card is a chance for a good power; it found Terror Turns to Madness and Nature's Resilience).
- Pair a Fast Shatter Homesteads with a second Fast effect (Terror Turns to Madness for strife, or Nature's Resilience for Defend) so two ravaged lands are stopped in one turn.
- Keep a sacred site next to the lands that ravage, so Shatter Homesteads (range 2 from a sacred site) can reach them.
- **Gift of Constancy** (minor, $0, Fast, elements sun and earth): targeted at yourself it gives +2 Energy and lets you reclaim one played card at the end of the turn (reclaim Shatter Homesteads so the next round can take a presence growth instead of Reclaim All). It resolves without asking for a target when you're alone.
- **Thundering Destruction for a City:** four cards with fire and air (Lightning's Boon, Harbingers of the Lightning, Shatter Homesteads, Raging Storm: 4 fire, 4 air, 6 Energy, all 4 card plays) meet the "destroy 1 City" tier. The prompt then lists Thundering Destruction among the Fast choices, and the 4 air give 4 Slow-as-Fast allowances to cover Harbingers, Shatter, Storm and it.

### The game-3 win (seed 4, board A, round 5)

1. Round 1: Place Presence(2)/(0) from the energy track to get a second sacred site and income 2. Shatter Homesteads on a Town (Fast).
2. Rounds 2-3: Shatter Homesteads and Raging Storm each turn to stop ravages; free Thundering Destruction on a new Town in the Slow phase. Two fear cards earned.
3. Round 4: only A2's City was left (no Towns). Placed both Presence on A1 (next to A2) from the energy track: sacred site next to the City and income 3. Played nothing and banked energy.
4. Round 5: Reclaim All (7 Energy). Played the four cards above. Resolved Shatter Homesteads on the Town in A4 (2 Fear and the ravage there stopped), then Thundering Destruction on A2's City (2 Fear). That filled the pool, earned the third fear card, set the Terror Level to 2, and the game ended "Terror Level 2 - no towns or cities".
