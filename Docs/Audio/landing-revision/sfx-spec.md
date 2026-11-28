# Landing sound revision: specification

27 November 2026. Charged Cabinet direction ([Sound Style](../../Sound%20Style.md)). The user chose: fill the silent moments, replace the placeholders, add a health-drain sound, and layer a low impact under the final slam.

**Roles:** the main session folded the director, sound-designer, accessibility and integration perspectives together. No separate agents ran.

**Hearing:** Claude cannot hear these sounds. Everything below comes from design intent and signal measurement, not listening.

## Event map

| Moment | Cue | Trigger and alignment | Bus / priority | Start gain |
| --- | --- | --- | --- | --- |
| Creature entrance (all rarities) | Reveal (approved) | Unchanged: face lands, pitch and level by rarity | Interface | as now |
| Rare/Legendary banner stamp | **Stamp** (new) | Stamp moment; peak on the stamp | Score / 120 | −11 dB |
| Strike, Good/Perfect, Miss | approved | Unchanged | — | — |
| Rig ram + floating number | Travel → Addition/Multiplier (approved) | Unchanged; the floating number rides on the impact | — | — |
| Final slam drop + landing | **SlamImpact** (new), layered under Payout Small/Medium/Big (approved) | Scheduled on the DSP clock so its peak meets `PayoutAt` (its air swell is the drop) | Score / 60 | −9 dB |
| Health bar drain (red, then white) | **HealthDrain** (new) | 30 ms after `PayoutAt`; one per damaging strike; none on a Miss | Motion / 200 | −21 dB |
| LANDED yank | Travel (approved), pitch 1.1 | Dip end (0.08 s), as now | — | as now |
| LANDED slam | **Landed** (new), replaces Attach | Peak meets `LandAt` (0.38 s); the swell overlaps the drop | Score / 70 | −8 dB |
| LANDED / ESCAPED stamp | **Stamp** (new) | Stamp moment | Score / 120 | −11 dB |
| ESCAPED sink | Failure (approved) | Unchanged | — | — |
| Haul cash-out ticks | Preview takes (approved card handling), replacing SlotTick | Each card's cash-out, pitch rising 4% per card, at most one every 50 ms | Interface | as now |
| HAUL PAID | PayoutSmall (approved), pitch 0.94, −5 dB | Unchanged | — | — |
| Phase shutter close | **ShutterClose** (new) | Transition start; peak meets the landing at 0.18 s | Motion / 210 | −13 dB |
| Phase shutter open | **ShutterOpen** (new) | 0.26 s (roll back up) | Motion / 210 | −17 dB |
| Build haul count-up | Preview (approved), replacing SlotTick | As the cash-out ticks | Interface | as now |

## Rules

- **Silent cases:** reduced motion keeps every cue, since sound isn't motion. The reduced-motion shutter crossfade plays only ShutterOpen, quieter. Mute cancels every new voice through the existing `SoundChanged` path.
- **Skipping:** skipping a beat stops its pending scheduled voices. A slam that never lands plays no SlamImpact or HealthDrain.
- **One per event:** at most one HealthDrain per strike, and one Stamp per banner. The two stamps (rarity, outcome) never overlap, because the rarity banner clears before scoring.
- **No new mechanics:** every cue follows an existing visual event and decides nothing.

## Accessibility

Every new cue doubles an existing visual (shutter, stamp, slam, bar, LANDED/ESCAPED), so none is gameplay-critical and none is BLOCKING.

**Sensitivity:** the slam adds low-frequency weight to the strongest accent, so its peak should stay at or below the PayoutBig peak. The shutter plays on every phase change, so it must stay quiet, which is why it's on Motion.

## Candidates

Two takes per cue are in `audition/` (X = A or B), made locally with Earshot recipes from its public-domain-model catalogue. There was no hosted service, upload or API key, and every take measured clean.

| Cue | Take A | Take B |
| --- | --- | --- |
| ShutterClose | chain-rattle roll into a heavy door slam | dry slat clatter into a latch thud |
| ShutterOpen | rising chain rattle | soft winding clatter |
| Stamp | rubber-stamp press with a small metal edge | heavier metal-plate clack |
| Landed | cabinet clunk, low boom and a small charged zap | punchy title slam, a switch and a faint synth spark |
| SlamImpact | burst of air into a low punch | swell into a deep thud |
| HealthDrain | sinking filtered sub sweep | airy downward sweep with a faint fizz |

## Integration plan

The plan follows the existing ownership and adds no new audio system.

- **Library:** add six cue identities to `DeepwaterSoundCue` / `DeepwaterSoundLibrary` through the manifest.
- **Bank build:** chosen takes are written by a revision step beside `build_charged_audio.py` as mono 48 kHz PCM, preloaded and decompressed. That step must not alter the approved Charged Cabinet hashes.
- **Owners:**
  - Score timeline: `LandingScoreRush.AddSound` (SlamImpact, HealthDrain, frozen and DSP-scheduled like every score cue).
  - Outcome beats: `LandingOutcome` (Landed, Stamp, cash-out ticks).
  - Rarity banner: `LandingRarityBanner` / `LandingCreatureReveal` (Stamp).
  - Shutter: `TacklePhaseTransition` (ShutterClose/Open, through a sound player bound by its setup).
  - Build count-up: `RigBuildPresentation.HaulTick` (Preview).
- **Checks:** extend the existing audio and live checks so each cue fires once at its event and is silent on Miss, skip and mute.

