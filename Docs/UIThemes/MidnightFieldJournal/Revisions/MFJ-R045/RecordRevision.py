"""Record the replacement tooltip contract and preserve imported Unity checkpoints."""
import copy
import hashlib
import json
from pathlib import Path

root=Path('Docs/UIThemes/MidnightFieldJournal'); folder=root/'Revisions/MFJ-R045'
def read(p): return json.loads(p.read_text(encoding='utf-8'))
def write(p,x): p.write_text(json.dumps(x,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
digest=lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
design=read(folder/'PenpotDesign.json'); ledger=read(root/'DesignChangeLedger.json'); index=read(root/'PendingUnityChanges.json')
assert ledger['nextRevisionId']=='MFJ-R045'
for r in design['records']:
    assert r['current']==r['base']+sum(x['amount'] for x in r['rows'])
    assert r['visibleRows']==min(3,len(r['rows']))
    assert bool(r.get('upArrowId'))==(r['offset']>0)
    assert bool(r.get('downArrowId'))==(r['offset']+r['visibleRows']<len(r['rows']))
for name in ['CompactReview.png','OverflowReview.png','TooltipFrameMaster.png']:
    assert (folder/name).read_bytes().startswith(b'\x89PNG\r\n\x1a\n')
checkpoint=read(root/ledger['currentUnityCheckpoint'])
assert all(digest(Path(f['path']))==f['sha256'] for f in checkpoint['files'])
old=next(c for c in ledger['changes'] if c['id']=='MFJ-R044'); entry=copy.deepcopy(old)
entry.update(id='MFJ-R045',supersedes=['MFJ-R044'],
    request='Use directional arrows above/below reasons, background-free flat icons with shared Technique/event glyphs, and a new decoration suitable for nine-slice resizing.',
    penpotSources=design,alternativeGroup='Modified-stat tooltip: choose R042 or R045 before integration',
    affectedPenpotInstances=[design['reviewBoardId'],design['scrollReviewBoardId'],design['masterBoardId'],*design['prototypeIds']],
    changeKinds=['arrow-only overflow states','flat creature silhouettes','shared category glyphs','new native engraved corner frame'],
    before='R044 uses numbered above/below labels, portrait backings, distinct event symbols and a rescaled illustrated frame.',
    after='Arrow up above reasons and down below; both only in the middle. Single-ink background-free icons: creature silhouettes, tied hook for Techniques and wave scroll for all events. New 64px fixed corner engraving with straight stretchable edges.',
    unityAssetIds=['ModifierTooltipFrameV2','ModifierSourceIconsV2'],
    configurationChanges='Replace R044 with R045. New master uses 64px borders, not the old 320/340px borders. No icon backings, scroll labels/counts or scrollbar. Bind arrow visibility to scroll bounds; preserve three visible source rows and truthful attribution.',
    exportRequirements={'designResolution':[3840,2160],'width':680,'heights':[348,420,492,536],
        'masterCanvas':[680,536],'slices':True,'sliceBorderPixels':design['sliceBorderPixels'],
        'dynamicFieldsSeparate':True,'visibleScrollbar':False,'icons':'native single-colour SVG; no backing',
        'arrowIndicators':'up above list; down below list; visibility follows actual scroll range'},
    exportEvidence={'compact':'Revisions/MFJ-R045/CompactReview.png','overflow':'Revisions/MFJ-R045/OverflowReview.png',
        'master':'Revisions/MFJ-R045/TooltipFrameMaster.png','geometry':'Revisions/MFJ-R045/PenpotDesign.json'},
    verificationEvidence={'report':'Revisions/MFJ-R045/Verification.json'})
entry['unityPaths'][0]='Assets/UIThemes/MidnightFieldJournal/Art/Tooltips/ModifierTooltipFrameV2.png (proposed, not created)'
old.update(status='superseded',supersededBy='MFJ-R045')
ledger['changes'].append(entry);ledger['nextRevisionId']='MFJ-R046'
write(folder/'Assets.json',{'revision':'MFJ-R045','status':'ready_for_review','unityImported':False,
    'master':{'path':(folder/'TooltipFrameMaster.png').as_posix(),'sha256':digest(folder/'TooltipFrameMaster.png'),
        'width':680,'height':536,'penpotShapeId':design['masterBoardId'],'borders':design['sliceBorderPixels']},
    'creation':'Native vector engraving and icons; reused clean parchment center only. No bitmap generation, no screenshot slicing.',
    'iconSources':'CreateDesign.js glyphs; original paths for each species and common category glyphs',
    'fonts':['Cinzel','Source Serif 4'],'maximumVisibleReasons':3,'scrollbar':False,'dynamicFieldsSeparate':True})
write(folder/'Verification.json',{'status':'design_verified','visualInspection':['CompactReview.png','OverflowReview.png','TooltipFrameMaster.png'],
    'scrollStates':['first: down only','middle: up and down','last: up only'],'arrowLabelsOrCounts':False,
    'exampleArithmeticCorrect':True,'iconBackings':False,'iconsSingleColour':True,
    'sharedTechniqueIcon':'tied hook','sharedEventIcon':'wave scroll','nativeCornerRegions':[64,64],
    'borderOrnamentCrossesSliceBoundaries':False,'prototypePlayedInViewer':False,'liveWheelScrollingImplemented':False,
    'unityFilesVerifiedUnchanged':len(checkpoint['files']),'unityCheckpointPreserved':ledger['currentUnityCheckpoint'],'unityImported':False})
index['pending']=[p for p in index['pending'] if p['id']!='MFJ-R044']
index['pending'].append({'id':'MFJ-R045','status':'ready_for_review','component':['Modified-stat tooltip'],'manifest':'Revisions/MFJ-R045/Assets.json','unityImported':False})
index['superseded'].append({'id':'MFJ-R044','supersededBy':'MFJ-R045','unityImported':False})
index['alternativeGroups']=[{'component':'Modified-stat tooltip','revisions':['MFJ-R042','MFJ-R045'],'instruction':'Choose one before Unity integration.'}]
write(root/'DesignChangeLedger.json',ledger);write(root/'PendingUnityChanges.json',index)
summary='''

## MFJ-R045 — Arrow cues, flat icons and new nine-slice engraving (Penpot only)

**Ready for review; not imported.** New native 64px shell-and-knot corners and straight double-rule edges resize independently. Background-free single-ink creature silhouettes; all Techniques use a tied hook and all events a wave scroll. Up arrow appears above reasons, down below, both in the middle; no counts, labels or scrollbar. Three reasons remain visible. [Previews and handoff](Revisions/MFJ-R045/README.md).

R045 supersedes R044; R042 remains a comparison alternative. Two pending alternatives, 37 completed imports, six superseded drafts. Choose R042 or R045 before Unity integration. MFJ-SYNC-006.16 and Unity files are unchanged.
'''
for name in ['Design Changes.md','Pending Unity Changes.md','VisualContract.md']:
    p=root/name;t=p.read_text(encoding='utf-8')
    if name=='Design Changes.md':
        a=t.index('## Pending Unity changes');b=t.index('## Change history',a)
        t=t[:a]+'## Pending Unity changes\n\n**Two pending alternatives: MFJ-R042 and MFJ-R045.** R045 replaces R044. 37 imported/verified revisions and six superseded drafts. [Pending index](PendingUnityChanges.json) · [Current checkpoint](DisplayedStatColours/UnityCheckpoint.json).\n\n'+t[b:]
    elif name=='Pending Unity Changes.md':
        a=t.index('## Explicit creature role choice')
        t='# Penpot changes awaiting Unity integration\n\n**Current: 2 pending alternatives (MFJ-R042/R045); 37 imported and verified; 6 superseded drafts.** Choose one tooltip version before integration. [Pending index](PendingUnityChanges.json) · [Full ledger](DesignChangeLedger.json).\n\nLatest baseline: [MFJ-SYNC-006.16](DisplayedStatColours/UnityCheckpoint.json). Next revision: **MFJ-R046**. Preserve all Unity-only refinements.\n\n'+t[a:]
    else:t=t.replace('2 pending alternatives,37 completed,5 superseded','2 pending alternatives,37 completed,6 superseded',1)
    p.write_text(t.rstrip()+summary,encoding='utf-8')
print('R045 recorded; R044 superseded. Unity checkpoint unchanged.')
