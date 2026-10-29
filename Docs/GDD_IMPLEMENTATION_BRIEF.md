# GDD Implementation Brief

## Approved creature timing flow (25 October 2026)

The journal comparison scene uses a rarity timing phase before creature reveals. Descend commits the selected Catch/Bait and advances normally; the next ordinary creature remains unrevealed until the player stops the moving marker. Before marker motion, the journal bar enters from below over one second with mirrored elastic settling; Stop stays locked until entry completes. Stop sets the maximum creature rarity: Common; Common/Uncommon; Common/Uncommon/Rare; or all four rarities for Legendary. Draw uniformly from distinct biome/depth-eligible creatures at or below that ceiling. Each species has one equal chance; previous rarity weights, encounter-weight modifiers and immediate-repeat exclusions do not bias this species draw. Event/category pacing is separate and unchanged. Show a brief stopped result, then move the journal instrument upward off-screen over one second with elastic-in motion (amplitude 1, period 0.74). Reveal and its normal reactions/Technique window wait until this exit finishes. Catch/Bait has no default. Events reveal directly without a bar; their existing action-confirmation rules remain intact. Surface is available during timing and cancels its pending state. A stop on the highest displayed non-Miss rarity emits a brief gold/ivory particle celebration, using the same hold/exit timing and uniform ceiling selection.

Unavailable rarities have no interval; their space goes to Common when available, otherwise available intervals normalize to the full track. Current tuning is Common/Uncommon/Rare 87/5/8 and 0.6 one-way traversals per second, editable in RarityTiming.asset. Uncommon remains 5% when Rare is unavailable and Common is eligible. No initial Miss or Legendary interval. Existing fixed queued/trail/chain/Apex selections use a single fixed-rarity interval to preserve their guarantee. Encounter replacements in an already revealed Technique window do not demand another bar. No new upgrade cards or effects are introduced by this increment. Legacy scenes without the profile retain the previous selection flow.

This brief summarizes `Docs/Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md` for day-to-day development. The full GDD remains the source of truth.

## Core Concept

The game is a cards-only fishing roguelike. The player descends through increasingly valuable and dangerous waters, builds a temporary chain of catches on the fishing line, and decides when to surface with the haul.

The core emotional tension is push-your-luck: "I should surface now, but I can probably handle one more catch."

## Design Pillars

- Cards are the game world. There is no animated character, explorable sea, or conventional world view.
- Push-your-luck is the emotional core.
- The Catch Chain is a temporary build, not instant loot.
- Weight creates pressure through Line Load and overload danger.
- The player deck defines how the player fishes, but basic fishing actions are always available.

## Card Groups

Use three functional card groups:

- Core action cards: always available and not shuffled into the player deck. The permanent actions are `Descend` and `Surface`; Release requires an explicit card effect.
- Player technique cards: the 4-card hand and deckbuilding layer. These include bait, hooks, knots, rigging tricks, line reinforcement, descent modifiers, encounter manipulation, and catch manipulation.
- Encounter cards: generated or drawn by the game based on depth, biome, current catches, and modifiers. This group includes creatures, treasure, hazards, environment cards, opportunities, discoveries, and biome-specific cards.

Do not put fundamental actions into the player deck. Bad draws should create tactical limitations, not prevent basic fishing.

## Core Actions

- `Descend`: permanent core action. A Hooked creature or Apex requires an explicit Catch/Bait selection before Descend; neither is selected by default. Each new occurrence/replacement clears the choice while ordinary refreshes preserve it. Boat, empty/skipped and event encounters have no role-selection gate. Advances deeper and normally commits the current Hooked creature to the Catch Chain unless a technique or special effect avoids, redirects, or replaces it. Before commitment, the player selects the card's Catch or Bait effect; only that selected effect set activates.
- `Surface`: always available. Ends the descent and attempts to bring the remaining Catch Chain home as the haul.

The game is structured around meaningful actions, not a separate traditional turn system.

## Encounter States

