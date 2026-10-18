# Midnight Field Journal — candidate-theme visual contract

## Scope and coexistence

For Penpot revisions after the accepted Unity assembly, use [Design Changes.md](Design%20Changes.md) and [DesignChangeLedger.json](DesignChangeLedger.json). The immutable [Unity baseline](UnityDesignBaseline.json) records the starting file hashes/GUIDs and selected sources. Record every artwork, layout, typography and interaction-visual revision with its dependent Unity consumers before a later selective import; do not mark exported work as imported or discard pending revisions.

UI-02 implementation is complete. See [UI02.md](UI02.md) for the current imported paths, bundled fonts, templates and validation. Source/draft paths and font availability statements captured during UI-01 below describe that earlier snapshot; the UI-02 import catalog is authoritative for current asset locations.

This is the rendering contract for the user-approved Midnight Field Journal candidate theme. It is additive to [Card Visual Specification](../../Card%20Visual%20Specification.md) and [UI Redesign Plan](../../UI%20Redesign%20Plan.md); their old-design artwork paths, colors and geometry remain valid for the existing scene. Shared gameplay/information rules remain binding in both themes.

Use `Assets/UIThemes/MidnightFieldJournal/` for theme-owned art, fonts, materials, profiles, prefabs, comparison scene and theme-specific code. Shared views/runtime owners stay shared and receive theme configuration without changing their default old-design behavior. Use a theme-owned mapping from actual `CardDefinition.UniqueId` to illustration/layout rather than rewriting shared card assets. For example, Unity uses `creature_clinging_crab`, not the illustrative YAML ID `clinging_crab`; Re-rig uses `rerig`. Exact identities are in the manifest.

Old `SampleScene` and build selection remain unchanged while the designs are compared. New integration targets the separate `FishingRunMidnightFieldJournal` scene. During UI-02, migrate only the existing new-theme draft assets/prefabs through GUID-preserving Unity moves and update the new builder consistently. Do not rebuild the old design with new artwork.

## Source references and coordinate convention

Penpot file **New File 1**, page **Concept B — Reusable Assembly (4K)**:

| Reference | Board ID | Role |
| --- | --- | --- |
| Editable gameplay assembly | `bae680cd-9225-8018-8008-cdde36733e6f` | Current composition and live-field reference |
| Complete detailed Catch Chain | `bae680cd-9225-8018-8008-cde2b511f13d` | Approved frames, illustrations, Bait styling; includes latest illustrated connectors |
| Engraved technique hand | `bae680cd-9225-8018-8008-cde85bfb5c79` | Editable card sources and illustration treatment |
| Gameplay B — laid-down hand | `bae680cd-9225-8018-8008-cdead5028f97` | Perspective-only reference; hand is a raster review composite |
| Component review sheet | `bae680cd-9225-8018-8008-cdde40529e8e` | Blank main creature/event frames; its earlier technique master is superseded |

The read-only [Penpot snapshot](PenpotLayoutSnapshot.json) captures current positions, typography and visible descendants. All rectangles below use **top-left origin**, in 3840 × 2160 design pixels or normalized to an untransformed card canvas. They are not Unity bottom-left anchors. Convert normalized `(x,y,w,h)` to `anchorMin=(x,1-y-h)` and `anchorMax=(x+w,1-y)` where stretch anchors are appropriate. Rotate/project the complete card after composing its fields.

| Gameplay region | X | Y | Width | Height |
| --- | ---: | ---: | ---: | ---: |
| Background layers | 0 | 0 | 3840 | 2160 |
| Header | 0 | 0 | 3840 | 120 |
| Catch Chain region | 105 | 170 | 855 | 1540 |
| Load and haul region | 145 | 1745 | 760 | 340 |
| Central card | 1250 | 125 | 1040 | 1300 |
| Right controls | 2720 | 170 | 970 | 1250 |
| Editable technique hand reference region | 960 | 1435 | 2480 | 720 |

