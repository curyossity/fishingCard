# EVT-04 event consequence resolution validation

Date: 2026-10-14. EVT-04 is implemented as a shared dispatch and creature-reaction boundary. Runtime and Editor sources compile against Unity 6000.5.9f1 references. Actual runtime sources passed 40 new assertions and all 624 previous assertions (664 total), using minimal Unity API adapters. This is not Unity Play Mode.

Preflight checks cover unsupported later effects preventing partial execution, rejected handler data, validation exceptions, malformed null effects, duplicate handler registration, stale contexts, and claiming only after the whole plan passes. Empty and no-target events complete without Hazard cancellation or Release accounting; cancelled/replaced events cannot dispatch. Repeated input/advances do not replay consequences. Dispatcher reuse clears old cursor/counters, and replacing the source definition array cannot change the validated plan snapshot.

Reaction checks use the actual CreatureEffectRuntime and TechniqueDeckRuntime. A registered fixture consequence applies a runtime modifier and queues a creature's existing immediate draw behavior. The dispatcher pauses before subsequent consequences and before final completion, including during a full-hand discard/draw choice. Repeated advances and handler callback reentry cannot replay the first mutation or complete early. Actual creature preparation/choice completion permits exactly one resume and final discard without attaching the event or reopening Techniques. Automatic queued reactions and prior creature work are also covered.

Fault/ownership checks cover handler exceptions preserving a closed Resolving occurrence without retries, coordinated reset, foreign contexts with coincident occurrence IDs, reset invalidating completion, and minimal registry/context rebinding simulation. That simulation does not verify Unity serialization/domain reload.

No authored event card assets or specific production event effects were invented. The runtime validates handlers registered by future setup; missing implementations are explicit preflight errors. Duration handlers are EVT-05 and controller confirmation/resumption is EVT-06. Test handlers are fixtures exercising approved runtime owners and are not registered in the game.

Unity Play Mode, validation menu execution, serialization round trips, and rendered checks remain unverified. In a licensed Unity editor, run **Fishing Cards > Validation > Event Consequence Resolution** and previous event/creature menus. Reference compilation and adapter checks do not replace Unity runtime verification.
