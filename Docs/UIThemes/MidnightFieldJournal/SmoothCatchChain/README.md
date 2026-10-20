# Smooth newest-catch following

New attachments that extend the catch chain now scroll to the bottom over 0.32 seconds with smoothstep easing. Animation uses unscaled time, keeps follow intent during movement, and retargets from the visible position if another creature arrives. Ordinary UI/stat refreshes do not restart it. Wheel and drag input interrupt immediately; manual inspection remains in place until the player returns to the bottom.

Initial binding, viewport resizing and shrinkage restore/clamp directly. Empty chains reset following for the next run. Hiding the chain cancels unfinished animation; rebinding restores the appropriate position. Edit Mode previews retain immediate positioning. No gameplay, artwork, card-definition, prefab or scene changes.

88 actual Play Mode assertions passed through the existing CatchChain prefab: intermediate positions and monotonic completion under repeated refreshes at timeScale 0, rapid attachment retargeting, wheel/drag interruption, resumed following, empty-chain reset, disable/re-enable and shrinking during motion. Test-owned objects and background/time-scale settings are restored. [Results](PlayModeVerification.json), [preservation](Preservation.json), [current checkpoint](UnityCheckpoint.json).

Unity-only followup MFJ-SYNC-006.2. Pending Penpot imports remain unchanged.
