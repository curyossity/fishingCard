# Catch Chain follows the newest catch

The chain automatically follows its bottom as catches are added and when the viewport changes size. A real wheel or drag gesture away from the bottom suspends following; new catches and ordinary stat/selection refreshes preserve that pixel offset. Returning to the bottom resumes following. Empty/short chains reset the override, so a new run follows automatically. Inertia updates end detection; hiding the HUD clears interrupted gesture state without losing the retained inspection position.

`JournalCatchChainScrollRect` extends the existing uGUI ScrollRect and is bound by the normal candidate prefab builder. The presentation sizes content/cards/links and then restores the intent-aware offset. There is no gameplay mutation, periodic run-state polling, new artwork or Penpot import.

[4K preview](NewestCatch4K.png) · [1080p preview](NewestCatch1080p.png) · [Component verification](Verification.json) · [Actual input verification](PlayModeVerification.json) · [Preservation](Preservation.json) · [Current baseline](UnityCheckpoint.json).

22 component checks passed: default follow through overflow and append, fully visible final card, wheel-away retention during additions/stat refreshes, return-to-end resumption, drag retention, shrinkage, viewport resize and empty/new-run reset.22 real Play Mode checks passed for actual canvas raycasts/clicks, wheel behavior, refresh retention, pooled indices and reactivation; the newest-catch offset also survived actual LateUpdate frames.4K/1080p captures were inspected. Original evidence was restored after copying the new input report here. Play Mode/background were restored off; scene and gameplay hashes remain unchanged. Only the isolated candidate presentation, builder, checks and regenerated prefabs changed.

This Unity-only change is checkpointMFJ-SYNC-005.3; no pending Penpot revision is created. The lowered hand, grayscale inactive depth tiers and all previous approved visuals remain intact.
