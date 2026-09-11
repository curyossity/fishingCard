# Full Game Development Guidelines

This project has moved beyond MVP development. The goal is now to build the full fishing card roguelike with production-quality foundations, not to prove the smallest playable loop.

## Development Posture

- Build full-game systems and content foundations, not throwaway MVP scaffolding.
- Prefer durable architecture, reusable prefabs, authored data pipelines, production-ready UI structure, and clear runtime ownership.
- Do not introduce shortcuts on the assumption that they will be fixed later.
- Do not use placeholder mechanics, placeholder state models, placeholder persistence, or simplified one-biome assumptions unless the user explicitly approves them for a narrow purpose.
- When a requested feature is large, deliver a complete clean increment that can remain in the final game.
- If a final-quality solution depends on missing art, design, audio, or data, record the missing input and keep the implementation boundary clean rather than inventing final content.

## Relationship To Existing Documents

- The full GDD remains the gameplay source of truth.
- `Docs/GDD_IMPLEMENTATION_BRIEF.md` is the low-token summary of the GDD.
- `Docs/Architecture.md` is the responsibility map and must be kept current when responsibilities change.
- `Docs/Card Visual Specification.md` remains the card-face rendering contract.
- `Docs/UI Redesign Plan.md` is the current visual implementation plan for the complete gameplay UI.
- `Docs/MVP Workplan.md` is historical planning context. Do not use its MVP scope limits, explicit postponements, or "acceptable for MVP" language to justify new implementation shortcuts.
- `Docs/MVP Technical Debt.md` is a backlog of known production-hardening work inherited from the earlier MVP phase. Existing items should be resolved or redesigned as development touches their area.

## Quality Bar

New or changed work should:

- Fit the final game architecture rather than a temporary prototype path.
- Keep gameplay rules separate from presentation.
- Keep runtime state out of shared authored assets.
- Support future multi-biome, progression, save/load, UI scaling, and content expansion where the touched system logically owns those concerns.
- Use authored data and reusable definitions instead of one-off hardcoding when content variation is expected.
- Include relevant validation, tests, or Unity-safe verification for the risk of the change.
- Update documentation when behavior, ownership, data contracts, or visual contracts change.

## Technical Debt Policy

Avoid adding new technical debt. If unavoidable:

- Make the limitation explicit in the implementation and final report.
- Record why it is unavoidable now.
- Define the production concern and the trigger for revisiting it.
- Prefer a small durable implementation over a broad temporary implementation.

Do not add debt merely because a feature is difficult, because a shortcut is faster, or because an earlier MVP-era system already took a shortcut.

## Existing MVP-Era Systems

When modifying a system that still contains MVP-era assumptions, use the opportunity to move it toward the full-game design. In particular, watch for:

- One-biome limits.
- Terminal placeholder transitions.
- Debug-only data paths used by normal gameplay.
- Simplified progression, deck editing, or save/load behavior.
- Placeholder UI composition that conflicts with the redesign plan.
- Runtime data stored in authored definitions.
- Effect handling that cannot support richer final-game cards.

Resolve these issues in the touched area when reasonable, or identify them clearly as remaining production-hardening work.
