# Supplemental creature artwork validation

19 November 2026. Added and assigned missing artwork for Acorn Barnacle, Peacock Worm and Sea Sponge. All 23 standard creatures now have both artwork fields assigned; the two existing Apex illustrations were preserved.

- 300 Unity checks passed across all 25 registered species: source hashes, crop dimensions, import settings, native-size rig names/portraits, Landing portraits, Journal central cards and compact portraits.
- The review sheet grows to five rows and uses current Level 1 rig cards. [View the native-size card review](RigRoster.png).
- The three imports have true alpha transparency, four clear border pixels, Point filtering, uncompressed pixels, no mipmaps and centered aspect-fit portraits.
- Alpha samples at or below 16/255 are cleared only for these three recorded sources, removing faint generated background noise. All remaining source colors are preserved without resampling. Original generated PNGs remain unchanged.
- All 22 previous imported sprite hashes are unchanged. Species gameplay data and the generic moving Landing indicator remain unchanged.
- Unity project integrity scan: 1,774 files checked, zero errors or warnings. Unity Console had no compilation errors.

[Machine-readable Unity result](Verification.json), [source transparency and bounds](../../../UIThemes/TackleApparatusConcept/CreatureSprites/Supplement20261119/Validation.json), [generation prompts and provenance](../../../UIThemes/TackleApparatusConcept/CreatureSprites/Supplement20261119/GenerationJobs.json).

Reproduce in Unity outside Play Mode with `CreatureSpriteSetup.Import()` followed by `CreatureSpriteChecks.ValidateTo("Docs/Validation/CreatureSprites/Supplement20261119/")`. Both are editor authoring/validation paths; no player save or expedition state is changed.
