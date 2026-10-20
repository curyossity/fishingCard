# Rapid numeric pop and tilt

Implemented in Unity, 20 October 2026. Current checkpoint: [MFJ-SYNC-006.14](UnityCheckpoint.json).

The shared number animator now uses 25ms ordinary steps instead of 160ms, a 50ms initial hold instead of 250ms, and a 180ms final spring instead of 360ms. Each displayed number change retriggers a 14% scale punch and alternating 2.5-degree tilt; the final beat retains its stronger 22% punch. The measured 8→9→10→11→12 count took about **0.12 seconds**. The existing persistent green/red 1.22× styling remains.

Large deltas use bounded integer strides aiming at about 0.6 seconds of counting rather than requiring hundreds or thousands of individual steps. Counting advances at most once per rendered frame, so each visible change has a pop/tilt. Timing carries fractional wait forward; actual duration remains frame-rate dependent. Retargeting recomputes the stride from the current displayed value. Intermediate presentation values never drive gameplay.

Both main and compact creatures use this same implementation. The candidate prefab's serialized timing was updated through Unity's native prefab API; normal prefab generation applies the same canonical constants. The Catch Chain still scrolls into view, plays its unchanged card pop/flash, runs numeric feedback, waits for the final spring, then moves to the next older creature. Bait zero remains immediate and neutral; hidden values remain hidden.

All **47 main-card checks** and **89 Catch Chain checks** passed. Main checks verify exact ordinary integer order, measured rapid timing, per-step scale punch, alternating tilt, stronger final beat, settling, retargeting, concealment, neutral Bait zero, transitions, Prize Hook/Upwelling, and a 10,000-point bonus finishing through multiple rapid strides. Chain checks retain all ordering, card prelude, hardware, removal, manual interruption and lifecycle coverage. No Unity console errors were present after stopping Play Mode; scene was clean and background execution restored.

[Main report](MainCard/PlayModeVerification.json), [chain report](Chain/PlayModeVerification.json), [summary](Summary.json), [preservation audit](Preservation.json). Four existing files changed: shared animator, theme editor binding, its check helper, and candidate creature prefab timing. Artwork, fonts, authored cards, other prefabs, scenes, settings and gameplay are unchanged. No Penpot edit or pending import was added.
