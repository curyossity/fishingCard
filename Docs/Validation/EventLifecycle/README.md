# EVT-01 event lifecycle validation

Date: 2026-10-14. Task EVT-01 is implemented. Runtime and Editor sources compile against Unity 6000.5.9f1 references. Actual gameplay sources passed 57 new lifecycle assertions and all 488 prior creature assertions (545 total) with minimal Unity API adapters. This execution is not Unity Play Mode.

Checks cover the four event categories and exclusion of player-deck/Location/catchable cards; events never becoming Hooked or being returned for Catch Chain commitment; repeated definitions receiving independent IDs; state and asset independence across runtime instances; monotonic IDs across full/biome resets; and stale ID rejection.

Cancellation checks confirm that observers see the correct Cancelled occurrence after the visible Hazard is discarded, consequences cannot execute, the window stays closed during synchronous reveal reactions, and a post-reaction window remains after visible-card removal. Cancelled-window closure is guarded and retains the cancellation outcome. Repeated Hazard reveals have independent identities/reward notifications.

Resolution checks cover unresolved presentation removal, current-ID claims, preventing duplicate claims/completion, invalid-state calls, active resolution preventing a replacement encounter, completion after presentation removal, resolved/no-target discards, replacement by an event or catchable card invalidating old consequences, and reset rejecting an abandoned completion.

Reveal checks exercise normal weighted draws, callbacks observing the event before its window opens, repeated draws, nested replacement not reviving an old window, and callback-side presentation removal preserving lifecycle state. Existing Creature/Treasure/Apex commitment and Squid charge consumption remain correct. The full previous suite also covers Mussel cancellation and Weathered Shell rewards.

No authored card assets, effect enum ordinals, or creature behavior were changed. The public lifecycle view contains no Catch/Bait role or stats. Runtime transition APIs are the boundary future event dispatch/action coordination will use.

Unity Play Mode, serialization round trips, and editor menu execution remain unverified. In a licensed Unity editor, run **Fishing Cards > Validation > Event Lifecycle** and existing creature validation menus. Compilation and adapter tests do not establish Unity import/rendering correctness.

Subsequent tasks still own event replacement semantics beyond the state boundary, legal Technique input, consequence dispatch, duration handling, confirmed Descend/Surface coordination, UI, and full integration verification. EVT-01 does not enable the new event Technique window in gameplay.