- `Encountered`: revealed and visible according to the current rules.
- `Hooked`: the current unresolved catchable encounter attached to the rig. This is the player's reaction window.
- `Caught`: committed to the Catch Chain after the player Descends without avoiding or replacing the Hooked creature.

There is no universal free Release. A Technique, creature, or event must grant a deliberate Release, respecting its targeting/release restrictions. There is no free universal Skip action. Avoiding an unwanted catch requires a technique, equipment effect, creature effect, or other specific rule.

## Catch Chain

Caught creatures/items remain attached to the fishing rig while the player continues descending. The chain is both a visible vertical record and a mechanical system.

- Every attached card contributes its resolved Weight to Line Load.
- Every attached card records whether it was committed as a Catch or as Bait. Bait cards remain in the same chain and are visibly marked.
- Only the effect set matching the card's stored Catch/Bait role is active.
- Catch attachments contribute their resolved Value to the haul. Bait attachments contribute zero haul Value.
- Many caught cards can provide passive effects while attached.
- Existing catches can influence future encounters.
- Releasing an attachment removes its Weight, any Catch-role haul Value, and ongoing attached effects, attraction modifiers, and source-dependent synergies. Completed one-time triggered effects are not automatically undone.
- The line visually connects the boat/start card to caught cards below it.

### Catch and Bait Effect Design

- Catch builds the haul; Bait builds the engine. Both remain attached and contribute full resolved Weight. Bait is not temporary by default and gives zero haul Value.
- Ground effects in the creature's anatomy, behavior, appearance, or ecology. Catch and Bait may use different targets, triggers, and mechanics; numerical scaling pairs should be occasional.
- Bait should generally provide stronger utility or build potential to justify lost sale value and occupied capacity. Catch should remain attractive in plausible game states.
- Favor meaningful creature/tag synergies and conditional persistent power. Allow downsides, simple commons, and explicitly effectless roles; avoid complexity for its own sake.
- Specify timing, target, duration, and Release interactions. Distinguish ongoing attached effects from completed triggers; Release ends the former without automatically undoing the latter. Distinguish encountering, Hooking, role selection, and commitment.
- Preserve Line Load, Catch Chain, core actions, and technique decisions. Keep rules text suitable for the supplied effect panels; compact chain cards do not display full rules text.
- The full GDD section 6 contains the authoring checklist and illustrative pairs. Copying, encounter choice, role changes, and example values require precise rules and design approval before finalized content; examples do not establish implemented capabilities.

### Approved Clinging Crab and Cod Refactors

- Clinging Crab: Weight 2, Value 14, Uncommon, depth 0–12, Anchored/Armored. Catch Iron Grip prevents all deliberate Releases but permits forced removal and exchanges. Bait Claw Cut immediately chooses and deliberately releases one other legal attached Catch or Bait; skip without a target and never save the use.
- Cod: Weight 2, Value 12, Common, depth 3–20, Fish/Predator. Catch has no effect. Bait Bottom Feeder offers one discarded Technique after the first deliberate Release of another Armored creature in either role, after the whole releasing ability resolves (including its just-used Technique). Empty discard still consumes the shared one-per-encounter limit; all Cod copies share it. Encounter replacements do not reset it; forced removal and exchanges do not trigger it.
- A resolving Technique enters discard after its effect finishes and before completion-waiting recovery. Completion waits for recovery choices and reopens input without drawing a replacement. The hand limit is four; full-hand Cod recovery allows discarding one Technique for room or declining. Explicitly empty refactored roles stay empty.

### Approved Cleaner Shrimp Refactor

- Coastal Waters; Weight 1, Value 8, Uncommon, depth 1–14 inclusive, Small.
- Catch / Quick Clean: on attachment, choose and remove one active negative card effect, including lasting applied penalties. Reverse only the selected modifier; preserve other effects and completed information/actions. No eligible effect means no choice.
- Bait / Cleaning Station: at each later biome entry while attached, offer two distinct eligible non-Apex starting-depth creatures using normal weights. The player replaces one pre-entry creature Catch; skip without an eligible Catch or two offers. Resolve multiple Shrimp in chain order; new replacements cannot be swapped again at that entry. Swaps do not trigger Release reactions or consume normal encounter draws. Resolve the replacement's Catch on-attach effect before continuing.
- The GDD section 6 contains the complete rules. Later biome content is still authored separately; do not invent a destination biome.

