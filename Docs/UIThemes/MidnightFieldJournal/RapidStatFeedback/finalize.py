"""Record verified rapid number feedback while preserving historical integrations."""
import hashlib
import json
from pathlib import Path

root = Path('Docs/UIThemes/MidnightFieldJournal')
out = root / 'RapidStatFeedback'
digest = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
before = json.loads((out / 'Before.json').read_text())
changed = sorted(p for p, sha in before.items() if not Path(p).exists() or digest(Path(p)) != sha)
assert changed == sorted([
    'Assets/Scripts/CreatureCardStatAnimator.cs',
    'Assets/UIThemes/MidnightFieldJournal/Editor/JournalStatFeedbackSync.cs',
    'Assets/UIThemes/MidnightFieldJournal/Editor/JournalStatFeedbackChecks.cs',
    'Assets/UIThemes/MidnightFieldJournal/Prefabs/CreatureCard.prefab',
]), changed
assert all(p.as_posix() in before for folder in ['Assets', 'ProjectSettings'] for p in Path(folder).rglob('*') if p.is_file())
main = json.loads((out / 'MainCard/PlayModeVerification.json').read_text())
chain = json.loads((out / 'Chain/PlayModeVerification.json').read_text())
assert main['passed'] and main['assertions'] == 47 and chain['passed'] and chain['assertions'] == 89
assert not (out / 'UnityCheckpoint.json').exists(), 'Already finalized'

def write_json(path, value):
    path.write_text(json.dumps(value, indent=2) + '\n', encoding='utf-8')

write_json(out / 'Preservation.json', {
    'passed': True, 'changedExistingFiles': changed, 'protectedFilesUnchanged': len(before) - len(changed),
    'artFontsDefinitionsScenesSettingsAndGameplayUnchanged': True,
    'candidatePrefabChanges': 'Native save of stat timing and previously implicit numeric punch defaults only',
})
write_json(out / 'Summary.json', {
    'passed': True, 'mainCardChecks': 47, 'chainChecks': 89, 'count8To12Seconds': main['countDuration'],
    'ordinaryStepSeconds': .025, 'initialHoldSeconds': .05, 'springSeconds': .18,
    'largeDeltaNominalCountSeconds': .6, 'sceneClean': True, 'consoleErrors': 0, 'penpotEdited': False,
})
detail = '''

## Rapid numeric feedback — MFJ-SYNC-006.14

Implemented directly in Unity on 20 October 2026. Both current and compact creatures use 25ms ordinary steps, a 50ms initial hold and a 180ms final spring. Every displayed change retriggers the existing 14% scale punch and alternating 2.5-degree tilt; the final beat remains 22%. Large deltas use bounded strides targeting about 0.6 seconds of counting, subject to frame rate, rather than long one-unit sequences. Each frame displays at most one transition so every visible change receives its own pop/tilt. Fractional timing carries forward. Colour, masking, neutral Bait zero, retargeting, card pop/flash and newest-to-oldest chain serialization remain unchanged. [Verification and current baseline](RapidStatFeedback/README.md). No Penpot revision or pending import was added.
'''
for name in ['Design Changes.md', 'Pending Unity Changes.md', 'VisualContract.md']:
    path = root / name
    text = path.read_text(encoding='utf-8').replace('(ChainCardPulse/UnityCheckpoint.json)', '(RapidStatFeedback/UnityCheckpoint.json)')
    path.write_text(text.rstrip() + detail, encoding='utf-8')
architecture = Path('Docs/Architecture.md')
architecture.write_text(architecture.read_text(encoding='utf-8').rstrip() + '''

Rapid numeric feedback remains entirely in `CreatureCardStatAnimator`: canonical timing constants feed both runtime compact components and candidate prefab migration/build binding. Ordinary counts use 25ms steps after 50ms; each visible transition resets the same pop/alternating tilt, with a 180ms final spring. Large deltas choose a bounded integer stride for a nominal 0.6-second count. Steps use wide arithmetic to avoid overflow, display at most one transition per frame, and carry fractional wait rather than hiding multiple beats within a frame. Target changes recalculate the stride from the current displayed number; unchanged refreshes retain timing. `JournalStatFeedbackSync` persists rapid timing with native prefab APIs so existing serialized slow values cannot override new defaults. [Checks](UIThemes/MidnightFieldJournal/RapidStatFeedback/README.md).
''', encoding='utf-8')
spec = Path('Docs/Card Visual Specification.md')
spec.write_text(spec.read_text(encoding='utf-8').rstrip() + '''

Current candidate numeric pacing supersedes the earlier readable-per-integer timing: 25ms ordinary steps, 50ms initial hold and 180ms spring settle. Every visible number change pops and tilts slightly. Small deltas retain each integer; large deltas may jump through intermediate integer values to convey direction rapidly instead of requiring every integer to be readable. Card-level pop/flash and one-creature-at-a-time Catch Chain ordering are retained.
''', encoding='utf-8')
for name in ['DesignChangeLedger.json', 'PendingUnityChanges.json']:
    path = root / name
    data = json.loads(path.read_text(encoding='utf-8'))
    data['currentUnityCheckpoint'] = 'RapidStatFeedback/UnityCheckpoint.json'
    write_json(path, data)
previous = json.loads((root / 'ChainCardPulse/UnityCheckpoint.json').read_text())
paths = sorted(set(f['path'] for f in previous['files']) | set(changed))
write_json(out / 'UnityCheckpoint.json', {
    'checkpoint': 'MFJ-SYNC-006.14', 'date': '2026-10-20',
    'previousCheckpoint': '../ChainCardPulse/UnityCheckpoint.json', 'kind': 'Rapid numeric pop and tilt', 'revisions': [],
    'files': [{'path': p, 'sha256': digest(Path(p))} for p in paths],
    'validation': 'Summary.json', 'preservation': 'Preservation.json',
})
print('Recorded MFJ-SYNC-006.14; 47 main-card and 89 chain assertions passed.')
