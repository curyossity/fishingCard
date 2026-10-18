# MFJ-R002 generation provenance

Tool: built-in `image_gen.imagegen`, transparent-background mode. No CLI/API fallback. The full gameplay mockup is a composition/style reference only; no frame or creature was cropped from it. Independent generated outputs were copied into this revision's Art folder.

## Selected frame — final edit prompt

Input: independently generated blank frame `exec-9f415fd8-4862-4e62-8549-9aad5b17ddd7.png`, itself freshly generated from the original mockup's central-card style.

Output: `Art/MainCreatureFrameV3.png`, original generation `exec-9da7a6d3-4185-414b-b806-9a3ba7a5981a.png`.

> Edit this blank fantasy marine game card frame, preserving its exact detailed antique engraved paper/copper/navy art style, all corner etching, kelp illustration, same layout proportions, dividers and empty fields. TWO REQUIRED CORRECTIONS ONLY: (1) Render this frame freshly at NATIVE 2160 x 2400 pixels (or the highest available native image resolution above 1800 pixels wide). It is a 4K game UI source; do not return a low-resolution preview or simply upscale the previous pixels; reproduce fine engraving at this larger native size. Outer card width:height remains 9:10. (2) Neutralize the LEFT bottom panel's bright orange/gold outline: give BOTH effect panels the SAME thin dim aged copper/silver line with NO glow and NO bright active highlight. Keep the left panel's warmer brown paper tone and the right panel's cooler navy tone. Panel selection will be a separate editable runtime overlay. Remove every stray mark or fringe outside the card: genuinely transparent background, clean smooth alpha, NO opaque rectangular background, NO colored artifacts above or around outer border, all card edges intact within a narrow uniform 2% transparent safety margin. Do not add any text, numbers, icons, creatures, badges or symbols. Keep this as one independent complete front-facing 2D reusable blank frame, with the spacious clear parchment header, EMPTY creature art center transitioning from light paper into dark marine kelp, empty stat band with two vertical separators, two blank lower effect panels. Frame should preserve all its rich fine texture and engravings, not become simplified.

Actual output: **1190×1322**, alpha present. The requested larger raster size was not returned. Record actual source dimensions, never infer them from this prompt.

## Selected separate illustration — final prompt

Inputs: previously generated separate engraved octopus `exec-2fc0a879-3002-4a5c-a9e0-5e8b60473f8b.png` and the original gameplay mockup `exec-7a5f1ac6-937e-422f-bd62-93682eea510b.png` as a pose/proportion reference.

Output: `Art/SmallOctopusEngravedV3.png`, original generation `exec-9374da1e-aaef-4bd9-901c-0e9b33bec5be.png`.

> Create a WIDER and LESS TALL version of the separate engraved octopus sprite in image 1, to fit the exact wide illustration window of the central card in image 2. Image 1 is the octopus to recompose; image 2 is pose/proportion reference only, do not crop any screenshot. Output a horizontal 16:9 canvas, native 2048x1152 pixels if supported. Preserve the rich brown/copper/slate/ivory natural-history engraving, fine dark cross-hatching, big eye, curled tentacles and rows of suction cups. Recompose eight arms spreading LEFT and RIGHT across the entire wide canvas with lower curls shallower, head in the upper-middle/right, so the COMPLETE OCTOPUS has a silhouette bounding ratio approximately 1.8:1 and fits the card art area without squashing or clipping. The head must be smaller relative to the tentacles than image 1, as in image 2. No tall upward arm reaching beyond the head. Every arm tip remains fully inside a narrow 3% transparent margin. Make a genuinely transparent isolated sprite: zero background alpha outside and between the octopus arms, no brown/grey radial glow, no ground, no vignette, no card, no paper, no coral, no cast shadow. Crisp detailed opaque subject with natural anti-aliased edge only. No text/numbers/symbols. Match the CENTRAL mockup card's illustrated full-width octopus presence, not a centered small photorealistic cutout.

Actual output: **1672×941**, alpha present. It is a separate illustration, not a flattened card.
