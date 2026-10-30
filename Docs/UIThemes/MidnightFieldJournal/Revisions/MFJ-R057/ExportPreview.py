"""Save a scaled Penpot render without sending large image buffers to the chat."""
import base64
import json
import os
from pathlib import Path
import subprocess
import sys

folder = Path(__file__).resolve().parent
shape_id, name = sys.argv[1:3]
scale = float(sys.argv[3]) if len(sys.argv) > 3 else .5
code = 'const s=penpot.currentPage.getShapeById(' + json.dumps(shape_id) + ');const data=await s.export({type:"jpeg",scale:' + str(scale) + '});let binary="";for(let i=0;i<data.length;i+=8192)binary+=String.fromCharCode(...data.slice(i,i+8192));return {base64:btoa(binary),width:s.width,height:s.height};'
r = subprocess.run(['python', os.environ['TEMP'] + '/penpot-modifier-tooltip-mcp.py'], input=json.dumps({'name': 'execute_code', 'arguments': {'code': code}}), text=True, capture_output=True)
if r.returncode:
    print(r.stderr)
    sys.exit(r.returncode)
raw = json.loads(r.stdout)
text = raw['result']['content'][0]['text']
try:
    data = json.loads(text)['result']
    path = folder / name
    path.write_bytes(base64.b64decode(data['base64']))
    print(json.dumps({'file': str(path), 'bytes': path.stat().st_size, 'scale': scale}))
except (ValueError, KeyError):
    print(text[:500])
    sys.exit(1)
