"""Place native-size frame renders while retaining all original editable foregrounds."""
import json,sys
from ApplySharpFrames import call,folder,targets

kind=sys.argv[1]
geometry=json.loads((folder/(kind.title()+'FrameGeometry.json')).read_text(encoding='utf-8'))
saved=call('execute_code',{'code':"return JSON.parse(penpot.currentPage.getPluginData('MFJ-R057'));"})['result']
if '--refresh' not in sys.argv and saved.get('screens',{}).get(kind,{}).get('method')=='native-4k-fixed-corner-render':
    (folder/'PenpotDesign.json').write_text(json.dumps(saved,indent=2),encoding='utf-8');print('Already placed '+kind);sys.exit(0)
# Remove the expensive native preview assembly before importing the final native-size render.
call('execute_code',{'code':'const b=penpot.currentPage.getShapeById('+json.dumps(targets[kind])+');for(const s of [...b.children])if(s.name.startsWith("MFJ-R057 /"))s.remove();return {cleared:true};'})
imported=call('import_image',{'filePath':str(folder/(kind.title()+'Frames4K.png')),'x':64000,'y':43000,'width':3840,'height':2160})
code='const KIND='+json.dumps(kind)+';const ID='+json.dumps(targets[kind])+';const IMPORT='+json.dumps(imported['shapeId'])+';const GEOMETRY='+json.dumps(geometry)+';\n'+r'''
const state=JSON.parse(penpot.currentPage.getPluginData('MFJ-R057'));
const b=penpot.currentPage.getShapeById(ID);
let previous=state.screens[KIND];
const source=b.children.find(s=>s.width>3800&&s.height>2100);
const foreground=[...b.children].filter(s=>s.id!==source.id);
let backup=penpot.currentPage.root.children.find(s=>s.name==='R057 comparison / original raster framing / '+KIND);
if(!backup){backup=b.clone();backup.name='R057 comparison / original raster framing / '+KIND;backup.x=b.x;backup.y=b.y+6000;}
const layer=penpot.currentPage.getShapeById(IMPORT);b.appendChild(layer);layer.name='MFJ-R057 / Native 4K sharp frame render / '+KIND;layer.resize(3840,2160);layer.x=b.x;layer.y=b.y;
for(const s of foreground)b.appendChild(s);
state.screens[KIND]={boardId:b.id,backupBoardId:backup.id,frameLayerId:layer.id,sourceArtId:source.id,foregroundIds:foreground.map(s=>s.id),method:'native-4k-fixed-corner-render',shells:GEOMETRY,frameSource:KIND[0].toUpperCase()+KIND.slice(1)+'Frames4K.png',nativeFrameSize:[3840,2160]};
penpot.currentPage.setPluginData('MFJ-R057',JSON.stringify(state));penpot.selection=[b];penpot.viewport.zoomIntoView([b]);return state;
'''
raw=call('execute_code',{'code':code});state=raw.get('result',raw)
(folder/'PenpotDesign.json').write_text(json.dumps(state,indent=2),encoding='utf-8');print(json.dumps({'placed':kind,'screens':list(state['screens'])}))
