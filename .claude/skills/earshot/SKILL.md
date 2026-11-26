---
name: earshot
description: Generate, measure, classify and compare sound files with the free local Earshot tools and CPU listening models. Use for evidence about audio an agent cannot hear. Supports team-audio as the primary creative workflow; never use hosted critique, audio uploads or API keys.
---

# Earshot — free local audio tools

Keep `team-audio` primary for sonic direction and use `audio-design` for technical
mixing guidance. Earshot provides sound generation and model-assisted analysis;
it does not establish taste, emotional fit or licensing.

## Deepwater installation

In `C:/Data/Projects/Unity/fishingCards`, use this guarded launcher for every call:

```powershell
& .local-tools/earshot/venv/Scripts/python.exe Tools/Audio/earshot_local.py doctor
& .local-tools/earshot/venv/Scripts/python.exe Tools/Audio/earshot_local.py make "a soft short click" -o Temp/click.wav
& .local-tools/earshot/venv/Scripts/python.exe Tools/Audio/earshot_local.py describe C:/absolute/path/click.wav
```

Use the equivalent `deepwater-earshot-local` MCP tools when that server is connected. It
runs the same guarded launcher; its hosted critique tool is removed.

The user requires free local tools only. Never retrieve, supply or forward an API
key, including existing keys. Never enable `llm`, `llm_critique`, `--yes-upload`,
Gemini or other hosted audio services. Do not bypass the guard with the raw binary
or module. The launcher strips inherited credentials and normal operation is offline.
Only explicit `doctor --fetch` allows public unauthenticated model downloads.

The [upstream skill](references/upstream-SKILL.md) describes the full command set;
its optional cloud/key instructions do not apply. Do not install `[all]` or `[llm]`.
The upstream skill is retained with its [license](LICENSE) and
[revision](references/provenance.json).

## Working with audio

- Generate candidates with `make`/`make_set`, inspect the selected sound ID and
  any mismatch notices, and use `build`/recipes when a catalogue item is unsuitable.
  Avoid passing an approximate catalogue result off as the requested sound.
- Compare candidate files with `rank` or `ab`; inspect the winning excerpt with
  `find`, `describe` and `measure`. Describe the audible property being sought,
  rather than treating a product name or mood word as an objective sound class.
- Review clips with `timeline` and `sheet`. Supply actual animation cue times for
  synchronization; visual cuts alone do not identify every game event. Use game
  references to analyse principles, not to copy recordings or melodies.
- Report model classifications and comparisons as model-assisted evidence. State
  uncertainty for very short cues, layered mixes and mood. Read the
  [documented limits](references/LIMITS.md) before relying on a verdict.
- Distinguish model inference, measured loudness/timing, and actual human listening.
  A strong match or clean PCM does not prove that a sound feels satisfying.
- Follow project instructions for paths, source licensing, import settings and
  keeping the game's rules separate from audio presentation. Analysis does not
  grant permission to replace an unresolved sound direction.
