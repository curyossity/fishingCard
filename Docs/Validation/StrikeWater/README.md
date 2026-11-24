# Strike-bar water level

Active presentation revision, 24 November 2026. [Watch weak and powerful native examples](Compare.html), or inspect the [fill and drain stages](Stages.png).

The timing glass fills from the bottom as each score delivery lands. Its target height is the running total of this strike divided by half the encountered creature's original Resistance, clamped to 0-100%. For example, 25 points against 100 Resistance fills half the glass, while 50 or more fills it completely. Earlier attempts are excluded. Additions use the new running subtotal and multipliers use the resulting total; earlier contributions are not added twice.

Each impact raises the water over 65 ms. The level is retained through every subsequent contributor's preparation and flip. Once the final score delivery lands, it has an 80 ms settling window, then drains smoothly to empty over exactly two-thirds of a second. The existing sequence, collision and audio timestamps are unchanged. Even ordinary no-rig feedback has enough time to finish the drain before completion. A zero-point Miss leaves the glass empty.

`LandingStrikeWater` is a separate native MaskableGraphic under the existing track RectMask2D. It is the first child, below both accuracy bands, the fish marker and existing contact overlays. A translucent blue-green gradient and restrained foam line follow a small surface wave; a two-pixel inset keeps water within the glass. Its 640-vertex bounded mesh creates no per-frame managed allocations. Fish/zone geometry, input intervals, scoring and the supplied artwork remain unchanged.

`LandingScoreRush` supplies the same unscaled elapsed time and immutable receipt Resistance used by the other presentation layers. Water sampling is stateless and reconstructs correctly on frame skips or seeking. Reduced motion uses immediate score-step height updates and a flat surface, while retaining the final drain. New receipts, skipped feedback, completion, independent disable and phase exit clear the glass. `LandingStrikeWaterSetup` retains the binding and layering through normal prefab regeneration.

The previously saved PixelFX collision splash and score punch remain as they were; this increment adds only the timing-glass water. All 42 accepted sound assets retain their hashes. No new texture, sound, reward, damage rule or player state is introduced.

**727 checks pass:** 196 native checks per fixture, 62 prefab/binding checks, 209 score/schedule checks and 64 Landing/model checks. New checks cover the 50% threshold, saturation with huge decimal values, no early fill, addition/multiplier accumulation without double counting, the halfway/empty drain states, the no-rig deadline, Miss/reduced-motion behavior, masking/layer order, mesh bounds and cleanup. Existing collision, score, audio, splash and interruption checks remain.

The real-definition fixtures remain 50.7 points in 3.647416 seconds and 12,199.2 points in 4.546624 seconds. Complete native 1920x1080 exports were checked before encoding: 237 weak and 291 powerful frames at 60 fps. Preview audio follows the runtime's frozen cue schedule. The isolated clones use no player-controller commands or save mutation. Unity reports zero errors and the isolated Play session is stopped. Project GUID/meta integrity covers 2,175 files with zero errors/warnings.
