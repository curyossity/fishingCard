> **Imported and verified at [MFJ-SYNC-003](../../Sync003/README.md) on2026-10-19.** The design-stage text below is retained as history; this revision is no longer pending.

# Restored shallow Technique fan — MFJ-R030

**Penpot only; not imported into Unity. Supersedes rejected MFJ-R029.**

[Preview](FanPreview.png) · [Layout](FanLayout.json) · [Geometry and instance IDs](PenpotDesign.json) · [Verification](Verification.json).

The middle cards remain exactly at R029's approved height and orientation: Y450, clockwise angles−5/+5°. Their preview image bytes and assembled shape IDs are unchanged. The outer cards move down20 to Y500 and soften from−15/+15° to−8/+8°. X centers remain500/1000/1500/2000, with the same40° laid-down table tilt,2000px perspective and2500×866 viewport. Artwork, typography, rules and scale are unchanged.

This accounts for the cards' rotated edges as well as center positions. Both outer top edges now sit lower than the middle pair, and projected near-edge fan amplitude is smaller than the first R028 fan. Analytic projected-edge checks and visual inspection are recorded in Verification.json. All four current gameplay/prototype assemblies and native templates are updated. Current Gameplay D is selected in Penpot. Original sources/exports are retained; only the two outer projected previews were uploaded.

The source and review board IDs remain unchanged. For a future Unity integration, use this layout instead of R028/R029, keeping existing independent dynamic TechniqueCardViews and sprites. Adapt the recorded screen-space curve and per-count angle table to the existing camera and stable hit planes. Projected PNGs are review aids, never runtime card/hand art. RenderFan.py rebuilds them from the unchanged native R028 card exports without slicing.

R027 tooltip and R030 fan are the only pending revisions. R025/R026/R028/R029 are superseded without imports. All24 earlier imports and all253 checkpointed Unity files remain unchanged.
