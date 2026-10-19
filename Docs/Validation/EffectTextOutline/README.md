# Semibold and outline readability trial

**Rejected and removed at the user's request.** Regular text with the earlier subtle shadow has been restored. The outline preset and runtime reapplication code were removed. Evidence below is historical; it is not the current design.

Unity-only trial requested after the shadow treatment remained hard to read. Catch/Bait effect names and descriptions now use the existing Source Serif 4 Semibold font with an isolated dark outline material. No artwork, text rectangles, authored font sizes, rules, or Penpot designs were changed. Automatic fitting remains enabled as before.

Preset: `Assets/UIThemes/MidnightFieldJournal/Profiles/EffectTextOutline.mat`. Outline color is near-black navy `(0.015,0.025,0.035,1)`, width `0.22` in TMP shader units, softness `0`. Underlay is disabled: this replaces the active shadow trial. The earlier shadow preset is retained for comparison but is no longer assigned to the central prefab.

The central-card builder preserves the four font/material assignments. `JournalCreatureCardVisuals.ApplyEffectReadability` restores the preset after the shared view resets description outlines; event rules retain the original regular font without the creature outline. Shared font assets/default materials and unrelated UI remain unchanged.

Eight field checks passed across two creature refreshes, including an intervening Reef Snag event. All four fields retained Semibold, enabled outline and the requested width after the shared-view reset. The code compiled. During live script reload Unity's URP pipeline failed to initialize, producing VolumeManager/Blitter errors; restarting Play Mode restored rendering. The previous depth-zero preview restarted with a new encounter. No project rendering settings were changed.

[Rendered 4K preview](OutlinePreview.png) was visually inspected after recovery. `SemiboldOutline.png` is the failed black capture from before recovery and is not the result preview. Play Mode remains available for user review; background execution was restored to its previous disabled setting. This is a trial awaiting visual approval, with no Penpot revision or Unity-import status changes.
