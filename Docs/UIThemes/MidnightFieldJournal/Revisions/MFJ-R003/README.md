> **Imported and verified in Unity — MFJ-SYNC-001.** [Integration evidence](../../Sync001/README.md). Earlier review/handoff notes below describe the pre-import design stage.

# MFJ-R003 — Cod

Created in Penpot from the accepted MFJ-R002 creature-card template. Ready for visual review; **not imported into Unity**.

Find **MFJ-R003 / Cod — detailed creature card** to the right of the octopus review on **Concept B — Reusable Assembly (4K)**. The original octopus and both gameplay boards are retained.

- [Cod review](CodReview.png)
- [Card-only preview](CodCard.png)
- [Independent Cod illustration](Art/CodEngravedV3.png)
- [Source manifest](Assets.json), [committed fields and verification](PenpotCard.json)
- [Generation prompt](GenerationPrompt.md)

| Field | Approved data |
| --- | --- |
| Stable identity | `creature_coastal_cod` |
| Name / type | Cod / Creature |
| Weight / Value | 2 / 12 |
| Rarity / tags | Common / Fish, Predator |
| Catch effect | None; effect title/body remain absent, without mirroring Bait. |
| Bait effect | **Bottom Feeder:** After releasing an Armored creature, recover one discarded technique. |

Source of truth: `Assets/Cards/Coastal Cod.asset` and the approved Cod rules in the GDD. The existing restriction to deliberate Releases, the shared first eligible Release per encounter, recovery timing and hand-limit rules are unchanged; the card face uses its approved short description.

The new engraved illustration has its own transparent 1672×941 source. It preserves the detailed antique style, mottled Cod body, fin rays and chin barbel. Card frame, marine background, labels, icons and selected-role treatment reuse MFJ-R002. Common rarity uses the existing dark parchment ink rather than the octopus's Uncommon cyan accent. Text/data and art remain independent. The example card is 1260×1400 on the 4K design canvas; neither the illustration nor the card-only preview is labelled a native 4K image.

The initial export retained template ability text after Penpot rejected a blank-text edit. It was corrected by removing the null Catch effect fields and applying Cod's Bait text in a separate operation. The committed fields were read again in a fresh call and the previews re-exported; `PenpotCard.json` contains the final data checks rather than the optimistic first edit response.

## Later selective integration

Depends on MFJ-R002's central-card template/configuration. Import only the Cod illustration and central portrait mapping/framing; reuse the parent frame, fonts and vector emblems. The Cod portrait canvas is `(110,190,1040,585.311)` on the 1260×1400 card; its native aspect is retained. Add an explicit central-only Cod mapping. Do not overwrite compact Cod artwork, approved chain styling or shared CardDefinition data as a side effect.

Verify Common rarity, effectless Catch, Bottom Feeder wrapping, concealment and portrait framing at the final Unity display sizes when that import is requested. The revision ledger records exact Penpot IDs and paths. `Verification.json` checks the frozen 218 Unity files and the existing authored Cod asset. No Unity code, prefab, scene, asset or gameplay definition was changed.
