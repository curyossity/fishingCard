# Deepwater — Equipment Design Direction

Design baseline: 6 November 2026. Equipment effects and meta-progression require redesign against the [GDD](Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md). No replacement effects or prices are finalized.

## Direction

Equipment should support the challenge and rig-building systems. Rod, Reel and Hook can remain as categories; their roles, category limits, loadout timing and acquisition rules need reconsideration.

Review all existing equipment, including Driftwood Rod, Ironback Rod, Stormbent Rod, Tideglass Reel, Clockwork Reel, Pearlwind Reel, Barbed Hook, Living Hook and Twinfang Hook. These names identify the review inventory, not approved Deepwater effects or a fixed final roster. Capacity-, weight- and overload-related effects require redesign; value and Technique effects also need compatibility and timing review.

Gold purchases are not guaranteed to remain the primary progression system. Achievement-based unlocks are considered. Permanent equipment purchases, fixed prices, prerequisites, default ownership and save migration are not finalized by this baseline.

## Effect authoring requirements

For each proposed effect, define its purpose, trigger/window, target, amount, limits, stacking, source attribution and lifetime. Explain its interaction with scoring, attempts, accuracy, rig roles and positioning where applicable. Do not resolve an open scoring formula or Technique window indirectly through equipment text.

Some upgrades may widen the landing zone or slow the fish; these complement score growth rather than canceling Resistance escalation. This is a possible effect direction, not approval of particular items.

Keep authored effects reusable and runtime state separate from shared assets. Views present equipment and forward commands; runtime owners validate effects and grants. Exact ownership boundaries follow the final progression decision.

## Decisions needed

- Category roles, loadout limits and when loadouts can change.
- Individual effects and tuning; scoring/Technique ordering.
- Acquisition, permanent ownership and whether Gold is retained.
- Achievement objectives/rewards if adopted; do not treat examples as an unlock table.
- Persistence and migration of existing saves once the new rules are approved.

The code, nine-item data, current shop artwork and player saves remain unchanged. Existing implementation details and verification are historical references in [Architecture Implementation Reference](Architecture%20Implementation%20Reference.md) and [equipment validation](Validation/EquipmentPassives/README.md).
