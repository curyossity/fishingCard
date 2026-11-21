# Discover slot-machine audio

Superseded by the selected [Charged Cabinet direction](../ChargedCabinet/README.md) and [active sound style](../../Sound%20Style.md). The original synthesized set, [recorded ratchet/wood/metal/bell edits](RecordedFoley/README.md) and [rejected acoustic set](../AudioDirection/README.md) are historical audition records.

Implemented 20 November 2026 using audio-design, game-feel and Unity C# guidance. Build/Upgrade improvements were reviewed as proposals only; this change installs no phase-three behavior.

## Delivered

- A wooden launch clack and quiet mechanical rolling texture accompany each initial spin or optional respin.
- Reel ticks follow actual symbol-boundary crossings with throttling; a slow frame cannot emit a burst of missed ticks.
- Each moving reel has a separate pitched stop sound at visual settlement. A locked reel remains silent during the respin.
- Accepted lock/unlock changes use distinct latch cues; repeated refreshes and rejected commands stay silent.
- Pair and triple results have separate modest brass cues after all reels settle. Three different tags end with the mechanical stops. These sounds do not imply creature acquisition or payout.
- A native SOUND toggle on Discover shares the existing saved sound preference with Landing. Muting stops every owned voice immediately; unmuting resumes a spin still in progress.
- Phase exit stops the rolling loop and every one-shot tail. Six pooled 2D voices plus a dedicated rolling voice route through the existing feedback mixer. Eight original PCM cues are authored under `Assets/UIThemes/TackleApparatus/Audio/Discover/`.

`TackleSlotAudio` owns audio only. `TackleDiscoverPresentation` invokes it at new spin revisions, after actual reel animation advances, and after accepted lock-state changes. `GameAudioPreferences` preserves the existing saved preference key and provides shared ownership for Discover/Landing. `TackleSlotAudioSetup.Install()` updates the existing Discover prefab; reel/housing regeneration retains the bindings through `TackleReelAnimationSetup.Configure`.

## Verification

`TackleSlotAudioChecks.ValidateAssets()` passed 36 binding/routing/PCM assertions. `TackleSlotAudioChecks.Begin()` passed 21 live assertions with progression persistence disabled: exactly-once spin starts, stop counts, result preservation, refresh silence, accepted/rejected locks, locked-reel silence, respin limits, muting, unmuting and sound-preference handoff to Landing. The previous player sound preference is restored after the fixture. See [live result](LiveVerification.json).

The existing `SlotReelAnimationChecks.Begin("Docs/Validation/SlotAudio/ReelRegression/")` checks actual reel motion, visible symbols, locked-reel geometry, the input gate and explicit acceptance in a separate fresh Play session.

Final result: all 40 existing live reel checks passed (97 assertions across the three checks). Unity Console reported no errors. Project integrity checked 1,880 Unity files with zero errors/warnings. See [verification summary](Verification.json).

[Listen to the cues](Listen.html). This page previews individual samples; the game applies decibel gains, per-reel pitch and synchronized playback. Speaker/headphone listening remains subjective tuning rather than an automated quality claim.
