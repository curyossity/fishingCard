# Springy number-only stat feedback

Adapted the requested lively scoring-number feel to the existing non-pixel UI. Each counting step adds a14% scale kick and alternating2.5-degree tilt, followed by a damped spring. The final integer adds a22% kick and settles over0.36 seconds to the existing enlarged green/red appearance. The initial hold and0.16-second integer pacing remain unchanged, so every integer is still readable. No flames, particles, sounds or pixel styling were added.

Each counter owns its pulse age/direction; ordinary refreshes preserve motion rather than restarting it. New occurrences, hiding, blanking, snapshots and disable restore the original rotation/scale. Existing masks, Catch/Bait semantics, chain-only baseline and transition pause remain intact. The change is source-only: no prefab, scene, artwork, font or gameplay changes.

31 actual Play Mode checks passed, including exact8→9→10→11→12, noticeable pop, alternating tilt, stronger final beat, damped settling and the full existing stat-feedback integration suite. Measured ordinary-beat peak scale1.3908 and final-beat peak1.4884, settling to1.22. Screenshot captures finish in the unique run folder. Play Mode/background restored off; scene clean and no compilation errors.

[Value gain4K](Capture-20261020-113230952/ValueIncrease4K.png) · [Weight reduction4K](Capture-20261020-113230952/WeightReduction4K.png) · [Play Mode report](PlayModeVerification.json) · [Summary](Summary.json) · [Preservation](Preservation.json) · [Checkpoint](UnityCheckpoint.json).

MFJ-SYNC-006.10,20 October2026. Unity-only refinement; Penpot history unchanged.
