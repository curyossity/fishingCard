# Deepwater — Implementation Brief

Working title; agreed direction dated 6 November 2026, subject to refinement and playtesting. The [full GDD](Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md) is authoritative. This is design, not a claim of implementation. See [document status](README.md).

## Core loop

**Discover → Land → Build**, in Unity 2D with illustrated cards as the world.

1. **Discover:** choose four of eight tags in the Harbor; keep those equally likely symbols for the expedition. Three independent reels initially spin, then accept or lock exactly one and respin the other two once. Keep the fixed respin result visible until explicit Accept Tags; no further lock/respin or timed continuation is allowed. Different symbols allow any eligible biome creature; a pair guarantees its repeated tag and prefers the third; a triple guarantees its tag. Pair/triple rarity chances progressively improve. Roll rarity separately and fall back to the nearest suitable tier, breaking ties toward the lower tier without abandoning the guaranteed tag. Normal creatures are biome-wide, not depth-limited; preserve unlocks and exclude separately gated Apex. Rarity probabilities and preference strength remain tunable. See [slot specification](Slot%20Machine%20Mechanic.md).
2. **Land:** generate Landing Points to reach the creature's Resistance within limited attempts. Skill determines action quality; the existing rig, creature abilities, Techniques and equipment may enhance it. Every creature encounter can be won or lost.
3. **Build:** after success, choose Catch or Bait and a permitted insertion position, commit and recalculate synergies. Continue automatically to the next encounter/progression step. New creatures contribute after placement, affecting future encounters.

Failing a Landing Challenge immediately ends the expedition; the failed creature is not acquired. Both failure and successful completion award the full accumulated sale value of all previously attached Catch creatures. Bait has zero sale value. Failure never reduces accumulated Catch value.

Every expedition starts with an empty rig. Collected creatures are temporary: all attachments are removed at expedition end on either outcome and never carry over to future expeditions. Preserve the resolved haul value before clearing the rig. Permanent unlocks and progression persist.

## Landing and scoring

Landing uses a horizontal track with independently moving fish/zone. Strike with **Space** or the UI button. Perfect overlap takes priority over Good; no overlap with Good is treated as Miss (clarification of the supplied wording is pending). The fish indicator has an interval, not a substituted artwork hit box.

Choose Steady, telegraphed Burst or learnable Erratic movement uniformly once per encounter for now. The zone oscillates predictably. Every strike consumes an attempt, including Miss; **four attempts is prototype tuning**. Reaching Resistance succeeds immediately, including on the final attempt; otherwise exhausting attempts fails immediately. Brief feedback freezes both indicators without delaying the outcome or allowing another strike.

Initial model: `Strike Points = (Accuracy Base + Applicable Rig Bonuses) × Product of Applicable Rig Multipliers`. Prototype bases **0 / 10 / 20** for Miss/Good/Perfect remain configurable. Ordinary bonuses do not score on Miss unless an explicit Miss rule changes that. Capture only committed rig instances and stored-role effects; attribute each bonus/multiplier. Current prototype retains fractional points without finalizing integer rounding. Existing species receive no illustrative abilities automatically.

The [Landing specification](Landing%20Challenge%20Mechanic.md) records the plain implementation, prototype tuning, scoring boundaries and open questions. Success currently supplies a Build handoff without attachment; failure pays full accumulated Catch value and clears the rig. Landing artwork and Build commitment are outside this increment.

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
