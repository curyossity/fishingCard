# Functional Tackle Shop — MFJ-SYNC-011.1

The Midnight Field Journal candidate scene now supports permanent purchases, separate equipment selection and real expedition benefits. Open Tackle Shop, inspect an item, Buy it with earned Gold, then Equip it. One equipment slot is available; buying another item does not silently replace the loadout. No item is initially owned/equipped. Effects and prices replace the illustrative captions; no design rationale is displayed.

| Item | Cost | Expedition benefit |
| --- | --- | --- |
| Driftwood Rod | 200 Gold | +1 starting Line Capacity |
| Tideglass Reel | 200 Gold | Two additional opening Techniques; expedition hand cap six |
| Barbed Hook | 250 Gold | First three newly attached creature Catches each gain +1 sale value until removal |

Equipment remains fixed during an expedition. Hook previews do not consume grants; Bait/mode changes do not earn them or refresh the three-use limit. Bonuses use the existing modifier attribution and stat animation. All explicit draw/recovery paths honor the Reel's cap; Technique plays never automatically refill the hand. Journal rewards remain descriptive only. Technique Collection deck editing is unchanged.

Gold, owned equipment, equipped identity and existing capacity purchases share `FishingCards.PlayerProgression.v1`. Transactions persist the complete document before publishing changes. Invalid/unsupported saves remain untouched; failed writes can be retried without double spending. Normal haul earnings are saved after Surface. Scenes without a configured catalog retain their previous behavior.

Verification completed in Unity 6000.5.9f1:

- Equipment rules: 31 assertions covering prices, insufficient funds, duplicate/active-run rejection, separate equip, reload, failure atomicity/retry, attribution, previews, first-three limits, mode changes, release and per-run reset.
- Live shop and expedition loop: 29 assertions, using actual pointer raycasts/click handlers. Bought/equipped all three items; verified Rod capacity, Reel opening hand, Hook's first three vs fourth Catch, real haul sale, wallet and ownership/loadout reload.
- Technique regression: 39 runtime and 33 authored-content assertions.
- Hub rendering: 376 component/input/text-overflow assertions across four pages at 4K and 1080p. Six active TechniqueCardViews verified for the Reel; the actual six-card fan capture was inspected.
- Zero final console errors; candidate scene clean and outside Play Mode. Tests used recorded disposable progression/Journal keys; cleanup ran after stopping Play Mode. Real player keys were not replaced with test data.

The live test originally serialized the running controller merely to inspect its starting capacity. Unity inline serialization manufactured empty nullable runtime objects and blocked actions. The test now reads the field without serializing the controller; no unrelated runtime serialization changes were retained.

[Verification](Verification.json), [asset checkpoint](UnityCheckpoint.json), [shop capture](shop-Unity4K.png), [six-card hand](ReelSixCardHand4K.png). This is a Unity-only behavior increment: no new Penpot revision or art import. All 45 completed / 13 superseded revisions stay recorded; zero pending imports. Previous checkpoint: [MFJ-SYNC-011](../../UIThemes/MidnightFieldJournal/Sync011/README.md).
