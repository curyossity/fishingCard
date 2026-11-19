# Deepwater — Game Design Document

**Working title:** Deepwater. **Design baseline:** 6 November 2026; Rig Building and Creature Upgrades updated 15 November; five-tag design updated 18 November 2026.
**Status:** Agreed direction, subject to refinement and playtesting. Confirmed rules, leading concepts and illustrative examples are distinguished below. This is the gameplay source of truth; it does not claim implementation is complete.

## 1. Core vision

Deepwater is a skill-based fishing roguelike built around three interconnected mechanics:

- **Discover:** influence which creatures appear using a tag slot machine.
- **Land:** complete a short skill-based fishing challenge, generating Landing Points through accuracy and the current rig's synergies.
- **Build / Upgrade:** insert a new species into an ordered rig, or automatically strengthen its existing position with a duplicate.

Each successful encounter strengthens or changes the build for increasingly difficult creatures and deeper biomes. The central strategic challenge is building a powerful rig through irreversible positioning decisions. The central execution challenge is using that rig effectively during fishing encounters.

### Design pillars

- Every creature encounter is a challenge the player can win or lose.
- The rig is the main build-building mechanic; the landing challenge tests it.
- Skillful actions remain meaningful as rig bonuses and multipliers grow.
- Placement creates long-term commitments, immediate synergies and future opportunities.
- Abilities support distinct strategies, including landing performance, haul value and Technique interactions.
- Illustrated cards are the game world. Use Unity 2D and a tactile physical-card presentation without an animated player character or explorable sea.

Traditional combat and additional tactical landing decision systems are not planned. Voluntary surfacing, Line Capacity, line overload and line-break failure are not game mechanics. Catch acquisition requires successful landing.

## 2. Presentation and card roles

Large vertical illustrated cards communicate creatures, discoveries and depth. The visible rig connects attached creature cards and remains a central visual feature. Supplied illustrations and reusable card masters remain protected by the [card visual contract](../Card%20Visual%20Specification.md).

![Card presentation reference](Fishing_Card_Roguelike_GDD_reference.png)

The reference supplies atmosphere and card presentation, not approval of every depicted control or statistic.

- **Creature cards:** encounter challenges and, after success, potential rig members with one primary, level-scaled ability.
- **Technique cards:** strategic modifiers whose exact timing and redesigned effects are still to be determined. Access to the core loop must not depend on drawing a Technique.
- **Equipment:** build support and possible permanent progression rewards; categories and acquisition rules are under review.
- **Other encounter content:** events, treasure, authored chains and Apex content require compatibility review. Existing content is not automatically an approved completion gate, challenge bypass or extra failure system.

## 3. Expedition and encounter loop

Every new expedition starts in the first biome, **Shallows**, with an **empty rig**. Initially only Shallows is unlocked. Creatures collected during an expedition are temporary and never carry over as attachments into a future expedition; permanent unlocks and progression are preserved.

### Phase 1 — Discover: slot machine

Before the expedition, select exactly four of the five supported tags in the Harbor: Schooling, Predator, Anchored, Armored and Elusive. These are the three independent reels' equally likely symbols (25% each) throughout the expedition. After the initial spin, accept or lock exactly one reel and respin the other two once. The respun result is fixed and stays visible with further lock/respin input disabled. Only Accept Tags reveals the creature, whenever the player is ready; there is no automatic continuation or timeout.

Three different symbols permit any eligible biome creature with baseline rarity probabilities. A pair guarantees the repeated tag and improves higher-rarity chances modestly; the unmatched symbol supplies no species preference. A triple guarantees its tag and supplies a stronger rarity improvement, without guaranteeing Rare or Legendary. Roll rarity separately, then choose a suitable species. Missing rarity tiers fall back to the nearest available tier while preserving the guaranteed tag; equal-distance ties use the lower tier.

