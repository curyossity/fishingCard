> **Superseded by [MFJ-R026](../MFJ-R026/README.md), without a Unity import.** Retained as historical design evidence; do not queue this draft.

# Catch Chain tooltip — MFJ-R025

**Penpot only; not imported into Unity.** Ready for visual review. [4K context preview](GameplayTooltip4K.png) · [Bait component](BaitTooltip.png) · [Editable geometry / exact IDs](PenpotDesign.json) · [Verification](Verification.json) · [Pending import list](../../Pending%20Unity%20Changes.md).

On **Concept B — Reusable Assembly (4K)**, the selected board **GAMEPLAY D / Catch Chain tooltip visible / MFJ-R025** is at `(4200,10100)` and shows Clinging Crab's tooltip. **PROTOTYPE / Catch Chain hover / MFJ-R025** is at `(0,10100)`. Select that prototype board and start Penpot presentation/playback, then hover Sardine, Clinging Crab or Glass Minnow. Native `mouse-enter` opens its overlay and `mouse-leave` closes it. Overlay actions have no background dimmer. Wiring/read-back is verified; actual viewer playback remains a review step, and canvas editing mode does not execute hover interactions.

The three editable tooltip sources are at `(8500,13200)`, `(9550,13200)` and `(10600,13200)`. Each uses a900×640 design canvas: navy fill, fine double copper rules, a left anchor pointer, a restrained native seaweed engraving, Cinzel labels, Source Serif4 body/name, role badge and tag pills. No raster assets or slices are needed. The existing Catch Chain cards/connector artwork and both approved gameplay assemblies remain unchanged.

Content follows the attachment's **current active role**: Clinging Crab Catch shows Iron Grip; Glass Minnow Bait shows Shoal Signal; Sardine Catch explicitly says “No Catch effect” and never substitutes Feeding Frenzy. The tooltip is inspection, not a Release/mode-change control. It does not expose concealed sale values, modifiers, internal effect IDs or a new hover frame.

## Later Unity behavior contract — not implemented

Use the existing attachment/runtime data: creature name, attached role, live effective tags and that role's effect title/rules, preserving explicit empty effects. Tag grants such as Slipstream must be reflected; do not hardcode these three examples. Keep the current data owners and information masks. Native text stays editable/data-bound.

Show on pointer hover (short delay) and keyboard/controller focus. Place beside the source card, anchored independently of pointer movement; flip/clamp to visible viewport. The tooltip must not intercept clicks, grant an action, add a compact hover outline or cover permanent action buttons. Hide/rebind on pointer exit, focus loss, scrolling, attachment removal, modal takeover or run end; refresh when source data changes. Keep modal and projected-hand/tooltip canvas ordering correct at4K,1080p and other supported aspects.

Future implementation belongs under the candidate theme, with shared view hooks only if necessary. Update builders and generated prefabs together, then validate live effective tags, Catch/Bait changes, effectless cards, removal/scrolling, modal/input locks, focus and concealment. A separate user request is required to integrate it.
