# MFJ-SYNC-012 — Engraved coastal tag timing

**R059 and R060 imported and verified, 31 October 2026.** The candidate comparison scene inherits the rebuilt `TagTiming.prefab`; its GUID and existing scene/controller binding remain stable. No scene rebuild or gameplay/profile changes were needed.

## Changes

- Exact independent Penpot R059 frame and R060 button source bytes copied into **Art/TagTiming/EngravedCoastal/**. Earlier timing artwork remains intact. No flattened bar/card screenshot is used as a UI face.
- Native eight-tag zones, glyphs, text, precision meshes, moving anchor, stopped burst and Stop button remain separate and dynamic. The builder reproduces the 2160 × 720 Penpot geometry, displayed at 80% scale between the Catch Chain and action column; the frame/button are not upscaled at 4K.
- Track geometry now drives widths and cursor position instead of the previous hardcoded 1440px channel. Serialized offsets/height/tint retain previous-prefab defaults. New Stop text and its button tint remain readable in normal, selected and stopped states.
- Existing one-second elastic entry from below and exit upwards, reveal gating, precision celebration, tag fatigue/recovery, normal/precision sampling and event bypass remain with their existing owners.
- The existing live-check utility accepts a separate evidence directory and measures minimum zones relative to the authored track, avoiding fixed old pixel widths and overwriting historical screenshots.

## Validation

2,955 existing deterministic rule assertions passed. A final real Play Mode run passed **103 checks**, covering entry/Stop locks, marker motion, precision particles, hidden identity until exit, Catch selection/next encounter, Surface cancellation, restart, fatigue to the 5% floor, proportional precision spans, and event bypass. Progression and Journal test writes were redirected to debug-only keys. No console errors; Play Mode stopped and active scene clean.

Final 3840 × 2160 capture inspected after adjusting the panel to avoid covering Surface. Native bitmap source is 2172 × 724, shown below native resolution. Labels and glyphs remain native UI. 1920 × 1080 plus precision/fatigue/event captures are also saved. Original model/card/profile/scene/build-setting files have no tracked modifications.

[Final 4K](TagBar4K.png) · [1080p](TagBar1080.png) · [Fatigued zones](TagBarFatigue4K.png) · [Precision](TagBarPrecision4K.png) · [Verification](Verification.json) · [Source hashes](ImportedSources.json).

Ledger: **0 pending imports, 47 completed, 13 superseded**. R051's original import remains historical evidence; its decorative presentation is replaced by R059/R060. Do not import these revisions again.
