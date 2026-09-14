# Deepwater UI Asset Manifest

This manifest starts the approved asset record for the gameplay UI asset package. It will be expanded as Phase 1 assets are supplied.

Values marked `TBD` are intentionally unresolved because the source asset has not been supplied yet. Do not replace missing final artwork with generated or hand-drawn substitutes.

## Import Defaults

| Field | Default |
| --- | --- |
| Unity texture type | Sprite (2D and UI) for runtime PNGs |
| Sprite mode | Single unless an approved atlas is supplied |
| Mesh type | Full Rect for UI sprites |
| Pixels per unit | 100 unless an asset-specific value is supplied |
| Pivot | Center unless an attachment point is specified |
| Compression | None or high quality for UI implementation |
| Text ownership | Runtime TextMeshPro/UI text |
| Runtime art ownership | Supplied images only |

## Task 1.1 Reference Assets

These files are approved reference/specification assets. They document the visual system and should not be used as flattened gameplay UI elements.

| Filename | Native size | Intended use | Transparency | Pivot | PPU | Nine-slice | Tint | Runtime dependencies | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `Reference/theme-palette.png` | 1672 x 941 | Palette and material reference | Opaque | N/A | N/A | N/A | No | None | Approved starting palette. |
| `Reference/typography-reference.png` | 1920 x 1080 | Runtime text hierarchy reference | Opaque | N/A | N/A | N/A | No | TextMeshPro styles | Runtime text must remain editable. |
| `Reference/component-dimensions.png` | 1920 x 1080 | Layout proportions and safe regions | Opaque | N/A | N/A | N/A | No | Canvas layout | Use for 16:9 composition targets. |
| `Reference/visual-states.png` | 1920 x 1080 | Interaction-state reference | Opaque | N/A | N/A | N/A | No | State layer visibility | Initial direction sheet; final state sprites arrive later. |
| `Reference/gameplay-layout-1920x1080.png` | 1920 x 1080 | Populated gameplay composition reference | Opaque | N/A | N/A | N/A | No | All gameplay UI prefabs | Do not use as a background. |
| `Reference/ui-style-guide.md` | Text | Implementation rules and style lock | N/A | N/A | N/A | N/A | N/A | All UI prefabs | Created in Task 1.1. |
| `Reference/asset-manifest.md` | Text | Asset metadata and import contract | N/A | N/A | N/A | N/A | N/A | All supplied assets | Created in Task 1.1; updated throughout Phase 1. |

## Required Task 1.1 Fonts

| Filename | Native size | Intended use | Transparency | Pivot | PPU | Nine-slice | Tint | Runtime dependencies | Status |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `Fonts/Marcellus/Marcellus-Regular.ttf` | 46,004 bytes | Display, navigation, card titles, panel headings, primary actions | N/A | N/A | N/A | N/A | Runtime text color | TextMeshPro font asset | Supplied. Licensed under SIL OFL 1.1. |
| `Fonts/Marcellus/OFL.txt` | 4,430 bytes | Marcellus license and copyright notice | N/A | N/A | N/A | N/A | N/A | Distribution notices | Supplied. Include with distribution notices. |
| `Fonts/SourceSerif4/SourceSerif4-Variable.ttf` | 1,209,508 bytes | Body, values, rules, small data, status labels | N/A | N/A | N/A | N/A | Runtime text color | TextMeshPro font asset | Supplied. Licensed under SIL OFL 1.1. |
| `Fonts/SourceSerif4/OFL.txt` | 4,400 bytes | Source Serif 4 license and copyright notice | N/A | N/A | N/A | N/A | N/A | Distribution notices | Supplied. Include with distribution notices. |

## Approved Font Usage

| UI role | Font | Weight |
| --- | --- | --- |
| Main card title | Marcellus | Regular |
| Panel heading | Marcellus | Regular |
| Button label | Marcellus | Regular |
| Location name | Marcellus | Regular |
| Short decorative label | Marcellus | Regular |
| Stat number | Source Serif 4 | Bold |
| Effect heading | Source Serif 4 | Semibold |
| Description | Source Serif 4 | Regular |
| Small status text | Source Serif 4 | Medium |

