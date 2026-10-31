"""Complete R058 only after verification and preserve every previous design integration."""
import hashlib,json,os,subprocess
from pathlib import Path

folder=Path(__file__).resolve().parent
project=folder.parents[3]
theme=folder.parent
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()

def write(path,data):
    path.write_text(json.dumps(data,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')

before={x['path']:x['sha256'] for x in read(folder/'Before.json')['files']}
font='Assets/UIThemes/MidnightFieldJournal/Fonts/TMP/Display Fallback.asset'
if sha(project/font)!=before[font]:
    raw=subprocess.check_output(['git','show','HEAD:'+font],cwd=project)
    candidates=[raw,raw.replace(b'\r\n',b'\n').replace(b'\n',b'\r\n')]
    original=next((b for b in candidates if hashlib.sha256(b).hexdigest()==before[font]),None)
    assert original is not None,'Unrecognized font change; left untouched.'
    (project/font).write_bytes(original)
reports={name:read(folder/name) for name in ['ComponentVerification.json','StateVerification.json','LiveProgressVerification.json','PlayVerification.json']}
assert all(x['passed'] for x in reports.values())
current={p.relative_to(project).as_posix():sha(p) for p in (project/'Assets').rglob('*') if p.is_file()}
build='ProjectSettings/EditorBuildSettings.asset';current[build]=sha(project/build)
changed=sorted(p for p in before if before[p]!=current.get(p))
allowed={
 'Assets/Editor/DiscoveryJournalChecks.cs',
 'Assets/UIThemes/MidnightFieldJournal/Harbor/Editor/JournalDiscoveryPageBuilder.cs',
 'Assets/UIThemes/MidnightFieldJournal/Harbor/Editor/JournalHarborBuilder.cs',
 'Assets/UIThemes/MidnightFieldJournal/Harbor/Editor/JournalHarborChecks.cs',
 'Assets/UIThemes/MidnightFieldJournal/Harbor/JournalDiscoveryPage.cs',
 'Assets/UIThemes/MidnightFieldJournal/Harbor/Prefabs/JournalHarbor.prefab'
}
assert set(changed)<=allowed,changed
added=sorted(set(current)-set(before))
assert all(p.startswith('Assets/UIThemes/MidnightFieldJournal/Harbor/Journal/') or p in ['Assets/UIThemes/MidnightFieldJournal/Harbor/Journal.meta','Assets/UIThemes/MidnightFieldJournal/Harbor/JournalHarborEmblem.cs','Assets/UIThemes/MidnightFieldJournal/Harbor/JournalHarborEmblem.cs.meta'] for p in added),added
for source in read(folder/'ImportedSources.json'):
    assert sha(project/source['path'])==source['sha256']
write(folder/'Verification.json',{'passed':True,'date':'2026-10-30','reports':reports,'changedExistingAssets':changed,'addedAssets':added,'protectedExistingFilesUnchanged':len(before)-len(changed),'questModelCatalogSaveSchemaAndGameplayUnchanged':True,'originalSceneAndBuildSettingsUnchanged':True,'consoleErrors':0,'sceneCleanOutsidePlayMode':True,'review':'Actual Unity Journal captures visually inspected; separate frames remain sharp at native size. Runtime status and actual gold replace mockup examples.'})
ledger=read(theme/'DesignChangeLedger.json')
previous=ledger['currentUnityCheckpoint']
write(folder/'UnityCheckpoint.json',{'checkpoint':'MFJ-SYNC-011','date':'2026-10-30','previousCheckpoint':os.path.relpath((theme/previous).resolve(),folder).replace('\\','/'),'revisions':['MFJ-R058'],'kind':'Approved ornate Journal presentation with shared live quest tracking, no reward grants','verification':'Verification.json','files':[{'path':p,'sha256':h} for p,h in sorted(current.items()) if p.startswith('Assets/')]})
entry=next(x for x in ledger['changes'] if x['id']=='MFJ-R058')
entry.update(status='verified',unityImported=True,integrationStatus='verified',synchronizationCheckpoint='MFJ-SYNC-011',remainingWork=[])
entry['approvalEvidence'].append('User requested integration into the game.')
entry['importEvidence']={'record':'Sync011/README.md','sourceHashes':'Sync011/ImportedSources.json'}
entry['verificationEvidence']['unity']='Sync011/Verification.json'
ledger['currentUnityCheckpoint']='Sync011/UnityCheckpoint.json'
ledger['synchronizationCheckpoints']=[x for x in ledger['synchronizationCheckpoints'] if x['id']!='MFJ-SYNC-011']+[{'id':'MFJ-SYNC-011','date':'2026-10-30','status':'verified','revisions':['MFJ-R058'],'manifest':'Sync011/UnityCheckpoint.json','verification':'Sync011/Verification.json'}]
write(theme/'DesignChangeLedger.json',ledger)
pending=read(theme/'PendingUnityChanges.json')
pending['pending']=[x for x in pending['pending'] if x['id']!='MFJ-R058']
pending['completed']=[x for x in pending['completed'] if x['id']!='MFJ-R058']+[{'id':'MFJ-R058','checkpoint':'MFJ-SYNC-011'}]
pending['counts']={k:len(pending[k]) for k in ['pending','completed','superseded']}
pending['currentUnityCheckpoint']='Sync011/UnityCheckpoint.json';write(theme/'PendingUnityChanges.json',pending)
notice='**MFJ-SYNC-011: the ornate R058 Journal is imported and verified in Unity.** Live quest states and gold are bound; tracking/persistence and display-only rewards are unchanged. **0 pending imports; 45 completed; 13 superseded.** Earlier pending statements below are historical. [Integration](Sync011/README.md).\n\n'
for name in ['Design Changes.md','VisualContract.md']:
    path=theme/name;text=path.read_text(encoding='utf-8')
    if notice not in text:
        heading,rest=text.split('\n',1);path.write_text(heading+'\n\n'+notice+rest.lstrip('\n'),encoding='utf-8')
revision=theme/'Revisions/MFJ-R058/README.md'
text=revision.read_text(encoding='utf-8');heading,rest=text.split('\n',1)
revision.write_text(heading+'\n\n**Imported and verified at MFJ-SYNC-011.** [Unity integration](../../Sync011/README.md). Earlier pending status below is historical.\n\n'+rest.lstrip('\n'),encoding='utf-8')
print(json.dumps({'passed':True,'changedExisting':changed,'addedAssets':len(added),'counts':pending['counts']}))
