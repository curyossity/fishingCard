> **Cleanup execution update:** Complete: 64 obsolete items and two empty pages removed; current library, style reference and archive organized. See [execution results](Execution.md). The review below is the original pre-cleanup snapshot.

# Penpot cleanup — review before deletion

**Read-only audit: 31 October 2026. Nothing has been moved, renamed or deleted.**
File: **New File 1**, revision **239**. Five pages, 177 top-level items; 16,311 hierarchy nodes and 95 prototype interactions checked. The local component library is empty: these are native boards/groups and image resources, not formal library components.

The current Unity integration records identify accepted designs, but `verified/imported` alone does not mean a design is still current. R035/R036 and R046 were integrated historically and later replaced. Several accepted tooltip boards still say R026 because R027 updated them in place.

## Proposal at a glance

| Classification | Top-level items | Meaning |
| --- | ---: | --- |
| KEEP_CURRENT | 79 | Keep: accepted current designs and component review/source boards. |
| KEEP_SHARED | 16 | Keep: shared or mixed reusable resources and required prototype destinations. |
| KEEP_REFERENCE | 2 | Keep as reference: original mood mockup and one general assembled gameplay view. |
| CONSOLIDATE_SOURCE | 4 | Duplicate source drops: retain a canonical copy, then remove the duplicate. |
| ARCHIVE_OPTIONAL | 12 | Unused legacy/comparison screens: proposed archive or removal after review. |
| REMOVE_CANDIDATE | 64 | Unused/replaced designs proposed for removal; not approved for deletion yet. |

The empty Page 1 is an additional page-removal candidate and is not counted as a top-level item. An entire page is not a deletion candidate merely because some boards on it are old.

## What we currently use — keep

| Area | Keep |
| --- | --- |
| Main creature cards | R002 main card/master plus all 18 approved creature illustrations and editable card examples, R003–R019. |
| Events | R020–R024: Reef Snag, Silt Burst, Gentle Upwelling, Loose Knot, Fresh Shell Fragments; event master, illustration sources and rosters. |
| Catch Chain | Detailed v2 chain cards, brass connectors and their source artwork. |
| Techniques | All eight approved Technique illustrations/frames; engraved initial four plus R031–R034; R030 editable/projected fan. |
| Catch Chain tooltip | R027 accepted no-pointer design, including the boards/master still named R026 and its working prototype. |
| Modified-stat tooltip | R045 compact flat-icon frame, blank 9-slice master and arrow-only scroll states/prototypes. |
| Actions / Line Load | R037–R038 parchment/card-style controls and reusable sources. |
| Opening / Depth / Hand backing | R039 boat, R040 depth guide, R041 backing sheet, including independent artwork. |
| Timing bar | R051 Lantern Focus with anchor cursor and rising precision highlights. |
| Harbor | R053 board with the R057 sharp 4K framing update. |
| Tackle Shop | R055 board and engraved rod/reel/hook artwork with R057 sharp 4K framing. |
| Technique Collection | R056 and its actual in-run card artwork with R057 sharp 4K framing. |
| Journal | R058 ornate Journal and separate Journal sources. |
| Shared masters | R057 DarkFrameMaster/PaperFrameMaster; current equipment/Technique image resources; mixed original source sheets until their used parts are separated. |

## What is no longer used — proposed cleanup groups

| Group | Why it is no longer current |
| --- | --- |
| Page 1 | Empty. |
| Fantasy Card — MCP Verification | Connection-test Moonlit Relic demo, unrelated to the game. |
| Gameplay Reference — Editable | Entire legacy nautical-style UI. Optional archive if this original style should remain available as reference. |
| Extra GAMEPLAY A/B/C/D/E/F comparisons | Earlier assembled reference versions. Preserve component masters separately; retain G as one general reference. |
| SUPERSEDED / MFJ-R025 tooltips | Replaced by the accepted R027 engraved tooltip. |
| R035 Line Load and R036 marine hardware buttons | Replaced by R037–R038 parchment controls. |
| R042–R044 modifier-tooltip studies, overlays and prototypes | Replaced by R045. R044 reused boards whose labels still say R043. |
| R046 rarity-bar review, animation states, parts and GAMEPLAY I/J/K/L | Rarity mechanic replaced by the tag bar. Existing Unity animation behavior does not require retaining every Penpot storyboard frame. |
| R047 / R048 / R049 tag-bar studies | Rejected/unselected alternatives and mid-run comparisons. |
| R050 Lantern Focus refinement | Replaced by R051 anchor cursor/rising precision version. |
| R052 Harbor/Shop/Collection/Journal/library | First hub suite, replaced by later ornate screens. |
| R054 Tackle Shop and old loose equipment/panorama drops | Replaced by R055 engraved items and current ornate scenery. |
| R057 comparison / original raster framing — Harbor, Shop, Collection | Blurry pre-correction copies, not the sharp 4K current screens. |