## Task 1.2 Base Materials

These assets establish repeatable material surfaces and reusable atmospheric overlays. They are source sprites/material textures, not complete UI components.

| Filename | Native size | Intended use | Transparency | Pivot | PPU | Nine-slice | Tint | Runtime dependencies | Status |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `Backgrounds/teal-paper-tile.png` | 1254 x 1254 | Dark teal seamless paper texture | Opaque | Center | 100 | No | No | UI material/panel backgrounds | Supplied. No obvious directional motif on visual inspection. |
| `Backgrounds/ivory-paper-tile.png` | 1254 x 1254 | Warm ivory seamless paper texture | Opaque | Center | 100 | No | No | UI material/panel backgrounds | Supplied. No obvious directional motif on visual inspection. |
| `Frames/oxidized-teal-tile.png` | 1254 x 1254 | Oxidized teal metal/material texture | Opaque | Center | 100 | No | No | Frame and panel material references | Supplied. No obvious directional motif on visual inspection. |
| `Frames/aged-brass-tile.png` | 1254 x 1254 | Aged brass material tile for borders, ornaments, and metal surfaces | Opaque | Center | 100 | No | No | Frame and ornament sprites/materials | Supplied. No obvious directional motif on visual inspection. |
| `Effects/print-noise-overlay.png` | 1024 x 1024 | Restrained print-noise overlay | Alpha | Center | 100 | No | Optional opacity tint only | Panel/card/background overlay layers | Supplied. Use at low opacity only. |
| `Effects/contact-shadow-soft.png` | 1024 x 512 | Soft contact shadow layer under cards and panels | Alpha | Center | 100 | No | Optional opacity tint only | Card/panel hover and placement feedback | Supplied. |

## Task 1.3 Gameplay Background Package

All seven supplied PNGs were moved without changing their contents. Their committed Unity import settings use Single sprites, Full Rect meshes, centered pivots, 100 PPU, bilinear filtering, Clamp wrapping, no mipmaps, no compression, and a 2048 maximum texture size. NPOT scaling is disabled to preserve native dimensions.

Nine-slice values below are implementation values derived from the supplied pixels, not creator-supplied metadata. Border order is left, bottom, right, top in source pixels. Confirm their rendered appearance during Task 2.2 assembly.

| Filename | Native size | Intended prefab/use | Transparency | Pivot | PPU | Nine-slice | Tint | Runtime dependencies |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `Backgrounds/gameplay-tabletop-16x9.png` | 1920 x 1080 | FishingRunView / Background base | Opaque RGB | Center | 100 | None | No | Background layout |
| `Backgrounds/bathymetric-overlay.png` | 1920 x 1080 | FishingRunView / Background contours | Alpha; sampled maximum 34/255 | Center | 100 | None | Opacity only | Layer visibility |
| `Backgrounds/navigation-chart-overlay.png` | 1920 x 1080 | FishingRunView / Background chart | Alpha; sampled maximum 30/255 | Center | 100 | None | Opacity only | Layer visibility |
| `Backgrounds/edge-vignette.png` | 1920 x 1080 | FishingRunView / Background vignette | Alpha; clear center, sampled maximum 150/255 | Center | 100 | None | Opacity only | Layer visibility |
| `Frames/screen-border-9slice.png` | 1920 x 1080 | FishingRunView / ScreenBorder | Alpha; clear center | Center | 100 | 72, 72, 72, 72 | No | Sliced Image; screen bounds |
| `Frames/main-column-divider.png` | 64 x 960 | FishingRunView / MainContent dividers | Alpha | Center | 100 | None | No | Column boundaries; preserve ornament proportions |
| `Frames/top-navigation-base-9slice.png` | 1200 x 144 | TopNavigationBar / Base | Alpha around opaque teal panel | Center | 100 | 64, 24, 64, 24 | No | Horizontal slicing; separate runtime navigation fields |