### Approved Garfish, Glass Minnow, and Jellyfish Refactors

- Garfish: Weight 1, Value 9, Common, depth 0–9, Fish/Small/Elusive. Catch Swift adds 3 Value per other Elusive creature Catch, using effective tags; Bait never counts. Bait Slipstream grants Elusive to every other attached Fish creature in either role before attachment triggers. Grants do not duplicate or erase native/other-source tags.
- Glass Minnow: Weight 1, Value 5, Common, depth 0–15, Fish/Small/Schooling. Catch has no effect. Bait Shoal Signal offers one optional, immediate hand-Technique discard/draw after another Schooling creature Catch finishes its on-attach effects. Empty hand or declining skips; multiple Minnow copies offer only one redraw per eligible attachment. Draw exactly one, including for oversized hands; normal reshuffle applies.
- Jellyfish: Weight 1, Value 8, Common, depth 0–18, Small. Catch Tangle queues +1 Weight for the next other creature Catch, survives source removal, ignores Bait, stacks, and then lasts on the recipient until removal regardless of later role. Quick Clean can remove pending or applied penalties. Bait Buoyant Bell selects up to two distinct other Armored creatures in either role once, reducing each Weight by 1 while the Bait source remains and the target is attached/Armored. Losing Armored suspends; regaining restores. Source copies stack with a zero floor. Unused/removed targets cannot be replaced; target role changes do not cancel reductions.

### Approved Lionfish, Mackerel, and Moonlit Ray Refactors

- Lionfish: Weight 2, Value 30, Rare, depth 10–20, Fish/Predator/Armored. Catch Venomous Spines loses 5 current sale Value per other creature deliberately Released from either role, floor 0 without hidden debt; loss lasts until removal, including across modes. Forced losses and exchanges do not count. Bait Venomous Lure offers one immediate optional Release of another legal Small creature in either role after a Predator Catch finishes its on-attach effects, excluding that Predator. All Lionfish copies share one opportunity per attachment; successful creature Releases trigger normal reactions.
- Mackerel: Weight 1, Value 10, Common, depth 0–16, Fish/Schooling. Catch Packed Shoal sets effective Weight to 0 with two other Schooling creature Catches, using live tags and immediate recalculation. Bait School Call activates once per card on first Bait attachment: immediately choose or decline from up to two distinct non-Apex Schooling species eligible at the next biome/depth, using normal weights. One offer is valid; none uses normal draw. Reveal at normal time without advancing depth for the choice. Modes/biomes never refresh activation. One queue: new selection overwrites, decline preserves it, source removal does not cancel. Apex cannot be replaced; actual destination ineligibility discards the selection and draws normally.
- Moonlit Ray: Weight 3, Value 24, Rare, depth 9–20, Fish/Predator/Elusive. Catch Graceful Glide grants a flat -1 Weight, floor 0, while any Elusive creature Bait is attached; additional Bait do not stack. Bait Moonlit Reflection records each deliberately Released Elusive Catch's modified sale Value immediately before individual removal, then transfers it to a remaining Catch after the entire action finishes. All copies share one transfer per released Catch; multi-release actions transfer each separately. Forced losses, exchanges, and released Bait do not count. No recipient skips; choice cannot be saved. Completed trigger and bonus survive source removal; bonus survives recipient modes until recipient removal. Bait still sells for 0.

### Approved Mussel, Sardine, and Sea Anemone Refactors

