const p=penpot.currentPage,ns='fishing-cards-design',rev='MFJ-R061';
if(p.id!=='bae680cd-9225-8018-8008-cddd6a6f3825')throw Error('Open Current Design Library');
if(p.getSharedPluginData(ns,rev))throw Error('Revision already created; inspect before retry');
const ink='#283B3D',navy='#111E25',ivory='#EEDCB9',muted='#C7B899',gold='#B78C55',green='#B9D1B4';
const masters={Paper:p.getShapeById('75555645-d407-8067-8008-dea9cfcb5568'),Dark:p.getShapeById('75555645-d407-8067-8008-dea9cd9a2881')};
const art={rod:p.getShapeById('75555645-d407-8067-8008-de9b972ff90c'),reel:p.getShapeById('75555645-d407-8067-8008-de9b99677d11'),hook:p.getShapeById('75555645-d407-8067-8008-de9b9b3a9112'),workshop:p.getShapeById("f31e67ca-185b-80af-8008-e02d267e1b63")};
if(Object.values(masters).some(s=>!s)||Object.values(art).some(s=>!s))throw Error('Missing independent source');
function board(name,x,y,w,h,parent){const b=penpot.createBoard();b.name=name;b.resize(w,h);b.x=x;b.y=y;b.fills=[];b.clipContent=false;if(parent)parent.appendChild(b);return b;}
function text(parent,name,copy,x,y,w,h,size,color=ivory,body=false,align='left'){
 const t=penpot.createText(copy);penpot.fonts.findByName(body?'Source Serif 4':'Cinzel').applyToText(t);t.name=name;t.fontSize=String(size);t.fontWeight='400';t.lineHeight='1.14';t.align=align;t.growType='fixed';t.resize(w,h);t.x=x;t.y=y;t.fills=[{fillColor:color,fillOpacity:1}];parent.appendChild(t);return t;
}
function rect(parent,name,x,y,w,h,color,opacity=1){const r=penpot.createRectangle();r.name=name;r.resize(w,h);r.x=x;r.y=y;r.fills=[{fillColor:color,fillOpacity:opacity}];parent.appendChild(r);return r;}
function frame(parent,name,key,x,y,w,h,corner=90,outer=false){
 const source=masters[key],b=board(name,x,y,w,h,parent);const cut=300,c=Math.min(corner,w/3,h/2-2),ux=[0,cut,source.width-cut,source.width],uy=[0,cut,source.height-cut,source.height],dx=[0,c,w-c,w],dy=[0,c,h-c,h];
 for(let row=0;row<3;row++)for(let col=0;col<3;col++){
  if(outer&&row===1&&col===1)continue;
  const clip=board('Fixed-corner slice / '+row+','+col,x+dx[col],y+dy[row],dx[col+1]-dx[col],dy[row+1]-dy[row],b);clip.clipContent=true;
  const image=source.clone();clip.appendChild(image);image.name='Independent '+key+' master / no mockup slicing';
  const sx=clip.width/(ux[col+1]-ux[col]),sy=clip.height/(uy[row+1]-uy[row]);image.resize(source.width*sx,source.height*sy);image.x=clip.x-ux[col]*sx;image.y=clip.y-uy[row]*sy;
 }
 return b;
}
function picture(parent,key,x,y,w,h){const src=art[key],s=src.clone(),k=Math.min(w/src.width,h/src.height);parent.appendChild(s);s.name='Independent illustration / '+key;s.resize(src.width*k,src.height*k);s.x=x+(w-s.width)/2;s.y=y+(h-s.height)/2;return s;}
function button(parent,name,copy,x,y,w,h,light=true){const b=frame(parent,name,light?'Paper':'Dark',x,y,w,h,Math.min(42,h*.42));text(b,'Editable control label / '+copy,copy,x+48,y+24,w-96,h-36,31,light?ink:ivory,false,'center');return b;}
const specs=[{key:'rod',type:'ROD',name:'Driftwood Rod',price:200,effect:'Start each expedition with +1 Line Capacity.',short:'+1 Line Capacity at expedition start.'},{key:'reel',type:'REEL',name:'Tideglass Reel',price:200,effect:'Draw 2 additional Technique cards at the start of each expedition.',short:'Start each expedition with 2 extra Techniques.'},{key:'hook',type:'HOOK',name:'Barbed Hook',price:250,effect:'Your first 3 creatures caught each expedition gain +1 sale value each while attached.',short:'Your first 3 Catches gain +1 sale value each.'}];
const screens=[];
function screen(state,x){
 const y=0,b=board('MFJ-R061 / Tackle Shop / '+state,x,y,3840,2160,p.getShapeById('92f68b17-9997-80de-8008-dfc1d6a06a0a'));b.fills=[{fillColor:navy,fillOpacity:1}];
 frame(b,'Reusable / outer coastal border','Dark',x,0,3840,2160,150,true);
 frame(b,'Reusable / shop wordmark','Paper',x+140,55,960,170,66);
 text(b,'Editable / screen title','TACKLE SHOP',x+235,104,770,85,65,ink,false,'center');
 frame(b,'Reusable / Harbor navigation','Dark',x+1140,55,1920,170,65);
 const navigation=[];for(const [i,name] of ['HARBOR','TACKLE SHOP','TECHNIQUES','JOURNAL'].entries()){
  const t=text(b,'Navigation / '+name,name,x+1185+i*458,113,450,55,34,ivory,false,'center');navigation.push({name,id:t.id});
  if(i===1)rect(b,'Active destination / underline',x+1705,188,320,3,gold);
 }
 frame(b,'Reusable / gold balance','Paper',x+3100,55,600,170,65);
 const balance=state==='Before purchase'?650:400;
 text(b,'Dynamic / gold balance',balance+' GOLD',x+3160,100,480,90,60,ink,false,'center');
 text(b,'Heading / prepare your rig','PREPARE YOUR RIG',x+170,300,1900,90,67);
 text(b,'Three category promise','One Rod. One Reel. One Hook.',x+174,418,1840,70,47,ivory,true);
 text(b,'Loadout instructions','Unlock equipment with Gold, then equip one item in each slot.\nAll equipped items are ready for your next expedition.',x+177,520,1850,110,34,muted,true);
 frame(b,'Loadout count plaque','Paper',x+170,667,590,95,38);
 text(b,'Dynamic / equipped count',state==='Hook equipped'?'3 / 3 EQUIPPED':'2 / 3 EQUIPPED',x+220,692,490,55,35,ink,false,'center');
 frame(b,'Reusable / workshop vignette surround','Dark',x+2180,258,1530,535,95);
 picture(b,'workshop',x+2225,287,1440,480);
 frame(b,'Reusable / equipped rig cabinet','Dark',x+140,820,1060,1160,100);
 frame(b,'Reusable / loadout title plaque','Paper',x+165,840,1010,115,45);
 text(b,'Section / your equipment','YOUR EQUIPMENT',x+250,870,840,65,45,ink,false,'center');
 const slotIds=[];
 for(let i=0;i<3;i++){
  const item=specs[i],sy=990+i*290,equipped=i<2||state==='Hook equipped';
  const slot=frame(b,'Equipment slot / '+item.type,equipped?'Paper':'Dark',x+185,sy,970,265,65);slotIds.push(slot.id);
  text(slot,'Slot category / '+item.type,item.type,x+235,sy+28,170,45,30,equipped?'#82613C':gold);
  if(equipped){
   picture(slot,item.key,x+225,sy+81,220,140);
   text(slot,'Equipped name / '+item.key,item.name,x+475,sy+34,610,60,35,ink);
   text(slot,'Equipped effect / '+item.key,item.short,x+475,sy+105,590,93,29,ink,true);
   text(slot,'State / equipped','EQUIPPED',x+475,sy+205,300,36,23,'#4F684D');
   const remove=text(slot,'Action / unequip '+item.key,'UNEQUIP',x+930,sy+205,170,36,23,'#765333',false,'right');
   if(i===2)b.setPluginData('hookUnequip',remove.id);
  }else{
   text(slot,'Empty slot heading','NO HOOK EQUIPPED',x+450,sy+42,620,60,35);
   picture(slot,'hook',x+245,sy+80,150,145);
   text(slot,'Empty slot guidance',state==='Before purchase'?'Unlock a Hook from the catalog,\nthen equip it here.':'Your Barbed Hook is unlocked.\nEquip it from the catalog.',x+450,sy+113,620,94,29,muted,true);
  }
 }
 text(b,'Slot limit guidance','Slots may be left empty. One item per category.',x+225,1882,910,55,26,muted,true);
 frame(b,'Reusable / equipment catalog','Dark',x+1240,820,2460,1160,100);
 text(b,'Section / equipment catalog','EQUIPMENT CATALOG',x+1310,868,1620,68,48);
 text(b,'Catalog count','3 ITEMS',x+3310,882,290,48,27,muted,false,'right');
 const filters=[];for(const [i,label] of ['ALL','RODS','REELS','HOOKS'].entries()){
  const tab=button(b,'Catalog filter / '+label,label,x+1300+i*360,956,320,86,i===0);filters.push(tab.id);
 }
 text(b,'Catalog ownership hint','Permanent unlocks · Gold only',x+2800,980,795,44,28,muted,true,'right');
 const catalog=[];
 for(let i=0;i<3;i++){
  const item=specs[i],cx=x+1300+i*790,cy=1075;
  const card=frame(b,'Catalog card / '+item.name,'Dark',cx,cy,740,682,68);
  frame(card,'Reusable / '+item.key+' illustration plate','Paper',cx+9,cy+9,722,330,68);
  picture(card,item.key,cx+50,cy+40,640,268);
  text(card,'Category / '+item.key,item.type,cx+65,cy+360,600,38,26,gold);
  text(card,'Name / '+item.key,item.name,cx+65,cy+410,610,60,37);
  text(card,'Effect / '+item.key,item.effect,cx+65,cy+492,610,118,29,ivory,true);
  const caption=i<2||state==='Hook equipped'?'EQUIPPED':state==='Before purchase'?'AVAILABLE · 250 GOLD':'OWNED · NOT EQUIPPED';
  text(card,'Ownership / '+item.key,caption,cx+65,cy+625,610,40,25,i<2||state==='Hook equipped'?green:gold);
  if(i===2)rect(card,'Selected item / amber underline',cx+65,cy+474,610,3,gold);
  catalog.push({item:item.key,id:card.id,state:caption});
 }
 frame(b,'Reusable / selected Hook action strip','Dark',x+1300,1790,2320,160,60);
 text(b,'Selected item / name','BARBED HOOK',x+1355,1813,1350,52,35);
 const selectedStatus=state==='Before purchase'?'Permanent unlock. Equip it in your Hook slot.':state==='Hook owned'?'Owned. Your Hook slot is ready.':'Equipped in your Hook slot.';
 text(b,'Selected item / status',selectedStatus,x+1355,1875,1440,43,29,muted,true);
 const action=button(b,'Primary action / Hook',state==='Before purchase'?'UNLOCK · 250 GOLD':state==='Hook owned'?'EQUIP HOOK':'UNEQUIP HOOK',x+2940,1818,620,105);
 text(b,'Footer / permanent ownership','Purchased equipment stays in your collection between expeditions.',x+180,2045,2400,60,31,muted,true);
 const back=button(b,'Navigation / Back to Harbor','BACK TO HARBOR',x+2910,2025,790,100);
 const result={state,id:b.id,width:3840,height:2160,balance,equipped:state==='Hook equipped'?3:2,slots:slotIds,catalog,filters,primaryActionId:action.id,backId:back.id,navigation,hookUnequipId:b.getPluginData('hookUnequip')||null};screens.push(result);return result;
}
const initial=screen('Before purchase',80000),owned=screen('Hook owned',84200),equipped=screen('Hook equipped',88400);
const link=(id,destination)=>p.getShapeById(id).addInteraction('click',{type:'navigate-to',destination:p.getShapeById(destination)});
link(initial.primaryActionId,owned.id);link(owned.primaryActionId,equipped.id);link(equipped.primaryActionId,owned.id);if(equipped.hookUnequipId)link(equipped.hookUnequipId,owned.id);
const targets={HARBOR:'75555645-d407-8067-8008-de8ff58e2639',TECHNIQUES:'75555645-d407-8067-8008-de9d62501389',JOURNAL:'75555645-d407-8067-8008-dedef8b3f171'};
for(const s of screens){link(s.backId,targets.HARBOR);for(const n of s.navigation)if(targets[n.name])link(n.id,targets[n.name]);}
const source=board('MFJ-R061 / Independent workshop illustration',80000,2700,2372,974,p.getShapeById('92f68b17-9997-80de-8008-dfc1d70ae173'));
source.appendChild(art.workshop);art.workshop.name='New independent engraved workshop panorama / no UI';art.workshop.resize(2172,724);art.workshop.x=80100;art.workshop.y=2800;
const design={revision:rev,fileId:penpot.currentFile.id,pageId:p.id,pageName:p.name,screens,sourceId:source.id,workshopArtId:art.workshop.id,frameMasterIds:Object.fromEntries(Object.entries(masters).map(([k,s])=>[k,s.id])),itemArtIds:Object.fromEntries(['rod','reel','hook'].map(k=>[k,art[k].id])),status:'ready_for_review',unityImported:false,mechanic:'Up to three equipped items, at most one Rod, one Reel and one Hook. Empty slots allowed.',prototype:'Illustrative Hook unlock → equip → unequip. Gold values are mockup state, not saved progression.',unchanged:'Existing Unity shop and prior Penpot source preserved; no gameplay implementation.',reusedSource:'Existing approved engraved equipment illustrations and independent paper/navy frame masters; no screen-image slicing.',frameConstruction:'Native editable fixed-corner 9-slice boards; 300px source corners displayed at 25–150px, never enlarged.'};
p.setSharedPluginData(ns,rev,JSON.stringify(design));for(const s of screens)p.getShapeById(s.id).setSharedPluginData(ns,rev,JSON.stringify(design));storage.r061=design;
penpot.selection=[p.getShapeById(initial.id)];penpot.viewport.zoomIntoView(penpot.selection);return design;
