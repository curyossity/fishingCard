# Page 04 recessed insertion rim

**Retired approach.** The user rejected this extraction because the rim and enlarged wood still overlaid an incompatible machine shell. It is historical provenance only and is not sampled by the active exporter or shipped as a runtime tray. The [integrated housing](../IntegratedMachine/README.md) replaces it.

The standalone rail on Penpot page 08 is a different component with its own green housing, vents and side fixtures. Fitting that rail to page 04's insertion area changed its proportions and overlaid a second housing on the machine. It is not an interchangeable replacement for the approved recessed brass rim.

No separate transparent export of this exact page 04 rim was available in the local Penpot handoff. `page04-insertion-rim.png` therefore isolates the original rim from `../../RigPlacement.png` at native scale. The original reference stays unchanged. `Manifest.json` records the source SHA-256, page ID, original crop bounds, outer silhouette, full interior cutout and resulting hash.

The 1135 × 269 transparent canvas retains only the recessed perimeter; surrounding casing and the entire printed interior are removed. The runtime exporter places it at its original 550,562 location with no stretching or nine-slicing. Blank wood, cards, ropes, clips, labels and insertion controls remain independent. The rim has no creature fields, buttons, background rectangle or side fixtures.

Run `../export_runtime_assets.py` to write the normalization commands, then render this directory before the RuntimeAssets directory using `../ConceptRenderer.cs` and the AssetLibrary fonts. Run the exporter once more to stamp hashes. `RigBuildingArtChecks` verifies dimensions, the transparent center/outside and sampled pixels against the original reference at the corresponding coordinates.
