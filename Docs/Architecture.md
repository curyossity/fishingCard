# Architecture

This document is the living responsibility map for the codebase. It should stay lightweight and practical: update it when class responsibilities, runtime flow, or system boundaries change.

The GDD remains the source of truth for game design. This document explains where the code should put those ideas.

## Core Principles

Creature/Apex attachment choice is controller-owned and keyed to `EncounterRuntime.EncounterRevision`. `CardDefinition.RequiresAttachmentChoice` identifies those encounters. `FishingRunController.TryChooseEncounterRole` validates an explicit Catch/Bait choice, optionally against an expected revision; `CanDescend` rejects unselected occurrences. The view mirrors that state silently and receives independent role-input readiness so disabled Descend does not prevent selecting a panel. Stat refreshes preserve selection; new reveals/replacements invalidate it, including repeated species. Surface/Technique rules are independent. [Explicit-choice verification](Validation/ExplicitAttachmentChoice/README.md).

- Keep gameplay data, runtime state, and presentation separate.
- `CardDefinition` assets describe base card designs. They should not store per-run mutations.
- Runtime systems own run state, encounter state, decks, hands, catches, Line Load, and effect resolution.
- Views display state. They should not decide gameplay rules or mutate card data.
- Core actions (`Descend`, `Surface`) remain outside the player Technique deck. Release requires an explicit card effect; there is no universal free Release.
- Technique cards modify or bend fishing actions; they do not replace fundamental actions.
- Encounter cards represent what the ocean reveals. Creatures, treasures, and Apex encounters can become `Hooked`.
- `BiomeDefinition` assets establish biome identity without storing per-run state.

## Current Classes

### Midnight Field Journal candidate-theme resources

The candidate design is isolated under `Assets/UIThemes/MidnightFieldJournal/`; the existing scene/build entry and old-design defaults remain unchanged. `MidnightFieldJournalTheme` is visual configuration only: project-local sprite/font/palette references, source geometry/provenance and optional artwork overrides keyed by stable `CardDefinition.UniqueId`. Its lookup never changes shared definitions or substitutes another card's art. Runtime consumers are integrated in subsequent component tasks.

`MidnightFieldJournalAssetSetup` owns Editor-only GUID-preserving candidate migration, verified image imports, local static SDF/font fallback preparation and theme configuration. `MidnightFieldJournalPrefabBuilder` lives in this theme's Editor folder and regenerates ten candidate templates, preserving prefab GUIDs and avoiding global asset saves or scene installation. `MidnightFieldJournalAssetValidation` checks import/font/dependency boundaries and uses a disposable preview scene for resource captures. Shared `CreatureCardVisualProfile`, card views and gameplay runtime owners remain outside the theme rather than duplicating game logic. See [UI-02 evidence and remaining boundaries](UIThemes/MidnightFieldJournal/UI02.md).

`JournalControlDesignSync` imports hash-verified control/Technique sources and records native dimensions/alpha bounds, then binds the canonical catalog during normal template regeneration without rebuilding font assets. All eight active Techniques have exact-identity artwork. `LineLoadMeterView` owns only gauge display state, pooled segment layout and load/capacity/status text. `JournalLoadSegment` draws rounded fill, border and highlights natively; capacity determines the segment count, graphical fill saturates, and overloaded numbers remain visible. The HUD still supplies effective load/capacity and owns the separate haul display. `JournalControlSyncChecks` validates source identities, capacity/state transitions, bounds, text and action feedback, with 4K/1080p renders. [MFJ-SYNC-004](UIThemes/MidnightFieldJournal/Sync004/README.md) records the latest control integration and protected-file checkpoint.

### Candidate Catch Chain presentation

`JournalCatchChainScrollRect` owns presentation-only scroll intent. The chain follows the newest attachment after layout changes by default; wheel/drag inspection away from the end suspends following and retains the pixel offset. Returning to the bottom resumes following. A chain that becomes empty or fits the viewport resets the override for the next overflow/new run. Inertia participates in end detection; layout notifications alone never count as manual intent. `JournalCatchChainPresentation` reapplies this policy after measuring its content. [Behavior and actual input verification](UIThemes/MidnightFieldJournal/CatchChainFollow/README.md).

The central candidate card's optional effect-shadow material is assigned by its builder to effect names/descriptions, using Source Serif 4 Regular. The semibold/outline trial was rejected and removed; no runtime override reapplies it. Shared font materials remain untouched. [Shadow trial](Validation/EffectTextShadow/README.md).

`CatchChainView` remains the controller-facing refresh/selection endpoint. Its optional shared `CatchChainPresentation` abstraction delegates layout and rendering while leaving the existing renderer as the serialized default. The Midnight Field Journal chain prefab binds `JournalCatchChainPresentation`, which owns a masked scroll viewport, pooled compact-card views, independent BrassConnectorV3 siblings and optional sibling `LineLoadMeterView` binding. It reads resolved instances and forwards current indices; it never calculates gameplay load, selects legal effect targets or grants Release.

The candidate theme's `CompactPortraitEntry` registry supplies exact-identity compact artwork and independent bounds for fourteen additional approved creatures, reusing their existing central illustration sprites. `TryGetCompactArtwork` preserves the four dedicated compact overrides before consulting this registry. `JournalCompactArtworkSync` restores the registry from the canonical source catalog during prefab regeneration. `JournalCompactArtworkChecks` verifies all eighteen approved creatures, live fields, override precedence and stale-art clearing, with 4K/1080p contact sheets. This presentation-only mapping does not change encounter membership or shared card definitions.

`CompactCatchCardView` supports optional portrait/framing overrides, configurable title case/alignment, role borders and `Graphic` state layers with pointer/navigation focus. Default values preserve the old prefab behavior. `JournalCardOutline` is a theme-owned hollow rounded UI mesh, avoiding baked state artwork and hardware tinting. `MidnightFieldJournalTheme` stores compact portrait bounds per mapped card identity; source definitions remain unchanged. `JournalCatchChainChecks` validates real runtime state and isolated rendering plus a separate actual Play Mode input smoke. See [UI-03 evidence](UIThemes/MidnightFieldJournal/UI03.md).

`JournalCatchTooltip` is the theme-owned, pointer-free inspection view. `JournalCatchInspectionTrigger` relays hover/navigation focus to `JournalCatchChainPresentation`, which owns delayed display, live attachment rebinding, viewport placement and dismissal on scrolling/removal/visibility/modal locks. The tooltip uses the existing tooltip canvas, independent engraved master and native text/tag pills/role emblems. It never resolves effects or exposes hidden sale values. `MidnightFieldJournalTooltipBuilder` and `JournalInspectionDesignSync` preserve this binding during regeneration.

### Candidate projected Technique hand

`TechniqueHandView` delegates to optional shared `TechniqueHandPresentation` while preserving its existing renderer as the default. `FishingRunController` passes the deck's existing HandVersion alongside the hand, availability, reasons, counts and use callback. `JournalTechniqueHandPresentation` owns individual pooled TechniqueCardViews, a detached camera/world-space Canvas, output-sized RenderTexture, exact theme-art lookup and explicit navigation. It uses resolved controller data and never mutates piles or executes effects.

`JournalTechniquePose` owns rest/inspect transforms and stable resting hit planes. `JournalHandRaycaster` maps HUD points to projected graphics and returns actual card hits in normal canvas/modal sort order. `JournalTechniqueButton` rejects pointer clicks whose hand binding changed after press. Offscreen input honors the owning HUD's CanvasGroups. `TechniqueCardView` retains its old transform/disabled defaults, with opt-in external pose and unavailable-card inspection for the candidate. The theme owns optional presentation word breaks; gameplay text stays in shared definitions. `JournalTechniqueHandChecks` validates component content/projection and real Play Mode input/resource lifetimes. See [UI-04 evidence](UIThemes/MidnightFieldJournal/UI04.md).

`JournalTechniqueFan` computes symmetric shallow rest poses for any physical hand count. It matches the approved four-card centers/angles, caps curve/tilt and compresses spacing for larger hands. It does not change gameplay hand limits. The presentation applies matching poses to visible cards and stable hit planes, preserving inspection and occurrence-based input.

### Candidate central encounter presentation

The optional `FishingRunHudPresentation.OpeningCard` exposes the art-only boat to the shared run view. On the first encounter transition, `FishingRunView` temporarily parents that artwork into the outgoing slot's existing wavy mask and motion root. Clearing the controller's opening flag does not hide artwork that is still moving. Completion or cancellation hides and restores the borrowed artwork to its resting parent; restart can reuse it without allocating another boat or altering encounter data. The boat uses the same duration, movement, clipping and input locks as other outgoing encounters. [MFJ-SYNC-006 verification](UIThemes/MidnightFieldJournal/Sync006/README.md).

MFJ-SYNC-002 adds the five active coastal events at the same 1260 by 1400 size. `MidnightFieldJournalTheme.CentralEventEntry` stores exact event artwork, framing, title size and presentation-only word breaks separately from creature/compact entries. `JournalEventDesignSync` hash-verifies imports and restores those mappings during normal regeneration. `JournalCreatureCardVisuals` switches native event category, emblem, rules layout and creature-only decoration when views are reused; `JournalEventEmblem` draws native category symbols. The shared view continues owning content, highlights, information masks and input; no event rules or gameplay data moved into presentation. `JournalEventCardChecks` covers the five-event roster, layout, content and event/creature resets. [Event integration evidence](UIThemes/MidnightFieldJournal/Sync002/README.md) is the latest synchronization checkpoint.

MFJ-SYNC-001 replaces the candidate central layout with the approved 1260 by 1400 detailed card. `MidnightFieldJournalTheme.CentralCardEntry` owns central-only portrait and text geometry for the eighteen refactored species, independently of existing compact/Technique sprite mappings. `JournalCreatureCardVisuals` applies these recorded bounds after the shared view masks information and populates live content. `JournalCardEmblem` draws the source hook, twin spark and rarity diamond as native UI meshes. These objects own presentation only; card definitions and effect resolution remain unchanged.

`JournalDesignSync` imports/hash-checks the canonical source catalog and binds central entries during normal theme regeneration. `MidnightFieldJournalCentralBuilder` authors dynamic fields, selected-role decoration and hit targets. `JournalDesignSyncChecks` covers the eighteen-card roster and pointer/navigation distinction, alongside existing full-encounter, HUD, scene and Play Mode checks. Shared `TechniqueCardView` and `CompactCatchCardView` retain their original hover-frame default, with the candidate opting out of pointer frames while preserving navigation focus. [Synchronization evidence](UIThemes/MidnightFieldJournal/Sync001/README.md) records the exact imported revisions and preservation checkpoint.

`CreatureCardView` remains the controller-facing encounter, transition and ascent-history renderer. Its optional `CreatureCardVisualProfile` controls template geometry, plain numeric stats, separate ability headings, event labels and parent input locks; absent configuration preserves the old renderer. `CreatureCardVisualExtension` supplies theme artwork, permitted metadata and selection decoration after the shared view applies identity/information masks. It owns no encounters, role commitments or effect execution.

