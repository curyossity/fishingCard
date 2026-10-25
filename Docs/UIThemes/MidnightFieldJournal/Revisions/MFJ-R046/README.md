# MFJ-R046 — Fishing rarity timing bar

**Integrated and verified as [MFJ-SYNC-008](../../Sync008/README.md) on 25 October 2026.** The design-only notes below preserve the original proposal.

Original Penpot-only visual proposal, 25 October 2026. **Ready for visual review; not implemented or imported into Unity.** The user's new brief authorizes this timing mechanic as a design exploration; this revision does not silently change existing encounter-selection rules or the GDD.

On **Concept B — Reusable Assembly (4K)**, use **MFJ-R046 / Rarity timing bar / design review**. A separate **GAMEPLAY I / Rarity timing phase / R046** shows the instrument in the central encounter area. It temporarily occupies the space before a creature is revealed, allowing the creature card to retain the visual hierarchy in its own phase.

## Visual design

The 1160×340 instrument uses the theme's parchment, fine sepia shell engraving, Cinzel headings and Source Serif 4 instructions. It has a 1048×62 horizontal track, separate colour/pattern regions, exterior rarity legends, an independent bright ivory marker with dark outline and compass-shaped head, a separate motion trail, a Stop control and layered stop feedback.

- Common: sea green, single-wave pattern and emblem; largest zone.
- Uncommon: marine blue, double-wave pattern and emblem; medium zone.
- Rare: warm ochre, star pattern and emblem; smallest initial zone.
- Legendary: optional narrow plum section with diamonds and a special stopped state.
- Miss: optional narrow pale section with diagonal marks and a gap emblem. It means **skip without penalty**, never a failure outcome.

Initial illustrations use 68/24/8 proportions, with no failure interval. The expanded illustration uses 62/25/8/3/2, including Legendary/Miss, and the modified illustration uses 58/28/14. **All widths are mockup values, not approved odds, difficulty or balance.** Either optional zone can be enabled independently in a future data-driven layout. Labels stay outside the narrow sections; they are never squeezed into a tiny target. Colour, pattern, emblem and result wording provide redundant cues.

Stopping freezes the marker. A restrained short burst and selected-zone wash support the rarity heading; there is no flame or full-screen flash. The motion trail, punch, wash and burst are separate elements so decorative motion can be reduced independently of the timing marker.

## Interaction prototype

The review's base Stop button opens the running prototype. Timed frames illustrate horizontal travel; click Stop to view the corresponding stopped-rarity state. Penpot uses discrete frame navigation, **not continuously sampled timing, a game engine, random selection or eligibility checks**. Frame timing is an illustrative study only. Common/Uncommon/Rare creature examples reuse approved existing cards. A future game chooses randomly from eligible creatures at the selected rarity rather than selecting these examples deterministically.

The upgraded, resized, Legendary and Miss examples are visual states for review. No Legendary creature is invented for the mockup. The playable implementation must resolve what to do when a rarity has no eligible creatures, how this phase interacts with events/Apex schedules and existing encounter replacements, and which upgrades/effects grant each width/speed change. Those gameplay decisions are outside this visual increment.

## Reusable elements

The parts board separates the blank frame, normalized zone container, fills, clipped patterns, independent marker/head/line/tail/trail, external labels/emblems, result heading, pulse layers and Stop-button face/text/emblem. Native vectors and text remain editable. The frame uses fixed 64px corners with simple stretchable rules and parchment centre; do not bake the marker, labels or zone widths into a texture.

Future Unity consumers should read zone definitions/normalized boundaries and speed from runtime data, use clear focus/input states, stop at the exact normalized position, and show only the rarity result until the eligible random creature is revealed. This folder contains design source and review exports only. Existing Unity assets, code, prefabs, scenes and gameplay remain untouched.
