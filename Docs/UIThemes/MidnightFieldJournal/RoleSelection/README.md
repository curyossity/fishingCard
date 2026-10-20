# Distinctive, aligned Catch/Bait selection

The candidate creature card now fits selected-role borders to the complete rounded panels. The previous role rectangles started 7 card pixels too low and ended 4 pixels too high. Catch uses (82,875,545,438); Bait uses (634,875,545,438) on the 1260×1400 face. Runtime text remains at its existing card-space coordinates.

Selection now uses an 8px opaque pale-gold border, warm outer glow, inset hairline and small native check beside the role heading. All feedback is nonblocking and inherits the existing selected-role visibility. No source image changes or regeneration were needed. The targeted editor sync and regular prefab builder share styling values so a later rebuild preserves this result.

Verified 20 actual Play Mode pointer/state checks: no default role, both role buttons usable while Descend is gated, exclusive selection, full-region border anchoring, stronger stroke, visible nonblocking checks, refresh retention, new occurrence reset, concealed selection and event hiding. Reviewed both roles at 4K and 1080p. No compilation errors; Play Mode/background restored off; scene clean. Art, shared gameplay, old theme, all other existing assets and saved scenes remain unchanged.

[Catch4K](Catch4K.png) · [Bait4K](Bait4K.png) · [Catch1080p](Catch1080p.png) · [Bait1080p](Bait1080p.png) · [Actual game capture](ActualBait4K.png) · [Pointer verification](PlayModeVerification.json) · [State verification](StateVerification.json) · [Preservation](Preservation.json) · [Checkpoint](UnityCheckpoint.json).

Unity-only followup MFJ-SYNC-006.3, 20 October 2026. Penpot pending/completed history is unchanged.
