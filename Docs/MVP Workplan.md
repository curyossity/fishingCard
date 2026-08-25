# Fishing Card Roguelike — MVP Workplan

## MVP Goal

Build a playable vertical slice that answers the main question:

> **Is it fun to descend through card encounters, accumulate creatures on a fishing line, manage their effects and Line Load, manipulate encounters with technique cards, and decide when to surface?**

The MVP should feel like a small but complete run of the game rather than a collection of disconnected prototypes.

### MVP Target

A player should be able to:

- Start a run.
- Enter one biome.
- Encounter creatures, hazards, opportunities, and treasure.
- Have catchable creatures become Hooked.
- Use technique cards to manipulate encounters.
- Descend and commit creatures into the Catch Chain.
- Gain passive and negative effects from caught creatures.
- Accumulate Line Load.
- Become overloaded.
- Release catches to reduce load.
- Decide whether to continue or Surface.
- Reach a special biome-ending Apex Encounter.
- Carry the Apex catch and its effect if continuing.
- Finish the run and receive the value of everything successfully brought back.
- Use a minimal reward/progression system.
- Start another run with some variation.

---

# Phase 1 — Build the Core Card Framework

**Goal:** Establish the technical foundation for every card type.

## 1.1 Generic card data

Create a reusable card data structure.

- [ ] Unique ID.
- [ ] Card name.
- [ ] Card type.
- [ ] Description.
- [ ] Artwork reference.
- [ ] Tags.
- [ ] Weight where applicable.
- [ ] Value where applicable.
- [ ] Effect definitions.
- [ ] Biome/depth availability.
- [ ] Rarity.

## 1.2 Card categories

Support at least:

- [ ] Creature.
- [ ] Treasure.
- [ ] Hazard.
- [ ] Environment.
- [ ] Opportunity.
- [ ] Technique.
- [ ] Apex Encounter.

## 1.3 Effect system

Avoid hardcoding every card as a completely separate script.

Create reusable effects such as:

- [ ] Add/remove Line Load modifier.
- [ ] Modify catch value.
- [ ] Modify future encounter properties.
- [ ] Affect specific creature tags.
- [ ] Hide encounter information.
- [ ] Replace encounter.
- [ ] Avoid encounter.
- [ ] Release catch.
- [ ] Modify next Descend.
- [ ] Modify another caught card.
- [ ] Trigger when caught.
- [ ] Trigger while attached.
- [ ] Trigger when released.
- [ ] Trigger on Descend.
- [ ] Trigger when Surface begins.

## 1.4 Debug tools

- [ ] Spawn a specific encounter.
- [ ] Give the player a specific technique card.
- [ ] Change depth manually.
- [ ] Change Line Capacity manually.
- [ ] Add/remove catches manually.
- [ ] Print active effects.
- [ ] Restart the run instantly.

### Phase 1 Deliverable

A generic card/effect architecture capable of creating new cards mostly through data rather than new code.

### Exit Gate

Create several mechanically different test cards without modifying the fundamental card system.

---

# Phase 2 — Implement the Core Fishing Loop

**Goal:** Make the game playable with placeholder cards.

## 2.1 Start a run

- [ ] Initialize Line Capacity.
- [ ] Initialize empty Catch Chain.
- [ ] Initialize player deck.
- [ ] Draw starting hand.
- [ ] Set starting biome/depth.
- [ ] Reveal first encounter.

## 2.2 Hooked encounter

- [ ] Show current Encounter card.
- [ ] Mark catchable creature as Hooked.
- [ ] Allow technique-card interaction.
- [ ] Track effects affecting the Hooked encounter.

## 2.3 Descend

- [ ] Commit Hooked creature to Catch Chain.
- [ ] Apply its catch effects.
- [ ] Add its weight.
- [ ] Advance depth.
- [ ] Update encounter pool if depth tier changes.
- [ ] Reveal next encounter.
- [ ] Refill technique hand as required.

## 2.4 Release

