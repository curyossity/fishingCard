# Build placement and upgrade polish

Implemented 20 November 2026 in the existing Tackle Build canvas. All five approved improvements are integrated:

| Improvement | Result |
| --- | --- |
| Placement ghost | A translucent creature occupies the selected legal gap; neighbors ease apart. The preview never changes committed order or sale value. |
| Consequence feedback | Existing creatures show model-derived link, coefficient or sale changes beneath their faces. Inspection explains full changes and original trigger conditions. |
| Physical placement | After commitment, the creature travels into its actual position, the clip snaps, rope settles and the shared routed sound plays. |
| Automatic upgrade reveal | The existing position remains fixed. Actual level, coefficient and accumulated-value changes receive a distinct reveal without confirmation. |
| Information hierarchy | Previous-strike information is dimmed during placement and hidden while the acquisition receipt uses the message band. |

The supplied artwork and printed card fields are preserved. Labels live outside the faces and clipping masks, follow visible cards while scrolling and do not intercept input. The ghost does not introduce an extra insertion command. Shared sound and reduced-motion preferences apply. Every receipt is already committed; repeated input and interruption cannot acquire or continue twice. The controller bounds feedback to four seconds if the view cannot complete it.

## Verification

- `RigBuildPolishChecks.Validate()`: 21 pure projection/comparison and prefab-wiring checks passed.
- `RigBuildingChecks.ValidateRules()`: 76 checks passed.
- `FiveTagAbilityChecks.Validate()`: 374 checks passed.
- `RigBuildPolishChecks.Begin()`: 20 live checks passed; see [report](LiveVerification.json). Covers selected-gap order, unchanged committed state, readable change labels, original legal gaps, real commitment before feedback, stale/double input, frozen before/after receipts, automatic duplicate upgrade and disabled-view cleanup.
- `RigBuildingSceneChecks.Begin(...)`: 109 live expedition checks passed; see [report](ExpeditionRegression/LiveVerification.json). Covers phase visibility, placement input, automatic upgrades, long-rig scrolling, full failure payout and an empty next expedition. Its forced-encounter fixture now retains legal prepared reel symbols even when the target creature's tag is unprepared. The final run reported zero Unity errors.
- Native Unity captures cover 1920 × 1080, 1280 × 800 and 2560 × 1080. Captions were moved below the faces after visual inspection caught overlap with rope clips.
- Project integrity scan: 1,890 files checked, zero errors or warnings.

Audio routing, preferences and cue invocation were verified; subjective loudness and animation satisfaction still need player review. There are no remaining implementation phases from this set of five suggestions.

## Review captures

- [Selected gap and consequences, 1920 × 1080](Ghost-1920x1080.png)
- [1280 × 800](Ghost-1280x800.png)
- [2560 × 1080](Ghost-2560x1080.png)
- [Committed placement in flight](Placement-Flight.png)
- [Automatic upgrade result](Upgrade-Reveal.png)

## Repeat

Open `MidnightFieldJournalComparison.unity`. The editor menu **Fishing Cards → Rig Building → Install Placement Polish** reapplies the focused prefab bindings; full `RigBuildingSetup` also retains them. Run the three pure checks above in Edit mode. In a fresh Play session, call `RigBuildPolishChecks.Begin()` and inspect its `Result`. In another fresh Play session, call `RigBuildingSceneChecks.Begin("Docs/Validation/BuildPolish/ExpeditionRegression/")` for the complete expedition regression. Fixtures disable progression persistence.
