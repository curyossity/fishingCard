"""Place the single ornate Harbor review screen in the connected Penpot file."""
import json, os, subprocess
from pathlib import Path

folder = Path(__file__).resolve().parent
def call(name, arguments):
    r = subprocess.run(['python', os.environ['TEMP']+'/penpot-modifier-tooltip-mcp.py'], input=json.dumps({'name':name,'arguments':arguments}), capture_output=True, text=True, check=True)
    raw = json.loads(r.stdout)
    content = raw['result']['content'][0]['text']
    try: return json.loads(content)
    except ValueError: raise RuntimeError(content[:1000])

if __name__ == '__main__':
    imported = call('import_image', {'filePath':str(folder/'Art/TackleShopOrnateBlank.png'), 'x':52700,'y':31600,'width':3840,'height':2160})
    (folder/'ImportedArt.json').write_text(json.dumps(imported,indent=2),encoding='utf-8')
    print(json.dumps(imported))
