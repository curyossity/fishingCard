# Rig Building / Upgrade validation — 15 November 2026

The third phase is installed in the saved comparison scene as `Assets/UIThemes/TackleApparatus/Prefabs/BuildCanvas.prefab`. `FishingRunView.rigBuildingView` references the scene instance. Successful new-species Landing opens this canvas; committing a gap returns automatically to Discover. A duplicate upgrades its existing member with no placement or extra confirmation.

The initial installation failed because Penpot manifest entries without nine-slice borders deserialize as empty arrays. The importer now accepts null/empty borders as zero, validates four-element borders and stops on malformed nonempty arrays. The working prefab and saved binding are validated explicitly, preventing a fallthrough to the legacy Catch/Bait screen.

## Verification

- `RigBuildingChecks.ValidateRules()` — 76 assertions: one position per species, Levels 1/2/3 scoring, copy-value accumulation, reference/ID/order preservation, all-gap previews, no preview grants, contiguous-group breaks/restoration, removal, restrictions, independent acquisition/upgrade triggers, overflow rejection, extra attempts, one-Miss protection, immutable Landing snapshots and result/reset behavior.
- `RigBuildingChecks.ValidateAssets()` — 23 assertions: prefab layer, 18 native references, readable dark ability text on ivory, a pointer-blocking canvas, saved scene binding and reference scaling.
- Landing rules — 64 assertions; slot rules — 71 assertions.
- Discover artwork — 155 assertions; Landing artwork — 96 assertions.
- `RigBuildingSceneChecks` — real native strikes and controls, independent phase visibility, first-species placement, automatic duplicate upgrading, stale command rejection, side-effect-free position selection, long scrolling rigs, full failure payout including copies, and a clean next expedition with persistent wallet value retained. The assertion count is recorded in `LiveVerification.json`.

The scene checks set the existing debug save-reset guard before the run and enable background execution only in Play mode. They constrain only a runtime selector to deterministically exercise duplicate/new-species cases. They do not change authored eligibility, probabilities, creature definitions or player progression. Historical role-specific Landing scene fixtures describe the previous implementation; current full-loop coverage is the Build/Upgrade scene check.

`First-Species.png` and `Placement-Preview.png` show real post-Landing Build screens. `Build-1920x1080.png`, `Build-1280x800.png` and `Build-2560x1080.png` cover the current long-rig presentation. Existing Common/Placement art and the approved species sprites are reused; no newly generated artwork is required.

## Content boundaries

The primary-ability schema and level scaling are implemented. Illustrative abilities are tested with disposable definitions, not silently assigned to real species. The final species ability catalog, maximum level, no-legal-position resolution, biome completion predicates and permanent completion reward tables remain open. Ordinary encounters complete the Discover → Land → Build/Upgrade loop; the existing special-biome route retains its separate design boundary.
