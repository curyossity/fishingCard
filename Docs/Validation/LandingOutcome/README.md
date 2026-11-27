# Landing outcome and haul count-ups: validation

27 November 2026. This adds a presentation beat after a Landing challenge ends and makes the haul count up in Build. No score, outcome, reward, payout or rule changes. Following the GDD, failing a Landing ends the expedition and still pays the full accumulated haul. The failed creature is not acquired.

## Behavior

- **Gate:** `FishingRunController.LandingOutcomePending` starts when a terminal strike's score feedback finishes, or on the strike itself when there is no score presentation. Its deadline is `LandingOutcomeTiming.Duration`. Space or the SKIP button ends it early.
- **What waits for the gate:** the outcome, the failure payout and rig cleanup have already resolved. Only these wait: a landed duplicate's automatic upgrade, Build opening for a new species, and Return to Harbor after a failure.
- **Landed (0.95 s):**
  - The creature card dips, is yanked up 46 px with stretch, then drops and slams home with a squash, a white panel flash, a small stage jolt and a green **LANDED** stamp.
  - The header reads `<CREATURE> LANDED` from the slam.
  - The phase transition shutter then closes over the stamped card and opens Build.
- **Escaped:**
  - The card thrashes, then sinks and fades out of its frame. A rust **ESCAPED** stamp lands on the empty frame, and the header reads `<CREATURE> ESCAPED`.
  - The stamp folds away and the header changes to **EXPEDITION OVER**. A **HAUL PAID** readout appears in the empty frame.
  - Each creature in the paid haul (`LastHaul`, in rig order) cashes out in turn: it lights and pops, its value rolls into the readout, and it dims. Each step lasts 0.03–0.22 s, within a 1.6 s budget.
  - The total punches on exactly `LastHaulValue`, the rig column clears to RIG CLEARED, and Return to Harbor appears.
- **Build haul count-up:**
  - During the placement and upgrade receipts, the HAUL plaque and the HAUL CHANGE readout roll from the receipt's previous haul to its committed haul, ticking as they climb and punching on arrival.
  - Placement counts after the card flight lands, and its receipt now lasts 1.05 s (was 0.68 s). Upgrades count straight away within their existing 1.8 s.
- **Reduced motion:** card thrash, sink, yank, pops and punches are removed. Fades and every number remain.
- **Sound:** existing cues stand in as placeholders. Landed uses Travel on the yank and Attach on the slam. The cash-out ticks use SlotTick and finish with a quiet PayoutSmall. Escape keeps the existing Failure cue. Final sounds are a `team-audio` decision.

## Ownership

| Owner | Responsibility |
| --- | --- |
| `FishingRunController.Landing` | Gate, deadline, `CompleteLandingOutcome` and the deferred duplicate upgrade (`ContinueAfterLandingOutcome`) |
| `LandingOutcomeTiming` | Pure schedule, plus the shared `TackleHaulCount` curve |
| `LandingOutcome` | Card, flash, stage, stamp, payout readout and cash-out; restores everything on clear or disable |
| `LandingRarityBanner` | Outcome styles, drawn on a second "Outcome banner" instance |
| `TackleLandingPresentation` | Starts the beat, the header, and the haul column during cash-out |
| `LandingChallengeView` | SKIP, and Return to Harbor only after the beat |
| `RigBuildingView` / `RigBuildPresentation.HaulTick` | Build count-up |
| `LandingOutcomeSetup` | Authors the layers; retained by `LandingScoreRushSetup` regeneration |

See [Architecture](../../Architecture.md).

## Verification

- **14 timing assertions** (`LandingOutcomeChecks.ValidateTiming`):
  - The Landed beat starts and ends at rest; the yank and dip have the right direction; the slam has its flash and squash.
  - The escaped creature is invisible when ESCAPED stamps, and the stamp clears before the payout.
  - The payout is a monotonic roll ending exactly on the total; the header order is right; cards clear in order.
  - Punches return to exactly 1; durations are bounded, including long hauls; an empty haul works; the count-up curve is correct.
- **[Live](LiveVerification.json): 34 assertions** in a fresh comparison Play session with persistence disabled, using real Discover, Landing and Build commands:
  - A new species shows Landed and holds Build. Placement is rejected during the beat. SKIP opens Build through the shutter, with the panel and stamp restored.
  - The placement haul rolls 0 → 7 without jumping.
  - A second species ends at the beat's own deadline.
  - A duplicate stays at its level during the beat and upgrades automatically after it. Its haul rolls upward.
  - A failure pays once immediately; Return to Harbor neither appears nor works before the payout; every haul creature is shown for cash-out.
  - With reduced motion, the escaping card doesn't move, only fades.
  - The header reads COD ESCAPED and then EXPEDITION OVER. The readout ends on the paid total, the rig column clears, and returning to Harbor restores every visual.
- **Regression runs after the change:**
  - `RigBuildingSceneChecks`: 106 passed (output written under `Temp/`).
  - `TacklePhaseTransitionChecks`: 15 passed.
  - `LandingScoreRushChecks`: 142 passed.
  - `LandingCreatureRevealChecks`: 21 passed.
  - The score-rush and reveal evidence folders were restored afterwards, so their historical captures are unchanged.
  - `LandingChallengeSceneChecks` still fails at its first assertion ("Landing did not initialize a committed-rig snapshot"), as it did before this change.
- **Previews:** [comparison page](index.html) with normal and quarter-speed videos of both beats, plus these stills:
  - `Landed-1…5`: dip, yank, drop, slam, stamped.
  - `Escaped-1…5`: thrash, sinking, stamp, cash-out, total.
  - `Escaped-Final`.
  - `Build-Placement-Counting` and `Build-Upgrade-Counting`.
## Known issues and gaps

- After a failure, the legacy Journal result modal (hidden behind the Tackle canvases) takes keyboard focus ("Start Next Run") on each view refresh. Keyboard Space therefore cannot skip failure feedback or the escape beat, and Return to Harbor does not receive focus. Pointer input works. This behavior predates this change.
- Not yet verified: human play feel and the tally pace with long hauls in normal play.
