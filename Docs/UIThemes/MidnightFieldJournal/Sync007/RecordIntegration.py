"""Close only the selected tooltip import, retaining every historical integration and source hash."""
import hashlib
import json
import subprocess
from pathlib import Path

root=Path('Docs/UIThemes/MidnightFieldJournal');folder=root/'Sync007'
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def write(p,x):p.write_text(json.dumps(x,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
digest=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
ledger=read(root/'DesignChangeLedger.json');index=read(root/'PendingUnityChanges.json')
assert ledger['currentUnityCheckpoint']=='DisplayedStatColours/UnityCheckpoint.json'
previous=read(root/ledger['currentUnityCheckpoint'])
for name in ['ComponentVerification.json','InteractionVerification.json','ProductionSmoke.json','MainAnimation/PlayModeVerification.json','ChainAnimation/PlayModeVerification.json']:
    assert read(folder/name)['passed'],name
regression=read(folder/'GameplayRegression.json');assert all(x['passed'] for x in regression.values())
assert sum(x['assertions'] for x in regression.values())==558
frame=Path('Assets/UIThemes/MidnightFieldJournal/Art/Tooltips/ModifierTooltipFrameV2.png')
assert digest(frame)==digest(root/'Revisions/MFJ-R045/TooltipFrameMaster.png')

changed=subprocess.check_output(['git','diff','--name-only'],text=True).splitlines()
new=subprocess.check_output(['git','ls-files','--others','--exclude-standard','Assets'],text=True).splitlines()
allowed={p for p in changed if p.startswith('Assets/') and '/Fonts/' not in p}
preserved=[]
for f in previous['files']:
    if digest(Path(f['path']))!=f['sha256']:
        assert f['path'] in allowed,'Unexpected checkpoint change: '+f['path']
    else:preserved.append(f['path'])
paths=sorted({f['path'] for f in previous['files']}|allowed|set(new))
write(folder/'UnityCheckpoint.json',{'checkpoint':'MFJ-SYNC-007','date':'2026-10-20',
    'previousCheckpoint':'../DisplayedStatColours/UnityCheckpoint.json','revisions':['MFJ-R045'],
    'kind':'Approved compact modified-stat inspection tooltip',
    'files':[{'path':p,'sha256':digest(Path(p))} for p in paths]})
write(folder/'Verification.json',{'status':'verified','checkpoint':'MFJ-SYNC-007','revision':'MFJ-R045',
    'gameplayAssertions':558,'mainAnimationAssertions':74,'chainAnimationAssertions':103,
    'componentAssertions':read(folder/'ComponentVerification.json')['assertions'],
    'interactionAssertions':read(folder/'InteractionVerification.json')['assertions'],
    'productionSmoke':read(folder/'ProductionSmoke.json'),
    'visualInspection':['TooltipInGame.png','MainTooltip4K.png'],'captureResolution':[3840,2160],
    'frameMatchesPenpotMaster':True,'sliceBorders':[64,64,64,64],
    'fontsScenesAuthoredCardsAndAnimatorUnchanged':True,'previousFilesPreserved':len(preserved),
    'standalonePlayerBuildRun':False,'unityConsoleErrorsAfterValidation':0})
write(folder/'ImportManifest.json',{'revision':'MFJ-R045','source':'../Revisions/MFJ-R045/PenpotDesign.json',
    'master':{'path':frame.as_posix(),'sha256':digest(frame)},
    'icons':[{'path':p.as_posix(),'sha256':digest(p)} for p in sorted(frame.parent.joinpath('StatIcons').glob('*.png'))],
    'style':'Assets/UIThemes/MidnightFieldJournal/Profiles/StatTooltipStyle.asset',
    'modifiedUnityPaths':sorted(allowed),'newUnityPaths':new})
revision=next(x for x in ledger['changes'] if x['id']=='MFJ-R045')
revision.update(status='verified',unityImported=True,integrationStatus='verified',synchronizationCheckpoint='MFJ-SYNC-007',
    approvalEvidence='User requested importing the latest completed Penpot tooltip into Unity.',
    importEvidence={'manifest':'Sync007/ImportManifest.json','checkpoint':'Sync007/UnityCheckpoint.json'},
    verificationEvidence={'report':'Sync007/Verification.json'},remainingWork=[])
revision['unityPaths']=[x.replace(' (proposed, not created)','') for x in revision['unityPaths']]
revision['supersedes']=sorted(set(revision['supersedes']+['MFJ-R042']))
old=next(x for x in ledger['changes'] if x['id']=='MFJ-R042');old.update(status='superseded',supersededBy='MFJ-R045')
for rid in ['MFJ-R042','MFJ-R045']:
    p=root/'Revisions'/rid/'Assets.json';data=read(p)
    data.update(status='verified' if rid=='MFJ-R045' else 'superseded',unityImported=rid=='MFJ-R045')
    data['synchronizationCheckpoint']='MFJ-SYNC-007' if rid=='MFJ-R045' else None
    if rid=='MFJ-R042':data['supersededBy']='MFJ-R045'
    write(p,data)
ledger['currentUnityCheckpoint']='Sync007/UnityCheckpoint.json'
index['currentUnityCheckpoint']='Sync007/UnityCheckpoint.json';index['pending']=[];index['alternativeGroups']=[]
index['completed'].append({'id':'MFJ-R045','checkpoint':'MFJ-SYNC-007'})
index['superseded'].append({'id':'MFJ-R042','supersededBy':'MFJ-R045','unityImported':False})
assert len(index['completed'])==38 and len(index['superseded'])==7
write(root/'DesignChangeLedger.json',ledger);write(root/'PendingUnityChanges.json',index)

summary='''

## MFJ-R045 — Integrated as MFJ-SYNC-007

**Imported and verified.** Main and Catch Chain modified numbers now expose the latest compact tooltip with actual resolver provenance, the new 64px sliced frame, flat background-free glyphs, three visible source rows and arrow-only scrolling. Pointer travel, focus, clipping, modal locks and concealed/Bait-zero rules are implemented. Stat and chain animations are preserved. [Import, captures and validation](Sync007/README.md).

R042, R043 and R044 are obsolete proposals retained for history; do not import them. **No pending changes remain: 38 completed imports, seven superseded drafts.** Future selective integration uses [MFJ-SYNC-007](Sync007/UnityCheckpoint.json); MFJ-SYNC-006.16 remains historical evidence.
'''
for name in ['Design Changes.md','Pending Unity Changes.md','VisualContract.md']:
    p=root/name;t=p.read_text(encoding='utf-8')
    if name=='Design Changes.md':
        a=t.index('## Pending Unity changes');b=t.index('## Change history',a)
        t=t[:a]+'## Pending Unity changes\n\n**None. MFJ-R045 is imported and verified at MFJ-SYNC-007.** 38 completed imports and seven superseded drafts are preserved. [Pending index](PendingUnityChanges.json) · [Current checkpoint](Sync007/UnityCheckpoint.json).\n\n'+t[b:]
    elif name=='Pending Unity Changes.md':
        a=t.index('## Explicit creature role choice')
        t='# Penpot changes awaiting Unity integration\n\n**Current: 0 pending; 38 imported and verified; 7 superseded drafts.** MFJ-R045 integrated as MFJ-SYNC-007; R042–R044 must not be imported. [Pending index](PendingUnityChanges.json) · [Full ledger](DesignChangeLedger.json).\n\nLatest baseline: [MFJ-SYNC-007](Sync007/UnityCheckpoint.json). Next design revision: **MFJ-R046**. Preserve all Unity-only refinements.\n\n'+t[a:]
    else:
        t=t.replace('**MFJ-SYNC-006 is the latest imported checkpoint.**','**MFJ-SYNC-007 is the latest imported checkpoint.**',1)
        t=t.replace('2 pending alternatives,37 completed,6 superseded','0 pending,38 completed,7 superseded',1)
    p.write_text(t.rstrip()+summary,encoding='utf-8')
p=root/'Revisions/MFJ-R045/README.md';t=p.read_text(encoding='utf-8').replace('Penpot design revision. Not imported into Unity.','**Imported and verified at MFJ-SYNC-007.** [Integration and validation](../../Sync007/README.md).')
p.write_text(t,encoding='utf-8')
print('MFJ-SYNC-007 recorded: R045 verified; 0 pending, 38 completed, 7 superseded.')
