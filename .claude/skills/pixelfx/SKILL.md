---
name: pixelfx
description: Find, customize, render and visually review pixel-art VFX with the local PixelFX library, then export transparent sprite sheets for Unity or other engines. Use for pixel-art water, splashes, projectiles, impacts and UI effect assets. Complements game-feel for timing and choreography.
license: MIT; retain the upstream notices. The gallery font has a separate license.
---

# PixelFX (Deepwater)

Create and evaluate pixel-art effect assets with PixelFX's local Node tools. This is a Deepwater adaptation of [patch-gg/pixelfx](https://github.com/patch-gg/pixelfx), not an AI model or a hosted generation service. The original instructions are preserved in [references/upstream-skill.md](references/upstream-skill.md).

## Local installation

- Library: `C:/Data/Projects/Unity/fishingCards/.local-tools/pixelfx`
- Installation/version record: `C:/Data/Projects/Unity/fishingCards/.local-tools/pixelfx-install.json`
- Node 18+ is required; the installed Node 22 is sufficient. Rendering and sprite-sheet export require no npm install.
- Optional video previews use the existing free FFmpeg at `C:/Data/Projects/Unity/fishingCards/.local-tools/earshot/ffmpeg/ffmpeg.exe`. Add that directory to the current child process's PATH when exporting MP4/GIF; do not change the system PATH.

Prefer this installed checkout. In a different workspace, locate an existing PixelFX installation first and use a project-owned output directory. Do not clone duplicate copies merely because the game's root lacks `tools/render.mjs`.

## Find, render and review

Run commands with the library as the working directory:

```text
node tools/list.mjs water
node tools/render.mjs water/bubbles --sheet --mp4 --no-actor --seed 4
node tools/check.mjs water/bubbles
```

Exports default to `records/<category>/<effect>/` in the library. `--out <directory>` selects another output root. The renderer produces `contact.png`, optional motion previews, and a transparent PNG sheet with JSON frame, timing and pivot metadata.

Read the selected effect's `meta` and implementation before running a custom recipe. Match the palette, intended pixel scale, shape and duration to the game. Open the generated contact sheet with the available image-viewing tool and inspect motion frames or the MP4 before claiming visual quality. Check the exported alpha, frame count and timing when preparing an engine import.

For a new effect, read the installed library's `docs/AUTHORING.md` and the closest two or three recipes. State the visual intention, draft, render, inspect and refine. Use seeded variation and verify that loops join cleanly and one-shots end empty with `tools/check.mjs`. Preserve custom recipes and selected exports in the project's maintained authoring/assets locations when integrating; the ignored local tool checkout alone is not durable game content.

## Unity and Deepwater

PixelFX is JavaScript authoring software, not a Unity package. Export transparent sheets and use their frame/pivot metadata for Unity slicing and playback; verify the importer rather than assuming an Aseprite-style JSON imports automatically. Preserve crisp pixel sampling where appropriate and inspect the result against the actual cabinet, including pale artwork and dark glass.

Keep score numbers, creature identities, operators and gameplay triggers native and authoritative. Use the existing Landing clock for effects, impacts and sound. For Deepwater, follow `AGENTS.md`, the visual specification and supplied-art boundary; use PixelFX for external effect art. `--no-actor` removes preview actors, but recipe scenery such as a stone basin or seabed may still need an authored variant. Do not bake sample combo text or bonus values into gameplay artwork.

Use game-feel for orchestration, create-game-assets for cohesive asset integration, and team-audio for sound. PixelFX does not replace those responsibilities. Retain reduced-motion and skip/exit cleanup when an exported effect is integrated.

## Free local operation and provenance

Use local Node rendering and the existing free FFmpeg. Do not retrieve, request or forward API keys. Do not add hosted generation, paid services, Patch CLI, account setup or publishing to an effect-authoring task. Treat upstream publishing instructions as a separate workflow requiring an explicit user request.

Retain [LICENSE](LICENSE) and [NOTICE.md](NOTICE.md) with copied/adapted code and effects, including their upstream credits. The `gallery/fonts/PatchPixel-Regular.woff2` font is separately reserved and must not be bundled into the game under the MIT license. These rights boundaries do not restrict use of the repository's MIT effect recipes.