Normal creatures are eligible throughout their biome rather than by individual depth. Preserve creature unlock requirements and separate Apex conditions; standard slots exclude Apex. Missing guaranteed-tag coverage must be detected before finalizing an encounter, never bypassed with an unrelated creature. Active symbols do not restrict the entire biome pool. Discover does not grant Landing Points or acquire the creature. See the [complete slot specification](../Slot%20Machine%20Mechanic.md) for probabilities, boundaries and future-extension scope. Exact rarity probabilities remain configurable balancing values. Five-tag selection and removal of secondary-tag preference were implemented on 19 November 2026; see [current validation](../Validation/FiveTagAbilities/README.md).

### Phase 2 — Land: challenge

Reveal the creature with a **Resistance** target. Generate **Landing Points** through a skill-based fishing challenge using a limited number of attempts. The existing rig, active creature abilities, Techniques and equipment can enhance performance.

- Reach Resistance within the available attempts to succeed.
- If the target is not reached before attempts run out, fail the encounter and immediately end the expedition.
- The encountered creature is not yet acquired and supplies no newly attached rig benefits to its own landing.
- Success opens new-species placement or automatic duplicate upgrading; failure does not acquire this creature.

### Phase 3 — Build / Upgrade

After successful landing, identify the species by stable creature ID. A species occupies at most one position.

1. A new species is previewed and inserted at any legal gap chosen by the player, at Level 1.
2. A duplicate automatically adds one level and one collected copy to the existing member, preserving its position; no placement or extra confirmation is required.
3. Every successfully landed copy contributes full base sale value. Applicable abilities modify the accumulated species value.
4. Resolve only the appropriate acquisition/upgrade triggers, then recalculate continuous positional effects.
5. Continue automatically to the next encounter or approved progression step.

Existing members retain their relative order and cannot be manually rearranged. The newly acquired/upgraded ability contributes to future challenges, never its own Landing. There is no Catch/Bait choice. See the [Rig Building specification](../Rig%20Building%20Mechanic.md).

## 4. Landing challenge

The 13 November 2026 [Landing Challenge specification](../Landing%20Challenge%20Mechanic.md) defines the current baseline. Its numeric defaults remain prototype tuning; final species ability assignments and numeric balance remain open; all three phase presentations have runtime implementations.

### Interaction and outcome

- A horizontal track contains an independently moving fish indicator and landing zone. The player controls neither element.
- Press **Space** or **STRIKE**. Evaluate overlap at the strike timestamp.
- A smaller centered **Perfect** region has priority over the wider **Good** region. A fish outside Good is a **Miss**. The supplied Miss wording is being clarified; the implementation uses no overlap with Good.
- Every strike consumes an attempt, including Miss. Initially use **four attempts**, configurable for balancing.
- Accumulate encounter-local Landing Points. Reach Resistance to succeed immediately, including on the final attempt; exhaust attempts below Resistance to fail immediately and end the expedition.
- Briefly freeze both indicators for accuracy/scoring feedback. Presentation does not delay authoritative success/failure or allow a duplicate strike.
- The zone follows a predictable oscillation. For now choose Steady, Burst or Erratic uniformly at random once per encounter. Burst is telegraphed; Erratic follows a learnable closed pattern with pauses. Species-specific behavior assignments are future content work.
- The current rig remains visible in committed order and cannot be rearranged. Only previously attached members contribute. The encountered creature cannot help land itself.

Region sizes, motion tuning and the boundary policy remain balancing inputs. The prototype treats touching an interval edge as overlap. No automatic success, traditional combat, health or separate tension resource is introduced.

### Difficulty progression

Resistance targets rise through the expedition. Creature characteristics and rarity may also influence difficulty. Increasing fish speed or shrinking the zone must not be the main escalation.

Rig growth should visibly increase results. Some abilities or upgrades may widen the landing zone or slow the fish, complementing score growth rather than simply erasing the main difficulty escalation.

### Skill and build balance

Timing determines action quality; the rig transforms that quality into a stronger result. Strong builds should make successful actions satisfying while preserving the value of accuracy. Different builds should achieve power through different interactions rather than every creature adding the same bonus.

## 5. Landing Points and scoring

The initial model is:

`Strike Landing Points = (Accuracy Base + Applicable Rig Bonuses) × Product of Applicable Rig Multipliers`

