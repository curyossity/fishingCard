# Deepwater — Gameplay Loop Mockups

Revision **DW-LOOP-002**, 6 November 2026. Status: **ready for review**. This revision applies the established card decoration to both phase mockups. These are design mockups, not finalized mechanics or a Unity implementation. The [GDD](../../Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md) remains authoritative.

## Penpot location

File: **New File 1** (`9e056cfe-f83f-81f8-8008-cc523ff45211`).

New page: **03 - Deepwater - Gameplay Loop** (`38e9e9ec-a6d5-8050-8008-e796f637ba87`). Existing pages, approved artwork and assemblies are preserved.

| Screen | Board ID | Preview |
| --- | --- | --- |
| Discover — Slot machine | `38e9e9ec-a6d5-8050-8008-e7971f4bfa02` | [SlotMachine.png](SlotMachine.png) |
| Land — Landing challenge | `38e9e9ec-a6d5-8050-8008-e7972b912b7f` | [LandingChallenge.png](LandingChallenge.png) |

Both screens are editable 1920 × 1080 boards with native text, controls, instrument geometry, independent illustrations and reusable frame layers. **Follow These Tags** links Discover to Land as a demonstration. Other controls depict proposed interaction affordances; this is not a playable minigame.

## Design scope

The screens show a mid-expedition rig shared between phases: Sardine Catch, Mackerel Bait and Glass Minnow Catch. Example sale values are 7, 0 and 5, giving a haul of 12. The sidebar states that this value pays in full on success or failure. This is not the expedition-start state; every expedition begins with an empty rig.

Discover presents three tag reels, locked/unlocked states and optional respin. Land presents a Sardine encounter, moving-fish/moving-zone alignment, Landing Points against Resistance, attempts remaining, a Strike action and an accuracy-plus-rig breakdown. Catch/Bait selection is gated until landing succeeds. No voluntary Surface, Weight, capacity or overload UI is included.

The landing sample uses Resistance 80, 36 accumulated points, two attempts remaining from an illustrative four, and a previous Good strike of 12 accuracy +6 rig =18. The Mackerel Bait contribution illustrates the GDD's contiguous Schooling example with two connected allies. These values, matching outcome presentation, controls, input bindings, attempt count and ability numbers are **provisional visual examples**. They do not finalize scoring, reel probabilities, lock/respin costs or Technique windows. No currency identity is assumed.

## Sources and handoff

Reuses Midnight Field Journal paper, kelp-edge imagery, blank compact parchment frame and separately authored Sardine, Mackerel and Glass Minnow illustrations. The slot-machine shell and landing instrument use the R059 engraved coastal artwork; action buttons use the R060 botanical frame. Reel faces and the match-result panel reuse the existing TechniqueFrameV2 parchment, copper corners, marine plants and etched rules. The haul/score plaques and illustrated brass rig connectors continue the same card family. These are existing source images composed with separate native information fields, not newly generated replacement artwork.

Cinzel and Source Serif 4 retain the established typography. The right-side encounter dossier is a layout study using separate art and native fields; it is not an approved replacement production creature-card master. The initial DW-LOOP-001 boards are preserved below the review screens as archive copies on the same Penpot page.

This revision is **design only**. No Unity scripts, scenes, prefabs, import settings or artwork files under Assets were changed. It is not marked imported. Any later implementation requires selected layout approval, finalized gameplay dependencies and a separate integration task. Existing artwork provenance remains in the Midnight Field Journal records.

Verification: exported previews were visually reviewed for hierarchy, readable gameplay information, decoration placement and clipping. Source-image rectangles inside clipping boards intentionally extend beyond their local clip; they are not additional visible UI. Reel locks sit on the navy footer, and fish/zone motion stays clear of the botanical borders. Only the cross-screen prototype navigation is connected; lock/respin and Strike are visual controls.
