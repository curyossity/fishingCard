# Mussel, Sardine, and Sea Anemone validation

Date: 2026-10-13. Runtime and Editor assemblies compile against Unity 6000.5.9f1
references. The gameplay harness passed 86 new assertions and all 222 prior
creature regressions (308 total), executing actual gameplay sources with minimal
Unity API adapters. This is not Unity Play Mode execution.

Checks cover Shell Growth after on-attach resolution, original-source/self
exclusions, per-copy stacking, effective Armored tags, Bait/non-creature
exclusions, and bonus persistence across removals/modes. Protection checks cover
Hazard classification, Apex/event exclusions, oldest attachment age independent
of replaced slots, one-use consumption and mode expiry, first Bait entry,
source removal, snapshot independence, biome persistence, normal draw history,
Technique replacements, authored-chain follow-ups, inactive cancelled encounters,
and normal next-Descend advancement.

Feeding checks cover shared-copy source selection, normal release restrictions,
declining, forced/external-removal exclusions, release-before-draw ordering,
Cod recovery before the draw request, survival after source removal, optional
exact hand discards, zero/one/two available slots, oversized recovery hands,
normal reshuffling, limited deck availability, on-attach ordering, and
coordinator action blocking/resumption.

Anemone checks cover individual automatic Technique targeting/release exclusions,
untargeted distributed effects, creature Releases, immediate mode-based immunity,
Quick Clean removing an exact targeting restriction, and two-stage hand/discard
selection. Atomic exchange checks verify existing-discard eligibility, no movement
after the first selection, invalid second selections, unchanged hand size/draw
pile, duplicate occurrences, decline, empty piles, shared-copy limits,
Bait-trigger exclusions, and coordinator resumption.

Approved stats, rarity, native tags, explicit role data, eligible depth tiers,
stable internal IDs, artwork references/layout, and artwork transforms were
checked separately. Original card GUIDs/paths remain stable. Anemone retains its
existing empty artwork references; no new artwork was invented. Its depth 0–16
range adds it to the deep tier for depth 16 only.

Unity execution and visual checks remain unverified. Prior isolated Unity startup
failed with headless entitlement and Package Manager IPC errors; current process
inspection was denied by the environment. Reference compilation and adapter-based
gameplay checks do not replace Play Mode or rendering.

In a licensed Unity editor, run **Fishing Cards > Validation > Mussel Sardine
and Sea Anemone**, then the existing creature-validation menus. The new menu
also checks the production assets. In Play Mode, inspect Sardine sacrifice,
optional-discard and finish choices, Anemone's hand/discard selection and decline
in both stages, and readability of the approved dual-effect text at supported
resolutions. Confirm a protected Hazard clears without a replacement reveal
or depth advance and that the next Descend advances normally.

GDD section 6 contains the complete approved card rules; the implementation brief,
architecture map, and effectless Catch-panel list are synchronized. SetAttachmentRole
is a runtime role mutation boundary and does not introduce a player mode-switch action.
