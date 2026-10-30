"""Build editable nine-slice shells with fixed, downsampled decorative corners."""
import hashlib,json,os,struct,subprocess,sys
from pathlib import Path

folder=Path(__file__).resolve().parent
targets={'harbor':'75555645-d407-8067-8008-de8ff58e2639','shop':'75555645-d407-8067-8008-de9b9c9710d1','collection':'75555645-d407-8067-8008-de9d62501389'}
def call(name,arguments):
    r=subprocess.run(['python',os.environ['TEMP']+'/penpot-modifier-tooltip-mcp.py'],input=json.dumps({'name':name,'arguments':arguments}),text=True,capture_output=True)
    if r.returncode:raise RuntimeError(r.stderr[:1200])
    raw=json.loads(r.stdout);value=raw['result']['content'][0]['text']
    try:return json.loads(value)
    except ValueError:raise RuntimeError(value[:1000])

if __name__=='__main__':
    kind=sys.argv[1]
    art=json.loads((folder/'ImportedMasters.json').read_text(encoding='utf-8')) if (folder/'ImportedMasters.json').exists() else {}
    for i,key in enumerate(['Dark','Paper']):
        p=folder/'Art'/f'{key}FrameMaster.png';width,height=struct.unpack('>II',p.read_bytes()[16:24])
        if key not in art:
            result=call('import_image',{'filePath':str(p),'x':64000,'y':39000+i*1400,'width':width,'height':height})
            art[key]={'id':result['shapeId'],'width':width,'height':height,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()}
            (folder/'ImportedMasters.json').write_text(json.dumps(art,indent=2),encoding='utf-8')
    code='const KIND='+json.dumps(kind)+';const ID='+json.dumps(targets[kind])+';const MASTERS='+json.dumps(art)+';\n'+(folder/'NineSlice.js').read_text(encoding='utf-8')
    result=call('execute_code',{'code':code});state=result.get('result',result)
    (folder/'PenpotDesign.json').write_text(json.dumps(state,indent=2),encoding='utf-8');print(json.dumps(state))
