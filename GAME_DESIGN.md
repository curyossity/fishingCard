# Deepwater

Working title. Agreed direction as of 6 November 2026, subject to refinement and playtesting.

Deepwater is a skill-based fishing roguelike in Unity 2D. Players influence encounters, land creatures through a short skill challenge, and build an ordered rig through irreversible placement decisions. Illustrated cards remain the world, with a tactile physical-card presentation.

The core loop is **Discover → Land → Build**:

1. Use three reels, matching tags and optional lock/respin to influence the next creature.
2. Generate Landing Points through accurate actions amplified by the existing rig. Reach the creature's Resistance within limited attempts.
3. On success, choose Catch or Bait and insert the creature at any permitted rig position. Existing creatures keep their relative order and cannot be manually rearranged. Continue automatically.

Failing a Landing Challenge immediately ends the expedition without acquiring that encounter's creature. Failure and successful completion both award the full accumulated sale value of previously attached Catch creatures; no Catch value is lost because of failure. Bait has zero sale value and can provide stronger utility or positional support. New creatures contribute after landing and placement, affecting future encounters.

Every expedition starts in Shallows with an empty rig. Creatures are temporary: all attachments are removed at expedition end on success or failure, and none carry over. Permanent unlocks and progression persist. Completing a biome permanently unlocks the next; earlier biomes must still be traversed. Successful completion may additionally unlock equipment, creature availability or other permanent rewards. Completion conditions, additional rewards, currency details and achievements remain open.

Timing determines action quality; creature abilities, Techniques and equipment amplify it. Traditional combat and extra tactical landing systems are outside the selected direction. Voluntary surfacing, Line Capacity, overload and line-break failure are excluded.

The [full GDD](Docs/Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md) is authoritative. See the [brief](Docs/GDD_IMPLEMENTATION_BRIEF.md) and [documentation index](Docs/README.md). Examples and existing assets do not finalize undecided rules. This update changes documents only.
