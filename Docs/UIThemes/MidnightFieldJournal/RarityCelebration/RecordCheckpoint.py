"""Record verified Unity particle feedback without rewriting prior checkpoints."""
from pathlib import Path
import hashlib
import json

folder=Path(__file__).resolve().parent
base=folder.parent
workspace=base.parents[2]


def read(path):
    return json.loads(path.read_text(encoding='utf-8'))


def write(path,data):
    payload=json.dumps(data,indent=2,ensure_ascii=False)+'\n'
    temporary=path.with_suffix(path.suffix+'.tmp')
    temporary.write_text(payload,encoding='utf-8')
    temporary.replace(path)


previous=read(base/'RarityBarElasticExit/UnityCheckpoint.json')
assert read(folder/'ProductionValidation.json')['status']=='Passed 12 production assertions'
paths={entry['path'] for entry in previous['files']}
for path in ['Assets/UIThemes/MidnightFieldJournal/Runtime/JournalRarityCelebration.cs',
             'Assets/UIThemes/MidnightFieldJournal/Editor/JournalRarityCelebrationChecks.cs']:
    paths.update({path,path+'.meta'})
files=[{'path':path,'sha256':hashlib.sha256((workspace/path).read_bytes()).hexdigest()} for path in sorted(paths)]
write(folder/'UnityCheckpoint.json',{
    'checkpoint':'MFJ-SYNC-008.4','date':'2026-10-25',
    'previousCheckpoint':'../RarityBarElasticExit/UnityCheckpoint.json','revisions':[],
    'kind':'Particle celebration for the highest displayed non-Miss rarity',
    'particleAssertions':14,'timingAssertions':75,'motionAssertions':6,'productionAssertions':12,'files':files,
})
for name in ['DesignChangeLedger.json','PendingUnityChanges.json']:
    path=base/name;data=read(path);data['currentUnityCheckpoint']='RarityCelebration/UnityCheckpoint.json';write(path,data)
note='''
## Unity feedback update — MFJ-SYNC-008.4

Stopping in the highest displayed non-Miss rarity now emits a gold/ivory particle burst at the marker. The effect emits once per occurrence, clears on cancellation/reuse, and preserves selection odds, hold and upward exit. [4K verification and current checkpoint](RarityCelebration/README.md). Unity-only change; no pending Penpot import.
'''
for name in ['Design Changes.md','Pending Unity Changes.md','VisualContract.md']:
    path=base/name;text=path.read_text(encoding='utf-8')
    if '## Unity feedback update — MFJ-SYNC-008.4' not in text:
        path.write_text(text+note,encoding='utf-8')
print(json.dumps({'checkpoint':'MFJ-SYNC-008.4','files':len(files),'pendingImports':len(read(base/'PendingUnityChanges.json')['pending'])}))
