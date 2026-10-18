# Midnight Field Journal — implementation handoff

UI-01 is complete as a source inventory and rendering contract. No Unity assets, scenes, prefabs, gameplay definitions or build settings were changed by this task. Existing draft Unity work remains untouched.

**UI-02 is now complete:** see [isolated assets, fonts and templates](UI02.md), [validation](UI02Validation.json) and [current imported paths](UI02ImportedAssets.json). The UI-01 records below remain historical snapshots; draft source paths were migrated with preserved GUIDs during UI-02. The current game scene and legacy design remain unchanged.

Read in this order:

1. [Visual contract](VisualContract.md): approved composition, ownership, field geometry, typography and integration constraints.
2. [Asset inventory](AssetInventory.md): measured source dimensions, selection status and coverage of all 38 active card definitions.
3. [Asset manifest](AssetManifest.json): exact source paths, SHA-256 hashes, planned isolated destinations, stable Unity card IDs and Penpot source IDs.
4. [Missing visual assets](MissingVisualAssets.md): remaining art, font, state and quality work, including which tasks it affects.
5. [Validation report](UI01Validation.json), [source audit](UI01SourceAudit.json) and [Penpot snapshot](PenpotLayoutSnapshot.json): validation scope and read-only capture of actual layout/text properties.

UI-03's [detailed Catch Chain](UI03.md), UI-04's [projected Technique hand](UI04.md) and UI-05's [central encounter cards](UI05.md) are complete for their stated component scopes and current source set. Four Technique illustrations and 26 encounter illustrations remain absent. All theme-owned runtime content lives under `Assets/UIThemes/MidnightFieldJournal/`; generic view seams remain shared. Documentation lives separately in this folder. Planned destinations in the UI-01 manifest are fulfilled only as recorded by UI-02's imported metadata; reference/archive-only sources remain excluded.

UI-06's [assembled gameplay HUD](UI06.md) and UI-07's [playable comparison scene](UI07.md) are complete for their stated scopes. Open `Assets/UIThemes/MidnightFieldJournal/Scenes/MidnightFieldJournalComparison.unity` and press Play. The original scene and default build selection are preserved. [4K gameplay](UI07Gameplay4K.png), [choice modal](UI07Choice4K.png) and [results](UI07Results4K.png) show live Unity components. Next is UI-08 in [the integration task list](../../Midnight%20Field%20Journal%20Integration%20Tasks.md).

## Reference priority

- Gameplay, card identities and rules: GDD and approved card definitions.
- Theme-specific visuals: this contract and the approved Penpot component revisions.
- Current production/old-design visuals: the existing Card Visual Specification and UI Redesign Plan retain their original applicability outside this candidate theme.
- The initial full-screen concept establishes atmosphere; the user-approved detailed chain, brass connector, engraved techniques and laid-down perspective revisions establish their specific component treatment.
- Penpot coordinates are reference-layout values, not gameplay constants. `Run 03`, `Depth 12`, `Deck 12`, the three displayed catches, four example techniques, `4 / 8` and `21` are sample data only.

## Evidence and limits

The source audit verified 31 PNGs exist and measured their headers/hashes. It checked unique isolated runtime destinations, active card identities and the eight-Technique set. A fresh read-only Penpot capture confirmed Cinzel and Source Serif 4 and recorded five reference boards. This is not Unity import, alpha-quality, dependency, PlayMode, visual parity or build validation. A prior perspective-board export failed; placement was checked, but that composite is not treated as full-screen visual verification.

Source generation used the built-in image tool in the preceding design work; UI-01 generated no artwork and used no paid external service. Sources outside the project remain staging inputs until UI-02 imports the selected individual originals. The retained source hash is the identity check if those staging paths change.

Do not delete the old design or its asset folders. The inventory distinguishes new-theme sources, shared gameplay/font resources and legacy dependencies awaiting classification. UI-08 must validate a precise retirement set after comparison; removal is a separate user-selected cleanup.
