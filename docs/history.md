# Spirit Island project history

Built from `git log` (1,553 commits, 2021-04-11 to 2026-11-18) plus first-add dates of files in each expansion project. Dates are commit dates. "Started / finished" for an expansion is taken from when its spirits, major/minor power cards, fear, blight and adversaries first appeared and when work on them stopped being mostly new-content.

## Timeline at a glance

| Era | Dates | What was going on |
|---|---|---|
| 1. Base game engine | 2021-04 to 2021-08 | Core engine, 8 base spirits, base cards |
| 2. Branch & Claw + Jagged Earth | 2021-09 to 2021-12 | New tokens, split into projects, 10 Jagged Earth spirits |
| 3. Hardening and adversaries | 2022-01 to 2023-03 | Adversaries, tokens/ActionScope rework, Feather & Flame (promo pack 2 and the project merge) |
| 4. Nature Incarnate | 2023-09 to 2024-01 | Incarna, 8 spirits, major/minor cards |
| 5. Maui and cleanup | 2024-01 to 2024-11 | .NET 8, Maui UI, issue-driven fixes |
| 6. Aspects and Horizons | 2024-11 to 2025-02 | Aspects, Horizons spirits, .NET 9 |
| 7. Dormant | 2025-03 to 2026-06 | Only a .NET 10 upgrade |
| 8. Serialization, MCP, Web | 2026-07 to 2026-11 | JSON save/rewind, MCP agent API, web client |

Commit volume peaks: 2021-08 (135), 2024-12 (105), 2023-11 (98), 2024-01 (93), 2026-11 (78 so far).

## Expansion modules

### Summary

Adversaries were added later than the rest of their expansion, so they have their own rows.

| Module | Started | Finished |
|---|---|---|
| Base Game (without adversaries) | 2021-04-11 | 2021-08-19 |
| Branch and Claw (without adversaries) | 2021-09-01 | 2021-09-16 |
| Feather and Flame, pack 1 (Heart, Serpent) | 2021-10-12 | 2021-10-13 |
| Jagged Earth (without adversaries) | 2021-10-15 | 2021-12-26 |
| Base Game adversaries (Brandenburg-Prussia, England, Sweden) | 2022-06-10 | 2022-06-14 |
| Branch and Claw adversaries (France) | 2022-06-18 | 2022-06-18 |
| Jagged Earth adversaries (Russia, Habsburg) | 2023-01-02 | 2023-01-07 |
| Feather and Flame, pack 2 (Downpour, Finder, Scotland) | 2022-06-25 | 2023-02 |
| Nature Incarnate | 2023-09-29 | 2023-12-05 |
| Horizons | 2024-12-27 | 2024-12-27 |

### Details

Each section covers spirits, major/minor power cards, fear, blight and adversaries.

### Base Game
- **Started:** 2021-04-11 (initial commit: 2 power cards and board spaces). First spirits (Thunderspeaker, River) 2021-04-21. Stub names for all spirits on 2021-04-22.
- **Spirits:** all 8 (River, Lightning, Vital Strength, Shadows, Thunderspeaker, Bringer, Ocean, Rampant Green) implemented between 2021-04 and 2021-08. Bringer finished 2021-08-19.
- **Power cards:** minor and major cards stubbed 2021-08-08 to 2021-08-14, then filled in. "Remainder of Minor Cards complete" 2021-08-14. Major cards had a big pass 2021-08-15 to 2021-08-19. Fear level 1 done 2021-08-03. Blight cards stubbed 2021-08-04.
- **Adversaries:** Brandenburg-Prussia, England, Sweden, all 2022-06-10 to 2022-06-14 (escalation and level-0 option 2022-06-14).
- **Finished:** core content 2021-08. Adversaries 2022-06. The `release_basegame` branch exists for this.

### Branch and Claw
- **Started:** 2021-09-01 (strife, disease, wilds tokens). Card art and stubs 2021-09-02. Split into its own project 2021-09-07.
- **Spirits:** Keeper and Sharp Fangs (Sharp Fangs reclaim finished 2021-05, but the Keeper/Fangs BAC rules landed with the expansion).
- **Power cards:** major cards stubbed and finished 2021-09-07 to 2021-09-10 ("BAC Major cards Cleanup"). Minor target-spirit cards 2021-09-07. Fear cards added to config 2021-09-13.
- **Adversaries:** France, 2022-06-18 (more rules in 2024-01 to 2024-01-26).
- **Finished:** content 2021-09-16 or so; adversary 2022-06-18.

