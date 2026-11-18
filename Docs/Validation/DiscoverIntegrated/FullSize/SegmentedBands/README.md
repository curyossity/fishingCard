# Reel motion alternative — segmented bands

18 November 2026. This second option uses eight beveled brass plates around each drum, with a small rivet between moving joints. Each plate follows the existing cylinder angle, compresses into the rounded ends and inherits the same elastic stop. The outer socket lip, lighting, reel dimensions, placement and curved paper mask are retained.

[Animated preview — middle reel locked](DrumMotion.gif) · [Compare both options](Comparison.html) · [Previous animation](../RotatingEdges/DrumMotion.gif)

The segmented version is currently selected. Outside Play Mode, use **Fishing Cards → Slot Machine → Reel Motion**:

- **Use Segmented Bands** selects this version.
- **Use Continuous Wear** restores the previous animation, whose shader branch and preview remain preserved.

`TackleReelAnimationSetup` saves the choice in `Assets/UIThemes/TackleApparatus/ReelAnimationSelection.json`. Both material and housing authoring reapply that choice. `TackleReelSurface` stores the selected enum in the prefab and forwards it to both private rendering layers. This changes presentation only, with no new raster artwork or gameplay rules.

Validation:

- Previous → segmented selection switching passed; all three prefab renderers retained the choice after material regeneration.
- **28 GPU band-motion checks**, **28 curved-clipping checks**, **40 live animation checks**.
- **19 housing, 9 aperture/binding, 12 surface and 155 existing art checks**.
- Four control raycasts and captures at 1920 × 1080, 1280 × 800 and 2560 × 1080.
- The 32-frame real respin recording has **zero changed samples on the locked middle reel**, with moving bands on the outer reels. See [frame verification](MotionPreviewVerification.json).

The GIF uses cropped display exports of the real fixed-step respin, including its normal recoil. Source artwork, collected creatures and progression are not changed by this alternative.
