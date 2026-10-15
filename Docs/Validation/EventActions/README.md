# EVT-06 confirmed event actions validation

Date: 2026-10-14. EVT-06 is implemented. Runtime and Editor sources compile against Unity 6000.5.9f1 references. Actual runtime sources passed 45 new checks and all 709 previous checks (754 total), using minimal Unity API adapters. This is not Unity Play Mode.

Action checks cover effectless/no-target resolution, exactly one normal Descend advance and next reveal, Surface effects before haul/load/result capture, and cancelled Hazard confirmation without consequences. Cancellation retains legal Techniques until confirmation. Replacement prevents the old event's effects. Deferred reveal reactions block confirmation until complete.

Queue checks use actual creature attachment draws and full-hand discard choices to suspend both Descend and Surface. Repeated action requests, Release, Techniques, biome entry, reentrant refresh/choice callbacks, and stale choice completion cannot interrupt or duplicate the pipeline. Choices finish before the action resumes, and the next encounter restores its normal input window.

Validation rejects unsupported handlers before confirmation, leaving the reaction window open with an exposed reason. A handler fault after partial mutation stays locked without retries or automatic creature-choice processing; explicit new-run initialization clears that state. No production handler or legacy event behavior was inferred. Approved individual event definitions must supply their handlers and any named lifetime boundary emissions; effectless events require no handler.

Cleanup checks cover voluntary Surface, post-event overload/forced line-break ending, and new-run reset. Results retain resolved haul/loss values while live event modifiers, pending work, and trails clear immediately. Separate ascent catch copies preserve values, and copied effect records reference those copies rather than live instances. Presentation completion clears only presentation data. Snapshot isolation is tested; animated rendering is not.

Previous event lifecycle, reveal, Technique, dispatch, persistence, and all refactored coastal creature suites pass. Durable checks are in Assets/Editor/EventActionChecks.cs, with menu **Fishing Cards > Validation > Confirmed Event Actions**.

Unity Play Mode, menu execution, serialization round trips, and rendered UI checks remain unverified. Run the validation menus in a licensed Unity editor and inspect suspended action choices, cancelled-event input, ascent snapshots, and result timing. Run UI/Events Collector alignment is EVT-07; complete integrated Unity verification is EVT-08.
