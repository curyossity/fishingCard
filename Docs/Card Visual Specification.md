# Deepwater — Card Visual Specification

Design baseline: 6 November 2026. This defines the supplied-art and information boundary for future Deepwater presentation. It does not authorize code, prefab, scene or artwork changes in this documentation pass. The [GDD](Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md) governs mechanics.

Detailed existing geometry, numeric animation and visual provenance are preserved in [Card Presentation Reference](Card%20Presentation%20Reference.md). The current phase views consume native live fields; historical records do not define current gameplay. Gameplay instructions in implementation/artwork records yield to the GDD.

## Supplied-art boundary

Preserve supplied reusable masters and individual illustrations. Do not replace the encounter master with a precomposed per-creature face. Keep static images separate from runtime text, icons, role state and interaction feedback. No artwork is generated, redrawn, relabeled or replaced by this documentation change.

Existing nautical resources:

- Creature face: `Assets/FishingUIAssets/Cards/Creature/catch-card-base-dual-effect-1122x1402.png`, 1122 × 1402.
- Event face: `Assets/FishingUIAssets/Cards/Event/event-card-base-storm-purple-1101x1429.png`, 1101 × 1429. The red/silver alternate remains available.
- Compact rig card: `Assets/FishingUIAssets/Cards/CompactCatch/fishing-ui-catch-chain-card-master.png`.
- Filled rarity marker: `Assets/FishingUIAssets/Icons/catch.png`.

Midnight Field Journal remains a separate theme under `Assets/UIThemes/MidnightFieldJournal/`. Preserve its geometry, typography and illustrations; do not silently mix themes. Its [visual record](UIThemes/MidnightFieldJournal/VisualContract.md) and [inventory](UIThemes/MidnightFieldJournal/AssetInventory.md) document resources, not redesigned mechanics.

Retain explicitly authored masked-region/full-card-overlay layouts, aspect ratios, clipping and safe regions. Encounter transforms remain independent of compact portraits; illustrations stay below runtime information.

## Dynamic fields and unresolved statistics

Keep type, name, level, accumulated species sale value, primary ability, rarity hooks and supported tag indicators independent. Every collected copy contributes full base sale value; ability effects and upgrades use explicit authored data.

Rig cards display level and resolved accumulated species value. A species occupies one position regardless of copy count or level. There is no Catch/Bait choice or marker in the active creature loop.

The existing masters include Weight labels and stat regions. **Weight's purpose is undecided.** Existing art does not require a weight mechanic. Do not relabel Weight as Resistance. Resistance must be clearly available during landing, but its final location and Weight-field treatment need a presentation decision. Surrounding challenge UI can provide information without changing the supplied face; no asset edit is authorized now.

Landing Points, attempts, accuracy and rig score contributions belong to runtime challenge UI. Their layout is a dependency in [UI Redesign Plan](UI%20Redesign%20Plan.md), not additional printed card fields.

Preserve the separation of resolved numbers from presentation-only animations and attribution tooltips. Animations never change authoritative values or expose concealed data. Existing numeric pacing is preserved in the reference; it does not finalize animations for new scores or upgrades.

## Species placement and upgrades

After successful landing, a new species opens the independent Build screen. Show its primary ability, legal insertion gaps and a side-effect-free preview of resulting values. Explicit confirmation commits a new position at Level 1. A duplicate automatically upgrades its existing position and briefly displays the upgraded result before returning to Discover; no placement or extra confirmation is needed.

The approved Tackle Build presentation uses one brass details frame for level, resolved species sale value and a concise complete ability description. It does not show New Species/Duplicate labels, copy-count explanations, nested role panels or Catch/Bait controls. New placement shows Level 1; upgrade feedback shows the prior and resulting levels, +1, the resulting accumulated value and the resulting effect coefficient. Native text formats the authored current-level effect, rather than repeating Level 1 rules. The Penpot Sardine coefficients remain illustrative content; integration does not assign those abilities to unfinalized creature definitions.

The controller owns phase, landed occurrence and rig revision. Refreshes must not commit, repeat effects or carry selection from another occurrence. Failure cannot open acquisition/placement commitment. Levels and collected copies reset with the expedition.

