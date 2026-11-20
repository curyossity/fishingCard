# Landing strike attribution

19 November 2026. Implements the approved combination of source activation cues, exact contribution breakdowns and inspectable explanations.

## Installed behavior

- Scoring sources briefly receive a brass outline and an external amount badge. Their portraits and amounts remain above the timing track until the next strike; source lists scroll without automatically moving the cabinet.
- The console readout retains the hit grade/total and scrollable rows: accuracy base, flat additions, multipliers, then total. Inspecting Total shows the complete arithmetic expression. Exact decimal scoring values are preserved.
- Hovering or focusing a contribution opens its frozen current-level explanation. Its direct source is strongly outlined and visible positional enablers are outlined softly. Explanations stay above the timing track and insertion controls. Native focus scrolls the inspected source list into view, without submitting a command.
- Extra attempts are attributed separately beside the attempts display and use a distinct recovery badge color. Protected Misses are identified separately; the existing shared protection pool does not invent an arbitrary consuming source.
- Continuous Good/Perfect widening and slowing have their own portrait strip and effective, capped encounter percentages. They do not pretend to add points on every strike. Value-only creatures do not receive scoring activation cues.
- Winning-hit portraits, amounts and explanations remain available in the existing Build message strip and automatic upgrade display. The receipt contains only committed pre-acquisition members, preserving their pre-upgrade levels. New creatures never credit their own acquisition.
- Scrolling/removal cannot leave floating card outlines. Phase exit and fresh encounters clear pending animation and inspection. Failure preserves the source receipt before clearing the rig; a new expedition clears it.

The pulse uses an unscaled 0.30-second envelope with at most 0.12 seconds of total stagger, regardless of source count. A new strike replaces the prior feedback. These are presentation deadlines only: scoring, attempts, immediate success/failure, existing feedback freeze and automatic upgrade timing are unchanged. All supplied card and machine images remain untouched.

## Ownership and regeneration

`RigRuntime.SnapshotForLanding` freezes resolved positional counts, explanation text and enabler IDs. `LandingScoring` and `LandingChallengeRuntime` attach that information to exact score/attempt contributions. `LandingStrikeReceipt` copies committed members before the controller performs terminal handling.

`LandingStrikeFeedback`, `LandingContributionView` and `LandingActivationView` render the receipt; the shared `TackleCardTooltip` renders source explanations. The views never rerun scoring or derive positional eligibility. The controller retains the winning receipt across automatic continuation and failure cleanup, and resets it at expedition start.

**Fishing Cards → Landing Feedback → Install** installs the feature in existing Landing/Build prefabs. Both normal phase-authoring paths also retain it during regeneration. Independent native chip/row/activation prefabs live in `Assets/UIThemes/TackleApparatus/Prefabs/`.

## Validation

- 27 asset/metadata checks: required phase bindings, non-intercepting activation layer, positional enablers and frozen explanations after upgrades.
- 76 rig-rule checks and 374 five-tag ability checks passed with unchanged scoring and ability behavior.
- 45 existing tooltip asset checks passed.
- 55 existing live tooltip checks passed; [fresh regression captures and result](TooltipRegression/Verification.json) are kept separately from the original tooltip evidence.
- [Unity project integrity](ProjectIntegrity.json): 1,792 files checked, zero errors or warnings. The Unity Console reported no compilation/runtime errors in the final live run.
- [Live result](LiveVerification.json) records real Perfect/Miss strikes, addition/multiplier order, offscreen scorers, all three assistance types, source inspection, unobstructed timing track, unchanged points/attempts/revision during rendering, real button raycasts, pulse cleanup, attempt attribution, winning placement/duplicate handoffs, failure cleanup and fresh-encounter invalidation. Persistence is disabled in this fixture.
- Captures cover 1280 × 800, 1920 × 1080 and 2560 × 1080, plus inspected sources, recovery and both winning-hit handoffs.

[Strike feedback](Strike-1920x1080.png), [smaller screen](Strike-1280x800.png), [wide screen](Strike-2560x1080.png), [source explanation](Inspected-Source-1920x1080.png), [attempt recovery](Attempt-Recovery.png), [winning placement](Winning-Hit-Build.png), [automatic upgrade](Winning-Hit-Upgrade.png).

Repeat asset checks with `LandingStrikeFeedbackChecks.ValidateAssets()`. In a fresh comparison Play session, run `LandingStrikeFeedbackChecks.Begin()` and read its `Result` property.

## Design references

These are historical reference behaviors and design interpretations, not claims of identical mechanics:

- [Balatro official visual references](https://www.playbalatro.com/press-kit/): source-linked scoring feedback and identifiable additions/multipliers.
- [Slay the Spire developer patch notes](https://store.steampowered.com/oldnews/?appgroupname=Slay+the+Spire&appids=646570&enddate=1546329600&feed=steam_community_announcements): visible feedback on triggered passive sources, including explicit orb flashes.
- [Monster Train 2 developer patch 1.1.2](https://steamcommunity.com/games/2742830/announcements/detail/519715574346941995): localized damage/status feedback and attention cues near relevant effects.
