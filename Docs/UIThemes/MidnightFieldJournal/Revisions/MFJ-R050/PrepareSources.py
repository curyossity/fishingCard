import hashlib
import json
from pathlib import Path

folder=Path(__file__).resolve().parent
previous=folder.parent/'MFJ-R049'
for name in ['NativeDrawing.js','RunDesign.py']:
    s=(previous/name).read_text(encoding='utf-8').replace('R049','R050')
    if name=='RunDesign.py':
        s=s.replace("source = (folder / 'NativeDrawing.js')", "source = 'const SOURCE=' + (folder / 'SourceDefinition.json').read_text(encoding='utf-8') + ';\\n' + (folder / 'NativeDrawing.js')")
    (folder/name).write_text(s,encoding='utf-8')
original=json.loads((folder.parent/'MFJ-R048/PenpotDesign.json').read_text(encoding='utf-8'))['bars']['2']
(folder/'SourceDefinition.json').write_text(json.dumps(original,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
exporter=(previous/'ExportDesign.py').read_text(encoding='utf-8')
exporter=exporter.replace("ids = [state['reviewId']] + state['rowIds']", "ids = [state['reviewId'], state['bars']['0']['id']]")
exporter=exporter.replace("names = ['DesignReview.png', 'TideDial.png', 'LineSpool.png', 'SoundingChart.png']", "names = ['DesignReview.png', 'LanternFocusParchment.png']")
(folder/'ExportDesign.py').write_text(exporter,encoding='utf-8')
root=folder.parents[1];repo=root.parents[2]
c=json.loads((root/'RarityBarEntrance/UnityCheckpoint.json').read_text(encoding='utf-8'))
(folder/'UnityBefore.json').write_text(json.dumps({'checkpoint':c['checkpoint'],'files':[{'path':f['path'],'sha256':hashlib.sha256((repo/f['path']).read_bytes()).hexdigest()} for f in c['files']]},indent=2)+'\n',encoding='utf-8')
print('Prepared refinement sources and captured Unity file hashes.')
