// Penpot-only compact alternative. Preserves R042 and all Unity resources.
if(storage.modifierR043) return storage.modifierR043;
const page=penpotUtils.getPageById('bae680cd-9225-8018-8008-cddd6a6f3825');penpot.openPage(page);
const paper=penpotUtils.findShapeById('bae680cd-9225-8018-8008-d1c2678546b4');
const ink='#24383A',muted='#695846',copper='#A8753E',green='#18703A',red='#AE2C26';
const title=penpot.fonts.findByName('Cinzel'),body=penpot.fonts.findByName('Source Serif 4');
function board(name,x,y,w,h,parent){const b=penpot.createBoard();b.name=name;b.resize(w,h);b.fills=[];if(parent)parent.appendChild(b);b.x=(parent?.x||0)+x;b.y=(parent?.y||0)+y;return b;}
function rect(b,n,x,y,w,h,c,a=1){let s=penpot.createRectangle();b.appendChild(s);s.name=n;s.resize(w,h);s.x=b.x+x;s.y=b.y+y;s.fills=[{fillColor:c,fillOpacity:a}];return s;}
function text(b,n,c,x,y,w,h,size,colour=ink,bold=false,heading=false,align='left'){let s=penpot.createText(c);b.appendChild(s);s.name=n;let f=heading?title:body;f.applyToText(s,f.variants.find(v=>v.fontWeight===(bold?'700':'400')&&v.fontStyle==='normal'));s.fontSize=String(size);s.fills=[{fillColor:colour,fillOpacity:1}];s.align=align;s.resize(w,h);s.x=b.x+x;s.y=b.y+y;return s;}
function frame(b){
 const sx=[0,320,870,1190],sy=[0,340,982,1322],dx=[0,80,b.width-80,b.width],dy=[0,85,b.height-85,b.height];
 const ids=[];
 for(let y=0;y<3;y++)for(let x=0;x<3;x++){
  const w=dx[x+1]-dx[x],h=dy[y+1]-dy[y];
  const tile=board('9-slice / '+['top','middle','bottom'][y]+'-'+['left','center','right'][x],dx[x],dy[y],w,h,b);tile.clipContent=true;
  const image=paper.clone();tile.appendChild(image);const scaleX=w/(sx[x+1]-sx[x]),scaleY=h/(sy[y+1]-sy[y]);
  image.resize(1190*scaleX,1322*scaleY);image.x=tile.x-sx[x]*scaleX;image.y=tile.y-sy[y]*scaleY;image.name='Static / shared border texture region';ids.push(tile.id);
 }
 return ids;
}
function icon(row,kind,name){
 const source=kind==='creature'?penpotUtils.findShape(s=>s.type==='rectangle'&&s.fills?.some(f=>f.fillImage)&&s.name.toLowerCase().includes(name.toLowerCase())&&/illustration/i.test(s.name)):null;
 const thumb=board('Dynamic / source image or icon',12,8,56,56,row);thumb.clipContent=true;thumb.borderRadius=9;thumb.fills=[{fillColor:'#E1CFAC',fillOpacity:.45}];
 if(source){const image=source.clone();thumb.appendChild(image);const h=56,w=56*source.width/source.height;image.resize(w,h);image.x=thumb.x+(56-w)/2;image.y=thumb.y;image.name='Dynamic / approved '+name+' portrait';return;}
 const p=kind==='technique'?'<path d="M12 6V25Q12 36 25 33Q35 30 32 20L28 25M12 6H20M24 9L31 3M28 14L37 11"/>':kind==='environment'?'<path d="M9 35V10M3 16L9 10L15 16M30 31V5M24 11L30 5L36 11"/>':'<path d="M21 4L40 36H2ZM21 14V24M21 29V30"/>';
 const g=penpot.createShapeFromSvg('<svg xmlns="http://www.w3.org/2000/svg" width="42" height="42" viewBox="0 0 42 42"><g fill="none" stroke="#A8753E" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">'+p+'</g></svg>');thumb.appendChild(g);g.name='Dynamic / '+kind+' icon';g.x=thumb.x+7;g.y=thumb.y+7;
}
const valueRows=[{name:'Prize Hook',kind:'technique',amount:8},{name:'Lionfish',kind:'creature',amount:-5},{name:'Moonlit Ray',kind:'creature',amount:9}];
const weightRows=[{name:'Jellyfish',kind:'creature',amount:1},{name:'Prize Hook',kind:'technique',amount:1},{name:'Reef Snag',kind:'hazard',amount:1},{name:'Gentle Upwelling',kind:'environment',amount:-1}];
const records=[];
function tooltip(parent,x,y,name,stat,base,rows,offset=0){
 const overflow=rows.length>3,visible=Math.min(3,rows.length),height=276+visible*72+(overflow?44:0);
 const b=board(name,x,y,680,height,parent),tiles=frame(b),current=base+rows.reduce((s,r)=>s+r.amount,0),good=stat==='Value'?current>base:current<base;
 text(b,'Static / default label','DEFAULT '+stat.toUpperCase(),90,45,235,36,22,muted,false,true,'center');
 text(b,'Static / current label','CURRENT '+stat.toUpperCase(),355,45,235,36,22,muted,false,true,'center');
 text(b,'Dynamic / default',String(base),90,86,235,66,50,ink,false,false,'center');
 text(b,'Static / arrow','→',305,94,70,55,38,copper,false,false,'center');
 text(b,'Dynamic / current',String(current),355,86,235,66,50,good?green:red,true,false,'center');
 rect(b,'Static / separator',76,169,528,1.5,copper,.55);
 const view=board('Reasons / clipped viewport / maximum 3',44,194,586,visible*72,b);view.clipContent=true;
 const content=board('Reasons / scroll content',0,-offset*72,586,rows.length*72,view);
 rows.forEach((r,i)=>{const row=board('Reason / '+r.name,0,i*72,586,72,content);icon(row,r.kind,r.name);
  text(row,'Dynamic / source name',r.name,90,18,390,43,30,ink,true);
  const benefit=stat==='Value'?r.amount>0:r.amount<0;
  text(row,'Dynamic / signed amount',(r.amount>0?'+':'−')+Math.abs(r.amount),495,15,76,47,34,benefit?green:red,true,false,'right');
 });
 let hint=null;
 if(overflow){
  const track=rect(b,'Scrollbar / track',644,202,4,visible*72-16,copper,.25);track.borderRadius=2;
  const thumbH=(visible*72-16)*visible/rows.length;
  const thumb=rect(b,'Scrollbar / draggable thumb',642,202+(offset/(rows.length-visible))*(visible*72-16-thumbH),8,thumbH,copper,.9);thumb.borderRadius=4;
  hint=board('Scroll / '+(offset?'earlier sources':'more sources'),210,194+visible*72+14,260,40,b);
  text(hint,'Dynamic / overflow indicator',offset?'↑ '+offset+' above':'↓ '+(rows.length-visible)+' more below',0,0,260,40,25,muted,false,false,'center');
 }
 const record={id:b.id,name,stat,base,current,rows,visibleRows:visible,offset,height,width:680,tiles,viewportId:view.id,contentId:content.id,hintId:hint?.id};records.push(record);return {b,hint,record};
}
const review=board('MFJ-R043 / Compact modifier tooltips / 1–3 sources',32000,11100,2520,900);
rect(review,'Review background',0,0,2520,900,'#111E23');
text(review,'Review heading','ONLY WHAT CHANGED',70,42,2350,70,44,'#E8D4AE',false,true);
text(review,'Review note','Default · current · source · amount. The frame grows with the list; three reasons remain the visible maximum.',70,123,2350,50,28,'#C9BDA5');
const one=tooltip(review,100,230,'MFJ-R043 / One source', 'Value',12,valueRows.slice(0,1));
const two=tooltip(review,920,230,'MFJ-R043 / Two sources','Value',30,valueRows.slice(0,2));
const three=tooltip(review,1740,230,'MFJ-R043 / Three sources','Value',30,valueRows);
const scrollReview=board('MFJ-R043 / Overflow / three visible + scrolling',34800,11100,1840,900);
rect(scrollReview,'Review background',0,0,1840,900,'#111E23');
text(scrollReview,'Review heading','MORE SOURCES, SAME FOOTPRINT',70,42,1700,70,40,'#E8D4AE',false,true);
text(scrollReview,'Review note','Four sources. Three visible at once. The scrollbar and indicator reveal the remainder.',70,123,1700,50,27,'#C9BDA5');
const top=tooltip(scrollReview,120,230,'MFJ-R043 / Overflow / first three','Weight',2,weightRows,0);
const bottom=tooltip(scrollReview,1040,230,'MFJ-R043 / Overflow / scrolled to last','Weight',2,weightRows,1);
const protoTop=top.b.clone();page.root.appendChild(protoTop);protoTop.name='PROTOTYPE / R043 / scroll down';protoTop.x=37200;protoTop.y=11100;
const protoBottom=bottom.b.clone();page.root.appendChild(protoBottom);protoBottom.name='PROTOTYPE / R043 / scroll up';protoBottom.x=38000;protoBottom.y=11100;
const topHint=penpotUtils.findShape(s=>s.name==='Scroll / more sources',protoTop),bottomHint=penpotUtils.findShape(s=>s.name==='Scroll / earlier sources',protoBottom);
topHint.addInteraction('click',{type:'navigate-to',destination:protoBottom});bottomHint.addInteraction('click',{type:'navigate-to',destination:protoTop});
const guide=board('MFJ-R043 / 9-slice assembly / fixed corners',32000,12300,2480,1180);
rect(guide,'Review background',0,0,2480,1180,'#111E23');
text(guide,'Guide heading','ONE FRAME · NINE REGIONS',70,42,2340,70,42,'#E8D4AE',false,true);
text(guide,'Guide note','Original engraved frame retained. Corners keep their size; borders and parchment stretch independently.',70,125,2340,50,28,'#C9BDA5');
const master=paper.clone();guide.appendChild(master);master.resize(595,661);master.x=guide.x+90;master.y=guide.y+240;master.name='Source / original border with slice guides';
for(let x of [160,435])rect(guide,'Guide / vertical cut',90+x,240,1.5,661,'#69B5B0',.9);
for(let y of [170,491])rect(guide,'Guide / horizontal cut',90,240+y,595,1.5,'#69B5B0',.9);
text(guide,'Guide source metadata','1190 × 1322 source\nBorders: left/right 320 px; top/bottom 340 px\nDisplay caps: 80 × 85 at quarter source scale',90,940,850,140,27,'#C9BDA5');
const blank=board('Source / actual nine-region blank module',1040,250,680,492,guide);const blankTiles=frame(blank);
text(guide,'Guide module metadata','680 px wide\n348 / 420 / 492 px for 1 / 2 / 3 sources\n536 px with a 3-row scroll viewport',1040,790,1120,150,32,'#C9BDA5');
storage.modifierR043={revision:'MFJ-R043',fileId:penpot.currentFile.id,pageId:page.id,reviewBoardId:review.id,scrollReviewBoardId:scrollReview.id,guideBoardId:guide.id,blankModuleId:blank.id,prototypeTopId:protoTop.id,prototypeBottomId:protoBottom.id,records,sourceFrameId:paper.id,sliceBorderPixels:{left:320,bottom:340,right:320,top:340},displayCapPixels:{left:80,bottom:85,right:80,top:85},blankTiles,unityImported:false,status:'ready_for_review'};
penpot.selection=[review];penpot.viewport.zoomIntoView([review]);return storage.modifierR043;
