# Architecture

This document is the living responsibility map for the codebase. It should stay lightweight and practical: update it when class responsibilities, runtime flow, or system boundaries change.

The GDD remains the source of truth for game design. This document explains where the code should put those ideas.

## Core Principles

- Keep gameplay data, runtime state, and presentation separate.
- `CardDefinition` assets describe base card designs. They should not store per-run mutations.
- Runtime systems own run state, encounter state, decks, hands, catches, Line Load, and effect resolution.
- Views display state. They should not decide gameplay rules or mutate card data.
- Core actions (`Descend`, `Release`, `Surface`) remain always available and outside the player technique deck.
- Technique cards modify or bend fishing actions; they do not replace fundamental actions.
- Encounter cards represent what the ocean reveals. Creatures and Apex encounters can become `Hooked`.

## Current Classes

### `CardDefinition`

Role:
Base data asset for card designs.

Owns:
- Unique ID
- Display name
- Card type
- Rarity
- Tags
- Artwork reference
- Rules text
- Base weight and value
- Biome/depth availability
- Effect definitions

Does not own:
- Per-copy runtime modifications
- Whether one specific card is currently Hooked, Caught, released, or modified
- UI state
- Effect execution

### `CardEffectDefinition`

Role:
Serializable data describing what an effect is and when/where it can apply.

Owns:
- Effect ID
- Effect type
- Trigger
- Target
- Amount
- Required tags
- Optional replacement card reference
- Expiration hint
- Reminder text

Does not own:
- Runtime execution
- Game-state mutation
- Validation that every effect is currently playable

### `FishingRunController`

Role:
Current run coordinator.

Owns:
- Run active state
- Current biome and depth
- Line Capacity
- Initial run setup and shutdown
- Coordination of Descend, Release, and Surface
- Technique-card entry point for the Hooked reaction window
- Inspector configuration and debug action wrappers
- Optional view refreshes and action summaries

Delegates to:
- `TechniqueDeckRuntime` for hand and pile operations
- `EncounterRuntime` for encounter selection and Hooked state
- `CatchChainRuntime` for catches, Line Load, and attached effects
- `FishingRunResult` for the last Surface result

Does not own:
- Detailed deck, encounter, Catch Chain, or result operations
- Full effect execution
- Final UI composition

### `TechniqueDeckRuntime`

Role:
Runtime owner of the Technique hand and card piles.

Owns:
- Filtering the configured deck to Technique cards
- Fisher-Yates shuffling
- Starting hand draw
- Hand refill
- Draw-pile and discard-pile state
- Clearing deck state when the run ends

Does not own:
- Whether a Technique card can affect the current encounter
- Technique effect execution
- Player input or card views

### `EncounterRuntime`

Role:
Runtime owner of encounter selection and the Hooked reaction window.

Owns:
- Filtering encounter candidates by card type, biome, and depth
- Selecting the current encounter
- Encountered, Hooked, Caught, and None state transitions
- Current Hooked encounter
- Hooked effect records and Technique-effect relevance

Does not own:
- Catch Chain commitment after a Hooked card is taken
- Depth progression
- Encounter pool authoring or weighted selection
- Effect execution

### `CatchChainRuntime`

Role:
Runtime owner of catches attached to the line.

Owns:
- Catch acquisition order
- Adding and releasing catches
- Calculated Line Load
- Active caught-card effect records
- Rebuilding effect records after Release
- Catch Chain reset and snapshots

Does not own:
- Line Capacity
- Overload consequences
- Surface result presentation
- Effect execution or per-copy card mutations

### `FishingRunResult`

Role:
Serializable data for the most recent Surface result.

Owns:
- Successful haul snapshot
- Total base haul value
- Surface depth
- Surface Line Load and capacity
- Whether Surface began while overloaded

Does not own:
- Overload consequence rules
- Rewards, selling, or progression
- Run-summary UI

### `CatchChainView`

Role:
Presentation component for the visible Catch Chain panel.

Owns:
- Displaying catches in current acquisition order
- Clearly separated weight and value labels
- Per-catch active effect text
- Distinct visual treatment for explicitly negative effects
- Scrollable layout for longer chains
- Empty Catch Chain presentation

Does not own:
- Catch Chain data or ordering rules
- Effect classification or execution
- Release input
- Line Load calculations

### `HookedEffectRecord`

Role:
Runtime record that an effect can influence the current Hooked encounter reaction window.

Owns:
- Source card
- Effect definition
- Source type

Does not own:
- Effect execution
- Permanent active effect state after a card is caught

### `ActiveCatchEffectRecord`

Role:
Runtime record that a caught card has an effect relevant to the Catch Chain.

Owns:
- Source card
- Effect definition
- Trigger that made the effect relevant

Does not own:
- Effect execution
- Per-card modified weight, value, or state
- Removal rules after Release

### `CardView`

Role:
Presentation component for showing card data in UI.

Owns:
- Optional UI references
- Displaying card name, type, rarity, state, stats, tags, rules text, and artwork

Does not own:
- Card gameplay data
- Runtime card mutations
- Deck, encounter, or catch logic
- Technique-card click behavior beyond future UI forwarding

