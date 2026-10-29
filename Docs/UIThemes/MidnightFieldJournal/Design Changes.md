# Penpot revisions after the Unity assembly

MFJ-SYNC-001 includes MFJ-R001–MFJ-R019 in the Unity candidate theme. MFJ-SYNC-002 now imports and verifies MFJ-R020–MFJ-R024, covering all five active event cards. [Import and validation record](Sync001/README.md). The original Unity assembly remains the immutable starting baseline. Penpot changes are recorded here and in [DesignChangeLedger.json](DesignChangeLedger.json); MFJ-SYNC-001 records the completed Unity import requested by the user. Future revisions remain separate until their own synchronization.

[UnityDesignBaseline.json](UnityDesignBaseline.json) freezes the current theme files, GUIDs, image dimensions/hashes, source catalog, and related shared presentation files. Do not overwrite that baseline when revising Penpot. It records the accepted assembled version, including the existing missing-art/source-quality boundaries; it does not certify the blocked standalone build.

## Pending Unity changes

**One pending selected design: MFJ-R051** (ready_for_review). R050 is superseded; all39 prior integrations remain verified. [Pending index](PendingUnityChanges.json).

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

New Descend/Surface pair, exactly 880×350 each. Functional bronze hardware, fine wave engraving and navy enamel distinguish the buttons from the parchment/botanical cards. Separate editable sources and static face exports are retained; eight gameplay instances updated. Penpot only, ready for review. [Preview and handoff](Revisions/MFJ-R036/README.md).

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

## Unity Catch Chain follow behavior — MFJ-SYNC-005.3

Newest catch remains visible by default. Manual wheel/drag inspection preserves its offset; returning to the bottom resumes following. Empty/short chains reset follow state. [Behavior and checks](CatchChainFollow/README.md). Unity-only change; no pending Penpot import. Current baseline:[CatchChainFollow/UnityCheckpoint.json](CatchChainFollow/UnityCheckpoint.json).

## Technique hand backing sheet — MFJ-R041

New independent dark sheet artwork is prepared. Penpot assembly awaits a connected design page. [Source and progress](Revisions/MFJ-R041/README.md). Not integrated into Unity;1 pending revision,36 completed imports,4 superseded drafts. Next revisionR042.

## MFJ-R041 — Penpot assembly ready

Sheet is assembled under the latest hand in Gameplay G, with a separate transparent source and isolated review. Previous awaiting-connection note is now resolved. [Preview and exact handoff](Revisions/MFJ-R041/README.md). Status:ready_for_review; Unity unchanged.1 pending revision,36 completed imports,4 superseded drafts.

## MFJ-SYNC-006 — Sheet integrated and boat transition fixed

R041 is imported and verified. The boat stays visible inside the outgoing encounter slot and moves upward with the existing animation; completion/cancellation restores the reusable art. [Verification and captures](Sync006/README.md). **0 pending,37 completed,4 superseded.** Earlier pendingR041 statements are historical.

## Explicit creature role choice — MFJ-SYNC-006.1

No default Catch/Bait choice; Descend requires explicit selection for each new creature/Apex occurrence. Runtime-owned choice survives ordinary refreshes and resets on replacement, including the same species. Role buttons remain usable while Descend is disabled; Surface, boat and event windows retain their existing availability. [Verification and current baseline](../../Validation/ExplicitAttachmentChoice/README.md). Unity-only gameplay/UI change, no pending Penpot import.

## Smooth catch-chain follow — MFJ-SYNC-006.2

Implemented in Unity: newly attached creatures scroll into view with a 0.32-second eased movement instead of snapping. Wheel/drag inspection interrupts following; returning to the bottom resumes it. Repeated refreshes preserve progress and rapid attachments retarget smoothly. [Verification and current baseline](SmoothCatchChain/README.md). Unity-only presentation change; no new Penpot import or pending design revision.

## Catch/Bait selection visibility — MFJ-SYNC-006.3

Implemented directly in Unity on 20 October 2026. Selected role outlines now fit the full rounded artwork panels and use a brighter 8px gold edge, warm glow, inset hairline and native checkmark. Rules text and artwork retain their layout. Existing explicit role choice, exclusive selection, concealment and event behavior passed actual pointer/state checks. [Captures, verification and current baseline](RoleSelection/README.md). No new Penpot revision or pending import.

## Catch/Bait fading trial — MFJ-SYNC-006.4

Implemented directly in Unity on 20 October 2026 for user review. The selected panel keeps its original appearance; the unselected panel is dimmed with a46% black rounded overlay. Both are bright before selection and remain clickable. Previous gold borders/checkmarks are inactive. New occurrences, concealed role choice and event resets passed actual pointer/state checks. [Captures, verification and current baseline](RoleFading/README.md). No new Penpot revision or pending import.

