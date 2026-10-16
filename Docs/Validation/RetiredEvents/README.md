# Retired event removal

Removed the old Feeding Frenzy, Ghost Net, Murky Current, Rough Current, and Snagged Kelp event assets and metadata. Removed their entries from all Coastal Waters depth-tier pools and deleted the Feeding Signs and Ghost Net Salvage chain assets that used those old events. The Calculated Overload debug scenario now references Gentle Upwelling, which is valid at its depth 5 and does not immediately change the scenario's existing load.

The only remaining event definitions are Reef Snag, Silt Burst, Gentle Upwelling, Loose Knot, and Fresh Shell Fragments. Shallows and Mid-depth include all five subject to each card's depth/availability rules; Deep Edge includes Reef Snag, Silt Burst, and Loose Knot. Generic chain runtime support, creatures, treasure, Apex cards, and Sardine's separate Feeding Frenzy Bait ability remain intact.

Executed audits verify that every remaining pool card reference resolves, all active event IDs match the approved set, and no serialized asset, scene, prefab, or metadata file under Assets references any of the seven retired GUIDs. See [removal audit](removal-audit.json) and [pool audit](active-pool-audit.json). `git diff --check` passes. This asset/document-only change was not exercised in Unity PlayMode; an already-running encounter may retain cached presentation until a new run starts.
