# Fishing Roguelike — Visual Implementation Plan

## Purpose

Implement the complete 16:9 gameplay presentation for the fishing card roguelike in Unity by assembling a supplied visual asset pack and binding it to runtime game data.

The visual reference is:

`fishing-roguelike-gameplay-ui-16x9.png`

Use the reference for composition, visual hierarchy, colors, materials, card scale, and spacing. Build the interface from reusable Unity UI components rather than using the reference screenshot as a background.

The visual assets will be produced separately and supplied to you. You must not generate, redraw, reinterpret, or replace the supplied artwork. Your job is to import the assets, assemble prefabs, implement layout and interaction states, and bind runtime data.

## Asset ownership contract

### Visual asset creator responsibilities

The visual asset package must provide:

- Every background, frame, panel, divider, ornament, button surface, card base, card back, slot, meter, and overlay.
- Every creature illustration as a transparent PNG.
- Every technique illustration as a transparent PNG.
- Every gameplay icon as an individual transparent PNG or approved sprite atlas.
- Normal, hover, selected, pressed, disabled, warning, and critical visual layers when a state requires distinct artwork.
- A palette reference, typography reference, nine-slice border values, pivot guidance, and asset manifest.
- Reference screenshots showing the intended result at `1920 × 1080`.

### Your responsibilities

You may:

- Import and configure supplied assets.
- Build Canvas hierarchies and reusable prefabs from those assets.
- Add TextMeshPro fields over blank visual regions.
- Bind runtime data to text, images, meters, markers, visibility, and interaction states.
- Implement layout, hover movement, card movement, fades, controller focus, tooltips, and transitions.
- Apply approved colors from the supplied theme to tintable elements.

You must not:

- Generate replacement artwork.
- Draw new icons, frames, textures, cards, buttons, or backgrounds.
- approximate missing assets with generic Unity sprites.
- Alter the supplied visual language or redesign the layout.
- Bake dynamic gameplay values into static images.
- Use the full-screen mockup as the gameplay background.

If an asset is missing, you must record it in `MissingVisualAssets.md`, use a clearly labeled temporary placeholder, and continue with integration work that does not depend on the final image.

## Static artwork and runtime data boundary

| Element | Supplied as artwork | Added at runtime |
| --- | --- | --- |
| Gameplay background | Complete background layers | Optional subtle parallax offset |
| Panel and card frames | Complete blank frames | State visibility and approved tint |
| Creature artwork | Transparent creature PNG | Sprite selection and positioning |
| Creature name | Blank title region | Name text |
| WEIGHT and VALUE labels | Baked into creature-card base | Weight and value numbers |
| Creature effect area | Blank ivory panel | Effect text and keywords |
| Anchor sockets | Baked empty sockets | Anchor icons and filled count |
| Technique-card frame | Blank card base | Title, description, cost, artwork sprite |
| Catch Rig | Rail, connectors, rings, card holders | Attached catches and order |
| Line Load meter | Track, segments, and state overlays | Current load, capacity, fill, state |
| Biome and depth plaques | Blank framed plaques | Biome name and depth value |
| Depth tracker | Track and node artwork | Zone labels, ranges, and current node |
| Tension meter | Track and state overlays | Tension value and state text |
| Action buttons | Blank button surfaces and icons | Labels, enabled state, click behavior |
| Tooltips and modals | Blank framed panels | Titles, descriptions, values, buttons |

## Asset package structure

```text
FishingUIAssets
├── Reference
│   ├── fishing-roguelike-gameplay-ui-16x9.png
│   ├── ui-style-guide.md
│   └── asset-manifest.md
├── Backgrounds
├── Frames
├── Cards
│   ├── Creature
│   ├── CompactCatch
│   └── Technique
├── Creatures
├── Techniques
├── CatchRig
├── Controls
├── Meters
├── Icons
├── Overlays
├── Effects
└── Fonts
```

Each manifest entry must include filename, intended prefab, native pixel size, pivot, pixels per unit where relevant, transparency requirement, nine-slice borders where relevant, tint permission, and runtime field dependencies.

## Core visual direction

- Cards represent the game world.
- The current encounter is the primary focal point.
- The Catch Rig is the signature visual feature.
- The four-card technique hand is always visible.
- Permanent actions are always easy to locate.
- The interface uses oxidized dark teal, aged brass, warm ivory, turquoise, and coral.
- Creature illustrations use a hand-inked natural-history engraving style.
- The background resembles a restrained nautical chart or maritime tabletop.
- Decoration must not resemble gameplay information unless it has a real function.
- Avoid literal underwater scenery, characters, boats, glossy mobile-game rendering, excessive glow, and conventional RPG HUD elements.

## Target resolutions

- Primary reference resolution: `1920 × 1080`
- Aspect ratio: `16:9`
- Additional validation resolutions:
  - `2560 × 1440`
  - `1366 × 768`
  - `1280 × 800` for Steam Deck
  - `2560 × 1080` for ultrawide behavior

## Target screen structure

```text
FishingRunView
├── Background
├── TopNavigationBar
├── MainContent
│   ├── CatchRigPanel
│   ├── EncounterPanel
│   └── RunControlsPanel
├── TechniqueHand
├── TooltipLayer
├── TransitionLayer
└── ModalLayer
```

## Phase 1 — Produce the complete static UI asset kit

### Phase objective

Create every feasible static image and UI surface before Unity integration begins. Phase 1 is no longer limited to references and background textures. It delivers the complete reusable visual shell of the gameplay screen: backgrounds, frames, panels, card bases, controls, meters, slots, overlays, icons, and visual-state layers.

You must not create, redraw, or approximate any UI artwork in Unity. Your Phase 1 responsibility is limited to importing the supplied files, configuring them correctly, assembling prefabs, adding runtime fields, and binding behavior.

Creature illustrations and technique illustrations are content assets rather than generic UI elements. They will also be created outside Unity, but they may be produced incrementally in their relevant content phases. Their card frames, masks, sockets, panels, and state treatments must already exist at the end of Phase 1.

### Static artwork and runtime boundary for Phase 1

| Created as an image during Phase 1 | Added or controlled in Unity at runtime |
| --- | --- |
| Background artwork and texture layers | Layer visibility and optional subtle parallax |
| Blank panel, frame, card, button, slot, and meter artwork | Size through approved nine-slicing, placement, and active state |
| Hover, selected, pressed, disabled, warning, and critical state layers | State selection and transitions |
| Blank title, stat, description, and rules regions | Names, numbers, descriptions, and keywords |
| Empty marker sockets and meter tracks | Marker count, fill amount, warning thresholds, and labels |
| Generic gameplay icons | Icon selection, count, tooltip, and visibility |
| Tooltip, modal, and summary surfaces | Runtime content and confirmation behavior |

