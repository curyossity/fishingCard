# Rarity ceiling selection — MFJ-SYNC-008.2

The stopped zone is now the maximum creature rarity. Common permits Common; Uncommon permits Common/Uncommon; Rare permits Common/Uncommon/Rare; Legendary permits all four.

Select uniformly from distinct biome/depth-eligible species in that combined pool: every species gets one equal chance. There is no intermediate rarity roll, species weighting, rarity weighting or immediate-repeat exclusion in this timed species draw. Repeated pool references or aliases with the same stable species ID cannot increase that species' probability. The timed selection API no longer receives or stores weights.

The existing encounter category/event pacing stays separate. Fixed queued choices, promised trails, authored follow-ups and scheduled Apex encounters keep their guaranteed identity. Events still bypass timing; concealment and reveal reactions still wait for Stop and the feedback hold. Bar widths remain Common 87 / Uncommon 5 / Rare 8, including the existing unavailable-zone policy. The result heading now says **UP TO [rarity]**, and the instruction explicitly describes a maximum rarity.

Validation: 75 timing assertions, including exhaustive controllable-index draws at all four ceilings, uniform species counts, duplicate identities, history, biome/depth eligibility, promises, concealment and Hazard bypass. All eight existing gameplay suites pass (558 assertions). The real comparison-scene Play Mode smoke passes 22 assertions covering Stop raycasting, explicit Catch/Bait choice, next encounter, Surface/restart and event bypass. [Production report](ProductionValidation.json), [stopped result](Stopped4K.png), [current checkpoint](UnityCheckpoint.json).

This is a Unity gameplay/rules update to the integrated timing bar. No new Penpot assets or pending import. Earlier checkpoints remain historical evidence.
