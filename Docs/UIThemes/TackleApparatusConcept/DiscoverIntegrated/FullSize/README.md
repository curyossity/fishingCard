# Discover — original-size wheels, unified housing

18 November 2026. This additional version restores the original **320 × 365** stage-unit wheels at **100% scale**, with unchanged symbols, text, projection, moving paper and elastic stop. Their positions are registered to the new artwork's socket centers rather than copied from the original machine. It retains the shared **1739 × 904** housing silhouette at **490,163 / 1310 × 681** stage bounds.

The registered reel centers on the native housing are **(440,423), (860,423), (1290,423)**. The horizontal coordinates follow the socket columns; the common vertical baseline is raised to clear the central lamp fascia. The authoring setup maps those points through the housing bounds, then centers the unchanged reel rectangles on them. The complete middle reel rectangle has at least eight stage pixels of clearance above the raised fascia, with all three reel tops aligned. Wheel dimensions and artwork pixels are unchanged.

[Unity preview](../../../../Validation/DiscoverIntegrated/FullSize/CurvedAperture/Discover-1920x1080.png) · [Housing master](discover-machine.png) · [Exact prompt](Prompt.txt)

The moving reel contents now share a curved stencil opening traced from the original drum's inner edge. The supplied rim remains fixed behind that opening, so the moving paper and native symbols/captions disappear behind its narrower top/bottom corners. No raster asset was modified for this clipping correction. See [rendering verification](../../../../Validation/DiscoverIntegrated/FullSize/CurvedAperture/README.md).

The interior crossmember is removed to make a tall reel bay. The lock controls sit in the lower fascia. The external casing, title plaque, shafts, vent, dial and alpha remain protected. `Normalization.json` records **zero alpha differences and zero protected exterior color differences**, covering 626,910 protected pixels. The enlarged interior mask intentionally includes the old crossmember and lower inner trim; those are interior features, not the shared outside silhouette.

Created with the **built-in image editing tool**, followed by native-coordinate normalization using `NormalizeFullSizeDiscoverHousing.cs`. The raw generated source is retained in `Sources/`. The runtime PNG is imported byte-for-byte with Point filtering, no mipmaps and no compression.

## Versions

Under **Fishing Cards → Slot Machine → Housing**, outside Play Mode:

- **Use Unified Housing (Original Wheel Size)** selects this version.
- **Use Unified Housing (Compact Wheels)** selects the preceding 72% wheel version.
- **Restore Original Housing** restores the original machine and its full-size reels.

The active prefab keeps its GUID. The original and compact functional prefabs are preserved separately as `DiscoverCanvas-Original.prefab` and `DiscoverCanvas-Compact.prefab`; their masters are unchanged. The project selection JSON retains the version across Editor restarts and full Discover regeneration.

[Validation](../../../../Validation/DiscoverIntegrated/FullSize/README.md) compares every new reel's dimensions and scale against the original prefab and independently checks the actual housing pixels. Positions follow the new socket registration.
