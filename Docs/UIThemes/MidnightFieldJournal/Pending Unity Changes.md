# Penpot changes awaiting Unity integration

**Current: 0 pending; 39 imported and verified; 7 superseded drafts.** R046 is integrated. [Pending index](PendingUnityChanges.json) · [Full ledger](DesignChangeLedger.json).

Latest baseline: [MFJ-SYNC-008](Sync008/UnityCheckpoint.json). Next revision: **MFJ-R047**.

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
