# Deepwater — Creature Ability Design

Design baseline: 6 November 2026. The [GDD](Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md) is authoritative. No redesigned creature ability catalog or balance table is finalized.

## Role and build principles

- Give creatures distinct Catch and Bait effects; activate only the committed role.
- Catch contributes resolved sale value. Bait has zero sale value and may provide stronger utility or positional support.
- Integrate positioning into some Catch/Bait effects. Do not add a universal third ability layer or require every creature to be positional.
- Both roles participate in adjacency and tag synergies unless an effect explicitly excludes a role.
- Support landing performance, haul value, Technique interactions and varied builds. Do not make every creature simply grant points.
- Ground abilities in anatomy, behavior or ecology. Allow simple cards and intentionally empty roles.
- The new creature begins contributing after successful landing and placement, affecting future encounters.
- All creature attachments are temporary. Every expedition starts with an empty rig; ending on success or failure removes all attachments after capturing their haul value. Both outcomes award full accumulated Catch sale value, with zero Bait value. Expedition cleanup is not a deliberate Release and must not erase resolved value before payout.

## Ordered-rig contract

Newly acquired creatures may be inserted anywhere permitted. Existing creatures retain relative order and cannot be manually rearranged. Placement is committed. Abilities may restrict future insertion. Positional effects update dynamically after attachment/removal.

Define orientation and terms such as before, after, adjacent and contiguous precisely when finalizing an effect. Position, acquisition age and identity are distinct. Before accepting a restriction, specify its interactions with other restrictions and any no-legal-position state. Do not add a rig-size limit as a substitute for removed capacity.

## Illustrative concepts, not approved card definitions

**Mackerel Catch:** +5 value per other contiguous Schooling creature connected in either direction.

**Mackerel Bait:** each other contiguous Schooling creature adds +3 Landing Points to successful strikes.

Both Catch and Bait can participate in a Schooling group unless final wording excludes one. Numbers, exact counting, multi-source stacking and scoring timing remain open.

**Anchored creature:** might prevent insertion before itself while doubling Catch value before itself. This is a candidate ability, not an automatic Anchored-tag rule. Restriction details and balancing are undecided.

## Authoring checklist

For each role, define:

- Intended build role and thematic basis.
- Trigger, targets, role eligibility, positional scope and use of effective tags.
- Ongoing versus triggered behavior; duration, source dependency and removal consequences.
- Stacking across copies, effect order, limits and any required choices.
- Interaction with attempts, accuracy, scoring or sale value where relevant.
- Whether it restricts future placement and how restrictions combine.
- Concise text fitting supplied panels; compact cards do not gain full rules text.

Exact Release, role-change and Technique timing rules remain open. Do not silently carry their existing implementation into new effects. An ability must not bypass a required landing challenge without an explicit design decision.

## Content review scope

All existing creatures require ability review, including Cleaner Shrimp, Clinging Crab, Cod, Garfish, Glass Minnow, Jellyfish, Lionfish, Mackerel, Moonlit Ray, Mussel, Sardine, Sea Anemone, Sea Bass, Sea Urchin, Small Octopus, Spiny Lobster, Squid, Tidepool Eel, Flounder, Cuttlefish and Apex creatures. Names identify content to review; they do not approve old effects, values, availability or biome assignments.

Review weight reduction, capacity/overload, surfacing, encounter bypasses, concealment, role swaps, releases and replacement grants for compatibility. Do not mechanically translate weight bonuses into Landing Points.

Vocabulary starting point: Fish, Predator, Schooling, Small, Heavy, Anchored, Armored, Elusive. Weight and Heavy need reevaluation. Resistance is a separate challenge target, not a renamed Weight field. Existing artwork, catalogs, stable IDs and runtime assets remain unchanged in this documentation update.
