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
`BiomeDefinition` owns the stable biome identity, three depth tiers, raw `CardDefinition[]` encounter subsets, and short `EncounterChainDefinition` sequences.

Why it is acceptable for MVP:
It is transparent in the Inspector and enough to test changing Coastal encounter composition across Shallows, Mid-depth, and Deep Edge.

Production concern:
Raw tier arrays do not express base encounter weights, rarity rules, or configurable repeat prevention. Chains are separate ordered arrays without branching, probability, cooldown, or validation tooling, while Apex possibilities are a separate unweighted array.

Revisit trigger:
Biome content needs weighted entries, branching or conditional chains, configurable repeat prevention, rare-event rules, or weighted Apex selection.

Likely future action:
Replace each raw tier array with a referenced `EncounterPoolDefinition` that owns validated weighted entries, rarity, repetition rules, chains, and Apex-specific data.

## Non-Catchable Encounter Resolution

Current approach:
Hazard, Environment, and Opportunity encounters provide pacing and can begin authored encounter chains, but their general effect payloads are not executed through a dedicated event context. Treasure encounters are catchable and enter the Catch Chain like creatures.

Why it is acceptable for MVP:
The current behavior makes encounter categories and short sequences testable before introducing another effect lifecycle and its presentation requirements.

Production concern:
Authored event cards such as `Feeding Frenzy`, `Snagged Kelp`, and `Murky Current` can describe effects that remain mechanically inert, which can mislead players and weaken encounter decisions.

Revisit trigger:
Non-catchable encounters must affect the current decision, or playtesting shows that their pacing and chain role is not meaningful by itself.

Likely future action:
Add an explicit encounter-event resolution context with supported triggers, effect handlers, lifecycle rules, and player-facing feedback while keeping gameplay decisions out of `CardView`.

## Prototype Apex Selection

Current approach:
`BiomeDefinition` stores two Apex possibilities outside regular depth-tier pools. `BiomeApexRuntime` uniformly selects one when the run crosses the calculated boundary, tracks its resolution, and routes it through the normal Hooked encounter flow.

Why it is acceptable for MVP:
It guarantees one identifiable climax per Coastal run while preserving existing Technique and Catch Chain rules.

Production concern:
Selection has equal probability with no unlock, rarity, history, or run-build influence. Presentation is currently limited to distinct card labels, Inspector state, and Console feedback.

Revisit trigger:
Playtesting requires weighted Apex odds, unlock rules, richer presentation, rewards, or selection influenced by the current run.

Likely future action:
Add authored Apex weights and eligibility rules, then connect the existing runtime state to final visual treatment, rewards, and biome transition presentation.

## Finite Coastal Boundary

Current approach:
Coastal Waters has regular tiers from depth 0 through depth 7. Crossing into depth 8 reveals one selected Apex; after it is caught or avoided, further biome transition remains unimplemented.

Why it is acceptable for MVP:
The explicit boundary now provides a reliable Coastal climax without requiring a second biome.

Production concern:
After resolving the Apex, continuing to Descend produces no further encounters because the next-biome or end-of-MVP transition is not connected yet.

Revisit trigger:
The Apex needs to carry into a following region or the prototype needs an explicit end-of-biome decision.

Likely future action:
Preserve the caught Apex and Catch Chain while presenting the next-waters transition, or route the prototype into a controlled Surface/end-of-MVP flow.

## Prototype Apex Balance

Current approach:
Harbor Leviathan is an 8 Load, 18 Value Apex that removes 1 Load from every Small catch while attached. Stormjaw Tuna is a 7 Load, 16 Value Apex that adds 1 Load to every Predator catch and gives future Predator encounters 2 additional selection weight.

Why it is acceptable for MVP:
The two cards create immediately different Catch Chain evaluations using effect types already supported by the runtime.

Production concern:
Harbor Leviathan can reduce 1 Load catches to zero, while Stormjaw's attraction effect only becomes relevant after encounters continue beyond the Coastal boundary. Neither card has enough playtest evidence for final values.

Revisit trigger:
Carry-forward playtesting shows that one Apex is consistently safer, more valuable, or more interesting than the other.

Likely future action:
Tune Apex weight, value, tag scope, and effect amounts together after the next-waters flow exists and both cards can be compared across complete runs.

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
Valid encounters receive one base selection ticket, and attached attraction effects modify those tickets. A valid queued chain follow-up takes priority. Otherwise, the selector avoids an immediate duplicate when another definition is valid and caps ordinary-creature streaks at two when a non-creature candidate is available.

Why it is acceptable for MVP:
It makes Catch Chain attraction mechanically testable and prevents the most obvious repetitive sequences while preserving simple Inspector-authored pools.

Production concern:
The fixed two-creature limit treats every biome identically and only remembers enough state for basic safeguards. The game still needs authored base weights, rarity handling, encounter bags or cooldowns, and richer chain rules.

Revisit trigger:
Playtesting needs biome-specific pacing, deliberate rarity frequency, longer repeat memory, or exceptions to the creature-streak rule.

Likely future action:
Move pacing and repetition settings into biome or pool data, then add weighted entries, encounter bags or cooldowns, and richer chain rules.

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
`TechniqueDeckRuntime`, `CatchChainRuntime`, `EncounterVarietyRuntime`, and effect-record collections store their Inspector-visible state as arrays.

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
