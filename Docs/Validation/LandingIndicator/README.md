# Generic Landing fish indicator

Updated 13 November 2026. The original, species-neutral 24 x 16 pixel fish replaces the large garfish body and its separate brackets/carets. Source SVG, palette map and PNG hash are recorded in [the source folder](../../UIThemes/TackleApparatusConcept/LandingIndicator/Source.json). No reference-game pixels were copied.

## Reference choice

[Stardew Valley](https://stardewvalleywiki.com/Fishing) provides the clearest fit: a compact fish symbol read against a scoring bar. [Core Keeper's legacy fishing interface](https://corekeeper.atma.gg/en/Fishing) was also considered. Deepwater uses an original warm cream silhouette, dark outline, small eye and restrained shading to match its existing pixel UI. The actual creature remains in the portrait panel.

## Size and grading

The icon and scoring interval share one RectTransform. Width is approximately **52.91 px**, two-thirds of the starting **79.36 px** Perfect region on the 992 px track at 1920 x 1080. The native 24:16 aspect ratio is maintained, and the source alpha silhouette spans its full horizontal width. Normalized sizing preserves the ratio across resolutions. Region widths, motion and the scoring formula are unchanged.

The previous 240 px illustration was decorative; its scoring interval was only 34.72 px. Consequently, the requested 52.91 px visible/scoring width does not itself make the existing overlap rule stricter. A whole-fish-containment alternative was raised with the user and has not been adopted without an answer. Current grading still awards Perfect/Good by inclusive interval overlap, with Perfect priority.

Reinstalling approved Landing art retains the generic indicator through `TackleLandingArtSetup.ImportIndicator`; the original Penpot garfish export is preserved for provenance and other uses.

## Validation

- Landing asset/binding checks: **96 passed**, including generic source bytes, no separate decorative child, and the icon serving as the actual scoring interval.
- Live artwork checks: **46 passed**, including the two-thirds ratio, exact visible/model widths, scrolling rig, button raycasts, press/disabled feedback and three resolution captures.
- Landing rule checks: **64 passed**.
- Live outcome fixtures: **16 success and 30 failure/restart checks passed**, with persistence writes disabled. The live console reported zero errors/warnings.
- Unity metadata checker: **1,587 files, zero errors/warnings**. Code/document whitespace checks passed.

[1920 x 1080 preview](Landing-1920x1080.png) · [1280 x 800](Landing-1280x800.png) · [2560 x 1080](Landing-2560x1080.png) · [success](Landing-Succeeded.png) · [failure](Landing-Failed.png).
