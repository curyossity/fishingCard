# Card Visual Specification

This document defines the current approved presentation contract for creature and catch cards. Read it before creating or changing card art, card prefabs, card views, or runtime card-field rendering.

## Source Card Art

The user supplies a separate card image for each creature. This is not a generic blank template: the supplied image already contains the creature illustration together with the decorative frame and other fixed visual artwork.

The supplied image should leave the runtime-owned information regions blank. Unity must not bake creature-specific gameplay text or numbers into the source image.

Use the Giant Octopus example in `Assets/examples/Giant_octopus_example.png` as the current layout reference. Future supplied card art or explicit user direction may refine that reference.

## Unity-Owned Fields

Unity fills these fields from card data:

- Card type
- Card name
- Weight
- Value
- Effect text
- Rarity hooks

These fields must remain separate UI elements layered over the supplied card image so they can be changed without editing the artwork.

Weight and Value display the currently resolved values. When gameplay modifiers change either value, update the number in place. Do not add modifier deltas, arrows, badges, or separate base-value text unless the user explicitly requests them.

Text must fit its authored region at supported resolutions. Use wrapping and bounded best-fit sizing where necessary, while preserving the visual hierarchy shown by the reference card.

## Rarity Hooks

Creature/catch card art provides four hook positions. Unity displays the card rarity by activating the corresponding number of hooks:

- `Common`: 1 active hook
- `Uncommon`: 2 active hooks
- `Rare`: 3 active hooks
- `Legendary`: 4 active hooks

Inactive hook positions remain visibly inactive. Hook count is a visual representation of `CardRarity`, not independent gameplay state.

## Hidden Card-Face Information

Do not add dedicated card-face labels, badges, icons, colors, or text for these runtime concepts:

- Encountered, Hooked, or Caught state
- Selected-for-Release state
- Hidden-information state
- Active modifier state
- Positive or negative modifier indicators
- Base-versus-modified value comparisons
- Relevant tags

These concepts may still exist and affect gameplay internally. Runtime modifiers may change the displayed Weight, Value, or Effect text in place, but the card face should not explain or expose the internal state that produced the final displayed value.

If interaction feedback for these concepts becomes necessary, keep it outside the card-face information layout unless the user approves a revised card design.

## Layering Contract

Build the card presentation from two ownership layers:

1. Supplied visual layer: creature illustration, frame, ornament, fixed labels or icons, and other non-changing artwork.
2. Unity UI layer: type, name, resolved Weight, resolved Value, Effect text, and four rarity-hook states.

Keep gameplay rules out of the card view. The view reads supplied card data and resolved runtime values, then updates only its visual fields.

## Scope

This specification currently applies to creature/catch cards. Technique cards, hazards, opportunities, treasure, locations, and core action cards may reuse parts of this language, but their final card-face contracts require their own supplied references or explicit user approval.

This user-approved card-face contract overrides earlier prototype assumptions that tags or runtime-state labels must be printed directly on creature cards.
