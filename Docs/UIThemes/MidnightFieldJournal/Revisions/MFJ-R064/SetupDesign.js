const p=penpot.currentPage,ns='fishing-cards-design';if(p.getSharedPluginData(ns,'MFJ-R064'))throw Error('R064 exists; inspect first');
const X=80000,Y=17000,R=1190/1322,C={ink:'#283B3D',navy:'#101D24',paper:'#EBD8B7',cream:'#F2E5C9',muted:'#7B705E',gold:'#C4A170',ivory:'#EDDFC2',green:'#4B6751'};
const refs={background:'bae680cd-9225-8018-8008-d00618505dcc',card:'f31e67ca-185b-80af-8008-e036aa77cd26',rod:'75555645-d407-8067-8008-de9b972ff90c',reel:'75555645-d407-8067-8008-de9b99677d11',hook:'75555645-d407-8067-8008-de9b9b3a9112'};
function board(name,x,y,w,h,parent){const b=penpot.createBoard();b.name=name;b.resize(w,h);b.x=x;b.y=y;b.fills=[];b.clipContent=false;if(parent)parent.appendChild(b);return b;}
const b=board('MFJ-R064 / Tackle Shop / The Open Field Journal',X,Y,3840,2160,p.getShapeById('92f68b17-9997-80de-8008-dfc1d6a06a0a'));b.fills=[{fillColor:C.navy,fillOpacity:1}];
function text(parent,name,copy,x,y,w,h,size=34,color=C.ink,body=false,align='left'){const t=penpot.createText(copy);penpot.fonts.findByName(body?'Source Serif 4':'Cinzel').applyToText(t);t.name=name;t.fontSize=String(size);t.fontWeight='400';t.lineHeight='1.16';t.align=align;t.growType='fixed';t.resize(w,h);t.x=X+x;t.y=Y+y;t.fills=[{fillColor:color,fillOpacity:1}];parent.appendChild(t);return t;}
function image(parent,key,x,y,w,h,fit=false){const src=p.getShapeById(refs[key]),a=src.clone(),k=Math.min(w/src.width,h/src.height);parent.appendChild(a);a.name='Independent illustration / '+key;a.resize(fit?src.width*k:w,fit?src.height*k:h);a.x=X+x+(w-a.width)/2;a.y=Y+y+(h-a.height)/2;return a;}
function svg(parent,name,body,x,y,w,h){const a=penpot.createShapeFromSvg(`<svg xmlns="http://www.w3.org/2000/svg" width="${w}" height="${h}" viewBox="0 0 ${w} ${h}">${body}</svg>`);a.name=name;parent.appendChild(a);a.x=X+x;a.y=Y+y;return a;}
image(b,'background',0,0,3840,2160);
const paper=board('Fixed / Parchment leaf',X+70,Y+65,2440,2030,b);paper.fixedWhenScrolling=true;
let grain='';for(let i=0;i<200;i++){const x=40+(i*127)%2340,y=30+(i*211)%1960;grain+=`<circle cx="${x}" cy="${y}" r="${i%3+1}" fill="#80664B" opacity=".075"/>`;}
svg(paper,'Native 4K parchment / no shop backdrop',`<defs><linearGradient id="paper" x1="0" y1="0" x2="1" y2="1"><stop stop-color="#F1E4C9"/><stop offset=".58" stop-color="#EAD6B2"/><stop offset="1" stop-color="#E2CBA4"/></linearGradient></defs><path d="M15 5Q1220 22 2428 4L2435 2009Q1220 2023 6 2016Z" fill="url(#paper)"/><path d="M2393 37Q2417 1020 2397 1988" stroke="#9D7950" stroke-opacity=".35" stroke-width="2"/>${grain}`,70,65,2440,2030);
svg(paper,'Ink botanical / page lower margin',`<g fill="none" stroke="#6C705E" stroke-width="2" opacity=".38"><path d="M20 165Q100 105 150 12M50 137Q19 99 5 105M60 130Q95 142 109 119M84 101Q43 79 47 57M98 82Q139 82 146 62M116 57Q93 25 105 15"/><path d="M186 162Q238 105 255 54M211 130Q183 113 180 89M225 109Q254 117 270 94M240 83Q223 68 229 50"/></g>`,143,1880,300,180);
const header=board('Fixed / Shop masthead',X+145,Y+100,2220,210,b);header.fixedWhenScrolling=true;
text(header,'Navigation / Harbor','‹  HARBOR',155,111,500,54,31,C.muted);
text(header,'Shop title','TACKLE SHOP',155,190,1810,104,76);
text(header,'Page subtitle','Choose what you take to sea.',1450,223,875,60,34,C.muted,true,'right');
const rig=board('Fixed / Portrait equipment slots',X+155,Y+325,2190,530,b);rig.fixedWhenScrolling=true;
text(rig,'Rig heading','EXPEDITION EQUIPMENT',180,333,1560,55,33);
text(rig,'Equipped count','0 / 3 EQUIPPED',1810,337,510,48,27,C.muted,false,'right');
const slots=[];
for(const [i,type] of ['ROD','REEL','HOOK'].entries()){
 const x=355+i*740,y=425,w=300,h=w/R,slot=board('Empty portrait slot / '+type,X+x,Y+y,w,h,rig);
 svg(slot,'Native empty card silhouette',`<rect x="3" y="3" width="${w-6}" height="${h-6}" rx="20" fill="#D8C7A9" fill-opacity=".38" stroke="#7E7865" stroke-width="2"/><rect x="15" y="15" width="${w-30}" height="${h-30}" rx="12" fill="none" stroke="#A09881" stroke-width="1"/><path d="M130 ${h/2}H170 M150 ${h/2-20}V${h/2+20}" stroke="#7E7865" stroke-width="2"/><path d="M34 58Q34 34 58 34 M${w-34} ${h-58}Q${w-34} ${h-34} ${w-58} ${h-34}" stroke="#8F805E" fill="none" stroke-width="2"/>`,x,y,w,h);
 text(rig,'Slot type / '+type,type,x-45,y+h+26,w+90,48,31,C.ink,false,'center');slots.push({id:slot.id,category:type,width:w,height:h});
}
svg(b,'Journal section rule','<path d="M0 1H2180" stroke="#8B7A5E" stroke-opacity=".5" stroke-width="2"/>',155,865,2180,3);
const filters=board('Fixed / Collection heading and filters',X+155,Y+900,2190,120,b);filters.fixedWhenScrolling=true;
text(filters,'Catalog heading','THE COLLECTION',170,903,1270,60,41);
text(filters,'Catalog count','9 ITEMS',1860,910,465,48,28,C.muted,false,'right');
for(const [i,label] of ['ALL','RODS','REELS','HOOKS'].entries())text(filters,'Filter / '+label,label,170+i*380,981,340,48,27,i===0?C.ink:C.muted,false,'left');
svg(filters,'Active filter underline','<path d="M0 1H110" stroke="#554F42" stroke-width="3"/>',170,1023,110,4);
const viewport=board('Scroll / Portrait catalog viewport',X+150,Y+1070,2210,920,b);viewport.clipContent=true;
const content=board('Scroll / Nine portrait placeholders',X+150,Y+1070,2210,2010,viewport);
storage.r064={X,Y,R,C,refs,b,paper,header,rig,slots,filters,viewport,content,board,text,image,svg,catalog:[]};
p.setSharedPluginData(ns,'MFJ-R064-WIP',JSON.stringify({id:b.id,viewportId:viewport.id,contentId:content.id}));return {id:b.id,slotRatio:R,slots};
