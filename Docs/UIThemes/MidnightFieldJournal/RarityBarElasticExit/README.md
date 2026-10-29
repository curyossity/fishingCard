# Rarity bar elastic exit — MFJ-SYNC-008.3

After the stopped-result hold, the journal timing instrument, title and subtitle move upward by one viewport height over one second. Motion uses an elastic-in curve with amplitude 1 and period 0.74: a small oscillating wind-up followed by the upward exit. Progress remains unclamped spatially so the wind-up is visible. The curve follows the mathematical easing used by [Anime.js](https://github.com/juliangarnier/anime/blob/master/src/easings/eases/parser.js), expressed natively in Unity without adding a JavaScript dependency.

The creature stays unrevealed until the exact timing occurrence completes its exit. The controller continues owning selection, pending effects and reveal reactions; the view only reports presentation completion. Headless runs, missing views and disabled views do not wait for nonexistent animation. Motion uses unscaled time, freezes the stopped marker, and restores its original position when hidden, cancelled or reused. Surface can cancel an exit normally. Zone sizes and uniform rarity-ceiling selection are unchanged.

Verified: six easing checks, 75 timing/rules checks, 30 actual Play Mode checks, and all eight existing gameplay suites (558 assertions). Play Mode includes delayed reveal during exit, Stop locking, next-attempt reset, Surface cancellation during motion, restart, event bypass and actual Stop raycasting. 4K captures: [stopped](Stopped4K.png), [wind-up](ExitWindUp4K.png), [upward exit](ExitUpward4K.png), [revealed creature](Revealed4K.png).

This is a Unity-only animation update. No art regeneration or Penpot import is needed. [Current checkpoint](UnityCheckpoint.json); [previous integration](../RarityCeilingSelection/README.md).
