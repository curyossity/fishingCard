# Phase 3 refinement

This is the current visual revision of the two concepts on Penpot pages 11 and 12. It supersedes the first composition in the parent directory. The previous Penpot groups are preserved but hidden.

- Page 11: [Level 1](https://design.penpot.app/?screen=workspace&file-id=3681d377-bc58-8110-8008-e7a38adc2112&page-id=389319c8-6b5a-80a0-8008-f36e5768d320)
- Page 12: [Level 3](https://design.penpot.app/?screen=workspace&file-id=3681d377-bc58-8110-8008-e7a38adc2112&page-id=389319c8-6b5a-80a0-8008-f371b5b35d7e)

## Visual decisions

The insertion tray reuses the original composition from page 04, including the warmer wooden boards, brass perimeter, sagging rope, larger hanging cards, and insertion sockets. Dynamic species, levels, and sale values replace the earlier Catch/Bait fields. The upgrade view keeps the same tray and highlights the upgraded slot, without insertion controls.

Creature details use one uninterrupted brass frame. The top line shows level and sale value. Below it is the full brief effect: "Successful strikes gain +0.2x per other contiguous Schooling creature." The Level 3 screen substitutes +0.6x. There are no nested panels, New Species/Duplicate labels, copy-count explanations, or tutorial paragraphs.

The Level 1 example uses Mackerel (10), Glass Minnow (5), and Sea Bass (16) already in the rig, with Sardine (7) pending. This preserves the original three-card/four-gap arrangement without showing Sardine already owned. Haul preview: 31 → 38. The Level 3 example retains Sardine (21), Mackerel (10), and Glass Minnow (5); haul change: 29 → 36. These are UI examples, not changes to creature balance.

## References reviewed

- [Balatro official press-kit screenshots](https://www.playbalatro.com/press-kit): compact labels, prominent numbers, and clear card hierarchy.
- [Slay the Spire upgrade screenshot](https://note.com/_pechi/n/n1172b7eac9b0): a brief full effect on the card and an upgraded result presented using its resulting numbers.

These informed the information hierarchy. The game's existing pixel art, palette, fonts, and physical tackle-machine presentation remain the visual source.

Run `../compose_v2.py`, then render this directory's `Composition.json` with the parent directory's `ConceptRenderer.cs`. PNGs are review exports; SVGs contain independent raster artwork and outlined vector labels, with text strings recorded separately in `-Text.json`. No Unity files are changed by this refinement.
