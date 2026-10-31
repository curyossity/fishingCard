# Unlimited Technique hand — MFJ-SYNC-011.2

User clarification, 31 October 2026: remove the Technique hand limit entirely. Four is the normal opening draw. Tideglass Reel adds two opening Techniques, giving six; all loadouts can grow beyond those opening counts through explicit effects. The equipment purchase/equip policy and Barbed Hook's new-Catch-only charges are confirmed unchanged.

The deck model no longer stores a capacity, accepts capacity arguments or rejects recovery because of hand size. Explicit draws stop only at their authored count or when both deck and discard are empty. A resolving Technique remains unavailable for draw/recovery until discarded. Cod returns the selected discarded occurrence directly; Sea Urchin returns only its exact played occurrence after earlier reactions. Physical cards cannot be duplicated by multiple recoveries.

Creature draw requests now resolve automatically after prior reactions. Obsolete discard-for-room state, callbacks and modal branches were removed. Required discards, optional redraws, Recast and exchanges keep their rules. Playing a Technique or Descend still never automatically refills the hand. The candidate fan already supports arbitrary counts; the original hand UI retains pagination.

Validation in Unity 6000.5.9f1:

- Eleven shared regression suites passed: Technique ownership/targets, creature releases, Octopus draws, Cod/Urchin recovery ordering, Sardine/Eel draws, event consequences, confirmed-action suspension and authored Techniques. Former capacity cases were updated to assert uncapped growth; event pause tests use a genuinely required discard rather than a removed room choice.
- Equipment rules passed 31 assertions, preserving one equipped item, permanent purchases and first-three new Catch grants.
- Live shop/expedition checks passed 31 assertions, including six starting cards with the Reel, then explicit draws to all 12 physical starter cards. All 12 active/inspectable slots were present. Actual [4K hand capture](UnlimitedTwelveCardHand4K.png) visually inspected.
- Player save fixtures used isolated temporary keys and were removed after leaving Play Mode. Final scene clean, outside Play Mode, no console errors.

The successful checks above completed before the final refresh. During that refresh Unity MCP stopped answering pings; the fallback window check also timed out. A final post-refresh console/scene recheck could not be completed. No editor restart or changes to user save data were attempted.

[Regression report](RegressionVerification.json), [equipment report](RulesVerification.json), [live report](PlayVerification.json), [verification/checkpoint](Verification.json).

This is a Unity-only rules increment. No Penpot art was reimported. Existing integration statuses remain 45 completed, 13 superseded, zero pending imports. [Previous equipment integration](../TackleShop/README.md) records the earlier capped implementation; this amendment supersedes its cap statements.
