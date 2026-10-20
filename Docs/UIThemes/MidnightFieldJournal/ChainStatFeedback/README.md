# Sequential Catch Chain stat feedback

Implemented directly in Unity, 20 October 2026. Current checkpoint: [MFJ-SYNC-006.12](UnityCheckpoint.json).

Compact Catch/Bait creatures use the same CreatureCardStatAnimator as the current encounter: 0.25s hold, every integer at least 0.16s, green for Value up/Weight down and red for the opposite, 1.22× modified size, alternating tilt/scale spring and stronger final beat. Both stats on one creature may animate together. The next creature starts only after this creature's final spring settles. Affected recipients run newest to oldest and scroll fully into view using the existing 0.32s smooth scroll before counting. Waiting creatures do not count or enlarge early. The last recipient remains visible until the chain extent changes or another creature arrives.

Resolved gameplay values update immediately; this sequence is presentation only. Existing pooled views now retain CardInstance identity across index changes. Repeated refreshes preserve progress, new changes retarget without restarting, removed recipients cannot block the queue, and empty/disabled chains release focus. Bait zero Value is immediate and neutral; masked Value remains masked. Manual wheel/drag scrolling cancels the automatic sequence and settles all numbers so inspection retains control. Ended-run snapshots use static values. Legacy compact presentations remain static.

Validation:

- All 68 chain Play Mode assertions passed, including seven mixed Catch/Bait recipients ordered 7→6→5→4→3→2→1, full spring completion before the next, deferred emphasis, overflow scrolling, final numbers, repeated refreshes, stable views after removal, removing the active recipient, manual interruption, later changes after interruption, ended-run state and empty-chain cleanup.
- All 45 existing main-card animation assertions passed, including integer steps, punch/tilt, masking, role changes, transition pause, history, Prize Hook and Upwelling.
- All 202 gameplay regression assertions passed: Coastal Support 62, Bass/Urchin/Octopus 101, Approved Techniques 39.
- Unity compiled with no console errors. Scene remained clean, Play Mode stopped, background execution restored. The 3840×2160 settled-chain capture was visually inspected.

[Chain report](PlayModeVerification.json), [main-card regression](MainCardRegression/PlayModeVerification.json), [summary](Summary.json), [preservation audit](Preservation.json). One initial test incorrectly assumed the fixture's initial scroll offset was already at the bottom; it was corrected to measure actual scroll travel during the sequence. Final checks above use that corrected assertion.

Seven existing source files changed and one Editor verification script plus metadata was added. Artwork, fonts, card definitions, prefabs, scenes and ProjectSettings are unchanged. No Penpot edits or pending imports were added; previous integrations remain recorded.
