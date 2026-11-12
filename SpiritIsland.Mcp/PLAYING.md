# Playing Spirit Island through the MCP tools

Notes for an agent playing via `create_game` / `get_observation` / `get_decision` / `select_action` / `get_card_details`. Read `docs/rules.md` first and don't play from memory.

**These notes were written while playing only Lightning's Swift Strike** on board A with no adversary (two losses: round 4, a blight cascade; round 10, Blighted Island). So the file has two parts:

- **Part 1 applies to every spirit:** the rules, the tools and general lessons.
- **Part 2 applies only to River Surges in Sunlight** (one lost game; first impressions).
- **Part 3 applies only to Lightning's Swift Strike.** It includes its special rule, its cards and what worked for it (including a win).

When you play a different spirit, don't carry a spirit's part over. Read that spirit's `specialRules`, `innatePowers[].tiers` and growth options from the observation, and add a new section for it here once you've learned what applies. Anything in Part 1 that I only saw with Lightning is marked "(seen with Lightning)".

---

# Part 1: All spirits

## Keep a game log of growth and Presence track choices

Which growth to take and which Presence track to focus on (energy vs card plays) is a key part of playing a spirit well, and the answer needs data. For every game, add a section to [GAMELOG.md](GAMELOG.md) with, for each round, the growth group chosen and the track each Presence came from (energy slot, card-play slot, or a power's reclaim slot), plus the income and card plays at the time and what the round achieved. Update the summary table and the "what the data suggests" notes. Don't draw conclusions from one game; compare spirits and seeds first.

## Play only from what the tools show

Don't open the game's source code to find out what a card, fear card or blight card does while you are playing. A player only knows what's on the table: your own cards, revealed Invader cards, and what the alerts report when a fear or blight card resolves. Reading `NervesFray.cs` or the fear card list during game 2 and the River game was using information a player wouldn't have. If the tools don't show something you need, treat it as a gap to report, not something to look up in the code. (Reading source is fine when you're building or fixing the tools themselves.)

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
- `fear`: `terrorLevel`, `victoryCondition` for that level, `fearPool` / `fearPoolMax`, `facedownCardsRemaining`, `earnedFearCardsPending` (how many earned fear cards are waiting face down; their names are hidden until the next Invader Phase flips them). Each card's name and its effect at the current Terror Level arrive in the decision's `alerts` when it resolves.
- `blight`: `cardFlipped`, `blightRemainingOnCard` and `blightCardsRemaining` are always shown. The card's `card` name and `blightedIslandEffect` (what happens when it flips) are **hidden until the card flips** (the card is set up without looking at its back), so before then both are null. The other side's blight count isn't visible until it flips either. Plan for an unknown effect: when `blightRemainingOnCard` gets low, assume something bad happens at 0.
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
- Range for powers **without** a sacred-site origin (for example "range 1") is measured from **any** of your Presence. A land more than that many lands from all your Presence can't be targeted. Check this against where the dangerous lands are before relying on a power (seen with River: Flash Floods couldn't reach A3).
- **The Blighted Island card changes the blight rules once it flips**, and which card you have is hidden until then. Examples seen: All Things Weaken (Invaders -1 Health, lands blight on 1 less damage than usual, and adding blight destroys all Presence and beasts in that land plus 1 Presence in an adjacent land), Disintegrating Ecosystem (a beast is destroyed and blight is added each time), Power Corrodes the Spirit (a Presence is lost if you have 3+ cards in play or one costing 4+). After it flips, check the alert for its effect and what it does to ravage thresholds before planning the next round.
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
- **Destroyed Presence alert (added, not yet seen in a live game).** In the River game blight destroyed my Presence at A7 and nothing was reported. The engine now logs a `PresenceDestroyed` entry (kind `bad`), e.g. `River Surges in Sunlight: 1 Presence destroyed from A7.` It also covers Presence removed or replaced by an effect. Until it's confirmed live, still check `presence.destroyed` and your Presence tokens after any blight.
- Whether "Place Presence" growth can be taken together with another group in one round (for Lightning the three groups looked mutually exclusive).

---

# Part 2: River Surges in Sunlight only

Seen in one game (board A, seed 1, no adversary), which I stopped in round 5 when it was lost: the blight card had 1 left and two lands I couldn't protect were about to ravage. Treat this as first impressions, not a tested plan.

## How to play River (a "solved" spirit: one growth pattern beats all the others)

