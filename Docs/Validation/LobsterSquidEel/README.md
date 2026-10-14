# Spiny Lobster, Squid, and Tidepool Eel validation

Date: 2026-10-14. Runtime and Editor assemblies compile against Unity
6000.5.9f1 references. The gameplay harness passed 80 new assertions and all
408 prior creature regressions (488 total), executing actual runtime sources
with minimal Unity API adapters. This is not Unity Play Mode execution.

Lobster checks cover other-creature-only support in both roles, live Armored
loss/regain, stacking and floor zero, self-exclusion, non-creature exclusion,
source removal/mode expiry, complete revealed-Hazard notification, independent
per-Catch rewards, Bait exclusion, retained bonuses across modes, and unchanged
authored stats. Normal reveal/removal and non-Hazard notifications do not award
Weathered Shell.

Ink checks cover exactly two independent charges, source removal, non-creature
and Apex exclusions, one assigned charge per reveal, identity release before
attachment, removal and replacement consumption, stacked consecutive charges,
biome preservation and full reset, exact pending/current Quick Clean removal,
other grants surviving cleansing, and no ordinary reveal-Technique bypass.
Skip checks cover once-per-card Bait grants, source removal/mode independence,
Apex exclusion, immediate concealed decisions, accepting/declining consumption,
no attachment or Release accounting, FIFO consecutive offers, and reveal effects
removing the original creature without moving its offer to another encounter.

Eel checks cover only other legal Elusive creature Baits, release locks and
Catch exclusions, optional decline/no-target expiry, deliberate Release reactions,
shared draw counts across copies, draws deferred until action completion,
multiple optional discards before any group draws, forced-removal exclusion,
Eel removed later in the action or leaving Bait, surviving-copy fallback,
pre-removal effective tags after their source is removed, and four-slot capacity
for groups larger than four.

Coordinator checks cover concealment binding and both hidden flags, reveal before
mandatory Catch on-attach choices, paused/resumed Descend, immediate skip blocking
new Techniques/core actions, unchanged depth on skip, normal Descend afterward,
Mussel cancellation to Lobster reward, biome-persistent Ink/skip, and a clean new
run. Presentation source review confirmed masking both art layers, name, stats,
both effects, tags/tooltips and rarity markers, with both role panels still
selectable; the identity flag reaches standby transitions and history/ascent
snapshots. This source review does not establish rendered visual correctness.

Approved stats, rarity, native tags, explicit role effects, internal IDs, artwork
references/layout/transforms, and every intersecting depth-tier pool membership
were checked separately. Existing effect ordinals 0–43 are preserved; new effects
44–49 and trigger 12 are appended. Existing GUIDs and paths remain stable,
including lowercase Assets/Cards/squid.asset. Existing empty/builtin artwork
references are retained; no replacement artwork was invented.

Unity Play Mode, menu execution, import and rendered visual checks remain
unverified. Prior isolated-editor startup failed with missing headless entitlement
and Package Manager IPC errors. Reference compilation and adapter gameplay
execution do not replace licensed Unity runtime verification.

In a licensed Unity editor, run **Fishing Cards > Validation > Lobster Squid and
Eel** and the existing creature validation menus. The new menu also validates
production assets. In Play Mode, inspect hidden encounters in both selected
roles, incoming transitions and ascent history, hidden skip accept/decline,
replacement/avoidance, reveal before on-attach modals, Hazard cancellation rewards,
and single/group Eel discard/draw choices. Check card text readability at
supported resolutions and that hovering stale tag positions cannot show tags.

Approved rules are integrated into GDD section 6, the implementation brief,
Architecture.md, and Card Visual Specification.md. Ink Escape is an effect-granted
immediate choice; it does not add a general free skip action.
