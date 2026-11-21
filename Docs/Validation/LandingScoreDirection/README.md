# Scoring direction: anticipation, chain and release

The visual presentation remains documented here. Its original sound set is superseded by the selected [Charged Cabinet direction](../ChargedCabinet/README.md) and [Sound Style](../../Sound%20Style.md).

Implemented 20 November 2026, using game-feel, audio-design, uGUI and Unity C# skills. This change applies to the restored master scoring presentation and preserves the independent insertion-tray glow work.

## Delivered

- One consistent firing point: the contributor temporarily replaces the target in the main encounter portrait, with its name and RIG EFFECT caption. The small icon/name below the digits is hidden and the operation is centered. Grouped beats show their contributors together in the main slot. Source cards acknowledge the beat with a restrained scale response; repeated tilting is removed.
- Every beat separates preparation, launch, arrival and recovery. The score and impact sound advance on arrival. The initial accuracy response occurs at 76 ms.
- Additions accelerate from measured clicks into a faster rhythm. Multipliers have additional preparation, a short reel ratchet and a heavier number response. Actual additions remain before multipliers.
- Ordinary results resolve quickly without machine movement or payout particles. Strong combinations receive a stronger release; exceptional amplification earns a longer final hold, fuller reel/brass release and modest vertical cabinet response. Intensity derives from actual score amplification, not fabricated retriggers or new rewards.
- Long rigs combine adjacent operations of the same arithmetic phase into readable beats. All operations are retained and evaluated in their original decimal order; grouped identities are acknowledged together and exact individual attribution remains in the inspectable breakdown. Presentation is bounded below 4.8 seconds and remains skippable.
- The secondary summary says “SCORING” until delivery rather than revealing the final total early. The source strip becomes quieter during the primary sequence and returns afterward.
- Seven original wood/reel/brass PCM cues are installed under `Audio/Landing/Mechanical`. Previous sound files remain preserved. The existing six 2D voices and feedback mixer route playback; sound preferences and reduced motion remain supported.

The main-slot follow-up preserves and restores the target illustration, enabled state, tint, scale, name and tag text at payout, skip, independent presentation disable and phase exit. Accuracy-only strikes leave the target visible. `LandingScoringPortrait` owns this temporary presentation; it never changes the encounter identity or scoring rules. The three-tier browser preview reflects this arrangement.

## Review

Open [Preview.html](Preview.html) in a browser and select a tier to play with sound. It loads [clips.js](clips.js) from the same folder; keep both files together. Frames are exported from the actual Unity UI at 24 fps. Audio is mixed from the runtime sound-event list at the same event times, sample pitches and cue gains. This is a staged, reproducible comparison rather than a live audio recording or final sound approval.

The three disposable rigs use installed creature abilities and the actual Landing model to resolve their results. `LandingScoreShowcase.Export()` rebuilds the comparison in a fresh Play session without modifying content or player saves. The preview intentionally isolates scoring from the surrounding expedition.

## Verification and limits

20 scheduling/rules assertions, 40 saved UI/audio assertions and 142 live assertions passed (202 total). The end-to-end score fixture verifies contributor replacement, target restoration at payout/skip/disable, grouped portraits without input interception, exact scoring, phase handoffs, reduced motion, actual large combinations, duplicate upgrades and full-haul failure payout; see [live report](LiveVerification.json). Project integrity checked 1,855 Unity files with zero errors/warnings.

Hands-on speaker/headphone listening and subjective pacing approval remain review work. Automated checks establish timing, routing and rule preservation; they do not establish that the new sound palette or animation direction is aesthetically approved.
