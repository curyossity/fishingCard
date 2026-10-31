const saved=penpot.currentPage.getPluginData('MFJ-R058');if(saved)return JSON.parse(saved);
const draft=penpotUtils.findShape(s=>s.name==='MFJ-R058 — Discovery Journal / Ornate 4K',penpot.currentPage.root);if(draft)draft.remove();
const b=penpot.createBoard();b.name='MFJ-R058 — Discovery Journal / Ornate 4K';b.resize(3840,2160);b.x=53000;b.y=34000;b.fills=[{fillColor:'#10262E',fillOpacity:1}];b.clipContent=true;
const fields=[],frames=[],icons=[],rows=[];
function frame(parent,key,x,y,name){
 const spec=GEOMETRY.find(g=>g.key===key),s=penpot.currentPage.getShapeById(ART[key]).clone();parent.appendChild(s);s.x=parent.x+x;s.y=parent.y+y;s.resize(spec.w,spec.imageDisplayHeight);s.name='Reusable frame / '+name;frames.push({id:s.id,key,x:s.x-b.x,y:s.y-b.y,sourcePixels:[spec.w,spec.imageDisplayHeight],displayPixels:[s.width,s.height],cornerScale:spec.cornerScale});return s;
}
function label(parent,value,x,y,w,h,size,body=false,color='#F1DFB7',align='left'){
 const t=penpot.createText(value);parent.appendChild(t);const f=penpot.fonts.findByName(body?'Source Serif 4':'Cinzel');if(f)f.applyToText(t,f.variants.find(v=>v.fontWeight==='400'&&v.fontStyle==='normal')||f.variants[0]);t.fontSize=String(size);t.fills=[{fillColor:color,fillOpacity:1}];t.resize(w,h);t.x=parent.x+x;t.y=parent.y+y;t.align=align;t.name='Editable / '+value.replace(/\n/g,' ');fields.push(t.id);return t;
}
function rect(parent,name,x,y,w,h,color,opacity=1){const r=penpot.createRectangle();parent.appendChild(r);r.name=name;r.resize(w,h);r.x=parent.x+x;r.y=parent.y+y;r.fills=[{fillColor:color,fillOpacity:opacity}];return r;}
function glyph(parent,name,path,x,y,size,color='#9A6A35'){
 const g=penpot.createShapeFromSvg('<svg xmlns="http://www.w3.org/2000/svg" width="100" height="100" viewBox="0 0 100 100"><path d="'+path+'" fill="none" stroke="'+color+'" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"/></svg>');parent.appendChild(g);g.resize(size,size);g.x=parent.x+x;g.y=parent.y+y;g.name='Native emblem / '+name;icons.push(g.id);return g;
}
const book='M50 23Q28 6 8 20V82Q28 73 50 89Q72 73 92 82V20Q72 6 50 23V89M18 33L39 38M18 46L39 51M61 38L82 33M61 51L82 46';
const anchor='M50 12V79M40 22A10 10 0 1 0 60 22A10 10 0 1 0 40 22M24 44H76M10 63Q12 88 50 90Q88 88 90 63M10 63L8 78M10 63L24 69M90 63L92 78M90 63L76 69';
frame(b,'perimeter',0,0,'screen perimeter');frame(b,'wordmark',156,57,'Journal wordmark');frame(b,'navigation',1171,46,'navigation');frame(b,'gold',3100,57,'gold balance');
glyph(b,'journal',book,205,88,105);label(b,'JOURNAL',330,92,680,94,64,false,'#243638');
glyph(b,'gold','M12 50A38 38 0 1 0 88 50A38 38 0 1 0 12 50M20 50A30 30 0 1 0 80 50A30 30 0 1 0 20 50M43 36L61 45L55 64L37 55Z',3150,92,95);label(b,'920',3250,90,350,95,72,false,'#243638','center');
const destinations=['75555645-d407-8067-8008-de8ff58e2639','75555645-d407-8067-8008-de9b9c9710d1','75555645-d407-8067-8008-de9d62501389',b.id];
['HARBOR','TACKLE SHOP','TECHNIQUES','JOURNAL'].forEach((s,i)=>{const t=label(b,s,1332+i*360,110,345,68,37,false,i===3?'#F1DFB7':'#C5AE89','center');if(i<3)t.addInteraction('click',{type:'navigate-to',destination:penpot.currentPage.getShapeById(destinations[i]),preserveScrollPosition:false});});
rect(b,'Native / active Journal underline',2460,183,200,3,'#B88D52');
frame(b,'sidebar',150,300,'field journal cabinet');frame(b,'illustration',190,340,'illustration matte');
const art=penpot.currentPage.getShapeById(ART.illustrationArt).clone();b.appendChild(art);art.name='Independent illustration / naturalist journal at harbor';art.resize(810,1080);art.x=b.x+232.5;art.y=b.y+365;
label(b,'THE FIELD JOURNAL',240,1510,790,70,48,false,'#E0BC80','center');
label(b,'Every expedition\nleaves a story.',255,1607,760,150,54,true,'#F1DFB7','center');
label(b,'Discover the sea.\nRecord what you bring home.',260,1745,750,85,33,true,'#C5AE89','center');
const states=['INCOMPLETE','COMPLETE','1 / 3 SUCCESSFUL EXPEDITIONS'];
QUESTS.forEach((q,i)=>{
 const y=300+i*540;
 const row=penpot.createBoard();b.appendChild(row);row.name='Reusable quest row / '+q.id;row.resize(2480,500);row.x=b.x+1200;row.y=b.y+y;row.fills=[];row.clipContent=true;
 frame(row,'quest',0,0,'quest body');frame(row,'questTitle',20,18,'quest title');
 label(row,'0'+(i+1),75,43,105,72,48,false,'#9A6A35');label(row,q.title.toUpperCase(),210,38,2150,85,57,false,'#243638');
 label(row,'OBJECTIVE',90,167,1010,50,28,false,'#D8B57F');
 const requirements=['Surface with 4 Schooling creatures\nattached in one expedition.','Surface with creatures representing\nat least 5 distinct tags.','Successfully surface while overloaded\nin 3 separate expeditions.'];
 label(row,requirements[i],90,230,1010,130,43,true);
 rect(row,'Native / objective reward divider',1160,170,2,240,'#B88D52',.48);
 label(row,'REWARD · NOT YET GRANTED',1240,167,1120,50,28,false,'#D8B57F');
 const rewards=["Unlock Schoolmaster’s Charm for purchase:\nyour first Schooling Catch each expedition\nhas −1 weight.",'Unlock a new special Event card.',"Unlock Salvager’s Kit for purchase:\nonce per expedition, release one attached\ncreature and draw 1 Technique."];
 label(row,rewards[i],1240,230,1120,180,39,true);
 const status=label(row,states[i],i===1?330:280,394,850,54,31,false,i===1?'#C4D9C4':'#E0BC80');
 if(i===1)glyph(row,'completed quest','M15 52L39 76L86 22',260,393,50,'#C4D9C4');
 if(i===2){for(let n=0;n<3;n++){const r=rect(row,'Native / expedition progress '+(n+1),280+n*260,455,240,6,n===0?'#D8B57F':'#728080');r.opacity=n===0?1:.4;}}
 rows.push({questId:q.id,boardId:row.id,status:states[i],statusId:status.id,partialCounter:i===2,bar:i===2});
});
frame(b,'footer',150,1920,'Journal footer');frame(b,'return',2670,1920,'return to Harbor');
glyph(b,'footer journal',book,220,1960,100);label(b,'PROGRESS IS RECORDED',375,1950,2050,55,32,false,'#E0BC80');label(b,'Rewards are listed for reference; nothing is granted yet.',375,2010,2080,58,37,true);
glyph(b,'return anchor',anchor,2725,1960,100);const back=label(b,'BACK TO HARBOR',2860,1980,730,75,48,false,'#243638','center');back.addInteraction('click',{type:'navigate-to',destination:penpot.currentPage.getShapeById(destinations[0]),preserveScrollPosition:false});
const result={revision:'MFJ-R058',fileId:penpot.currentFile.id,pageId:penpot.currentPage.id,pageName:penpot.currentPage.name,boardId:b.id,boardName:b.name,width:3840,height:2160,frames,editableTextFields:fields,icons,rows,illustrationId:art.id,unityImported:false,exampleStates:true};
penpot.currentPage.setPluginData('MFJ-R058',JSON.stringify(result));penpot.selection=[b];penpot.viewport.zoomIntoView([b]);return result;
