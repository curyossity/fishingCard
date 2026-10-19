> **Imported and verified at [MFJ-SYNC-004](../../Sync004/README.md). No remaining Unity import work.** The original design-only notes below are retained as history and do not indicate pending work.

# Engraved Line Load — MFJ-R035

**Penpot only; ready for visual review. Not imported into Unity.**

[Four state previews](LoadStates.png) · [Assembled load and preserved haul](AssembledLoadAndHaul.png) · [Exact sources and instance IDs](PenpotDesign.json) · [Verification](Verification.json) · [Asset dimensions/hash](Asset.json) · [Original generation prompt](GenerationPrompt.json).

The new independent brass frame uses rope engraving, fine corner rivets, kelp endcaps and a dark navy face. Ornament stays outside live text and segments. The housing is a newly generated transparent image using the built-in imagegen tool; it was not sliced from a mockup. Editable Cinzel/Source Serif 4 fields and native segment shapes show exact load/capacity and capacity status. Empty (0/8), normal (4/8), at capacity (8/8) and overloaded (10/8, +2) examples distinguish states through text and numbers as well as color. These are visual examples, not new mechanics.

The source review board is `MFJ-R035 / Engraved Line Load / state review` on `Concept B — Reusable Assembly (4K)` and is selected in Penpot. A separate adjacent board preserves the original blank housing. Four current gameplay layouts received the replacement at their existing load region. Old label/value/pips are hidden for comparison, not deleted. Haul fields and divider are untouched. Readback verified four variants, four assembly instances, hidden old meter shapes and visible haul fields. The full gameplay PNG export timed out; the state review export was inspected successfully.

At the 4K design scale the meter is 760×175 within the existing 760×340 combined load/haul region. Source image2103×748 is aspect-preserved at760×270.319 with Y−48, accounting for its transparent margins; its pixels are not cropped or resampled. Geometry and native field IDs are recorded in PenpotDesign.json. The original font licenses are unchanged.

For a future requested Unity import: add only the separate housing sprite and its mapping to the candidate theme, update the LineLoadMeter prefab/builder and dynamic view, and verify CatchChainView/JournalCatchChainPresentation/canvas consumers. Generate segments from real capacity and size them to the same rail, clamp graphical fill only, retain actual overloaded numerals and explicit excess. Use existing resolved load/capacity and overload rules; do not introduce a new capacity, tension or energy mechanic. Preserve haul and other gameplay presentation. State/assembly PNGs are reference-only. Previous26 imported and4 superseded revisions remain unchanged; the four pending Technique revisions remain pending.
