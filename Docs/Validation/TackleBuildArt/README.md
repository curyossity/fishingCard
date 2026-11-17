# Build / Upgrade artwork integration — 16 November 2026

**Current implementation: framed v2, 17 November 2026.** The [integrated Build housing](../../UIThemes/TackleApparatusConcept/BuildUpgradeRevision/IntegratedMachine/FramesV2/README.md) contains Landing's exact creature frame and effects-panel border at matching native positions and dimensions. The effects well has no progress trough. The wooden rig, casing and alpha are unchanged. Separate `build-portrait`/`build-effect` overlays are removed from the prefab, manifest and runtime art. Native portraits, tags, level, sale value and current-level effect text occupy the integrated frames. The insertion heading is below their borders. Existing cards, ropes, clips and interactive controls retain their layout.

Current validation: **83 Build asset assertions, 40 artwork/live assertions, 100 full-loop assertions**, plus passing Discover/Landing asset checks. Metadata validation reports **0 errors and 0 warnings** across 1721 files. `FramedHousingVerification.json` records the result. Pixel checks verify both shared frame borders/positions, an unchanged creature frame and unchanged artwork outside the upper working region. The Landing PNG remains byte-identical. Current captures cover empty/populated placement, upgrades, long rigs and multiple aspect ratios.

## Integration

- One details frame with live level, accumulated species sale value and a concise complete effect.
- Level 1 legal insertion previews and a native Place & Continue button.
- Duplicate upgrades commit automatically and briefly show the prior/resulting levels, +1, the resulting effect/value and the existing highlighted position. Discover and Landing are hidden while this receipt is shown; no additional confirmation is required.
- Portraits come from creature definitions. Artwork contains no baked creature identity, sale value, level, effect text or legal-placement decisions.
- Pooled cards, rope and insertion controls preserve unlimited rig growth through vertical/horizontal scrolling. The three-card example composes transparent rope/clip masters at the existing positions. Keyboard/gamepad gap navigation shares the same guarded command as pointer/touch input.
- Native glyph ink aligns with the approved reference geometry. The 1920 × 1080 stage scales to fit other aspect ratios, retaining the complete composition.

## Verification history

Initial integration results: 63 Build artwork/binding assertions, 76 rig rules, 64 Landing rules, 71 slot rules, 155 Discover-art checks, 96 Landing-art checks, 33 new artwork/live-flow checks and 100 full-loop checks. Unity metadata validation found zero errors or warnings. Counts are recorded in the adjacent JSON results and the full-loop validation directory.

The insertion-transparency correction replaces the opaque screen crops with the approved page 08 socket/plus exports and common selection arrow, preserves their dimensions and sprite GUIDs, clears the wood patches beneath the controls and masks the hanging card's outside corners. The primary/upgrade button artwork already has alpha. Updated validation passes **75 asset assertions** (including alpha/corner checks on both insertion states, the arrow and card) and **33 live-flow assertions**. Updated native captures show the corrected controls. `TransparencyVerification.json` records the additional checks; gameplay and hit areas are unchanged.

The initial [crop audit](../TackleAssetAudit/README.md) substituted a transparent page 08 rail and passed 81 asset/33 live assertions, but the user rejected its shape and fit. The subsequent isolated page 04 rim passed 86 asset/40 live assertions but still overlaid an incompatible shell and enlarged wood patch. Those checks established transparency and source matching, not a correct integrated assembly. [TrayFrame](../../UIThemes/TackleApparatusConcept/BuildUpgradeRevision/TrayFrame/README.md) and `TrayFitVerification.json` are retired evidence. The integrated housing described at the top replaces both attempts.

`RigBuildingArtChecks.ValidateAssets()` checks independent source bytes/import settings, one-frame composition, native bindings, resulting-level text and the unassigned-content boundary. `RigBuildingChecks.ValidateRules()` checks acquisition, upgrades, transactional previews, positional effects, scoring, value, stale commands and overflow.

`RigBuildingArtChecks.Begin()` exercises the real Harbor → Discover → Landing → Build flow with persistence disabled. The captures use disposable Sardine ability authoring (0.2 plus 0.2 per upgrade); real creature definitions remain untouched. The Level 1 fixture has Mackerel, Glass Minnow and Sea Bass already attached; the Level 3 fixture begins with Level 2 Sardine, Mackerel and Glass Minnow. It verifies native target raycasts, preview purity, stale-input rejection, full failure payout, resulting-level/value/ability labels, separate visible phases and automatic upgrade continuation. Results are recorded in `LiveVerification.json`.

- `Level1-1920x1080.png`: native Level 1 presentation.
- `EmptyRig-1920x1080.png`: first successful landing, empty rig and one live insertion target.
- `Level3-1920x1080.png`: native Level 3 presentation.
- `Level3-1280x800.png` and `Level3-2560x1080.png`: aspect-ratio checks.
- `UpgradeGameView.png`: actual Game view capture, including screen-space overlay UI.

Offscreen captures use a disposable native canvas copy so changing a capture camera cannot leave stale clipping rectangles on the live scene. The original Game view remains authoritative for visual inspection.

After the successful live checks, Unity emitted two internal EditMode lifecycle scope messages while leaving Play mode. Their stacks are confined to Unity's editor lifecycle; no project-code exception was reported during gameplay verification. The editor is left outside Play mode.

## Source and content boundaries

Sources, dimensions, composition commands and page IDs are under `Docs/UIThemes/TackleApparatusConcept/BuildUpgradeRevision/Refinement/RuntimeAssets/`. The exporter uses independent artwork and a whole-file copy of `IntegratedMachine/integrated-build-machine-v2.png`. `FramesV2/ComposeBuildFrames.cs` derives that version from frozen native references and the blank effects-well edit. The retired TrayFrame stage is not run. `export_runtime_assets.py` plus `ConceptRenderer.cs` reproduce delivery. Filtering is Point, compression/mipmaps are disabled and FullRect sprites retain native dimensions. The complete 1739 × 904 machine draws at 1310 × 681 without nonuniform scaling.

Illustrative mockup coefficients are not newly assigned gameplay content. Unassigned creatures display "No active ability." Authored supported effects use their actual current-level amounts and conditions. This presentation work does not change creature eligibility, rarity, Landing tuning, expedition settlement or permanent rewards.
