# Midnight Field Journal — Unity Integration

## Objective and approved reference

Integrate the Penpot Midnight Field Journal design into the playable game, with reusable Unity views and live game data. Target a 3840 × 2160 presentation and readable scaling to 1920 × 1080. Keep game rules and authored card statistics unchanged.

Penpot source: New File 1, page **Concept B — Reusable Assembly (4K)**. Use the detailed Catch Chain cards, illustrated brass connectors, engraved Technique designs, and the separate **GAMEPLAY B — techniques laid on table / perspective preview** as visual references. Preserve the prior Unity assets and existing project work while migrating consumers.

The laid-down hand is currently a rendered review image. It must never become the in-game hand: each physical Technique needs its own artwork, live title/rules, availability, target selection, hover/focus state and input handling. Reproduce the perspective on those individual Unity views.

## Design isolation and later retirement

Keep both designs independently reviewable until the user chooses one. All new theme-owned content belongs under one dedicated root:

```text
Assets/UIThemes/MidnightFieldJournal/
  Art/
    Backgrounds/
    Cards/
    Creatures/
    Techniques/
    Connectors/
    Controls/
    Meters/
    Overlays/
  Fonts/
  Prefabs/
  Profiles/
  Scenes/
  Runtime/
  Editor/
```

- New illustrations, sprites, textures, materials, font resources, profiles, prefabs and theme-specific scripts use this root. Reusable gameplay/presentation owners remain shared rather than being copied into a second game implementation.
- During UI-02, migrate the existing draft `Assets/FishingUIAssets/MidnightFieldJournal/` and `Assets/Prefabs/UI/MidnightFieldJournal/` content into the new root through Unity asset moves, preserving `.meta` GUIDs. Update builder paths and references together. Do not move/delete the old design's folders as part of this migration.
- Use a dedicated scene, `Assets/UIThemes/MidnightFieldJournal/Scenes/FishingRunMidnightFieldJournal.unity`, for integration and comparison. Preserve `Assets/Scenes/SampleScene.unity` and its default build entry while the design decision is pending.
- New-theme rendering must not overwrite shared `CardDefinition` artwork or other content fields that change the old design. Use a theme-owned artwork mapping keyed to stable card identity where illustrations differ between designs.
- Record assets as **new-theme-only**, **old-theme-only**, or **shared** in the asset manifest. Shared fonts, font atlases/materials, creature art, shaders and other dependencies must remain outside any proposed old-theme deletion set. If necessary, migrate shared resources into an explicitly shared location with GUID-preserving Unity moves.
- Audit transitive dependencies, not just top-level sprites: accepting the new design must not leave its fonts, materials, prefabs or scenes dependent on old-theme-only folders.
- UI-08 must demonstrate that the original scene still works and that the new scene has no old-theme-only dependencies. Include a precise retirement manifest and a separate shared-resource list.
- Delete neither design during integration. After the user chooses the replacement, old-theme-only assets/images can be removed in a separate cleanup, with reference validation before deletion and compilation/build checks afterward. Deleting the entire existing `FishingUIAssets` tree is safe only after every surviving shared dependency has been moved out and verified.

## Current integration boundary

- Existing `FishingRunView`, `CreatureCardView`, `CompactCatchCardView`, `CatchChainView`, `TechniqueCardView` and `TechniqueHandView` already implement presentation and command forwarding. Preserve their connections to runtime owners.
- `MidnightFieldJournalPrefabBuilder` and seven draft prefabs exist, but predate the approved detailed chain and Technique revisions. Their earlier prefab validation does not establish readiness of the new design or playable-scene integration.
- The enabled build scene is `Assets/Scenes/SampleScene.unity`.
- Several approved illustrations still exist only in the generated-image staging area and Penpot. Import selected individual originals with durable project references and provenance; do not crop the full-screen mockup.
- The visual specification and previous UI redesign plan describe the earlier nautical treatment. Record the approved replacement contract before changing consumers, while retaining gameplay, concealment and dynamic-field restrictions.
- Inspect scene, prefabs, builder-generated hierarchy and runtime-created children together. Updating only a prefab is insufficient if a runtime view rebuilds the old layout.

## Ordered tasks

### UI-01 — Finalize the implementation contract and asset inventory

Depends on: none.

Status: **Complete — documentation/source audit only**. See [handoff](UIThemes/MidnightFieldJournal/README.md), [visual contract](UIThemes/MidnightFieldJournal/VisualContract.md), [asset manifest](UIThemes/MidnightFieldJournal/AssetManifest.json), [missing-assets ledger](UIThemes/MidnightFieldJournal/MissingVisualAssets.md) and [source audit](UIThemes/MidnightFieldJournal/UI01SourceAudit.json). Captured 31 existing image sources, 20 selected/candidate runtime images, all 38 active card identities and actual Penpot typography/layout. Unfinished artwork, font preparation and perspective implementation remain explicit dependencies of later tasks.

