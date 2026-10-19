> **Imported and verified at [MFJ-SYNC-004](../../Sync004/README.md). No remaining Unity import work.** The original design-only notes below are retained as history and do not indicate pending work.

# Descend and Surface — MFJ-R036

**Penpot only; ready for visual review. Not imported into Unity.**

[Pair preview](ActionPair.png) · [Descend face](DescendFace.png) · [Surface face](SurfaceFace.png) · [Source and instance IDs](PenpotDesign.json) · [Verification](Verification.json) · [Hashes](Exports.json) · [Generation prompt](GenerationPrompt.json).

Both controls are exactly **880×350**, with the same independent raised bronze/navy enamel housing. Fine wave engraving, slotted corner screws, bevels and stepped endcaps give a marine hardware character distinct from the illustrated parchment cards. Descend has copper lettering/double-down chevrons; Surface has ivory lettering/double-up chevrons. Decoration stays outside the label and icon. Title is Cinzel Semibold74; exact source geometry is recorded in PenpotDesign.json. Both final static face PNGs are880×350. The original transparent housing is1989×791, aspect-preserved at the 4K design footprint; it was newly generated using the built-in imagegen tool, never sliced from a concept image.

The review board `MFJ-R036 / Descend + Surface / marine hardware review` is selected on `Concept B — Reusable Assembly (4K)`. Blank source housing, vector direction symbols and editable title fields remain separate in Penpot. Eight replacements appear in the four current gameplay assemblies, at their former top-left positions. External hints are unchanged. Old face shapes are hidden for comparison, not deleted. Readback verified equal sizes, correct text, vector icons and hidden replaced faces. The pair preview was visually inspected.

For a future requested Unity import, replace only candidate Art/Controls/DescendButton.png and SurfaceButton.png with the separately exported faces while preserving asset GUIDs; update source hashes and prefab/profile/builder dimensions to880×350 for both. Keep existing gameplay callbacks, legality, disabled/pressed/navigation feedback and the no-pointer-hover-frame contract. Do not bake the pair, hints or a gameplay screenshot into a single runtime image. Shared housing and editable sources support future higher-resolution/localized exports. This follows the existing static action-label exception; do not duplicate the words as extra runtime labels over these PNGs.

Unity assets/scenes were not modified. MFJ-R031–R035 stay pending;26 prior revisions stay imported and4 historical drafts stay superseded. New revisions start atMFJ-R037.
