"""Record verified bold stat feedback without rewriting historical checkpoints."""
import hashlib
import json
from pathlib import Path

root = Path('Docs/UIThemes/MidnightFieldJournal')
out = root / 'BoldStatFeedback'
digest = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
before = json.loads((out / 'Before.json').read_text())
changed = sorted(p for p, sha in before.items() if not Path(p).exists() or digest(Path(p)) != sha)
assert changed == sorted([
    'Assets/Scripts/CreatureCardStatAnimator.cs',
    'Assets/UIThemes/MidnightFieldJournal/Editor/JournalStatFeedbackChecks.cs',
    'Assets/UIThemes/MidnightFieldJournal/Editor/JournalChainStatChecks.cs',
    'Assets/UIThemes/MidnightFieldJournal/Prefabs/CreatureCard.prefab',
]), changed
assert all(p.as_posix() in before for folder in ['Assets', 'ProjectSettings'] for p in Path(folder).rglob('*') if p.is_file())
main = json.loads((out / 'MainCard/PlayModeVerification.json').read_text())
chain = json.loads((out / 'Chain/PlayModeVerification.json').read_text())
assert main['passed'] and main['assertions'] == 54 and chain['passed'] and chain['assertions'] == 103
assert not (out / 'UnityCheckpoint.json').exists(), 'Already finalized'

def write_json(path, value):
    path.write_text(json.dumps(value, indent=2) + '\n', encoding='utf-8')

write_json(out / 'Preservation.json', {
    'passed': True, 'changedExistingFiles': changed, 'protectedFilesUnchanged': len(before) - len(changed),
    'artFontsDefinitionsOtherPrefabsScenesSettingsAndGameplayUnchanged': True,
    'candidatePrefabChanges': 'Native save of step timing and maximum count duration only',
})
write_json(out / 'Summary.json', {
    'passed': True, 'mainCardChecks': 54, 'chainChecks': 103,
    'count8To12Seconds': main['countDuration'], 'ordinaryStepSeconds': .04,
    'largeDeltaNominalCountSeconds': .8, 'boldWhileAnimatingAndModified': True,
    'sceneClean': True, 'consoleErrors': 0, 'penpotEdited': False,
})
entry = '''

## Bold manipulated stats and adjusted pace — MFJ-SYNC-006.15

Implemented in Unity on 20 October 2026. Both main and compact creature numbers use 40ms ordinary steps instead of 25ms; large-delta counting targets 0.8 seconds instead of 0.6. Initial hold, per-change pop/tilt, final spring, card pop/flash and chain ordering are retained. Manipulated stats become bold while animating and remain bold at a non-default result. Reaching the default and finishing its landing restores the original weight. Queued, hidden and neutral Bait-zero fields retain normal styling. [Verification and current baseline](BoldStatFeedback/README.md). No Penpot revision or pending import was added.
'''
for name in ['Design Changes.md', 'Pending Unity Changes.md', 'VisualContract.md']:
    path = root / name
    text = path.read_text(encoding='utf-8').replace('(RapidStatFeedback/UnityCheckpoint.json)', '(BoldStatFeedback/UnityCheckpoint.json)')
    path.write_text(text.rstrip() + entry, encoding='utf-8')
architecture = Path('Docs/Architecture.md')
architecture.write_text(architecture.read_text(encoding='utf-8').rstrip() + '''

`CreatureCardStatAnimator` also owns manipulated-number font emphasis. Each counter captures original TMP FontStyles and FontWeight alongside colour/transform state. Active manipulation (including the landing spring) uses Bold; a settled non-default result keeps it. Returning to default restores the captured style/weight, and masking/reset/static reuse use the same restoration boundary. Deferred queued fields do not turn bold early; Bait zero remains neutral. Current canonical pacing is 40ms per ordinary step with a nominal 0.8-second large-delta count; the existing 50ms hold and 180ms spring remain. Native prefab migration and runtime compact defaults share these constants. [Verification](UIThemes/MidnightFieldJournal/BoldStatFeedback/README.md).
''', encoding='utf-8')
spec = Path('Docs/Card Visual Specification.md')
spec.write_text(spec.read_text(encoding='utf-8').rstrip() + '''

Current numeric pacing is 40ms per ordinary step, with a nominal 0.8-second large-delta count; retain the existing 50ms hold and 180ms spring. Manipulated main/compact numbers are bold during animation and stay bold at a non-default result. On return to the default, restore the original font style/weight after the landing completes. Preserve existing colour, enlargement and per-change pop/tilt. Deferred queued numbers do not turn bold before their turn. Hidden values, static snapshots and neutral Bait-zero Value retain their normal styling.
''', encoding='utf-8')
for name in ['DesignChangeLedger.json', 'PendingUnityChanges.json']:
    path = root / name
    data = json.loads(path.read_text(encoding='utf-8'))
    data['currentUnityCheckpoint'] = 'BoldStatFeedback/UnityCheckpoint.json'
    write_json(path, data)
previous = json.loads((root / 'RapidStatFeedback/UnityCheckpoint.json').read_text())
paths = sorted(set(f['path'] for f in previous['files']) | set(changed))
write_json(out / 'UnityCheckpoint.json', {
    'checkpoint': 'MFJ-SYNC-006.15', 'date': '2026-10-20',
    'previousCheckpoint': '../RapidStatFeedback/UnityCheckpoint.json',
    'kind': 'Bold manipulated numbers and adjusted pace', 'revisions': [],
    'files': [{'path': p, 'sha256': digest(Path(p))} for p in paths],
    'validation': 'Summary.json', 'preservation': 'Preservation.json',
})
print('Recorded MFJ-SYNC-006.15; 54 main-card and 103 chain checks passed.')
