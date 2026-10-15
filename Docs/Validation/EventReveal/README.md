# EVT-02 event reveal, cancellation, and replacement validation

Date: 2026-10-14. Task EVT-02 is implemented. Runtime and Editor sources compile against Unity 6000.5.9f1 references. Actual gameplay sources passed 41 new checks, 57 lifecycle checks, and all 488 creature checks (586 total), executed with minimal Unity API adapters. This is not Unity Play Mode.

Reveal checks cover multiple deferred reactions, unique tokens, wrong/stale IDs, duplicate completion, final-completion window opening, late registration rejection, nested reveal cancellation, observers running behind a closed window, and source reset/replacement invalidation. These APIs allow future automatic reactions requiring player choices to hold event input closed until their exact completion.

Cancellation checks use the actual Mussel protection and per-Lobster Catch reward path. They cover automatic discard before notifications, excluding Hazard reveal consequences, preserved cancelled windows, once-only notifications, Bait exclusion, spent protection, current/stale ID validation, no-target completion without rewards, Environment/Apex exclusion, and explicit cancellation after initial reveal. Cancelled weighted/chain draws remain in encounter history.

Replacement checks cover failure preserving the original event/revision/chain, stale revision rejection, successful replacement preventing old consequences without cancellation rewards, clearing the old chain, independent cancellation of the replacement Hazard, refusing discarded targets, and blocked reveals preserving history during active resolution. Normal and authored-chain paths use the same cancellation rules without an extra draw. Reentrant reveal observers cannot restore an old chain after replacing its starter. TechniqueEffectRuntime's actual replacement backend covers event-to-event and creature-to-event transitions.

All prior lifecycle and creature regressions remain green. No authored cards, new event effects, effect enum ordinals, or depth mechanics were changed. Consequence dispatch and legal player Technique input are separate subsequent tasks. In particular, FishingRunController still gates player Technique input on Hooked state until EVT-03.

Unity Play Mode, menu execution, and rendered checks remain unverified. In a licensed Unity editor, run **Fishing Cards > Validation > Event Reveal Cancellation and Replacement**, **Event Lifecycle**, and the previous creature validation menus. Compilation and adapter checks do not replace Unity runtime/serialization verification.