- [ ] Select an existing caught card.
- [ ] Remove it from Catch Chain.
- [ ] Remove its Line Load.
- [ ] Remove its active effects.
- [ ] Lose its eventual value.
- [ ] Keep the current Hooked encounter unresolved.
- [ ] Do not advance depth.

## 2.5 Surface

- [ ] Initiate Surface.
- [ ] Resolve any Line Load consequences.
- [ ] Determine successful haul.
- [ ] End run.
- [ ] Calculate haul value.
- [ ] Display run summary.

### Phase 2 Deliverable

A complete gameplay loop using simple placeholder cards.

### Exit Gate

You should be able to play:

**Encounter → Technique → Descend → Catch → Encounter → Release → Descend → Surface**

without debug commands.

---

# Phase 3 — Catch Chain & Line Load Prototype

**Goal:** Determine whether the game's signature mechanic is actually interesting.

## 3.1 Catch Chain

- [ ] Display catches in acquisition order.
- [ ] Clearly show each catch's weight.
- [ ] Clearly show each catch's value.
- [ ] Clearly show active effects.
- [ ] Clearly show negative effects.
- [ ] Update chain immediately after catching/releasing.

## 3.2 Line Load

- [ ] Display current Load / Capacity.
- [ ] Make approaching capacity visually obvious.
- [ ] Make overload visually obvious.
- [ ] Implement overload consequences.
- [ ] Allow player to remain overloaded rather than immediately failing.

## 3.3 Catch interactions

Implement at least five types of catch interaction:

- [ ] Attraction effect.
- [ ] Value synergy.
- [ ] Weight/load modification.
- [ ] Negative persistent effect.
- [ ] Effect targeting another catch.

## 3.4 Test the important decision

Create scenarios where:

- [ ] A low-value creature is mechanically useful.
- [ ] A high-value creature is dangerously heavy.
- [ ] Releasing the lowest-value card is not automatically the best choice.
- [ ] Being slightly overloaded can be strategically reasonable.
- [ ] Continuing deeper while overloaded can be tempting.

### Phase 3 Deliverable

A small sandbox where 8–10 creatures can form meaningfully different Catch Chains.

### Critical Exit Gate

Ask:

> **Does deciding what to keep attached to the line feel interesting?**

If **no**, stop expanding the game and redesign this system.

This is the most important validation point in the MVP.

---

# Phase 4 — Technique Deck

**Goal:** Make the player's deck define *how* they fish.

## 4.1 Hand system

- [ ] Four-card hand.
- [ ] Draw system.
- [ ] Discard system.
- [ ] Deck reshuffling rules.
- [ ] Card-use restrictions.
- [ ] Clear indication of playable/unplayable cards.

## 4.2 Initial technique categories

Create techniques for:

### Encounter manipulation

- [ ] Avoid an unwanted creature.
- [ ] Replace an encounter.
- [ ] Alter what kinds of encounters appear next.
- [ ] Reveal hidden information.

### Catch manipulation

- [ ] Reduce a catch's weight.
- [ ] Increase a catch's value.
- [ ] Release a catch more efficiently.
- [ ] Manipulate interaction between catches.

### Descent manipulation

- [ ] Descend differently.
- [ ] Influence the next depth pool.
- [ ] Take additional risk for greater reward.

### Load manipulation

- [ ] Temporary capacity increase.
- [ ] Reduce effective weight.
- [ ] Reward overloaded play.

## 4.3 Prototype deck size

Target approximately:

**12–20 technique-card designs**

for the MVP.

Do not create dozens yet.

### Phase 4 Deliverable

At least 2–3 noticeably different ways to play the same encounter sequence.

### Exit Gate

A player receiving a difficult creature should sometimes think:

> "Which card should I use?"

rather than simply:

> "Do I have the Skip card?"

---

# Phase 5 — First Complete Biome

**Goal:** Build one biome with enough content to test pacing and repetition.

This should be the MVP's biggest content phase.

## 5.1 Choose biome identity

Use a relatively understandable biome such as **Coastal Waters**.

Possible core identity:

**Schools + small catches + predator attraction.**

## 5.2 Depth structure

Create approximately three internal tiers:

### Shallows

