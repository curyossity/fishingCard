# Midnight Field Journal — outstanding visual dependencies

This is the candidate theme's missing-assets ledger. It does not authorize deleting old content, inventing card mechanics, changing names/statistics, or calling incomplete art final. All active identities and existing artwork references are recorded in [AssetManifest.json](AssetManifest.json). Existing old-theme art is not automatically approved for the new style.

UI-02 update: MFJ-01 is resolved by theme-local Cinzel/Source Serif fonts and licenses. MFJ-11's source alpha-bound/import and template font-dependency checks are complete; full scene/player/retirement validation remains UI-08. Other entries retain the boundaries below. See [UI-02 evidence](UI02.md).

UI-03 update: native UI now supplies the compact cards' Bait border/badge, hover/navigation, selection, disabled and legal-target overlays. The three approved species' compact framing, connector eyelets, scrolling and 4K/1080p captures are verified. MFJ-07 remains open for the other components and full modal/interaction flows; missing roster art remains unchanged. See [UI-03 evidence](UI03.md).

UI-04 update: MFJ-10's component projection/input gap is resolved by an isolated camera rendering individual live Technique views, with mapped HUD raycasts, rest/inspect poses, hand-version guards and modal/input-lock checks. MFJ-07 now also has native Technique focus/disabled treatment. Full comparison-scene verification remains UI-07/08. Clear Water, Hold Steady, Re-rig and Recast still need illustrations; component checks cover their real text and explicit empty art slots. See [UI-04 evidence](UI04.md).

| ID | Gap | Affected work | Resolution boundary |
| --- | --- | --- | --- |
| MFJ-01 | Resolved: Cinzel 400/600/700 and Source Serif 4 regular/semibold/bold now have isolated source fonts, atlases, materials, fallbacks and OFL notices | UI-02 complete | Existing fonts and their modifications were preserved; candidate templates use the new font stack. |
| MFJ-02 | Four active Techniques have no selected new-theme illustration: `clear_water`, `hold_steady`, `rerig`, `recast` | UI-04 / full-deck visual completion | Separate illustration pass in the approved engraved style; keep definitions, rules and deck membership unchanged. Do not reuse unrelated rope/fish/hook art for these identities. |
| MFJ-03 | Sixteen ordinary creatures have no selected new-theme illustration | UI-03/05 and complete encounter coverage | See exact IDs below. Assess existing independent portrait compatibility explicitly; otherwise supply/review new portraits. Do not use old full-card overlays as cropped new portraits or modify shared definitions. |
| MFJ-04 | Two Apex, two treasures, five coastal events and one Location have no selected new-theme illustration | UI-05/07 and full run coverage | Theme-specific art and category presentation remain needed. Events retain their single rules panel; no extra Catch/Bait behavior. Do not invent new biome/progression designs. |
| MFJ-05 | Central masters remain initial candidates; component geometry/metadata/selection are implemented, richer artwork and frame cleanup remain open | UI-05 / UI-08 | UI-05 uses the existing footer for native hook counts and tags, independent ability headings and hollow role selection. Captures verify component rendering, not full concept parity. The unchanged CreatureTemplate has a dark rectangular paint patch in its art field, conspicuous when no portrait is shown; replace/clean the source through an art revision before final visual approval. Event data excludes catch-only metadata. |
| MFJ-06 | Illustrated kelp background source is 1672 × 941 enlarged over native 4K paper | UI-02 / UI-08 | Inspect its intended low-opacity treatment at 100% 4K. If insufficient, supply a genuinely higher-resolution equivalent and retain the selected visual language. Upscaling alone is not native 4K detail. |
| MFJ-07 | No new painted state variants for selected role, hover/focus, disabled, target-valid/invalid, warning/critical | UI-03 through UI-07 | Use authored palette and separate native tint/outline/transform treatments where sufficient. Record any state requiring new art; preserve existing legality/reason semantics and do not bake runtime status into card faces. Verify contrast and transformed hit targets. |
| MFJ-08 | Native journal choice/recovery, result/progression and read-only settings shells implemented; final visual/tooltip review remains | UI-08 | UI-07 preserves existing controller callbacks with parchment-color surfaces, local fonts, scrolling and explicit focus/input ownership. No legacy skin or invented settings/progression system is used. Review the resulting shells and any remaining tooltip treatment in the final visual pass. |
| MFJ-09 | Depth nodes, settings icon, empty-state/backs and miscellaneous decorative art are not independently selected for this theme | UI-06/07 | Inventory the actual existing flow before creating any asset. Approved simple rules/typography may remain native UI. Reuse a selected blank master when it matches an actual state; do not invent screens to justify art. |
| MFJ-10 | Perspective hand exists only as a flattened reference image; no Unity projection profile or input validation yet | UI-04 | Apply genuine camera/plane perspective to individual data-bound card views. Create measured Unity camera/tilt/layout settings and test pointer/controller/modal behavior; never import the composite as a runtime hand. |
| MFJ-11 | Alpha quality/visible bounds and full font/legacy transitive dependencies are not validated by PNG headers | UI-02 / UI-08 | Check painted edges, padding, connector eyelets, native source versus visible canvas, font atlases/materials and scene/prefab dependencies. No legacy deletion certification until verified. |

