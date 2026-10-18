# Penpot revisions after the Unity assembly

MFJ-SYNC-001 now includes every revision below in the Unity candidate theme. [Import and validation record](Sync001/README.md). The original Unity assembly remains the immutable starting baseline. Penpot changes are recorded here and in [DesignChangeLedger.json](DesignChangeLedger.json); MFJ-SYNC-001 records the completed Unity import requested by the user. Future revisions remain separate until their own synchronization.

[UnityDesignBaseline.json](UnityDesignBaseline.json) freezes the current theme files, GUIDs, image dimensions/hashes, source catalog, and related shared presentation files. Do not overwrite that baseline when revising Penpot. It records the accepted assembled version, including the existing missing-art/source-quality boundaries; it does not certify the blocked standalone build.

## Current changes

| Revision | Change | Status | Later Unity update |
| --- | --- | --- | --- |
| MFJ-R001 | Remove added pointer-hover frames from Descend/Surface, Technique cards and compact Catch Chain cards. Keep other hover motion and state-specific indicators. | Imported and verified at MFJ-SYNC-001. | Native candidate feedback; no source PNG change. |
| MFJ-R002 | Recreate the main creature card with mockup-style engraving, large separate illustrated octopus, stat/rarity row, role icons and detailed effect panels. | Imported and verified at MFJ-SYNC-001. | Central-only sources/configuration imported; approved compact art retained. |
| MFJ-R003 | Create the editable Cod variant: new engraved illustration, Common rarity, Weight 2, Value 12, Fish/Predator tags, empty Catch effect, Bottom Feeder Bait effect. | Imported and verified at MFJ-SYNC-001. | Central-only sources/configuration imported; approved compact art retained. |

| MFJ-R004 | Garfish: separate engraving and editable card, existing gameplay data. | Imported and verified at MFJ-SYNC-001. | Central-only sources/configuration imported; approved compact art retained. |
| MFJ-R005 | Glass Minnow: separate engraving and editable card, existing gameplay data. | Imported and verified at MFJ-SYNC-001. | Central-only sources/configuration imported; approved compact art retained. |
| MFJ-R006 | Jellyfish: separate engraving and editable card, existing gameplay data. | Imported and verified at MFJ-SYNC-001. | Central-only sources/configuration imported; approved compact art retained. |

| MFJ-R007 | Lionfish: separate engraving and editable card, existing gameplay data. | Imported and verified at MFJ-SYNC-001. | Central-only sources/configuration imported; approved compact art retained. |
| MFJ-R008 | Mackerel: separate engraving and editable card, existing gameplay data. | Imported and verified at MFJ-SYNC-001. | Central-only sources/configuration imported; approved compact art retained. |
| MFJ-R009 | Moonlit Ray: separate engraving and editable card, existing gameplay data. | Imported and verified at MFJ-SYNC-001. | Central-only sources/configuration imported; approved compact art retained. |
| MFJ-R010 | Mussel: separate engraving and editable card, existing gameplay data. | Imported and verified at MFJ-SYNC-001. | Central-only sources/configuration imported; approved compact art retained. |
| MFJ-R011 | Sardine: separate engraving and editable card, existing gameplay data. | Imported and verified at MFJ-SYNC-001. | Central-only sources/configuration imported; approved compact art retained. |

| MFJ-R012 | Cleaner Shrimp: separate engraving and editable card, existing gameplay data. | Imported and verified at MFJ-SYNC-001. | Central-only sources/configuration imported; approved compact art retained. |
| MFJ-R013 | Clinging Crab: separate engraving and editable card, existing gameplay data. | Imported and verified at MFJ-SYNC-001. | Central-only sources/configuration imported; approved compact art retained. |
| MFJ-R014 | Sea Anemone: separate engraving and editable card, existing gameplay data. | Imported and verified at MFJ-SYNC-001. | Central-only sources/configuration imported; approved compact art retained. |
| MFJ-R015 | Sea Bass: separate engraving and editable card, existing gameplay data. | Imported and verified at MFJ-SYNC-001. | Central-only sources/configuration imported; approved compact art retained. |
| MFJ-R016 | Sea Urchin: separate engraving and editable card, existing gameplay data. | Imported and verified at MFJ-SYNC-001. | Central-only sources/configuration imported; approved compact art retained. |
| MFJ-R017 | Spiny Lobster: separate engraving and editable card, existing gameplay data. | Imported and verified at MFJ-SYNC-001. | Central-only sources/configuration imported; approved compact art retained. |
| MFJ-R018 | Squid: separate engraving and editable card, existing gameplay data. | Imported and verified at MFJ-SYNC-001. | Central-only sources/configuration imported; approved compact art retained. |
| MFJ-R019 | Tidepool Eel: separate engraving and editable card, existing gameplay data. | Imported and verified at MFJ-SYNC-001. | Central-only sources/configuration imported; approved compact art retained. |

