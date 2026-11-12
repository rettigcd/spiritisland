# Spirit Island Rules (condensed from SpiritIsland_RulesReference_v1.2.pdf)

Read this before playing. Page references `[pN]` are to the PDF. Where the game implementation differs from or adds to the PDF, see `SpiritIsland.Mcp/PLAYING.md`.

## Sequence of play [p8]

Each turn:

1. **Spirit Phase**: (1) Growth, (2) Gain Energy, (3) Play and Pay for Power Cards
2. **Fast Power Phase**: Fast cards and Innate Powers resolve. Fast powers cannot be delayed to a later phase.
3. **Invader Phase**:
   1. Blighted Island effect (once appropriate)
   2. **Fear Effects**: earned Fear Cards resolve now
   3. Invader Actions: **3a Ravage, 3b Build, 3c Explore**
   4. Advance Invader Cards (slide all cards left one space)
4. **Slow Power Phase**: Slow cards and Innate Powers resolve (AFTER the Invader Phase)
5. **Time Passes**: discard played cards, damage and elements clear, reminder tokens removed

Play is simultaneous within a phase. A player may skip a Power's text effect; Elements are still gained.

## Spirit Phase [p8, p13]

**Growth**: choose **1** option from those next to "Growth" on the Spirit Panel. Each option is a single choice; you must do everything shown in it, in any order. Common options:

- Add 1 Presence at Range 2 (up to 2 lands from existing Presence). When adding Presence you may move 1 already on the board instead.
- Gain 2 Energy (in addition to normal income).
- Gain a Power Card.
- Reclaim all played Power Cards from the discard pile to hand.

**Gain Energy**: gain Energy equal to the highest uncovered number on the Energy Presence Track. Energy is individual, carries over between turns.

**Play and Pay for Power Cards**: choose cards from hand to play. Maximum = highest uncovered number on the Card Plays Track (even if you could afford more). Pay Energy immediately. You gain the card's Elements immediately (Fast or Slow). Effects are not resolved yet. Unused Card Plays do not carry over.

### Presence tracks

Top track = Energy per turn, bottom track = Card Plays. All but the leftmost space start covered. When placing Presence on the island you choose which track to take it from, always left to right. Only the highest revealed number counts (not additive). Some tracks show bonus Elements that are always on while uncovered. "Reclaim One" spaces let you return 1 card to hand any time in the Spirit Phase.

### Gaining Power Cards [p14]

Choose Minor or Major. Draw 4, keep 1, discard the rest. After gaining a **Major** you must **Forget** a Power Card (permanently lose one from hand, discard, or play; unique cards go under the Spirit Panel). Forgetting a card in play loses its Elements immediately.

## Powers [p16-19]

- **Cost**: circle at top left. Red circle = Fast, blue = Slow.
- **Speed**: Fast resolves before the Invader Phase, Slow after.
- **Range**: max number of lands from your Presence. Can always use shorter. Range 0 = a land where you have Presence. Some powers require the range be measured from a **Sacred Site** (land with 2 or more of your Presence) or a terrain type.
- **Target**: land type (or Spirit). "Any" targets any land. A Power targets a single land unless stated.
- **Elements**: Sun, Moon, Fire, Air, Water, Earth, Plant, Animal. Gained when you pay for a card, regardless of speed. Cleared in Time Passes. Never spent, only checked.
- **Thresholds**: optional extra effects usable only on turns you have gained all the required Elements. You may resolve as if you had fewer Elements. If several thresholds are met, do each from top to bottom.
- **Innate Powers**: on the Spirit Panel. Free, no Card Play. Require Elements. Only trigger once per turn.
- **Repeat**: use a Power's text effect again. No extra Elements. Repeats cannot chain.
- One land, one turn, one use: a Power affects 1 target land, only this turn, and can only be used once per turn. Permanent changes are represented by board changes. If something makes a Slow Power Fast, it may only be used in the Fast phase.
- Do as much as you can; skip parts that can't be done.

### Damage, Destruction, Removal [p18]

- "Damage" means Damage to Invaders, divided as you choose. Damage >= Health destroys it.
- Destroying a Town generates 1 Fear, a City 2 Fear. Removing or replacing generates no Fear.
- Replace: remove, then place the other piece (keeps damage).
- **Defend N**: reduce total Damage done by Invaders to the land and Dahan by N during Ravage. Stacks. Lasts the turn.
- **Gather**: move things *into* the target land from adjacent lands. **Push**: move things *out of* the target land to adjacent lands (1 land away only, not into the Ocean).

## Invader Phase [p9-11]

Invader Cards on the three action spaces decide which terrain is affected. No card in a space = no action.

### Ravage (cards in the Ravage space)