- Mussel: Weight 1, Value 6, Common, depth 0–12, Small/Armored/Anchored. Catch Shell Growth permanently gains 2 Value per other Armored creature Catch after on-attach resolution, once per Mussel Catch; Bait does not trigger, bonuses survive modes until removal. Bait Shelter Shell gives one protection on first Bait attachment: cancel the next revealed Hazard before effects, without depth advance or immediate redraw. Next Descend proceeds normally. Oldest attached protected Mussel consumes its use; other copies retain theirs. Only Hazard classification qualifies, never Apex. Protection survives biomes, expires outside Bait, and never refreshes after use or expiry.
- Sardine: Weight 1, Value 7, Common, depth 0–14, Fish/Schooling/Small. Catch has no effect. Bait Feeding Frenzy optionally deliberately Releases one legal Sardine Bait after a Predator Catch's on-attach resolution; all copies share one activation per Predator. Resolve Release effects first, then optionally discard hand Techniques and draw up to two that fit four slots, using normal reshuffle. Completed draws survive source removal. Failed/prohibited release gives none; decline spends the immediate chance. Forced loss and other-effect Releases never activate it.
- Sea Anemone: Weight 1, Value 8, Uncommon, depth 0–16, Small/Predator/Anchored. Catch Stinging Tentacles blocks Technique selection, including individual targets and targeted Releases, while in Catch mode. Untargeted and creature effects still apply. Bait Sheltered Exchange offers one immediate optional hand/discard Technique swap after a Small creature Catch finishes on-attach effects, shared across copies. Require both piles; choose both existing occurrences before moving either. Atomic exchange preserves hand size and never plays or draws. Decline moves nothing; opportunity cannot be saved.

### Approved Sea Bass, Sea Urchin, and Small Octopus Refactors

- Sea Bass: Weight 2, Value 16, Uncommon, depth 4–20, Fish/Predator. Catch Thrashing Catch immediately requires one chosen hand Technique discard; empty hand skips. Bait Shoal Scatter activates once per card on first Bait entry: choose up to two distinct other legal Small creature attachments in either role before either leaves, then deliberately Release together. Capture all pre-group tags/values; only surviving sources react after both removals. No Technique needed; Hooked excluded. Fewer targets, decline, or later modes never save/refresh the use.
- Sea Urchin: Weight 1, Value 9, Common, depth 0–16, Small/Armored/Anchored. Catch Bristling Spines adds a flat 1 Weight while any Predator creature is attached in either role, using live tags. Bait Spiny Grip grants one independent pending recovery once per card. The next Technique selecting an attached Armored creature consumes the oldest grant; one per play, eligibility at play time. Explicit individual targets count; untargeted and Hooked-only effects do not. After all triggered choices, recover the exact played discard occurrence before input reopens. Target/source removal cannot cancel it. Full four-card hand permits one discard or declining; a declined/missing occurrence still spends the grant and never substitutes a different copy.
- Small Octopus: Weight 1, Value 9, Uncommon, depth 6–20, Small/Predator/Elusive. Catch Clever Catch immediately draws one Technique that fits four slots; full hand permits one discard or declining, never a saved draw. Bait Helping Arms activates once per card: choose another creature Catch and Bait, require both, and switch together without position/ID/bonus changes. End old ongoing effects, activate new ones, resolve the new Catch own on-attach first, then surviving new Bait own on-attach, respecting limits. User-confirmed: only own on-attach effects activate, with no other after-catching reactions or next-attachment consumption. No Release or encounter/depth advance. Cleansing/lifecycle keys are role-specific; mode changes never restore spent one-use abilities. No free general mode-switch action.

### Approved Spiny Lobster, Squid, and Tidepool Eel Refactors

