---
name: team-audio
description: "Primary game-audio workflow for Deepwater: sonic identity, sound selection, SFX specifications, audio accessibility and Unity integration, run as an audio team (audio-director, sound-designer, accessibility-specialist, technical-artist, gameplay-programmer). Use for any game sound or music work, especially before choosing or generating sounds. audio-design supports it for mixing, buses, loudness and variation."
argument-hint: "[feature or area to design audio for] [--team individual|small|studio]"
user-invocable: true
---

# Team Audio (Deepwater)

Adapted from `team-audio` in [Donchitos/Claude-Code-Game-Studios](https://github.com/Donchitos/Claude-Code-Game-Studios)
(MIT, see `LICENSE`; upstream commit `abc0de79d6ceb0b5d7aa693945849b3937afbe6a`). The upstream
framework's config helper, hooks, `design/` and `production/` folders, ADRs and stories are not
installed here; this version uses Deepwater's own documents and rules.

If no argument is given, print the usage line above and stop.

## Deepwater ground rules (apply to every step and every agent)

- **Start from feeling, not from available files.** Begin with the player's intended feeling and
  the approved palette in `Docs/Sound Style.md` (the project's sound bible: the selected
  *Charged Cabinet* direction, cue vocabulary, timing and mix hierarchy). Asset availability or a
  passing signal test does not establish that a sound fits.
- **Keep the user's accepted and rejected examples.** Rejected directions recorded in
  `Docs/Sound Style.md` and `Docs/Validation/AudioDirection/` stay rejected unless the user reopens
  them. For a small adjustment, reuse the accepted direction instead of reopening the whole design.
- **No claimed hearing.** Claude cannot hear audio. Always distinguish: developer descriptions,
  signal measurements (peak/RMS/LUFS, PCM validity), timing/routing checks, model-assisted analysis
  (Earshot) and actual human listening. Never mark creative fit as validated because technical
  checks passed; say which checks actually ran.
- **Free local tools only.** Never supply, retrieve or forward an API key, even if one is available.
  No paid or hosted audio services, no Gemini critique, no audio uploads. Earshot runs only through
  `Tools/Audio/earshot_local.py` (see `Tools/Audio/README.md`).
- **The GDD owns mechanics.** Sound responds to accepted gameplay events; it never decides outcomes,
  consumes gameplay randomness or adds rewards. Do not invent mechanics to justify a cue.
- **Existing ownership** (see the audio paragraphs of `Docs/Architecture.md`): `DeepwaterSoundLibrary`
  (cue takes, category, gain, priority, cooldown, pitch growth), `DeepwaterSoundPlayer` (pooled
  DSP-scheduled voices), `GameAudioPreferences` (saved sound choice, `SoundChanged` cancellation),
  `TackleSlotAudio` (Discover), `DeepwaterAudioSetup` (manifest import and prefab bindings), and the
  bank built by `Tools/Audio/build_charged_audio.py`. Extend these; do not add a new audio system or
  middleware for a sample replacement.
- **Original or properly licensed material only.** Use reference games for principles, never their
  recordings or melodies. Record source, license, edits and import settings for every asset.
- Follow `AGENTS.md`, `CODING_STANDARDS.md` and the "no MVP shortcuts" policy for any code.

## Team size

Default **`individual`**: one sound-designer pass (or the main session applying all perspectives
for a small request). The user can ask for more with `--team`:

- `individual`: `sound-designer` only; other perspectives come through it.
- `small`: + `audio-director`, `technical-artist`, `gameplay-programmer`.
- `studio`: + `accessibility-specialist`.

Before spawning anything, state in one line which agents will actually run and which named roles
are folded into another agent. Never claim an independent specialist review that did not happen.
There is no Unity engine-specialist agent installed: the main session validates Unity integration
itself using the installed Unity skills (`unity-csharp-scripting`, `audio-design`, etc.) and says so.

## Pipeline

1. **Read the argument** (e.g. `landing impacts`, `build placement`, `discover reels`) and make a
   slug (`landing impacts` → `landing-impacts`).
2. **Gather context once** and distil it for the agents:
   - The GDD / brief sections and feature spec for the area (`Docs/Slot Machine Mechanic.md`,
     `Docs/Landing Challenge Mechanic.md`, `Docs/Rig Building Mechanic.md`, …), read by heading.
   - `Docs/Sound Style.md` and the relevant validation pages under `Docs/Validation/` (e.g.
     `ChargedCabinet/`, `SlotAudioSync/`, `ModifierImpact/`).
   - Existing cues and authoring: `Assets/` audio folders, `DeepwaterSoundLibrary` assets,
     `Tools/Audio/`.
3. **Delegate with briefs, not file dumps.** Spawn agents with the `Agent` tool
   (`subagent_type: audio-director`, `sound-designer`, `accessibility-specialist`,
   `technical-artist`, `gameplay-programmer`). Pass the lines each agent needs inline, plus the
   Deepwater ground rules above. End every prompt with this return contract:
   "Write your full output to `[path]`; that named path is your write authorisation. Return only
   (1) the path written, (2) at most 5 bullet decisions, (3) any BLOCKED/CONCERNS items, one line
   each." Working drafts go under `Docs/Audio/[slug]/`:

   | Step / agent | Draft path |
   |---|---|
   | 1 audio-director | `Docs/Audio/[slug]/direction.md` |
   | 2 sound-designer | `Docs/Audio/[slug]/sfx-spec.md` |
   | 2 accessibility-specialist | `Docs/Audio/[slug]/accessibility.md` |
   | 3 technical-artist | `Docs/Audio/[slug]/integration-plan.md` |
   | 4 gameplay-programmer | code/assets only after the user approves the file list |

   Agents may write their named draft without a separate prompt only when all hold: the path was
   named by you, it is a new file under `Docs/Audio/`, and the step ends in a user decision before
   the pipeline continues. Anything else (existing docs, code, assets, `Docs/Sound Style.md`) needs
   explicit approval.

### Step 1: Direction (audio-director)
Sonic identity for the area within the approved palette, emotional role in the loop, priority and
contrast between anticipation, action, resolution and celebration, mix targets, adaptive rules.
Ends with a user decision (`AskUserQuestion`: concise options, recommendation first).

### Step 2: Specification and accessibility (parallel)
`sound-designer`: per-event trigger and cancellation, timbre, attack/body/tail, duration,
pitch/variation, priority, concurrency/cooldown, gain and bus routing, and how related events form
one phrase. `accessibility-specialist`: gameplay-critical cues that need a visual equivalent,
sensitivity to repetition, sharp highs and sudden peaks, and the existing sound toggle and
reduced-motion behaviour. A gameplay-critical cue with no visual equivalent is **BLOCKING** until
the user resolves or accepts it. Report gaps without inventing new mechanics.

### Step 3: Integration plan (technical-artist; main session validates Unity)
Bus/mixer routing against the existing Interface/Motion/Score buses, import settings, memory,
streaming vs preload, DSP scheduling and cancellation, audio-reactive visuals. Retain mute
behaviour, cancellation and approved content outside the scope.

### Step 4: Implementation (gameplay-programmer)
Only after the user approves the file list. Wire cues to accepted events through the existing
owners, keep `DeepwaterAudioSetup` bindings intact on regeneration, and add or extend the
relevant `*Checks` validation.

### Step 5: Audition and verification
When the direction is unresolved, make a short, complete interaction preview reviewable before a
broad replacement, comparing alternatives at similar perceived loudness. Use Earshot (local) for
generation and analysis where useful. Report which checks ran: measurements, timing/routing,
model analysis, and whether a human has listened yet.

## Output

Compile the drafts into `Docs/Audio/[slug].md` after asking "May I write the audio design to
`Docs/Audio/[slug].md`?". Propose changes to `Docs/Sound Style.md` or `Docs/Architecture.md` as
explicit diffs for approval; never edit them silently. If `Docs/Audio/` is new, suggest listing
it in `Docs/README.md`.

Finish with: event count, estimated asset count, implementation tasks, open questions between
roles, which checks ran, and a verdict: **COMPLETE**, **COMPLETE — [named check] NOT ASSESSED**, or
**BLOCKED — [reason]**.

## Error recovery

If an agent's contract named a path, confirm the file exists before treating the step as done; a
fluent reply with no file is a failed step. If an agent is blocked or fails, surface it
immediately, do not continue past what it blocks, and produce a partial report with the gap named.
Never fill a missing agent's section with your own content while presenting it as theirs.

For the bus, loudness, ducking and variation technique itself, use the `audio-design` skill.
For a sound-bible structure when a new area has none, `references/sound-bible-template.md` can be
used selectively; its example numbers, mechanics and paths are not project requirements.
