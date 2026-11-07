# Deepwater — Architecture

Design baseline: 6 November 2026. This is the intended responsibility map for the agreed design, not a claim these systems are implemented. Existing class names identify integration boundaries, not completed migrations or required new classes. The existing-code map is preserved in [Architecture Implementation Reference](Architecture%20Implementation%20Reference.md). The [GDD](Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md) governs gameplay.

## Principles

Separate authored data, per-expedition runtime state, permanent progression and presentation. Shared definitions hold no runtime mutations. Runtime owners determine scoring, legal positions, phases and input availability; views display state and forward commands. Use stable instance/encounter identities: array position is not identity or acquisition time. Keep Unity 2D and reusable, data-driven foundations.

## Responsibility map

| Boundary / existing integration point | Intended ownership |
| --- | --- |
| Expedition / FishingRunController | Phase transitions, availability, landing outcome, role/placement commitment, automatic continuation and shutdown |
| Selection / EncounterRuntime | Biome eligibility, approved slot-outcome selection and occurrence identity; no early attachment |
| Slot-machine / TagSlotMachineRuntime, SlotMatch | Three reel faces, exactly-one-lock input, one baseline respin, revision-checked commands, immutable accepted results and tag-only match interpretation |
| Slot selection / SlotCreatureSelector, SlotSelectionSettings | Distinct unlocked biome-wide normal-creature pools, separate rarity roll, tag-preserving nearest-tier fallback (lower-tier ties) and secondary-tag species weights; immutable expedition tuning from SlotMachineProfile |
| Harbor preparation / SlotTagPreparationView, FishingRunController | Explicit four-tag preparation, no mid-expedition edits, frozen expedition symbols/tuning and missing-tag eligibility diagnostics |
| Landing runtime | Fish/zone motion, actions, accuracy, attempts, points, Resistance and terminal outcome |
| Scoring / EffectResolver boundary | Approved accuracy and rig effects, bonuses/multipliers and source attribution; final math awaits design |
| Rig / CatchChainRuntime boundary | Ordered membership, stable identity, roles, legal insertion and removal; preserve existing relative order |
| CreatureEffectRuntime boundary | Selected-role effects, positional/tag dependencies, effect lifetimes and approved reaction sequencing |
| Technique deck/effect runtimes | Physical card ownership, legal windows, targets and lifetimes under redesigned rules |
| EquipmentPassiveRuntime boundary | Approved expedition equipment effects and contribution attribution |
| Progression | Permanent sequential biome unlocks; currency/equipment/achievement ownership only as finalized |
| Results / FishingRunResult boundary | End reason and resolved haul-value snapshot before rig cleanup; full accumulated Catch-value payout on success or failure, zero Bait value, failed creature excluded |
| Views | Authoritative state rendering, feedback and input forwarding; no scoring, placement or unlock decisions |
| Editor authoring | Validate approved definitions, supported effects, biome references and tuning |

Implement responsibilities within existing owners or focused services as appropriate to a complete increment. This does not prescribe a speculative class hierarchy.

## Required phase flow

1. Initialize every expedition in Shallows with an empty rig using preserved permanent unlock/progression evidence. Never restore previous expedition attachments.
2. Discover: use the expedition's four chosen symbols, resolve the initial spin or final optional one-lock respin, then select a biome-eligible creature under the approved tag/rarity rules.
3. Create the creature occurrence and start Land with Resistance, attempts and the existing rig's contributions.
4. Resolve accepted actions through approved accuracy/scoring rules.
5. Landing failure immediately ends the expedition. Capture the full accumulated Catch sale value before rig cleanup; never attach the failed creature. Bait contributes zero. Award the full haul value without a failure penalty, then remove all attachments and clear expedition state while preserving the result snapshot and permanent progression.
6. Success permits Catch/Bait and position selection. Previews must not activate effects or consume pending grants.
7. Validate and commit role/position once for that occurrence. New creature contribution starts after placement.
8. Recalculate positional/ongoing effects and finish reactions before automatic continuation.
9. Evaluate biome/completion steps under finalized criteria; preserve earned permanent unlocks through failure.