`JournalCreatureCardVisuals` resolves exact central artwork, tags and the current rarity name/diamond row. `JournalCardEmblem` and `JournalCardOutline` draw independent native UI decoration; `JournalRarityHook` remains available for historical/other presentation consumers. Catch/Bait buttons notify the existing controller without attaching cards; events suppress all catch-only fields and controls. `JournalCreatureCardChecks` validates all 30 encounter definitions, masks, stats, rules fitting, highlights, portrait framing, legacy asset isolation and rendered Play Mode role input. See [UI-05 evidence](UIThemes/MidnightFieldJournal/UI05.md).

### Candidate gameplay HUD

MFJ-SYNC-005 imports the parchment action/load alternatives, separate opening boat artwork and illustrated depth guide. `JournalDepthGuide` pools `JournalDepthTierView` rows from biome-authored tiers; `JournalDepthNode` draws native markers/halos with an explicit CanvasRenderer. The compact Unity guide preserves text size while clearing the projected hand. All guide objects are presentation-only. `JournalGameplayHud` forwards current biome/depth to that guide and displays the controller's explicit opening state, never inferring a boat from a skipped/empty encounter.

`FishingRunController.beginWithBoatCard` defaults false for existing scenes and is enabled in the isolated comparison scene. When enabled, startup initializes normal run owners/hand but defers encounter reveal; the first normal Descend advances depth and reveals without attaching an invisible creature. `IsOpeningBoatVisible` is forwarded through the optional `FishingRunHudPresentation.SetOpeningVisible` hook. The boat is not a CardDefinition, deck member or attachment. Restart resets this state; later skips do not restore it. [Current integration checkpoint](UIThemes/MidnightFieldJournal/Sync005/README.md).

`FishingRunView` optionally binds authored `FishingRunHudPresentation` surfaces instead of constructing legacy navigation/actions. It continues owning encounter slots, transitions, history, role callbacks, settings requests and command callbacks. Without that serialized binding its prior layout remains the default. Authored role controls honor motion-root/section input locks; the inactive transition slot never accepts input.

`JournalGameplayHud` displays actual biome/tier definitions, depth, draw count, effective Line Load/capacity, nullable haul and learned trail text. `FishingRunController` forwards the sum of existing haul contributions, masking it when an attached Catch's value is concealed. The HUD never reconstructs effective load from card weights or decides risk. `LineLoadMeterView` marks overload only above capacity. `JournalActionFeedback` supplies independent focus/hover visuals; static painted action margins are excluded from the Button hit rectangle.

`MidnightFieldJournalHudBuilder` authors the existing candidate canvas with a 16:9 fitted design viewport, section CanvasGroups, the prepared components, and independent tooltip/transition/modal canvases. `JournalGameplayHudChecks` captures the assembled prefab and exercises actual input without installing or saving a scene. See [UI-06 evidence](UIThemes/MidnightFieldJournal/UI06.md).

### Candidate playable comparison scene and modals

`JournalComparisonSceneBuilder` saves `Assets/UIThemes/MidnightFieldJournal/Scenes/MidnightFieldJournalComparison.unity`. It copies the existing controller setup, EventSystem/input configuration, camera and 2D light, then binds one candidate run view, chain, hand and result view. It does not change the original scene or EditorBuildSettings. Regenerating the scene preserves its GUID; prefab regeneration preserves scene references.

`CreatureChoicePresentation` and `RunResultPresentation` are optional shared rendering boundaries, retaining the legacy choice/result implementations when unset. `JournalChoiceModal` presents the controller's authorized text in pooled, scrolling rows; no legacy portraits or inferred replacements are rendered. `JournalChoiceButton` rejects stale pointer bindings and scrolls focused rows into view. Result lists scroll independently; `JournalResultModal` displays completed snapshots and forwards existing purchase/restart callbacks. An unconfigured Technique unlock stays hidden rather than inventing content.

`JournalModalHost` locks the flat HUD and projected hand through their parent CanvasGroups, keeps one modal visible and restores valid focus. Native parchment-color surfaces and theme-local fonts come from `JournalModalLayout`. `JournalSettingsModal` consumes the pre-existing settings event as a read-only current-display panel; the project has no adjustable settings owner, and none is introduced. These classes own presentation only, never pending game choices or rewards.

Encounter transition completion notifies `FishingRunController` to refresh availability. Core actions and Techniques reject input during encounter motion, and inactive transition slots cannot receive role input. `JournalComparisonChecks` validates saved wiring, regeneration, targeted actual-controller flows, scene reload and a normal run through Surface/results/restart. Missing roster art and final build/performance validation remain explicit. See [UI-07 evidence](UIThemes/MidnightFieldJournal/UI07.md).

`JournalReleaseChecks` owns explicit per-build scene options, build reports and the retirement dependency inventory; it never changes the default build list. `JournalOutputChecks` owns temporary fixed-resolution Game View entries and restores the previous selection. The opt-in `JournalPlayerValidation` harness is compiled only for Editor/development builds, has no ordinary-gameplay startup behavior, and shares output/raycast/focus/idle-render/scene smoke checks between Editor and an explicitly flagged player. `JournalTechniqueHandPresentation.RenderedFrameCount` reports actual projection renders for bounded diagnostics. The canvas supplies a non-interactive navy aspect matte outside its fitted design viewport. See [UI-08 evidence and the blocked standalone build](UIThemes/MidnightFieldJournal/UI08.md).

### Editor integration: Unity MCP

`com.coplaydev.unity-mcp` is a release-pinned Editor package. Its Python MCP server lives in the ignored `.local-tools/unity-mcp/` environment and listens on loopback HTTP. `UnityMcpSetup` is an Editor-only bootstrap/menu command: it starts or reuses that installed server, configures the package's HTTP bridge through its public services, and records connection status under Library. It skips automatic setup on machines without the local Python installation. Gameplay runtime and authored cards do not depend on this integration. Codex registration lives in the user's configuration, outside game data. See [Unity MCP Setup](Unity%20MCP%20Setup.md) for paths, startup, and verification status.

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
- Short authored encounter-chain definitions available in the biome
- Apex encounter possibilities selected when the run crosses the final tier boundary
- The Location card presented after the biome's Apex is resolved
- Optional authored destination biome and the starting depth derived from its shallowest tier

Does not own:
- Current run depth or encounter state
- Per-card biome and depth eligibility
- Encounter weighting or random selection
- Biome progression and unlock rules

### `EncounterChainDefinition`

Role:
Authored data for a short ordered sequence of encounter cards.

Owns:
- Stable chain ID
- Player-facing chain name
- Narrative intent for authoring and review
- Ordered encounter-card sequence

Does not own:
- Whether the chain is active in the current run
- The current position within an active chain
- Encounter validation, selection, or state transitions

### `CardDefinition`

Role:
Base data asset for card designs.

Owns:
- Unique ID
- Display name
- Card type
- Rarity
- Tags
- Canonical creature tags constrained to `Fish`, `Predator`, `Schooling`, `Small`, `Heavy`, `Anchored`, `Armored`, and `Elusive`
- Artwork reference
- Optional central encounter-art scale, rotation, and offset independent of compact Catch Chain portrait framing
- Rules text
- Authored rules-text word/phrase highlights for central-card color and relative size
- Base weight and value
- Optional inclusive random starting-Value range rolled independently for each caught copy
- Biome/depth availability
- Effect definitions

Does not own:
- Per-copy runtime modifications
- Whether one specific card is currently Hooked, Caught, released, or modified
- UI state
- Effect execution

### `CardCatalogWindow`

Role:
Shared editor-only authoring workspace for card catalogs.

Owns:
- Asset discovery under Assets/Cards and Assets/Biomes, name sorting, search by rules/name/ID/type/rarity/tags/biome, and category filtering
- Selection and scroll/filter state retained through editor reloads, with visible selection reconciled after filtering/deletion/type edits
- Serialized editing of supported card-group fields with Undo/Redo, dirty tracking, Save Changes, and Locate Asset; event editing preserves but does not expose Catch/Bait stats or mode data
- Controlled creature/Apex tags and free-form event tags
- Rules-text highlighting metadata and aspect-preserving artwork previews
- Actual biome depth-tier pool membership, including complete removal of duplicate references when unchecked
- Apex possibility membership for Apex cards

Does not own:
- Runtime card state, resolved Weight/Value, encounter rules, or effect execution
- Automatic synchronization between eligibility constraints and pool membership
- Import settings or source artwork generation

### `CreatureCatalogWindow`

Role:
Concrete creature/catch catalog using CardCatalogWindow. Keeps the existing Creature Catalog menus and default Creature scope; All Catch Cards additionally includes Treasure and Apex. Displays Catch/Bait rules, stats, effects, and membership using the shared authoring surface.

### `EventCatalogWindow`

Role:
Concrete Events Collector using CardCatalogWindow. Opens from Fishing Cards/Data/Events Collector or Window/Fishing Cards/Events Collector.

Owns:
- All Events, Hazards, Environments, Opportunities, and Other Events category filters
- Inclusion of Hazard, Environment, Opportunity, and generic Encounter definitions
- Event-focused overview without Catch Weight/Value columns, single event rules/effects sections, free-form tags, artwork and availability editing
- Event-only type selection; encounter rarity/tags remain clearly labelled metadata without face hooks or tag rails
- Handler/lifetime authoring guidance: shared effect enum choices require approved registered handlers; no implied mechanics or legacy duration
- Catch-only serialized fields remain untouched and hidden, rather than editable as event behavior

Treasure, creatures, Apex, player-deck cards, and Location progression cards remain outside this event catalog. Event editing does not add Catch/Bait behavior or new runtime mechanics.

### `CardEffectDefinition`

Role:
Serializable data describing what an effect is and when/where it can apply.

Owns:
- Effect ID
- Optional player-facing ability name
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
- Shared CanDescend, CanSurface, and CanReleaseCatch/CanReleaseSelectedCatch availability for both command validation and view controls; reveal/action/choice locks also block catch selection
- Suspending Descend after attachment until immediate creature choices finish
- Entering an authored destination biome after next-waters presentation without resetting the run build
- Deferring the first new-biome encounter until entry choices finish
- Technique-card entry point for catchable and event reaction windows, including after cancelled-event discard
- ConfirmedEventAction and occurrence identity: preflight locks reentrant input, accepted Descend/Surface stays locked through consequences and queued creature choices, then executes once
- Registering approved event handlers and exposing preflight/dispatch failures; unsupported data leaves the pre-action window open, while partial dispatch failures stay locked until explicit run reset
- Immediate run-ending runtime cleanup after result capture; separate catch/effect/load snapshots preserve ascent presentation without retaining live gameplay state
- Inspector configuration and debug action wrappers
- Loading repeatable Catch Chain decision scenarios during Play Mode
- Optional view refreshes and action summaries

- Binding encounter reveal reservations and complete Hazard cancellation notifications to their runtime owners
- Revealing committed Ink identity before selected-role on-attach effects, forwarding separate information/identity masks to views
- Suspending new actions until immediate skip/release/draw choices resolve; preserving unused encounter concealment on biome entry

Delegates to:
- `TechniqueDeckRuntime` for hand and pile operations
- `TechniqueEffectRuntime` for Technique validation, execution, and delayed effects
- `EncounterRuntime` for encounter selection and Hooked state
- `BiomeApexRuntime` for one-per-run Apex selection and resolution state
- `CatchChainRuntime` for catches, Line Load, and attached effects
- `CreatureEffectRuntime` for ordered creature attachment/biome-entry choices
- `EventEffectRuntime` for validated event consequence dispatch and reaction suspension
- `EventPersistenceRuntime` for pending/applied event ownership, explicit lifetimes, and cleanup
- `NegativeEffectRuntime` for exact removable negative occurrences
- `LineLoadRiskRuntime` for overload checks and tuning
- `EffectResolver` for catch interactions, attraction, and encounter concealment
- `FishingRunResult` for the last Surface result
- `RunRewardRuntime` for Gold conversion and the current wallet
- `RunProgressionRuntime` for the between-run Line Capacity upgrade


