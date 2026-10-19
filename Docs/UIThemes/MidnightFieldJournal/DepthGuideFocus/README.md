# Current-tier color emphasis

Implemented directly in Unity,2026-10-19. Only the current tier retains full original illustration colors and amber marker/text. Inactive artwork is grayscale at42% opacity; inactive labels/ranges use neutral gray. The shared material changes automatically when depth crosses a tier boundary; rows returning to active state use the normal UI material and full opacity. No textures were regenerated, no per-row materials are allocated and no gameplay data changed.

[1080p preview](Gameplay1080p.png) · [4K preview](Gameplay4K.png) · [Verification](Verification.json) · [Preservation](Preservation.json) · [Current checkpoint](UnityCheckpoint.json).

673 existing component/runtime assertions passed, including new active/inactive material and opacity checks at all authored boundary depths, clearing/reusing pooled rows, varying biome tier counts, load display states and boat/legacy startup. The4K/1080p output was visually inspected. The shader preserves sprite alpha, UI stencil support, rectangular clipping and optional alpha clipping. Original illustration bytes, saved scenes and gameplay remain unchanged. Play Mode was not needed for this material-only change; the scene remains clean and Play Mode/background remain off.

This is a Unity-only refinement of the integratedR040 component. No Penpot revision is awaiting import. The native Penpot study retains the previous coloring; these Unity rendering rules take precedence for the live depth guide. Sync005 remains immutable; MFJ-SYNC-005.1 is the next comparison baseline.
