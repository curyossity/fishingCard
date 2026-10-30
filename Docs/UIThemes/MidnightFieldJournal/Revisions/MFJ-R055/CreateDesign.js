const existing=penpot.currentPage.getPluginData('MFJ-R055');
if(existing)return JSON.parse(existing);
const b=penpot.currentPage.getShapeById('75555645-d407-8067-8008-de9192c78c73').clone();
b.name='MFJ-R055 — Tackle Shop / Engraved equipment';b.x=52700;b.y=34000;
const names={rod:'Driftwood Rod',reel:'Tideglass Reel',hook:'Barbed Hook'};
const sx=3840/1672,sy=2160/941;
const old=b.children.filter(s=>s.name.startsWith('Independent equipment artwork / '));
const newArt=[];
for(const s of old){
 const name=s.name; s.remove();
 const key=name.includes('Driftwood')?'rod':name.includes('Barbed')?'hook':'reel';
 const detail=name.includes('selected detail');
 const x=detail?1170:key==='rod'?111:key==='reel'?446:781;
 const y=detail?367:358,w=detail?360:254,h=detail?160:170;
 const art=penpot.currentPage.getShapeById(ART[key]).clone();b.appendChild(art);
 const dimensions=DIMENSIONS[key];
 const scale=Math.min(w*sx/dimensions[0],h*sy/dimensions[1]);
 art.resize(dimensions[0]*scale,dimensions[1]*scale);
 art.x=b.x+x*sx+(w*sx-art.width)/2;art.y=b.y+y*sy+(h*sy-art.height)/2;
 art.name='Independent engraved equipment / '+names[key]+(detail?' / selected detail':'');
 newArt.push({key,shapeId:art.id,detail});
}
const state={revision:'MFJ-R055',fileId:penpot.currentFile.id,pageId:penpot.currentPage.id,pageName:penpot.currentPage.name,boardId:b.id,boardName:b.name,width:b.width,height:b.height,equipmentArtwork:newArt,editableTextFields:b.children.filter(s=>s.name.startsWith('Editable / ')).map(s=>s.id),sourceArtwork:ART,unityImported:false};
penpot.currentPage.setPluginData('MFJ-R055',JSON.stringify(state));
penpot.selection=[b];penpot.viewport.zoomIntoView([b]);return state;
