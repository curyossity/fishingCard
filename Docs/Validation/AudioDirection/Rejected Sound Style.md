# Deepwater sound style

**Revision status:** the user rejected this installed acoustic direction on 21 November 2026. Its palette below is retained as a record of that attempt, not an accepted creative direction. The timing, ownership and cancellation contracts remain useful. [The revised in-context comparison](Validation/AudioRevision/Compare.html) offers Charged Cabinet and Arcade Pulse; neither has human listening acceptance yet. Do not use the harp-led identity below as the default brief for further sound creation.

21 November 2026. Presentation direction for the implemented Discover → Land → Build loop. The GDD owns all mechanics; sound follows accepted commands and resolved receipts. This replaces the active synthesized and ratchet/wood/bell directions. Earlier files remain historical references, not the palette for new work.

## Identity

**A close, tactile card table that becomes a small acoustic instrument when the rig works.** Deliberate decisions feel precise; a good strike releases tension; contributing creatures build an accelerating phrase; a powerful result resolves with weight and a short flourish. The nautical apparatus suggests tensioned strings and hand-operated objects. It does not require literal shipyard machinery, gulls, splashes or bubbles on every action.

The foreground palette is recorded concert harp and pizzicato strings, dry card handling, soft kick and cross-stick percussion, and low-level shaker texture. Instrument attacks retain their recorded detail. Short edits, filtering, damping and layers give these a common close perspective. The tonal vocabulary is rooted in D with D/F/G/A/C steps; voicings favor open fifths. Routine actions are mostly unpitched. Effects are centered, with only a small left/right hint for reel contacts. There is no long global reverb or stereo trick required to identify an event.

The intended contrast is **quiet choice → motion → distinct contact → space → reward**. A score becomes richer through note register, lower support and harmonic layering. Its gain rises only modestly. Ordinary results must leave room for extraordinary ones. Do not add jackpots to unmatched reels, repeated alarms to misses, hover chatter to every card, or another generic click underneath an action that already has a signature cue.

## Reference principles

These are design interpretations, not copied samples or melodies:

