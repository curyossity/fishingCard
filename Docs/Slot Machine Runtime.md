# Slot machine runtime

Implemented 7 November 2026 against the [agreed slot specification](Slot%20Machine%20Mechanic.md). Discover replaces the tag bar in `Assets/UIThemes/MidnightFieldJournal/Scenes/MidnightFieldJournalComparison.unity`. The [approved Discover artwork is integrated](Validation/TackleDiscoverArt/README.md). Existing source scenes and saved progression are preserved.

## Ownership

- `TagSlotMachineRuntime`: three authoritative faces, one locked reel, spin state, respin budget, monotonic session/revision identities and immutable results. The baseline owner supplies four equal symbols and one optional respin. A respin requires exactly one lock. Every accepted command advances the revision; duplicate/stale commands are rejected.
- `SlotMatch`: pure three-different/pair/triple classification with mandatory and secondary tags.
- `SlotMachineProfile`: authored rarity weights and secondary-tag preference strength. `SlotSelectionSettings` copies these into an immutable expedition snapshot and validates progressively improved higher-rarity probabilities.
- `SlotCreatureSelector`: distinct eligible normal creatures, separate rarity roll, nearest-tier fallback inside the guaranteed tag (lower-tier ties), then weighted species selection. Secondary preference applies within the resolved tier and never excludes candidates lacking the secondary tag.
- `BiomeDefinition.GetStandardCreaturePool()`: union of normal creatures from all authored biome tiers. Standard Discover ignores individual normal-creature depth restrictions. Apex remains outside the pool and retains its existing separate conditions.
- `CardDefinition.RequiredCreatureUnlockId`: optional unlock evidence requirement. Without evidence, a required creature unlock is denied. `FishingRunController.CreatureUnlockIsGranted` is the progression integration boundary; this increment invents no unlock reward table or save system.
- `EncounterRuntime`: holds the candidate snapshot, rechecks eligibility at commitment, reveals once and owns the pending landing gate. It validates coverage of the four selected tags before starting Discover. Missing coverage produces an explicit error rather than changing symbols or dropping the guarantee.
- `FishingRunController`: freezes the player's four selected tags/settings, forwards validated commands, keeps the fixed respin result visible until acceptance and publishes `CreatureReadyForLanding(creature, encounterRevision)` only on Accept Tags.
- `SlotTagPreparationView` and `SlotMachineView`: native preparation and occurrence-bound Discover commands. The Harbor modal blocks background interaction, requires four explicit selections, and allows editing before each expedition. `TackleDiscoverPresentation` supplies the approved artwork and live rig/haul state. Views do not choose creatures or determine legality.

Rarity is rolled independently of the number of species in each tier. Nonmatching outcomes retain access to the entire eligible normal pool, including tags outside the active four. No tag fatigue, precision zones, landing-point bonuses or repeated respin action are carried into this path.

## Provisional balancing

`Assets/SlotMachine/SlotMachine.asset` exposes the following relative weights. These are implementation tuning for testing, **not finalized balance**.

| Outcome | Common | Uncommon | Rare | Legendary |
| --- | --- | --- | --- | --- |
| Three different | 70 | 22 | 7 | 1 |
| Pair | 55 | 28 | 14 | 3 |
| Triple | 35 | 35 | 23 | 7 |

The pair's secondary-tag species weight is initially 2 versus 1. All tables and that multiplier are editable independently of reel/match logic. Values are frozen when an expedition starts. No equipment, Technique, creature or biome modifier changes the baseline reels.

## Installation and boundary

**Fishing Cards > Slot Machine > Integrate Approved Discover Art** imports the original approved sprites/fonts and installs the separate Discover canvas. The existing **Install Plain Controls** profile/setup command, comparison-scene regeneration and Harbor regeneration preserve the artwork binding once its prefab exists. Existing tag/rarity timing objects are retained inactive with their controller bindings cleared. Obsolete Descend/Surface controls and capacity meters are hidden. Original artwork files and other card masters are unchanged; this theme uses its own blank rig faces and independent portraits/text.

Set Sail opens tag preparation. Confirming four tags starts the expedition and animates the initial spin. Accept reveals a creature; alternatively, lock one reel and respin the other two once. After the animation settles, the final reels remain visible with lock/respin disabled and Accept Tags enabled. There is no timeout. Only explicit acceptance selects the creature and starts the landing handoff.

