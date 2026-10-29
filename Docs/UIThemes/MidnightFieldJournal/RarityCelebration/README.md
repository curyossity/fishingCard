# Highest-rarity particles — MFJ-SYNC-008.4

A stop on the highest creature rarity currently displayed on the bar emits one gold/ivory glint-particle burst at the stopped marker. The check excludes Miss and respects depth-dependent availability and optional Legendary: Uncommon qualifies when it is the highest displayed rarity, while Rare does not qualify when Legendary is displayed. A single-rarity bar also qualifies. Lower stops and Miss do not emit this celebration.

The reusable native UI emitter has 42 pooled glints, fine trails and geometric halos. It uses private visual randomness, unscaled time and no imported textures or packages. It lives outside the track mask, does not intercept input, follows the existing upward exit, expires automatically and clears on cancellation/reuse. A stopped occurrence can emit only once, including during repeated view refreshes. Zone widths, uniform ceiling selection, reveal hold and exit duration are unchanged.

Verified: 14 particle eligibility/lifecycle assertions, 12 real Play Mode assertions, all 75 timing assertions and six exit-motion assertions. The production test stops the live narrow highest zone, checks single emission across refresh, verifies hidden creature information, cancels via Surface, restarts and confirms a lower stop does not emit. [Production report](ProductionValidation.json), [4K capture](HighestRarityParticles4K.png), [current checkpoint](UnityCheckpoint.json).

Unity-only feedback update; no Penpot asset import or art regeneration is required. Previous checkpoints remain historical evidence.
