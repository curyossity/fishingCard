# Charged Cabinet — selected and integrated

The subsequent [slot timing correction](../SlotAudioSync/README.md) moves contacts to the visible brake/catch, follows reel velocity and aligns match sounds with the result lamps. It preserves this selected sound palette and all scoring calibration.

21 November 2026. The user selected **A — Charged Cabinet** in the native-animation comparison. The calibrated 28-cue / 42-take family is now installed across Discover, Landing, Build/Upgrade and Harbor tag preparation. [Listen to the installed mix](Listen.html), [review the selected comparison](../AudioRevision/Compare.html), or [read the active sound style](../../Sound%20Style.md).

## Delivered behavior

- Substantial sprung contacts, quiet pressure/friction under reel motion, damped electrical scoring pulses, firmer multiplier impacts and compact score releases replace the rejected harp-led family.
- The chosen audition's **+7.54 dB calibration** is preserved through bounded PCM gain and per-cue faders. No source needs gain above one; the mixer does not add that trim again.
- Addition and multiplier pitch retains **28% of the scheduled pitch excursion**, reaching at most 1.28. The previous additional intensity gain is disabled for this family, matching the selected audition.
- Existing event ownership, reel-boundary ticks, impact timing, priority protection, immediate mute, same-frame unmute/remute, skip and phase cleanup remain in place. Discover contacts now meet the visible brake and match cues meet settlement, as recorded in the timing correction above. No gameplay calculation, randomness, artwork or interaction rule changed.

## Audition agreement

`ChargedApproval.json` under the Editor source folder freezes the selected raw recipe hashes and mix calibration. The production builder verifies those hashes before authoring the active clips. `Calibration.json` and `AuditionParity.json` check all **42 takes**: installed PCM × cue gain matches the selected candidate × audition trim with maximum sample error **0.0000226**, caused by 16-bit quantization.

The actual Unity score exports retain the same cue times and selected pitch treatment. Measured loudness differences from the chosen comparison are **−0.04 LU** for the strong combination and **−0.18 LU** for the exceptional combination. The exceptional runtime export measures **−24.97 LUFS / −6.45 dBTP**. Its tiny difference includes playback resampling and alternate contact takes. Measurements use BS.1770 with dual-mono compensation and no post-export normalization.

The original A/B page remains a frozen decision reference. Its brighter B bank and rejected acoustic clips are preserved for comparison; they are not active production sounds.

## Verification

- `AssetVerification.json`: 363 assertions covering every cue/take, ready PCM import, amplitude/edges/seam, expression bounds, phase binding and category routing.
- `ScheduleVerification.json`: 170 fixture/schedule/voice assertions, including exact impacts, flight endpoints, selected pitch treatment, frozen takes, single Miss, mute, priority and disable cancellation.
- Real-flow regressions: 21 slot-audio checks, 142 score-rush checks and 20 Build-polish checks. These cover accepted/stale input, lock/respin, duplicate strike protection, skip, placement, automatic upgrade, phase handoffs and failure handling.
- `Runtime*.json` / `Runtime*.wav`: real frozen score schedules over installed creature abilities, rendered with the actual samples, pitches and gains. These are exports, not device loopback captures.
- Discover and Build examples are explicitly labeled authored interaction storyboards using the installed calibrated bank. `StoryboardLevels.json` verifies their headroom without changing their authored contrast.

Automated assertion counts include fixtures and are not creative-quality scores. The user's A selection establishes the direction; full-session headphone/speaker review remains a separate human judgment.

## Reproduce

1. Run `.local-tools/earshot/venv/Scripts/python.exe Tools/Audio/build_charged_audio.py` offline. It rebuilds from checked-in Editor-only source material and verifies the selected raw hashes. The former `build_deepwater_audio.py` entry point delegates to this selected builder; `--archive-acoustic` explicitly writes historical material outside the active audio folder.
2. Outside Play mode, run **Fishing Cards → Audio → Install Deepwater Sound Direction**. This imports the authored manifest and updates the four GUID-stable prefabs. Ordinary art regeneration preserves Inspector tuning.
3. Run `DeepwaterAudioChecks.ValidateAssets()` outside Play mode and `DeepwaterAudioChecks.Begin()` in Play mode to recreate active validation and score exports.
4. Run `Tools/Audio/verify_charged_audio.py`, `render_charged_storyboards.py` and `build_audio_review.py` with the local audio venv to recreate parity checks and the current listening page.

All generation, processing and measurement remains free and local. No API key, hosted generation/critique or audio upload is used. Source IDs, credits, licenses and hashes remain with `Assets/Editor/AudioSources/DeepwaterRevision/`; the earlier CC0 card source is reused. These originals are excluded from player content.