Successful expedition completion uses the same full accumulated Catch-value payout and attachment cleanup. Additional permanent completion rewards are separate from haul payout. End-of-expedition cleanup must not trigger deliberate Release effects or erase positional/value contributions before the result is captured. Collected creatures never carry over; unlocking creature availability is distinct from retaining runtime instances. The payout owner must prevent duplicate awards from repeated completion callbacks.

No voluntary Surface action, capacity check or overload-loss branch belongs here. Completion endpoints and event/Technique windows need design before implementation.

## Data and state

Authored definitions describe identity, art, rarity, tags, sale value, Catch/Bait effects, biome eligibility and approved Resistance/movement tuning. Exact schema follows final design. Weight and Heavy remain under review; do not mechanically rename Weight into Resistance.

Runtime state owns phase, occurrence, reels, challenge motion/attempts/points, ordered instances, roles, modifiers and choices. Two copies can differ. Stable IDs must survive insertion/removal without transferring another copy's effects or animation state. All attachments are temporary and are cleared after end-of-expedition value capture on either outcome. Historical result snapshots do not restore creatures into the next rig.

Permanent state owns completed-biome unlocks. If adopted, rewards require explicit evidence/grant ownership and repeat-grant protection. Currency, equipment ownership and achievements are undecided. Save migration is future implementation work; preserve player data until a migration is designed.

## Rig and effects

The rig owner validates insertion before commit and preserves existing relative order. Resolve adjacency, contiguous groups and positional conditions from current membership/effective tags. Both roles participate unless an approved effect excludes one. Removal triggers recalculation; exact Release access/lifetimes need design.

Only the stored role supplies effects. Catch contributes resolved sale value; Bait contributes zero. Newly attached benefits cannot assist that creature's own landing. No manual reorder API or replacement slot cap is implied.

Define every effect's trigger, target, scope, duration, source dependency, stacking and order. Separate ongoing derived bonuses from completed triggers so recalculation cannot award points/value repeatedly. Exact reaction/scoring order remains open. Illustrative Mackerel/Anchored concepts are not final content.

## Presentation and validation

Follow [Card Visual Specification](Card%20Visual%20Specification.md) and [UI Redesign Plan](UI%20Redesign%20Plan.md). UI must not infer legality or delay authoritative scoring for animations.

Future checks must cover stale/duplicate input, failed-creature exclusion, correct contribution timing, preserved order, both-role synergies, removal recalculation, full Catch-value payouts on both outcomes without duplicate awards, zero Bait value, value capture before cleanup, complete attachment removal, persistent progression and empty-rig Shallows starts. Scoring/Technique/event checks need finalized rules. Historical test results do not validate redesigned gameplay.

The [slot increment](Slot%20Machine%20Runtime.md) is implemented in the comparison scene with the approved Discover artwork and live native controls. `EncounterRuntime` holds the selected creature behind `AwaitingLandingChallenge`; `FishingRunController.CreatureReadyForLanding` supplies the occurrence-bound handoff. Slot completion does not authorize attachment, role choice, Techniques or legacy Descend/Surface. Landing execution and its success/failure continuation are separate implementation work; the slot increment stops at that handoff.

The installed Tackle Apparatus presentation uses `TackleDiscoverPresentation` for model-driven reel artwork, shared shell, haul and ordered rig; `TackleRigCardView` supplies separate face/portrait/role/value fields, and `TackleButtonFeedback` owns visual input feedback. `TackleDiscoverArtSetup` is the repeatable import/prefab/scene binding path. The separate 1920 × 1080 canvas appears only during Discover and hides at the landing handoff. Comparison-scene and Harbor regeneration retain the binding. It changes presentation only; see [Unity validation](Validation/TackleDiscoverArt/README.md) for captures, covered controls and missing illustration mappings.
