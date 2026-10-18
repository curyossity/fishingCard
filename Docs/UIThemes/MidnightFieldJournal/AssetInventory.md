# Source asset inventory

This is the UI-01 source snapshot. UI-02 has now imported the selected runtime assets; see [UI02ImportedAssets.json](UI02ImportedAssets.json) for current paths and GUIDs and [UI02.md](UI02.md) for the handoff. Historical source paths in the original manifest remain provenance records.

Generated from actual file headers and SHA-256 hashes. Full paths, provenance, card identities and target locations are in [AssetManifest.json](AssetManifest.json). All target paths are planned for UI-02; no Unity assets were imported by UI-01.

| Source key | Native pixels | State | 4K display canvas |
| --- | --- | --- | --- |
| ConceptReference | 1672 × 941 | reference-only | reference/archive |
| PaperBackground4K | 3840 × 2160 | selected | [3840, 2160] |
| Background | 1672 × 941 | selected | [3840, 2160] |
| CreatureTemplate | 1122 × 1402 | assembled-candidate | [1040, 1300] |
| EventTemplate | 1122 × 1402 | assembled-candidate | [1040, 1300] |
| SmallOctopusPortrait | 1254 × 1254 | selected | [570, 570] |
| DescendButton | 2001 × 786 | selected | [880, 346] |
| SurfaceButton | 1983 × 793 | selected | [880, 352] |
| LoadEmpty | 128 × 96 | selected | [77, 58] |
| LoadFilled | 128 × 96 | selected | [77, 58] |
| LoadDanger | 128 × 96 | selected | [77, 58] |
| CatchFrameV2 | 1659 × 948 | selected | [680, 382.022472] |
| SardineV2 | 1983 × 793 | selected | [552, 221] |
| CrabV2 | 1728 × 910 | selected | [440, 250] |
| MinnowV2 | 1983 × 793 | selected | [552, 221] |
| BrassConnectorV3 | 1024 × 1536 | selected | [104, 156] |
| TechniqueFrameV2 | 1224 × 1285 | selected | [620, 640] |
| CutLooseV2 | 1681 × 936 | selected | [520.8, 291.2] |
| SlipPastV2 | 1681 × 936 | selected | [520.8, 291.2] |
| SteadyLineV2 | 1683 × 935 | selected | [520.8, 291.2] |
| PrizeHookV2 | 1679 × 937 | selected | [520.8, 291.2] |
| TechniquePerspectiveV3 | 2129 × 738 | reference-only | [2500, 866.604039] |
| CompactCatchTemplate | 1536 × 1024 | superseded | reference/archive |
| TechniqueTemplate | 1086 × 1448 | superseded | reference/archive |
| Sardine | 1774 × 887 | superseded | reference/archive |
| Crab | 1374 × 1145 | superseded | reference/archive |
| Minnow | 1774 × 887 | superseded | reference/archive |
| CutLooseArt | 1254 × 1254 | superseded | reference/archive |
| SlipPastArt | 1312 × 1199 | superseded | reference/archive |
| SteadyLineArt | 1774 × 887 | superseded | reference/archive |
| PrizeHookArt | 1024 × 1536 | superseded | reference/archive |

## Active card coverage

| Stable Unity ID | Type | New theme illustration |
| --- | --- | --- |
| creature_cleaner_shrimp | 3 | Not supplied for this theme |
| clear_water | 0 | Not supplied for this theme |
| creature_clinging_crab | 3 | CrabV2 |
| creature_coastal_cod | 3 | Not supplied for this theme |
| cut_loose | 0 | CutLooseV2 |
| creature_cuttlefish | 3 | Not supplied for this theme |
| creature_flounder | 3 | Not supplied for this theme |
| fresh_shell_fragments | 7 | Not supplied for this theme |
| creature_garfish | 3 | Not supplied for this theme |
| gentle_upwelling | 6 | Not supplied for this theme |
| creature_glass_minnow | 3 | MinnowV2 |
| creature_mackerel | 3 | Not supplied for this theme |
| apex_harbor_leviathan | 8 | Not supplied for this theme |
| hold_steady | 0 | Not supplied for this theme |
| creature_jellyfish | 3 | Not supplied for this theme |
| creature_lionfish | 3 | Not supplied for this theme |
| loose_knot | 7 | Not supplied for this theme |
| creature_moonlit_ray | 3 | Not supplied for this theme |
| creature_mussel | 3 | Not supplied for this theme |
| treasure_pearl_cache | 4 | Not supplied for this theme |
| prize_hook | 0 | PrizeHookV2 |
| rerig | 0 | Not supplied for this theme |
| recast | 0 | Not supplied for this theme |
| reef_snag | 5 | Not supplied for this theme |
| creature_sardine | 3 | SardineV2 |
| creature_sea_anemone | 3 | Not supplied for this theme |
| creature_sea_bass | 3 | Not supplied for this theme |
| creature_sea_urchin | 3 | Not supplied for this theme |
| silt_burst | 5 | Not supplied for this theme |
| slip_past | 0 | SlipPastV2 |
| creature_small_octopus | 3 | SmallOctopusPortrait |
| creature_spiny_lobster | 3 | Not supplied for this theme |
| creature_squid | 3 | Not supplied for this theme |
| steady_line | 0 | SteadyLineV2 |
| apex_stormjaw_tuna | 8 | Not supplied for this theme |
| treasure_sunken_tackle_box | 4 | Not supplied for this theme |
| transition_next_waters | 10 | Not supplied for this theme |
| creature_tidepool_eel | 3 | Not supplied for this theme |

Type numbers follow CardDefinition.cs: 0 Technique, 3 Creature, 4 Treasure, 5 Hazard, 6 Environment, 7 Opportunity, 8 Apex, 9 generic encounter, 10 Location. Existing gameplay artwork references are preserved in the JSON; lack of new theme artwork does not imply a missing old-design illustration.

## Classification boundaries

- Selected and assembled-candidate images: new-theme-only. Sources remain in staging or draft folders until UI-02.
- Superseded images: earlier new-theme-only work; excluded from the new runtime selection, retained until a separate cleanup.
- Full concept and four-card perspective composite: reference-only; never runtime backgrounds, cards or hands.
- Card definitions and runtime rules: shared gameplay content, never part of a design-art deletion set.
- Existing Fonts, licenses, atlas textures, font materials and fallbacks: shared until full transitive dependency isolation is complete.
- Other legacy art: deletion classification is pending a Unity dependency audit. No broad legacy folder is declared safe to delete.