- Spiny Lobster: Weight 3, Value 24, Rare, depth 8–20, Armored/Heavy. Catch Weathered Shell gains 4 lasting sale Value per completely cancelled revealed Hazard, once per Lobster Catch; partial prevention/pre-reveal replacement do not qualify. Bait Shell Support reduces every other attached Armored creature's Weight by 1 in either role, live tags/attachments, stacking with floor 0, never self-support. Bonuses persist across modes until removal.
- Squid: Weight 2, Value 20, Rare, depth 8–20, Predator/Elusive. Catch Ink Veil conceals next two ordinary creature encounters' identity/art/stats/tags/both effects until Catch/Bait commitment, then reveals before on-attach. Both panels remain selectable. Assign one charge at reveal; commitment or removal/avoidance/replacement/skip consumes it. Apex/non-creatures do not consume. Charges stack consecutively, survive source removal and biomes, clear on new run, and are exact Quick Clean candidates. Clear Water explicitly ends one assigned Ink concealment; legacy information-only reveal effects do not bypass it. Bait Ink Escape grants one next-ordinary-creature skip per card on first Bait entry. Reveal effects first; decision immediately before new techniques/core actions, still hidden if Ink. Accept discards without attachment/Release/depth advance and consumes assigned Ink; next Descend advances normally. Decline or removal during reveal spends offer. Source removal/modes/biomes preserve pending FIFO grants; no refresh. Skip breaks that encounter's authored chain.
- Tidepool Eel: Weight 2, Value 14, Uncommon, depth 0–14, Fish/Predator/Elusive. Catch Slippery Exit immediately offers one legal deliberate Release of another Elusive creature Bait, never saved. Bait Escape Current records pre-removal Elusive and draws one Technique per other qualifying deliberately released creature in either role after the whole action; observing Eel must survive as Bait. Copies share one draw per creature. Before any drawing, optionally discard to fit four slots; draw as many as fit. Forced removals/exchanges excluded, no saved draws.

### Creature Tag Vocabulary

Creature and creature-like Apex cards use only this closed gameplay vocabulary:

- `Fish`: broad biological fish group.
- `Predator`: hunts other creatures and supports attraction/value synergies.
- `Schooling`: commonly appears or behaves in groups.
- `Small`: low-weight or physically small catches.
- `Heavy`: large or high-load catches.
- `Anchored`: naturally clings, anchors, grabs, or remains fixed.
- `Armored`: protected by shells, spines, or a hard body.
- `Elusive`: slippery, camouflaged, fast, or difficult to control.

Do not add taxonomy, rarity, role, flavor, card-type, or state labels to creature `Tags`. Card type and rarity already have dedicated fields. Effects may still use a card-type name such as `Creature` as a type selector; that is not a creature tag.

## Line Load

Line Load is the main pressure system.

- Each catch has a weight/load contribution.
- Equipment defines safe line capacity.
- Over capacity is allowed.
- Overload increases risk rather than causing immediate automatic failure.
- Descend and Surface perform a break check while overloaded; the chance rises with excess load.
- When the break check succeeds, the run ends, one to half of the attached catches are lost at random, and the player is forced through the normal Surface ascent.
- When the line holds, an overloaded Descend continues normally and an overloaded Surface retains the full haul.

## Depth, Biomes, And Encounters

Depth is a central run progression axis. Different depth bands and biomes use different encounter pools.

Encounter decks should include more than creatures:

- Creatures
- Treasure and salvage
- Environment cards
- Opportunity cards
- Hazard cards
- Rare or special encounters
- Biome-specific cards
- Biome Apex Encounters

Each biome should introduce a recognizable gameplay idea, not only higher-value fish or new artwork. Biome depth tiers can overlap and evolve their pools over time.

Biome Apex Encounters are high-value, mechanically significant climax cards that carry pressure. Catching one should feel like a legendary reward, a run-changing build component, and a new source of risk.

## Event Cards

