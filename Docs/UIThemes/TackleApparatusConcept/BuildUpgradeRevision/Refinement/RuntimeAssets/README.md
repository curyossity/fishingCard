# Runtime Build artwork masters

These are independent decorative exports for the 16 November 2026 Unity integration of Penpot pages 11/12 in file `3681d377-bc58-8110-8008-e7a38adc2112`. `Manifest.json` records dimensions, PNG hashes and the original page 04 composition hash. All artwork derives from the approved project assets; no external image API or new image generation was used.

`build-tray` and `build-tray-three` use the transparent page 08 insertion rail, fitted to the existing bounds with nine independent border/center regions. The three-card version adds page 05's separate transparent rope and clip exports at the existing card positions. Longer rigs use separate runtime rope/clip elements. `build-rail-card` uses the complete transparent page 05 ivory face. There are no composed-screen crops, erased creature fields, background patches or painted-out controls in these masters.

Insertion controls use the transparent approved AssetLibrary exports from Penpot page **08 - Rig Placement UI Assets**: `Placement/insertion-socket-idle.png`, `Placement/insertion-socket-selected.png` and `Placement/insertion-plus.png`, with `Common/caret-down.png` for the selection arrow. The exporter composites these on clear canvases at the existing runtime dimensions. It rejects any image source outside the independent `AssetLibrary/Exports` directory. Existing Unity sprite GUIDs, hit areas and layout remain preserved.

From the repository root:

1. Run `python Docs/UIThemes/TackleApparatusConcept/BuildUpgradeRevision/export_runtime_assets.py` to create the extraction commands.
2. Compile the parent `ConceptRenderer.cs` with System.Drawing and System.Web.Extensions, then run it with this directory and `AssetLibrary/Fonts` as its two arguments. Runtime scenes request transparent raster-only output.
3. Run the export script again to stamp final PNG hashes.
4. Run `RigBuildingSetup.InstallCurrentScene()` in Unity outside Play mode.

Unity imports the PNGs into `Assets/UIThemes/TackleApparatus/Art/RigBuilding/` without scaling or compression. Original concept images and layered SVGs remain in the parent refinement directory. See [validation](../../../../../Validation/TackleBuildArt/README.md) for native captures and gameplay checks.
