# Deepwater documentation

Design baseline: 6 November 2026, with the slot specification agreed on 7 November, the Landing specification supplied on 13 November and the five-tag design updated on 18 November. Deepwater is the working title. The [slot runtime](Slot%20Machine%20Runtime.md) implements the five-tag Discover rules with approved artwork. The [Landing increment](Landing%20Challenge%20Mechanic.md) implements movement, scoring and success/failure boundaries with the approved Tackle Apparatus artwork; see [art validation](Validation/TackleLandingArt/README.md). The [Build/Upgrade increment](Rig%20Building%20Mechanic.md) implements new-species placement, automatic duplicates, levels and sale-value accumulation in its own Tackle screen; see [validation](Validation/RigBuilding/README.md). Its approved Penpot artwork and automatic upgrade display were integrated on 16 November; see [Build artwork validation](Validation/TackleBuildArt/README.md). Biome completion/reward predicates remain open. Numeric prototype settings and historical illustrative abilities are not final content.

The current design gives every creature exactly one tag and one primary ability. Tags are Schooling, Predator, Anchored, Armored and Elusive; Harbor preparation chooses four of five, and a pair's unmatched symbol adds no species preference. The 19 November implementation migrates the creature definitions, native preparation controls, reel mappings and supporting effect handlers; see [current validation](Validation/FiveTagAbilities/README.md). Shared philosophies and mechanics belong in the GDD and supplements; species assignments, individual effects, descriptions, tuning and level progressions belong in editable game content definitions, without a duplicate Markdown roster. Tuning remains provisional, and new creature portraits require supplied artwork.

## Active specifications

The 24 November [card-collision presentation](Validation/StrikeWater/Contained/README.md) replaces scoring projectiles with a framed creature-card windup/dash, score-card recoil and short visual-stage shake. Three vertical turns separate the encounter creature from rig scoring at entry and return; contributor changes retain a single turn. Reduced motion uses one stationary fade per transition. A custom PixelFX water burst now marks positive contact, with gain-based visual tiers and a brief native score-digit punch. Reduced motion suppresses this layer. The timing glass also fills with the current strike's delivered damage, reaches full at half Resistance, and drains over two-thirds of a second after the final contribution. Fish and accuracy bands remain above the water. Exact scoring, collision deadlines and existing sound assets remain unchanged.

The 17 November artwork update gives Landing and Build integrated machine variants with identical outer pixels and geometry. The 18 November Discover variants extend that same outside silhouette to the first phase; the current version restores the original-size wheels in a taller internal bay. Original and compact animated prefabs remain preserved with tested version menus. See [current Discover validation](Validation/DiscoverIntegrated/FullSize/README.md), [compact variant and rollback](Validation/DiscoverIntegrated/README.md), [Landing validation](Validation/LandingIntegrated/README.md) and [shared-shell provenance](UIThemes/TackleApparatusConcept/LandingIntegrated/README.md). No transition animation is implemented by these artwork updates.

| Document | Responsibility |
| --- | --- |
| [Full GDD](Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md) | Authoritative mechanics, proposals and open decisions |
| [Brief](GDD_IMPLEMENTATION_BRIEF.md) | Concise day-to-day rules |
| [Slot machine](Slot%20Machine%20Mechanic.md) | Agreed preparation, reel, matching, rarity fallback and eligibility rules |
| [Landing Challenge](Landing%20Challenge%20Mechanic.md) | Track, strikes, attempts, motion, scoring, failure payout and Build handoff |
| [Architecture](Architecture.md) | Intended responsibilities for the agreed design |
| [Card specification](Card%20Visual%20Specification.md) | Supplied-art boundary and dynamic fields |
| [UI plan](UI%20Redesign%20Plan.md) | Presentation requirements and unresolved inputs |
| [Rig Building](Rig%20Building%20Mechanic.md) | Unique species, placement, levels, duplicate upgrades and sale value |
| [Creature abilities](Creature%20Ability%20Design.md) | Content ownership, tag identity, shared ability timing, positional authoring and implementation boundaries |
| [Equipment](Equipment%20Passive%20Rules.md) | Design scope and progression decisions |
| [Techniques](Technique%20Card%20Definitions.yaml) | Review status; no redesigned catalog is finalized |
| [Events](Event%20Rules%20Implementation%20Tasks.md) | Compatibility decisions and technical constraints |
| [Development guidelines](Full%20Game%20Development%20Guidelines.md) | Production quality policy |
| [Sound style](Sound%20Style.md) | Selected Charged Cabinet effects palette, timing, event vocabulary, mix hierarchy and authoring contracts |

The GDD takes precedence over code, serialized content, screenshots and historical approvals for shared gameplay rules. Editable game definitions own individual species content within those rules. Supplements must not indirectly finalize an open GDD decision. Existing filenames and code identifiers remain stable; the player-facing working title is Deepwater.

## Reference status

[Architecture Implementation Reference](Architecture%20Implementation%20Reference.md), [UI Presentation Reference](UI%20Presentation%20Reference.md) and [Card Presentation Reference](Card%20Presentation%20Reference.md) preserve existing implementation/visual details. They do not define current gameplay.

The following are historical implementation, validation or artwork records unless the current GDD explicitly adopts a rule:

- Validation/, Playtests/, image-generation task documents and validation scripts.
- UIThemes/MidnightFieldJournal/, including rosters, manifests, revision/sync evidence, ledgers and pending lists. Preserve artwork provenance and import status; depicted card text and controls do not approve mechanics.
- Midnight Field Journal Integration Tasks.md, MVP Workplan.md and MVP Technical Debt.md. Technical concerns require reassessment; obsolete gameplay tasks are not requirements.
- Fishing_Card_Roguelike_Examples.docx, an earlier reference collection, not an active creature catalog.

Historical test counts validate recorded behavior only. Do not rewrite evidence to claim the redesigned game has passed checks. No runtime or artwork changes are authorized by this documentation update.
