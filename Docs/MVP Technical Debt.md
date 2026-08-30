# MVP Technical Debt

This file tracks implementation choices that are acceptable for MVP development but should likely be revisited before turning the prototype into a production-quality game.

Each item should keep the same format:

- Current approach
- Why it is acceptable for MVP
- Production concern
- Revisit trigger
- Likely future action

## Runtime Card Instances

Current approach:
`CardDefinition` assets are used directly as card data during runtime.

Why it is acceptable for MVP:
Shared card definitions are simple to author, easy to inspect in Unity, and enough while cards do not need per-copy mutations.

Production concern:
If one specific Sardine gains modified weight, value, state, or temporary effects, changing the shared `Sardine.asset` would incorrectly affect every Sardine.

Revisit trigger:
Any gameplay system needs to modify one specific card copy during a run.

Likely future action:
Introduce a runtime `CardInstance` model that references a `CardDefinition` and stores per-card state such as current weight, current value, temporary modifiers, attached effects, and encounter/catch state.

## Line Load Storage

Current approach:
`CurrentLineLoad` is calculated from the Catch Chain whenever requested.

Why it is acceptable for MVP:
The Catch Chain is small, and recalculating avoids desync while add/remove/effect paths are still under construction.

Production concern:
As load modifiers, temporary effects, and frequent UI updates grow, recalculating can become harder to reason about or less efficient.

Revisit trigger:
Line Load changes from multiple systems such as catch effects, release effects, temporary capacity modifiers, or equipment.

Likely future action:
Store runtime `currentLineLoad` and update it through centralized add/remove/modify methods that keep the value synchronized with Catch Chain changes.

## Encounter Pool Authoring

Current approach:
`FishingRunController` uses a raw `CardDefinition[] encounterPool` assigned in the Inspector.

Why it is acceptable for MVP:
It is fast to wire, transparent in the Inspector, and enough for manually testing early encounter reveal behavior.

Production concern:
Raw arrays do not express encounter weights, depth tiers, rarity, biome rules, exclusions, encounter chains, or Apex separation.

Revisit trigger:
Biome content needs weighted selection, depth-tier pools, encounter chains, rare encounters, or proper Apex rules.

Likely future action:
Create an `EncounterPoolDefinition` or biome definition asset that owns validated encounter entries, weights, depth ranges, rarity, and Apex-specific data.

## Runtime Card-Type Guards

Current approach:
Runtime methods filter out invalid cards from decks and encounter pools, such as non-Technique cards in the player deck.

Why it is acceptable for MVP:
The project currently uses one flexible `CardDefinition` type, and runtime guards prevent bad Inspector wiring from creating broken run state.

Production concern:
Invalid setup should be caught before Play Mode, not quietly ignored at runtime.

Revisit trigger:
Card pools and deck assets become large enough that manual Inspector mistakes are likely or hard to spot.

Likely future action:
Add authoring validation through `OnValidate`, custom editor tooling, typed pool assets, or import/build validation that reports invalid card assignments.

## Technique Card Interaction Wiring

Current approach:
`TryUseTechniqueCard(int handIndex)` exists as the gameplay entry point, with a debug context-menu wrapper for testing.

Why it is acceptable for MVP:
The backend can be tested before the full hand UI exists, and future UI buttons can call the same method.

Production concern:
Players need clickable card slots, playable/unplayable feedback, targeting, discard, refill, reshuffle, and clear effect feedback.

Revisit trigger:
Technique-card use needs to be part of normal play rather than Inspector/debug testing.

Likely future action:
Build a hand UI controller that binds each visible card slot to runtime hand state and calls technique-use methods with proper validation and feedback.

## Effect Resolution

Current approach:
Effects are defined in card data and can be tracked as relevant, but most effects do not yet execute game-state changes.

Why it is acceptable for MVP:
Tracking establishes the vocabulary and debugging surface before every target system exists.

Production concern:
Effects must eventually produce consistent, testable state changes across Hooked encounters, Descend, Catch Chain, Release, Surface, and future encounter generation.

Revisit trigger:
A gameplay action needs an effect to actually change state, such as replacing an encounter or modifying Line Load.

Likely future action:
Introduce effect resolution services or handlers that execute each `CardEffectType` against explicit runtime contexts.

## Active Catch Effect Tracking

Current approach:
`CatchChainRuntime` adds `WhenCaught` and `WhileAttached` effects to an Inspector-visible active effect list. Releasing a catch rebuilds that list from the cards that remain attached, but those effects do not yet mutate game state.

Why it is acceptable for MVP:
It confirms that Descend can connect caught cards to the effect system before Release, Surface, overload, and full effect targets exist.

Production concern:
One-time effects and persistent attached effects need different lifetimes, clear execution timing, and removal rules.

Revisit trigger:
Surface or a gameplay effect needs to distinguish one-time execution, persistent attachment, expiration, or a specific runtime card copy.

Likely future action:
Move active effects into a dedicated Catch Chain or effect resolver runtime model with explicit effect lifetimes and execution contexts.

## Card View Data Source

Current approach:
`CardView` displays `CardDefinition` data directly, with optional encounter state text.

Why it is acceptable for MVP:
Cards are mostly static so far, and displaying definitions directly is simple.

Production concern:
Once runtime card copies can differ from their base definition, the UI must show instance-specific weight, value, state, and effects.

Revisit trigger:
The game introduces `CardInstance` or any card-specific runtime modification.

Likely future action:
Update `CardView` to display runtime card instances, or provide a read-only view model that can represent either base definitions or modified runtime cards.

## Encounter Selection

Current approach:
The first encounter is chosen randomly from all valid cards in the assigned encounter pool.

Why it is acceptable for MVP:
It proves biome/depth filtering and first encounter reveal without building the full biome system.

Production concern:
The game needs weighted pools, rarity, encounter chains, biome identity, depth-tier evolution, repeat prevention, and Apex handling.

Revisit trigger:
One biome needs to feel replayable and avoid simple random repetition.

Likely future action:
Build a biome encounter system with weighted entries, depth tiers, chain support, special encounter rules, and Apex selection outside normal pools.

## Array-Backed Runtime State

Current approach:
`TechniqueDeckRuntime`, `CatchChainRuntime`, and effect-record collections store their Inspector-visible state as arrays.

Why it is acceptable for MVP:
Arrays are easy to serialize and inspect in Unity while runtime state is small.

Production concern:
Frequent add/remove/draw/discard operations become awkward and allocation-heavy with arrays.

Revisit trigger:
Deck, hand, discard, Catch Chain, or effect state starts changing often during normal gameplay.

Likely future action:
Use `List<T>` internally where state changes frequently, while preserving read-only snapshots or serialized debug views for useful Inspector visibility.

## Surface Overload Resolution

Current approach:
Surface records whether Line Load exceeded capacity, but all attached catches currently return successfully as the haul.

Why it is acceptable for MVP:
The core run-ending flow, haul snapshot, and value calculation can be verified before choosing an overload penalty.

Production concern:
Surfacing while overloaded needs meaningful risk so exceeding capacity creates the intended push-your-luck tension.

Revisit trigger:
Line Load consequences are implemented and playtested during the Catch Chain and Line Load prototype work.

Likely future action:
Resolve the approved overload rule before finalizing the successful haul, then report lost catches, failed effects, or other consequences in the Surface result.
