# MFJ-R053 — Ornate Harbor style study

**Imported and verified at MFJ-SYNC-010.** [Unity integration](../../Sync010/README.md). Earlier review-only status below is historical.

One revised main Harbor screen, created on 2026-10-30 in response to the request to match the existing creature cards more closely. The other three Harbor views have deliberately not been redesigned yet.

## Penpot

Page: **Concept B — Reusable Assembly (4K)**. Board: **MFJ-R053 — Harbor / Ornate card-style study**, at `(48600,31600)`, size `3840 × 2160`. Exact identifiers and editable field IDs are in [PenpotDesign.json](PenpotDesign.json).

The frame uses layered worn parchment, copper filigree, navy textured panels, engraved seaweed and shoreline rocks, taking the approved R002 creature frame as its visual reference. Native Cinzel and Source Serif 4 labels remain editable. The three destinations, gold balance, loadout summary and Set Sail action are visible. Displayed balances and loadouts are illustrative.

## Scope and production boundary

This is a visual approval study: the scenery and decorative framing are one newly generated, text-free static composition, with separate Penpot text. It is not a production sprite sheet or a fully separated reusable asset set. The generated raster is 1672 × 941; the board and review export are 3840 × 2160. The 4K export does not make the raster native 4K.

After style approval, recreate the individual decorative panels, background and button assets independently at production resolution; do not slice them out of this composition. Retain dynamic text and UI state as native Unity elements. No Unity files, prefabs, scenes, code or assets were changed for this revision.

R052 remains available for structural comparison, but its visual direction is superseded. Its expanded screens are historical references and must not be automatically imported as approved designs. R053 is pending visual review and has not been integrated into Unity.

## Review evidence

- [Harbor preview](Harbor.jpg)
- [4K Penpot export](Harbor4K.jpg)
- [Blank generated composition](Art/HarborOrnateBlank.png)

The exported screen was visually inspected for panel text placement, contrast and decoration collisions. An unnecessary subtitle below Set Sail was removed because it crossed the bottom engraving. No new paid plugin, stock service or font was introduced.
