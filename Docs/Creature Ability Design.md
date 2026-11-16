# Deepwater — Creature Ability Authoring

Current baseline: Rig Building and Creature Upgrades, 15 November 2026. The [GDD](Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md) and [Rig Building specification](Rig%20Building%20Mechanic.md) govern mechanics. The current species ability catalog remains open; illustrative examples are not automatically assigned.

## One primary ability

Each species has one `CardDefinition.primaryAbility`: name, description and related data-driven components. Components specify operation, trigger, accuracy filter, positional count, tag filter, whether to count each qualifying position and value recipients. A related benefit/restriction can belong to one ability. No Catch/Bait choice or sale-value sacrifice exists.

`CreatureAbilityDefinition.Snapshot()` validates and freezes the blueprint when a species joins the rig. Its runtime level and copy count live on the member. Amounts currently scale as initial amount plus increment per level after Level 1; those parameters are editable and are not final balancing values. No gameplay level cap is introduced.

## Positional semantics

Index zero is nearest the Rod. Before/after are directional; adjacency has at most two positions. Contiguous-other counting excludes the source and stops at the first nonmatching position in each direction. A contiguous-tag source must itself carry that tag. A Level 5 neighbor still counts once. A condition may activate once rather than scale per qualifying position.

## Supported components

- Landing bonus or additive multiplier growth above ×1, conditioned on successful/Good/Perfect/explicit-Miss accuracy.
- Additional encounter attempts or a count of protected Misses.
- Whole-unit sale-value bonuses and multipliers applied to self, adjacent, before, after or all matching positions.
- Continuous before/after insertion restrictions.
- Explicit self-value grants triggered only by Acquired or only by Upgraded.

All continuous flat value bonuses resolve before multiplier products. They use accumulated base value for all copies. Completed trigger grants are stored separately. Recalculation never repeats a trigger. Landing receives immutable current-level rules and source IDs from already committed members only.

Unsupported trigger operations, invalid enum values, negative/nonfinite scaling, fractional economy authoring and arithmetic overflow are rejected before mutation. Technique manipulation, physical motion/zone assistance and removal abilities require precise content and supported handlers before assignment; a UI description alone never implements an effect.

## Illustrative validation fixtures

Sardine: +0.2 / +0.3 / +0.4 successful-strike multiplier per other contiguous Schooling position at Levels 1 / 2 / 3. Three neighbors give ×1.6 / ×1.9 / ×2.2. Mackerel: +5 sale value per other connected Schooling position. These numbers are exercised using disposable definitions in `RigBuildingChecks`, not installed as final species data.

The content review must select each real species' primary ability, progression, restrictions, trigger scope and readable description. Existing weight, overload, role-switch and voluntary-surfacing effects do not become primary abilities automatically. Preserve old artwork/provenance records without treating their printed rules as current behavior.