Does not own:
- Detailed deck, encounter, Catch Chain, or result operations
- Full effect execution
- Final UI composition

### CreatureEffectRuntime

Role:
Runtime owner of ordered creature attachment and later-biome choice requests.

Owns:
- Run-local biome visit ordinal and attachment-origin stamps
- Immediate selected-role attachment choices
- Pre-entry creature Catch instance-ID snapshots
- Sequential later-biome Bait swap requests in chain order
- Offers generated once per request and skipped without two distinct eligible creatures
- Replacement on-attach choices resolved before the next queued Bait source

- Binding independent Ink grants to EncounterRuntime rather than the source card lifetime
- FIFO next-creature skip grants, once-per-card activation, original encounter revision validation, and immediate skip choices
- Optional Elusive Bait release choices and per-released-creature Eel eligibility snapshots
- Shared Eel draws deferred until the entire release action and prior Release choices finish, checking observing Bait survival before drawing
- Optional hand discards completed before any shared release draws, with four-slot capacity
Does not own:
- Encounter pool eligibility/weight calculation or normal encounter draws
- Catch collection mutation, negative-effect removal, or presentation

### NegativeEffectRuntime and NegativeEffectOption

Role:
Collect exact removable active negative occurrences across attached effects,
lasting applied stat modifiers, Hooked records, queued Technique effects, exact event pending/applied lifetime occurrences, and
exact independent pending/current Ink concealment occurrences.
Options delegate removal to each owning runtime; they do not mutate authored
assets. Completed information/actions and base statistics are not candidates.

### CreatureEffectChoiceView

Role:
Reusable modal-layer presentation for negative-effect choices and two-stage
Catch/replacement selection. Owns scrolling option rows, typography, portraits,
input blocking, and callback forwarding. Eligibility, ordering, and resolution
remain in runtime owners. Choice information does not alter card-face layouts.

### `EventEffectRuntime`

Role:
Serializable owner of an event's validated consequence plan and once-only dispatch cursor. It exposes Ready, WaitingForCreatureReactions, Completed, and Failed progress without owning depth, haul results, or a confirmed core action.

Owns:
- Registration of approved IEventEffectHandler implementations by effect type; no implicit mapping from legacy event descriptions to new behavior
- Whole-plan validation before claiming the current unresolved/reveal-complete occurrence; unsupported, malformed, or rejected effect data leaves the event window open and executes nothing
- A structural snapshot of the validated effect array, current occurrence identity/source, dispatch cursor, and applied/no-target counts
- Guarded ordered effect calls, claiming each cursor step before invoking effect code to prevent callback reentry or duplicate execution
- Waiting for CreatureEffectRuntime's queue to become empty before dispatching another effect or completing/discarding the event
- Current event/runtime-owner validation; coincident IDs from another run cannot address the active plan
- Fault state that preserves closed input and rejects retries after a handler partially applies mutations or throws
- Reset of unfinished dispatch state; coordinated EncounterRuntime reset aborts the owned occurrence at run ending

Delegates to:
- EncounterRuntime for consequence claim, lifecycle guards, final resolution/discard, and Technique-window closure
- IEventEffectHandler implementations for specifically approved effects, targeting, timing, duration validation, and mutations through existing runtime owners
- CreatureEffectRuntime for resulting reaction/choice queue state; callers prepare and finish that queue before resuming dispatch

Does not own:
- Event-specific mechanics not yet supplied or approved
- Confirming/resuming Descend or Surface; FishingRunController owns the confirmed action and resumes it after the reaction queue completes
- Named duration boundary emission and specific card rules; shared lifetime ownership is provided by EventPersistenceRuntime
- Presentation or choice UI

### `IEventEffectHandler`, `EventResolutionContext`, and result enums

Role:
The handler contract validates an authored effect before execution and resolves it once as Applied or NoEligibleTarget. Handlers queue resulting creature work in the existing owner and do not reopen Techniques, attach the event itself, or grant forced Releases for this batch. EventResolutionContext binds one occurrence to the run's encounter, chain, creature, deck, resolver, and random owners. Context reconstruction is permitted for the same owners; restored serialized dispatch requires runtime setup to rebind its approved handlers and context. This rebinding simulation is tested, but Unity serialization round trips remain unverified.

EventEffectRuntime is usable as a complete dispatch boundary independently of controller actions. Production event handlers are supplied with approved individual card definitions; no legacy card behavior was guessed or enabled by this task. Unknown effect types fail preflight with a reason rather than silently disappearing.

### `EventPersistenceRuntime`, `PersistentEventEffect`, and `EventEffectLifetime`

Role:
Per-run owner of event-created pending effects and recipient stat modifiers after the event card is discarded. The lifetime descriptor is explicit: until run cleanup (or an applied target leaving the chain), or a named card-authored boundary counted a positive number of times. No generic encounter/biome expiry rule is inferred.

Owns:
- Independent monotonic effect IDs, source event occurrence/definition/effect provenance, pending/applied state, recipient instance IDs, and copied lifetime counters
- Explicit pending claims or transfer to an existing recipient without restarting lifetime
- Named boundary notifications with monotonic tokens, duplicate rejection, and run epochs rejecting old cleanup/expiry callbacks
- Exact positive/negative expiry and negative-effect cleansing without reversing other sources or native/untracked mutations
- Pruning removed/exchanged recipients and modifier records removed through existing APIs; historical snapshots are never mutation targets
- Resetting pending effects, undoing live target contributions, clearing boundary history, and invalidating old epochs at run cleanup

Integration:
- CardEffectDefinition exposes EventLifetime only when hasEventLifetime is enabled. Legacy definitions have no inferred event duration. The collector's recursive effect editing exposes this descriptor; supported boundary names/counts must come from approved card definitions and handlers.
- CardInstance.AddEventModifier stores an event effect ID on immutable AppliedCatchModifier provenance. TryRemoveTrackedModifier supports exact positive expiry as well as negative removal; legacy TryRemoveAppliedModifier retains its negative-only contract. Recalculation and mode changes retain event contributions and their counters.
- NegativeEffectRuntime collects the persistence owner's pending/applied options once, avoiding duplicate recipient provenance. Unowned tracked negative modifiers remain cleanseable through the existing modifier path. Event persistence options delegate exact-ID removal to their owner.
- FishingRunController binds one persistence owner to its chain/resolver at setup, includes it in Quick Clean, and resets it before clearing the chain on new runs, debug scenario replacement, and run-ending cleanup. Haul/loss snapshots retain their resolved values.
- EventResolutionContext requires the run's explicit persistence owner; context reconstruction must reuse it. The dispatch engine checks this owner alongside the other runtime owners. Approved effect handlers choose a target and lifetime explicitly rather than inventing timing inside this store.

Duration boundary emission and concrete effect handlers are supplied by action coordination/approved card definitions. This task adds the complete ownership and expiry boundary, not an automatic new gameplay clock or guessed legacy event mechanic. Unity serialization round trips remain unverified; rebuilding modifier provenance by stable IDs is covered by rule checks.

### `TechniqueDeckRuntime`

Role:
Runtime owner of the Technique hand and card piles.

Owns:
- Filtering the configured deck to Technique cards
- Fisher-Yates shuffling
- Starting hand draw
- Removing used cards from the hand
- Separate resolving-card ownership with a reserved occurrence ID; discard only after its effect finishes, then controller completion after resulting creature choices; no automatic replacement draw
- A global four-card limit for setup, effect draws, and recovery; exact discard-occurrence recovery without duplicating physical cards
- Reshuffling the discard pile when the draw pile is empty
- Draw-pile and discard-pile state
- Clearing deck state when the run ends

Does not own:
- Whether a Technique card can affect the current encounter
- Technique effect execution
- Player input or card views

### `TechniqueHandView`

Role:
Interactive presentation of the current Technique hand, capped at four by the runtime owner.

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
- Validation of legal attached-card/delayed effects during event windows, including after cancellation, without requiring a Hooked encounter
- Authored HookedEncounter versus CurrentEncounter targets, effective attached-target restrictions, and missing/discarded encounter rejection
- Backend entry checks preventing Technique execution or queuing during reveal reactions or closed/resolving/resolved windows
- Immediate avoidance, replacement, and information reveal
- Queued next-Descend and next-encounter effects
- Temporary capacity, depth, committed-catch, and overload-reward resolution
- Removing exact queued negative occurrences and retaining resolved stat-effect provenance
- Clearing one-use effects after their matching action

Delegates to:
- `TechniqueTargetPlan` for validated player-selected targets before commitment
- `CatchChainRuntime` for eligible-target enumeration and exact selected-target modification and release
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
- Sampling distinct eligible ordinary-creature offers without advancing normal encounter/history state
- Encountered, Hooked, Caught, and None state transitions
- Current Hooked encounter
- Current EventEncounterOccurrence, monotonic per-runtime occurrence IDs, event-only classification, and lifecycle ownership independent of the visible card
- Guarded event consequence claim/completion and cancelled-window closure; stale or terminal occurrences cannot claim effects
- Nested reveal callback phases and exact deferred-reaction tokens; the pre-action window opens only after callbacks and all registered reactions finish
- Guarded explicit Hazard cancellation and encounter-revision-validated replacement without mutating failed replacement state
- Selection/history/chain bookkeeping established before reveal observers; new reveals rejected before bookkeeping during active consequences
- Hooked effect records and Technique-effect relevance
- Active encounter chain and the index of its next queued card
- Prioritizing a valid queued chain card before normal weighted selection
- Applying an explicitly supplied encounter through the normal state transition rules
- Activating a preselected Apex through the normal Hooked state

- Independent pending Ink occurrences and a single assigned concealment per ordinary creature reveal
- Exact pending/current Ink cleansing and commitment/removal consumption, excluding Apex/non-creatures
- Monotonic encounter revisions preventing skip offers from acting on replacements
- Complete cancelled-Hazard notifications after discard, and creature reveal reservations before the reveal-effect window
- Resetting encounter state at biome entry while preserving unused concealment; full new-run reset clears it

Delegates to:
- `EncounterVarietyRuntime` for repetition filtering, streak state, and sequence diagnostics


Does not own:
- Catch Chain commitment after a Hooked card is taken
- Depth progression
- Encounter pool authoring or attraction-weight calculation
- Repetition history or filtering policy
- Effect execution

### `EventEncounterOccurrence` and `EventOccurrenceState`

Role:
Serializable per-reveal event state owned by EncounterRuntime. Public properties expose identity, authored definition, Unresolved/Cancelled/Replaced/Resolving/Resolved state, reveal-reaction completion, and whether the pre-action window is open. It has no Catch/Bait role, Weight, sale Value, or attachment state.

