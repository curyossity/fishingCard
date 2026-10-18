> **Imported and verified in Unity — MFJ-SYNC-001.** [Integration evidence](../../Sync001/README.md). Earlier review/handoff notes below describe the pre-import design stage.

# Squid — MFJ-R018

Ready for visual review in Penpot on **Concept B — Reusable Assembly (4K)**. Independent review board sits in the third and fourth review rows beneath the earlier creature cards.

[Card preview](SquidReview.png) · [Opaque board](SquidReview.png) · [Source manifest](Assets.json) · [Final geometry and committed data](PenpotCard.json) · [Generation prompt](GenerationPrompt.md) · [Preservation checks](Verification.json).

New separate transparent illustration generated with the built-in image tool; shared R002 frame and native emblems reused. Names, stats, tags, rarity and both effects remain native editable fields. A null Catch effect has no ability text, deliberately. Full authored rules remain in the unchanged CardDefinition.

Selective Unity handoff: depends on MFJ-R002. Add Art/SquidEngravedV3.png as a central-only mapping for `creature_squid` with the portrait bounds in PenpotCard.json. Keep approved compact Catch Chain art unchanged. Source is 1672x941, not a native 4K raster; its display footprint is smaller than the source. Shared frame resolution has the existing R002 limitation. No Unity files imported or modified.

The opaque review board is the final visual evidence. Optional card-only PNG export was omitted after renderer timeouts; the independent illustration and editable Penpot card are complete.
