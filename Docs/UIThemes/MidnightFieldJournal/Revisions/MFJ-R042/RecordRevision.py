"""Record the Penpot-only modifier-tooltip design as pending, preserving every Unity checkpoint."""
import hashlib
import json
import struct
from pathlib import Path

root = Path('Docs/UIThemes/MidnightFieldJournal')
folder = root / 'Revisions/MFJ-R042'
design = json.loads((folder / 'PenpotDesign.json').read_text(encoding='utf-8'))
ledger = json.loads((root / 'DesignChangeLedger.json').read_text())
assert ledger['nextRevisionId'] == 'MFJ-R042'
assert not any(c['id'] == 'MFJ-R042' for c in ledger['changes'])
for c in design['configs']:
    assert c['base'] + sum(r['amount'] for r in c['rows']) == c['current']
    assert c['current'] - c['base'] == c['net']
assert all(not x['overflow'] for x in design['fields'])
digest = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
checkpoint = json.loads((root / ledger['currentUnityCheckpoint']).read_text())
assert all(digest(Path(f['path'])) == f['sha256'] for f in checkpoint['files'])

def write_json(path, data):
    path.write_text(json.dumps(data, indent=2, ensure_ascii=False) + '\n', encoding='utf-8')

frame = folder / 'Art/ModifierTooltipFrameV1.png'
width, height = struct.unpack('>II', frame.read_bytes()[16:24])
write_json(folder / 'Assets.json', {
    'revision': 'MFJ-R042', 'status': 'ready_for_review', 'unityImported': False,
    'generation': 'Built-in image_gen; new independent frame, no slicing or paid external API',
    'assets': [{'id': 'ModifierTooltipFrameV1', 'path': frame.as_posix(), 'sha256': digest(frame),
                'width': width, 'height': height, 'penpotShapeId': design['frameId'], 'unityImported': False}],
    'dynamicFieldsSeparate': True, 'nativeIcons': 'CreateDesign.js source emblems',
    'fonts': ['Cinzel', 'Source Serif 4'],
})
write_json(folder / 'Verification.json', {
    'status': 'design_verified', 'variants': 2, 'editableTextFields': sum(len(x['fields']) for x in design['fields']),
    'textBoundsContained': True, 'exampleArithmeticCorrect': True,
    'prototypeConnectionsVerified': ['mouse-enter opens independent overlay', 'mouse-leave closes overlay'],
    'prototypeInteractionPlayedInViewer': False,
    'visualInspection': 'TooltipReviewV2.png inspected; footer clear of corner art and Environment symbol corrected',
    'gameplayPreview': 'Assembled as editable Gameplay H; full PNG export timed out and is not claimed visually verified',
    'unityCheckpointPreserved': ledger['currentUnityCheckpoint'],
    'unityFilesVerifiedUnchanged': len(checkpoint['files']), 'unityImported': False,
})
entry = {
    'id': 'MFJ-R042', 'date': '2026-10-20',
    'request': 'Design a themed hover tooltip explaining modified creature Weight/Value and all contributing sources, in Penpot.',
    'component': ['Modified-stat tooltip'], 'status': 'ready_for_review', 'supersedes': [],
    'penpotSources': design,
    'affectedPenpotInstances': [design[k] for k in ['reviewBoardId','valueTooltipId','weightTooltipId','gameplayBoardId','prototypeBoardId','overlayBoardId']],
    'changeKinds': ['new independent engraved frame', 'editable breakdown fields', 'native source emblems', 'hover prototype'],
    'before': 'Modified numbers have colour/scale/bold feedback without a source-breakdown tooltip.',
    'after': '900x1020 parchment tooltip with stat/creature heading, default/current comparison, signed source rows, explanations and net change; Value and Weight examples plus editable gameplay preview.',
    'unityAssetIds': ['ModifierTooltipFrameV1'],
    'unityPaths': ['Assets/UIThemes/MidnightFieldJournal/Art/Tooltips/ModifierTooltipFrameV1.png (proposed, not created)',
                   'Assets/Scripts/CreatureCardView.cs', 'Assets/Scripts/CompactCatchCardView.cs',
                   'Assets/UIThemes/MidnightFieldJournal/Runtime/JournalCatchChainPresentation.cs'],
    'dependentConsumers': ['Main creature modified Weight/Value hover and focus', 'Compact Catch/Bait modified Weight/Value hover and focus',
                          'Tooltip canvas and viewport placement', 'Runtime modifier attribution across creatures, events and Techniques'],
    'configurationChanges': 'New tooltip only; keep existing creature-effect inspection and all Unity-only animation refinements. Bind real attribution, never hardcode the illustrative rows. Keep text editable and border independent.',
    'exportRequirements': {'designResolution': [3840,2160], 'tooltipCanvas': [900,1020],
                           'newRasterImages': ['ModifierTooltipFrameV1'], 'slices': False, 'dynamicFieldsSeparate': True,
                           'nativeVectorAndText': True, 'variableRowCount': True},
    'approvalEvidence': 'User requested the Penpot design; this visual is ready for review, not yet approved for Unity import.',
    'exportEvidence': {'review': 'Revisions/MFJ-R042/TooltipReviewV2.png', 'frame': 'Revisions/MFJ-R042/Art/ModifierTooltipFrameV1.png',
                       'geometry': 'Revisions/MFJ-R042/PenpotDesign.json', 'prompt': 'Revisions/MFJ-R042/GenerationPrompt.json'},
    'importEvidence': None, 'verificationEvidence': {'report': 'Revisions/MFJ-R042/Verification.json'},
    'remainingWork': ['Visual review', 'Unity integration only when requested; audit and bind complete runtime cause attribution',
                      'Test hover/focus, masking, modal locks, viewport clamping, mixed/stacked/conditional effects and variable row count'],
    'unityImported': False, 'integrationStatus': 'not_imported', 'synchronizationCheckpoint': None,
}
assert all(k in entry for k in ledger['requiredEntryFields'])
ledger['changes'].append(entry); ledger['nextRevisionId'] = 'MFJ-R043'
write_json(root / 'DesignChangeLedger.json', ledger)
index = json.loads((root / 'PendingUnityChanges.json').read_text())
index['generatedDate'] = '2026-10-20'
index['pending'].append({'id':'MFJ-R042','status':'ready_for_review','component':['Modified-stat tooltip'],
                         'manifest':'Revisions/MFJ-R042/Assets.json','unityImported':False})
