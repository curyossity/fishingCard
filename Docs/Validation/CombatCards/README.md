# Landing combat cards: validation

27 November 2026. This change is presentation only: no score, Resistance, attempt, outcome or reward rules change. The Landing machine's two moving cards swap meaning so the screen reads as **your strike against the creature**:

- **Left: Strike gauge (yours).**
  - At rest it shows a hook, READY, the frozen accuracy bases (GOOD 10 · PERFECT 20) and SPACE TO STRIKE.
  - On a strike, the score readout takes the display: the grade in the header tab and the running damage in the window.
  - Each contributing rig creature flips in with its portrait beside the number, its bonus, and its name and level in the label slot.
  - After the challenge it reads LANDED / CREATURE LANDED, or SPENT / NO ATTEMPTS LEFT with a dimmed hook.
- **Right: Specimen plate (the creature).**
  - Shows the encounter portrait, its tag on the brass name tab, and the **Resistance gauge, which drains**. The number shows what is left, e.g. 29 / 40.
  - The creature entrance, the rarity banner (now across the specimen window, clear of the gauge), the LANDED / ESCAPED beats, the hurt reaction and the damage chip all play on this card.
  - The header keeps the creature's name throughout; contributors no longer replace it.
- **The machine:** both cards rest directly on the machine's own recess. The original painted panels are erased from the machine art, and the flat placeholder block that used to replace them is gone.
- **Turns:** every identity change on the Strike panel is now one 0.14 s turn: strike to the first contributor, between contributors, and back to the strike. The old three-turn 0.21 s boundary existed only to tell the creature apart from rig cards on a shared slot.
- **The ram:** the collision is unchanged (left rams right), so every ram is now the player's side hitting the creature. Previously the first ram was the creature's own card hitting the score.

Art and provenance: [Landing combat cards source](../../UIThemes/TackleApparatusConcept/CombatCards/README.md).

## Ownership

| Owner | Responsibility |
| --- | --- |
| `LandingCombatCardsSetup` | Retained by `LandingScoreRushSetup` right after the collision groups; authors the frames, sizes, field placement and bindings |
| `LandingStrikePanel` | Idle state, label, legend, and the number's placement beside contributors (on `LandingScoringPortrait.Changed`) |
| `TackleLandingPresentation` | Remaining-Resistance text and draining fill |
| `LandingDamageFeedback` | Knockback away from the attacker; the chip spans the Resistance just lost |
| `LandingOutcome.behindCard` | Keeps the recess's assistance readouts hidden while an escaped creature's card is gone |

## Verification

All runs were fresh comparison-scene Play sessions with persistence disabled.

| Check | Result |
| --- | --- |
| `LandingDamageFeedbackChecks` | 8 timing + 12 live |
| `LandingOutcomeChecks` | 34 live |
| `LandingScoreRushChecks` | 142 live |
| `LandingCreatureRevealChecks` | 21 live |
| `RigBuildingSceneChecks` | 106 live |
| `TacklePhaseTransitionChecks` | 15 live |

- **Assertion updated:** one score-rush assertion now accepts the Strike panel's idle prompt after a skip, since the label no longer holds the creature's identity.
- **Evidence folders:** the score-rush, reveal, outcome and phase-transition folders were restored to their committed captures. The damage-feedback previews were re-encoded on the new cards.

Captures:

- [At rest](At-Rest.png)
- [Hit lands](Hit-Lands.png)
- [Contributor strike](Contributor-Strike.png) (mid-flip)
- [Legendary reveal](Reveal-Legendary.png)
- [Landed](Landed.png)
- [Escape cash-out](Escaped-CashOut.png)
- [Escape final](Escaped-Final.png)
- [Failure settled](Failure-Settled.png)

## Known gaps

- The "ongoing assistance" readout (zone-assist sources and totals) still sits in the recess behind the creature card, where it was already mostly hidden before this change. It needs a proper visible home, probably on the Strike panel.
- The hook icon is small.
- Not yet verified: human play feel.
