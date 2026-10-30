"""Assemble the Collection using exact copies of the active game's card resources."""
import json,os,subprocess,sys
from pathlib import Path

folder=Path(__file__).resolve().parent
def call(name,arguments):
    r=subprocess.run(['python',os.environ['TEMP']+'/penpot-modifier-tooltip-mcp.py'],input=json.dumps({'name':name,'arguments':arguments}),text=True,capture_output=True,check=True)
    raw=json.loads(r.stdout);value=raw['result']['content'][0]['text']
    try:return json.loads(value)
    except ValueError:raise RuntimeError(value[:1000])

if __name__=='__main__':
    sources=json.loads((folder/'GameCardSources.json').read_text(encoding='utf-8'))
    art=json.loads((folder/'ImportedArt.json').read_text(encoding='utf-8')) if (folder/'ImportedArt.json').exists() else {}
    requested=list(sources)
    if '--sources-only' not in sys.argv:requested+=['background']
    for i,key in enumerate(requested):
        if key not in art:
            path=folder/sources[key]['file'] if key in sources else folder/'Art/CollectionBlank.png'
            art[key]=call('import_image',{'filePath':str(path),'x':60000,'y':40000+i*500,'width':700,'height':450})['shapeId']
            (folder/'ImportedArt.json').write_text(json.dumps(art,indent=2),encoding='utf-8')
            print('Imported '+key,flush=True)
    if '--sources-only' in sys.argv:sys.exit(0)
    code='const ART='+json.dumps(art)+';const SOURCES='+json.dumps(sources)+';\n'+(folder/'CreateDesign.js').read_text(encoding='utf-8')
    raw=call('execute_code',{'code':code});state=raw.get('result',raw)
    (folder/'PenpotDesign.json').write_text(json.dumps(state,indent=2),encoding='utf-8');print(json.dumps(state))
