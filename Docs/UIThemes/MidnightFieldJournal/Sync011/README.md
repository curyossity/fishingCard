# MFJ-SYNC-011 — Ornate Discovery Journal

Integrated and verified on 30 October 2026. R058 is imported and complete; no pending Penpot imports remain. Earlier checkpoints and their evidence are retained.

Open `Assets/UIThemes/MidnightFieldJournal/Scenes/MidnightFieldJournalComparison.unity`, enter Play Mode and select Journal. The screen now uses the approved Penpot layout: detailed teal/copper and parchment frames, separate naturalist illustration, native editable labels and native emblems. It uses the existing game typography, with actual gold and quest progress replacing the mockup's illustrative values. All new assets remain in the candidate theme under `Harbor/Journal/`.

The Perfect School and The Collector display only COMPLETE/INCOMPLETE and show a check mark when completed. Risk Taker displays successful expeditions and three independently lit segments. Rewards remain descriptive and are never granted. The tracker, save schema, existing saved evidence, quest requirements and reward behavior are unchanged.

## Asset and reconstruction boundary

Imported ten independent R058 frame PNGs plus the separate 1086×1448 illustration. All textures retain native dimensions, uncompressed alpha, no mipmaps and a 4096 import ceiling. Frame resources are placed at 1:1 pixel scale within the fitted 3840×2160 stage. Their transparent short-frame padding is retained. The picture is displayed proportionally at 810×1080. No flattened full-screen mockup is used.

`JournalLayout.json` preserves actual verified Penpot field/frame geometry. `JournalHarborEmblem` reconstructs the source book, coin, anchor and check paths as native Canvas meshes. Status colours, check visibility and segmented progress are runtime UI. The new builder replaces only the Journal subtree of the existing Harbor prefab; other hub page identities/content and existing scene bindings remain intact. The original scene, build settings and gameplay/card data are unchanged.

## Verification

- All four hub pages captured at 4K and 1080p; 368 component/layout assertions passed, with no overflowing text.
- All 11 imported textures retained source dimensions with lossless settings; six native emblems are installed.
- Actual Surface completion updated the Journal and persisted through a fresh tracker, with no extra gold or quest unlocks. Test progress used an isolated temporary PlayerPrefs key, removed afterward.
- Partial and completed UI states passed: one versus three lit expedition segments, two completed single-expedition check marks, and actual gold binding.
- 26 live pointer/navigation/run-loop assertions passed, including Journal entry/return, Set Sail, Surface/results, Harbor return and a second fresh run.
- Final Unity console error check was empty. The scene is clean outside Play Mode. A test-populated dynamic fallback font atlas was restored to its hash-verified pre-task contents.

`ImportedSources.json` records exact source hashes. `ComponentVerification.json`, `StateVerification.json`, `LiveProgressVerification.json` and `PlayVerification.json` contain the test evidence; `Verification.json` combines it with protected-asset hashes. `journal-Unity4K.png` and `journal-Unity1080p.png` are the actual Unity renders. The immutable previous checkpoint is referenced by `UnityCheckpoint.json`.
