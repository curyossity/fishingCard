"""Record the verified Unity bottom entrance without changing earlier integration records."""
from pathlib import Path
import hashlib
import json

folder=Path(__file__).resolve().parent
base=folder.parent
workspace=base.parents[2]


def read(path):
    return json.loads(path.read_text(encoding='utf-8'))


def write(path,data):
    temporary=path.with_suffix(path.suffix+'.tmp')
    temporary.write_text(json.dumps(data,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
    temporary.replace(path)


reports={}
for name,relative,expected in [('entrance','ProductionValidation.json',16),('timing','TimingFlow/ProductionValidation.json',30),('celebration','Celebration/ProductionValidation.json',12)]:
    data=read(folder/relative)
    assert data['status']==f'Passed {expected} production assertions'
    reports[name]=data['assertions']
previous=read(base/'RarityCelebration/UnityCheckpoint.json')
paths={entry['path'] for entry in previous['files']}
path='Assets/UIThemes/MidnightFieldJournal/Editor/JournalRarityEntranceChecks.cs'
paths.update({path,path+'.meta'})
files=[{'path':path,'sha256':hashlib.sha256((workspace/path).read_bytes()).hexdigest()} for path in sorted(paths)]
write(folder/'UnityCheckpoint.json',{
    'checkpoint':'MFJ-SYNC-008.5','date':'2026-10-27','previousCheckpoint':'../RarityCelebration/UnityCheckpoint.json','revisions':[],
    'kind':'One-second bottom entrance with elastic settling and occurrence-bound marker/Stop gates',
    'entranceAssertions':8,'timingAssertions':75,'motionAssertions':6,'particleAssertions':14,'productionSuites':reports,'files':files,
})
for name in ['DesignChangeLedger.json','PendingUnityChanges.json']:
    path=base/name;data=read(path);data['currentUnityCheckpoint']='RarityBarEntrance/UnityCheckpoint.json';write(path,data)
note='''
## Unity entrance update — MFJ-SYNC-008.5

The rarity bar now enters from below over one second using the mirrored elastic exit curve. The marker and Stop remain inactive until it settles; cancellation, restart, the upward exit and highest-rarity particles are verified. [4K captures and current checkpoint](RarityBarEntrance/README.md). Unity-only update; no pending Penpot import.
'''
for name in ['Design Changes.md','Pending Unity Changes.md','VisualContract.md']:
    path=base/name;text=path.read_text(encoding='utf-8')
    if '## Unity entrance update — MFJ-SYNC-008.5' not in text:
        path.write_text(text+note,encoding='utf-8')
print(json.dumps({'checkpoint':'MFJ-SYNC-008.5','files':len(files),'productionAssertions':reports,'pendingImports':len(read(base/'PendingUnityChanges.json')['pending'])}))
