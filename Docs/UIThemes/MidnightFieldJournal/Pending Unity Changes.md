# Penpot changes awaiting Unity integration

**Current:0 pending revisions;37 imported and verified;4 superseded drafts.** [Machine-readable index](PendingUnityChanges.json) · [Full ledger](DesignChangeLedger.json).

Latest baseline:[MFJ-SYNC-006](Sync006/UnityCheckpoint.json). [Integration and validation](Sync006/README.md). Next design revision:**MFJ-R042**.

R041’s independent Technique backing sheet is imported and verified. The boat now uses the normal upward encounter transition. The Unity-only depth focus, lowered hand and newest-catch scroll refinements are retained; prior checkpoints remain historical. R025/R026/R028/R029 remain superseded drafts, excluded from future imports.

Future imports must compare against Sync006 and select only new pending entries, preserving completed revisions and Unity-only refinements. No screenshot assembly is used as a game UI asset.

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
