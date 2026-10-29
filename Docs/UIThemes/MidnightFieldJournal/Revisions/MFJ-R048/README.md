# MFJ-R048 — A Quieter Cast

**Superseded by MFJ-R050.** Retained as historical design evidence; excluded from future import. The selected Lantern Focus is continued in [R050](../MFJ-R050/README.md). Earlier status notes below describe the historical review.

**Ready for review. Three new initial-state Penpot alternatives, exported and visually checked on 28 October 2026. No Unity changes.**

The user requested three independent, clearer ideas with a shared palette instead of eight tag colours. These are additional studies: MFJ-R047 stays available for comparison and is not marked integrated or superseded.

1. **The Engraved Ruler:** one continuous sea-ink rail on parchment. Fine separators define the eight spans, each identified by a name and monochrome glyph directly beneath it. Precision is a short 4px gold notch along the rail edge rather than a filled stripe.
2. **The Tackle Labels:** identical paper tackle labels suspended from one fishing line. The float moves on the timing rail beneath the labels; short gold stitches indicate precision. The paper forms and fastening details provide character without assigning colours to tags.
3. **The Lantern Focus:** a dark instrument with eight equal spans. The tag under the marker receives one pale highlight; only that tag exposes its fine precision guide. Other precision areas remain mechanically present but visually quiet. This is a visual proposal for progressive disclosure, not a change to the precision mechanic.

Exactly one initial bar per idea, each1600×400. All eight spans remain12.5%. Precision remains an illustrative18% of its parent, matching R047 for comparison; no odds, resizing or gameplay are implemented. No percentage labels, separate legend or repeated miniature width readouts appear in the bars. Icons, names, marker, precision cues, native Stop button and tag regions remain independent editable Penpot elements. The existing clean parchment texture and established Cinzel/Source Serif4 fonts are reused; the frame and tackle-label shapes are new native vectors.

## Location

**Concept B — Reusable Assembly (4K)** → **MFJ-R048 / Quiet tag bars / three initial-state ideas**, at (36000,25000), beside R047. `PenpotDesign.json` records the actual IDs. No new page is required, preserving the plugin connection.

## Verification

Creation, native geometry checks and actual Penpot PNG exports are complete. All three individual row exports and the full comparison were visually inspected. `UnityBefore.json` preserves442 existing Unity file hashes for comparison; all remain unchanged and the active checkpoint remains MFJ-SYNC-008.5. See `Action-audit.json`, `VisualReview.json` and `Verification.json`.

[Full comparison](DesignReview.png) · [Engraved Ruler](EngravedRuler.png) · [Tackle Labels](TackleLabels.png) · [Lantern Focus](LanternFocus.png).

Lantern Focus is the strongest option for reducing persistent visual noise; the ruler keeps all precision targets visible for advance planning. Choosing between them is a visual review decision, not an implemented mechanic. No version is approved for import yet. R047 and R048 are both logged as pending design studies, and earlier Unity integrations remain completed.
