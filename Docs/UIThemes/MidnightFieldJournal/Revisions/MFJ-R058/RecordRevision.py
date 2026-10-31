"""Record the new reviewed Penpot design as pending, retaining every completed Unity checkpoint."""
import hashlib
import json
import struct
from pathlib import Path

folder=Path(__file__).resolve().parent
theme=folder.parent.parent
project=folder.parents[4]
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()

def write(path,data):
    path.write_text(json.dumps(data,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')

design=read(folder/'PenpotDesign.json')
live=read(folder/'PenpotVerification.json')
assert (live['width'],live['height'])==(3840,2160)
assert not live['outside'] and not live['overflow']
assert all(f['scaleX']==1 and f['scaleY']==1 and f['cornerScale']<=.44 for f in live['frames'])
assert [r['progressSegments'] for r in live['rows']]==[0,0,3]
png=folder/'Journal4K.png'
assert struct.unpack('>II',png.read_bytes()[16:24])==(3840,2160)
before={x['path']:x['sha256'] for x in read(folder/'UnityBefore.json')['files']}
current={p.relative_to(project).as_posix():sha(p) for p in (project/'Assets').rglob('*') if p.is_file()}
differences=sorted(p for p in before.keys()|current.keys() if before.get(p)!=current.get(p))
assert not differences,differences
verification={'revision':'MFJ-R058','status':'ready_for_review','unityImported':False,'nativeCanvas':[3840,2160],'frameInstances':len(live['frames']),'separateFrameResources':10,'allFrameInstancesOneToOnePixels':True,'maximumDecorativeCornerScale':.44,'textOverflow':0,'outsideScreen':0,'singleExpeditionPartialTrackers':False,'riskTakerSegments':3,'exampleStatesNotPlayerData':True,'rewardsGranted':False,'visualReview':'Inspected lossless actual Penpot export; moved statuses clear of engraved corners and raised sidebar caption. Matches current ornate shop framing and typography.','capture':{'file':'Journal4K.png','dimensions':[3840,2160],'sha256':sha(png)},'illustration':{'file':'Art/JournalDesk.png','source':[1086,1448],'display':[810,1080],'sha256':sha(folder/'Art/JournalDesk.png')},'unityAssetComparison':{'checkedFiles':len(current),'differences':differences,'unchanged':True}}
write(folder/'Verification.json',verification)
ledger=read(theme/'DesignChangeLedger.json')
template=next(x for x in ledger['changes'] if x['id']=='MFJ-R056')
entry={**template,'id':'MFJ-R058','date':'2026-10-30','status':'ready_for_review','request':'Design the Discovery Journal in Penpot to match the ornate Harbor/shop/card style, with sharp 4K frames and approved quest-status presentation.','component':['Discovery Journal screen','Reusable ornate quest rows','Journal illustration','Hub prototype Journal links'],'supersedes':[],'penpotSources':design,'affectedPenpotInstances':[design['boardId'],design['sourceBoardId'],'75555645-d407-8067-8008-de8ff58e2639','75555645-d407-8067-8008-de9b9c9710d1','75555645-d407-8067-8008-de9d62501389'],'changeKinds':['new_journal_screen','independent_frame_assets','native_editable_quest_fields','prototype_navigation'],'before':'The functional Unity Journal uses the previous native paper/ink layout; no matching ornate Journal screen is approved.','after':'Editable 4K Journal with fourteen separate sharp frame instances, new downsampled illustration, three approved quests, status-only single-expedition objectives and a cumulative expedition tracker.','unityAssetIds':[],'unityPaths':['Assets/UIThemes/MidnightFieldJournal/Harbor/JournalDiscoveryPage.cs','Assets/UIThemes/MidnightFieldJournal/Harbor/Editor/JournalDiscoveryPageBuilder.cs','Assets/UIThemes/MidnightFieldJournal/Harbor/Prefabs/JournalHarbor.prefab'],'dependentConsumers':['JournalDiscoveryPage','JournalDiscoveryPageBuilder','JournalHarborFlow'],'configurationChanges':['Preserve existing discovery tracking, persistence and no-reward behavior. Replace presentation only after explicit integration.','Gold and demonstrated quest states are mockup values; bind actual runtime values in Unity.'],'exportRequirements':['Use separate Frames PNGs/HTML geometry and existing R057 nine-slice source masters, not a flattened whole-screen UI.','Preserve 1:1 frame image dimensions including transparent short-frame padding; do not enlarge decorative corners.','Keep illustration, native quest/reward/status fields and progress segments separate.'],'approvalEvidence':['User requested a Penpot Journal design matching the other screens, explicitly requiring non-blurry frames.'],'exportEvidence':{'capture':'Revisions/MFJ-R058/Journal4K.png','sources':'Revisions/MFJ-R058/FrameGeometry.json'},'importEvidence':{},'verificationEvidence':{'report':'Revisions/MFJ-R058/Verification.json'},'remainingWork':['Visual approval and selective Unity integration.'],'unityImported':False,'integrationStatus':'not_imported','synchronizationCheckpoint':None}
ledger['changes']=[x for x in ledger['changes'] if x['id']!='MFJ-R058']+[entry]
ledger['nextRevisionId']='MFJ-R059'
write(theme/'DesignChangeLedger.json',ledger)
pending=read(theme/'PendingUnityChanges.json')
pending['pending']=[x for x in pending['pending'] if x['id']!='MFJ-R058']+[{'id':'MFJ-R058','status':'ready_for_review','unityImported':False,'source':'Revisions/MFJ-R058/README.md'}]
pending['counts']={key:len(pending[key]) for key in ['pending','completed','superseded']}
write(theme/'PendingUnityChanges.json',pending)
notice='**MFJ-R058 is ready for review in Penpot:** the ornate 4K Discovery Journal, with separate sharp frames and native quest fields. **1 pending import; 44 completed; 13 superseded.** Unity remains at its existing checkpoint; no game assets changed. [Design and handoff](Revisions/MFJ-R058/README.md).\n\n'
for name in ['Design Changes.md','VisualContract.md']:
    path=theme/name;text=path.read_text(encoding='utf-8')
    if notice not in text:
        heading,rest=text.split('\n',1);path.write_text(heading+'\n\n'+notice+rest.lstrip('\n'),encoding='utf-8')
print(json.dumps({'status':'ready_for_review','pending':pending['counts'],'unityAssetDifferences':differences}))
