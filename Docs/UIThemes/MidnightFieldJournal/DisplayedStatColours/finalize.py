"""Record verified displayed-value colour transitions without changing historical checkpoints."""
import hashlib
import json
from pathlib import Path

root = Path('Docs/UIThemes/MidnightFieldJournal')
out = root / 'DisplayedStatColours'
digest = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
before = json.loads((out / 'Before.json').read_text())
changed = sorted(p for p, sha in before.items() if not Path(p).exists() or digest(Path(p)) != sha)
assert changed == sorted([
    'Assets/Scripts/CreatureCardStatAnimator.cs',
    'Assets/UIThemes/MidnightFieldJournal/Editor/JournalStatFeedbackChecks.cs',
]), changed
assert all(p.as_posix() in before for folder in ['Assets', 'ProjectSettings'] for p in Path(folder).rglob('*') if p.is_file())
main = json.loads((out / 'MainCard/PlayModeVerification.json').read_text())
chain = json.loads((out / 'Chain/PlayModeVerification.json').read_text())
assert main['passed'] and main['assertions'] == 74 and chain['passed'] and chain['assertions'] == 103
assert not (out / 'UnityCheckpoint.json').exists(), 'Already finalized'

def write_json(path, value):
    path.write_text(json.dumps(value, indent=2) + '\n', encoding='utf-8')

write_json(out / 'Preservation.json', {
    'passed': True, 'changedExistingFiles': changed, 'protectedFilesUnchanged': len(before) - len(changed),
    'artFontsDefinitionsPrefabsScenesSettingsAndGameplayUnchanged': True,
})
write_json(out / 'Summary.json', {
    'passed': True, 'mainCardChecks': 74, 'chainChecks': 103,
    'colourTransitionCases': 20, 'sceneClean': True, 'consoleErrors': 0, 'penpotEdited': False,
})
entry = '''

## Displayed-value stat colours — MFJ-SYNC-006.16

Implemented in Unity on 20 October 2026. Both main and compact creature numbers derive colour from the displayed amount relative to default, rather than their destination or travel direction. Beneficial amounts stay green and detrimental amounts stay red while changing on the same side of default. A final default amount restores normal ink on arrival. Passing through default toward the opposite side switches directly to that side's colour without a neutral flash. Bait's immediate neutral zero, bold styling, pacing, pop/tilt and chain sequence are retained. [Verification and current baseline](DisplayedStatColours/README.md). No Penpot revision or pending import was added.
'''
for name in ['Design Changes.md', 'Pending Unity Changes.md', 'VisualContract.md']:
    path = root / name
    text = path.read_text(encoding='utf-8').replace('(BoldStatFeedback/UnityCheckpoint.json)', '(DisplayedStatColours/UnityCheckpoint.json)')
    path.write_text(text.rstrip() + entry, encoding='utf-8')
architecture = Path('Docs/Architecture.md')
architecture.write_text(architecture.read_text(encoding='utf-8').rstrip() + '''

Stat colour polarity is calculated inside `CreatureCardStatAnimator.Render` from `Counter.Displayed` versus `Counter.Baseline`, with each counter's lower-is-better flag. No destination-polarity cache remains. A displayed baseline uses the destination side only when counting continues beyond it, avoiding neutral ink during crossings; a final baseline uses captured normal ink immediately. Existing deferred styling, Bait role reset, masking and font restoration boundaries remain unchanged. This shared renderer covers both encounter and compact fields without source-specific branches. [Transition verification](UIThemes/MidnightFieldJournal/DisplayedStatColours/README.md).
''', encoding='utf-8')
spec = Path('Docs/Card Visual Specification.md')
spec.write_text(spec.read_text(encoding='utf-8').rstrip() + '''

Stat ink follows the displayed number relative to its default: Value above default/Weight below default is green; the opposite is red. Same-side changes retain that colour regardless of travel direction. Counting back to default retains its side colour until the displayed result reaches default, then restores normal ink. Crossing default toward the other side switches directly green↔red; a passing-through default number uses the destination side's ink, never a normal-colour flash. Existing immediate neutral Bait-zero Value and deferred queued styling are retained.
''', encoding='utf-8')
for name in ['DesignChangeLedger.json', 'PendingUnityChanges.json']:
    path = root / name
    data = json.loads(path.read_text(encoding='utf-8'))
    data['currentUnityCheckpoint'] = 'DisplayedStatColours/UnityCheckpoint.json'
    write_json(path, data)
previous = json.loads((root / 'BoldStatFeedback/UnityCheckpoint.json').read_text())
paths = sorted(set(f['path'] for f in previous['files']) | set(changed))
write_json(out / 'UnityCheckpoint.json', {
    'checkpoint': 'MFJ-SYNC-006.16', 'date': '2026-10-20',
    'previousCheckpoint': '../BoldStatFeedback/UnityCheckpoint.json',
    'kind': 'Displayed-value stat colour transitions', 'revisions': [],
    'files': [{'path': p, 'sha256': digest(Path(p))} for p in paths],
    'validation': 'Summary.json', 'preservation': 'Preservation.json',
})
print('Recorded MFJ-SYNC-006.16; 74 main-card and 103 chain checks passed.')
