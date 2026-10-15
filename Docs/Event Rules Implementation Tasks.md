# Event Rules Implementation Tasks

Status: EVT-01 through EVT-08 implemented and available validation executed. Unity execution/rendering checks remain unverified due to the environment failure recorded under EVT-08. All eight tasks are required. Priority indicates dependency order, not optional scope. This plan covers the shared foundation before individual event-card effects are implemented.

Source of truth: [GDD section 9 — Event cards and resolution window](Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md#event-cards-and-resolution-window). Responsibility map: [Architecture](Architecture.md#approved-event-lifecycle-contract). Presentation: [Card Visual Specification](Card%20Visual%20Specification.md#event-interaction-timing).

## Priority and dependencies

| Order | Task | Priority | Depends on | Deliverable |
| --- | --- | --- | --- | --- |
| 1 | EVT-01: Event lifecycle state | P0 | None | Per-occurrence event state independent of the visible card |
| 2 | EVT-02: Reveal, cancellation, and replacement | P0 | EVT-01 | Correct reveal-reaction ordering and distinct terminal outcomes |
| 3 | EVT-03: Legal Technique window and targets | P0 | EVT-01, EVT-02 | Techniques work during events and after cancellation |
| 4 | EVT-04: Event consequence resolution and reactions | P0 | EVT-01, EVT-02 | Resolve consequences once and finish resulting creature reactions |
| 5 | EVT-05: Pending effects and modifier lifetimes | P0 | EVT-04 | Runtime-owned duration/provenance and mode-change persistence |
| 6 | EVT-06: Confirmed Descend/Surface and run cleanup | P0 | EVT-03, EVT-04, EVT-05 | Resolve-before-action, action locking/resumption, and complete cleanup |
| 7 | EVT-07: Run UI and Events Collector alignment | P1 | EVT-03, EVT-06 | Correct input states and event-focused authoring/presentation |
| 8 | EVT-08: Integration verification and documentation | P1 | EVT-02 through EVT-07 | Complete regression evidence and current implementation docs |

Recommended execution order: **01 → 02 → 03 → 04 → 05 → 06 → 07 → 08**. EVT-03 and EVT-04 have the same completed prerequisites and can be developed independently; both must be ready before EVT-06. Add relevant tests during each task; EVT-08 integrates and verifies the complete behavior rather than postponing testing.

## EVT-01 — Event lifecycle state

Status: complete. Implemented EventEncounterOccurrence and guarded EncounterRuntime lifecycle APIs. Validation passed 57 new lifecycle assertions and 488 existing creature assertions (545 total), plus runtime/Editor reference compilation. [Validation evidence](Validation/EventLifecycle/README.md). Unity Play Mode/menu execution remains unverified.

Establish runtime ownership of each revealed event occurrence in EncounterRuntime. Keep that state independent of CurrentEncounter so discarding a cancelled card does not destroy its pre-action window. Distinguish unresolved, cancelled, replaced, resolving, and resolved occurrences; track the confirmed action through the controller when it is introduced in EVT-06. Keep authored CardDefinition assets immutable.

Acceptance checks:

- Each reveal has an independent identity; revealing the same species/card again creates a new occurrence.
- Cancellation/replacement/resolution cannot accidentally execute old consequences.
- An event never becomes Hooked or attaches to the Catch Chain and has no Catch/Bait role.
- Clearing the visible card does not erase the current event's actionable window.
- Existing Creature, Treasure, Apex, and Squid encounter identity/concealment behavior remains valid.

## EVT-02 — Reveal, cancellation, and replacement

Status: complete. Implemented authoritative Hazard cancellation, exact deferred reveal-reaction completion, revision-validated replacement, and callback-safe history/chain setup. Validation passed 41 new assertions plus 545 prior assertions (586 total), with runtime/Editor reference compilation. [Validation evidence](Validation/EventReveal/README.md). Player event Technique input remains EVT-03; Unity Play Mode/menu execution remains unverified.

Run automatic reveal reactions before event consequences, retaining the existing Hazard cancellation and Weathered Shell integration. Distinguish cancellation from replacement and ordinary resolution without a target. Cover normal, authored-chain, and Technique-triggered replacement reveals.

Acceptance checks:

- A cancelled revealed Hazard is discarded without consequences, triggers cancellation reactions once, and preserves the pre-action Technique window after those reactions finish.
- Replacing an unresolved event prevents its effects without awarding cancellation rewards for the replaced event. A replacement Hazard can independently be cancelled when revealed.
- No eligible target resolves without effect; it is not cancellation.
- No automatic extra encounter draw or depth advance is introduced by cancellation or event resolution.
- Mussel protection ordering, per-Lobster rewards, and creature reveal reactions continue to work.

## EVT-03 — Legal Technique window and targets

Status: complete. Enabled event/cancelled-event Technique input with authored target checks, reveal/resolution/action guards, and existing discard/recovery/refill ordering. Validation passed 38 new assertions plus 586 prior assertions (624 total), with runtime/Editor reference compilation. [Validation evidence](Validation/EventTechnique/README.md). Consequence dispatch and confirmed-action coordination remain later tasks; Unity Play Mode/menu execution remains unverified.

Update FishingRunController and TechniqueEffectRuntime together. Replace blanket Hooked-only gating with validation of the active reaction window and each Technique's actual targets/conditions. Support legal attached-card or delayed effects when the event was cancelled, while rejecting effects that require a discarded/nonexistent encounter. Enable unresolved-event replacement where the Technique's rules permit it; do not broaden every Technique indiscriminately.

Acceptance checks:

- Legal Techniques can be played after event reveal reactions and before confirming Descend or Surface, including after Hazard cancellation.
- Technique effects that need an encounter cannot target a discarded event; attached-card effects still use their normal legal targets.
- Played-card discard, resulting creature choices, exact recovery, and normal hand refill retain their established order.
- Technique input stays blocked during automatic reactions and after event resolution begins through completion of the confirmed action.
- Creature/Apex windows and Ink concealment restrictions remain correct.

## EVT-04 — Event consequence resolution and reactions

Status: complete. Implemented validated handler/context contracts and EventEffectRuntime with once-only ordered dispatch, no-target outcomes, reaction-queue suspension/resumption, guarded completion, and non-retryable fault handling. Validation passed 40 new checks plus 624 prior checks (664 total), with runtime/Editor reference compilation. [Validation evidence](Validation/EventResolution/README.md). Production effect implementations follow approved card/duration definitions; controller confirmation remains EVT-06. Unity execution/serialization remains unverified.

Add the shared consequence-dispatch and completion boundary for events, using runtime owners for mutations and the existing creature reaction queue where applicable. Resolve an event once at the default confirmation boundary, with explicitly authored timing exceptions only when an approved card requires them. Complete resulting choices and creature reactions before reporting the event resolution complete.

Acceptance checks:

- The same occurrence cannot resolve twice through repeated input, view refreshes, or resuming a choice.
- Cancelled and replaced occurrences execute no consequences.
- No eligible target completes as resolution without effect.
- Resulting reactions/choices finish before the confirmed action can proceed.
- An event never attaches to the chain or selects a Catch/Bait mode, and no event in this batch forces a Release.
- No Technique opportunity is inserted between consequences and their resulting reactions.

## EVT-05 — Pending effects and modifier lifetimes

Status: complete. Implemented explicit authored lifetimes, pending/applied event ownership, exact positive/negative expiry, epoch/boundary guards, mode-independent recipient provenance, Quick Clean integration, and controller cleanup. Validation passed 45 new checks plus 664 prior checks (709 total), with runtime/Editor reference compilation. [Validation evidence](Validation/EventPersistence/README.md). Named boundaries are emitted according to approved card rules during action coordination; no new timing mechanic or legacy event duration was guessed. Unity execution/serialization remains unverified.

Provide runtime ownership and provenance for event-created pending effects/modifiers that survive the event card's discard. Use each supplied event's stated duration; do not invent new duration rules or finalized effects before their definitions arrive. Reuse existing lasting-modifier/cleansing contracts when their semantics match.

Acceptance checks:

- Discarding the resolved event does not prematurely expire an explicitly lasting effect.
- Target Catch/Bait mode changes do not reset the modifier or its duration.
- Expiry removes only the corresponding effect/modifier, preserving other sources and native stats/tags.
- Lasting/temporary negative event effects participate in Quick Clean under its approved scope.
- Pending effects retain or expire across encounters/biomes according to their authored duration, without mutating shared assets.
- Ownership exposes a complete cleanup boundary for EVT-06.

## EVT-06 — Confirmed Descend/Surface and run cleanup

Status: complete. Implemented validated confirmation, action locking, once-only event dispatch/action execution, creature-choice suspension/resumption, cancellation bypass, fault guards, and immediate runtime cleanup with separate ascent snapshots. Validation passed 45 new checks plus 709 prior checks (754 total), with runtime/Editor reference compilation. [Validation evidence](Validation/EventActions/README.md). Approved card handlers and named duration boundaries remain explicit; no legacy event behavior was guessed. Unity Play Mode/menu execution, serialization, and rendered UI remain unverified.

Integrate the event pipeline into both controller action paths. Confirmation locks in the requested action, closes Techniques, resolves the event and all resulting reactions, then performs that action exactly once. Choices may suspend and resume the pipeline without reopening the Technique window. Clear unfinished event effects and encounter trails on every run-ending path and on new-run initialization.

Acceptance checks:

- Descend resolves the event first, then applies normal action rules, advances depth normally, and reveals the next encounter; there is no additional advance for the event.
- Surface resolves the event and resulting reactions before evaluating the return/haul action and recording results.
- Cancellation still permits legal Techniques until confirmation, then executes the confirmed action without event consequences.
- Repeated action clicks, choice completion callbacks, and view refreshes cannot duplicate resolution or action execution.
- A confirmed action cannot be replaced by another action or interrupted by a new Technique while resolution is underway.
- Natural Surface, line-break endings, and starting another run clear unfinished event state, pending effects, and encounter trails. Presentation-only ascent snapshots must not retain live gameplay effects.
- Existing catch commitment, own on-attach effects, overload handling, biome progression, and creature choice resumption pass regression checks.

## EVT-07 — Run UI and Events Collector alignment

Status: implemented. Independent controller-owned action availability drives run controls; cancelled-event windows remain usable, locked inputs cannot change selection, event faces suppress Catch/Bait feedback, and the Events Collector hides unsupported mode/stats fields while preserving shared asset data. Runtime/Editor reference compilation passed; 20 new input assertions plus 754 prior checks passed (774 total). [Validation evidence](Validation/EventPresentation/README.md). Real Unity prefab/authoring checks are supplied but remain unexecuted here.

Drive Technique/action availability from the new runtime window and action state, including when a cancelled event leaves the encounter region empty. Keep event presentation to name, type, artwork, and description. Align the Events Collector's event authoring fields with the runtime contract; catch-only shared-schema fields must not imply supported event modes or stats.

Acceptance checks:

- Event cards expose no Weight, sale Value, rarity hooks, tag rail, Catch/Bait selectors, or compact chain attachment presentation under the supplied event-master contract.
- Technique and action controls show correct availability before confirmation and remain blocked until the confirmed action completes.
- Required event/reaction choices use the existing modal presentation without adding gameplay decisions to views.
- Collector editing exposes event rules, supported effect definitions, art, availability, and actual pool membership with normal Undo/Redo and saving.
- Unsupported Catch/Bait behavior cannot be mistaken for event behavior in the authoring workflow.
- Supplied card artwork and layout boundaries are preserved; no new card-face runtime badges are introduced.

## EVT-08 — Integration verification and documentation

Status: implementation and available checks complete, with explicit Unity validation limitations. Added 44 end-to-end assertions across all event categories/actions, consecutive Techniques, and authored-chain follow-ups; all 818 runtime adapter assertions pass. Runtime/Editor sources compile against Unity references. Combined Unity menus include production prefab controls, Undo/Redo, save/reload, membership, and three-resolution readability captures. An isolated Unity startup attempt on 2026-10-15 failed for missing headless entitlement and Package Manager IPC, so these real Editor/rendering checks remain unverified. [Validation evidence and remaining checks](Validation/EventIntegration/README.md). No individual event behavior or duration was invented.

Run focused event integration checks and the existing creature regressions. Verify editor/runtime compilation, authoring round trips, and interactive event UI in Unity where available. Update the architecture and implementation brief to distinguish completed behavior from remaining work, and record exactly which validations were executed.

Acceptance checks:

- Cover Descend and Surface with ordinary, cancelled, replaced, and no-target events.
- Cover normal/chain/replacement reveals, multiple legal Techniques, action locking, suspended choices, one-time resolution, normal depth advance, mode changes, expiry, Quick Clean, and run cleanup.
- Include interaction coverage for Mussel/Weathered Shell and existing Technique recovery/refill ordering.
- Verify event authoring save/reload, Undo/Redo, input availability after cancellation, and event-card readability at supported resolutions.
- Existing creature, Catch/Bait, Squid, Apex, and biome regressions remain green.
- GDD remains the gameplay source of truth; implementation docs reflect the final responsibility boundaries.
- Validation reports identify any Unity execution/rendering checks that remain unavailable; compilation alone is not reported as Play Mode verification.

## Individual event cards after the foundation

After EVT-08, implement each user-supplied event definition as a complete asset/effect/validation increment. Reuse the shared lifecycle instead of creating a separate resolution window per card. Exact effects and duration exceptions come from those definitions; this plan does not redesign existing event cards or invent the upcoming batch.
