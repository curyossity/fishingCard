# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project rules (read first)

`AGENTS.md` is the binding instruction file for this repository; follow it fully. In short:

- **Deepwater** (working title) is a Unity 2D fishing card roguelike. Unity **6000.5.9f1**, URP, uGUI + TextMesh Pro, Input System.
- Before changing code, assets, prefabs, scenes or architecture, read `CODING_STANDARDS.md`, `Docs/Architecture.md`, `Docs/Card Visual Specification.md`, `Docs/GDD_IMPLEMENTATION_BRIEF.md`, `Docs/Full Game Development Guidelines.md`, and consult the full GDD (`Docs/Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md`) for any mechanic the brief does not settle.
- The GDD is the source of truth for gameplay. Do not invent mechanics, card roles or progression rules; open decisions and illustrative abilities are not final. Existing code/serialized content does not establish redesigned rules.
- Not MVP mode: no "good enough for now" shortcuts. Ship complete vertical increments; flag any unavoidable new debt to the user explicitly.
- `Docs/README.md` separates active specs from historical records. `Docs/Architecture Implementation Reference.md`, `UI Presentation Reference.md`, `Card Presentation Reference.md`, `MVP Workplan.md`, `MVP Technical Debt.md` and everything under `Docs/Validation/` are historical/evidence, not requirements. Never rewrite past validation evidence to claim the redesigned game passed.
- Update `Docs/Architecture.md` when responsibilities change.
- Code style: serialized private fields over public mutable fields; `/// <summary>` on functions you add or change; comments explain long-lived gameplay intent, never roadmap/phase history.
- Audio work: `team-audio` skill primary (project copy in `.claude/skills/team-audio/`, with its agents in `.claude/agents/`), `audio-design` supporting; free local tools only, never supply/forward API keys, Earshot only via `Tools/Audio/earshot_local.py` (see `Tools/Audio/README.md`).

Feature specs to read when touching that area: `Docs/Slot Machine Mechanic.md` + `Slot Machine Runtime.md` (Discover), `Landing Challenge Mechanic.md`, `Rig Building Mechanic.md`, `Creature Ability Design.md`, `UI Redesign Plan.md`, `Sound Style.md`, `Equipment Passive Rules.md`, `Technique Card Definitions.yaml`, `Event Rules Implementation Tasks.md`. Artwork sources and provenance live under `Docs/UIThemes/`.

## Scenes and persistence

- `Assets/Scenes/SampleScene.unity` is the only scene in Build Settings. The current Harbor → Tackle Apparatus experience (and all live validation) runs in `Assets/UIThemes/MidnightFieldJournal/Scenes/MidnightFieldJournalComparison.unity`, which hosts `JournalHarborFlow`. Both contain a `FishingRunController`.
- Saves use `PlayerPrefs`: permanent progression as JSON under `FishingRunController.DefaultProgressionSaveKey` (`FishingCards.PlayerProgression.v1`, written transactionally with rollback), Discovery Journal progress in `DiscoveryJournalTracker`, and the sound toggle in `GameAudioPreferences`. The temporary rig is not meant to carry over between expeditions.
- When testing in Play mode, call `FishingRunController.SetDebugSaveResetPending(true)` (as the live checks do) or use **Fishing Cards → Debug → Game Saves** so tests don't corrupt the real save.

## Core loop and architecture

Expedition loop: **Discover → Land → Build/Upgrade**, starting in Shallows with an empty rig; the rig is temporary and the full accumulated haul is paid on both failure and success.

Core runtime code is in `Assets/Scripts/`, editor tooling in `Assets/Editor/` (no asmdefs; everything compiles into `Assembly-CSharp`/`-Editor`). The Harbor flow, Journal views and their editor checks live under `Assets/UIThemes/MidnightFieldJournal/` (`Runtime/`, `Harbor/`, `Editor/`). Key split (see `Docs/Architecture.md` ownership table):