Do not bake creature names, technique names, weight values, value numbers, effects, load totals, depth values, tension values, button availability, or changing labels into the artwork.

### Phase 1 asset inventory

#### Reference and specification

- `Reference/ui-style-guide.md`
- `Reference/theme-palette.png`
- `Reference/typography-reference.png`
- `Reference/component-dimensions.png`
- `Reference/visual-states.png`
- `Reference/gameplay-layout-1920x1080.png`
- `Reference/ui-component-contact-sheet.png`
- `Reference/gameplay-layout-blank-1920x1080.png`
- `Reference/asset-manifest.md`
- Approved display and body font files in `Fonts/`

#### Base materials and repeatable textures

- `Backgrounds/teal-paper-tile.png`
- `Backgrounds/ivory-paper-tile.png`
- `Frames/oxidized-teal-tile.png`
- `Frames/aged-brass-tile.png`
- `Effects/print-noise-overlay.png`
- `Effects/contact-shadow-soft.png`

#### Gameplay background layers

- `Backgrounds/gameplay-tabletop-16x9.png`
- `Backgrounds/bathymetric-overlay.png`
- `Backgrounds/navigation-chart-overlay.png`
- `Frames/screen-border-9slice.png`
- `Frames/main-column-divider.png`
- `Frames/top-navigation-base-9slice.png`

#### Reusable panels and frames

- `Frames/maritime-panel-9slice.png`
- `Frames/panel-header-strip-9slice.png`
- `Frames/encounter-panel-9slice.png`
- `Frames/catch-rig-panel-9slice.png`
- `Frames/run-controls-panel-9slice.png`
- `Frames/technique-hand-tray-9slice.png`
- `Frames/compact-card-frame-9slice.png`
- `Frames/creature-card-frame-9slice.png`
- `Frames/technique-card-frame-9slice.png`

#### Blank card components

- `Cards/creature-card-base.png`
- `Cards/creature-art-mask.png`
- `Cards/creature-title-plate-9slice.png`
- `Cards/creature-effect-panel-9slice.png`
- `Cards/creature-stat-plate-9slice.png`
- `Cards/creature-card-back.png`
- `Cards/compact-catch-card-base.png`
- `Cards/technique-card-base.png`
- `Cards/technique-art-mask.png`
- `Cards/technique-title-plate-9slice.png`
- `Cards/technique-rules-panel-9slice.png`
- `Cards/technique-card-back.png`
- `Cards/card-disabled-overlay.png`

#### Catch Rig, load, depth, and marker components

- `Rig/main-line-segment.png`
- `Rig/branch-line-segment.png`
- `Rig/attachment-clasp.png`
- `Rig/catch-slot-empty.png`
- `Rig/hook-terminal.png`
- `Meters/line-load-track-9slice.png`
- `Meters/line-load-fill-safe-9slice.png`
- `Meters/line-load-fill-warning-9slice.png`
- `Meters/line-load-fill-critical-9slice.png`
- `Meters/depth-track-9slice.png`
- `Meters/depth-marker.png`
- `Meters/tension-track-9slice.png`
- `Meters/tension-fill-9slice.png`
- `Markers/anchor-socket-empty.png`
- `Markers/anchor-filled.png`
- `Markers/status-socket-empty.png`

#### Controls and interaction surfaces

- `Controls/action-button-normal-9slice.png`
- `Controls/action-button-hover-9slice.png`
- `Controls/action-button-pressed-9slice.png`
- `Controls/action-button-disabled-9slice.png`
- `Controls/action-button-destructive-9slice.png`
- `Controls/confirm-button-9slice.png`
- `Controls/cancel-button-9slice.png`
- `Controls/controller-focus-frame-9slice.png`
- `Controls/card-hover-frame-9slice.png`
- `Controls/card-selected-frame-9slice.png`

#### Overlays and feedback layers

- `Overlays/tooltip-panel-9slice.png`
- `Overlays/modal-dim.png`
- `Overlays/release-mode-vignette.png`
- `Overlays/release-valid-frame.png`
- `Overlays/release-invalid-overlay.png`
- `Overlays/surface-summary-panel-9slice.png`
- `Overlays/warning-glow-9slice.png`
- `Overlays/critical-glow-9slice.png`
- `Overlays/unavailable-hatch.png`

#### Generic gameplay icons

- `Icons/weight.png`
- `Icons/value.png`
- `Icons/depth.png`
- `Icons/tension.png`
- `Icons/load.png`
- `Icons/descend.png`
- `Icons/catch.png`
- `Icons/release.png`
- `Icons/surface.png`
- `Icons/value-loss.png`
- `Icons/weight-removed.png`
- `Icons/effect-lost.png`
- `Icons/haul-value.png`
- `Icons/creature-count.png`
- `Icons/warning.png`
- `Icons/critical.png`
- `Icons/locked.png`
- `Icons/info.png`

### Task 1.1 — Lock the visual specification

#### Assets to create or prepare in Task 1.1

- [x] `Reference/theme-palette.png` — already created before this task breakdown; treat it as the approved starting palette.
- [x] `Reference/ui-style-guide.md`
- [x] `Reference/typography-reference.png`
- [x] `Reference/component-dimensions.png`
- [x] `Reference/visual-states.png` — initial direction sheet; it will be validated and finalized in Task 1.8.
- [x] `Reference/gameplay-layout-1920x1080.png` — approved populated composition reference, not a gameplay background.
- [x] `Reference/asset-manifest.md` — initial manifest containing filenames, native dimensions, transparency, pivots, safe regions, tint rules, and nine-slice borders.
- [x] Approved decorative display font files in `Fonts/`.
- [x] Approved readable body font files in `Fonts/`.

#### Assets reused from previous tasks

- None. This is the first Phase 1 task.

- [ ] Finalize the palette, typography, dimensions, spacing, border widths, and corner radii.
- [ ] Define the native pixel dimensions and pivot for every Phase 1 asset.
- [ ] Define which assets are opaque, translucent, tintable, tiled, masked, or nine-sliced.
- [ ] Define the safe text and artwork regions for all card and panel bases.
- [ ] Ensure the visual system is original and does not reproduce the stacked illustrated-scene composition associated with *Spire's End: Hildegard*.

### Task 1.2 — Produce base materials

#### Assets to create in Task 1.2

- [x] `Backgrounds/teal-paper-tile.png`
- [x] `Backgrounds/ivory-paper-tile.png`
- [x] `Frames/oxidized-teal-tile.png`
- [x] `Frames/aged-brass-tile.png`
- [x] `Effects/print-noise-overlay.png`
- [x] `Effects/contact-shadow-soft.png`

Task 1.2 integration note: all required base material assets are now present in the project and recorded in `Reference/asset-manifest.md`.

#### Assets reused from previous tasks

