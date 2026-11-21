# Deepwater acoustic effects review

**Rejected creative direction.** The user did not feel this set matched Deepwater and subsequently selected [Charged Cabinet, now installed](../ChargedCabinet/README.md). Technical results below establish the historical implementation's behavior, not sonic fit. The [native-animation comparison](../AudioRevision/Compare.html) preserves the decision reference.

21 November 2026. The active implementation follows [Sound Style](../../Sound%20Style.md). Open [Listen.html](Listen.html) directly to compare full interactions and individual cues. No server, network service or API key is required.

## Implemented

- Replaced every previously active Discover, Landing and Build effect with the coherent acoustic family: **28 cue types / 42 mono PCM takes**, approximately **1.02 MB** of runtime WAV data.
- Original recorded harp, pizzicato strings and card handling replace the plain tonal/ratchet vocabulary. Filtered, locally reconstructed CC0 percussion supplies quiet rhythmic motion and contact support.
- Added deliberate tag-choice and insertion-preview feedback, encounter reveal, distinct Good/Perfect contacts and one ended-expedition closure. Hover, steady track movement and refreshes stay quiet.
- Added bounded phase players, deterministic alternate takes, category buses, cue gains in dB, cooldowns and priority protection for scheduled score impacts/payouts.
- Score takes/gains freeze once per receipt. Impacts use the DSP clock; projectile approaches fit the flight span. A late frame drops overdue sounds. Skip, mute and phase exit cancel scheduled voices. Zero-point Miss no longer sounds twice.
- Reel rewards leave 90 ms after the last stop. Locked reels stay silent, ticks follow actual crossings, and the rolling texture stays behind the contacts.
- Attachment follows the receiving-card impact; reduced motion gives one immediate contact; automatic upgrades have their own phrase. Placement and scoring rules are unchanged.

## Audit decisions

| Previous problem | Delivered response |
| --- | --- |
| Separate collections of synthesized notes and literal ratchets/wood/bells | One close acoustic perspective across all phases; earlier sets are preserved as historical resources. |
| Routine and exceptional results used similar contact shapes | Distinct accuracy, addition, multiplier and three-tier resolution voicings. Harmonic growth does more work than volume. |
| Quarter-tone pitch progression could drift away from the intended scale | Real semitone-based D minor pentatonic steps, capped at one octave. |
| Match cue piled onto the final stop | A cancellable 90 ms result pocket. |
| Miss repeated at score transfer | Exactly one miss cue, with no zero-point payout sound. |
| Contribution launch sound did not match flight length | Bounded pitch fits the recorded reverse-pluck approach to each flight. |
| Frame polling could bunch notes or steal important tails | Audio-clock look-ahead, bounded lateness and explicit voice priority. |
| Silent decisions, shared latch/payout samples for unrelated Build actions | Quiet accepted-choice feedback and separate attachment/upgrade identities. |
| Review mixer assumed every clip was 44.1 kHz | Exports now resample by actual clip rate and use the runtime's effective gains. |

## Verification

The checked-in verification JSON files are the current run's evidence. Automated counts include fixture setup assertions; they are not listening scores.

- `AssetVerification.json`: 335 checks across every cue/take, phase binding, ready PCM import, amplitude/edge/seam and mixer route.
- `ScheduleVerification.json`: 164 fixture/schedule/voice checks, including exact receipt impacts, one Miss, distinct accuracy, flight endpoints, frozen takes, protected future payouts, immediate mute, same-frame unmute/remute and disable cancellation.
- Existing real-flow regression suites passed: 21 slot-audio checks, 142 score-rush checks and 20 Build-polish checks. They cover accepted/stale input, lock/respin, duplicate strike prevention, skip, phase handoff, placement, duplicate upgrades and terminal handling.
- `Runtime*.json` and `Runtime*.wav` are **exports from actual frozen runtime schedules over real installed creature abilities**, not device loopback recordings. `Discover*.wav`, `GrowingRig.wav` and `BuildAndUpgrade.wav` are authored audio storyboards, clearly distinguished in their JSON. Storyboards are useful for comparing complete phrases; the runtime exports are authoritative for scoring timing.
- `TimelineEvidence.json`: offline CLAP/AST inference plus measured onsets and mix levels for the exceptional runtime sequence. Soft approach cues may deliberately lack sharp onsets; impact sync is evaluated separately.
- `MixMeasurements.json`: measured levels for the final review mixes, without normalizing their contrast away.
- `CandidateEvidence.json`: exploratory analysis, including discarded candidates. It retains model uncertainty and is not a claim that all candidates were accepted.

The first source/model pass rejected the brush reconstruction. The reconstructed instrument candidates were replaced by actual CC0 recordings; the procedural paper candidates were replaced by Kenney's recorded card handling. Local candidate ranking identified the three selected card-place recordings as strong matches for a dry card contact. Some model “realness” judgments remain uncertain or synthetic even for short, edited recorded-instrument phrases. That classifier cannot certify production quality and does not establish a sound's provenance. No human headphone/speaker listening pass was performed by the agent.

## Reproduce and tune

1. Sources, their license texts and SHA-256 manifests are under `Assets/Editor/AudioSources/Deepwater/`. The builder uses checked-in files and no network. Earshot analyses use the guarded free-local launcher.
2. Run `.local-tools/earshot/venv/Scripts/python.exe Tools/Audio/build_deepwater_audio.py` to rebuild the WAV family and storyboards offline.
3. Outside Play Mode, run **Fishing Cards → Audio → Install Deepwater Sound Direction**. This deliberately imports the render manifest's library settings and updates the four existing prefabs. Normal art/prefab authoring preserves an existing library's Inspector tuning.
4. Use the native shared SOUND control. For detailed authoring, tune the library's gain/takes and the mixer's Interface/Motion/Score groups. No SFX is generated or downloaded during gameplay.

The effects pass is integrated. Human listening acceptance and any subsequent taste adjustments remain open. Music, ambience and VO are separate creative work and were not silently added to this request for sound effects.
