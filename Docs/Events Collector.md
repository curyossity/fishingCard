# Events Collector

Open **Fishing Cards > Data > Events Collector**, or **Window > Fishing Cards > Events Collector** in Unity.

The list shows authored Hazard, Environment, Opportunity, and generic Encounter cards from Assets/Cards. Choose a category or search by name, ID, rules text, type, rarity, tags, or biome. Currently this includes Feeding Frenzy, Ghost Net, Murky Current, Rough Current, and Snagged Kelp. Creatures, Treasure, and Apex are available through the Creature Catalog.

Select a card to edit its identity, tags, artwork references and encounter-art transforms, event rules and highlighted phrases, biome/depth constraints, and effects. The artwork previews use the assigned sprites. Catch-only fields exposes the remaining shared CardDefinition fields for schema access; event designs have no Weight, sale Value, or Catch/Bait modes and never attach to the Catch Chain. Those fields do not grant event cards catch behavior.

Under **Actual Encounter Pool Membership**, check the biome depth tiers that should include the event. Biome IDs and minimum/maximum depth are eligibility constraints applied to pool membership; editing those constraints alone does not add the card to a tier. Use Locate to find the biome asset.

Edits use Unity serialized properties and support Undo/Redo. **Save Changes** writes dirty assets to disk. **Locate Asset** selects the card in the Project browser. The collector refreshes after asset changes; **Refresh** also reloads the inventory. Search, category, selection, and scroll positions survive editor reloads.

To author another event, create a Card Definition through Unity's existing **Create > Fishing Cards > Card Definition** asset menu under Assets/Cards, set its Card Type to Hazard, Environment, Opportunity, or Encounter, and edit it in this collector. Configure actual pool membership before expecting it in encounters.

## Event authoring rules

The canonical rules are in [GDD section 9](Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md#event-cards-and-resolution-window). Reveal reactions finish before consequences. The player can use legal Techniques afterward until confirming Descend or Surface, even after cancellation. Unless the card states otherwise, event consequences and resulting creature reactions resolve once before that action; no Technique window reopens until the action finishes, and resolution never adds an extra depth advance.

Distinguish full cancellation of a revealed Hazard (discard without effects; trigger Weathered Shell and other cancellation reactions) from replacement of an unresolved event (prevent effects; no cancellation reward) and no eligible target (resolve without effect; not cancelled). No event in this batch forces a Release.

Specify pending-effect/modifier durations explicitly. Discard resolved event cards; mode changes do not reset a target modifier. Run ending clears unfinished event effects and encounter trails. These are approved authoring requirements; collector editing does not itself implement the new runtime resolution window.

Verification: both collectors and their shared authoring code compile against Unity 6000.5.9f1 Editor assemblies. Asset inventory, serialized-field coverage, existing Creature Catalog entry points, and diff hygiene were checked. Interactive Unity GUI, Undo/Redo persistence, sprite previews, and save/reopen behavior were not executed in this environment.