- `Reference/theme-palette.png` — created in Task 1.1.
- `Reference/ui-style-guide.md` — created in Task 1.1.
- `Reference/component-dimensions.png` — created in Task 1.1.
- `Reference/asset-manifest.md` — created in Task 1.1 and updated with the material assets from this task.

- [ ] Create the dark teal and ivory seamless paper textures.
- [ ] Create oxidized teal and aged-brass material tiles.
- [ ] Create restrained print-noise and contact-shadow layers.
- [ ] Verify that tiled textures do not contain visible seams or directional motifs.

### Task 1.3 — Produce the gameplay background package

#### Assets to create in Task 1.3

- [x] `Backgrounds/gameplay-tabletop-16x9.png`
- [x] `Backgrounds/bathymetric-overlay.png`
- [x] `Backgrounds/navigation-chart-overlay.png`
- [x] `Backgrounds/edge-vignette.png`
- [x] `Frames/screen-border-9slice.png`
- [x] `Frames/main-column-divider.png`
- [x] `Frames/top-navigation-base-9slice.png`

#### Assets reused from previous tasks

- `Reference/theme-palette.png` — created in Task 1.1.
- `Reference/ui-style-guide.md` — created in Task 1.1.
- `Reference/gameplay-layout-1920x1080.png` — created in Task 1.1; use it for composition and density.
- `Reference/component-dimensions.png` — created in Task 1.1.
- `Reference/asset-manifest.md` — created in Task 1.1 and updated during this task.
- `Backgrounds/teal-paper-tile.png` — created in Task 1.2.
- `Frames/oxidized-teal-tile.png` — created in Task 1.2.
- `Frames/aged-brass-tile.png` — created in Task 1.2.
- `Effects/print-noise-overlay.png` — created in Task 1.2.
- `Effects/contact-shadow-soft.png` — created in Task 1.2.

- [x] Create the complete `1920 × 1080` tabletop background.
- [x] Export bathymetric, navigation-chart, vignette, and border elements as separate layers.
- [x] Keep background geometry low contrast and away from important text zones.
- [x] Do not add literal underwater scenery, boats, creatures, arrows, or current lines.

Task 1.3 integration note: all seven supplied images are sorted into `Backgrounds/` and `Frames/`, inspected, and recorded in `Reference/asset-manifest.md`. Committed sprite metadata preserves native size and alpha without compression or mipmaps. Slice boundaries are derived from the artwork because the drop supplied no slice metadata. Canvas assembly, rendered slice validation, and final contrast with runtime text remain in Task 2.2; Task 1.3 does not change the gameplay scene.

### Task 1.4 — Produce reusable frames and panels

#### Assets to create in Task 1.4

- [x] `Frames/maritime-panel-9slice.png`
- [x] `Frames/panel-header-strip-9slice.png`
- [x] `Frames/encounter-panel-9slice.png`
- [x] `Frames/catch-rig-panel-9slice.png`
- [x] `Frames/run-controls-panel-9slice.png`
- [x] `Frames/technique-hand-tray-9slice.png`
- [x] `Frames/compact-card-frame-9slice.png`
- [x] `Frames/creature-card-frame-9slice.png`
- [x] `Frames/technique-card-frame-9slice.png`

#### Assets reused from previous tasks

- `Reference/theme-palette.png` — created in Task 1.1.
- `Reference/ui-style-guide.md` — created in Task 1.1.
- `Reference/component-dimensions.png` — created in Task 1.1.
- `Reference/visual-states.png` — created in Task 1.1; use it for shared border and state language.
- `Reference/asset-manifest.md` — created in Task 1.1 and updated during this task.
- `Backgrounds/teal-paper-tile.png` — created in Task 1.2.
- `Backgrounds/ivory-paper-tile.png` — created in Task 1.2.
- `Frames/oxidized-teal-tile.png` — created in Task 1.2.
- `Frames/aged-brass-tile.png` — created in Task 1.2.
- `Effects/print-noise-overlay.png` — created in Task 1.2.
- `Effects/contact-shadow-soft.png` — created in Task 1.2.
- `Frames/screen-border-9slice.png` — created in Task 1.3; use it as the reference for outer brass treatment.
- `Frames/main-column-divider.png` — created in Task 1.3; reuse its line weight.
- `Frames/top-navigation-base-9slice.png` — created in Task 1.3; reuse its corner and border system.

- [x] Create every panel and frame in the Phase 1 inventory.
- [x] Keep center regions clean enough for nine-slicing and runtime content.
- [x] Export ornamental corners separately when they must not stretch. These designs protect all ornament within nine-slice corner regions, so separate exports are unnecessary.
- [ ] Verify that brass and oxidized-teal treatments remain consistent at every scale.

Task 1.4 creation note: the user explicitly authorized image generation for this task. All nine individual PNGs were generated, visually inspected, saved in `Assets/FishingUIAssets/Frames/`, and given Unity sprite import settings. Native dimensions, alpha, safe regions and slice borders are recorded in the manifest; prompts are in `Docs/Task 1.4 Image Generation.md`. Source-level checks are complete. The final scale-validation checkbox remains open until sliced rendering is checked in Unity. No gameplay scene or runtime mechanics were changed.

### Task 1.5 — Produce blank card systems

#### Assets to create in Task 1.5

##### Creature card

- [x] `Cards/Creature/creature-card-base.png`
- [x] `Cards/Creature/creature-art-mask.png`
- [x] `Cards/Creature/creature-title-plate-9slice.png`
- [x] `Cards/Creature/creature-effect-panel-9slice.png`
- [x] `Cards/Creature/creature-stat-plate-9slice.png`
- [x] `Cards/Creature/creature-card-back.png`

##### Compact catch card

- [x] `Cards/CompactCatch/compact-catch-card-base.png`

##### Technique card

- [x] `Cards/Technique/technique-card-base.png`
- [x] `Cards/Technique/technique-art-mask.png`
- [x] `Cards/Technique/technique-title-plate-9slice.png`
- [x] `Cards/Technique/technique-rules-panel-9slice.png`
- [x] `Cards/Technique/technique-card-back.png`

##### Shared card and marker layers

- [x] `Cards/card-disabled-overlay.png`
- [x] `Markers/anchor-socket-empty.png`
- [x] `Markers/anchor-filled.png`
- [x] `Markers/status-socket-empty.png`

#### Assets reused from previous tasks

