# Approved coastal event validation

Date: 2026-10-15. Implemented Reef Snag, Silt Burst, Gentle Upwelling, Loose Knot, and Fresh Shell Fragments as five new authored cards, production effect handlers, shared runtime interactions, and trail presentation.

## Executed checks

Actual runtime sources passed **57 new coastal event assertions and 778 rerun regression assertions (835 total)** using minimal Unity API adapters. Runtime and Editor source compilation against Unity 6000.5.9f1 references passed, including actual views, collector, handler setup, tests, and asset-check code.

The rerun suites cover previous event lifecycle/reveal/Technique/dispatch/persistence/confirmed-action behavior and all refactored coastal creature groups. The older separate Cleaner Shrimp adapter's 40 checks were not recreated in this session; this report does not add them to the rerun total. Cleaner Shrimp/Quick Clean interactions covered by the existing event-persistence/controller suites still run.

Focused coverage:
- Snag chooses from current effective weights after Techniques, excludes Armored, pauses on ties, accepts either attachment mode, rejects illegal/declined mandatory targets, stacks, survives later Armored, and cleanses/expires exactly. Surface captures load before cleanup. Additive effective event weight remains visible even when a creature's own effect sets its weight to zero.
- All pending Upwellings award together, before on-attach effects, with one combined floor at zero after ongoing creature interactions. Normal Bait attachment and immediate mode entry qualify. Pending grants survive biome ending; awarded grants survive modes and disappear with their recipient. Preview includes reductions without consuming grants, including stacked reductions interacting with Predator weight penalties.
- Silt follows existing Squid grants, one concealment per eligible reveal; Apex/events do not consume it. Removal, cross-biome continuation, and exact exhaustion work. Mussel cancellation adds no extra charge. Squid Bait skip consumes both skip and assigned concealment. Learned promised-creature identity/stats remain in the trail preview while the card itself is concealed.
- Loose Knot handles legal targets, Iron Grip, decline, and no-target resolution. It can release Sea Anemone despite Technique targeting immunity. Cod recovery, Ray transfer, Eel full-hand draw choices, and Lionfish value loss finish before the confirmed action. Sea Urchin's pending Technique recovery remains unconsumed.
- Trails schedule one depth+3 promise, preview normal identity/stats, preserve intervening draws, reject forced duplicates, yield to valid queued School Call, mark Technique replacement/avoidance, retarget passed destinations, and clear without follow-ups on Surface/biome loss or no remaining eligible slot. Finite and unbounded biome cases reject expired species slots and Apex displacement.

Static asset audit verifies supplied IDs/types/rarities/depths, zero Catch stats, approved effect/trigger mappings, distinct GUIDs, and all 15 coastal tier membership decisions. Fresh Shell Fragments references the existing Spiny Lobster. See asset-audit.json.

## Unity verification boundary

The adapter harness substitutes Unity APIs and view shells; it does not execute Unity rendering, GUI, asset import, Undo/Redo, or serialization round trips. Reference compilation checks the real view/Editor source, not its execution.

In Unity, run **Fishing Cards > Validation > Coastal Events** for 57 runtime assertions plus the authored-asset checks, or **All Event Integration Checks** for combined suites and the existing three-resolution preview captures. Test Snag tie and Loose Knot choice modals, Bait previews, trail text positioning/readability during transitions, and Collector editing. These actual Unity checks remain unverified in this environment; the existing isolated Editor probe failed on headless licensing and Package Manager IPC (see ../EventIntegration/unity-startup-summary.txt).

No event-specific illustrations were supplied, so the new assets retain empty artwork references and use the existing event master. Cuttlefish keeps its current self-hidden Value behavior; its mention in Silt's queue-stacking rule does not silently refactor it. Legacy event assets retain their original data and are not mapped to these new effects.
