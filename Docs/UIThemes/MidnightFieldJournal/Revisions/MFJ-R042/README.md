# MFJ-R042 — Modified-stat tooltip

**Superseded by MFJ-R045, imported at MFJ-SYNC-007. Historical source record; do not import this draft.**

Penpot-only design, 20 October 2026. **Ready for review; not integrated into Unity.**

On **Concept B — Reusable Assembly (4K)**, select **MFJ-R042 / Stat modifier tooltips / editable review**. The viewport is centered on that board. **Gameplay H / Value modifier tooltip / MFJ-R042** contains an editable in-game example. **PROTOTYPE / Hover modified Value / MFJ-R042** wires mouse-enter/leave on Lionfish's modified Value to an independent overlay; viewer playback itself was not tested.

The 900×1020 tooltip uses a new independent engraved parchment frame with dark marine foliage, shells, thin copper ornament and a clear text area. Cinzel headings and Source Serif 4 body text match the existing theme. All 46 text fields across the Value/Weight examples are native editable Penpot elements, as are the source icons. Signed amounts and named sources communicate changes independently of colour. No pointer triangle is added.

- Value example: Lionfish 30 + Prize Hook 8 − Venomous Spines 5 + Moonlit Reflection 9 = **42**, net +12. The transfer illustration assumes a released Garfish worth 9.
- Weight example: Cod 2 + Tangle 1 + Prize Hook 1 + Reef Snag 1 − Gentle Upwelling 1 = **4**, net +2. Upwelling persists after switching back to Catch. Positive Weight penalties are red; reductions are green.

The artwork is separate from stat name, creature name, default/current amounts, source categories, ability names, signed modifiers, explanations and net change. Future Unity integration must provide real runtime provenance for all effect sources, support variable row counts/stacking/conditional effects, respect hidden information and preserve the existing effect-inspection tooltip. Do not infer causes from the numerical difference or bake illustrative rows into production.

[Final review PNG](TooltipReviewV2.png), [editable geometry and IDs](PenpotDesign.json), [asset manifest](Assets.json), [verification](Verification.json), [generation prompt](GenerationPrompt.json). The review was exported and visually inspected; footer text was moved clear of corner ornaments and the Environment source uses rising currents rather than a Hazard symbol. Both tooltip variants have no containment violations. Full gameplay PNG export timed out; the editable gameplay board remains available in Penpot and is not claimed visually verified by that export.

The new frame was created with the built-in image-generation tool and saved at [Art/ModifierTooltipFrameV1.png](Art/ModifierTooltipFrameV1.png); no CLI/API-key fallback was used. It is 1190×1322, displayed at 900×1020 without upscaling. No font assets or licences were added. All files recorded by the latest Unity checkpoint were checked unchanged. The integration log records one pending revision and preserves all 37 completed imports and four superseded drafts.