### Jagged Earth
- **Started:** 2021-10-15 (Lure of the Deep Wilderness stubbed, project created).
- **Spirits:** 10 spirits, 2021-10-15 to 2021-12-10: Lure, Grinning Trickster, Stone, Shifting Memory, Many Minds, Volcano, Shroud, Vengeance, Starlight (2021-12-05), Fractured Days (2021-12-10). Starlight triggered the .NET 6 upgrade.
- **Power cards:** minor 2021-12-14, major 2021-12-15, fear 2021-12-23, blight cards 2021-12-25/26.
- **Adversaries:** Russia 2023-01-02, Habsburg Monarchy 2023-01-07 (a Habsburg Mining Expedition event card came with Nature Incarnate in 2024-01).
- **Finished:** cards and spirits 2021-12-26; adversaries 2023-01. Continued rule-accuracy fixes through 2023-02 (Badlands, Finder, etc).

### Feather and Flame (two pieces: Promo Pack 1 and Promo Pack 2)
Sold as two packs and built separately; both ended up in the `FeatherAndFlame` project (renamed from `PromoPack1` on 2023-01-21).

**Promo Pack 1: Heart of the Wildfire and Serpent Slumbering Beneath the Island**
- **Started:** 2021-10-12 (Heart growth fixes, Flame's Fury; Serpent scaffolding).
- **Finished:** 2021-10-13 for Serpent ("Innates and cards finished"). Heart's rules refined through 2022-03 (Flame's Fury damage pool). Built inside the Jagged Earth work, before the project was split out.

**Promo Pack 2: Finder of Paths Unseen, Downpour Drenches the World, and the Scotland adversary**
- **Started:** 2022-06-25 (UI components for Downpour). Downpour spirit 2022-12-17.
- **Finder of Paths Unseen:** 2022-11-27, setup on a chosen board 2023-02-07, tests 2024-01-21, cleanup 2024-12-11.
- **Scotland:** 2023-01-09, tests and loss condition 2024-11-09 to 2024-11-11.
- **Finished:** 2023-02 (core); extra fixes through 2024-01.

**Combined:** project renamed and consolidated 2023-01-21; aspects for Heart and Serpent 2024-12-22.

### Nature Incarnate
- **Started:** 2023-09-29 (Incarna token images, Towering Roots).
- **Spirits:** 8, 2023-09-29 to 2023-11-21: Towering Roots, Breath of Darkness (2023-10-09), Hearth (10-18), Wounded Waters (10-26), Dances (10-31), Sun (11-04), Ember-Eyed (11-17), Wandering Voice (11-21).
- **Power cards:** minor 2023-11-24, 10 major cards 2023-11-24 to 2023-11-28. Fear 2023-12-04. Blight 2023-12-05.
- **Adversaries / events:** Habsburg Mining Expedition 2024-01-01.
- **Finished:** content 2023-12-05; follow-up fixes (Towering Roots, Breath of Darkness) through 2024-06.

### Horizons
- **Started and finished:** 2024-12-27 ("All Horizons Spirits": Devouring Teeth, Eyes Watch, Fathomless Mud, Rising Heat, Sun Bright). Bug fix on Watching Eyes 2025-02-04. Boards G and H offered in web client 2026-11-17.
- There are no Horizons power cards, adversaries or blight cards in the code.

## Device and platform history

| Platform | Started | First playable | Last activity | Status |
|---|---|---|---|---|
| Command line (`SpiritIslandCmd`) | 2021-06-23 | 2021-06-25 | 2024-03-29 | Retired; replaced by WinForms within a week |
| WinForms | 2021-06-30 | 2021-06-30 (text buttons), board view same day | 2026-07-21 | Primary UI for 5 years; still builds (net10.0) |
| Maui | 2024-03-11 | 2026-07-23 (Windows build running) | 2026-11-12 | Present; Tizen dropped 2024-11-17 |
| MCP server (`SpiritIsland.Mcp`) | 2026-11-11 | 2026-11-12 | 2026-11-17 | Active |
| Web (`SpiritIsland.Web`) | 2026-11-15 | 2026-11-16 (client, Phase 3) | 2026-11-17 | Active; LAN and WAN access added 2026-11-17 |

