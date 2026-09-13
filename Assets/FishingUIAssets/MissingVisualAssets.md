# Missing Visual Assets

## Task 1.5

No Task 1.5 image files are missing. Sixteen generated assets and their Unity import metadata are present under the card-family folders, `Cards/`, and `Markers/`.

The first white artwork mask was rejected; the selected masks have solid red RGB and filled alpha interiors. Their color must remain hidden (`Mask.showMaskGraphic = false`). Runtime card assembly must verify stencil edges, text fit, disabled-overlay legibility, and marker size. These are outstanding integration checks, not missing images. Source metadata and constraints are recorded in `Reference/asset-manifest.md`.

## Task 1.4

All nine required frame and panel images have been generated at the user's request and saved in `Frames/`, with sprite import metadata. No Task 1.4 image is missing. Corner details are retained within nine-slice corner regions and need no independent exports for these designs.

Remaining validation: inspect sliced rendering and material consistency at the target Unity display sizes. This is a rendering check, not a missing-art requirement. See `Reference/asset-manifest.md` for native dimensions, slices and safe regions, and `Docs/Task 1.4 Image Generation.md` for prompts.

This file records required visual assets that are not yet supplied. Use obvious temporary placeholders only when implementation cannot continue without a visible object, and replace them once the approved asset is available.

## Task 1.1

No missing font files remain for Task 1.1.

Supplied fonts:

- `Assets/FishingUIAssets/Fonts/Marcellus/Marcellus-Regular.ttf`
- `Assets/FishingUIAssets/Fonts/Marcellus/OFL.txt`
- `Assets/FishingUIAssets/Fonts/SourceSerif4/SourceSerif4-Variable.ttf`
- `Assets/FishingUIAssets/Fonts/SourceSerif4/OFL.txt`

Remaining integration work:

- Generate TextMeshPro font assets inside Unity.
- Validate whether the Unity/TextMeshPro version in this project handles Source Serif 4 variable font weights as needed. If not, fetch static Source Serif 4 weight files under the same OFL license and record them in `Reference/asset-manifest.md`.

## Task 1.2

No missing base material assets remain for Task 1.2.

## Task 1.3

No missing background package images remain for Task 1.3. All seven images are supplied and have Unity sprite import metadata.

The drop did not include creator-authored nine-slice values. Implementation boundaries derived from the artwork and their resizing constraints are recorded in `Reference/asset-manifest.md`. Verify them during Task 2.2 Canvas assembly, together with final overlay contrast and text readability.
