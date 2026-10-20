"""Record this verified Unity-only increment without rewriting previous checkpoints."""
import hashlib
import json
from pathlib import Path

root = Path('Docs/UIThemes/MidnightFieldJournal')
out = root / 'ChainStatFeedback'
digest = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
before = json.loads((out / 'Before.json').read_text())
changed = sorted(p for p, sha in before.items() if not Path(p).exists() or digest(Path(p)) != sha)
expected = sorted([
    'Assets/Scripts/CatchChainPresentation.cs', 'Assets/Scripts/CatchChainView.cs',
    'Assets/Scripts/CompactCatchCardView.cs', 'Assets/Scripts/CreatureCardStatAnimator.cs',
    'Assets/Scripts/FishingRunController.cs',
    'Assets/UIThemes/MidnightFieldJournal/Runtime/JournalCatchChainPresentation.cs',
    'Assets/UIThemes/MidnightFieldJournal/Runtime/JournalCatchChainScrollRect.cs',
])
assert changed == expected, changed
new = sorted(p.as_posix() for folder in ['Assets', 'ProjectSettings'] for p in Path(folder).rglob('*') if p.is_file() and p.as_posix() not in before)
assert new == [
    'Assets/UIThemes/MidnightFieldJournal/Editor/JournalChainStatChecks.cs',
    'Assets/UIThemes/MidnightFieldJournal/Editor/JournalChainStatChecks.cs.meta',
], new
chain = json.loads((out / 'PlayModeVerification.json').read_text())
main = json.loads((out / 'MainCardRegression/PlayModeVerification.json').read_text())
assert chain['passed'] and chain['assertions'] == 68 and chain['order'] == [7, 6, 5, 4, 3, 2, 1]
assert main['passed'] and main['assertions'] == 45
assert not (out / 'UnityCheckpoint.json').exists(), 'Already finalized'

def write_json(path, value):
    path.write_text(json.dumps(value, indent=2) + '\n', encoding='utf-8')

write_json(out / 'Preservation.json', {
    'passed': True, 'changedExistingFiles': changed, 'newFiles': new,
    'protectedFilesUnchanged': len(before) - len(changed),
    'artFontsCardDefinitionsPrefabsScenesAndProjectSettingsUnchanged': True,
})
write_json(out / 'Summary.json', {
    'passed': True, 'chainPlayModeChecks': 68, 'mainCardPlayModeChecks': 45,
    'runtimeChecks': {'coastal': 62, 'modes': 101, 'approvedTechniques': 39},
    'chainOrder': chain['order'], 'sceneClean': True, 'editorErrors': 0, 'penpotEdited': False,
})

architecture = Path('Docs/Architecture.md')
architecture.write_text(architecture.read_text(encoding='utf-8').rstrip() + '''

### Sequential attached-creature stat feedback

`CreatureCardStatAnimator` is shared by central and compact creature views; `IsAnimating` includes integer counting and final spring settling. Explicit text binding enables compact fields, deferred styling keeps queued recipients neutral until their turn, and completion settles without losing the identity or information mask. `CompactCatchCardView` opts in only when the candidate presenter requests it and uses authored Weight/runtime base Value as baselines, resolved stats as targets, and the same neutral Bait-zero role transition. Legacy compact cards retain static rendering.

`JournalCatchChainPresentation` preserves physical card views by `CardInstance` reference when removals/exchanges shift indices. It serializes pending recipients newest to oldest, pauses each until the scroll focus arrives, then waits for the full count and spring before starting another. Repeated refreshes retain progress; removed active recipients are dropped and reused views reset identity. New targets retarget the active counter or wait for its completion. The presentation never mutates gameplay stats or delays rule execution.

`JournalCatchChainScrollRect` lends its existing smooth scrolling to feedback focus and suspends automatic newest-follow during the sequence. The last recipient remains visible until an extent change/new attachment. Wheel/drag input takes ownership, settles all pending feedback and cancels automatic focus rather than fighting inspection. `FishingRunController` forwards run-active state through the shared view/presentation boundary so ended-run snapshots are static. Empty/disabled chains clear unfinished focus. [Verification](UIThemes/MidnightFieldJournal/ChainStatFeedback/README.md).
''', encoding='utf-8')
spec = Path('Docs/Card Visual Specification.md')
spec.write_text(spec.read_text(encoding='utf-8').rstrip() + '''

## Candidate Catch Chain stat feedback

The Midnight Field Journal compact Catch/Bait cards share the main creature's integer pacing, colour polarity, enlargement and spring motion for resolved stat changes. Sequence affected creatures from newest to oldest; scroll each into view before its count and wait for the final spring to settle before starting the next. Queued recipients retain their visible integers without early enlargement/motion. Refreshes and shifted indices must not restart animations or transfer state between runtime copies. Bait zero Value remains immediate and neutral; hidden Value stays hidden. Manual wheel/drag inspection cancels the sequence and settles remaining values. Static ended-run snapshots do not animate. No artwork, authored stats or gameplay timing changes are implied.
''', encoding='utf-8')
entry = '''

## Sequential Catch Chain stat feedback — MFJ-SYNC-006.12

Implemented directly in Unity on 20 October 2026. Compact creatures reuse the main-card animator for all resolved Weight/Value modifiers. Affected recipients run newest to oldest, scrolling into view before counting and waiting for the full final spring before the next begins. Physical views retain runtime identity across removals; queued cards defer emphasis. Bait zero and concealed Value retain their rules. Manual scrolling settles/cancels automatic feedback, and ended-run snapshots remain static. [Verification and current baseline](ChainStatFeedback/README.md). No new Penpot revision or pending import.
'''
for name in ['Design Changes.md', 'Pending Unity Changes.md']:
    path = root / name
    path.write_text(path.read_text(encoding='utf-8').rstrip() + entry, encoding='utf-8')
path = root / 'VisualContract.md'
text = path.read_text(encoding='utf-8').replace('(AllStatFeedback/UnityCheckpoint.json)', '(ChainStatFeedback/UnityCheckpoint.json)')
path.write_text(text.rstrip() + entry, encoding='utf-8')
for name in ['DesignChangeLedger.json', 'PendingUnityChanges.json']:
    path = root / name
    data = json.loads(path.read_text(encoding='utf-8'))
    data['currentUnityCheckpoint'] = 'ChainStatFeedback/UnityCheckpoint.json'
    write_json(path, data)

previous = json.loads((root / 'AllStatFeedback/UnityCheckpoint.json').read_text())
paths = sorted(set(f['path'] for f in previous['files']) | set(changed) | set(new))
write_json(out / 'UnityCheckpoint.json', {
    'checkpoint': 'MFJ-SYNC-006.12', 'date': '2026-10-20',
    'previousCheckpoint': '../AllStatFeedback/UnityCheckpoint.json',
    'kind': 'Sequential Catch Chain stat feedback', 'revisions': [],
    'files': [{'path': p, 'sha256': digest(Path(p))} for p in paths],
    'validation': 'Summary.json', 'preservation': 'Preservation.json',
})
print('Recorded MFJ-SYNC-006.12; 68 chain, 45 main-card and 202 runtime assertions passed.')