- Hazard, Environment, Opportunity, and generic Encounter events show name/type/artwork/description only. They have no Weight, sale Value, or Catch/Bait modes, and never attach to the Catch Chain.
- Reveal the event and finish automatic reveal reactions, including Hazard cancellation, before event consequences. Then allow legal Techniques until confirming Descend or Surface, even if the event was cancelled; this window does not depend on a Hooked creature.
- Unless the card states otherwise, resolve the event once, including resulting creature reactions, before performing the confirmed action. Once resolution starts, do not reopen the Technique window until that action finishes. Descend advances normally and reveals the next encounter; event resolution adds no extra depth advance. Surface resolves the event before returning the haul.
- Cancelling a revealed Hazard discards it without effects and triggers Hazard-cancellation reactions, including Weathered Shell. Replacing an unresolved event prevents its effects but is not Hazard cancellation. No eligible target means resolution without effect, not cancellation. No event in this batch forces a Release.
- Discard resolved event cards; pending effects/modifiers follow their stated durations. A target's mode change does not reset a modifier. Run ending clears all unfinished event effects and encounter trails. Full rules are in GDD section 9, Event cards and resolution window.

Implementation status: EVT-01 through EVT-08 provide lifecycle state, reveal/cancellation/replacement handling, legal event Techniques, once-only dispatch, explicit pending/modifier lifetimes, Quick Clean, and confirmed Descend/Surface locked through creature choices before the action. Run shutdown immediately clears live effects and trails; ascent uses separate presentation snapshots. Specific handlers and named duration boundaries require approved card definitions; unsupported legacy effects reject confirmation without closing the reaction window. The run UI consumes controller-owned action/Technique availability; the Events Collector hides Catch/Bait data and explains handler/lifetime requirements. Integrated runtime checks pass; Unity UI, serialization, Undo/Redo, and screenshots remain unverified because isolated Editor startup lacks a headless entitlement. See Event Rules Implementation Tasks.md and Validation/EventIntegration/README.md.

### Approved coastal event batch

- Reef Snag: Common Hazard, depth 3-18. At resolution select the heaviest current non-Armored creature in either mode, player breaks ties; +1 effective Weight per Snag until removal/biome completion. Later Armored does not remove it. Surface checks before expiry.
- Silt Burst: Common Hazard, depth 4-17. Append one ordinary-creature concealment to Squid's FIFO queue; commit/removal consumes it, including across biomes. Events/Apex do not consume it; learned preview information remains visible. Cuttlefish's existing self-hidden Value is unchanged.
- Gentle Upwelling: Uncommon Environment, depth 1-15. All pending grants reduce the same next creature entering Bait (including mode changes), before own on-attach effects. Each reduces effective Weight by 1, floor 0; pending grants cross biomes and awarded reductions persist across modes until removal.
- Loose Knot: Uncommon Opportunity, depth 3-18. Optional legal creature Release in either mode, followed by its full Release reactions before the confirmed action. Not a Technique; no saved use or release-lock bypass.
- Fresh Shell Fragments: Rare Opportunity, depth 5-13. One biome-local Spiny Lobster trail to current depth +3, eligible ordinary slots only. Preview identity/stats. Queued choices take precedence and consume it; Technique replacement/avoidance marks the consumed promise replaced. Passed destinations retarget the first eligible normal slot without displacing Apex. Clear on biome/run ending.
- These five are the only active coastal event definitions. Feeding Frenzy, Ghost Net, Murky Current, Rough Current, and Snagged Kelp were removed from assets and every depth-tier pool, together with the Feeding Signs and Ghost Net Salvage encounter chains. The Calculated Overload debug scenario now uses Gentle Upwelling. Sardine's Feeding Frenzy ability is separate and remains unchanged.
- These handlers register at runtime setup. Event-specific illustrations have not been supplied; assets use the existing event master with empty artwork references.

## Active Technique content

Use only the eight designs in [Technique Card Definitions.yaml](Technique%20Card%20Definitions.yaml): Cut Loose (Release one legal creature), Slip Past (skip Hooked non-Apex creature), Clear Water (reveal current hidden creature or remove one future concealment), Hold Steady (cancel unresolved Hazard), Re-rig (one in-place mode switch and own on-attach effects), Steady Line (-2 total Load through the next confirmed action), Recast (discard/draw one or two other hand Techniques), and Prize Hook (stacking next-Catch +8 Value/+1 Weight, including mode entry and Apex).