### Background Assembly Contract

- Task 2.2 assembles these separate sprites; Task 1.3 prepares the asset package and import metadata only.
- Back-to-front background order: tabletop, restrained print noise from Task 1.2, bathymetric overlay, navigation-chart overlay, edge vignette. Keep navigation surfaces, dividers, and the screen border separate from the background layers.
- Start supplied chart overlays at white tint with full Image opacity: their low contrast is already encoded in source alpha. Do not treat their transparent pixels as a black background.
- Keep all decorative Images non-raycastable. No background sprite owns gameplay data or text.
- Preserve the 16:9 aspect of the tabletop and chart layers together. Keep the screen border independently anchored and sliced; responsive layout and final readability checks belong to Phase 2.
- Screen-border slices preserve the complete corner medallions. Keep content at least 72 source pixels inside the border artwork.
- Top-navigation slices preserve end details during horizontal resizing. Keep its source height at 144 while slicing horizontally, and scale the complete presentation uniformly to the 72 px navigation target. Do not vertically stretch its central end ornaments. Keep text within the panel interior, clear of the end ornaments.
- The divider contains end and center ornaments. Preserve its aspect ratio rather than stretching it independently in height and width.
- Visual inspection found no baked gameplay text, creatures, boats, underwater scene, directional arrows, or current indicators. Contours and chart geometry leave the central encounter region clear. Final contrast with runtime text must be checked in the assembled Canvas.

## Task 1.4 Reusable Panels And Frames

Created with the built-in image generation tool at the user's explicit request on 2026-09-13. Prompt provenance is in `Docs/Task 1.4 Image Generation.md`. These are individual blank sprites, not flattened gameplay screens. Art creation is complete; final Unity scale validation remains open.

All entries use centered pivots, 100 PPU, Single mode, Full Rect mesh, bilinear filtering, Clamp wrapping, no mipmaps, no compression, and no NPOT resizing. Header and tray allow a 4096 maximum import size to retain their 2172 px width; other entries use 2048. Do not apply color tints; opacity transitions are permitted.

Borders are source pixels in left/bottom/right/top order. Safe content insets include transparent gutters and protected corner details. Use sliced Images and adjust `pixelsPerUnitMultiplier` with the shared UI scale so source-resolution borders do not become oversized at final display size. Do not scale the corner axes independently. Hollow card frames should use `fillCenter = false`.

| Filename under Frames/ | Native size | Intended prefab | Transparency | Slice L/B/R/T | Safe inset on each edge | Runtime dependencies |
| --- | --- | --- | --- | --- | --- | --- |
| `maritime-panel-9slice.png` | 1254 x 1254 | MaritimePanel | Alpha exterior; textured teal center | 160/160/160/160 | 176 px | Content and layout |
| `panel-header-strip-9slice.png` | 2172 x 724 | MaritimePanel header | Alpha exterior; textured teal center | 200/200/200/200 | 216 px | Separate heading text |
| `encounter-panel-9slice.png` | 1122 x 1402 | EncounterPanel | Alpha exterior; textured teal center | 112/112/112/112 | 128 px | Separate encounter holder/card |
| `catch-rig-panel-9slice.png` | 887 x 1774 | CatchRigPanel | Alpha exterior; textured teal center | 136/136/136/136 | 152 px | Separate rail, catches, title, meter |
| `run-controls-panel-9slice.png` | 887 x 1774 | RunControlsPanel | Alpha exterior; textured teal center | 128/128/128/128 | 144 px | Separate depth, tension and controls |
| `technique-hand-tray-9slice.png` | 2172 x 724 | TechniqueHand tray | Alpha exterior; textured teal center | 184/184/184/184 | 200 px | Separate four-card hand |
| `compact-card-frame-9slice.png` | 1536 x 1024 | CompactCatchCard | Alpha exterior and hollow center | 160/160/160/160 | 176 px | Separate card base, artwork and fields |
| `creature-card-frame-9slice.png` | 1024 x 1536 | CreatureCardView | Alpha exterior and hollow center | 160/160/160/160 | 176 px | Separate card base, artwork and fields |
| `technique-card-frame-9slice.png` | 1024 x 1536 | TechniqueCardView | Alpha exterior and hollow center | 144/144/144/144 | 160 px | Separate card base, artwork and fields |

