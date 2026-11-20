# Landing score rush

20 November 2026. Implements the approved animated scoring preview in the actual Landing presentation. Creature content, scoring coefficients and supplied artwork are unchanged.

## Behavior

- The temporary score readout steps through the resolved accuracy base, each flat addition, then each multiplier. Current sources kick and receive synchronized brass outlines; curved light trails connect them to the score. A named portrait represents a source outside the scrolling cabinet.
- Musical cues rise through the operations. The final strike value transfers into accumulated points/progress, with a short impact and brass sparks. Amplification determines pacing and intensity; ordinary hits and zero-point Misses remain quieter and shorter.
- Exact decimal scoring and source explanations remain in the persistent Last Hit display. Large animated numbers may be abbreviated. Ongoing assistance and extra attempts retain separate attribution.
- Sound and reduced-motion toggles sit above the machine. User choices persist only when changed. Reduced motion removes transforms, trails and bursts while retaining the numerical sequence.
- Space or Finish ends the count-up early without accepting another strike. No confirmation is required: every sequence also finishes automatically in less than 3.1 seconds, including long rigs.

## State ownership

`LandingScoreSequence` reads immutable score contributions and builds a bounded presentation schedule. `LandingScoreRush` owns native text, transforms, source cues and six reusable 2D audio voices. `LandingRushGraphic` draws light trails/sparks with a native canvas renderer. The original PCM cues and dedicated mixer are authored under `Assets/UIThemes/TackleApparatus/Audio/Landing/`; synthesis is deterministic editor authoring, with no runtime generation or external audio dependency.

The controller's `LandingFeedbackPending` gate holds motion and input during feedback. Points and success/failure resolve immediately. Failure immediately pays the full accumulated haul and clears the live rig; the receipt preserves the pre-cleanup source cards until its feedback finishes. Build and automatic duplicate acquisition follow presentation. A controller deadline and missing-presentation fallback prevent a disabled or interrupted visual from retaining the gate. Build keeps the winning receipt without replaying its celebration.

`LandingScoreRushSetup.Install()` updates the existing Landing prefab in place. **Fishing Cards → Landing Feedback → Install Score Rush** invokes the same operation. The existing Landing/feedback authoring paths retain the bindings during regeneration. Existing scene instances inherit the prefab update and retain their profile and Harbor return route.

## Verification

[Rules and asset results](RulesAndAssets.json): 735 assertions passed: 14 score-sequence/clock checks, 36 score-rush asset/audio checks, 64 Landing rules, 76 rig rules, 374 installed ability checks, 144 Landing artwork checks and 27 attribution asset checks.

[Live results](LiveVerification.json): 136 assertions passed in a fresh comparison Play session with persistence disabled. Coverage includes real Perfect/Miss strikes, once-only scoring, blocked duplicate input and placement during feedback, routed audio playback, source/portrait cues, reduced motion, skip cleanup, ordinary hits, a 993.3-point combo from actual level-scaled abilities, immediate terminal outcome, delayed automatic duplicate acquisition, immutable pre-upgrade levels and immediate full-value failure payout/cleanup. The payout mesh is checked for actual native geometry. The fixture upgrades only its disposable runtime rig; it does not edit species definitions or balance.

[Project integrity](ProjectIntegrity.json): 1,818 Unity files checked, no missing metadata/GUID/script errors or warnings. The final live run had no Unity Console errors. Audio playback was verified through routed active voices and non-silent bounded PCM samples; speaker/headphone listening remains a subjective tuning step.

Native captures cover [1920 × 1080](Combo-1920x1080.png), [1280 × 800](Combo-1280x800.png), [2560 × 1080](Combo-2560x1080.png), [an offscreen source](Offscreen-Source.png), [a large combo](Big-Combo.png), [its payout](Big-Payout.png), [failure feedback](Failure-Feedback.png) and [settled failure](Failure-Settled.png).

Repeat `LandingScoreRushChecks.ValidateRules()` and `ValidateAssets()` outside Play mode. In a fresh comparison Play session, call `LandingScoreRushChecks.Begin()` and inspect its `Result`. The shared Build fixture now waits for this controller-owned presentation gate before asserting the next phase.
