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
- Current encounter and encounter state
- Hooked encounter tracking
- Technique hand, draw pile, and discard pile
- Catch Chain placeholder state
- Descend core action prototype
- Release core action prototype
- Surface core action and last-haul result state
- Active caught-card effect tracking
- Initial run setup
- Encounter reveal from the current biome and depth
- Technique-card entry point for the Hooked reaction window

Does not own long term:
- Full deck/hand implementation once that grows complex
- Full Catch Chain rules once catch interactions grow complex
- Full effect resolution once effects affect multiple systems
- Final UI composition

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
2. The starting Technique deck is filtered and shuffled.
3. The starting hand is drawn.
4. The current biome and depth are set.
5. The first valid encounter is selected from the encounter pool.
6. The encounter reaction state is updated:
   - `None` when no encounter exists.
   - `Encountered` for non-catchable encounter cards.
   - `Hooked` for catchable Creature and Apex encounters.
7. Optional `CardView` references are refreshed.

Current Descend flow:

1. `FishingRunController.TryDescend()` resolves the always-available Descend action.
2. If a catchable encounter is Hooked, it is appended to the Catch Chain.
3. Effects with `WhenCaught` or `WhileAttached` triggers are tracked as active Catch Chain effects.
4. Current Line Load updates because it is calculated from the Catch Chain.
5. Current depth advances.
6. Encounter candidates are filtered again using the new depth and current biome.
7. The next valid encounter is revealed.
8. The encounter reaction state and optional `CardView` references are refreshed.

Current Release flow:

1. `FishingRunController.TryReleaseCatch(int)` selects an attached card by Catch Chain index.
2. The selected card is removed from the Catch Chain, so its weight and future haul value are lost.
3. Active Catch Chain effect records are rebuilt from the catches that remain attached.
4. Depth, the current encounter, and Hooked state remain unchanged.
5. The action logs the released card and immediate Line Load change.

Current Surface flow:

1. `FishingRunController.TrySurface()` snapshots the attached Catch Chain as the successful haul.
2. The result records total base value, Surface depth, Line Load, and whether the line was overloaded.
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
- Future `CardInstance` data
- Future active effect instances

Presentation:
- `CardView`
- Future hand views
- Future encounter view
- Future Catch Chain view
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

### `DeckRuntime`

Runtime owner of draw pile, discard pile, shuffling, drawing, discarding, and reshuffling.

### `HandRuntime`

Runtime owner of visible Technique hand slots, card-use validation, and refill behavior.

### `CatchChainRuntime`

Runtime owner of caught card order, add/remove operations, release rules, active attached effects, and Line Load updates.

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
