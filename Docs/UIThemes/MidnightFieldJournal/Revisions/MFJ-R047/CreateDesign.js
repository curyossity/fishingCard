// Six static native Penpot bars only. No prototype, animation or Unity implementation.
// Invoke with ACTION='initialize' or ACTION='bar:0:0' ... 'bar:2:1'.
const tagNames=['Fish','Predator','Schooling','Small','Heavy','Anchored','Armored','Elusive'];
const colours=['#3C7B84','#985653','#4B6C9F','#66895E','#665A78','#8A6B4C','#AA7146','#9277A4'];
const ink='#304448',sepia='#9B713F',paperColour='#F0E2C2',gold='#E7BC61',night='#14262C';
const initial=Array(8).fill(.125),mid=[.14,.10,.14,.06,.14,.14,.14,.14];
const names=['The Field Journal','The Night Sounder','The Naturalist’s Ledger'];
const titleFont=penpot.fonts.findByName('Cinzel'),bodyFont=penpot.fonts.findByName('Source Serif 4');
let state=storage.tagBarR047;

function board(name,x,y,w,h,parent){const s=penpot.createBoard();s.name=name;s.resize(w,h);s.fills=[];if(parent)parent.appendChild(s);s.x=(parent?.x||0)+x;s.y=(parent?.y||0)+y;return s;}
function rect(parent,name,x,y,w,h,colour,opacity=1,radius=0){const s=penpot.createRectangle();parent.appendChild(s);s.name=name;s.resize(w,h);s.x=parent.x+x;s.y=parent.y+y;s.fills=[{fillColor:colour,fillOpacity:opacity}];s.borderRadius=radius;return s;}
function label(parent,name,value,x,y,w,h,size=26,colour=ink,heading=false,align='left'){
 const s=penpot.createText(value);parent.appendChild(s);s.name=name;
 const f=heading?titleFont:bodyFont;f.applyToText(s,f.variants.find(v=>v.fontWeight==='400'&&v.fontStyle==='normal'));
 s.fontSize=String(size);s.fills=[{fillColor:colour,fillOpacity:1}];s.resize(w,h);s.x=parent.x+x;s.y=parent.y+y;s.align=align;return s;
}
function svg(parent,name,x,y,w,h,paths,view=`0 0 ${w} ${h}`){const s=penpot.createShapeFromSvg(`<svg xmlns="http://www.w3.org/2000/svg" width="${w}" height="${h}" viewBox="${view}">${paths}</svg>`);parent.appendChild(s);s.name=name;s.x=parent.x+x;s.y=parent.y+y;return s;}
const fish='<path d="M5 16C11 7 22 7 28 16C22 25 11 25 5 16ZM5 16L1 10V22ZM22 14h.1"/>';
const icons=[fish,
 '<path d="M3 12Q16 2 29 12L25 26H7ZM5 13L10 20L14 13L18 20L23 13M11 7l-3 -3M21 7l3 -3"/>',
 '<path d="M4 9Q10 2 17 9Q10 16 4 9L1 5v8ZM17 17Q23 10 30 17Q23 24 17 17L14 13v8ZM4 25Q10 18 17 25Q10 32 4 25L1 21v8Z"/>',
 '<path d="M10 16Q16 9 22 16Q16 23 10 16L7 12v8ZM3 6H0v20h3M29 6h3v20h-3"/>',
 '<path d="M10 10V7a6 6 0 0 1 12 0v3M7 11H25L29 29H3Z"/>',
 '<circle cx="16" cy="5" r="3"/><path d="M16 8V28M8 12h16M3 19Q3 29 16 29Q29 29 29 19M3 19l-2 5M3 19l5 2M29 19l2 5M29 19l-5 2"/>',
 '<path d="M16 2L29 8V17Q27 27 16 31Q5 27 3 17V8ZM16 7V26M8 11h16M9 17h14M12 23h8"/>',
 '<path d="M25 3Q8 3 8 16Q8 29 25 29Q17 24 17 16Q17 8 25 3ZM1 10h4M0 17h5M1 24h4"/>'];