Lifecycle:
- SetCurrentEncounter creates a new occurrence only for Hazard, Environment, Opportunity, or generic Encounter; the same definition revealed twice gets different IDs.
- Full Hazard cancellation records Cancelled before cancellation observers run. After nested reveal callbacks and all deferred reactions finish, that occurrence retains a pre-action window even though CurrentEncounter is null.
- Clearing visible presentation alone preserves current event ownership. A new non-null encounter supersedes unresolved consequences, closes the previous window, and establishes independent ownership.
- TryBeginEventResolution claims only the current unresolved, reveal-complete occurrence, returning its authored definition and closing its window. TryCompleteEventResolution accepts only that current Resolving ID, records Resolved, and discards the visible event.
- TryCloseCancelledEventWindow closes a cancelled occurrence without changing its outcome or allowing effects. Duplicate/stale calls return false.
- Reset clears the current occurrence and closes its window but keeps the ID counter monotonic, including biome resets, so old callbacks cannot address new-run events.
- New non-null encounters cannot replace an event while it is Resolving. TryDeferEventRevealReaction registers exact runtime tokens during reveal/cancellation callbacks; TryCompleteEventRevealReaction accepts each token once for its original current event. Callback depth and pending tokens prevent early reopening, including nested cancellation. Future action coordination owns confirmation, consequence dispatch, and action resumption.

This is lifecycle state infrastructure. EVT-03 enables legal Technique input using HasTechniqueReactionWindow in both the controller and effect backend. EVT-04 supplies the validated consequence dispatcher and creature-queue completion boundary. EVT-05 provides pending-effect/modifier lifetime ownership and Quick Clean/cleanup. EVT-06 coordinates confirmed Descend/Surface through those owners before performing the action.

### `BiomeApexRuntime`

Role:
Runtime owner of one biome's one-per-run Apex selection and resolution state.

Owns:
- Detecting the calculated boundary after all finite biome tiers
- Uniform random selection from valid biome-authored Apex possibilities
- Selected Apex and valid-candidate diagnostics
- Hooked, Avoided, Caught, and Unavailable Apex states
- Updating the selected Apex when a Technique replaces it
- Whether the post-Apex next-waters card has been presented

Does not own:
- Hooked encounter behavior or Catch Chain commitment
- Technique-card rules
- Apex effects after the card enters the Catch Chain
- Changing the active biome or resetting run-owned systems

### `EncounterVarietyRuntime`

Role:
Runtime owner of lightweight encounter repetition safeguards and recent sequence diagnostics.

Owns:
- Immediate-repeat prevention when another valid card is available
- Capping ordinary-creature streaks at two when a non-creature candidate is available
- The most recent eight selected encounters
- Inspector-friendly sequence and applied-rule summaries

Does not own:
- Encounter eligibility or weighted selection
- Authored encounter chains
- Biome pool composition or final pacing rules

### `CatchChainRuntime`

Role:
Runtime owner of catches attached to the line.

Owns:
- Catch acquisition order
- Creating an independent `CardInstance` for each committed catch
- Adding and releasing catches
- Snapshot histories for voluntarily released and overload-lost catches
- Calculated Line Load from resolved instance weights
- Active caught-card effect records
- Rebuilding and resolving catch interactions after Catch or Release
- Applying authored `WhenAnotherCatchReleased` self-value reactions after deliberate player or Technique releases
- Consuming authored `WhenNextMatchingCatchCaught` effects once and applying their lasting modifier to the first later catch whose tags match
- Enforcing attached-card release locks for player and Technique releases while leaving involuntary line-break losses valid
- Consuming one-encounter attached effects after their affected encounter resolves
- Invalidating conditional attached effects when another catch is deliberately released later
- Building a non-mutating resolved preview of a possible catch against the current Catch Chain
- Catch Chain reset and snapshots
- Per-instance cleansing followed by persistent-effect recalculation
- Replacing a creature Catch in place without Release reactions, old modifiers, or release/loss accounting

- Lasting per-Lobster Catch Value awards on fully cancelled revealed Hazards; the resolver owns live other-Armored-creature support
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
- Released and overload-lost catch snapshots
- Total resolved haul value
- Gold awarded for the completed run
- Surface depth
- Surface Line Load and capacity
- Whether Surface began while overloaded

Does not own:
- Overload consequence rules
- Gold conversion, selling choices, or progression
- Run-summary UI

### `RunRewardRuntime`

Role:
In-memory owner of run-reward conversion and the current Gold wallet.

Owns:
- Configurable Gold awarded per point of resolved haul value
- Gold awarded by the most recent completed run
- Total Gold accumulated during the current play session
- Recording the award on `FishingRunResult`
- Validated wallet deductions requested by progression systems

Does not own:
- Haul-value calculation
- Saving currency between play sessions
- Purchase eligibility, shops, unlocks, or other progression choices

### `RunProgressionRuntime`

Role:
In-memory owner of the minimal between-run Line Capacity upgrade and Technique deck progression.

Owns:
- Upgrade cost, capacity increase, and purchase limit tuning
- Number of purchased Line Capacity upgrades
- Total cumulative Line Capacity bonus for future runs in the current session
- Validation that a purchase happens after a completed run, at most once per run, with enough Gold
- Purchase summaries and Gold-spend requests
- A session-saved Technique deck initialized once from the authored starting deck
- The configured Technique unlock, its cost, and unlocked state
- Appending an unlocked valid Technique card without duplicating its stable card ID

Does not own:
- The Gold wallet
- Current-run Line Capacity
- Save/load persistence
- Deck-size limits, card removal/replacement choices, or broader progression content

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
- Displaying the supplied blank Catch Rig heading with independently localizable TextMeshPro text
- Displaying catches in current acquisition order
- Clearly separated current weight and value labels, including modifiers
- Optional passive-effect icon when the catch has an active tracked effect
- Distinct visual treatment for explicitly negative effects
- Continuous authored brass rod tiled through the chain viewport
- Separate authored ring, rotated clip, and anchor medallion instances for every catch row
- Independent catch-card frames separated from the rig and neighboring cards
- Catch selection feedback and forwarding the selected index to the controller
- Supplied blank Line Load panel positioned below the scrolling Catch Rig contents
- Runtime-owned Load / Capacity label and one pip per capacity slot
- Supplied empty, filled, and danger pip sprites for stable, approaching-limit, and overloaded visual states
- Rapid presentation-only shake on red Line Load pips while the line is overloaded, with stable positions restored immediately when overload ends
- Scrollable layout for longer chains
- Empty Catch Chain presentation

Does not own:
- Catch Chain data or ordering rules
- Effect classification or execution
- Release validation or execution
- Line Load calculations

### `MaritimePanel`

Role:
Reusable presentation shell for framed run-interface regions.

Owns:
- Dark teal, ivory, turquoise, and coral interior presentation variants
- Aged-brass and oxidized-teal material-border selection
- Stable serialized content padding
- Optional header text, header icon, and four corner ornament slots
- Applying supplied nine-sliced frame artwork without recoloring it

Does not own:
- Gameplay data or interaction rules
- Child-content layout beyond its safe content root
- Panel-specific labels, values, controls, or state transitions

### `RunResultView`

Role:
Presentation component for the latest completed run outcome.

Owns:
- Hiding the result overlay while a run is active
- Showing Gold earned and the current wallet total
- Showing Surface depth, Line Load, capacity, and overload status
- Separate brought-home, released, and lost catch lists
- Showing Line Capacity upgrade progress and purchase availability
- Showing the configured Technique unlock, rules, cost, and saved deck size
- Forwarding progression and next-run commands to the controller

Does not own:
- Haul snapshots, value calculation, or reward conversion
- Gold persistence, spending rules, or upgrade rules
- Starting-run logic

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
- The independently rolled base Value for cards with an authored random catch-value range
- Attachment biome-visit origin and per-copy removed effect indices
- AppliedCatchModifier records retaining each lasting stat effect's source and exact contribution for cleansing
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
- Recalculating self-Value bonuses based on the number of other attached catches matching authored tags
- Selecting a previous, next, first, or last matching catch target
- Adding tag-based attraction weight to future encounters
- Reporting persistent encounter-information concealment

Does not own:
- Catch Chain collection mutation
- Encounter state transitions
- Technique-card execution
- Final effect balance or player-selected targeting

### `CardView`

For events, the fallback view leaves rarity, stats, tags, and encounter-state fields empty; only identity, type, art, and description are presented. Production CreatureCardView uses the supplied event master and suppresses Catch/Bait interaction and feedback, including programmatic role selections.

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

### Approved event lifecycle contract

This is the required contract for the upcoming event-card implementation. EVT-01 implements per-occurrence lifecycle state and guarded transitions. EVT-02 implements reveal reaction completion, explicit Hazard cancellation, and guarded replacement paths. EVT-03 enables legal player Techniques in event windows, including after cancellation. EVT-04 supplies consequence dispatch through registered approved handlers, waiting for creature reactions before completion. EVT-05 supplies explicit lifetime tracking and precise expiry/cleanup. EVT-06 integrates confirmed actions, queue resumption, input locking, and immediate shutdown cleanup. Production handlers still require approved individual card definitions; unsupported legacy effect data fails preflight before confirmation. EVT-07 supplies controller-owned input availability and event-only collector fields. EVT-08 supplies combined runtime regressions and Unity validation/capture commands. Unity execution, authoring round trips, and screenshots remain unverified here because isolated editor startup cannot obtain its headless entitlement.

1. EncounterRuntime owns the revealed event and its per-occurrence lifecycle. Resolve automatic reveal reactions, including full Hazard cancellation and creature cancellation rewards, before event consequences. Events never become Catch Chain attachments and have no Catch/Bait role, Weight, or sale Value.
2. FishingRunController keeps the pre-action Technique window open after reveal reactions, even when cancellation has discarded the visible card. TechniqueEffectRuntime validates each Technique against its own legal targets/conditions; an event window must not require a Hooked card.
3. Confirming Descend or Surface commits that action and closes Technique input. Unless the event specifies another timing, resolve its consequences once, then finish all resulting creature reactions before performing the committed action. Do not reopen Technique input between event resolution and action completion.
4. Full cancellation of a revealed Hazard discards it without effects and emits Hazard-cancellation reactions such as Weathered Shell. Replacement prevents the unresolved event's effects without emitting cancellation for that event; a replacement reveal still follows its normal rules. No eligible target is resolution without effect, not cancellation. No event in this batch forces a Release.
5. Discard a resolved event without attaching it. Its pending effects/modifiers belong to runtime state for their stated durations; target mode changes do not reset them. Descend advances normally and reveals the next encounter with no extra advance for event resolution. Surface performs event resolution before the return/haul action.
6. Run shutdown clears unfinished event effects and encounter trails. Authored CardDefinition assets never store per-run lifecycle or modifiers; views only render state and forward decisions.


Current run startup flow:

1. `FishingRunController.StartRun()` initializes run state.
2. `TechniqueDeckRuntime` filters and shuffles the starting deck, then draws the hand.
3. The current biome and depth are set.
4. `BiomeDefinition` supplies the encounter subset for the starting depth tier.
5. `EncounterRuntime` selects the first valid encounter and updates reaction state:
   - `None` when no encounter exists.
   - `Encountered` for non-catchable encounter cards.
   - `Hooked` for catchable Creature, Treasure, and Apex encounters.
6. If the selected encounter starts an authored chain, `EncounterRuntime` queues its next card.
7. Optional `CardView` references are refreshed.

Current Descend flow:

