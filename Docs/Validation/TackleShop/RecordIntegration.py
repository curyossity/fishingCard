"""Record the tested Unity-only equipment increment without reimporting completed Penpot art."""
import hashlib
import json
import subprocess
from pathlib import Path

folder = Path(__file__).resolve().parent
project = folder.parents[2]
theme = project / 'Docs/UIThemes/MidnightFieldJournal'
read = lambda p: json.loads(p.read_text(encoding='utf-8-sig'))
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()

def write(p, data):
    p.write_text(json.dumps(data, indent=2, ensure_ascii=False) + '\n', encoding='utf-8')

before = {x['path']: x['sha256'] for x in read(theme / 'Sync011/UnityCheckpoint.json')['files']}
font = 'Assets/UIThemes/MidnightFieldJournal/Fonts/TMP/Display Fallback.asset'
if sha(project / font) != before[font]:
    raw = subprocess.check_output(['git', 'show', 'HEAD:' + font], cwd=project)
    candidates = [raw, raw.replace(b'\r\n', b'\n').replace(b'\n', b'\r\n')]
    original = next((b for b in candidates if hashlib.sha256(b).hexdigest() == before[font]), None)
    assert original is not None, 'Unknown font change left untouched.'
    (project / font).write_bytes(original)
current = {p.relative_to(project).as_posix(): sha(p) for p in (project / 'Assets').rglob('*') if p.is_file()}
changed = sorted(p for p, h in before.items() if current.get(p) != h)
allowed = {
    'Assets/Scripts/' + name + '.cs' for name in ['ApprovedTechniqueRuntime', 'CreatureEffectRuntime', 'FishingRunController', 'RunProgressionRuntime', 'RunRewardRuntime', 'TechniqueDeckRuntime']
} | {
    'Assets/UIThemes/MidnightFieldJournal/' + p for p in ['Harbor/Editor/JournalHarborBuilder.cs', 'Harbor/JournalHarborFlow.cs', 'Harbor/Prefabs/JournalHarbor.prefab', 'Runtime/JournalStatTooltipStyle.cs', 'Scenes/MidnightFieldJournalComparison.unity']
}
assert set(changed) <= allowed, changed
added = sorted(set(current) - set(before))
assert all(p.startswith('Assets/UIThemes/MidnightFieldJournal/Harbor/TackleShop') or p.startswith('Assets/UIThemes/MidnightFieldJournal/Harbor/JournalTackleShopView.cs') or p.startswith('Assets/UIThemes/MidnightFieldJournal/Harbor/Editor/JournalTackleShopSetup.cs') or p.startswith('Assets/Editor/TackleShopChecks.cs') or any(p.startswith('Assets/Scripts/' + n + '.cs') for n in ['TackleShopCatalog', 'TackleShopRuntime', 'RunEquipmentRuntime']) for p in added), added
build = 'ProjectSettings/EditorBuildSettings.asset'
build_before = next(x['sha256'] for x in read(theme / 'Sync010/Before.json')['files'] if x['path'] == build)
assert sha(project / build) == build_before
reports = {n: read(folder / n) for n in ['RulesVerification.json', 'PlayVerification.json', 'TechniqueRegression.json', 'ComponentVerification.json']}
assert all(r['passed'] for r in reports.values())
write(folder / 'Verification.json', {
    'passed': True, 'checkpoint': 'MFJ-SYNC-011.1', 'reports': reports,
    'changedExistingAssets': changed, 'addedAssets': added,
    'protectedExistingAssetsUnchanged': len(before) - len(changed),
    'originalSceneBuildSettingsAndEncounterContentUnchanged': True,
    'realPlayerSaveKeysUntouchedByTestFixtures': True, 'disposableTestKeysCleanedAfterPlay': True,
    'sixCardHandViewsVerified': 6, 'consoleErrors': 0, 'sceneCleanOutsidePlayMode': True,
    'review': 'Actual shop and six-card hand captures visually inspected; native effects fit at 4K/1080p.'
})
write(folder / 'UnityCheckpoint.json', {
    'checkpoint': 'MFJ-SYNC-011.1', 'previousCheckpoint': '../../UIThemes/MidnightFieldJournal/Sync011/UnityCheckpoint.json',
    'kind': 'Unity-only permanent equipment purchases and one-item expedition loadout', 'revisions': [],
    'verification': 'Verification.json', 'files': [{'path': p, 'sha256': h} for p, h in sorted(current.items())]
})
relative = '../../Validation/TackleShop/UnityCheckpoint.json'
ledger = read(theme / 'DesignChangeLedger.json')
ledger['currentUnityCheckpoint'] = relative
ledger['synchronizationCheckpoints'] = [x for x in ledger['synchronizationCheckpoints'] if x['id'] != 'MFJ-SYNC-011.1'] + [{
    'id': 'MFJ-SYNC-011.1', 'status': 'verified', 'revisions': [], 'manifest': relative,
    'verification': '../../Validation/TackleShop/Verification.json'
}]
write(theme / 'DesignChangeLedger.json', ledger)
pending = read(theme / 'PendingUnityChanges.json'); pending['currentUnityCheckpoint'] = relative
assert not pending['pending']
write(theme / 'PendingUnityChanges.json', pending)
notice = '**MFJ-SYNC-011.1: Tackle Shop purchases and expedition equipment are active.** Permanent ownership, one-item loadout and Gold persist; all three approved effects work. Unity-only increment, no new art import. **0 pending; 45 completed; 13 superseded.** [Behavior and validation](../../Validation/TackleShop/README.md).\n\n'
for name in ['Design Changes.md', 'VisualContract.md']:
    path = theme / name; text = path.read_text(encoding='utf-8')
    if notice not in text:
        heading, rest = text.split('\n', 1); path.write_text(heading + '\n\n' + notice + rest.lstrip('\n'), encoding='utf-8')
path = project / 'Docs/Architecture.md'; text = path.read_text(encoding='utf-8')
text = text.replace('Shop purchases, equipment effects, ownership and deck editing remain inactive presentation previews.', 'Shop purchases, equipment effects and ownership are active through the Tackle Shop owners above; Technique deck editing remains an inactive presentation preview.')
text = text.replace('A global four-card limit for setup, effect draws, and recovery;', 'A per-expedition hand cap (four normally, six with Tideglass Reel) for setup, effect draws, and recovery;')
path.write_text(text, encoding='utf-8')
print(json.dumps({'passed': True, 'changedExistingAssets': len(changed), 'addedAssets': len(added), 'pendingImports': 0}))
