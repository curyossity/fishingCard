"""Record the verified, presentation-only Harbor integration without resetting design history."""
import hashlib
import json
from pathlib import Path

project = Path(__file__).resolve().parents[4]
folder = Path(__file__).resolve().parent
theme = folder.parent
revisions = ['MFJ-R053', 'MFJ-R055', 'MFJ-R056', 'MFJ-R057']

def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))

def write(path, data):
    path.write_text(json.dumps(data, indent=2, ensure_ascii=False) + '\n', encoding='utf-8')

def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

component = read(folder / 'ComponentVerification.json')
play = read(folder / 'PlayVerification.json')
assert component['passed'] and play['passed']
before = {x['path']: x['sha256'] for x in read(folder / 'Before.json')['files']}
current = {p.relative_to(project).as_posix(): digest(p) for p in (project / 'Assets').rglob('*') if p.is_file()}
current['ProjectSettings/EditorBuildSettings.asset'] = digest(project / 'ProjectSettings/EditorBuildSettings.asset')
changed = sorted(p for p in before if before[p] != current.get(p))
added = sorted(set(current) - set(before))
allowed = {
    'Assets/UIThemes/MidnightFieldJournal/Editor/JournalComparisonSceneBuilder.cs',
    'Assets/UIThemes/MidnightFieldJournal/Runtime/JournalResultModal.cs',
    'Assets/UIThemes/MidnightFieldJournal/Scenes/MidnightFieldJournalComparison.unity',
}
assert set(changed) <= allowed, changed
assert all(p.startswith('Assets/UIThemes/MidnightFieldJournal/Harbor/') or p == 'Assets/UIThemes/MidnightFieldJournal/Harbor.meta' for p in added), added
write(folder / 'Verification.json', {
    'passed': True, 'date': '2026-10-30', 'component': component, 'play': play,
    'protectedExistingFilesUnchanged': len(before) - len(changed),
    'changedExistingFiles': changed, 'addedFiles': added,
    'originalSceneAndBuildSettingsUnchanged': True,
    'cardDefinitionsAndGameplayRulesUnchanged': True,
    'sceneCleanOutsidePlayMode': True,
    'notes': ['The smoke test yields a frame between page changes so newly enabled uGUI graphics register before the next pointer raycast.',
              'The runtime-generated fallback font atlas was restored to its hash-verified pre-task bytes.']
})
write(folder / 'UnityCheckpoint.json', {
    'checkpoint': 'MFJ-SYNC-010', 'date': '2026-10-30',
    'previousCheckpoint': '../Sync009/UnityCheckpoint.json', 'revisions': revisions,
    'kind': 'Current ornate Harbor, Tackle Shop and Technique Collection presentation, navigation and existing expedition lifecycle',
    'verification': 'Verification.json',
    'files': [{'path': p, 'sha256': h} for p, h in sorted(current.items()) if p.startswith('Assets/')]
})
ledger = read(theme / 'DesignChangeLedger.json')
for change in ledger['changes']:
    if change['id'] not in revisions:
        continue
    change.update(status='verified', unityImported=True, integrationStatus='verified', synchronizationCheckpoint='MFJ-SYNC-010')
    approval = 'User requested screen navigation and starting a new run; full shop functionality is explicitly out of scope.'
    if isinstance(change['approvalEvidence'], list):
        if approval not in change['approvalEvidence']:
            change['approvalEvidence'].append(approval)
    else:
        change['approvalEvidence']['unityIntegrationRequest'] = approval
    change['importEvidence'] = {'record': 'Sync010/README.md', 'sources': 'Sync010/ImportedSources.json', 'nativeLayout': 'Sync010/PenpotLayout.json'}
    change['verificationEvidence']['unity'] = 'Sync010/Verification.json'
    change['unityPaths'] = ['Assets/UIThemes/MidnightFieldJournal/Harbor/', 'Assets/UIThemes/MidnightFieldJournal/Scenes/MidnightFieldJournalComparison.unity']
    change['remainingWork'] = []
ledger['currentUnityCheckpoint'] = 'Sync010/UnityCheckpoint.json'
ledger['synchronizationCheckpoints'] = [x for x in ledger['synchronizationCheckpoints'] if x['id'] != 'MFJ-SYNC-010']
ledger['synchronizationCheckpoints'].append({'id': 'MFJ-SYNC-010', 'date': '2026-10-30', 'revisions': revisions, 'status': 'verified', 'manifest': 'Sync010/UnityCheckpoint.json', 'verification': 'Sync010/Verification.json'})
write(theme / 'DesignChangeLedger.json', ledger)
pending = read(theme / 'PendingUnityChanges.json')
pending['pending'] = [x for x in pending['pending'] if x['id'] not in revisions]
pending['completed'] = [x for x in pending['completed'] if x['id'] not in revisions] + [{'id': r, 'checkpoint': 'MFJ-SYNC-010'} for r in revisions]
pending['counts'] = {k: len(pending[k]) for k in ['pending', 'completed', 'superseded']}
pending['currentUnityCheckpoint'] = 'Sync010/UnityCheckpoint.json'
write(theme / 'PendingUnityChanges.json', pending)
notice = '**MFJ-SYNC-010: imported and verified in Unity (2026-10-30).** Harbor, Tackle Shop and Technique Collection now support navigation and Set Sail. Shop/deck controls remain visual previews. [Integration and checks](Sync010/README.md). **0 pending; 44 completed; 13 superseded.** Earlier pending statements below are historical.\n\n'
for name in ['Design Changes.md', 'VisualContract.md']:
    path = theme / name
    content = path.read_text(encoding='utf-8')
    heading, rest = content.split('\n', 1)
    if notice not in content:
        path.write_text(heading + '\n\n' + notice + rest.lstrip('\n'), encoding='utf-8')
for revision in revisions:
    path = theme / 'Revisions' / revision / 'README.md'
    content = path.read_text(encoding='utf-8')
    revision_notice = '**Imported and verified at MFJ-SYNC-010.** [Unity integration](../../Sync010/README.md). Earlier review-only status below is historical.\n\n'
    if revision_notice not in content:
        heading, rest = content.split('\n', 1)
        path.write_text(heading + '\n\n' + revision_notice + rest.lstrip('\n'), encoding='utf-8')
print(json.dumps({'changedExisting': changed, 'added': len(added), 'counts': pending['counts']}))
