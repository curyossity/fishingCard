# Asymmetric fade boundaries

Corrected the fade geometry to preserve the decorative lower outside corners: larger lower-left curve for Catch and lower-right for Bait. Ordinary radii are24 card pixels, the larger curve66, with a6px inset from the printed border. The two contours are authored independently in the prefab/editor sync; input bounds and text geometry remain unchanged. Fade opacity remains46%, with no new borders or checkmarks.

Reviewed both choices at4K/1080p against the master. Six sampled decorative-border pixels remain exactly identical with either selection, while four interior checks confirm dimming only on the alternative role. Compilation clean; scene unchanged. This geometry-only correction preserves the existing pointer/state behavior verified at006.4. No artwork regeneration or Penpot changes.

[Catch dimmed detail](BaitDetail.png) · [Bait dimmed detail](CatchDetail.png) · [Catch selected1080p](Catch1080p.png) · [Bait selected1080p](Bait1080p.png) · [Pixel checks](Verification.json) · [Preservation](Preservation.json) · [Checkpoint](UnityCheckpoint.json).

MFJ-SYNC-006.5,20 October2026. Prior evidence remains in RoleFading.
