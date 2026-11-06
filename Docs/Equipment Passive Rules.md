# Equipment passive rules — nine-item revision

Nine-item revision integrated on 4 November 2026. These rules supersede the previous three equipment effects.

## Confirmed decisions

- Empty slots are allowed: equip at most one Rod, one Reel and one Hook. Nothing is initially owned or equipped.
- Fixed provisional prices: Driftwood Rod 100, Ironback Rod 250, Stormbent Rod 350; Tideglass Reel 150, Clockwork Reel 300, Pearlwind Reel 250; Barbed Hook 100, Living Hook 250, Twinfang Hook 300 Gold. All items can be purchased in any order without prerequisites. Prices do not escalate after purchases. Empty slots grant no bonuses. Purchase permanently unlocks an item; equipping is separate and allowed only between expeditions.
- Equipment Technique draws use normal discard reshuffling. There is no hand-size limit. If neither draw nor discard pile can supply a card, no card is invented and Pearlwind requires no discard.

## Authored passive definitions

Each item can contain reusable `EquipmentPassiveEffect` definitions with behavior, amount and threshold. Runtime dispatch uses behavior kinds, never equipment IDs or display names. Stat provenance comes from an equipment CardDefinition outside encounter pools. New artwork has not been requested or generated.

| Equipment | Category | Passive behavior | Amount | Threshold |
| --- | --- | --- | --- | --- |
| Driftwood Rod | Rod | ReduceFirstAttachmentWeights | 1 | First 2 creature attachments |
| Ironback Rod | Rod | ReduceHeaviestWeight | 2 | 1 target |
| Stormbent Rod | Rod | CatchValueWhileOverloaded | 1 | Attached weight exceeds capacity |
| Tideglass Reel | Reel | DrawAtBiomeStart | 1 | Every biome, including first |
| Clockwork Reel | Reel | DrawAfterEmptyHandTechnique | 1 | Once per encounter |
| Pearlwind Reel | Reel | FilterAfterBaitAttachment | 1 | Each new Bait attachment |
| Barbed Hook | Hook | ValueEveryNthCatch | 2 | Every 3rd new Catch |
| Living Hook | Hook | ReduceBaitWeightsWithMinimumCount | 1 | At least 2 Bait |
| Twinfang Hook | Hook | CatchValueWithMinimumBaitCount | 1 | At least 1 Bait |

## Resolution contract

- Equipment is permanently unlocked at the Harbor, independently equipped by category, then frozen for the expedition. Passives resolve automatically with no activation button.
- All weight reductions stop at zero and never modify creature definitions. Dynamic conditions rebuild after creature/event stats; weight conditions settle before overload-dependent value is evaluated. The Stormbent calculation follows the supplied definition: sum of effective attached weights greater than Line Capacity.
- Driftwood's two grants follow the first two physical creature attachments in either role until removal. Releasing a recipient never transfers its grant. Barbed counts only new Catch attachments; third/sixth/ninth recipients gain +2, and releasing them never refunds the count. Mode changes do not count as new physical attachments. Creature-like Apex follow the existing creature-attachment convention; treasures and events do not qualify.
- Ironback compares original/base creature weights and chooses the earliest attached on ties. Its reduction follows that target only while attached; removal immediately selects the next eligible heaviest creature. Ranking by original weight avoids reduction-driven target oscillation.
- Living Hook updates all Bait weights when the live Bait count is at least two; dropping below two removes its reductions. Twinfang grants +1 to every creature Catch while any creature Bait remains. Role changes update these conditions. Bait contributes zero haul value.
- Stormbent grants +1 to creature Catches only while attached weight exceeds capacity; it does not change line-break risk. The live bonus is included in Surface haul snapshots when its condition holds.
- Tideglass's additional draw follows the normal biome-start draw, including the first biome. Clockwork checks the final hand after the Technique and its reactions finish, and resets only on advancement to a new encounter, never on replacing the Hooked card.
- Pearlwind waits for the new Bait's own effect requests to finish, draws first, then requires a player-selected discard. No successful draw means no discard. Temporary hand growth is allowed; the final hand does not grow. The shared queue supports discarding exactly the successfully drawn amount if the authored amount changes in balancing.
- Source-attributed modifiers use the existing stat contribution system, and reuse the current number animation and cause tooltips. Conditional bonuses are recomputed rather than accumulated; all expedition counters clear between runs.

## Implementation and authoring

`EquipmentPassiveRuntime` is the single active equipment engine bound to `FishingRunController`; the superseded `RunEquipmentRuntime` was removed. `EquipmentDefinitions.json` authors all nine item identities, prices, descriptions and reusable passive definitions. `JournalEquipmentCatalogAuthoring` validates that source and updates stable catalog/CardDefinition assets. The approved R067 shop now displays nine distinct designs instead of repeated placeholders. Existing category illustrations are reused; new per-item artwork is not claimed.

The resolver runs dynamic equipment stats after creature/event modifiers. The existing choice queue resolves Pearlwind's mandatory discard, while Clockwork runs after Technique reactions finish. Equipment contributions feed the existing number animations and source tooltips. Existing ownership IDs and save version 2 are retained; version 1 category migration remains supported. No player save reset or reward grant is part of this update. No Penpot assets changed or require reintegration.

[Verification](Validation/EquipmentPassives/README.md).
