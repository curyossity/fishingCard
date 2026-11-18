# Discover shared-shell variant — 18 November 2026

**Latest version:** [original-size wheels](FullSize/README.md) restores the original 320 × 365 reels inside the same outer silhouette and is currently selected. This page records the preserved compact variant.

The first phase now uses a complete 1739 × 904 housing with the same outer pixels and alpha as Landing and Build. All three draw at **490,163 / 1310 × 681** in the centered 1920 × 1080 stage. The two internal recesses change; the title plaque, crossmember, rim, shafts, vent, dial and silhouette stay fixed.

[Unity preview](../../../Validation/DiscoverIntegrated/Discover-1920x1080.png) · [Housing master](discover-machine.png) · [Exact generation prompt](Prompt.txt)

The built-in image editing tool created `Sources/discover-generated.png`. `NormalizeDiscoverHousing.cs` registers its interior at native 1:1 coordinates against the immutable original Build master and keeps all original alpha values. [Normalization.json](Normalization.json) records **zero alpha differences and zero protected visible exterior differences**, with 720,063 protected pixels. No API/CLI image generation was used.

The parchment wheels, symbols, captions, lock levers, lamps and buttons remain independent Unity elements. Each wheel assembly is uniformly scaled to 72% of its original native coordinates, preserving its cylinder projection, moving paper/dividers and elastic snap-back. The lower control beds belong to the integrated housing; there is no extra plaque background over each lock label. Slot rules and timing are unchanged.

## Restore the previous version

Outside Play Mode, with the comparison scene open:

- **Fishing Cards → Slot Machine → Housing → Restore Original Housing** restores the complete saved animated composition.
- **Fishing Cards → Slot Machine → Housing → Use Unified Housing (Compact Wheels)** installs this version again.

The functional original is `Assets/UIThemes/TackleApparatus/Prefabs/DiscoverCanvas-Original.prefab`. Its existing sprite and material sources are preserved. `Original/` also retains the previous prefab serialization and authoring script as text provenance. The original archive is never overwritten by either menu action.

The selected version is stored in `Assets/UIThemes/TackleApparatus/DiscoverHousingSelection.json`, so full Discover regeneration honors the choice. The active prefab keeps its GUID and controller binding. [Validation](../../../Validation/DiscoverIntegrated/README.md) includes a tested original → unified round trip.
