# Respin result review

User clarification and implementation: 7 November 2026. Respin fixes the reel result but does not reveal a creature. Keep Discover visible until the player explicitly presses Accept Tags. Further locks and respins are disabled; the player has no timeout. Initial-result acceptance remains available without using the optional respin.

`FishingRunController.TryRespinSlotMachine` now rolls and refreshes the reels only. `TryAcceptSlotMachine` owns creature selection and the landing handoff for either result. The artwork labels the fixed result `FINAL`; keyboard focus moves from a disabled Respin button to Accept Tags when applicable.

Unity 6000.5.9f1 validation:

- `SlotMachineRuleChecks.ValidateRules()`: 67 checks passed.
- `SlotMachineSceneChecks.ValidateLive(true, ...)`: 67 checks passed in the recorded run, including no early reveal, visible final faces matching runtime tags, disabled lock/respin commands, enabled acceptance, stale initial-Accept rejection, preserved locked face, and one landing handoff only after current-result acceptance.
- No Unity errors were reported during the live run.
- [Final result capture](FinalRespin1080.png) was visually inspected. The rig remains empty and haul remains zero until later gameplay acquires creatures.

The previous slot/artwork validation reports record the behavior validated at their original integration points. This result-review change supersedes automatic reveal after respin; it does not change matching probabilities, rarity tables, eligibility or landing rules.
