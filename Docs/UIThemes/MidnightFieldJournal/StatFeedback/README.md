# Animated current-encounter stats

The candidate main creature/Apex card now emphasises Catch Chain-derived stat changes. Value gains and Weight reductions are green; Value losses and Weight increases are red. Affected numbers grow to 1.22× their normal size and retain that styling while modified. A 0.25s initial hold precedes visible integer steps spaced 0.16s apart, using unscaled time. Every integer is shown; larger differences therefore take proportionally longer. Counting never blocks gameplay or delays authoritative values.

The baseline is resolved without chain influences but includes the card’s own effects, pending Technique modifiers and pending event reductions. The full preview remains authoritative. This also captures chain-dependent own abilities such as Garfish Swift/Mackerel Packed Shoal, and pending chain effects whose source left play. Bait’s intrinsic zero haul Value does not become a red penalty. Net-zero chain changes remain neutral. Mixed modifiers use the net chain contribution after normal clamps/resolution.

Repeated refreshes do not restart the count. Changed targets retarget from the visible integer. Returning to an unmodified baseline counts back and restores normal styling. New occurrences reset even for the same species. Hidden fields never expose an intermediate number, colour or scale hint. Incoming motion pauses counting so steps remain visible after the card settles. Outgoing/history snapshots use authoritative numbers without replaying animations. Only the candidate prefab opts in; artwork and numerical-field geometry remain unchanged.

Actual Play Mode checks record exactly 8→9→10→11→12 and verify readable pacing, all four polarity cases, enlargement, unchanged fields, removal/recovery, mid-count retargeting, changing baselines, same-species occurrences, Bait zero, concealment, static snapshots, transition pause/freeze and real Lobster/Garfish controller previews. Runtime regressions cover Garfish/Minnow/Jellyfish, Bass/Urchin/Octopus and approved Techniques. Three older controller fixtures were updated to explicitly choose Catch before Descend, matching the already-existing selection rule. See Summary.json for final counts. Scene clean, Play Mode/background restored off; no compile errors or source-art mutations.

[Value gain 4K](ValueIncrease4K.png) · [Weight reduction 4K](WeightReduction4K.png) · [Play Mode results](PlayModeVerification.json) · [Runtime regressions](RuntimeRegression.json) · [Summary](Summary.json) · [Preservation](Preservation.json) · [Checkpoint](UnityCheckpoint.json).

MFJ-SYNC-006.9, 20 October 2026. Unity-only feature; Penpot history unchanged.

Latest verification captures: [Value gain](Capture-20261020-111617137/ValueIncrease4K.png), [Weight reduction](Capture-20261020-111617137/WeightReduction4K.png). [1080p styling](ValueIncrease1080p.png). Each test run writes to a unique capture directory so an image open for review cannot block a repeated capture.
