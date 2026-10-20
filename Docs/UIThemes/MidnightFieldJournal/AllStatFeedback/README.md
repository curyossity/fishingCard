# Current-creature stat feedback from every effect

Implemented in Unity, 20 October 2026. Current checkpoint: [MFJ-SYNC-006.11](UnityCheckpoint.json).

The full resolved preview is compared with authored Weight/Value, so Technique grants, event grants, the creature's own abilities and chain effects receive the existing integer count, green/red enlargement and spring beat. No second chain-free preview is needed. Choosing Bait shows neutral zero Value immediately, cancelling any Value count/punch. Returning to Catch starts at printed Value and counts its effect modifiers; it never counts from Bait's zero. Weight continues normally across role changes. Load-only relief does not alter individual Weight.

All 45 Play Mode assertions passed, including exact 8→9→10→11→12 pacing, spring/tilt/settling, retargeting, baseline recovery, masking, transition pause, history, repeated refresh, same-occurrence Bait switching during a count, actual Prize Hook play and pending Gentle Upwelling. Pending grants were not consumed by previews. Runtime regression checks passed: Coastal Support 62, Bass/Urchin/Octopus 101, Approved Techniques 39. Unity compiled with no console errors; scene remained clean, Play Mode stopped and background execution restored to false.

[Presentation report](PlayModeVerification.json), [summary](Summary.json), [preservation audit](Preservation.json). The presentation report links its unique 3840×2160 capture directory; TechniqueStats4K shows Cod at Weight 3 in red and Value 20 in green after Prize Hook, without a supporting chain. The capture was visually inspected. Artwork, fonts, authored cards, prefabs, scenes and ProjectSettings are unchanged. Penpot has no new edit or pending import from this task; historical entries remain preserved.
