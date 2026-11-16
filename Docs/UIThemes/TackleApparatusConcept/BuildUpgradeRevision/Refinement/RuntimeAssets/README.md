# Runtime Build artwork masters

These are independent decorative exports for the 16 November 2026 Unity integration of Penpot pages 11/12 in file `3681d377-bc58-8110-8008-e7a38adc2112`. `Manifest.json` records dimensions, PNG hashes and the original page 04 composition hash. All artwork derives from the approved project assets; no external image API or new image generation was used.

`build-tray-three` retains the original three-card furniture and clears the printed creature fields/control locations. Independent native masters and controls cover those cleared locations. The general `build-tray` uses the same perimeter and approved clean wood for uncapped scrolling rigs, with separate rope and clip elements. The card master contains no name, creature, level, sale value or effect.

From the repository root:

1. Run `python Docs/UIThemes/TackleApparatusConcept/BuildUpgradeRevision/export_runtime_assets.py` to create the extraction commands.
2. Compile the parent `ConceptRenderer.cs` with System.Drawing and System.Web.Extensions, then run it with this directory and `AssetLibrary/Fonts` as its two arguments. Runtime scenes request transparent raster-only output.
3. Run the export script again to stamp final PNG hashes.
4. Run `RigBuildingSetup.InstallCurrentScene()` in Unity outside Play mode.

Unity imports the PNGs into `Assets/UIThemes/TackleApparatus/Art/RigBuilding/` without scaling or compression. Original concept images and layered SVGs remain in the parent refinement directory. See [validation](../../../../../Validation/TackleBuildArt/README.md) for native captures and gameplay checks.
