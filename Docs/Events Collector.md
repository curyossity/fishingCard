# Events Collector — Existing Editor Reference

This describes the existing tool, not approved Deepwater mechanics. The [GDD](Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md) and [event planning](Event%20Rules%20Implementation%20Tasks.md) govern design. No editor code, event assets or artwork changed.

Open **Fishing Cards > Data > Events Collector**, or **Window > Fishing Cards > Events Collector** in Unity.

The list shows authored Hazard, Environment, Opportunity and generic Encounter cards from Assets/Cards. Category/text filters cover identity, rules, type, rarity, tags and biome. Creatures, Treasure and Apex records use the Creature Catalog.

Select a record to inspect identity, tags, illustration references/transforms, rules, highlights, biome/depth eligibility and effects. Event records have no creature Weight, sale Value, Catch/Bait role or rig attachment. Rarity/tags are metadata, not creature hooks/tag rails.

Actual Encounter Pool Membership controls biome-tier membership independently of eligibility constraints. Locate selects the asset. Serialized editing supports Undo/Redo; Save Changes writes dirty assets. Search/selection/scroll state belongs to the editor.

Exposed effect fields do not approve mechanics or imply handler support. New event content needs designed timing, targets, durations and supported behavior within Deepwater. Do not copy weight/overload effects or Surface/Descend windows into new designs.

Reef Snag, Silt Burst, Gentle Upwelling, Loose Knot and Fresh Shell Fragments identify existing content needing review; redesigned effects are undecided. Handler/lifetime details are in [the implementation reference](Architecture%20Implementation%20Reference.md). [Historical validation](Validation/EventIntegration/README.md) does not validate redesigned gameplay.
