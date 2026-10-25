"""Record verified ceiling selection without modifying earlier checkpoints or Penpot revisions."""
from pathlib import Path
import hashlib
import json

base=Path(__file__).resolve().parent.parent
workspace=base.parents[2]
folder=Path(__file__).resolve().parent


def read(path):
    return json.loads(path.read_text(encoding='utf-8'))


def write(path,data):
    payload=json.dumps(data,indent=2,ensure_ascii=False)+'\n'
    temporary=path.with_suffix(path.suffix+'.tmp')
    temporary.write_text(payload,encoding='utf-8')
    temporary.replace(path)


previous=read(base/'RarityWidthTuning/UnityCheckpoint.json')
production=read(folder/'ProductionValidation.json')
assert production['status'].startswith('Passed') and production['assertions']==22
files=[{'path':entry['path'],'sha256':hashlib.sha256((workspace/entry['path']).read_bytes()).hexdigest()} for entry in previous['files']]
write(folder/'UnityCheckpoint.json',{
    'checkpoint':'MFJ-SYNC-008.2','date':'2026-10-25',
    'previousCheckpoint':'../RarityWidthTuning/UnityCheckpoint.json','revisions':[],
    'kind':'Uniform species selection at or below stopped rarity ceiling',
    'timingAssertions':75,'productionAssertions':22,'gameplayAssertions':558,'files':files,
})
for name in ['DesignChangeLedger.json','PendingUnityChanges.json']:
    path=base/name;data=read(path);data['currentUnityCheckpoint']='RarityCeilingSelection/UnityCheckpoint.json';write(path,data)
note='''
## Unity gameplay update — MFJ-SYNC-008.2

The stopped rarity now sets a ceiling. Every distinct biome/depth-eligible species at or below that rarity has one equal chance, without rarity/species weights or immediate-repeat exclusions. Bar sizes stay unchanged. Result text says “UP TO [rarity]”. Event pacing and fixed promises remain intact. [Validation and current checkpoint](RarityCeilingSelection/README.md). No new Penpot revision or pending import.
'''
for name in ['Design Changes.md','Pending Unity Changes.md','VisualContract.md']:
    path=base/name;text=path.read_text(encoding='utf-8')
    if '## Unity gameplay update — MFJ-SYNC-008.2' not in text:
        path.write_text(text+note,encoding='utf-8')
print(json.dumps({'checkpoint':'MFJ-SYNC-008.2','files':len(files),'pendingImports':len(read(base/'PendingUnityChanges.json')['pending'])}))