- **Gameplay owners decide; views render.** `FishingRunController` (partial classes: `.Landing`, `.Rig`, `.SlotMachine`) owns phases, commands, settlement and gates such as `LandingFeedbackPending`. Pure runtimes own rules: `EncounterRuntime`/`TagSlotMachineRuntime` + `SlotCreatureSelector` (Discover), `LandingChallengeRuntime` + `LandingScoring` (Land), `RigRuntime` (Build — transactional previews on candidate snapshots, revisions, legal insertion, upgrades, sale value).
- Views forward commands bound to an occurrence/revision (e.g. `TryStrikeLanding(occurrence, revision)`, `TryCommitRigPlacement(occurrence, rigRevision, gap)`); stale commands are rejected. Views never compute score, placement legality or rewards.
- **Content**: `CardDefinition` / `CreatureAbilityDefinition` ScriptableObjects own species identity, exactly one tag (Schooling/Predator/Anchored/Armored/Elusive), one primary ability, tuning and level progression. Do not duplicate the roster in Markdown. `CardInstance` is runtime member state. Effect families need explicit handlers (`EffectResolver`, `CreatureEffectRuntime`, `IEventEffectHandler`).
- **Presentation** is "Tackle Apparatus": three phase canvases (Discover, Landing, Build) sharing one outer machine shell (`TackleMachineGeometry.StageBounds`). Presentation classes (`LandingScoreRush`, `LandingCardCollision`, `LandingImpactCamera`, `RigBuildPresentation`, `TackleReelAnimator`, …) run on unscaled/mapped clocks, must respect the reduced-motion and sound preferences (`GameAudioPreferences`), must restore every transform they own on skip/disable/phase exit, and must never consume gameplay randomness or change Unity time scale.
- Creature/catch card faces follow `Docs/Card Visual Specification.md` (supplied art vs. native dynamic fields).
- Audio: `DeepwaterSoundLibrary` (cue data) + `DeepwaterSoundPlayer` (pooled DSP-scheduled voices); asset bank built by `Tools/Audio/build_charged_audio.py`.

## Editor authoring ("Setup") pattern

Prefabs and scene bindings are generated, not hand-edited. Each feature has an idempotent `*Setup` editor class (e.g. `LandingScoreRushSetup`, `LandingCardCollisionSetup`, `RigBuildingSetup`, `TackleDiscoverArtSetup`, `CreatureSpriteSetup`) exposed under the **Fishing Cards/** menu. Setups update prefabs in place, preserve GUIDs and existing bindings, and are chained so a full phase regeneration re-applies nested setups. When adding a presentation layer, add/extend its Setup and make sure the parent regeneration path retains it; don't rely on manual Inspector wiring.

## Validation (no Unity Test Framework tests)

Validation is done with static editor classes named `*Checks.cs` (in `Assets/Editor/` and `Assets/UIThemes/MidnightFieldJournal/**/Editor/`), run from the **Fishing Cards/Validation/** menu, via Unity MCP, or in batch mode:

```powershell
& "C:/Program Files/Unity/Hub/Editor/6000.5.9f1/Editor/Unity.exe" -batchmode -projectPath . -executeMethod <Class>.<Method> -logFile Logs/<name>.log
```

- Edit-mode checks are methods like `ValidateRules()` / `ValidateTo(...)` returning assertion counts.
- Live checks use a `Begin()` entry point that requires a fresh Play session of the comparison scene (`Assets/UIThemes/MidnightFieldJournal/Scenes/MidnightFieldJournalComparison.unity`, which hosts `JournalHarborFlow`), drives real controller commands with persistence disabled (`SetDebugSaveResetPending(true)`), and writes JSON results to `Docs/Validation/<Feature>/`.
- Each feature's `Docs/Validation/<Feature>/README.md` documents its coverage and captures. Report which checks you ran and which you could not.
- Rendering/audio audit snippets live as `.cs.txt` files in `Tools/Rendering/` and `Tools/Audio/` (copied into the project when needed, not compiled by default).

## Tooling

- Unity's official skills (`create-ui`, `optimize-audio`, `urp-postprocessing`, …) assume the Unity CLI and `com.unity.pipeline`, which this project does not use. Perform their Editor steps (compile, console, Play mode, C# eval, screenshots) through Unity MCP instead.
- **Unity MCP** (`com.coplaydev.unity-mcp`, server at `http://127.0.0.1:8080/mcp`, instance `fishingCards`) auto-connects when the Editor is open; reconnect via **Fishing Cards → Tools → Connect Unity MCP**. See `Docs/Unity MCP Setup.md`.
- `Tools/PixelFX/` holds water-impact effect recipes/exports (Node `.mjs`) consumed by `LandingWaterImpactSetup`. The `pixelfx` and `earshot` project skills (`.claude/skills/`) drive the local PixelFX library and Earshot audio tools under `.local-tools/`.
- Earshot's MCP server `deepwater-earshot-local` is registered in `.mcp.json` and runs only through the guarded `Tools/Audio/earshot_local.py mcp` launcher (19 offline tools, paid hosted critique removed). Never call Earshot directly or enable its online features.
- Python audio scripts in `Tools/Audio/` run with the local venv: `& .local-tools/earshot/venv/Scripts/python.exe Tools/Audio/<script>.py`.