### Validation And Assembly Notes

- All nine selected files were visually inspected and checked for native dimensions and actual alpha. Transparent corners have alpha 0; card-frame center pixels have alpha 0. Panel center sample alpha is 253/255, so their supplied near-opaque paper should be composited over the tabletop.
- Card-frame RGB previews may show colored haze where alpha is zero or near zero. Preserve the alpha and do not convert these assets to opaque RGB. An attempted compact-frame correction introduced a baked checkerboard and was rejected.
- All changing information is absent from the artwork. All corner ornament is inside the protected slice rectangles; separate corner exports are unnecessary for these designs because nine-slicing preserves those rectangles.
- Slice values are derived from actual output dimensions, not requested generation sizes. AI output dimensions differ from prompt targets and are recorded above without resampling.
- Source-level inspection confirms the shared teal/brass family and empty content areas. Verification of border weight, corner shape, texture stretching, and readability at every target Unity resolution is still required. Do not mark that Task 1.4 validation checkbox complete before rendering it.

## Task 1.5 Blank Card Systems

Generated with the built-in image generation tool at the user's request. Canonical paths use the existing card-family subfolders; the plan's shorter `Cards/` names refer to these entries. Selected output provenance and prompt specifications are in `Docs/Task 1.5 Image Generation.md`.

All sprites: centered pivot, 100 PPU, Single mode, Full Rect mesh, bilinear filtering, Clamp wrapping, alpha preserved, no compression, no mipmaps, no NPOT resizing. Title plates use a 4096 maximum import size; other assets use 2048. All dimensions below are actual output dimensions. Borders are L/B/R/T source pixels and protect complete corner shapes. Tint permission: no recoloring of artwork; opacity only for transitions and the disabled overlay. Masks use alpha only and are not visible artwork.

| Path relative to FishingUIAssets | Native size | Alpha / intended use | Nine-slice L/B/R/T |
| --- | --- | --- | --- |
| `Cards/Creature/creature-card-base.png` | 1024 x 1536 | CreatureCardView backing; fixed WEIGHT and VALUE labels, no numbers; alpha exterior | None |
| `Cards/Creature/creature-art-mask.png` | 1448 x 1086 | CreatureCardView alpha mask; solid red RGB is hidden; alpha exterior | None |
| `Cards/Creature/creature-title-plate-9slice.png` | 2172 x 724 | CreatureCardView blank title plate; alpha exterior | 240/240/240/240 |
| `Cards/Creature/creature-effect-panel-9slice.png` | 1536 x 1024 | CreatureCardView blank ivory effect panel; alpha exterior | 176/176/176/176 |
| `Cards/Creature/creature-stat-plate-9slice.png` | 1536 x 1024 | CreatureCardView reusable blank numeric plate; alpha exterior | 192/192/192/192 |
| `Cards/Creature/creature-card-back.png` | 1024 x 1536 | CreatureCardView complete compass-pattern back; alpha exterior | None |
| `Cards/CompactCatch/compact-catch-card-base.png` | 1536 x 1024 | CompactCatchCard blank backing; alpha exterior | None |
| `Cards/Technique/technique-card-base.png` | 1024 x 1536 | TechniqueCardView blank backing; alpha exterior | None |
| `Cards/Technique/technique-art-mask.png` | 1536 x 1024 | TechniqueCardView alpha mask; solid red RGB is hidden; alpha exterior | None |
| `Cards/Technique/technique-title-plate-9slice.png` | 2172 x 724 | TechniqueCardView blank title plate; alpha exterior | 208/208/208/208 |
| `Cards/Technique/technique-rules-panel-9slice.png` | 1536 x 1024 | TechniqueCardView blank ivory rules panel; alpha exterior | 160/160/160/160 |
| `Cards/Technique/technique-card-back.png` | 1024 x 1536 | TechniqueCardView complete knot-pattern back; alpha exterior | None |
| `Cards/card-disabled-overlay.png` | 1024 x 1536 | Shared low-opacity hatch overlay; no text; alpha exterior | None |
| `Markers/anchor-socket-empty.png` | 1254 x 1254 | Round rarity socket; empty teal center; alpha exterior | None |
| `Markers/anchor-filled.png` | 1254 x 1254 | Rarity anchor glyph; no backing disk; alpha exterior | None |
| `Markers/status-socket-empty.png` | 1254 x 1254 | Empty status socket; no implied gameplay effect; alpha exterior | None |