- [ ] Introduce basic creatures.
- [ ] Low Line Load pressure.
- [ ] Teach Catch Chain interactions.

### Mid-depth

- [ ] Introduce more meaningful synergies.
- [ ] Add hazards and opportunities.
- [ ] Increase average weight/value.

### Deep edge

- [ ] Introduce predators.
- [ ] Increase dangerous effects.
- [ ] Prepare for Apex encounter.

## 5.3 Encounter content target

For MVP, aim for approximately **18–25 encounters**, not the final 25–35+ target.

Example distribution:

- [ ] 8–10 creatures.
- [ ] 3–4 hazards/environment encounters.
- [ ] 2–3 treasures/opportunities.
- [ ] 2–3 rare encounters.
- [ ] 1–2 encounter chains.
- [ ] 2 Apex Encounter possibilities.

## 5.4 Prevent repetition

Verify that the biome does not become:

**fish → fish → fish → fish**

Test sequences containing:

- [ ] Creature.
- [ ] Opportunity.
- [ ] Creature.
- [ ] Hazard.
- [ ] Treasure.
- [ ] Creature interaction.
- [ ] Rare encounter.
- [ ] Apex.

### Phase 5 Deliverable

One biome that takes enough decisions to feel like a meaningful section of a run.

### Critical Exit Gate

Play the biome repeatedly.

After several runs, ask:

> **Am I making different decisions, or merely seeing different card artwork?**

If decisions repeat despite different cards, improve interactions before increasing card count.

---

# Phase 6 — Biome Apex Encounter

**Goal:** Give the biome a memorable climax.

## 6.1 Apex system

- [ ] Trigger Apex at biome boundary.
- [ ] Remove Apex cards from normal encounter pool.
- [ ] Select one Apex from biome possibilities.
- [ ] Present it as a clearly special encounter.
- [ ] Allow technique interaction.
- [ ] Commit Apex to Catch Chain through normal game rules.

## 6.2 Apex design

Create at least **two Apex creatures**.

Example design goals:

### Apex A

- Very high value.
- Very heavy.
- Powerful positive ongoing effect.

### Apex B

- Different strategic identity.
- Meaningful downside.
- Alters future gameplay.

## 6.3 Carry-forward behavior

- [ ] Apex remains in Catch Chain.
- [ ] Apex contributes Line Load.
- [ ] Apex effect continues.
- [ ] The player does not reset between biomes.

For the MVP, the next biome does **not** need to be fully implemented.

After catching the Apex, a temporary card can indicate:

> **The next waters lie ahead...**

The run can then transition to the Surface/end-of-MVP flow.

### Phase 6 Deliverable

The first biome has an identifiable climax that changes the player's Catch Chain.

---

# Phase 7 — Minimal Roguelike Progression

**Goal:** Give repeated runs a purpose without building the full metagame.

Keep this extremely small for the MVP.

## 7.1 Run rewards

- [ ] Add together successful catch values.
- [ ] Award Gold.
- [ ] Show catches brought home.
- [ ] Show catches released/lost.

## 7.2 Between-run choices

Implement one minimal progression mechanism.

Recommended MVP version:

After a run, Gold can unlock a small selection of:

- [ ] Technique cards.
- [ ] Starting deck alternatives.
- [ ] Line Capacity upgrades.
- [ ] Equipment with simple mechanical effects.

Do **not** build a large permanent upgrade tree yet.

## 7.3 Deck modification

- [ ] Add unlocked technique card.
- [ ] Remove/replace a technique card if appropriate.
- [ ] Save deck between runs.

### Phase 7 Deliverable

Completing one run changes what can happen in the next run.

### Exit Gate

Progression should create:

> "I want to try another run with this new possibility."

It should not merely increase numbers.

---

# Phase 8 — MVP UX and Visual Identity

**Goal:** Make the prototype understandable and start establishing the game's own visual language.

Do not aim for final art yet.

## 8.1 Main gameplay composition

Prototype the screen around:

