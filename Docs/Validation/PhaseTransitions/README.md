# Tackle phase transitions — validation

27 November 2026. Presentation-only change between the three Tackle Apparatus phases (Discover ↔ Landing ↔ Build). No phase, command, score or reward waits on it. Harbor entry and expedition endings still cut directly.

## Behavior

- Just before the phase views switch, `FishingRunView.PreparePhaseChange` asks each view whether it will show (`WouldShow`, the same expression its `Present` uses). When the visible phase is about to change, `TacklePhaseTransition` renders the outgoing canvas once into a screen-sized texture.
- The shutter covers the machine body with a clean, regular outline measured from the shared housing artwork (`landing-machine.png`; all three phases use pixel-identical outer shells): the cabinet's straight left edge, its innermost straight right edge, its main top line and bottom, with chamfers matching its cut corners (texture 120–1652 × 31–887, 46 px top and 14 px bottom chamfers; stage 580,186 1154×645). The pipes, side knobs, the small raised top sections and the right-side bulges stay outside it as hardware. An exact alpha trace was tried first and rejected: following every bulge made the closed shutter look like a cut-out rather than a manufactured part. The brass cylinders on the cabinet face are covered.
- `TackleShutterGraphic` draws slatted iron (native geometry, no new artwork) row by row inside those spans, with a dark rim and brass trim along its edge, and a strong inward darkening at both sides (44 units, 62% at the edge) so the slats seem to turn away into side channels and the shutter sits in the machine rather than on it and a brass bottom bar with a handle. It drops from the machine's own top edge with gravity (0.18 s), rebounds slightly and jolts the held frame by a few pixels on landing (0.08 s), then rolls back up (0.22 s): 0.48 s in total on the unscaled clock.
- The strip is bent over an imaginary drum behind the top of the cabinet: slats near the top are foreshortened and darker, so the shutter appears to come over from behind the machine. Slats keep fixed wear marks as they travel.
- The held outgoing frame (including the labels outside the window) is removed only while the window is fully covered, so the header and rig column swap together with the landing, and the new interior is first seen as the shutter rises. Reduced motion (the shared Landing preference) uses a plain 0.2 s crossfade.
- A transparent blocker stops pointer input while the overlay is up. Landing's creature entrance starts after the transition: `BeginSelectedLanding` offsets `LandingRevealStartedAt` by `FishingRunView.PhaseTransitionLead`, and the reveal gate already holds motion and strikes until then.
- The overlay is its own screen-space canvas (sorting order 450): above Discover (70), Build (85) and Landing (400), below Harbor tag preparation (500). It is disabled when idle.

## Ownership

`TacklePhaseTransition` (snapshot, shutter timing, editor-only `Seek`/`Release` for review captures), `TackleShutterGraphic` (slats, drum curve, bottom bar), `FishingRunView.PreparePhaseChange` / `PhaseTransitionLead` (trigger and lead), `SlotMachineView.WouldShow`, `LandingChallengeView.WouldShow`, `RigBuildingView.WouldShow` (shared visibility rules), `TacklePhaseTransitionSetup` (prefab `PhaseTransitionCanvas` and scene binding, retained by `SlotMachineSetup`'s scene regeneration). See [Architecture](../../Architecture.md).

## Verification

- **18 asset assertions** (`TacklePhaseTransitionChecks.ValidateAssets`): every binding present, overlay disabled at rest, sorting order between Landing and Harbor preparation, clipped window, only the blocker intercepts input, bounded durations, body cover inside the shared shell with an outline, shutter rolled up at both ends and fully closed at the swap, jolt only around the landing, continuous and invertible drum mapping.
- [Live](LiveVerification.json): **15 assertions** in a fresh comparison Play session with persistence disabled, through real commands: no transition on Harbor entry; Discover → Landing plays one, the creature entrance waits behind it and the overlay clears; a real Perfect landing opens Build with a transition; placement feedback hands back to Discover with a transition; the overlay is never left on.
- Previews: [comparison page](index.html) with normal and quarter-speed videos of all three changes, plus five stills each (`*-1.png` … `*-5.png`). Frames were captured by seeking the transition to exact ages, because a 4K screen capture takes longer than the whole transition.

Not yet done: transition sounds (a rattling roll and a clunk as the shutter lands) are a `team-audio` decision. Not yet verified: human play-feel and timing in normal play.

History: the first version (an interior that folded down while labels crossfaded) was replaced by a shutter inside the machine window, then by an exact-silhouette body shutter, then by this regular-outline body shutter, each at the user's request. Its window measurement fix is retained: the window uses screen-normalized anchors measured before the snapshot.