- `Reference/theme-palette.png` — created in Task 1.1.
- `Reference/ui-style-guide.md` — created in Task 1.1.
- `Reference/typography-reference.png` — created in Task 1.1; use it to size blank title and rules regions.
- `Reference/component-dimensions.png` — created in Task 1.1.
- `Reference/visual-states.png` — created in Task 1.1.
- `Reference/asset-manifest.md` — created in Task 1.1 and updated during this task.
- All material tiles and `Effects/print-noise-overlay.png` — created in Task 1.2.
- `Effects/contact-shadow-soft.png` — created in Task 1.2; keep shadows separate from card bases.
- `Frames/compact-card-frame-9slice.png` — created in Task 1.4.
- `Frames/creature-card-frame-9slice.png` — created in Task 1.4.
- `Frames/technique-card-frame-9slice.png` — created in Task 1.4.
- `Frames/panel-header-strip-9slice.png` — created in Task 1.4; reuse its title-plate treatment.

- [x] Create blank creature, compact catch, and technique card bases.
- [x] Create separate masks, title plates, stat plates, rules/effect panels, sockets, and card backs.
- [x] Leave all changing names, numbers, descriptions, effects, and artwork absent.
- [x] Preserve the flat, tactile, shadowless engraved maritime presentation.

Task 1.5 asset creation is complete: sixteen individual generated PNGs and their sprite import metadata are present. The user authorized generation for this task. Canonical family subfolders are shown above. The creature base includes only the fixed WEIGHT and VALUE labels. Alpha, native dimensions and slice settings were checked at source level; card assembly, live text fit, stencil clipping and final-size legibility have not been tested in Unity. See the asset manifest and `Docs/Task 1.5 Image Generation.md`.

### Task 1.6 — Produce meters, Catch Rig components, and controls

#### Assets to create in Task 1.6

##### Catch Rig

- [x] `Rig/main-line-segment.png`
- [x] `Rig/branch-line-segment.png`
- [x] `Rig/attachment-clasp.png`
- [x] `Rig/catch-slot-empty.png`
- [x] `Rig/hook-terminal.png`

##### Line Load, depth, and tension

- [x] `Meters/line-load-track-9slice.png`
- [x] `Meters/line-load-fill-safe-9slice.png`
- [x] `Meters/line-load-fill-warning-9slice.png`
- [x] `Meters/line-load-fill-critical-9slice.png`
- [x] `Meters/depth-track-9slice.png`
- [x] `Meters/depth-marker.png`
- [x] `Meters/tension-track-9slice.png`
- [x] `Meters/tension-fill-9slice.png`

##### Controls and interaction frames

- [x] `Controls/action-button-normal-9slice.png`
- [x] `Controls/action-button-hover-9slice.png`
- [x] `Controls/action-button-pressed-9slice.png`
- [x] `Controls/action-button-disabled-9slice.png`
- [x] `Controls/action-button-destructive-9slice.png`
- [x] `Controls/confirm-button-9slice.png`
- [x] `Controls/cancel-button-9slice.png`
- [x] `Controls/controller-focus-frame-9slice.png`
- [x] `Controls/card-hover-frame-9slice.png`
- [x] `Controls/card-selected-frame-9slice.png`

#### Assets reused from previous tasks

- `Reference/theme-palette.png` — created in Task 1.1.
- `Reference/ui-style-guide.md` — created in Task 1.1.
- `Reference/component-dimensions.png` — created in Task 1.1.
- `Reference/visual-states.png` — created in Task 1.1; use it for all interaction states.
- `Reference/asset-manifest.md` — created in Task 1.1 and updated during this task.
- All material tiles and shared effect textures — created in Task 1.2.
- `Frames/main-column-divider.png` — created in Task 1.3; reuse its brass line treatment for Rig segments.
- `Frames/maritime-panel-9slice.png` — created in Task 1.4.
- `Frames/catch-rig-panel-9slice.png` — created in Task 1.4.
- `Frames/run-controls-panel-9slice.png` — created in Task 1.4.
- `Markers/anchor-socket-empty.png` — created in Task 1.5.
- `Markers/anchor-filled.png` — created in Task 1.5.
- `Markers/status-socket-empty.png` — created in Task 1.5.
- `Cards/card-disabled-overlay.png` — created in Task 1.5; reuse its disabled-state texture language.

- [x] Create line, clasp, hook, slot, load, depth, tension, and marker assets as separate PNGs.
- [x] Create normal, hover, pressed, disabled, destructive, selected, and controller-focus surfaces.
- [ ] Ensure state differences remain readable without relying on color alone.
- [ ] Keep all controls visually compatible with mouse, keyboard, and controller focus.

All 23 generated PNGs and their import metadata are present. Source alpha and bounds checks passed. The final two checks remain open: final-size readability, state alignment and focus compatibility have not been verified in Unity. See the asset manifest and `Docs/Task 1.6 Image Generation.json`.

### Task 1.7 — Produce overlays and icon set

#### Assets to create in Task 1.7

##### Overlays

- [ ] `Overlays/tooltip-panel-9slice.png`
- [ ] `Overlays/modal-dim.png`
- [ ] `Overlays/release-mode-vignette.png`
- [ ] `Overlays/release-valid-frame.png`
- [ ] `Overlays/release-invalid-overlay.png`
- [ ] `Overlays/surface-summary-panel-9slice.png`
- [ ] `Overlays/warning-glow-9slice.png`
- [ ] `Overlays/critical-glow-9slice.png`
- [ ] `Overlays/unavailable-hatch.png`

##### Generic gameplay icons

- [ ] `Icons/weight.png`
- [ ] `Icons/value.png`
- [ ] `Icons/depth.png`
- [ ] `Icons/tension.png`
- [ ] `Icons/load.png`
- [ ] `Icons/descend.png`
- [ ] `Icons/catch.png`
- [ ] `Icons/release.png`
- [ ] `Icons/surface.png`
- [ ] `Icons/value-loss.png`
- [ ] `Icons/weight-removed.png`
- [ ] `Icons/effect-lost.png`
- [ ] `Icons/haul-value.png`
- [ ] `Icons/creature-count.png`
- [ ] `Icons/warning.png`
- [ ] `Icons/critical.png`
- [ ] `Icons/locked.png`
- [ ] `Icons/info.png`

#### Assets reused from previous tasks

- `Reference/theme-palette.png` — created in Task 1.1.
- `Reference/ui-style-guide.md` — created in Task 1.1.
- `Reference/component-dimensions.png` — created in Task 1.1.
- `Reference/visual-states.png` — created in Task 1.1.
- `Reference/asset-manifest.md` — created in Task 1.1 and updated during this task.
- All material tiles and shared effect textures — created in Task 1.2.
- `Backgrounds/edge-vignette.png` — created in Task 1.3; reuse its edge behavior for Release mode.
- `Frames/maritime-panel-9slice.png` — created in Task 1.4; reuse it as the basis for tooltip and summary surfaces.
- `Frames/encounter-panel-9slice.png` — created in Task 1.4; reuse its highlight geometry.
- `Frames/compact-card-frame-9slice.png` — created in Task 1.4; reuse its release-selection geometry.
- `Cards/card-disabled-overlay.png` — created in Task 1.5; reuse its hatch and opacity language.
- All control-state surfaces — created in Task 1.6.
- All safe, warning, and critical meter fills — created in Task 1.6; reuse their state colors and textures.

