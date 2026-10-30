"""Replace magnified raster UI framing with native editable vector paths in Penpot."""
import json,os,subprocess,sys
from pathlib import Path

folder=Path(__file__).resolve().parent
targets={'harbor':'75555645-d407-8067-8008-de8ff58e2639','shop':'75555645-d407-8067-8008-de9b9c9710d1','collection':'75555645-d407-8067-8008-de9d62501389'}
def call(code):
    r=subprocess.run(['python',os.environ['TEMP']+'/penpot-modifier-tooltip-mcp.py'],input=json.dumps({'name':'execute_code','arguments':{'code':code}}),text=True,capture_output=True)
    if r.returncode:raise RuntimeError(r.stderr[:2000])
    raw=json.loads(r.stdout);value=raw['result']['content'][0]['text']
    try:return json.loads(value)
    except ValueError:raise RuntimeError(value[:1000])

if __name__=='__main__':
    kind=sys.argv[1];svg=(folder/'Vectors'/f'{kind.title()}Frames.svg').read_text(encoding='utf-8')
    chunk_size=32000
    call('storage.r057svg=[];return {ready:true};')
    for offset in range(0,len(svg),chunk_size):
        call('storage.r057svg.push('+json.dumps(svg[offset:offset+chunk_size])+');return {chunks:storage.r057svg.length};')
    code='const KIND='+json.dumps(kind)+';const ID='+json.dumps(targets[kind])+';const SVG=storage.r057svg.join("");\n'+r'''
const saved=penpot.currentPage.getPluginData('MFJ-R057');const state=saved?JSON.parse(saved):{revision:'MFJ-R057',fileId:penpot.currentFile.id,pageId:penpot.currentPage.id,pageName:penpot.currentPage.name,screens:{},unityImported:false};
const b=penpot.currentPage.getShapeById(ID);
if(state.screens[KIND]){penpot.selection=[b];penpot.viewport.zoomIntoView([b]);return state;}
const old=b.children.find(s=>s.width>3800&&s.height>2100);
if(!old)throw Error('Full-screen raster review art not found');
const foreground=[...b.children].filter(s=>s.id!==old.id);
const backup=b.clone();backup.name='R057 comparison / original raster framing / '+KIND;backup.x=b.x;backup.y=b.y+6000;
const layer=penpot.createShapeFromSvg(SVG);b.appendChild(layer);layer.name='MFJ-R057 / SHARP VECTOR FRAMES / '+KIND;layer.resize(3840,2160);layer.x=b.x;layer.y=b.y;
for(const s of foreground)b.appendChild(s);
state.screens[KIND]={boardId:b.id,backupBoardId:backup.id,vectorLayerId:layer.id,sourceArtId:old.id,foregroundIds:foreground.map(s=>s.id),width:b.width,height:b.height,vectorSource:'Vectors/'+KIND[0].toUpperCase()+KIND.slice(1)+'Frames.svg'};
penpot.currentPage.setPluginData('MFJ-R057',JSON.stringify(state));penpot.selection=[b];penpot.viewport.zoomIntoView([b]);return state;
'''
    result=call(code);state=result.get('result',result)
    (folder/'PenpotDesign.json').write_text(json.dumps(state,indent=2),encoding='utf-8');print(json.dumps(state))
