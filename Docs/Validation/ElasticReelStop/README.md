# Elastic reel stop

Implemented 13 November 2026. Each moving wheel travels past its selected position, snaps back through it with a smaller recoil, and settles exactly at the selected face. Paper, dividers, plaques, icons and captions use the same motion. Locked wheels stay still and the result still requires **Accept Tags** after all animation completes.

The user's `outElastic(1, 0.47)` example supplies the motion direction; Unity uses a native damped spring during the final stop rather than introducing animejs. The initial spin and respin share the existing `TackleReelAnimator` path.

| Authored tuning | Initial value |
| --- | --- |
| Forward overshoot | 0.45 symbol pitches / 40.5° |
| Stop/recoil interval | Final 34% of the existing spin duration |
| Elastic period | 0.47 of that interval |
| First reverse excursion | Approximately −9.9° |
| Total durations and stagger | Preserved: 1.05 / 1.20 / 1.35 seconds for the initial spin |

These fields are serialized on the three reel components in `DiscoverCanvas.prefab` and retained by their standard defaults on regeneration. The forward deceleration reaches the overshoot at zero velocity; the spring begins at zero velocity and its tail fades smoothly to the exact endpoint. Rotation remains independent of gameplay RNG and uses unscaled time.

[Overshoot → recoil → rest pose preview](Overshoot-Recoil-Rest.png). This is an isolated real-prefab fixture with the same selected Fish tag shown at three different stop times; it does not select a creature or modify player saves.

`SlotReelStopChecks.ValidateAndCapture()` passed 204 assertions, including finite frame samples, a clear bounded overshoot, reverse movement with a smaller excursion, exact final alignment, no drift after completion, synchronized paper material, matching stop motion at 30/60/144 FPS and correct large-frame completion. The live animation check allows a busy Editor to finish a complete wrapped rotation before its delayed frame sample; matching start/end angles alone do not imply that the reel failed to move.

Additional verification: 37 live initial-spin/respin/locked-wheel/review/acceptance assertions passed; slot rules passed 71 checks, surface bindings 12, and original artwork bindings 155. Unity reported no errors or warnings after the final live check. Metadata/reference validation checked 1,544 files with zero errors or warnings. Historical captures remain in their original folders; this change's live captures are under `Live/`.