1. `FishingRunController.TryDescend()` validates action availability. For an event, it confirms Descend, resolves consequences and resulting creature choices, then enters the normal action path. Cancelled events close their reaction window without consequences; unsupported effects reject confirmation without closing the window. No event resolution advances depth.
2. `EncounterRuntime` returns the Hooked card for commitment, if one exists.
3. `CatchChainRuntime` creates a `CardInstance` in the selected Catch/Bait role, resolves any pending next-matching-catch bonuses onto it, appends it, and tracks its catch-related effects. `CreatureEffectRuntime` stamps its biome origin and queues immediate attachment choices. Descend pauses here until those choices finish; completed cleanup effects are not tracked as ongoing passive effects.
4. `TechniqueEffectRuntime` consumes pending Descend effects for catch stats, distance, capacity, risk, and overload rewards.
5. `EffectResolver` recalculates catch interactions from each instance's lasting base state.
6. Current Line Load updates from the resolved instance weights.
7. `LineLoadRiskRuntime` checks for strain using any one-use capacity bonus.
8. If the break check succeeds, `CatchChainRuntime` randomly loses between one catch and half of the attached chain, records those cards as Line Strain losses, and the run ends immediately.
9. A line-break ending snapshots the result and surviving chain for the shared accelerated Surface replay, then immediately clears active runtime owners and trails. Presentation snapshots retain resolved stats and copied effect-record sources; replay completion only clears those snapshots.
10. If the line holds, current depth advances using any one-use distance modifier.
11. `TechniqueDeckRuntime` retains the remaining hand. Descend does not refill it; only explicit card effects draw or recover Techniques.
12. `BiomeDefinition` supplies the tier pool at the effective selection depth.
13. At the depth after the biome's final finite tier, `BiomeApexRuntime` selects one valid Apex possibility exactly once.
14. `EncounterRuntime` reveals that Apex as Hooked; otherwise it first reveals a valid queued chain card or filters the regular tier subset using card availability.
15. If alternatives exist, repetition rules remove the immediately previous regular card and interrupt an ordinary-creature streak after two reveals.
16. Attached effects and one-use modifiers determine the remaining regular candidates' selection weights.
17. A normally selected chain starter queues its next authored card.
18. The next valid encounter is revealed; persistent concealment can hide its details.
19. `CatchChainView` rebuilds from catch instances, effects, current Load, and capacity.

Current Apex flow:

1. Crossing the biome boundary asks `BiomeApexRuntime` to select one valid authored Apex possibility.
2. `EncounterRuntime` presents the selected card as a Hooked encounter, and `CardView` uses distinct biome-Apex labels.
3. Technique validation continues through the normal Hooked reaction window; replacement draws from the other Apex possibilities.
4. Avoidance records the Apex as Avoided and prevents another boundary selection during the run.
5. Descend commits the Hooked Apex through `CatchChainRuntime.Add(...)` and records the Apex as Caught.
6. The resolving Descend advances normally after catch effects and overload risk unless a line break ends the run.
7. The biome-authored next-waters Location card is then presented once without resetting the Catch Chain, Line Load, active effects, or Technique piles.
8. With an authored destination biome, the next Descend enters it at its starting depth, resets only encounter/Apex state, and resolves ordered biome-entry choices before the first normal reveal. Without an authored destination, Descend stops at the boundary; Release and Surface remain available.

Current biome-entry choice flow:

1. CreatureEffectRuntime snapshots original pre-entry creature Catch instance IDs and queues later-biome Bait sources in chain order.
2. EncounterRuntime samples two distinct eligible ordinary creatures with normal encounter weights without consuming normal encounter/history state. Requests with no original Catch or fewer than two offers are skipped.
3. CreatureEffectChoiceView forwards the player's Catch and replacement selections through the controller.
4. CatchChainRuntime replaces the selected creature in place, ending its ongoing effects without firing Release reactions or copying modifiers.
5. The replacement's immediate Catch choices resolve before the next queued Shrimp. New replacements are not eligible again at the same entry.
6. The first normal encounter is revealed after the queue finishes; run build, capacity, and Technique piles persist.

Current Technique-card use flow:

1. `TechniqueHandView` forwards a slot's `USE` command to `FishingRunController`.
2. The controller validates the reaction window and collects each required target through the shared choice modal. Cancelling spends nothing. The card remains in hand until all selections validate against current hand version, encounter revision, tags, targeting immunity, and Release restrictions.
3. `TechniqueDeckRuntime.TryBeginPlay` removes the physical card from hand into resolving ownership, reserving its eventual discard occurrence ID. It cannot be recovered or reshuffled during its effect.
4. `TechniqueEffectRuntime` executes immediate effects against the exact selected targets or queues authored delayed effects. A target removed by an earlier effect is not replaced by an automatically selected recipient. Targetless effects emit no attached-target selection.
5. `CatchChainRuntime` applies lasting per-copy changes and recalculates persistent interactions when required. Encounter replacement performs a weighted reveal while excluding the replaced card.
6. `TryCompletePlay` moves the resolving occurrence to discard, then the controller finishes deferred Release and Technique reactions. Cod can recover that exact occurrence; Sea Urchin recovery follows the other reactions. Both offer discard-one-or-decline when a four-card hand is full.
7. After the entire ability and all creature choices finish, unlock the remaining hand without drawing replacement cards. Descend also does not refill the hand. Only explicit draw/recovery effects add cards; explicit draws reshuffle remaining discards when needed and respect the four-card cap. Techniques and core actions remain blocked throughout resolution and choices; a partial failure remains blocked until explicit run replacement clears it.
8. The controller refreshes the hand, piles, encounter, Catch Chain, and playability states.

Granted Release flow:

1. There is no universal free Release. The retired public controller command rejects direct requests; the player-facing control is hidden. Descend and Surface remain permanent actions.
2. A Technique, creature, or event validates its own legal recipients and grants deliberate Release through CatchChainRuntime. Iron Grip and targeting restrictions still apply.
3. Removal forfeits the attachment's Weight, Catch-role Value, and ongoing effects. Recalculate surviving cards; preserve completed effects with independent durations.
4. Cut Loose defers survivor Release reactions and notifications until its physical card enters discard. CreatureEffectRuntime then resolves Cod, Eel, Ray and other queued choices before Urchin recovery and input reopening.
5. Release does not itself advance depth or resolve the current encounter. Forced losses and exchanges remain distinct from deliberate Release.

Current Surface flow:

1. `FishingRunController.TrySurface()` validates availability and, for an event, confirms Surface and resolves consequences and creature choices first. Only then does it record the starting Surface load. New Techniques, Release, and alternate core actions remain blocked throughout the confirmed pipeline.
2. `LineLoadRiskRuntime` checks for strain when the line is overloaded.
3. A successful break check randomly loses between one catch and half of the attached chain; a held check preserves the overloaded chain.
4. `FishingRunResult` snapshots the remaining haul, released catches, and overload-lost catches, then records value, depth, starting Line Load, capacity, overload state, and whether a line break forced the ending.
5. An unresolved Hooked encounter is excluded because it has not entered the Catch Chain.
6. `RunRewardRuntime` converts resolved haul value to Gold, adds it to the in-memory wallet, and records the award on the result.
7. The controller captures separate presentation copies of the surviving chain, effect records, and load, ends the active run, and immediately clears event dispatch, pending effects/modifiers, encounters/trails, creature choices, Catch Chain, and Technique-deck state.
8. The accelerated Surface replay uses the presentation copies. Replay completion clears only these copies; a new run also clears them before initialization.
9. `RunResultView` presents the reward and separate brought-home, released, and lost lists; line-break endings are identified explicitly, and the same details remain Inspector-visible and appear in the Console summary.

Current between-run progression flow:

1. A completed Surface result opens `RunResultView` with the wallet total and Line Capacity upgrade state.
2. The player may start the next run immediately or request one Line Capacity purchase.
3. `RunProgressionRuntime` validates completed-run state, the once-per-run limit, maximum purchases, and available Gold.
4. `RunRewardRuntime` deducts the configured cost after validation succeeds.
5. The result view refreshes with the new wallet total and cumulative capacity bonus.
6. Starting the next run adds the cumulative progression bonus to the authored starting Line Capacity without resetting purchased upgrades.
7. The player may instead spend Gold to unlock the configured Technique card; the runtime appends it to the session-saved deck once.
8. Starting a later run shuffles the saved deck, so the unlocked card can enter the normal hand, discard, and reshuffle lifecycle.

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

Authored gameplay data:
- `BiomeDefinition`
- `EncounterChainDefinition`
- `CardDefinition`
- `CardEffectDefinition`

Runtime state:
- Current encounter state
- Hooked encounter
- Active encounter chain and next sequence index
- `EncounterVarietyRuntime` history and consecutive ordinary-creature count
- `BiomeApexRuntime` boundary, selected Apex, and resolution state
- Hand/draw/discard piles
- Catch Chain
- Per-copy current catch weight and value
- Per-copy lasting Technique weight and value modifiers
- Pending next-Descend and next-encounter Technique effects
- Line Load
- Last overload-risk check
- Last Surface result
- Released and overload-lost catch histories
- In-memory Gold wallet
- In-memory Line Capacity upgrade state
- Session-saved Technique deck and unlock state
- Future active effect instances

Presentation:
- `CardView`
- `CreatureCardView`
- `CompactCatchCardView`
- `TechniqueCardView`
- `RunActionButton`
- `CatchChainView`
- `TechniqueHandView`
- `RunResultView`
- `FishingRunView`

`FishingRunView` owns the reusable root gameplay prefab and its stable presentation regions: background, top navigation, Catch Rig, current Encounter, run controls, Technique hand, tooltips, transitions, and modals. Its root Canvas scales from 1920 x 1080 with equal width/height matching. The background is one user-supplied complete 1920 x 1080 nautical-chart image with all gameplay UI layered above it. The top navigation uses one complete edge-to-edge frame displayed at 1920 x 80. Three supplied blank plaque sprites remain separate fixed-size children: the region plaque displays the active biome, the depth plaque displays current depth in metres, and the deck plaque displays the remaining Technique draw-pile count. Their TextMeshPro labels are runtime-owned. A decorative compass sits beside the region plaque, and a deck icon remains inside the deck plaque; neither owns state. The gear icon is a Unity `Button` with standard focus and color transitions that raises `FishingRunView.SettingsRequested` without owning settings behavior. Warning treatments remain separate children or overlays. Screen-wide warning, release, and modal feedback remain separate overlays. Major gameplay regions expose independent visibility through `CanvasGroup` state, while tooltip, transition, and modal canvases have deterministic render order above gameplay.

The top navigation remains the final base-canvas sibling so moving Encounter cards pass behind its frame during Descend transitions. Ordered tooltip, transition, and modal canvases retain their explicit higher sorting contracts.

The right status region uses the supplied 416 x 748 outer frame, 416 x 256 Depth Zone subsection, and 416 x 108 Tension subsection. `FishingRunView` renders biome-authored tier names and numerical ranges, tiles the supplied 8 x 160 track between the configured zone positions, and instantiates one supplied active/inactive node sprite per tier. The node and text emphasis follow the tier containing the current depth. The three gameplay action buttons remain inside the outer frame. Tension is presentation-only line strain derived from current Line Load divided by capacity; it does not introduce a separate gameplay resource or alter overload resolution. Its supplied 352 x 40 track remains constant, while `Image.fillAmount` controls the supplied 338 x 24 fill and swaps that fill from teal to coral at the warning threshold.

