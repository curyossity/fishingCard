# Deepwater — Event Integration Planning

Design baseline: 6 November 2026. Event integration is under review. The [GDD](Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md) governs the selected creature loop; this document does not approve an event roster, event frequency or a new player decision phase.

## Design decisions required

- Whether and where non-creature events occur between Discover, Land and Build.
- Event pacing, eligible pools and interaction with slot-machine selection.
- Technique windows, reveal reactions, cancellation/replacement and confirmation timing.
- Effect targets, durations and removal interactions in an ordered rig.
- Compatibility of concealment with learnable movement, challenge information and post-landing role choice.
- Whether encounter chains, treasures and Apex remain, and their rewards/completion role.
- Treatment of any effect that would grant a creature without its required landing challenge.

Review Reef Snag, Silt Burst, Gentle Upwelling, Loose Knot and Fresh Shell Fragments as existing content identities. Their old effects and depth ranges are not approved Deepwater definitions. Weight/overload effects and Surface/Descend timing cannot define the new sequence. Earlier retired event definitions are not reactivated.

## Technical constraints for future implementation

Keep events distinct from creature Catch/Bait attachments. Event presentation uses name, type, illustration and rules; no creature sale value, role selector or rarity hooks. Any deliberate change to that distinction requires design approval.

Use independent occurrence identity, once-only consequence resolution, explicit effect lifetimes and source attribution. Keep runtime state out of shared definitions. A discarded visual must not accidentally delete required runtime ownership, and stale callbacks must not affect another encounter. Cancellation, replacement and no eligible target must remain distinguishable in the model if used by the approved design.

Finish effect reactions before automatic continuation. Views must not execute effects or choose progression. Exact timing and gameplay rules await design; these technical principles do not create an extra tactical landing system.

## Work boundary and verification

This is documentation planning only. No event assets, handlers, code or artwork change. Implement a complete vertical increment after the required rules are specified; unsupported content should not silently invent behavior.

Historical EVT implementation and checks remain evidence of the recorded code only: [implementation map](Architecture%20Implementation%20Reference.md), [event validation](Validation/EventIntegration/README.md), [coastal validation](Validation/CoastalEvents/README.md). They do not validate Deepwater's redesigned loop.
