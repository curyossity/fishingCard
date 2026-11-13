# Landing Challenge validation

Historical plain-control validation. The runtime now uses the approved artwork; see [current Landing artwork validation](../TackleLandingArt/README.md).

Validated 13 November 2026 in Unity 6000.5.9f1, using the MidnightFieldJournal comparison scene and its authored plain Landing canvas. See the [mechanic specification](../../Landing%20Challenge%20Mechanic.md).

## Results

| Check | Result |
| --- | --- |
| `LandingChallengeChecks.ValidateRules()` | 64 passed |
| `SlotMachineRuleChecks.ValidateRules()` | 71 passed |
| `LandingChallengeSceneChecks.Begin(true)` in fresh Play Mode | 16 live success checks passed |
| `LandingChallengeSceneChecks.Begin(false)` in fresh Play Mode | 30 live failure/restart checks passed |
| `SlotMachineSceneChecks.ValidateLive(true, output)` in fresh Play Mode | 80 Discover/respin/handoff checks passed |
| Unity project metadata checker | 1,565 files; zero errors or warnings |
| C# compilation and live console | Compiled successfully; zero live errors/warnings after each fixture |
| Code/document whitespace check | Passed |

The pure checks cover overlap priority and inclusive boundaries; base points; flat-before-multiplier scoring; ordinary Miss zero and explicit Miss effects; positional conditions across both roles; fractional points; immutable snapshots; stale/duplicate commands; feedback freezes; final-attempt success; early success; exhausted-attempt failure; bounded, frame-independent motion; all three patterns and Burst warning; Resistance tuning; full-value payout and duplicate reward rejection.

The live fixtures use temporary Catch/Bait definitions and disable persistence writes before starting. They do not assign illustrative abilities to real creatures or modify player saves. A Perfect strike with a committed +5 Catch bonus and ×2 Bait multiplier produces `(20 + 5) × 2 = 50`. Editing the temporary source after challenge start does not change that frozen score. The encountered creature never enters the scoring rig or attaches early.

Failure pays the temporary Catch's full value of 17, pays zero for Bait despite its authored sale value, excludes the failed creature and clears the live rig. Deliberately large legacy weight does not impose a capacity penalty. Repeat strikes/payout calls grant nothing further. Harbor return and the next expedition preserve the in-memory permanent balance and start with an empty rig and zero Landing Points.

## Captures

- [Initial challenge with fixture rig](Success-Initial.png)
- [Successful scoring and Build handoff](Landing-Succeeded.png)
- [Failed challenge and full Catch payout](Landing-Failed.png)
- [Discover regression captures](DiscoverRegression/Discover1080.png)
- [Final respin results before explicit acceptance](DiscoverRegression/FinalRespin1080.png)

Success/failure captures were visually checked at 1920 × 1080 for readable track, grade, attempts, points, contribution text, committed rig order and terminal feedback. Rig and contribution lists have native clipped scrolling. These captures validate plain controls, not the final art direction or an exhaustive device/resolution matrix.

## Scope and limits

- Space and STRIKE use the same guarded controller command. Live scoring fixtures invoke the native button; physical keyboard timing still needs player playtesting.
- Miss currently means no overlap with the Good region. The conflicting supplied Miss wording remains flagged for clarification.
- Four attempts, 0/10/20 base points, movement tuning and base 40 plus 10 Resistance per progressed depth are editable prototype values, not final balance.
- Success stops at `AwaitingRigPlacement`. Catch/Bait selection, insertion, subsequent encounter continuation and redesigned species abilities are separate work.
- No Landing artwork was integrated. No finalized biome completion, success reward grants, new economy or equipment/Technique effects are claimed by these checks.
- Human playtesting must assess readability, repeated encounters, skill versus rig strength and the Resistance curve.