Descend, Release, and Surface use their supplied plate and icon pairs as reusable Unity buttons. Labels and selected-catch numbering remain runtime-owned. `LinkedGraphicButton` extends the standard `Button` color transition to the icon and label so hover, controller selection, press, and disabled state remain synchronized without baking those states into the base artwork. Release keeps its destructive coral plate; Surface keeps dark label text against its ivory plate.

It renders state supplied by `FishingRunController` and forwards player commands without resolving gameplay rules. Descend, Release, and Surface each receive independent controller-owned availability, including during cancelled-event windows with no visible card. Disabled command callbacks do not forward input; Descend also retains its presentation transition guard. Technique slots receive their own legal-target availability and restriction reasons. The current Encounter region uses two reusable `CreatureCardView` presentation slots so Descend can slide the outgoing card upward while the incoming card rises from below without allocating a new card each time. Catch cards expose their supplied Catch and Bait effect panels as mutually exclusive click targets, defaulting to Catch for each newly presented encounter. `CreatureCardView` owns the selected/unselected feedback and reports `EffectChoiceChanged`; `FishingRunView` forwards the active card's choice as `EncounterEffectChoiceChanged`. `FishingRunController` reads that choice when Descend commits the Hooked encounter and passes the corresponding `CatchAttachmentRole` into `CatchChainRuntime`. A procedural wavy stencil changes the actual top-to-bottom reveal silhouette, and a separate soft mask feathers the card itself without drawing a visible wave overlay or making the entire card suddenly translucent. The inactive slot holds no independent gameplay state, and the transition is triggered only by a supplied depth increase. For presentation only, `FishingRunView` records one immutable encounter snapshot per visited depth. Surface continuously replays the previous catch-card snapshots from deepest to shallowest with linear reverse card motion and the wave fade, skipping event-card snapshots and accelerating each successive transition to a capped speed without settling on individual cards, then raises `SurfaceTransitionCompleted`; `FishingRunController` keeps `RunResultView` hidden until that signal and refreshes it afterward. These snapshots do not preserve or restore gameplay state. The region does not repeat encounter state, tags, or statistics outside the card. `FishingRunViewPrefabBuilder` is the editor-owned assembly path for the prefab, supplied complete background, scene installation, project sorting layers, and static responsive captures.

`CatchChainView`, `TechniqueHandView`, and `RunResultView` are installed in their owning prefab regions and continue to build their internal dynamic content. Their model/controller boundaries are unchanged; the prefab owns placement and render-layer context.

`TechniqueHandView` places the supplied 1920 x 494 `fishing-ui-technique-hand-pad-1920x494.png` behind its dynamic title, pile counts, and four card slots. The pad is a Simple `Image` anchored bottom-center and uses width-driven aspect fitting, so narrower layouts scale it uniformly rather than stretching the artwork. The owning `CanvasGroup` continues to control whole-hand visibility and interaction.

The hand view also supports separate 1536 x 1024 decorative clusters for the lower-left map/dividers and lower-right rope/compass. Each is a non-raycast Simple Image at a uniform 3:2 display ratio, layered above the pad but below card content. Missing optional decoration bindings are omitted cleanly; both inherit whole-hand visibility and opacity from the owning CanvasGroup.

`CreatureCardView` is a serialized layered prefab that composes the supplied blank creature base, hidden stencil artwork mask, optional approved overflow layer, safe title/stat/effect regions, hollow sliced frame, exactly four rarity sockets, and a four-socket tag rail. Runtime fields use TextMesh Pro and update the type, name, resolved Weight, resolved Value, the built-in Catch and Bait effect panels, and one-to-four filled rarity anchors without changing authored geometry. Both effect panels currently repeat the definition's existing rules text as placeholders; the view does not infer separate gameplay effects. The tag rail maps authored card tags to supplied icon sprites, fills recognized tags in canonical order, and leaves unsupported or unused sockets empty. Each populated icon is a hover target that presents its authored tag name through the run's ordered, non-interactive tooltip layer. A complete user-supplied card face remains supported and replaces the generic base/artwork/frame combination; global hidden-information effects conceal dynamic details in place, while a self-concealing card replaces only its Value with `?` until results, without adding a visible state label.

`CompactCatchCardView` is the Catch Rig's serialized horizontal card. Its normal surface is the supplied `fishing-ui-catch-chain-card-master.png`; Unity places the catch name, creature artwork, resolved Weight and Value, an optional passive-effect icon, and a small `BAIT` marker driven by `CardInstance.AttachmentRole` into the authored presentation. A self-concealing card displays `?` instead of its Value for the rest of the run when that concealment belongs to its selected role. It deliberately has no full rules field. `CatchChainView` instantiates one compact-card prefab per attached creature beyond the separate rig connector gutter. The view owns a stable left-side rig attachment transform, click forwarding, pointer hover, and visually distinct selected, disabled, and release-candidate layers. It does not decide whether a catch may be selected or released.

`CardInstance` owns the per-copy attachment role (`Catch` or `Bait`) chosen at commit time. `CatchChainRuntime` builds active records only from `CardInstance.ActiveEffects`, which selects exactly one definition-owned effect array. Both roles retain their resolved Weight for Line Load. `CardInstance.HaulValueContribution` is the canonical reward boundary: it returns the resolved Value for Catch attachments and zero for Bait attachments, and result/reward/presentation consumers use it instead of raw `CurrentValue`. Role state is preserved in chain snapshots and run results. Refactored definitions set `hasExplicitBaitData`, making empty role arrays and text authoritative. Unrefactored definitions retain legacy Bait fallback until their approved redesign arrives.

`CreatureCardView` is the production current-Encounter card. It selects the supplied 1122 x 1402 dual-effect catch master for `Creature`, `Treasure`, and `ApexEncounter`, and the supplied 1101 x 1429 event master for every non-catchable encounter category. Event presentation hides the catch-only Weight, Value, rarity, tag rail, and Bait effect region, switches text colors for the ivory title field, and expands the Catch rules region to the event master's single rules panel. Unity renders each card's explicitly authored `EncounterArtwork` (falling back to `Artwork`) either inside the chart-field mask or as a transparent full-card overlay. `CardDefinition.ArtworkLayout` selects the artwork path without runtime size heuristics, while `Artwork` remains the cropped image used by compact-card consumers. Legacy precomposed per-creature faces do not replace these masters.

`TechniqueCardView` is a serialized portrait card with a fixed slot root and a separately animated visual root. It owns masked equipment artwork, TMP name/rules/keyword fields, normal/hovered/selected/playable/disabled presentation, and forwarding a use command. Hover elevation is bounded inside the visual root; hand layout and effect resolution remain outside this component. Playable and disabled states carry explicit text/icon cues in addition to color and frame changes.

`RunActionButton` is the reusable Descend, Release, and Surface command surface. It owns theme icon/label selection, fixed-geometry sprite feedback for normal/hover/pressed/disabled/dangerous states, controller focus presentation, and forwarding a button command. It does not determine action availability or execute gameplay rules.

`CatchChainView` renders catches in acquisition order and reports the selected catch index to `FishingRunController`. Selection is controller-owned because Release validity and execution are gameplay concerns, while the view owns only the selected visual treatment.

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

It may replace the raw tier arrays currently embedded in `BiomeDefinition` when weighted authoring, repetition rules, and dedicated Apex selection are implemented. Encounter-chain references can remain biome-owned or move into this asset if later authoring workflows benefit from pool-specific chains.

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

### Deliberate Release and recovery coordination

`CatchChainRuntime.DeliberatelyReleased` reports each completed deliberate removal (player, Technique, or creature) with its exact released instance. Forced losses and swaps bypass this event. `CreatureEffectRuntime` owns immediate Release choices, a shared encounter recovery-consumed flag, and a deferred recovery request. It records eligibility while the source is attached, consumes the opportunity even with no discard, and exposes the request only after the releasing ability completes. A completed recovery trigger survives source removal. `FishingRunController` resets this flag only on normal encounter advancement; Technique replacements never reset it. The controller suspends actions until queued choices finish, then reopens input without an automatic draw. `TechniqueHandView` forwards the actual hand index; the deck owner enforces four cards. Full-hand Cod recovery reserves its exact discard occurrence before offering discard-one-or-decline.

### Effective tags, independent attachment penalties, and deferred redraws

- `CardInstance` owns effective tags, an optional per-copy base-tag override, and frozen role-effect target selections by instance ID. Snapshots copy these arrays and selections. Shared definitions retain native tags and authored effects.
- `EffectResolver` rebuilds base tags and applies surviving tag grants before any tag-dependent stat calculation. `RequiredTagsMatchInstance` matches attached copies; definition matching remains for encounter selection. Swift counts only other creature Catches. Selected weight reductions run after other weight changes, stack, and clamp at zero; tag loss suspends their eligibility without discarding selected target IDs.
- `CatchChainRuntime.TrySetCreatureBaseTags` immediately recalculates attached tag mutations. `CatchChainRuntime` establishes tag grants before attachment triggers. It owns independent serialized `PendingCreatureCatchEffect` records for completed on-attach penalties, applies them only to the next creature Catch (including a replacement attachment), and transfers each to a recipient's lasting modifier provenance. Source removal does not cancel the queue. Previews read the queue without consumption and bind active source records to snapshot copies. Sequential Technique removals rebuild effective tags between targets.
- `NegativeEffectRuntime` exposes exact pending and applied negative occurrences, so Quick Clean handles Tangle at both stages.
- `CreatureEffectRuntime.RecordAttachment` queues immediate attachment choices, then one deferred redraw evaluation against original surviving Bait sources. This runs after on-attach choices, checks the recipient's effective tags, skips an empty hand, and coalesces all Minnow copies. Replacement attachment reactions precede later biome swap sources. It also owns the one-time optional Buoyant Bell selection and freezes target IDs on completion.
- `FishingRunController` blocks other gameplay actions while these choices resolve and resumes Descend/biome entry afterward. `TechniqueDeckRuntime.TryRedrawTechnique` discards one exact hand occurrence and draws exactly one card without filling other empty hand slots; it never plays the discarded Technique.

### Predator releases, queued encounters, and captured Value transfers

