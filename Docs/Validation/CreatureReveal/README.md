# Landing creature reveal — validation

26 November 2026. Presentation-only entrance for the encounter creature when Landing opens after Accept Tags. Rules, Resistance, scoring, attempts and motion tuning are unchanged.

## Behavior

- The creature panel starts hidden (empty recess, blank name band) and turns face-up using the machine's existing vertical-turn language. Intensity scales with the creature's authored rarity:

  | Rarity | Anticipation | Turns | Settle | Flash / stage impulse | Total |
  |---|---|---|---|---|---|
  | Common | none | 1 quick | small pop | none | 0.38 s |
  | Uncommon | none | 2 | pop | none | 0.54 s |
  | Rare | 0.22 s machine rumble | 3, accelerating | larger pop | one 55 ms flash, 6 px impulse | 0.90 s |
  | Legendary | 0.42 s rising rumble | 3, accelerating | largest pop | one 55 ms flash, 10 px impulse | 1.28 s |

- **Rarity banner (Rare and Legendary only).** When the face lands, a tilted enamel tag stamps across the lower creature card: teal with ivory `RARE` and 3 of 4 hook studs lit, or near-black with gold `LEGENDARY`, all 4 studs lit and one gold glint. It reuses the strike ribbon's brass bevel, studs and swallowtails, but its dark face keeps it distinct from scoring results. It holds about one second into play, never covers the timing track, folds away with the card-turn motion, and clears as soon as a strike's scoring begins. Skipping the reveal stamps it immediately, so rarity is never hidden. Reduced motion fades it. Common and Uncommon have no banner. Native geometry and the existing PressStart2P font only; no new artwork files.
- The message band shows only `SPACE TO SKIP` during the entrance and never names the rarity.
- The existing `Reveal` cue plays once when the face lands, pitched and levelled by rarity. A skipped reveal still plays it once. Rarity-specific cues remain a `team-audio` decision.
- While the reveal plays, the controller holds the motion clock and rejects strikes. Space/Strike (captioned `SKIP`) ends it without striking. A deadline equal to the reveal's duration releases the gate if the view is interrupted.
- Reduced motion keeps a single stationary fade: no turns, pop, flash or stage motion.

## Ownership

`LandingRevealTiming` (pure rarity table), `LandingCreatureReveal` (presentation), `LandingRarityBanner` (Rare/Legendary tag), `FishingRunController.LandingRevealPending` / `CompleteLandingReveal` (gate), `LandingImpactCamera.RenderOffset` (unzoomed stage offset), `LandingCreatureRevealSetup` (bindings, retained by `LandingScoreRushSetup.Configure`). See [Architecture](../../Architecture.md).

## Verification

- [Rules and assets](RulesAndAssets.json): **56 rule assertions** (bounded durations, hidden start, face-up landing, exact rest values, name timing, flash/shake lifetimes, monotonic reduced-motion fade, emphasis increasing by rarity, no anticipation/flash/shake below Rare, at most one flash per reveal; banner only for Rare/Legendary, stamp settles exactly at its tilt, holds then folds, bounded lifetime, Legendary-only glint) and **21 prefab binding assertions** (including a non-intercepting banner drawn above the creature card).
- [Live](LiveVerification.json): **21 assertions** in a fresh comparison Play session with persistence disabled, using the Rare Squid through real Discover acceptance: entrance starts with the creature's rarity, face hidden at start, skip-only caption, strike rejected without consuming an attempt, fish and zone frozen, no banner before the face lands, Strike button skips without striking, panel/stage/face restored, plain instruction without rarity text, banner shown after the skip, motion resumes, strike accepted afterwards, banner cleared when scoring begins, reduced-motion fade without displacement.
- Existing checks: score-rush rules/assets (232/86), strike-feedback assets (27), Build art assets (93) and Landing art assets (146) pass after installation. Shared Accept fixtures in `RigBuildingSceneChecks`, `RigBuildingArtChecks`, `LandingChallengeSceneChecks` and `TackleLandingArtChecks` now skip the entrance so their strike timings are unchanged.

Captures (Legendary pacing and banner on the Squid panel): [anticipation](Legendary-Anticipation.png) · [turning](Legendary-Turning.png) · [settle flash and impulse](Legendary-Settle.png) · [landed](Legendary-Landed.png).

## Previews

[Side-by-side preview page](Preview/index.html): one native 60 fps video per rarity, with the runtime Reveal cue (same take, pitch and gain; mixer-bus level not reproduced). Representatives: Sardine (Common), Small Octopus (Uncommon), Squid (Rare), Stormjaw Tuna (Legendary; the current roster's only Legendary creatures are Apex encounters). [All four in sequence](Preview/Reveal-All.mp4).

- **Live, with real sound:** in Play mode (comparison scene) use **Fishing Cards → Preview → Creature Reveal → Common / Uncommon / Rare / Legendary**. From the Harbor it starts a save-protected preview expedition and opens Landing with that rarity's representative. On an idle Landing it replays the chosen rarity on the current machine. The panel temporarily shows the representative's portrait, name and tag, then restores the real encounter; scores, attempts and definitions are untouched (`FishingRunController.PreviewLandingReveal` is editor-only).
- **Re-render the videos:** **Fishing Cards → Preview → Creature Reveal → Render Videos** in Play mode writes the frames, then run `python -I Tools/Rendering/encode_creature_reveal_preview.py` from the repository root. Encoding is a separate free local ffmpeg step, so videos can be re-encoded without opening Unity.

Not yet verified: human play-feel and listening. The captures are still frames; judge the motion, pacing and sound in the Editor.
