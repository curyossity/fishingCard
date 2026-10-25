# MFJ-SYNC-008 — Creature rarity timing

MFJ-R046 is integrated into the isolated `MidnightFieldJournalComparison` scene. The original scene and build selection are preserved.

Descend advances normally, then a creature slot shows the parchment timing instrument. Stop freezes the exact marker position; after 0.65 seconds of rarity feedback, the eligible creature is revealed through the existing reveal/reaction/concealment path. The player must explicitly select Catch or Bait before Descending again. Events reveal directly with their normal Technique window and consequences. Surface can cancel a pending attempt and end the run.

Ordinary category/event odds retain their existing weighted selection. Within a creature slot, rarity comes from the stopped interval and species is sampled using normal encounter weights within that rarity. Unavailable rarities are omitted and the rest renormalized. Existing queued choices, trails, authored chains and scheduled Apex encounters use a fixed-rarity interval and preserve their identity. Technique replacements in an existing reaction window do not start a new timing phase.

`Profiles/RarityTiming.asset` contains initial relative widths 68/24/8, speed 0.6 one-way track traversals per second, and result hold 0.65 seconds. Legendary and Miss default to zero; no upgrade content is added. The runtime snapshots configuration without mutating authored assets. Zones, patterns, legends, marker, Stop and feedback remain separate native Unity UI elements. Only the approved blank 1160×340 Penpot frame is raster artwork, with 64px fixed sliced corners. It is rendered at its native panel size in the 3840×2160 design viewport.

Validation: 46 timing/eligibility/reveal assertions, 22 real comparison-scene Play Mode assertions including Stop raycasting, event bypass, explicit role selection, cancellation/restart and single depth advancement. All eight existing gameplay suites pass (558 assertions). No compilation/runtime errors or production-smoke runtime warnings; older scripts still produce existing obsolete-API compilation warnings. [Validation report](Verification.json), [production report](ProductionValidation.json).

4K Game View captures: [moving](Moving4K.png), [stopped result](Stopped4K.png), [revealed creature](Revealed4K.png), [direct event](EventBypass4K.png). Input is locked during outgoing-card transitions; the timing marker starts once that transition finishes. Stop receives normal EventSystem focus for keyboard/controller Submit as well as pointer input.

[Checkpoint](UnityCheckpoint.json) records the selective integration and source hashes. R046 is verified; no Penpot revisions await import. The original design-only proposal and all earlier checkpoints remain historical evidence.
