# Midnight Field Journal — candidate-theme visual contract

**MFJ-SYNC-010.2:** single-expedition Journal quests show COMPLETE/INCOMPLETE only; cumulative quests retain successful-expedition counters and bars. Tracking and rewards are unchanged. No pending Penpot import. [Validation](../../Validation/DiscoveryJournal/StatusPresentation/README.md).

**MFJ-SYNC-010.1 is the current Unity checkpoint.** Discovery Journal is active with three persistent quests and display-only rewards. This is a Unity-only increment; all earlier Penpot integrations remain completed, with no pending imports. [Behavior and verification](../../Validation/DiscoveryJournal/README.md). Earlier inactive-Journal statements below describe the previous checkpoint.

**MFJ-SYNC-010: imported and verified in Unity (2026-10-30).** Harbor, Tackle Shop and Technique Collection now support navigation and Set Sail. Shop/deck controls remain visual previews. [Integration and checks](Sync010/README.md). **0 pending; 44 completed; 13 superseded.** Earlier pending statements below are historical.

## Scope and coexistence

**MFJ-SYNC-009 is the latest Unity checkpoint.** R053/R055/R056 remain pending, with [R057 sharp-frame correction](Revisions/MFJ-R057/README.md) ready_for_review. Use its verified standalone frame construction rather than enlarged full-screen review images. 40 completed revisions and 13 superseded studies remain unchanged. No Unity import.

**MFJ-SYNC-002 remains the prior event integration checkpoint.** All five active coastal events use the detailed event frame at **1260 × 1400**, matching the creature face and the current central placement **(1250,110)**. [Sources, captures and checks](Sync002/README.md); [event checkpoint](Sync002/UnityCheckpoint.json). Earlier coordinate/reference tables below remain historical where superseded by this section and the detailed creature section.

Event geometry (top-left card pixels): category `(460,70,340,38)` at26; name `(140,114,980,82)` at66 (Fresh Shell Fragments50); illustration `(100,205,1060,596.57)`; type `(260,801,740,46)` at32; native type emblem `(610,892,40,40)`; EFFECT heading `(250,970,760,60)` at35; description `(225,1070,810,205)` at46, bounded minimum32. Body uses Source Serif4 Regular and centered top alignment; category/title/type/heading use Cinzel. Description word breaks are presentation data and preserve all live rule words/highlights. Both catch and event profile aspect ratios are0.9. All creature-only stats, rarity, tags, role decorations and role input are hidden for events.

The event master and each illustration are separate images. Native emblem paths match the Penpot warning triangle (Hazard), rising-current arrows (Environment) and four-point spark (Opportunity). Event category, name, type, description and emblem remain editable/native. The frame source1190×1322 is displayed1260×1400; source illustrations1672×941 exceed their display footprint. Captures are4K, while source images retain their recorded native dimensions. `centralEvents` in the canonical source catalog owns exact identity mappings, framing, title sizes and line breaks. Do not apply event mappings to compact catch portraits.

For Penpot revisions after the accepted Unity assembly, use [Design Changes.md](Design%20Changes.md) and [DesignChangeLedger.json](DesignChangeLedger.json). The immutable [Unity baseline](UnityDesignBaseline.json) records the starting file hashes/GUIDs and selected sources. Record every artwork, layout, typography and interaction-visual revision with its dependent Unity consumers before a later selective import; do not mark exported work as imported or discard pending revisions.

**MFJ-SYNC-001 remains the previous imported checkpoint.** [Creature sources, geometry, captures and checks](Sync001/README.md). Revisions MFJ-R001 through MFJ-R019 replace the previous central-card and pointer-hover presentation. The original baseline and Sync001 checkpoint are retained; use Sync004 for the next synchronization comparison.

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
| Gameplay B — laid-down hand | `bae680cd-9225-8018-8008-cdead5028f97` | Latest MFJ-R030 uses four independent projected preview elements; native flat sources are preserved |
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

Use **Cinzel** for display capitals and small type labels, and **Source Serif 4** for body copy, compact creature names and numeric stats. Both are now bundled under the candidate theme's `Fonts/Sources` with their SIL Open Font License 1.1 notices, isolated TMP assets and fallbacks. The live Penpot audit on 19 October 2026 confirms both in the current assembly. Do not substitute the older Marcellus SC. Both families permit free commercial use with no required on-screen credit; preserve copyright and license notices with distributed font software. See the [font license inventory](../../ThirdPartyLicenses.md) and [consolidated notices](../../../Assets/FishingUIAssets/Fonts/ThirdPartyNotices.txt).

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

