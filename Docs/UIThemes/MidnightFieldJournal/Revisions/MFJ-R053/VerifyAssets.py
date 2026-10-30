import hashlib,json
from pathlib import Path
folder=Path(__file__).resolve().parent
project=folder.parents[4]
before=json.loads((folder.parent/'MFJ-R052/UnityBefore.json').read_text(encoding='utf-8'))
expected={f['path']:f['sha256'] for f in before['files']}
actual={p.relative_to(project).as_posix():hashlib.sha256(p.read_bytes()).hexdigest() for p in (project/'Assets').rglob('*') if p.is_file()}
delta=[k for k in expected.keys()|actual.keys() if expected.get(k)!=actual.get(k)]
report=json.loads((folder/'Verification.json').read_text(encoding='utf-8'))
report['unityAssetsComparison']={'baseline':'../MFJ-R052/UnityBefore.json','checkedFiles':len(actual),'differences':delta,'unchanged':not delta}
(folder/'Verification.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
state=json.loads((folder/'PenpotDesign.json').read_text(encoding='utf-8'))
(folder/'ImportedArt.json').write_text(json.dumps({'shapeId':state['artId']},indent=2)+'\n',encoding='utf-8')
print(json.dumps(report['unityAssetsComparison']))