- [ ] Create tooltip, confirmation, Release, Surface, warning, critical, and unavailable layers.
- [ ] Create every generic gameplay icon listed in the inventory.
- [ ] Match icon line weight, engraving texture, silhouette clarity, and optical size.
- [ ] Export each icon separately with transparent padding and a consistent canvas size.

### Task 1.8 — Export, document, and validate the package

#### Assets to create or finalize in Task 1.8

- [ ] `Reference/ui-component-contact-sheet.png`
- [ ] `Reference/gameplay-layout-blank-1920x1080.png`
- [ ] `Reference/visual-states.png` — already created as an initial direction sheet in Task 1.1; update it with the final approved assets from Tasks 1.4–1.7.
- [ ] `Reference/component-dimensions.png` — already created in Task 1.1; update it only if validation changed a native size, pivot, safe region, or nine-slice border.
- [ ] `Reference/ui-style-guide.md` — already created in Task 1.1; finalize it with approved typography and texture usage.
- [ ] `Reference/asset-manifest.md` — already created in Task 1.1 and updated throughout Tasks 1.2–1.7; finalize every entry.

#### Assets reused from previous tasks

- Every reference and font asset created in Task 1.1.
- Every material and shared effect asset created in Task 1.2.
- Every gameplay background layer created in Task 1.3.
- Every frame and panel created in Task 1.4.
- Every blank card and marker asset created in Task 1.5.
- Every Catch Rig, meter, and control asset created in Task 1.6.
- Every overlay and icon created in Task 1.7.

No new gameplay UI component should be invented during Task 1.8. If validation reveals a missing asset, add it to the task that owns that asset category, create it there, and then repeat final validation.

- [ ] Use transparent PNGs for isolated UI elements and icons.
- [ ] Keep opaque full-frame backgrounds in RGB PNG format.
- [ ] Record native dimensions, pivot, border values, tint permission, opacity, and intended prefab in `Reference/asset-manifest.md`.
- [ ] Record the exact font files and TextMeshPro roles in `Reference/ui-style-guide.md`.
- [ ] Create a contact sheet showing every UI element and every visual state.
- [ ] Create a `1920 × 1080` assembly reference showing the intended final hierarchy.
- [ ] Test nine-sliced assets at minimum, reference, and expanded sizes.
- [ ] Verify transparent edges at high zoom against both dark teal and warm ivory.

### Phase 1 acceptance criteria

- Every static UI image required to assemble the gameplay screen exists before Unity implementation proceeds.
- The Unity project can construct the full blank gameplay interface without drawing or generating any new artwork.
- All changing gameplay data remains absent from the images and can be inserted at runtime.
- Repeated textures tile without obvious seams.
- Nine-sliced elements stretch without distorting corners, borders, or ornaments.
- Every recurring interaction state has an approved supplied visual treatment.
- Text remains readable at `1366 × 768` and `1920 × 1080`.
- No asset contains decorative arrows, current indicators, literal underwater scenery, unexplained status symbols, or accidental button-like marks.
- The complete kit shares the same original engraved maritime card identity.
- `Reference/asset-manifest.md` contains no unresolved Phase 1 asset entries.

## Phase 2 — Build the gameplay screen foundation

### Supplied UI elements required for Phase 2

- `Backgrounds/gameplay-tabletop-16x9.png`
- `Backgrounds/bathymetric-overlay.png`
- `Backgrounds/edge-vignette.png`
- `Frames/screen-border-9slice.png`
- `Frames/main-column-divider.png`
- `Frames/top-navigation-base-9slice.png`
- `Effects/contact-shadow-soft.png`

### Task 2.1 — Create the root gameplay view

- [ ] Create a scene or prefab named `FishingRunView`.
- [ ] Add the complete screen hierarchy shown above.
- [ ] Configure the Canvas Scaler with reference resolution `1920 × 1080`.
- [ ] Use `Scale With Screen Size` and start with Match set to approximately `0.5`.
- [ ] Create separate sorting layers for normal UI, tooltips, transitions, and modals.

#### Acceptance criteria

- The view fills a 16:9 screen without stretching.
- Modal and tooltip layers always render above gameplay UI.
- Major sections can be enabled independently.

### Task 2.2 — Assemble the supplied background

- [ ] Add the supplied deep desaturated teal tabletop/map surface.
- [ ] Layer the supplied paper grain.
- [ ] Layer the supplied bathymetric and brass navigation artwork.
- [ ] Add the supplied restrained edge vignette.
- [ ] Keep the center clear enough for the encounter card.

#### Acceptance criteria

- The background supports the maritime theme without becoming a scene.
- It contains no prominent arrows, current streaks, or unexplained symbols.
- UI remains readable without adding heavy opaque panels everywhere.

### Task 2.3 — Implement the main responsive layout

- [ ] Reserve approximately 7% of the screen height for the top bar.
- [ ] Allocate approximately 22% of screen width to the Catch Rig.
- [ ] Allocate approximately 43% to the encounter area.
- [ ] Allocate approximately 22% to the controls.
- [ ] Reserve the lower portion for the four-card technique hand.
- [ ] Add consistent margins and gutters between sections.

#### Acceptance criteria

- The encounter card and technique hand form the strongest hierarchy.
- Side panels remain visually secondary.
- No section overlaps another at supported resolutions.

## Phase 3 — Create reusable visual components

### Supplied UI elements required for Phase 3

- `Frames/maritime-panel-dark-9slice.png`
- `Frames/maritime-panel-ivory-9slice.png`
- `Frames/maritime-panel-turquoise-9slice.png`
- `Frames/maritime-panel-coral-9slice.png`
- `Frames/header-plaque-ivory-9slice.png`
- `Cards/Creature/creature-card-base.png`
- `Cards/Creature/creature-art-mask.png`
- `Cards/Creature/creature-art-overflow-mask.png`
- `Cards/Creature/creature-card-back.png`
- `Cards/CompactCatch/compact-catch-card-base.png`
- `Cards/Technique/technique-card-base.png`
- `Cards/Technique/technique-card-back.png`
- `Overlays/card-hover-border.png`
- `Overlays/card-selected-border.png`
- `Overlays/card-disabled-overlay.png`
- `Controls/action-button-turquoise-9slice.png`
- `Controls/action-button-coral-9slice.png`
- `Controls/action-button-ivory-9slice.png`
- `Controls/action-button-disabled-overlay.png`

### Task 3.1 — Assemble `MaritimePanel` from supplied sprites

