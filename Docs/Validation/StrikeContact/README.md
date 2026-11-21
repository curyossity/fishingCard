# Game-feel phase 1: strike contact

21 November 2026. Revised and installed in the actual Landing prefab. The user accepted the strike-contact idea but found the first narrow filament visually too weak; this revision replaces that treatment with a more visible compression, glass impact and quality stamp.

| Phase | Scope | Status |
| --- | --- | --- |
| 1 | Immediate strike contact and grade distinction | Implemented and validated |
| 2 | Weight individual score operations by their actual contribution | Proposed; not implemented |
| 3 | Distinct climax when accumulated points cross Resistance | Proposed; not implemented |
| 4 | Clearer supporter-to-source-to-score choreography | Proposed; not implemented |
| 5 | Continuity from encounter to acquired card and clearer upgrade improvement | Proposed; not implemented |
| 6 | Consistent intensity hierarchy and quieter intervals | Proposed; not implemented |

## Installed behavior

- Every new accepted receipt presses the Strike button's visual face by four reference pixels immediately, including keyboard/submit-originated strikes. It holds for 45 ms and returns over 95 ms. Pointer release cannot erase a still-active accepted press; the button's hit area does not move.
- The receipt freezes the accepted marker center. A non-raycastable native mesh overlays the existing track mask at that position; the live marker and Good/Perfect bands are never altered.
- Good/Perfect contact meets the existing accuracy sound and base-score impact at 76 ms. Two light strokes compress toward the contact before impact. Perfect releases a 200-pixel feathered reflection, hot core, paired outward pressure fronts and eight short rectangular brass flecks. Good uses a smaller mint reflection and four flecks. Miss responds at its existing sound onset with two subdued slack strokes. No sound file, mix gain, modifier timing or gameplay clock changes.
- A shadowed native `PERFECT`, `GOOD` or `MISS` stamp appears above the contact outside the track mask. It lands with a brief compression/overshoot, holds and fades over 360/290/220 ms respectively. Its position clamps within the track edges and uses shared-stage local transforms, remaining correct even while an enabled canvas initially has zero world scale.
- Housing recoil peaks at 5 reference pixels for Perfect, 2.5 for Good and a subdued opposite-direction 0.65 for Miss, with a short damped return. The score owner composes this with its existing housing response, avoiding two scripts writing the same transform.
- All contact/stamp state clears within 500 ms; the existing modifier sequence still starts at 180 ms. The quality stamp occupies the track's upper bezel, away from the score readout. Reduced motion removes accepted-button movement, reflection/fleck travel, stamp scaling and housing recoil, retaining a stationary grade-distinct contact and label. Finish, skip, repeated receipt refresh and phase exit cannot leave or replay the effect.
- `LandingScoreRushSetup.Configure` retains the overlay and bindings through Landing regeneration. Card faces, artwork, scoring, attempts, overlap policy and terminal handoffs are unchanged.

## Verification

`Tools/Rendering/StrikeContactAudit.cs.txt` is an executable Unity MCP method body. In Play mode it creates an isolated instance of the actual Landing prefab with input blocked and voices muted; it never binds or commands the player's controller. Three real model strikes produce Perfect, Good and Miss receipts without assigning marker positions or bypassing accuracy classification. After receipt capture, the model is advanced to verify the contact still uses its original frozen position.

`Verification.json` records 88 passing checks: 56 focused checks plus 32 render-request checks from the optional motion export. Coverage includes serialized bindings, contact/audio deadlines, immutable position, unchanged aiming geometry and gameplay values, grade distinction beyond color, reduced-motion geometry, button hit-area preservation, pointer-release composition, repeated receipts, skip and phase exit. The expanded checks also catch quality-label drift during canvas layout, text overflow, input interception and reduced-motion scaling.

`Perfect.png`, `Good.png` and `Miss.png` are actual render-target captures of the isolated prefab sampled 25 ms after contact. `Preview.mp4` is a silent, cropped 30 fps export of the real Perfect effect followed by a quiet hold; its source frames are sampled by the same Unity runtime renderer into ignored `Temp/StrikeContactReview`. These are presentation captures, not a complete controller-driven encounter. The final Perfect frame and a four-timepoint motion contact sheet were visually inspected in the cabinet layout. Set `captureMotion = true` in the audit method body to regenerate its 32 frames; the default focused run performs 56 checks.

Existing Landing rules (64), score-sequence rules (20) and saved-prefab checks (40) also passed. Unity reported no errors after final compilation and the Play-mode audit. This is engineering/timing and visual-capture validation; no new human audio listening judgment is claimed.