Prototype bases are **Miss 0 / Good 10 / Perfect 20**. Apply eligible flat bonuses before multiplying eligible multipliers. A normal Miss receives no ordinary successful-strike bonuses or multipliers; an explicit Miss scoring ability is required to change that behavior. Retain fractional scores in the current prototype rather than assigning a final integer-rounding policy. Each strike resolves and adds its score once, with source attribution for feedback. Points reset at the next encounter and are not sale value, Gold or permanent progression.

These numbers and the final numeric precision/rounding/special interactions remain open to balancing. Existing creature lists do not gain illustrative scoring abilities automatically. Primary abilities and their level/positional conditions must be explicitly authored. Upgraded species still supply one qualifying position.

Possible ability directions include flat bonuses, multipliers, additional attempts and special scoring interactions. Each exact effect needs an approved trigger, target, duration and limits. Additional attempts are a possible ability type, not a universal purchase during landing.

The player should understand how a skillful strike becomes a visibly stronger result, with satisfying combinations comparable in spirit to Balatro's build growth.

## 6. Ordered rig, upgrades and sale value

The rig contains one ordered position per attached species. New species may be inserted before the first, between members or after the last, subject to explicit ability restrictions. Placement is committed. Future insertions may change adjacency while preserving every existing member's relative order. No manual rearrangement or replacement slot cap is granted.

Each species begins at Level 1. Each additional landed copy increases its level and copy count by one without adding a position. Upgraded members count as one position for tag counts, adjacency and contiguous groups. Levels and copies last only for the expedition.

`Species accumulated base value = species base sale value × number of copies represented by the slot`.

All copies retain their economic contribution. A value multiplier applies to the accumulated species amount unless its ability explicitly says otherwise. Continuous bonuses are recalculated from base value and completed triggered grants; they do not permanently accumulate every time the UI or rig refreshes.

Before means nearer the Rod; after means nearer the Line Tip. Adjacent means immediately before or after. A contiguous group is an uninterrupted matching sequence extending both ways and stopping at the first nonmatching position. No maximum rig size or maximum creature level is established. Numeric and catalog validity must be checked before commitment. A restriction combination that permits no insertion requires a design decision before assignment.

## 7. Creature abilities

Each creature has one primary ability. Its related components may improve Landing Points, multipliers, attempts, accuracy assistance, haul value, Technique interactions or placement constraints. Not every species needs a positional or scoring effect. Upgrades strengthen the same ability's identity; their exact progressions and maximum level remain open.

Individual species, tag assignments, ability descriptions, effect values and level progressions belong in editable game content definitions. Those definitions are the source of truth for species content within these shared design rules; do not maintain a duplicate roster or ability catalog in Markdown. Historical examples and artwork are not current content assignments. Content proposals do not become installed abilities through a documentation update.

Continuous effects respond to insertion, upgrades, removal and changed positional relationships. Acquired and Upgraded triggers are distinct; an upgrade never repeats unrelated acquisition effects. Previews grant nothing. Sources, recipients, conditions, level scaling, trigger timing and stacking must be explicitly authored.

The current implementation supports a frozen primary-ability blueprint, level-scaled Landing bonuses/multipliers, conditional strike rules and attempt grants, scoring-base substitution, zone/motion assistance, positional value effects and insertion restrictions. It retains protected Misses and explicit self-value grants on acquisition or upgrade. Additional effect families require supported handlers before content assignment. Level coefficients, step intervals, caps and short descriptions are authored in game data. See [creature ability authoring](../Creature%20Ability%20Design.md) for stacking and shared contracts.

### Tags and properties

Every creature species, including Apex species, has exactly one gameplay tag and one primary ability expressing that tag's philosophy. Tags describe build identity rather than a complete biological classification. They supply ability conditions but grant no automatic universal powers. There is no additional lead-theme property.

| Tag | Philosophy | Design boundary |
| --- | --- | --- |
| Schooling | Cooperation and shared strength | Relationships and mutual support; may cooperate with other tags rather than requiring only Schooling groups. |
| Predator | Exploitation and decisive payoff | Capitalize on favorable build or encounter conditions; precision itself belongs primarily to Elusive. |
| Anchored | Structure and foundations | Placement is the central decision; establish positions that organize and strengthen the rig. |
| Armored | Dependability and resilience | Reliable performance and recovery; remain useful even when the player makes no mistakes. |
| Elusive | Finesse and mastery | Timing assistance and rewards for skillful execution; broader than Perfect-score multipliers alone. |