Preserve these proportions at 16:9 with a 3840 × 2160 CanvasScaler reference. Make regions responsive through authored anchors and safe areas, rather than fixing gameplay state to these example coordinates. Preserve aspect for illustrations; the approved frame display aspect may differ slightly from native source aspect and should be explicit profile data. Do not stretch portraits indiscriminately. At other supported aspects prevent action, modal, hand and central-card collisions; UI-08 supplies actual captures.

## Static art and runtime ownership

| Element | Static/source layer | Dynamic owner and fields |
| --- | --- | --- |
| Background | Native 4K paper, separate illustrated kelp layer | `FishingRunView`; background never contains gameplay cards or labels |
| Header | Restrained copper rules | `FishingRunView`; actual biome, depth, pile count; run number only if backed by an existing source |
| Compact catch | `CatchFrameV2`, no baked text/creature | `CompactCatchCardView`; name, portrait, resolved Weight, role-aware haul Value and Bait badge |
| Chain | `BrassConnectorV3`, independent of cards | `CatchChainView`; acquisition order, target forwarding, scrolling and connector placement |
| Central creature | `CreatureTemplate`, blank two-effect master | `CreatureCardView`; type/name/art/stats, two rules regions, selected role and permitted identity metadata |
| Event | `EventTemplate`, blank single-rules master | `CreatureCardView`; name, type, art, description; no creature-only fields |
| Technique | `TechniqueFrameV2`, separate engraved illustration | `TechniqueCardView`; type/title/rules, availability/focus; `TechniqueHandView` owns physical slots/layout |
| Descend / Surface | Approved complete face images, including their baked words/icons | `RunActionButton` / `FishingRunView`; legality, focus, press and callback forwarding; no duplicate printed label |
| Load meter | Individual empty/filled/danger pips | `CatchChainView` / `LineLoadMeterView`; effective load, capacity, active pips; no new load calculation in UI |
| Haul | Live typography | `FishingRunView` / existing presentation binding; aggregate runtime haul contribution |
| Depth guide | Restrained rules/nodes if supplied | `FishingRunView`; biome-authored tier names/ranges and current position |
| Tooltips, choices, results, transitions | Separate themed shells when supplied | Existing view/controller owners retain masking, modal input and transition sequencing |

Do not bake names, stats, rules, card identities, pile counts or load into reusable runtime sprites. Baked action labels are an explicit exception approved by the user. Treat missing localized button faces as an asset dependency if localization is later required. Full concept screenshots and the four-card perspective composite are reference-only and excluded from runtime destinations.

## Palette and typography

These are the authored Penpot token values, not sampled guesses from compressed art:

| Use | Color |
| --- | --- |
| Navy paper base | `#111E25` / `#111D24` in component backing |
| Main text on dark paper | `#F0E0BD` |
| Parchment text | `#232A29` (techniques `#252923`) |
| Muted interface text | `#B8B9AA` |
| Copper accents | `#B98553` / detailed-chain `#B88755` |
| Bait frame | `#8ED6DB` |
| Bait badge | `#548C94`, ivory label `#F4EDD9` |

Use **Cinzel** for display capitals and small type labels, and **Source Serif 4** for body copy, compact creature names and numeric stats. The Penpot snapshot confirms both fonts. Do not silently substitute Marcellus SC from the draft builder: Cinzel is not currently bundled. Acquire a free, license-verified font distribution and include its license during UI-02. Existing Source Serif 4 files and OFL notice are project resources; isolate their full font/atlas/material dependencies before old art can be retired.

Reference 4K sizes: header 35–44; compact name 47–50; compact stat numerals 54, labels 24; central title 57, effect names 37, rules 32–34; technique title approximately 40, rules 30–33, type label approximately 20. These are reference design-pixel sizes, not the old theme's fixed 18-point rule. Fit actual authored text in bounded regions and verify projected readability at 1080p. Do not edit gameplay descriptions merely to fit a mockup sample. Preserve authored highlights and explicit empty effect text.

## Detailed compact Catch Chain card

