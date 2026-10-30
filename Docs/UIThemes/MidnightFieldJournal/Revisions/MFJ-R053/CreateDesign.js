const saved=penpot.currentPage.getPluginData('MFJ-R053');
if(saved) return JSON.parse(saved);
const b=penpot.createBoard();
b.name='MFJ-R053 — Harbor / Ornate card-style study';
b.resize(3840,2160);b.x=48600;b.y=31600;b.fills=[];b.clipContent=true;
const art=penpot.currentPage.getShapeById('75555645-d407-8067-8008-de8fd4e31436');
b.appendChild(art);art.resize(3840,2160);art.x=b.x;art.y=b.y;
art.name='Original ornate Harbor artwork — static review composition';
const sx=3840/1672,sy=2160/941;
const fields=[];
function text(value,x,y,w,h,size,serif=false,color='#F1DFB7',align='center') {
 const t=penpot.createText(value);b.appendChild(t);
 const f=penpot.fonts.findByName(serif?'Source Serif 4':'Cinzel');
 if(f) f.applyToText(t,f.variants.find(v=>v.fontWeight==='400'&&v.fontStyle==='normal')||f.variants[0]);
 t.fontSize=String(size*sx);t.fills=[{fillColor:color,fillOpacity:1}];
 t.resize(w*sx,h*sy);t.x=b.x+x*sx;t.y=b.y+y*sy;t.align=align;
 t.name='Editable / '+value.replace(/\n/g,' ');fields.push(t.id);return t;
}
text('THE HARBOR',178,45,280,42,27,false,'#243638');
text('920',1415,42,140,45,32,false,'#243638');
['HARBOR','TACKLE SHOP','TECHNIQUES','JOURNAL'].forEach((s,i)=>text(s,580+i*157,49,150,35,16,false,i===0?'#F1DFB7':'#C5AE89'));
text('A quiet shore. A new beginning.',505,140,660,48,25,true);
const panels=[
 {x:62,title:'TACKLE SHOP',tag:'Choose your edge.',body:'Unlock equipment.\nEquip one defining item.',cta:'ENTER WORKSHOP  →'},
 {x:588,title:'TECHNIQUE COLLECTION',tag:'Build your next hand.',body:'Collect Techniques.\nPrepare your starting deck.',cta:'OPEN COLLECTION  →'},
 {x:1102,title:'DISCOVERY JOURNAL',tag:'Leave no story untold.',body:'Complete challenges.\nDiscover new possibilities.',cta:'TURN THE PAGE  →'}
];
for(const p of panels){
 text(p.title,p.x+80,550,403,39,p.title.length>18?21:24,false,'#243638');
 text(p.tag,p.x+63,613,385,31,21,true);
 text(p.body,p.x+61,648,385,57,19,true);
 text(p.cta,p.x+78,711,350,26,14,false,'#E3BD79');
}
text('READY FOR THE WATER',181,798,800,24,14,false,'#E0BC80','left');
text('Driftwood Rod · 1 item equipped   /   Starting deck 12 / 12',181,825,835,38,20,true,'#F1DFB7','left');
text('SET SAIL',1273,807,275,54,31,false,'#243638');
const state={revision:'MFJ-R053',fileId:penpot.currentFile.id,pageId:penpot.currentPage.id,pageName:penpot.currentPage.name,boardId:b.id,boardName:b.name,width:b.width,height:b.height,editableTextFields:fields,artId:art.id,unityImported:false};
penpot.currentPage.setPluginData('MFJ-R053',JSON.stringify(state));
penpot.selection=[b];penpot.viewport.zoomIntoView([b]);return state;
