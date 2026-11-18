# Deepwater — Creature Ability Authoring

Current baseline: Rig Building and Creature Upgrades, 15 November 2026; five-tag authoring rules updated 18 November 2026. The [GDD](Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md) and [Rig Building specification](Rig%20Building%20Mechanic.md) govern mechanics. This document specifies shared authoring contracts, not a species catalog. The five-tag content and additional effect handlers have not been installed by this documentation update.

## Content authority and tag identity

Each creature species, including Apex, has exactly one gameplay tag: Schooling, Predator, Anchored, Armored or Elusive. Its single primary ability expresses that tag's philosophy as defined in the [GDD](Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md#tags-and-properties). Tags identify a build role, not every biological trait; there is no separate lead-theme field or automatic tag power.

Author species identities, tag assignments, abilities, short descriptions, tuning and level progressions in editable game content definitions. Do not duplicate that roster or its per-species rules in Markdown. Documentation governs common mechanics and contracts; historical content and conversational proposals must not be treated as installed data. Tentative assignments and unresolved scaling/stacking must be resolved before the affected content is committed.

Abilities may share outcomes while using different conditions and decisions. Support same-tag relationships, cross-tag support and independently useful creatures. Schooling may support mixed groups; Armored should offer proactive dependability as well as recovery; Elusive includes timing assistance as well as precision rewards. Do not restrict any tag to only sale value, multipliers or another single output. Technique interactions still depend on their separate unresolved rules.

## One primary ability

Each species has one `CardDefinition.primaryAbility`: name, description and related data-driven components. Components specify operation, trigger, accuracy filter, positional count, tag filter, whether to count each qualifying position and value recipients. A related benefit/restriction can belong to one ability. No Catch/Bait choice or sale-value sacrifice exists.

`CreatureAbilityDefinition.Snapshot()` validates and freezes the blueprint when a species joins the rig. Its runtime level and copy count live on the member. Amounts currently scale as initial amount plus increment per level after Level 1; those parameters are editable and are not final balancing values. No gameplay level cap is introduced.

## Positional semantics

Index zero is nearest the Rod. Before/after are directional; adjacency has at most two positions. Contiguous-other counting excludes the source and stops at the first nonmatching position in each direction. A contiguous-tag source must itself carry that tag. A Level 5 neighbor still counts once. A condition may activate once rather than scale per qualifying position.

The Line Tip is the final occupied position. Exactly at the center means equal numbers of occupied positions before and after the source; a qualifying center ability requires at least one on each side, so a creature alone does not qualify. Distinct-tag counts count each qualifying tag once, not once per creature. Bounded directional effects must name their stopping condition and whether the boundary member is included. Insertion and authorized removal recalculate these relationships without rearranging existing members.

## Shared encounter and trigger contracts

- Only committed members at encounter start contribute; the encountered creature and Build previews never assist their own acquisition. Capture their current-level ability rules and positional conditions for the encounter.
- Successful means Good or Perfect. Ordinary Misses still score zero and consume an attempt unless an explicitly authored rule changes that behavior.
- Consecutive-strike conditions refer to immediately successive strikes in the same encounter. An intervening nonqualifying strike breaks the sequence. First-strike flags, sequences and once-per-encounter limits reset for each encounter.
- Substituting an accuracy base changes scoring only; it does not reclassify a Good strike as Perfect for other triggers. Bonuses remain before multiplier products.
- A condition expressed as points already accumulated is evaluated before the current strike adds its score. A final-attempt condition uses attempts available before that strike is consumed or any resulting attempt grant occurs.
- Resolve strike-triggered attempt grants once before checking exhaustion. Reaching Resistance still succeeds immediately; an extra attempt cannot revive an already failed encounter or extend a successful one.
- Acquired and Upgraded are separate events. Continuous recalculation cannot repeat a grant, and expedition cleanup cannot fire gameplay removal triggers.

Every effect must specify its trigger, recipients, condition, duration, limits and level progression. Author whether multiple components/sources stack, how overlapping recipients are handled and how assistance is bounded before installing affected content. General multi-source stacking, final level scaling and zone/motion assistance limits remain unresolved; these guidelines do not silently choose additive, multiplicative or strongest-only policies. Zone assistance must preserve a centered Perfect region contained within Good. Movement assistance must preserve learnable movement and the value of timing.

## Current implementation support

- Landing bonus or additive multiplier growth above ×1, conditioned on successful/Good/Perfect/explicit-Miss accuracy.
- Additional encounter attempts or a count of protected Misses.
- Whole-unit sale-value bonuses and multipliers applied to self, adjacent, before, after or all matching positions.
- Continuous before/after insertion restrictions.
- Explicit self-value grants triggered only by Acquired or only by Upgraded.

All continuous flat value bonuses resolve before multiplier products. They use accumulated base value for all copies. Completed trigger grants are stored separately. Recalculation never repeats a trigger. Landing receives immutable current-level rules and source IDs from already committed members only.

Unsupported trigger operations, invalid enum values, negative/nonfinite scaling, fractional economy authoring and arithmetic overflow are rejected before mutation. Technique manipulation, physical motion/zone assistance and removal abilities require precise content and supported handlers before assignment; a UI description alone never implements an effect.

## Existing validation fixtures

`RigBuildingChecks` exercises level-scaled contiguous-group multipliers and sale-value bonuses using disposable definitions. Preserve the test evidence without interpreting its historical species labels or numeric fixtures as current catalog assignments. Encounter-sequence triggers, conditional attempt grants, scoring-base substitutions, distinct-tag/center/bounded-segment conditions and movement/zone assistance require supported handlers and appropriate validation before content assignment; the component list above does not claim these are already implemented.

The content review must select each real species' primary ability, progression, restrictions, trigger scope and readable description. Existing weight, overload, role-switch and voluntary-surfacing effects do not become primary abilities automatically. Preserve old artwork/provenance records without treating their printed rules as current behavior.
