# Supplemental reel symbols

Created 7 November 2026 using the **built-in image_gen tool**, with the existing Fish (`portrait-mackerel.png`) and Schooling (`tag-schooling.png`) sprites as visual references. No direct API or CLI generation was used. The original references are unchanged.

| Tag | Visual |
| --- | --- |
| Predator | Left-facing predatory fish with visible teeth |
| Small | Small single minnow with deliberate surrounding space |
| Heavy | Broad weathered fishing weight |
| Anchored | Iron anchor with ochre crossbar and rope |
| Armored | Broad teal crab with protective carapace and pincers |
| Elusive | Slender eel in a shallow S curve |

The family follows the reference artwork's stepped pixel clusters, charcoal outlines, muted teal/sage/silver, warm ivory highlights and restrained ochre accents. No labels or functional UI are baked into the images. Heavy is a visual metaphor only; it adds no weight/capacity mechanic. Animal illustrations are tag symbols and do not create new collectible creature definitions.

`Source/` contains the six unchanged generated transparent PNG originals, each 1774 × 887. Native display is 212 × 98 with preserved aspect ratio and Point filtering; Small intentionally reads smaller. No 4K work or upscaling was performed. `Manifest.json` records dimensions and SHA-256 hashes. `GenerationJobs.json` records the generation prompts, reference roles and original tool output paths; workspace copies are authoritative project sources.

Unity copies are under `Assets/UIThemes/TackleApparatus/Art/Discover/Symbols/`. `TackleReelSymbolSetup.Install()` imports and updates the existing Discover prefab, while full Discover regeneration calls the same import/binding methods. Original Fish and Schooling mappings remain intact.

Armored revision 2 is a completely new generation, replacing the first crab at the user's request. It has two clearly separated eyes on short stalks, a simple plated shell, and lower pincers. Only the original Fish and Schooling sprites were supplied as style references; the previous crab was not an edit target or reference. `Armored-v2-generation.json` records the complete prompt and tool output; `Archive/tag-armored-v1.png` preserves the superseded illustration. The existing Unity sprite path and GUID are retained, so no code or mapping change is required.

[Native-size preview and validation](../../../Validation/TackleReelSymbols/README.md)
