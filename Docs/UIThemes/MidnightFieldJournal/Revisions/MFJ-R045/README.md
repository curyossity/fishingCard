# MFJ-R045 — Arrow cues, ink icons and resize-safe engraving

**Imported and verified at MFJ-SYNC-007.** [Integration and validation](../../Sync007/README.md).

This revision replaces R044's visual contract:

- Scroll indicators are arrows only: up above the reasons, down below them, both in a middle position. No counts, above/below text, scrollbar or slider.
- Icons have no background tile. Creature sources use individual single-colour silhouettes. Every Technique uses the same tied-hook glyph; every event uses the same wave-scroll glyph, including Hazards and Environments.
- The new frame has compact engraved shell-and-knot corners contained entirely inside fixed 64×64 regions. Straight double rules form the stretchable edges. Only clean parchment fills the middle; no foliage or curved decoration crosses a slice boundary.

The native blank master is 680×536, with 64px borders on all four sides. It can be exported whole for Unity's Sliced Image; do not use the earlier R042 frame or its 320/340px borders for this variant. Fonts, source text and source icons remain editable and separate. No generated bitmap or new font asset is required.

The compact review board shows one, two and three sources. The overflow board shows first, middle and last positions of five reasons through a three-row clipped viewport. Current values include all sources. The example Weight arithmetic is 2 + Jellyfish 1 + Prize Hook 1 + Reef Snag 1 − Gentle Upwelling 1 − Spiny Lobster 1 = 3. The current creature is in Bait mode, allowing Jellyfish's persistent weight and Upwelling to coexist with Lobster support. Example Value arithmetic remains illustrative, not production attribution.

The prototype arrows navigate between three scroll-state boards. This simulates scrolling in Penpot; actual wheel/focus/scroll behaviour belongs to future Unity integration. Arrow visibility must follow the real scroll position and content bounds. The current design uses native paths and clipping, not text baked into a screenshot.

R045 was selected and integrated at MFJ-SYNC-007. R042?R044 remain historical proposals and must not be imported. Earlier integration checkpoints are preserved.

[Compact preview](CompactReview.png) · [Scroll positions](OverflowReview.png) · [Blank 9-slice master](TooltipFrameMaster.png) · [Penpot IDs](PenpotDesign.json)
