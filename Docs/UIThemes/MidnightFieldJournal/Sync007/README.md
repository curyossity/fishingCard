# MFJ-SYNC-007 — Modified-stat tooltips

Imports the latest approved Penpot tooltip **MFJ-R045**, replacing R042–R044 proposals without importing their obsolete frames. Earlier imports and MFJ-SYNC-006.16 remain in the history.

Hover or focus a modified Weight/Value on the main creature or a Catch Chain creature to see its default, current value and named sources. Reasons show one flat icon and signed effective amount, with up to three rows visible. Scroll with the wheel, drag, keyboard arrows or directional arrow controls. Up appears above the reasons and down below; no scrollbar, numbered caption or icon backing is present. Bait's intrinsic zero and concealed stats do not open inspection.

The new independently assembled Penpot frame is imported as a sliced sprite with **64px borders**. Its shell-and-knot corners remain fixed at each tooltip height. Source names, numbers, icons and clipping stay native. All Techniques use the exported tied-hook glyph; all events use the wave-scroll glyph. Lionfish, Moonlit Ray, Jellyfish and Spiny Lobster use their exact exported Penpot silhouettes. Other approved creatures use a single-colour alpha silhouette of their own approved central illustration, fitted without cropping; no new or unapproved creature art is substituted.

`CardInstance` now retains effective contributions during the existing stat resolution. Permanent effects, active creature effects, events and pending Prize Hook/Upwelling previews preserve their real origin; zero-floor reductions report only their effective contribution. Snapshot and removal/expiry behavior retain source data. The tooltip groups repeated effects from the same named source; it does not reproduce effect calculations or infer causes from the final difference. Existing stat pacing, colour, bold emphasis, pop/tilt, chain card flash and newest-to-oldest sequencing are retained.

The candidate's two card prefabs contain the independent stat inspection targets. The normal theme builder invokes the same import/binding helper, so regeneration preserves the feature. The existing creature-effect tooltip remains available over the rest of each compact card. The modifier host handles live data, pointer-travel grace, clipping, focus, input locks and transitions on the dedicated tooltip layer.

Validation evidence:

- [Component checks](ComponentVerification.json): provenance conservation, clamping, expiry, snapshots, real pending previews, exact glyph lookup, slice borders and prefab targets.
- [Interaction checks](InteractionVerification.json): main/compact data, rendered glyphs, three-row clipping, first/middle/last arrows, wheel input, pointer travel, modal/input locks and concealed/Bait values.
- [Production smoke](ProductionSmoke.json): actual main-number raycast, actual tooltip-layer input and pending Prize Hook provenance.
- [Gameplay regressions](GameplayRegression.json): eight suites, 558 assertions. Three older controller fixtures were updated to explicitly choose Catch before their intended successful Descend; negative input-lock assertions remain direct. No production default-choice rule was weakened.
- [Main animation](MainAnimation/PlayModeVerification.json): 74 assertions. [Chain animation](ChainAnimation/PlayModeVerification.json): 103 assertions.
- [4K overflow capture](TooltipInGame.png) and [4K main hover capture](MainTooltip4K.png), visually inspected. The overflow capture uses disposable illustrative mixed-source data for layout validation, not a legal gameplay build.

The integration preserves scenes, authored cards, old-design art and existing fonts. No new package or commercial service is required. This is Editor/Play Mode verification, not a standalone-player build certification.