All eighteen approved creatures have compact artwork. Existing dedicated compact mappings take precedence. The canonical catalog's separate `compactPortraits` registry maps the other fourteen identities to their existing central illustration sprites and explicit compact bounds; central-card framing stays independent. `JournalCompactArtworkSync` restores these entries during prefab regeneration. Unmapped identities clear the pooled portrait rather than displaying a previous creature. No additional artwork or Penpot revision is introduced by this Unity coverage fix.

For Bait, show the cyan edge/glow and badge and `0` haul Value. Catch has no extra Catch badge in this approved version. Keep full rules, tags, card-order numbers, modifier deltas and negative-effect badges off compact faces. Selection/target legality feedback belongs to separate overlays and must not recolor connectors. A self-hidden Catch Value remains `?` until allowed by runtime state; do not expose it through a tooltip.

### Catch Chain inspection tooltip — integrated

**MFJ-R027 is imported and verified at MFJ-SYNC-003.** It supersedes unimported R025/R026. The candidate chain now owns the live tooltip lifecycle. See the [tooltip handoff](Revisions/MFJ-R027/README.md), [preview](Revisions/MFJ-R027/CatchTooltip.png) and [pending import list](Pending%20Unity%20Changes.md).

The proposed native 900 × 640 tooltip shows the attachment's name, current Catch/Bait role, effective tags and that role's effect title/rules. The active role is indicated by the existing hook or twin-spark icon, with no textual Catch Chain/Catch/Bait heading. Explicitly empty roles show “No effect”. Do not mirror the other role or leak hidden values. Keep the existing compact face free of rules/tags; the tooltip is a separate inspection layer. It adds no pointer-hover frame, gameplay action or mode-switch control. Its independently generated engraved master supplies the copper/parchment/navy decoration. Name, tag pills, role icon and effect text remain editable vectors/text. The tooltip has no anchor pointer or protruding triangle.

Penpot has separate Catch, Bait and effectless examples and native overlay interactions on a dedicated prototype board. Future Unity work must use live attachment data, support navigation focus, clamp/flip placement and dismiss/rebind on scroll, removal, modal takeover or run end. This paragraph is a pending design contract, not evidence of implemented Unity behavior. Only a separate authorized import may mark it integrated.

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

Do not reuse artificial spaces in `T E C H N I Q U E` as localization content; implement the approved letter spacing as typography. The latest pending [MFJ-R030 fan](Revisions/MFJ-R030/README.md) replaces the older shallow arrangement: screen-clockwise angles −8°, −5°, +5°, +8°, 500-unit center spacing, and outer centers 50 units below the middle pair (middle Y450, outer Y500); outer projected top edges must stay lower than the middle pair. Use the recorded curve for 0–4 physical occurrences. Preserve the laid-down perspective and grounded shadows; Z rotations alone do **not** produce it. MFJ-R030 is imported and verified at MFJ-SYNC-003; the native fan caps curvature/tilt and compresses spacing for arbitrary hand counts without changing the current four-card gameplay cap.

Use the later perspective-only reference to tune real projective foreshortening: receding upper edges, near edges wider, compressed apparent height, subtle edge thickness and grounded shadows. It is a view transform on each complete card, including TMP text and artwork. UI-04 must author camera/plane/tilt settings and validate them in Unity; no trustworthy Euler angle or camera FOV can be inferred from a single raster reference. Do not label guessed numbers as Penpot measurements.

Use a camera-backed uGUI hand or equivalent genuine projective UI solution isolated from other HUD layers. This is a visual transform, not a 3D game-world or physics system. Configure its event camera/raycaster and mask/modal ordering. Keep the untransformed content layout reusable. Hover/focus can lift a card toward a readable inspect pose, while its identity, legal targets and command come from existing owners. Restore the resting pose on exit/disable; avoid conflicting with `TechniqueCardView`'s current local scale/anchored-position hover behavior.

