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
- **DAMAGE counter.**
  - A small dark plate in the gap between the header and the creature card shows the strike's running total, updating on each impact. The water impact's digit punch now lands on it.
  - In the 0.16 s before the payout it drops and shrinks toward the Resistance bar, landing just as the bar starts to drain.
- **Reduced motion:** numbers and the counter appear and fade in place, with no drift, pop or flight.

## Ownership

| Owner | Responsibility |
| --- | --- |
| `LandingDamageNumbers` | Pure curves plus a pooled renderer, sampled statelessly from the score sequence |
| `LandingScoreRush` | Begins, renders and clears it |
| `LandingStrikePanel` | Strike face (hook and grade) vs. contributor portrait |
| `LandingDamageNumbersSetup` | Authors the layer, template, counter and outline material; retained by score-rush and combat-card regeneration |

## Verification

- **8 timing assertions** (`LandingDamageNumbersChecks.ValidateTiming`): labels per kind, distinct colours, no number on a Miss, pop and settle scaled by strength, rise and fan-out, fade, the counter landing at the payout after the last ram.
- **[Live](LiveVerification.json): 20 assertions.** A real Perfect against Cod with a seeded Sardine / Cuttlefish / Stormjaw Tuna rig:
  - The panel's own number, effect, caption and glow stay hidden.
  - The base ram shows hook and grade on the left, floats +20 and shows DAMAGE 20.
  - Every contributor ram shows only that creature on the left, floats its own effect, and the counter follows the running total.
  - The counter drops toward the bar and is gone at the payout.
  - Skipping clears everything.
- **Regression:** the score-rush asset check (83, rewritten for the combat-card faces and the empty machine) and its rules (232) and live (142) checks pass. Its evidence folder was restored afterwards.
- **Previews:** [normal speed](Combo.mp4) and [quarter speed](Combo-QuarterSpeed.mp4), cropped to the machine. Stills `Combo-1…4` show the base ram, contributor rams and the counter's drop.
- Not yet verified: human play feel. Sound for the number pops is a `team-audio` decision.
