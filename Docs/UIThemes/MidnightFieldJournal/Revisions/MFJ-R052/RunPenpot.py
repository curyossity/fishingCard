"""Run one bounded Harbor design action through the initialized local Penpot bridge."""
import json
import os
from pathlib import Path
import subprocess
import sys

folder = Path(__file__).resolve().parent
action = sys.argv[1]
art = json.loads((folder / 'ImportedArt.json').read_text(encoding='utf-8'))
art.update(clear='bae680cd-9225-8018-8008-d04eeb30a002', hold='bae680cd-9225-8018-8008-d04eec929d98', rerig='bae680cd-9225-8018-8008-d04eede4b4a5', recast='bae680cd-9225-8018-8008-d04eef263bea')
recovery = json.loads((folder / 'Action-audit.json').read_text(encoding='utf-8'))['result'] if (folder / 'Action-audit.json').exists() else {}
if isinstance(recovery.get('screens'), list):
    recovery['screens'] = {item['key']: item['id'] for item in recovery['screens']}
recovery.setdefault('links', [])
code = 'const ACTION=' + json.dumps(action) + ';const ART=' + json.dumps(art) + ';\n'
code += 'if(!storage.harborR052){const saved=penpot.currentPage.getPluginData("MFJ-R052");storage.harborR052=saved?JSON.parse(saved):' + json.dumps(recovery) + ';}\n'
code += 'const outcome=(function(){\n' + (folder / 'CreateDesign.js').read_text(encoding='utf-8') + '\n})();penpot.currentPage.setPluginData("MFJ-R052",JSON.stringify(storage.harborR052));return outcome;'
code = code.replace("'Independent Technique illustration',32,167,484,270", "'Independent Technique illustration',32,160,484,200")
code = code.replace("['technique0',120,1100],['technique6',780,1100],['technique7',1440,1100]", "['technique0',120,1300],['technique6',780,1300],['technique7',1440,1300]")
request = {'name': 'execute_code', 'arguments': {'code': code}}
result = subprocess.run(['python', os.environ['TEMP'] + '/penpot-modifier-tooltip-mcp.py'], input=json.dumps(request), capture_output=True, text=True)
if result.returncode:
    print(result.stderr)
    sys.exit(result.returncode)
raw = json.loads(result.stdout)
text = raw['result']['content'][0]['text']
try:
    parsed = json.loads(text)
    (folder / ('Action-' + action + '.json')).write_text(json.dumps(parsed, indent=2, ensure_ascii=False), encoding='utf-8')
    state = parsed.get('result', parsed)
    if 'screens' in state and isinstance(state['screens'], dict):
        (folder / 'PenpotDesign.json').write_text(json.dumps(state, indent=2, ensure_ascii=False), encoding='utf-8')
    print(json.dumps(state, ensure_ascii=True))
except json.JSONDecodeError:
    print(text)
    sys.exit(1)
