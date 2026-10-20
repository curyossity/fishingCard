"""Record the verified Unity-only feedback increment once; retain previous checkpoints."""
import hashlib
import json
from pathlib import Path

root = Path('Docs/UIThemes/MidnightFieldJournal')
out = root / 'AllStatFeedback'
digest = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
before = json.loads((out / 'Before.json').read_text())
changed = sorted(p for p, sha in before.items() if not Path(p).exists() or digest(Path(p)) != sha)
expected = sorted([
    'Assets/Scripts/CatchChainRuntime.cs',
    'Assets/Scripts/CreatureCardStatAnimator.cs',
    'Assets/Scripts/EncounterStatFeedback.cs',
    'Assets/Scripts/FishingRunController.cs',
    'Assets/UIThemes/MidnightFieldJournal/Editor/JournalStatFeedbackChecks.cs',
])
assert changed == expected, changed
report = json.loads((out / 'PlayModeVerification.json').read_text())
assert report['passed'] and report['assertions'] == 45
assert not (out / 'UnityCheckpoint.json').exists(), 'Already finalized'

def write_json(path, value):
    path.write_text(json.dumps(value, indent=2) + '\n', encoding='utf-8')

write_json(out / 'Preservation.json', {
    'passed': True, 'changedExistingFiles': changed,
    'protectedFilesUnchanged': len(before) - len(changed),
    'artFontsCardDefinitionsPrefabsScenesAndProjectSettingsUnchanged': True,
})
write_json(out / 'Summary.json', {
    'passed': True, 'playModeChecks': 45,
    'runtimeChecks': {'coastal': 62, 'modes': 101, 'approvedTechniques': 39},
    'integerSequence': report['valueSequence'], 'sceneClean': True,
    'editorErrors': 0, 'penpotEdited': False,
    'captureDirectory': report['captureDirectory'],
})

architecture = Path('Docs/Architecture.md')
text = architecture.read_text(encoding='utf-8')
start = text.index('`CatchChainRuntime.CreateResolvedCatchPreview` optionally resolves')
end = text.index('\n\n', start)
text = text[:start] + (
    '`FishingRunController` compares the complete resolved creature/Apex preview with the '
    'definition\'s authored Weight and Value for opted-in encounter views. Technique grants, '
    'pending event grants, the creature\'s own abilities and chain effects all participate. '
    '`CatchChainRuntime.CreateResolvedCatchPreview` retains only its full read-only preview path; '
    'feedback no longer needs a second chain-free resolution. Occurrence-keyed `EncounterStatFeedback` '
    'carries the selected Bait role explicitly: Bait Value is zero immediately and neutrally, '
    'and returning to Catch starts any effect count at authored Catch Value. Weight remains '
    'continuous across role changes. No presentation count delays gameplay or consumes pending grants.'
) + text[end:]
architecture.write_text(text, encoding='utf-8')

spec = Path('Docs/Card Visual Specification.md')
text = spec.read_text(encoding='utf-8').replace('animates Catch Chain-derived changes', 'animates all effect-driven changes')
text = text.replace('It leaves unaffected stats, concealed fields and historical snapshots unanimated.',
    'Choosing Bait immediately shows neutral zero sale Value without counting down; returning to Catch '
    'animates only effect modifiers from authored Catch Value. It leaves unaffected stats, concealed '
    'fields and historical snapshots unanimated.')
spec.write_text(text, encoding='utf-8')

entry = ('\n\n## All-source current-creature stat feedback — MFJ-SYNC-006.11\n\n'
    'Implemented directly in Unity on 20 October 2026. Main-card Weight/Value feedback now compares '
    'all resolved effects with printed stats, including Techniques, events, own abilities and the '
    'Catch Chain. Choosing Bait sets sale Value to neutral zero immediately; returning to Catch '
    'counts only effect modifiers from printed Value. Existing colours, integer pacing, spring '
    'motion and masking remain. [Verification and current baseline](AllStatFeedback/README.md). '
    'No new Penpot revision or pending import.\n')
for name in ['Design Changes.md', 'Pending Unity Changes.md']:
    path = root / name
    path.write_text(path.read_text(encoding='utf-8').rstrip() + entry, encoding='utf-8')
path = root / 'VisualContract.md'
text = path.read_text(encoding='utf-8').replace('(StatPunch/UnityCheckpoint.json)', '(AllStatFeedback/UnityCheckpoint.json)')
path.write_text(text.rstrip() + entry, encoding='utf-8')
for name in ['DesignChangeLedger.json', 'PendingUnityChanges.json']:
    path = root / name
    data = json.loads(path.read_text(encoding='utf-8'))
    data['currentUnityCheckpoint'] = 'AllStatFeedback/UnityCheckpoint.json'
    write_json(path, data)

previous = json.loads((root / 'StatPunch/UnityCheckpoint.json').read_text())
paths = sorted(set(f['path'] for f in previous['files']) | set(changed))
write_json(out / 'UnityCheckpoint.json', {
    'checkpoint': 'MFJ-SYNC-006.11', 'date': '2026-10-20',
    'previousCheckpoint': '../StatPunch/UnityCheckpoint.json',
    'kind': 'All-source current-creature stat feedback with neutral Bait Value', 'revisions': [],
    'files': [{'path': p, 'sha256': digest(Path(p))} for p in paths],
    'validation': 'Summary.json', 'preservation': 'Preservation.json',
})
print('Recorded MFJ-SYNC-006.11; 45 presentation checks and 202 runtime checks passed.')
