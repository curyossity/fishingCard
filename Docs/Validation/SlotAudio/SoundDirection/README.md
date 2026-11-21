# Deepwater slot sound direction — review only

21 November 2026. Original audio sketches, not installed in the game. This is an art-direction review following rejection of the recorded ratchet set. No gameplay, prefab, runtime audio or authoring selection is changed by this preview.

**Listening limitation:** the available tools cannot audition video/audio. The agent researched published developer descriptions and reference links, but did not hear these games or the exported previews. Signal checks below are not a listening test. Human listening is required before claiming the sound fits Deepwater.

## References and interpretation

| Reference | Verified source | Proposed lesson for Deepwater |
| --- | --- | --- |
| CloverPit | [Developer's Xbox Wire article](https://news.xbox.com/en-us/2025/11/20/clover-pit-xbox-partner-preview/): describes extended work on sound/animation for spins, clicks and jackpots to make the machine satisfying. [Official trailer](https://www.youtube.com/watch?v=6f_6xETRrOI). | Treat launch, rotation, individual stops and result as one phrase. Contrast anticipation with the final accent. Do not import its horror atmosphere. |
| Slots & Daggers | [Creator's Steam description](https://store.steampowered.com/app/3631290/Slots__Daggers/?l=english): identifies crunchy hip-hop drum machines and arcade coin sounds among its inspirations. | Give the machine a musical/percussive identity rather than reproducing workshop mechanisms. Use an original short motif. |
| Luck be a Landlord | [Developer presskit and trailer](https://trampolinetales.com/lbal/presskit) establish the 2D slot/deckbuilding reference. They do not document the sound's timbre, and it has not been auditioned here. | A useful future listening comparison for frequent, compact spin/result feedback; no unsupported claim about its precise sound. |

These are proposed applications of the sources, not measured or heard descriptions of their audio. No clips, melodies, source files or extracted game sounds from these games are used in the preview.

## Original proposed phrase

- Launch: one rounded low contact, with a brief soft attack.
- Rotation: quiet filtered air and light percussive ticks whose density follows the existing eased reel travel. No ratchet sample or constant drill-like loop.
- Stops: three distinct short tonal contacts, in the same original D-major pitch family. They identify the wheels; the ordinary stops do not depend on a match.
- Ordinary result: allow the last stop to finish without a reward cue.
- Pair: a short two-note answer after the final stop.
- Triple: a brighter ascending answer and a low supporting note, still much shorter than a Landing score celebration.
- Locked respin: only the two moving wheels tick and stop. No new rule, anticipation of an unknown outcome or fake near-miss is introduced.

[Open the complete-spin previews](Preview.html). Listen to the whole phrase, not only isolated samples. The durations mirror the current 1.05/1.20/1.35-second reel timing; these are authored review examples, not audio captured from runtime.

Run `python Docs/Validation/SlotAudio/SoundDirection/render_preview.py` to reproduce four mono 44.1-kHz/16-bit WAVs from original mathematical sound design. [Verification data](Verification.json) records durations, sample peaks, RMS and authored stop/accent times. These checks validate files and timing; they do not validate artistic quality, true peaks or session loudness.