## Asymmetric Catch/Bait fade corners — MFJ-SYNC-006.5

Implemented in Unity on 20 October 2026: the fade follows Catch’s larger lower-left corner and Bait’s mirrored lower-right corner, inset from the printed border. Opacity, role input and text layout are unchanged. [Captures, pixel verification and current baseline](RoleFadeCorners/README.md). Unity-only correction; Penpot history and pending imports unchanged.

## Subtle selected-role edge — MFJ-SYNC-006.6

Implemented directly in Unity on 20 October 2026. Selected Catch/Bait gains a soft3px gold edge matching the fade’s exact asymmetric corners and inset. The opposite role retains its approved fade; glow/check decorations stay off. [Captures, verification and current baseline](RoleSoftHighlight/README.md). No new Penpot revision or pending import.

## Selected-role inner glow trial — MFJ-SYNC-006.7

Implemented in Unity on 20 October 2026. Actual previous outer meshes matched exactly, but their authored contour approximates the printed art. The sharp line is replaced by a soft inward glow using the same geometry renderer as the unchanged opposite-panel fade. [Contour audit, captures and current baseline](RoleInnerGlow/README.md). No new Penpot revision or pending import.

## Initial role dimming — MFJ-SYNC-006.8

Implemented in Unity on 20 October 2026: both panels start faded; opacity increased from46% to58%. Selected panel returns to full brightness with its unchanged glow; alternative stays faded. [Captures, verification and baseline](RoleInitialFade/README.md). No new Penpot revision or pending import.

## Animated main-card chain stats — MFJ-SYNC-006.9

Implemented directly in Unity on 20 October 2026: chain-modified Weight/Value grow, turn green/red by benefit and count one integer at a time. Refreshes retain progress; new occurrences, masks, role-only zero Value and history are handled separately. Gameplay values do not wait for the animation. [Captures, checks and current baseline](StatFeedback/README.md). No new Penpot revision or pending import.

## Springy number-only stat feedback — MFJ-SYNC-006.10

Implemented in Unity on20 October2026. Counting stats now pop with a short scale spring and alternating tilt on every integer; the final beat is slightly stronger and settles to the existing enlarged green/red style. Fonts, artwork, pacing and gameplay remain unchanged. [Checks and current baseline](StatPunch/README.md). No new Penpot revision or pending import.

## All-source current-creature stat feedback — MFJ-SYNC-006.11

Implemented directly in Unity on 20 October 2026. Main-card Weight/Value feedback now compares all resolved effects with printed stats, including Techniques, events, own abilities and the Catch Chain. Choosing Bait sets sale Value to neutral zero immediately; returning to Catch counts only effect modifiers from printed Value. Existing colours, integer pacing, spring motion and masking remain. [Verification and current baseline](AllStatFeedback/README.md). No new Penpot revision or pending import.

## Sequential Catch Chain stat feedback — MFJ-SYNC-006.12

Implemented directly in Unity on 20 October 2026. Compact creatures reuse the main-card animator for all resolved Weight/Value modifiers. Affected recipients run newest to oldest, scrolling into view before counting and waiting for the full final spring before the next begins. Physical views retain runtime identity across removals; queued cards defer emphasis. Bait zero and concealed Value retain their rules. Manual scrolling settles/cancels automatic feedback, and ended-run snapshots remain static. [Verification and current baseline](ChainStatFeedback/README.md). No new Penpot revision or pending import.

## Catch Chain card pop and flash — MFJ-SYNC-006.13

Implemented directly in Unity on 20 October 2026. Each affected Catch/Bait now receives a gentle 4.5% centered pop and warm ivory flash before its existing number animation. Scroll padding accommodates the expansion; chain connectors remain still. The newest-to-oldest sequence waits for both stages, and interruption/removal restores resting visuals. [Verification and current baseline](ChainCardPulse/README.md). No Penpot revision or pending import was added.

## Rapid numeric feedback — MFJ-SYNC-006.14

Implemented directly in Unity on 20 October 2026. Both current and compact creatures use 25ms ordinary steps, a 50ms initial hold and a 180ms final spring. Every displayed change retriggers the existing 14% scale punch and alternating 2.5-degree tilt; the final beat remains 22%. Large deltas use bounded strides targeting about 0.6 seconds of counting, subject to frame rate, rather than long one-unit sequences. Each frame displays at most one transition so every visible change receives its own pop/tilt. Fractional timing carries forward. Colour, masking, neutral Bait zero, retargeting, card pop/flash and newest-to-oldest chain serialization remain unchanged. [Verification and current baseline](RapidStatFeedback/README.md). No Penpot revision or pending import was added.