- [ ] Build a reusable framed panel component.
- [ ] Support dark teal, ivory, turquoise, and coral interior variants.
- [ ] Support an optional header and icon.
- [ ] Support brass and oxidized-teal nested borders.
- [ ] Support optional corner ornament sprites.
- [ ] Expose padding and accent color through serialized fields.

#### Acceptance criteria

- The Catch Rig, controls, load display, and depth tracker can all use this component.
- Borders remain crisp when panels are resized.
- Nine-slicing does not distort corner ornaments.

### Task 3.2 — Assemble `CreatureCardView` from supplied layers

- [ ] Build the creature card from layered UI elements.
- [ ] Use the blank Sea Creature Template as its base artwork.
- [ ] Add dynamic creature artwork.
- [ ] Add dynamic creature name, weight, value, and effect text.
- [ ] Add exactly four dynamic anchor-marker slots.
- [ ] Create explicit safe regions for title, stats, rules, and markers.
- [ ] Allow creature artwork to overlap only approved decorative boundaries.

Suggested hierarchy:

```text
CreatureCardView
├── CardBackground
├── CreatureArtworkMask
│   └── CreatureArtwork
├── ArtworkOverflowLayer
├── NameText
├── WeightText
├── ValueText
├── EffectText
├── AnchorSlots
│   ├── Anchor01
│   ├── Anchor02
│   ├── Anchor03
│   └── Anchor04
└── InteractionOverlay
```

#### Acceptance criteria

- Every data field can be updated at runtime.
- Blank fields remain visually clean.
- Creature artwork never blocks essential text or markers.
- All creature cards share identical geometry.

### Task 3.3 — Assemble `CompactCatchCard` from supplied layers

- [ ] Create a compact creature-card variant for the Catch Rig.
- [ ] Display creature name, portrait, weight, value, and optional passive-effect icon.
- [ ] Hide the full effect description.
- [ ] Add an attachment point for the rig.
- [ ] Add selectable, disabled, and release-candidate states.

#### Acceptance criteria

- Compact cards are readable at their final displayed size.
- Three or more catches can appear without hiding important information.
- The compact card remains recognizably part of the creature-card family.

### Task 3.4 — Assemble `TechniqueCardView` from supplied layers

- [ ] Create the technique-card frame using the shared theme.
- [ ] Add technique name, equipment illustration, rules text, and optional keyword area.
- [ ] Add normal, hovered, selected, playable, and disabled states.
- [ ] Keep the design simpler than a creature card.

#### Acceptance criteria

- Four technique cards remain readable simultaneously.
- Hovering one card does not hide the other cards completely.
- Playability is understandable without relying only on color.

### Task 3.5 — Assemble `RunActionButton` from supplied layers

- [ ] Create a reusable large action button with brass frame, icon, and label.
- [ ] Add normal, hover, pressed, disabled, and dangerous states.
- [ ] Create theme variants for Descend, Release, and Surface.

#### Acceptance criteria

- Every action is readable at a glance.
- Disabled actions remain legible but clearly unavailable.
- Buttons share consistent geometry and feedback.

## Phase 4 — Build the top navigation bar

### Supplied UI elements required for Phase 4

- `Frames/top-navigation-base-9slice.png`
- `Frames/top-biome-plaque-9slice.png`
- `Frames/top-depth-plaque-9slice.png`
- `Frames/top-deck-plaque-9slice.png`
- `Icons/compass-location.png`
- `Icons/deck.png`
- `Icons/settings.png`
- `Icons/depth-marker.png`
- `Overlays/top-navigation-divider.png`

### Task 4.1 — Add the biome display

- [ ] Place the biome name on the upper-left.
- [ ] Add a small compass or location icon.
- [ ] Support dynamic biome names.

### Task 4.2 — Add the depth display

- [ ] Place the current depth in the upper center.
- [ ] Format it as `DEPTH 180 m`.
- [ ] Support smooth numeric updates.

### Task 4.3 — Add secondary controls

- [ ] Add the remaining deck count on the upper-right.
- [ ] Add a small deck icon.
- [ ] Add the settings button.
- [ ] Keep these visually secondary to biome and depth.

#### Phase acceptance criteria

- Biome and depth can be read immediately.
- The top bar remains compact.
- Long biome names do not collide with the center depth display.

## Phase 5 — Build the Catch Rig

### Supplied UI elements required for Phase 5

- `CatchRig/catch-rig-panel-9slice.png`
- `CatchRig/catch-rig-title-plaque.png`
- `CatchRig/vertical-rail.png`
- `CatchRig/rail-end-cap.png`
- `CatchRig/attachment-ring.png`
- `CatchRig/card-connector.png`
- `CatchRig/empty-catch-slot.png`
- `CatchRig/release-selection-frame.png`
- `CatchRig/unavailable-catch-overlay.png`
- `Icons/anchor.png`
- `Icons/passive-effect-placeholder.png`
- `Meters/line-load-panel-9slice.png`
- `Meters/line-load-track.png`
- `Meters/line-load-segment-empty.png`
- `Meters/line-load-segment-safe.png`
- `Meters/line-load-segment-warning.png`
- `Meters/line-load-segment-overloaded.png`
- `Meters/line-load-critical-overlay.png`
- Initial compact-card creature artwork: Sardine, Crab, and Squid

### Task 5.1 — Assemble the rig structure from supplied assets

- [ ] Create a vertical brass rail or line inside the Catch Rig panel.
- [ ] Add attachment points for compact catch cards.
- [ ] Use small brass rings and anchor tokens to communicate attachment.
- [ ] Keep this rig outside the creature-card artwork.

### Task 5.2 — Arrange caught creatures

- [ ] Stack compact catch cards vertically along the rig.
- [ ] Allow small controlled card angles.
- [ ] Allow restrained overlap when the chain grows.
- [ ] Add layout rules for one through six visible catches.
- [ ] Add scrolling or compression behavior for larger chains.

### Task 5.3 — Add release-selection feedback

- [ ] Highlight creatures that can be released.
- [ ] Dim unavailable catches.
- [ ] Show the weight removed by releasing a selected creature.
- [ ] Show an indicator when a passive effect would be lost.

### Task 5.4 — Assemble `LineLoadMeter` from supplied meter sprites

- [ ] Display the current and maximum load numerically.
- [ ] Add segmented graphical load markers.
- [ ] Use turquoise for safe load, aged gold near capacity, and coral when overloaded.
- [ ] Add a critical state for severe overload.

#### Phase acceptance criteria

- The Catch Rig is recognizable as loot, risk, and temporary build state.
- The player can identify individual catches and their weight.
- Load state is understandable from both color and numbers.

## Phase 6 — Build the encounter area

### Supplied UI elements required for Phase 6