| Runtime | Date |
|---|---|
| .NET 5 | 2021-06-28 |
| .NET 6 | 2021-12-05 |
| .NET 8 | 2024-01-22 |
| .NET 9 | 2024-12-05 |
| .NET 10 | 2025-12-12 |

## Major refactors / breakthroughs

| # | Date | Breakthrough | What changed |
|---|---|---|---|
| 1 | 2021-04 to 2021-06 | From options to actions | Presence placement refactored; Board/Island/Space renames (2021-04-26); growth split into factory + action (2021-06-27) |
| 2 | 2021-06-30 | Engine separated from UI | WinForms and CLI share one game via `IPhases` / `GamePlayer` |
| 3 | 2021-07-08 to 2021-07-11 | Async everything | Cards and actions made async so the UI can prompt mid-action |
| 4 | 2021-07-30 to 2021-09-07 | Project split | `Basegame` namespace, then BranchAndClaw and Basegame in separate projects |
| 5 | 2021-08-27 to 2021-09-01 | Tokens unified | Dahan, fear, beasts, wilds pulled out of `GameState` into one `CountDictionary<Token>` |
| 6 | 2021-10-06 | Memento rewind | Go back to start of round; extended 2022-03, 2022-08, 2023-02 |
| 7 | 2021-12-17 to 2021-12-22 | Token events and Ravage engine | Token-change events; Attackers/Defenders |
| 8 | 2022-04 to 2022-08 | HealthTokens | Ravage and damage made health-level agnostic |
| 9 | 2022-11 to 2023-02 | Space / token / ActionScope rewrite (largest) | `SpaceState`; `UnitOfWork` became `ActionScope` (2023-02-10); token classes renamed (SpaceEntities vs visible ITokens); GameState mostly ambient; island saved to memento |
| 10 | 2023-01-05 to 2023-01-09 | Invader engines | Explore / Build / Ravage split into overridable engines (for adversaries) |
| 11 | 2023-10 to 2023-12 | Incarna, targeting, Cmd renames | Incarna folded into `SpiritPresence` (2023-12-02); `SelfCtx` removed (2023-12-01); A.* selection API |
| 12 | 2024-01 | Issue-driven refactor sprint | GitHub issues #2 to #60: mementos completed, token replace/adjust APIs, presence track reveal, button refactor |
| 13 | 2024-02-15 to 2024-03-11 | Engine decoupled from drawing | `System.Drawing` removed from engine; `Space` became `SpaceSpec`, `SpaceState` became `Space`; Maui created |
| 14 | 2024-11 to 2024-12 | Aspects and API cleanup | Mutable special rules and presence (2024-11-29); Adversary/AdversaryBuilder split; TargetRoutes; nullable enabled; Aspects for most spirits |
| 15 | 2026-07-14 to 2026-07-21 | JSON serialization | JSON save (07-20); growth phase step-wise; `DoGrowth` moved to `SoloGame`; rewind is one action |
| 16 | 2026-11-11 to 2026-11-18 | Agent and web era | MCP server; every-spirit playthrough fixes (11-16); web server and client; action + target as one choice (11-17) |

## Quiet periods

- 2022-01 to 2022-05 and 2022-09: low activity.
- 2023-04 to 2023-08: no commits.
- 2024-07 to 2024-10: near-silent.
- 2025-03 to 2026-06: dormant apart from the .NET 10 upgrade (2025-12-12).

## Caveats

- Many files show 2024-12 or 2026-07 "added" dates because of folder reorganizations. This history uses commit messages and rename-aware first-add dates, not raw file dates.
- Spirits are identified by when their folders first appeared; spirit "done" dates for 2021-2022 work come from commit messages where they exist.
- A branch history (`web`, `mcp`, `action-targets`, `undo-action`, `serialize-space-entity`) is in the repo but not itemized here.
