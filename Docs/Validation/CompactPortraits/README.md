# Approved creature compact artwork coverage

The candidate Unity Catch Chain previously mapped only four compact portraits, leaving fourteen approved creatures without visible illustrations. The fix explicitly maps those fourteen to their existing approved central illustration sprites, with independent compact bounds. Sardine, Clinging Crab, Glass Minnow and Small Octopus keep their dedicated compact portraits. No new images, Penpot revisions, gameplay definitions or encounter pools were changed.

The user requested using only the eighteen approved creature illustrations. Cuttlefish, Flounder, Harbor Leviathan and Stormjaw Tuna remain outside this artwork scope; treasures also retain empty portrait slots.

`MidnightFieldJournalPrefabBuilder.Build()` and `JournalCompactArtworkChecks.Run()` succeeded in Unity. The checks passed 93 assertions, covering all eighteen identities, exact portrait binding, original override precedence, live name/weight/value/Bait fields and portrait clearing when a pooled view receives an unmapped identity. Both contact sheets were rendered using the actual compact-card prefab; the 4K sheet was visually inspected.

- [4K roster](CompactRoster4K.png)
- [1080p roster](CompactRoster1080p.png)
- [Verification report](Verification.json)

The additional `JournalCatchChainChecks.Run()` regression was not executed: automatic approval review failed due to a usage limit. No new Play Mode interaction verification is claimed for this fix. Unity reported no compile errors before the successful build and artwork checks.
