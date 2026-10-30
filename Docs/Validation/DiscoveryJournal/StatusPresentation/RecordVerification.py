"""Record the verified status-only presentation change without changing saved quest evidence."""
import hashlib
import json
import subprocess
from pathlib import Path

project = Path(__file__).resolve().parents[4]
folder = Path(__file__).resolve().parent
theme = project / 'Docs/UIThemes/MidnightFieldJournal'
read = lambda path: json.loads(path.read_text(encoding='utf-8-sig'))
sha = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()

def write(path, value):
    path.write_text(json.dumps(value, indent=2, ensure_ascii=False) + '\n', encoding='utf-8')

previous = read(folder.parent / 'UnityCheckpoint.json')
before = {x['path']: x['sha256'] for x in previous['files']}
font = 'Assets/UIThemes/MidnightFieldJournal/Fonts/TMP/Display Fallback.asset'
if sha(project / font) != before[font]:
    raw = subprocess.check_output(['git', 'show', 'HEAD:' + font], cwd=project)
    candidates = [raw, raw.replace(b'\r\n', b'\n').replace(b'\n', b'\r\n')]
    original = next((b for b in candidates if hashlib.sha256(b).hexdigest() == before[font]), None)
    assert original is not None, 'Unrecognized font change; left untouched.'
    (project / font).write_bytes(original)
current = {p.relative_to(project).as_posix(): sha(p) for p in (project / 'Assets').rglob('*') if p.is_file()}
changed = sorted(p for p in before if before[p] != current.get(p))
assert set(changed) <= {
    'Assets/UIThemes/MidnightFieldJournal/Harbor/JournalDiscoveryPage.cs',
    'Assets/UIThemes/MidnightFieldJournal/Harbor/Editor/JournalDiscoveryPageBuilder.cs',
    'Assets/UIThemes/MidnightFieldJournal/Harbor/Prefabs/JournalHarbor.prefab',
    'Assets/Editor/DiscoveryJournalChecks.cs'
}, changed
write(folder / 'Verification.json', {
    'passed': True, 'consoleErrors': 0,
    'partialLabels': ['INCOMPLETE', 'INCOMPLETE', '1 / 3 successful expeditions'],
    'completeLabels': ['COMPLETE', 'COMPLETE', '3 / 3 successful expeditions · COMPLETE'],
    'visibleProgressTracks': [False, False, True],
    'validation': 'Actual Play Mode page Refresh and OnEnable with temporary in-memory evidence; no PlayerPrefs writes or reward grants.',
    'changedAssets': changed, 'trackingRulesAndCatalogUnchanged': True,
    'prefabUpdate': 'Targeted labels and track visibility; existing layout and object identities preserved.'
})
write(folder / 'UnityCheckpoint.json', {
    'checkpoint': 'MFJ-SYNC-010.2', 'date': '2026-10-30', 'previousCheckpoint': '../UnityCheckpoint.json',
    'kind': 'Unity-only quest status presentation; counters only for cumulative expeditions',
    'revisions': [], 'verification': 'Verification.json',
    'files': [{'path': p, 'sha256': h} for p, h in sorted(current.items())]
})
relative = '../../Validation/DiscoveryJournal/StatusPresentation/UnityCheckpoint.json'
ledger = read(theme / 'DesignChangeLedger.json')
ledger['currentUnityCheckpoint'] = relative
ledger['synchronizationCheckpoints'] = [x for x in ledger['synchronizationCheckpoints'] if x['id'] != 'MFJ-SYNC-010.2']
ledger['synchronizationCheckpoints'].append({'id': 'MFJ-SYNC-010.2', 'date': '2026-10-30', 'revisions': [], 'status': 'verified', 'manifest': relative, 'verification': '../../Validation/DiscoveryJournal/StatusPresentation/Verification.json'})
write(theme / 'DesignChangeLedger.json', ledger)
pending = read(theme / 'PendingUnityChanges.json'); pending['currentUnityCheckpoint'] = relative
write(theme / 'PendingUnityChanges.json', pending)
for name in ['Design Changes.md', 'VisualContract.md']:
    path = theme / name
    text = path.read_text(encoding='utf-8')
    notice = '**MFJ-SYNC-010.2:** single-expedition Journal quests show COMPLETE/INCOMPLETE only; cumulative quests retain successful-expedition counters and bars. Tracking and rewards are unchanged. No pending Penpot import. [Validation](../../Validation/DiscoveryJournal/StatusPresentation/README.md).\n\n'
    if notice not in text:
        heading, rest = text.split('\n', 1)
        path.write_text(heading + '\n\n' + notice + rest.lstrip('\n'), encoding='utf-8')
brief = project / 'Docs/GDD_IMPLEMENTATION_BRIEF.md'
text = brief.read_text(encoding='utf-8').replace('progress shows the best successful expedition rather than adding creatures/tags across runs', 'internal evidence retains the best successful expedition rather than adding creatures/tags across runs')
brief.write_text(text, encoding='utf-8')
policy = '\nSingle-expedition objectives display only COMPLETE or INCOMPLETE, without partial numbers, a best-expedition caption or a progress bar. Cumulative objectives display their qualifying successful-expedition count and progress bar. Saved evidence and reward behavior are unchanged.\n'
for relative_doc in ['Docs/Architecture.md', 'Docs/GDD_IMPLEMENTATION_BRIEF.md', 'Docs/Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md', 'Docs/Validation/DiscoveryJournal/README.md']:
    path = project / relative_doc
    text = path.read_text(encoding='utf-8')
    if policy.strip() not in text:
        if relative_doc.endswith('Fishing_Card_Roguelike_GDD.md'):
            text = text.replace('# 12. Intended player tension', policy + '\n# 12. Intended player tension')
        elif relative_doc == 'Docs/Architecture.md':
            text = text.replace('## Core Principles', policy + '\n## Core Principles', 1)
        else:
            text += '\n' + policy
        path.write_text(text, encoding='utf-8')
print(json.dumps({'passed': True, 'changedAssets': changed, 'pendingImports': len(pending['pending'])}))
