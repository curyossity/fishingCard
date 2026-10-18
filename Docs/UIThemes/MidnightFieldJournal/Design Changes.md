# Penpot revisions after the Unity assembly

The current Unity assembly is the starting point for this revision round. Future Penpot changes are recorded here and in [DesignChangeLedger.json](DesignChangeLedger.json); importing them into Unity is a separate step requested by the user.

[UnityDesignBaseline.json](UnityDesignBaseline.json) freezes the current theme files, GUIDs, image dimensions/hashes, source catalog, and related shared presentation files. Do not overwrite that baseline when revising Penpot. It records the accepted assembled version, including the existing missing-art/source-quality boundaries; it does not certify the blocked standalone build.

## Current changes

No new design revisions yet. The earlier chain, connector and laid-down Technique iterations are already part of the baseline and must not be imported again as pending changes.

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