Render actual hand occurrences for 0–4 cards. The hand must support repeated definitions, draw/discard/recovery, disabled input and target selection. All eight active designs remain active; the four illustrated examples do not constrain the deck. UI-04 cannot be declared visually complete for all content while four illustrations remain absent.

## Central creature and event cards

The current creature master is the detailed MFJ-R002 frame at a **1260 × 1400** display size. Its gameplay placement is `(1250,110)` on the 3840 × 2160 canvas. `MidnightFieldJournalCentralBuilder` owns native UI construction; the source catalog and `MidnightFieldJournalTheme.CentralCardEntry` own the eighteen per-creature portrait/text rectangles measured from Penpot. Sources, exact fields, hashes and instance IDs are recorded in [the synchronization checkpoint](Sync001/UnityCheckpoint.json) and [the revision ledger](DesignChangeLedger.json).

Names, resolved Weight/Value, tags, rarity name and both ability titles/descriptions remain separate TMP fields. Type is `C R E A T U R E` for creatures; Apex and treasure retain their distinct categories. The shared full-card renderer continues to control role choice, resolved content and information masks. Null effects stay blank. Native hook, twin-spark and rarity-diamond meshes follow the saved SVG sources. Rarity uses dark ink for Common, muted teal for Uncommon and copper for Rare; it is hidden with concealed identity and on events.

The selected role has independent warm hollow edges and a diffuse surrounding glow; it has no cyan selection wash. These are role-selection states, not hover frames. Per-creature central geometry overrides the old-design artwork scale/rotation/offset without mutating shared definitions or compact portrait mapping. Existing long descriptions without a separate heading can use the available description area and bounded fitting. Artwork gaps for unrefactored/special identities remain empty rather than substituting another species.

The frame source is 1190 × 1322, approximately 6% smaller than its approved display footprint. The eighteen central portraits are 1672 × 941 and displayed smaller than the source. This is a 4K gameplay composition with independently scaling native text/emblems, not a claim that each raster was generated at 4K. Inspect [actual output](Sync001/ActualGameplay3840x2160.png) and the six roster captures at target resolution.

The event master remains separate and unchanged. It uses its existing aspect, header and expanded single-description layout. Creature-only stats, icons, rarity and role controls are hidden. No event gameplay or data was changed by this synchronization.

Identity-concealed creatures display `UNKNOWN CREATURE`, no illustration, `?` stats, blank effects, no disclosed tags/rarity, and functional Catch/Bait selection. Preserve weaker information-only concealment and self-hidden Value independently. A reveal restores authorized information before attachment effects; history, ascent, target choices and tooltips must obey the same masks.

Events show name, type, artwork and description only. Hide both stat labels/numerals, Catch/Bait role labels, role hit targets, selection feedback and catch-only identity markers. They never become compact-chain cards. Event cancellation must not close the controller-owned pre-action Technique window; views do not resolve timing or effects.

## Background, actions, HUD and states

The 3840 × 2160 paper source is native 4K. The kelp layer is an **opaque** 1672 × 941 image used at opacity 0.45 over it, not a transparent native-4K overlay. Its enlargement and softness are recorded quality dependencies; inspect at 100% 4K before selecting final background treatment. Neither the concept screenshot nor the perspective composite qualifies as a native 4K background.

Descend and Surface faces contain their words/icons and substantial transparent outer margins. Their reference image canvases are 880 × 346 and 880 × 352, not their visible clickable face sizes. Inspect alpha bounds and set a matching rectangular Button hit region; transparent padding must not block neighboring input. Pointer hover uses the existing tint/scale/inspection without added frames. Native navigation focus is independent; pointer selection must not activate its frame. Pressed/disabled, Bait, selected-role and legal-target layers retain their existing purposes. Do not print a second TMP label over the baked face.

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

Current selective integration checkpoint: [MFJ-SYNC-003](Sync003/README.md). R027 tooltip and R030 adaptive fan are verified; no pending visual revisions remain as of2026-10-19.

## Engraved Line Load proposal — MFJ-R035

