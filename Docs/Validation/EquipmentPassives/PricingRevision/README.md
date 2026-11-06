# Tackle Shop price revision

The nine per-item prices in `EquipmentDefinitions.json` now replace the earlier category-wide prices. The generated Unity catalog and native prefab price labels use the same values.

| Item | Gold |
| --- | ---: |
| Driftwood Rod | 100 |
| Ironback Rod | 250 |
| Stormbent Rod | 350 |
| Tideglass Reel | 150 |
| Clockwork Reel | 300 |
| Pearlwind Reel | 250 |
| Barbed Hook | 100 |
| Living Hook | 250 |
| Twinfang Hook | 300 |

These fixed prices are provisional for balancing. Players start with no equipment, purchase permanent unlocks in any order using Gold, and may equip at most one item per category simultaneously. Empty slots grant no effects. Purchases do not escalate prices. Existing owned items and saves remain valid; this update does not reset progress or retroactively change past transactions.

`TackleShopChecks.ValidateRules` passed 60 assertions, including exact-price and one-Gold-short boundaries for every item, starting ownership, reverse-order purchasing, duplicate purchase prevention, fixed prices, category replacement, persistence and legacy save compatibility. Passive regression also passed. Results: [rules](RulesVerification.json), [passives](PassiveEngine/PassiveEngineVerification.json).

Live pointer-driven purchase/equip and expedition checks use isolated test keys and verify the updated wallet deductions. [Live verification](PlayVerification.json). Test keys are removed after Play Mode ends. Current documentation is updated; no Penpot asset import is needed for a Unity price change.
