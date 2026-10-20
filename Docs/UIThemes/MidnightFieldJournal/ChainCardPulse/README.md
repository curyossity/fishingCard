# Catch Chain card pop and warm flash

Implemented in Unity, 20 October 2026. Current checkpoint: [MFJ-SYNC-006.13](UnityCheckpoint.json).

Each affected compact creature scrolls into view with expansion padding, plays one 0.38-second centered pop (4.5% maximum enlargement) and warm ivory flash (18% peak opacity), then starts its existing number feedback. The next creature waits until that number animation and its final spring finish. The same prelude applies to benefits and penalties; numeric green/red polarity remains unchanged.

JournalCatchCardPulse wraps only that card's visual children once. Its layout/input root and independent connectors stay stationary. Its rounded MaskableGraphic flash respects the viewport mask, has no input target and returns to zero opacity. The wrapper returns exactly to scale one. Refreshes do not restart the prelude; manual scrolling, removal, disable or ended-run presentation stop it and clear its visual state. The shared number animator, game rules and authored assets are unchanged.

All **89 Play Mode assertions passed**, including seven recipients in 7→6→5→4→3→2→1 order, a prelude on all seven, no counting during the pop/flash, no moved/scaled hardware, 4.5%/18% amplitude limits, resting-state cleanup, serial number completion, pooling, actual selection callback after wrapping, manual cancellation during an active pulse, removal during an active pulse, ended-run state and empty-chain cleanup. [Report](PlayModeVerification.json), [summary](Summary.json), [preservation audit](Preservation.json).

The final 3840×2160 pop/flash capture was visually inspected in Capture-20261020-142916753. Compilation succeeded. Scene remained clean, Play Mode stopped and background execution restored to false. Stopping Play Mode while a Technique was hovered emitted an unrelated existing teardown error from unchanged JournalTechniquePose.OnDisable → ApplyPose → SetSiblingIndex (line 46), while its parent was deactivating. This was outside the completed chain tests and is recorded separately from their passing result; this increment does not claim an error-free entire application shutdown.

Three existing theme source/test files changed and one theme runtime component plus metadata was added. No source images, fonts, card data, prefabs, scenes, shared number-animation code, gameplay or ProjectSettings changed. Penpot has no new revision or pending import for this Unity-only increment.
