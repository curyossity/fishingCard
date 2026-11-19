# Creature tooltip integration

19 November 2026. Runtime integration of the [editable tooltip proposal](../../UIThemes/TackleApparatusConcept/CardTooltips/README.md).

The installed native tooltip supports the left cabinet in Discover, Landing and Build, plus the horizontal insertion tray. It shows identity, rarity, tag, current level, resolved accumulated sale value, ability name and frozen current-level rules. Relevant contiguous/consecutive wording receives a short explanation. Tray cards use the selected placement snapshots and label them **After placement**; cabinet values remain committed.

One reusable `CardTooltipOverlay.prefab` provides editable native geometry and TMP fields. It uses the supplied Press Start 2P and VT323 fonts, brass accents, a 460-reference-pixel width and content-driven height. The source outline and popup are outside scroll masks, clamp to the safe area and do not intercept raycasts. All original card and machine artwork is preserved.

Pointer hover opens after 180 ms and closes 80 ms after exit. Native keyboard/gamepad focus opens immediately; navigate up from an insertion gap to inspect its adjacent tray card, then down to return. Build's independent placement shortcuts ignore inspection focus. Escape/gamepad Back, scrolling, recycling, source removal and phase exit dismiss the tooltip. Card inspection does not submit a gameplay command.

Validation:

- 45 serialized asset checks: all three phase hosts, references, overlay isolation, raycast pass-through and card master targets.
- 55 live checks through actual Landing into Build: hover/focus timing, upgraded Level 3 rules, preview labeling, visible insertion-button raycasts, unchanged rig revision/value/occurrence, stale-source events, recycling, scrolling, removal and phase exit.
- Captures at 1280 × 800, 1920 × 1080 and 2560 × 1080; native text overflow checks and visual inspection passed.
- Regression checks: 100 live Build/Upgrade checks and 76 rig-rule checks passed, including placement, automatic duplicates, long-rig scrolling, full failure payout and a fresh expedition.
- Unity project integrity scan: 1,768 files, zero errors or warnings.

[Left cabinet](LeftRig-1920x1080.png), [smaller screen](LeftRig-1280x800.png), [wide screen](LeftRig-2560x1080.png), [insertion preview](InsertionPreview-1920x1080.png), [live result](LiveVerification.json).

Repeat installation with **Fishing Cards → Card Tooltips → Install** outside Play Mode. The Discover, Landing and Build authoring paths retain tooltip wiring during regeneration. Run `TackleCardTooltipChecks.ValidateAssets()` for prefab validation; in a fresh comparison Play session run `TackleCardTooltipChecks.Begin()` for live checks. Fixtures disable persistence and do not edit species definitions.

These SVG designs were prepared locally; the repaired Penpot connection was verified separately. No publication of the boards to Penpot is claimed by this runtime integration.