- CardEffectDefinition.TargetTags separates release-target tags (Small) from trigger tags (Predator). New enum entries append after existing serialized values.
- CardInstance records consumed one-time role-effect activations and copies them into snapshots. School Call spends activation on first Bait attachment even if declined, ineligible, or blocked by an Apex.
- EffectResolver applies the flat Graceful Glide reduction after ordinary modifiers and evaluates Elusive creature Bait from live roles/tags. Packed Shoal sets final Weight to zero after all other modifiers when two other Schooling creature Catches qualify. Both effects operate only on Catch sources.
- CatchChainRuntime records Venomous Spines as exact lasting negative modifier occurrences. Each deliberate creature removal takes only the available Value up to 5, avoiding debt at zero. Bait sources, forced removals, and exchanges do not apply the penalty.
- CreatureEffectRuntime.RecordAttachment groups Lionfish releases and Minnow redraws by reaction type using original surviving Bait sources. Immediate on-attach choices finish first, then reaction groups follow their first source's chain order. Lionfish offers one optional legal Small-creature Release, excluding the just-attached Predator.
- CreatureEffectRuntime captures released Elusive Catch Value from the exact removed instance, whose final stats/tags remain intact when survivors recalculate. Ray copies coalesce per removal. FinishReleaseResolution exposes separately captured transfers only after the whole ability finishes. Completed triggers survive later source removal; recipient bonuses use lasting modifier provenance. Only remaining Catch-role attachments can receive them.
- NextEncounterDestination previews the next selection depth/pool without advancing depth or consuming modifiers. FishingRunController includes pending Descend-distance and encounter-depth modifiers and reserves Apex/next-waters priority before offering School Call.
- CreatureEffectRuntime owns one queued species independently of source lifetime. New selection overwrites it; decline preserves it. Normal reveal consumes and revalidates it against actual pool, Schooling tag, biome/depth, and scheduling. Invalid selections fall back to the ordinary draw.
- EncounterRuntime.OfferCreatures filters required native tags and draws distinct ordinary creature species using normal effect-modified selection tickets without consuming encounter history. RevealSelectedCreature uses normal Hooked/on-reveal state, history, and authored chain setup at normal reveal time.
- FishingRunController exposes explicit choice actions for optional Predator release, next-encounter selection, and captured Value transfer. Existing creature-choice presentation forwards these choices and blocks other actions while they resolve.

### Shell protection, sacrifice draws, and technique targeting/exchanges

- CardInstance owns serialized protection initialization and availability, copied independently into snapshots. SetAttachmentRole preserves permanent modifiers while expiring unused Bait protection; returning to Bait never refreshes it. This is a runtime mutation boundary for approved effects, not a new player mode-switch action. CatchChainRuntime.Recalculate rebuilds selected-role active records before resolving tags and stats.
- CatchChainRuntime.TryCancelHazard checks only Hazard classification and consumes the eligible protection with the oldest attachment instance ID, independently of replaced chain slots. EncounterRuntime.HazardCancellation is rebound by the controller at runtime setup and intercepts every SetCurrentEncounter before active state/effect records, covering normal, authored-chain, and Technique-replacement reveals. Cancelled draws retain selection history and expose LastCancelledHazard, leave no active encounter, and do not request another reveal or depth change. The approved event contract additionally requires preserving the pre-action Technique window after this visible-card removal; an empty encounter reference must not imply that the window is closed.
- CreatureEffectRuntime groups original source IDs for post-attachment reactions, resolving on-attach choices before chain-ordered reaction groups. Shell Growth automatically applies one lasting bonus per surviving Mussel Catch and uses the newly attached creature's effective Armored tag. Bait attachments and non-creatures do not trigger growth.
- Feeding Frenzy presents one shared optional legal-source selection. A successful deliberate creature Release queues a source-independent DrawTechniques request behind all resulting recovery/transfer requests. It then offers optional exact hand discards while two draws cannot fit four slots. Drawing consumes only the requested count that fits, including after oversized recovery; the suspended Descend resumes after the whole queue without refilling the hand. Other-effect Releases and forced losses never activate this sacrifice.
- TechniqueDeckRuntime.TryDiscardTechnique does not play or refill. DrawTechniques owns exact capacity-limited draws and normal reshuffling. TryExchangeTechniques validates both existing indexed occurrences before atomically exchanging them, preserving hand size and draw pile state. Duplicate definitions remain separate occurrences.
- Sheltered Exchange records its first hand selection in the runtime request without moving it, then presents the existing discard pile. Both selections validate at completion; decline moves nothing. Multiple Anemone sources coalesce into one immediate opportunity.
- CardInstance.PreventsTechniqueTargeting depends on current Catch role and active Stinging Tentacles. CatchChainRuntime excludes immune attachments from individual Technique targeting, including release targets. Untargeted distributed ModifyCaughtCard effects, delayed Descend effects, core Releases, and creature targeting retain their existing paths.
- FishingRunController exposes explicit sacrifice, optional discard/draw, and two-stage exchange actions. Existing creature-choice presentation shows legal options and decline choices, blocks other gameplay actions, and resumes suspended actions after completion. New effect enum entries append after prior serialized values.

### Simultaneous releases, exact played recovery, and ordered role activation

- CatchChainRuntime.TryReleaseTogether validates all distinct selections before mutation, snapshots every selected instance's pre-group tags/stats, removes the whole group, and recalculates ongoing state before survivor Release reactions. Each removal dispatches its own snapshot, preserving simultaneous values and excluding observers removed with the group. CreatureEffectRuntime exposes resulting choices only after the completed group removal.
- CreatureEffectRuntime owns mandatory hand discards and one-time optional Small-creature multi-selection. It records all selections before removal and spends activation even when skipped/declined. Immediate Octopus draws use the existing draw request with a one-discard limit; Sardine retains its own multi-discard policy.
- EffectResolver applies the non-stacking Bristling Spines penalty before selected reductions, using live attached creature tags in either role. CardInstance stores removed effect keys by role/index and migrates legacy removed indices before role changes. Snapshots copy these keys independently.
- TechniqueDeckRuntime gives every discard entry a serialized occurrence ID. Use/discard/exchange/recovery update definitions and IDs together; reshuffling invalidates discarded occurrences. Exact recovery validates both the played token and its definition, avoiding substitution of an identical copy. Existing Cod recovery still selects a discard index.
- CatchChainRuntime.TechniqueTargetSelected reports actual individual targets before modification/removal. Untargeted distributed effects emit no selection event. FishingRunController captures play-time effective-tag eligibility and the played discard token before effects, then appends exact recovery behind all resulting choices. Technique completion waits for the queue to finish; it never draws a replacement.
- CreatureEffectRuntime stores FIFO recovery grants independently of Urchin lifetime, using authored required tags. At most one grant is consumed per qualifying play. A missing played occurrence or declining still consumes it. Full-hand recovery permits one discard; available-space recovery returns only the exact token.
- CatchChainRuntime.TrySwapCreatureModes atomically changes an attached creature Catch/Bait pair, preserving IDs, positions, permanent modifiers, and pending effects. Creature eligibility governs current mode legality; Release locks do not become mode locks. Lifecycle keys include role, with legacy keys associated with the pre-swap role before mutation.
- Helping Arms queues two deferred role activations: new Catch first, then new Bait only if still attached in the expected mode. ActivateRoleOnAttach runs only the card's own immediate chain effects, and RecordAttachment skips shared after-catching reactions for a role change. Pending entry effects follow their own rules: Prize Hook qualifies on Catch entry and Upwelling on Bait entry; ordinary next-attachment triggers are not consumed. New ongoing effects are already recalculated after both role changes.
- Existing one-time Bait abilities retain their activation keys across mode swaps. Crab release and frozen Jellyfish target selection spend their first Bait activation even without targets. Mussel protection expires when leaving Bait, while independent Tangle/Spiny Grip queues and completed Value bonuses retain their authored durations.
- FishingRunController presents mandatory discard, simultaneous release selection/finish, two-stage mode pairing/decline, and exact recovery discard/decline through the existing creature-choice modal. Source-role validation prevents stale on-attach requests from operating in a different role. Gameplay actions remain blocked until the ordered queue completes. New effect enum values append after existing serialized entries.

### Event reveal, cancellation, and replacement integration

- EncounterRuntime.TryCancelRevealedHazard accepts only a current Unresolved Hazard ID. It discards presentation, preserves the Cancelled occurrence, and dispatches HazardCancelled exactly once; non-Hazards, stale IDs, Resolving/Resolved, and already-Cancelled occurrences are rejected. Existing Mussel protection invokes this path before the event reveal-effect callbacks; Weathered Shell observes the discarded Hazard and updated cancellation state.
- EventEncounterOccurrence tracks nested reveal callback depth and independently identified pending reveal reactions. Reveals/cancellation notifications can register deferred tokens while callbacks run. Completion rejects stale occurrence IDs, wrong tokens, and duplicate completions, and cannot release another pending reaction. No pre-action window or consequence claim opens until all callbacks/tokens complete.
- EncounterRuntime.TryReplaceEncounter validates the current encounter revision, target, eligible event window/Hooked state, and existence of an alternative before cancelling the old chain or drawing. Failure leaves the original occurrence/chain intact. A successful reveal supersedes unresolved consequences without emitting cancellation for the replaced event; a new Hazard reveal may independently be cancelled.
- TechniqueEffectRuntime routes replacement effects through this API, including unresolved event targets in its effect backend. FishingRunController now validates HasTechniqueReactionWindow and supports this event replacement path when the Technique targets CurrentEncounter. HookedEncounter-only Techniques retain their catchable target requirement.
- Normal, authored-chain, selected-creature, and Apex selection history/chain state is committed before reveal observers run, preventing returning callbacks from restoring an old chain after replacement. Cancelled draws remain in normal selection history without extra draws/depth changes. Reveal entry points reject active event resolution before mutating selection state.
- An event completing without eligible targets follows the Unresolved → Resolving → Resolved path without a cancellation notification; actual event consequence dispatch is EVT-04.

### Event Technique windows

- EncounterRuntime.HasTechniqueReactionWindow permits a Hooked catchable encounter or an open event occurrence, including a cancelled occurrence with no visible card. A runtime callback-depth guard closes Technique input during nested synchronous reveal callbacks; deferred event reaction tokens keep it closed afterward until completion.
- FishingRunController.CanUseTechniqueCard also rejects pending creature choices, suspended Descend/biome entry, and an unresolved Technique completion. The completion-pending flag is set before applying a consumed Technique, preventing a new Technique from interrupting effect/recovery processing. Choices and exact recovery finish before input reopens; no automatic refill follows. Legal plays can continue using the remaining hand in the same event window.
- TechniqueEffectRuntime validates its own open-window boundary for both validation and execution. Attached-card mutations use the existing chain's target/restriction checks; delayed effects can be queued even after Hazard discard. Effects requiring CurrentEncounter or HookedEncounter reject missing targets and apply their authored required tags. Replacement targeting CurrentEncounter supports unresolved events; Hooked-target replacement and avoidance retain catchable-target restrictions. Information reveal requires a visible target and hidden information, and the legacy information-only effect does not bypass Squid Ink. Clear Water explicitly ends the assigned concealment.
- Cancelled, resolving, and resolved event outcomes have different availability: cancellation retains a pre-action window after reactions, but its discarded card is not a target. Consequence claim closes the window; resolving/resolved occurrences accept no new Techniques. FishingRunController coordinates confirmed Descend/Surface through EventEffectRuntime and the creature queue; input stays locked until the action completes.
- No authored Technique card or effect enum was changed, and no new event avoidance rule was introduced.

### Squid concealment presentation boundary

`CreatureCardView` and legacy `CardView` accept separate information and identity masks. Identity concealment replaces the name, removes both art layers, tags/tooltips and rarity markers, and masks stats/both rules while leaving Catch/Bait selection available. `FishingRunView` carries both masks through standby transitions and encounter-history snapshots. Views never consume charges or decide skips. Legacy Technique reveal applies only to the prior information-only concealment contract. Clear Water explicitly ends one assigned concealment, including Ink, without consuming future charges.

## Event foundation verification

