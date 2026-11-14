# Tackle Apparatus Landing artwork

The moving fish was subsequently replaced with a smaller generic icon. See [current indicator validation](../LandingIndicator/README.md); the captures below document the original artwork integration.

Integrated and verified 13 November 2026 in Unity 6000.5.9f1, in the MidnightFieldJournal comparison scene. This replaces the plain Landing presentation while preserving its gameplay profile, scoring, input commands and success/failure boundaries.

## Source and integration

The approved source is [the Penpot Landing asset page](https://design.penpot.app/?screen=workspace&team-id=9e056cfe-f83f-81f8-8008-cc4e858053b7&file-id=3681d377-bc58-8110-8008-e7a38adc2112&page-id=38e9e9ec-a6d5-8050-8008-e7b1dd7c6506), together with the Common assets and [saved assembly layout](../../UIThemes/TackleApparatusConcept/AssetLibrary/Layout.json). The Penpot connector returned “Server not initialized” during this integration, so the checked-in original exports and manifests supplied the artwork. No Penpot design was edited.

All six Landing exports were copied byte-for-byte to `Assets/UIThemes/TackleApparatus/Art/Landing/`. [ImportedAssets.json](ImportedAssets.json) records their original dimensions, SHA-256 hashes and Penpot board IDs. Each hash matches the original source manifest. No image generation, upscaling or replacement artwork was used.

The shared desk, cabinet, housing, plaques, controls, fonts and rig-card prefab are reused. Functional text, progress, attempts and outcomes remain independent runtime UI. The four supplied pixel portraits take priority in the encounter panel; species without one retain their existing authored creature illustration. Shared rig cards retain their existing four portrait mappings, showing real names/roles/values when an illustration is unavailable. No sample rig or illustrative haul is inserted into normal expeditions.

`TackleLandingArtSetup` replaces the contents of `Assets/LandingChallenge/LandingControls.prefab`, preserving GUID `8e54c158da9f3d544a68cbfb2c52107c`. The old colored track/zone/fish rectangles, plain panels, default-font labels and plain-controls generator are removed. Transparent native raycast surfaces and the aspect-ratio letterbox remain intentional UI infrastructure. Historical plain screenshots remain validation records, not runtime assets.

The larger fish body is decorative. Supplied carets and brackets mark the exact model interval; changing illustration size never changes overlap grading. Both region widths and all positions map directly from the normalized model. The original 1920 × 1080 stage uses Expand scaling and letterboxing on other aspect ratios.

Install/reinstall with **Fishing Cards > Landing Challenge > Integrate Approved Landing Art**. Repeated installation keeps the existing tuning and one Landing canvas. Scene/Harbor regeneration retains the artwork binding.

## Verification

| Check | Passed |
| --- | ---: |
| `TackleLandingArtChecks.ValidateAssets()` | 96 |
| `TackleLandingArtChecks.ValidateLiveArtwork()` | 45 |
| Existing Discover artwork checks | 155 |
| Landing rule checks | 64 |
| Slot-machine rule checks | 71 |
| Live Landing success | 16 |
| Live Landing failure/Harbor/restart | 30 |

Asset checks cover original pixels, sprite settings, art/font references and removal of visible unsourced rectangles. Live artwork checks cover the separate phase, exact model-to-track widths, eight ordered rig cards with supplied portraits, unclipped native labels, scrolling content, real Strike raycasts and press/disabled feedback. The success/failure fixtures verify attributed 50-point scoring, attempt updates, no early attachment, full prior Catch payout, zero Bait value, duplicate rejection, rig cleanup and empty-rig restart. All live fixtures disable progression writes.

Repeated installation preserved the prefab GUID and profile bytes and left exactly one Landing view. The Unity project metadata checker examined 1,584 files with zero errors/warnings. C# compiled successfully and the live console had zero errors/warnings. Code/document whitespace checks passed; Unity-authored YAML retains its standard empty-value formatting.

## Captures

- [1920 × 1080, ordered rig fixture](Landing-1920x1080.png)
- [1280 × 800](Landing-1280x800.png)
- [2560 × 1080](Landing-2560x1080.png)
- [Successful scoring / Build handoff](Landing-Succeeded.png)
- [Failure / full Catch payout](Landing-Failed.png)

The captures were visually reviewed. The success/failure captures preceded the added paired caret accents; their scoring, controls and composition are unchanged. The three resolution captures show the final carets. Playtesting is still needed for timing readability and balance. Build insertion remains a separate implementation; successful Landing still stops at its handoff.
