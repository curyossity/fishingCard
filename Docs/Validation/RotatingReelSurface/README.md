# Complete rotating reel surface

Validated 9 November 2026 in Unity 6000.5.9f1, using the installed comparison scene. Paper grain, divider lines, blank plaques, icons and native tag captions share one rotation per wheel. The rim, housing and directional shadows remain fixed. The locked wheel stays completely stationary during respin, and the result still waits for **Accept Tags**.

## Evidence

- [Motion A, after 0.04 seconds of deterministic visual advancement](Motion-A.png)
- [Motion B, after 0.08 seconds](Motion-B.png)
- [Respin with stationary middle wheel](Motion-Locked.png)
- [Settled result](FinalReview1080.png)
- [Ultrawide result](FinalReviewUltrawide.png)

Motion A/B advance the real presentation directly while the normal gameplay result remains unchanged. They demonstrate divider/plaque displacement without depending on Editor frame scheduling. Comparing those rendered frames found **269,594 changed pixels inside the wheels, zero changed pixels outside the wheels, and zero changed pixels in the fixed rim strips**. The separate frame-driven check exercises real Update timing, commands and the phase handoff. Its `InitialSpinning.png`/`LockedRespin.png` snapshots can occur later than their requested wait on a busy Editor; use the deterministic Motion captures for the intermediate visual poses.

## Checks

| Validation | Result |
| --- | --- |
| Layer/source/shader/prefab authoring | 12 checks passed |
| Original artwork and bindings | 155 checks passed; original 37 PNGs unchanged |
| Slot rules | 71 checks passed |
| Frame-driven initial spin/respin/review/acceptance | 37 checks passed |
| Live control regression | 80 checks passed |
| UI raycasts, focus/press/disabled feedback and long rig | 16 checks passed |
| Runtime material reuse and immutable authored phase | Passed |
| Supplemental icon sources and mappings | 40 checks passed |
| Unity metadata/reference validation | 1,542 files checked; zero errors or warnings |
| Unity console after live validation | No errors or warnings |

The frame-driven checks cover independent wheel materials, exact paper/symbol phase synchronization, fixed locked-wheel paper phase, input gating, source result preservation, no automatic acceptance, stale acceptance rejection, readable settled captions and phase-exit cancellation. The installed prefab has three independent surface renderers; the static baked drum renderers are disabled. Native 1080p motion/rest and ultrawide captures were visually reviewed for plaque alignment, edge compression/shadows and clipping.

New texture source, prompt and hashes: [reel surface record](../../UIThemes/TackleApparatusConcept/ReelSurface/README.md). No creature, icon, probability, landing or rig rules changed. Historical animation evidence remains separately preserved.
