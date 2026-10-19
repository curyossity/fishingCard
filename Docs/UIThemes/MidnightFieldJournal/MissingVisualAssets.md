# Midnight Field Journal — remaining visual dependencies

MFJ-SYNC-002 imports all five event designs through MFJ-R024. [Event integration evidence](Sync002/README.md) and [the creature roster](Creature%20Roster.md) supersede the older coverage snapshots. All eighteen refactored creatures and all five active coastal events have central artwork.

All **18 refactored creatures have central illustrations** in Unity. The detailed main frame, editable stat/rarity/effect fields, native role icons, selected-role glow and pointer-hover changes are complete. The previous central-frame paint blemish is resolved by the new frame. Fonts, projected Technique hand, modal shells and the comparison scene remain implemented and verified.

| Remaining gap | Scope | Boundary |
| --- | --- | --- |
| Four Technique illustrations | `clear_water`, `hold_steady`, `rerig`, `recast` | New standalone illustrations and editable Penpot cards are ready for review in MFJ-R031–R034; Unity slots remain empty until a requested integration. |
| Two ordinary central illustrations | `creature_cuttlefish`, `creature_flounder` | Outside the eighteen approved refactors. Do not substitute another species. |
| Special-card central illustrations | Two Apex, two treasures and one Location | Existing type and information rules remain binding. All five coastal events are complete. |
| Compact portrait coverage | Cuttlefish, Flounder, two Apex and two treasures | All eighteen approved creatures are mapped. Four original compact portraits remain; fourteen reuse their approved central illustrations with explicit compact framing. Other identities remain unillustrated as requested. |
| Kelp background source | 1672 × 941, opacity 0.45 over native 3840 × 2160 paper | Remains enlarged; a sharper source requires a separate art revision. |
| Main-frame source size | 1190 × 1322 displayed at 1260 × 1400 | Approximately 6% enlargement; actual 4K Editor output is captured. Native text and emblems scale independently. |
| Standalone build certification | Previous local Windows build-data process stalled | This synchronization validates Editor output and gameplay, not a standalone executable. |
| Legacy retirement | Shared definitions still reference old art/fonts | No old assets were deleted; dependency retirement remains a separate request. |

Remaining special IDs: `apex_harbor_leviathan`, `apex_stormjaw_tuna`, `treasure_pearl_cache`, `treasure_sunken_tackle_box`, `transition_next_waters`.

The canonical source catalog records **11 missing central/Technique identities** and a separate **6 missing compact identities**. Dedicated compact portraits remain Sardine, Clinging Crab, Glass Minnow and Small Octopus; fourteen additional approved creatures use explicit compact mappings to their existing illustrations. All art is selected by exact stable identity; neither shared gameplay data nor encounter/deck membership was changed. The user requested using only the eighteen approved creature illustrations. [Compact coverage verification](../../Validation/CompactPortraits/README.md).