**Imported and verified at MFJ-SYNC-004.** Historical design proposal: [state previews and handoff](Revisions/MFJ-R035/README.md). A separate blank brass/navy housing frames native label, actual load/capacity, per-capacity segments and explicit capacity-state text. It occupies 760×175 within the existing 760×340 load/haul region; haul and all surrounding layout remain unchanged. Artwork is 2103×748 with transparent outer margins, displayed at 760×270.319 and offset Y−48 without slicing. Only the decorative housing may be imported as a sprite. Dynamic values, segments and status must remain native UI. Normal and capacity colors remain copper/gold; overload uses muted vermilion plus explicit excess text. Overload is still permitted and has its existing runtime consequences. The housing and native gauge presentation are now integrated in the isolated Unity theme.

## Marine action-button proposal — MFJ-R036

**Imported and verified at MFJ-SYNC-004.** [Matched button pair](Revisions/MFJ-R036/README.md), ready for review. Descend and Surface are both880×350, sharing an identical independent bronze/navy housing. Fine wave engraving, chamfered metal edging, four slotted corner fasteners and stepped endcaps distinguish these from card frames. Down/up vector symbols and Cinzel74 titles are separate editable sources; their combined static face exports follow the existing approved baked-button-label exception. Descend ink#E0B46F; Surface ink#F0E0BD. Button positions and external hints are preserved. Eight instances in four gameplay assemblies are replaced; former faces remain hidden for comparison. Housing source1989×791 exceeds the880×350 4K design footprint. Both isolated face assets and their source/profile/builder records are updated in Unity, retaining callbacks, disabled/pressed/navigation states and no pointer-hover frame.

## Parchment control alternatives — MFJ-R037–R038

**Penpot only, awaiting review.** [Parchment controls](Revisions/MFJ-R037/README.md) appear on the separate Gameplay E comparison. Buttons share a newly generated ivory paper blank, fine sepia corner rules, faded navy fronds and printed dark-ink direction marks; both remain880×350. Cinzel titles70 and separate vector icons retain editable sources. Line Load retains760×175, separate label/value/status fields and ink-filled native segments; normal navy#314A53, full sepia#8F693A and overload oxide#934A35. Source blanks are1988×791 and2196×716, aspect-preserved with transparent margins. These proposals replace only copied controls on Gameplay E; existing assemblies and the current Sync004 Unity implementation are unchanged. No new mechanic or overload probability is introduced. Future imports must reconcile only these two pending revisions and retain their separate art/dynamic-field boundaries.

## Selected Catch/Bait feedback — MFJ-SYNC-006.3

The Unity candidate now uses complete rounded role boundaries: Catch `(82,875,545,438)` and Bait `(634,875,545,438)` on the 1260×1400 card. Content keeps its existing card-space coordinates. Selected roles use an opaque pale-gold 8px edge, radius30, warm outer glow and inset hairline, plus a native gold check in the role header. The check is36×36 at panel-local `(120,43)` and never intercepts input. Neither role is selected by default. Only the selected role shows these decorations; events hide both. Source artwork and other-theme styling remain unchanged. This is a Unity-only refinement; no Penpot import was added. [Captures and verification](RoleSelection/README.md).

## Current Catch/Bait fading trial — MFJ-SYNC-006.4

Supersedes the active gold border/check treatment from006.3. Both roles remain fully visible before selection. Selecting Catch or Bait keeps that complete panel unchanged and dims the other with a46% black rounded overlay, matching the existing aligned panel bounds. Text, artwork and heading/icon dim together; both role targets remain clickable. Border and check objects remain inactive. New occurrences, events and blank cards clear dimming; concealed creatures retain role selection without revealing information. No images or text layouts changed. This Unity-only trial is implemented for user review; Penpot history remains unchanged. [Captures and verification](RoleFading/README.md).

## Fade boundary correction — MFJ-SYNC-006.5

Retains the approved fading appearance and 46% opacity. Native fade shapes now have a 6px inset to leave the printed border untouched, 24px ordinary corner radii and a 66px lower outside radius (Catch lower-left, Bait lower-right). No role target, artwork or text layout changes. [Corner details and verification](RoleFadeCorners/README.md).

## Subtle selected-role highlight — MFJ-SYNC-006.6

The approved fading treatment now includes a thin selected-role gold edge: 3px, #F6CF93 at170/255 alpha, inside the same6px inset and asymmetric24/66px corners as the fade. Both contours share one editor configuration function. No broad glow or checkmark; no artwork/text tint inside the selected panel. None/events/blank cards clear both treatments. Fade opacity remains46%. [Captures and verification](RoleSoftHighlight/README.md).

