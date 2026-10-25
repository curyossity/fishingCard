# Card Visual Specification

This document defines the current approved presentation contract for creature and catch cards. Read it before creating or changing card art, card prefabs, card views, or runtime card-field rendering.

## Candidate theme: Midnight Field Journal

The user has approved a separate design for Unity comparison. Its theme-specific artwork, geometry, typography, Catch Chain hardware and technique perspective are defined in [Midnight Field Journal Visual Contract](UIThemes/MidnightFieldJournal/VisualContract.md), with [source inventory](UIThemes/MidnightFieldJournal/AssetInventory.md) and [outstanding visual dependencies](UIThemes/MidnightFieldJournal/MissingVisualAssets.md). The artwork paths and geometry below continue to govern the existing nautical design; they must not overwrite the candidate or be silently mixed into it. Shared dynamic-field, gameplay, concealment, role and event rules remain binding. Keep the original scene and assets intact while integrating the candidate under `Assets/UIThemes/MidnightFieldJournal/`.

## Source Card Art

The production Encounter card uses the reusable supplied dual-effect master `Assets/FishingUIAssets/Cards/Creature/catch-card-base-dual-effect-1122x1402.png` at its native 1122 x 1402 resolution. The master owns the decorative frame, chart field, fixed `WEIGHT` and `VALUE` labels, stat icons, separate built-in `CATCH` and `BAIT` effect panels, four lower rarity sockets, and four large tag-rail sockets. Previous catch masters remain available as alternate source art and are not deleted when the active master changes.

Non-catchable encounters currently use the storm-purple `Assets/FishingUIAssets/Cards/Event/event-card-base-storm-purple-1101x1429.png` at 1101 x 1429. This event master owns its frame, blank type and title plaques, chart field, and expanded ivory rules panel. It intentionally has no Weight, Value, or rarity-socket presentation. The earlier red/silver `event-card-base-empty-with-type-1101x1429.png` remains available as an alternate visual treatment.

Unity layers each creature's illustration and runtime-owned information over this master. A precomposed per-creature card face must not replace the production Encounter master, even when legacy card data still contains one.

Creature artwork supports two explicitly authored layouts on `CardDefinition`: `MaskedRegion` for a cropped illustration clipped to the chart field, and `FullCardOverlay` for a transparent image authored on the established full-card canvas. Full-card overlays fill the current catch master so previously supplied 1101 x 1429 transparent artwork is normalized to the new 1122 x 1402 face geometry, while remaining below all Unity-owned text, stats, rules, and rarity markers. A definition may provide a dedicated `EncounterArtwork` overlay while retaining a cropped `Artwork` sprite for compact cards and other portrait consumers. Do not infer layout from filenames or image dimensions.

## Unity-Owned Fields

The Midnight Field Journal candidate supports the user-requested modified-number tooltip (MFJ-R045): hover or focus a visible non-default Weight or Catch Value to inspect default/current values and named source contributions. It uses a separate sliced frame, flat icons and at most three visible reasons, with scroll arrows above/below the reasons as appropriate. No scrollbar, source-count caption or icon background is shown. Bait's intrinsic zero sale value and concealed information never open this tooltip. The card face itself gains no extra modifier badges or printed baseline fields. The tooltip reads resolver-owned attribution and preserves the approved stat animations.

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

Event cards show their name, type, artwork, and description in the event master. They have no Weight, sale Value, Catch/Bait selection panels, or Catch Chain attachment/compact-card presentation. The single rules panel presents the event description; it does not imply a Catch role.

These fields must remain separate UI elements layered over the supplied card image so they can be changed without editing the artwork.

Weight and Value display the currently resolved values. When gameplay modifiers change either value, update the number in place. The user-approved Midnight Field Journal current-encounter exception animates all effect-driven changes through visible integers and enlarges the affected number: green for Value increases/Weight decreases, red for the opposite. Choosing Bait immediately shows neutral zero sale Value without counting down; returning to Catch animates only effect modifiers from authored Catch Value. It leaves unaffected stats, concealed fields and historical snapshots unanimated. Gameplay always uses authoritative resolved values, independently of the displayed count. Do not add modifier deltas, arrows, badges, or separate base-value text unless the user explicitly requests them.

A card-authored self-concealment effect may replace only that card's Value with `?` during the run. Weight, name, tags, rules, and other encounter information remain visible unless a separate global concealment effect applies. End-of-run results reveal the resolved Value normally. If a card has an authored random caught-Value range, each committed runtime copy rolls once, retains that base Value through recalculation, and then receives normal interaction modifiers.

Text must fit its authored region at supported resolutions while preserving the visual hierarchy shown by the reference card. Catch-card rules use dark-teal text inside each of the master's two ivory effect panels. Definitions may author separate Catch and Bait rules text and effect arrays. Unrefactored definitions mirror Catch as a migration fallback. Refactored definitions set `hasExplicitBaitData`; an empty effect array or blank rules text then remains empty and never mirrors the other role. Cod, Glass Minnow, and Sardine have effectless Catch panels. Main-card rules text uses 18 points whenever the content fits, with bounded automatic sizing allowed to reduce the text only when required by each authored region. Event-card rules remain in the event master's single ivory panel.

