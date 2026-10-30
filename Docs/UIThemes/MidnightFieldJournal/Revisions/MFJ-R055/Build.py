"""Import independent replacement equipment illustrations and assemble the shop."""
import json,os,subprocess,struct
from pathlib import Path

folder=Path(__file__).resolve().parent
def call(name,arguments):
    r=subprocess.run(['python',os.environ['TEMP']+'/penpot-modifier-tooltip-mcp.py'],input=json.dumps({'name':name,'arguments':arguments}),text=True,capture_output=True,check=True)
    raw=json.loads(r.stdout)
    text=raw['result']['content'][0]['text']
    try:return json.loads(text)
    except ValueError:raise RuntimeError(text[:1000])

if __name__=='__main__':
    art=json.loads((folder/'ImportedArt.json').read_text(encoding='utf-8')) if (folder/'ImportedArt.json').exists() else {}
    for i,key in enumerate(['rod','reel','hook']):
        if key not in art:
            art[key]=call('import_image',{'filePath':str(folder/'Art'/f'{key.title()}Engraved.png'),'x':60000,'y':37000+i*600,'width':700,'height':450})['shapeId']
            (folder/'ImportedArt.json').write_text(json.dumps(art,indent=2),encoding='utf-8')
    dimensions={key:struct.unpack('>II',(folder/'Art'/f'{key.title()}Engraved.png').read_bytes()[16:24]) for key in art}
    code='const ART='+json.dumps(art)+';const DIMENSIONS='+json.dumps(dimensions)+';\n'+(folder/'CreateDesign.js').read_text(encoding='utf-8')
    result=call('execute_code',{'code':code})
    state=result.get('result',result)
    (folder/'PenpotDesign.json').write_text(json.dumps(state,indent=2),encoding='utf-8')
    print(json.dumps(state))