## Proposed organization — no changes applied

1. **Current Screens:** Harbor, Tackle Shop, Technique Collection, Journal, and one clearly labelled gameplay reference.
2. **Current Cards:** creature/event/Technique collections and their editable templates.
3. **Current Components:** Catch Chain, controls, timing bar, tooltips, depth guide and backing sheet.
4. **Source Art & Masters:** independent artwork and reusable frames, separate from screen review boards.
5. **Archive:** original static mockup and any old comparisons you elect to retain; remove the approved obsolete groups after review.

There is no single gameplay board that combines every latest component: G predates the final stat tooltip and tag bar. Before deleting all comparison screens, assemble one up-to-date gameplay reference from the kept parts. This is a proposed follow-up, not work performed by this audit.

## Complete item-by-item review

The table below uses actual live names and IDs. IDs distinguish duplicates with the same filename. Full machine-readable classification and reasons are in [Review.json](Review.json); the untouched live inventory is in [LiveInventory.json](LiveInventory.json). Prototype connections are in [PrototypeLinks.json](PrototypeLinks.json).

### Page 1

Page ID: `9e056cfe-f83f-81f8-8008-cc523ff45212` · **REMOVE_EMPTY_PAGE_CANDIDATE**

No shapes.

### Fantasy Card — MCP Verification

Page ID: `55e98d0e-b63f-8025-8008-cc53feeed652` · **REMOVE_DEMO_PAGE_CANDIDATE**

| Actual Penpot item | Recommendation | ID |
| --- | --- | --- |
| Fantasy Card / Moonlit Relic | REMOVE_CANDIDATE | `55e98d0e-b63f-8025-8008-cc5424e80374` |

### Gameplay Reference — Editable

Page ID: `8a17fa10-389b-804e-8008-cc5591039a4e` · **ARCHIVE_LEGACY_PAGE_OPTIONAL**

| Actual Penpot item | Recommendation | ID |
| --- | --- | --- |
| Original reference / locked comparison | ARCHIVE_OPTIONAL | `8a17fa10-389b-804e-8008-cc559f8249d4` |
| Gameplay layout / editable 1920 × 1080 | ARCHIVE_OPTIONAL | `8a17fa10-389b-804e-8008-cc55bf618f5e` |
| Source artwork / reusable | ARCHIVE_OPTIONAL | `8a17fa10-389b-804e-8008-cc55bf667282` |
| Previous right sidebar / comparison | ARCHIVE_OPTIONAL | `8a17fa10-389b-804e-8008-cc5a092d4141` |
| Catch Rig / individual Unity parts | ARCHIVE_OPTIONAL | `8a17fa10-389b-804e-8008-cc5d983ad2a9` |
| Descend / Hover state | ARCHIVE_OPTIONAL | `8a17fa10-389b-804e-8008-cc6d5c4d3b2e` |

### Concept B — Midnight Field Journal (Static)

Page ID: `bae680cd-9225-8018-8008-cdd5868103f5` · **KEEP_ONE_STYLE_REFERENCE**

| Actual Penpot item | Recommendation | ID |
| --- | --- | --- |
| Midnight Field Journal / single static mockup | KEEP_REFERENCE | `bae680cd-9225-8018-8008-cdd5885a6014` |

### Concept B — Reusable Assembly (4K)

Page ID: `bae680cd-9225-8018-8008-cddd6a6f3825` · **KEEP_AND_ORGANIZE**

