# Deepwater — Landing Challenge

Agreed direction supplied 13 November 2026. Focus: Landing motion, strikes, attempts, scoring and terminal outcomes. Numeric defaults are prototype balancing values. Build/Upgrade is specified separately in Rig Building Mechanic.md; species assignments, Techniques and equipment tuning remain open. The approved Landing artwork was integrated on 13 November 2026.

## Encounter flow

Discover selects and reveals an eligible creature. Land tests that creature against the **already attached rig**. The encountered creature contributes nothing toward landing itself.

Reach the creature's **Resistance** with accumulated **Landing Points** within limited attempts. Success immediately completes Land and supplies a creature/occurrence-bound handoff to Build. Success does not grant the encountered creature power toward its own Landing. Build then previews/commits a new species at Level 1 or automatically upgrades an existing species in place, preserving relative order. The updated rig affects later encounters.

Failure immediately ends the expedition. The failed creature is not acquired. Award the full accumulated sale value of all previously landed creatures, including duplicates and applicable value bonuses. Snapshot that value before removing every attachment. Permanent progression is preserved; the next expedition starts with an empty rig. There is no voluntary Surface, capacity check, overload or line-break penalty.

## Track and strikes

- A horizontal track contains a clearly visible moving fish indicator and an independently moving landing zone.
- The zone has a wider **Good** region and a smaller centered **Perfect** region.
- The player controls neither moving element. Press **Space** or **STRIKE** to act.
- Evaluate their positions at the exact strike timestamp, using the fish indicator's interval rather than an unrelated sprite/pixel hit box.
- Perfect overlap takes priority. Good overlaps the wider region without overlapping Perfect. Miss has no overlap with Good.
- Prototype boundary policy: touching an interval edge counts as overlap. Track widths, zone widths and motion are configured in normalized units, independent of screen resolution.

**Clarification pending:** the supplied Miss line said the fish overlaps the landing zone, conflicting with the two successful grades. The current interpretation is **Miss = no overlap with Good**, consistent with the rest of the description; this was raised with the user for confirmation.

Every accepted strike consumes one attempt, including Miss. Initially use **four attempts**, subject to balancing. Show attempts remaining at all times. Reaching Resistance succeeds immediately, including on the final attempt; otherwise exhausting attempts fails immediately. No challenge timer, automatic strike, held-key repetition or extra tactical decision is introduced.

## Motion

The zone follows a predictable oscillation while remaining entirely on the track. The fish also stays on the track. Select **one of three patterns uniformly at random for each encounter**; individual species assignments are not part of this increment.

| Pattern | Behavior |
| --- | --- |
| Steady | Consistent travel reflected at track endpoints |
| Burst | Normal travel, visibly telegraphed slowdown, then a short burst |
| Erratic | A repeatable closed sequence of direction changes and pauses |

Choose the fish's starting phase once. Do not reroll motion on strikes or use fresh frame-by-frame randomness. Analytical motion should give the same result at the same time across frame rates. Random pattern selection uses a separate expedition stream from slot/creature selection.

Prototype motion tuning is authored in `LandingChallengeProfile`: zone period, fish speed, Burst stage lengths/speed factors and an Erratic key sequence. The pattern must remain understandable and learnable. Future species-specific patterns, feints and rare-creature behavior remain possible additions. Simpler zone motion is preferable to extra complexity if the two moving elements prove frustrating.

## Landing Points and scoring

Landing Points are encounter-local. They reset for the next creature and are distinct from sale value, Gold and permanent progression.

Prototype accuracy bases:

| Result | Points |
| --- | --- |
| Miss | 0 |
| Good | 10 |
| Perfect | 20 |

Initial model:

`Strike Points = (Accuracy Base + Applicable Rig Bonuses) × Product of Applicable Rig Multipliers`

Resolve matching flat bonuses first, then multiply all matching multipliers. Retain fractional points rather than inventing a final integer-rounding rule. Example: Perfect 20, bonus +5 and multiplier ×2 gives **50**. A score is computed once and added once; animation and UI refresh must not rerun effects.

A normal Miss receives no ordinary successful-strike bonuses or multipliers and scores zero. Only an explicitly authored Miss scoring rule may change that behavior. Ability conditions must be deterministic and understandable. Reject invalid/nonfinite/negative authoring before entering Land; reject arithmetic overflow without consuming an attempt or selecting an unrelated outcome.

## Rig scoring boundaries

Capture committed species, stable IDs, effective tags, levels and primary abilities. Never include the encountered creature or a placement preview. Each species counts once for adjacency and contiguous groups, regardless of upgrade level or collected copies.