- [ ] Boat/start card.
- [ ] Current Encounter card.
- [ ] Catch Chain.
- [ ] Visible connecting fishing line/rig.
- [ ] Four-card technique hand.
- [ ] Line Load indicator.
- [ ] Current depth/biome.
- [ ] Core actions.

## 8.2 Catch Chain identity

Ensure the presentation does **not** resemble cards simply forming one continuous illustrated scene.

Instead:

- [ ] Every creature card is visually independent.
- [ ] Cards appear physically attached to the central fishing rig.
- [ ] Catch Chain is immediately recognizable.
- [ ] Line connectors communicate attachment.
- [ ] Active catch effects are visible without opening menus.

## 8.3 Card readability

Each creature card should communicate quickly:

- [ ] Name.
- [ ] Illustration.
- [ ] Weight.
- [ ] Value.
- [ ] Effect.
- [ ] Relevant tags.

Technique cards should communicate:

- [ ] Name.
- [ ] Effect.
- [ ] Target requirements.
- [ ] Whether currently playable.

## 8.4 Feedback

Even with minimal/no character animation:

- [ ] Clear feedback when a creature becomes Hooked.
- [ ] Clear feedback when committed to Catch Chain.
- [ ] Clear feedback when Load changes.
- [ ] Clear feedback when overloaded.
- [ ] Clear feedback when an effect activates.
- [ ] Clear feedback when something is Released.
- [ ] Strong reveal treatment for Apex Encounter.

### Phase 8 Deliverable

A player unfamiliar with the code can understand what is happening without developer explanation.

---

# Phase 9 — Balancing & Playtesting

**Goal:** Determine whether the MVP is genuinely fun.

## 9.1 Internal balancing

Track:

- [ ] Average number of encounters before Surface.
- [ ] Average number of catches.
- [ ] Average maximum Line Load.
- [ ] How frequently players become overloaded.
- [ ] How frequently catches are Released.
- [ ] Typical haul value.
- [ ] Technique cards most/least played.
- [ ] Encounters most frequently avoided.
- [ ] Apex catch rate.
- [ ] Run success/failure rate.

## 9.2 Decision-quality testing

After encounters, ask:

- [ ] Was there an obvious correct choice?
- [ ] Did Catch Chain composition influence the choice?
- [ ] Did current Load influence the choice?
- [ ] Did the technique hand influence the choice?
- [ ] Did depth influence the choice?
- [ ] Did the decision affect later encounters?

The strongest encounters should involve several of these simultaneously.

## 9.3 Repetition testing

Play at least:

- [ ] 5 runs yourself.
- [ ] 10+ runs across external testers.

Look for:

- [ ] Repeated encounter sequences.
- [ ] Repeated optimal decisions.
- [ ] Technique cards that are always/never useful.
- [ ] Creatures automatically Released every time.
- [ ] Creatures automatically desirable every time.
- [ ] Sections where players stop reading cards.

## 9.4 Player comprehension

Check whether players understand:

- [ ] Why a creature was caught.
- [ ] Why they cannot simply Skip it.
- [ ] What Release costs them.
- [ ] What Line Load means.
- [ ] Why being overloaded is dangerous.
- [ ] Which effects are currently active.
- [ ] Why they might intentionally keep a weak creature.
- [ ] Why they might continue deeper despite risk.

### Phase 9 Deliverable

A playtest report containing:

**Keep / Change / Remove / Investigate**

for every major mechanic.

---

# Phase 10 — MVP Polish

**Goal:** Turn the validated prototype into something presentable.

Only do this after the previous gameplay gates pass.

## 10.1 Content cleanup

- [ ] Rewrite placeholder card names.
- [ ] Rewrite effect text consistently.
- [ ] Standardize terminology.
- [ ] Remove redundant cards.
- [ ] Improve weak encounters.
- [ ] Balance extreme cards.

## 10.2 Visual pass

- [ ] Establish final-ish card frame direction.
- [ ] Establish typography.
- [ ] Establish color language.
- [ ] Create representative creature art.
- [ ] Create Apex visual treatment.
- [ ] Improve Catch Chain readability.

Final artwork for every card is **not required** for MVP.

## 10.3 Audio

