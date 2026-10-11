# Clinging Crab and Cod validation

Runtime and editor assemblies compile against Unity 6000.5.9f1 references.
The standalone harness passed 34 Crab/Cod gameplay and controller assertions
and all 40 existing Cleaner Shrimp regression assertions, using the real
gameplay source files with minimal Unity API adapters.

Checks cover selected-role locks, Technique release filtering, forced removal,
exchanges, legal Catch/Bait targets, lost unused Claw Cut, explicit empty roles,
shared Cod limits, empty-discard consumption, non-creature exclusion, creature
Release triggers, exact discard occurrences, excess hand recovery, completed
trigger lifetime, just-used recovery before reshuffle, encounter replacement,
normal advancement reset, and multi-effect/multi-target Technique timing.

Unity execution and UI rendering were not verified. The preceding isolated
editor attempt failed during startup due to missing headless entitlement and
Package Manager IPC. Compilation and adapter-based rules checks do not replace
Play Mode or visual inspection. In a licensed editor, run **Fishing Cards >
Validation > Crab and Cod** and **Fishing Cards > Validation > Cleaner Shrimp**.
Inspect Claw Cut/recovery modal choices and recovered hand-page navigation
in Play Mode at supported resolutions.

Used Techniques enter discard before effects; refill waits until recovery
choices finish and then fills only up to normal hand size. Recovery itself may
exceed that size. Existing card GUIDs, stable internal IDs, and artwork references
are retained. Cod joins the depth 16–20 pool; Crab leaves that ineligible pool.
