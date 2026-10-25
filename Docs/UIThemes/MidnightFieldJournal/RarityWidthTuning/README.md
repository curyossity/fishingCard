# Rarity width tuning — MFJ-SYNC-008.1

User-requested Unity tuning after R046 integration: Common 87%, Uncommon 5%, Rare 8%. When Rare is unavailable at the current depth, its space goes to Common so Uncommon remains exactly 5%. Unavailable rarities remain absent. Fixed promised encounters retain their single-rarity bar; pools without Common still normalize their eligible intervals to avoid an empty/failure zone.

Updated the active profile, new-profile defaults, gameplay documentation and eligibility layout. Verified 48 timing assertions, including the exact Uncommon interval both with and without eligible Rare creatures. No art or Penpot changes, and no pending visual import.

This is a tuning update to the previously integrated R046, not another Penpot revision. [Current checkpoint](UnityCheckpoint.json); [original integration](../Sync008/README.md).
