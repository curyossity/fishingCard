> **Imported and verified in Unity — MFJ-SYNC-001.** [Integration evidence](../../Sync001/README.md). Earlier review/handoff notes below describe the pre-import design stage.

# MFJ-R002 — detailed main creature card

Status: accepted as the creature-card template when the user requested Cod based on it; **not imported into Unity**.

The previous central card had a narrow, boxed title, a small floating octopus on a dark field, simplified effect panels and no mockup-style rarity strip. This revision recreates the mockup's engraved parchment, integrated marine backdrop, wide natural-history illustration, stat/rarity band, role icons and warm selected-Catch treatment.

The result is installed on both gameplay boards in **Concept B — Reusable Assembly (4K)**. It also has a standalone board named **MFJ-R002 / Main creature card — mockup detail review**, to the right of the existing boards. Previous central-card instances are hidden and retained for comparison. The original static mockup is untouched.

- [Main card](MainCardReview.png)
- [Initial full 3840×2160 placement snapshot](GameplayPlacement4K.png): captured before the final small body-text inset/font refinements. Use the current card preview, live Penpot and FinalGeometry.json for the final text layout; subsequent full-board export refreshes timed out.
- [Source images and hashes](Assets.json)
- [Final editable geometry](FinalGeometry.json)
- [Live Penpot checks](PenpotVerification.json)
- [Generation prompts](GenerationPrompts.md)

## Editable construction

The 1260×1400 design contains one independently generated blank frame, one separate octopus illustration, 14 editable text fields, three native vector icons and a native selected-role outline/glow. No screenshot was sliced or used as a flattened replacement card. The glow belongs to selected Catch, not hover; MFJ-R001 remains the separate hover-frame removal request.

The example uses approved Small Octopus data: Creature, Weight **1**, Value **9**, **Uncommon**, Small/Predator/Elusive; **Clever Catch — When caught, draw 1 technique.** and **Helping Arms — When attached as Bait, you may swap the modes of another Catch and another Bait.** The complete Bait trigger remains visible instead of silently shortening the mechanic to the mockup's example copy. The tag text remains an independent small field at the illustration foot.

The new illustration uses engraved copper/ivory/slate linework and fills the wider marine stage. The type/title are parchment ink, stats use separate live labels/numerals, rarity has its own editable name/diamond, and the two roles have separate hook/twin-spark emblems, titles and rules. Artwork does not contain these data fields.

## Selective Unity update

Import only the two selected raster images, three vector-emblem sources/native UI equivalents, and affected central-card configuration after review/request. The SVG sources in `Art/` are the exact hook, twin-spark and rarity-diamond paths used to create the editable Penpot icons. They must be implemented/exported appropriately for Unity; this revision does not assume an installed SVG runtime package. The frame source is **1190×1322**, displayed on a **1260×1400** canvas (about 6% enlargement); the illustration is **1672×941** with alpha. The full assembly export is 3840×2160; that does not make each source image native 4K. Check source sharpness/edge quality at the final 4K display size during import rather than labelling either source as a 4K raster. No API/CLI image service was used; generation used the built-in image tool.

Update the main-card width/placement, art framing, stat/rarity row, label/rules typography and selected-role treatment in the candidate's authoritative profiles/builders. Preserve dynamic field/masking contracts and verify long names/rules, all rarities, other creature/Apex/Treasure faces, both selected roles, transitions/history/ascent and 1080p after integration. Event cards retain their separate template.

**Keep the existing compact-card portrait mapping.** The current `SmallOctopusPortrait` asset is shared by view consumers; overwriting it would unintentionally change compact Catch Chain cards. Add a central-only illustration asset/override for this new wide portrait, and leave approved chain art and hardware unchanged. Rarity text/diamond is a new candidate central presentation driven by existing CardRarity, not a new gameplay system. Other rarities need their presentation verified in the later integration.

No Unity source, prefab, profile, scene or asset file has changed. The synchronization report in `Verification.json` compares all 218 frozen baseline files, so this revision can be applied alongside MFJ-R001 without reimporting unrelated assets.
