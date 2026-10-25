"""Record the compact Penpot alternative without changing Unity or historical completion records."""
import hashlib
import json
import struct
from pathlib import Path

root = Path('Docs/UIThemes/MidnightFieldJournal')
folder = root / 'Revisions/MFJ-R043'
design = json.loads((folder / 'PenpotDesign.json').read_text(encoding='utf-8'))
ledger = json.loads((root / 'DesignChangeLedger.json').read_text(encoding='utf-8'))
index = json.loads((root / 'PendingUnityChanges.json').read_text(encoding='utf-8'))
assert ledger['nextRevisionId'] == 'MFJ-R043'
assert not any(c['id'] == 'MFJ-R043' for c in ledger['changes'])
digest = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
checkpoint = json.loads((root / ledger['currentUnityCheckpoint']).read_text(encoding='utf-8'))
assert all(digest(Path(f['path'])) == f['sha256'] for f in checkpoint['files'])
assert len(index['completed']) == 37 and len(index['superseded']) == 4
for r in design['records']:
    assert r['base'] + sum(x['amount'] for x in r['rows']) == r['current']
    assert r['visibleRows'] == min(3, len(r['rows']))
    assert len(r['tiles']) == 9
    assert r['width'] == 680
    assert r['height'] == 276 + r['visibleRows'] * 72 + (44 if len(r['rows']) > 3 else 0)
for name in ['CompactReview.png', 'OverflowReview.png', 'NineSliceGuide.png']:
    assert (folder / name).read_bytes().startswith(b'\x89PNG\r\n\x1a\n')

def write(path, data):
    path.write_text(json.dumps(data, indent=2, ensure_ascii=False) + '\n', encoding='utf-8')

frame = root / 'Revisions/MFJ-R042/Art/ModifierTooltipFrameV1.png'
write(folder / 'Assets.json', {
    'revision':'MFJ-R043', 'status':'ready_for_review', 'unityImported':False,
    'alternativeGroup':'Modified-stat tooltip: choose R042 or R043 before integration',
    'generation':'No new raster generation; shared independently generated R042 frame and approved portraits; native icons/text',
    'assets':[{'id':'ModifierTooltipFrameV1','path':frame.as_posix(),'sha256':digest(frame),
               'dimensions':list(struct.unpack('>II',frame.read_bytes()[16:24])),
               'penpotShapeId':design['sourceFrameId'],'reusedFrom':'MFJ-R042'}],
    'sliceBorderPixels':design['sliceBorderPixels'], 'displayCapPixels':design['displayCapPixels'],
    'designResolution':[3840,2160], 'width':680, 'heights':[348,420,492,536],
    'rowHeight':72, 'maximumVisibleReasons':3, 'overflowViewportHeight':216,
    'fonts':['Cinzel','Source Serif 4'], 'dynamicFieldsSeparate':True,
    'unityImplementation':'Sliced Image with quarter-scale caps; clipped ScrollRect; real runtime source attribution',
})
write(folder / 'Verification.json', {
    'status':'design_verified', 'visualInspection':['CompactReview.png','OverflowReview.png','NineSliceGuide.png'],
    'resizing':'Fixed corner details retained across all sizes; no visible frame seams',
    'examples':5, 'exampleArithmeticCorrect':True, 'currentIncludesHiddenReasons':True,
    'maximumVisibleReasons':3, 'overflowStates':[0,1], 'nativeClippedViewports':True,
    'editableTextAndSeparateIcons':True, 'prototype':'Click navigation between first/last scroll states; not a live ScrollRect',
    'prototypeInteractionPlayedInViewer':False, 'mouseWheelOrThumbDraggingImplemented':False,
    'unityCheckpointPreserved':ledger['currentUnityCheckpoint'],
    'unityFilesVerifiedUnchanged':len(checkpoint['files']), 'unityImported':False,
})
entry = {
    'id':'MFJ-R043','date':'2026-10-20',
    'request':'Create a smaller modular stat tooltip with default/current, source name/icon/amount, up to three visible reasons and scroll overflow.',
    'component':['Modified-stat tooltip'],'status':'ready_for_review','supersedes':[],
    'alternativeGroup':'Modified-stat tooltip; retain R042 and R043 for comparison, select one before integration',
    'penpotSources':design,
    'affectedPenpotInstances':[design[k] for k in ['reviewBoardId','scrollReviewBoardId','guideBoardId','prototypeTopId','prototypeBottomId']],
    'changeKinds':['compact alternative','native nine-region frame','three-row clipping','scroll-state prototype'],
    'before':'R042 uses a 900x1020 detailed breakdown with explanations and net footer.',
    'after':'680px-wide variable-height parchment, default/current and icon/name/signed-amount rows only; three visible reasons, scrollbar and overflow indicator.',
    'unityAssetIds':['ModifierTooltipFrameV1'],
    'unityPaths':['Assets/UIThemes/MidnightFieldJournal/Art/Tooltips/ModifierTooltipFrameV1.png (proposed, not created)',
                  'Assets/Scripts/CreatureCardView.cs','Assets/Scripts/CompactCatchCardView.cs',
                  'Assets/UIThemes/MidnightFieldJournal/Runtime/JournalCatchChainPresentation.cs'],
    'dependentConsumers':['Main and compact creature modified Weight/Value inspection','Runtime modifier provenance','Tooltip placement, focus and scroll input'],
    'configurationChanges':'Choose between R042 and R043 before integration. Preserve all current Unity refinements. Bind real sources; retain at most three visible rows and include hidden rows in current totals.',
    'exportRequirements':{'designResolution':[3840,2160],'width':680,'heights':[348,420,492,536],
                          'newRasterImages':[],'slices':True,'sliceBorderPixels':design['sliceBorderPixels'],
                          'displayCapPixels':design['displayCapPixels'],'dynamicFieldsSeparate':True},
    'approvalEvidence':'User authorized creation of this Penpot alternative; not approval for Unity import.',
    'exportEvidence':{'compact':'Revisions/MFJ-R043/CompactReview.png','overflow':'Revisions/MFJ-R043/OverflowReview.png',
                      'sliceGuide':'Revisions/MFJ-R043/NineSliceGuide.png','geometry':'Revisions/MFJ-R043/PenpotDesign.json'},
    'importEvidence':None,'verificationEvidence':{'report':'Revisions/MFJ-R043/Verification.json'},
    'remainingWork':['User review and variant choice','Unity integration only when requested',
                     'Real runtime provenance and ScrollRect; pointer travel, wheel routing, focus, viewport placement, hidden information and variable source counts'],
    'unityImported':False,'integrationStatus':'not_imported','synchronizationCheckpoint':None,
}
assert all(k in entry for k in ledger['requiredEntryFields'])
ledger['changes'].append(entry)
ledger['nextRevisionId']='MFJ-R044'
write(root / 'DesignChangeLedger.json',ledger)
index['generatedDate']='2026-10-20'
index['pending'].append({'id':'MFJ-R043','status':'ready_for_review','component':['Modified-stat tooltip'],
                         'manifest':'Revisions/MFJ-R043/Assets.json','unityImported':False})
