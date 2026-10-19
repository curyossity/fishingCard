> **Superseded by [MFJ-R027](../MFJ-R027/README.md), without a Unity import.** Do not queue this draft.

# Detailed engraved tooltip — MFJ-R026

**Penpot only, not imported into Unity. Supersedes MFJ-R025.** [4K preview](GameplayTooltip4K.png) · [Catch tooltip](CatchTooltip.png) · [Bait tooltip](BaitTooltip.png) · [Source manifest](Assets.json) · [Geometry](PenpotDesign.json) · [Verification](Verification.json).

The selected Penpot board is **GAMEPLAY D / Engraved tooltip / MFJ-R026** on **Concept B — Reusable Assembly (4K)**. At `(0,10100)`, **PROTOTYPE / Engraved Catch Chain tooltip / MFJ-R026** retains the native mouse-enter/open-overlay and mouse-leave/close-overlay connections for all three sample cards. Source boards remain at `(8500,13200)`, `(9550,13200)`, `(10600,13200)`. A separate art source board is at `(11800,13200)`. Select the prototype and start presentation/playback to review hover; normal canvas editing does not execute prototype interactions. Wiring is checked; viewer playback is not certified.

The900×640 tooltip uses a new independent blank engraved frame, matching the event/creature masters: layered copper border, aged patterned parchment, inset midnight-navy panel and dense etched marine foliage. No mockup slicing. The source bitmap is1487×1058, displayed856×608, so no source enlargement is needed at the intended4K footprint. There is no baked text or role icon. Built-in image_gen was used; see GenerationPrompt.md.

“Catch Chain”, the mode badge and “Catch Effect”/“Bait Effect” headings are removed. The existing **Catch hook** and **Bait twin-spark** from MFJ-R002 appear as native vectors beside the active effect name. Text remains native: creature name, effective tag chips, effect name and effect description. The effectless Sardine example uses the hook with “No effect”. Fonts remain Cinzel and Source Serif4. The transparent anchor pointer is a separate native shape.

The content/behavior contract remains the same as the initial concept: active attached role only, live effective tags, explicit empty effects, no hidden-value disclosure, no new compact hover frame, no action controls; future Unity must support focus, placement clamping/flipping, modal ordering and dismissal on scroll/removal/state changes. R025 source versions are retained as hidden comparison boards and historical exports. The prototype destination IDs were preserved, so interactions now open the revised sources.

Only R026 is pending for a future import. R025 is superseded and must not be implemented separately. MFJ-R001–R024 remain verified imports. [Current pending list](../../Pending%20Unity%20Changes.md). No Unity assets or code were modified.
