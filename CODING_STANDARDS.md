# Coding Standards

Read this file before making code, asset, prefab, scene, or architecture changes in this repository.

## Project Context

- This project is a 2D card game built in Unity.
- Code and implementation choices should fit Unity conventions and the needs of a 2D game.
- Prefer Unity 2D systems, components, physics, rendering, and assets unless there is a clear reason to use a 3D equivalent.

## Unity 2D Guidelines

- Use 2D components such as `SpriteRenderer`, `Rigidbody2D`, `Collider2D`, `Physics2D`, `SortingGroup`, and UI components where appropriate.
- Avoid 3D-specific components, physics, cameras, or workflows unless explicitly requested.
- Keep card visuals, board elements, and interactable objects compatible with sprite-based 2D rendering.
- Consider sorting layers, order in layer, canvas mode, and pixel-perfect presentation when changing visual behavior.

## Code Style

- Follow existing project patterns before introducing new architecture.
- Keep scripts focused, readable, and Unity-friendly.
- Prefer serialized fields over public mutable fields for Inspector configuration.
- Use clear names that describe gameplay intent.
- Integrate concise implementation comments while coding when they clarify gameplay intent, Unity wiring expectations, data flow, or non-obvious rule logic.
- Add XML documentation comments (`/// <summary>`) for functions as they are created or changed, so their purpose is understandable from code navigation and IntelliSense.
- Keep code comments focused on long-lived intent and behavior; avoid mentioning roadmap phases, workplan checkpoints, sprint context, or temporary implementation history unless the code specifically depends on that context.
- Avoid comments that only restate the code; comments should help future implementation and tuning decisions.
- Keep MonoBehaviour lifecycle methods simple and avoid hiding complex game logic inside `Update` unless frame-by-frame behavior is required.

## Gameplay Architecture

- Keep card-game logic separate from presentation where practical.
- Let visual scripts handle animation, input feedback, sprites, and scene objects.
- Let gameplay/model scripts handle rules, card state, deck state, turn flow, scoring, and validation.
- Avoid tightly coupling game rules to specific scene objects unless the behavior is intentionally scene-specific.

## Before Finishing Changes

- Check that new code is appropriate for a Unity 2D card game.
- Verify that 2D assets, prefabs, and components are used consistently.
- Run relevant tests or Unity-safe validation steps when available.
- Mention any validation that could not be run.
