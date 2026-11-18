# Curved reel aperture validation

18 November 2026. Each full-size reel now has one curved stencil opening traced from the inner paper edge of the original 412 × 490 drum image. The original rim renders as a fixed background, outside the mask. Moving paper, plaques, dividers, symbols and native captions all render inside the same aperture. No artwork file, reel size, position or timing was changed.

`ReelApertureMask` generates only invisible clipping geometry. `TackleReelApertureSetup` authors the mask, fixed rim and native-face container, and removes those additions when switching back to the preserved compact version. `TackleReelSurface` synchronizes the stencil-derived rendering material with its private animated material; masking cannot freeze the paper while symbols move.

Validation:

- **9 aperture asset checks:** hidden masks, shared content ownership and fixed-rim ordering.
- **28 live rendering checks:** a bright test layer is blocked at all four narrow corners of each wheel, stays visible across the wider middle, and leaves the rim pixels unchanged. Native faces share the mask and the rendered paper material uses the expected stencil and phase.
- **38 live animation checks:** initial spin, synchronized rendered material, one stationary locked wheel, single respin, fixed review and explicit acceptance.
- **19 housing, 155 existing art and 12 reel-surface checks** passed. Compact → full-size switching passed.
- **Four input raycasts** and captures at 1920 × 1080, 1280 × 800 and 2560 × 1080.
- Unity metadata validator: **1,742 files, zero errors/warnings**.

[Current preview](Discover-1920x1080.png) · [1280 × 800](Discover-1280x800.png) · [Ultrawide](Discover-2560x1080.png) · [Stencil verification](StencilVerification.json)

`StencilBaseline.png` and `StencilProbe.png` are diagnostic fixtures; the bright probe exists only during the synchronous check and is destroyed afterward. The normal `Discover-*` captures contain the actual game UI. Earlier captures in the parent folder record the previous rectangular clipping.
