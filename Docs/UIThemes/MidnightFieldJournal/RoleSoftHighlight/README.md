# Subtle selected-role highlight

The selected panel gains a thin gold edge while the alternative keeps its approved fading. Edge thickness is3 card pixels, color#F6CF93 at170/255 alpha. Its6px inset and24/66px asymmetric corner radii come from the same configuration as the fade: larger lower-left corner for Catch and lower-right for Bait. Ordinary outlines elsewhere retain their existing geometry. Broad glows and checkmarks remain inactive; artwork and rules remain untouched.

16 actual Play Mode checks passed: pointer selection, exclusive edge/opposite fade, matching corners/insets, nonblocking input, subtle stroke, disabled legacy decorations, new occurrence reset and event clearing. Six decorative-border pixel comparisons and four panel-interior comparisons passed, confirming the edge/fade leave external borders and selected interiors unchanged. Both choices and None reviewed at4K/1080p. No compile errors; Play Mode/background restored off; saved scene unchanged.

[Catch selected](Catch1080p.png) · [Bait selected](Bait1080p.png) · [Before selection](None1080p.png) · [Play Mode checks](PlayModeVerification.json) · [Pixel checks](Verification.json) · [Preservation](Preservation.json) · [Checkpoint](UnityCheckpoint.json).

MFJ-SYNC-006.6,20 October2026. Unity-only change; Penpot pending/completed history unchanged.