The built-in Catch and Bait panels are separate click targets. Creature and Apex encounters begin with neither Catch nor Bait selected. Descend stays disabled until the player explicitly selects one role for that encounter occurrence. Role panels remain clickable while Descend is gated. New reveals and replacements, including the same species, clear the choice; ordinary stat/preview refreshes preserve it. Clicking either panel keeps it selected, applies a restrained teal selection wash and gold outline, and deselects the other panel. On Descend, the committed `CardInstance` records the selected role. Only that role's effect array becomes active: Catch effects never activate for a Bait attachment, and Bait effects never activate for a Catch attachment. Both roles remain cards in the Catch Chain and contribute their resolved Weight to Line Load. Catch attachments contribute their resolved Value to the haul; Bait attachments contribute zero haul Value. Compact Catch Chain cards display an explicit `BAIT` marker and zero Value when their stored role is Bait.

Rules text may include per-definition highlighted words or phrases authored through the Creature Catalog. The central Encounter card renders each configured match with its authored color and relative font size calculated from the fitted base size while preserving the surrounding rules style. Highlighting is presentation metadata only: it must not change effect resolution, and it does not appear on compact Catch Chain cards.

## Event interaction timing

Event reveal reactions, including Hazard cancellation, finish before event consequences. After those reactions the surrounding run UI allows legal Techniques until the player confirms Descend or Surface, even when the event has been cancelled and discarded. The cancelled card need not remain visible to preserve that window. Technique availability still follows legal targets and conditions.

Unless an event states otherwise, its consequences and resulting creature reactions resolve once before the confirmed action. Once resolution begins, keep Technique input closed until that action finishes. Event resolution adds no extra depth advance. Views forward commands and display runtime-owned state; they do not decide cancellation, event timing, or modifier duration. Do not add card-face runtime badges for these phases.

## Squid Ink Veil concealment

The approved Squid effect conceals an ordinary creature until its Catch/Bait choice is committed. Use the existing supplied catch master: replace the dynamic name with `UNKNOWN CREATURE`, remove both illustration layers, display `?` for Weight and Value, leave both effect texts blank, and suppress populated tag icons/tooltips and rarity hooks. Keep the existing Catch and Bait panels selectable with their normal selection feedback. Do not add a hidden-state badge or alter supplied artwork.

Reveal identity and the normal fields before the committed role's on-attach effects resolve. An immediate Ink Escape choice exposes no species name, art, tags, stats, or effects while concealment applies. Carry the identity mask through incoming-card transitions and encounter-history/ascent snapshots. Existing information-only concealment and self-hidden Value retain their own behavior.

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

Catch Chain summaries use the reusable supplied master `Assets/FishingUIAssets/Cards/CompactCatch/fishing-ui-catch-chain-card-master.png`, rather than the full per-creature encounter face. Unity owns the compact card's creature name, creature artwork, resolved Weight, role-aware haul Value, optional passive-effect icon, and the required `BAIT` role marker. A Bait attachment displays zero Value because it contributes no haul Value. Do not add full rules text, tags, modifier deltas, catch order, or other internal runtime-state labels to this compact face. The rig ring, clip, and medallion remain separate sibling images and must not be merged into the card sprite. Valid-release, invalid-release, focus, warning, and critical presentation remains in separate overlays.

## Candidate Catch Chain stat feedback

The Midnight Field Journal compact Catch/Bait cards share the main creature's integer pacing, colour polarity, enlargement and spring motion for resolved stat changes. Sequence affected creatures from newest to oldest; scroll each into view before its count and wait for the final spring to settle before starting the next. Queued recipients retain their visible integers without early enlargement/motion. Refreshes and shifted indices must not restart animations or transfer state between runtime copies. Bait zero Value remains immediate and neutral; hidden Value stays hidden. Manual wheel/drag inspection cancels the sequence and settles remaining values. Static ended-run snapshots do not animate. No artwork, authored stats or gameplay timing changes are implied.

Before each affected candidate compact creature's number animation, play one gentle centered card pop (4.5% maximum) with a rounded warm-ivory flash (18% peak opacity) over 0.38 seconds. Use the same prelude for beneficial and harmful changes; numbers retain benefit/harm colours. Scroll into view first with expansion padding. Keep chain hardware stationary. Restore normal card scale/zero flash before counting, and finish counting/spring settling before starting the next recipient. No repeating flash, new artwork, frame or input-blocking overlay is added.

Current candidate numeric pacing supersedes the earlier readable-per-integer timing: 25ms ordinary steps, 50ms initial hold and 180ms spring settle. Every visible number change pops and tilts slightly. Small deltas retain each integer; large deltas may jump through intermediate integer values to convey direction rapidly instead of requiring every integer to be readable. Card-level pop/flash and one-creature-at-a-time Catch Chain ordering are retained.

Current numeric pacing is 40ms per ordinary step, with a nominal 0.8-second large-delta count; retain the existing 50ms hold and 180ms spring. Manipulated main/compact numbers are bold during animation and stay bold at a non-default result. On return to the default, restore the original font style/weight after the landing completes. Preserve existing colour, enlargement and per-change pop/tilt. Deferred queued numbers do not turn bold before their turn. Hidden values, static snapshots and neutral Bait-zero Value retain their normal styling.

Stat ink follows the displayed number relative to its default: Value above default/Weight below default is green; the opposite is red. Same-side changes retain that colour regardless of travel direction. Counting back to default retains its side colour until the displayed result reaches default, then restores normal ink. Crossing default toward the other side switches directly green↔red; a passing-through default number uses the destination side's ink, never a normal-colour flash. Existing immediate neutral Bait-zero Value and deferred queued styling are retained.
