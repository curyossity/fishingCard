# Deepwater — Implementation Brief

Working title; agreed direction dated 6 November 2026, subject to refinement and playtesting. The [full GDD](Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md) is authoritative. This is design, not a claim of implementation. See [document status](README.md).

## Core loop

**Discover → Land → Build**, in Unity 2D with illustrated cards as the world.

1. **Discover:** choose four of eight tags in the Harbor; keep those equally likely symbols for the expedition. Three independent reels initially spin, then accept or lock exactly one and respin the other two once. Keep the fixed respin result visible until explicit Accept Tags; no further lock/respin or timed continuation is allowed. Different symbols allow any eligible biome creature; a pair guarantees its repeated tag and prefers the third; a triple guarantees its tag. Pair/triple rarity chances progressively improve. Roll rarity separately and fall back to the nearest suitable tier, breaking ties toward the lower tier without abandoning the guaranteed tag. Normal creatures are biome-wide, not depth-limited; preserve unlocks and exclude separately gated Apex. Rarity probabilities and preference strength remain tunable. See [slot specification](Slot%20Machine%20Mechanic.md).
2. **Land:** generate Landing Points to reach the creature's Resistance within limited attempts. Skill determines action quality; the existing rig, creature abilities, Techniques and equipment may enhance it. Every creature encounter can be won or lost.
3. **Build / Upgrade:** a new species is previewed and inserted into a permitted gap at Level 1; a duplicate automatically upgrades the existing position. Every landed copy adds full base sale value. Recalculate primary abilities and continue automatically. No Catch/Bait choice or manual rearrangement. See [Rig Building](Rig%20Building%20Mechanic.md).

Failing a Landing Challenge immediately ends the expedition; the failed creature is not acquired. Both failure and successful completion award the full accumulated sale value of all previously landed creatures, including duplicates and applicable value bonuses. Failure never reduces accumulated creature value.

Every expedition starts with an empty rig. Collected creatures are temporary: all attachments are removed at expedition end on either outcome and never carry over to future expeditions. Preserve the resolved haul value before clearing the rig. Permanent unlocks and progression persist.

## Landing and scoring

Landing uses a horizontal track with independently moving fish/zone. Strike with **Space** or the UI button. Perfect overlap takes priority over Good; no overlap with Good is treated as Miss (clarification of the supplied wording is pending). The fish indicator has an interval, not a substituted artwork hit box.

Choose Steady, telegraphed Burst or learnable Erratic movement uniformly once per encounter for now. The zone oscillates predictably. Every strike consumes an attempt, including Miss; **four attempts is prototype tuning**. Reaching Resistance succeeds immediately, including on the final attempt; otherwise exhausting attempts fails immediately. Brief feedback freezes both indicators without delaying the outcome or allowing another strike.

Initial model: `Strike Points = (Accuracy Base + Applicable Rig Bonuses) × Product of Applicable Rig Multipliers`. Prototype bases **0 / 10 / 20** for Miss/Good/Perfect remain configurable. Ordinary bonuses do not score on Miss unless an explicit Miss rule changes that. Capture only committed species, levels and primary abilities; attribute each bonus/multiplier. Current prototype retains fractional points without finalizing integer rounding. Existing species receive no illustrative abilities automatically.

The [Landing specification](Landing%20Challenge%20Mechanic.md) records the implementation, prototype tuning, scoring boundaries and open questions. Success currently supplies a Build handoff without attachment; failure pays full accumulated creature value and clears the rig. Approved Landing artwork is integrated; Build placement and automatic duplicate upgrades are implemented in a separate Tackle canvas.

Difficulty rises mainly through Resistance. Characteristics/rarity may influence it. Faster fish and smaller zones should not be the primary escalation; widening/slowing upgrades may complement scoring growth.

## Rig and abilities

- Each species occupies at most one ordered position. New species enter at Level 1; duplicates add one level and one collected copy automatically.
- Choose a legal insertion gap for a new species, preview without mutation, then commit once. Existing members retain relative order; no manual rearrangement.
- Every copy contributes full base sale value. Value effects modify accumulated species value unless explicitly scoped otherwise.
- One primary ability per species. Upgrades strengthen its identity; exact species assignments and level caps remain open.
- Count positions, not copies, for adjacency, contiguous groups and tag counts. Before is nearer the Rod; after is nearer the Line Tip.
- Recalculate continuous effects on insertion, upgrade and authorized removal. Acquired and Upgraded triggers are separate.
- Only committed current-level abilities assist the next Landing. The encountered creature and previews cannot assist their own acquisition.
- No rig capacity or replacement slot cap is introduced. No-legal-position handling must be designed before assigning a restriction combination that can cause it.

Illustrative Sardine multiplier growth and Mackerel contiguous-value bonuses remain test examples, not finalized catalog assignments. The primary-ability schema supports landing, attempts, Miss protection, value and placement restrictions; other effect families require explicit handlers and content.

The approved Penpot Build/Upgrade artwork is integrated as a separate live-data canvas. New placements retain legal-gap selection and confirmation; duplicates show a brief automatic result with their current level, value and effect before Discover becomes visible. No extra upgrade decision or gameplay reward is introduced. See [artwork validation](Validation/TackleBuildArt/README.md).

Vocabulary: Fish, Predator, Schooling, Small, Heavy, Anchored, Armored, Elusive. Weight/Heavy remain under review; Weight does not imply Resistance or capacity.

## Expedition and progression

Only **Shallows** is initially unlocked. Every expedition starts there with an empty rig and traverses earlier biomes to reach later ones. Completing a biome permanently unlocks the following biome. Failure preserves permanent progression and pays full accumulated creature value. Success pays that same full value and may additionally grant permanent completion rewards.

Completion criteria, transition timing, length, endpoint and additional reward tables remain open. Equipment/upgrades, creature availability, extra options and special unlocks are possible completion rewards. Creature unlocks make creatures available in future expeditions; they do not preserve attachments. Gold and conversion details remain open; the full creature-value payout on both outcomes is decided. Achievement unlocks are considered; sample objectives are not finalized quests.

## Review scope and boundaries

Redesign creature abilities. Review equipment (Rod/Reel/Hook may remain), Techniques and deck configuration, Release,  events, treasure, Apex, chains, concealment and replacements. Exact Technique windows/effects remain open. No universal free Release, manual reorder or challenge skip is granted.

Voluntary surfacing, Line Capacity, overload and line-break failure are excluded. Traditional combat and extra tactical landing systems are not planned. Do not import historical controls, prices, quests, decks or effects as current design.

Validate repetition, skill/build balance, Resistance scaling, creature variety, readable rig complexity, earlier-biome replay, completion rewards and powerful combinations. Consult the GDD's open decision register before implementing unresolved rules.

Discover, Landing and Build/Upgrade have runtime implementations; completion predicates, permanent reward grants and the finalized species catalog remain separate design work. [Architecture](Architecture.md) defines intended ownership; [README](README.md) identifies historical records.