- Map each approved Penpot element to its source image and Unity owner, including latest chain frame, creature illustrations, brass connectors, Technique frame and four illustrated Technique examples.
- Define blank frame geometry, text/art regions, typography, palette, native source dimensions and intended 4K display dimensions. Distinguish native-resolution assets from scaled previews.
- Extend the visual specification/redesign documentation with a separate contract for this candidate theme; preserve the original design's contract during comparison. Keep resolved stats, explicit Bait markers, concealed information and event-only presentation contracts.
- Record assets not yet supplied for the remaining active Techniques, creature roster and interaction states. Keep content identities data-driven; the four example hand cards do not replace the eight-design active deck.
- Identify existing compatible artwork versus illustrations requiring a separately authorized art pass; do not silently redesign unreviewed cards.

Done when: a durable asset manifest and rendering contract account for every runtime element, source and outstanding art dependency.

### UI-02 — Import selected assets and configure the reusable theme

Depends on: UI-01.

Status: **Complete — isolated source/font/theme preparation and reusable template regeneration**. See [UI-02 delivery/evidence](UIThemes/MidnightFieldJournal/UI02.md). Imported 20 audited images, prepared eight theme-local font assets, regenerated seven prefabs, preserved migration/regeneration GUIDs, verified canonical regeneration without ignored staging, and checked that 804 protected legacy files remain unchanged. Gameplay scene installation and component behavior remain later tasks.

- Bring approved individual source assets into the dedicated theme root, preserving existing alternatives and GUID references. Consolidate only the new-theme draft folders using GUID-preserving Unity moves.
- Configure Sprite import dimensions, alpha, filtering, packing and platform limits from actual display requirements. Background targets native 4K; small UI assets need sufficient pixels for their own displayed size.
- Configure existing display/body TMP fonts and fallbacks; do not change card definitions to compensate for text-layout issues.
- Update theme/profile configuration and the existing prefab builder so regeneration produces the approved templates rather than stale draft visuals.

Done when: selected assets and typography resolve through project-local references and regenerated templates pass import/reference validation.

### UI-03 — Integrate the detailed Catch Chain

Depends on: UI-02.

Status: **Complete — reusable controller-facing component, isolated visual captures and Play Mode input checks**. See [UI-03 delivery/evidence](UIThemes/MidnightFieldJournal/UI03.md). The new chain uses approved revised frames/portraits and brass links, dynamic resolved fields, Bait styling, pooled views and real scrolling. It is included in the candidate canvas; the original scene/build remain unchanged. Full comparison-scene wiring remains UI-07.

- Apply the approved parchment template with independent portrait, name, resolved Weight and role-aware Value fields.
- Apply the cyan Bait border/badge and zero haul Value for Bait.
- Replace the old rail/oval connectors with the approved brass connector artwork as separate sibling elements. Keep hardware independent of card state tints.
- Support arbitrary chain lengths, scrolling and changing modes/values/weights, retaining legal-target selection, overlays and controller callbacks.

Done when: the real chain matches the approved three-card sample and remains correct with more, fewer or modified attachments.

### UI-04 — Integrate the engraved Technique cards and tabletop perspective

Depends on: UI-02.

Status: **Complete for component behavior and the approved source set; four Technique illustrations remain an explicit art dependency**. See [UI-04 handoff/evidence](UIThemes/MidnightFieldJournal/UI04.md). Real hand slots use individual dynamic views, a perspective camera, HUD-mapped input, inspect/focus poses and existing controller legality/version data. The candidate canvas contains the hand prefab; original scenes/build remain unchanged. Full comparison-scene integration remains UI-07.

- Build each card from the approved blank frame, separate illustration, live type/title/rules and existing Technique view behavior.
- Bind all eight active designs and real hand occurrences; do not use the rendered four-card review image as a runtime asset.
- Implement a camera-backed uGUI presentation for genuine foreshortening, isolated to the hand as appropriate. Tilt the complete card visual so text, illustration and frame share the same plane; no 3D physics or gameplay mechanics are introduced.
- Preserve hand ordering and identity across draws, discards, recovery and targeting. Derive layout from actual card count.
- Configure raycasting, overlap order and hover/controller-focus elevation; cards must return to the correct resting pose without changing the underlying hand.
- Verify labels and effect text remain readable when tilted. Use an inspect/focus presentation if required without changing the resting design or information rules.

Done when: real Techniques retain legal play/targeting behavior and dynamic content while matching the laid-down perspective reference.

### UI-05 — Integrate central creature and event presentations

Depends on: UI-02.

Status: **Complete for the reusable component and current source set; remaining illustration/frame quality dependencies are explicit**. See [UI-05 delivery/evidence](UIThemes/MidnightFieldJournal/UI05.md). All 30 encounter designs pass dynamic-field, concealment and layout checks; rendered Play Mode checks cover role input and locks. Original scene/build and gameplay definitions remain unchanged. Comparison-scene/controller integration remains UI-07.

