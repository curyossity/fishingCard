# Landing damage numbers: validation

27 November 2026. This change is presentation only: no score, Resistance or timing change.

## Behavior

- **Strike panel (left) shows only who attacks.**
  - **Accuracy hit:** the hook and the grade (PERFECT / GOOD / MISS), with "YOUR STRIKE" in the slot.
  - **Rig contributor:** its portrait fills the display, with its name and level in the slot. The panel's own number, effect line, grade caption and glow are hidden.
- **Floating effect numbers (`LandingDamageNumbers`).**
  - At every ram, the hit's own effect pops out of the impact point on the creature card: **+20** for the accuracy base, **+3** for an addition, **×2** for a multiplier.
  - Each kind has its own colour: cream base, gold addition, sea-green multiplier. Numbers use the pixel font with a solid dark outline (`damage-number-outline.mat`).
  - Each number bursts past full size and settles within 0.16 s (bigger for stronger steps), rises 70 px with an ease-out, and fans out at a fixed set of angles so consecutive numbers don't stack. It fades out over 0.45–0.7 s.
  - A zero-point Miss floats nothing.
- **Final damage slam.** There is no running total during the rams.
  - In the 0.2 s before the payout, the strike's total appears large (2.3×) above the creature and drops onto its portrait, accelerating like a falling weight.
  - It lands exactly at the payout, the moment the hurt reaction and the Resistance bar drain begin.
  - On landing it squashes past full size and settles, holds 0.3 s, then fades over 0.25 s while the bar drains.
  - It is bright gold with the dark outline, so it reads against both the parchment and the red hurt flash.
  - Floating numbers still in the air fade out as the slam comes down.
- **Reduced motion:** the numbers and the slam appear and fade in place, with no drift, pop or drop.

## Ownership

| Owner | Responsibility |
| --- | --- |
| `LandingDamageNumbers` | Pure curves plus a pooled renderer, sampled statelessly from the score sequence |
| `LandingScoreRush` | Begins, renders and clears it |
| `LandingStrikePanel` | Strike face (hook and grade) vs. contributor portrait |
| `LandingDamageNumbersSetup` | Authors the layer, template, slam and outline material; retained by score-rush and combat-card regeneration |

## Verification

- **11 timing assertions** (`LandingDamageNumbersChecks.ValidateTiming`): labels per kind, distinct colours, no number on a Miss, pop and settle scaled by strength, rise and fan-out, fade. The slam drops large and lands at full size exactly at the payout, after the last ram; it appears, holds and fades within the strike's feedback; floating numbers clear for it.
- **[Live](LiveVerification.json): 23 assertions.** A real Perfect against Cod with a seeded Sardine / Cuttlefish / Stormjaw Tuna rig:
  - The panel's own number, effect, caption and glow stay hidden.
  - The base ram shows hook and grade on the left and floats +20; there is no slam during the rams.
  - Every contributor ram shows only that creature on the left and floats its own effect.
  - The total drops in large and lands on the creature's portrait at the payout, with the floats cleared and control still held.
  - Skipping clears everything.
- **Regression:** the score-rush asset check (83, rewritten for the combat-card faces and the empty machine) and its rules (232) and live (142) checks pass. Its evidence folder was restored afterwards.
- **Previews:** [normal speed](Combo.mp4) and [quarter speed](Combo-QuarterSpeed.mp4), cropped to the machine. Stills `Combo-1…5` show the base ram, contributor rams, the slam dropping and landing.
- Not yet verified: human play feel. Sound for the number pops is a `team-audio` decision.
