import json
import os
from pathlib import Path
import subprocess
import sys

folder = Path(__file__).resolve().parent
state = json.loads((folder / 'PenpotDesign.json').read_text(encoding='utf-8'))
index = int(sys.argv[1])
ids = [state['reviewId'], state['bars']['0']['id']]
names = ['DesignReview.png', 'LanternAnchorPrecision.png']
request = {'name': 'export_shape', 'arguments': {'shapeId': ids[index], 'format': 'png', 'filePath': str(folder / names[index])}}
result = subprocess.run([sys.executable, str(Path(os.environ['TEMP']) / 'penpot-modifier-tooltip-mcp.py')], input=json.dumps(request), capture_output=True, encoding='utf-8')
if result.returncode:
    raise RuntimeError(result.stderr)
response = json.loads(result.stdout)
content = '\n'.join(p.get('text', '') for p in response.get('result', {}).get('content', []) if p.get('type') == 'text')
if 'has been exported' not in content:
    raise RuntimeError(content)
print(content)
