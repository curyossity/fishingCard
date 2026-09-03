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
- `BiomeDefinition` assets establish biome identity without storing per-run state.

## Current Classes

### `BiomeDefinition`

Role:
Authored identity asset for one biome.

Owns:
- Stable biome ID used by card availability filters
- Player-facing biome name
- Core mechanical identity
- Strategic tension statement
- Signature card tags
- Ordered internal depth tiers and their inclusive ranges
- Tier-specific design intent and encounter subsets

Does not own:
- Current run depth or encounter state
- Per-card biome and depth eligibility
- Encounter weighting or random selection
- Biome progression and unlock rules

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
- Automatic caught-card target selection mode
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
- Starting and current biome references
- Current depth
- Line Capacity
- Initial run setup and shutdown
- Coordination of Descend, Release, and Surface
- Technique-card entry point for the Hooked reaction window
- Inspector configuration and debug action wrappers
- Loading repeatable Catch Chain decision scenarios during Play Mode
- Optional view refreshes and action summaries

Delegates to:
- `TechniqueDeckRuntime` for hand and pile operations
- `TechniqueEffectRuntime` for Technique validation, execution, and delayed effects
- `EncounterRuntime` for encounter selection and Hooked state
- `CatchChainRuntime` for catches, Line Load, and attached effects
- `LineLoadRiskRuntime` for overload checks and tuning
- `EffectResolver` for catch interactions, attraction, and encounter concealment
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
- Removing used cards from the hand
- Discarding used cards and automatically refilling the hand
- Reshuffling the discard pile when the draw pile is empty
- Draw-pile and discard-pile state
- Clearing deck state when the run ends

Does not own:
- Whether a Technique card can affect the current encounter
- Technique effect execution
- Player input or card views

### `TechniqueHandView`

Role:
Interactive four-slot presentation of the current Technique hand.

Owns:
- Stable card-slot layout
- Card names and rules text
- Draw-pile and discard-pile counts
- Playable and unavailable visual states
- Forwarding `USE` button clicks to the controller

Does not own:
- Card-use rules
- Deck mutations
- Technique effect execution

### `TechniqueEffectRuntime`

Role:
Runtime owner of Technique-card rules and effects that wait for the next Descend or encounter reveal.

Owns:
- Checking whether a Technique card has a valid target in the Hooked reaction window
- Immediate avoidance, replacement, and information reveal
- Queued next-Descend and next-encounter effects
- Temporary capacity, depth, committed-catch, and overload-reward resolution
- Clearing one-use effects after their matching action

Delegates to:
- `CatchChainRuntime` for automatic Catch Chain targeting, modification, and release
- `EncounterRuntime` for replacement and encounter selection
- `EffectResolver` for encounter weights and Catch Chain recalculation

Does not own:
- Hand, draw-pile, or discard-pile state
- Core-action availability
- Technique-card presentation

### `EncounterRuntime`

Role:
Runtime owner of encounter selection and the Hooked reaction window.

Owns:
- Filtering the selected biome-tier pool by card type, biome, and per-card depth limits
- Weighted encounter selection using attached attraction effects
- Last candidate, selected, and total encounter weights for debugging
- Encountered, Hooked, Caught, and None state transitions
- Current Hooked encounter
- Hooked effect records and Technique-effect relevance
- Applying an explicitly supplied encounter through the normal state transition rules

Does not own:
- Catch Chain commitment after a Hooked card is taken
- Depth progression
- Encounter pool authoring or attraction-weight calculation
- Effect execution

### `CatchChainRuntime`

Role:
Runtime owner of catches attached to the line.

Owns:
- Catch acquisition order
- Creating an independent `CardInstance` for each committed catch
- Adding and releasing catches
- Calculated Line Load from resolved instance weights
- Active caught-card effect records
- Rebuilding and resolving catch interactions after Catch or Release
- Catch Chain reset and snapshots

Does not own:
- Line Capacity
- Overload consequences
- Surface result presentation
- Effect rule implementation

### `FishingRunResult`

Role:
Serializable data for the most recent Surface result.

