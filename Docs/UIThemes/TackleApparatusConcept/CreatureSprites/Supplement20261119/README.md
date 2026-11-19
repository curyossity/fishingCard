# Missing-creature artwork supplement

19 November 2026. Added Acorn Barnacle, Peacock Worm and Sea Sponge using the built-in `image_gen` tool, one transparent illustration per call. Existing mussel and sea-anemone sprites were inspected for the library's coarse clusters, dark outlines, muted maritime palette, cream highlights and readable silhouettes. Existing creature and Apex artwork was preserved.

The complete prompts and original generated locations are recorded in [GenerationJobs.json](GenerationJobs.json). Selected originals are copied unchanged into the parent `Sprites/` directory under their stable species IDs. The active parent `Manifest.json` registers their hashes, dimensions and measured visible bounds; its original Penpot metadata continues to describe the earlier 22-image page. These three supplemental images have not been uploaded to Penpot.

Runtime files:

- `Assets/UIThemes/TackleApparatus/Art/Creatures/creature_acorn_barnacle.png`
- `Assets/UIThemes/TackleApparatus/Art/Creatures/creature_peacock_worm.png`
- `Assets/UIThemes/TackleApparatus/Art/Creatures/creature_sea_sponge.png`

Both illustration fields in each matching creature definition reference the imported sprite. Existing views consume those authoritative definition fields; no species-specific rendering branch is needed.

Generated originals contain faint stray pixels with alpha at or below 16/255. The importer records a per-source cutoff, removes only those samples, crops to the measured silhouette and adds its standard four-pixel transparent border. It preserves all remaining RGBA samples without resampling or recoloring. Legacy entries retain cutoff zero, and all 22 previous imported hashes are unchanged. Point filtering, uncompressed sprites, no mipmaps, aspect-fit and centered pivots match the existing library.

[Source validation](Validation.json) records original hashes and bounds. [Unity validation and native-size preview](../../../../Validation/CreatureSprites/Supplement20261119/README.md) cover every registered species through the actual rig, Landing, Journal central and compact renderers. All 23 standard species now have both illustrations assigned. Gameplay, abilities and Apex assets were not changed.

To reproduce source registration, install Pillow 13 in a local tool environment and run `register_sources.py` with access to the recorded originals. It copies originals without pixel edits and refuses to replace a different existing source. Then run `CreatureSpriteSetup.Import()` in Unity. The manifest records permit later imports directly from the workspace without needing the generated-image directory.
