# Missing Visual Assets

## Task 1.7

All nine overlay and eighteen icon images are present with Unity sprite metadata. No Task 1.7 file is missing. Exact prompts and selected sources are recorded in `Docs/Task 1.7 Image Generation.json`.

Task 1.7 asset acceptance passed in Unity: 32/48/64-pixel icon comparisons, three slice sizes and both compositing backgrounds were reviewed. See `Docs/Validation/Task1.7/README.md`. Icons now use mipmaps and trilinear filtering. The dim source still requires runtime alpha 0.6; use the manifest's other opacity settings during assembly.

## Task 1.6

No image files are missing: all 23 Rig, Meters and Controls PNGs have sprite import metadata. Source transparency and artwork bounds passed validation. See the manifest for imported sub-sprite rectangles, slice borders and runtime dependencies.

Task 1.8 verified final-size state readability, sliced rendering, stable state alignment and independent selected/focus combinations in Unity. Runtime input routing remains Phase 2 implementation work.

## Task 1.5

No Task 1.5 image files are missing. Sixteen generated assets and their Unity import metadata are present under the card-family folders, `Cards/`, and `Markers/`.

The first white artwork mask was rejected; the selected masks have solid red RGB and filled alpha interiors. Their color must remain hidden (`Mask.showMaskGraphic = false`). Runtime card assembly must verify stencil edges, text fit, disabled-overlay legibility, and marker size. These are outstanding integration checks, not missing images. Source metadata and constraints are recorded in `Reference/asset-manifest.md`.

## Task 1.4

All nine required frame and panel images have been generated at the user's request and saved in `Frames/`, with sprite import metadata. No Task 1.4 image is missing. Corner details are retained within nine-slice corner regions and need no independent exports for these designs.

Task 1.8 verified sliced rendering and material consistency at the target Unity display sizes. See `Reference/asset-manifest.md` for native dimensions, slices and safe regions, and `Docs/Task 1.4 Image Generation.md` for prompts.

This file records required visual assets that are not yet supplied. Use obvious temporary placeholders only when implementation cannot continue without a visible object, and replace them once the approved asset is available.

## Task 1.8

The contact sheet, corrected blank assembly and updated state reference are present. All Task 1.8 checks and Phase 1 static package acceptance passed. See `Docs/Validation/Task1.8/README.md`.

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

All six base-material files are present. The four repeating material sources were repaired and pass the Unity 2x2 seam review.

## Task 1.3

No missing background package images remain for Task 1.3. All seven images are supplied and have Unity sprite import metadata.

The drop did not include creator-authored nine-slice values. Implementation boundaries derived from the artwork and their resizing constraints are recorded in `Reference/asset-manifest.md`. Verify them during Task 2.2 Canvas assembly, together with final overlay contrast and text readability.
