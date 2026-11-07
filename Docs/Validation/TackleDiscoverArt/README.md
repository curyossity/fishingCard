# Discover artwork integration — validated

The approved 37 Common/Discover sprites, original font sources and font licenses are copied into `Assets/UIThemes/TackleApparatus/`. [CopiedAssets.json](CopiedAssets.json) confirms every sprite's dimensions and SHA-256 match the original handoff. No image was regenerated, upscaled or flattened with runtime text.

`TackleDiscoverArtSetup` installs the full 1920 × 1080 native composition and reusable scrollable rig-card prefab in `Assets/UIThemes/MidnightFieldJournal/Scenes/MidnightFieldJournalComparison.unity`. The slot view forwards its model state to `TackleDiscoverPresentation`, which displays the cabinet, live haul/biome, reels, one lock, match lamps and supplied control artwork. Pointer/keyboard focus, disabled dimming and four-pixel press feedback are separate native UI behavior. Six missing tag illustrations and unprovided creature portraits retain explicit text/empty-portrait fallbacks; unrelated artwork is not substituted.

Validated 7 November 2026 in Unity 6000.5.9f1:

- `TackleDiscoverArtChecks.ValidateAssets()`: 151 checks passed for original sprite bytes, import settings, fonts, native hierarchy and serialized bindings.
- `SlotMachineRuleChecks.ValidateRules()`: 67 deterministic rule checks passed.
- `SlotMachineSceneChecks.ValidateLive(false, ...)`: 50 live acceptance checks passed.
- `SlotMachineSceneChecks.ValidateLive(true, ...)`: 56 live respin checks passed on the final run.
- `TackleDiscoverArtChecks.ValidateLiveArtwork()`: 16 checks passed for actual UI raycast targets, keyboard focus, four-pixel press/restoration, disabled dimming, lever artwork, role-aware haul values, eight-card scrolling and stable physical-card reuse after removal.
- Reinstalling the standard slot setup retained the artwork binding and exactly one artwork canvas; it did not duplicate the screen.
- Final fresh Play Mode runs and captures reported no Unity errors or warnings. Existing obsolete-API compiler warnings in unrelated validation scripts remain outside this change.
- The final Unity GUID/metadata check reported zero errors and warnings; code/document diff checks passed.

Actual Unity captures were visually reviewed at [1920 × 1080](Discover-1920x1080.png), [1280 × 800](Discover-1280x800.png) and [2560 × 1080](Discover-2560x1080.png). The 16:9 composition stays centered with letterboxing at other aspect ratios. [Locked control](Locked1080.png) and [three-card presentation fixture](RigFixture1080.png) show independent lever/card/portrait/text layers. The three-card fixture uses isolated display data and never adds creatures to the player's rig. Normal new expeditions show an empty cabinet and zero haul.

The capture helper uses URP's supported `SingleCameraRequest` submission path, with `Camera.Render` reserved for the built-in pipeline. All captures retain runtime text and controls as separate objects; no preview screenshot is used as a game background.

The artwork canvas is visible only during Discover and hides at the landing handoff. The existing Landing/Build implementation boundary is unchanged: no creature is acquired just by accepting or respinning. Harbor preparation and the other phase artwork remain separate. Install menu: **Fishing Cards > Slot Machine > Integrate Approved Discover Art**. Original sources, resolution, saved progression and unrelated scene objects remain preserved.