Choose a permitted insertion position and commit. The rig view reflects unique ordered species, preserves existing relative order and offers no manual rearrangement. Keep insertion targets/restriction explanations outside face information. Current Tackle Build uses native gap buttons, a projected value readout, horizontal insertion scrolling and the shared vertical rig cabinet. New creature contribution begins after acquisition, affecting future challenges.

The integrated Build housing uses Landing's exact creature-frame location/size and right-panel border for its effects section. The Build effects interior has no Landing progress trough. Both frames belong to the reusable whole-machine background; separate frame overlays are removed. Creature art, tags, level, resolved sale value and current-level ability text remain native live fields. The lower rig and common outer silhouette retain their existing pixels.

## Rarity hooks and tags

Four lower sockets represent rarity only:

| Rarity | Active hooks |
| --- | --- |
| Common | 1 |
| Uncommon | 2 |
| Rare | 3 |
| Legendary | 4 |

Inactive sockets stay visibly inactive. Hook count is not independent gameplay state.

The 18 November 2026 vocabulary is Schooling, Predator, Anchored, Armored and Elusive. Each creature has exactly one gameplay tag. Show that tag using its supported supplied icon in an authored tag socket; unused sockets stay empty. Fish, Small and Heavy are retired gameplay tags. Preserve the reusable master and historical icon resources; the 19 November implementation migrates gameplay tags and supplied-icon mappings without artwork edits. Tag-name tooltips use a separate non-intercepting layer. Do not invent icons or universal abilities from tag artwork.

## Compact rig cards

Tackle compact cards support an external hover/focus tooltip with creature name, rarity, one tag, current level, resolved accumulated sale value, ability name and current-level rules. Coefficients use the shared brass accent. A native 460-reference-pixel panel wraps at the authored font size, opens beside cabinet cards or above insertion-tray cards, and clamps to the canvas safe area. It remains outside clipping masks and does not intercept input. Tray details explicitly label selected-placement preview values. The face retains its existing fields and artwork. Pointer inspection is delayed; keyboard/gamepad focus shows details immediately. Scrolling, source recycling, dismissal and phase exit clear inspection.

The approved Tackle Apparatus theme lives under `Assets/UIThemes/TackleApparatus/`. Its blank faces, portrait, clip and live name/level/sale-value fields follow the same separation contract. Original ivory/sage source faces remain preserved; active rig members use their shared face and native level/value fields. Unprovided portraits remain empty; do not substitute unrelated illustrations. The cabinet has a clipped scrolling list and no three-card cap. Original nautical and Midnight Field Journal resources remain preserved for their other presentations and provenance.

The 14 November 2026 creature-art integration replaces species illustrations with the 22 selected sprites from Penpot's **10 - Creature Sprites** page. Creature definitions supply both compact and encounter illustrations; old illustration-specific rotations, offsets and full-card overlays are reset for these standalone sprites. The old precomposed Squid face yields to the reusable face and native information fields. Tackle and Journal creature views consume those definition references, while their frames and non-creature resources remain theme-owned. Transparent source margins may be cropped without resampling; preserve the complete silhouette, aspect ratio and clear border. Rig names fit one line inside the dark nameplate. This does not change creature rules, unlocks or the separate generic Landing marker.

Preserve compact geometry and separate rod, ring, clip and medallion siblings. Runtime content includes name, portrait, level, accumulated species value and optional approved effect icon. No manual reorder controls or Catch/Bait marker belongs on a rig member. Weight-region treatment on historical masters awaits design.

Do not add full rules, tag lists, catch-order labels or modifier deltas. Physical layout shows current order; instance identity must survive insertion/removal. Interaction overlays stay outside face information. Scroll/stat feedback is presentation only and does not determine gameplay order.

## Hidden runtime state and events

Do not print lifecycle, selected-for-Release, hidden-state, modifier-state, positive/negative modifier or base-versus-modified badges on faces. Approved tooltips and external interaction feedback remain separate. Text must fit authored regions at supported resolutions.

Event masters show name, type, art and one rules panel; no Catch/Bait interaction, creature stats, rarity hooks or compact rig attachment. Event timing/content need review; old Descend/Surface timing does not define them.

Concealment timing relative to landing needs design review. Do not carry forward commit-only concealment, skip rules or ascent-history mechanics as approved gameplay. Preserve art/masking capabilities without adding hidden-state badges.
