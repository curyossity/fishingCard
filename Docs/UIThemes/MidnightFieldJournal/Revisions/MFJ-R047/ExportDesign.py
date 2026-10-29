import json
import os
from pathlib import Path
import subprocess
import sys

folder = Path(__file__).resolve().parent
state = json.loads((folder / 'PenpotDesign.json').read_text(encoding='utf-8'))
index = int(sys.argv[1])
ids = [state['reviewId']] + state['rowIds']
names = ['DesignReview.png', 'FieldJournal.png', 'NightSounder.png', 'NaturalistsLedger.png']
request = {'name': 'export_shape', 'arguments': {'shapeId': ids[index], 'format': 'png', 'filePath': str(folder / names[index])}}
result = subprocess.run([sys.executable, str(Path(os.environ['TEMP']) / 'penpot-modifier-tooltip-mcp.py')], input=json.dumps(request), capture_output=True, encoding='utf-8')
print(result.stdout)
if result.returncode:
    raise RuntimeError(result.stderr)
