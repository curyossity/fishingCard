# Landing damage feedback: validation

27 November 2026. This makes a strike's Landing Points read as damage to the encounter creature. It is presentation only: no score, timing gate, attempt or outcome changes.

## Behavior

At the score payout, the moment a strike's points transfer into accumulated Landing Points, several reactions play together for 0.36 s on the score sequence's clock:

- **Creature hurt reaction** (on the right-hand specimen plate since the [combat cards](../CombatCards/README.md) change):
  - The creature card is knocked away from the Strike panel (8–28 px, to the right), with a short shake, a tilt of up to 5.5° and a brief squash.
  - A red hurt tint flashes over its panel silhouette, reusing the existing contact-flash outline in a tint.
- **Fish thrash:** the moving fish in the timing glass thrashes sideways while the motion clock is held, then returns exactly to its gameplay position.
- **Damage chip:**
  - A bright cream segment marks the part of the draining Resistance bar the strike just took away.
  - It swells slightly, holds 0.1 s, then fades over 0.24 s while the fill drains underneath it, like a fighting-game health bar.
  - It sits beside the clipped bar on the creature card, so it travels with the card.
- **Severity:**
  - It follows the share of Resistance the strike dealt: `0.25 + 1.5 × share`, clamped to 1.
  - A finishing blow is lifted to at least 0.85.
  - Zero-point strikes, such as an ordinary Miss, get no reaction.
- **Composition:** the reaction is layered over the collision pose each frame (the collision resets the card to rest first), and it fits inside the shortest existing post-payout tail, so nothing is lengthened.
- **Reduced motion:** no knockback, tilt, squash, thrash or chip swell. A softer tint and the chip remain.
- **Sound:** none new yet. The existing payout cue still marks the moment. A hurt cue is a `team-audio` decision.

## Ownership

| Owner | Responsibility |
| --- | --- |
| `LandingDamageFeedback` | Pure reaction curves and the component that applies them |
| `LandingScoreRush` | Renders it right after the collision and clears it on finish |
| `LandingPanelFlash.Show(opacity, tint)` | Tinted flash over the panel silhouette |
| `LandingCardCollision.IsCaptured` | Composition guard |
| `LandingDamageFeedbackSetup` | Authors the chip layer and bindings; retained by `LandingScoreRushSetup` regeneration |

## Verification

- **8 timing assertions** (`LandingDamageFeedbackChecks.ValidateTiming`):
  - There is no reaction without damage, and severity scales with the share of Resistance, with the finishing-blow lift.
  - Every channel returns to rest, knockback points away from the Strike panel, scaled by severity, and the chip span matches the lost Resistance.
  - Reduced motion softens the tint, and the reaction fits within the post-payout tail.
  - The chip holds, then fades.
- **[Live](LiveVerification.json): 12 assertions** in a fresh comparison Play session with persistence disabled. A real Perfect against Cod deals non-finishing damage:
  - There is no chip before the payout.
  - At the payout the creature card is knocked back (away from the Strike panel), takes the hurt tint, and the chip spans exactly the Resistance just lost.
  - Reduced motion keeps the card still while keeping the tint and the chip.
  - Everything settles inside the tail, and skipping restores the card, flash and chip.
  - A real Miss produces no reaction.
- **Previews:** [normal speed](Strike.mp4) and [quarter speed](Strike-QuarterSpeed.mp4), cropped to the machine. Stills `Strike-1…5` run from just before the payout to settling.
- Not yet verified: human play feel across large late-game hits.
