# MFJ-SYNC-009 — Lantern Focus tag bar

Integrated and verified on 29 October 2026. **MFJ-R051 is complete.** The pending import index now contains zero pending changes, 40 completed revisions and 11 superseded studies.

The journal comparison scene uses the latest light-parchment Lantern Focus instead of its rarity bar. All eight tag spans, glyphs, labels, proportional fading precision areas, overlay anchor and Stop control are separate reusable elements. The independent blank engraved frame and clean paper were exported from Penpot; no assembled mockup was sliced. Source images are 1600 × 400, matching their native size in the 3840 × 2160 composition. Native geometry and text scale independently.

Normal hits choose uniformly among distinct eligible species with the stopped tag. Precision uses relative per-species weights Common 1 / Uncommon 2 / Rare 4 / Legendary 8. If that tag has no eligible species, the draw uses the complete biome/depth-eligible creature pool with the same odds policy. Fixed promises and scheduled Apex creatures retain their identity. Events bypass the bar.

One accepted Stop shrinks the selected raw tag weight by 15%; other weights recover one percentage point toward their 12.5% baseline. Normalization fills the track and enforces a final 5% minimum. Displayed widths may exceed 12.5%, as approved. The minimum cannot accumulate hidden fatigue. Balance survives biome transitions and resets with the run. `Profiles/TagTiming.asset` exposes tuning; the prefab is `Prefabs/TagTiming.prefab` under the candidate theme.

The shared presentation preserves the previous one-second bottom entrance and upward elastic exit, entry input lock, short stopped hold and marker pulse. Precision hits reuse the gold/ivory particle celebration. Width changes animate over 0.3 seconds. At narrow widths glyphs remain visible, and the phase field supplies the tag name rather than squeezing illegible text into a 5% span.

Validation passed in Unity 6000.5.9f1:

- 2,955 deterministic tag assertions: exact normal/precision lottery tickets, fallback, eligibility, deduplication, fatigue/recovery, normalization, delayed reveal, promises, Apex and events.
- 75 legacy rarity assertions, 6 exit-curve assertions, 8 entrance assertions and 14 particle assertions.
- 103 Play Mode assertions, including actual advancing frames, entry/Stop locks, precision particles, resizing, delayed reveal, explicit Catch/Bait choice, next attempt, Surface, stale callbacks, restart, minimum-width geometry and event bypass.
- Rendered inspection at 4K and 1080p. Unity console contained no errors after the final run.
- 429 files in the previous checkpoint remain unchanged. Changes to the other previous files are limited to the timing implementation, candidate canvas/scene bindings and builders; the old abstract view was renamed with its GUID retained. Shared card data, creature/event/Technique artwork, fonts and unrelated templates remain unchanged. The original scene and build settings were independently compared with Git HEAD.

[Moving bar, 4K](TagBar4K.png) · [1080p](TagBar1080.png) · [Precision feedback](TagBarPrecision4K.png) · [Minimum-width state](TagBarFatigue4K.png) · [Event bypass](EventBypass4K.png).

[Source manifest](SourceManifest.json), [verification](Verification.json), [live report](PlayModeVerification.json), [current checkpoint](UnityCheckpoint.json).

The candidate canvas/scene builders now bind tag timing during normal regeneration. The old rarity assets remain available as backup. The original scene/build selection is unchanged.