## Bold manipulated stats and adjusted pace — MFJ-SYNC-006.15

Implemented in Unity on 20 October 2026. Both main and compact creature numbers use 40ms ordinary steps instead of 25ms; large-delta counting targets 0.8 seconds instead of 0.6. Initial hold, per-change pop/tilt, final spring, card pop/flash and chain ordering are retained. Manipulated stats become bold while animating and remain bold at a non-default result. Reaching the default and finishing its landing restores the original weight. Queued, hidden and neutral Bait-zero fields retain normal styling. [Verification and current baseline](BoldStatFeedback/README.md). No Penpot revision or pending import was added.

## Displayed-value stat colours — MFJ-SYNC-006.16

Implemented in Unity on 20 October 2026. Both main and compact creature numbers derive colour from the displayed amount relative to default, rather than their destination or travel direction. Beneficial amounts stay green and detrimental amounts stay red while changing on the same side of default. A final default amount restores normal ink on arrival. Passing through default toward the opposite side switches directly to that side's colour without a neutral flash. Bait's immediate neutral zero, bold styling, pacing, pop/tilt and chain sequence are retained. [Verification and current baseline](DisplayedStatColours/README.md). No Penpot revision or pending import was added.

## MFJ-R042 — Modified-stat source tooltip (Penpot only)

Ready for review on **Concept B — Reusable Assembly (4K)**: select **MFJ-R042 / Stat modifier tooltips / editable review**. Detailed independent parchment frame, editable default/current amounts, signed source rows, source emblems and explanations, plus net change. Lionfish Value and Cod Weight examples cover creature, Technique, Hazard and Environment causes. Gameplay H shows an assembled example; the separate prototype wires hover on Lionfish Value to an independent overlay. [Preview and source contract](Revisions/MFJ-R042/README.md).

**Pending Unity integration; not imported.** Existing creature-effect tooltips and all integrated revisions remain unchanged. Preserve the current Unity-only numeric/chain animation refinements when later adding this tooltip. Runtime attribution must provide the real causes; the Penpot example rows are illustrative content only.

## MFJ-R043 — Compact modifier tooltip alternative (Penpot only)

**Ready for review; not imported.** On Concept B — Reusable Assembly (4K), select **MFJ-R043 / Compact modifier tooltips / 1–3 sources**. Default/current amounts plus portrait/icon, source name and signed amount only. The 680px-wide nine-region parchment frame grows for one to three reasons; overflow keeps three visible and adds a scrollbar and hidden-source indicator. Separate first/last scroll states and a click-navigation prototype document the intended interaction; live wheel/drag scrolling belongs to future Unity integration. [Previews and modular contract](Revisions/MFJ-R043/README.md).

R042 remains pending as a comparison alternative. **Choose one tooltip design before integrating; do not import both.** Current Unity checkpoint remains MFJ-SYNC-006.16; all 37 completed imports and four superseded drafts are preserved. No Unity files changed.

## MFJ-R044 — Tooltip portraits and overflow correction (Penpot only)

**Ready for review; not imported.** Refines the existing R043 boards in place: scrollbar tracks/thumbs removed, three-row overflow and “more” indicators retained. Creature portraits now fit completely inside their icon tiles with padding and preserved aspect ratio. [Preview and handoff](Revisions/MFJ-R044/README.md).

R044 supersedes pending R043; R042 remains available for comparison. Choose R042 or R044 before integration. Two pending alternatives, 37 completed imports, five superseded drafts. MFJ-SYNC-006.16 and Unity files are unchanged.

## MFJ-R045 — Arrow cues, flat icons and new nine-slice engraving (Penpot only)

**Ready for review; not imported.** New native 64px shell-and-knot corners and straight double-rule edges resize independently. Background-free single-ink creature silhouettes; all Techniques use a tied hook and all events a wave scroll. Up arrow appears above reasons, down below, both in the middle; no counts, labels or scrollbar. Three reasons remain visible. [Previews and handoff](Revisions/MFJ-R045/README.md).

R045 supersedes R044; R042 remains a comparison alternative. Two pending alternatives, 37 completed imports, six superseded drafts. Choose R042 or R045 before Unity integration. MFJ-SYNC-006.16 and Unity files are unchanged.

## MFJ-R045 — Integrated as MFJ-SYNC-007

**Imported and verified.** Main and Catch Chain modified numbers now expose the latest compact tooltip with actual resolver provenance, the new 64px sliced frame, flat background-free glyphs, three visible source rows and arrow-only scrolling. Pointer travel, focus, clipping, modal locks and concealed/Bait-zero rules are implemented. Stat and chain animations are preserved. [Import, captures and validation](Sync007/README.md).

