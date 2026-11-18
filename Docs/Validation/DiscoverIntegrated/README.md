# Discover integrated housing validation — 18 November 2026

This records the compact version. The [original-size wheel variant](FullSize/README.md) is now selected; this composition and its verification remain preserved.

The unified first-phase variant is installed in the existing live Discover prefab. The original composition remains available through the Housing restore menu and its separately saved prefab. No gameplay rules, Landing art, Build art or animation tuning changed.

- **155 asset checks** for the original and unified versions, **12 reel-surface checks**.
- **15 shared-housing checks** independently decode all three actual runtime PNGs, compare their complete alpha and protected exterior pixels, compare their serialized stage bounds, and verify the retained original prefab and source bytes.
- Successful **original → unified** restoration round trip; unified remains selected.
- **37 live animation checks** cover initial spin, moving decoration/symbol synchronization, stationary locked wheel, single respin, stable final review, explicit acceptance and handoff to Landing.
- **Four real UI raycasts** reach all three lock controls and Accept without artwork obstruction.
- Visual review at **1920 × 1080**, **1280 × 800** and **2560 × 1080**.

[1080p preview](Discover-1920x1080.png) · [1280 × 800](Discover-1280x800.png) · [Ultrawide](Discover-2560x1080.png) · [Asset verification](AssetVerification.json) · [Source, prompt and rollback instructions](../../UIThemes/TackleApparatusConcept/DiscoverIntegrated/README.md)

The frame-driven animation capture set precedes the final removal of redundant lock-label plaque backgrounds. The final `Discover-*` captures include that cleanup; its four control raycasts were checked afterward. Captures use the real Harbor/Discover flow and selected tags. Unity was left outside Play Mode.
