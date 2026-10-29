"""Reuse native drawing helpers, not the previous layouts."""
import hashlib
import json
from pathlib import Path

folder=Path(__file__).resolve().parent
previous=folder.parent/'MFJ-R048'
source=(previous/'CreateDesign.js').read_text(encoding='utf-8')
(folder/'NativeDrawing.js').write_text(source[source.index('function board('):source.index('function frame(')],encoding='utf-8')
runner=(previous/'RunDesign.py').read_text(encoding='utf-8').replace('R048','R049')
runner=runner.replace("source = (folder / 'CreateDesign.js').read_text(encoding='utf-8')", "source = (folder / 'NativeDrawing.js').read_text(encoding='utf-8') + '\\n' + (folder / 'CreateDesign.js').read_text(encoding='utf-8')")
(folder/'RunDesign.py').write_text(runner,encoding='utf-8')
exporter=(previous/'ExportDesign.py').read_text(encoding='utf-8').replace('EngravedRuler.png','TideDial.png').replace('TackleLabels.png','LineSpool.png').replace('LanternFocus.png','SoundingChart.png')
(folder/'ExportDesign.py').write_text(exporter,encoding='utf-8')
root=folder.parents[1]
repo=root.parents[2]
checkpoint=json.loads((root/'RarityBarEntrance/UnityCheckpoint.json').read_text(encoding='utf-8'))
files=[{'path':f['path'],'sha256':hashlib.sha256((repo/f['path']).read_bytes()).hexdigest()} for f in checkpoint['files']]
(folder/'UnityBefore.json').write_text(json.dumps({'checkpoint':checkpoint['checkpoint'],'files':files},indent=2)+'\n',encoding='utf-8')
print('Prepared native helpers and captured',len(files),'Unity file hashes.')
