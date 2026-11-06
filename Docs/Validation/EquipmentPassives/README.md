# Nine-equipment integration — 4 November 2026

All nine approved passive items are active in the current Tackle Shop. Slots remain optional, with at most one Rod/Reel/Hook. Prices were subsequently revised to the per-item balancing values in [Equipment Passive Rules](../../Equipment%20Passive%20Rules.md). The original integration evidence below records the earlier prices; [pricing revision validation](PricingRevision/README.md) covers the current prices. Equipment draws use normal reshuffling; the Technique hand is unlimited. Original item IDs and progression saves are preserved.

The approved R067 shop layout and 4K background remain in use. Its nine repeated placeholders are now nine actual item definitions. New items reuse the existing category portraits; distinct per-item artwork has not been generated. No Penpot design changes were made, and no old revision should be imported again for this update.

Validation uses disposable runtime fixtures or separately recorded test save keys, never the player's progression. The live test keys were removed after Play Mode ended.

- [Passive engine](Integration/PassiveEngine/PassiveEngineVerification.json): attachment counters, non-consuming previews, lasting grants, original-weight/oldest targeting, dynamic Bait/overload conditions, line-loss recalculation, zero floors, provenance, every-third Catch bonuses, biome draws, normal reshuffling, Clockwork limits and ordered Pearlwind filtering.
- [Catalog and persistence](Integration/RulesVerification.json): nine prices, permanent ownership, independent category replacement, all-nine reload, legacy migration, transaction failure atomicity and unsupported-save preservation.
- [Controller integration](Integration/ControllerVerification.json): real Catch/Bait/Descend and Technique commands; mandatory Pearlwind choice blocks advancement, Ironback/Living combine correctly, removal retargets, Stormbent/Twinfang combine, Clockwork draws after completion and resets on advancement.
- [Live game](Integration/PlayVerification.json): pointer-driven purchase/equip, three simultaneous categories, opening hands, twelve-card hand presentation, Surface sale values, wallet/loadout persistence and return to Harbor.
- [4K/1080p layout](Integration/ComponentVerification.json): wiring, uncompressed 4K sources, text overflow, fixed slots, clipping and scrolling. [Shop](Integration/shop-Unity4K.png), [scrolled shop](Integration/shop-scrolled-Unity4K.png).

Existing shared Technique rules also passed 56 assertions after integration, including no general automatic hand refill and reaction ordering. Historical pre-activation results remain archived; `Integration/` describes the active nine-item revision.

Authoring source: `Assets/UIThemes/MidnightFieldJournal/Harbor/TackleShop/EquipmentDefinitions.json`. Runtime behaviors dispatch by reusable passive kind, amount and threshold, never by equipment name or ID. The superseded three-item engine was removed. [Rules](../../Equipment%20Passive%20Rules.md) and [architecture](../../Architecture.md) are updated.
