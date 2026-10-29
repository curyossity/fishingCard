# MFJ-R047 — Dynamic Fishing Tag Bar studies

**Superseded by MFJ-R050.** Retained as historical design evidence; excluded from future import. The selected Lantern Focus is continued in [R050](../MFJ-R050/README.md). Earlier status notes below describe the historical review.

**Ready for visual review. All six bars created in Penpot and their actual exports checked on 28 October 2026. Not imported into Unity.**

The requested scope is exactly three visual variations, each with one static initial bar and one static mid-run bar. No prototype, gameplay implementation, animation frames, extra bar states or Unity changes.

1. **The Field Journal:** pale parchment, shell engraving, a continuous sea-ink ribbon, exterior tag emblems and manuscript-style legend.
2. **The Night Sounder:** dark sea-chart instrument, fine compass engraving, saturated colour intervals, bright precision windows and an ivory needle.
3. **The Naturalist’s Ledger:** botanical parchment, tall segmented tag panels, coloured line illustrations and lower precision slots.

All six bars are 1500×480 design pixels, using separate native text, icons, zone fills, centred gold Precision Areas, marker and Stop control. Initial widths are 12.5% each. The illustrative mid-run widths are Fish 14 / Predator 10 / Schooling 14 / Small 6 / Heavy 14 / Anchored 14 / Armored 14 / Elusive 14 percent, using the user-confirmed normalization interpretation. Precision occupies an illustrative 18% of each parent zone in these visual studies; no rarity-probability bonus is invented.

Icons and labels supplement colour, and labels sit outside narrow intervals. The two mid-run precision examples and one ordinary tag-hit example show distinct feedback without creating extra bar states. The instrument is intended for the existing pre-reveal encounter area, clearing before the creature card appears.

**Confirmed normalization:** eight zones that sum to 100%, each capped at 12.5%, can only remain equal. The proposed interpretation treats 12.5% as the recovery baseline before normalization, permitting normalized widths above it while retaining a 5% final floor. The user approved this baseline-before-normalization interpretation. No runtime rule is implemented here. The static mid-run widths are illustrative and do not certify a particular hit/recovery sequence.

`CreateDesign.js` creates one dedicated review board on Concept B — Reusable Assembly (4K) and six editable native bars in the existing Penpot file. Each bar is created as a separate action to keep operations bounded. Saved IDs, actual exports and verification are recorded alongside the source. The current Unity checkpoint remains unchanged.

## Where to find it

On **Concept B — Reusable Assembly (4K)**, select **MFJ-R047 / Dynamic Fishing Tag Bar / three static variations**, at (32000,25000), size3400×2530. Each row contains one initial and one mid-run bar; exactly six bars. The attempted separate-page switch did not persist, so the final review lives on the existing assembly page. `PenpotDesign.json` contains the saved IDs.

`Action-audit.json` checks all48 native zone widths and precision centers, plus one independent marker per bar. `UnityBefore.json` records442 existing Unity file hashes for comparison; the current Unity checkpoint remains MFJ-SYNC-008.5.

## Review evidence

[Full comparison](DesignReview.png) · [Field Journal](FieldJournal.png) · [Night Sounder](NightSounder.png) · [Naturalist’s Ledger](NaturalistsLedger.png).

Exports succeeded after the Penpot tab was brought to the foreground. All four exports were visually inspected; the legend spacing and text clearance corrections are present. See `VisualReview.json` and `Verification.json`. No variation has been selected for implementation; MFJ-R047 remains a pending design study, while earlier integrations retain their completed status.
