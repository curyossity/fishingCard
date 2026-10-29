"""Record the verified Unity-only bar exit without rewriting prior import history."""
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


previous=read(base/'RarityCeilingSelection/UnityCheckpoint.json')
production=read(folder/'ProductionValidation.json')
assert production['status']=='Passed 30 production assertions'
paths={entry['path'] for entry in previous['files']}
paths.update({'Assets/UIThemes/MidnightFieldJournal/Runtime/JournalRarityExitMotion.cs',
              'Assets/UIThemes/MidnightFieldJournal/Runtime/JournalRarityExitMotion.cs.meta'})
files=[{'path':path,'sha256':hashlib.sha256((workspace/path).read_bytes()).hexdigest()} for path in sorted(paths)]
write(folder/'UnityCheckpoint.json',{
    'checkpoint':'MFJ-SYNC-008.3','date':'2026-10-25',
    'previousCheckpoint':'../RarityCeilingSelection/UnityCheckpoint.json','revisions':[],
    'kind':'One-second upward elastic timing-bar exit before encounter reveal',
    'motionAssertions':6,'timingAssertions':75,'productionAssertions':30,'gameplayAssertions':558,'files':files,
})
for name in ['DesignChangeLedger.json','PendingUnityChanges.json']:
    path=base/name;data=read(path);data['currentUnityCheckpoint']='RarityBarElasticExit/UnityCheckpoint.json';write(path,data)
note='''
## Unity animation update — MFJ-SYNC-008.3

The stopped bar now winds up elastically and exits upward over one second before the encounter is revealed. Motion matches inElastic(1,0.74), resets between attempts, and cancels safely on Surface/restart. Selection and zone widths are unchanged. [Validation and current checkpoint](RarityBarElasticExit/README.md). Unity-only change; no pending Penpot import.
'''
for name in ['Design Changes.md','Pending Unity Changes.md','VisualContract.md']:
    path=base/name;text=path.read_text(encoding='utf-8')
    if '## Unity animation update — MFJ-SYNC-008.3' not in text:
        path.write_text(text+note,encoding='utf-8')
print(json.dumps({'checkpoint':'MFJ-SYNC-008.3','files':len(files),'pendingImports':len(read(base/'PendingUnityChanges.json')['pending'])}))
