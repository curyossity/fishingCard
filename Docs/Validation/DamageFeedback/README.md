# Landing damage feedback: validation

27 November 2026. This makes a strike's Landing Points read as damage to the encounter creature. It is presentation only: no score, timing gate, attempt or outcome changes.

## Behavior

At the score payout, the moment a strike's points transfer into accumulated Landing Points, several reactions play together for 0.36 s on the score sequence's clock:

- **Creature hurt reaction** (on the right-hand specimen plate since the [combat cards](../CombatCards/README.md) change):
  - The creature card is knocked away from the Strike panel (8–28 px, to the right), with a short shake, a tilt of up to 5.5° and a brief squash.
  - A red hurt tint flashes over its panel silhouette, reusing the existing contact-flash outline in a tint.
- **Fish thrash:** the moving fish in the timing glass thrashes sideways while the motion clock is held, then returns exactly to its gameplay position.
- **Health trail** (`LandingHealthTrail`):
  - The red fill eases down from the old Resistance to the new one (0.45–0.63 s, ease-out). A white trail stays on the old value meanwhile, showing how much was lost.
  - Once the red has settled, the trail holds 0.15 s, then eases down over 0.35 s (ease in-out) until it meets the red. A damaging strike keeps control until the trail settles (`LandingScoreSequence.Duration` = payout + `LandingHealthTrail.Tail`); a Miss keeps its short tail.
  - It sits beside the clipped bar on the creature card, under the remaining-Resistance plate, so it travels with the card and never hides the number.
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

- **12 timing assertions** (`LandingDamageFeedbackChecks.ValidateTiming`):
  - There is no reaction without damage, and severity scales with the share of Resistance, with the finishing-blow lift.
  - Every channel returns to rest, knockback points away from the Strike panel, scaled by severity, and the chip span matches the lost Resistance.
  - Reduced motion softens the tint, and the reaction fits within the post-payout tail.
  - The bar holds for `LandingHealthTrail.Delay` (0.2 s) after the slam; then the red eases down first, and the trail holds, then eases after it and never ahead of it. Control waits for the trail on damaging strikes but not on a Miss.
- **[Live](LiveVerification.json): 16 assertions** in a fresh comparison Play session with persistence disabled. A real Perfect against Cod deals non-finishing damage:
  - There is no chip before the payout.
  - At the payout the creature card is knocked back (away from the Strike panel) and takes the hurt tint, while the bar still holds. After the hold, the white trail holds the old Resistance while the red drains, the red settles on the new value, then the trail eases down while control is still held.
  - Reduced motion keeps the card still while keeping the tint and the chip.
  - Everything settles inside the tail, and skipping restores the card, flash and chip.
  - A real Miss produces no reaction.
- **Previews:** [normal speed](Strike.mp4) and [quarter speed](Strike-QuarterSpeed.mp4), cropped to the machine, were recorded with an earlier, faster red drain (0.27 s) and no hold. Stills `Strike-1…5` (28 November 2026) show the current timing, from just before the payout to settling.
- Not yet verified: human play feel across large late-game hits.
