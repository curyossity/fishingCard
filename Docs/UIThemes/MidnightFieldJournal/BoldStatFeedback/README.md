# Bold manipulated stats and adjusted pace

Implemented in Unity, 20 October 2026. Current checkpoint: [MFJ-SYNC-006.15](UnityCheckpoint.json).

Main and compact creature numbers use the same shared animator. Ordinary steps now take 40ms instead of 25ms; large deltas target about 0.8 seconds instead of 0.6. The 50ms initial hold, 180ms spring, per-change pop/alternating tilt, colours and enlarged size are unchanged. The measured 8→9→10→11→12 count took about 0.16 seconds.

Manipulated numbers use TMP Bold during counting and the landing spring. Modified results remain bold after settling. Returning to default restores the captured original font style and weight once the landing finishes. Original styling also returns when hidden, reset or reused as a static snapshot. Neutral Bait-zero Value stays normal. Queued compact cards do not become bold before their turn. This uses existing font assets; no font, material or image regeneration was needed.

All **54 main-card checks** and **103 chain checks** passed. These cover measured pace, exact ordinary integer order, large-delta bounds, pop/tilt, bold during manipulation, persistent modified bold, baseline restoration, Bait zero, hidden/static state, no early bold on queued cards, persistent compact Weight/Value bold, serial chain order, card prelude, hardware, removal and interruption. The final main-card 3840×2160 capture was visually inspected: modified Cod Weight 3 and Value 20 are bold and retain their respective red/green colours.

[Main report](MainCard/PlayModeVerification.json), [chain report](Chain/PlayModeVerification.json), [summary](Summary.json), [preservation audit](Preservation.json). Unity compiled successfully and console contained no errors after stopping Play Mode; scene remained clean and background execution was restored. Four existing files changed: shared animator, its two check helpers, and candidate creature prefab timing. Artwork, fonts, definitions, other prefabs, scenes, settings and gameplay are unchanged. No new Penpot revision or pending import was added.
