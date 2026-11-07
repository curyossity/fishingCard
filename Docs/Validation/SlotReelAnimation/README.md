# Cylindrical Discover reel animation

Validated 7 November 2026 in Unity 6000.5.9f1, using the installed `MidnightFieldJournalComparison` scene and the approved Tackle Apparatus Discover prefab. Original PNG exports, fonts and other screen themes are preserved.

Initial spin animates all three wheels. Respin animates only the two unlocked wheels. Sprite triangles and native TMP glyphs use the same cylindrical projection: full size at the center, vertically compressed and shaded near the edges, transparent on the back. Housing and existing drum shading remain fixed. Stop durations are 1.05 / 1.20 / 1.35 seconds for the initial spin and 1.05 / 1.20 seconds for the two moving respin wheels, including a small settle. Neighbor faces fade at startup/rest to avoid a visual pop.

The gameplay RNG resolves each roll once, independently of the animation. Lock/accept refreshes do not restart motion. UI and direct controller commands reject input during motion. Completion restores controls and match feedback; it never accepts tags automatically. The final respin remains visible without a timeout until **Accept Tags**. Phase exit cancels the animation and its completion callback.

## Results

| Check | Result |
| --- | --- |
| `SlotMachineRuleChecks.ValidateRules()` | 71 assertions passed, including distinct spin identity across rolls and unchanged identity on lock edits |
| `TackleDiscoverArtChecks.ValidateAssets()` | 155 assertions passed, including 37 byte-identical original PNGs and all three cylinder/template bindings |
| `SlotReelAnimationChecks.Begin()` | 33 frame-driven assertions passed |
| `SlotMachineSceneChecks.ValidateLive(false, …)` | 48 initial-acceptance assertions passed |
| `SlotMachineSceneChecks.ValidateLive(true, …)` | 65 respin/review assertions passed |
| `TackleDiscoverArtChecks.ValidateLiveArtwork(…)` | 16 interaction, feedback and long-rig assertions passed |
| Unity console after live validation | No errors or warnings |
| Unity metadata/reference validator | 1,514 files checked; zero errors and warnings |

Live animation coverage includes initial wheel movement, input rejection during both animations, no additional gameplay rolls, stationary locked-wheel phase/tag, resolved tag preservation, readable opaque final native captions, correct rest geometry and edge shading, exhausted respin/lock controls, stale acceptance rejection, no automatic reveal, explicit acceptance, no acquisition before landing, and animation cancellation at phase exit. Existing synchronous fixtures explicitly settle motion; those fixtures validate controls rather than animation timing. Assertion totals can vary with the number of visible tag labels for a random result.

## Captures

- [Initial spin](InitialSpinning.png)
- [Respin with middle wheel locked](LockedRespin.png)
- [Final result waiting for Accept Tags](FinalReview1080.png)
- [Final result at 2560 × 1080](FinalReviewUltrawide.png)

`Controls/`, `InitialAccept/` and `Artwork/` retain regression-fixture captures separately from historical artwork evidence. Rendered captures were reviewed to confirm captions, clipping, shading and letterboxing. These are still images; the frame-driven checks validate motion.

Six tags still use native text instead of unprovided illustrations. This animation adds no new artwork, landing mechanic, scoring effect, reward rule or reel probability.
