# Discover — original-size wheels, unified housing

18 November 2026. This additional version restores the original **320 × 365** stage-unit wheels at **100% scale**, with unchanged symbols, text, projection, moving paper and elastic stop. Their positions are registered to the new artwork's socket centers rather than copied from the original machine. It retains the shared **1739 × 904** housing silhouette at **490,163 / 1310 × 681** stage bounds.

The socket centers on the native housing are **(440,445), (860,445), (1290,445)**. The authoring setup maps those points through the housing bounds, then centers the unchanged reel rectangles on them. This corrects the first integration's spacing and vertical alignment without resizing the wheels or changing any artwork pixels.

[Unity preview](../../../../Validation/DiscoverIntegrated/FullSize/Discover-1920x1080.png) · [Housing master](discover-machine.png) · [Exact prompt](Prompt.txt)

The interior crossmember is removed to make a tall reel bay. The lock controls sit in the lower fascia. The external casing, title plaque, shafts, vent, dial and alpha remain protected. `Normalization.json` records **zero alpha differences and zero protected exterior color differences**, covering 626,910 protected pixels. The enlarged interior mask intentionally includes the old crossmember and lower inner trim; those are interior features, not the shared outside silhouette.

Created with the **built-in image editing tool**, followed by native-coordinate normalization using `NormalizeFullSizeDiscoverHousing.cs`. The raw generated source is retained in `Sources/`. The runtime PNG is imported byte-for-byte with Point filtering, no mipmaps and no compression.

## Versions

Under **Fishing Cards → Slot Machine → Housing**, outside Play Mode:

- **Use Unified Housing (Original Wheel Size)** selects this version.
- **Use Unified Housing (Compact Wheels)** selects the preceding 72% wheel version.
- **Restore Original Housing** restores the original machine and its full-size reels.

The active prefab keeps its GUID. The original and compact functional prefabs are preserved separately as `DiscoverCanvas-Original.prefab` and `DiscoverCanvas-Compact.prefab`; their masters are unchanged. The project selection JSON retains the version across Editor restarts and full Discover regeneration.

[Validation](../../../../Validation/DiscoverIntegrated/FullSize/README.md) compares every new reel's dimensions and scale against the original prefab and independently checks the actual housing pixels. Positions follow the new socket registration.
