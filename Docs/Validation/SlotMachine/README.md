# Slot machine validation

Validated 7 November 2026 in Unity 6000.5.9f1 against the [slot specification](../../Slot%20Machine%20Mechanic.md).

| Check | Result |
| --- | --- |
| `SlotMachineRuleChecks.ValidateRules()` | 67 assertions passed |
| `SlotMachineSceneChecks.ValidateLive(false)` — accept initial result | 31 assertions passed |
| `SlotMachineSceneChecks.ValidateLive(true)` — one lock and final respin | 35 assertions passed |
| Unity Console after compilation and both live branches | No errors or warnings |
| Unity metadata/GUID checker | Zero errors and warnings |

The deterministic suite enumerates all 64 initial combinations and the 1024 equally weighted branches of accepting matches/respining three different symbols. Counts are respectively 24/36/4 and 144/792/88 for three different/pair/triple. It also checks locked-face preservation, exact-one-lock restrictions, respin limits, revision/session safety, secondary species weighting, tag-preserving rarity fallback and lower-tier ties, biome/unlock filtering, normal-creature depth independence, Apex exclusion, reset and no acquisition at the landing handoff.

Live checks exercise real serialized Harbor controls, four-tag confirmation, no mid-run symbol edits, both selection branches, final result handoff, hidden obsolete action/capacity controls, no pre-landing attachment, stale/double command rejection and label overflow. Captured [Harbor preparation](Preparation1080.png) and [Discover](Discover1080.png) were visually inspected at 1920 × 1080. These are plain functional controls on the existing UI, not the approved new artwork integration.

The installed starting biome has 20 eligible normal creatures. Authored tag coverage was inspected: Fish 10, Predator 9, Schooling 3, Small 10, Heavy 1, Anchored 4, Armored 5 and Elusive 7. Apex is outside this pool. Counts are validation observations, not balancing requirements.

No expedition was completed and no payout or progression mutation was exercised. This increment stops with the creature awaiting landing. Landing execution, Resistance/scoring, expedition failure payout and Build continuation are not validated by these tests. No new artwork files were imported or changed, and no claim is made that the remaining redesign is implemented.
