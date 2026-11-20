# Tackle tag preparation

20 November 2026. New tag-selection screen designed and integrated into the existing Harbor Set Sail flow. See the [design record](../../UIThemes/TackleApparatusConcept/TagPreparation/README.md).

## Installed behavior

Five illustrated tag plates use the exact symbols shown on the Discover reels. Selected tags have ivory faces, lit lamps and **In your reels** text. Unselected tags have sage faces, unlit lamps and explicit text. Shared tag philosophies remain readable below every plate. A four-lamp count readout controls Set Sail readiness; draft editing cannot select a fifth symbol.

Back or the native Cancel action discards the draft and restores Harbor interaction/focus. Reopening restores the controller's prepared selection. Repeated Set Sail requests preserve an open draft. Confirm commits exactly four symbols, starts one expedition and freezes those symbols for Discover; stale confirmation cannot restart an active run. A failed start keeps the draft editable and shows its error in the guidance region without overflowing the counter.

The screen uses an independent 1920 × 1080 Expand root canvas at sorting order 500. It does not inherit the old Harbor's resolution or layout. The saved scene retains one preparation instance and the existing Harbor/controller bindings. Its prefab GUID is preserved. Existing slot/Harbor authoring reuses the standalone instance through the explicit `JournalHarborFlow.SlotPreparation` binding. Native UI focus, press feedback and direction navigation use the current game's Tackle controls.

## Verification

- [Assets and rule results](AssetsAndRules.json): 116 preparation asset/binding/theme assertions and 69 slot-rule assertions passed.
- [Live preparation results](LiveVerification.json): 71 assertions passed in a fresh comparison Play session, with progression persistence disabled. Tests cover all five possible exclusions, empty/partial/full states, invalid confirmation, fifth-symbol rejection, draft isolation, duplicate open requests, native Cancel, Back, tag swaps, failed starts, actual pointer raycast destinations, directional focus routes, text fit, exactly-once expedition start and frozen active symbols.
- [Expedition regression](ExpeditionRegression/LiveVerification.json): 102 live Discover/Land/Build/Upgrade assertions passed after the new preparation handoff, including new-species placement, duplicates, longer rigs, stale input, failure payout/cleanup and an empty next expedition.
- [Project integrity](ProjectIntegrity.json): 1,824 Unity files checked, zero metadata/GUID/script errors or warnings. The final live runs reported no Unity Console errors.

Representative native captures: [empty](Empty-1920x1080.png), [one more tag needed](Choose-One-More.png), [selected at 1920 × 1080](Selected-1920x1080.png), [1280 × 800](Selected-1280x800.png), [2560 × 1080](Selected-2560x1080.png) and [failed start](Start-Failure.png). The test submits an actual UI render before pointer checks because an unfocused Editor can defer Game-view rendering and leave native graphic depths uninitialized.

Repeat `TackleTagPreparationChecks.ValidateAssets()` outside Play mode. In a fresh comparison Play session, call `TackleTagPreparationChecks.Begin()` and inspect `Result`. The separate full-loop fixture is `RigBuildingSceneChecks.Begin("Docs/Validation/TagPreparation/ExpeditionRegression/")`.
