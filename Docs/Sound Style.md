# Deepwater sound style — Charged Cabinet

21 November 2026. The user selected **A — Charged Cabinet** after reviewing the native-animation comparison. This is the active effects direction for Discover → Land → Build. The GDD owns mechanics; sound follows accepted actions, visible contacts and frozen score receipts.

## Identity

**A substantial, worn tackle machine that releases compact bursts of power when the rig works.** Controls acknowledge contact, reel motion carries tension, contributing creatures strike the readout with weight, and a powerful result releases that pressure. Routine inspection remains quiet.

The palette combines short sprung button/flipper contacts, dry card handling, damped electrical pulses, low impact support, filtered approaches and small metallic/glassy sparks. It is deliberately stylized sound design. Recorded or synthesized origin does not determine whether a sound fits the game.

The previous harp/pizzicato identity was rejected for failing to match Deepwater. Earlier plain synthesized tones and literal ratchet/wood/bell edits were also rejected. Do not return to gentle instrument jingles, generic notifications or unprocessed industrial recordings as the default. The rejected set's [historical style record](Validation/AudioDirection/Rejected%20Sound%20Style.md) preserves that attempt.

The scoring phrase is **contact → approach → impact → compact release**. Additions have a concise contact/pulse identity; multipliers add a firmer attack and electrical body; large payouts add a dense, brief spray of score sparks. There is no long success song or literal coin payout. Landing Points remain distinct from haul value.

## Cue vocabulary

Times below are file durations, not gameplay delays. Immediate cues are requested in the accepted action's frame; hardware latency remains device-dependent.

| Event | Character and timing |
| --- | --- |
| Tag select / deselect | 105 / 100 ms damped contact or card acknowledgment for an actual draft change. Three takes; 70 ms retrigger guard. |
| Insertion preview | 100 ms paper/contact when the chosen legal insertion position changes. Three takes; 75 ms guard. Hover and repeated layout stay silent. |
| Spin start | 230 ms sprung contact with low support, once per spin revision; no second generic click. |
| Reel motion | Quiet 900 ms circular pressure/friction texture, with a 40 ms gain attack and a quieter last moving reel. |
| Symbol crossing | 45 ms muted contact at a real boundary. Three takes, global 45 ms cooldown and per-reel throttling; no catch-up burst. |
| Reel stops | 150 ms cabinet contact at complete visual settlement, varied slightly by reel. A locked reel stays silent during respin. |
| Lock / unlock | 120 / 110 ms contact for an accepted change. Initial synchronization and stale commands stay silent. |
| Pair / triple | 230 / 440 ms acknowledgment or fuller release, 90 ms after the final reel settles. No match cue for three different symbols. |
| Encounter reveal | 200 ms card/air transition, once per new Landing occurrence. |
| Accepted strike | 80 ms firm contact at time zero. Rejected input and Finish add no strike cue. |
| Good / Perfect | 160 / 210 ms weight/contact versus a clearer electrical accent at the base-points impact, normally 76 ms after strike. |
| Miss | One 140 ms damped low contact at time zero. Zero points produce no payout cue; a scoring/protected Miss retains actual rig-operation cues. |
| Creature launch | Quiet 120 ms approach, with bounded playback pitch fitted to the actual projectile flight. |
| Addition / multiplier | 180 / 250 ms pulse/contact versus a more substantial electrical impact at the frozen step's exact ImpactAt. Three takes each. |
| Points transferred | 200 / 320 / 570 ms small, medium or large release at PayoutAt. Tier follows real amplification; large results add texture and density. |
| Expedition failure | One 340 ms damped closure after scoring feedback, with no taunt, repeated alarm or implication that accumulated haul was lost. |
| Placement / attachment | 220 ms paper/air at receipt start, then 210 ms contact at the receiving-card impact, currently 320 ms into travel. Reduced motion uses one immediate attachment contact without flight. |
| Duplicate upgrade | 450 ms contact and charged accent at the automatic upgrade's first pulse, distinct from scoring payout and attachment. |

