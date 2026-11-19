# Creature hover details — design proposal

Prepared 19 November 2026. These are locally prepared, editable SVG boards and a browser prototype. The Penpot MCP connection has since been repaired and design-file access verified; these boards have not been published to Penpot, and no Penpot page/shape IDs are claimed. Subsequent Unity integration uses native editable panel geometry and text from this design; see [runtime validation](../../../Validation/CardTooltips/README.md).

## Review artifacts

- [Interactive hover/focus prototype](Review.html)
- [Component sheet](00-component-sheet.png) / [editable SVG](00-component-sheet.svg)
- [Left-rig context](01-left-rig.png) / [editable SVG](01-left-rig.svg)
- [Insertion-tray context](02-insertion-preview.png) / [editable SVG](02-insertion-preview.svg)

SVG panels contain native editable paths and text. Fonts are embedded for local rendering and use the project's supplied VT323 and Press Start 2P families. Context boards embed an unchanged runtime screenshot as a reference layer; the proposed tooltip and hover outline remain independent editable groups. Do not flatten the complete board into a production tooltip asset.

Once Penpot is connected, create a new **Creature Tooltips** page in the existing Tackle Apparatus design file. Import the SVGs as editable shapes, preserving their named groups; make the frame, identity, ability and optional context groups into one reusable component with compact, contextual and keyword-help variants. Preserve all existing design pages and supplied artwork. Check the fonts and exports again in Penpot before claiming that stage complete.

## Information hierarchy

1. Creature name and quiet rarity label identify the hovered card.
2. One tag, current level and resolved accumulated sale value provide context.
3. Ability name and its short **current-level** effect are the main content. Accent the coefficient; keep conditions fully readable.
4. Add a resolved explanation only when useful: a group count, a positional requirement or the consequence of the selected insertion. Preview information is explicitly labeled **After placement**. It grants nothing and does not represent a committed rig change.
5. Explain a keyword only if it appears in the effect. Keep this secondary; the optional Contiguous example explains that another tag breaks a connected group.

The value shown is accumulated species sale value, including represented copies and applicable value effects. It is not a new Gold payout. The effect shown for a committed member uses its current level; a pending new species uses Level 1. Context examples are illustrative design fixtures drawn from current definitions, not a second creature catalog or gameplay authority.

Do not add Weight, Resistance, copy-count explanations, Catch/Bait roles, removal/reorder actions, full upgrade tables or visible runtime-state badges to the card face. No duplicate enlarged portrait is needed. The pending creature already has a large portrait and ability panel; inspecting it may reuse the tooltip without introducing another acquisition decision.

## Position and behavior

- Reference resolution: 1920 × 1080. Panel width: 460 reference pixels; body: VT323 at 28 px, headings: Press Start 2P at 16 px. Use automatic wrapping and content-driven height rather than shrinking text.
- Left-rig cards open to the right. During insertion, shift the tooltip upward to keep all gap buttons clear. The mockup uses x=396, y=260 for the hovered Cleaner Shrimp.
- Tray cards open above the source. Clamp to the visible canvas safe area and flip left/right or below if necessary. The mockup uses x=1214, y=216 for Glass Minnow.
- Highlight the source with a thin brass outline outside its face. Keep the source visible. Use no triangle pointer, cursor-following motion, modal dimming or whole-card lift that could obscure nearby placement targets.
- Proposed tuning: 180 ms hover delay, immediate display on keyboard/gamepad focus, 80 ms hide delay. Moving between cards cancels the previous pending tooltip. No entrance motion is needed beyond a short fade; reduced-motion mode shows it directly.
- Dismiss on Escape, pointer exit, source removal, scrolling, phase change or changed placement occurrence. Tooltip graphics do not intercept gameplay input. Do not retain a tooltip for a recycled pooled card.
- Focus inspection must not select a gap, move a card, confirm placement, consume an attempt or advance a phase. For touch, offer a separate inspect gesture/control rather than changing the meaning of placement confirmation.
- The example keyword panel is secondary help, not an interactive nested hover chain. A final input treatment for expanded help can be chosen during integration.

## Runtime integration boundary

The Unity integration uses a single overlay outside the rig/tray clipping masks. Both representations bind the same stable species/member identity and hover presentation. Current-level text comes from `RigAbilityText`; sale values and placement previews come from authoritative rig snapshots. Views do not count synergies or mutate preview state themselves. Additional structured explanatory data, if needed, belongs to the gameplay owner rather than tooltip-side arithmetic. In Landing, explanations refer to the frozen committed ability, not a later edited definition or preview.

The static browser prototype demonstrates hover/focus, dismissal and screen placement. Its example results do not calculate gameplay and must not be mistaken for installed Unity behavior. Responsive integration should check at least 1280 × 800, 1920 × 1080 and 2560 × 1080, plus lower-edge sources and long upgraded coefficients.

## Reference study

These are design interpretations of inspected gameplay images and published behavior, not claims that another game's layout is universally best:

- **Balatro:** official gameplay and deck-view images use compact information groups, strong number emphasis and a consistent pixel type/material language. Keep the effect and coefficient prominent without duplicating the whole card. [Official press kit](https://www.playbalatro.com/press-kit/)
- **Slay the Spire:** the inspected original-game screenshot enlarges the source card and highlights rules terms; the accompanying player report documents expected keyword hover help. Preserve source identification and offer secondary explanations only for relevant terms. This is a historical 2018 reference, not a claim about the latest game. [Screenshot and report](https://steamcommunity.com/app/646570/discussions/1/1694922526907471397/)
- **Monster Train:** developer patch notes explicitly address missing keyword tooltips. Treat ability terms as information that needs reliable explanation, while avoiding a wall of permanent help text. [Developer update](https://steamcommunity.com/games/1102190/announcements/detail/2845666883512849122)

Third-party screenshots were inspected under the temporary research directory and are not embedded in the deliverable. Deepwater's source screenshot, palettes, supplied fonts and card-face boundary determine this design's appearance.

## Reproduction and verification

Run `python Docs/UIThemes/TackleApparatusConcept/CardTooltips/build_design.py` from the repository root to rebuild SVGs and the local prototype. Render SVGs in a browser with the supplied fonts loaded. `QA.html` measures tooltip text against its panel's inner bounds and reports font status. Penpot publication and validation remain pending; Unity validation is recorded separately.
