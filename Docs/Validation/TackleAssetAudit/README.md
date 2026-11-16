# Tackle artwork crop audit — 16 November 2026

Scope: all 100 PNGs installed under `Assets/UIThemes/TackleApparatus/Art`, including the 65 serialized PNG dependencies of the Discover, Landing and Build prefabs, plus dynamically selected creature illustrations and other installed exports. Historical themes and concept reference images are outside this audit.

The remaining composed-screen sources were `build-tray`, `build-tray-three` and `build-rail-card`. They now use the transparent page 08 insertion rail and page 05 ivory face, rope and clip exports. No newly generated art is needed. Sprite GUIDs, runtime dimensions, insertion targets and dynamic text fields are preserved.

The export pipeline now rejects image sources outside `AssetLibrary/Exports`. Every generated Build layer uses an independent approved asset. Discover and Landing imports already match their independent source exports byte for byte. Atlas extraction of an isolated transparent component is valid; cropping a composed screen with neighboring background pixels is the defect checked here.

Alpha was counted per pixel, rather than inferred from the PNG format. Of 100 files, 94 have fully transparent pixels. The six intentional exceptions are two full desk backdrops, the opaque seamless reel-paper material, the translucent Good/Perfect zone fills and the solid score-fill segment. None is an unresolved screenshot crop. The 20 Build masters derive only from independent exports.

**No crop without an available transparent Penpot replacement was found within this scope.** Existing supplemental tag art and the generic Landing indicator are separate approved sources, not screen crops.

Validation: 81 Build asset assertions, 155 Discover asset assertions, 96 Landing asset assertions and 33 live Build assertions, including all insertion-target raycasts, preview purity, placement commitment, automatic upgrades and phase separation. Updated captures remain in [TackleBuildArt](../TackleBuildArt/README.md). [Report.json](Report.json) records alpha counts, source-byte checks and replacements.
