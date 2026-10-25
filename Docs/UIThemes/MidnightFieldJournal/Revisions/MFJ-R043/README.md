# MFJ-R043 — Compact modifier tooltip alternative

**Superseded by MFJ-R045, imported at MFJ-SYNC-007. Historical source record; do not import this draft.**

Penpot only, 20 October 2026. **Ready for review; not imported into Unity.** R042 remains available for comparison. Choose one tooltip variant before any integration; do not import both as competing runtime tooltips.

On **Concept B — Reusable Assembly (4K)**, select **MFJ-R043 / Compact modifier tooltips / 1–3 sources**. The adjacent **MFJ-R043 / Overflow / three visible + scrolling** shows the first and last positions of a four-source list.

The tooltip contains only default/current Value or Weight and source rows: portrait/icon, source name and signed amount. Ability names, explanations, creature headings and the net-change footer have been removed. Cinzel and Source Serif 4 retain the established typography. Beneficial changes are green; detrimental changes are red, with signed amounts also communicating direction.

At the 3840×2160 design scale, width is 680; height grows from 348 to 420 to 492 for one, two or three sources. With overflow, height is 536 and a 216-pixel viewport shows at most three 72-pixel rows. A narrow scrollbar and “1 more below” / “1 above” indicators expose the fourth reason. Current always includes **all** sources, including those outside the viewport.

- One-source Value example: 12 + Prize Hook 8 = **20**.
- Two-source Value example: 30 + Prize Hook 8 − Lionfish 5 = **33**.
- Three-source Value example: 30 + Prize Hook 8 − Lionfish 5 + Moonlit Ray 9 = **42**.
- Overflow Weight example: 2 + Jellyfish 1 + Prize Hook 1 + Reef Snag 1 − Gentle Upwelling 1 = **4**.

These are illustrative examples of existing effects, not new mechanics or hardcoded production attribution. The Moonlit Ray example assumes a prior transfer from an Elusive Catch worth 9. The Upwelling recipient has since changed to Catch, preserving its modifier.

## Reusable frame

The original independently generated R042 frame is reused through **nine native clipped regions**. Fixed corners retain their detail; edges and parchment resize independently. This is an asset-frame subdivision, not a slice from a flattened mockup. All text and icons remain separate editable elements. The **MFJ-R043 / 9-slice assembly / fixed corners** board includes the source, cut guides and a blank assembled module.

Source is 1190×1322. Source borders: left/right 320, top/bottom 340. Display caps: left/right 80, top/bottom 85 at quarter source scale. A future Unity Sliced Image must preserve this cap scale (for example, a pixels-per-unit multiplier of 4 when sprite and reference PPU match), rather than displaying the source borders at full size.

## Interaction boundary

The two separate **PROTOTYPE / R043 / scroll down/up** boards wire clicks on their overflow indicators to the opposite scroll state. This is a scroll-state simulation, not live mouse-wheel scrolling or a draggable thumb implementation. Viewer playback has not been tested. The clipped overflow states themselves were exported and visually inspected.

Future Unity integration must use an actual clipped ScrollRect, show at most three rows, update the thumb and hidden-source counts, route wheel input to the tooltip, allow pointer travel into it, support focus navigation and clamp/flip placement. Bind truthful runtime modifier sources, respect concealment, and preserve existing creature-effect tooltips and numeric animation. No Unity code, assets, scenes or prefabs were modified for this revision.

[Compact preview](CompactReview.png) · [Overflow preview](OverflowReview.png) · [9-slice guide](NineSliceGuide.png) · [Penpot IDs](PenpotDesign.json) · [Asset contract](Assets.json) · [Verification](Verification.json)