function icon(parent,index,x,y,size,colour){return svg(parent,'Tag icon / '+tagNames[index],x,y,size,size,`<g fill="none" stroke="${colour}" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round">${icons[index]}</g>`,'0 0 32 32');}
function parchment(parent,dark=false){
 const source=penpotUtils.getPageById('bae680cd-9225-8018-8008-cddd6a6f3825').getShapeById('bae680cd-9225-8018-8008-d1c2678546b4');
 const crop=board('Static / reusable parchment grain',0,0,1500,480,parent);crop.clipContent=true;crop.borderRadius=24;
 const im=source.clone();crop.appendChild(im);im.resize(1190*1500/550,1322*480/642);im.x=crop.x-320*1500/550;im.y=crop.y-340*480/642;im.name='Paper grain / approved clean texture';
 if(dark)rect(crop,'Sea-ink wash',0,0,1500,480,night,.96);
}
function surround(parent,variant){
 const dark=variant===1;parchment(parent,dark);const edge=dark?'#B69A69':sepia;
 svg(parent,'Static / fine stretchable rules',3,3,1494,474,`<rect x="1" y="1" width="1492" height="472" rx="22" fill="none" stroke="${edge}" stroke-width="1.7"/><rect x="9" y="9" width="1476" height="456" rx="16" fill="none" stroke="${edge}" stroke-opacity=".7" stroke-width=".8"/>`);
 const shell='<path d="M10 52Q2 22 25 12Q52 3 53 31L36 51Q23 63 10 52ZM15 47L17 22M24 50L27 16M33 47L38 17M41 40L48 24M12 56Q30 67 49 46M49 8Q66 12 60 27Q55 35 49 26"/>';
 const kelp='<path d="M12 65Q28 36 58 8M23 49Q4 48 11 34Q26 31 26 45M31 36Q22 16 35 14Q46 21 37 30M42 25Q43 4 56 7Q63 18 48 23M20 56Q44 57 45 43Q33 36 26 49"/>';
 const compass='<path d="M4 38V4H38M11 35V11H35M20 42L26 27L42 21L27 18L21 3L18 18L3 21L18 26Z"/>';
 const paths=variant===0?shell:variant===1?compass:kelp;
 for(const[x,y,sx,sy]of[[17,17,1,1],[1413,17,-1,1],[17,393,1,-1],[1413,393,-1,-1]])
  svg(parent,'Static / fixed '+(variant===2?'botanical':'engraved')+' corner',x,y,70,70,`<g transform="translate(${sx<0?70:0} ${sy<0?70:0}) scale(${sx} ${sy})" fill="none" stroke="${edge}" stroke-width="1" stroke-linecap="round" stroke-linejoin="round">${paths}</g>`);
}
function needle(parent,x,y,height,variant,precision){
 const head=variant===1?`<path d="M16 1L28 18L16 12L4 18Z"/>`:`<path d="M16 1L29 14L16 29L3 14ZM16 7L23 14L16 22L9 14Z"/>`;
 svg(parent,'Marker / independent needle',x-16,y,32,height,`<g stroke="${variant===1?paperColour:ink}" stroke-width="2" fill="${variant===1?night:paperColour}">${head}<path d="M16 28V${height-14}" stroke-width="5"/><path d="M16 28V${height-14}" stroke="${precision?gold:'#FFF5D9'}" stroke-width="2"/><path d="M16 ${height-13}l6 6l-6 6l-6 -6Z"/></g>`);
 if(precision)svg(parent,'Feedback / precision rays',x-32,y-16,64,64,`<g stroke="${gold}" stroke-width="2" fill="none">${Array.from({length:8},(_,i)=>{let a=i*Math.PI/4;return `<path d="M${32+23*Math.cos(a)} ${32+23*Math.sin(a)}L${32+31*Math.cos(a)} ${32+31*Math.sin(a)}"/>`;}).join('')}</g>`);
}
function legend(parent,widths,variant){
 const dark=variant===1;
 for(let i=0;i<8;i++){
  const x=60+(i%4)*345,y=268+Math.floor(i/4)*51;
  icon(parent,i,x,y+4,29,dark?colours[i]:colours[i]);
  label(parent,'Native tag label / '+tagNames[i],tagNames[i],x+43,y,205,40,27,dark?paperColour:ink,true);
  label(parent,'Native optional width / '+tagNames[i],(widths[i]*100).toFixed(widths[i]===.125?1:0)+'%',x+255,y+2,62,36,24,dark?'#BCA98D':'#80694D',false,'right');
 }
}
function stop(parent,variant,midgame){
 const dark=variant===1,face=dark?paperColour:ink,labelColour=dark?ink:paperColour;
 const p=board('Control / Stop / independent',1185,395,255,58,parent);rect(p,'Stop face',0,0,255,58,face,1,6);
 svg(p,'Stop / etched inner rule',4,4,247,50,`<rect x="1" y="1" width="245" height="48" rx="4" fill="none" stroke="${labelColour}" stroke-opacity=".5"/>`);
 rect(p,'Stop emblem',29,23,12,12,labelColour);label(p,'Native Stop label',midgame?'STOPPED':'STOP',52,10,183,40,25,labelColour,true,'center');
}
function buildBar(variant,midgame){
 const key=`${variant}:${midgame?1:0}`;if(state.bars[key]?.complete)return state.bars[key];
 const review=penpot.currentPage.getShapeById(state.reviewId),row=penpot.currentPage.getShapeById(state.rowIds[variant]);
 let p=state.bars[key]?penpot.currentPage.getShapeById(state.bars[key].id):null;
 if(p)return {id:p.id,incomplete:true};
 p=board(`${midgame?'Mid-run':'Initial'} / ${names[variant]} / editable static bar`,midgame?1740:0,120,1500,480,row);
 state.bars[key]={id:p.id,complete:false};surround(p,variant);
 const widths=midgame?mid:initial,dark=variant===1,fg=dark?paperColour:ink;
 const heading=['READ THE CURRENT','FOLLOW THE CURRENT','THE NATURALIST’S CAST'][variant];
 label(p,'Native / phase heading',heading,100,30,950,48,32,fg,true);
 svg(p,'Precision / gold legend',1080,39,18,24,`<path d="M9 1L16 12L9 23L2 12Z" fill="${gold}" stroke="${dark?'#F5D28D':'#7F5D29'}"/>`);
 label(p,'Precision / helper legend','GOLD CENTRE = PRECISION',1110,35,332,38,21,dark?paperColour:sepia);
 const trackY=variant===2?116:150,trackH=variant===2?132:72;
 const track=board('Track / normalized tag-zone container',60,trackY,1380,trackH,p);track.clipContent=true;track.borderRadius=7;
 const selected=midgame?(variant===1?1:3):-1,precision=midgame&&variant!==1;
 let offset=0;const zoneRecords=[];
 widths.forEach((fraction,i)=>{
  const w=1380*fraction,z=board('Zone / '+tagNames[i]+' / normalized width',offset*1380,0,w,trackH,track);z.clipContent=true;
  rect(z,'Independent tag colour',0,0,w,trackH,colours[i],variant===2?.25:dark?.88:.88);
  svg(z,'Independent zone separator',0,0,w,trackH,`<path d="M.7 0V${trackH}M${w-.7} 0V${trackH}" fill="none" stroke="${dark?paperColour:ink}" stroke-opacity=".4" stroke-width="1.4"/>`);
  const py=variant===2?48:0,ph=trackH-py,pw=w*.18,px=(w-pw)/2;
  rect(z,'Precision / 18 percent of parent / independent',px,py,pw,ph,gold,precision&&selected===i?1:.88,variant===1?4:0);
  svg(z,'Precision / fine boundary',px,py,pw,ph,`<rect x=".7" y=".7" width="${Math.max(1,pw-1.4)}" height="${ph-1.4}" rx="${variant===1?3:0}" fill="none" stroke="${dark?'#FFE5A5':'#7D5D2A'}" stroke-width="1.2"/><path d="M${pw*.5} 6V${ph-6}" fill="none" stroke="#FFF1C9" stroke-opacity=".6"/>`);
  if(variant===2)icon(z,i,w/2-17,8,34,colours[i]);else icon(p,i,60+offset*1380+w/2-17,99,34,dark?paperColour:colours[i]);
  if(midgame&&selected===i)svg(z,'Feedback / selected-tag outline',0,0,w,trackH,`<rect x="2" y="2" width="${w-4}" height="${trackH-4}" rx="4" fill="none" stroke="${precision?'#FFF2C7':paperColour}" stroke-width="3"/>`);
  zoneRecords.push({tag:tagNames[i],id:z.id,start:offset,end:offset+fraction,fraction,width:w,precisionWidth:pw,precisionFraction:.18});offset+=fraction;
 });
 svg(p,'Static / track outline',58,trackY-2,1384,trackH+4,`<rect x="1" y="1" width="1382" height="${trackH+2}" rx="8" fill="none" stroke="${dark?'#C3AD7F':sepia}" stroke-width="2"/>`);
 let position=.24;
 if(midgame){let start=widths.slice(0,selected).reduce((a,b)=>a+b,0);position=start+widths[selected]*(precision?.5:.79);}
 needle(p,60+1380*position,trackY-26,trackH+51,variant,precision);
 legend(p,widths,variant);rect(p,'Footer / fine manuscript rule',60,379,1380,1,dark?'#8B8064':sepia,.45);
 const copy=midgame?(precision?'SMALL · PRECISION HIT · HIGHER-RARITY CHANCE':'PREDATOR · TAG SELECTED'):'Stop on a tag. Aim for its gold centre.';
 label(p,'Native / feedback and instruction',copy,100,402,1050,41,24,dark?paperColour:ink,midgame);
 stop(p,variant,midgame);
 const record={id:p.id,variant,state:midgame?'mid-run':'initial',width:1500,height:480,trackId:track.id,trackWidth:1380,zones:zoneRecords,position,selectedTag:selected<0?null:tagNames[selected],precisionHit:precision,complete:true};state.bars[key]=record;return record;
}
if(ACTION==='initialize'){
 if(state&&penpot.currentPage.getShapeById(state.reviewId))return state;
 const page=penpot.currentPage;
 state=storage.tagBarR047={revision:'MFJ-R047',fileId:penpot.currentFile.id,pageId:page.id,bars:{},rowIds:[],initialWidths:initial,midWidths:mid,precisionFraction:.18,normalizationPolicy:'Design assumption: 12.5% recovery baseline before normalization; final normalized zones may exceed 12.5%; 5% floor. Widths illustrative.'};
 const review=board('MFJ-R047 / Dynamic Fishing Tag Bar / three static variations',32000,25000,3400,2530);state.reviewId=review.id;rect(review,'Review / deep sea background',0,0,3400,2530,'#111E23');
 label(review,'Review title','FOLLOW THE CURRENT',80,60,3240,100,64,paperColour,true);
 label(review,'Review subtitle','Three directions · eight tags · initial and mid-run · static UI studies only',85,174,3200,60,32,'#C8BBA3');
 for(let v=0;v<3;v++){
  const row=board(`${String(v+1).padStart(2,'0')} / ${names[v]} / two static states`,80,300+v*725,3240,620,review);state.rowIds.push(row.id);
  label(row,'Variation title',`${String(v+1).padStart(2,'0')} — ${names[v].toUpperCase()}`,0,0,3240,52,34,paperColour,true);
  label(row,'Initial state caption','INITIAL / EIGHT EQUAL ZONES · 12.5% EACH',0,72,1500,40,25,'#C8BBA3');
  label(row,'Mid-run state caption','MID-RUN / PREDATOR + SMALL REDUCED · OTHERS EXPANDED',1740,72,1500,40,25,'#C8BBA3');
 }
 label(review,'Review / visual-only scope','Precision centres shown at 18% of their parent zone for comparison; no rarity odds or runtime rules are implemented.',85,2440,3200,45,26,'#C8BBA3');
 penpot.selection=[review];penpot.viewport.zoomIntoView([review]);return state;
}
if(!state)throw Error('Initialize the review first.');
if(penpot.currentPage.id!==state.pageId)penpot.openPage(penpotUtils.getPageById(state.pageId));
if(ACTION.startsWith('bar:')){const[,v,m]=ACTION.split(':');return buildBar(+v,+m===1);}
if(ACTION.startsWith('polish:')){const key=ACTION.slice(7),p=penpot.currentPage.getShapeById(state.bars[key].id);for(const s of p.children){if(s.name==='Native / phase heading'||s.name==='Native / feedback and instruction')s.x=p.x+100;for(let i=0;i<8;i++){if(s.name==='Native optional width / '+tagNames[i])s.x=p.x+60+(i%4)*345+255;if(s.name==='Native tag label / '+tagNames[i])s.resize(205,40);}}return {key,polished:true};}
if(ACTION==='audit'){return Object.entries(state.bars).map(([key,b])=>{const p=penpot.currentPage.getShapeById(b.id);return {key,id:p.id,width:p.width,height:p.height,markerCount:p.children.filter(s=>s.name==='Marker / independent needle').length,zones:b.zones.map(z=>{const s=penpot.currentPage.getShapeById(z.id),precision=s.children.find(c=>c.name.startsWith('Precision / 18'));return {tag:z.tag,width:s.width,expected:z.width,precisionWidth:precision.width,precisionCentered:Math.abs((precision.x-s.x)-(s.width-precision.width)/2)<.01};})};});}
if(ACTION==='finish'){const review=penpot.currentPage.getShapeById(state.reviewId);penpot.selection=[review];penpot.viewport.zoomIntoView([review]);return state;}
throw Error('Unknown design action');
