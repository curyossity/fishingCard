# EVT-05 event pending effects and modifier lifetimes validation

Date: 2026-10-14. EVT-05 is implemented. Runtime and Editor sources compile against Unity 6000.5.9f1 references. Actual runtime sources passed 45 new checks and all 664 previous checks (709 total), using minimal Unity API adapters. This is not Unity Play Mode.

Lifetime checks cover explicit authored data opt-in without legacy defaults, copied/shared descriptor independence, source occurrence/recipient provenance, Catch/Bait mode changes without lifetime reset, duplicate/unrelated boundary notifications, newly queued effects not consuming an old boundary token, and exact positive/negative expiry. Native/untracked contributions and historical snapshots remain unchanged. Cloned recipient provenance is found by stable effect IDs rather than object reference; this is not a Unity serialization round trip.

Pending checks cover separate copies of the same definition, exact negative removal, source-independent duration, missing targets not consuming pending application, pending-to-applied transfer retaining remaining duration, once-only application/consumption, and authored boundary expiry across fixture encounter/biome clocks. The runtime does not assume any production clock names.

Recipient/cleanup checks cover removal/exchange invalidation, resetting pending and applied contributions, monotonic IDs/epochs rejecting old cleanse and boundary callbacks, unowned negative provenance falling back to existing cleansing, and active owner rebinding rejection.

Integration checks use the actual consequence dispatcher, persistence owner, controller, and Quick Clean. A completed event's modifier survives final card discard and later cleans up independently. The controller exposes each pending/applied negative once, clears state on new runs, normal Surface and deterministic line-break endings, and preserves recorded haul/loss values. Actual Cleaner Shrimp choice resolution removes a pending event occurrence.

No production event cards or duration rules were invented. Event effect data has an explicit lifetime opt-in; old definitions retain no implied duration. Contexts require the per-run persistence owner. Concrete handlers and named boundary emission follow approved event definitions/action coordination. Reset is wired into current cleanup paths; confirmed event-to-action coordination remains EVT-06.

Unity Play Mode, menu execution, serialization round trips, and rendered UI checks remain unverified. In a licensed Unity editor, run **Fishing Cards > Validation > Event Persistence and Lifetimes** and previous event/creature validation menus. Inspect effect lifetime authoring, recipient mode changes, expiry, Quick Clean, and run-ending snapshots. Compilation and adapter checks do not establish Unity import/rendering correctness.