In each land of the shown terrain: Explorers deal 1, Towns 2, Cities 3 (reduced by Defend).

1. **Damage the land**: if 2+ damage is dealt, add 1 Blight (only 1 regardless of amount).
   - **Cascade**: if the land already had Blight, also add 1 Blight to 1 adjacent land; if that land has Blight, it cascades again.
   - **Destroy Presence**: adding Blight destroys 1 Presence of each Spirit in that land (and Cascade adds likewise).
2. **Invaders fight the Dahan**: every 2 damage destroys 1 Dahan, as efficiently as possible. 1 damage to a Dahan marks it damaged (heals at end of turn).
3. **Dahan fight back** (skipped in this land if the Ravage was skipped/stopped): each surviving Dahan deals 2 damage to Invaders, divided as you choose.

### Build (cards in the Build space)

In each land of the shown terrain *that has Invaders*: add 1 City or 1 Town. If the land has more Towns than Cities add a City, otherwise a Town.

### Explore (top card of the deck, placed in the Explore space)

In accessible lands of the shown terrain: add 1 Explorer if the land contains a Town or City, or is adjacent to a Town, City, or Ocean. Only 1 Explorer per land. If there is no card to turn up, **you lose**.

### Fear Effects [p12]

If any Fear Cards were earned, flip the whole stack and resolve them one at a time in the order earned, using only the effect for the **current Terror Level**. Effects last the current turn. Then discard. New Fear Cards earned during this are placed at the bottom of the stack.

## Invaders [p15]

| Piece | Health | Damage in Ravage | Fear if destroyed |
|---|---|---|---|
| Explorer | 1 | 1 | 0 |
| Town | 2 | 2 | 1 |
| City | 3 | 3 | 2 |

Towns and Cities act as an Invader source during Explore. Damage clears at Time Passes.

## Dahan [p16]

Start with 6 per board. 2 Health, deal 2 damage. They only attack Invaders when a Spirit Power prompts it, or when attacked (fight back after Ravage). Spirit Damage does not hurt Dahan.

## Blight [p15]

When Blight is added after Setup (including by Cascade) it takes from the Blight Card. It Cascades and Destroys Presence (see Ravage). If you remove Blight, return it to the card. If the Blight Card runs out of Blight, follow its instructions; the **Blighted Island** side flips once and never back. If the last Blight leaves the Blighted side, **you lose**.

## Fear and Terror Level [p12]

Each Fear generated moves a marker from the Fear Pool (4 per player) to Generated Fear. When all have advanced: move the top Fear Card face-down to Earned Fear Cards, return markers to the pool (leftover Fear stays). Revealing a Terror Level divider moves to the next Terror Level immediately.

Fear Deck is 9 cards in three groups of 3: top = Terror Level 1, middle = 2, bottom = 3. Earning the last card ends the deck (Terror Level 4, a win in this implementation).

## Victory and defeat [p12]

You win immediately when the current Terror Level's victory condition is met:

| Terror Level | Victory condition |
|---|---|
| 1 | no Invaders on the island |
| 2 | no Towns or Cities |
| 3 | no Cities |
| 4 | victory |

You lose if: the Blight Card's last Blight is used, any Spirit has no Presence left, or you must draw an Invader Card to Explore and the deck is empty.

## Board [p13]

8 numbered lands per board, 2 each of Jungle, Mountain, Sands, Wetland. Adjacent = share a border or corner. Lands touching the Ocean are Coastal, others Inland. The Ocean is not a land. "Your land" = a land with your Presence.

## Presence [p13-14]

Destroyed Presence is removed from play (not returned to the panel). If a Spirit has no Presence on the island, players lose. Sacred Site = 2 or more Presence in a land.

## Tracking damage [p15]

City 3 health, Town 2, Dahan 2. Partially damaged pieces lie on their sides; all reset in Time Passes.

## Adversaries and scenarios (optional) [p22-23]

Adversaries add escalation effects (Stage II cards with an escalation icon), extra loss conditions and per-level effects (cumulative). The Adversary panel lists them. Scenarios change victory conditions or rules and win conflicts with adversary rules.

## Scoring (optional) [p23]

Victory: 5x Difficulty, +10 for winning, +2 per Invader Card left in the deck, +1 per (Dahan / players), -1 per (Blight / players). Defeat: 2x Difficulty, +1 per Invader Card not in the deck, +1 per (Dahan / players), -1 per (Blight / players).

## Glossary highlights

- **Up to** may be zero. **Outnumber** is true where there is no B and at least one A.
- **Land with X**: has at least one X.
- **Remove** is not **Destroy** (no Fear).
- **Isolate** is not defined in the PDF (see PLAYING.md).
