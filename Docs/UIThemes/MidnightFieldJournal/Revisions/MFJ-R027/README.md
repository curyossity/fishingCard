> **Imported and verified at [MFJ-SYNC-003](../../Sync003/README.md) on2026-10-19.** The design-stage text below is retained as history; this revision is no longer pending.

# Engraved tooltip without pointer — MFJ-R027

**Penpot only; not imported into Unity. Supersedes MFJ-R026.**

Removed the separate left-edge triangular pointer from all three current tooltip sources (Crab Catch, Minnow Bait, Sardine Catch) and their assembled preview. The marked triangle was a native Penpot group, not part of the generated frame or a Unity tooltip. The detailed frame, typography, role icons and data remain unchanged. Source board IDs and prototype targets are preserved.

[Catch preview](CatchTooltip.png) · [Bait preview](BaitTooltip.png) · [4K context](GameplayTooltip4K.png) · [Geometry](PenpotDesign.json) · [Verification](Verification.json).

Reuse [R026 frame](../MFJ-R026/Art/EngravedTooltipMasterV2.png) and its role icons. No newly generated raster artwork. See the [previous handoff](../MFJ-R026/README.md) for layout and future runtime behavior; omit its obsolete pointer instruction. Prototype wiring remains intact; viewer playback has not been certified.

R027 is the sole pending tooltip revision. R025/R026 are superseded and must not be imported separately. All 24 prior integrated revisions and the Sync002 checkpoint remain unchanged.

The full 4K context export timed out. Component exports succeeded; the live assembled preview is updated and its pointer removal was verified by shape read-back.
