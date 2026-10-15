# Technique rules validation

Validated 2026-10-15 against the approved shared Technique rules and Cod's discard-one-or-decline full-hand policy.

The implementation adds separate resolving-card ownership, required player target selection before commitment, a four-card hand cap, and exact Cod/Sea Urchin recovery ordering. It retains reaction-window restrictions, Sea Anemone targeting immunity, Iron Grip, and targetless-effect behavior. Existing Load Lifter, Counterweight Rig, Prize Tag, and Double Release text now describes player-selected recipients. Double Release retains its up-to-two count and permits committing one selected creature. No new energy resource, biome counter, or additional Technique design is introduced.

## Executed checks

- Runtime sources and Editor sources compile against Unity 6000.5.9f1 reference assemblies using its bundled .NET SDK.
- A .NET adapter harness executes the real rule classes with minimal Unity object/view adapters: **884 assertions passed**, including **47 new Technique assertions** and 837 assertions in the existing event/creature suites. Two existing suites were extended to check the approved hand-cap behavior instead of expecting overflow.
- Technique coverage includes cancellable selection, explicit current-encounter targets, stale and duplicate target rejection, targeting and Release restrictions, multi-effect target removal without retargeting, resolving-card exclusion from discard/reshuffle, bounded setup/draw/refill/recovery, exact Cod recovery with a full hand, Cod-before-Urchin recovery without duplication, reentrant input rejection, and failure-state ownership/input guards.
- Existing fixtures now supply explicit targets through test helpers. Those helpers are not used by gameplay to choose targets automatically.

See [runtime results](runtime-results.txt) and [machine-readable summary](validation-summary.json).

## Unity verification remaining

These are adapter executions, not Unity PlayMode results. The `Fishing Cards/Validation/Technique Rules` menu includes four additional Editor assertions using the production run prefab: prefab availability, target-selection action locks, restoration of actions after completion, and concealment masking in the encounter-target modal. The combined event integration menu also includes them. These Editor assertions compile but have not been run in Unity.

Actual modal interaction/layout, Unity serialization round trips, domain reload during pending selection/resolution, and PlayMode behavior remain unverified. The earlier isolated Unity startup was blocked by licensing entitlement and Package Manager IPC issues; see [that startup evidence](../EventIntegration/unity-startup-summary.txt). No new Unity launch is claimed for this change.
