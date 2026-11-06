> **Historical implementation/artwork record.** Preserve resources and provenance. Mechanics, statistics, controls and approvals below do not define Deepwater gameplay. Consult [document status](../../README.md) and the current GDD. The 6 November 2026 redesign changes documents only.

**6 November 2026 — DW-LOOP-002 decorative revision ready for review:** the [Discover and Land screens](../DeepwaterLoop/README.md) reuse the established card, coastal-frame and botanical-button artwork. No Unity integration is requested or marked complete. DW-LOOP-001 is retained as archived review history.

**6 November 2026 — Deepwater mockups ready for review, no Unity integration requested:** [DW-LOOP-001](../DeepwaterLoop/README.md) contains Discover/slot-machine and Land/challenge screens on a separate Penpot page. Gameplay examples and controls remain provisional. These are not approved implementation/import tasks, and the older completion counts below do not include them.

# Penpot changes awaiting Unity integration

**Equipment pricing revision integrated in Unity.** Source data, catalog and shop labels use the nine approved per-item prices. No additional Penpot asset changes or imports are pending for pricing. [Verification](../../Validation/EquipmentPassives/PricingRevision/README.md).

**4 November 2026 — no additional Penpot import required.** The active R067 shop now contains nine real equipment designs and the approved passive behavior in Unity. Existing category artwork is reused; no new design assets were generated. All earlier completed revisions remain completed. [Gameplay/content integration evidence](../../Validation/EquipmentPassives/README.md).

**2 November 2026 — MFJ-SYNC-013 final verification complete.** Initial/scrolled Unity captures passed at 4K and 1080p; fixed slots, clipping and a clean scene verified; zero console errors. The earlier MCP verification gap is resolved. [Evidence](Sync013/README.md).

**MFJ-SYNC-013 completed — R067 Tackle Shop integrated.** Preferred concept restored to 3840 × 2160 and used in Penpot/Unity; native fields, catalog scrolling and Rod/Reel/Hook loadout active. R061/R063 alternatives are superseded, not pending imports. **0 pending; 48 completed; 19 superseded.** [Integration and validation](Sync013/README.md).


**MFJ-R067 — preferred concept applied in Penpot; visual preview only.** Replaces rejected R066 background while retaining cards/layout. The exact approved image is 1672 × 941 on a 3840 × 2160 board; this is not native 4K artwork. Final Unity import is blocked on background quality. Unity unchanged. [Record](Revisions/MFJ-R067/README.md).


**MFJ-R066 — original native 4K shop background, current review revision.** Old reused page ornaments are hidden; new vector folio/rigging-line artwork and new independent engraving replace them. No raster upscaling. Equipment cards, layout and scrolling are unchanged. R065 is superseded and preserved. **3 pending review designs (R066 current; R061/R063 older comparisons); 47 completed; 16 superseded.** Unity unchanged. [Preview and source record](Revisions/MFJ-R066/README.md).


**MFJ-R065 — decorated equipment journal, current refinement ready for review.** Preferred R064 layout/proportions retained; new equipment-only card family appears in slots, catalog and right detail card; existing game page ornaments restored. R064 is superseded and preserved. **3 pending review designs (R065 current; R061/R063 older comparisons); 47 completed; 15 superseded.** Do not import old shop alternatives cumulatively. Unity unchanged. [Preview and handoff](Revisions/MFJ-R065/README.md).


**MFJ-R064 — The Open Field Journal, ready for review.** A new light parchment catalog beside an unframed dark equipment study. All top slots and catalog items preserve the game card ratio **1190:1322**. Nine placeholders and linked catalog scroll states retained. **3 pending design alternatives; 47 completed; 14 superseded.** Choose one shop before integration; do not import alternatives cumulatively. Unity is unchanged. [Preview and handoff](Revisions/MFJ-R064/README.md).


