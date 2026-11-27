# Landing combat cards: art source

Added 27 November 2026. These two pixel-art frames give the Landing machine a clear "your side, their side" read:

| File (`Assets/UIThemes/TackleApparatus/Art/Landing/Combat/`) | Size | Role |
| --- | --- | --- |
| `strike-panel.png` | 452 × 220 (native 226 × 110) | **Strike gauge**, the player's side: teal machine-steel housing, brass faceplate and header tab, dark display window, recessed label slot. |
| `creature-card.png` | 428 × 232 (native 214 × 116) | **Specimen plate**, the encounter creature: parchment clamped in a rust-iron holder by brass corner clamps, a brass name tab, and a riveted Resistance gauge (dark glass trough, brass rim, quarter notches). |
| `hook-icon.png` | 32 × 52 | Brass hook for the Strike gauge's ready state. |
| `bar-fill.png` | 12 × 20, 2 px nine-slice border | Red Resistance fill. |

| `landing-machine-empty.png` | 1739 × 904 | The Landing machine with its two original painted panels erased, so the cards sit directly on the machine's own recess. |

### Empty machine interior

[`Tools/Art/make_empty_landing_machine.py`](../../../../Tools/Art/make_empty_landing_machine.py) derives `landing-machine-empty.png` from the untouched `Integrated/landing-machine.png`:

- **The fill:** the old panels sat inside the dark recess with a narrow channel of clean recess around them. The script fills the panel area, including the old panels' drop shadows, with a Coons-patch blend of that channel's colours. This keeps the recess shading, which is darker under the top bevel. It adds quiet 2×2 grain on top.
- **The corners:** the fill follows the recess's chamfered corners, so the original bevel survives.
- **Everything else:** every other pixel and every alpha value is copied byte for byte.
- **Import:** `LandingCombatCardsSetup` imports it with the original's settings and draws the machine from it. It also switches off the old collision housing, which showed a flat dark block where the painted panels were.
- **Rerun:** `.local-tools/earshot/venv/Scripts/python.exe -I Tools/Art/make_empty_landing_machine.py Assets/UIThemes/TackleApparatus/Art/Landing/Integrated/landing-machine.png Assets/UIThemes/TackleApparatus/Art/Landing/Combat/landing-machine-empty.png`

## How they were made

- [`Tools/Art/make_combat_cards.py`](../../../../Tools/Art/make_combat_cards.py) draws every frame procedurally on its native pixel grid, then scales it 2× with nearest neighbour.
- Light comes from the top-left: a two-step bevel, an outline, an inner lip, clustered grain and wear specks, and rivets.
- Palette ramps are sampled from the existing painted assets (`portrait-frame.png`, `score-readout.png`, `progress-trough.png`). The rust and teal-steel ramps match the cabinet.
- Each frame uses 21–26 colours, has real transparent corners and multiple-of-4 dimensions, and contains no baked text.
- `Tools/Art/combat-cards-layout.json` records the native rectangles of every live region (display, tab, label slot, portrait, tag, trough). `LandingCombatCardsSetup` uses them doubled.
- **Rerun:** `.local-tools/earshot/venv/Scripts/python.exe -I Tools/Art/make_combat_cards.py Assets/UIThemes/TackleApparatus/Art/Landing/Combat`, then move `layout.json` to `Tools/Art/combat-cards-layout.json`.
- **Provenance:** made with the free local Pillow/numpy script only. No image-generation model, paid tool or external service was used; this is not AI-generated content.

## Design review

The direction was reviewed in Penpot on page **13 - Landing Combat Cards (Proposal)**:

- "B (new design)" is the chosen direction, with at-rest, hit-lands and card-state boards.
- "A (existing art)" is kept as the alternative that reused the original standalone frames.
- An earlier flat vector draft was rejected as not matching the game.

Checked with the `create-game-assets` skill's `asset_report.py`: frames have alpha, tight palettes and no oversize. The bar fill is intentionally fully opaque.