### Cylindrical reel presentation

Both initial spin and respin use 2D cylinder projection. Paper texture, divider lines, blank name plaques, illustrations and native tag captions rotate together. They compress toward the top/bottom edges; native graphics fade into the cylinder's edge shadow. The housing, rim and directional shading stay fixed. Neighboring faces remain part of the continuous surface at rest. No text is baked into images and the original artwork/font materials are preserved. Only the expedition's four symbols appear during motion; animation never calls the gameplay RNG.

`TackleReelSurface` owns one private material per wheel. The surface shader uses the exact symbol angle and radius, with inverse cylindrical UV projection; a separate flat paper texture supplies moving grain, and authored UV regions sample the original drum's dividers and blank plaque. Mirrored periodic sampling avoids paper-tile seams. Runtime material instances are retained across Discover visibility cycles and released on destruction. **Fishing Cards > Slot Machine > Install Rotating Reel Surface** installs the background layers without replacing the original art, controls or scene overrides. Full art and cylindrical-animation regeneration retain this composition.

`TackleDiscoverPresentation` authors duration and stagger (initially 1.05 seconds plus 0.15 seconds per moving reel). Each moving reel decelerates past the selected position and snaps back with a damped elastic recoil. `TackleReelAnimator` authors overshoot (initially 0.45 of a symbol, or 40.5°), settling fraction (34% of the existing duration) and elastic period (0.47). A smaller return bounce dies away to the exact selected position. Forward travel and recoil meet at zero velocity; the endpoint is exact and uses unscaled time. This applies the user's elastic-easing reference to the stop only, without adding a JavaScript dependency or changing the total spin duration. The locked reel does not move during a respin. Controls and direct controller commands are blocked through the recoil; match lamps and the result summary appear after all moving reels stop. The resolved result remains until explicit Accept Tags, including when a respin happens to return identical symbols. `SpinRevision` prevents lock-only refreshes from restarting the animation.

**Fishing Cards > Slot Machine > Install Cylindrical Reel Animation** updates the existing Discover prefab without rebuilding approved art or replacing scene overrides. Full art regeneration includes the same composition. All eight baseline tags have reel illustrations; [six supplemental symbols](Validation/TackleReelSymbols/README.md) extend the original Fish and Schooling family. Tag captions remain native text. **Install Remaining Reel Symbols** imports and binds those additions without changing selection rules or the existing artwork.

The selected creature is **awaiting landing**, not acquired. `AwaitingLandingChallenge` blocks attachment, Catch/Bait choice, Techniques and legacy Descend/Surface; the new creature supplies no rig benefit. Reels are hidden during this handoff. Landing input, Resistance/scoring, success/failure resolution and Build continuation are not implemented by this slot increment. It stops at the occurrence-bound landing handoff and does not fabricate a successful catch. Other event/forced-chain redesigns remain separate work.

## Validation

`SlotMachineRuleChecks.ValidateRules()` checks deterministic reel state, all initial combinations and the stated respin strategy probabilities, guaranteed/secondary tags, rarity fallback/ties, biome membership, depth independence, Apex exclusion, unlock evidence, reset/stale commands and no pre-landing acquisition.

`SlotMachineSceneChecks.ValidateLive(false)` and `ValidateLive(true)` exercise Harbor preparation, plain controls and the acceptance/respin branches in fresh Play Mode sessions without modifying saved progression. Evidence and actual check counts are recorded under [Validation/SlotMachine](Validation/SlotMachine/README.md). The original tag-timing checks remain historical fixtures, not validation of Discover.

`SlotReelAnimationChecks.Begin()` runs frame-driven animation checks in a fresh comparison Play Mode session. Existing synchronous control fixtures explicitly settle visual motion; they do not replace the frame-driven checks. See [animation evidence](Validation/SlotReelAnimation/README.md).

The [rotating-surface validation](Validation/RotatingReelSurface/README.md) covers shared paper/symbol phase, independent materials, locked-wheel immobility, fixed rim pixels and the complete initial/respin/review flow. Pass an output directory to `SlotReelAnimationChecks.Begin(output)` to retain new captures separately from historical evidence.

`SlotReelStopChecks.ValidateAndCapture()` samples the real prefab's overshoot, reverse motion, exact endpoint and frame-rate independence. See [elastic-stop validation](Validation/ElasticReelStop/README.md) for the authored tuning and pose preview.
