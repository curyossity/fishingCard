# Penpot revisions after the Unity assembly

MFJ-SYNC-001 includes MFJ-R001–MFJ-R019 in the Unity candidate theme. MFJ-SYNC-002 now imports and verifies MFJ-R020–MFJ-R024, covering all five active event cards. [Import and validation record](Sync001/README.md). The original Unity assembly remains the immutable starting baseline. Penpot changes are recorded here and in [DesignChangeLedger.json](DesignChangeLedger.json); MFJ-SYNC-001 records the completed Unity import requested by the user. Future revisions remain separate until their own synchronization.

[UnityDesignBaseline.json](UnityDesignBaseline.json) freezes the current theme files, GUIDs, image dimensions/hashes, source catalog, and related shared presentation files. Do not overwrite that baseline when revising Penpot. It records the accepted assembled version, including the existing missing-art/source-quality boundaries; it does not certify the blocked standalone build.

## Pending Unity changes

**Two new pending revisions: MFJ-R037–R038.** MFJ-R031–R036 are now imported and verified at [MFJ-SYNC-004](Sync004/README.md), joining the previous26 completed revisions. The four historical superseded drafts remain excluded. Latest comparison baseline: [Sync004 checkpoint](Sync004/UnityCheckpoint.json). Use the [pending index](PendingUnityChanges.json) to avoid reimporting completed work. Design-creation notes below are historical where their original Penpot-only status is superseded by this checkpoint.

## Change history

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

| MFJ-R020 | Detailed event template plus Reef Snag engraving; same1260×1400 creature-card canvas, full-width effect panel and editable event fields. | Imported and verified at MFJ-SYNC-002. | Replace candidate event frame/layout and add Reef Snag central event mapping. [Handoff](Revisions/MFJ-R020/README.md). |

| MFJ-R021 | Silt Burst: separate engraved illustration, editable event card using R020 template. | Imported and verified at MFJ-SYNC-002. | Exact central-only event mapping; [handoff](Revisions/MFJ-R021/README.md). |
| MFJ-R022 | Gentle Upwelling: separate engraved illustration, editable event card using R020 template. | Imported and verified at MFJ-SYNC-002. | Exact central-only event mapping; [handoff](Revisions/MFJ-R022/README.md). |
| MFJ-R023 | Loose Knot: separate engraved illustration, editable event card using R020 template. | Imported and verified at MFJ-SYNC-002. | Exact central-only event mapping; [handoff](Revisions/MFJ-R023/README.md). |
| MFJ-R024 | Fresh Shell Fragments: separate engraved illustration, editable event card using R020 template. | Imported and verified at MFJ-SYNC-002. | Exact central-only event mapping; [handoff](Revisions/MFJ-R024/README.md). |

| MFJ-R025 | Initial tooltip draft. | **Superseded by R026; never imported.** | Do not queue; retained for comparison. |
| MFJ-R026 | Detailed engraved tooltip with native role icons. | **Superseded by R027; never imported.** | Do not queue; retained for comparison. |
| MFJ-R027 | Pointer-free engraved Catch Chain inspection tooltip with live tags/current-role effect and native role icon. | **Imported and verified at MFJ-SYNC-003.** | [Implementation](Sync003/README.md); do not queue again. |
| MFJ-R028 | Initial Technique fan arrangement. | **Superseded by R029; never imported.** | Historical preview retained; do not queue. |
| MFJ-R029 | Lowered middle pair without restoring outer-card alignment. | **Rejected and superseded by R030; never imported.** | Historical evidence; do not queue. |
| MFJ-R030 | Approved shallow fan; adaptive layout for arbitrary physical hand counts, matching stable hit planes. | **Imported and verified at MFJ-SYNC-003.** | [Implementation](Sync003/README.md); do not queue again. |

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

Import state is explicit: `unityImported`, `integrationStatus`, `synchronizationCheckpoint` and import evidence must agree. Generate the next-import checklist from the pending index, never from the full history table.

