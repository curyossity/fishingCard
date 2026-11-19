# Five-tag runtime and creature abilities

Implemented and validated 19 November 2026 in Unity 6000.5.9f1.

The installed game uses five tags, one tag and one primary ability per creature, four-of-five Harbor preparation and no secondary preference from a pair's unmatched symbol. Individual abilities and level progressions are editable in game definitions; this record does not duplicate their catalog.

## Validation

| Check | Passed assertions |
| --- | --- |
| `SlotMachineRuleChecks.ValidateRules()` | 69 |
| `LandingChallengeChecks.ValidateRules()` | 64 |
| `RigBuildingChecks.ValidateRules()` | 76 |
| `FiveTagAbilityChecks.Validate()` | 374 |
| `TackleDiscoverArtChecks.ValidateAssets()` | 155 |
| `RigBuildingChecks.ValidateAssets()` | 33 |
| `TackleLandingArtChecks.ValidateAssets()` | 105 |
| `TackleReelSymbolSetup.Validate()` | 37 |
| Live Harbor / Discover / respin / Landing handoff | 80 |
| Live Landing / placement / automatic duplicates / failure payout / next expedition | 100 |

The content checks cover all installed creature/Apex definitions, biome coverage, conditional scoring, source attribution, positional boundaries, previews, duplicate progression, terminal attempt grants, encounter resets and deterministic bounded assistance. The live checks use the existing comparison scene and disable progression writes; they do not reset or delete user saves.

Captures are under [Discover](Discover/) and [Build](Build/), including Build at 1280 × 800, 1920 × 1080 and 2560 × 1080. [Live verification](Build/LiveVerification.json) records successful placement, duplicate upgrades, complete failure payout and an empty next-run rig. Earlier validation directories remain unchanged.

## Content and artwork boundaries

Effect coefficients, level intervals, caps and the new creatures' rarity/base values are initial editable tuning. The supplied creature-art library remains unchanged. New creature definitions have no portrait rather than borrowing an unrelated illustration; their artwork is still required. Apex eligibility/progression rules, biome completion and Technique design are unchanged.

`FiveTagPresentationSetup.Install()` migrates saved controls/mappings without rebuilding machine artwork. Regeneration uses the five-tag vocabulary. The creature importer validates existing artwork provenance while allowing new definitions with no supplied illustration.