- `Frames/encounter-holder.png`
- `Frames/current-encounter-plaque-9slice.png`
- `Cards/Creature/creature-card-base.png`
- `Cards/Creature/creature-card-back.png`
- `Overlays/encounter-hidden-overlay.png`
- `Overlays/encounter-reveal-border.png`
- `Overlays/encounter-catch-highlight.png`
- `Effects/card-contact-shadow.png`
- Initial full-size creature artwork: Mackerel

### Task 6.1 — Assemble the encounter holder from supplied assets

- [ ] Center one large `CreatureCardView`.
- [ ] Support empty, hidden, revealing, active, caught, and released states.
- [ ] Keep space around the card for movement and emphasis.

### Task 6.2 — Add the encounter plaque

- [ ] Add a small ivory plaque beneath the card.
- [ ] Display `CURRENT ENCOUNTER`.
- [ ] Keep the plaque visually secondary to the card.

### Task 6.3 — Implement card transitions

- [ ] Reveal an encounter with a short card-slide or flip motion.
- [ ] Move a caught creature toward the Catch Rig.
- [ ] Move a released creature away from the rig.
- [ ] Avoid large particles, flashes, or combat-style effects.

#### Phase acceptance criteria

- The current encounter is always the primary focal element.
- State changes are clear without explanatory popups.
- Movement resembles physical cards on a tabletop.

## Phase 7 — Build the run controls

### Supplied UI elements required for Phase 7

- `Controls/run-controls-panel-9slice.png`
- `Controls/depth-tracker-panel-9slice.png`
- `Controls/depth-track.png`
- `Controls/depth-node-empty.png`
- `Controls/depth-node-complete.png`
- `Controls/depth-node-current.png`
- `Controls/depth-node-locked.png`
- `Meters/tension-track.png`
- `Meters/tension-fill-safe.png`
- `Meters/tension-fill-warning.png`
- `Meters/tension-fill-critical.png`
- `Controls/action-button-turquoise-9slice.png`
- `Controls/action-button-coral-9slice.png`
- `Controls/action-button-ivory-9slice.png`
- `Icons/descend.png`
- `Icons/release.png`
- `Icons/surface.png`
- `Icons/tension.png`

### Task 7.1 — Assemble `DepthTracker` from supplied assets

- [ ] Add a vertical zone tracker.
- [ ] Support biome names and depth ranges.
- [ ] Show the player's current zone.
- [ ] Show completed, current, upcoming, and locked zones.

Initial visual example:

- Shelf: `0–200 m`
- Open Water: `200–1000 m`
- Trench: `1000+ m`

### Task 7.2 — Assemble the tension display from supplied assets

- [ ] Display a text state such as `TENSION: LOW`.
- [ ] Add a compact visual meter.
- [ ] Support Low, Rising, High, and Critical states.
- [ ] Use restrained color and movement.

### Task 7.3 — Add permanent actions

- [ ] Add Descend using the turquoise button style.
- [ ] Add Release using the coral button style.
- [ ] Add Surface using the ivory button style.
- [ ] Add icons and tooltips.
- [ ] Support disabled and confirmation-required states.

#### Phase acceptance criteria

- Permanent actions remain visible regardless of the technique hand.
- The player can read depth and tension without opening another panel.
- Controls do not compete visually with the encounter card.

## Phase 8 — Build the technique hand

### Supplied UI elements required for Phase 8

- `Cards/Technique/technique-hand-tray-9slice.png`
- `Cards/Technique/technique-card-base.png`
- `Cards/Technique/technique-card-back.png`
- `Overlays/technique-hover-border.png`
- `Overlays/technique-selected-border.png`
- `Overlays/technique-disabled-overlay.png`
- `Effects/card-contact-shadow.png`
- Initial transparent technique illustrations:
  - `Techniques/strong-knot.png`
  - `Techniques/live-bait.png`
  - `Techniques/quick-release.png`
  - `Techniques/deep-lure.png`

### Task 8.1 — Assemble the supplied hand tray

- [ ] Add a restrained teal-and-brass tray across the lower center.
- [ ] Create four stable card positions.
- [ ] Keep the full card titles visible.

### Task 8.2 — Implement card arrangement

- [ ] Display exactly four technique cards.
- [ ] Apply a very slight fan or rotation.
- [ ] Raise the hovered card.
- [ ] Highlight the selected card.
- [ ] Desaturate unavailable cards.

### Task 8.3 — Implement card inspection

- [ ] Enlarge the hovered or selected card slightly.
- [ ] Reveal its full rules text.
- [ ] Keep the other cards visible.
- [ ] Prevent the inspected card from covering permanent actions.

#### Phase acceptance criteria

- Four cards are visible and readable at all supported resolutions.
- Selection state is clear.
- The hand feels tactile without requiring complex 3D movement.

## Phase 9 — Add interaction feedback

### Supplied UI elements required for Phase 9

- `Effects/card-contact-shadow.png`
- `Effects/card-selection-sheen.png`
- `Effects/load-segment-flash.png`
- `Effects/rig-tension-overlay.png`
- `Effects/button-pressed-overlay.png`
- `Effects/button-disabled-overlay.png`
- Approved animation timing reference in `Reference/ui-style-guide.md`
- Approved audio-event names in `Reference/asset-manifest.md`

### Task 9.1 — Add physical card movement

- [ ] Animate encounter reveal.
- [ ] Animate catch-card attachment.
- [ ] Animate release.
- [ ] Animate technique play and replacement draw.
- [ ] Animate Catch Rig reordering.

### Task 9.2 — Add load feedback

- [ ] Animate newly filled load segments.
- [ ] Briefly emphasize the updated numeric total.
- [ ] Add restrained rig movement when load increases.
- [ ] Transition meter color at defined thresholds.

### Task 9.3 — Add button feedback

- [ ] Add small press displacement.
- [ ] Add brass-edge emphasis on hover.
- [ ] Add short color transitions.
- [ ] Add clear disabled feedback.

### Task 9.4 — Add audio hooks

- [ ] Expose events for card slide, card placement, brass click, line strain, and button press sounds.
- [ ] Do not hard-code audio assets inside UI components.

#### Phase acceptance criteria

- Feedback communicates actions without excessive animation.
- UI remains responsive while animations play.
- Animation can be shortened or skipped for rapid interactions.

## Phase 10 — Add tooltips and overlays

### Supplied UI elements required for Phase 10

- `Overlays/tooltip-panel-9slice.png`
- `Overlays/modal-dim.png`
- `Overlays/release-mode-vignette.png`
- `Overlays/release-valid-frame.png`
- `Overlays/release-invalid-overlay.png`
- `Overlays/surface-summary-panel-9slice.png`
- `Controls/confirm-button-9slice.png`
- `Controls/cancel-button-9slice.png`
- `Icons/value-loss.png`
- `Icons/weight-removed.png`
- `Icons/effect-lost.png`
- `Icons/haul-value.png`
- `Icons/creature-count.png`

