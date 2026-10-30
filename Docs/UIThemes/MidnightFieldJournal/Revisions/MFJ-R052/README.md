> Visual direction superseded by [MFJ-R053](../MFJ-R053/README.md). Retained for structural comparison; these screens are not approved for automatic Unity import.

# MFJ-R052 — The Harbor

**Penpot design; not imported into Unity.** Four editable 3840 × 2160 mockups and a separate reusable-element sheet, created on 30 October 2026.

Find them on **Concept B — Reusable Assembly (4K)** in the existing Penpot file. The main board is **MFJ-R052 / 01 — Harbor / 3840 × 2160**, at (48600,26000). The three expanded views sit beside and below it; the reusable library is to their right. IDs are recorded in [PenpotDesign.json](PenpotDesign.json).

## Screens

- **Harbor:** an engraved dusk waterfront with lantern-lit workshop, chart cabin, moored boat, three destination placards, current equipment/deck summary and a clear Set Sail action.
- **Tackle Shop:** equipment cabinet, rod/reel/hook illustrations, category filters, equipped/available/journal-locked states and an expanded sidegrade detail panel. One equipment item is equipped. Gold unlocks an option without automatically replacing that item.
- **Technique Collection:** eight existing Technique designs in a card folio, distinct in-deck/owned/locked badges, a gold unlock example and a starting-deck sidebar with copy counts and saved state. The illustrated custom deck totals 12 cards; it is not a change to the actual starter deck.
- **Discovery Journal:** an open folio with objectives, numerical progress, completed and claimable states, and reward delivery labels. Examples cover equipment, creatures, events and Black Market contracts. The selected completed discovery opens a shop purchase; other examples distinguish direct grants and encounter-pool unlocks.

The same Cinzel / Source Serif 4 typography and sea-ink, warm parchment, muted copper and restrained teal palette connect all screens. Gold is the only currency. No infinite stat-upgrade ladder or multiple equipment slots are presented.

## Editable construction and Unity handoff

All titles, gold, prices, descriptions, ownership states, deck quantities, objectives and progress values are native editable text. Paper, corner engraving, glyphs, equipment illustrations, Technique illustrations, state badges, progress tracks/fills and buttons are separate layers. There are no baked UI labels in the Harbor scenery or equipment sprites.

The library contains location/product/detail/card/reward/departure examples, a blank engraved shell, state badges, progress and button samples. Treat them as reconstruction templates, not as a single image to slice. For Unity, retain corner ornament geometry at fixed size (roughly 96 design pixels) and stretch only calm edges/centre areas. Reconstruct fields and progress from game state. Import scenery and individual illustrations as independent sprites, preserving aspect and alpha. Decorative art must not intercept pointer input.

Native layouts and typography target a 3840 × 2160 PC composition. Generated bitmap source sizes are recorded in [Verification.json](Verification.json); the Harbor scenery is a review-resolution source, not a native 4K production master. A native-resolution scenery replacement or approved enlargement must be prepared before final Unity shipping. Do not silently call an enlarged source native 4K. Equipment and Technique art is shown within its source resolution in these layouts. These are design mockups, not a Unity integration.

## Illustrative content boundary

The 920-gold wallet, purchase prices, equipment names/effects/tradeoffs, ownership, customized deck quantities and discovery objectives/rewards are sample content for reviewing the interface. They do not authorize new runtime mechanics or balance. Existing Technique identities and short effect descriptions are reused. New equipment art is independently generated. No new fonts, additional currencies, paid plugins or external stock subscriptions were introduced.

## Prototype

The destination placards and navigation controls connect the four boards; current-location tabs are intentionally inert. The journal's shop reward and the shop's journal-locked product also connect their relevant views. Gold purchases, deck edits, claims and departure are visual design states, not gameplay implementations. Use the selected Harbor board in Present mode; the current Penpot API rejects named flow creation, which is recorded in the metadata, without preventing the board-to-board links.

## Integration status

Recorded as **ready for review**, `unityImported=false`. The earlier MFJ-SYNC-009 checkpoint and all prior imported statuses are retained. Current Unity assets, including the separate all-depth playtest changes, are verified against the before-design hash snapshot; this task modifies no Unity project assets.

[Harbor preview](Harbor.jpg) · [Shop](TackleShop.jpg) · [Collection](TechniqueCollection.jpg) · [Journal](DiscoveryJournal.jpg) · [Reusable elements](ReusableElements.jpg).
