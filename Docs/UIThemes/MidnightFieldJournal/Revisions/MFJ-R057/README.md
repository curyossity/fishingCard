# MFJ-R057 — Sharp 4K Harbor UI frames

**Imported and verified at MFJ-SYNC-010.** [Unity integration](../../Sync010/README.md). Earlier review-only status below is historical.

## Cause

R053, R055 and R056 placed generated 1672 × 941 full-screen review compositions on 3840 × 2160 boards. The scene framing was enlarged about 2.3×, so engraving became soft. A 4K JPEG export increased the file dimensions without restoring native frame detail.

## Replacement construction

Two new standalone, transparent frame masters are authored independently of the screen mockups: [Dark frame](Art/DarkFrameMaster.png) and [Parchment frame](Art/PaperFrameMaster.png). They use asymmetric nine-slice source borders: top 300, right 500, bottom 560, left 500 pixels. This contains the tall lower-corner engraving without stretching it along an edge. Decorative corners display at no more than 220 / 500 = **0.44× native size** in the 4K layout. Quiet edge lengths and calm fills resize independently. These are not slices cut from a full-screen mockup.

`RenderFrames.py`, the three `*Frames.html` files and `*FrameGeometry.json` files are the authoritative reusable assembly sources. Local Chromium renders the frame canvas directly at **3840 × 2160**, saving lossless transparent PNGs. The generated masters are not edited or enlarged as a whole. The images displayed in Penpot are these native-size 4K frame renders, rather than hundreds of native clipped duplicates that made the large Penpot page's exporter unreliable. Individual frame masters and component geometry remain separate for Unity reconstruction. Native text, existing exact-game Technique card faces and independent equipment artwork retain their original Penpot IDs. Original review art remains underneath for the scenic illustration only; this revision does not certify that scenery as native 4K artwork.

A vector-only experiment changed the illustrative character too much and was rejected/restored. `Vectors/`, `CreateVectors.py`, `ApplyFrames.py`, `NineSlice.js` and `ApplySharpFrames.py` are experimental, not import sources. Use the final `RenderFrames.py` geometry and `PlaceRendered.py` placement instead. [Master generation prompts](GenerationPrompts.md).

## Review and integration

Target screens are the current Harbor R053, Tackle Shop R055 and Technique Collection R056. Their original board IDs and navigation destinations remain stable. Old raster-frame comparison boards are retained. Integration status must be taken from `Verification.json` and the canonical ledger, not inferred from the presence of generated images.

- [Harbor — lossless 4K export](Harbor4K.png)
- [Tackle Shop — lossless 4K export](Shop4K.png)
- [Technique Collection — lossless 4K export](Collection4K.png)
- [Verification](Verification.json)

This is Penpot-only work. No Unity code, art, prefabs, scenes or gameplay state is modified. The new frame masters use the existing local Penpot and built-in image generation workflow; no paid plugin or stock service is introduced.
