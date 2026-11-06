# Deepwater — Implementation Brief

Working title; agreed direction dated 6 November 2026, subject to refinement and playtesting. The [full GDD](Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md) is authoritative. This is design, not a claim of implementation. See [document status](README.md).

## Core loop

**Discover → Land → Build**, in Unity 2D with illustrated cards as the world.

1. **Discover:** three reels, matching tags and optional lock/respin influence the next creature. Retain this slot-machine direction without adding landing connections. Detailed matching, probabilities, lock/respin limits/costs and fallback still need specification; tag-bar rules do not supply them.
2. **Land:** generate Landing Points to reach the creature's Resistance within limited attempts. Skill determines action quality; the existing rig, creature abilities, Techniques and equipment may enhance it. Every creature encounter can be won or lost.
3. **Build:** after success, choose Catch or Bait and a permitted insertion position, commit and recalculate synergies. Continue automatically to the next encounter/progression step. New creatures contribute after placement, affecting future encounters.

Failing a Landing Challenge immediately ends the expedition; the failed creature is not acquired. Both failure and successful completion award the full accumulated sale value of all previously attached Catch creatures. Bait has zero sale value. Failure never reduces accumulated Catch value.

Every expedition starts with an empty rig. Collected creatures are temporary: all attachments are removed at expedition end on either outcome and never carry over to future expeditions. Preserve the resolved haul value before clearing the rig. Permanent unlocks and progression persist.

## Landing and scoring

Leading concept: moving fish and moving landing zone; time a strike when they align. Candidate grades: Miss, Good, Perfect. Creatures have distinct, learnable movement patterns, potentially influenced by tags/characteristics.

Attempts are limited. **Four is only a prototype suggestion.** Controls, grades, points, movement and attempt details remain open.

Conceptual formula: `Landing Points = (Accuracy Base + Rig Bonuses) × Rig Multipliers`.

This is not a final algorithm. Miss bonuses, stacking, rounding, ordering and limits remain undecided. Abilities may offer flat bonuses, multipliers, extra attempts and special scoring interactions. Accuracy must remain meaningful as builds become powerful.

Difficulty rises mainly through Resistance. Characteristics/rarity may influence it. Faster fish and smaller zones should not be the primary escalation; widening/slowing upgrades may complement scoring growth.

## Rig and abilities

- Ordered attached creatures; insert new acquisitions anywhere permitted.
- No manual rearrangement after commitment. Preserve existing relative order.
- Some abilities may restrict insertion; no-legal-position handling remains open.
- Recalculate positional effects dynamically after attachment/removal.
- Both roles participate in adjacency/tag synergies unless explicitly excluded.
- Catch contributes resolved sale value. Bait has zero sale value and may offer stronger utility/support.
- Only the selected role's effects operate. Position belongs within some Catch/Bait effects, not a third universal layer.
- Support landing, haul value, Techniques and varied builds; not every creature needs positional or point bonuses.

Illustrative only: Mackerel Catch gains +5 value per other contiguous connected Schooling creature; Bait grants +3 Landing Points to successful strikes per such creature. An Anchored creature might restrict insertion before itself while doubling Catch value before itself. These are not finalized abilities or automatic tag behavior.

Vocabulary review baseline: Fish, Predator, Schooling, Small, Heavy, Anchored, Armored, Elusive. Weight and Heavy need reevaluation; Weight does not imply Resistance or a rig limit.

## Expedition and progression

Only **Shallows** is initially unlocked. Every expedition starts there with an empty rig and traverses earlier biomes to reach later ones. Completing a biome permanently unlocks the following biome. Failure preserves permanent progression and pays full accumulated Catch value. Success pays that same full value and may additionally grant permanent completion rewards.

Completion criteria, transition timing, length, endpoint and additional reward tables remain open. Equipment/upgrades, creature availability, extra options and special unlocks are possible completion rewards. Creature unlocks make creatures available in future expeditions; they do not preserve attachments. Gold and conversion details remain open; the full Catch-value payout on both outcomes is decided. Achievement unlocks are considered; sample objectives are not finalized quests.

## Review scope and boundaries

Redesign creature abilities. Review equipment (Rod/Reel/Hook may remain), Techniques and deck configuration, Release, role changes, events, treasure, Apex, chains, concealment and replacements. Exact Technique windows/effects remain open. No universal free Release, manual reorder or challenge skip is granted.

Voluntary surfacing, Line Capacity, overload and line-break failure are excluded. Traditional combat and extra tactical landing systems are not planned. Do not import historical controls, prices, quests, decks or effects as current design.

Validate repetition, skill/build balance, Resistance scaling, creature variety, readable rig complexity, earlier-biome replay, completion rewards and powerful combinations. Consult the GDD's open decision register before implementing unresolved rules.

Code, artwork, prefabs, scenes and runtime data remain unchanged by this update. [Architecture](Architecture.md) defines intended ownership; [README](README.md) identifies historical records.
