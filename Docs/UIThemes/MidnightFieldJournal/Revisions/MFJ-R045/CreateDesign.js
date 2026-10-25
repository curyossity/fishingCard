// Penpot-only revision: vector engraving designed for nine-slice resizing.
if(storage.modifierR045)return storage.modifierR045;
const prior=storage.modifierR044??{pageId:'bae680cd-9225-8018-8008-cddd6a6f3825',sourceFrameId:'bae680cd-9225-8018-8008-d1c2678546b4'};
const page=penpotUtils.getPageById(prior.pageId);penpot.openPage(page);
const paper=penpotUtils.findShapeById(prior.sourceFrameId);
const ink='#304448',sepia='#9B713F',muted='#725C44',green='#18703A',red='#AE2C26';
const title=penpot.fonts.findByName('Cinzel'),body=penpot.fonts.findByName('Source Serif 4');
function board(name,x,y,w,h,parent){const s=penpot.createBoard();s.name=name;s.resize(w,h);s.fills=[];if(parent)parent.appendChild(s);s.x=(parent?.x||0)+x;s.y=(parent?.y||0)+y;return s;}
function rect(p,n,x,y,w,h,c,a=1){const s=penpot.createRectangle();p.appendChild(s);s.name=n;s.resize(w,h);s.x=p.x+x;s.y=p.y+y;s.fills=[{fillColor:c,fillOpacity:a}];return s;}
function text(p,n,c,x,y,w,h,size,colour=ink,bold=false,heading=false){const s=penpot.createText(c);p.appendChild(s);s.name=n;const f=heading?title:body;f.applyToText(s,f.variants.find(v=>v.fontWeight===(bold?'700':'400')&&v.fontStyle==='normal'));s.fontSize=String(size);s.fills=[{fillColor:colour,fillOpacity:1}];s.resize(w,h);s.x=p.x+x;s.y=p.y+y;return s;}
function svg(p,n,x,y,w,h,paths,view='0 0 64 64'){const s=penpot.createShapeFromSvg(`<svg xmlns="http://www.w3.org/2000/svg" width="${w}" height="${h}" viewBox="${view}">${paths}</svg>`);p.appendChild(s);s.name=n;s.x=p.x+x;s.y=p.y+y;return s;}
const corner=`<g fill="none" stroke="${sepia}" stroke-linecap="round" stroke-linejoin="round"><path d="M64 4H22Q4 4 4 22V64M64 11H23Q11 11 11 23V64" stroke-width="1.3"/><path d="M64 17H51M17 64V51M23 48Q7 36 22 22Q36 7 48 23M23 48Q37 48 48 23" stroke-width=".9"/><path d="M24 40Q14 31 24 23Q35 15 42 25L34 40Z" fill="#E4CF9F" stroke-width="1.2"/><path d="M25 38L23 26M28 39L28 23M31 39L33 23M34 38L38 26" stroke-width=".75"/><path d="M24 41Q29 45 34 41M21 45Q30 50 39 40M45 18Q54 20 53 29Q52 36 45 32Q40 29 45 25Q49 23 49 28M18 45Q20 54 29 53Q36 52 32 45Q29 40 25 45Q23 49 28 49" stroke-width="1"/><path d="M16 23L20 18M18 30L21 27M37 18L40 15M23 16L26 13" stroke-width=".65"/></g>`;
function frame(b){
 const w=b.width,h=b.height;
 // Only clean center parchment is reused; every border ornament is newly authored vector work.
 const tex=board('Static / parchment center texture',0,0,w,h,b);tex.clipContent=true;tex.borderRadius=20;
 const im=paper.clone();tex.appendChild(im);im.name='Static / clean parchment only';im.resize(1190*w/550,1322*h/642);im.x=tex.x-320*w/550;im.y=tex.y-340*h/642;
 const corners=[];
 for(const [x,y,sx,sy] of [[0,0,1,1],[w-64,0,-1,1],[0,h-64,1,-1],[w-64,h-64,-1,-1]]){
  corners.push(svg(b,'9-slice / fixed engraved corner',x,y,64,64,`<g transform="translate(${sx<0?64:0} ${sy<0?64:0}) scale(${sx} ${sy})">${corner}</g>`).id);
 }
 for(const y of [4,11,h-11,h-4])rect(b,'9-slice / straight horizontal rule',64,y,w-128,1.1,sepia);
 for(const x of [4,11,w-11,w-4])rect(b,'9-slice / straight vertical rule',x,64,1.1,h-128,sepia);
 return {corners,borders:{left:64,right:64,top:64,bottom:64}};
}
const glyphs={
 technique:`<g fill="none" stroke="${ink}" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><path d="M29 9V39Q29 55 43 51Q54 48 49 36L43 42M29 9H39M20 16L38 25M20 23L38 32"/><circle cx="29" cy="8" r="4"/></g>`,
 event:`<g fill="none" stroke="${ink}" stroke-width="2.6" stroke-linecap="round" stroke-linejoin="round"><path d="M17 13H45Q52 13 52 20V45H20V22Q20 13 13 13Q7 13 7 20H20M20 45H10Q10 54 18 54H46Q54 54 54 45H44"/><path d="M26 28Q31 22 36 28T46 28M26 36Q31 30 36 36T46 36"/></g>`,
 'Lionfish':`<path fill="${ink}" d="M7 34Q11 25 22 25L18 10L29 23L31 6L36 23L45 11L44 27Q48 28 51 31L61 22L58 35L61 48L49 39Q39 46 27 42L19 54L20 41Q11 40 7 34Z"/><circle cx="17" cy="32" r="2.2" fill="#F3E7CB"/>`,
 'Moonlit Ray':`<path fill="${ink}" d="M31 12Q24 23 6 30Q15 46 29 40Q34 50 43 58L46 57Q34 44 35 38Q49 45 59 29Q40 23 34 12L33 7Z"/>`,
 'Jellyfish':`<g fill="none" stroke="${ink}" stroke-width="2.7" stroke-linecap="round"><path fill="${ink}" d="M10 29Q10 9 31 8Q53 9 54 29Q43 35 32 29Q21 35 10 29Z"/><path d="M16 33Q11 40 20 46T17 58M25 33Q20 42 29 48T26 59M35 33Q42 41 34 48T39 58M45 33Q54 43 46 50T50 58"/></g>`,
 'Spiny Lobster':`<g fill="none" stroke="${ink}" stroke-width="2.5" stroke-linecap="round"><path d="M24 24L9 7M39 24L55 7M22 29L9 25M22 36L8 39M24 43L12 51M41 29L55 25M41 36L57 39M39 43L51 51"/><path fill="${ink}" d="M25 19Q32 14 39 19L41 38L36 49L43 58L32 54L21 58L27 49L22 38Z"/></g>`
};
function icon(p,name,kind,x,y){const key=kind==='creature'?name:kind==='technique'?'technique':'event';return svg(p,'Icon / '+key+' / flat ink / no backing',x,y,48,48,glyphs[key]);}
function arrow(p,dir,x,y){return svg(p,'Scroll / '+dir+' arrow',x,y,36,22,`<path d="${dir==='up'?'M6 16L18 6L30 16':'M6 6L18 16L30 6'}" fill="none" stroke="${sepia}" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"/>`,'0 0 36 22');}
const values=[{name:'Prize Hook',kind:'technique',amount:8},{name:'Lionfish',kind:'creature',amount:-5},{name:'Moonlit Ray',kind:'creature',amount:9}];
const weights=[{name:'Jellyfish',kind:'creature',amount:1},{name:'Prize Hook',kind:'technique',amount:1},{name:'Reef Snag',kind:'event',amount:1},{name:'Gentle Upwelling',kind:'event',amount:-1},{name:'Spiny Lobster',kind:'creature',amount:-1}];
const records=[];
function tip(parent,x,y,name,stat,base,rows,offset=0){
 const visible=Math.min(3,rows.length),over=rows.length>3,h=276+visible*72+(over?44:0),b=board(name,x,y,680,h,parent),f=frame(b),current=base+rows.reduce((a,r)=>a+r.amount,0);
 text(b,'Label / default','DEFAULT '+stat.toUpperCase(),88,45,245,36,22,muted,false,true).align='center';
 text(b,'Label / current','CURRENT '+stat.toUpperCase(),347,45,245,36,22,muted,false,true).align='center';
 text(b,'Dynamic / default',String(base),88,86,245,66,50).align='center';
 text(b,'Static / comparison arrow','→',305,94,70,55,38,sepia).align='center';
 text(b,'Dynamic / current',String(current),347,86,245,66,50,(stat==='Value'?current>base:current<base)?green:red,true).align='center';
 rect(b,'Static / divider',76,169,528,1,sepia,.5);
 const up=offset>0?arrow(b,'up',322,184):null;
 const view=board('Reasons / clipped viewport / maximum three',44,216,592,visible*72,b);view.clipContent=true;
 const content=board('Reasons / scroll content',0,-offset*72,592,rows.length*72,view);
 rows.forEach((r,i)=>{const row=board('Reason / '+r.name,0,72*i,592,72,content);icon(row,r.name,r.kind,16,12);text(row,'Dynamic / source name',r.name,90,18,390,43,30,ink,true);text(row,'Dynamic / signed amount',(r.amount>0?'+':'−')+Math.abs(r.amount),495,15,76,47,34,(stat==='Value'?r.amount>0:r.amount<0)?green:red,true).align='right';});
 const down=offset+visible<rows.length?arrow(b,'down',322,216+visible*72+12):null;
 const r={id:b.id,name,stat,base,current,rows,offset,visibleRows:visible,height:h,width:680,viewportId:view.id,contentId:content.id,upArrowId:up?.id,downArrowId:down?.id,frame:f};records.push(r);return {b,r,up,down};
}
const review=board('MFJ-R045 / Compact tooltip / flat icons + corner engraving',32000,13900,2520,900);rect(review,'Review background',0,0,2520,900,'#111E23');text(review,'Review title','ENGRAVED CORNERS · QUIET INK',70,45,2350,70,42,'#E8D4AE',false,true);text(review,'Review note','Clean source silhouettes. Fixed corner detail. Straight rules that resize without stretching ornament.',70,128,2350,50,28,'#C9BDA5');
tip(review,100,230,'MFJ-R045 / One reason','Value',12,values.slice(0,1));tip(review,920,230,'MFJ-R045 / Two reasons','Value',30,values.slice(0,2));tip(review,1740,230,'MFJ-R045 / Three reasons','Value',30,values);
const overflow=board('MFJ-R045 / Arrow-only scrolling / first · middle · last',34800,13900,2600,900);rect(overflow,'Review background',0,0,2600,900,'#111E23');text(overflow,'Review title','SCROLL CUES, WITHOUT CLUTTER',70,45,2450,70,42,'#E8D4AE',false,true);text(overflow,'Review note','Five sources · three visible. Up above the list, down below it; both only when both directions are available.',70,128,2450,50,28,'#C9BDA5');
const states=[0,1,2].map((o,i)=>tip(overflow,100+i*850,230,'MFJ-R045 / Scroll offset '+o,'Weight',2,weights,o));
const prototypes=states.map((s,i)=>{const b=s.b.clone();page.root.appendChild(b);b.name='PROTOTYPE / R045 / scroll state '+i;b.x=38000+i*800;b.y=13900;return b;});
for(let i=0;i<3;i++){const b=prototypes[i];for(const s of b.children){if(s.name==='Scroll / up arrow')s.addInteraction('click',{type:'navigate-to',destination:prototypes[i-1]});if(s.name==='Scroll / down arrow')s.addInteraction('click',{type:'navigate-to',destination:prototypes[i+1]});}}
const master=board('MFJ-R045 / Blank master / 64px nine-slice borders',32000,15100,680,536);const masterFrame=frame(master);
storage.modifierR045={revision:'MFJ-R045',fileId:penpot.currentFile.id,pageId:page.id,reviewBoardId:review.id,scrollReviewBoardId:overflow.id,masterBoardId:master.id,prototypeIds:prototypes.map(b=>b.id),records,sliceBorderPixels:{left:64,right:64,top:64,bottom:64},masterFrame,scrollbar:false,scrollIndicators:'Arrow up above list; arrow down below list; both for middle scroll positions. No counts or labels.',icons:'Flat single ink colour, no backing. Tied hook for every Technique; wave scroll for every event; individual creature silhouettes.',status:'ready_for_review',unityImported:false};
penpot.selection=[review];penpot.viewport.zoomIntoView([review]);return storage.modifierR045;