Minimal audio can dramatically improve a card-only game.

- [ ] Card draw.
- [ ] Card attach/hook.
- [ ] Descend.
- [ ] Release.
- [ ] Load warning.
- [ ] Apex reveal.
- [ ] Surface/result.

## 10.4 Technical cleanup

- [ ] Save/load.
- [ ] Settings.
- [ ] Restart run.
- [ ] Error handling.
- [ ] Resolution/UI scaling.
- [ ] Performance check.
- [ ] Build executable.

---

# Phase 11 — MVP Evaluation

Do **not** immediately start Biome 2.

Evaluate the game first.

## Question 1 — Catch Chain

> Is building and dismantling the Catch Chain fun?

- [ ] Yes
- [ ] No
- [ ] Needs revision

## Question 2 — Push Your Luck

> Do players genuinely hesitate between Descend and Surface?

- [ ] Yes
- [ ] No
- [ ] Needs revision

## Question 3 — Load

> Does Line Load create interesting pressure instead of annoying inventory management?

- [ ] Yes
- [ ] No
- [ ] Needs revision

## Question 4 — Technique Deck

> Does the player's deck create meaningfully different approaches?

- [ ] Yes
- [ ] No
- [ ] Needs revision

## Question 5 — Encounters

> Does the biome feel like a journey rather than a sequence of fish?

- [ ] Yes
- [ ] No
- [ ] Needs revision

## Question 6 — Apex

> Does reaching the Apex feel like a meaningful climax?

- [ ] Yes
- [ ] No
- [ ] Needs revision

## Question 7 — Replayability

> After completing a run, do testers voluntarily want another?

- [ ] Yes
- [ ] No
- [ ] Needs revision

---

# MVP Scope Summary

### Build now

- [ ] Core card architecture.
- [ ] Encounter system.
- [ ] Hooked state.
- [ ] Catch Chain.
- [ ] Descend.
- [ ] Release.
- [ ] Surface.
- [ ] Line Load and overload.
- [ ] Four-card technique hand.
- [ ] Small technique deck.
- [ ] One complete biome.
- [ ] Multiple encounter categories.
- [ ] Depth tiers.
- [ ] Approximately 18–25 biome encounters.
- [ ] Two Apex Encounter variants.
- [ ] Minimal run rewards.
- [ ] Minimal progression.
- [ ] Functional cards-only UI.
- [ ] Basic save/load.
- [ ] Playtesting and analytics.

### Explicitly postpone

- [ ] Multiple complete biomes.
- [ ] Hundreds of creature cards.
- [ ] Large permanent upgrade tree.
- [ ] Complex equipment system.
- [ ] Final artwork for every card.
- [ ] Extensive story/lore.
- [ ] Achievements.
- [ ] Collection encyclopedia.
- [ ] Multiple characters/classes.
- [ ] Daily challenges.
- [ ] Online functionality.
- [ ] Steam/platform integration.
- [ ] Localization.
- [ ] Advanced accessibility/settings suite.
- [ ] Full tutorial campaign.

---

# Recommended Development Order

For day-to-day development, follow this dependency order:

**Rules**\
→ **Card architecture**\
→ **Encounter / Hooked**\
→ **Catch Chain**\
→ **Line Load**\
→ **Descend / Release / Surface**\
→ **Creature effects**\
→ **Technique cards**\
→ **Biome encounter system**\
→ **Biome content**\
→ **Apex**\
→ **Rewards/progression**\
→ **UX**\
→ **Playtesting**\
→ **Polish**

Avoid building the shop, progression tree, multiple biomes, or large card libraries before the **Catch Chain + Load + Descend/Surface loop has passed playtesting**.

# MVP Definition of Done

The MVP is complete when a new player can launch the game, play several complete runs through one biome, build different Catch Chains, experience meaningful overload decisions, use a small technique deck to manipulate encounters, reach different Apex creatures, surface with a haul, make a small between-run progression choice, and willingly start another run.

The MVP is **not** complete merely because all planned systems have been implemented.

It is complete when the core loop has demonstrated that it is worth expanding.