## Missing Technique illustrations — MFJ-R031–R034

Clear Water, Hold Steady, Re-rig and Recast now have separate transparent engravings and editable Penpot cards. All four are ready for visual review and remain pending Unity integration. Existing completed/superseded revisions retain their status. [Preview and source IDs](Revisions/MFJ-R031/README.md).

## MFJ-R035 — themed Line Load

New separate engraved brass/navy housing and editable values, segments and capacity-state text. Updated the four gameplay assemblies while hiding the replaced meter shapes and retaining haul. Penpot only, awaiting review and Unity integration. [Handoff and previews](Revisions/MFJ-R035/README.md).

## MFJ-R036 — matched marine action buttons

New Descend/Surface pair, exactly880×350 each. Functional bronze hardware, fine wave engraving and navy enamel distinguish the buttons from the parchment/botanical cards. Separate editable sources and static face exports are retained; eight gameplay instances updated. Penpot only, ready for review. [Preview and handoff](Revisions/MFJ-R036/README.md).

## MFJ-SYNC-004 — latest Unity integration

MFJ-R031–R036 are imported and verified: four missing Technique illustrations, the engraved Line Load gauge and matched Descend/Surface buttons. No pending revisions remain. [Evidence and preservation boundaries](Sync004/README.md).

## Parchment control alternatives — MFJ-R037–R038

User found the imported buttons and meter too metallic. New paper/ink designs match the cards’ aged ivory, fine engraving and marine botanical sketches. Review-only Gameplay E preserves the previous assemblies and current Unity implementation. MFJ-R037 covers matched action controls; MFJ-R038 covers dynamic Line Load. [Preview and sources](Revisions/MFJ-R037/README.md). These two revisions remain pending review/import;32 earlier imports remain completed.

## Opening boat card — MFJ-R039

New illustration-only run-opening card with a fisherman fishing from a small wooden boat. Independent frame and scene layers, same1260×1400 footprint as the main cards. No stats, mode or effects. [Preview, source IDs and import boundary](Revisions/MFJ-R039/README.md). Penpot only; all Unity files preserved. Pending: R037, R038, R039.

## Illustrated depth guide — MFJ-R040

Restores the original mockup’s fine depth line, small engraved corals and amber current-tier emphasis. Assembled in Gameplay F while preserving Gameplay E. Actual tier labels/ranges are native editable fields; no gameplay changes. [Preview, source IDs and import contract](Revisions/MFJ-R040/README.md). Pending Unity integration.

## MFJ-SYNC-005 — Latest integration

R037–R040 are imported and verified: parchment action controls, paper/ink load meter, opening boat and illustrated live depth guide. **0 pending;36 completed;4 superseded.** [Integration evidence and layout adaptation](Sync005/README.md). Earlier pending statements above are historical and superseded by this checkpoint.

## Unity depth-guide focus refinement — MFJ-SYNC-005.1

Only the current tier keeps original artwork colors and amber emphasis. Other tiers use a shared grayscale material at42% artwork opacity, with subdued neutral labels/ranges. State changes follow actual depth and clear on pooled-row reuse. No artwork regeneration or pending Penpot import was created. Native Penpot study retains its previous colors; this is a Unity-only presentation override. Current comparison baseline: [MFJ-SYNC-005.1](DepthGuideFocus/UnityCheckpoint.json). [Preview and checks](DepthGuideFocus/README.md). Completed/imported revision counts remain36/0pending/4superseded.

## Unity Technique hand position — MFJ-SYNC-005.2

Hand lowered48 design pixels (24 at1080p), preserving fan/perspective/hover and fixed sidebar pile counts. [Preview and verification](HandPosition/README.md). Unity-only change, no pending Penpot import. Current baseline:[HandPosition/UnityCheckpoint.json](HandPosition/UnityCheckpoint.json).