### `CardDebugSandbox`

Role:
Editor/debug helper for manually inspecting card data, catches, load, and effects.

Owns:
- Debug-only card assignment and state inspection helpers

Does not own:
- Real gameplay loop behavior
- Production UI behavior

## Runtime Flow

Current run startup flow:

1. `FishingRunController.StartRun()` initializes run state.
2. `TechniqueDeckRuntime` filters and shuffles the starting deck, then draws the hand.
3. The current biome and depth are set.
4. `EncounterRuntime` selects the first valid encounter and updates reaction state:
   - `None` when no encounter exists.
   - `Encountered` for non-catchable encounter cards.
   - `Hooked` for catchable Creature and Apex encounters.
5. Optional `CardView` references are refreshed.

Current Descend flow:

1. `FishingRunController.TryDescend()` resolves the always-available Descend action.
2. `EncounterRuntime` returns the Hooked card for commitment, if one exists.
3. `CatchChainRuntime` appends that card and tracks its catch-related effects.
4. Current Line Load updates because `CatchChainRuntime` calculates it from attached cards.
5. Current depth advances.
6. `TechniqueDeckRuntime` refills the hand as required.
7. `EncounterRuntime` filters candidates again using the new depth and current biome.
8. The next valid encounter is revealed and optional `CardView` references are refreshed.
9. `CatchChainView` rebuilds from the ordered catches and their indexed active effect records.

Current Release flow:

1. `FishingRunController.TryReleaseCatch(int)` selects an attached card by Catch Chain index.
2. `CatchChainRuntime` removes the selected card, so its weight and future haul value are lost.
3. `CatchChainRuntime` rebuilds active effects from catches that remain attached.
4. Depth, the current encounter, and Hooked state remain unchanged.
5. `CatchChainView` refreshes immediately with the remaining ordered catches.
6. The action logs the released card and immediate Line Load change.

Current Surface flow:

1. `FishingRunController.TrySurface()` passes the attached Catch Chain to `FishingRunResult`.
2. `FishingRunResult` snapshots the haul and records value, depth, Line Load, capacity, and overload state.
3. An unresolved Hooked encounter is excluded because it has not entered the Catch Chain.
4. The active run ends and transient encounter, Catch Chain, effect, and Technique deck state is cleared.
5. Last-haul fields remain Inspector-visible and a Console summary reports the result.

Future core loop direction:

1. Revealed catchable encounter becomes Hooked.
2. Player may use Technique cards during the reaction window.
3. `Descend` commits unresolved Hooked creatures to the Catch Chain, including effect execution.
4. Caught cards contribute Line Load, value, and active effects.
5. Player may `Release` caught cards to reduce load and remove effects.
6. `Surface` ends the descent and records the haul.

## Data, Runtime State, And Presentation

Base card data:
- `CardDefinition`
- `CardEffectDefinition`

Runtime state:
- Current encounter state
- Hooked encounter
- Hand/draw/discard piles
- Catch Chain
- Line Load
- Last Surface result
- Future `CardInstance` data
- Future active effect instances

Presentation:
- `CardView`
- `CatchChainView`
- Future hand views
- Future encounter view
- Future Line Load view

Rule of thumb:
If the value can differ between two copies of the same card in the same run, it belongs in runtime state, not in `CardDefinition`.

## Planned Classes

These are expected future directions, not required all at once.

### `CardInstance`

Runtime copy of a card that references a `CardDefinition` and stores per-copy state.

Expected responsibilities:
- Current weight
- Current value
- Current encounter/catch state
- Temporary modifiers
- Active effects attached to that specific copy

### Expanded Technique Runtime

As Technique play grows, `TechniqueDeckRuntime` may be split into dedicated deck and hand models for discard, reshuffle, targeting, and card-use validation.

### `EncounterPoolDefinition`

Authoring asset for biome/depth encounter pools.

Expected responsibilities:
- Encounter entries
- Weights
- Depth tiers
- Rarity
- Biome identity rules
- Apex exclusions or references

### `EffectResolver`

Runtime system that executes effect definitions against explicit contexts such as Hooked encounter, Descend, Catch, Release, Surface, or future encounter generation.

### UI Controllers

Presentation-layer components that bind runtime state to views and forward player input to gameplay systems.

Examples:
- Encounter UI controller
- Technique hand UI controller
- Catch Chain UI controller
- Line Load UI controller
- Core action button controller

## Boundaries

Do:
- Keep `CardDefinition` as reusable card design data.
- Keep `CardView` presentation-only.
- Add runtime models when card copies need independent state.
- Let gameplay systems expose explicit methods for player actions.
- Keep debug helpers separate from real gameplay UI.

Do not:
- Store per-run card changes in shared card assets.
- Put gameplay decisions inside `CardView`.
- Put core actions into the player Technique deck.
- Let encounter pools reveal Technique cards as ocean encounters.
- Build broad progression, shop, or multi-biome systems before the core Catch Chain and Line Load loop is validated.

## Relationship To Technical Debt

Use this file to describe the intended structure.

Use `Docs/MVP Technical Debt.md` to record places where the MVP intentionally uses a simpler approach than this architecture ultimately wants.