Use a 680 × 382.022472 untransformed reference canvas (aspect 1.78). `CatchFrameV2` supplies aged parchment, chart contours, seaweed sketches, fine corner etching and the central stat divider. All labels and values remain live UI. Do not restore the retired boxed header/art/stats layout.

| Field | X | Y | W | H | 4K size |
| --- | ---: | ---: | ---: | ---: | ---: |
| Name | 67 | 36 | 470 | 69 | 50, Clinging Crab 47 |
| Fish illustration reference | 64 | 100 | 552 | 221 | Aspect-preserved framing |
| Crab illustration reference | 120 | 75 | 440 | 250 | Aspect-preserved framing |
| WEIGHT label | 76 | 310 | 144 | 40 | 24 |
| Weight numeral | 228 | 296 | 83 | 68 | 54 |
| VALUE label | 381 | 310 | 115 | 40 | 24 |
| Value numeral | 527 | 296 | 104 | 68 | 54 |
| Bait badge face | 528 | 40 | 104 | 42 | Label 23 |

Keep generous artwork framing per species, encoded in the theme mapping. Source images contain transparent padding; the display canvas is not the visible silhouette. Preserve the approved three reference crops while fitting future creatures without crossing names or stats.

For Bait, show the cyan edge/glow and badge and `0` haul Value. Catch has no extra Catch badge in this approved version. Keep full rules, tags, card-order numbers, modifier deltas and negative-effect badges off compact faces. Selection/target legality feedback belongs to separate overlays and must not recolor connectors. A self-hidden Catch Value remains `?` until allowed by runtime state; do not expose it through a tooltip.

### Brass connectors

Use the **illustrated** `BrassConnectorV3`, not the earlier oval vectors. It owns straight metal sides, patina, warm highlights, dark central opening and small eyelets. Suppress the older black socket/oval artwork when this integrated hardware is used.

Cards in the reference start at `(190,280)`, `(190,730)`, `(190,1180)`; pitch is 450. Attach hardware at horizontal center, independently of card movement/selection visuals. The inter-card upper/lower eyelet centers lie 14 pixels inside the facing card edges. Source eyelet centers are approximately 0.151 and 0.845 of the connector image height. Size its canvas so those two points reach their anchors, retaining its source 2:3 canvas aspect; do not stretch its visible metal to fill the entire canvas. Top/bottom hanging-reference canvases are 104 pixels high. The manifest's 104 × 156 connector display is illustrative: actual span derives from adjacent cards, not a gameplay constant.

Hardware must scroll with its cards and remain behind modal/tooltip layers. Longer chains use a real viewport; no arbitrary three-card cap and no invented positional Release rules.

## Engraved techniques and perspective

UI-04 implements the camera-backed individual-card component described below. Current Unity-authored projection values, input mapping, validation and the four remaining illustration gaps are recorded in [UI04.md](UI04.md). The later flattened reference remains excluded from runtime art.

Use `TechniqueFrameV2`; earlier `TechniqueTemplate` is superseded. The illustrated rope/fish/knot/hook vignettes blend into paper, with no boxed illustration or lower-panel divider. No additional role footer was approved for this revision.

Compose each card flat first on a 620 × 640 reference canvas:

| Layer | Normalized top-left rectangle | Text reference |
| --- | --- | --- |
| Frame | `(0,0,1,1)` | Static |
| Type label | `(0.23,0.073,0.54,0.047)` | Cinzel, `TECHNIQUE`, approximately 20 |
| Title | `(0.12,0.135,0.76,0.09)` | Cinzel, approximately 40 |
| Engraved vignette | `(0.08,0.275,0.84,0.455)` | Separate sprite |
| Rules | `(0.14,0.76,0.72,0.17)` | Source Serif 4, approximately 30–33 |

Do not reuse artificial spaces in `T E C H N I Q U E` as localization content; implement the approved letter spacing as typography. The reference fan uses Z rotations -5°, -1.2°, +1.2°, +5° with slightly raised middle cards, soft contact shadows and restrained overlap. These rotations alone do **not** produce the approved laid-down perspective.

