# MFJ-R050 — Lantern Focus, parchment refinement

**Superseded by MFJ-R051.** Historical refinement only; the current anchor cursor and in-bar fading precision design is [R051](../MFJ-R051/README.md). Earlier review notes below describe the previous state.

**Ready for review. Penpot-only continuation of the selected R048 Lantern Focus, exported and visually checked on 28 October 2026.**

The user selected **03 — The Lantern Focus**, requested the lighter parchment palette, permanent centre marks for every tag, removal of the current-tag highlight, and a more visible marker.

The new editable1600×400 board is cloned from R048's exact Lantern Focus source. The dark paper wash is removed. Every tag uses the same warm parchment channel, dark ink text and monochrome glyphs; no active-tag wash remains. All eight centre sections are marked by matching small ochre brackets below the labels. Each stays18% of its parent span, matching the previous visual studies; this is not a new probability setting.

The independent cursor has a larger dark diamond above the channel and an upward pointer below it, both outlined in ivory. They align at the same horizontal position and leave the icons and names unobstructed. The Stop control uses the normal dark face on parchment. No motion or gameplay logic is implemented.

## Location

**Concept B — Reusable Assembly (4K)** → **MFJ-R050 / Lantern Focus / light parchment refinement**, at (40250,25000). The original R048 source and all rejected alternatives remain available as historical boards. Exact source and current IDs are in `SourceDefinition.json` and `PenpotDesign.json`.

## Tracking

R050 continues the chosen R048 concept. R047, R048 and R049 are superseded design studies, retained for reference and excluded from a future import checklist. Their `unityImported` flags remain false. The39 completed integrations retain their existing statuses; this task does not change Unity. `UnityBefore.json` records442 existing Unity file hashes.

Native checks and both actual Penpot exports passed. The eight centres are visible and aligned, no active-tag fill remains, and the cursor is clearly visible without obscuring the icons or names. `Action-audit.json`, `VisualReview.json` and `Verification.json` record the checks. All442 Unity file hashes remain unchanged.

[Bar preview](LanternFocusParchment.png) · [Review board](DesignReview.png).
