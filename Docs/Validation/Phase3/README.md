# Phase 3 Validation

## Task 3.1 - MaritimePanel

- Unity version: 6000.5.9f1
- Prefab: `Assets/Prefabs/UI/Components/MaritimePanel.prefab`
- Automated checks: required hierarchy, serialized component references, sliced outer/header borders
- Visual checks: four interior variants, both border materials, optional icon/header, four panel aspect ratios
- Evidence: `task-3.1-maritime-panel.png`, `task-3.1-result.json`

## Task 3.2 - CreatureCardView

- Unity version: 6000.5.9f1
- Prefab: `Assets/Prefabs/UI/Components/CreatureCardView.prefab`
- Automated checks: supplied layer hierarchy, hidden stencil mask, explicit safe regions, hollow sliced frame, exactly four anchor slots
- Visual checks: common and legendary content, long-title fit, runtime stats/rules, mask clipping, fixed geometry at two gameplay sizes
- Evidence: `task-3.2-creature-card.png`, `task-3.2-result.json`

## Task 3.3 - CompactCatchCard

- Unity version: 6000.5.9f1
- Prefab: `Assets/Prefabs/UI/Components/CompactCatchCard.prefab`
- Automated checks: selectable root, fixed rig attachment, compact-only fields, passive icon socket, selected/disabled/release-candidate layers
- Visual checks: three-card Catch Rig stack, resolved stats, optional icon, selected double frame, disabled hatch, release-candidate perimeter
- Evidence: `task-3.3-compact-catch-card.png`, `task-3.3-result.json`

## Task 3.4 - TechniqueCardView

- Unity version: 6000.5.9f1
- Prefab: `Assets/Prefabs/UI/Components/TechniqueCardView.prefab`
- Automated checks: hidden artwork mask, dedicated title/rules/keyword regions, selectable root, hover/selected/playable/disabled layers
- Visual checks: five simultaneous cards, bounded hover elevation, selected frame, explicit playable plaque, lock plus disabled hatch
- Evidence: `task-3.4-technique-card.png`, `task-3.4-result.json`

## Task 3.5 - RunActionButton

- Unity version: 6000.5.9f1
- Prefab: `Assets/Prefabs/UI/Components/RunActionButton.prefab`
- Automated checks: sliced fixed-size input surface, icon, TMP label, focus frame, serialized state and theme sprites
- Visual checks: Descend/Release/Surface themes and normal/hover/pressed/disabled/dangerous states at two sizes
- Evidence: `task-3.5-run-action-button.png`, `task-3.5-result.json`

## Integration

`FishingRunViewPrefabBuilder.Build` passed after wiring `CreatureCardView.prefab` into the current Encounter region. This removes the previous bare-component runtime construction while preserving the controller/view data boundary.
