# Game-feel phase 2: modifier impact

22 November 2026. Implemented in the actual Landing presentation. Phase 1's ivory/brass ribbon and water burst remain installed. Phase 3, a distinct Resistance-crossing climax, is next; supporter-chain choreography, acquisition continuity and the broader intensity pass remain proposed.

## Behavior

Each frozen scoring step records its previous value, delivered gain and a bounded presentation strength. Strength blends relative growth with gained points compared with the accuracy base. A tiny multiplier stays restrained; the same factor acting on a larger subtotal can have a more substantial response. This calculation never evaluates abilities or changes score arithmetic.

- Small additions have short preparation, a compact single trail and a restrained number impact. Later additions accelerate slightly.
- Major additions receive more preparation, a thicker accelerating comet and a stronger score-glass/number response.
- Substantial multipliers compress their source portrait before release, enlarge the visible operator during preparation and use paired mint trail filaments around the core. The number remains at the previous delivered value until impact.
- Trail size, number/operator response, glass accent and small housing contact follow the relevant operation's strength. Final payout still uses the existing whole-strike intensity. No additional result banners, points, rewards or gameplay triggers were added.
- Number and operator pivots are centered. The numeric text reserves expansion room, preventing six-digit impact values from extending outside the glass. Source/portrait transforms, operation scale, particles and voices clear on skip or phase exit. Reduced motion retains exact numerical delivery with no charge, travel, recoil or scale changes.
- Long rigs retain every operation and grouped source identity. Time above a 190 ms per-beat floor compresses into a 3.25-second modifier budget; the complete sequence remains below 4.8 seconds. Accuracy contact remains at 76 ms and the first modifier starts at 180 ms.
- The accepted Charged Cabinet WAVs, gains, pitch-growth settings and mixer are unchanged. Launch cues fit flight; operation cues follow the revised exact impact deadlines through the existing frozen sound-event list.

## Review

Open [Compare.html](Compare.html) for native small/major scoring clips. Each clip includes audio mixed from the runtime's frozen cue list. The comparison replay button mutes both clips to permit visual comparison without two overlapping mixes.

| Current-definition fixture | Strike points | Sequence duration | Last multiplier gain | Last multiplier strength |
| --- | ---: | ---: | ---: | ---: |
| Level 1 | 50.7 | 1.665 s | 16.9 | 0.161 |
| Selected members upgraded to Level 30 | 1,016.4 | 2.418 s | 785.4 | 0.811 |

The fixtures acquire current Sardine, Glass Minnow, Mackerel, Cuttlefish and Stormjaw Tuna definitions into independent rigs. In the upgraded case, Sardine, Cuttlefish and Stormjaw Tuna receive duplicate copies through `RigRuntime`; other members stay at Level 1. Species authoring, unlocks, gameplay rules and player saves are not changed. These are visual contrast fixtures, not balance targets.

`MajorFlight.png` and `MajorImpact.png` are actual native-resolution frames, visually inspected for source readability, projectile distinction and containment of the enlarged score. Source frames are kept under ignored `Temp/ModifierImpactReview`; only selected captures and review clips are retained here.

## Validation

- `LandingScoreRushChecks.ValidateRules`: 32 checks, including twelve new checks for small/major hierarchy, equal-factor/different-gain behavior, exact impact delivery, phase-one timing, mixed long rigs, decimal precision and readable bounded pacing.
- Existing saved-prefab checks: 40. Existing Landing/model checks: 64.
- `Tools/Rendering/ModifierImpactAudit.cs.txt`: 39 small-rig and 41 upgraded-rig checks. These cover actual prefab sampling, readable operators, pre-impact values, exact impact values, source compression, stronger number responses, launch/impact cue alignment, reduced motion, skip, active-modifier phase exit, centered pivots and score-glass bounds. The invisible test canvas blocks input, mutes voices and issues no player-controller command.
- `StrikeRegression.json`: 65 passing phase-one regression checks, exported into a separate temporary folder so accepted phase-one captures were preserved.
- Unity Console: no errors after compilation and validation. Project GUID/meta validation passed.

The short authored WAV exports measured -28.9 / -28.0 LUFS and -9.0 / -6.4 dBTP for small/upgraded fixtures using local FFmpeg EBU R128 measurement. These measurements validate export headroom, not human listening, device output latency or a complete in-game mix. The clips use the current runtime event list; their audio is not a loopback recording.

To reproduce, enter Play mode and execute the audit file as a Unity MCP method body. Set `previewLevel` to 1 or 30; `exportFrames` controls the optional 30 fps frame/audio export. `Level-1.json` and `Level-30.json` retain actual score, duration, per-operation gain, strength and deadlines. The user's live run and sound preferences remain untouched.