The active scene uses the recommended 12-card starter: Cut Loose ×3; Slip Past ×2; Clear Water ×1; Hold Steady ×1; Re-rig ×1; Steady Line ×2; Recast ×1; Prize Hook ×1. Hand size remains four. All 15 previous Techniques are flagged backups under `Assets/Cards/Backup/Techniques/`, retaining GUIDs. Deck setup, play, and unlock paths reject them; old session decks migrate at next run setup. The legacy unlock is disconnected. No universal free Release remains in the controller or UI.

## Player Deck

The Technique hand limit is four. Play during the reaction window before confirming Descend or Surface, completing one Technique and all its reactions before another. A play itself neither advances depth nor Surfaces.

Select required targets before committing. The played physical card leaves hand into resolving ownership, stays outside discard during its effect, then enters discard before reactions waiting for Technique completion. Cod may recover that just-used occurrence; Sea Urchin recovery follows the other triggered effects. A card already returned by one recovery cannot be duplicated by another.

Techniques cannot select Sea Anemone Catch with active Stinging Tentacles; deliberate Release respects Iron Grip. Targetless effects do not count as targeting attached creatures. Deal the opening hand at run start. Playing a Technique and Descend never automatically draw or refill cards; only explicit card effects draw, redraw or recover them. No energy resource or once-per-biome counter is added; each card specifies its durations, and run ending clears pending effects. For Cod recovery with a full hand, discard one Technique to make room or decline; Sea Urchin retains the same policy. Overflow past four is not allowed.

The shared runtime now enforces resolving ownership, cancellable required-target selection before commitment, exact recovery identity/order, and the four-card cap. Core actions and additional Techniques wait for the effect and its reactions; partial resolution failures remain blocked until explicit restart. See GDD section 10, Architecture.md, Approved Technique rules implementation, and [validation evidence](Validation/TechniqueRules/README.md). Runtime and Editor compilation and adapter rule checks pass; Unity PlayMode, serialization round trips, and actual modal presentation remain unverified.

Technique cards should interact with:

- Depth
- Line Load
- Creature tags
- Catch Chain state
- Current Hooked encounter or unresolved event during its pre-action Technique window
- Risk

Some technique cards should act as escape valves for mandatory catches by bypassing, replacing, cleansing, neutralizing, or otherwise managing Hooked encounters.

## Economy And Progression

Value is realized primarily after surfacing, not immediately when a creature is caught. Only catches still attached and successfully returned become the haul.

The haul can be sold or used for progression such as equipment improvements, new cards, deck upgrades, and unlocks. Long-term progression should expand strategic possibilities rather than only increasing raw power.

## Full-Game Development Scope

The project is no longer in MVP/prototype mode. Use the following as the minimum gameplay foundation that must remain clean enough for the full game:

- 2 permanent core actions: `Descend` and `Surface`; Release requires a specific card effect.
- A 4-card technique hand and expandable technique deck.
- Encounter pools across creatures, hazards, opportunities, treasure, rare encounters, biome-specific cards, and encounter chains.
- Downside creatures and pressure cards that are meaningful in different run states.
- Technique cards that manage bad Hooked encounters without becoming a universal Skip button.
- A visible vertical Catch Chain.
- Line Load and capacity with overload risk.
- Passive and active catch effects with clean runtime ownership.
- Multiple depth bands and biome-ready encounter selection.
- End-of-run haul, reward, and progression systems that can grow into the final game.

Development success criterion: each shipped increment should strengthen the final game rather than create temporary scaffolding. Do not accept "good enough for MVP" as a reason to postpone known architecture, content, UI, persistence, or validation work.

## Open Questions

Do not lock these without explicit design approval or prototype evidence:

- Exact overload probability tuning and loss-count balance within the locked one-to-half range.
- Whether catch-chain order/position has rules.
- Exact draw count, deck size, discard/shuffle rules, and rarity structure.
- How locations/biomes are selected.
- Permanent versus run-specific meta-progression.
- Exact biome length, depth-tier count, encounter-pool size, and visit length.
