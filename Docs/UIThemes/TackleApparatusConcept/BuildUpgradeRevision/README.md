# Phase 3 concept revision — new species and upgrades

**Current visual revision:** see [Refinement/README.md](Refinement/README.md). The first composition below is retained as source history; the simplified versions are displayed on the same two Penpot pages.

Concept-only revision requested on 15 November 2026. These screens reuse the approved Tackle Apparatus artwork and creature sprites at the original 1920 × 1080 resolution. They do not modify Unity scenes, assets, gameplay rules, or balancing data.

## Penpot pages

- [11 - Build - New Species Lv1](https://design.penpot.app/?screen=workspace&team-id=9e056cfe-f83f-81f8-8008-cc4e858053b7&file-id=3681d377-bc58-8110-8008-e7a38adc2112&page-id=389319c8-6b5a-80a0-8008-f36e5768d320)
- [12 - Build - Upgrade Lv3](https://design.penpot.app/?screen=workspace&team-id=9e056cfe-f83f-81f8-8008-cc4e858053b7&file-id=3681d377-bc58-8110-8008-e7a38adc2112&page-id=389319c8-6b5a-80a0-8008-f371b5b35d7e)

The earlier Rig Placement page remains available as an earlier concept.

## Display contract illustrated

| Field | New Sardine | Duplicate Sardine |
| --- | --- | --- |
| Acquisition | New species | Duplicate / +1 level |
| Level | Level 1 | Level 2 → 3 |
| Resulting accumulated species sale value | 7 | 21 |
| Value explanation | 1 copy × 7 | 14 + 7 = 21 |
| Resulting ability | +0.2× per other connected Schooling species | +0.6× per other connected Schooling species |
| Rig interaction | Choose an insertion gap; confirm placement | Upgrade original slot automatically; preserve order |
| Haul | 15 before placement; preview 22 | 29 → 36 after upgrade |

The Level 3 coefficient follows the user's latest explicit concept example. The display always describes the resulting creature level, rather than repeating a Level 1 ability. Values and coefficients here illustrate the UI; this task does not apply balancing changes to creature definitions. An upgraded species occupies one rig position and counts once for positional conditions.

The Catch/Bait controls are replaced by informational fields. The upgrade screen is a brief automatic feedback state, with no insertion or confirmation action. The new-species screen retains insertion choices and a placement confirmation. Slot-machine reels and the Landing Challenge are separate phases and are not shown here.

## Sources and reproduction

- `compose.py` writes `Composition.json`, a layer command list built from the existing `AssetLibrary/Manifest.json` and approved creature sprites under `Assets/UIThemes/TackleApparatus/Art/Creatures`.
- `ConceptRenderer.cs` renders PNG review images and layered SVG import sources using System.Drawing and the licensed fonts already in `AssetLibrary/Fonts`.
- Each SVG contains separate artwork and vector label groups. Labels are outlined paths, not native editable Penpot text. Original strings, font names, and measured bounds are retained in the matching `-Text.json` files.
- Raster image segments are embedded directly in SVG rather than referenced through SVG `use`/clip-path transforms, preserving their placement through Penpot's importer.
- No image-generation API, new external artwork service, or Unity integration was used for this revision.
