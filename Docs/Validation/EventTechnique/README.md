# EVT-03 event Technique window validation

Date: 2026-10-14. EVT-03 is implemented. Runtime and Editor sources compile against Unity 6000.5.9f1 references. Actual runtime sources passed 38 new checks and all 586 previous checks (624 total), using minimal Unity API adapters. This is not Unity Play Mode.

Backend checks cover open event/cancelled windows, missing attached targets, valid attachment modifiers, authored CurrentEncounter versus HookedEncounter replacement targets, required event/attachment tags, information reveal relevance, explicit discard preventing encounter targeting, legal delayed effects after cancellation, closed cancelled windows, resolving/resolved rejection, direct execution not queuing effects through a closed window, and unchanged catchable targets.

Reveal checks verify that Technique input stays closed during creature/event callbacks, nested Hazard cancellation rewards, and deferred event reactions, then opens only after those reactions finish. Existing creature regression checks also retain Squid concealment and skip restrictions.

Controller checks cover actual event plays, multiple Techniques before action confirmation, normal refill without depth/encounter advancement, unresolved event replacement, legal attached-card plays after cancellation, and missing-target rejection without consuming/discarding the Technique. Suspended Descend, biome entry, and Technique/refill processing block new plays. Event resolution claim/completion closes input.

A real event-window Release Technique integration fixture exercises Cod discard recovery, Spiny Grip exact played-card recovery, full-hand discard for recovery, blocked new Techniques during choices, no premature draw/refill, and legal input reopening afterward. All previous creature and lifecycle checks remain green.

No authored card assets, enum ordinals, or event consequences changed. Hooked-only avoidance retains its established catchable target rules. This task enables event Techniques; consequence dispatch and confirmed Descend/Surface integration remain EVT-04/EVT-06.

Unity Play Mode, validation menu execution, and rendered UI checks remain unverified. In a licensed Unity editor, run **Fishing Cards > Validation > Event Technique Windows** and the existing event/creature validation menus. Inspect playable Technique slots for ordinary and cancelled events, missing targets, reveal-reaction holds, and recovery choices. Compilation and adapter checks do not establish Unity rendering or serialization correctness.
