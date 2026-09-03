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
Each committed catch becomes a `CardInstance` that references its immutable `CardDefinition` and stores lasting Technique modifiers plus resolved current weight and value.

Why it is acceptable for MVP:
The small instance model safely supports independent weight/value changes while keeping authored definitions reusable and Inspector-friendly.

Production concern:
The instance currently covers only identity, weight, and value. More complex state such as encounter state, temporary effects, disabled effects, and save/load identity is not modeled yet.

Revisit trigger:
Effects need temporary duration, stacking provenance, disabled abilities, player-selected targets, or persistence across save/load.

Likely future action:
Replace the aggregate lasting modifier fields with explicit modifier records and lifecycle state when provenance, removal, or richer stacking rules matter.

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
`BiomeDefinition` owns the stable biome identity, while `FishingRunController` still uses a raw `CardDefinition[] encounterPool` assigned in the Inspector.

Why it is acceptable for MVP:
It is fast to wire, transparent in the Inspector, and enough for manually testing early encounter reveal behavior.

Production concern:
Raw arrays do not express encounter weights, depth tiers, rarity, biome rules, exclusions, encounter chains, or Apex separation.

Revisit trigger:
Biome content needs weighted selection, depth-tier pools, encounter chains, rare encounters, or proper Apex rules.

Likely future action:
Extend biome authoring with a referenced `EncounterPoolDefinition` that owns validated entries, weights, depth tiers, rarity, and Apex-specific data.

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
`TechniqueHandView` presents four clickable slots and forwards use commands to `TryUseTechniqueCard(int handIndex)`. The controller exposes clear playable/unavailable states, while `TechniqueDeckRuntime` handles discard, refill, and discard-pile reshuffling.

Why it is acceptable for MVP:
The complete hand lifecycle and its basic restrictions can be tested through normal Play Mode interaction without requiring final card art, animation, or targeting presentation.

Production concern:
Technique effects are still recorded for later execution, and restrictions currently assume the Hooked encounter is the only target. The hand also needs final card visuals, animation, richer feedback, and interactive target selection where future rules require it.

Revisit trigger:
Technique effects gain selectable targets, timing choices, costs, or more detailed unavailable reasons.

Likely future action:
Separate target-selection rules from the controller and connect the hand to final authored card prefabs, effect previews, animation, and player-facing feedback.

## Effect Resolution

Current approach:
`EffectResolver`, `TechniqueEffectRuntime`, and the focused runtime owners execute attached catch interactions plus the initial Technique categories. A few broader effect enum values remain available without general-purpose execution.

Why it is acceptable for MVP:
The implemented contexts cover the initial Technique categories and existing Catch Chain interactions without introducing a general scripting engine.

Production concern:
Execution is split by practical context rather than a general effect pipeline. More complex durations, costs, stacking, selectable targets, and cross-action effects will need consistent lifecycle handling and feedback.

Revisit trigger:
A card requires multi-turn duration, cancellation, source provenance, player-selected targets, or a new action context.

Likely future action:
Introduce explicit effect contexts or registered handlers as the supported vocabulary grows, without moving gameplay decisions into card views.

## Prototype Technique Semantics

Current approach:
Immediate catch modifiers automatically select the first or last matching catch. `Schooling Rig` modifies all currently attached Schooling catches, and `Double Release` interprets efficient release as dropping up to two catches from the end of the chain in one Technique use. Delayed effects expire on the next matching Descend or encounter-selection attempt.

Why it is acceptable for MVP:
Every initial Technique category has a playable rule and visible result without first building target-selection UI, action costs, a duration engine, or final balance systems.

Production concern:
Automatic targets can remove meaningful choice, Double Release may not be valuable while the always-available Release action has no action cost, and next-action expiration may be too rigid for final card designs.

Revisit trigger:
Playtesting compares Technique cards as strategic choices, or action economy and target selection receive final rules.

Likely future action:
Add explicit target requests, formal effect durations and stacking, and redesign release Techniques after the cost and timing of the core Release action are validated.

## Active Catch Effect Tracking

Current approach:
`CatchChainRuntime` rebuilds Inspector-visible effect records after Catch and Release. Records reference the exact source instance and its current index; the resolver currently executes persistent `WhileAttached` interactions while `WhenCaught` records remain tracking-only.

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
`CardView` displays encounter and Technique `CardDefinition` data directly, with optional concealed encounter details. `CatchChainView` displays resolved `CardInstance` stats.