R042, R043 and R044 are obsolete proposals retained for history; do not import them. **No pending changes remain: 38 completed imports, seven superseded drafts.** Future selective integration uses [MFJ-SYNC-007](Sync007/UnityCheckpoint.json); MFJ-SYNC-006.16 remains historical evidence.

## MFJ-R046 — Rarity timing bar (Penpot only)

**Ready for review; not implemented or imported into Unity.** Parchment timing instrument with separate Common/Uncommon/Rare zones, optional tiny Legendary/Miss sections, external colour/pattern/emblem labels, independent outlined marker, Stop control and restrained stop feedback. Review states include zone resizing and Legendary/Miss results; a timed-frame prototype freezes the current marker position and leads to existing eligible creature examples. Widths and frame timing are illustrative. [Previews and handoff](Revisions/MFJ-R046/README.md).

This is the user's new visual/mechanic proposal, not an update to the current runtime's encounter rules. One pending design revision; 38 completed integrations and seven superseded drafts remain preserved. Latest Unity baseline remains MFJ-SYNC-007. Gameplay integration needs its own explicit task.

## MFJ-R046 integration — MFJ-SYNC-008

Imported and verified on 25 October 2026. Creature timing waits for Stop before reveal; events bypass the bar. Reusable 4K presentation, configuration, gameplay validation and source hashes are recorded in [Sync008](Sync008/README.md). Earlier design-only statements describe historical status. No pending import remains.

## Unity tuning — MFJ-SYNC-008.1

Uncommon is now 5%, Common 87% and Rare 8%. Unavailable rarity space goes to Common where possible so Uncommon stays 5% at shallow depths. No new Penpot change or pending import. [Validation and current checkpoint](RarityWidthTuning/README.md).

## Unity gameplay update — MFJ-SYNC-008.2

The stopped rarity now sets a ceiling. Every distinct biome/depth-eligible species at or below that rarity has one equal chance, without rarity/species weights or immediate-repeat exclusions. Bar sizes stay unchanged. Result text says “UP TO [rarity]”. Event pacing and fixed promises remain intact. [Validation and current checkpoint](RarityCeilingSelection/README.md). No new Penpot revision or pending import.

## Unity animation update — MFJ-SYNC-008.3

The stopped bar now winds up elastically and exits upward over one second before the encounter is revealed. Motion matches inElastic(1,0.74), resets between attempts, and cancels safely on Surface/restart. Selection and zone widths are unchanged. [Validation and current checkpoint](RarityBarElasticExit/README.md). Unity-only change; no pending Penpot import.

## Unity feedback update — MFJ-SYNC-008.4

Stopping in the highest displayed non-Miss rarity now emits a gold/ivory particle burst at the marker. The effect emits once per occurrence, clears on cancellation/reuse, and preserves selection odds, hold and upward exit. [4K verification and current checkpoint](RarityCelebration/README.md). Unity-only change; no pending Penpot import.

## Unity entrance update — MFJ-SYNC-008.5

The rarity bar now enters from below over one second using the mirrored elastic exit curve. The marker and Stop remain inactive until it settles; cancellation, restart, the upward exit and highest-rarity particles are verified. [4K captures and current checkpoint](RarityBarEntrance/README.md). Unity-only update; no pending Penpot import.

## Dynamic Fishing Tag Bar — MFJ-R047

| MFJ-R047 | Three Dynamic Fishing Tag Bar alternatives, each initial + mid-run, with separate native zones/precision/icons/marker/text. | Superseded by MFJ-R050; not imported. | Historical study retained; selected Lantern Focus continues in R050. |
| MFJ-R048 | Three additional initial-only monochrome tag-bar ideas, with quieter precision cues. R047 retained. | Superseded by MFJ-R050; not imported. | Historical study retained; selected Lantern Focus continues in R050. |
| MFJ-R049 | Three new initial-state instrument forms: Tide Dial, Line Spool, Sounding Chart. Earlier studies retained. | Superseded by MFJ-R050; not imported. | Historical study retained; selected Lantern Focus continues in R050. |
| MFJ-R050 | Selected Lantern Focus: parchment palette, eight centre brackets, no tag highlight and a larger dark cursor. | Superseded by MFJ-R051; not imported. | Earlier selected refinement retained as historical evidence. |
| MFJ-R051 | Anchor-bottom cursor over the bar; eight in-channel tapered precision highlights fading upward. | ready_for_review; Penpot only. | Not imported; [evidence](Revisions/MFJ-R051/README.md). |
