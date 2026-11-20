# Landing feedback polish

Approved 20 November 2026. This increment implements all six recommendations: a larger active creature cue; anticipation/flight/arrival; distinct effect responses; physical source acknowledgment; amplification-driven machine feedback; and readable contribution history. [Phase status and remaining work](Implementation.md) is the delivery checklist.

## Installed behavior

- The active source has a 72 × 64 unframed portrait at its original bottom position beneath the growing score, with the operation on its left and name on its right. A curved connector remains visible during projectile flight. This is also the firing point for offscreen sources; the cabinet never scrolls automatically. This revision retains the larger size and pop while removing the circular frame at the player's request.
- Each score operation has a preparation, launch and arrival time. The visible number and musical impact change on arrival. Additions fire a gold bead and give a compact number bounce. Multipliers fire a mint pulse, produce concentric impact rings and briefly stretch the digits. Color is paired with +/× text and different motion/geometry.
- Source cards lift six reference pixels, tilt slightly and enlarge their illustration briefly. The faint edge response and softer positional-enabler cues stay external to card fields. Enablers do not fire projectiles. All transforms return to their authored values.
- The source history uses 40-pixel portraits in 52-pixel-high brass chips. Pending operations are subdued, the current operation is bright and completed operations settle to a quieter green/brass state. Long lists remain scrollable and inspectable, retaining exact decimal attribution. Build keeps the completed receipt without replaying its animation.
- Seven score-well lamps light progressively according to actual amplification and delivered operations. Larger combinations have fuller impact sound, stronger final machine response and a progress-fill accent. The total still transfers once; zero-point Misses do not earn score celebration effects.
- Real extra attempts pulse their relit lamps and show the granting creature in the attempts well. The persistent recovery explanation returns when the transient cue ends. Assisted Good/Perfect zones receive restrained edge cues. Actual in-Good slowing shows its source portrait and percentage in the behavior well; Burst telegraph text takes priority. These cues do not change zone geometry, fish motion or attempt rules.
- Reduced motion retains the numerical sequence, readable source states and impact timing while removing card/portrait movement, travel, rings, bursts and shake. Space/Finish settles the score early and clears transient scoring/recovery work. Phase exit cancels owned audio and restores affected visuals.

## Ownership and authoring

`LandingScoreSequence` schedules already resolved operations and preserves per-operation support identities. Its impact lookup is independent of rendering frequency and keeps long chains below the existing 3.1-second presentation bound. Authoritative points, attempts, outcomes and payout still resolve in the original model/controller owners.

`LandingScoreRush` renders the active source portrait, timed arrivals, source motion, operation-specific responses and machine escalation. `LandingRushGraphic` draws pooled native UI geometry. `LandingStrikeFeedback`/`LandingContributionView` retain source inspection and history states. `LandingEffectFeedback` reads the model for local attempt and assistance feedback. `LandingFeedbackPolishSetup` updates the existing prefab through `LandingScoreRushSetup`; normal Landing regeneration retains the feature. Supplied artwork, creature definitions and sound source files are unchanged; existing Tackle sprites/cues are reused.

## Evidence

[Rules/assets](RulesAndAssets.json) cover operation timing, pre-arrival stability, exact arrivals, specific enablers, 150-operation bounds, larger portrait/history geometry, native view/audio bindings, Landing and rig rules, installed abilities and unchanged protected machine artwork.

[Focused live verification](LiveVerification.json) exercises real strikes and receipts with persistence disabled: launch versus impact, routed sound on impact, local slowing, actual Miss recovery, reduced motion, skip cleanup, readable completed history, large-combo lamps, immediate authoritative success and the normal Build/automatic duplicate handoff. Presentation-only clock sampling makes the captures reproducible without modifying gameplay values.

[Full expedition regression](ExpeditionRegression/LiveVerification.json) exercises Discover/Land/Build/Upgrade, longer rigs, stale input, exactly-once failure payout/cleanup and an empty next expedition. [Project integrity](ProjectIntegrity.json) records metadata/GUID/script validation. Speaker/headphone listening and subjective pacing remain hands-on tuning work, distinct from automated correctness checks.

Native captures: [flight at 1280 × 800](Flight-1280x800.png), [1920 × 1080](Flight-1920x1080.png), [2560 × 1080](Flight-2560x1080.png), [addition impact](Addition-Impact.png), [multiplier impact](Multiplier-Impact.png), [attempt recovery](Attempt-Recovery.png), [large combo](Large-Combo.png) and [final transfer](Payout.png).

Repeat `LandingFeedbackPolishChecks.Validate()` outside Play mode. In a fresh comparison Play session, call `LandingFeedbackPolishChecks.Begin()` and inspect its `Result`. For the full-loop regression use `RigBuildingSceneChecks.Begin("Docs/Validation/LandingFeedbackPolish/ExpeditionRegression/")` in a separate fresh session.
