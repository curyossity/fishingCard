# Shared-contour selected-role inner glow trial

Audited all52 outer vertices per role from the previous highlight and fade in card space. Maximum difference was0 pixels for both roles. They shared geometry, but both approximated the raster master; the thin bright line made that approximation more visible. Earlier matching settings/pixel samples did not establish an exact trace of the artwork.

Replaced the thin outline with a warm inward glow. It uses the exact same JournalRoleFade contour builder as the dimming layer, on sibling root regions. Color#F2BF6F, peak alpha50/255, width24 card pixels; alpha is zero at the outer boundary, peaks slightly inward, and falls to zero before the panel center. The fade remains46% black. Checkmarks and previous outlines stay hidden. The asymmetric corner and6px inset are retained; this remains an authored approximation of the printed art, not a traced contour.

14 actual Play Mode checks passed: pointer switching, chosen glow/opposite fade, no hard border, nonblocking input, exact shared mesh boundary, transparent glow ends, fresh occurrence reset and event clearing. Compared104 boundary vertices with zero difference. Six decorative-border pixel samples and four interior comparisons passed. Reviewed at4K/1080p. No compile errors; Play Mode/background restored off, scene clean. No artwork regeneration or Penpot changes.

[Catch selected](Catch1080p.png) · [Bait selected](Bait1080p.png) · [Catch detail](CatchDetail.png) · [Bait detail](BaitDetail.png) · [Prior contour audit](PreviousContourAudit.json) · [Play Mode verification](PlayModeVerification.json) · [Pixel checks](Verification.json) · [Preservation](Preservation.json) · [Checkpoint](UnityCheckpoint.json).

MFJ-SYNC-006.7,20 October2026. Implemented trial for user review; previous evidence retained in RoleSoftHighlight.
