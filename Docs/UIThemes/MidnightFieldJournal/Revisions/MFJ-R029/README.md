> **Rejected and superseded by [MFJ-R030](../MFJ-R030/README.md), without a Unity import.** The outer-card alignment broke the fan. Do not queue.

# Shallower Technique fan — MFJ-R029

**Penpot only; not imported into Unity. Supersedes MFJ-R028.**

[Preview](FanPreview.png) · [Layout](FanLayout.json) · [Geometry and instance IDs](PenpotDesign.json) · [Verification](Verification.json).

Only the middle pair move down: Y400 → Y450. Outer cards stay at Y480, reducing center-to-outer offset80 →30. X centers500/1000/1500/2000, rotations−15/−5/+5/+15°, tilt40°, perspective2000px, the2500×866 viewport, artwork and native text are unchanged. This reduces the overall visible curvature while keeping the original fan orientation and laid-down view.

All four current projected gameplay/prototype assemblies and native editable source templates are updated. Current Gameplay D is selected in Penpot. Native sources and projected review boards retain their previous IDs and positions. Old R028 source images are hidden for comparison and its exported revision is retained. Only the two changed middle-card previews were uploaded; outer-card images are reused. RenderFan.py rebuilds the preview from the existing R028 native-source exports without slicing.

For a future Unity import, use this revision's curve instead of R028. Keep existing independent dynamic card views and adapt the screen-space recipe to the current camera. Preview PNGs are review evidence, never runtime hand/card artwork. The future0–4 layout uses centered index c, X=1250+500c, YDown=450+15(c²−minimum centered index²), clockwise angle10c°. Existing inspection and stable hit planes must follow it. See the R028 historical handoff for unchanged runtime validation requirements.

R027 tooltip and R029 fan are the only pending revisions. R025/R026/R028 are superseded without imports. R001–R024 remain verified imports. Unity remains unchanged.
