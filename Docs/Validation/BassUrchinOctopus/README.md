# Sea Bass, Sea Urchin, and Small Octopus validation

Date: 2026-10-14. Runtime and Editor assemblies compile against Unity 6000.5.9f1
references. The gameplay harness passed 100 new assertions and all 308 prior
creature regressions (408 total), executing actual runtime sources with minimal
Unity API adapters. This is not Unity Play Mode execution.

Sea Bass checks cover mandatory chosen Technique discard without playing/drawing,
empty-hand skipping, legal distinct Small targets, both attachment roles,
release locks and non-creature exclusions, no mutation after the first selection,
atomic validation, simultaneous pre-group Value snapshots, all targets absent
before Release reactions, per-creature Lionfish penalties, separate Ray transfers,
removed observers not triggering, declining, and one-use persistence across modes.

Urchin checks cover a flat conditional Predator weight penalty, Catch/Bait
Predators, effective tags, non-creature exclusions, preview independence,
Quick Clean, role-specific cleansed indices, and snapshot independence.
Spiny Grip checks cover independent FIFO grants after source removal/mode changes,
automatic individual selected targets, untargeted/Hooked-only exclusions,
play-time tag eligibility, one grant per multi-target play, released targets,
exact played discard tokens versus identical copies, missing/reshuffled occurrences,
declining, discard-for-room, and recovery after Cod choices but before normal refill.
An oversized hand that cannot fit the recovery after its one allowed discard skips
without discarding or substituting a copy.

Octopus checks cover immediate one-card draws, full-hand discard/decline, one
discard maximum, oversized-hand skipping, required legal Catch/Bait pairs,
no mutation after the first selection, stable positions/IDs/permanent modifiers,
new Catch on-attach before new Bait, removal of the prospective new Bait,
ending old tag grants while preserving native tags, protection expiry,
new Urchin grants, skipped/declined activations remaining spent, release locks
not becoming mode locks, and returning a spent Crab to Bait without another release.
Per the user's confirmation, swaps activate only the two cards' own on-attach
effects; Shell Growth and other after-catching reactions do not trigger. Other
cards' pending next-attachment effects are not consumed; the new Catch's own
Tangle still queues its independent effect.

Integrated coordinator checks cover mandatory-discard action blocking/resumption,
simultaneous releases without encounter/depth advancement, Hooked-only targeting,
Cod-to-Spiny full-hand recovery ordering, duplicate played-card recovery by Cod,
and ordered mode swapping without creating an encounter.

Approved stats, rarity, native tags, explicit role effects, internal IDs,
artwork references/layout/transforms, and eligible depth-tier pool membership
were checked separately. Existing effect enum ordinals are preserved. Card
GUIDs and paths remain stable; Sea Urchin retains its existing empty artwork
references. No replacement artwork was invented.

Unity Play Mode, menu execution, and visual checks remain unverified. Prior
isolated-editor startup failed with missing headless entitlement and Package
Manager IPC errors. Reference compilation and adapter-based gameplay execution
do not replace Unity runtime/import/rendering verification.

In a licensed Unity editor, run **Fishing Cards > Validation > Sea Bass Urchin
and Small Octopus**, followed by the existing creature validation menus. The new
menu also checks production assets. In Play Mode, inspect mandatory hand discard,
Scatter selection/finish, exact recovery discard/decline, Clever Catch draw
choices, and Helping Arms' Catch/Bait pairing and resulting modal order. Confirm
dual-effect card text remains readable at supported resolutions.

The approved rules are integrated into GDD section 6 and the implementation
brief. Architecture.md records simultaneous-removal snapshots, exact discard
occurrences, per-role lifecycle/cleansing state, and deferred role activation.
Helping Arms provides the requested mode exchange; no free general mode-switch
action was introduced.
