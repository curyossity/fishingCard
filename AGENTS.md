# Agent Instructions

Before making any code, asset, prefab, scene, or architecture changes in this repository, read and follow:

- `CODING_STANDARDS.md`
- `Docs/Architecture.md`
- `Docs/GDD_IMPLEMENTATION_BRIEF.md`
- `Docs/Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md`
- `Docs/MVP Workplan.md`

This repository is a Unity 2D card game. Keep all implementation choices aligned with Unity, 2D card-game conventions, and the game idea described in the GDD unless the user explicitly asks otherwise.

The GDD under `Docs/Fishing_Card_Roguelike_GDD_with_image/` is the source of truth for gameplay, naming, architecture, and feature priority. Do not invent mechanics, card roles, systems, or progression rules that are not supported by that GDD unless the user explicitly approves.

Use `Docs/GDD_IMPLEMENTATION_BRIEF.md` as the low-token development brief. Read the full GDD when a decision touches a specific mechanic, term, system, or ambiguity that the brief does not fully resolve.

Use `Docs/MVP Workplan.md` as the forward development roadmap. Follow its phases, deliverables, exit gates, recommended development order, and explicit postponements unless the user explicitly changes priorities. The GDD remains the design source of truth; the MVP workplan defines implementation order and scope.

Use `Docs/Architecture.md` as the living responsibility map for code structure. Keep new classes, runtime state, view code, and effect handling aligned with it, and update it when architectural responsibilities change.

During MVP development, document implementation choices that are acceptable for the MVP but should likely be replaced, hardened, or redesigned before a production version. Record these in `Docs/MVP Technical Debt.md` using the existing format: current approach, why it is acceptable for MVP, production concern, revisit trigger, and likely future action.
