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

Implemented in Unity on20 October2026: both panels start faded; opacity increased from46% to58%. Selected panel returns to full brightness with its unchanged glow; alternative stays faded. [Captures, verification and baseline](RoleInitialFade/README.md). No new Penpot revision or pending import.
