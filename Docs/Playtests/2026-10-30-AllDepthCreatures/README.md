# Temporary all-depth creature playtest — 30 October 2026

**Status: ACTIVE.** User requested all creatures in a biome at all depths to try the new tag-bar mechanic. This is a temporary data change, not a permanent balance revision or Penpot import.

Before any asset changed, [OriginalAvailability.json](OriginalAvailability.json) recorded every affected card's identity, GUID, path, original depth range and biome IDs, plus each biome tier's original bounds and complete ordered encounter pool (including non-creatures).

All 20 existing Coastal Waters creatures now have minimum depth **0** and maximum depth **-1** (unlimited). Every normal tier contains the union of that biome's existing creature pool. No new creature definitions were introduced. Original non-creature entries remain in their tiers; event depth ranges, biome membership and scheduled Apex encounters are unchanged. The biome's normal range remains 0–20, with its Apex boundary at 21. These shared assets affect both scenes that use this biome.

## Original creature ranges

| Creature | Original minimum | Original maximum |
| --- | ---: | ---: |
| Cleaner Shrimp | 1 | 14 |
| Clinging Crab | 0 | 12 |
| Cod | 3 | 20 |
| Cuttlefish | 5 | 17 |
| Flounder | 0 | 10 |
| Garfish | 0 | 9 |
| Glass Minnow | 0 | 15 |
| Jellyfish | 0 | 18 |
| Lionfish | 10 | 20 |
| Mackerel | 0 | 16 |
| Moonlit Ray | 9 | 20 |
| Mussel | 0 | 12 |
| Sardine | 0 | 14 |
| Sea Anemone | 0 | 16 |
| Sea Bass | 4 | 20 |
| Sea Urchin | 0 | 16 |
| Small Octopus | 6 | 20 |
| Spiny Lobster | 8 | 20 |
| Squid | 8 | 20 |
| Tidepool Eel | 0 | 14 |

## Original tier membership

The snapshot preserves full ordered pools by asset GUID. Originally, the shallows contained 15 creature species, mid-depth 20, and deep edge 14. The playtest appends missing creatures without moving the existing entries. Restore the original pools as well as the card ranges; restoring ranges alone would leave some former tier restrictions removed.

## Revert

Run [RestoreAvailability.cs.txt](RestoreAvailability.cs.txt) as a Unity MCP `execute_code` method body outside Play Mode. It restores only the backed-up depth fields and ordered tier pools, preserving later artwork, effect, value and weight changes. It preflights all identities, tier structure and availability edits before saving. Unexpected subsequent depth/pool edits cause it to stop for review. The restoration body is supplied for later use and has not been executed while this playtest is active.

After restoring, verify each card's minimum/maximum and each tier's ordered pool against the snapshot, then mark this playtest restored here. Do not overwrite the original snapshot or the immutable UI integration checkpoints.

## Verification

Unity verified all 20 creatures in every normal depth from 0 through 20, plus unchanged creature biome memberships: **440 checks passed**. [Verification.json](Verification.json). Native Unity serialization also materialized existing default fields and reformatted some strings; no runtime code was changed.

