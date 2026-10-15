# Events Collector

Open **Fishing Cards > Data > Events Collector**, or **Window > Fishing Cards > Events Collector** in Unity.

The list shows authored Hazard, Environment, Opportunity, and generic Encounter cards from Assets/Cards. Choose a category or search by name, ID, rules text, type, rarity, tags, or biome. The approved coastal batch adds Reef Snag, Silt Burst, Gentle Upwelling, Loose Knot, and Fresh Shell Fragments. Legacy Feeding Frenzy, Ghost Net, Murky Current, Rough Current, and Snagged Kelp remain separately authored. Creatures, Treasure, and Apex are available through the Creature Catalog.

Select a card to edit its identity, tags, artwork references and encounter-art transforms, event rules and highlighted phrases, biome/depth constraints, and effects. The artwork previews use the assigned sprites. Event Type offers only Hazard, Environment, Opportunity, and Encounter. Catch-only stats, random caught Value, Bait effects, and Bait description/highlights are hidden and preserved in shared assets. Event designs have no Weight, sale Value, or Catch/Bait modes and never attach to the Catch Chain. Encounter Rarity and free-form tags are selection metadata; events display no rarity hooks or tag rail.

Under **Actual Encounter Pool Membership**, check the biome depth tiers that should include the event. Biome IDs and minimum/maximum depth are eligibility constraints applied to pool membership; editing those constraints alone does not add the card to a tier. Use Locate to find the biome asset.

Edits use Unity serialized properties and support Undo/Redo. **Save Changes** writes dirty assets to disk. **Locate Asset** selects the card in the Project browser. The collector refreshes after asset changes; **Refresh** also reloads the inventory. Search, category, selection, and scroll positions survive editor reloads.

To author another event, create a Card Definition through Unity's existing **Create > Fishing Cards > Card Definition** asset menu under Assets/Cards, set its Card Type to Hazard, Environment, Opportunity, or Encounter, and edit it in this collector. Configure actual pool membership before expecting it in encounters.

## Event authoring rules

The canonical rules are in [GDD section 9](Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md#event-cards-and-resolution-window). Reveal reactions finish before consequences. The player can use legal Techniques afterward until confirming Descend or Surface, even after cancellation. Unless the card states otherwise, event consequences and resulting creature reactions resolve once before that action; no Technique window reopens until the action finishes, and resolution never adds an extra depth advance.

Distinguish full cancellation of a revealed Hazard (discard without effects; trigger Weathered Shell and other cancellation reactions) from replacement of an unresolved event (prevent effects; no cancellation reward) and no eligible target (resolve without effect; not cancelled). No event in this batch forces a Release.

For a pending effect or lasting modifier, enable **Has Event Lifetime** inside its effect data. A blank boundary ID specifies run-end cleanup (applied modifiers also end when their recipient leaves); otherwise use the boundary name and positive count supported by that approved event handler. Immediate one-time effects can leave this disabled. Existing cards do not acquire a duration automatically.

An Event Effects enum choice requires a registered approved handler; the collector does not imply that every shared effect is executable as an event. Handler preflight validates its approved trigger, target, and duration before confirmation. Unsupported data leaves the Technique window open with a diagnostic. Expires After Use is legacy metadata and does not supply event duration.

Specify pending-effect/modifier durations explicitly. Discard resolved event cards; mode changes do not reset a target modifier. Run ending clears unfinished event effects and encounter trails. The controller now implements that resolution window; collector editing supplies authored data without changing its timing.

Verification: runtime and Editor sources compile against Unity 6000.5.9f1 assemblies, and 818 runtime adapter assertions pass. Unity GUI, Undo/Redo persistence, sprite previews, and disk round trips remain unverified. Run **Fishing Cards > Validation > Event Presentation and Authoring** for prefab/input and serialized authoring checks, or **All Event Integration Checks** for the combined suites plus captures at three supported resolutions. These tests use temporary fixture assets and do not edit production cards. See [validation evidence](Validation/EventIntegration/README.md).

## Approved coastal handlers

The five new cards resolve at OnEventResolution through CoastalEventHandlers. Reef Snag uses SnagHeaviestUnarmoredCreature and an explicit BiomeEnd lifetime with count 1. Silt Burst uses QueueCreatureEncounterConcealment with one charge; its FIFO concealment owner supplies the lifetime, so Has Event Lifetime remains disabled. Gentle Upwelling uses QueueBaitWeightReduction with a run-end lifetime (blank boundary). Loose Knot uses OptionalEventCreatureRelease without a duration descriptor. Fresh Shell Fragments uses ScheduleCreatureTrail, amount 3, referencing the existing Spiny Lobster; the trail owner supplies its biome-local duration.

Fresh Shell Fragments has runtime availability in addition to depth/pool constraints: no pending trail/active authored chain and a legal normal destination with Lobster eligibility. The runtime, not this editor list, filters that changing condition. A forced invalid copy safely creates no trail.

Event-specific illustration references are empty because no art was supplied. Existing event master artwork is retained. Use **Fishing Cards > Validation > Coastal Events** for the focused rules/asset suite, or **All Event Integration Checks** for combined regression and UI captures. See [coastal validation evidence](Validation/CoastalEvents/README.md).
