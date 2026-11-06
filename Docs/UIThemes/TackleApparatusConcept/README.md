# Tackle Apparatus concept

6 November 2026. The user approved the Tackle Apparatus Discover, Landing and Rig Placement concepts' visual direction and requested a reusable UI asset library for Unity. This approves the art direction for continued design work, not implementation or provisional mechanics. The Coastal Print direction remains rejected. Existing game code and runtime artwork assets are unchanged.

The [reusable UI kit](AssetLibrary/README.md) separates shared and phase-specific artwork, composed Penpot components, live labels, fonts and sprite metadata. Its working screen reference is the original **1920 × 1080**, with initial sprite dimensions restored. Higher-resolution work is deferred at the user's request. Source files are preserved for a possible later change. The original concept screens below remain preserved.

[Open in Penpot](https://design.penpot.app/?screen=workspace&team-id=9e056cfe-f83f-81f8-8008-cc4e858053b7&file-id=3681d377-bc58-8110-8008-e7a38adc2112&page-id=38e9e9ec-a6d5-8050-8008-e7a689a6b1e6).

Page: **02 - Tackle Apparatus - Discover**. Board: `38e9e9ec-a6d5-8050-8008-e7a710a9475c`. [Finished preview](Discover.png), 1920 × 1080. The containing file still has its earlier Coastal Print name; this revision is on its own page.

## Reference study

The actual official gameplay screenshots were downloaded and visually inspected before composition. These observations are art-direction interpretations:

- [Balatro](https://store.steampowered.com/app/2379780/Balatro/): tactile cards, distinct silhouettes, concentrated score information and strong readable type.
- [Slots & Daggers](https://store.steampowered.com/app/3631290/Slots__Daggers/): the machine, lettering, sprites and surrounding props share one material language. Wear and lighting carry atmosphere.
- [Hull Rupture](https://store.steampowered.com/app/4358660/Hull_Rupture/): confident pixel clusters, detailed surroundings and functional frames integrated into the world.
- [Slotbound](https://store.steampowered.com/app/4459590/Slotbound/): coherent pixel treatment across symbols, illustrated pieces and surrounding controls.

The resulting direction uses dark maritime enamel, aged ivory, brass clips, physical reel drums, pixel fish, chipped paint and warm peripheral light. It is a 2D illustrated apparatus, not an explorable environment. No third-party game art is placed in Penpot; the downloaded screenshots were research inputs only.

## Scope and provenance

The Discover screen contains three tag reels, a Schooling pair, lock controls, respin, accepting tags, the committed rig and accumulated haul. The game title is absent. Landing and post-success Catch/Bait placement are separate phases. Sale values 7 + 0 + 5 total 12; Bait is explicitly zero. The shown rig is an example, not a three-creature cap. Exact reel rules, lock/respin behavior and displayed values remain provisional under the GDD.

[ArtPlate.png](ArtPlate.png) was generated with the built-in image generation tool using the four public screenshots as visual references and the preserved [prompt](GenerationPrompt.txt). Penpot contains the raster plate plus editable native text and letter shadows. Fish, frames and props within the plate are not individually editable components. This is concept art for review, not a claim of manually drawn production sprites, a pixel-perfect sprite sheet or a playable interface.

The prior clean-vector proposal missed the dense, tactile visual character requested. This revision explores that character directly rather than changing only the palette.

## Verification

The final Penpot export was visually inspected. All 68 descendants remain inside the 1920 × 1080 frame; second-reel lock lettering was moved clear of the latch. Native text contains no game title, landing score, Resistance, Surface or capacity labels. Notes sit outside the gameplay board. No Unity implementation or responsiveness testing applies to this static art study.

## Matching Landing Challenge screen

[Open Landing in Penpot](https://design.penpot.app/?screen=workspace&team-id=9e056cfe-f83f-81f8-8008-cc4e858053b7&file-id=3681d377-bc58-8110-8008-e7a38adc2112&page-id=38e9e9ec-a6d5-8050-8008-e7a8487ae841).

Page: **03 - Tackle Apparatus - Landing**. Board: `38e9e9ec-a6d5-8050-8008-e7a8d44eb2db`. [Finished preview](Landing.png), 1920 × 1080.

The matching cabinet keeps the rig, sale values, haul counter, materials, props and lighting. The central reels and locks are replaced with a Garfish portrait, Landing Points against Resistance, point progress, a horizontal timing track, remaining-attempt lamps, rig contribution, last-strike feedback and one Strike action. Motion arrows distinguish the moving fish from the moving zone. This screen has no game title or slot-machine controls. Catch/Bait choice follows successful landing and is not offered here; the encountered Garfish is not attached and does not supply its own landing bonus.

The depicted state uses 36 / 80 points, two remaining attempts from an illustrative four, Good = 12 accuracy + 6 rig, and Space input. These remain prototype choices. The +6 example refers to the existing rig and the GDD's provisional Mackerel Schooling concept; exact creature tags, values, movement, ability content and controls still require design review. Haul remains 12 and Bait remains zero. The concept grants no rearrangement, Release or voluntary surfacing control.

[LandingArtPlate.png](LandingArtPlate.png) was produced by editing the approved Discover art plate through the built-in image generation tool, using the preserved [landing prompt](LandingGenerationPrompt.txt). Labels, scoring segments and motion arrows are native Penpot elements. The raster art is a concept reference rather than individually editable production sprites or a validated pixel grid.

Validation: the exported landing screen was visually inspected for label fit, score readability and consistency with Discover. All 106 descendants remain inside the gameplay frame. Native labels contain no game title, reels, respin, capacity, overload or Surface. No gameplay code, Unity asset, scene or prefab changed.

## Matching Rig Placement screen

[Open Rig Placement in Penpot](https://design.penpot.app/?screen=workspace&team-id=9e056cfe-f83f-81f8-8008-cc4e858053b7&file-id=3681d377-bc58-8110-8008-e7a38adc2112&page-id=38e9e9ec-a6d5-8050-8008-e7af29ab88cf).

Page: **04 - Tackle Apparatus - Rig Placement**. Board: `38e9e9ec-a6d5-8050-8008-e7afbca1154a`. [Finished preview](RigPlacement.png), 1920 × 1080.

This is the Build phase after successfully landing Garfish. The existing rig remains Sardine → Mackerel → Glass Minnow. The new Garfish is pending, with Catch explicitly selected for this illustrated state; the screen does not establish an automatic default. Bait is available and displays zero sale value. The insertion rail repeats the committed order horizontally and offers four gap controls: before the first creature, between each pair, and after the last. The after-Minnow target is selected. These are insertion gaps, not empty card slots or a maximum rig size. Existing creatures have no rearrangement handles.

Current haul stays 12; the separate haul preview shows 12 → 20 using an illustrative Garfish Catch value of 8. Confirmation commits both the selected role and insertion position, recalculates effects, then continues automatically. The pending Garfish contributes nothing before commitment. The permanent-placement wording concerns manual rearrangement within the current expedition: all attachments still clear at expedition end under the GDD. No Surface, Release, timing challenge or reel control appears here.

The Garfish value, tags, final Catch/Bait effects and preview/confirmation interaction details remain design work. This composition does not invent ability text for the pending creature. Final role selection needs the approved ability descriptions and resulting synergy feedback once those are specified; the visible role panels currently communicate selection and sale value only.

[RigPlacementArtPlate.png](RigPlacementArtPlate.png) was produced by editing the approved landing plate through the built-in image generation tool with the preserved [placement prompt](RigPlacementGenerationPrompt.txt). Native Penpot labels and plus markers remain editable; fish, frames and props belong to the raster concept plate. The preceding Discover and Landing pages are preserved.

Validation: the final export was visually inspected for style consistency, role-selection clarity, four insertion targets, preserved order, text fit and correct current-versus-preview haul. All 103 descendants fit inside the 1920 × 1080 board. Native labels contain no game title, reel controls, landing score, Resistance, capacity, overload or Surface. This is a static concept, with no game implementation, scene, prefab or production-artwork changes.
