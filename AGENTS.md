# Agent Instructions

Before making any code, asset, prefab, scene, or architecture changes in this repository, read and follow:

- `CODING_STANDARDS.md`
- `Docs/Architecture.md`
- `Docs/Card Visual Specification.md`
- `Docs/GDD_IMPLEMENTATION_BRIEF.md`
- `Docs/Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md`
- `Docs/Full Game Development Guidelines.md`

This repository is a Unity 2D card game. Keep all implementation choices aligned with Unity, 2D card-game conventions, and the game idea described in the GDD unless the user explicitly asks otherwise.

The GDD under `Docs/Fishing_Card_Roguelike_GDD_with_image/` is the source of truth for gameplay, naming, architecture, and feature priority. Do not invent mechanics, card roles, systems, or progression rules that are not supported by that GDD unless the user explicitly approves.

Use `Docs/GDD_IMPLEMENTATION_BRIEF.md` as the low-token development brief. Read the full GDD when a decision touches a specific mechanic, term, system, or ambiguity that the brief does not fully resolve.

Use `Docs/Full Game Development Guidelines.md` as the forward development guidance. The project is no longer in MVP mode: do not make implementation choices because they are merely "good enough for MVP," and do not defer known design, architecture, data, UI, asset, persistence, or validation problems on the assumption they will be fixed later. When a full production solution is too large for one change, implement a complete, clean vertical increment with clear boundaries instead of a temporary shortcut.

Use `Docs/Architecture.md` as the living responsibility map for code structure. Keep new classes, runtime state, view code, and effect handling aligned with it, and update it when architectural responsibilities change.

Use `Docs/Card Visual Specification.md` as the source of truth for constructing and rendering creature/catch card faces. Follow its supplied-art boundary, dynamic-field contract, rarity-hook rules, and restrictions on visible runtime state unless the user explicitly changes the card design.

Treat `Docs/MVP Workplan.md` and `Docs/MVP Technical Debt.md` as historical context only. Existing entries in `Docs/MVP Technical Debt.md` are production-hardening backlog items, not permission to add more MVP shortcuts. If new debt is unavoidable, call it out explicitly to the user before relying on it and prefer resolving the underlying issue in the same change.