**MFJ-R063 — fixed rig, scrollable catalog and right inspection, ready for review.** All three equipment slots start empty; nine repeated item placeholders test the layout. Two linked 4K states demonstrate catalog-only scrolling. Rejected R062 is superseded and preserved; R061 remains an older comparison. **2 pending review alternatives; 47 completed; 14 superseded.** Do not import alternatives cumulatively. Unity is unchanged. [Preview and handoff](Revisions/MFJ-R063/README.md).


**MFJ-R062 — Equipment Card Table alternative, ready for review.** In-game paper/kelp background, collectible equipment cards and horizontal Rod/Reel/Hook rig slots; no shop background image. R061 remains available for comparison. **2 pending alternative designs; 47 completed; 13 superseded.** Choose a concept before integration; these are alternatives, not two cumulative shop imports. [Preview and notes](Revisions/MFJ-R062/README.md).


**MFJ-R061 — three-slot Tackle Shop, ready for review in Penpot.** Separate Rod, Reel and Hook slots; available-item catalog; illustrative unlock/equip/unequip states. **1 pending design; 47 completed; 13 superseded.** Unity stays at Sync012 with its current shop behavior. [Preview and handoff](Revisions/MFJ-R061/README.md).


**MFJ-SYNC-012 — R059 tag bar and R060 Stop are integrated and verified in Unity.** Separate decorative sprites and native dynamic elements; existing timing, selection and elastic animations retained. 4K/1080p captures and 103 live checks passed. **0 pending; 47 completed; 13 superseded.** [Integration](Sync012/README.md). Earlier review-only statements below are historical.


**MFJ-R060 — engraved parchment Stop button, ready for review.** Applied to both R059 bar examples, with separate native Stop icon and label. **2 pending design revisions (R059 + R060); 45 completed; 13 superseded.** Unity remains unchanged. [Design and preview](Revisions/MFJ-R060/README.md).


**MFJ-R059 — new engraved coastal tag-bar variation, ready for review.** One design shown in initial and illustrative mid-run states. Detailed parchment/copper border, botanical engraving and navy footer; native labels, existing tag glyphs, precision areas, anchor marker and Stop remain separate. R051 stays active in Unity. **1 design awaiting review/import; 45 completed; 13 superseded.** [Preview and handoff](Revisions/MFJ-R059/README.md).


**Current: 1 pending selected design; 39 imported and verified; 11 superseded drafts/studies.** R051 is ready_for_review. R050 is superseded. [Pending index](PendingUnityChanges.json) · [Full ledger](DesignChangeLedger.json).

Latest baseline: [MFJ-SYNC-008.5](RarityBarEntrance/UnityCheckpoint.json). Next revision: **MFJ-R052**.

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

Three alternatives with initial and mid-run static states. Penpot-only exploration; the existing Unity rarity bar remains active. [Design and evidence](Revisions/MFJ-R047/README.md). No variation has been selected for implementation.

## A Quieter Cast — MFJ-R048

Three additional initial-state alternatives: Engraved Ruler, Tackle Labels and Lantern Focus. Each uses a shared palette and quiet precision cues. R047 is retained for comparison; neither study is imported. [Design and evidence](Revisions/MFJ-R048/README.md).

## Three Ways to Read the Sea — MFJ-R049

Three initial-only instrument concepts: Tide Dial, Line Spool and Sounding Chart. R047/R048 are retained. Motion/viewport changes are visual proposals only; Unity is unchanged. [Design and evidence](Revisions/MFJ-R049/README.md).

## Selected Lantern Focus — MFJ-R050

Light parchment, eight permanent centre marks, no active-tag wash and a larger dark cursor. This is the only pending tag-bar design. Earlier concepts remain as historical Penpot boards. [Design and verification](Revisions/MFJ-R050/README.md). Unity remains unchanged.

## Anchor cursor and rising precision — MFJ-R051

The selected parchment bar now uses an overlay cursor ending in an anchor, with all eight precision highlights inside the channel, rising and fading from its bottom edge. R050 is historical; only R051 remains pending. [Design and verification](Revisions/MFJ-R051/README.md). Unity is unchanged.