Tooltips, hover, steady Landing movement, invalid commands, focus synchronization and refreshes stay silent. This effects delivery adds no music, ambience or voice system.

## Mix and pitch

The selected comparison applied **+7.54 dB static calibration** to the raw candidate. The production builder preserves that effective level by distributing gain between available PCM headroom and cue faders. Samples remain at or below 0.639; no voice requires gain above one. Do not add that calibration again at the mixer.

Addition and multiplier pitch is `1 + (scheduledPitch − 1) × 0.28`, with a maximum of 1.28 instead of an octave jump. Other cues retain their authored pitch behavior, including flight fitting. This is presentation, not a scoring rule. The selected set has no additional intensity-based gain boost: voicing, timing, tiered release and modest register growth carry the escalation.

`DeepwaterSoundLibrary` owns takes, category, dB gain, priority, cooldown, pitch growth and optional intensity gain. `DeepwaterSoundPlayer` owns bounded phase pools and deterministic take cycling without gameplay randomness. SoundEvents freezes actual takes, pitches and gains once per receipt for both live playback and export.

Score playback uses Unity's DSP clock with 100 ms look-ahead. Cues over 80 ms late are dropped. Mute immediately cancels sounding and future voices, including unmute/remute within one frame. Skip, phase exit and interruption leave no pending celebration. Sound never quantizes or delays authoritative scoring, or changes an outcome.

Interface, Motion and Score groups retain the existing mixer GUID. Keep motion beneath decisions, impacts in the foreground, and the large release as the strongest accent. Priority prevents incidental feedback stealing a pending score impact. Pools remain bounded: Discover eight one-shots plus its loop, Landing eight, Build six, preparation four. All are 2D, preloaded/decompressed mono PCM at 48 kHz, without play-on-load.

Preserve ordinary-to-exceptional dynamics; do not normalize each interaction independently. Program meters use BS.1770 with dual-mono compensation for mono effects delivered to stereo. Keep representative true peaks below −1 dBTP and check complete interactions.

## Sources and authoring

`Assets/Editor/AudioSources/DeepwaterRevision/ChargedApproval.json` freezes the user's selected source hashes, comparison calibration and selection. `Tools/Audio/build_charged_audio.py` rebuilds the runtime family offline and verifies its raw recipe still matches that selection. It writes `Assets/Audio/Deepwater/manifest.json`; **Fishing Cards → Audio → Install Deepwater Sound Direction** imports the manifest and retains all four prefab bindings. Normal art/prefab configuration preserves library tuning; deliberate manifest import restores the authored mix.

There are 28 cue identities and 42 takes. Material comes from the guarded free-local Earshot catalogue and reused CC0 Kenney card handling. IDs, model-family credits, licenses and hashes stay with the Editor-only sources, outside player content. Some catalogue material is procedural and some is reconstructed from public-domain recording models; neither is presented as an original commercial-game recording. No paid or hosted service, API key, upload or hosted critique is used.

The acoustic builder and its listening page are historical material. Use the Charged Cabinet builder for active production audio.

## References and acceptance

Characterful arcade feedback, visual contact and distinct rewards are the useful principles from [SomaTone on Peggle](https://www.gamezebo.com/news/interview-with-somatone-interactive-audio/), [PopCap's Peggle Blast audio session](https://www.gdcvault.com/play/1022188/Peggle-Blast-Big-Concepts-Small), [CloverPit's developer account](https://news.xbox.com/en-us/2025/11/20/clover-pit-xbox-partner-preview/) and [Slots & Daggers' creator description](https://store.steampowered.com/app/3631290/Slots__Daggers/?l=english). No sample or melody was copied. Detailed research boundaries remain with the [comparison](Validation/AudioRevision/README.md).

The user's A selection establishes the creative direction. Automated checks establish timing, level, source integrity and cancellation behavior; they do not replace human review of a complete game session. [Charged Cabinet validation](Validation/ChargedCabinet/README.md) records the integration and its agreement with the selected audition.
