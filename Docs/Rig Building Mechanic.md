# Deepwater — Rig Building and Creature Upgrades

Agreed Phase 3 direction supplied 14 November 2026; implementation updated 15 November; shared five-tag rules updated 18 November. The complete loop is **Discover → Land → Build / Upgrade**. This specification governs rig acquisition, positioning, levels, primary abilities and sale value. Individual species content belongs in editable game definitions, not a duplicate Markdown catalog. The five-tag revision is not implemented by this documentation change.

## Successful acquisition

After a successful Landing Challenge, identify the landed species by stable creature ID. A species occupies at most one rig position.

For a new species, show the independent Build screen. Offer every legal gap: before the first member, between members and after the last. Preview the resulting order, species values and total haul without changing the live rig or firing acquisition effects. Commit once using the landed occurrence and current rig revision. The creature enters at Level 1. Existing members retain their relative order. Continue automatically to the next encounter.

For a duplicate, upgrade the existing member automatically. Add one level and one collected copy, retain its original position and add that copy's full base sale value. Resolve only explicitly authored upgrade triggers. Recalculate current effects and continue to Discover with a short upgrade notice; no placement or extra confirmation is required.

There is no Catch/Bait choice, sale-value sacrifice, manual rearrangement, Line Capacity, weight overload or voluntary surfacing. A pending or failed creature cannot contribute toward landing itself.

## Levels, copies and sale value

The first copy is Level 1. Each further copy increases level by one. No gameplay maximum level is established; numeric overflow is an authoring diagnostic and must reject the change without partial mutation.

`Species accumulated base value = species base sale value × collected copies represented by that position`.

Every collected copy contributes its base sale value. Applicable value effects operate on the accumulated species value unless their authored scope states otherwise. Calculate continuous flat bonuses first, then multiplier products. Recalculate from accumulated base value and explicit triggered grants, never from the previous derived result.

Current sale value remains in whole units. Fractional economy components are rejected by authoring validation until a rounding policy is designed; Landing Points retain decimal precision. Temporary levels, copies and derived values belong to runtime members, never shared definitions or permanent progression.

## Position vocabulary

- **Before:** nearer the Rod; index zero is the Rod end.
- **After:** nearer the Line Tip.
- **Adjacent:** the immediately preceding or following position.
- **Contiguous group:** an uninterrupted sequence satisfying a tag condition, extending in both directions and stopping at the first nonmatching position.
- **Center:** equal numbers of occupied positions before and after the source, with at least one on each side for a qualifying center ability.
- **Line Tip:** the final occupied position.

An upgraded member counts as one position for adjacency, contiguous groups and tag counts. A Level 3 Sardine does not supply three Schooling neighbors. New insertion may break an existing adjacency or group. The rig owner validates placement restrictions at commitment, including after any change that invalidated a preview. No manual reorder API or UI is provided.

## Primary abilities

Each creature has exactly one gameplay tag and one `primaryAbility` definition expressing that tag's philosophy. The tags are Schooling, Predator, Anchored, Armored and Elusive; see the [GDD](Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md#tags-and-properties). Related components belong to that ability and share its identity. Components specify operation, accuracy condition, positional count, tag filter, recipients, trigger and level progression. Level amounts currently use an editable initial amount plus an editable increment per additional level. These are authoring capabilities, not fixed species designs.

Supported continuous components: flat Landing Points, successful/Good/Perfect/explicit-Miss multipliers, extra attempts, protected Misses, flat sale value, sale-value multipliers and before/after insertion restrictions. Value recipients can be self, adjacent, before, after or all matching positions. Conditions can count adjacent, contiguous-other, before, after or other matching positions, or activate once when the condition is satisfied.

Multipliers start at ×1. Disposable validation fixtures exercise contiguous-group bonuses and level scaling; they do not define the species catalog. Editable content owns individual descriptions, values and progression. Follow [Creature Ability Authoring](Creature%20Ability%20Design.md) for shared timing, positional and stacking contracts and the distinction between intended rules and currently supported handlers.

Acquired and Upgraded are separate triggers. The current trigger interpreter supports explicit self sale-value grants. Other triggered interactions, including Technique draws or creature removal, require precise authored definitions and a supported handler before assignment. Recalculating a continuous ability never retriggers acquisition or upgrade grants.

Every change is transactional: invalid data, restrictions, stale commands and arithmetic overflow leave members, levels and earned value unchanged. Previews simulate on independent snapshots and publish no live acquisition notifications.

## Landing integration

At encounter start, capture committed members, effective tags, levels and their primary ability rules. Resolve positional conditions from one position per species. The encountered creature and the Build preview are excluded. Current-level flat bonuses and multipliers produce immutable source-attributed scoring rules. Extra attempts and Miss protection are encounter-local allowances. A protected Miss does not consume an attempt; ordinary bonuses still cannot score on Miss without an explicit Miss rule.

Changing a shared definition or upgrading the live rig later must not retroactively alter an already-started challenge. Movement patterns, strike accuracy, slot matching and rarity selection retain their existing owners.

## Removal and restrictions

Ordinary manual removal is not offered. A gameplay effect may request removal through the rig owner; derived bonuses and insertion restrictions are recalculated from the remaining arrangement. The current value-preservation convention retains the removed member's collected base value and completed value grants, while its continuous effects stop. A final removal ability must state its value consequences explicitly before assignment.

No behavior is invented for a catalog combination that blocks every insertion gap. The UI reports no legal position and commits nothing. Restriction combinations that can create this state must not be assigned until their resolution rule is specified.

## Expedition lifecycle

Every expedition begins with an empty rig. Failure acquires nothing from the failed encounter and awards the full accumulated sale value of previously landed creatures, including duplicates and applicable bonuses. Snapshot the haul before clearing all members, levels and copies. Permanent progression is retained. Successful completion uses the same full-value settlement and may additionally grant approved permanent rewards.

Ordinary Build/Upgrade resolution advances depth and starts the next encounter automatically through the encounter owner. Biome completion predicates, final success endpoints, unlock grants and reward tables remain separate design decisions. This increment does not invent a new completion gate or permanent reward.

## Runtime and presentation ownership

`RigRuntime` owns ordered unique membership, levels, copies, legal gaps, pure previews, upgrade triggers, continuous recalculation and haul snapshots. `CardInstance` carries temporary rig-member data; its shared `CardDefinition` supplies fixed base price and a frozen primary ability blueprint. Historical role effects do not operate on a rig member.

`FishingRunController` owns the landed occurrence, placement command, automatic duplicate handling and continuation. Commands include Build occurrence and rig revision. `RigBuildingView` forwards commands and displays the committed rig, new creature, insertion targets and projected values. Its separate 1920 × 1080 Tackle canvas uses existing Common/Placement art; it appears only during Build. `TackleRigCardView` displays level and resolved sale value. Native labels, text and interaction feedback remain separate from artwork.

`RigBuildingSetup` imports placement assets and binds the prefab to `FishingRunView`. Empty border arrays mean no nine-slice border. Scene/Harbor regeneration retains the Build binding. Validation covers the real native Landing → Build → Discover flow, duplicates without confirmation, positional previews, long scrolling rigs, full failure payout and empty next-run state.