`RigRuntime.SnapshotForLanding` resolves current positional conditions and level amounts into immutable source-attributed flat bonuses and multipliers. Flat bonuses resolve before multiplier products. Successful/Good/Perfect/explicit-Miss conditions remain distinct. Extra attempts and protected Misses are captured as encounter-local allowances; a protected Miss does not consume an attempt. Ordinary bonuses still do not score on Miss.

The [Rig Building specification](Rig%20Building%20Mechanic.md) owns unique species, upgrades, ability authoring and sale-value derivation. Examples there are not automatically assigned to actual species. Techniques, physical accuracy assistance and other effect families need approved content/handlers.

## Resistance and difficulty

Every challenge resolves a positive Resistance target. It may use an explicitly authored creature base or a profile default. **Never derive Resistance from Weight.**

Difficulty should grow primarily through Resistance and player power primarily through scoring growth, combinations and multipliers. Do not mainly increase fish speed/shrink zones and then make upgrades reverse those same changes. Creature motion complexity can vary without becoming the sole progression axis.

The 40 / 150 / 600 early/middle/late targets and the Lionfish 120 / 125-point example are illustrative. They are not final creature data or a progression curve. For a runnable prototype, the editable default is **base 40 + 10 per progressed depth**; an authored positive creature base replaces the default base. This is explicitly provisional and must be playtested/replaced as encounter pacing is designed. It grants no biome completion rule or expedition endpoint.

## Feedback and presentation

Show the current creature, Resistance/progress, attempts, moving track and committed rig order. Keep the rig immutable during Land. Every strike immediately shows Miss/Good/Perfect, base, attributed bonuses/multipliers and total points. Briefly freeze the two indicators and highlight contributing sources, then resume if the challenge remains active.

Authoritative scoring and terminal decisions happen immediately; feedback never postpones success/failure or permits another strike during the freeze. Keep feedback short and avoid long unskippable sequences. The prototype uses a configurable 0.22-second freeze and a short native contribution highlight.

This phase uses the approved independent Tackle Apparatus Penpot exports: shared desk/cabinet/machine, creature panel, scoreboard, timing track, moving Good/Perfect regions, attempt lamps and controls. Runtime labels remain native text. Long rig and contribution lists scroll without a rig cap. Discover and Land remain separate screens. The plain colored rectangles and their generation path have been removed.

The moving indicator is an original generic pixel fish. Its visible width and scoring interval agree. Baseline width is two-thirds of the initial Perfect region: approximately 53 px against 79 px on the 992 px track at 1080p. It replaces the oversized decorative garfish and separate markers. The agreed interval-overlap grading remains in force; full containment has been raised as a separate difficulty option and is not assumed. All 22 selected creature sprites supply species portraits through the creature definitions. The generic moving indicator remains separate. Shared rig cards display level and resolved accumulated species value.

## Current implementation boundary

`LandingChallengeRuntime` owns motion/time/attempts/grade/outcome. `LandingScoring` produces immutable attributed score results. `LandingChallengeProfile` supplies frozen expedition tuning. `FishingRunController` owns the Discover handoff, strike legality, success handoff and full-value failure settlement. `LandingChallengeView` owns visibility and forwards input; `TackleLandingPresentation` renders state; `TackleLandingArtSetup` imports the original sprites and authors the reusable skinned prefab. `LandingChallengeSetup` retains profile and scene/Harbor wiring.

The success handoff opens the independent Build screen for a new species or automatically upgrades a duplicate. Legal placement and ordinary automatic continuation are implemented by the rig/controller owners. No manual rearrangement, legacy Descend or challenge bypass is granted. Failure has a skinned result/Harbor return and preserves permanent state; the existing wallet adapter remains in use without finalizing Gold as the redesigned economy.

See [original gameplay validation and historical plain UI captures](Validation/LandingChallenge/README.md) for rule, live outcome and Discover regression coverage.

See [current artwork validation and captures](Validation/TackleLandingArt/README.md) for the installed presentation and removal checks.

## Open balancing and design decisions

- Confirm the Miss wording and playtest overlap/boundary/region-size policies.
- Final attempts, accuracy bases, Resistance data/curve, motion tuning and encounter pacing.
- Final numeric precision/rounding and exceptional scoring interactions.
- Redesigned species abilities, effects that change attempts or physical accuracy, Techniques and equipment windows.
- Final restriction combinations/no-legal-position handling, biome completion and permanent reward grants.
- Final economy/currency conversion and additional success rewards; full accumulated creature value on both outcomes is already decided.
