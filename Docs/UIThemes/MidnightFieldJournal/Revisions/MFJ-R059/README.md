> **Imported and verified in Unity at [MFJ-SYNC-012](../../Sync012/README.md), 31 October 2026. No pending import remains.** Original design-only notes below are historical.

> **Stop-button refinement:** R060 updates both live bar examples in place. Original R059 previews below are historical; use the [R060 preview](../MFJ-R060/BarWithStop.png) and current manifest for the latest Stop.

# MFJ-R059 — Engraved Coastal Tag Bar

**Ready for visual review. Penpot only; not imported into Unity. R051 remains the active game design.**

Location: **01 - Current Design Library → 05 - Current Components and Prototypes → MFJ-R059 / Engraved Coastal Tag Bar / NEW REVIEW - not imported**. The review board is at **(70000, 1500)** and is selected/zoomed into view. The independent frame source is in **06 - Source Art and Reusable Masters** at **(70000, 4500)**.

The new variation follows the approved creature cards: aged ivory paper, fine copper/brown engraving, botanical coastal corners and a navy footer. All tags share one palette. There is no active-tag wash. The existing eight native tag glyphs were reused. The independent ivory-edged anchor marker stays above the track, and each ochre precision area rises from its own span's lower edge and fades toward its label.

The board shows one design twice: the initial eight equal 12.5% spans, and an illustrative mid-run arrangement. The latter is a geometry study, not a simulated hit history or gameplay change. Zone widths total 100%; precision bases remain 18% of their own spans.

## Reusable parts

- **Raster:** newly generated ornamental parchment/ink surround and texture; no baked tags, names, marker, precision centres or controls.
- **Native Penpot:** eight tag spans, labels, existing glyphs, boundary lines, editable gradient precision shapes, independent anchor cursor and Stop control/label.
- **Fonts:** existing Cinzel and Source Serif 4. No new font dependency.

The decorative source is **2172 × 724** and each example displays it at **2160 × 720**, without upscaling. The **3840 × 2160** review export is a presentation board, not a 4K frame master. Future Unity import must check final on-screen pixel size, preserve the existing narrow-zone icon/phase-label behavior, and export independent parts rather than flatten the whole bar.

## Validation and status

Actual Penpot exports of both examples and the 4K review board were inspected. Structural checks confirm 16 editable tag zones, normalized widths, centred proportional precision areas, an overlay marker and preservation of the original R051 board. Unity assets, prefabs, scenes and gameplay were not changed. Visual acceptance and integration remain separate future steps.

[Initial bar](InitialBar.png) · [Mid-run example](MidRunBar.png) · [4K review](DesignReview.png) · [Exact IDs](PenpotDesign.json) · [Verification and hashes](Verification.json).

Generated decorative artwork used the built-in imagegen tool with the approved R002 creature-card image as a style reference. No card image was sliced. Generation brief: one straight-on wide blank parchment UI frame; fine copper engraving, seaweed/coral corner ornaments, calm pale centre, navy footer, transparent exterior; no text, icons, compartments, precision areas, marker or controls. The dynamic layers were assembled separately in Penpot.