### Card Assembly Contract

- Bases, masks, text plates, artwork, and markers remain separate components. Card backs are complete images: do not add a second frame over them or nine-slice their central emblems.
- Creature base keeps only the fixed WEIGHT and VALUE labels baked into its pixels. Names, numeric values, effect text, card type, and active rarity count stay dynamic. Labels are near x=350 and x=675, y=910 on the 1024 x 1536 source (origin top-left); keep numeric plates below y=950 so they do not cover these labels.
- Reserve the region inside the Task 1.4 frame's protected corners for content. Creature source placement guidance: title above y=280; artwork within x=176..848 and y=300..860; numeric plates below the fixed labels; effect panel below the numeric plates. Final text fitting belongs to card-prefab assembly, not to the backing texture.
- For sliced text plates keep text at least slice-border + 16 source pixels from every canvas edge. Scale this inset with the plate's border scale. Straight edge profiles stretch; corner geometry does not.
- Mask RGB is red solely to obtain a reliable filled alpha silhouette. Configure Unity `Mask.showMaskGraphic = false`; do not display the red Image or use RGB as mask coverage. Sampled mask interior alpha is 253..254/255 with no sampled holes. Validate stencil clipping at the edge during prefab assembly; no in-Unity stencil test has been run.
- Preserve the supplied mask aspect ratios; do not stretch corner axes independently. Transparent padding is part of the mask and must be included when aligning artwork.
- Compact base includes a thin brass hairline. Align its outer silhouette beneath the separate compact frame so the two do not appear as offset double borders.
- The disabled overlay has sampled alpha 0..48/255. Start at full Image opacity, clip to the owning card silhouette, and keep text above it. Its final legibility and hatch visibility require an assembled-card check. Do not add forbidden runtime-state labels to creature faces.
- Rarity uses four round sockets, with anchor glyphs activated according to the existing rarity contract; these images do not introduce an anchor-count mechanic. Maintain glyph and socket aspect ratios. Status sockets do not authorize additional creature-face state indicators.

### Validation

All sixteen selected PNGs were visually inspected. Dimensions, transparent corner samples, mask interior samples, alpha ranges, and slice bounds were checked. No changing names, numbers, rules text or creature/technique illustrations are baked in. The first white mask was rejected for holes and edge artifacts; red alpha silhouettes replaced it. Art files are copied from selected generated outputs without procedural repainting. Unity rendering, text fitting, marker legibility at final size, and stencil-edge validation remain part of integration; these checks are not claimed complete.

## Task 1.6 Rig, Meters, And Controls

All 23 generated PNGs are present. Exact prompts, selected source paths and import rectangles are recorded in `Docs/Task 1.6 Image Generation.json`. Artwork is copied unchanged from the built-in generator.

Import settings: Multiple mode with one named sprite per PNG, Full Rect mesh, centered pivot, 100 PPU, bilinear, Clamp, uncompressed, no mipmaps, no NPOT resizing, maximum size 4096. Drag the named sub-sprite into Image fields. Import rectangles remove excess padding using the alpha >= 128 artwork bounds plus eight source pixels; this is metadata only, not pixel editing. Rectangles below use bottom-left x/y/width/height. Borders are L/B/R/T relative to the imported sprite, not the original PNG.