| Reference | Evidence and application |
| --- | --- |
| [CloverPit, developer Matteo Gonano](https://news.xbox.com/en-us/2025/11/20/clover-pit-xbox-partner-preview/) | The developer describes deliberate work on spin/click/jackpot sound and animation, and anticipation growing with the build. Borrow a clear anticipation/contact/payoff arc; Deepwater uses a warmer, quieter palette and bounded feedback. |
| [Slots & Daggers, creator's description](https://store.steampowered.com/app/3631290/Slots__Daggers/?l=english) | Friedemann explicitly cites drum machines and rewarding arcade sounds. Borrow percussive rhythm and a concise reward vocabulary. Its official trailer excerpt received local timing/model analysis; music and other layers prevent reliable isolation of individual game sounds. |
| [Celeste / Power Up Audio, Kevin Regamey interview](https://www.nintendolife.com/features/the-secrets-of-sounds-and-the-joy-of-door-noises-an-interview-with-celestes-audio-designer) | Regamey connects audio direction to the project's creative boundaries and describes emotionally effective sound with a light touch. Here that means one coherent acoustic perspective, clear feedback and restraint during deliberation. |
| [Into the Breach, composer Ben Prunty's first-person account](https://www.pcgamer.com/how-i-made-into-the-breachs-soundtrack/) | A useful process reference for finding a project's own sonic identity and combining acoustic and electronic materials. This is a music-direction reference, not evidence of its individual SFX. Deepwater's current delivery covers effects, not a new soundtrack. |
| [Dynamic mixing, Randy Coppinger's AES session notes](https://randycoppinger.com/2014/10/22/dynamic-mixing-for-games/) | Prioritize sounds according to the interaction rather than leaving every layer at equal prominence. Deepwater places motion below decisions, scoring above both, and reserves space before reward. No middleware dependency is introduced. |

The user's rejected examples remain important: plain synthesized slot tones, literal ratchet/wood/metal/bell slot edits, and unrelated feedback sounds are superseded. Installing an audio skill or obtaining a favorable model score does not establish artistic quality.

## Event vocabulary and timing

All durations below are authored file lengths, not gameplay delays. Immediate events are requested in the frame of the accepted action. Device/output latency still applies. Score events use a frozen shared schedule and Unity DSP scheduling with 100 ms look-ahead; an event over 80 ms late is discarded rather than played in a burst. Stop and placement events follow their actual visual contacts, within the rendering frame.

| Event | Sound and timing | Mix behavior |
| --- | --- | --- |
| Tag select / deselect | 150 / 130 ms card contact, select adds a very quiet string body. Only actual draft changes. | Three takes, 70 ms retrigger guard. No sound for opening, focus or unchanged refresh. |
| Insertion preview | 130 ms dry paper flick at a deliberately changed insertion position, including keyboard navigation. | Three takes, 75 ms guard; hover inspection stays silent. |
| Spin start | 250 ms shuffled-paper attack with low string and kick support, once per spin revision. | Motion foreground; no separate generic button click. |
| Reel motion | Quiet 900 ms circular shaker bed, with a 40 ms gain attack and less energy as reels stop. | Runs only while reels move. Mute and phase exit stop it immediately. |
| Symbol crossing | 55 ms softened shaker contact at a real boundary, with global 45 ms cooldown and per-reel throttling. | Three takes; substantially below the stops. No clock-driven fake ticks or catch-up burst. |
| Reel stops | 200 ms contact and harp D/G/A, in reel order, at complete visual settlement. | Locked reel is silent on respin. Small stereo offsets; mono remains readable. |
| Lock / unlock | 160 / 150 ms short damped pluck and contact on an accepted change. | No sound on initial binding or rejected commands. |
| Pair / triple | 290 / 550 ms two-note / lifted three-note phrase, **90 ms after the final reel settles**. | No match cue for three different symbols; pending reward cancels on exit. This celebrates matching, not creature acquisition. |
| Encounter reveal | 300 ms low harp/card turn when a new Landing occurrence is shown. | Once per occurrence; no rarity-dependent promise or repeated refresh sound. |
| Accepted strike | 100 ms tactile contact at time zero. | No click for rejected input or Finish. A Miss uses its own contact instead. |
| Good / Perfect | 220 / 300 ms pluck versus higher, fuller harp/string contact at the base-point impact, normally 76 ms after strike. | Distinct timbre/register; Perfect is not a much louder Good. |
| Miss | 180 ms damped low contact at time zero. | Exactly once. Zero points have no payout cue. Protected/scoring Misses retain their real contribution sounds. |
| Contributing creature launch | Reversed recorded pluck, stretched by bounded playback pitch to fit the projectile's flight. | Quiet approach; ends at impact within the supported pitch range. No effect changes the score early. |
| Addition / multiplier impact | 230 / 320 ms note versus fuller harp/string/bass voicing, exactly at the frozen step's `ImpactAt`. | Three takes each. Minor pentatonic rise capped at one octave; no endless squeaks. Multipliers are distinguished by voicing, not arbitrary pitch jumps. |
| Points transferred | 240 / 390 / 650 ms three-tier resolution at `PayoutAt`. | Tier follows actual score amplification. Larger tiers add harmonic weight and a brief answer, with only modest gain growth. No coins are implied by Landing Points. |
| Expedition failure | 520 ms descending, damped string/card closure when the ended-expedition display is available after scoring feedback. | Once; no buzz, taunt, jackpot, or sound implying a lost haul. |
| Placement flight / attachment | 240 ms paper movement at receipt start; 260 ms card/string contact at the actual receiving-card impact (currently 320 ms into travel). | Distinct actions, three attachment takes. Reduced motion uses one immediate attachment contact and no flight. |
| Duplicate upgrade | 650 ms rising harp/string phrase at the automatic upgrade's first visible pulse. | Separate from attachment and scoring payout. No confirmation sound or extra reward. |

Silence is intentional during tooltips, card hover, steady Landing movement, instructional refreshes, invalid commands and initial UI synchronization. No additional timer beeps, near-miss deception, ambient loop, music bed or voice system is introduced by this effects pass.

## Mix, authoring and ownership

`Assets/Audio/Deepwater/DeepwaterSoundLibrary.asset` is the editable runtime source for takes, bus, gain in dB, priority and retrigger interval. The checked-in render manifest records the initial authored mix; the offline builder reproduces its assets. Running **ImportLibrary** deliberately resets the library to that manifest. Normal prefab configuration reuses an existing library and preserves Inspector tuning.

The existing feedback mixer retains its GUID and has Interface, Motion and Score groups under Master. Phase-owned `DeepwaterSoundPlayer` pools are bounded (Discover 8 one-shots plus one motion loop, Landing 8, Build 6, preparation 4). Higher-priority score contacts can replace disposable voices; incidental UI cannot steal a pending payout. Alternate takes cycle without consuming gameplay randomness. UI cooldowns do not apply to frozen score events. Mono PCM, 48 kHz, preloaded/decompressed clips keep short attacks ready; all voices are 2D and never play on load.

Normal runtime uses the saved shared SOUND preference. Mute, disable, interruption and phase exit stop both sounding and scheduled voices; re-enabling does not replay missed notes. The fixed score schedule also feeds review export, so a video does not quietly use a different mix. Gameplay resolves first and is never quantized to audio, delayed for a phrase, or altered by a muted/skipped presentation.

Keep routine UI below machine stops, addition impacts below multipliers, and the largest payout as the foreground accent. The authored set uses sample peaks no higher than 0.64 before cue attenuation. Verify representative mixes against a true-peak ceiling of −1 dBTP, and inspect dynamics rather than normalizing every isolated cue to the same LUFS value. A PC effects-only play sequence should sit around −24 LUFS with deliberate quiet gaps; this is a tuning reference, not a promise that every short cue independently meets a program-loudness target. Check mono, headphones and small speakers during human review.

## Sources, review and remaining listening

The active palette uses public CC0 instrument recordings from VCSL and VSCO-2 CE, original CC0 card-handling recordings from Kenney's Casino Audio, and Earshot's locally reconstructed CC0-derived percussion material. Sources, pinned revisions, licenses and hashes are recorded in `Assets/Editor/AudioSources/Deepwater/`. Original sources stay under Editor and are excluded from the player. No other game's recording is shipped. All composition and processing runs locally, without credentials, paid tools, uploads or hosted critique.

[Audio review](Validation/AudioDirection/README.md) distinguishes technical verification, local CLAP/AST inference and human listening. Model-assisted classification can reject obvious mismatches and check source character, but its musical/mood judgments and very short SFX judgments are uncertain. It cannot certify that this feels professional or satisfying. The soundboard and in-game flow are supplied for that listening decision; human headphone/speaker acceptance remains unverified.
