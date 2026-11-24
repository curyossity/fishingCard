# Smaller cards and readable collisions

Spacing and pacing revision, 24 November 2026. The later [elastic-motion revision](../Elastic/README.md) adds squash and stretch while preserving these sizes and timings. Open [Compare.html](Compare.html) for the updated native 60 fps examples. The [initial collision treatment](../README.md) retains the previous layout and timing as historical evidence.

- Both machine faces use **88% scale**, including their native content. Each center moves another 12 reference pixels outward. Actual contact travel grows from 16 to **97.6 pixels**, calculated from the scaled panel bounds.
- Scoring beats last roughly **0.52–0.90 seconds**. The wider dash takes 100–130 ms, contact holds for **75–105 ms**, and both panels have a slower readable recovery. The receiver settles over 520 ms while screen shake remains brief.
- The exposed working space uses one continuous dark recess sampled from the supplied housing. Original PNGs, rig-card sizes, gameplay, accepted audio assets and the collision-only visual direction remain unchanged.

Base-score contact occurs at 301.6 ms; the first modifier starts at 600 ms. Long rigs preserve a 500 ms beat floor within a 5.6-second modifier budget. `LandingScoreSequence.MaximumDuration` bounds feedback at 7.5 seconds; the controller derives its existing skippable gate from the actual schedule. Ordinary zero-point misses retain a short quiet sequence.

**278 checks pass:** 68 native checks per fixture, 46 prefab/binding checks, 32 schedule/score checks and 64 Landing/model checks. The native audit also checks equal face scaling, widened collision travel and a minimum readable beat. Reduced motion, exact impact arithmetic/sound, stationary rig cards, no particles, interruption cleanup and Good/Miss behavior remain covered.

| Current-definition fixture | Earned points | Previous duration | Current duration |
| --- | ---: | ---: | ---: |
| Modest | 50.7 | 2.187 s | 3.227 s |
| Powerful | 12,199.2 | 2.949 s | 4.127 s |

The screenshots and consecutive native frames were reviewed for spacing, whole-card movement, readable digits and contact. Complete frame sequences were verified before encoding. Videos mix the existing frozen audio cues; they are not device loopback recordings. [Verification.json](Verification.json) and the two Level JSON files retain the results. Unity reports no errors, and the isolated Play session is stopped afterward.

`Tools/Rendering/CardCollisionAudit.cs.txt` exports 60-frame batches. Run consecutive `exportFrom` values (0, 60, 120…) until the complete `(Duration + 0.3) × 60` frame count is reached; this keeps each MCP request bounded. Temporary renders are under `Temp/CardCollisionReview/Readable`. The fixture uses real definitions without player-controller commands or save mutation.