- Assemble the themed central card from blank master, artwork, name, resolved stats, independent Catch/Bait panels and selection feedback.
- Preserve supported tags/rarity presentation, effectless panels, per-definition highlighting and authored portrait framing.
- Handle creatures, Apex, treasures, events and location/progression presentations through their existing contracts. Events have no Weight/Value or Catch/Bait modes.
- Preserve unknown-creature masking, self-hidden Value, reveal timing and transitions without leaking identity through tooltips or previews.

Done when: live encounter states render accurately and Catch/Bait selection still commits through the controller.

### UI-06 — Assemble the full gameplay screen and HUD

Depends on: UI-03, UI-04, UI-05.

Status: **Complete for the assembled reusable HUD and controller-facing bindings**. See [UI-06 delivery/evidence](UIThemes/MidnightFieldJournal/UI06.md). The candidate canvas now composes all primary regions, live navigation/load/haul/tier data, trimmed action faces and independent overlay layers. Actual Play Mode input and 4K/1080p captures pass. Full playable comparison-scene installation, settings/result shells and end-to-end flows remain UI-07; the original scene/build remain unchanged.

- Integrate the static background, live navigation, existing settings entry point, depth/tier guide, Line Load pips/numbers and haul display.
- Use the approved Descend/Surface faces with proper hover, pressed, focused and disabled states. Preserve controller-owned availability.
- Remove the retired universal Release control and Tension presentation from this layout. Release remains available only through legal card effects.
- Use actual biome, depth, draw-pile count, load/capacity and haul values rather than concept examples such as Deck 12 or Run 03.
- Set 4K scaling and anchors, preserving tooltip, transition and modal layers. Keep the prior UI available as a recoverable alternative during integration.

Done when: the assembled screen matches the design with all primary regions connected to live presentation state.

### UI-07 — Complete playable-scene integration and interaction flows

Depends on: UI-06.

Status: **Complete — separate playable comparison scene, matching native modal presentations and actual controller flows**. See [UI-07 delivery/evidence](UIThemes/MidnightFieldJournal/UI07.md). Saved wiring, prefab regeneration, role commitments, Technique targeting/cancellation/recovery, hidden targets, events, Apex/boundary handling, Surface/results, progression purchase, restart and scene reload pass. Original scene/build settings remain unchanged. The real biome has no destination; entry wiring is verified with transient test data. Final visual/build/performance validation remains UI-08.

- Wire the new UI into its dedicated playable comparison scene and keep prefab/builder/runtime hierarchy ownership consistent. Preserve the original scene and default build selection until the user chooses the new design.
- Exercise technique target selection, cancellation, recovery, modal choices, role changes, hidden encounters, events, Apex/biome transitions, Surface and run results.
- Prevent click-through, duplicate callbacks and actions during locked resolution or transitions. Existing runtime owners remain authoritative.
- Preserve UI references across scene reloads and prefab regeneration; remove obsolete active consumers only after their replacements are wired.

Done when: a full real run uses the new UI without missing bindings or interaction regressions.

### UI-08 — Validate visuals, gameplay integration and build readiness

Depends on: UI-07.

Status: **Editor validation complete for the current source set; standalone build/player certification blocked**. See [UI-08 evidence](UIThemes/MidnightFieldJournal/UI08.md). Actual 4K, 1080p, ultrawide and 16:10 output, input routing, idle rendering, long/empty views and original-scene smoke pass. The aspect matte fixes exposed blue margins. Two local Windows builds stalled at the build-data generator shell, so no player smoke is claimed. Missing art/source-quality gates and the exact legacy/shared retirement boundary remain explicit; nothing was deleted.

- Run relevant compilation and existing gameplay checks; add focused checks for new layout/input ownership where they protect meaningful behavior.
- Capture actual gameplay at 3840 × 2160 and 1920 × 1080, plus supported aspect-ratio stress cases. Compare against Penpot, not just prefab structure.
- Check long names/rules, all active Techniques, empty/full hands, long Catch Chains, modifiers, overload, hidden information and event/progression layouts.
- Validate pointer/controller focus and transformed-card hit targets, sorting, alpha edges, text sharpness, texture memory and UI rendering cost.
- Produce a build smoke test of the new scene, confirm the old scene remains functional, and validate the old-theme-only dependency boundary. Record evidence, the retirement manifest, shared resources, remaining art dependencies and any checks blocked by environment limitations.

Done when: evidence supports visual parity and functional gameplay, with any unresolved limitations explicitly recorded rather than treated as passed checks.

## Dependency order

`UI-01 → UI-02 → {UI-03, UI-04, UI-05} → UI-06 → UI-07 → UI-08`

Execute the component tasks in the listed order for incremental review; their shared prerequisite permits independent development but does not require parallel agents. Review each completed component in Unity before building the complete scene on it.

## Status

UI-01 through UI-07 are complete for their stated scopes and current source set. UI-08's Editor validation and retirement audit are complete; standalone build/player certification remains blocked by the documented local generator-launch stall. The isolated comparison scene is playable and the original scene/default build selection remain unchanged. Missing illustrations, source quality and player/hardware validation remain readiness gates; no legacy deletion is authorized by this status.