Each philosophy may influence Landing Points, attempts, accuracy assistance or haul value. Outcomes are not exclusive to particular tags. Preserve different conditions and player decisions while supporting same-tag synergies, cross-tag support and independently useful abilities. Do not make every tag depend on collecting more of itself. Adaptability, efficiency and concentrated power remain available across the five philosophies.

Fish, Small and Heavy are retired gameplay tags; their descriptive qualities may still inform creature art and ability flavor. Weight's purpose remains undecided. Do not derive Resistance from Weight or restore weight/capacity failure. The five-tag code/content migration is implemented; preserved historical artwork and validation do not define current gameplay.

## 8. Removal

Only an explicitly defined ability may remove a creature; ordinary manual removal is not offered. Recalculate adjacency, contiguous groups, value and placement restrictions afterward. The removal ability must state what happens to collected value and completed triggered grants. Current rig-owner removal preserves collected base value and completed value grants while removing continuous effects. This implementation convention does not finalize a new species ability.

End-of-expedition cleanup is not deliberate removal and cannot fire release effects. Capture the full haul before clearing every temporary member, level and copy.

## 9. Biomes and expedition progression

- Only **Shallows** is initially unlocked.
- Completing a biome permanently unlocks the following biome.
- Every expedition begins in Shallows; traverse earlier biomes to reach later unlocked ones.
- Failure ends the expedition without reversing permanent biome unlocks.
- Failure awards the full accumulated sale value of previously landed creatures, including duplicates; the failed creature is not acquired.
- Success must grant meaningful rewards beyond unlocking the next biome.

Biome length, completion criteria, pacing, biome count, expedition success conditions and what happens after the last available biome remain open. No particular Apex, encounter count or terminal placeholder is an approved completion condition. Whether a newly unlocked biome is entered in the same expedition also needs specification.

Biomes should provide meaningful creature and build variety. Earlier-biome repetition is a playtest risk; allowing deeper starts would conflict with the agreed start rule.

### Expedition failure, success and haul rewards

- Every expedition begins with an empty rig. Collected creatures are temporary expedition build components.
- Failing a Landing Challenge immediately ends the expedition. The failed encounter's creature is not acquired.
- On failure, award the full accumulated sale value of all previously landed creatures, including duplicates, as if the haul had been returned successfully. No accumulated creature value is lost because of failure.
- Successful expedition completion also awards the full accumulated creature sale value. Success and failure use the same full-value haul payout rule.
- Preserve the resolved haul value for payout before clearing the rig. All attached creatures, temporary levels and copy counts are cleared when the expedition ends, regardless of success or failure; they do not become a permanent creature inventory.
- Every subsequent expedition begins with an empty rig while permanent unlocks and progression remain preserved.
- Successful completion may additionally unlock biomes, equipment, creatures available in future expeditions, or other permanent progression rewards. Unlocking creature availability does not carry over an attached creature.

The full-value payout rule is decided. Currency identity, conversion details and the exact additional completion rewards remain open. Neither voluntary surfacing nor Line Capacity is part of the design.

### Rewards and permanent progression

Possible completion rewards include equipment/upgrades, creatures becoming available in future expeditions, additional gameplay options, special unlocks and achievements. These are candidate categories, not a reward table.

Gold-based purchases are not guaranteed to remain the main progression system. Full accumulated creature sale value is awarded at expedition end on both success and failure; whether Gold is used and how value is represented or converted remain open. Successful expeditions should feel more rewarding through additional completion/progression rewards, not by withholding collected creature value on failure.

Achievement unlocks are under consideration. Illustrative objectives: capture 25 Schooling creatures across expeditions; capture five unique Predator creatures in one expedition. These do not finalize counting, targets, rewards or requirements. Existing Journal objectives require review; surfacing and overload objectives are not active requirements.

## 10. Equipment and Techniques

### Equipment

Review all equipment against the new encounter, scoring and rig systems. Rod, Reel and Hook can remain as categories; roles and loadout rules need reconsideration. Existing prices, purchase rules and permanent equipment ownership are not finalized progression.

