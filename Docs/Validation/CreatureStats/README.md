# Main-card Weight and Value visibility

The production card's stat fields inherited the generic text configuration: 48-point text, a 26.4-point auto-size minimum, wrapping, and ellipsis overflow. At the prefab's 611-unit height, each stat box has 32.1 units of usable height after padding. The font's line metrics at the old minimum require about 36.2 units. Ellipsis could therefore remove the entire first line, leaving a blank stat despite valid resolved card data. Garfish has authored Weight 1 and Value 9; the Catch Chain uses separate text geometry.

CreatureCardView now applies shared stat-specific fitting to both fields: single-line rich text, auto-sizing from 10 to 48 points, and non-truncating overflow. The serialized prefab and Phase3ComponentPrefabBuilder use the same configuration. The supplied artwork, stat regions, colors, number/unit format, and hidden-information rules are preserved.

Executed verification:

- Runtime and Editor sources compile against Unity 6000.5.9f1 reference assemblies.
- Static checks confirm both serialized fields, runtime/builder wiring, and a fitting lower bound at card heights 480, 611, and 800. See [results](static-results.json).
- The prefab diff changes only the two stat fields' minimum font size, wrapping, and overflow settings.

The `Fishing Cards/Validation/Creature Stat Presentation` Editor command checks rendered glyphs for every authored catchable card at those three sizes, plus zero/modified stats, hidden values, and transitions from events and blank cards. It also captures Garfish. This Unity preview check compiled but did not execute in this session; no screenshot or PlayMode verification is claimed.
