# Deepwater — UI Design Requirements

Design baseline: 6 November 2026. The [GDD](Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md) defines mechanics. These are presentation requirements, not a finished layout or art commission. Implementation status is stated in the phase sections below.

[UI Presentation Reference](UI%20Presentation%20Reference.md) preserves resource paths, geometry and assembly details. Existing screenshots/controls are evidence, not the Deepwater interaction contract. Preserve nautical and Midnight Field Journal resources separately.

## Presentation direction

Keep illustrated cards as the world and the ordered rig as the main build display. Use tactile cards and restrained surrounding UI without a combat HUD or explorable sea.

Preserve supplied artwork. Keep runtime text, counters, controls and feedback separate. Record missing inputs rather than inventing final artwork or relabeling baked Weight fields. The [card contract](Card%20Visual%20Specification.md) governs faces and rarity hooks.

## Discover

Provide Harbor preparation for four active tags. Present three reels and clearly distinguish unrestricted discovery, a guaranteed pair tag with a secondary preference, and a triple with a stronger rarity opportunity. Offer Accept or exactly one locked reel plus one respin; the respun result is fixed, remains visible, and reveals a creature only after explicit Accept Tags. Disable locks/respin after that one respin. Follow the [slot specification](Slot%20Machine%20Mechanic.md). Exact rarity odds and preference strength remain balancing inputs. The slot machine influences selection; do not add landing bonuses or another tag-timing instrument. The comparison scene now uses the approved Tackle Apparatus Discover artwork, separate native labels, focus/pressed/disabled feedback and a scrollable rig. All eight tag illustrations are mapped, with native captions; the rotating paper/dividers/plaques share their cylindrical motion.

## Land

Present the creature, Resistance target, accumulated Landing Points, attempts remaining, fish/zone motion and action feedback. Make accuracy and rig amplification understandable.

The [Landing specification](Landing%20Challenge%20Mechanic.md) defines Space/UI strikes, Perfect-first overlap, limited attempts and immediate success/failure. The comparison scene uses a separate Tackle Apparatus Landing canvas with the approved Penpot exports and native text. The placeholder canvas contents and generator have been replaced. See [artwork validation](Validation/TackleLandingArt/README.md). Show grade, base, attributed bonuses/multipliers and final points, with a brief indicator freeze and source highlight. Rig and contribution lists scroll without a cap. Long-term accessibility refinement and final widths/motion/numeric tuning remain open; do not bake four attempts or base points into art. Timing input must not be captured by unrelated rig/card interactions. Landing success currently displays the Build handoff; failure shows full Catch payout, rig cleanup and Harbor return.

## Build

After successful landing of a new species, present its primary ability and every permitted insertion gap. Preview supported consequences without applying effects. One explicit placement commits a Level 1 member while preserving existing relative order. Explain restrictions outside card faces; no manual reorder control. The installed Tackle Build canvas uses native gap buttons, a projected haul readout, a horizontally scrolling rail and the shared vertical rig cabinet.

A duplicate automatically upgrades its existing member, adds the copy's base sale value and returns to Discover with a notice. It requires no placement or extra confirmation. New/updated abilities contribute to future Landing Challenges. Finish effects/reactions before automatic continuation; no repeated Descend commitment is required. Resolve no-legal-position gameplay before assigning content that can create it.

## Rig inspection

Display unique species in committed order, with level and accumulated species sale value clear. Upgrades count as one position. Preserve identity, scrolling and separate hardware as rows shift. Make positional interactions inspectable without adding full rules to compact faces; exact highlighting remains open.

Removal controls depend on an explicitly defined ability. Do not imply free removal or rearrangement through drag/drop. Catch/Bait choice is absent from the creature loop.

## Progression and results

Show Shallows with an empty rig as every expedition's start and display permanent sequential unlock state. No creature attachment carries into a new expedition. Completion criteria, progress indicators, available-biome endpoints and transitions await rules.

Both failure and success results show the full accumulated creature sale value, including all copies and applicable bonuses. Failure immediately ends the expedition, excludes the failed creature and deducts no accumulated haul value. All members, levels and copies clear on either ending; result displays use historical snapshots, not a persistent creature inventory. Distinguish end reason, haul payout and additional permanent success rewards. Preserve unlocks/progression when returning to an empty-rig expedition. Currency conversion, prices, shop flow and exact completion/achievement rewards remain undecided; the full-value payout on both outcomes is decided.

Capacity/overload/line-break indicators, voluntary Surface controls and related Journal objectives are not target UI. Preserve their existing artwork. Weight's display is open; Resistance must not inherit its label.

## Design inputs and future validation

Finalize rarity/preference balance; landing width/motion/point tuning and skinned feedback; Resistance/Weight presentation; insertion orientation, restrictions and confirmation; Technique/event windows; biome completion and reward presentation.

Validate readable targets/text, input focus, stale input rejection, levels/duplicate values, long-rig insertion/inspection and result transitions. Preserve scaling from existing 1920 × 1080 and candidate 3840 × 2160 references; check 2560 × 1440, 1366 × 768, 1280 × 800 and ultrawide behavior for affected layouts. Build/Upgrade live validation and current multi-viewport captures are recorded in [Rig Building validation](Validation/RigBuilding/README.md).
