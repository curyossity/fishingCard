# Third-party licenses

Font audit verified on **19 October 2026** against the imported font files, Unity font assets and the connected Penpot file **New File 1**. This document covers fonts; it is not a complete audit of Unity, packages, plugins or artwork licenses.

## Fonts in this project

All six font families listed below are available for **free commercial use under the SIL Open Font License 1.1 (OFL)**. There are no per-copy font royalties or required paid font subscriptions for these OFL distributions. The current game design uses **Cinzel and Source Serif 4** in both Unity and Penpot.

| Font | Unity usage | Penpot usage | Commercial use | Required credit / notices |
| --- | --- | --- | --- | --- |
| Cinzel | Midnight Field Journal display text; Regular, SemiBold and Bold assets, plus a Regular fallback. | Current assembly, earlier reference and initial verification card; weights 400, 500 and 600 observed. | Free, OFL 1.1. | No on-screen credit; retain copyright and OFL when distributing font software. |
| Source Serif 4 | Current and earlier UI body text, names and numbers; Regular, SemiBold and Bold; original variable font also retained. | Current assembly and earlier reference; weights 400, 500 and 600 observed. | Free, OFL 1.1. | No on-screen credit; retain copyright and OFL when distributing font software. |
| Inter Tight | No imported font file or TMP asset found. | Eight labels on **Fantasy Card — MCP Verification**, weight 400. Not part of the current gameplay assembly. | Free, OFL 1.1. | No credit required for rendered design exports. Include copyright and OFL if its font files are later distributed. |
| Marcellus SC | Earlier nautical UI through `Marcellus SC SDF.asset`; source is `MarcellusSC-Regular.ttf`. Cached TMP family metadata says “Marcellus”; source-file metadata identifies “Marcellus SC”. | None found. | Free, OFL 1.1. | No on-screen credit; retain copyright and OFL. Reserved Font Name: **Marcellus**. |
| Marcellus | `Marcellus-Regular.ttf` remains in the project. No corresponding TMP asset was found; it is not a font of the current candidate theme. | None found. | Free, OFL 1.1. | No on-screen credit; retain copyright and OFL if distributed. Reserved Font Name: **Marcellus**. |
| Liberation Sans | TextMesh Pro default font and fallback assets remain under `Assets/TextMesh Pro/`. Not the selected typography of the current theme. | None found. | Free, OFL 1.1, confirmed by the bundled font and license. | No on-screen credit; retain copyright and OFL. Preserve the reserved-name statements in its notice. |

Penpot inspection covered all five pages, including hidden/older design content: **789 nonempty editable text shapes**, with three reported families and no text shapes with an unreported family. Current assembly fonts are Cinzel and Source Serif 4; Inter Tight occurs only on the test page. Rasterized/generated artwork cannot identify an editable font family and is outside this text-font inventory. This inspection did not independently hash Penpot's server-hosted font binaries; their family licenses were checked against the official distributions.

Unity inspection covered **13 imported TTF files** and **14 TMP font assets**. The theme's static Cinzel instances are derived from the upstream variable font; the conversion provenance is retained at [FontProvenance.json](../Assets/UIThemes/MidnightFieldJournal/Fonts/FontProvenance.json). Its supplied OFL declares no Reserved Font Name. Source Serif 4's supplied OFL also declares no Reserved Font Name. Marcellus and Liberation reserved-name statements remain intact. No font files, font names, atlases or runtime typography were changed by this audit.

### Unity built-in runtime font

The older fallback paths in `FishingRunView` and `RunResultView` call Unity's built-in `LegacyRuntime.ttf`. Unity 6000.5.9f1 reports its fallback name as **Arial**. Prototype `Sardine.prefab` also retains engine built-in font references. These are engine/OS font references, not an imported Arial TTF; no Arial font file exists under `Assets`.

