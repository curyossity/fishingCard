# Tackle Apparatus — Discover artwork

Approved source: `Docs/UIThemes/TackleApparatusConcept/AssetLibrary`. The 37 Common/Discover exports are copied byte-for-byte at their initial dimensions. `SourceManifest.json` retains the original SHA-256 hashes, Penpot identities and border metadata. Original canvas: 1920 × 1080. No artwork has been regenerated or upscaled.

The licensed Press Start 2P and VT323 font sources and OFL licenses are under `Fonts/`. Functional labels are separate TextMeshPro objects; exported sprites contain no runtime names, roles, scores or button labels. Sprite import uses Single/Full Rect, PPU 100, Point filtering, Clamp, no mipmaps, no Read/Write, uncompressed textures and the manifest's left/bottom/right/top borders.

`TackleDiscoverArtSetup` builds GUID-stable Discover and reusable rig-card prefabs. **Fishing Cards > Slot Machine > Integrate Approved Discover Art** installs them in the saved comparison scene. Subsequent comparison-scene and Harbor regeneration retain that binding. Harbor preparation and the other phase presentations remain separate.

The full-screen canvas uses reference resolution 1920 × 1080 and Expand scaling, preserving the centered 16:9 composition with letterboxing on other aspect ratios. The sprite shell, reel faces, levers, lamps, buttons, portraits and native labels remain independent. Buttons have rectangular hit areas, separate focus corners, a four-pixel pressed offset and disabled dimming.

State comes from the existing slot runtime: exactly one lock, real matching outcomes, current biome, current rig order and resolved haul value. New runs display an empty cabinet and zero haul. The illustrative three creatures and haul 12 are not inserted into gameplay. The Discover canvas hides at the landing handoff.

The supplied package contains illustrated Fish and Schooling reel symbols and four creature portraits. Other tags use native text in the symbol area; other creature portraits remain empty while the card's actual name, role and resolved value are shown. Add approved sprites through the serialized tag/portrait mappings without altering selection rules. No unrelated theme artwork or invented creature illustration is substituted.

Installed and visually verified in the comparison scene on 7 November 2026. Asset checks, live acceptance/respin controls, real UI raycasts, focus/press/disabled feedback and an eight-card scrolling fixture passed. Native 1080p, 1280 × 800 and ultrawide captures and exact check counts are under `Docs/Validation/TackleDiscoverArt/`.