UI-06 update: native journal typography/rules provide the settings entry point, authored tier guide, current haul and action focus borders. Static action art is unchanged and receives independent trimmed hit rectangles, hover/focus, pressed and disabled behavior. Tooltip/transition/modal layers are prepared; their complete shells/flows remain UI-07. The kelp resolution and central-frame blemish remain open source-quality items. No new artwork or paid service was used.

UI-07 update: journal-native choice/result/settings shells now exist with explicit navigation, scrolling, background input locks and existing runtime callbacks. Choice rows intentionally use authorized text without substituting legacy portraits. The settings panel reports the current display configuration; no preferences system existed behind the settings event. Scene integration is complete, while final visual/style approval, complete-roster art, aspect/input stress and player-build cost remain UI-08. The source biome has no authored destination; no extra biome was invented for the comparison scene.

## Exact ordinary-creature coverage gap

UI-08 update: actual 4K/1080p/ultrawide/16:10 Editor output and input checks pass, with an added native navy matte outside the fitted design. Hand projection performs no idle renders in the bounded samples. The original scene still functions; exact legacy/shared dependency paths are recorded in [UI08RetirementDependencies.json](UI08RetirementDependencies.json). Standalone builds stalled twice at the local build-data shell, so player certification remains blocked. Thirty missing illustrations, the low-resolution kelp source and the central-frame blemish remain open; see [UI08.md](UI08.md).

`creature_cleaner_shrimp`, `creature_coastal_cod`, `creature_cuttlefish`, `creature_flounder`, `creature_garfish`, `creature_mackerel`, `creature_jellyfish`, `creature_lionfish`, `creature_moonlit_ray`, `creature_mussel`, `creature_sea_anemone`, `creature_sea_bass`, `creature_sea_urchin`, `creature_spiny_lobster`, `creature_squid`, `creature_tidepool_eel`.

Selected new portraits exist for `creature_sardine`, `creature_clinging_crab`, `creature_glass_minnow` and `creature_small_octopus`. UI-03/05 verify independent compact/central framing and exact identity lookup. They remain separate portraits, not flattened full-card overlays.

## Exact special-card coverage gap

- Apex: `apex_harbor_leviathan`, `apex_stormjaw_tuna`.
- Treasure: `treasure_pearl_cache`, `treasure_sunken_tackle_box`.
- Events: `reef_snag`, `silt_burst`, `gentle_upwelling`, `loose_knot`, `fresh_shell_fragments`.
- Location: `transition_next_waters`.

## Work that can proceed

UI-02 can import the 20 selected/candidate source images and prepare the isolated theme/fonts. UI-03 can implement the approved three-card sample with dynamic bindings, and UI-04 can implement the four illustrated examples and real hand mechanics without restricting the active deck. Missing content remains an explicit art dependency for complete-roster review; it is not permission for wrong-species images, changed rules or a permanently blank final presentation.

Resolve dependencies in their owning tasks. UI-01 is complete when their identities, impact and resolution boundary are recorded; it does not imply these later deliverables are already complete.
