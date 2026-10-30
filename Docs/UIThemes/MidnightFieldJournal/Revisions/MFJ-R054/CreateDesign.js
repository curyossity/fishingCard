const saved=penpot.currentPage.getPluginData('MFJ-R054');
if(saved)return JSON.parse(saved);
const b=penpot.createBoard();b.name='MFJ-R054 — Tackle Shop / Ornate card-style study';
b.resize(3840,2160);b.x=52700;b.y=31600;b.fills=[];b.clipContent=true;
const a=penpot.currentPage.getShapeById(ART);
b.appendChild(a);a.resize(3840,2160);a.x=b.x;a.y=b.y;a.name='Static / newly illustrated workshop and ornate review framing';
const sx=3840/1672,sy=2160/941,fields=[],pieces=[];
function text(value,x,y,w,h,size,serif=false,color='#F1DFB7',align='center',parent=b){
 const t=penpot.createText(value);parent.appendChild(t);
 const font=penpot.fonts.findByName(serif?'Source Serif 4':'Cinzel');
 if(font)font.applyToText(t,font.variants.find(v=>v.fontWeight==='400'&&v.fontStyle==='normal')||font.variants[0]);
 t.fontSize=String(size*sx);t.fills=[{fillColor:color,fillOpacity:1}];t.resize(w*sx,h*sy);
 t.x=b.x+x*sx;t.y=b.y+y*sy;t.align=align;t.name='Editable / '+value.replace(/\n/g,' ');fields.push(t.id);return t;
}
function gear(id,name,x,y,w,h){
 const s=penpot.currentPage.getShapeById(id).clone();b.appendChild(s);s.name='Independent equipment artwork / '+name;
 const factor=Math.min(w*sx/s.width,h*sy/s.height);s.resize(s.width*factor,s.height*factor);s.x=b.x+x*sx+(w*sx-s.width)/2;s.y=b.y+y*sy+(h*sy-s.height)/2;pieces.push(s.id);return s;
}
text('TACKLE SHOP',178,45,280,42,27,false,'#243638');text('920',1415,42,140,45,32,false,'#243638');
['HARBOR','TACKLE SHOP','TECHNIQUES','JOURNAL'].forEach((s,i)=>text(s,580+i*157,49,150,35,16,false,i===1?'#F1DFB7':'#C5AE89'));
['ALL EQUIPMENT','RODS','REELS','HOOKS'].forEach((s,i)=>text(s,95+i*242,310,230,35,18,false,'#243638'));
const items=[
 ['Driftwood Rod','75555645-d407-8067-8008-de8860eb1e21','EQUIPPED','In your collection'],
 ['Tideglass Reel','d76f6cbf-1fc5-8066-8008-de81e6709a76','AVAILABLE','180 GOLD'],
 ['Barbed Hook','75555645-d407-8067-8008-de8862e7702f','JOURNAL LOCKED','Complete a discovery']
];
items.forEach(([name,id,status,note],i)=>{
 const x=85+335*i;
 gear(id,name,x+26,358,254,170);
 text(name.toUpperCase(),x+14,554,282,41,20,false,'#243638');
 text(status,x+25,615,260,32,16,false,status==='EQUIPPED'?'#BFD7C0':'#E3BD79');
 text(note,x+20,648,270,40,19,true);
});
text('“The right tool makes a different fisher.”',115,753,915,42,23,true);
text('REEL / PERMANENT UNLOCK',1140,296,440,25,13,false,'#725536');
text('TIDEGLASS REEL',1140,321,440,33,23,false,'#243638');
gear(items[1][1],'Tideglass Reel / selected detail',1170,367,360,160);
text('ONE EQUIPMENT SLOT',1154,570,410,34,18,false);
text('Unlock with gold. Equip one item\nfor your next expedition.',1153,630,410,49,18,true);
text('Unlocking does not replace your loadout.',1165,697,390,28,15,true);
text('UNLOCK · 180 GOLD',1190,752,342,35,22,false,'#243638');
text('CURRENT LOADOUT · 1 / 1',225,838,850,25,14,false,'#E0BC80','left');
text('Driftwood Rod equipped',225,864,850,34,22,true,'#F1DFB7','left');
const back=text('BACK TO HARBOR',1235,845,315,40,23,false,'#243638');
back.addInteraction('click',{type:'navigate-to',destination:penpot.currentPage.getShapeById('75555645-d407-8067-8008-de8ff58e2639'),preserveScrollPosition:false});
const state={revision:'MFJ-R054',fileId:penpot.currentFile.id,pageId:penpot.currentPage.id,pageName:penpot.currentPage.name,boardId:b.id,boardName:b.name,width:b.width,height:b.height,editableTextFields:fields,independentEquipmentArtwork:pieces,artId:a.id,unityImported:false};
penpot.currentPage.setPluginData('MFJ-R054',JSON.stringify(state));penpot.selection=[b];penpot.viewport.zoomIntoView([b]);return state;