Use the later perspective-only reference to tune real projective foreshortening: receding upper edges, near edges wider, compressed apparent height, subtle edge thickness and grounded shadows. It is a view transform on each complete card, including TMP text and artwork. UI-04 must author camera/plane/tilt settings and validate them in Unity; no trustworthy Euler angle or camera FOV can be inferred from a single raster reference. Do not label guessed numbers as Penpot measurements.

Use a camera-backed uGUI hand or equivalent genuine projective UI solution isolated from other HUD layers. This is a visual transform, not a 3D game-world or physics system. Configure its event camera/raycaster and mask/modal ordering. Keep the untransformed content layout reusable. Hover/focus can lift a card toward a readable inspect pose, while its identity, legal targets and command come from existing owners. Restore the resting pose on exit/disable; avoid conflicting with `TechniqueCardView`'s current local scale/anchored-position hover behavior.

Render actual hand occurrences for 0–4 cards. The hand must support repeated definitions, draw/discard/recovery, disabled input and target selection. All eight active designs remain active; the four illustrated examples do not constrain the deck. UI-04 cannot be declared visually complete for all content while four illustrations remain absent.

## Central creature and event cards

The current assembled central blank masters are candidates for UI-05, not the later detailed compact/Technique revisions. Keep this distinction in asset status; do not silently claim they match the original concept's elaborate illustration/effect lighting.

On the 1040 × 1300 central reference, name is `(110,65,820,100)`, stats labels/numerals occupy the ivory strip around Y 796–893, Catch label/name/rules begin at Y 915/968/1031, and Bait uses the corresponding right column. The snapshot contains exact independent text rectangles and sizes. An event's single rules safe region from the draft profile is normalized top-left `(0.09,0.63,0.82,0.30)`; validate it against the actual event frame before shipping. The draft builder's creature art/title geometry does not match every Penpot field; copy the measured theme profile rather than reusing hard-coded old values.

Preserve type/name, resolved Weight and Value, selected role, separate Catch/Bait rules and authored highlighting. Zero Weight or Value is visible, not blank. Role selection defaults to Catch per new encounter and does not fire attachment effects. Only commitment invokes gameplay. Explicitly effectless roles stay blank.

The candidate frame has no authored rarity/tag sockets. UI-05 retains the assembled example's footer tag line, with the existing one-to-four rarity-hook count directly below it inside that same footer. These are separate native UI hook strokes in dark parchment ink, with inactive positions at 22% opacity; no old nautical sockets or icon assets are imported. Tags occupy `(160,1210,720,36)` and the hook row `(442,1249,156,28)` on the 1040 × 1300 face. This is the candidate implementation's explicit resolution of the metadata gap, available for visual review in UI-05's captures; it is not a claim of a newly user-approved Penpot revision. The original theme's hook-count/tag-rail contract continues unchanged.

UI-05 uses the measured main-card fields with small fitting corrections: type `(110,34,820,44)` avoids the painted top rule while accommodating font metrics; the two description regions extend slightly lower within their existing panels to fit the longest authored Apex rules at a minimum 22 reference pixels. Ability headings remain independently fitted. Selection is a native hollow copper edge with a subtle cyan wash; it must not use a solid-quad Outline that obscures the panel. Source frame/portrait PNGs remain byte-identical.

Identity-concealed creatures display `UNKNOWN CREATURE`, no illustration, `?` stats, blank effects, no disclosed tags/rarity, and functional Catch/Bait selection. Preserve weaker information-only concealment and self-hidden Value independently. A reveal restores authorized information before attachment effects; history, ascent, target choices and tooltips must obey the same masks.

Events show name, type, artwork and description only. Hide both stat labels/numerals, Catch/Bait role labels, role hit targets, selection feedback and catch-only identity markers. They never become compact-chain cards. Event cancellation must not close the controller-owned pre-action Technique window; views do not resolve timing or effects.

## Background, actions, HUD and states