Individual effects need approval. Weight-, capacity- and overload-related effects need redesign. See [Equipment Passive Rules](../Equipment%20Passive%20Rules.md).

### Techniques

Evaluate compatibility with landing. Techniques may influence points, attempts, accuracy or other strategic interactions. Exact play windows, effects, durations, deck composition and access remain to be determined. Capacity-, weight-, overload- and surfacing-based effects need redesign.

Do not carry forward challenge bypasses or arbitrary play windows merely because a Technique exists. Do not add energy, a turn system, mandatory per-strike card play or automatic refills without approval. Existing deck behavior is implementation reference, not finalized balance.

## 11. Other encounter mechanics

Events, treasure, encounter chains, concealment, replacements and Apex behavior need review against Discover → Land → Build. A reward effect must not silently grant an unchallenged creature where the core loop requires landing one.

Frequency, place in the sequence, Technique windows and completion implications remain open. Preserve separate presentation/data categories while those rules are designed. Do not invent extra tactical landing phases or combat. See [Event Rules Implementation Tasks](../Event%20Rules%20Implementation%20Tasks.md).

## 12. Validation priorities

| Risk | What playtesting must establish |
| --- | --- |
| Repetition | Landing remains enjoyable across many encounters. |
| Skill versus build | Accuracy matters and rig decisions substantially improve results. |
| Difficulty | Resistance creates pressure without relying mainly on speed or tiny zones. |
| Creature variety | Learnable movement and abilities produce distinct encounters. |
| Rig complexity | Positioning offers strategy without every creature becoming complicated. |
| Earlier biomes | Mandatory traversal remains worthwhile after deeper unlocks. |
| Completion rewards | Additional completion/progression rewards make success worthwhile while both outcomes pay full accumulated creature value. |
| Balance | Powerful combinations do not consistently trivialize future challenges. |

A focused playtest can evaluate proposed controls, attempt counts or formulas without making them final. Production work must keep authored tuning and runtime state separate and deliver complete clean increments.

## 13. Open decision register

| Area | Decisions still required |
| --- | --- |
| Slot machine | Exact rarity probability tables; no secondary-tag preference or extra landing connection. Four-of-five preparation and match rules are specified in the Slot Machine Mechanic supplement. |
| Landing | Confirm the supplied Miss wording; final zone widths, boundary policy, attempts, movement tuning and feedback balance. Space/button strikes, graded overlap and immediate terminal outcomes are specified. |
| Scoring | Final base values, numeric precision/rounding/limits and special interactions. Initial flat-before-product model, ordinary Miss-zero rule and source attribution are specified. |
| Difficulty | Resistance curves, rarity/characteristic influence and skill/build balance. |
| Rig | Orientation, restriction combinations, no-legal-position handling and effect order. |
| Creature content | Complete and validate editable game definitions: one tag per species, primary abilities, level scaling, numbers, durations and movement. Resolve tentative assignments in content authoring, without a duplicate Markdown catalog. |
| Ability interactions | Continue balancing authored coefficients, step intervals and assistance caps; current stacking, timing and positional contracts are specified in Creature Ability Authoring. Additional effect families need explicit rules and handlers. |
| Weight | Whether retained and for what purpose; Heavy is retired as a gameplay tag. |
| Ability-driven removal | Access, timing, targets, costs, triggers, lifetimes and placement consequences. |
| Techniques | Windows, effects, deck/hand/draw configuration and progression. |
| Equipment | Category roles, effects, loadout, acquisition and ownership. |
| Biomes | Completion, length, transitions, unlock entry timing and expedition success endpoint. |
| Rewards | Additional biome/expedition rewards, currency representation/conversion and whether Gold remains; both success and failure already award full accumulated creature sale value at expedition end. |
| Achievements | Whether used, counting, grants and persistence. |
| Other encounters | Events, treasure, Apex, chains, concealment and replacements. |
| Presentation | Slot/landing layouts, Resistance display, weight fields and insertion feedback. |

Resolve these through explicit design decisions and playtesting. Examples and existing assets do not fill the gaps automatically.