index['alternativeGroups']=[{'component':'Modified-stat tooltip','revisions':['MFJ-R042','MFJ-R043'],
                             'instruction':'Select one before Unity integration; both retained for visual comparison.'}]
write(root / 'PendingUnityChanges.json',index)

summary='''

## MFJ-R043 — Compact modifier tooltip alternative (Penpot only)

**Ready for review; not imported.** On Concept B — Reusable Assembly (4K), select **MFJ-R043 / Compact modifier tooltips / 1–3 sources**. Default/current amounts plus portrait/icon, source name and signed amount only. The 680px-wide nine-region parchment frame grows for one to three reasons; overflow keeps three visible and adds a scrollbar and hidden-source indicator. Separate first/last scroll states and a click-navigation prototype document the intended interaction; live wheel/drag scrolling belongs to future Unity integration. [Previews and modular contract](Revisions/MFJ-R043/README.md).

R042 remains pending as a comparison alternative. **Choose one tooltip design before integrating; do not import both.** Current Unity checkpoint remains MFJ-SYNC-006.16; all 37 completed imports and four superseded drafts are preserved. No Unity files changed.
'''
for name in ['Design Changes.md','Pending Unity Changes.md','VisualContract.md']:
    path=root/name
    text=path.read_text(encoding='utf-8')
    if name=='Design Changes.md':
        start=text.index('## Pending Unity changes')
        end=text.index('## Change history',start)
        text=text[:start]+'## Pending Unity changes\n\n**Two pending design alternatives: MFJ-R042 and MFJ-R043 — modified-stat tooltips, ready for review. Choose one before Unity integration.** 37 imported/verified revisions and four superseded drafts remain preserved. Use [the pending index](PendingUnityChanges.json) and [MFJ-SYNC-006.16](DisplayedStatColours/UnityCheckpoint.json) for selective integration.\n\n'+text[end:]
    elif name=='Pending Unity Changes.md':
        end=text.index('## Explicit creature role choice')
        text='# Penpot changes awaiting Unity integration\n\n**Current: 2 pending design alternatives (MFJ-R042/R043); 37 imported and verified; 4 superseded drafts.** Choose one tooltip version before integration. [Pending index](PendingUnityChanges.json) · [Full ledger](DesignChangeLedger.json).\n\nLatest integration baseline: [MFJ-SYNC-006.16](DisplayedStatColours/UnityCheckpoint.json). Next design revision: **MFJ-R044**. Preserve all Unity-only refinements and import only selected new pending entries.\n\n'+text[end:]
    else:
        text=text.replace('1 pending,37 completed,4 superseded','2 pending alternatives,37 completed,4 superseded',1)
    path.write_text(text.rstrip()+summary,encoding='utf-8')
print('Recorded MFJ-R043: 2 pending alternatives; 37 completed and 4 superseded unchanged. Unity hashes unchanged.')
