# EVT-08 integrated event foundation validation

Date: 2026-10-15. EVT-01 through EVT-08 are implemented. Available runtime and reference-compilation checks pass. Real Unity UI, authoring, serialization, and rendering verification remains unexecuted because isolated startup failed; those checks are explicitly listed below.

## Executed evidence

- Actual runtime sources passed **818 assertions** using minimal Unity API adapters: 44 new integrated sequences, 20 input availability checks, 266 event lifecycle/reveal/Technique/dispatch/persistence/action checks, and 488 existing coastal creature/Cleaner Shrimp regressions.
- All runtime sources compile against Unity 6000.5.9f1 references, including the actual CardView, CreatureCardView, FishingRunView, and TechniqueHandView. Editor sources, both collectors, all new suites, and capture/authoring code also compile against the corresponding Editor references.
- End-to-end sequences play two legal Techniques before either Descend or Surface for Hazard, Environment, Opportunity, and generic Encounter. Consequences resolve once after the plays; Descend advances once and reveals an independent event, while Surface records the combined value before live modifier cleanup. Authored event chains retain follow-up order and clear at run ending.
- Earlier suites cover cancellation/replacement/no-target distinctions, Mussel protection/Weathered Shell rewards, normal/chain/replacement reveal guards, full-hand creature-choice suspension, exact Technique discard/recovery/refill, fault locks, mode-independent pending/modifier lifetimes, named-boundary expiry, Quick Clean, and immediate cleanup with separate presentation snapshots.
- Authored event inventory remains Feeding Frenzy, Ghost Net, Murky Current, Rough Current, and Snagged Kelp. Their assets, GUIDs, and legacy effects were not rewritten. Whitespace/diff hygiene passed.

The adapter executable substitutes Unity APIs and view shells; its passed assertions establish runtime rules and coordination, not actual Unity import, GUI, animation, or serialization behavior. Reference compilation verifies real view/Editor source contracts but does not execute them.

## Unity attempt and remaining checks

An isolated editor startup probe was attempted on the current client date. Licensing reported zero matching entitlements and missing com.unity.editor.headless. Package Manager failed to connect to its IPC stream and startup terminated with return-code-1 text. See [sanitized startup evidence](unity-startup-summary.txt). No project validation method ran during that probe.

In a licensed Unity editor, run **Fishing Cards > Validation > All Event Integration Checks**. The durable command runs the event suites and refactored coastal regression suites, then production-prefab and authoring checks, and writes unity-result.json or unity-failure.txt under this directory. Its combined runtime count excludes the separately invoked Cleaner Shrimp asset/modal suite; the adapter total above includes its 40 runtime checks. Run **Fishing Cards > Validation > Cleaner Shrimp** separately for those asset/modal checks.

The Editor checks instantiate the production prefab, verify all event categories suppress stats, rarity hooks, tags, and Catch/Bait input, and check independent buttons for open/empty/locked states. Temporary fixture assets exercise serialized description/effect/lifetime editing, Undo/Redo, disk reload, and exact pool membership. Production assets are preserved. Captures render the existing authored events at **1920×1080**, **1366×768**, and **1280×800** in an isolated preview scene and reject title/type/description overflow. Inspect the resulting PNGs for readability and supplied-art layering; an overflow check does not prove visual quality.

Still unverified: these real Unity checks, interactive Events Collector GUI, prefab import, effect-state serialization round trips, modal interaction, animated Descend/Surface transitions, and screenshot visual inspection. Supplied checks and compilation are not reported as their execution.

## Content boundary

No production event mechanics or lifetime clocks were inferred. Each new supplied event requires its approved registered handler and explicit authored lifetime/boundary rules. Unknown legacy effect types reject confirmation before claiming the occurrence, preserving the legal pre-action Technique window with a diagnostic. Effectless events need no handler. The foundation is ready for individual event-card increments; legacy content is not silently assumed compatible.
