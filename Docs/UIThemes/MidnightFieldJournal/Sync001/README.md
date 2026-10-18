# Penpot to Unity synchronization — MFJ-SYNC-001

All changes recorded since the previous Unity integration are imported and verified in the isolated Midnight Field Journal candidate theme: **MFJ-R001 through MFJ-R019**. This includes the pointer-hover changes, the detailed main-card frame/layout and all eighteen refactored creature illustrations.

Open `Assets/UIThemes/MidnightFieldJournal/Scenes/MidnightFieldJournalComparison.unity` to play the candidate. The original scene and build selection remain unchanged.

[Representative assembled 4K HUD](Gameplay4K.png) · [Actual 4K Play Mode output](ActualGameplay3840x2160.png) · [1080p](ActualGameplay1920x1080.png) · [Ultrawide](ActualGameplay3440x1440.png) · [16:10](ActualGameplay1920x1200.png).

## Imported design

- One revised blank frame, retaining the existing `CreatureTemplate` GUID; eighteen independent portraits under `Assets/UIThemes/MidnightFieldJournal/Art/Creatures/CentralV3/`. The other nineteen original catalog images remain byte-identical.
- A 1260 × 1400 main card at `(1250,110)` on the 3840 × 2160 gameplay composition. All names, resolved stats, tags, rarity names and ability texts remain native runtime fields. Effects remain governed by existing card data, including empty Catch panels.
- Native independent hook, twin-spark and rarity-diamond meshes; selected-role warm edges remain separate from the artwork. The old cyan selection wash is removed from this candidate card to match Penpot.
- Per-creature portrait and text geometry from the saved Penpot revisions. These mappings override old-design central offsets without touching shared CardDefinitions or approved compact Catch Chain portraits. Long existing Apex descriptions use the full available description area when no separate ability heading exists.
- Pointer hover adds no frame to Descend/Surface, Techniques or compact cards. Hover motion/inspection, keyboard/controller focus, Bait styling, selection and legal-target feedback retain their purposes. Shared views opt into the candidate behavior; original defaults remain enabled.

`SourceAssetCatalog.json` is the canonical regeneration source. `JournalDesignSync` binds the separate central mappings, and the normal asset/theme and prefab builders retain them on regeneration. No sliced mockup, flattened runtime card face or SVG package dependency was introduced.

## Verification

[Verification.json](Verification.json) records **1,182 assertions** across the eighteen-card layout/hover suite, all thirty encounter definitions, HUD and scene checks, actual role/action input, controller flow, Surface/results/restart and output checks. Four actual Editor output sizes pass: 3840 × 2160, 1920 × 1080, 3440 × 1440 and 1920 × 1200. Hand projection performed zero extra renders during each 120-frame idle sample. This is Editor evidence; it does not certify a standalone player build.

[Penpot reconciliation](PenpotReconciliation.json) checks eighteen review cards and both assembled central-card instances against the recorded fields and geometry. The raw historical comparison is retained separately because two older assembly records contained incorrectly decoded punctuation; normalizing those stored strings reveals no live design differences. No Penpot visual content was changed during reconciliation.

All 820 protected files were checked. Only the two explicitly allowed shared presentation scripts changed outside the candidate theme. Original scenes, build settings, gameplay/card assets and old fonts remain byte-identical to the pre-import snapshot. Existing asset GUIDs are preserved. No legacy assets were deleted.

The immutable original [UnityDesignBaseline.json](../UnityDesignBaseline.json) remains intact. [UnityCheckpoint.json](UnityCheckpoint.json) is the next synchronization reference, with exact hashes/GUIDs and the current source catalog. Each revision's ledger entry points here, so it is no longer pending import.

## Source quality and remaining art

The frame source is 1190 × 1322, enlarged by approximately 6% to its approved 1260 × 1400 footprint. Portraits are 1672 × 941 and displayed smaller than their sources. Native text and vector emblems scale independently. The game canvas and captures are 4K; these raster sources are not described as native 4K images.

The existing low-opacity kelp backdrop remains 1672 × 941 over native 4K paper. Outside the completed refactored set, Flounder, Cuttlefish, special encounters and four Techniques still lack selected central illustrations. Compact artwork coverage is a separate inventory: the four approved compact portraits were deliberately preserved, and the new central mappings do not silently replace them. See [MissingVisualAssets.md](../MissingVisualAssets.md).

The prior local Windows build-data blocker was not retried in this visual synchronization. No paid service or new external dependency was added.