## Selection and installation (28 November 2026)

**Picks:** the user chose ShutterClose **B**, ShutterOpen **B**, Stamp **B**, Landed **A**, SlamImpact **A** and HealthDrain **A** after the in-context audition. Phrase 2 used Stamp A, but the user picked Stamp B in phrases 3 and 5.

**Build:**
- `Tools/Audio/build_landing_revision_audio.py` writes the takes into `Assets/Audio/Deepwater/` as mono 48 kHz PCM16 with a 0.639 peak. Gains absorb the peak change, so in-game levels equal the audited phrases.
- It merges the rows, with each take's measured `impactAt`, into the manifest and records `LandingRevisionApproval.json` (source hashes).
- `build_charged_audio.py` reapplies it after any rebuild. The approved Charged Cabinet takes are unchanged.

**Runtime:**
- `DeepwaterSoundCue` has six cues appended (34 identities).
- `Definition.ImpactAt` and `DeepwaterSoundPlayer.PlayContact` schedule a take so its contact meets the visual.
- Wiring:
  - `LandingScoreRush`: SlamImpact contact on `PayoutAt`; HealthDrain at `PayoutAt + 0.03`.
  - `LandingOutcome`: Landed contact on `LandAt`; Stamp at the outcome stamp; Preview cash-out ticks.
  - `LandingCreatureReveal`: Stamp for the Rare/Legendary banner.
  - `TacklePhaseTransition`: ShutterClose contact on the landing; ShutterOpen at the roll-up. Reduced motion plays only a quieter roll-up, and an interruption stops both.
  - `RigBuildPresentation.HaulTick`: Preview.
- **Authoring:** `DeepwaterAudioSetup` imports `impactAt` and gives the shutter prefab a 2-voice pool, retained by `TacklePhaseTransitionSetup`.

**Checks:**

| Check | Assertions | What it covers |
| --- | --- | --- |
| `DeepwaterAudioChecks.ValidateAssets` | 425 | All 34 cues: mono 48 kHz preloaded PCM, no clipping, clean edges, routing; shutter pool and clunk timing; motion cues can't steal score impacts; slam level bounded by the large release |
| `DeepwaterAudioChecks.Begin` | 176 | Slam contact on the payout within 12 ms; exactly one drain after it; none on a Miss; mute, unmute and phase exit cancel future voices |
| Live suites | 34 / 15 / 21 | Outcome, phase transitions and creature reveal |

Runtime renders of the frozen score schedules are in `runtime/`. They are mixes of the scheduled takes, not device recordings.

**Not yet verified:** human listening in a full game session.

## Round 2: health drain and ram (28 November 2026)

**Feedback:** the user disliked HealthDrain A and heard it overlap the slam; the rig-ram contact (approved Addition/Multiplier pulse) "doesn't sound too good".

**Timing change (implemented):** `LandingHealthTrail.Delay` = 0.2 s. The Resistance bar holds for that beat after the slam's contact, then drains; the HealthDrain cue starts with it (`PayoutAt + Delay`). The slam's audible body ends about 0.15 s after contact, so the two no longer overlap. Control still waits for the whole drain, so each damaging strike's feedback is 0.2 s longer.

**Candidates** (`round2/`, level-matched to the cue they would replace; listening page `round2/index.html`):

| Option | Health drain | Ram (+ / ×) |
| --- | --- | --- |
| 1 | gauge needle notching down: dry dial clicks, slowing, falling in pitch | card body-check: card slap with a low punch; × adds a crack and boom |
| 2 | electric charge sagging: humming whine sliding down an octave | cabinet knock: hollow wooden knock; × is a wooden slam with a small spark |
| 3 | pressure leaking out: soft hiss falling in tone | charged punch: tight punch with a bright zap tail; × deeper with a crackle |

Phrases are rendered by `Tools/Audio/render_landing_round2.py`. The ram phrases keep the approved Travel whoosh and leave out the drain.

**Checks after the timing change:** audio assets 425; audio schedule 176 (asserting the drain starts with the bar, at least 0.15 s after the slam's contact); damage feedback timing 12 and live 16 (new: the bar holds on the slam); damage numbers timing 11 and live 34.

**Selection (28 November 2026):**
- **Ram:** the user picked option 1, the card body-check. It replaces the approved electric-pulse Addition and Multiplier takes, with one take each (`RamAddition_1`, `RamMultiplier_1`). The 0.28 pitch growth per step is kept.
- **First hit:** at the user's request, the Strike card's first ram (Good/Perfect, before the rig contributes) uses the same family:
  - Good is the + slap (`RamGood_1`).
  - Perfect is the slap plus a small bright electrical accent (`RamPerfect_1`). It was not auditioned on its own.
- **Levels:** each new contact matches the level of the approved take it replaces.
- **Timing:** all four are scheduled with `AddContact`, so each take's measured contact (`impactAt`, 31–46 ms in) meets the visible impact at its playback pitch. The slam uses the same path.
- **Health drain:** switched off until it is revised. HealthDrain A stays in the bank but nothing schedules it. The bar's 0.2 s hold stays.
- **Bank:** the approved Charged Cabinet files (`Addition_1..3`, `Multiplier_1..3`, `Good_1`, `Perfect_1`) remain on disk, but the manifest no longer references them.

**Checks:** audio assets 409 (fewer because the ram cues now have one take each; new: every ram cue has a measured contact time); audio schedule 176 (now: each ram contact lands within 2 ms of its visible impact, and no drain sound is scheduled).
