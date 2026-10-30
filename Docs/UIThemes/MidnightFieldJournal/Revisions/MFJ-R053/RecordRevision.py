import json, hashlib, struct
from pathlib import Path

folder=Path(__file__).resolve().parent
root=folder.parent.parent
def read(p): return json.loads(p.read_text(encoding='utf-8'))
def write(p,data):
    tmp=p.with_suffix(p.suffix+'.tmp')
    tmp.write_text(json.dumps(data,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
    tmp.replace(p)
design=read(folder/'PenpotDesign.json')
ledger=read(root/'DesignChangeLedger.json')
previous=next(c for c in ledger['changes'] if c['id']=='MFJ-R052')
previous.update(status='superseded',supersededBy='MFJ-R053')
previous['remainingWork']=['Retained for structural comparison only. Visual direction superseded by R053; expanded screens require a later approved redesign before integration.']
entry={
 'id':'MFJ-R053','date':'2026-10-30',
 'request':'Revise only one main Harbor screen to match the rich decorative style of the approved creature cards.',
 'component':['Harbor hub style study'],'status':'ready_for_review','supersedes':['MFJ-R052'],
 'penpotSources':design,'affectedPenpotInstances':[design['boardId']],
 'changeKinds':['visual_direction','ornate_frame','single_screen_review'],
 'before':'R052 Harbor used simpler UI decoration that did not match the approved creature cards.',
 'after':'One ornate Harbor composition with editable labels, layered parchment and copper borders, engraved kelp and textured navy destination panels.',
 'unityAssetIds':[],'unityPaths':[],
 'dependentConsumers':['Future Harbor presentation; no runtime consumer changed'],
 'configurationChanges':[],
 'exportRequirements':['Approval study only. Recreate independent production-resolution decorative assets after visual approval; do not slice the composite image.'],
 'approvalEvidence':['User requested a single revised Harbor screen before expanding the design. Style approval remains pending.'],
 'exportEvidence':{'folder':'Revisions/MFJ-R053','files':['Harbor.jpg','Harbor4K.jpg'],'sourceRasterDimensions':[1672,941],'boardDimensions':[3840,2160]},
 'importEvidence':{},'verificationEvidence':{'report':'Revisions/MFJ-R053/Verification.json'},
 'remainingWork':['User visual review. Rebuild separable production assets only after the visual direction is approved.'],
 'unityImported':False,'integrationStatus':'not_imported','synchronizationCheckpoint':None
}
ledger['changes']=[c for c in ledger['changes'] if c['id']!='MFJ-R053']+[entry]
ledger['nextRevisionId']='MFJ-R054'
write(root/'DesignChangeLedger.json',ledger)
pending=read(root/'PendingUnityChanges.json')
pending['pending']=[{'id':'MFJ-R053','status':'ready_for_review','unityImported':False,'source':'Revisions/MFJ-R053/README.md'}]
if not any(c['id']=='MFJ-R052' for c in pending['superseded']): pending['superseded'].append({'id':'MFJ-R052','supersededBy':'MFJ-R053','unityImported':False})
write(root/'PendingUnityChanges.json',pending)
p=root/'Design Changes.md'
s=p.read_text(encoding='utf-8')
start=s.index('**One pending design: MFJ-R052')
end=s.index('\n',start)
s=s[:start]+'**One pending design: MFJ-R053 — ornate main Harbor style study, ready for review.** R052 is superseded for visual direction and retained for structural comparison; its expanded screens are not approved for import. All 40 completed revisions remain unchanged, with 12 superseded studies. Unity checkpoint remains MFJ-SYNC-009. [New preview and scope](Revisions/MFJ-R053/README.md) · [Pending index](PendingUnityChanges.json).'+s[end:]
s+='\n\n## MFJ-R053 — Ornate Harbor revision\n\nOne main screen only. Rich creature-card framing and editable labels; composite raster decoration is for review, not production slicing. Pending visual approval; no Unity import. [Review](Revisions/MFJ-R053/README.md).\n'
p.write_text(s,encoding='utf-8')
p=root/'VisualContract.md';s=p.read_text(encoding='utf-8')
start=s.index('**MFJ-SYNC-009 is the latest Unity checkpoint.**');end=s.index('\n',start)
s=s[:start]+'**MFJ-SYNC-009 is the latest Unity checkpoint.** 40 revisions remain imported and verified; 12 studies are superseded. [MFJ-R053 ornate Harbor](Revisions/MFJ-R053/README.md) is the sole pending visual review. R052 is retained as historical structure only; none of its screens should be imported automatically. R053 is one composite style study with native editable text, not a separated production asset set. [Current checkpoint](Sync009/UnityCheckpoint.json).'+s[end:]
p.write_text(s,encoding='utf-8')
p=folder.parent/'MFJ-R052/README.md';s=p.read_text(encoding='utf-8')
notice='> Visual direction superseded by [MFJ-R053](../MFJ-R053/README.md). Retained for structural comparison; these screens are not approved for automatic Unity import.\n\n'
if not s.startswith('> Visual direction superseded'):p.write_text(notice+s,encoding='utf-8')
write(folder/'Verification.json',{'revision':'MFJ-R053','boardSize':[3840,2160],'screenCount':1,'editableTextFields':len(design['editableTextFields']),'visualReview':'Inspected Penpot export; text fits panels. Removed lower subtitle that collided with decoration.','unityImported':False,'unityFilesModifiedByThisRevision':False,'previousDesignRetained':True,'productionAssetSeparation':'Not performed; visual approval study only.'})
print(json.dumps({'pending':['MFJ-R053'],'completed':len(pending['completed']),'superseded':len(pending['superseded']),'checkpoint':pending['currentUnityCheckpoint']}))
