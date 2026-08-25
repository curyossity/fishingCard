# GDD Implementation Brief

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

- Core action cards: always available and not shuffled into the player deck. Prototype actions are `Descend`, `Release`, and `Surface`.
- Player technique cards: the 4-card hand and deckbuilding layer. These include bait, hooks, knots, rigging tricks, line reinforcement, descent modifiers, encounter manipulation, and catch manipulation.
- Encounter cards: generated or drawn by the game based on depth, biome, current catches, and modifiers. This group includes creatures, treasure, hazards, environment cards, opportunities, discoveries, and biome-specific cards.

Do not put fundamental actions into the player deck. Bad draws should create tactical limitations, not prevent basic fishing.

## Core Actions

- `Descend`: always available. Advances deeper and normally commits the current Hooked creature to the Catch Chain unless a technique or special effect avoids, redirects, or replaces it.
- `Release`: always available. Drops an existing caught creature/item from the Catch Chain, immediately reducing Line Load but losing value and passive/run effects. Release does not advance depth and does not resolve the current Hooked encounter.
- `Surface`: always available. Ends the descent and attempts to bring the remaining Catch Chain home as the haul.

The game is structured around meaningful actions, not a separate traditional turn system.

## Encounter States

- `Encountered`: revealed and visible according to the current rules.
- `Hooked`: the current unresolved catchable encounter attached to the rig. This is the player's reaction window.
- `Caught`: committed to the Catch Chain after the player Descends without avoiding or replacing the Hooked creature.

There is no free universal Skip action. Avoiding an unwanted catch requires a technique, equipment effect, creature effect, or other specific rule.

## Catch Chain

Caught creatures/items remain attached to the fishing rig while the player continues descending. The chain is both a visible vertical record and a mechanical system.

- Every caught card usually contributes value and Line Load.
- Many caught cards can provide passive effects while attached.
- Existing catches can influence future encounters.
- Releasing a catch removes its weight, value, passive effects, attraction modifiers, and synergies.
- The line visually connects the boat/start card to caught cards below it.

## Line Load

Line Load is the main pressure system.

- Each catch has a weight/load contribution.
- Equipment defines safe line capacity.
- Over capacity is allowed.
- Overload increases risk rather than causing immediate automatic failure.
- The further above capacity the player goes, the more dangerous descent, catching, and surfacing can become.

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

## Player Deck

The player has approximately a 4-card technique hand. After using a technique card, a replacement is drawn according to final deck rules.

Technique cards should interact with:

- Depth
- Line Load
- Creature tags
- Catch Chain state
- Current Hooked encounter
- Risk

Some technique cards should act as escape valves for mandatory catches by bypassing, replacing, cleansing, neutralizing, or otherwise managing Hooked encounters.

## Economy And Progression

Value is realized primarily after surfacing, not immediately when a creature is caught. Only catches still attached and successfully returned become the haul.

The haul can be sold or used for progression such as equipment improvements, new cards, deck upgrades, and unlocks. Long-term progression should expand strategic possibilities rather than only increasing raw power.

## Prototype Scope

Build the first prototype around:

- 3 permanent core actions: `Descend`, `Release`, `Surface`.
- A 4-card technique hand and small technique deck.
- A small encounter pool of roughly 15-20 cards across creatures, hazards, opportunities, treasure, and at least one short encounter chain.
- 2-3 downside creatures.
- Several technique cards that manage bad Hooked encounters.
- A visible vertical Catch Chain.
- Line Load and capacity with overload risk.
- Simple passive effects on selected catches.
- A few depth bands with different encounter pools.
- A basic end-of-run haul and sell screen.

Prototype success criterion: is it fun to build an increasingly valuable and mechanically useful Catch Chain while deciding whether to release something, keep descending, or surface?

## Open Questions

Do not lock these without explicit design approval or prototype evidence:

- Exact overload consequences.
- Whether catch-chain order/position has rules.
- Exact draw count, deck size, discard/shuffle rules, and rarity structure.
- How locations/biomes are selected.
- Permanent versus run-specific meta-progression.
- Exact biome length, depth-tier count, encounter-pool size, and visit length.
