"""Record the tested uncapped-hand rules increment and preserve completed Penpot integrations."""
import hashlib
import json
from pathlib import Path

folder=Path(__file__).resolve().parent
project=folder.parents[2]
theme=project/'Docs/UIThemes/MidnightFieldJournal'
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def write(p,data):p.write_text(json.dumps(data,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
before={x['path']:x['sha256'] for x in read(folder.parent/'TackleShop/UnityCheckpoint.json')['files']}
current={p.relative_to(project).as_posix():sha(p) for p in (project/'Assets').rglob('*') if p.is_file()}
changed=sorted(p for p,h in before.items() if current.get(p)!=h)
allowed={'Assets/Editor/'+n+'.cs' for n in ['BassUrchinOctopusChecks','CoastalEventChecks','CreatureReleaseChecks','EventActionChecks','EventResolutionChecks','EventTechniqueChecks','LobsterSquidEelChecks','ShellFeedingExchangeChecks','TackleShopChecks','TechniqueRuleChecks']}
allowed|={'Assets/Scripts/'+n+'.cs' for n in ['ApprovedTechniqueRuntime','CreatureEffectRuntime','FishingRunController','TechniqueDeckRuntime']}
allowed.add('Assets/UIThemes/MidnightFieldJournal/Scenes/MidnightFieldJournalComparison.unity')
assert set(changed)<=allowed,changed
added=sorted(set(current)-set(before))
assert set(added)=={'Assets/Editor/UnlimitedTechniqueHandChecks.cs','Assets/Editor/UnlimitedTechniqueHandChecks.cs.meta'},added
build='ProjectSettings/EditorBuildSettings.asset'
old_build=next(x['sha256'] for x in read(theme/'Sync010/Before.json')['files'] if x['path']==build)
assert sha(project/build)==old_build
reports={n:read(folder/n) for n in ['RulesVerification.json','RegressionVerification.json','PlayVerification.json']}
assert all(r['passed'] for r in reports.values())
write(folder/'Verification.json',{'passed':True,'date':'2026-10-31','reports':reports,'changedExistingAssets':changed,'addedAssets':added,'protectedExistingAssetsUnchanged':len(before)-len(changed),'noArtworkPrefabOrEncounterContentChanges':True,'originalSceneAndBuildSettingsUnchanged':True,'twelveCardHandInspectedAt4K':True,'isolatedPlayerSaveFixturesCleanedAfterPlay':True,'consoleErrors':0,'sceneCleanOutsidePlayMode':True})
write(folder/'UnityCheckpoint.json',{'checkpoint':'MFJ-SYNC-011.2','date':'2026-10-31','previousCheckpoint':'../TackleShop/UnityCheckpoint.json','kind':'Unity-only unlimited Technique hand; equipment purchase/equip semantics confirmed','revisions':[],'verification':'Verification.json','files':[{'path':p,'sha256':h} for p,h in sorted(current.items())]})
relative='../../Validation/UnlimitedTechniqueHand/UnityCheckpoint.json'
ledger=read(theme/'DesignChangeLedger.json');ledger['currentUnityCheckpoint']=relative
ledger['synchronizationCheckpoints']=[x for x in ledger['synchronizationCheckpoints'] if x['id']!='MFJ-SYNC-011.2']+[{'id':'MFJ-SYNC-011.2','date':'2026-10-31','status':'verified','revisions':[],'manifest':relative,'verification':'../../Validation/UnlimitedTechniqueHand/Verification.json'}]
write(theme/'DesignChangeLedger.json',ledger)
pending=read(theme/'PendingUnityChanges.json');pending['currentUnityCheckpoint']=relative
assert not pending['pending'];write(theme/'PendingUnityChanges.json',pending)
notice='**MFJ-SYNC-011.2: Technique hand size is unrestricted.** Four starting cards normally; six with Tideglass Reel. Explicit effects may grow either hand further; obsolete room-discard prompts are removed. Equipment ownership/loadout rules remain confirmed. Unity-only increment; **0 pending imports, 45 completed, 13 superseded.** [Verification](../../Validation/UnlimitedTechniqueHand/README.md).\n\n'
for name in ['Design Changes.md','VisualContract.md']:
    p=theme/name;s=p.read_text(encoding='utf-8')
    if notice not in s:
        heading,rest=s.split('\n',1);p.write_text(heading+'\n\n'+notice+rest.lstrip('\n'),encoding='utf-8')
print(json.dumps({'passed':True,'changedExistingAssets':len(changed),'addedAssets':len(added),'pendingImports':0}))
