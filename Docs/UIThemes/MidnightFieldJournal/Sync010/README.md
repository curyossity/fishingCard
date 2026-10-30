# MFJ-SYNC-010 — Harbor presentation and navigation

Integrated and verified on 2026-10-30. Revisions R053, R055, R056 and R057 are complete for this requested presentation increment. R052 and R054 remain superseded; previous checkpoints are retained.

Open `Assets/UIThemes/MidnightFieldJournal/Scenes/MidnightFieldJournalComparison.unity` and enter Play Mode. It opens at Harbor. The destination panels and top navigation open Tackle Shop and Technique Collection; Back to Harbor returns home. Set Sail starts the existing fresh expedition at its opening boat. Surface uses existing results; Return to Harbor closes results, and Set Sail can begin another fresh run. Duplicate Set Sail commands cannot reset an active expedition.

Purchasing, equipment selection, Technique ownership and deck editing remain visual previews. Their action controls are inactive and the existing starter deck is unchanged. Discovery Journal remains unavailable because its revised screen is not yet approved. Actual gold is displayed from the existing controller; no extra currency or new persistence system was introduced. The original SampleScene and Editor build selection are unchanged.

## Sources and reconstruction

`Harbor/HarborLayout.json` holds the current Penpot field geometry. `JournalHarborBuilder` creates a reusable native uGUI prefab with separate TMP text, individual illustrations, scenery and transparent decorative frame layers. The Collection reuses the eight exact active-game Technique illustrations/frame. R055 supplies the engraved equipment and R057 supplies the current lossless 3840×2160 frame overlays. Earlier flattened screen mockups are not used as replacement UI. Source scenery remains lower-resolution illustrative review art; the 4K certification applies to the UI frame renders, not invented detail in the scenery.

All added Unity files live under `Assets/UIThemes/MidnightFieldJournal/Harbor/`. `JournalHarborFlow` owns presentation navigation only; existing `FishingRunController.StartRun` owns run reset and gameplay. `JournalResultModal` has an optional Harbor-return binding, preserving its previous restart behavior in scenes without the hub. The comparison scene builder reinstalls this prefab when regenerating the candidate scene. A fitted 16:9 stage preserves authored proportions on other window shapes.

## Verification

- 293 component/render assertions passed, including imported 4K texture dimensions, lossless settings, native text overflow, persistent routes and isolated captures at 4K and 1080p.
- 22 live assertions passed: actual pointer raycasts/navigation, clean initial Harbor, Set Sail, four-card fresh hand, empty chain, duplicate-start rejection, Surface/results, Harbor return, second fresh run and Descend.
- Unity compiled with no errors; final console error check was empty.
- Protected-file hashes confirm no changes to card definitions, gameplay rules, the original scene or build settings. See `Verification.json` for exact additions and allowed existing-file changes. A test-populated dynamic fallback font atlas was restored to its verified pre-task bytes.

`*-Unity4K.png` and `*-Unity1080p.png` are captures of the assembled native Unity pages. `ImportedSources.json` records exact source/target hashes; `PenpotLayout.json` preserves the source geometry; `UnityCheckpoint.json` records the resulting Assets hashes. `PrepareSources.py` refreshes inputs; use the Unity builder outside Play Mode, then `JournalHarborChecks.ValidateAndCapture()` and the Play Mode smoke check before rerunning `RecordIntegration.py`.
