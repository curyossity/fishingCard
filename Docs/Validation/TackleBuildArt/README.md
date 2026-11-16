# Build / Upgrade artwork integration — 16 November 2026

The saved comparison scene uses the approved Penpot composition from pages 11/12, including page 04's original insertion furniture. `RigBuildingSetup.InstallCurrentScene()` imports and installs the layout without rebuilding Discover or Landing.

## Integration

- One details frame with live level, accumulated species sale value and a concise complete effect.
- Level 1 legal insertion previews and a native Place & Continue button.
- Duplicate upgrades commit automatically and briefly show the prior/resulting levels, +1, the resulting effect/value and the existing highlighted position. Discover and Landing are hidden while this receipt is shown; no additional confirmation is required.
- Portraits come from creature definitions. Artwork contains no baked creature identity, sale value, level, effect text or legal-placement decisions.
- Pooled cards, rope and insertion controls preserve unlimited rig growth through vertical/horizontal scrolling. The three-card example preserves the approved original rope/clip spacing. Keyboard/gamepad gap navigation shares the same guarded command as pointer/touch input.
- Native glyph ink aligns with the approved reference geometry. The 1920 × 1080 stage scales to fit other aspect ratios, retaining the complete composition.

## Verification

Final results: 63 Build artwork/binding assertions, 76 rig rules, 64 Landing rules, 71 slot rules, 155 Discover-art checks, 96 Landing-art checks, 33 new artwork/live-flow checks and 100 full-loop checks. Unity metadata validation found zero errors or warnings. Counts are recorded in the adjacent JSON results and the full-loop validation directory.

`RigBuildingArtChecks.ValidateAssets()` checks independent source bytes/import settings, one-frame composition, native bindings, resulting-level text and the unassigned-content boundary. `RigBuildingChecks.ValidateRules()` checks acquisition, upgrades, transactional previews, positional effects, scoring, value, stale commands and overflow.

`RigBuildingArtChecks.Begin()` exercises the real Harbor → Discover → Landing → Build flow with persistence disabled. The captures use disposable Sardine ability authoring (0.2 plus 0.2 per upgrade); real creature definitions remain untouched. The Level 1 fixture has Mackerel, Glass Minnow and Sea Bass already attached; the Level 3 fixture begins with Level 2 Sardine, Mackerel and Glass Minnow. It verifies native target raycasts, preview purity, stale-input rejection, full failure payout, resulting-level/value/ability labels, separate visible phases and automatic upgrade continuation. Results are recorded in `LiveVerification.json`.

- `Level1-1920x1080.png`: native Level 1 presentation.
- `Level3-1920x1080.png`: native Level 3 presentation.
- `Level3-1280x800.png` and `Level3-2560x1080.png`: aspect-ratio checks.
- `UpgradeGameView.png`: actual Game view capture, including screen-space overlay UI.

Offscreen captures use a disposable native canvas copy so changing a capture camera cannot leave stale clipping rectangles on the live scene. The original Game view remains authoritative for visual inspection.

After the successful live checks, Unity emitted two internal EditMode lifecycle scope messages while leaving Play mode. Their stacks are confined to Unity's editor lifecycle; no project-code exception was reported during gameplay verification. The editor is left outside Play mode.

## Source and content boundaries

Sources, independent PNGs, dimensions, extraction commands and Penpot page IDs are under `Docs/UIThemes/TackleApparatusConcept/BuildUpgradeRevision/Refinement/RuntimeAssets/`. The extraction retains original artwork and removes printed concept data before runtime import. `export_runtime_assets.py` plus `ConceptRenderer.cs` reproduce the decorative masters. Filtering is Point, compression disabled, mipmaps disabled, FullRect sprites and native dimensions preserved.

Illustrative mockup coefficients are not newly assigned gameplay content. Unassigned creatures display "No active ability." Authored supported effects use their actual current-level amounts and conditions. This presentation work does not change creature eligibility, rarity, Landing tuning, expedition settlement or permanent rewards.
