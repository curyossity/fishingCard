# Landing integrated housing validation — 17 November 2026

The second phase now uses a single blank integrated instrument housing based on the third phase's machine master. Dynamic creature art, labels, Landing Points, Resistance, progress, fish, scoring zones and attempt lamps remain independent. Gameplay rules and tuning are unchanged.

The **1739 × 904** Landing and Build sources have identical alpha at every pixel. Outside the two documented working recesses, **720,063 protected pixels** retain the original shell; all visible color comparisons pass. Both machines draw at **490,163 / 1310 × 681** via `TackleMachineGeometry`, on identical 1920 × 1080 Expand stages. The common corners, shafts, vent, dial, title plaque and casing will not change shape at a future phase transition.

The normalized source imports byte-for-byte. Separate portrait/score/track/status background images are absent from the Landing prefab. The original isolated Penpot component files remain preserved for provenance and other uses. Native text, the generic fish indicator and variable lamps are not painted into the machine.

- `Landing-1920x1080.png`: populated Landing screen and registered native indicators.
- `Landing-1280x800.png` and `Landing-2560x1080.png`: aspect-ratio captures.
- `LiveVerification.json`: live artwork assertions, including pointer hit targets, button feedback, the uncapped rig, preserved state and exact model-driven fish/zone widths.
- `Verification.json`: current static, full-loop, metadata and shell-comparison results.

Captures use a disposable cloned canvas, preserving the live scene's clipping and camera state. Persistence is disabled for gameplay fixtures. Full-loop checks exercise real Landing success/failure, new-species placement, automatic duplicates, long-rig scrolling, full payout and clean subsequent expeditions.

[Source, prompts and normalization](../../UIThemes/TackleApparatusConcept/LandingIntegrated/README.md) describe the exact shared-shell contract. The first phase has not been regenerated in this increment; its older housing is not claimed to match these two phases.
