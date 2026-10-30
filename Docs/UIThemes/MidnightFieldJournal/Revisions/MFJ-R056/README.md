# MFJ-R056 — Technique Collection

**Imported and verified at MFJ-SYNC-010.** [Unity integration](../../Sync010/README.md). Earlier review-only status below is historical.

One expanded Technique Collection screen in the ornate R053 Harbor and R055 Tackle Shop style. The user clarified that “Event cards” was a mistake: use the actual **Technique** cards from the player's in-run hand.

## Penpot

Page: **Concept B — Reusable Assembly (4K)**. Board: **MFJ-R056 — Technique Collection / In-run card faces**, at `(48600,34000)`, size `3840 × 2160`. Exact IDs are in [PenpotDesign.json](PenpotDesign.json).

All eight active Technique designs are shown. Their frame and illustration files are exact copies of current Unity resources, verified by SHA-256 in [GameCardSources.json](GameCardSources.json). They retain their actual names and effect descriptions as native editable text. Collection cards are shown upright for browsing; the in-run hand's projected fan is not applied to the library grid. No Event frame or new Technique artwork is substituted.

Owned, in-deck and locked examples use labels outside the card faces. An adjacent deck list shows copy controls, the 12-card loadout and four-card starting hand. Filters, copy controls and saved-state button are visual mockups, not functioning progression or persistence. Back to Harbor and the Harbor/Tackle Shop navigation labels link to the revised screens.

The ownership, 120-gold example price and custom deck composition are illustrative UI states, not changes to the active starter deck or new unlock rules. They retain the earlier collection study's demonstration: Cut Loose ×3, Slip Past ×3, Clear Water ×1, Hold Steady ×1, Re-rig ×1, Steady Line ×3; Recast owned but outside the deck; Prize Hook shown locked. Actual approved starter data and runtime ownership are unchanged.

## Asset boundary

Each card is a separate Penpot board containing the actual frame, standalone illustration and native category/name/rules. The new backdrop and outer decorative shell are one static review composition; independently recreate those production assets after approval rather than slicing this mockup. Native source dimensions and hashes are recorded. The 4K export is a layout review and does not imply a native 4K generated backdrop.

No Unity assets, code, scenes or prefabs are changed. R053 and R055 remain pending separately; R052's earlier collection stays preserved for comparison. No new paid plugin, stock subscription or font is introduced.

- [Preview](TechniqueCollection.jpg)
- [4K export](TechniqueCollection4K.jpg)
- [Verification](Verification.json)
- [Background generation prompt](GenerationPrompt.md)
