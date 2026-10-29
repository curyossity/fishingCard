"""Run a bounded native Penpot creation action and retain its stable IDs."""
import json
import os
from pathlib import Path
import subprocess
import sys

folder = Path(__file__).resolve().parent
bridge = Path(os.environ['TEMP']) / 'penpot-modifier-tooltip-mcp.py'
action = sys.argv[1]
source = (folder / 'NativeDrawing.js').read_text(encoding='utf-8') + '\n' + (folder / 'CreateDesign.js').read_text(encoding='utf-8')
saved = folder / 'PenpotDesign.json'
hydrate = 'if(!storage.tagBarR049)storage.tagBarR049=' + saved.read_text(encoding='utf-8') + ';\n' if saved.exists() else ''
request = {'name': 'execute_code', 'arguments': {'code': hydrate + 'const ACTION=' + json.dumps(action) + ';\n' + source}}
result = subprocess.run([sys.executable, str(bridge)], input=json.dumps(request), capture_output=True, encoding='utf-8')
if result.returncode:
    raise RuntimeError(result.stderr or result.stdout)
outer = json.loads(result.stdout)
joined = '\n'.join(p.get('text', '') for p in outer.get('result', {}).get('content', []) if p.get('type') == 'text')
try:
    value = json.loads(joined)
except json.JSONDecodeError:
    raise RuntimeError(joined)
if 'result' not in value:
    raise RuntimeError(value)
data = value['result']
(folder / ('Action-' + action.replace(':', '-') + '.json')).write_text(json.dumps(data, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
if action.startswith('bar:') and data.get('complete'):
    state = json.loads(saved.read_text(encoding='utf-8'))
    state['bars'][action.split(':')[1]] = data
    saved.write_text(json.dumps(state, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
if action in ('initialize', 'finish'):
    saved.write_text(json.dumps(data, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
print(json.dumps(data, ensure_ascii=True))
