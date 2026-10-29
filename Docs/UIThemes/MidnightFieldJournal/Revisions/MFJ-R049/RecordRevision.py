"""Record only this design increment; keep prior approvals and Unity hashes intact."""
import hashlib
import json
from pathlib import Path
import re

folder=Path(__file__).resolve().parent
root=folder.parents[1]
repo=root.parents[2]
def read(p): return json.loads(p.read_text(encoding='utf-8'))
def write(p,d):
    t=p.with_suffix(p.suffix+'.tmp')
    t.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    t.replace(p)
state=read(folder/'PenpotDesign.json')
bars=list(state['bars'].values())
audit=read(folder/'Action-audit.json') if (folder/'Action-audit.json').exists() else []
review=read(folder/'VisualReview.json') if (folder/'VisualReview.json').exists() else {'passed':False}
exports=['DesignReview.png','TideDial.png','LineSpool.png','SoundingChart.png']
before=read(folder/'UnityBefore.json')
modified=[f['path'] for f in before['files'] if not (repo/f['path']).exists() or hashlib.sha256((repo/f['path']).read_bytes()).hexdigest()!=f['sha256']]
complete=len(bars)==3 and all(b.get('complete') for b in bars)
checks={'threeInitialConcepts':complete and all(b['state']=='initial' for b in bars),
        'eightEqualLogicalTags':complete and all(len(b['zones'])==8 and all(z['fraction']==.125 for z in b['zones']) for b in bars),
        'nativeObjectsExist':len(audit)==3 and all(b['exists'] and b['markerExists'] and b['zonesExist'] and b['equalPhases'] for b in audit),
        'viewportVisibility':complete and [len(state['bars'][str(i)]['visibleTags']) for i in range(3)]==[8,3,8],
        'exportsExist':all((folder/n).exists() for n in exports),'visualReviewPassed':review['passed'],'unityFilesUnchanged':not modified}
status='ready_for_review' if all(checks.values()) else 'in_progress'
write(folder/'Verification.json',{'revision':'MFJ-R049','date':'2026-10-28','checks':checks,'unityCheckpointUnchanged':before['checkpoint'],'unityFileCount':len(before['files']),'modifiedUnityFiles':modified,'visualReview':review})
entry={'id':'MFJ-R049','date':'2026-10-28','request':'Three more genuinely different themed initial-state tag-timing designs; keep a restrained shared palette.',
       'component':['Fishing tag timing instrument studies'],'status':status,'supersedes':[],
       'penpotSources':state,'affectedPenpotInstances':[state['reviewId']]+[b['id'] for b in bars],
       'changeKinds':['additional_design_alternatives','alternative_instrument_forms','native_editable_ui'],
       'before':'R047 and R048 remain preserved comparisons; the existing Unity rarity timing mechanic is active.',
       'after':'Tide Dial, Line Spool and Sounding Chart: one initial mockup each, separate editable parts, shared palette. Alternate motion cues are visual proposals only.',
       'unityAssetIds':[],'unityPaths':[],'dependentConsumers':[],'configurationChanges':[],
       'exportRequirements':['Penpot review PNGs only; no Unity import in this task.'],
       'approvalEvidence':['User requested three more creative variations, not repetitions of the previous visual solutions.'],
       'exportEvidence':{'folder':'Revisions/MFJ-R049','files':exports if checks['exportsExist'] else []},
       'importEvidence':{},'verificationEvidence':{'report':'Revisions/MFJ-R049/Verification.json'},
       'remainingWork':['Select a visual and motion proposal before any implementation.'] if status=='ready_for_review' else ['Finish native creation and exported visual checks.'],
       'unityImported':False,'integrationStatus':'not_imported','synchronizationCheckpoint':None}
ledger=read(root/'DesignChangeLedger.json')
ledger['changes']=[c for c in ledger['changes'] if c['id']!='MFJ-R049']+[entry]
ledger['nextRevisionId']='MFJ-R050'
write(root/'DesignChangeLedger.json',ledger)
index=read(root/'PendingUnityChanges.json')
index['generatedDate']='2026-10-28'
index['pending']=[{'id':c['id'],'status':c['status'],'unityImported':False,'source':'Revisions/'+c['id']+'/README.md'} for c in ledger['changes'] if not c['unityImported'] and c['status']!='superseded']
index['counts']={'pending':len(index['pending']),'completed':len(index['completed']),'superseded':len(index['superseded'])}
write(root/'PendingUnityChanges.json',index)
p=root/'Pending Unity Changes.md';s=p.read_text(encoding='utf-8')
s=re.sub(r'\*\*Current:.*?\n','**Current: 3 pending design studies; 39 imported and verified; 7 superseded drafts.** R047/R048 are ready_for_review; R049 is '+status+'. [Pending index](PendingUnityChanges.json) · [Full ledger](DesignChangeLedger.json).\n',s,count=1)
s=s.replace('Next revision: **MFJ-R049**.','Next revision: **MFJ-R050**.')
if '## Three Ways to Read the Sea — MFJ-R049' not in s:
    s+='\n## Three Ways to Read the Sea — MFJ-R049\n\nThree initial-only instrument concepts: Tide Dial, Line Spool and Sounding Chart. R047/R048 are retained. Motion/viewport changes are visual proposals only; Unity is unchanged. [Design and evidence](Revisions/MFJ-R049/README.md).\n'
p.write_text(s,encoding='utf-8')
p=root/'Design Changes.md';s=p.read_text(encoding='utf-8')
s=re.sub(r'(## Pending Unity changes\s*\n)\*\*.*?\n',r'\1**Three pending design studies: MFJ-R047, MFJ-R048 and MFJ-R049.** R049 is '+status+'. All 39 previously integrated revisions remain verified; seven drafts are superseded. [Pending index](PendingUnityChanges.json).\n',s,count=1)
row='| MFJ-R049 | Three new initial-state instrument forms: Tide Dial, Line Spool, Sounding Chart. Earlier studies retained. | '+status+'; design only. | Not imported; [evidence](Revisions/MFJ-R049/README.md). |\n'
if '| MFJ-R049 |' in s:s=re.sub(r'^\| MFJ-R049 \|.*\n',lambda m:row,s,flags=re.MULTILINE)
else:
    m=list(re.finditer(r'^\| MFJ-R048 \|.*\n',s,re.MULTILINE))[-1];s=s[:m.end()]+row+s[m.end():]
p.write_text(s,encoding='utf-8')
p=root/'VisualContract.md';s=p.read_text(encoding='utf-8')
s=s.replace('Two pending Penpot-only studies, MFJ-R047 and MFJ-R048, offer coloured and quieter monochrome tag-bar alternatives; neither replaces the live rarity bar.','Three pending Penpot-only studies, MFJ-R047/R048/R049, explore tag-timing visuals and instrument forms; none replaces the live rarity bar.')
p.write_text(s,encoding='utf-8')
print(json.dumps({'status':status,'counts':index['counts'],'checks':checks}))