## Current selected-role inner-glow trial — MFJ-SYNC-006.7

Replaces the hard selected outline with a restrained warm inner glow (#F2BF6F, peak alpha50/255, 24px inward falloff). Fade and glow now use the same native geometry renderer and root-relative bounds, including 6px inset and asymmetric24/66px corner data. The printed master is raster artwork; the contour remains an authored approximation rather than an exact traced border. The previous outline/fade outer vertices actually matched; a sharp line exposed that approximation. Both ends of the new glow falloff are transparent and no light extends outward. Fade remains46%, center/text layout unchanged, no checkmarks. [Audit, trial captures and verification](RoleInnerGlow/README.md).

## Initial role dimming and deeper fade — MFJ-SYNC-006.8

Before role selection, both Catch and Bait panels are dimmed and neither glows. Fade opacity is now58% black (previously46%). The chosen panel returns to full brightness with the existing inner glow; the alternative stays faded. Both targets remain clickable. Contour, artwork, text, glow parameters and gameplay are unchanged. Events and blank cards show neither overlay. [Captures and checks](RoleInitialFade/README.md).

## Current encounter animated chain stats — MFJ-SYNC-006.9

User-approved exception to normal stat styling: Catch Chain-influenced Weight/Value numbers use 1.22× scale, green #18703A for Value gains/Weight reductions and red #AE2C26 for Value losses/Weight increases. Hold the chain-free baseline for 0.25s, then show each integer for at least 0.16s. Emphasis persists at the modified result. Unaffected numbers stay normal; Bait’s intrinsic zero sale value is neutral. Return-to-baseline counts visibly and restores normal styling on arrival. Hidden stats stay hidden. Incoming transitions hold counting until settled; historical/outgoing cards show their authoritative final values. All values remain separate live text; no art, numerical-delta labels or gameplay changes. [Captures and checks](StatFeedback/README.md).

## Springy number-beat motion — MFJ-SYNC-006.10

The current-encounter integer animation adds a short damped spring on each step: 14% scale punch over the existing enlarged style, alternating2.5° tilt, 0.36s settling. The final integer uses a stronger22% punch before returning to the persistent1.22× green/red style. Existing hold, integer pacing, masking, role handling and transition pause are unchanged. No flames, particles, sound additions, pixel treatment or new fonts/assets. [Verification](StatPunch/README.md).

## All-source current-creature stat feedback — MFJ-SYNC-006.11

Implemented directly in Unity on 20 October 2026. Main-card Weight/Value feedback now compares all resolved effects with printed stats, including Techniques, events, own abilities and the Catch Chain. Choosing Bait sets sale Value to neutral zero immediately; returning to Catch counts only effect modifiers from printed Value. Existing colours, integer pacing, spring motion and masking remain. [Verification and current baseline](AllStatFeedback/README.md). No new Penpot revision or pending import.

## Sequential Catch Chain stat feedback — MFJ-SYNC-006.12

Implemented directly in Unity on 20 October 2026. Compact creatures reuse the main-card animator for all resolved Weight/Value modifiers. Affected recipients run newest to oldest, scrolling into view before counting and waiting for the full final spring before the next begins. Physical views retain runtime identity across removals; queued cards defer emphasis. Bait zero and concealed Value retain their rules. Manual scrolling settles/cancels automatic feedback, and ended-run snapshots remain static. [Verification and current baseline](ChainStatFeedback/README.md). No new Penpot revision or pending import.

## Catch Chain card pop and flash — MFJ-SYNC-006.13

Implemented directly in Unity on 20 October 2026. Each affected Catch/Bait now receives a gentle 4.5% centered pop and warm ivory flash before its existing number animation. Scroll padding accommodates the expansion; chain connectors remain still. The newest-to-oldest sequence waits for both stages, and interruption/removal restores resting visuals. [Verification and current baseline](ChainCardPulse/README.md). No Penpot revision or pending import was added.

## Rapid numeric feedback — MFJ-SYNC-006.14

Implemented directly in Unity on 20 October 2026. Both current and compact creatures use 25ms ordinary steps, a 50ms initial hold and a 180ms final spring. Every displayed change retriggers the existing 14% scale punch and alternating 2.5-degree tilt; the final beat remains 22%. Large deltas use bounded strides targeting about 0.6 seconds of counting, subject to frame rate, rather than long one-unit sequences. Each frame displays at most one transition so every visible change receives its own pop/tilt. Fractional timing carries forward. Colour, masking, neutral Bait zero, retargeting, card pop/flash and newest-to-oldest chain serialization remain unchanged. [Verification and current baseline](RapidStatFeedback/README.md). No Penpot revision or pending import was added.

## Bold manipulated stats and adjusted pace — MFJ-SYNC-006.15

Implemented in Unity on 20 October 2026. Both main and compact creature numbers use 40ms ordinary steps instead of 25ms; large-delta counting targets 0.8 seconds instead of 0.6. Initial hold, per-change pop/tilt, final spring, card pop/flash and chain ordering are retained. Manipulated stats become bold while animating and remain bold at a non-default result. Reaching the default and finishing its landing restores the original weight. Queued, hidden and neutral Bait-zero fields retain normal styling. [Verification and current baseline](BoldStatFeedback/README.md). No Penpot revision or pending import was added.

## Displayed-value stat colours — MFJ-SYNC-006.16

Implemented in Unity on 20 October 2026. Both main and compact creature numbers derive colour from the displayed amount relative to default, rather than their destination or travel direction. Beneficial amounts stay green and detrimental amounts stay red while changing on the same side of default. A final default amount restores normal ink on arrival. Passing through default toward the opposite side switches directly to that side's colour without a neutral flash. Bait's immediate neutral zero, bold styling, pacing, pop/tilt and chain sequence are retained. [Verification and current baseline](DisplayedStatColours/README.md). No Penpot revision or pending import was added.

## MFJ-R042 — Modified-stat source tooltip (Penpot only)

Ready for review on **Concept B — Reusable Assembly (4K)**: select **MFJ-R042 / Stat modifier tooltips / editable review**. Detailed independent parchment frame, editable default/current amounts, signed source rows, source emblems and explanations, plus net change. Lionfish Value and Cod Weight examples cover creature, Technique, Hazard and Environment causes. Gameplay H shows an assembled example; the separate prototype wires hover on Lionfish Value to an independent overlay. [Preview and source contract](Revisions/MFJ-R042/README.md).

**Pending Unity integration; not imported.** Existing creature-effect tooltips and all integrated revisions remain unchanged. Preserve the current Unity-only numeric/chain animation refinements when later adding this tooltip. Runtime attribution must provide the real causes; the Penpot example rows are illustrative content only.

## MFJ-R043 — Compact modifier tooltip alternative (Penpot only)

**Ready for review; not imported.** On Concept B — Reusable Assembly (4K), select **MFJ-R043 / Compact modifier tooltips / 1–3 sources**. Default/current amounts plus portrait/icon, source name and signed amount only. The 680px-wide nine-region parchment frame grows for one to three reasons; overflow keeps three visible and adds a scrollbar and hidden-source indicator. Separate first/last scroll states and a click-navigation prototype document the intended interaction; live wheel/drag scrolling belongs to future Unity integration. [Previews and modular contract](Revisions/MFJ-R043/README.md).

R042 remains pending as a comparison alternative. **Choose one tooltip design before integrating; do not import both.** Current Unity checkpoint remains MFJ-SYNC-006.16; all 37 completed imports and four superseded drafts are preserved. No Unity files changed.

## MFJ-R044 — Tooltip portraits and overflow correction (Penpot only)

**Ready for review; not imported.** Refines the existing R043 boards in place: scrollbar tracks/thumbs removed, three-row overflow and “more” indicators retained. Creature portraits now fit completely inside their icon tiles with padding and preserved aspect ratio. [Preview and handoff](Revisions/MFJ-R044/README.md).

R044 supersedes pending R043; R042 remains available for comparison. Choose R042 or R044 before integration. Two pending alternatives, 37 completed imports, five superseded drafts. MFJ-SYNC-006.16 and Unity files are unchanged.

## MFJ-R045 — Arrow cues, flat icons and new nine-slice engraving (Penpot only)

**Ready for review; not imported.** New native 64px shell-and-knot corners and straight double-rule edges resize independently. Background-free single-ink creature silhouettes; all Techniques use a tied hook and all events a wave scroll. Up arrow appears above reasons, down below, both in the middle; no counts, labels or scrollbar. Three reasons remain visible. [Previews and handoff](Revisions/MFJ-R045/README.md).

R045 supersedes R044; R042 remains a comparison alternative. Two pending alternatives, 37 completed imports, six superseded drafts. Choose R042 or R045 before Unity integration. MFJ-SYNC-006.16 and Unity files are unchanged.

## MFJ-R045 — Integrated as MFJ-SYNC-007

**Imported and verified.** Main and Catch Chain modified numbers now expose the latest compact tooltip with actual resolver provenance, the new 64px sliced frame, flat background-free glyphs, three visible source rows and arrow-only scrolling. Pointer travel, focus, clipping, modal locks and concealed/Bait-zero rules are implemented. Stat and chain animations are preserved. [Import, captures and validation](Sync007/README.md).

R042, R043 and R044 are obsolete proposals retained for history; do not import them. **No pending changes remain: 38 completed imports, seven superseded drafts.** Future selective integration uses [MFJ-SYNC-007](Sync007/UnityCheckpoint.json); MFJ-SYNC-006.16 remains historical evidence.

## MFJ-R046 — Integrated as MFJ-SYNC-008

**Imported and verified.** The approved parchment timing instrument temporarily replaces the central encounter during creature selection. Its sliced frame has fixed 64px corners; native zones/patterns, external legends, independent outlined compass marker, Stop and stopped-result wash/burst remain editable and reusable. No creature information is visible before Stop and its brief feedback hold. Events bypass timing. Available rarity intervals normalize dynamically to the full track; queued/trail/chain/Apex promises retain their identity through a fixed-rarity interval. [Import and 4K verification](Sync008/README.md).

**No pending changes remain: 39 completed imports, seven superseded drafts.** Future selective integration uses [MFJ-SYNC-008](Sync008/UnityCheckpoint.json). The original Penpot design study and previous checkpoints remain historical evidence.

## Unity tuning — MFJ-SYNC-008.1

Uncommon is now 5%, Common 87% and Rare 8%. Unavailable rarity space goes to Common where possible so Uncommon stays 5% at shallow depths. No new Penpot change or pending import. [Validation and current checkpoint](RarityWidthTuning/README.md).

## Unity gameplay update — MFJ-SYNC-008.2

The stopped rarity now sets a ceiling. Every distinct biome/depth-eligible species at or below that rarity has one equal chance, without rarity/species weights or immediate-repeat exclusions. Bar sizes stay unchanged. Result text says “UP TO [rarity]”. Event pacing and fixed promises remain intact. [Validation and current checkpoint](RarityCeilingSelection/README.md). No new Penpot revision or pending import.

## Unity animation update — MFJ-SYNC-008.3

The stopped bar now winds up elastically and exits upward over one second before the encounter is revealed. Motion matches inElastic(1,0.74), resets between attempts, and cancels safely on Surface/restart. Selection and zone widths are unchanged. [Validation and current checkpoint](RarityBarElasticExit/README.md). Unity-only change; no pending Penpot import.

## Unity feedback update — MFJ-SYNC-008.4

Stopping in the highest displayed non-Miss rarity now emits a gold/ivory particle burst at the marker. The effect emits once per occurrence, clears on cancellation/reuse, and preserves selection odds, hold and upward exit. [4K verification and current checkpoint](RarityCelebration/README.md). Unity-only change; no pending Penpot import.

## Unity entrance update — MFJ-SYNC-008.5

The rarity bar now enters from below over one second using the mirrored elastic exit curve. The marker and Stop remain inactive until it settles; cancellation, restart, the upward exit and highest-rarity particles are verified. [4K captures and current checkpoint](RarityBarEntrance/README.md). Unity-only update; no pending Penpot import.

## R051 tag timing integration

The candidate now uses the 1600×400 Lantern Focus instrument centred in its 4K gameplay viewport. Paper and engraved frame are independent sliced exports; marker, monochrome tag glyphs, native labels, precision gradients, Stop and feedback are separate uGUI elements. No assembled mockup is sliced. Eight zones normalize at runtime with a 5% minimum. Small spans retain their glyph; a full tag name appears in the phase field when needed. No active-zone wash is shown. Precision hits reuse the gold/ivory particle celebration and exact old entrance/exit animations. See [Sync009](Sync009/README.md).