All assets require transparent exteriors. Focus/hover/selected frame centers are transparent. Tint permission: preserve authored colors; opacity-only transitions. The ivory tension fill includes a brass rim, so it is not a color-neutral tint mask.

| Path relative to FishingUIAssets | Native pixels | Sprite rect x/y/w/h | Borders L/B/R/T | Intended prefab role |
| --- | --- | --- | --- | --- |
| `Rig/main-line-segment.png` | 724 x 2172 | 319/0/86/2172 | 0/0/0/0 | CatchRig |
| `Rig/branch-line-segment.png` | 2172 x 724 | 0/305/2172/114 | 0/0/0/0 | CatchRig |
| `Rig/attachment-clasp.png` | 1254 x 1254 | 193/100/879/1072 | 0/0/0/0 | CatchRig |
| `Rig/catch-slot-empty.png` | 1536 x 1024 | 83/137/1370/779 | 0/0/0/0 | CatchRig |
| `Rig/hook-terminal.png` | 1254 x 1254 | 312/36/633/1190 | 0/0/0/0 | CatchRig |
| `Meters/line-load-track-9slice.png` | 2172 x 724 | 10/149/2153/433 | 96/96/96/96 | LineLoadMeter |
| `Meters/line-load-fill-safe-9slice.png` | 2172 x 724 | 13/185/2143/354 | 96/96/96/96 | LineLoadMeter |
| `Meters/line-load-fill-warning-9slice.png` | 2172 x 724 | 46/253/2080/217 | 72/72/72/72 | LineLoadMeter |
| `Meters/line-load-fill-critical-9slice.png` | 2172 x 724 | 15/198/2142/328 | 96/96/96/96 | LineLoadMeter |
| `Meters/depth-track-9slice.png` | 724 x 2172 | 219/4/286/2164 | 48/160/48/160 | DepthTracker |
| `Meters/depth-marker.png` | 1254 x 1254 | 181/32/891/1192 | 0/0/0/0 | DepthTracker |
| `Meters/tension-track-9slice.png` | 2172 x 724 | 17/171/2137/380 | 96/96/96/96 | TensionMeter |
| `Meters/tension-fill-9slice.png` | 2172 x 724 | 14/219/2145/287 | 96/96/96/96 | TensionMeter |
| `Controls/action-button-normal-9slice.png` | 2172 x 724 | 32/81/2110/566 | 180/180/180/180 | Action/confirmation button |
| `Controls/action-button-hover-9slice.png` | 2172 x 724 | 35/117/2101/496 | 180/180/180/180 | Action/confirmation button |
| `Controls/action-button-pressed-9slice.png` | 2172 x 724 | 24/105/2124/537 | 180/180/180/180 | Action/confirmation button |
| `Controls/action-button-disabled-9slice.png` | 2172 x 724 | 46/118/2081/496 | 180/180/180/180 | Action/confirmation button |
| `Controls/action-button-destructive-9slice.png` | 2172 x 724 | 43/117/2086/501 | 180/180/180/180 | Action/confirmation button |
| `Controls/confirm-button-9slice.png` | 2172 x 724 | 38/106/2096/522 | 180/180/180/180 | Action/confirmation button |
| `Controls/cancel-button-9slice.png` | 2172 x 724 | 34/75/2104/578 | 180/180/180/180 | Action/confirmation button |
| `Controls/controller-focus-frame-9slice.png` | 1254 x 1254 | 18/22/1218/1207 | 128/128/128/128 | Selectable focus overlay |
| `Controls/card-hover-frame-9slice.png` | 1024 x 1536 | 48/48/928/1439 | 128/128/128/128 | Card interaction overlay |
| `Controls/card-selected-frame-9slice.png` | 1024 x 1536 | 42/47/941/1443 | 128/128/128/128 | Card interaction overlay |