EventPresentationChecks validates controller input gates independently of a visible card. Its Editor-only checks instantiate the production prefab and exercise serialized event edits, Undo/Redo, disk reload, and actual pool membership in disposable validation assets. Readability capture uses an isolated preview scene, preserving the open scene, and checks text overflow at 1920×1080, 1366×768, and 1280×800. EventIntegrationChecks adds complete two-Technique/confirmed-action sequences for all four event categories and authored chain follow-ups. Its combined Unity menu runs event and coastal suites, Editor checks, and screenshots; Cleaner Shrimp has its separate asset/modal validation menu. See Docs/Validation/EventIntegration/README.md for executed evidence and remaining Unity checks.

## Approved coastal event implementation

CoastalEventHandlers registers five approved effect types at controller setup/reload without replacing explicit registrations. EventResolutionContext carries the current biome/depth and binds the shared persistence owner for event choices. Legacy shared effect types remain unsupported unless explicitly registered.

CreatureEffectRuntime owns source-independent EventSnagTarget and EventCreatureRelease requests. Snag auto-resolves a unique heaviest eligible creature or waits for a mandatory tie choice; Loose Knot waits for an optional legal creature Release. Both reuse the existing modal and confirmed-action suspension. Event removals use a distinct deliberate Event reason, bypass Technique-only targeting/recovery, and still dispatch normal Cod/Eel/Ray/Lionfish reactions.

CatchChainRuntime reports BeforeAttachmentEffects after new attachment or simultaneous Bait mode entry, before own on-attach execution. EventPersistenceRuntime applies all pending Upwellings at that boundary and supplies a non-consuming preview. CardInstance retains exact modifier provenance; EffectResolver adds combined event weight contributions after ongoing creature interactions, flooring at zero once. This keeps stacked reductions from being lost to an earlier base-stat clamp and makes Snag add to current effective Weight.

Reef Snag uses the explicit BiomeEnd boundary. FishingRunController emits it on completed-biome next-waters presentation or biome entry, with duplicate-safe visit tokens. Surface/load/result capture precedes normal run-end cleanup. Pending Upwellings and Silt concealment survive biome entry.

EventEncounterTrailRuntime, owned by EncounterRuntime, stores one promised species/biome/destination and a consumed-slot preview. It validates ordinary slots and actual species pool membership, retargets skipped depths using eligible tier intervals, yields to existing queued choices, and never creates a second draw. Random and replacement candidate filtering excludes ineligible trail events. Controller reveal arbitration reserves Apex priority before consuming a due promise. Replacement or avoidance marks a consumed promise replaced; run/biome cleanup clears it. Authored encounter chains remain separately owned, and a trail event cannot create a promise while one is active.

FishingRunView displays the controller-supplied trail preview outside card faces, independent of concealment. It retains learned name/normal stats while reporting reached/replaced slots. Supplied card masters remain unchanged. New event-specific illustrations are missing inputs, not generated placeholders.

CoastalEventChecks supplies runtime integration checks plus Unity asset/placement checks. The combined EventIntegrationChecks menu includes this batch. See Validation/CoastalEvents/README.md for executed checks and Unity limitations.

## Approved Technique rules implementation

GDD section 10, Approved Technique rules, is the source of truth. The shared runtime implements the following contract; individual new Technique designs still require authored effect data and appropriate handlers.

Required ownership and order:
1. Validate the reaction window and collect player-selected required targets before commitment; validate target identity, current eligibility, Stinging Tentacles, and Release restrictions at commit.
2. TechniqueDeckRuntime must remove the exact physical card from hand into separate resolving ownership. It must remain absent from discard and reshuffle candidates until its effect is finished.
3. TechniqueEffectRuntime resolves the effect and any work needed to finish that effect. The controller keeps other Techniques and core actions blocked through the whole play/reaction sequence.
4. Move the resolving occurrence to discard, then finish reactions waiting for Technique completion. Cod may select the just-used discard occurrence; exact Sea Urchin recovery follows the other triggered effects. A recovered occurrence cannot be duplicated or replaced by an identical definition.
5. Complete explicit effect draws and deferred reactions according to the card rules before reopening input. Playing a Technique and Descend never automatically refill the hand. Keep all hand mutation paths within four cards. Apply only card-authored modifier durations; run shutdown clears pending effects. No new energy resource or once-per-biome counter is introduced.

Ownership (2026-10-15):

| Concern | Owner | Implemented behavior |
| --- | --- | --- |
| Played-card location | TechniqueDeckRuntime | TryBeginPlay / TryCompletePlay hold one exact resolving occurrence outside discard and reshuffle until completion |
| Target choice | TechniqueTargetPlan and FishingRunController | Cancellable selection before commit; exact IDs per effect, duplicate rejection, stale-hand/encounter rejection, concealed-information masking, and fixed current-encounter selection |
| Hand cap | TechniqueDeckRuntime and CreatureEffectRuntime | Four-card setup/effect-draw/recovery cap; Cod reserves an exact discard occurrence before discard-one-or-decline room selection |
| Recovery identity/order | CreatureEffectRuntime | Capture Urchin eligibility when targets are committed; expose Cod after discard and Urchin after the other reactions; consumed occurrences cannot be duplicated |
| Input and failure guards | FishingRunController | Block core actions, other Techniques, reentrant choices, and synchronous restart during effect dispatch; partial failure suppresses queued choices and retains resolving ownership until explicit restart |

TechniqueTargetPlan contains serialized per-effect selections and required counts. It enumerates legal attached targets through CatchChainRuntime and stores the exact current encounter and revision for encounter targets. The controller owns the precommit selection token and hand version, while the backend independently validates the completed plan before mutation. Up-to-count Release effects can finish selection after one recipient instead of requiring the maximum. Existing Load Lifter, Counterweight Rig, Prize Tag, and Double Release text describes the selected recipients. Test helpers supply explicit fixture inputs; they are not a gameplay fallback for choosing targets.

TechniqueRuleChecks covers these rules and is included in the combined EventIntegrationChecks menu. Runtime checks execute through a .NET adapter against the real rule classes; runtime and Editor sources also compile against Unity references. Production-prefab Editor checks are provided but have not been executed in Unity. See [Technique validation evidence](Validation/TechniqueRules/README.md) for counts, scope, and remaining PlayMode/serialization/UI verification.

## Active approved Techniques and archived content

The GDD section 10 and its Technique Card Definitions.yaml supplement define the eight active Techniques and 12-card starter deck. CardDefinition stores the authored Technique role and an explicit backup flag. The previous 15 definitions and their metadata retain their GUIDs under Assets/Cards/Backup/Techniques; TechniqueDeckRuntime and TechniqueEffectRuntime reject backup cards. RunProgressionRuntime rebuilds an initialized session deck containing a backup from the current authored starter and excludes backup unlocks. SampleScene references only the new starter cards and no legacy unlock. Authored assets remain outside per-run state.

ApprovedTechniqueRuntime, serialized inside TechniqueEffectRuntime, owns Steady Line's pending/active grant and pending Prize Hook descriptors. Its explicit effect-family handlers validate the eight new effect types without repurposing legacy enums. EncounterRuntime clears only one assigned or oldest future concealment charge for Clear Water and retains future charges from the same source. Hold Steady uses the existing exact Hazard-cancellation path, including reveal reactions and Weathered Shell.

TechniqueTargetPlan supports exact other-hand-slot selections for Recast as well as attachment and encounter targets. It excludes the played physical slot, rejects duplicate slots and stale hand versions, and remaps slots after the played card leaves hand. One or two selected cards are discarded without being played, then the same number is drawn while Recast remains in resolving ownership. The controller presents hand targets in the existing choice modal, including cancellation and finishing with one selection.

CatchChainRuntime.TryChangeCreatureMode switches one creature in place, retaining modifiers and activation history. BeforeAttachmentEffects runs for new attachments, replacement Catches, and both sides of role swaps, allowing Prize Hook on Catch entry and Upwelling on Bait entry. CreatureEffectRuntime queues only the changed card's own on-attach work, after the Technique's discard, without emitting other creatures' after-catching triggers. Source-dependent effects end on the role change; spent Bait activations never refresh.

Steady Line is activated by the confirmed-action coordinator after successful preflight and before event consequences or commitment. CatchChainRuntime subtracts its action-scoped reduction from total Line Load with a zero floor, so every consumer uses the same effective load while individual stats and capacity stay unchanged. Relief survives suspended event/creature choices. Expire it after Descend checks and before reveal, or on Surface/run cleanup; failed preflight does not consume it. Prize Hook awards all pending modifiers before on-attach processing and retains them on the recipient across modes/biomes until removal. All unfinished grants and deferred releases clear on run shutdown/restart.

ApprovedTechniqueChecks runs the actual rule/controller classes and supplies an Editor-only asset/deck/backup check. The combined event menu includes both. Validation and remaining Unity checks are recorded under Docs/Validation/ApprovedTechniques/README.md.

`JournalCatchChainScrollRect` also owns the newest-catch scroll animation: attachment-count growth that extends the viewport starts a 0.32-second smoothstep using unscaled time. Repeated state refreshes retain animation progress; another attachment retargets from the current offset. Wheel/drag input cancels it immediately and preserves manual inspection. Initial binding, responsive extent changes and shrinkage restore/clamp directly; empty chains and disable clear unfinished motion. The presenter supplies the attachment count after measuring content, with no gameplay-state dependency. [Play Mode verification](UIThemes/MidnightFieldJournal/SmoothCatchChain/README.md).

`JournalRoleSelectionSync` authors only the candidate creature profile and selected-role prefab decorations, and shares its values with the central-card builder. `JournalRoleSelectionMark` draws the selected-role check as nonblocking native Canvas geometry; visibility stays inherited from the existing selected-border object and `JournalCreatureCardVisuals.RefreshSelection`. Shared controller selection, concealment and input-lock rules are unchanged.

The candidate selected-role trial now uses `JournalRoleFade`, a nonblocking rounded native Canvas overlay drawn above the complete unselected panel, including its independently parented heading/icon. `JournalCreatureCardVisuals.RefreshSelection` enables only the opposite panel fade, restores both for None/events/blank cards, and suppresses the earlier gold border/check treatment. `JournalRoleSelectionSync.ConfigureFading` binds the same geometry for targeted updates and future prefab builds. Fading changes presentation only, never either role button or gameplay state.

`JournalRoleFade` uses authored per-corner radii and an inset inside the printed panel edge. Catch has the larger lower-left corner; Bait mirrors it at lower-right. The theme editor sync persists this geometry into the candidate prefab and future builds; role targets and content rectangles are unchanged.

The candidate selected-role edge uses `JournalCardOutline`’s opt-in per-corner radii, preserving its existing single-radius behavior for other UI. `JournalRoleSelectionSync` supplies identical role-corner data and inset to the outline and fade. `JournalCreatureCardVisuals` shows the thin selected edge together with the opposite role’s existing fade; previous glow/check decorations stay inactive.

Current selected-role feedback uses `JournalRoleFade` in soft-inner-glow mode. Its fill and glow modes call the same contour builder on sibling root regions, eliminating independent outline rendering and parent-space differences. Five inward rings ramp alpha from zero at the boundary to a restrained warm peak, then back to zero before the content center. `JournalCreatureCardVisuals` shows the chosen glow/opposite fade and hides old outlines/checks. Role input and gameplay remain shared and unchanged.

Current candidate role feedback dims both panels while choice is None. Selecting a role removes its fade and enables its existing glow; the alternative stays dim. Theme fade opacity is58%. Events and cleared cards hide both treatments. No selection or action rules changed.
