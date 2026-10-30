const saved=penpot.currentPage.getPluginData('MFJ-R056');if(saved)return JSON.parse(saved);
const b=penpot.createBoard();b.name='MFJ-R056 — Technique Collection / In-run card faces';
b.resize(3840,2160);b.x=48600;b.y=34000;b.fills=[];b.clipContent=true;
const sx=3840/1672,sy=2160/941,fields=[],cards=[];
const bg=penpot.currentPage.getShapeById(ART.background);b.appendChild(bg);bg.resize(3840,2160);bg.x=b.x;bg.y=b.y;bg.name='Static / newly illustrated collection workshop and ornate framing';
function label(parent,value,x,y,w,h,size,serif=false,color='#F1DFB7',align='center',scale=sx){
 const t=penpot.createText(value);parent.appendChild(t);
 const f=penpot.fonts.findByName(serif?'Source Serif 4':'Cinzel');if(f)f.applyToText(t,f.variants.find(v=>v.fontWeight==='400'&&v.fontStyle==='normal')||f.variants[0]);
 t.fontSize=String(size*scale);t.fills=[{fillColor:color,fillOpacity:1}];t.resize(w*scale,h*scale);t.x=parent.x+x*scale;t.y=parent.y+y*scale;t.align=align;t.name='Editable / '+value.replace(/\n/g,' ');fields.push(t.id);return t;
}
function rule(x,y,w){const r=penpot.createRectangle();b.appendChild(r);r.resize(w*sx,sy);r.x=b.x+x*sx;r.y=b.y+y*sy;r.fills=[{fillColor:'#B88D52',fillOpacity:.65}];r.name='Native / copper rule';}
function resource(parent,key,x,y,w,h,scale=sx,contain=false){
 const r=penpot.currentPage.getShapeById(ART[key]).clone();parent.appendChild(r);
 if(contain){const d=SOURCES[key].dimensions,k=Math.min(w*scale/d[0],h*scale/d[1]);r.resize(d[0]*k,d[1]*k);}else r.resize(w*scale,h*scale);
 r.x=parent.x+x*scale+(w*scale-r.width)/2;r.y=parent.y+y*scale+(h*scale-r.height)/2;
 r.name='Exact active-game resource / '+key;return r;
}
label(b,'TECHNIQUES',178,45,280,42,27,false,'#243638');label(b,'920',1415,42,140,45,32,false,'#243638');
['HARBOR','TACKLE SHOP','TECHNIQUES','JOURNAL'].forEach((s,i)=>{const t=label(b,s,580+i*157,49,150,35,16,false,i===2?'#F1DFB7':'#C5AE89');if(i<2)t.addInteraction('click',{type:'navigate-to',destination:penpot.currentPage.getShapeById(i===0?'75555645-d407-8067-8008-de8ff58e2639':'75555645-d407-8067-8008-de9b9c9710d1'),preserveScrollPosition:false});});
rule(918,79,100);
['ALL TECHNIQUES','IN DECK','OWNED','LOCKED'].forEach((s,i)=>label(b,s,90+i*243,310,235,35,17,false,'#243638'));
rule(140,337,130);
const designs=[
 ['cut_loose','Cut Loose','CutLoose','Release 1 attached\ncreature.',3],
 ['slip_past','Slip Past','SlipPast','Skip the Hooked\nnon-Apex creature.',3],
 ['clear_water','Clear Water','ClearWater','Reveal a hidden Hooked\ncreature; otherwise remove\n1 pending concealment.',1],
 ['hold_steady','Hold Steady','HoldSteady','Cancel the current\nunresolved Hazard.',1],
 ['rerig','Re-rig','Rerig','Switch 1 attached creature\nbetween Catch and Bait.',1],
 ['steady_line','Steady Line','SteadyLine','Reduce Line Load by 2 for\nyour next Descend or Surface.',3],
 ['recast','Recast','Recast','Discard 1 or 2 other techniques,\nthen draw that many.',0],
 ['prize_hook','Prize Hook','PrizeHook','Your next Catch gains 8\nvalue and 1 weight.',-1]
];
designs.forEach(([id,name,key,description,count],i)=>{
 const x=112.5+(i%4)*240,y=358+Math.floor(i/4)*221,k=190*sx/620;
 const c=penpot.createBoard();b.appendChild(c);c.name='Reusable Technique / '+id;c.resize(620*k,640*k);c.x=b.x+x*sx;c.y=b.y+y*sy;c.fills=[];
 resource(c,'frame',0,0,620,640,k);
 label(c,'TECHNIQUE',142.6,46.7,334.8,30.1,20,false,'#243638','center',k);
 label(c,name.toUpperCase(),74.4,86.4,471.2,57.6,39.68,false,'#243638','center',k);
 resource(c,key,49.6,176,520.8,291.2,k,true);
 label(c,description,86.8,486.4,446.4,108.8,28,true,'#243638','center',k);
 if(count<0)c.opacity=.56;
 const state=count<0?'LOCKED · 120 GOLD':count===0?'OWNED · NOT IN DECK':count+' IN DECK';
 label(b,state,x-10,y+198,210,20,12,false,count<0?'#E3BD79':count===0?'#DDC8A6':'#C4D9C4');
 cards.push({id,boardId:c.id,resource:key,deckCopies:count<0?0:count,illustrativeState:count<0?'locked':count===0?'owned':'in_deck'});
});
label(b,'STARTING DECK',1142,310,410,42,27,false,'#243638');
label(b,'12 / 12',1160,367,230,58,39,false);
label(b,'READY',1402,382,145,30,16,false,'#C4D9C4');
label(b,'Starting hand: 4 Techniques',1155,429,405,32,18,true);rule(1160,470,390);
designs.forEach(([id,name,key,desc,count],i)=>{
 const y=486+i*28;
 label(b,name,1215,y,190,31,18,true,count<0?'#A89E87':'#F1DFB7','left');
 label(b,String(Math.max(0,count)),1425,y,40,31,18,false,count<=0?'#A89E87':'#F1DFB7');
 label(b,'−',1390,y-2,30,32,23,false,count<=0?'#766E61':'#D9BF93');
 label(b,count<0?'—':'+',1484,y-2,30,32,22,false,count<0?'#766E61':'#D9BF93');
});
label(b,'LOADOUT SAVED',1195,751,365,37,23,false,'#243638');
label(b,'YOUR COLLECTION',225,837,825,26,15,false,'#E0BC80','left');
label(b,'7 owned · 1 locked   /   Deck changes apply next expedition.',225,866,825,32,19,true,'#F1DFB7','left');
const back=label(b,'BACK TO HARBOR',1235,845,315,40,23,false,'#243638');back.addInteraction('click',{type:'navigate-to',destination:penpot.currentPage.getShapeById('75555645-d407-8067-8008-de8ff58e2639'),preserveScrollPosition:false});
const result={revision:'MFJ-R056',fileId:penpot.currentFile.id,pageId:penpot.currentPage.id,pageName:penpot.currentPage.name,boardId:b.id,boardName:b.name,width:b.width,height:b.height,cardInstances:cards,editableTextFields:fields,sourceArtwork:ART,unityImported:false,cardDesign:'Exact active in-run Technique frame and illustrations; planar collection presentation with native title and rules'};
penpot.currentPage.setPluginData('MFJ-R056',JSON.stringify(result));penpot.selection=[b];penpot.viewport.zoomIntoView([b]);return result;
