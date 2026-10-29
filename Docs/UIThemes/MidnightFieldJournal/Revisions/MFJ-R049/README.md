# MFJ-R049 — Three Ways to Read the Sea

**Superseded by MFJ-R050.** Retained as historical design evidence; excluded from future import. The selected Lantern Focus is continued in [R050](../MFJ-R050/README.md). Earlier status notes below describe the historical review.

**Ready for review. All three initial-state concepts are created in Penpot, exported and visually checked on 28 October 2026. No Unity integration.**

The user requested genuinely different designs, retaining the game's theme and the quieter shared palette. R047 and R048 are preserved for comparison.

1. **The Tide Dial:** a broad crescent instrument with names and monochrome glyphs following the outside of its engraved rail. An ivory needle rises from a single pivot. Fine arc notches indicate precision; no eight-colour fill or duplicate legend.
2. **The Line Spool:** a parchment ribbon passes through a dark spool viewport beneath a fixed ivory index. Only the current tag and its two neighbours are visible. All eight tags exist on the ribbon; the others are clipped outside the window. The current centre stitch is the precision cue. This explores relative motion of the ribbon rather than repeating the same eight labelled rectangles.
3. **The Sounding Chart:** a small engraved sea chart with one meandering sounding route, eight named bearings and an independent boat marker. Labels alternate above and below the route. Small sounding targets provide precision cues without coloured bands.

These are **visual and interaction-form proposals**, not approved changes to Unity's motion or selection rules. Crescent travel, a moving ribbon beneath a fixed index and a boat following a curved route would each need a deliberate implementation choice if selected. The proposals retain the same eight tag identities and eight equal12.5% initial phases; precision is illustrated at18% of each phase, matching the previous studies for comparison. No odds, shrinking/recovery rules or creature pools are implemented.

Exactly one initial-state mockup per idea, each1600×600. Native text, glyphs, path geometry, marker, precision shapes and Stop controls remain independently editable. The established Cinzel/Source Serif4 fonts and approved clean paper grain are reused. The instrument geometry is new native vector work.

## Location

**Concept B — Reusable Assembly (4K)** → **MFJ-R049 / Fishing instruments / three new forms**, at (38200,25000), beside R048. Actual IDs are saved in `PenpotDesign.json`.

## Verification

Native checks confirm all three concepts and their independent markers exist, with eight equal logical tag phases each. The spool viewport exposes three neighbouring tags; the other concepts show all eight. `Action-audit.json` records those checks. Exports succeeded with Penpot active; all three row PNGs and the full comparison were inspected for readability, clipping, marker/text clearance and composition. `VisualReview.json` and `Verification.json` record the completed checks. The ledger status is `ready_for_review`.

`UnityBefore.json` records442 existing Unity file hashes; all are unchanged. The active Unity checkpoint remains MFJ-SYNC-008.5, and prior imports retain their completed status. R047, R048 and R049 remain separate pending visual studies; none is imported by this task.

[Full comparison](DesignReview.png) · [Tide Dial](TideDial.png) · [Line Spool](LineSpool.png) · [Sounding Chart](SoundingChart.png).