The earlier chain, connector and laid-down Technique iterations are already part of the baseline and must not be imported again as pending changes. MFJ-R001 affects the added hover outline, not the cards' painted ornamental frames, Bait border or independent selection/target indicators. Keyboard/controller focus remains a separate state to verify.

MFJ-R001: [live Penpot evidence](MFJ-R001-Penpot.json) records exact source/instance IDs and read-back checks. The requested pointer-hover frames originated in Unity's native feedback code and have now been removed from the candidate pointer feedback at MFJ-SYNC-001. The approved interaction contract is stored on the Penpot page and relevant source/instance shapes using shared metadata `fishing-cards-design` / `MFJ-R001`; this metadata does not itself execute Unity hover behavior. [Synchronization verification](MFJ-R001-Verification.json) tracks the unchanged Unity baseline.

MFJ-R002: [design handoff, previews and selective-import boundary](Revisions/MFJ-R002/README.md). Preserve the already-approved compact Catch Chain art: the new wide octopus is a central-only illustration, not an automatic replacement for its shared compact portrait. The old central cards remain hidden for comparison, and the original static mockup remains intact.

MFJ-R003: [Cod handoff, preview and data verification](Revisions/MFJ-R003/README.md). The standalone Cod board uses the accepted MFJ-R002 template. Unity maps its central portrait independently at runtime. Its null Catch effect is deliberate, not missing data.

## Record each revision

Assign a stable ID (`MFJ-R001`, then increment) when work starts. Each entry records:

- The requested change, date, affected component, and exact Penpot file/page/board/shape IDs. Record every changed reusable source and affected assembled instance, including changes to shared masters.
- Before/after reference or export paths and hashes when available. Record the latest approved revision and mark abandoned iterations superseded, so an earlier image cannot be imported by mistake.
- Whether the change affects artwork, geometry, typography, source alpha/padding, interaction visuals, or a shared template. Layout-only changes still need an entry even when no PNG changes.
- Exact Unity asset IDs/paths, profile mappings, prefab/builder/runtime consumers and dependent components that need updating or verification. Include both central and compact cards when they share artwork, and all instances when a common frame changes.
- 4K display/source dimensions, transparent bounds, pivots/borders and changed placement/font/projection values as applicable. Dynamic game fields remain separate from static artwork.
- Status: `in_progress`, `ready_for_review`, `approved`, `exported`, `imported`, `verified`, or `superseded`. Store approval/export/import/verification evidence separately; seeing a change in Penpot does not mean Unity has received it.

Preserve exported revisions until the matching Unity import is verified. Link repeated requests for the same component to their predecessor rather than silently dropping the earlier pending work. Keep missing dependencies and intentionally deferred imports explicit.

## Selective Unity import

1. Reconcile all changed Penpot sources and assembly instances against the baseline/reference snapshot and ledger, including any edits made outside these recorded sessions. This is an explicit checkpoint, not automatic background monitoring of Penpot.
2. Derive an import checklist from the latest approved, unimported revisions. Separate changed image files, layout/profile values, font/configuration changes and dependent consumers. Reused, unchanged source images are not re-exported unnecessarily.
3. Check current Unity hashes/GUIDs against the baseline before applying updates. Review intervening Unity edits rather than overwriting them. Preserve asset GUIDs and update theme-owned builders/profiles along with their generated prefabs so regeneration cannot undo a revision.
4. Import only that checklist and its necessary dependencies. Rebuild/recheck affected consumers even when their own artwork did not change. Existing gameplay owners, original assets and default scene selection remain protected.
5. Compare the assembled result to the latest approved Penpot revision at 4K, check relevant dynamic states/input and other aspects, then record imported hashes and verification evidence per revision. Pending or failed rows remain pending.
6. Write a new numbered synchronization checkpoint referencing the old baseline and completed revision IDs. Retain the original baseline and revision history; do not reset the ledger or treat an export as a completed import.

Use the existing [visual contract](VisualContract.md), [source catalog](../../../Assets/UIThemes/MidnightFieldJournal/Profiles/SourceAssetCatalog.json), [missing-assets ledger](MissingVisualAssets.md) and [retirement boundary](UI08Retirement.md) alongside these records. This revision round does not authorize deleting the old theme or importing a flattened mockup as reusable game UI.