The 3840 × 2160 paper source is native 4K. The kelp layer is an **opaque** 1672 × 941 image used at opacity 0.45 over it, not a transparent native-4K overlay. Its enlargement and softness are recorded quality dependencies; inspect at 100% 4K before selecting final background treatment. Neither the concept screenshot nor the perspective composite qualifies as a native 4K background.

Descend and Surface faces contain their words/icons and substantial transparent outer margins. Their reference image canvases are 880 × 346 and 880 × 352, not their visible clickable face sizes. Inspect alpha bounds and set a matching rectangular Button hit region; transparent padding must not block neighboring input. Hover/pressed/disabled/focus feedback uses approved tint/outline/transform layers or separately supplied state art. Do not print a second TMP label over the baked face.

Show only the two permanent core actions. No universal Release button or Tension bar is part of this candidate layout. Card-granted Release and its legal target UI remain available. Availability, locked resolution, current depth, biome tiers and draw-pile count come from the existing controller. A decorative `RUN 03` sample does not authorize a new run counter or persistence system.

Use live effective total Line Load, capacity and haul. A load-relief Technique can affect effective load without changing creature weights: do not recompute the meter solely from visible card numerals. Numeric overload stays visible beyond capacity; the pip pool represents capacity and uses the existing danger state. Do not introduce new risk thresholds, gains, or counters to match art.

Noninteractive art does not receive raycasts. Pointer/controller focus, disabled reasons, legal/illegal targets, selected Catch/Bait panels, pending choices, empty chain/hand, transitions, results and progression must retain functional presentation. UI-01 records missing theme shells/state artwork in [MissingVisualAssets](MissingVisualAssets.md); no unrelated old-art fallback or generic placeholder is authorized as the finished new design.

## Import, layering and acceptance boundaries

UI-07 supplies native modal shells using the theme's parchment color, copper edge and local fonts, with scrolling textual choices/results. Choice portraits are omitted rather than replaced with old or inferred images. The modal host owns presentation input/focus locks; the controller continues to own target legality, pending reactions, rewards and restart. The settings event opens read-only current-display information, since no adjustable preferences owner exists. These implemented shells are shown in [UI-07's 4K captures](UI07.md); final visual/input/build review remains UI-08.

UI-06 implements the assembled 3840 × 2160 layout inside a fitted 16:9 viewport with section input/visibility groups. Native typography supplies settings (in place of the sample run number), actual tier names/ranges, current haul and learned trail text. Hand pile counts occupy the spare row beneath Surface rather than overlapping the depth guide. Catch Chain, central card and hand retain their independently authored component geometry. See [assembled captures and binding scope](UI06.md).

Action hit rectangles use source alpha bounds while the unsliced full button face preserves its measured position and proportions. Focus edges/hover scale, pressed/disabled tints and controller availability remain independent of the static image. Whole-line effective load is supplied directly by the controller; danger pips indicate actual overload only. Hidden Catch values also conceal the aggregate haul. This screen is a reusable candidate prefab; installing its playable scene and full modal/settings/results flows remains UI-07.

- Preserve source pixels and `.meta` GUIDs during moves; never slice the concept. These full painted frames use Simple sprites, not speculative nine-slice borders. Add authored borders only if a specific resizable asset actually supplies them.
- Source alpha-channel presence is not a quality verdict. UI-02 checks transparent bounds, dark fringes, clipping, import limits, mip/filter behavior, font atlas sharpness and actual 4K display sizes.
- Keep ordinary UI surfaces and the perspective hand independently sorted as needed, with deterministic tooltip/transition/modal canvases above them. Modals block both projected and flat gameplay input.
- Do not overwrite existing shared font assets or their current modifications as a side effect of theme generation. New fonts/materials/atlases live under the theme root or an explicitly shared location with their licenses and dependencies.
- The earlier seven draft prefabs and their validation report predate these revisions. Their existence is not proof of this contract's implementation.
- UI-02 imports assets; UI-03/04/05 implement card views; UI-06/07 compose and bind; UI-08 validates actual gameplay and safe retirement boundaries. UI-01 establishes the handoff, not visual or functional completion.
