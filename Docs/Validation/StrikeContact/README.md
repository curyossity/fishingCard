# Game-feel phase 1: strike contact

22 November 2026. Revised and installed in the actual Landing prefab to follow the user's supplied ivory/brass ribbon and bright water-burst reference. The earlier narrow filament and small text stamp were visually insufficient. This treatment uses a larger readable result ribbon, cyan-white impact, water drops, bubbles and gold glints.

| Phase | Scope | Status |
| --- | --- | --- |
| 1 | Immediate strike contact and grade distinction | Implemented and validated |
| 2 | Weight individual score operations by their actual contribution | Implemented; see [modifier impact](../ModifierImpact/README.md) |
| 3 | Distinct climax when accumulated points cross Resistance | Proposed; not implemented |
| 4 | Clearer supporter-to-source-to-score choreography | Proposed; not implemented |
| 5 | Continuity from encounter to acquired card and clearer upgrade improvement | Proposed; not implemented |
| 6 | Consistent intensity hierarchy and quieter intervals | Proposed; not implemented |

## Installed behavior

- Every new accepted receipt presses the Strike button's visual face by four reference pixels immediately, including keyboard/submit-originated strikes. It holds for 45 ms and returns over 95 ms. Pointer release cannot erase a still-active accepted press; the button's hit area does not move.
- The receipt freezes the accepted marker center. A non-raycastable native mesh overlays the existing track mask at that position; the live marker and Good/Perfect bands are never altered.
- Good/Perfect contact meets the existing accuracy sound and base-score impact at 76 ms. Two light strokes compress toward the contact before impact. The clipped glass response is complemented by a cyan-white glow and a water burst outside the mask. Perfect has fourteen faceted drops, six rising bubbles and five gold glints; Good has seven drops, three bubbles and two glints. Miss retains a subdued slack release and no water burst. No sound file, mix gain, modifier timing or gameplay clock changes.
- The native result ribbon has a 630 by 110 reference-pixel ivory/brass plaque, folded tails, inset studs and dark dynamic text. It is centered between portrait and track; its water origin independently follows the frozen marker center. Good and Miss use 82% and 68% scale. Shared-stage local transforms remain correct while an enabled canvas initially has zero world scale.
- `PERFECT +20`, for example, changes to `PERFECT +32` only when an actual +12 operation arrives. The existing score owner supplies its delivered display value. No creature ability or score calculation is changed to reproduce the screenshot's illustrative numbers. The native text auto-fits long values.
- Housing recoil peaks at 5 reference pixels for Perfect, 2.5 for Good and a subdued opposite-direction 0.65 for Miss, with a short damped return. The score owner composes this with its existing housing response, avoiding two scripts writing the same transform.
- The ribbon holds and fades over 540/460/250 ms after contact for Perfect/Good/Miss. All contact state clears within 780 ms and is also bounded by the existing score sequence's completion. Modifier presentation still starts at 180 ms. Contributor portraits, names and the main score digits remain outside the ribbon. Reduced motion removes accepted-button movement, water/reflection travel, bright burst, ribbon scaling and housing recoil, retaining a stationary grade-distinct contact and ribbon. Finish, skip, repeated receipt refresh and phase exit cannot leave or replay the effect.
- `LandingScoreRushSetup.Configure` retains the overlay and bindings through Landing regeneration. Card faces, artwork, scoring, attempts, overlap policy and terminal handoffs are unchanged.

## Verification

`Tools/Rendering/StrikeContactAudit.cs.txt` is an executable Unity MCP method body. In Play mode it creates an isolated instance of the actual Landing prefab with input blocked and voices muted; it never binds or commands the player's controller. Three real model strikes produce Perfect, Good and Miss receipts without assigning marker positions or bypassing accuracy classification. After receipt capture, the model is advanced to verify the contact still uses its original frozen position. The optional populated preview acquires the current Sardine, Acorn Barnacle and Sea Urchin definitions into a separate `RigRuntime` and uses their actual level-one rules, haul and attempts. A separate supported-rule +12 fixture tests delivered ribbon text without assigning that illustrative ability to a species asset.

`Verification.json` records 105 passing checks: 65 focused checks plus 40 render-request checks from the optional motion export. Coverage includes serialized bindings, contact/audio deadlines, immutable position, unchanged aiming geometry and gameplay values, grade distinction beyond color, reduced-motion geometry, button hit-area preservation, pointer-release composition, repeated receipts, skip and phase exit. Expanded checks cover ribbon layout, text overflow, input interception, reduced-motion water removal and score delivery before/after a real bonus impact.

`Perfect.png`, `Good.png` and `Miss.png` are actual render-target captures sampled 25 ms after contact. `Splash.png` shows the populated Perfect effect later in its burst. `Preview.mp4` is a silent native-1920-by-1080, 30 fps export followed by a quiet hold; source frames are sampled by the same Unity runtime renderer into ignored `Temp/StrikeContactReview`. These are presentation captures, not a complete controller-driven encounter. The populated ribbon/burst and later water flight were visually inspected. Set `captureMotion = true` in the audit method body to regenerate its 40 frames; the default focused run performs 65 checks. [Visual provenance and native asset contract](VisualManifest.json) records the new procedural geometry.

Existing Landing rules (64), score-sequence rules (20) and saved-prefab checks (40) also passed. Unity reported no errors after final compilation and the Play-mode audit. This is engineering/timing and visual-capture validation; no new human audio listening judgment is claimed.
