# Unity effect-text shadow trial

**Restored as the active Unity treatment at the user's request**, after rejecting the [Semibold and outline trial](../EffectTextOutline/README.md). The regular font and subtle shadow are assigned to all four effect fields again; the outline preset and runtime override have been removed.

Added a theme-local TextMeshPro SDF underlay preset for the main card's Catch/Bait effect names and descriptions. This is a Unity readability trial, awaiting user review; no Penpot design or pending-import status changed.

The persistent preset is `Assets/UIThemes/MidnightFieldJournal/Profiles/EffectTextShadow.mat`. It uses the existing Source Serif 4 font atlas with dark navy-black at 90% opacity, underlay offsets 0.45/-0.45, dilation 0.15 and softness 0.25. These are TMP shader units, not screen pixels. Text positions, size, content and artwork are unchanged. The original font material remains unchanged, so unrelated UI text is unaffected.

The four effect fields on `CreatureCard.prefab` use the preset; the central-card builder preserves that assignment when regenerating templates. The same preset was applied to the running Sea Urchin card without restarting its run. Four visible fields and four saved prefab fields were confirmed with underlay enabled; the active scene stayed clean. No Unity console errors were reported. Script recompilation may be deferred until Play Mode ends under the Editor's current settings.

Captured at 3840×2160:

- [Before](Before.png)
- [After rendering the shadow](After-1.png)

`After.png` was an early capture before the unfocused Game View rendered a new frame; use `After-1.png` for review. The final capture was visually inspected. The previous `runInBackground=false` setting was restored after capture.

To compare in the Inspector, disable Underlay on this isolated material; re-enable it to restore the trial. No source-art regeneration is needed. The shadow improves contrast but does not change the overlap with decorative artwork.