**Do not classify Arial as an OFL font or copy a Windows Arial font into the project.** Unity documents its built-in/OS fallback mechanism separately; an engine reference is not permission to redistribute a separately obtained Windows font. Microsoft's terms allow system font use and rendered graphics but do not generally permit bundling Windows font files or converting them into game bitmap fonts. The project’s actual imported Liberation Sans copy has its own OFL notice. See [Unity's font manual](https://docs.unity3d.com/2023.2/Documentation/Manual/class-Font.html) and [Microsoft's font redistribution FAQ](https://learn.microsoft.com/en-us/typography/fonts/font-faq). No additional paid font file was introduced or requested.

## Attribution and distribution requirements

- **Game screen / credits:** these OFL fonts do not require a visible “font by…” credit. Acknowledgement is optional. Copyright notices accompanying distributed font software are still required.
- **Commercial games and editable design/font packages:** font software may be bundled with a paid game. Keep the relevant copyright statements and full OFL text with distributed fonts and font-derived assets, including the project's TMP/SDF resources. The game itself does not need to be open source or licensed under the OFL.
- **PNG/JPEG artwork and rendered text:** using these fonts to render a title, card or mockup does not require a font credit or make that artwork OFL-licensed. If an export also carries font files, retain their notices.
- **Font changes:** preserve OFL licensing and notices. Respect any Reserved Font Names when distributing modified font software. Do not sell the font software by itself or imply author endorsement.

These conclusions follow [the OFL license](https://openfontlicense.org/open-font-license-official-text/) and [the official FAQ, especially 1.1–1.4, 1.10 and 1.20](https://openfontlicense.org/ofl-faq/). The FAQ distinguishes optional acknowledgements on artwork from the required notices for bundled fonts.

The consolidated, distributable font copyright notices and full OFL are maintained in [ThirdPartyNotices.txt](../Assets/FishingUIAssets/Fonts/ThirdPartyNotices.txt). It includes all six families, conservatively retaining legacy and Penpot-only entries. **Include that text file, or equivalent accessible copyright/license text, with a shipped build that includes these fonts.** Keeping a notice only in the source repository is not proof that a Unity build includes it. This task updates the notices and documentation; it does not certify a release package or add a build-copy mechanism.

## Original copyright and license sources

| Family | Bundled notice / source evidence | Official distribution |
| --- | --- | --- |
| Cinzel | [Cinzel-OFL.txt](../Assets/UIThemes/MidnightFieldJournal/Fonts/Sources/Cinzel-OFL.txt) | [Google Fonts OFL](https://github.com/google/fonts/blob/main/ofl/cinzel/OFL.txt); [author repository](https://github.com/NDISCOVER/Cinzel) |
| Source Serif 4 | [Theme OFL](../Assets/UIThemes/MidnightFieldJournal/Fonts/Sources/SourceSerif4-OFL.txt); [earlier OFL](../Assets/FishingUIAssets/Fonts/SourceSerif4/OFL.txt) | [Google Fonts OFL](https://github.com/google/fonts/blob/main/ofl/sourceserif4/OFL.txt); [Adobe source](https://github.com/adobe-fonts/source-serif) |
| Inter Tight | Penpot-only family; copyright **2022 The Inter Project Authors**. No font binary imported. | [Google Fonts OFL](https://github.com/google/fonts/blob/main/ofl/intertight/OFL.txt); [author repository](https://github.com/rsms/inter-tight) |
| Marcellus SC | [OFL.txt](../Assets/FishingUIAssets/Fonts/MarcellusSC/OFL.txt) | [Google Fonts OFL](https://github.com/google/fonts/blob/main/ofl/marcellussc/OFL.txt) |
| Marcellus | [OFL.txt](../Assets/FishingUIAssets/Fonts/Marcellus/OFL.txt) | [Google Fonts OFL](https://github.com/google/fonts/blob/main/ofl/marcellus/OFL.txt) |
| Liberation Sans | [Bundled OFL](<../Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt>) | [Liberation Fonts license](https://github.com/liberationfonts/liberation-fonts/blob/main/LICENSE) |

## Audit evidence

- [Live Penpot text-family inventory](Licenses/PenpotFontAudit-2026-10-19.json): file/page IDs, shape counts, family/weight usage and examples. The design was read without modification.
- [Unity font-asset inventory](Licenses/UnityFontAudit-2026-10-19.json): TMP family/style/source fields and the built-in runtime font's reported names.
- [Imported TTF hashes and embedded metadata](Licenses/ImportedFontFiles-2026-10-19.json): copyright, family/style, license descriptions and URLs for all imported TTF files.

The original local OFL files are retained unchanged. No commercial-use restriction or required on-screen attribution was found in any of the six family licenses.
