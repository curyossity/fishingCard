import json
import os
from pathlib import Path
import subprocess
import sys

folder = Path(__file__).resolve().parent
bridge = Path(os.environ['TEMP']) / 'penpot-modifier-tooltip-mcp.py'
action = sys.argv[1]
source = (folder / 'CreateDesign.js').read_text(encoding='utf-8')
source = source.replace('Design assumption: 12.5% recovery baseline', 'User confirmed: 12.5% recovery baseline')
saved = folder / 'PenpotDesign.json'
hydrate = 'if(!storage.tagBarR047)storage.tagBarR047=' + saved.read_text(encoding='utf-8') + ';\n' if saved.exists() else ''
request = {'name': 'execute_code', 'arguments': {'code': hydrate + 'const ACTION=' + json.dumps(action) + ';\n' + source}}
result = subprocess.run([sys.executable, str(bridge)], input=json.dumps(request), capture_output=True, encoding='utf-8')
if result.returncode:
    raise RuntimeError(result.stderr or result.stdout)
outer = json.loads(result.stdout)
content = outer.get('result', {}).get('content', [])
parts = [p.get('text', '') for p in content if p.get('type') == 'text']
joined = '\n'.join(parts)
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
    state['bars'][':'.join(action.split(':')[1:])] = data
    saved.write_text(json.dumps(state, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
if action in ('initialize', 'finish'):
    (folder / 'PenpotDesign.json').write_text(json.dumps(data, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
print(json.dumps(data, ensure_ascii=True))
