import copy
import hashlib
import json
from pathlib import Path

root=Path('Docs/UIThemes/MidnightFieldJournal')
folder=root/'Revisions/MFJ-R044'
read=lambda p: json.loads(p.read_text(encoding='utf-8'))
def write(p,data): p.write_text(json.dumps(data,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
ledger=read(root/'DesignChangeLedger.json'); index=read(root/'PendingUnityChanges.json')
design=read(folder/'PenpotDesign.json')
assert ledger['nextRevisionId']=='MFJ-R044'
assert len(design['changes']['removedScrollbars'])==8
assert len(design['changes']['portraits'])==7
assert all(max(p['width'],p['height'])<=48.001 for p in design['changes']['portraits'])
for name in ['CompactReview.png','OverflowReview.png']:
    assert (folder/name).read_bytes().startswith(b'\x89PNG\r\n\x1a\n')
checkpoint=read(root/ledger['currentUnityCheckpoint'])
assert all(hashlib.sha256(Path(f['path']).read_bytes()).hexdigest()==f['sha256'] for f in checkpoint['files'])
previous=next(c for c in ledger['changes'] if c['id']=='MFJ-R043')
entry=copy.deepcopy(previous)
entry.update(id='MFJ-R044',request='Remove the overflow slider and fit complete creature portraits within source icon tiles.',
    supersedes=['MFJ-R043'],penpotSources=design,
    alternativeGroup='Modified-stat tooltip: choose R042 or R044 before Unity integration',
    changeKinds=['remove scrollbar visuals','contain-fit source portraits'],
    before='R043 has visible scrollbars and height-fitted creature portraits that clip horizontally.',
    after='No scrollbar; overflow indicator and scrolling retained. Whole creature portraits fit proportionally with padding.',
    configurationChanges='Use R044 in place of R043. No visible scrollbar at any source count; retain scrolling and overflow indicators. Creature thumbnails use centered aspect-preserving contain fit with padding.',
    exportEvidence={'compact':'Revisions/MFJ-R044/CompactReview.png','overflow':'Revisions/MFJ-R044/OverflowReview.png','geometry':'Revisions/MFJ-R044/PenpotDesign.json'},
    verificationEvidence={'report':'Revisions/MFJ-R044/Verification.json'})
entry['exportRequirements']['visibleScrollbar']=False
entry['exportRequirements']['portraitFit']='contain'
entry['exportRequirements']['portraitPadding']=4
previous.update(status='superseded',supersededBy='MFJ-R044')
ledger['changes'].append(entry); ledger['nextRevisionId']='MFJ-R045'
assets=read(root/'Revisions/MFJ-R043/Assets.json')
assets.update(revision='MFJ-R044',alternativeGroup='Choose R042 or R044',visibleScrollbar=False,portraitFit='contain',portraitPadding=4)
write(folder/'Assets.json',assets)
write(folder/'Verification.json',{'status':'design_verified','visualInspection':['CompactReview.png','OverflowReview.png'],
    'removedScrollbarShapes':8,'containFittedPortraits':7,'maximumVisibleReasons':3,
    'overflowIndicatorRetained':True,'liveScrollingImplemented':False,
    'unityFilesVerifiedUnchanged':len(checkpoint['files']),'unityImported':False,'unityCheckpointPreserved':ledger['currentUnityCheckpoint']})
index['pending']=[p for p in index['pending'] if p['id']!='MFJ-R043']
index['pending'].append({'id':'MFJ-R044','status':'ready_for_review','component':['Modified-stat tooltip'],'manifest':'Revisions/MFJ-R044/Assets.json','unityImported':False})
index['superseded'].append({'id':'MFJ-R043','supersededBy':'MFJ-R044','unityImported':False})
index['alternativeGroups']=[{'component':'Modified-stat tooltip','revisions':['MFJ-R042','MFJ-R044'],'instruction':'Choose one before Unity integration.'}]
write(root/'DesignChangeLedger.json',ledger);write(root/'PendingUnityChanges.json',index)
summary='''

## MFJ-R044 — Tooltip portraits and overflow correction (Penpot only)

**Ready for review; not imported.** Refines the existing R043 boards in place: scrollbar tracks/thumbs removed, three-row overflow and “more” indicators retained. Creature portraits now fit completely inside their icon tiles with padding and preserved aspect ratio. [Preview and handoff](Revisions/MFJ-R044/README.md).

R044 supersedes pending R043; R042 remains available for comparison. Choose R042 or R044 before integration. Two pending alternatives, 37 completed imports, five superseded drafts. MFJ-SYNC-006.16 and Unity files are unchanged.
'''
for name in ['Design Changes.md','Pending Unity Changes.md','VisualContract.md']:
    path=root/name; text=path.read_text(encoding='utf-8')
    if name=='Design Changes.md':
        a=text.index('## Pending Unity changes'); b=text.index('## Change history',a)
        text=text[:a]+'## Pending Unity changes\n\n**Two pending alternatives: MFJ-R042 and MFJ-R044. Choose one before Unity integration.** R044 supersedes R043. 37 imported/verified revisions and five superseded drafts. [Pending index](PendingUnityChanges.json) · [Current checkpoint](DisplayedStatColours/UnityCheckpoint.json).\n\n'+text[b:]
    elif name=='Pending Unity Changes.md':
        a=text.index('## Explicit creature role choice')
        text='# Penpot changes awaiting Unity integration\n\n**Current: 2 pending alternatives (MFJ-R042/R044); 37 imported and verified; 5 superseded drafts.** R044 replaces R043. Choose one tooltip version before integration. [Pending index](PendingUnityChanges.json) · [Full ledger](DesignChangeLedger.json).\n\nLatest baseline: [MFJ-SYNC-006.16](DisplayedStatColours/UnityCheckpoint.json). Next revision: **MFJ-R045**. Preserve all Unity-only refinements.\n\n'+text[a:]
    else: text=text.replace('2 pending alternatives,37 completed,4 superseded','2 pending alternatives,37 completed,5 superseded',1)
    path.write_text(text.rstrip()+summary,encoding='utf-8')
print('Recorded R044, superseded R043, retained R042. Unity hashes unchanged.')