The user says River is *solved*: one growth **pattern** is better than every alternative, and once it's found you use it in every game and never anything else. The pattern is a **sequence of growth choices across the rounds of a game** (with the track each Presence comes from), and it is the same in every game. It is **not** one group repeated every round. I haven't found it yet. My games varied the growth from round to round without any plan, which can't show which sequence is best. Compare whole sequences on the same seed and log them (see GAMELOG.md). (Earlier versions of this note called River a "slow" spirit and then a "pure group every round" spirit; both were my misreadings of the hint.)

**A growth sequence that won (game 6 in GAMELOG.md, seed 2, round 8):** Presence, Reclaim, Presence, Reclaim, Presence, Reclaim, Reclaim, Reclaim. Take the two-Presence group early, putting almost every Presence on the **card-play track** (one on the energy track first for income 2), until 4 plays and the "reclaim 1" slot are uncovered, and take Reclaim All in between so the unique cards are back in hand. This isn't proven to be *the* best sequence: only one seed was tried, and I don't know whether a different order or more energy would do better.

**Confirmed in play:** Boon of Vigor + Flash Floods + Sunset's Fire (a drawn minor with sun and water) + Wash Away gave 3 sun, 4 water, 1 earth, and Massive Flooding **tier 3 killed a City and a Town in one land** (with 1 damage from Flash Floods first). River's own cards did most of the work: Flash Floods is 2 damage on a Coastal land and is Fast, so it can kill the last Town **before the Invader Phase builds**; River's Bounty gives +1 Dahan and +1 Energy each time it has 2 Dahan in a land; Massive Flooding is range 1 from a *sacred site* (any Wetland with Presence), so A2 (a Wetland) is only reachable if I have Presence there.

The following was my earlier inference from the observation's element numbers:

- River's four unique cards (Boon of Vigor, Flash Floods, River's Bounty, Wash Away) add up to **3 sun, 4 water, 1 earth**, which is exactly Massive Flooding's top tier ("2 damage to each Invader"). Playing all four costs only $3 (Boon 0, Flash Floods 2, River's Bounty 0, Wash Away 1) but needs **4 card plays**, so the plan is to push the **card-play track** (River has to uncover its slots: 1, 2, 2, 3, reclaim 1, 4, 5), then play the full kit each turn and keep the unique cards in hand (don't forget River's Bounty or any other unique card).
- Massive Flooding is Slow and has range 1 from a sacred site, so it lands after the ravage. Sacred sites matter: every Wetland counts, plus any land with 2 Presence. Use the Fast cards (Flash Floods, Boon of Vigor) and Defend cards for the ravage, and Massive Flooding for the cleanup.
- Understand a spirit through **its own unique cards and innate**, not through whatever minors happen to be drawn. In games 4 and 5 I leaned on drawn minors for defence and fear, and learned little about River itself.

## What differs from Lightning

