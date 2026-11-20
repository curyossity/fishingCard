# Insertion tray combination inspection

Implemented 20 November 2026 on the restored master branch, independently of the alternate scoring-polish changes.

Hover or focus an existing creature in the insertion tray:

- A gold glow identifies the inspected creature.
- Green glows identify its positional supporters or continuous sale-value recipients in the selected insertion preview.
- Red glows identify previously connected supporters/recipients disconnected by that insertion.
- The existing tooltip explains the colors and names the related creatures. Lists summarize after three names to keep uncapped rigs readable. Conditional strike requirements are explicitly retained.

The newly landed portrait also glows when it supplies a preview link. Offscreen cards remain clipped and are never automatically scrolled by these highlights. Cabinet inspection retains its existing behavior. Escape/dismissal, scrolling, source recycling and phase exit clear the highlights. Placement still requires the existing explicit command; hovering and previewing grant nothing.

`RigRuntime.InspectAbility` reuses the gameplay count/support/target methods on an independent snapshot. No definitions, scoring, economy, acquisition triggers or progression are modified. `RigCombinationPreview` owns pooled native halos drawn by `RigRelationshipGlow`; `RigCombinationPreviewSetup.Install()` installs into the existing Build prefab. Standard Build and tooltip generation retain its serialized bindings.

Visual follow-up: corner frames were replaced with diffuse glows, following the game-feel and uGUI skills. A 140 ms ease-out reveal settles into steady illumination without repeating pulses or card movement. The native vertex-alpha mesh has a transparent center and 28-pixel feathered falloff; it needs no bloom, extra material or generated texture. The installed prefab/relationship checks (23) and live interaction checks (14) were rerun successfully with no Unity Console errors. Project integrity checked 1,834 files with zero errors/warnings. Captures show the revised glows.

Validation: `RigCombinationPreviewChecks.Validate()` checks installed Schooling boundaries, full/partial insertion breaks, preview purity, Anchored value-target boundaries and native bindings. `RigCombinationPreviewChecks.Begin()` runs in a fresh comparison Play session with persistence disabled, reaching Build through actual Landing and testing pointer/focus inspection, selected-gap changes, named explanations, dismissal, scrolling, same-source reentry and placement cleanup. See [live result](LiveVerification.json).

Results: 23 relationship/asset assertions, 76 existing rig-rule assertions, 374 installed-ability assertions, 45 tooltip-asset assertions, 14 live combination assertions and 55 live tooltip regression assertions passed (587 total). Native captures reviewed at 1920 × 1080 and 1280 × 800; 2560 × 1080 capture is also supplied. Unity Console reported no errors; project integrity checked 1,832 Unity files with zero errors or warnings. No implementation remains in this change's scope.

Captures: [connected school](Connected-1920x1080.png), [broken school](Disconnected-1920x1080.png), [1280 × 800](Disconnected-1280x800.png), [2560 × 1080](Disconnected-2560x1080.png).
