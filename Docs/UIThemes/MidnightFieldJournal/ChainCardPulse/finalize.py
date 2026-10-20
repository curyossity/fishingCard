"""Record the verified Unity-only card pulse without rewriting historical integrations."""
import hashlib
import json
from pathlib import Path

root = Path('Docs/UIThemes/MidnightFieldJournal')
out = root / 'ChainCardPulse'
digest = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
before = json.loads((out / 'Before.json').read_text())
changed = sorted(p for p, sha in before.items() if not Path(p).exists() or digest(Path(p)) != sha)
expected = sorted([
    'Assets/UIThemes/MidnightFieldJournal/Runtime/JournalCatchChainPresentation.cs',
    'Assets/UIThemes/MidnightFieldJournal/Runtime/JournalCatchChainScrollRect.cs',
    'Assets/UIThemes/MidnightFieldJournal/Editor/JournalChainStatChecks.cs',
])
assert changed == expected, changed
new = sorted(p.as_posix() for folder in ['Assets', 'ProjectSettings'] for p in Path(folder).rglob('*') if p.is_file() and p.as_posix() not in before)
assert new == [
    'Assets/UIThemes/MidnightFieldJournal/Runtime/JournalCatchCardPulse.cs',
    'Assets/UIThemes/MidnightFieldJournal/Runtime/JournalCatchCardPulse.cs.meta',
], new
report = json.loads((out / 'PlayModeVerification.json').read_text())
assert report['passed'] and report['assertions'] == 89 and report['pulsedRecipients'] == 7
assert not (out / 'UnityCheckpoint.json').exists(), 'Already finalized'

def write_json(path, value):
    path.write_text(json.dumps(value, indent=2) + '\n', encoding='utf-8')

write_json(out / 'Preservation.json', {
    'passed': True, 'changedExistingFiles': changed, 'newFiles': new,
    'protectedFilesUnchanged': len(before) - len(changed),
    'gameplaySharedAnimatorArtFontsDefinitionsPrefabsScenesAndSettingsUnchanged': True,
})
write_json(out / 'Summary.json', {
    'passed': True, 'playModeChecks': 89, 'chainOrder': report['order'],
    'pulsedRecipients': report['pulsedRecipients'], 'peakScale': report['peakScale'], 'peakFlash': report['peakFlash'],
    'sceneClean': True, 'penpotEdited': False,
    'unrelatedEditorTeardownObservation': 'Stopping Play Mode while a Technique was hovered emitted SetSiblingIndex from unchanged JournalTechniquePose.OnDisable (line 46). Not a card-pulse assertion failure; see README.',
})

architecture = Path('Docs/Architecture.md')
architecture.write_text(architecture.read_text(encoding='utf-8').rstrip() + '''

`JournalCatchCardPulse` adds a presentation-only prelude to each queued recipient after scroll focus arrives. It wraps that compact card's existing visual children once in a centered motion root; the card's top-anchored layout/input root and independent chain connectors stay stationary. Its native MaskableGraphic paints a pointer-free rounded warm-ivory flash. The queue waits for the 0.38-second, 4.5% pop/18%-opacity flash to return exactly to rest before unpausing number feedback. Repeated refreshes retain the pulse phase. Manual interruption, removal, disabling and ended-run state stop the pulse and restore resting scale/opacity. Focus includes room for the expanded silhouette. Numeric animation and gameplay owners are unchanged. [Verification](UIThemes/MidnightFieldJournal/ChainCardPulse/README.md).
''', encoding='utf-8')
spec = Path('Docs/Card Visual Specification.md')
spec.write_text(spec.read_text(encoding='utf-8').rstrip() + '''

Before each affected candidate compact creature's number animation, play one gentle centered card pop (4.5% maximum) with a rounded warm-ivory flash (18% peak opacity) over 0.38 seconds. Use the same prelude for beneficial and harmful changes; numbers retain benefit/harm colours. Scroll into view first with expansion padding. Keep chain hardware stationary. Restore normal card scale/zero flash before counting, and finish counting/spring settling before starting the next recipient. No repeating flash, new artwork, frame or input-blocking overlay is added.
''', encoding='utf-8')
entry = '''

## Catch Chain card pop and flash — MFJ-SYNC-006.13

Implemented directly in Unity on 20 October 2026. Each affected Catch/Bait now receives a gentle 4.5% centered pop and warm ivory flash before its existing number animation. Scroll padding accommodates the expansion; chain connectors remain still. The newest-to-oldest sequence waits for both stages, and interruption/removal restores resting visuals. [Verification and current baseline](ChainCardPulse/README.md). No Penpot revision or pending import was added.
'''
for name in ['Design Changes.md', 'Pending Unity Changes.md']:
    path = root / name
    path.write_text(path.read_text(encoding='utf-8').rstrip() + entry, encoding='utf-8')
path = root / 'VisualContract.md'
text = path.read_text(encoding='utf-8').replace('(ChainStatFeedback/UnityCheckpoint.json)', '(ChainCardPulse/UnityCheckpoint.json)')
path.write_text(text.rstrip() + entry, encoding='utf-8')
for name in ['DesignChangeLedger.json', 'PendingUnityChanges.json']:
    path = root / name
    data = json.loads(path.read_text(encoding='utf-8'))
    data['currentUnityCheckpoint'] = 'ChainCardPulse/UnityCheckpoint.json'
    write_json(path, data)

previous = json.loads((root / 'ChainStatFeedback/UnityCheckpoint.json').read_text())
paths = sorted(set(f['path'] for f in previous['files']) | set(changed) | set(new))
write_json(out / 'UnityCheckpoint.json', {
    'checkpoint': 'MFJ-SYNC-006.13', 'date': '2026-10-20',
    'previousCheckpoint': '../ChainStatFeedback/UnityCheckpoint.json',
    'kind': 'Catch Chain card pop and warm flash', 'revisions': [],
    'files': [{'path': p, 'sha256': digest(Path(p))} for p in paths],
    'validation': 'Summary.json', 'preservation': 'Preservation.json',
})
print('Recorded MFJ-SYNC-006.13; 89 Play Mode assertions passed.')
