# Rotating reel edge bands

## Opacity-support correction

The first implementation's opaque-only cutoff excluded most supplied metal texels (alpha 251/255 or 252/255), leaving the frame visibly static except for a few seams. The shader now tests support inside the eroded silhouette instead of requiring full opacity. It also samples the material continuously in one direction around the circumference, without mirrored reversal.

The GPU motion check now requires broad band coverage rather than a few changing seam pixels. Current results show **179–183 changed samples per band**, versus roughly 8–15 previously. The 32-frame real locked-respin capture independently verifies **zero changed samples on the locked middle reel** and **2,697 changed samples on each sampled rotating band**. The existing 40 animation checks and 28 clipping checks pass with the correction.

[Animated motion preview — middle reel locked](DrumMotion.gif) · [Frame-sequence verification](MotionPreviewVerification.json)

`ReelEdgeMotionChecks.CaptureRespinFrames` captures the real respin at fixed 0.05-second visual steps, including its elastic stop. The GIF is a cropped/downscaled display export; native source captures and game artwork are unchanged.

18 November 2026. Full-size Discover wheels now animate the drum's edge material, wear and bevel seams with the same cylindrical phase as the paper and symbols. The housing, curved aperture, outer alpha silhouette, thin socket lip and lighting stay fixed. Reel dimensions, positions, elastic stop and gameplay rules are unchanged.

`TackleReelSurface` owns a separate private material for the outer drum layer. Both layers use the same phase/radius; stencil-derived paper materials receive those values too. `ReelSurface.shader` samples narrow material strips from the unchanged original drum image along the rotating circumference, adds a fine bevel at the existing divider angles, and retains the source alpha/outer translucent fringe. There is no time-driven animation independent of the wheel, so locking freezes the edge bands and stopping naturally includes the existing recoil.

The full-size aperture authoring migrates the earlier `Fixed Reel Rim` layer into `Reel Edge Bands`. Compact/original restoration retains its previous rendering and clears the optional edge binding. Source artwork is unchanged.

Validation:

- **28 GPU edge-motion checks:** both visible bands change at two angles on every reel, with clear moving seam contrast; six sampled outer-lip points per reel remain unchanged. [Measurements](EdgeMotionVerification.json).
- **28 curved-clipping checks** and **9 aperture/binding checks** passed.
- **40 live animation checks** passed, including synchronized paper/symbol/edge phases, stationary locked bands during respin, final review and explicit acceptance.
- **19 housing, 155 existing artwork and 12 reel-surface checks** passed; compact → full-size switching passed.
- **Four control raycasts**, plus captures at 1920 × 1080, 1280 × 800 and 2560 × 1080.

[Motion preview](DrumMotion.gif) · [Current Unity preview](Discover-1920x1080.png) · [1280 × 800](Discover-1280x800.png) · [Ultrawide](Discover-2560x1080.png) · [Angle A](Edges-A.png) · [Angle B](Edges-B.png)

`Edges-A/B` isolate material rotation for pixel comparisons, then restore the original phase. The native faces remain at their actual resolved state in those diagnostic captures. Normal `Discover-*` captures and the live animation checks use the real encounter presentation.
