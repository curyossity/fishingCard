import hashlib
import json
from pathlib import Path

folder=Path(__file__).resolve().parent
previous=folder.parent/'MFJ-R050'
for name in ['NativeDrawing.js','RunDesign.py','ExportDesign.py']:
    s=(previous/name).read_text(encoding='utf-8').replace('R050','R051').replace('LanternFocusParchment.png','LanternAnchorPrecision.png')
    (folder/name).write_text(s,encoding='utf-8')
original=json.loads((previous/'PenpotDesign.json').read_text(encoding='utf-8'))['bars']['0']
(folder/'SourceDefinition.json').write_text(json.dumps(original,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
root=folder.parents[1];repo=root.parents[2]
c=json.loads((root/'RarityBarEntrance/UnityCheckpoint.json').read_text(encoding='utf-8'))
(folder/'UnityBefore.json').write_text(json.dumps({'checkpoint':c['checkpoint'],'files':[{'path':f['path'],'sha256':hashlib.sha256((repo/f['path']).read_bytes()).hexdigest()} for f in c['files']]},indent=2)+'\n',encoding='utf-8')
print('Prepared refinement sources and captured Unity hashes.')
