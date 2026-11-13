# Deepwater — Landing Challenge

Agreed direction supplied 13 November 2026. Focus: Landing motion, strikes, attempts, scoring and terminal outcomes. Numeric defaults are prototype balancing values. Artwork integration, Build/placement, new species abilities, Techniques and equipment redesign are separate work.

## Encounter flow

Discover selects and reveals an eligible creature. Land tests that creature against the **already attached rig**. The encountered creature contributes nothing toward landing itself.

Reach the creature's **Resistance** with accumulated **Landing Points** within limited attempts. Success immediately completes Land and supplies a creature/occurrence-bound handoff to Build. Success does not attach a creature, choose a role, apply its abilities or commit a position. Build subsequently chooses Catch/Bait, previews permitted insertion consequences, commits once and preserves existing relative order. The updated rig affects later encounters.

Failure immediately ends the expedition. The failed creature is not acquired. Award the full accumulated sale value of all previously attached Catch creatures; Bait contributes zero. Snapshot that value before removing every attachment. Permanent progression is preserved; the next expedition starts with an empty rig. There is no voluntary Surface, capacity check, overload or line-break penalty.

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

Capture committed rig membership, stable instance identity, effective tags, stored Catch/Bait role and the selected role's approved landing effects when Land begins. Never include the revealed creature or pending role/placement previews. Catch and Bait both participate in adjacency/tag conditions unless an explicit effect excludes them.

The scoring authoring boundary supports flat bonuses, multipliers, accuracy conditions, neighboring-tag conditions and flat bonuses per other connected contiguous tagged creature. Connected groups stop at the first nonmatching member; a connected-tag source must belong to that tag. A rule's multiplier applies once per eligible source; connected counts scale its flat bonus only. Contributions retain source IDs and names for feedback. Recalculation/preview does not award points.

Existing creature definitions have no newly assigned landing effects by default. Do not infer them from Weight, rarity, old overload effects, tags, or example text. Catch and Bait landing lists remain distinct, including explicitly empty Bait lists.

Illustrative examples, **not finalized creature data**:

- Mackerel Catch: +5 sale value per other connected contiguous Schooling creature; Bait: +3 Landing Points per such creature on successful strikes.
- A Predator's Bait ability could multiply Perfect points by 1.5 while adjacent to another Fish.
- An Armored creature's Bait ability could protect the first Miss from attempt consumption while adjacent to another Armored creature.

Miss protection, extra attempts, widening regions, slowing fish, altered zone motion, Technique interactions, sale-value effects and placement restrictions may support future builds. They do not become automatic tag abilities. This increment provides the scoring foundation, not final redesigned creature content or all possible future effects.

## Resistance and difficulty

Every challenge resolves a positive Resistance target. It may use an explicitly authored creature base or a profile default. **Never derive Resistance from Weight.**

Difficulty should grow primarily through Resistance and player power primarily through scoring growth, combinations and multipliers. Do not mainly increase fish speed/shrink zones and then make upgrades reverse those same changes. Creature motion complexity can vary without becoming the sole progression axis.

The 40 / 150 / 600 early/middle/late targets and the Lionfish 120 / 125-point example are illustrative. They are not final creature data or a progression curve. For a runnable prototype, the editable default is **base 40 + 10 per progressed depth**; an authored positive creature base replaces the default base. This is explicitly provisional and must be playtested/replaced as encounter pacing is designed. It grants no biome completion rule or expedition endpoint.

## Feedback and plain UI

Show the current creature, Resistance/progress, attempts, moving track and committed rig order. Keep the rig immutable during Land. Every strike immediately shows Miss/Good/Perfect, base, attributed bonuses/multipliers and total points. Briefly freeze the two indicators and highlight contributing sources, then resume if the challenge remains active.

Authoritative scoring and terminal decisions happen immediately; feedback never postpones success/failure or permits another strike during the freeze. Keep feedback short and avoid long unskippable sequences. The prototype uses a configurable 0.22-second freeze and a short native contribution highlight.

This phase uses colored rectangles and native text only. No approved Landing concept art, creature artwork or rig-card artwork is integrated. Long rig and contribution lists must scroll without imposing a rig cap. Discover and Land remain separate screens.

## Current implementation boundary

`LandingChallengeRuntime` owns motion/time/attempts/grade/outcome. `LandingScoring` produces immutable attributed score results. `LandingChallengeProfile` supplies frozen expedition tuning. `FishingRunController` owns the Discover handoff, strike legality, success handoff and full-value failure settlement. `LandingChallengeView` renders and forwards input; `LandingChallengeSetup` authors repeatable plain controls.

The current success endpoint is **ready for rig placement**. It intentionally grants no attachment, manual rearrangement, legacy Descend or challenge bypass. Build UI/commitment is not implemented here. Failure has a plain result/Harbor return and preserves permanent state; the existing wallet adapter remains in use without finalizing Gold as the redesigned economy.

See [validation results and plain UI captures](Validation/LandingChallenge/README.md) for rule, live outcome and Discover regression coverage.

## Open balancing and design decisions

- Confirm the Miss wording and playtest overlap/boundary/region-size policies.
- Final attempts, accuracy bases, Resistance data/curve, motion tuning and encounter pacing.
- Final numeric precision/rounding and exceptional scoring interactions.
- Redesigned species abilities, effects that change attempts or physical accuracy, Techniques and equipment windows.
- Build insertion restrictions, preview/commit UI, biome completion and permanent reward grants.
- Final economy/currency conversion and additional success rewards; full accumulated Catch value on both outcomes is already decided.
