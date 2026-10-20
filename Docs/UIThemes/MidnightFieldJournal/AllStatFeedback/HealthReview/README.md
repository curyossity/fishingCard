# Stat feedback code health review

Reviewed 20 October 2026 following expansion from chain-only to all effect sources. No runtime, prefab or gameplay edits were needed. MFJ-SYNC-006.11 remains the integration baseline; no new Penpot import is pending.

Findings:

- One runtime animator class (`CreatureCardStatAnimator`) and one shared counter implementation serve both Weight and Value. There are no source-specific chain/Technique/event animation branches or competing coroutines.
- `FishingRunController.RefreshViews` builds one resolved encounter preview and submits feedback once after role selection. The obsolete `includeChainEffects` flag and second chain-free preview are absent. Authored baselines include all effect sources without reimplementing their rules.
- `CatchChainRuntime` owns preview resolution. Its two resolver passes bracket pending catch effects; they are recalculation stages on detached state, not two animation paths. Previewed Prize Hook and Upwelling do not consume pending grants. Tests verify final amounts without double application.
- The prefab has exactly one CreatureCardView and one animator; the serialized animator reference and both numeric text references match. Editor binding reuses the existing component.
- The view owns masking and final snapshot fields; the animator owns pacing, colour and transforms. Re-evaluating Value masking after controller role selection is intentional because a role change can reveal or conceal that field.
- Repeated refreshes retain progress. Occurrence changes, hidden fields, outgoing snapshots, incoming transitions and disable/reset have explicit handling. Bait zero bypasses counting; returning to Catch counts only the effect delta from printed Value.

Fresh verification: all 45 Play Mode assertions passed ([report](PlayModeVerification.json)); 202 runtime assertions passed (Coastal Support 62, Bass/Urchin/Octopus 101, Approved Techniques 39). Prefab binding checks passed, Unity reported no errors, Play Mode was stopped, and the scene remained clean. All 356 files in the existing integration checkpoint matched their recorded hashes before testing. This review adds documentation and test evidence only.
