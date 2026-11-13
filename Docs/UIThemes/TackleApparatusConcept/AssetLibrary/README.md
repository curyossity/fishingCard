# Tackle Apparatus UI kit — original layout

Reusable artwork and editable Penpot components for the approved Discover, Landing Challenge and Rig Placement screens. The working game canvas is the original **1920 × 1080**. This is a design handoff; Unity code, scenes, prefabs and existing runtime artwork have not been changed.

Integration update, 7 November 2026: the handoff's Common/Discover sprites and fonts are now installed in the comparison scene. See [Unity artwork validation](../../../Validation/TackleDiscoverArt/README.md). Landing artwork was integrated on 13 November 2026; see [Landing validation](../../../Validation/TackleLandingArt/README.md). Placement remains a handoff reference. The provenance and original-resolution decisions below are preserved.

[Download the current UI kit](../Deepwater-UI-Kit.zip).

**Generation constraint:** the user has explicitly ruled out the separate image API option. Use built-in image generation only; do not switch to a CLI/API workflow.

**Resolution decision:** continue with the initial layout and initial sprite dimensions. Higher-resolution work is deferred at the user's request. Original source artwork, separate functional layers and editable text are preserved for a possible later change. Source and export sizes are recorded in [Inventory.md](Inventory.md) and [Manifest.json](Manifest.json). The earlier `Deepwater-UI-Kit-4K.zip` is an archived experiment, not the current handoff.

## Penpot organization

