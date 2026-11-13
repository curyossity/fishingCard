# Rotating reel surface

Created 9 November 2026 for the approved Tackle Apparatus Discover composition. The original `reel-drum.png` is preserved unchanged and remains the source for the fixed outer rim and the rotating decorative details.

`reel-paper-flat.png` is a new opaque 1254 × 1254 paper material, generated with the **built-in image_gen tool**, using the drum's cream paper as a color/style reference. It contains no borders, divider lines, plaques, symbols or text. No direct API/CLI generation or upscaling was used. `Generation.json` contains the complete prompt and tool output path; `Manifest.json` records dimensions and SHA-256.

Unity imports a byte-identical copy under `Assets/UIThemes/TackleApparatus/Art/ReelSurface/` with Point filtering, no mipmaps, read/write or compression. The shader mirrors the paper into a continuous periodic strip so texture-edge differences cannot introduce a seam.

Authored UV regions in `TackleReelSurfaceSetup` sample the preserved 412 × 490 drum: the blank plaque uses texels X49–365 / bottom Y85–164; a divider crop uses X34–378 / bottom Y390–400. These are material sampling regions, not raster edits. Paper and decorations use the same angle/radius as the native icons and TMP captions. Rim sampling and directional lighting use fixed screen/cylinder coordinates. Each wheel owns a runtime material instance, so locking one leaves its whole surface stationary.

**Fishing Cards > Slot Machine > Install Rotating Reel Surface** updates only the existing prefab's reel background layers. Full Discover and cylindrical-animation regeneration call the same setup. Functional text remains native; source icons and all gameplay rules are preserved.

[Validation and in-motion captures](../../../Validation/RotatingReelSurface/README.md)