### Binding And Layout Contract

- Rig line/branch pieces, clasp, hook and empty catch slot remain separate Images. Preserve clasp and hook aspect ratios; extend rails along their long axis. Catch order, attachment visibility and slot occupancy come from runtime data.
- Load fill amount, capacity, risk state, depth position and tension remain runtime-owned. There are no baked numbers, zone boundaries or gameplay thresholds.
- Use a stable track container and fixed inset fill rectangle for all three load states. Plain turquoise, diagonal gold hatching and coral crosshatching provide distinct source treatments. Clip a full-sized sliced fill to represent progress, rather than shrinking corners toward zero width. Keep the authored fill rims inside the track.
- Button labels, availability and commands remain runtime fields. Keep the owning RectTransform and text layout fixed during sprite swaps; do not call SetNativeSize. The source silhouettes and corner details differ slightly between generated states, so check their alignment at the final display size.
- Hover adds an ivory keyline; pressed uses an inset rim; disabled has diagonal hatching; selected uses a double frame and diamond corner fittings. Destructive actions also require an explicit runtime command label; coral alone must not carry meaning.
- Use the independent controller-focus outline for keyboard and controller navigation, including when hover or selection is also active. Overlays must have raycastTarget disabled and leave the underlying button as the input target. The sprites contain no device-specific glyphs.
- Keep interaction frames outside creature-card information regions. No new status text, badges or gameplay mechanics are authorized by these assets.
- Nine-slice borders protect source corners, but their on-screen scale must be set during prefab assembly. Keep labels within the sliced center with additional text padding.

### Verification Status

All 23 sources passed full-image alpha-range and visible-art bounds checks. Frame center pixels are transparent. Metadata and slice bounds are checked by the validation script. Final-size slicing, pattern readability, state-swap alignment, simultaneous selection/focus and mouse/keyboard/controller interaction have not been tested in Unity. Task 1.6's final two visual acceptance checks remain open until that validation; asset presence is not full task acceptance.

## Phase 1 Asset Entries To Complete Later

The following groups require per-file native dimensions, pivot, pixels per unit, transparency, nine-slice borders, tint permission, and runtime dependencies when the assets are supplied:

- `Backgrounds/`
- `Frames/`
- `Cards/Creature/`
- `Cards/CompactCatch/`
- `Cards/Technique/`
- `Creatures/`
- `Techniques/`
- `CatchRig/`
- `Rig/`
- `Controls/`
- `Meters/`
- `Markers/`
- `Icons/`
- `Overlays/`
- `Effects/`
- `Fonts/`

## Layout Contract From Task 1.1

| Region | Reference target |
| --- | --- |
| Canvas | 1920 x 1080 primary reference |
| Top navigation | 72 px height |
| Outer safe margin | 48-64 px |
| Catch Rig column | 410 px target width |
| Center available area | 930 px target width |
| Run controls column | 420 px target width |
| Technique hand | Bottom-center safe area |
| Encounter | Primary center focal area |

## Typography Contract From Task 1.1

| Runtime role | Reference size |
| --- | --- |
| Biome / location | 48 px |
| Primary card title | 64 px |
| Panel heading | 38 px |
| Primary action | 46 px |
| Small plate | 29 px |
| Stat value | 58 px |
| Secondary value | 52 px |
| Rules headline | 30 px |
| Rules text | 26 px |
| Status label | 27 px |
| Small data | 22 px |

## State Contract From Task 1.1

| State | Visual requirement |
| --- | --- |
| Normal | Legible brass-framed base surface |
| Hover | Slight lift plus outline emphasis |
| Selected | Stronger turquoise/brass selected treatment |
| Pressed | Compressed active treatment |
| Disabled | Desaturated and hatched, still legible |
| Warning | Aged gold emphasis plus non-color support |
| Critical | Coral emphasis plus non-color support |
| Focus | Distinct controller-focus outline |
