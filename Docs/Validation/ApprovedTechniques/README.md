# Approved Technique set validation

Validated 2026-10-15. The active set contains eight new Technique designs and the approved 12-card starter deck with a four-card hand. All 15 previous definitions are archived under `Assets/Cards/Backup/Techniques/`, retain their original GUIDs, and are explicitly excluded from normal deck setup, play, and unlocks.

Implemented behavior includes granted Release with reactions after discard, non-Apex Hooked avoidance, single-encounter concealment removal, unresolved Hazard cancellation, in-place Catch/Bait switching, non-stacking action load relief, exact other-hand-slot filtering, and stacked pending Catch bonuses. The universal free Release command is disabled and its UI control is hidden; Descend and Surface occupy the two active action positions. New card artwork was not supplied; definitions use the existing Technique presentation with empty artwork references.

## Executed

- **922 runtime assertions passed** in the .NET adapter harness using the actual rule/controller classes: 38 new approved-Technique assertions and 884 existing Technique/event/creature assertions. Legacy reaction fixtures now grant Release directly rather than depending on the retired free player command; controller input checks assert that the free command stays unavailable.
- **106 content assertions passed**: eight active definitions with the expected IDs, rarities, effect types/targets/amounts; exact starter copy counts and hand size; disconnected old unlock; 15 backup bodies preserved except their explicit backup flag; unchanged original backup metadata/GUIDs; no archived card reference in the active build scene.
- Actual runtime and Editor sources compile against Unity **6000.5.9f1** reference assemblies using the bundled .NET SDK.
- `git diff --check` passes.

See [runtime results](runtime-results.txt), [content results](content-results.txt), and [summary](validation-summary.json).

The new runtime checks cover deferred Cut Loose/Cod/Lionfish timing, release and targeting restrictions, skipping concealed creatures without replacement, Apex/event rejection for Slip Past, one-charge Clear Water alternatives, Hazard cancellation and Lobster reward, in-place role changes and own on-attach activation, preserved modifiers, stacked Prize Hooks including Apex and non-consuming previews, Steady Line anti-stacking and Descend expiry before reveal, event-before-Surface load checks, one/two-card Recast draws with resolving-card exclusion, duplicate/stale slot rejection, exact played-slot identity among identical Recast copies, backup deck filtering, and old session deck migration.

## Unity verification remaining

Adapter results are not Unity PlayMode results. Asset import, actual modal interaction/layout, visual card readability, and Unity serialization/domain-reload round trips have not been executed. The `Fishing Cards/Validation/Approved Techniques` menu runs runtime rules and 33 Editor asset/deck assertions without changing the open scene; the combined event integration menu includes this suite. These Editor checks compile but have not been run here.

The previous isolated Unity launch was blocked by licensing entitlement and Package Manager IPC issues; [prior startup evidence](../EventIntegration/unity-startup-summary.txt) remains applicable context. No new Unity launch is claimed for this change. The existing Technique prefab modal checks remain available through the combined menu as well.
