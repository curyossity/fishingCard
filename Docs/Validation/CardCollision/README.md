# Machine-card collision

The later [smaller-card and slower-collision revision](Readable/README.md) is active. This page preserves the initial accepted treatment and its earlier clips.

Implemented 24 November 2026 after the requested rollback to the committed baseline. PixelFX's ignored local installation and global skill remain installed; the rejected uncommitted water effects, scripts, exported assets and documentation were removed. This implementation introduces no PixelFX effects or new artwork.

Open [Compare.html](Compare.html) for native 60 fps modest/powerful examples. [Windup.png](Windup.png) and [Impact.png](Impact.png) show the framed machine card before and during contact.

## Motion

The creature card inside the machine pulls left by roughly 19–34 reference pixels with a slight backward tilt. Its 60–85 ms accelerating dash closes the gap and contacts the score card. Both faces share a 26–46 ms contact hold while the receiver moves right, then the source rebounds and the heavier receiver settles. Recoil and short visual-stage shake scale with actual gained points. The rig cards remain stationary.

The full framed face, portrait and tag text move together. The score frame, native numbers, progress and transient score readout also move as one unit. The existing contributor/name substitution remains: each source temporarily occupies the machine card, grouped beats retain every identity and the encounter returns at payout or termination.

The motion reference is the attack rhythm in the developer's [official Slots & Daggers trailer](https://store.steampowered.com/app/3631290/Slots_Daggers/), inspected as consecutive frames. Only the general anticipation/attack/recovery concept is used. Its artwork, particles and gameplay mechanics are not imported. Downloaded reference footage remains under ignored Temp.

## Implementation boundary

`LandingCollisionGraphic` samples the original housing sprite for the static shell and two independent faces. The original whole-image component remains bound but does not duplicate the faces. Its surrounding pixels are reconstructed; dark glass from that same source fills the vacated recesses during movement. The PNG is unchanged. Existing native fields retain serialized identity and reference-pixel layout beneath their own center-pivoted group. `LandingCardCollisionSetup` is retained by normal score-rush regeneration and is safe to reapply.

`LandingCardCollision` samples the existing score owner's unscaled clock. It exclusively owns the two group transforms and stage translation. Because this UI uses Screen Space Overlay, moving the world camera would not shake it: the visual stage supplies the requested camera-view shake while model aiming intervals and the gameplay camera remain unchanged. Shake is smooth and damped, with no per-frame random jitter or global time-scale changes.

The first accuracy contact is now 212.8 ms; the first modifier begins at 340 ms. Grouped beats retain a 280 ms minimum within the 3.25-second modifier budget, and the complete sequence remains below 4.8 seconds. Exact numerical delivery and existing impact sounds share physical contact times. Current scoring arithmetic, effects, attempts, outcome timing and the skippable controller gate are unchanged.

There are no projectiles, particles, flashes, new ribbons or added sound assets. The older projectile/contact components remain for historical prefab compatibility but are cleared during this sequence. All 42 accepted sound-file hashes still match their manifest. The existing small/final payout sounds are retained.

Reduced motion preserves exact numbers and contributor identities with both cards and stage at rest. Ordinary zero-point misses have no collision. Skip, receipt replacement, finish, component disable and phase exit restore owned transforms and the encounter identity.

## Validation

**272 checks pass:** 65 native checks for each combo fixture, 46 prefab/binding checks, 32 score/schedule checks and 64 Landing/model checks. [Verification.json](Verification.json) and the two Level JSON files retain results. The fixtures earn 50.7 points in 2.187 s and 12,199.2 points in 2.949 s from current definitions; these are contrast fixtures, not balance targets.

The native checks exercise backward windup, accelerating dash, exact contact/score/sound alignment, rigid contact hold, receiver mass, stage shake, stationary rig cards, absent particle geometry, restored transforms, reduced motion, skip during windup/contact, phase exit, Good and Miss behavior. Reapplying the authoring preserves group positions and bindings. Native motion frames were reviewed for readable digits and whole-card movement.

`Tools/Rendering/CardCollisionAudit.cs.txt` is a reproducible Unity MCP method body. It uses disposable rigs and real model receipts, blocks input, mutes cloned voices and issues no player-controller or save commands. `previewLevel` selects a fixture; `exportFrames` controls captures. Frames export at 60 fps under ignored `Temp/CardCollisionReview`. Video audio is mixed from the runtime's frozen cue list, not recorded device output or a human listening assessment. Large exports can outlast an MCP response timeout; the final JSON and expected frame count verify completion.

Unity reports no errors. The isolated Play session was stopped after verification. The supplied housing PNG and gameplay content definitions are unchanged.
