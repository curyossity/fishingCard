# Deepwater documentation

Design baseline: 6 November 2026, with the slot specification agreed on 7 November and the Landing specification supplied on 13 November. Deepwater is the working title. The [slot runtime](Slot%20Machine%20Runtime.md) implements Discover with its approved artwork. The [Landing increment](Landing%20Challenge%20Mechanic.md) adds plain controls, movement, scoring and success/failure boundaries; its artwork is not integrated. Build/placement and the remaining redesign are separate work. Numeric prototype settings and illustrative abilities are not final content.

## Active specifications

| Document | Responsibility |
| --- | --- |
| [Full GDD](Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md) | Authoritative mechanics, proposals and open decisions |
| [Brief](GDD_IMPLEMENTATION_BRIEF.md) | Concise day-to-day rules |
| [Slot machine](Slot%20Machine%20Mechanic.md) | Agreed preparation, reel, matching, rarity fallback and eligibility rules |
| [Landing Challenge](Landing%20Challenge%20Mechanic.md) | Track, strikes, attempts, motion, scoring, failure payout and Build handoff |
| [Architecture](Architecture.md) | Intended responsibilities for the agreed design |
| [Card specification](Card%20Visual%20Specification.md) | Supplied-art boundary and dynamic fields |
| [UI plan](UI%20Redesign%20Plan.md) | Presentation requirements and unresolved inputs |
| [Creature abilities](Creature%20Ability%20Design.md) | Catch/Bait and positional authoring |
| [Equipment](Equipment%20Passive%20Rules.md) | Design scope and progression decisions |
| [Techniques](Technique%20Card%20Definitions.yaml) | Review status; no redesigned catalog is finalized |
| [Events](Event%20Rules%20Implementation%20Tasks.md) | Compatibility decisions and technical constraints |
| [Development guidelines](Full%20Game%20Development%20Guidelines.md) | Production quality policy |

The GDD takes precedence over code, serialized content, screenshots and historical approvals. Supplements must not indirectly finalize an open GDD decision. Existing filenames and code identifiers remain stable; the player-facing working title is Deepwater.

## Reference status

[Architecture Implementation Reference](Architecture%20Implementation%20Reference.md), [UI Presentation Reference](UI%20Presentation%20Reference.md) and [Card Presentation Reference](Card%20Presentation%20Reference.md) preserve existing implementation/visual details. They do not define current gameplay.

The following are historical implementation, validation or artwork records unless the current GDD explicitly adopts a rule:

- Validation/, Playtests/, image-generation task documents and validation scripts.
- UIThemes/MidnightFieldJournal/, including rosters, manifests, revision/sync evidence, ledgers and pending lists. Preserve artwork provenance and import status; depicted card text and controls do not approve mechanics.
- Midnight Field Journal Integration Tasks.md, MVP Workplan.md and MVP Technical Debt.md. Technical concerns require reassessment; obsolete gameplay tasks are not requirements.
- Fishing_Card_Roguelike_Examples.docx, an earlier reference collection, not an active creature catalog.

Historical test counts validate recorded behavior only. Do not rewrite evidence to claim the redesigned game has passed checks. No runtime or artwork changes are authorized by this documentation update.
