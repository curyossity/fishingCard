# Complete reel symbol family

Validated 7 November 2026 in Unity 6000.5.9f1. Six new transparent symbols complete the original Fish/Schooling family: Predator, Small, Heavy, Anchored, Armored and Elusive. All eight are mapped in the installed Discover prefab and preserved by its authoring pipeline.

[View all eight at actual game size](ReelSymbolFamily1080.png). Each icon uses the real 212 × 98 illustration rectangle, original paper drum and native tag caption. This review sheet is an asset preview, not a change to the three-reel game screen.

| Validation | Result |
| --- | --- |
| Supplemental symbol provenance, dimensions, usable alpha, visible edge padding, import settings and eight-tag coverage | 40 checks passed |
| Original artwork and prefab bindings | 155 checks passed; original 37 PNGs remain byte-identical |
| Slot rules | 71 checks passed |
| Live Harbor → Discover → respin review → explicit acceptance | 62 checks passed |
| Unity console after live validation | No errors or warnings |
| Unity metadata/reference validation | 1,529 files checked; zero errors or warnings |

Generated originals and Unity copies are byte-identical. All six contain transparent and opaque pixels; their visible silhouettes have padding. Three files contain a few invisible edge pixels with alpha 1/255; the visible-padding check permits only this one-unit noise. Textures retain Point filtering, Clamp, no mipmaps/read-write/compression, and Full Rect sprites. Existing Fish and Schooling artwork is unchanged. Native-size rendering and the [live final result](Gameplay/FinalRespin1080.png) were visually reviewed.

Gameplay probabilities, creature selection, lock/respin rules and cylinder animation are unchanged. The six tag illustrations replace the symbol-area text fallback; lower captions remain native text. Original artwork/animation validation records remain historical evidence.

Sources, prompts and hashes: [reel-symbol source record](../../UIThemes/TackleApparatusConcept/ReelSymbols/README.md).

Armored revision 2: replaced with a fresh generation featuring exactly two visible eyes, a plain plated shell and low pincers. The other seven mapped icons are unchanged. The 40 symbol checks passed again and the native-size family preview was recaptured. `Archive/ReelSymbolFamily-v1.png` preserves the original review sheet.
