# Deepwater — Card Visual Specification

Design baseline: 6 November 2026. This defines the supplied-art and information boundary for future Deepwater presentation. It does not authorize code, prefab, scene or artwork changes in this documentation pass. The [GDD](Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md) governs mechanics.

Detailed existing geometry, numeric animation and visual provenance are preserved in [Card Presentation Reference](Card%20Presentation%20Reference.md). Existing views have not been migrated. Gameplay instructions in implementation/artwork records yield to the GDD.

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

Keep type, name, sale value, Catch/Bait rules, rarity hooks and supported tag indicators independent. Only the committed role's effects operate. Explicitly empty roles remain empty and must not mirror the other role as final content.

Catch displays resolved sale value; attached Bait displays neutral zero and an explicit `BAIT` marker. Both roles participate in adjacency/tag synergies unless excluded by an ability. Selecting a role alone does not activate attachment effects.

The existing masters include Weight labels and stat regions. **Weight's purpose is undecided.** Existing art does not require a weight mechanic. Do not relabel Weight as Resistance. Resistance must be clearly available during landing, but its final location and Weight-field treatment need a presentation decision. Surrounding challenge UI can provide information without changing the supplied face; no asset edit is authorized now.

Landing Points, attempts, accuracy and rig score contributions belong to runtime challenge UI. Their layout is a dependency in [UI Redesign Plan](UI%20Redesign%20Plan.md), not additional printed card fields.

Preserve the separation of resolved numbers from presentation-only animations and attribution tooltips. Animations never change authoritative values or expose concealed data. Bait-zero stays neutral. Existing numeric pacing is preserved in the reference; it does not finalize animations for new scores.

## Role choice and placement

Catch/Bait choice occurs **after successful landing**. Use the supplied effect panels as mutually exclusive targets in that phase. Require an explicit choice rather than silently committing a default. Preserve restrained theme-approved selection feedback.

The controller owns phase, occurrence and pending choice. Refreshes must not commit, repeat effects or carry selection from another occurrence. Failure cannot open acquisition/placement commitment.

Choose a permitted insertion position and commit. The rig view reflects ordered membership, preserves existing relative order and offers no manual rearrangement. Keep insertion targets/restriction explanations outside face information. Exact preview/confirmation layout remains open. New creature contribution begins after landing and placement.

## Rarity hooks and tags

Four lower sockets represent rarity only:

| Rarity | Active hooks |
| --- | --- |
| Common | 1 |
| Uncommon | 2 |
| Rare | 3 |
| Legendary | 4 |

Inactive sockets stay visibly inactive. Hook count is not independent gameplay state.

Vocabulary review baseline: Fish, Predator, Schooling, Small, Heavy, Anchored, Armored, Elusive. Heavy remains under review. Supported supplied icons fill authored tag sockets in vocabulary order; unsupported/unused sockets stay empty. Tag-name tooltips use a separate non-intercepting layer. Do not invent icons or universal abilities from tag artwork.

## Compact rig cards

Discover's approved Tackle Apparatus theme is integrated separately under `Assets/UIThemes/TackleApparatus/`. Its reusable ivory/sage blank faces, portrait, clip and live name/role/sale-value fields follow the same separation contract. Catch/Bait face selection follows the stored role. Unprovided portraits remain empty; do not substitute unrelated illustrations. The cabinet has a clipped scrolling list and no three-card cap. Original nautical and Midnight Field Journal resources remain preserved for the other presentations.

The 14 November 2026 creature-art integration replaces species illustrations with the 22 selected sprites from Penpot's **10 - Creature Sprites** page. Creature definitions supply both compact and encounter illustrations; old illustration-specific rotations, offsets and full-card overlays are reset for these standalone sprites. The old precomposed Squid face yields to the reusable face and native information fields. Tackle and Journal creature views consume those definition references, while their frames and non-creature resources remain theme-owned. Transparent source margins may be cropped without resampling; preserve the complete silhouette, aspect ratio and clear border. Rig names fit one line inside the dark nameplate. This does not change creature rules, unlocks or the separate generic Landing marker.

Preserve compact geometry and separate rod, ring, clip and medallion siblings. Runtime content includes name, portrait, role-aware sale value, optional approved effect icon and Bait marker. Weight-region treatment awaits design.

Do not add full rules, tag lists, catch-order labels or modifier deltas. Physical layout shows current order; instance identity must survive insertion/removal. Interaction overlays stay outside face information. Scroll/stat feedback is presentation only and does not determine gameplay order.

## Hidden runtime state and events

Do not print lifecycle, selected-for-Release, hidden-state, modifier-state, positive/negative modifier or base-versus-modified badges on faces. Approved tooltips and external interaction feedback remain separate. Text must fit authored regions at supported resolutions.

Event masters show name, type, art and one rules panel; no Catch/Bait interaction, creature stats, rarity hooks or compact rig attachment. Event timing/content need review; old Descend/Surface timing does not define them.

Concealment timing relative to landing needs design review. Do not carry forward commit-only concealment, skip rules or ascent-history mechanics as approved gameplay. Preserve art/masking capabilities without adding hidden-state badges.
