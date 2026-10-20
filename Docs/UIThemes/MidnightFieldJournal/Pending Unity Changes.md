# Penpot changes awaiting Unity integration

**Current:0 pending revisions;37 imported and verified;4 superseded drafts.** [Machine-readable index](PendingUnityChanges.json) · [Full ledger](DesignChangeLedger.json).

Latest baseline:[MFJ-SYNC-006](Sync006/UnityCheckpoint.json). [Integration and validation](Sync006/README.md). Next design revision:**MFJ-R042**.

R041’s independent Technique backing sheet is imported and verified. The boat now uses the normal upward encounter transition. The Unity-only depth focus, lowered hand and newest-catch scroll refinements are retained; prior checkpoints remain historical. R025/R026/R028/R029 remain superseded drafts, excluded from future imports.

Future imports must compare against Sync006 and select only new pending entries, preserving completed revisions and Unity-only refinements. No screenshot assembly is used as a game UI asset.

## Explicit creature role choice — MFJ-SYNC-006.1

No default Catch/Bait choice; Descend requires explicit selection for each new creature/Apex occurrence. Runtime-owned choice survives ordinary refreshes and resets on replacement, including the same species. Role buttons remain usable while Descend is disabled; Surface, boat and event windows retain their existing availability. [Verification and current baseline](../../Validation/ExplicitAttachmentChoice/README.md). Unity-only gameplay/UI change, no pending Penpot import.

## Smooth catch-chain follow — MFJ-SYNC-006.2

Implemented in Unity: newly attached creatures scroll into view with a 0.32-second eased movement instead of snapping. Wheel/drag inspection interrupts following; returning to the bottom resumes it. Repeated refreshes preserve progress and rapid attachments retarget smoothly. [Verification and current baseline](SmoothCatchChain/README.md). Unity-only presentation change; no new Penpot import or pending design revision.
