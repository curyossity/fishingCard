"""Inspect actual Penpot geometry and preserve the separable source assets for the next Unity handoff."""
import json
from Bridge import call, folder
design=json.loads((folder/'PenpotDesign.json').read_text(encoding='utf-8'))
art=json.loads((folder/'ImportedArt.json').read_text(encoding='utf-8'))
code='const D='+json.dumps(design)+';const ART='+json.dumps(art)+';'+r'''
const b=penpot.currentPage.getShapeById(D.boardId);
let sources=penpotUtils.findShape(s=>s.name==='MFJ-R058 / Reusable Journal sources',penpot.currentPage.root);
if(!sources){sources=penpot.createBoard();sources.name='MFJ-R058 / Reusable Journal sources';sources.resize(5500,6100);sources.x=58000;sources.y=34000;sources.fills=[{fillColor:'#10262E',fillOpacity:1}];}
const positions={perimeter:[0,0],wordmark:[0,2280],navigation:[1100,2280],gold:[3000,2280],sidebar:[0,2700],illustration:[1100,2700],quest:[2150,2700],questTitle:[2150,3300],footer:[2150,3600],return:[2150,3900],illustrationArt:[0,4400]};
for(const [key,id] of Object.entries(ART)){const s=penpot.currentPage.getShapeById(id);sources.appendChild(s);s.x=sources.x+positions[key][0];s.y=sources.y+positions[key][1];s.name='Source / '+key;if(key==='illustrationArt')s.resize(810,1080);}
const frames=D.frames.map(f=>{const s=penpot.currentPage.getShapeById(f.id);return {id:s.id,name:s.name,width:s.width,height:s.height,nativePixels:f.sourcePixels,scaleX:s.width/f.sourcePixels[0],scaleY:s.height/f.sourcePixels[1],cornerScale:f.cornerScale};});
const outside=penpotUtils.analyzeDescendants(b,(root,s)=>!penpotUtils.isContainedIn(s,root)?{name:s.name,bounds:s.bounds}:null);
const overflow=penpotUtils.analyzeDescendants(b,(root,s)=>s.type==='text'&&s.textBounds&&(s.textBounds.width>s.width+2||s.textBounds.height>s.height+2)?{name:s.name,actual:s.textBounds,box:s.bounds}:null);
const text=penpotUtils.findShapes(s=>s.type==='text',b).map(s=>({id:s.id,text:s.characters,font:s.fontFamily,fontSize:s.fontSize,x:s.x-b.x,y:s.y-b.y,width:s.width,height:s.height}));
const rows=D.rows.map(r=>{const row=penpot.currentPage.getShapeById(r.boardId);return {questId:r.questId,status:penpot.currentPage.getShapeById(r.statusId).characters,progressSegments:row.children.filter(s=>s.name.startsWith('Native / expedition progress ')).length};});
penpot.selection=[b];penpot.viewport.zoomIntoView([b]);return {boardId:b.id,sourceBoardId:sources.id,width:b.width,height:b.height,frames,rows,text,outside,overflow,selected:true};
'''
result=call('execute_code',{'code':code})
(folder/'PenpotVerification.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
design['sourceBoardId']=result['sourceBoardId']
(folder/'PenpotDesign.json').write_text(json.dumps(design,indent=2),encoding='utf-8')
print(json.dumps({'frames':len(result['frames']),'outside':len(result['outside']),'textOverflow':len(result['overflow']),'rows':result['rows']}))
