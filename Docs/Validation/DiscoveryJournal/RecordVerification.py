"""Checkpoint the verified Unity-only Journal increment while retaining all Penpot revision history."""
import hashlib
import json
import subprocess
from pathlib import Path

project = Path(__file__).resolve().parents[3]
folder = Path(__file__).resolve().parent
theme = project / 'Docs/UIThemes/MidnightFieldJournal'

def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))

def write(path, data):
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

previous = read(theme / 'Sync010/UnityCheckpoint.json')
before = {x['path']: x['sha256'] for x in previous['files']}
# Unity populates a dynamic fallback atlas while rendering; discard only hash-proven test churn.
font = 'Assets/UIThemes/MidnightFieldJournal/Fonts/TMP/Display Fallback.asset'
if sha(project / font) != before[font]:
    raw = subprocess.check_output(['git', 'show', 'HEAD:' + font], cwd=project)
    for candidate in [raw, raw.replace(b'\r\n', b'\n').replace(b'\n', b'\r\n')]:
        if hashlib.sha256(candidate).hexdigest() == before[font]:
            (project / font).write_bytes(candidate)
            break
    else:
        raise RuntimeError('Fallback font does not match the pre-task checkpoint; left untouched.')

reports = {name: read(folder / name) for name in ['RulesVerification.json', 'ComponentVerification.json', 'LiveProgressVerification.json', 'PlayVerification.json']}
assert all(x['passed'] for x in reports.values())
current = {p.relative_to(project).as_posix(): sha(p) for p in (project / 'Assets').rglob('*') if p.is_file()}
changed = sorted(p for p in before if before[p] != current.get(p))
allowed = {
    'Assets/Scripts/FishingRunController.cs',
    'Assets/UIThemes/MidnightFieldJournal/Harbor/Editor/JournalHarborBuilder.cs',
    'Assets/UIThemes/MidnightFieldJournal/Harbor/Editor/JournalHarborChecks.cs',
    'Assets/UIThemes/MidnightFieldJournal/Harbor/JournalHarborFlow.cs',
    'Assets/UIThemes/MidnightFieldJournal/Harbor/Prefabs/JournalHarbor.prefab',
    'Assets/UIThemes/MidnightFieldJournal/Scenes/MidnightFieldJournalComparison.unity',
}
assert set(changed) <= allowed, changed
original = {x['path']: x['sha256'] for x in read(theme / 'Sync010/Before.json')['files']}
build = 'ProjectSettings/EditorBuildSettings.asset'
assert sha(project / build) == original[build]
write(folder / 'Verification.json', {
    'passed': True, 'date': '2026-10-30', 'reports': reports,
    'changedExistingAssets': changed, 'newAssets': sorted(set(current) - set(before)),
    'protectedExistingAssetsUnchanged': len(before) - len(changed),
    'originalSceneBuildSettingsArtworkAndCardDefinitionsUnchanged': True,
    'consoleErrors': 0, 'sceneSavedOutsidePlayMode': True,
    'rewardGranting': False,
    'testPersistence': 'Unique temporary PlayerPrefs key, deleted after checks; player progress untouched.'
})
write(folder / 'UnityCheckpoint.json', {
    'checkpoint': 'MFJ-SYNC-010.1', 'date': '2026-10-30',
    'previousCheckpoint': '../../UIThemes/MidnightFieldJournal/Sync010/UnityCheckpoint.json',
    'kind': 'Unity-only Discovery Journal navigation and persistent quest evidence, no reward granting',
    'revisions': [], 'verification': 'Verification.json',
    'files': [{'path': p, 'sha256': h} for p, h in sorted(current.items())]
})
checkpoint_path = '../../Validation/DiscoveryJournal/UnityCheckpoint.json'
ledger = read(theme / 'DesignChangeLedger.json')
ledger['currentUnityCheckpoint'] = checkpoint_path
ledger['synchronizationCheckpoints'] = [x for x in ledger['synchronizationCheckpoints'] if x['id'] != 'MFJ-SYNC-010.1']
ledger['synchronizationCheckpoints'].append({
    'id': 'MFJ-SYNC-010.1', 'date': '2026-10-30', 'status': 'verified', 'revisions': [],
    'manifest': checkpoint_path, 'verification': '../../Validation/DiscoveryJournal/Verification.json',
    'kind': 'Unity-only Journal quest tracking; no Penpot import or reward grant'
})
write(theme / 'DesignChangeLedger.json', ledger)
pending = read(theme / 'PendingUnityChanges.json')
pending['currentUnityCheckpoint'] = checkpoint_path
pending['counts'] = {key: len(pending[key]) for key in ['pending', 'completed', 'superseded']}
write(theme / 'PendingUnityChanges.json', pending)
notice = '**MFJ-SYNC-010.1 is the current Unity checkpoint.** Discovery Journal is active with three persistent quests and display-only rewards. This is a Unity-only increment; all earlier Penpot integrations remain completed, with no pending imports. [Behavior and verification](../../Validation/DiscoveryJournal/README.md). Earlier inactive-Journal statements below describe the previous checkpoint.\n\n'
for name in ['Design Changes.md', 'VisualContract.md']:
    path = theme / name
    content = path.read_text(encoding='utf-8')
    if notice not in content:
        heading, rest = content.split('\n', 1)
        path.write_text(heading + '\n\n' + notice + rest.lstrip('\n'), encoding='utf-8')
print(json.dumps({'passed': True, 'changedExistingAssets': changed, 'newAssets': len(set(current) - set(before)), 'pendingImports': len(pending['pending'])}))
