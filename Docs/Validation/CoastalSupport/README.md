# Garfish, Glass Minnow, and Jellyfish validation

Date: 2026-10-11. Runtime and editor assemblies compile against the project's
Unity 6000.5.9f1 references. The standalone harness passed 61 new assertions
and 74 existing Cleaner Shrimp/Crab/Cod regressions (135 total), using actual
gameplay sources with minimal Unity API adapters. Authored stats, encounter-pool
membership, preserved internal IDs, artwork references, and art transforms were
checked separately from the adapter-based fixture suite.

The new checks cover native and overlapping tag grants, role and creature
eligibility, Swift recalculation, runtime-tag Technique targeting, grant timing
before attachment triggers, preview/snapshot independence, Tangle source removal,
Bait and non-creature exclusions, multiple queued penalties, pending/applied
cleansing, recipient role changes, replacement attachments, Buoyant Bell distinct
optional targets, suspension/restoration on Armored changes, role-independent
target retention, stacking/zero clamping, no later retargeting, redraw ordering,
multiple Minnow coalescing, decline, empty hands, exact single-card replacement
for oversized hands, normal reshuffle, removed-source/recipient exclusions, and
controller action suspension/resume.

Unity execution and visual checks remain unverified. The earlier isolated-editor
startup failed with missing headless entitlement and Package Manager IPC errors.
This change was verified by compilation and the adapter-based gameplay harness;
those checks do not replace Play Mode or rendering checks.

In a licensed Unity editor, run **Fishing Cards > Validation > Garfish Minnow and
Jellyfish**, followed by **Crab and Cod** and **Cleaner Shrimp** in the same menu.
Inspect Shoal Signal's hand selection and decline option and Buoyant Bell's
selection count/finish option in Play Mode at supported resolutions.

The source of truth for these cards is GDD section 6; runtime responsibilities
are recorded in Architecture.md. No mode-switch gameplay action was added.
Tangle's applied modifier and Buoyant Bell target IDs are independent of the
recipient's role, preserving the supplied rules if another approved ability
later changes that role.
