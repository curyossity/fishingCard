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