Owns:
- Successful haul snapshot
- Total resolved haul value
- Surface depth
- Surface Line Load and capacity
- Whether Surface began while overloaded

Does not own:
- Overload consequence rules
- Rewards, selling, or progression
- Run-summary UI

### `LineLoadRiskRuntime`

Role:
Tunable prototype resolver for overload strain.

Owns:
- Base break chance
- Additional chance per point above capacity
- Maximum break chance
- Last overload roll, chance, excess load, outcome, and selected catch index
- Selecting a random catch index when strain causes a loss

Does not own:
- Catch Chain mutation
- Line Load or capacity
- Final overload balance
- Visual feedback

### `CatchChainView`

Role:
Presentation component for the visible Catch Chain panel.

Owns:
- Displaying catches in current acquisition order
- Clearly separated current weight and value labels, including modifiers
- Per-catch active effect text
- Distinct visual treatment for explicitly negative effects
- Current Load / Capacity display
- Stable, approaching-limit, and overloaded visual states
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
- Exact source `CardInstance`
- Effect definition
- Trigger that made the effect relevant
- Current Catch Chain index of the source

Does not own:
- Effect execution
- Removal rules after Release

### `CardInstance`

Role:
Independent runtime copy of a card attached to the Catch Chain.

Owns:
- Runtime instance ID
- Reference to its immutable `CardDefinition`
- Current resolved weight and value
- Weight and value modifier reporting
- Snapshot creation for Surface results

Does not own:
- Effect target selection
- Encounter selection
- Presentation

### `EffectResolver`

Role:
Executes the currently supported attached-catch interactions.

Owns:
- Recalculating catch stats from base definitions
- Applying attached value and weight modifiers
- Selecting a previous, next, first, or last matching catch target
- Adding tag-based attraction weight to future encounters
- Reporting persistent encounter-information concealment

Does not own:
- Catch Chain collection mutation
- Encounter state transitions
- Technique-card execution
- Final effect balance or player-selected targeting

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

### `CatchChainScenarioDefinition`

Role:
Data-only setup for a repeatable Catch Chain decision playtest.

Owns:
- Scenario name and decision to observe
- Test Line Capacity and depth
- Ordered starting catch definitions
- Explicit current encounter

Does not own:
- Runtime catch instances
- Effect resolution
- Player-facing progression or encounter content
- Whether the tested decision is fun or balanced

## Runtime Flow

Current run startup flow:

1. `FishingRunController.StartRun()` initializes run state.
2. `TechniqueDeckRuntime` filters and shuffles the starting deck, then draws the hand.
3. The current biome and depth are set.
4. `BiomeDefinition` supplies the encounter subset for the starting depth tier.
5. `EncounterRuntime` selects the first valid encounter and updates reaction state:
   - `None` when no encounter exists.
   - `Encountered` for non-catchable encounter cards.
   - `Hooked` for catchable Creature and Apex encounters.
6. Optional `CardView` references are refreshed.

Current Descend flow:

1. `FishingRunController.TryDescend()` resolves the always-available Descend action.
2. `EncounterRuntime` returns the Hooked card for commitment, if one exists.
3. `CatchChainRuntime` creates a `CardInstance`, appends it, and tracks its catch-related effects.
4. `TechniqueEffectRuntime` consumes pending Descend effects for catch stats, distance, capacity, risk, and overload rewards.
5. `EffectResolver` recalculates catch interactions from each instance's lasting base state.
6. Current Line Load updates from the resolved instance weights.
7. `LineLoadRiskRuntime` checks for strain using any one-use capacity bonus.
8. If the check fails, `CatchChainRuntime` releases the randomly selected catch; otherwise the line remains overloaded.
9. Current depth advances using any one-use distance modifier.
10. `TechniqueDeckRuntime` refills the hand as required.
11. `BiomeDefinition` supplies the tier pool at the effective selection depth.
12. `EncounterRuntime` filters that subset using card availability, attached effects, and one-use weight modifiers.
13. The next valid encounter is revealed; persistent concealment can hide its details.
14. `CatchChainView` rebuilds from catch instances, effects, current Load, and capacity.

Current Technique-card use flow:

