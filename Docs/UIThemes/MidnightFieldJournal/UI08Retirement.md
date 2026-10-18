# Legacy-theme retirement boundary

Nothing has been deleted. The original scene remains the default build entry. The candidate is a separate comparison scene.

[UI08RetirementDependencies.json](UI08RetirementDependencies.json) contains the exact explicit asset paths: 146 candidate-scene dependencies, 78 candidate-prefab dependencies, 84 shared between the scenes, and 67 original-only dependencies. Folder entries are included where returned by Unity; these counts are dependency entries, not a count of independent textures or deletion candidates.

| Set | Treatment |
| --- | --- |
| `Assets/UIThemes/MidnightFieldJournal/` | Keep while evaluating the candidate. Owns new art, isolated fonts, templates, runtime presentation, editor builders and comparison scene. |
| Fifteen legacy visual assets listed in `legacyVisualsStillReferenced` | Keep. Shared CardDefinitions still reference them. The candidate renderer uses exact theme mappings, but serialized gameplay-data references retain these files transitively. |
| `originalOnly` entries | Review only after choosing to retire the original scene. This set includes shared framework/font resources as well as old artwork; absence from the candidate's explicit dependency graph alone is insufficient authorization to delete. |
| Shared cards, biome, runtime/controller code, rendering/input packages, TMP shaders/settings | Keep. These support gameplay or framework behavior in both presentations. |
| Fifteen archived Technique definitions under `Assets/Cards/Backup/Techniques/` | Keep as requested. They are inactive backups, not retired UI files. |
| `implicitResources`, StreamingAssets, preloaded assets, address/string-based references | Review separately. Unity Resources and runtime/editor path lookups are not fully represented by `AssetDatabase.GetDependencies`. |
| Superseded draft artwork and reference/mockup composites | Reference-only. Review provenance and Penpot/source usage before a separate approved cleanup. |

The development smoke build intentionally includes both scenes, so its packed-asset list contains original-only assets too. It proves successful packaging, not that those assets are required by the candidate alone. See `UI08BuildValidation.json` for actual packed paths.

Before retirement: choose the candidate, complete its missing roster art/source-quality work, audit or rebind shared authored artwork references, switch the build/start scene deliberately, audit old Editor builders and string paths, and produce a candidate-only build. Only then derive an approved deletion set and verify gameplay after removal. No complete legacy folder is currently certified safe to delete.
