# Rarity bar entrance — MFJ-SYNC-008.5

The complete timing instrument, heading and subtitle now enter from one viewport height below their resting position. Entrance uses the existing amplitude-1/period-0.74 elastic curve mirrored in space and time, giving a one-second upward arrival with elastic settling. It starts after the outgoing encounter transition. The existing upward exit remains unchanged.

The controller pauses marker timing and rejects Stop until the current occurrence's entrance finishes. The bar receives pointer/Submit focus after settling. Surface can cancel entrance normally. New attempts and restarts reset both motion phases and clear particles; refreshing the same occurrence does not restart the entrance. Absent/disabled optional views do not block headless gameplay. Rarity zones, creature odds, concealment and event bypass stay unchanged.

Verified: eight entrance geometry checks, 75 timing checks, six exit-curve checks, 14 particle checks and three actual comparison-scene Play Mode suites. Entrance checks cover off-screen start, paused marker, controller and button input locks, same-occurrence refresh, resting position, cancellation during entry and restart. The complete timing flow and highest-rarity celebration are rechecked with the entrance enabled. [Entrance report](ProductionValidation.json), [timing flow](TimingFlow/ProductionValidation.json), [celebration](Celebration/ProductionValidation.json).

4K captures: [entering from below](EnteringFromBelow4K.png), [settled](Settled4K.png). [Current checkpoint](UnityCheckpoint.json). Unity-only animation update; no Penpot artwork import or regeneration is required. Earlier checkpoints remain historical evidence.
