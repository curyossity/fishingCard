# Runtime Build artwork masters

These are decorative exports for the Build UI using the information layout of Penpot pages 11/12 in file `3681d377-bc58-8110-8008-e7a38adc2112`. The 17 November correction includes a newly authored integrated machine housing made with built-in image editing and the supplied references. `Manifest.json` records dimensions, hashes and provenance. No external image API was used.

`build-machine` is the complete [framed integrated housing v2](../../IntegratedMachine/FramesV2/README.md). Its creature and effects frames match Landing in placement and border pixels; the effects well omits the score progress trough. The lower wooden rig and outer casing retain v1 pixels. The 1739 × 904 source PNG is copied byte-for-byte and drawn proportionally at 1310 × 681. The active manifest/Unity art have no `build-tray`, `build-tray-three`, `build-portrait` or `build-effect` overlays. Cards, ropes, clips, portraits and text remain native independent elements. `build-rail-card` uses the transparent page 05 ivory face.

Insertion controls use the transparent approved AssetLibrary exports from Penpot page **08 - Rig Placement UI Assets**: `Placement/insertion-socket-idle.png`, `Placement/insertion-socket-selected.png` and `Placement/insertion-plus.png`, with `Common/caret-down.png` for the selection arrow. The exporter composites these on clear canvases. Its image-layer allowlist accepts only independent `AssetLibrary/Exports` artwork; the housing uses an explicit whole-file `copySource`. Existing interactive hit areas are preserved and their rail viewport fits the new wood opening.

From the repository root:

1. Run `python Docs/UIThemes/TackleApparatusConcept/BuildUpgradeRevision/export_runtime_assets.py` to create runtime composition commands.
2. Compile the parent `ConceptRenderer.cs` with System.Drawing and System.Web.Extensions. Run it with this RuntimeAssets directory and `AssetLibrary/Fonts` as its two arguments. Raster-only scenes preserve transparency; the integrated housing is copied directly.
3. Run the export script again to stamp final PNG hashes.
4. Run `RigBuildingSetup.InstallCurrentScene()` in Unity outside Play mode.

Unity imports the PNGs into `Assets/UIThemes/TackleApparatus/Art/RigBuilding/` without scaling or compression. Original concept images and layered SVGs remain in the parent refinement directory. See [validation](../../../../../Validation/TackleBuildArt/README.md) for native captures and gameplay checks.