write_json(root / 'PendingUnityChanges.json', index)

summary = '''

## MFJ-R042 — Modified-stat source tooltip (Penpot only)

Ready for review on **Concept B — Reusable Assembly (4K)**: select **MFJ-R042 / Stat modifier tooltips / editable review**. Detailed independent parchment frame, editable default/current amounts, signed source rows, source emblems and explanations, plus net change. Lionfish Value and Cod Weight examples cover creature, Technique, Hazard and Environment causes. Gameplay H shows an assembled example; the separate prototype wires hover on Lionfish Value to an independent overlay. [Preview and source contract](Revisions/MFJ-R042/README.md).

**Pending Unity integration; not imported.** Existing creature-effect tooltips and all integrated revisions remain unchanged. Preserve the current Unity-only numeric/chain animation refinements when later adding this tooltip. Runtime attribution must provide the real causes; the Penpot example rows are illustrative content only.
'''
for name in ['Design Changes.md','Pending Unity Changes.md']:
    path = root / name
    text = path.read_text(encoding='utf-8')
    if name == 'Design Changes.md':
        start = text.index('## Pending Unity changes')
        end = text.index('## Change history', start)
        text = text[:start] + '## Pending Unity changes\n\n**One pending revision: MFJ-R042 — modified-stat source tooltip, ready for review.** 37 imported/verified revisions and four superseded drafts remain preserved. Use [the pending index](PendingUnityChanges.json) and the current [MFJ-SYNC-006.16 checkpoint](DisplayedStatColours/UnityCheckpoint.json) for selective integration.\n\n' + text[end:]
    else:
        end = text.index('## Explicit creature role choice')
        text = '# Penpot changes awaiting Unity integration\n\n**Current: 1 pending revision (MFJ-R042); 37 imported and verified; 4 superseded drafts.** [Pending index](PendingUnityChanges.json) · [Full ledger](DesignChangeLedger.json).\n\nLatest integration baseline: [MFJ-SYNC-006.16](DisplayedStatColours/UnityCheckpoint.json). Next design revision: **MFJ-R043**. Compare against this current checkpoint, preserve all Unity-only refinements and import only new pending entries. Never use a screenshot assembly as game UI artwork.\n\n' + text[end:]
    path.write_text(text.rstrip() + summary, encoding='utf-8')
path = root / 'VisualContract.md'
text = path.read_text(encoding='utf-8').replace('0 pending,37 completed,4 superseded','1 pending,37 completed,4 superseded',1)
path.write_text(text.rstrip() + summary, encoding='utf-8')
print('Recorded MFJ-R042 as ready for review; 1 pending import. Unity checkpoint unchanged.')