1. `TechniqueHandView` forwards a slot's `USE` command to `FishingRunController`.
2. The controller validates the run and hand slot; `TechniqueEffectRuntime` validates effect targets.
3. `TechniqueDeckRuntime` moves the card from hand to discard and refills the hand.
4. `TechniqueEffectRuntime` executes immediate effects or queues them for the next matching action.
5. `CatchChainRuntime` applies lasting per-copy changes and recalculates persistent interactions when required.
6. Encounter replacement performs a weighted reveal while excluding the replaced card.
7. If the draw pile is empty, the discard pile is shuffled into a new draw pile before refilling.
8. The controller refreshes the hand, piles, encounter, Catch Chain, and playability states.

Current Release flow:

1. `FishingRunController.TryReleaseCatch(int)` selects an attached card by Catch Chain index.
2. `CatchChainRuntime` removes the selected card, so its weight and future haul value are lost.
3. `CatchChainRuntime` rebuilds active effects and resolves all remaining interactions from base stats.
4. Depth, the current encounter, and Hooked state remain unchanged.
5. `CatchChainView` refreshes immediately with the remaining ordered catches.
6. The action logs the released card and immediate Line Load change.

Current Surface flow:

1. `FishingRunController.TrySurface()` records the starting Surface load.
2. `LineLoadRiskRuntime` checks for strain when the line is overloaded.
3. A failed check releases one random catch before the successful haul is recorded; a held check preserves the overloaded chain.
4. `FishingRunResult` snapshots the remaining haul and records value, depth, starting Line Load, capacity, and overload state.
5. An unresolved Hooked encounter is excluded because it has not entered the Catch Chain.
6. The active run ends and transient encounter, Catch Chain, effect, and Technique deck state is cleared.
7. Last-haul fields remain Inspector-visible and a Console summary reports the result and strain outcome.

Current debug scenario flow:

1. `FishingRunController.LoadDebugScenario(...)` receives a scenario asset during Play Mode.
2. The controller applies the scenario's capacity and depth and resets the current Catch Chain.
3. `CatchChainRuntime` creates instances for the ordered starting catches and resolves their interactions.
4. `EncounterRuntime` applies the scenario encounter through the same Hooked/Encountered state rules used by normal reveals.
5. Views refresh and the Console reports the decision the scenario is intended to expose.
6. Normal Descend, Release, Surface, overload, and effect behavior continue from that state.

Future core loop direction:

1. Revealed catchable encounter becomes Hooked.
2. Player may use Technique cards during the reaction window.
3. `Descend` commits unresolved Hooked creatures to the Catch Chain, including effect execution.
4. Caught cards contribute Line Load, value, and active effects.
5. Player may `Release` caught cards to reduce load and remove effects.
6. `Surface` ends the descent and records the haul.

## Data, Runtime State, And Presentation

Base card data:
- `BiomeDefinition`
- `CardDefinition`
- `CardEffectDefinition`

Runtime state:
- Current encounter state
- Hooked encounter
- Hand/draw/discard piles
- Catch Chain
- Per-copy current catch weight and value
- Per-copy lasting Technique weight and value modifiers
- Pending next-Descend and next-encounter Technique effects
- Line Load
- Last overload-risk check
- Last Surface result
- Future active effect instances

Presentation:
- `CardView`
- `CatchChainView`
- `TechniqueHandView`
- Future encounter view

Rule of thumb:
If the value can differ between two copies of the same card in the same run, it belongs in runtime state, not in `CardDefinition`.

## Planned Classes

These are expected future directions, not required all at once.

### Expanded Technique Runtime

As Technique play grows, `TechniqueDeckRuntime` may be split into dedicated deck and hand models. Target selection and richer card-use validation may also move into dedicated rule services.

### `EncounterPoolDefinition`

Authoring asset for biome/depth encounter pools.

Expected responsibilities:
- Encounter entries
- Weights
- Depth tiers
- Rarity
- Biome identity rules
- Apex exclusions or references

It may replace the raw tier arrays currently embedded in `BiomeDefinition` when weighted authoring and encounter chains are implemented.

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