### Task 10.1 — Assemble `MaritimeTooltip` from supplied assets

- [ ] Use a compact ivory panel with brass border.
- [ ] Support title, description, icon, and keyword explanation.
- [ ] Keep tooltips inside the screen bounds.

### Task 10.2 — Create Release mode

- [ ] Dim unrelated areas.
- [ ] Highlight releasable catches.
- [ ] Show lost value, removed weight, and lost passive effects.
- [ ] Add Confirm and Cancel controls.

### Task 10.3 — Assemble `SurfaceSummaryPanel` from supplied assets

- [ ] Display total catch value.
- [ ] Display total creature count.
- [ ] Display current load.
- [ ] Display haul modifiers.
- [ ] Keep the Catch Rig visible behind the summary.

#### Phase acceptance criteria

- Overlays preserve context rather than replacing the whole screen.
- The player can review consequences before confirming Release or Surface.
- Modal focus and navigation work with mouse and controller.

## Phase 11 — Responsive layout and accessibility

### Supplied UI elements required for Phase 11

- Nine-slice metadata for every stretchable frame and button.
- Safe-area diagram for `1920 × 1080`.
- Reference screenshots at all target resolutions.
- Controller-focus frame sprites.
- Non-color warning and critical-state icons.
- Reduced-motion timing values.

No new visual assets should be invented during responsive implementation. Any missing edge treatment, alternate frame, or accessibility icon must be requested through `MissingVisualAssets.md`.

### Task 11.1 — Configure anchors and layout groups

- [ ] Anchor the top bar to the top edge.
- [ ] Anchor the Catch Rig to the left.
- [ ] Anchor run controls to the right.
- [ ] Anchor the hand to the bottom.
- [ ] Keep the encounter centered in remaining space.

### Task 11.2 — Support alternate aspect ratios

- [ ] Validate all target resolutions.
- [ ] Reduce side-panel width before reducing card readability.
- [ ] Add optional side padding on ultrawide displays.
- [ ] Prevent the hand from moving outside the safe area.

### Task 11.3 — Add accessibility support

- [ ] Do not communicate load or availability through color alone.
- [ ] Add controller focus states.
- [ ] Support UI scale options.
- [ ] Maintain adequate text contrast.
- [ ] Add optional reduced-motion behavior.

#### Phase acceptance criteria

- The interface remains usable with mouse, keyboard, and controller.
- Important text remains readable at minimum supported resolution.
- Critical states have icons or text in addition to color.

## Phase 12 — Final visual consistency pass

### Supplied UI elements required for Phase 12

- Complete approved sprite atlas or organized individual PNG package.
- Final `Reference/asset-manifest.md`.
- Final `Reference/ui-style-guide.md`.
- Final `1920 × 1080` reference screenshot.
- Reference screenshots for all supported resolutions.
- A visual-state sheet covering normal, hover, selected, pressed, disabled, warning, and critical states.
- A content sheet showing the initial creature and technique artwork at native size.

### Task 12.1 — Audit the shared style

- [ ] Confirm consistent brass treatment.
- [ ] Confirm consistent oxidized-teal treatment.
- [ ] Confirm consistent paper texture.
- [ ] Confirm consistent border thickness.
- [ ] Confirm consistent typography.
- [ ] Confirm consistent icon line weight.

### Task 12.2 — Remove misleading decoration

- [ ] Remove decorative arrows and current indicators.
- [ ] Remove unexplained resources or status icons.
- [ ] Remove decorative marks that resemble buttons.
- [ ] Remove literal underwater scenery.
- [ ] Keep visual noise away from text and values.

### Task 12.3 — Compare with the reference

- [ ] Capture the Unity scene at `1920 × 1080`.
- [ ] Compare it side by side with `fishing-roguelike-gameplay-ui-16x9.png`.
- [ ] Match the composition and hierarchy rather than copying accidental image-generation details.
- [ ] Confirm the encounter card is the primary focus.
- [ ] Confirm the Catch Rig is immediately understandable.
- [ ] Confirm four technique cards are visible.
- [ ] Confirm permanent actions are easy to find.
- [ ] Confirm load and depth can be read quickly.
- [ ] Confirm the screen does not feel overcrowded.

### Task 12.4 — Create final prefabs

- [ ] `FishingRunView`
- [ ] `TopNavigationBar`
- [ ] `MaritimePanel`
- [ ] `CatchRigPanel`
- [ ] `CompactCatchCard`
- [ ] `CreatureCardView`
- [ ] `TechniqueCardView`
- [ ] `LineLoadMeter`
- [ ] `DepthTracker`
- [ ] `RunActionButton`
- [ ] `MaritimeTooltip`
- [ ] `SurfaceSummaryPanel`

## Final definition of done

The visual implementation is complete when:

- [ ] The Unity screen closely matches the reference composition at `1920 × 1080`.
- [ ] All UI sections are constructed from reusable prefabs.
- [ ] Creature and technique information can be populated dynamically.
- [ ] The Catch Rig visually communicates accumulated catches and line load.
- [ ] The current encounter, technique hand, depth, tension, and permanent actions are visible simultaneously.
- [ ] The interface scales correctly across all target resolutions.
- [ ] Text is readable and controller navigation is functional.
- [ ] Animations communicate card actions without slowing down play.
- [ ] No decorative element can be mistaken for unexplained gameplay information.
- [ ] The entire screen shares the same engraved maritime card identity.
- [ ] Every visible image comes from the approved supplied asset package.
- [ ] You generated no replacement artwork or UI graphics.
- [ ] All names, numbers, descriptions, load values, depth values, marker counts, and availability states are populated at runtime.
- [ ] `MissingVisualAssets.md` is empty or contains only explicitly deferred content.

## Your execution rules

1. Complete phases in order unless a later task is required to validate an earlier component.
2. Use only assets listed in `Reference/asset-manifest.md` for final visuals.
3. Do not generate, draw, approximate, or redesign images or UI artwork.
4. When an asset is missing, record the required filename, dimensions, transparency, state, and intended location in `MissingVisualAssets.md`.
5. Use obvious temporary placeholders only while waiting for a missing supplied asset.
6. Use placeholder ScriptableObjects and mock runtime data during visual integration.
7. Keep dynamic text, numbers, artwork selection, meter fill, markers, and state visibility separate from static artwork.
8. Do not redesign game mechanics during this work.
9. Do not flatten the interface into a screenshot or single image.
10. Reuse prefabs and theme values instead of duplicating configuration.
11. Preserve existing user code and unrelated project assets.
12. After every phase, capture a `1920 × 1080` screenshot and compare it with the reference.
13. Report completed tasks, changed files, validation performed, missing assets, and remaining visual differences.
14. Stop optional polish once the phase acceptance criteria are satisfied.