Why it is acceptable for MVP:
Cards are mostly static so far, and displaying definitions directly is simple.

Production concern:
The two view paths use different data shapes, and a future reusable card presentation will need one read-only model for definitions and instances.

Revisit trigger:
Catch cards move into the reusable `CardView`, or more instance-specific state must be displayed consistently.

Likely future action:
Provide a read-only card view model that can represent either base definitions or modified runtime instances.

## Encounter Selection

Current approach:
Valid encounters receive one base selection ticket. Attached attraction effects add or remove tickets when the candidate matches their required tags.

Why it is acceptable for MVP:
It makes Catch Chain attraction mechanically testable while preserving the current simple Inspector-authored pool.

Production concern:
The game needs weighted pools, rarity, encounter chains, biome identity, depth-tier evolution, repeat prevention, and Apex handling.

Revisit trigger:
One biome needs to feel replayable and avoid simple random repetition.

Likely future action:
Build a biome encounter system with weighted entries, depth tiers, chain support, special encounter rules, and Apex selection outside normal pools.

## Automatic Caught-Card Targeting

Current approach:
Persistent effects targeting `SpecificCaughtCard` automatically choose the previous, next, first, or last matching catch according to authored data.

Why it is acceptable for MVP:
Automatic deterministic targeting makes positional interactions testable without requiring a full selection UI.

Production concern:
The GDD has not locked physical Catch Chain position as a permanent rule, and many future effects should let the player choose a target.

Revisit trigger:
Playtesting rejects positional targeting, or Technique cards require interactive target selection.

Likely future action:
Separate automatic passive target policies from player-selected targeting contexts and add appropriate UI feedback.

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

## Prototype Overload Risk Rule

Current approach:
Overloaded Descend and Surface actions roll a configurable line-break chance. The default starts at 10% plus 8% per excess Load, capped at 65%. A failed check releases one random catch; a successful check lets the line remain overloaded.

Why it is acceptable for MVP:
It creates an immediate, testable push-your-luck consequence without ending the run automatically, and all tuning remains visible in the Inspector.

Production concern:
Randomly losing one catch may be too punishing, too mild, or insufficiently influenced by catch order, equipment, effects, depth, and the amount of overload.

Revisit trigger:
Playtesting provides evidence about whether slight and severe overload create interesting decisions.

Likely future action:
Tune or replace the prototype resolver with the approved consequence model, then integrate equipment, effects, animations, and clearer probability communication.

## Programmatic Catch Chain View

Current approach:
`CatchChainView` creates its compact scrolling UI at runtime and rebuilds entry GameObjects whenever Catch Chain state changes.

Why it is acceptable for MVP:
The panel is self-contained, needs one scene component, and makes ordered catches, stats, and effect tone testable before final card presentation exists.

Production concern:
Runtime-created UI is less convenient for visual authoring, and destroying/recreating every entry causes avoidable allocations as presentation complexity grows.

Revisit trigger:
The Catch Chain receives final visual design, interaction, animation, runtime card instances, or frequent effect-only refreshes.

Likely future action:
Move the entry layout into an authored prefab or UI document, bind it to a catch view model, and reuse entries through pooling or keyed updates.

## Programmatic Technique Hand View

Current approach:
`TechniqueHandView` creates its four card slots, pile counter, status labels, and buttons at runtime.

Why it is acceptable for MVP:
The fixed generated layout makes the hand lifecycle and playability rules testable without depending on final card prefabs or art.

Production concern:
Runtime-created UI is harder to tune visually in the Unity Editor and does not yet provide final card art, animation, responsive targeting, or accessibility feedback.

Revisit trigger:
The Technique hand receives final visual design, additional interaction modes, animation, or platform-specific input behavior.

Likely future action:
Replace the generated slots with authored reusable prefabs or a UI document while keeping `TechniqueHandView` as the presentation boundary.

## Inspector-Loaded Decision Scenarios

Current approach:
`CatchChainScenarioDefinition` assets preload exact Catch Chains, capacity, depth, and a current encounter through a `FishingRunController` context-menu action.

Why it is acceptable for MVP:
Repeatable fixtures make important Release and overload decisions quick to compare without waiting for a random run to produce the required state.

Production concern:
The fixtures bypass normal encounter history and are controlled through the Unity Inspector rather than player-facing game flow.

Revisit trigger:
Automated balance tests, external playtest builds, or reproducible bug reports need scenario setup without the Unity Editor.

Likely future action:
Move reusable fixtures into test utilities or a development-only scenario runner and keep them out of production builds.
