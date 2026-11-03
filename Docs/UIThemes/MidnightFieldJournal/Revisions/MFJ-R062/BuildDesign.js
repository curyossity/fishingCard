const p=penpot.currentPage,rev='MFJ-R062',ns='fishing-cards-design';
if(p.getSharedPluginData(ns,rev))throw Error('R062 already exists; inspect before retry');
if(p.id!=='bae680cd-9225-8018-8008-cddd6a6f3825')throw Error('Open Current Design Library');
const X=80000,Y=7000,C={paper:'#EAD6B3',ink:'#29383A',gold:'#CBA16A',muted:'#B9B7AB',ivory:'#EFE2C5',green:'#B6CBA9',navy:'#101B22'};
const refs={background:'bae680cd-9225-8018-8008-d00618505dcc',kelp:'bae680cd-9225-8018-8008-d00618505dcd',compact:'bae680cd-9225-8018-8008-d00618505ded',card:'f31e67ca-185b-80af-8008-e036aa77cd26',rod:'75555645-d407-8067-8008-de9b972ff90c',reel:'75555645-d407-8067-8008-de9b99677d11',hook:'75555645-d407-8067-8008-de9b9b3a9112'};
function board(name,x,y,w,h,parent){const b=penpot.createBoard();b.name=name;b.resize(w,h);b.x=x;b.y=y;b.fills=[];b.clipContent=false;if(parent)parent.appendChild(b);return b;}
const b=board('MFJ-R062 / Tackle Shop / Equipment Card Table',X,Y,3840,2160,p.getShapeById('92f68b17-9997-80de-8008-dfc1d6a06a0a'));
b.fills=[{fillColor:C.navy,fillOpacity:1}];
function picture(parent,key,x,y,w,h,fit=false){const src=p.getShapeById(refs[key]);if(!src)throw Error('Missing '+key);const s=src.clone();parent.appendChild(s);s.name='Independent in-game source / '+key;const k=Math.min(w/src.width,h/src.height);s.resize(fit?src.width*k:w,fit?src.height*k:h);s.x=X+x+(w-s.width)/2;s.y=Y+y+(h-s.height)/2;return s;}
function txt(parent,name,copy,x,y,w,h,size=36,color=C.ivory,body=false,align='left'){const t=penpot.createText(copy);penpot.fonts.findByName(body?'Source Serif 4':'Cinzel').applyToText(t);t.name=name;t.fontSize=String(size);t.fontWeight='400';t.lineHeight='1.16';t.align=align;t.growType='fixed';t.resize(w,h);t.x=X+x;t.y=Y+y;t.fills=[{fillColor:color,fillOpacity:1}];parent.appendChild(t);return t;}
function svg(parent,name,markup,x,y,w,h){const s=penpot.createShapeFromSvg(`<svg xmlns="http://www.w3.org/2000/svg" width="${w}" height="${h}" viewBox="0 0 ${w} ${h}">${markup}</svg>`);s.name=name;parent.appendChild(s);s.x=X+x;s.y=Y+y;return s;}
function divider(x,y,w){return svg(b,'Native engraved rule',`<path d="M0 8 H${w/2-15} M${w/2+15} 8 H${w}" stroke="${C.gold}" stroke-width="2"/><path d="M${w/2} 0 L${w/2+8} 8 L${w/2} 16 L${w/2-8} 8Z" fill="${C.gold}"/>`,x,y,w,16);}
function action(parent,name,label,x,y,w,enabled=true){const a=board(name,X+x,Y+y,w,100,parent);svg(a,'Reusable native parchment action',`<rect x="2" y="2" width="${w-4}" height="96" rx="14" fill="${enabled?C.paper:'#18282D'}" stroke="${C.gold}" stroke-width="2"/><path d="M28 50 L38 40 L48 50 L38 60Z M${w-28} 50 L${w-38} 40 L${w-48} 50 L${w-38} 60Z" fill="none" stroke="${enabled?'#805737':C.gold}" stroke-width="2"/><path d="M68 13 H${w-68} M68 87 H${w-68}" stroke="${enabled?'#B38B5B':'#705B40'}" stroke-width="1"/>`,x,y,w,100);txt(a,'Editable action / '+label,label,x+65,y+28,w-130,54,30,enabled?C.ink:C.green,false,'center');return a;}
picture(b,'background',0,0,3840,2160);picture(b,'kelp',0,0,3840,2160);
// Minimal HUD rather than a shop building illustration or framed cabinet.
txt(b,'Navigation / Harbor','‹  HARBOR',140,93,520,65,38,C.ivory);
txt(b,'Title','TACKLE SHOP',1100,65,1640,115,86,C.ivory,false,'center');
txt(b,'Subtitle','Choose the equipment you take to sea.',1130,189,1580,65,37,C.muted,true,'center');
txt(b,'Dynamic / Gold','650 GOLD',3010,85,660,85,58,C.ivory,false,'right');
divider(3050,177,600);
txt(b,'Section / expedition rig','YOUR EXPEDITION RIG',140,328,400,100,31,C.gold);
txt(b,'Loadout rule','One of each.\nEmpty slots are welcome.',140,447,370,120,31,C.muted,true);
txt(b,'Dynamic / equipped count','1 / 3\nEQUIPPED',3260,407,420,145,42,C.ivory,false,'center');
const slots=[];
for(const [i,item] of [{key:'rod',type:'ROD',name:'Driftwood Rod',equipped:true},{key:'reel',type:'REEL',name:'Choose a Reel',equipped:false},{key:'hook',type:'HOOK',name:'Choose a Hook',equipped:false}].entries()){
 const x=570+i*880,y=315,slot=board('Rig slot / '+item.type,X+x,Y+y,820,410,b);
 picture(slot,'compact',x,y,820,410);
 txt(slot,'Slot category / '+item.type,item.type,x+70,y+33,680,48,29,C.ink,false,'center');
 if(item.equipped){picture(slot,item.key,x+270,y+94,280,160,true);txt(slot,'Slot identity / '+item.key,item.name,x+70,y+257,680,50,34,C.ink,false,'center');}
 else{svg(slot,'Native empty '+item.type+' socket',`<circle cx="52" cy="52" r="49" fill="none" stroke="#9B7F5A" stroke-width="2" stroke-dasharray="5 9"/><path d="M52 30 V74 M30 52 H74" stroke="#7E694D" stroke-width="2"/>`,x+358,y+110,104,104);txt(slot,'Empty slot / '+item.key,item.name,x+70,y+257,680,50,34,C.ink,false,'center');}
 txt(slot,'Slot state / '+item.key,item.equipped?'EQUIPPED':'EMPTY',x+80,y+333,660,42,25,item.equipped?'#435A43':'#776752',false,'center');
 slots.push({id:slot.id,category:item.type,equipped:item.equipped});
}
divider(550,751,2740);
txt(b,'Catalog / title','EQUIPMENT COLLECTION',270,800,1100,55,38,C.ivory);
const tabs=[];for(const [i,label] of ['ALL','RODS','REELS','HOOKS'].entries()){
 const t=txt(b,'Catalog filter / '+label,label,1760+i*340,804,300,50,31,i===0?C.gold:C.muted,false,'center');tabs.push(t.id);
 if(i===0)svg(b,'Active collection filter',`<path d="M0 1H160" stroke="${C.gold}" stroke-width="2"/>`,1830,862,160,3);
}
txt(b,'Catalog count','3 ITEMS',3220,806,410,50,29,C.muted,false,'right');
const items=[{key:'rod',type:'ROD',name:'DRIFTWOOD ROD',effect:'Start each expedition with\n+1 Line Capacity.',state:'EQUIPPED',action:'EQUIPPED',enabled:false,x:350,y:928},{key:'reel',type:'REEL',name:'TIDEGLASS REEL',effect:'Draw 2 additional Technique cards\nat the start of each expedition.',state:'OWNED · NOT EQUIPPED',action:'EQUIP REEL',enabled:true,x:1470,y:888},{key:'hook',type:'HOOK',name:'BARBED HOOK',effect:'Your first 3 creatures caught each\nexpedition gain +1 sale value each\nwhile attached.',state:'AVAILABLE · 250 GOLD',action:'UNLOCK · 250 GOLD',enabled:true,x:2590,y:928}];
const cards=[];
for(const item of items){const {x,y}=item,card=board('Equipment card / '+item.key,X+x,Y+y,900,1000,b);
 picture(card,'card',x,y,900,1000);
 txt(card,'Category / '+item.key,item.type,x+120,y+59,660,40,26,C.ink,false,'center');
 txt(card,'Name / '+item.key,item.name,x+90,y+115,720,65,45,C.ink,false,'center');
 picture(card,item.key,x+190,y+205,520,300,true);
 txt(card,'Ownership / '+item.key,item.state,x+112,y+568,676,42,27,C.ink,false,'center');
 txt(card,'Effect label / '+item.key,'EXPEDITION EFFECT',x+150,y+677,600,44,26,C.gold,false,'center');
 txt(card,'Effect / '+item.key,item.effect,x+170,y+754,560,160,34,C.ivory,true,'center');
 const button=action(b,'Card action / '+item.key,item.action,x+125,y+1010,650,item.enabled);
 cards.push({id:card.id,item:item.key,actionId:button.id,ownership:item.state});
}
txt(b,'Selection indicator','SELECTED',1650,845,540,42,24,C.gold,false,'center');
txt(b,'Footer / equipment promise','Permanent unlocks. A different way to fish.',1140,2090,1560,52,31,C.muted,true,'center');
const d={revision:rev,fileId:penpot.currentFile.id,pageId:p.id,pageName:p.name,reviewId:b.id,position:[X,Y],size:[3840,2160],slots,cards,filters:tabs,sourceIds:refs,status:'ready_for_review',unityImported:false,concept:'Equipment Card Table: compact loadout ribbon above three large collectible equipment cards. A selected central Reel is raised subtly; purchase/equip controls sit beneath each card.',background:'Existing in-game 4K dark paper and kelp only. No shop scene, panorama, workshop, full-screen shop image or R061 frame master.',prototype:'Static editable design review; category filters and equip/purchase actions are visual specifications, not functional.',mechanic:'Up to three simultaneous items: one Rod, one Reel, one Hook. Empty slots allowed. Existing ownership, prices and effects retained.',preserved:['MFJ-R061 and all earlier shop designs','Unity assets, scenes, behavior and saves']};
txt(b,'Design-only metadata marker','',0,0,1,1,1);
p.setSharedPluginData(ns,rev,JSON.stringify(d));b.setSharedPluginData(ns,rev,JSON.stringify(d));storage.r062=d;penpot.selection=[b];penpot.viewport.zoomIntoView([b]);
return d;
