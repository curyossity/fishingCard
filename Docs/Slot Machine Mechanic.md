# Deepwater — Slot Machine Mechanic

Agreed design direction, 7 November 2026. Exact rarity probabilities and preference strength remain balancing values. This specification supplements the [GDD](Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md).

## Preparation and reels

Before an expedition, the player chooses exactly four distinct active tags in the Harbor from Fish, Predator, Schooling, Small, Heavy, Anchored, Armored and Elusive. Those four symbols remain active throughout the expedition. Each of three independent reels initially has a 25% chance of each active tag. Active symbols influence discovery; they do not restrict the entire creature pool to creatures with those tags.

Every standard creature encounter begins with an initial spin. The player either accepts that result or locks **exactly one** reel and respins the other two **once**. Both respun reels independently draw from the same four tags. Locking two reels, respinning without a lock and additional respins are unavailable. A lock can be undone before using the respin. After respinning, show the final result with lock/respin controls disabled. The player may review it for as long as desired; only **Accept Tags** selects and reveals the creature. There is no timed or automatic continuation.

## Outcomes

| Result | Tag constraint | Rarity |
| --- | --- | --- |
| Three different | None; any eligible biome creature, including creatures with tags outside the active four | Baseline distribution |
| Exactly two matching | Repeated tag is mandatory; the third tag increases species-selection weight but is optional | Modest improvement to higher-rarity probabilities |
| Three matching | Repeated tag is mandatory | Stronger higher-rarity improvement than a pair |

For `Predator / Predator / Schooling`, Predator is guaranteed and Schooling is preferred. A creature with both tags has higher selection weight than an otherwise eligible Predator in the resolved rarity tier. If no creature has both, eligible Predators remain selectable. A triple never guarantees Rare or Legendary.

Tag interpretation, requested rarity and final species selection are separate steps. Roll Common, Uncommon, Rare or Legendary from the outcome's configurable distribution. Restrict candidates to the guaranteed tag, if any. If the requested tier has no suitable candidate, select the nearest available tier **within that same tag constraint**. Equally close tiers resolve to the lower rarity. Apply the pair's secondary-tag preference when selecting a species within the resolved tier. This keeps the rarity distribution independent of species counts and secondary-tag prevalence.

A guaranteed tag must have an eligible candidate before an encounter can be finalized. Never satisfy rarity by discarding the guaranteed tag. Content/eligibility validation must detect missing tag coverage; it must not silently change active symbols, reroll forever or choose an unrelated creature.

## Eligibility

Normal creatures assigned to a biome are available across the entire biome. Individual normal-creature depth intervals do not restrict standard slot selection. Selection still respects biome membership and permanent creature unlock requirements. Do not introduce new unlock rewards or grant locked creatures by default.

Apex creatures are special Legendary encounters with separate eligibility and progression conditions. Standard slots cannot select them or bypass their conditions. Other Legendary creatures retain any authored unlock requirements. Events, forced encounters and authored chains are separate compatibility work; they do not silently replace standard slot selection.

## Probabilities and decisions

| Outcome | Initial spin | Accept pairs/triples; respin three different while preserving one face |
| --- | --- | --- |
| Three different | 37.50% | 14.0625% |
| Pair | 56.25% | 77.34375% |
| Triple | 6.25% | 8.59375% |

These follow four equally likely symbols and independent reels. The second column of final outcomes assumes that particular player strategy, not a forced strategy. Players may accept three different symbols for variety or respin a match while pursuing a preferred tag. Future effects could alter probabilities, but none are part of the baseline.

## Encounter handoff

Discover selects and reveals a creature. Land then tests the player's timing and existing rig against its Resistance. Discover does not catch the creature, generate Landing Points or supply new rig benefits. Only successful landing permits Catch/Bait choice and irreversible rig insertion. The new attachment helps future encounters. Failure ends the expedition under the GDD's full accumulated Catch-value payout rule.

Keep preparation, reel state, tag matching, rarity selection, eligibility, final selection and presentation separate. Preserve immutable expedition settings and occurrence-safe commands. Equipment, Techniques, creature effects, replacement symbols and biome modifiers are possible future extensions, not enabled baseline mechanics. Avoid unnecessary confirmation steps or delays.

Implementation and provisional tuning are recorded in [Slot Machine Runtime](Slot%20Machine%20Runtime.md).
