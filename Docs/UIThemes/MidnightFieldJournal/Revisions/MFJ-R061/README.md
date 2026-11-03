# MFJ-R061 — Tackle Shop: Rod, Reel and Hook

**Penpot only; ready for review; not imported into Unity.**

Find it on **01 - Current Design Library → 01 - Current Screens and Gameplay Reference → MFJ-R061 / Tackle Shop / Before purchase**. Three 3840 × 2160 boards sit at (80000, 0), (84200, 0) and (88400, 0). The initial board is selected and framed in the viewport.

The left cabinet contains separate Rod, Reel and Hook slots. Equip up to one of each simultaneously; any slot can be empty. The right catalog shows all three existing approved items, category filters, their effects and ownership/equipment states. Unlocking is a permanent Gold purchase; equipping an owned item does not spend Gold again. An item replaces only the item in its matching category.

The prototype illustrates the Hook flow: **Unlock 250 Gold → owned → Equip Hook → 3/3 equipped → Unequip Hook → owned**. Rod and Reel remain equipped throughout these sample states. Wallet values of 650/400 are illustrative. Filters and Rod/Reel unequip controls are visual specifications, not wired prototype interactions. Harbor navigation links to existing boards.

Existing approved artwork, names and effects are retained: Driftwood Rod (200 Gold), Tideglass Reel (200 Gold), Barbed Hook (250 Gold). No new items, rewards or currencies are introduced. Existing R055 and Unity behavior are preserved pending review and a separate implementation request.

Native editable labels, navigation, slot containers, catalog cards and action strip are separate. Frames reuse independent R057 parchment/navy masters through clipped fixed-corner 9-slice boards; no mockup screenshot is sliced. Ornamental corners are downscaled from 300 source pixels to 25–150 display pixels. The new independent engraved workshop scene is 2172 × 724, displayed at 1440 × 480, without enlargement. Cinzel and Source Serif 4 reuse the established free font stack.

Workshop art was newly generated with the built-in imagegen tool using the existing creature-card style as reference: a wide engraved naturalist fishing workshop with rod, reel, hooks, coiled line, lantern and twilight harbor/lighthouse window; navy/ochre ink and stippling, no UI, text or frame.

[Before purchase](BeforePurchase.png) · [Hook owned](HookOwned.png) · [All three equipped](HookEquipped.png) · [Exact Penpot IDs](PenpotDesign.json) · [Verification](Verification.json).
