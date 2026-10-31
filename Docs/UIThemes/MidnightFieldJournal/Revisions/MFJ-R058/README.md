# MFJ-R058 — Ornate Discovery Journal

**Imported and verified at MFJ-SYNC-011.** [Unity integration](../../Sync011/README.md). Earlier pending status below is historical.

**Penpot design, ready for review; not integrated into Unity.** Created 30 October 2026.

Page: **Concept B — Reusable Assembly (4K)**. Board: **MFJ-R058 — Discovery Journal / Ornate 4K**, at `(53000, 34000)`, size **3840 × 2160**. It sits beside the current Technique Collection screen. The board is selected and fitted in the editor. `PenpotDesign.json` records exact IDs. The current Harbor, Shop and Collection Journal links navigate to this board; its top navigation and Back to Harbor return to those screens.

The screen matches the R053/R055/R056 family: midnight teal engraved surfaces, aged ivory title plaques, marine botanical corners, copper rules, Cinzel headings and Source Serif 4 body text. A new, separate naturalist's journal illustration shows specimen drawings, a lantern and the harbor at dusk. It is not a full-screen raster mockup.

## Editable structure and progress states

Three independently grouped quest rows contain native editable quest names, requirements, rewards and status. The Perfect School is shown Incomplete; The Collector is shown Complete. Neither has a partial count or progress bar. Risk Taker shows an illustrative `1 / 3 successful expeditions` counter and three native progress segments. These demonstrate the approved UI policy, not player data. Rewards explicitly say **not yet granted**, with no claim control. The `920` gold balance is an editable layout example; Unity must continue binding actual gold.

All major frames are separate reusable image elements. The illustration, native emblems, native rules, native status/check and segmented tracker remain independent. The companion **MFJ-R058 / Reusable Journal sources** board holds the ten source frame assets and separate illustration. No Unity script, prefab, scene or asset changed.

## Sharpness and handoff

Frames are rebuilt from the approved R057 Dark/Paper masters using their `[300,500,560,500]` source border contract. Chromium rasterizes each independent frame at its actual 4K display footprint. Fourteen frame instances use ten separate PNG resources at **1:1 pixel scale**; decorative corners never exceed **0.44×** source size. No full-screen image is enlarged, and no frame is cut from a screen mockup. Small frame resources have transparent bottom padding to accommodate Chromium's minimum viewport height; retain their recorded image dimensions/positions rather than stretching that padding into the visible frame.

`FrameGeometry.json`, the individual `Frames/*.html` and `Frames/*.png` files are the reusable construction sources. `Art/JournalDesk.png` is 1086 × 1448, displayed proportionally at 810 × 1080; it is downsampled, not enlarged. `Journal4K.png` is the lossless export of the actual Penpot board. `PenpotVerification.json` reports no content outside the screen and no overflowing text. All imported images remain separate from native labels.

[Lossless 4K screen](Journal4K.png) · [Preview](JournalPreview.png) · [Verification](Verification.json) · [Generation prompt](GenerationPrompt.md).

Future Unity integration should replace only the current Journal page's visual assembly, preserve its existing three-quest tracking/save behavior and keep rewards disabled. Existing completed imports remain completed; R058 is one new pending revision. No new paid plugin, stock asset service, font or external image API is introduced.
