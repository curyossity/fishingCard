# Deepwater — UI Design Requirements

Design baseline: 6 November 2026. The [GDD](Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md) defines mechanics. These are presentation requirements, not a finished layout or art commission. No code, assets, prefabs or scenes change in this pass.

[UI Presentation Reference](UI%20Presentation%20Reference.md) preserves resource paths, geometry and assembly details. Existing screenshots/controls are evidence, not the Deepwater interaction contract. Preserve nautical and Midnight Field Journal resources separately.

## Presentation direction

Keep illustrated cards as the world and the ordered rig as the main build display. Use tactile cards and restrained surrounding UI without a combat HUD or explorable sea.

Preserve supplied artwork. Keep runtime text, counters, controls and feedback separate. Record missing inputs rather than inventing final artwork or relabeling baked Weight fields. The [card contract](Card%20Visual%20Specification.md) governs faces and rarity hooks.

## Discover

Provide Harbor preparation for four active tags. Present three reels and clearly distinguish unrestricted discovery, a guaranteed pair tag with a secondary preference, and a triple with a stronger rarity opportunity. Offer Accept or exactly one locked reel plus one respin; the respun result is final. Follow the [slot specification](Slot%20Machine%20Mechanic.md). Exact rarity odds and preference strength remain balancing inputs. The slot machine influences selection; do not add landing bonuses or another tag-timing instrument. The comparison scene now uses the approved Tackle Apparatus Discover artwork, separate native labels, focus/pressed/disabled feedback and a scrollable rig. Supplied Fish/Schooling illustrations are mapped; the other six tags retain native symbol-area text pending matching art.

## Land

Present the creature, Resistance target, accumulated Landing Points, attempts remaining, fish/zone motion and action feedback. Make accuracy and rig amplification understandable.

Moving fish/zone and Miss/Good/Perfect grades are the leading concept, not final controls/thresholds. Do not bake four attempts into art. Input, score breakdown, feedback, success/failure transitions and accessibility details need design. Timing input must not be captured by unrelated rig/card interactions.

## Build

After success, present explicit Catch/Bait choice and permitted insertion positions. Make committed placement and preservation of existing order clear. Preview supported consequences without applying effects. Explain restrictions outside card faces; no manual reorder control.

Require role and legal position before commit. New creature contribution starts after placement. Finish effects/reactions before automatic continuation; no repeated Descend commitment is required by the loop. Resolve no-legal-position gameplay before implementing UI for content that can create it.

## Rig inspection

Display both roles in current positional order, with Catch sale value and Bait-zero clear. Preserve identity, scroll inspection and separate hardware as rows shift. Make positional interactions inspectable without adding full rules to compact faces; exact highlighting remains open.

Release and role-change controls depend on approved access/timing. Do not imply free removal, role switching or rearrangement through drag/drop.

## Progression and results

Show Shallows with an empty rig as every expedition's start and display permanent sequential unlock state. No creature attachment carries into a new expedition. Completion criteria, progress indicators, available-biome endpoints and transitions await rules.

Both failure and success results show the full accumulated Catch sale value awarded, with Bait contributing zero. Failure immediately ends the expedition, excludes the failed creature and displays no failure deduction from accumulated Catch value. All attachments are removed on either ending; any result display uses historical snapshots, not a persistent creature inventory. Distinguish end reason, haul payout and any additional permanent success rewards. Preserve unlocks/progression when returning to an empty-rig expedition. Currency/Gold conversion, prices, shop flow and exact completion/achievement rewards remain undecided; the full-value payout on both outcomes is decided.

Capacity/overload/line-break indicators, voluntary Surface controls and related Journal objectives are not target UI. Preserve their existing artwork. Weight's display is open; Resistance must not inherit its label.

## Design inputs and future validation

Supply missing slot-symbol illustrations and finalize rarity/preference balance; landing input, grades and feedback; Resistance/Weight presentation; insertion orientation, restrictions and confirmation; Technique/event windows; biome completion and reward presentation.

After approval, validate readable targets/text, input focus, stale input rejection, both-role presentation, long-rig insertion/inspection and result transitions. Preserve scaling from existing 1920 × 1080 and candidate 3840 × 2160 references; check 2560 × 1440, 1366 × 768, 1280 × 800 and ultrawide behavior for affected layouts. Existing screenshots do not validate new gameplay.