| Actual Penpot item | Recommendation | ID |
| --- | --- | --- |
| GAMEPLAY — assembled reusable parts / 3840 × 2160 | ARCHIVE_OPTIONAL | `bae680cd-9225-8018-8008-cdde36733e6f` |
| GAMEPLAY B — techniques laid on table / perspective preview | ARCHIVE_OPTIONAL | `bae680cd-9225-8018-8008-cdead5028f97` |
| GAMEPLAY D / Engraved tooltip / MFJ-R027 | ARCHIVE_OPTIONAL | `bae680cd-9225-8018-8008-d0061921543d` |
| GAMEPLAY E / Parchment controls / MFJ-R037–R038 | ARCHIVE_OPTIONAL | `bae680cd-9225-8018-8008-d07d339bae02` |
| GAMEPLAY F / Illustrated depth guide / MFJ-R040 | ARCHIVE_OPTIONAL | `bae680cd-9225-8018-8008-d08a9ab76de5` |
| GAMEPLAY G / Technique hand backing sheet / MFJ-R041 | KEEP_REFERENCE | `bae680cd-9225-8018-8008-d0a91363085a` |
| GAMEPLAY H / Value modifier tooltip / MFJ-R042 | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d1c31b44a376` |
| PROTOTYPE / Hover modified Value / MFJ-R042 | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d1c31fa6ec4b` |
| PROTOTYPE / Engraved Catch Chain tooltip / MFJ-R027 | KEEP_CURRENT | `bae680cd-9225-8018-8008-d00618505dcb` |
| GAMEPLAY C / Reef Snag event / MFJ-R020 | ARCHIVE_OPTIONAL | `bae680cd-9225-8018-8008-cf65f248e1e2` |
| COMPONENT REVIEW — independent templates / not a screenshot | KEEP_SHARED | `bae680cd-9225-8018-8008-cdde40529e8e` |
| SOURCE ASSETS — independent originals | KEEP_SHARED | `bae680cd-9225-8018-8008-cdde421c05c9` |
| CATCH CHAIN CARD v2 — mockup detail study | KEEP_CURRENT | `bae680cd-9225-8018-8008-cde0da1d370c` |
| CATCH CHAIN — complete assembled v2 | KEEP_CURRENT | `bae680cd-9225-8018-8008-cde2b511f13d` |
| TECHNIQUES — engraved hand / mockup detail | KEEP_CURRENT | `bae680cd-9225-8018-8008-cde85bfb5c79` |
| MFJ-R002 / Main creature card — mockup detail review | KEEP_CURRENT | `bae680cd-9225-8018-8008-cee1b54ddb82` |
| MFJ-R002 / Independent source art | KEEP_CURRENT | `bae680cd-9225-8018-8008-cee1b64ceecf` |
| MFJ-R003 / Cod — detailed creature card | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf09f3ff1e59` |
| MFJ-R004 / Garfish / detailed creature card | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf0f6729044a` |
| MFJ-R004 / Garfish / separate illustration source | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf0f6787a406` |
| MFJ-R005 / Glass Minnow / detailed creature card | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf0f935a97c4` |
| MFJ-R005 / Glass Minnow / separate illustration source | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf0f93bd126d` |
| MFJ-R006 / Jellyfish / detailed creature card | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf0faf77ee33` |
| MFJ-R006 / Jellyfish / separate illustration source | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf0fafe405f7` |
| MFJ-R007 / Lionfish / detailed creature card | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf1b7b9c8773` |
| MFJ-R007 / Lionfish / separate illustration source | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf1b7c5effd8` |
| MFJ-R008 / Mackerel / detailed creature card | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf1bcde4302e` |
| MFJ-R008 / Mackerel / separate illustration source | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf1bce5f4ce3` |
| MFJ-R009 / Moonlit Ray / detailed creature card | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf1bf62e7a36` |
| MFJ-R009 / Moonlit Ray / separate illustration source | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf1bf69ba01a` |
| MFJ-R010 / Mussel / detailed creature card | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf1c19384688` |
| MFJ-R010 / Mussel / separate illustration source | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf1c19b520b8` |
| MFJ-R011 / Sardine / detailed creature card | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf1c3e4397e4` |
| MFJ-R011 / Sardine / separate illustration source | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf1c3eb155ab` |
| MFJ-R012 / Cleaner Shrimp / detailed creature card | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf347ef37602` |
| MFJ-R012 / Cleaner Shrimp / separate illustration source | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf347f8f2574` |
| MFJ-R013 / Clinging Crab / detailed creature card | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf3481346f34` |
| MFJ-R013 / Clinging Crab / separate illustration source | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf34819ae5c4` |
| MFJ-R014 / Sea Anemone / detailed creature card | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf348378e765` |
| MFJ-R014 / Sea Anemone / separate illustration source | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf3483fa3603` |
| MFJ-R015 / Sea Bass / detailed creature card | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf34857f82ac` |
| MFJ-R015 / Sea Bass / separate illustration source | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf3486054010` |
| MFJ-R016 / Sea Urchin / detailed creature card | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf348891dbf4` |
| MFJ-R016 / Sea Urchin / separate illustration source | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf34893f0aaf` |
| MFJ-R017 / Spiny Lobster / detailed creature card | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf348b8ecb45` |
| MFJ-R017 / Spiny Lobster / separate illustration source | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf348c2f1a6c` |
| MFJ-R018 / Squid / detailed creature card | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf348e983db3` |
| MFJ-R018 / Squid / separate illustration source | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf348f3681bc` |
| MFJ-R019 / Tidepool Eel / detailed creature card | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf3490e909a3` |
| MFJ-R019 / Tidepool Eel / separate illustration source | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf349181fdae` |
| MFJ-R020 / Reef Snag / detailed EVENT card | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf65f1a30046` |
| MFJ-R024 / Fresh Shell Fragments / detailed EVENT card | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf687f35e870` |
| MFJ-R023 / Loose Knot / detailed EVENT card | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf687d96a259` |
| MFJ-R022 / Gentle Upwelling / detailed EVENT card | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf687c2dd7ec` |
| MFJ-R021 / Silt Burst / detailed EVENT card | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf687a9a6f4f` |
| MFJ-R020 / Event frame and independent Reef Snag art | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf65f2346433` |
| MFJ-R021 / Silt Burst / separate illustration source | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf687ab47057` |
| MFJ-R022 / Gentle Upwelling / separate illustration source | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf687c5f28e5` |
| MFJ-R023 / Loose Knot / separate illustration source | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf687dc371b4` |
| MFJ-R024 / Fresh Shell Fragments / separate illustration source | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf687f67e74b` |
| EVENT ROSTER / Hazards and Environment / Unity integration | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf68a5c60f8c` |
| EVENT ROSTER / Opportunities / Unity integration | KEEP_CURRENT | `bae680cd-9225-8018-8008-cf68a5f46780` |
| MFJ-R026 / Engraved tooltip / Clinging Crab Catch | KEEP_CURRENT | `bae680cd-9225-8018-8008-d00614dd5194` |
| SUPERSEDED / MFJ-R025 / Clinging Crab Catch | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d0094ff4e6c7` |
| MFJ-R026 / Engraved tooltip / Glass Minnow Bait | KEEP_CURRENT | `bae680cd-9225-8018-8008-d00616026a38` |
| SUPERSEDED / MFJ-R025 / Glass Minnow Bait | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d009513f1f95` |
| MFJ-R026 / Engraved tooltip / Sardine Catch | KEEP_CURRENT | `bae680cd-9225-8018-8008-d00617505c88` |
| SUPERSEDED / MFJ-R025 / Sardine Catch | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d009522a12b1` |
| MFJ-R026 / Independent engraved tooltip master | KEEP_CURRENT | `bae680cd-9225-8018-8008-d00953150b66` |
| MFJ-R030 / Technique fan / editable planar sources | KEEP_CURRENT | `bae680cd-9225-8018-8008-d0105575afda` |
| MFJ-R030 / Technique fan / independent projected cards | KEEP_CURRENT | `bae680cd-9225-8018-8008-d01110225b74` |
| TECHNIQUE ROSTER / New engraved cards / MFJ-R031–R034 | KEEP_CURRENT | `bae680cd-9225-8018-8008-d04eef43c54d` |
| MFJ-R031–R034 / Separate Technique illustrations | KEEP_CURRENT | `bae680cd-9225-8018-8008-d04eef7634b4` |
| MFJ-R035 / Independent engraved Line Load housing | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d05df1e230d1` |
| MFJ-R035 / Engraved Line Load / state review | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d05df1f67423` |
| MFJ-R036 / Independent marine action-button housing | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d0617f174fec` |
| MFJ-R036 / Descend + Surface / marine hardware review | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d0617f36896a` |
| MFJ-R037–R038 / Independent parchment control sources | KEEP_CURRENT | `bae680cd-9225-8018-8008-d07d2d33d218` |
| MFJ-R037–R038 / Parchment controls / card-style alternative | KEEP_CURRENT | `bae680cd-9225-8018-8008-d07d2d621005` |
| MFJ-R039 / Opening boat / independent original art | KEEP_CURRENT | `bae680cd-9225-8018-8008-d086c7786357` |
| MFJ-R039 / Run opening / fisherman boat card | KEEP_CURRENT | `bae680cd-9225-8018-8008-d086c7a1bbc3` |
| MFJ-R040 / Independent engraved depth vignettes | KEEP_CURRENT | `bae680cd-9225-8018-8008-d08a980a65fb` |
| MFJ-R040 / Depth guide / mockup-style coral timeline | KEEP_CURRENT | `bae680cd-9225-8018-8008-d08a985b4ebe` |
| MFJ-R041 / Independent Technique hand backing sheet | KEEP_CURRENT | `bae680cd-9225-8018-8008-d0a9132b5870` |
| MFJ-R041 / Sheet source review / no cards baked in | KEEP_CURRENT | `bae680cd-9225-8018-8008-d0a91419d889` |
| MFJ-R042 / Independent modifier-tooltip frame | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d1c312ce8f8a` |
| MFJ-R042 / Stat modifier tooltips / editable review | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d1c312ec4bb6` |
| OVERLAY / Value modifiers / MFJ-R042 | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d1c443614e62` |
| MFJ-R043 / Compact modifier tooltips / 1–3 sources | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d1d61e2e0eeb` |
| MFJ-R043 / Overflow / three visible + scrolling | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d1d631064172` |
| PROTOTYPE / R043 / scroll down | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d1d6441a5fcd` |
| PROTOTYPE / R043 / scroll up | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d1d64433eb39` |
| MFJ-R043 / 9-slice assembly / fixed corners | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d1d6446675ed` |
| MFJ-R045 / Compact tooltip / flat icons + corner engraving | KEEP_CURRENT | `bae680cd-9225-8018-8008-d1f8e5a830ea` |
| MFJ-R045 / Arrow-only scrolling / first · middle · last | KEEP_CURRENT | `bae680cd-9225-8018-8008-d1f8eb58cd20` |
| PROTOTYPE / R045 / scroll state 0 | KEEP_CURRENT | `bae680cd-9225-8018-8008-d1f8f4e9dda4` |
| PROTOTYPE / R045 / scroll state 1 | KEEP_CURRENT | `bae680cd-9225-8018-8008-d1f8f504cab5` |
| PROTOTYPE / R045 / scroll state 2 | KEEP_CURRENT | `bae680cd-9225-8018-8008-d1f8f51f5f10` |
| MFJ-R045 / Blank master / 64px nine-slice borders | KEEP_CURRENT | `bae680cd-9225-8018-8008-d1f8f54b16ca` |
| MFJ-R046 / Rarity timing bar / design review | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d863692957f6` |
| PROTOTYPE / R046 / Sweep 01 | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d8638f85a121` |
| PROTOTYPE / R046 / Sweep 02 | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d863952301d3` |
| PROTOTYPE / R046 / Sweep 03 | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d8639c21d6ad` |
| PROTOTYPE / R046 / Sweep 04 | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d863a28b6c13` |
| PROTOTYPE / R046 / Sweep 05 | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d863a940fe56` |
| PROTOTYPE / R046 / Sweep 06 | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d863b05c9237` |
| PROTOTYPE / R046 / Sweep 07 | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d863b7c48167` |
| PROTOTYPE / R046 / Sweep 08 | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d863bdf8a086` |
| PROTOTYPE / R046 / Sweep 09 | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d863c63a9df7` |
| PROTOTYPE / R046 / Sweep 10 | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d863cd2030c0` |
| PROTOTYPE / R046 / Sweep 11 | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d863d458f83a` |
| PROTOTYPE / R046 / Sweep 12 | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d863dc4a48ef` |
| PROTOTYPE / R046 / Sweep 13 | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d863e547bc62` |
| PROTOTYPE / R046 / Sweep 14 | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d863ecbc489b` |
| PROTOTYPE / R046 / Stopped Common | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d863f4fc6f19` |
| PROTOTYPE / R046 / Stopped Uncommon | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d863fe964834` |
| PROTOTYPE / R046 / Stopped Rare | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d86406d660e3` |
| GAMEPLAY J / Rare creature revealed after timing / R046 | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d8640f15ebc8` |
| GAMEPLAY I / Rarity timing phase / R046 | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d8640fcd648c` |
| MFJ-R046 / Reusable parts / native Unity handoff | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d8641c80b574` |
| GAMEPLAY K / Common Cod reveal / R046 | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d865098b5f41` |
| GAMEPLAY L / Uncommon Small Octopus reveal / R046 | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d8650df0234d` |
| PROTOTYPE / R046 / Exact stop Common 0.04 | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d865325f5866` |
| PROTOTYPE / R046 / Exact stop Common 0.2 | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d8654119f2fe` |
| PROTOTYPE / R046 / Exact stop Common 0.52 | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d86558d28746` |
| PROTOTYPE / R046 / Exact stop Common 0.67 | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d865668b9eef` |
| PROTOTYPE / R046 / Exact stop Uncommon 0.9 | REMOVE_CANDIDATE | `bae680cd-9225-8018-8008-d865798766fb` |
| MFJ-R047 / Dynamic Fishing Tag Bar / three static variations | REMOVE_CANDIDATE | `0ee2d4ff-05c9-80cd-8008-db1942f00ae7` |
| MFJ-R048 / Quiet tag bars / three initial-state ideas | REMOVE_CANDIDATE | `0ee2d4ff-05c9-80cd-8008-db1f8145f4b7` |
| MFJ-R049 / Fishing instruments / three new forms | REMOVE_CANDIDATE | `0ee2d4ff-05c9-80cd-8008-dc4a1d5c480f` |
| MFJ-R050 / Lantern Focus / light parchment refinement | REMOVE_CANDIDATE | `0ee2d4ff-05c9-80cd-8008-dc52279538d6` |
| MFJ-R051 / Lantern Focus / anchor cursor and rising precision | KEEP_CURRENT | `0ee2d4ff-05c9-80cd-8008-dc54976c9872` |
| HarborPanorama.png | REMOVE_CANDIDATE | `d76f6cbf-1fc5-8066-8008-de811c62398a` |
| TideglassReel.png | REMOVE_CANDIDATE | `d76f6cbf-1fc5-8066-8008-de81e6709a76` |
| CutLoose.png | CONSOLIDATE_SOURCE | `d76f6cbf-1fc5-8066-8008-de81e8d1c288` |
| SlipPast.png | CONSOLIDATE_SOURCE | `d76f6cbf-1fc5-8066-8008-de81eb799a87` |
| SteadyLine.png | CONSOLIDATE_SOURCE | `d76f6cbf-1fc5-8066-8008-de81ee4b23fd` |
| PrizeHook.png | CONSOLIDATE_SOURCE | `d76f6cbf-1fc5-8066-8008-de81f0851a96` |
| MFJ-R052 / Harbor design collection title | REMOVE_CANDIDATE | `d76f6cbf-1fc5-8066-8008-de832a7f59c0` |
| MFJ-R052 / 01 — Harbor / 3840 × 2160 | REMOVE_CANDIDATE | `d76f6cbf-1fc5-8066-8008-de832b61fc2d` |
| MFJ-R052 / 02 — Tackle Shop / 3840 × 2160 | REMOVE_CANDIDATE | `d76f6cbf-1fc5-8066-8008-de83518583ea` |
| MFJ-R052 / 03 — Technique Collection / 3840 × 2160 | REMOVE_CANDIDATE | `d76f6cbf-1fc5-8066-8008-de8437dc384a` |
| MFJ-R052 / 04 — Discovery Journal / 3840 × 2160 | REMOVE_CANDIDATE | `75555645-d407-8067-8008-de8623eba843` |
| MFJ-R052 / 05 — Reusable Unity element library | REMOVE_CANDIDATE | `75555645-d407-8067-8008-de86a08036bb` |
| DriftwoodRod.png | REMOVE_CANDIDATE | `75555645-d407-8067-8008-de8860eb1e21` |
| BarbedHook.png | REMOVE_CANDIDATE | `75555645-d407-8067-8008-de8862e7702f` |
| MFJ-R053 — Harbor / Ornate card-style study | KEEP_CURRENT | `75555645-d407-8067-8008-de8ff58e2639` |
| R057 comparison / original raster framing / harbor | REMOVE_CANDIDATE | `75555645-d407-8067-8008-debc97203dbe` |
| MFJ-R054 — Tackle Shop / Ornate card-style study | REMOVE_CANDIDATE | `75555645-d407-8067-8008-de9192c78c73` |
| MFJ-R055 — Tackle Shop / Engraved equipment | KEEP_CURRENT | `75555645-d407-8067-8008-de9b9c9710d1` |
| R057 comparison / original raster framing / shop | REMOVE_CANDIDATE | `75555645-d407-8067-8008-dea8242484c7` |
| RodEngraved.png | KEEP_SHARED | `75555645-d407-8067-8008-de9b972ff90c` |
| ReelEngraved.png | KEEP_SHARED | `75555645-d407-8067-8008-de9b99677d11` |
| HookEngraved.png | KEEP_SHARED | `75555645-d407-8067-8008-de9b9b3a9112` |
| TechniqueFrame.png | KEEP_SHARED | `75555645-d407-8067-8008-de9cef76aab3` |
| CutLoose.png | KEEP_SHARED | `75555645-d407-8067-8008-de9cf120757d` |
| SlipPast.png | KEEP_SHARED | `75555645-d407-8067-8008-de9cf2d49de0` |
| ClearWater.png | KEEP_SHARED | `75555645-d407-8067-8008-de9cf4970a6a` |
| HoldSteady.png | KEEP_SHARED | `75555645-d407-8067-8008-de9cf70d6a10` |
| Rerig.png | KEEP_SHARED | `75555645-d407-8067-8008-de9cf8a41a16` |
| SteadyLine.png | KEEP_SHARED | `75555645-d407-8067-8008-de9cfb3ca568` |
| Recast.png | KEEP_SHARED | `75555645-d407-8067-8008-de9cfcd8d253` |
| PrizeHook.png | KEEP_SHARED | `75555645-d407-8067-8008-de9cfe5b9d50` |
| MFJ-R056 — Technique Collection / In-run card faces | KEEP_CURRENT | `75555645-d407-8067-8008-de9d62501389` |
| R057 comparison / original raster framing / collection | REMOVE_CANDIDATE | `75555645-d407-8067-8008-debcce9e38b3` |
| DarkFrameMaster.png | KEEP_SHARED | `75555645-d407-8067-8008-dea9cd9a2881` |
| PaperFrameMaster.png | KEEP_SHARED | `75555645-d407-8067-8008-dea9cfcb5568` |
| MFJ-R058 — Discovery Journal / Ornate 4K | KEEP_CURRENT | `75555645-d407-8067-8008-dedef8b3f171` |
| MFJ-R058 / Reusable Journal sources | KEEP_CURRENT | `75555645-d407-8067-8008-dedff4d4b1dd` |

## Before any deletion

No deletion is authorized by this list. After the user reviews it, export a backup of the file, recheck its revision/targets, and use the exact reviewed IDs. Do not delete Unity assets, revision exports or source/license records as part of Penpot cleanup. Keep required prototype destinations with their retained source boards.
