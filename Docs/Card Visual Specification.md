# Card Visual Specification

This document defines the current approved presentation contract for creature and catch cards. Read it before creating or changing card art, card prefabs, card views, or runtime card-field rendering.

## Source Card Art

The production Encounter card uses the reusable supplied dual-effect master `Assets/FishingUIAssets/Cards/Creature/catch-card-base-dual-effect-1122x1402.png` at its native 1122 x 1402 resolution. The master owns the decorative frame, chart field, fixed `WEIGHT` and `VALUE` labels, stat icons, separate built-in `CATCH` and `BAIT` effect panels, four lower rarity sockets, and four large tag-rail sockets. Previous catch masters remain available as alternate source art and are not deleted when the active master changes.

Non-catchable encounters currently use the storm-purple `Assets/FishingUIAssets/Cards/Event/event-card-base-storm-purple-1101x1429.png` at 1101 x 1429. This event master owns its frame, blank type and title plaques, chart field, and expanded ivory rules panel. It intentionally has no Weight, Value, or rarity-socket presentation. The earlier red/silver `event-card-base-empty-with-type-1101x1429.png` remains available as an alternate visual treatment.

Unity layers each creature's illustration and runtime-owned information over this master. A precomposed per-creature card face must not replace the production Encounter master, even when legacy card data still contains one.

Creature artwork supports two explicitly authored layouts on `CardDefinition`: `MaskedRegion` for a cropped illustration clipped to the chart field, and `FullCardOverlay` for a transparent image authored on the established full-card canvas. Full-card overlays fill the current catch master so previously supplied 1101 x 1429 transparent artwork is normalized to the new 1122 x 1402 face geometry, while remaining below all Unity-owned text, stats, rules, and rarity markers. A definition may provide a dedicated `EncounterArtwork` overlay while retaining a cropped `Artwork` sprite for compact cards and other portrait consumers. Do not infer layout from filenames or image dimensions.

## Unity-Owned Fields

Unity fills these fields from card data:

- Card type
- Card name
- Weight
- Value
- Catch effect text
- Bait effect text
- Rarity hooks

The supplied four-socket rail displays Unity-owned tag indicators without reducing the existing content regions. Creature tags use the closed vocabulary `Fish`, `Predator`, `Schooling`, `Small`, `Heavy`, `Anchored`, `Armored`, and `Elusive`. Supported icons are matched case-insensitively to the card's authored tags and filled from top to bottom in canonical vocabulary order. Hovering a populated icon shows its tag name on the dedicated tooltip layer; the tooltip does not intercept input. Unsupported icon artwork and unused sockets remain empty and non-interactive. The currently supplied icon mappings are `Predator` and `Small`. Event cards do not display the catch-card tag rail.

The upper card-type field separates catchable encounters from event encounters:

- `Creature` and `Treasure`: `CATCH CARD`
- `ApexEncounter`: `APEX CATCH`
- Non-catchable encounter categories, including `Hazard`, `Environment`, `Opportunity`, and generic `Encounter`: `EVENT CARD`

`Creature`, `Treasure`, and `ApexEncounter` use the catch master. All other encounter categories use the event master. Event cards hide the catch-only Weight, Value, and rarity-marker objects and expand the rules field into the space available on the event artwork.

These fields must remain separate UI elements layered over the supplied card image so they can be changed without editing the artwork.

Weight and Value display the currently resolved values. When gameplay modifiers change either value, update the number in place. Do not add modifier deltas, arrows, badges, or separate base-value text unless the user explicitly requests them.

A card-authored self-concealment effect may replace only that card's Value with `?` during the run. Weight, name, tags, rules, and other encounter information remain visible unless a separate global concealment effect applies. End-of-run results reveal the resolved Value normally. If a card has an authored random caught-Value range, each committed runtime copy rolls once, retains that base Value through recalculation, and then receives normal interaction modifiers.

Text must fit its authored region at supported resolutions while preserving the visual hierarchy shown by the reference card. Catch-card rules use dark-teal text inside each of the master's two ivory effect panels. Definitions may author separate Catch and Bait rules text and effect arrays. Existing definitions with no Bait data mirror the Catch presentation and effect set as a migration fallback. Main-card rules text uses 18 points whenever the content fits, with bounded automatic sizing allowed to reduce the text only when required by each authored region. Event-card rules remain in the event master's single ivory panel.

The built-in Catch and Bait panels are separate click targets. Catch is selected when a new catchable encounter is presented. Clicking either panel keeps it selected, applies a restrained teal selection wash and gold outline, and deselects the other panel. On Descend, the committed `CardInstance` records the selected role. Only that role's effect array becomes active: Catch effects never activate for a Bait attachment, and Bait effects never activate for a Catch attachment. Both roles remain cards in the Catch Chain and contribute their resolved Weight and Value normally. Compact Catch Chain cards display an explicit `BAIT` marker when their stored role is Bait.

Rules text may include per-definition highlighted words or phrases authored through the Creature Catalog. The central Encounter card renders each configured match with its authored color and relative font size calculated from the fitted base size while preserving the surrounding rules style. Highlighting is presentation metadata only: it must not change effect resolution, and it does not appear on compact Catch Chain cards.

## Rarity Hooks

Creature/catch card art exposes four lower rarity sockets. Unity fills them with `Assets/FishingUIAssets/Icons/catch.png`, activating the corresponding number of catch icons:

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

These concepts may still exist and affect gameplay internally. Runtime modifiers may change the displayed Weight, Value, or Effect text in place, but the card face should not explain or expose the internal state that produced the final displayed value.

If interaction feedback for these concepts becomes necessary, keep it outside the card-face information layout unless the user approves a revised card design.

## Layering Contract

Build the card presentation from two ownership layers:

1. Supplied visual layer: the reusable empty master with frame, ornament, fixed labels, icons, and empty sockets.
2. Unity UI layer: creature illustration, type, name, resolved Weight, resolved Value, Catch/Bait effect text, four filled rarity-marker states, supported tag icons, and the selected-effect feedback. A creature may author a central-card-only scale, rotation, and offset for its encounter illustration; this must not alter its separately framed compact Catch Chain portrait.

Keep gameplay rules out of the card view. The view reads supplied card data and resolved runtime values, then updates only its visual fields.

## Scope

This specification applies to catchable Encounter cards and to non-catchable event cards using the supplied event master. Technique cards, locations, and core action cards require their own supplied references or explicit user approval.

This user-approved card-face contract overrides earlier prototype assumptions that tags or runtime-state labels must be printed directly on creature cards.

## Compact Catch Chain Cards

Catch Chain summaries use the reusable supplied master `Assets/FishingUIAssets/Cards/CompactCatch/fishing-ui-catch-chain-card-master.png`, rather than the full per-creature encounter face. Unity owns the compact card's creature name, creature artwork, resolved Weight, resolved Value, optional passive-effect icon, and the required `BAIT` role marker. Do not add full rules text, tags, modifier deltas, catch order, or other internal runtime-state labels to this compact face. The rig ring, clip, and medallion remain separate sibling images and must not be merged into the card sprite. Valid-release, invalid-release, focus, warning, and critical presentation remains in separate overlays.
