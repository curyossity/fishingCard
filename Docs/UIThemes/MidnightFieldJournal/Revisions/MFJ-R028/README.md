> **Superseded by [MFJ-R029](../MFJ-R029/README.md), without a Unity import.** Retained for comparison; do not queue.

# Technique hand fan — MFJ-R028

**Penpot ready for review; not imported into Unity.** This is a separate pending change from the R027 tooltip.

[Penpot fan preview](PenpotFan.png) · [Layout recipe](FanLayout.json) · [Penpot source/instance IDs](PenpotDesign.json) · [Verification](Verification.json).

Four independent projected cards replace the old single composite in Gameplay B, Gameplay C event review, Gameplay D current preview and its tooltip prototype. The old composite is hidden and retained for comparison. The current Gameplay D preview is selected in Penpot. A dedicated projected review board is at `(8500,17400)` and native editable flat card sources are at `(0,17400)` on Concept B — Reusable Assembly (4K).

The cards follow a shared 40° table tilt and a 2000px CSS perspective, with clockwise screen angles −15°, −5°, +5°, +15°. Centers are `(500,480)`, `(1000,400)`, `(1500,400)`, `(2000,480)` within a 2500×866 hand viewport. Later cards overlap earlier cards. The regular curve gives a controlled arc without changing artwork, typography or gameplay data. The rendering recipe constructs this view from separate 620×640 native-template exports. No mockup slicing or AI-generated replacement art was used.

Projected PNGs are Penpot review aids only, including their current text. **Do not import them as Unity card faces or a hand composite.** Unity already has independent dynamic TechniqueCardViews, correct engraved sprites and perspective rendering. The later integration should change native resting position/rotation and corresponding hit planes, keeping those existing resources. Convert clockwise/downward screen coordinates appropriately for its camera, and visually match the fan without replacing the camera with guessed values. The CSS numbers are authored preview settings, not measurements recovered from the old generated reference.

On integration, verify 0–4 cards, duplicate definitions, hover/navigation lifting and restoration, stable hit mapping, draw/discard/recovery rebinding, modal blocking and responsive fitting at 4K/1080p/ultrawide. Preserve the four-card hand limit and all eight active designs. Source sprites, fonts and gameplay remain unchanged. All 253 checkpointed Unity files are unchanged; R001–R024 remain verified imports, R025/R026 remain superseded, and R027/R028 are the only pending revisions.
