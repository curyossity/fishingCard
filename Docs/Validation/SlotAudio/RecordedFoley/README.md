# Recorded slot-machine sound replacement

Historical rejected audition. Superseded by the [Deepwater acoustic effects family](../../AudioDirection/README.md); these recordings are not the active slot palette.

21 November 2026. Replaces all eight Discover cues with edits of recorded mechanisms and physical contacts. Gameplay, reel motion, sound preference ownership and six-voice routing remain unchanged. The earlier synthesized WAVs remain available; Build's existing latch reference is preserved.

## Source and direction

- [Ratchet Wrench Fast Multiple — Rudmer_Rotteveel](https://freesound.org/people/Rudmer_Rotteveel/sounds/591529/): CC0 recording, downloaded as the site's high-quality MP3 preview. Three short strokes at 0.05, 0.69 and 1.29 seconds form the 0.60-second rolling texture. A shortened stroke also supports the launch.
- [Ratchet.wav — romulofs](https://freesound.org/people/romulofs/sounds/127533/): CC0 recording, downloaded as the site's high-quality MP3 preview. An isolated contact near 8.05 seconds supplies the quiet symbol tick and lock contact.
- [Impact Sounds — Kenney](https://kenney.nl/assets/impact-sounds): CC0 original OGG files. `impactWood_medium_001`, `impactWood_light_000`, `impactMetal_medium_001` and `impactMetal_light_002` supply launch, stop and latch layers. `impactBell_heavy_000` supplies a short pair accent and a two-contact triple accent. The pack's original license notice is retained.

The authoring source recordings live under `Assets/Editor/AudioSources/DiscoverFoley/` and are excluded from runtime builds. Derived runtime files live under `Assets/UIThemes/TackleApparatus/Audio/Discover/Foley/`. Edits downmix to mono, apply modest pitch changes, remove low rumble/DC, soften contact highs and fade boundaries. PCM peaks are normalized to 0.62, leaving headroom before the quieter runtime decibel gains. These edits use no synthesized noise or oscillators.

The recorded ratchet has sharp natural transients and a quieter body, so its runtime gain is 9 dB higher than the old loop; the additional symbol ticks are 3 dB quieter. Pair/triple accents are quieter than the earlier set. Existing actual-reel pitch, stop timing, locked-reel silence and cancellation behavior are retained.

## Integration and review

`TackleSlotAudioSetup.Configure` selects the recorded set for existing and newly authored Discover prefabs. **Fishing Cards → Slot Machine → Install Sound** retains existing recorded WAV edits and repairs the bindings; **Rebuild Recorded Sound** recreates the eight edits from source. It does not regenerate the machine artwork.

[Listen to the current individual cues](../Listen.html). The previous set is also selectable for comparison. The game supplies the final mix and synchronization; subjective headphone/speaker judgment remains player review, not an automated quality claim.

## Verification

- `TackleSlotAudioChecks.ValidateAssets()`: 36 checks passed for bindings, native routing, cue signal content and click-free boundaries.
- `TackleSlotAudioChecks.Begin()`: 21 real-flow audio checks passed, including stop counts, refresh silence, accepted/rejected locks, locked-reel silence, mute/unmute and phase cleanup.
- Reinstalling sound retained all eight recorded references. Prefab changes are limited to the eight clip GUIDs.
- All eight WAVs are mono 44.1-kHz/16-bit PCM, have zero first/last samples and a measured sample peak of −4.15 dBFS before runtime gains. Source/output SHA-256 hashes and RMS levels are in [verification data](Verification.json).
- All eight legacy WAVs are byte-identical to the previous committed files, preserving existing Build cue references.
- Unity Console: zero errors. Project integrity: 1,929 files, zero errors or warnings.
