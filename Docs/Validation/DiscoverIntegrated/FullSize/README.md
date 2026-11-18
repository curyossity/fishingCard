# Original-size Discover wheels — validation

**Current rendering:** [curved aperture validation and previews](CurvedAperture/README.md) replaces rectangular clipping while retaining the registered positions and original reel size. The parent-folder captures below record the earlier rendering.

18 November 2026. The new version is installed and selected in Unity. Earlier original and compact variants remain preserved.

- **19 housing assertions:** identical alpha/protected exterior and stage bounds across the three phases, original archive/source preservation, all three wheels matching the original prefab's exact dimensions and scale, and at least eight stage pixels between the complete middle reel rectangle and raised lamp fascia.
- **155 existing art assertions**, **12 reel-surface assertions**.
- **37 live animation assertions:** initial spin, moving paper and symbols, locked wheel during respin, final review, explicit Accept and handoff to Landing.
- **Four UI raycasts** reach the three locks and Accept.
- Successful compact → original-size version switch. The compact archive retains its 72% scale.
- Captured at 1920 × 1080, 1280 × 800 and 2560 × 1080. Unity was left outside Play Mode.

**Socket alignment correction:** reels now use the native artwork's measured socket centers instead of the old machine's screen positions. The housing/reel source images and original wheel size are unchanged. All 18 housing, 155 art and 12 reel-surface assertions passed again, along with four control raycasts; `Discover-*` captures show the corrected layout. The frame-driven animation captures record the earlier placement.

**Raised-fascia clearance correction:** all three reels share a lifted baseline at native Y=423. Their horizontal registration and 320 × 365 size remain unchanged. The new clearance assertion passed, bringing the housing total to 19. Art/surface checks and four control raycasts passed again; `Discover-*` and `AlignmentVerification.json` now reflect this final baseline.

[1080p](Discover-1920x1080.png) · [1280 × 800](Discover-1280x800.png) · [Ultrawide](Discover-2560x1080.png) · [Asset verification](AssetVerification.json) · [Source, prompt and version menus](../../../UIThemes/TackleApparatusConcept/DiscoverIntegrated/FullSize/README.md)
