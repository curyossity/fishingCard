# Deepwater UI Style Guide

Task 1.1 locks the initial visual specification for the fishing roguelike gameplay UI. The supplied reference sheets in this folder are the approved source for palette, typography roles, component proportions, interaction states, and 1920 x 1080 composition.

## Source References

- `theme-palette.png`: approved palette and material direction.
- `typography-reference.png`: runtime text hierarchy at the 1920 x 1080 reference resolution.
- `component-dimensions.png`: target screen regions, safe margins, and panel proportions.
- `visual-states.png`: shared interaction-state language.
- `gameplay-layout-1920x1080.png`: populated composition reference. This is not a gameplay background.

## Visual Identity

- Cards are the primary visual language and should feel like tactile engraved maritime game pieces.
- The UI uses oxidized dark teal, aged brass, warm ivory, turquoise, and coral.
- Materials should read as textured paper, oxidized metal, and worn brass rather than glossy digital panels.
- The current encounter remains the main focal point.
- The Catch Rig must read as a physical temporary build attached to a line, not as a generic inventory list.
- Decorative elements must stay visually subordinate to gameplay information.
- Do not add literal underwater scenery, characters, boats, combat VFX, glossy mobile-game styling, or conventional RPG HUD ornamentation.
- Do not reproduce a stacked illustrated-scene composition. The approved composition is a tabletop/card interface with independent card objects.

## Palette And Materials

Use `theme-palette.png` as the visual source of truth.

Core color roles:

- Deep teal: primary tabletop/background field and dark UI bases.
- Oxidized teal: panels, card interiors, and selected surfaces.
- Aged brass: borders, dividers, rings, sockets, and ornamental linework.
- Warm ivory: paper panels, readable text regions, neutral buttons, and labels.
- Turquoise: safe/positive/action emphasis and low-risk meter fill.
- Coral: release/destructive/critical pressure emphasis.

Material rules:

- Keep teal and ivory surfaces subtly textured.
- Keep brass borders worn and engraved, not flat yellow.
- Use turquoise and coral as state colors, not as dominant background themes.
- Avoid using color alone for critical gameplay states; pair warning/critical colors with icons, outlines, hatching, or text.

## Typography

Use `typography-reference.png` for role hierarchy and approximate sizes at 1920 x 1080.

Runtime text must remain TextMeshPro/UI text and must not be baked into static artwork.

Display roles:

| Role | Example | Reference size |
| --- | --- | --- |
| Biome / location | `COBALT SHELF` | 48 px |
| Primary card title | `MACKEREL` | 64 px |
| Panel heading | `CATCH RIG` | 38 px |
| Primary action | `DESCEND` | 46 px |
| Small plate | `CURRENT ENCOUNTER` | 29 px |

Body, value, and status roles:

| Role | Example | Reference size |
| --- | --- | --- |
| Stat value | `1.8 kg` | 58 px |
| Secondary value | `7` | 52 px |
| Rules headline | `+4 LINE LOAD` | 30 px |
| Rules text | `Increase your line capacity by 4.` | 26 px |
| Status label | `TENSION: LOW` | 27 px |
| Small data | `0-200 m - DECK 12` | 22 px |

Font intent:

- Display: condensed serif with engraved nautical weight.
- Body: clear sans serif for rules, values, and status text.
- Approved font files have not yet been supplied. See `../MissingVisualAssets.md`.

Text rules:

- Keep letter spacing at 0 in Unity unless a supplied typography spec gives an explicit value.
- Use bounded regions and responsive best-fit only where needed to prevent clipping.
- Keep long rules text readable before preserving decorative density.
- Do not bake runtime names, values, descriptions, load totals, depth values, deck counts, or availability states into art.

## Layout

Use `component-dimensions.png` as the primary layout reference for the 1920 x 1080 canvas.

Reference regions:

- Top navigation height: 72 px, approximately 7 percent of screen height.
- Outer safe margin: 48-64 px.
- Catch Rig target width: 410 px, approximately 22 percent of screen width.
- Run controls/depth target width: 420 px, approximately 22 percent of screen width.
- Center available region: approximately 930 px.
- Encounter panel: primary focal area in the center column.
- Technique hand: bottom-center safe area below the encounter.

Layout rules:

- Anchor top navigation to the top edge.
- Anchor Catch Rig to the left.
- Anchor run controls to the right.
- Anchor the technique hand to the bottom-center safe area.
- Keep the encounter centered in the remaining space.
- Preserve the visible hierarchy: encounter first, Catch Rig second, technique hand third, controls always findable.
- Do not use the populated screenshot as a flattened background.

## Component Shape Rules

- Main panels use brass borders over dark teal or ivory interiors.
- Stretchable assets require approved nine-slice borders recorded in `asset-manifest.md`.
- Corners and ornamental caps should not stretch unless the source asset was designed for nine-slicing.
- Runtime fields must sit inside explicit safe regions.
- Card artwork and text are separate layers.
- Creature card text fields remain dynamic: name, weight, value, effect text, and marker count.

## Interaction States

Use `visual-states.png` as the shared state reference.

States:

- Normal: ivory or teal body with brass frame.
- Hover: subtle lift plus teal/brass outline emphasis.
- Selected: turquoise fill or stronger turquoise/brass outline.
- Pressed: compressed/darker active surface.
- Disabled: desaturated surface plus hatch treatment.
- Warning: aged gold emphasis.
- Critical: coral emphasis.
- Focus: distinct outline, suitable for controller navigation.

State rules:

- Runtime chooses visible state layers.
- Text remains separate from state artwork.
- Hover may lift slightly.
- Pressed may compress slightly.
- Disabled must remain legible.
- Focus must be distinct from hover and selected.
- Warning and critical states must not rely on color alone.

## Asset Import Defaults

Use these defaults unless a later manifest entry overrides them:

- Texture type: Sprite (2D and UI).
- Sprite mode: Single for individual PNGs.
- Mesh type: Full Rect for UI sprites.
- Pixels per unit: 100 for UI sprites unless a prefab-specific value is supplied.
- Pivot: center for panels, cards, icons, and overlays unless a rig/connector asset requires an explicit attachment pivot.
- Compression: None or high quality for UI source sprites during visual implementation.
- Alpha is required for icons, overlays, masks, card artwork, and non-rectangular frames.
- Reference sheets are non-runtime documentation assets and should not be used as UI sprites.

## Task 1.1 Lock Status

Locked by this task:

- Initial palette and material direction.
- Typography roles and reference sizes.
- 1920 x 1080 composition, safe margins, and major panel proportions.
- Shared state language for buttons, cards, meters, and focus.
- Static-art/runtime-data boundary.

Deferred to later Phase 1 tasks:

- Exact native dimensions for every individual sprite not yet supplied.
- Final nine-slice border values for stretchable sprites.
- Final pivot values for rig connectors, slots, markers, and meters.
- Final font file names after display/body font files are supplied.
