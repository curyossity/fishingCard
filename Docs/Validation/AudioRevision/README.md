# Deepwater sound revision: cabinet impact and arcade payoff

**Selected and installed:** the user chose A — Charged Cabinet. [Current integration and parity evidence](../ChargedCabinet/README.md) supersedes the pre-integration status below. The A/B files remain the decision reference.

21 November 2026. [Open the native-animation comparison](Compare.html). The installed acoustic family was rejected by the user for failing to match the game. This revision changes the sound character rather than presenting another technical test pass as evidence of creative fit.

## What was wrong with the prior direction

These are design diagnoses informed by the user's rejection, the actual game image, source construction and model-assisted classifications; the agent has not performed human listening:

- Harp and pizzicato became the identity of almost every positive event. Their tidy little phrases suggested an instrument or a gentle notification system, while the image shows a worn, substantial apparatus and the score presentation is meant to feel forceful.
- A shared musical scale gave consistency, but it did not establish a convincing material or powerful result. Growing scores often changed pitch without changing the character enough.
- Sourcing recorded instruments and passing loudness, routing and cancellation tests did not demonstrate game fit. Source legitimacy and subjective appeal are separate.
- Literal industrial ratchets/wood/bells, from the earlier rejected attempt, are also not an appropriate fallback. The revised goal is stylized game feedback with physical contact and power, rather than a recording of a workshop.

## Two concrete directions

| Direction | Intended character | What changes |
| --- | --- | --- |
| **A — Charged cabinet**, recommended starting point | A substantial antique apparatus that releases a compact electrical/magical pulse when creatures drive the score | Short button/flipper contacts, damped pressure and electrical body, quiet approaches, stronger multiplier attacks, a dense score-spark release instead of a harp arpeggio. The rise in register is restrained. |
| **B — Arcade pulse** | A brighter, more playful apparatus, with a pinball-like sense of contact and reward | Glassy rounded contacts, wider register changes, brighter multiplier voicing and a more buoyant reward. This is a deliberately different option, not a claim that bright is always better. |

The art reference is the actual Tackle Apparatus image: dark wood, aged brass, short physical controls, illuminated readouts and sharp point impacts. This is an interpretation of its feel, not a request to change artwork or invent new mechanics. No global music bed, long success song, voice, literal coin payout or new gameplay cue is introduced.

Each candidate bank contains the full **28 cue identities / 42 takes**, including Discover, Landing, preparation and Build. The comparison concentrates on three complete scoring interactions so a listener can decide whether the core timbre fits before another full-game replacement. The game is still bound to the previous set; candidate rendering does not change prefab bindings.

## Reference principles

- [SomaTone's Peggle audio interview](https://www.gamezebo.com/news/interview-with-somatone-interactive-audio/) describes supporting the game's character with a playful arcade vocabulary. Borrow the combination of tactile event identity and exaggerated reward; no Peggle sample or melody is used. The publisher's full page was inaccessible during this pass; the indexed interview excerpt supplied the cited statement.
- [PopCap's GDC session, Peggle Blast: Big Concepts, Small Project](https://www.gdcvault.com/play/1022188/Peggle-Blast-Big-Concepts-Small) documents dynamic effects, procedural sound and close audio-logic integration. It supports a designed event vocabulary rather than treating “recorded” as equivalent to “professional.” The session page was read; no claim of watching its full talk is made.
- [CloverPit developer Matteo Gonano](https://news.xbox.com/en-us/2025/11/20/clover-pit-xbox-partner-preview/) describes deliberate work on spin/click/jackpot feel and anticipation. Keep the contact and payoff attached to the visible event; do not borrow its horror atmosphere or recordings.
- [Slots & Daggers' creator description](https://store.steampowered.com/app/3631290/Slots__Daggers/?l=english) names drum machines and rewarding arcade sounds as influences. Here rhythm and transient weight are the useful interpretation, not a new hip-hop soundtrack.

These are source-informed design proposals. Neither a comparison to those games nor a local model score establishes that the candidates now fit Deepwater.

## Comparison and verification

- The preview uses existing native Unity scoring frames at 24 fps, over the frozen runtime operation times. It does not redraw the game or simulate a different scoring mechanic. The ordinary archive shows a Perfect 20 strike, so its audio uses Perfect rather than the separate Good-10 runtime export.
- The strong and exceptional rejected-set references come from the actual prior runtime audio exports. Candidate sounds use the same event times, with the intended direction-specific pitch treatment. They are audition mixes, not device loopback recordings.
- All three exceptional clips target **−24.8 LUFS**, measured with FFmpeg's BS.1770 loudness implementation and dual-mono compensation for mono effects delivered to stereo. `ComparisonLevels.json` records both original and comparison levels. A single static trim per direction preserves that direction's ordinary-to-exceptional contrast; no compressor or loudness pumping is added to win the comparison.
- Cue files are 48 kHz mono PCM with bounded peaks, clean one-shot edges and matching loop endpoints. The original DSP scheduling/mute/skip fixes are retained in the game. They are not being redesigned just because the palette was rejected.
- `ModelEvidence.json` contains local CLAP/AST source-character/timing evidence, with uncertainty retained. This is not a human listening pass or a production-quality score.

## Source and implementation boundary

New raw source material is under `Assets/Editor/AudioSources/DeepwaterRevision/`: public-domain-derived and procedural material rendered by the guarded local Earshot server. IDs and hashes are recorded there. Recorded CC0 card contacts from Kenney's Casino Audio are reused from the prior source folder. No source from another commercial game is included; no paid service, account, API key, upload or hosted critique was used.

`Tools/Audio/render_sound_revision.py` reproduces both banks and the level-matched comparison entirely offline. This audition tool writes only to the review folder and has no game-install branch. Integration must carry the selected comparison's static mix calibration and direction-specific pitch treatment into runtime before claiming that the preview and the installed sound are identical. Simply changing WAV bindings would not accomplish that.

The pending creative choice is which direction, if either, matches the game. The existing sound architecture is ready to accept the revised set once that character is established.
