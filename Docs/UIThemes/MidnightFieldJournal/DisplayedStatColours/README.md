# Displayed-value stat colours

Implemented in Unity, 20 October 2026. Current checkpoint: [MFJ-SYNC-006.16](UnityCheckpoint.json).

The shared stat renderer now compares the **displayed** integer with its default, rather than caching destination polarity. Value above default and Weight below default are green; the opposite is red. Becoming better or worse while remaining on the same side retains that side's colour. Returning to default restores normal ink exactly when the displayed number reaches it. When continuing across default, its passing-through integer uses the destination side's colour: green switches directly to red or vice versa, without neutral ink. Fresh counts from default retain the intended side's colour from their existing initial hold.

Immediate neutral Bait-zero Value, bold manipulated results, 40ms pacing, number pop/tilt, card pop/flash, concealed fields and newest-to-oldest chain ordering are unchanged. Default font weight still returns after its landing spring; normal ink returns upon numerical arrival.

All **74 main-card checks** and **103 chain checks** passed. Twenty transition cases inspect actual TMP text/colour frame by frame across both stats: same-side improvement and deterioration, returns from either side, and crossings in both directions, under repeated refreshes. The real Prize Hook→Upwelling retarget now verifies red while displayed Weight remains excessive and green after it becomes beneficial. Existing masking, neutral Bait zero, bold, pacing, large deltas, pop/tilt, transition/history and chain lifecycle checks remain passing.

[Main report](MainCard/PlayModeVerification.json), [chain report](Chain/PlayModeVerification.json), [summary](Summary.json), [preservation audit](Preservation.json). An older event-preview assertion initially expected the destination's green immediately; it was updated to the approved displayed-number rule. Compilation succeeded and no Unity console errors remained after the successful final run. Scene remained clean, Play Mode stopped and background execution restored.

Only the shared animator and main-card check helper changed. No prefab, scene, font, image, authored card, setting or gameplay changes were needed. Penpot has no new revision or pending import; previous integrations remain recorded.
