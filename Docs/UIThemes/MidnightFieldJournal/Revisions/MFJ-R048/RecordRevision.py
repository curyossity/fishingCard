"""Record this Penpot-only alternative without altering completed integrations."""
import hashlib
import json
from pathlib import Path
import re

folder = Path(__file__).resolve().parent
root = folder.parents[1]
repo = root.parents[2]

def read(path):
    return json.loads(path.read_text(encoding='utf-8'))

def write(path, value):
    temporary = path.with_suffix(path.suffix+'.tmp')
    temporary.write_text(json.dumps(value, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    temporary.replace(path)

state = read(folder/'PenpotDesign.json')
bars = list(state['bars'].values())
complete = len(bars) == 3 and all(b.get('complete') for b in bars)
audit = read(folder/'Action-audit.json') if (folder/'Action-audit.json').exists() else []
review = read(folder/'VisualReview.json') if (folder/'VisualReview.json').exists() else {'passed': False}
exports = ['DesignReview.png','EngravedRuler.png','TackleLabels.png','LanternFocus.png']
before = read(folder/'UnityBefore.json')
modified = [f['path'] for f in before['files'] if not (repo/f['path']).exists() or hashlib.sha256((repo/f['path']).read_bytes()).hexdigest() != f['sha256']]
checks = {
    'threeInitialBarsOnly': complete and all(b['state']=='initial' for b in bars),
    'eightEqualTags': complete and all(len(b['zones'])==8 and all(z['fraction']==.125 for z in b['zones']) for b in bars),
    'sharedPalettePerDesign': complete and len({b['palettePolicy'] for b in bars})==1,
    'nativeGeometry': len(audit)==3 and all(b['markers']==1 and len(b['zones'])==8 and all(abs(z['width']-180)<.01 and abs(z['precisionWidth']-32.4)<.01 and z['centred'] for z in b['zones']) for b in audit),
    'precisionCueCounts': complete and [sum(z['precisionVisible'] for z in state['bars'][str(i)]['zones']) for i in range(3)]==[8,8,1],
    'exportsExist': all((folder/name).exists() for name in exports),
    'visualReviewPassed': review['passed'],
    'unityFilesUnchanged': not modified,
}
ready = all(checks.values())
status = 'ready_for_review' if ready else 'in_progress'
write(folder/'Verification.json',{'revision':'MFJ-R048','date':'2026-10-28','checks':checks,'unityCheckpointUnchanged':before['checkpoint'],'unityFileCount':len(before['files']),'modifiedUnityFiles':modified,'visualReview':review})
entry = {
    'id':'MFJ-R048','date':'2026-10-28',
    'request':'Three additional initial-state tag-bar alternatives with no individual tag colours and quieter precision cues.',
    'component':['Dynamic Fishing Tag Bar alternatives'], 'status':status, 'supersedes':[],
    'penpotSources':state,'affectedPenpotInstances':[state['reviewId']]+[b['id'] for b in bars],
    'changeKinds':['additional_design_alternatives','reduced_visual_density','native_editable_ui'],
    'before':'R047 remains an available coloured comparison study; the existing Unity rarity bar remains active.',
    'after':'Three new initial-only ideas: Engraved Ruler, Tackle Labels, Lantern Focus. One shared palette per design; no per-tag colours or percentage legends.',
    'unityAssetIds':[],'unityPaths':[],'dependentConsumers':[],'configurationChanges':[],
    'exportRequirements':['Review PNGs only. Any future integration needs a chosen design and a separate implementation task.'],
    'approvalEvidence':['User requested three additional independent monochrome initial-state ideas; no gameplay or Unity changes.'],
    'exportEvidence':{'folder':'Revisions/MFJ-R048','files':exports if checks['exportsExist'] else []},
    'importEvidence':{},'verificationEvidence':{'report':'Revisions/MFJ-R048/Verification.json','nativeAudit':'Revisions/MFJ-R048/Action-audit.json'},
    'remainingWork':['Choose a design before implementation.'] if ready else ['Complete native creation, exports and rendered visual checks.'],
    'unityImported':False,'integrationStatus':'not_imported','synchronizationCheckpoint':None,
}
ledger = read(root/'DesignChangeLedger.json')
ledger['changes'] = [c for c in ledger['changes'] if c['id']!='MFJ-R048']+[entry]
ledger['nextRevisionId']='MFJ-R049'
write(root/'DesignChangeLedger.json',ledger)
index = read(root/'PendingUnityChanges.json')
index['generatedDate']='2026-10-28'
index['pending']=[{'id':c['id'],'status':c['status'],'unityImported':False,'source':'Revisions/'+c['id']+'/README.md'} for c in ledger['changes'] if not c['unityImported'] and c['status']!='superseded']
index['counts']={'pending':len(index['pending']),'completed':len(index['completed']),'superseded':len(index['superseded'])}
write(root/'PendingUnityChanges.json',index)
pending = root/'Pending Unity Changes.md'
content = pending.read_text(encoding='utf-8')
content = re.sub(r'\*\*Current:.*?\n','**Current: 2 pending design studies; 39 imported and verified; 7 superseded drafts.** R047 is ready_for_review; R048 is '+status+'. [Pending index](PendingUnityChanges.json) · [Full ledger](DesignChangeLedger.json).\n',content,count=1)
content = content.replace('Next revision: **MFJ-R048**.','Next revision: **MFJ-R049**.')
if '## A Quieter Cast — MFJ-R048' not in content:
    content+='\n## A Quieter Cast — MFJ-R048\n\nThree additional initial-state alternatives: Engraved Ruler, Tackle Labels and Lantern Focus. Each uses a shared palette and quiet precision cues. R047 is retained for comparison; neither study is imported. [Design and evidence](Revisions/MFJ-R048/README.md).\n'
pending.write_text(content,encoding='utf-8')
history = root/'Design Changes.md'
content = history.read_text(encoding='utf-8')
content = re.sub(r'(## Pending Unity changes\s*\n)\*\*.*?\n',r'\1**Two pending design studies: MFJ-R047 and MFJ-R048.** R048 is '+status+'. All 39 previously integrated revisions remain verified; seven drafts are superseded. [Pending index](PendingUnityChanges.json).\n',content,count=1)
row='| MFJ-R048 | Three additional initial-only monochrome tag-bar ideas, with quieter precision cues. R047 retained. | '+status+'; design only. | Not imported; [evidence](Revisions/MFJ-R048/README.md). |\n'
if '| MFJ-R048 |' in content:
    content=re.sub(r'^\| MFJ-R048 \|.*\n',lambda m:row,content,flags=re.MULTILINE)
else:
    match=list(re.finditer(r'^\| MFJ-R047 \|.*\n',content,re.MULTILINE))[-1]
    content=content[:match.end()]+row+content[match.end():]
history.write_text(content,encoding='utf-8')
contract=root/'VisualContract.md'
content=contract.read_text(encoding='utf-8')
content=content.replace('One pending Penpot-only study, MFJ-R047, offers three tag-bar alternatives; it does not replace the live rarity bar.','Two pending Penpot-only studies, MFJ-R047 and MFJ-R048, offer coloured and quieter monochrome tag-bar alternatives; neither replaces the live rarity bar.')
contract.write_text(content,encoding='utf-8')
print(json.dumps({'status':status,'counts':index['counts'],'checks':checks}))