- **Special rule "Rivers Domain":** "Your presense in wetlands count as sacred." `sacredSites` reflects it: A5 (a Wetland) is listed with only 1 Presence. This game's wetlands were A2 and A5.
- **No Slow-as-Fast.** The Fast prompt is the plain `Select Fast to resolve`, and Slow powers really are Slow (they resolve after the Invader Phase). Only Fast cards can stop a ravage.
- **Start:** 1 Presence (in A5), 1 Energy per turn, 1 card play per turn. The tracks grow to 5 plays and 5 energy, with a "reclaim 1" slot on the card play track. Two Presence placements (the "Place Presence(1)" x 2 group) uncover a slot on each track.
- **Growth groups:** (1) Reclaim All + Gain Power Card + Gain 1 Energy; (2) Place Presence(1) twice (range 1 each, so options are labelled `... [2]`); (3) Gain Power Card + Place Presence(2). Presence tokens are labelled `RSiS` in `spaces` (Lightning's are `LSS`), so a spirit's tokens are named by its initials.
- **Starting hand:** Boon of Vigor (Fast, free, +1 Energy if you target yourself), Flash Floods ($2 Fast, 1 damage, +1 if the land is Coastal), River's Bounty (Slow, gather Dahan), Wash Away ($1 Slow, push up to 3 Explorers/Towns).
- **Innate Massive Flooding** (Slow, range 1 from a sacred site; tiers use sun, water, earth): 1 sun 2 water pushes 1 Explorer/Town; 2 sun 3 water does 2 damage and pushes up to 3; 3 sun 4 water 1 earth does 2 damage to each Invader. Boon plus Flash Floods give 2 sun and 2 water, so tier 1 is easy, tier 2 needs a third water.

## Second River game (board A, seed 2): defence worked, offence didn't

Stopped in round 9 with no way to win in time: no blight lost beyond the card's flip (the first game lost in round 5), Terror Level 2 reached in round 6 and 5 fear cards earned, but the island held 3 Cities and 7 Towns and River couldn't clear them before the Invader deck ran out.

- **Defend is River's survival tool, and it came from drawn cards.** Haunted by Primal Memories ($1 Fast, range 2 from a sacred site, Defend 3, 1 Fear) cancelled ravage damage from a land with up to 3 damage and often let the Dahan there kill the Town afterward. **Indomitable Claim** (a major, $4 Fast, range 1, **Defend 20**, and it adds a Presence, so it can take the next track slot, e.g. the "reclaim 1" slot) fully protected a land with a City. Each round, I pair a Defend with each ravaged land that already has blight or that would otherwise blight.
- **After a Defend card is played, it's in the discard.** Reclaim All is needed the next round to play it again; once I skipped it (took a major instead) and had no Defend card for the next ravage. The "reclaim 1" slot (uncovered by Indomitable Claim here) returns one played card at will.
- **Energy piles up (10-12) while card plays stay at 2-3.** Use growth for the extra Presence slots that raise card plays, and spend the surplus on a major.
- **Fear came from drawn minors again** (Portents of Disaster, Terror Turns to Madness, Haunted's 1 Fear), plus Town kills by Dahan. That got me 5 fear cards by round 9, so the win has to come from the Terror Level: at Level 3 (6 cards) only Cities need to die. Plan the City kills from the start, not at the end.
- **River has little damage.** Cities (3 health) were out of reach. Massive Flooding tier 2 (2 sun 3 water) does 2 damage; Flash Floods does 2 only in a Coastal land; Pact of the Joined Hunt (gathers Dahan, 1 damage per Dahan in the land) was drawn too late. To kill Cities, aim for those damage sources and 3 plays a turn well before round 8.
- **The island fills fast.** Builds added a Town or City nearly every land each round; by round 9 there were 10 Towns/Cities. Kill or push Towns early (Prowling Panthers with a Beast, Scour the Land, Wash Away) so they don't need clearing at the end.
- Fear card effects seen this game (names arrive in alerts): Supply Chains Abandoned, Too Many Monsters (remove an Explorer/Town from a land with a Beast), Distracted by Local Troubles (Terror Level 2: 1 damage to up to 2 Invaders in a ravage land), Dahan Raid (1 damage per Dahan in a land: it killed a Town where 2 Dahan stood), Emigration Accelerates (remove an Explorer/Town from a Coastal land).
- Blighted Island card this time was Disintegrating Ecosystem: destroys a Beast and adds blight to a land with a Town/City, once, when it flips. After the flip the card held 4.

## What I learned (first game)

- **Range for non-sacred-site powers is measured from any Presence.** Flash Floods (range 1) couldn't reach A3, because none of my Presence was within one land of it. Plan Presence placement around where the ravages will land.
- River has almost no Fast damage and no Defend in the starting hand, so it can't stop big ravages. Ravage-proofing has to come from drawn cards: Entrancing Apparitions (Fast, Defend 2), Confounding Mists (Fast, Defend 4), Isolate/Disease effects, or pushing Invaders away before the ravage.
- **Fear came from drawn minors, not the starting hand:** Tormenting Rotflies ($1, Slow, range 2, Sands/Wetland: 2 Fear, +1 with Blight, +1 with Disease) and Sucking Ooze ($0, Fast, 2 Fear if Invaders are present, and Isolate). With those I earned 3 fear cards by round 4 and reached Terror Level 2.
- Gift of Constancy again gave +2 Energy and a free reclaim, which matters more for River's low energy.
- **The blight went too fast:** 3 blights by round 3, and the card flipped in round 4 to All Things Weaken (Invaders -1 Health; lands blight on 1 less damage; adding blight destroys all Presence and beasts in that land and 1 Presence in an adjacent land). With 1 blight left on that side, any further blight loses the game. Deal with the lands that will ravage before the blight card is low.
- Fear card effects that matter (alerts name them): Supply Chains Abandoned (isolate a land), Seek Company (gather an Explorer into a land with 2+ Invaders; skip the move if it would add to a ravaging land), Nerves Fray (Strife in a land that does not match a Ravage card, so it can't protect a ravage land).

# Part 3: Lightning's Swift Strike only

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