- [05 — Common UI Assets](https://design.penpot.app/?screen=workspace&team-id=9e056cfe-f83f-81f8-8008-cc4e858053b7&file-id=3681d377-bc58-8110-8008-e7a38adc2112&page-id=38e9e9ec-a6d5-8050-8008-e7b1dd775f52): background, machine, rig cabinet, reusable Catch/Bait card faces, clips, ropes, portraits, title/haul/message plaques, buttons, readouts, indicators, typography, colors and composed shared widgets.
- [06 — Discover UI Assets](https://design.penpot.app/?screen=workspace&team-id=9e056cfe-f83f-81f8-8008-cc4e858053b7&file-id=3681d377-bc58-8110-8008-e7a38adc2112&page-id=38e9e9ec-a6d5-8050-8008-e7b1dd7a566e): reel drum, bezel, lock lever states, match lamps and a composed reel with a clipping viewport.
- [07 — Landing UI Assets](https://design.penpot.app/?screen=workspace&team-id=9e056cfe-f83f-81f8-8008-cc4e858053b7&file-id=3681d377-bc58-8110-8008-e7a38adc2112&page-id=38e9e9ec-a6d5-8050-8008-e7b1dd7c6506): timing track, scoreboard, progress trough and a composed track with independent moving fish and zone layers.
- [08 — Rig Placement UI Assets](https://design.penpot.app/?screen=workspace&team-id=9e056cfe-f83f-81f8-8008-cc4e858053b7&file-id=3681d377-bc58-8110-8008-e7a38adc2112&page-id=38e9e9ec-a6d5-8050-8008-e7b1dd7dc043): insertion rail, role selection states, insertion socket states, plus/blocked symbols and composed role/gap widgets.
- [09 — Unity Assembly Reference](https://design.penpot.app/?screen=workspace&team-id=9e056cfe-f83f-81f8-8008-cc4e858053b7&file-id=3681d377-bc58-8110-8008-e7a38adc2112&page-id=38e9e9ec-a6d5-8050-8008-e7b1dd7f5bfb): three separate 1920 × 1080 screen compositions assembled from the independent artwork and live labels. The earlier approved concept pages remain preserved.

The backdrop includes the desk, peripheral net/rope, cork float, lantern, spool and chart as one static decorative layer. Those props are not separately animated sprites. The cabinet's small bolts, vents and wear are integrated into its housing texture. These boundaries preserve the approved visual treatment while keeping functional modules independent.

## Common elements detected across the three screens

| Shared system | Reusable pieces | Variable content |
|---|---|---|
| Screen shell | Background, machine housing, lower console | Central phase module |
| Left rig | Cabinet, ivory/sage faces, clip, connector rope, portraits | Ordered creatures, names, Catch/Bait roles, resolved sale values |
| Header | Biome/phase plaque, haul plaque | Biome, phase and accumulated haul |
| Actions | Primary/secondary button faces, focus overlay, keycap | Action label, availability and binding |
| Feedback | Message strip, readout frames, lamps, tooltip frame | Instructions, results, ability descriptions and numerical feedback |
| Visual language | Palette and licensed fonts | All functional text |

Discover alone uses reels and locks. Landing alone uses Resistance, attempts and timing. Placement alone uses pending role choice, insertion gaps and haul preview. Their controls are not shown together.

## Files and construction

[Exports/](Exports/) contains independent PNG sprites. All artwork except the desk background uses alpha. [Manifest.json](Manifest.json) records Penpot IDs, actual export dimensions, source crop dimensions, proposed borders and usage. [Inventory.md](Inventory.md) is the readable index. [Previews/](Previews/) contains the rebuilt screen references.

Card construction is **blank frame + independent portrait + clip + live labels**. Both full and compact card examples use the same scalable face. The three sample rig cards do not imply a capacity limit. `NameLabel`, `RoleLabel`, `SaleValueLabel` and `Portrait` are named layers in the composed card components. The rig widget supplies a clipped viewport for an ordered list.

Raster exports contain no functional names, roles, prices, scores, instructions or button labels. Penpot supplies those as editable text; recreate them using TextMeshPro. Composed components are design references, not Unity prefabs. Press Start 2P and VT323 font files and OFL licenses are included under [Fonts/](Fonts/). Barlow is used for annotations outside the game, and is not needed by the game interface.

The native overlays include focus corners, direction arrows, a caret, landing/perfect bands, a score segment and insertion legality markers. Do not merge moving fish, zone, attempt lamps or score feedback into the track texture.

## Unity handoff

Use a 1920 × 1080 reference canvas and preserve the 16:9 game composition. The exported dimensions are texture dimensions, not a requirement to display every sprite at its natural size. `displaySize` is a nominal UI footprint; the assembled references are authoritative for this example layout. Keep aspect ratio on portraits, lamps, clips, locks and cabinet silhouettes. Use sliced frames for variable-width panels rather than stretching their corners.

Recommended initial import choices are Sprite (2D and UI), Single, Full Rect, centered pivot, Clamp, Point filtering, no mipmaps, sRGB enabled, input alpha, Alpha Is Transparency enabled for cutouts, and Read/Write disabled. Start without texture compression for visual comparison. Use a 2048 maximum texture size for these individual exports. These are handoff recommendations; confirm the final project's scaling, atlas and platform settings in Unity. See [Unity's sprite import reference](https://docs.unity3d.com/6000.0/Documentation/Manual/texture-type-sprite.html).

`borderPixels` uses named left/top/right/bottom values in exported-image pixels. Unity's Vector4 border order is **left, bottom, right, top**; `unityBorderLBRT` provides that order. Borders are design guides exercised in the Penpot compositions, and still need an in-engine slicing check. Keep border thickness consistent with the reference canvas. PPU 100 is a proposed baseline, not an instruction to change existing project conventions.

`Layout.json` records named layers, positions, dimensions, live text and the displayed border thickness for the three screen references. For a uGUI Sliced Image with both sprite and canvas reference PPU set to 100, its recorded `pixelsPerUnitMultiplier` reproduces the example's corner thickness. Other PPU conventions require the corresponding adjustment. This distinguishes the exported cut positions from the border's final displayed thickness; see [Unity's Image API](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/api/UnityEngine.UI.Image.html#UnityEngine_UI_Image_pixelsPerUnitMultiplier).

Suggested view structure:

```text
GameCanvas
  Background
  SharedShell
    RigPanel / RigViewport / ordered RigCard views
    BiomePhaseLabel
    HaulCounter
    MachineHousing
    MessageStrip
    ControlConsole
  ActivePhase
    Discover: three ReelCells + locks + match lamps + actions
    Landing: creature panel + score + TimingViewport + attempts + strike
    Placement: pending creature + role choices + insertion rail + preview + confirm
  TooltipLayer
```

Interaction states: normal uses the base face; keyboard/gamepad focus adds corner markers; pressed offsets art and label by **4 pixels**; disabled dims both and disables input. Selected role and gap use their selected artwork. Blocked insertion uses a dim socket plus the blocked symbol, so legality does not depend on color alone. Hover can share focus treatment. Actual hit regions should use clean UI rectangles, not the irregular alpha silhouette.

## Gameplay and content boundary

The shown Sardine Catch 7 → Mackerel Bait 0 → Glass Minnow Catch 5 order is illustrative. Haul is 12. Landing's 36/80 points, two remaining attempts from an initial four, +6 rig and Good +18 attribution are prototype examples. Garfish Catch 8 and a preview of 12 → 20 are provisional. They do not finalize creature content, abilities, controls, attempt counts or reel rules.

Placement chooses Catch/Bait and a legal insertion gap; existing creatures retain their relative order. The pending Garfish does not contribute before confirmation. No reorder handle, voluntary Surface control, Line Capacity or combat UI is introduced. New expeditions start with an empty rig. Failure does not acquire the failed creature and pays the full accumulated Catch sale value before attachments clear; Bait pays zero. All temporary attachments clear on either ending. Permanent progression remains separate.

Final Catch/Bait ability descriptions and resolved positional synergy feedback remain content-design inputs. Use live data in tooltip/details views; do not infer final ability text from these sample panels. No maximum rig size, extra equipment rule or permanent ownership is implied by the mockups.

## Provenance and validation

Artwork families in [Sources/](Sources/) were generated through the **built-in image generation tool**, referencing the approved Tackle Apparatus concepts. Their prompt set is preserved beside each PNG as `*-prompt.txt`. They were segmented with Penpot clipping frames into reusable assets. Original generated sources remain intact. No third-party game's artwork is included.

Font sources and licenses: [Press Start 2P](https://github.com/google/fonts/tree/main/ofl/pressstart2p), [VT323](https://github.com/google/fonts/tree/main/ofl/vt323). Generated art is a production candidate, not a claim of hand-authored pixel-grid cleanup. Source alpha and crop gutters were checked, followed by exported PNG dimension/alpha checks and composition review. Export QA is recorded in `Validation.json`.

Unity integration, motion, navigation, localization, aspect-ratio adaptation, sprite-atlas packing and final card data have not been tested here. This package prepares those integrations without changing the running game.
