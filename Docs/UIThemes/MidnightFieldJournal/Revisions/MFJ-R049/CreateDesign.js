// Alternate instrument forms. Static concepts only; no runtime path/motion changes.
const tags=['Fish','Predator','Schooling','Small','Heavy','Anchored','Armored','Elusive'];
const ink='#293D40',paper='#EEE0C3',sepia='#95734D',gold='#B58A49',night='#17282C',ivory='#F4E9D3';
const headingFont=penpot.fonts.findByName('Cinzel'),bodyFont=penpot.fonts.findByName('Source Serif 4');
const titles=['The Tide Dial','The Line Spool','The Sounding Chart'];
const captions=['A crescent tide instrument. The needle follows the curve instead of a rectangular strip.',
 'A travelling ribbon beneath a fixed index. Three tags are visible; the rest live on the spool.',
 'A miniature sea chart. A boat follows one sounding route through eight named bearings.'];
let state=storage.tagBarR049;
function frame(p,dark=false){
 const crop=board(p,'Static / paper grain',0,0,1600,600);crop.clipContent=true;crop.borderRadius=24;
 const im=penpot.currentPage.getShapeById('bae680cd-9225-8018-8008-d1c2678546b4').clone();crop.appendChild(im);im.resize(1190*1600/550,1322*600/642);im.x=crop.x-320*1600/550;im.y=crop.y-340*600/642;im.name='Approved clean paper texture';
 if(dark)rect(crop,'Sea-ink wash',0,0,1600,600,night,.97);
 svg(p,'Static / instrument frame',4,4,1592,592,`<rect x="1" y="1" width="1590" height="590" rx="21" fill="none" stroke="${sepia}" stroke-width="2"/><rect x="9" y="9" width="1574" height="574" rx="15" fill="none" stroke="${sepia}" stroke-opacity=".55" stroke-width=".8"/>`);
 text(p,'Native / phase','CHOOSE A TAG',1250,43,270,40,21,dark?paper:sepia,true,'right');
}
function stop(p,dark=false){const s=board(p,'Control / Stop',1260,500,260,64);rect(s,'Stop face',0,0,260,64,dark?paper:ink,1,5);text(s,'Native Stop','STOP',40,13,180,40,27,dark?ink:paper,true,'center');svg(s,'Fine inner rule',5,5,250,54,`<rect x="1" y="1" width="248" height="52" rx="3" fill="none" stroke="${dark?ink:paper}" stroke-opacity=".5"/>`);return s;}
function pathLine(points){return points.map((p,i)=>`${i?'L':'M'}${p[0].toFixed(2)} ${p[1].toFixed(2)}`).join('');}
function create(v){
 if(state.bars[v]?.complete)return state.bars[v];
 if(state.bars[v])return {id:state.bars[v].id,incomplete:true};
 const row=penpot.currentPage.getShapeById(state.rowIds[v]),p=board(row,'Initial / '+titles[v]+' / native concept',0,125,1600,600);state.bars[v]={id:p.id,complete:false};
 frame(p,v===1);text(p,'Native / heading',['READ THE TIDE','LET THE LINE RUN','FOLLOW THE SOUNDINGS'][v],80,31,1110,52,35,v===1?ivory:ink,true);
 const zones=[];let markerId,guideCount=0;
 if(v===0){
  const y=t=>190+190*Math.pow(2*t-1,2),curve=Array.from({length:121},(_,i)=>[80+i*12,y(i/120)]);
  svg(p,'Static / crescent rail',72,178,1456,216,`<path d="${pathLine(curve.map(([x,yy])=>[x-72,yy-178]))}" stroke="${ink}" stroke-width="22" fill="none" stroke-linecap="round"/><path d="${pathLine(curve.map(([x,yy])=>[x-72,yy-185]))}" stroke="${sepia}" stroke-width="1.5" fill="none"/>`);
  for(let i=0;i<8;i++){
   const t=(i+.5)/8,x=80+1440*t,yy=y(t);
   const z=board(p,'Zone / '+tags[i],x-90,yy-100,180,123);
   icon(z,i,73,0,34);text(z,'Native tag name',tags[i],0,43,180,38,25,ink,true,'center');
   const samples=Array.from({length:9},(_,j)=>{const tt=t+(.18/8)*(j/8-.5);return [80+1440*tt-z.x+p.x,y(tt)-z.y+p.y];});
   svg(z,'Precision / fine arc notch',0,0,180,123,`<path d="${pathLine(samples)}" fill="none" stroke="${gold}" stroke-width="4"/>`);
   zones.push({tag:tags[i],id:z.id,fraction:.125,visible:true,precisionFraction:.18});
  }
  for(let i=0;i<=8;i++){const t=i/8,x=80+1440*t,yy=y(t);svg(p,'Boundary / crescent hairline',x-2,yy-16,4,32,`<path d="M2 0V32" stroke="${paper}" stroke-width="1.4"/>`);}
  const t=.355,x=80+1440*t,yy=y(t),base=[800,461],vx=x-base[0],vy=yy-base[1],len=Math.hypot(vx,vy),px=-vy/len*6,py=vx/len*6;
  const m=svg(p,'Marker / independent tide needle',0,0,1600,490,`<path d="M${base[0]+px} ${base[1]+py}L${x} ${yy-9}L${base[0]-px} ${base[1]-py}Z" fill="${ivory}" stroke="${ink}" stroke-width="2"/><circle cx="800" cy="461" r="16" fill="${ink}" stroke="${sepia}" stroke-width="3"/><circle cx="800" cy="461" r="5" fill="${paper}"/>`);markerId=m.id;guideCount=8;
  text(p,'Instrument / low tide','LOW TIDE',82,414,230,35,21,sepia,true);text(p,'Instrument / high tide','HIGH TIDE',1290,414,230,35,21,sepia,true,'right');
  text(p,'Native / instruction','Stop the needle on a tag. The fine gold notch marks precision.',80,513,1130,43,26,ink);
 }else if(v===1){
  // The viewport is intentionally cropped: it previews a different presentation, not a new pool.
  svg(p,'Static / spool side cheeks',112,132,1376,310,`<g fill="#203237" stroke="#9D8966" stroke-width="2"><ellipse cx="110" cy="155" rx="95" ry="150"/><ellipse cx="1266" cy="155" rx="95" ry="150"/></g><g fill="none" stroke="#A69678" stroke-opacity=".45"><ellipse cx="110" cy="155" rx="66" ry="126"/><ellipse cx="1266" cy="155" rx="66" ry="126"/>${Array.from({length:8},(_,i)=>`<path d="M${70+i*11} 65V245M${1226+i*11} 65V245"/>`).join('')}</g>`);
  const viewport=board(p,'Viewport / clipped travelling ribbon',332,166,936,244);viewport.clipContent=true;viewport.borderRadius=12;
  rect(viewport,'Independent / parchment ribbon',0,0,936,244,paper,1,12);
  for(let i=0;i<8;i++){
   const display=i-1,z=board(viewport,'Zone / '+tags[i]+' / travelling ribbon',display*312,0,312,244);const visible=i>=1&&i<=3;
   icon(z,i,132,40,48);text(z,'Native tag name',tags[i],8,112,296,50,35,ink,true,'center');
   svg(z,'Static / ribbon stitching',0,0,312,244,`<path d="M0 18H312M0 226H312" stroke="${sepia}" stroke-opacity=".45" stroke-dasharray="3 7"/><path d="M311 32V212" stroke="${sepia}" stroke-opacity=".25"/>`);
   const alpha=i===2?1:0;rect(z,'Precision / ribbon centre stitch',(312-56.16)/2,190,56.16,4,gold,alpha);
   zones.push({tag:tags[i],id:z.id,fraction:.125,visible,precisionFraction:.18});
  }
  svg(p,'Static / ribbon edge',330,164,940,248,`<rect x="1" y="1" width="938" height="246" rx="13" fill="none" stroke="${sepia}" stroke-width="2"/>`);
  const m=svg(p,'Marker / independent fixed spool index',775,133,50,310,`<path d="M25 0L39 22H11ZM25 310L39 288H11Z" fill="${ivory}" stroke="${gold}" stroke-width="1.5"/><path d="M25 23V33M25 277V287" stroke="${ivory}" stroke-width="2"/>`);markerId=m.id;guideCount=1;
  text(p,'Native / ribbon motion cue','THE RIBBON TRAVELS · THE INDEX STAYS',390,442,820,36,22,'#CDBFA5',true,'center');
  text(p,'Native / instruction','Stop the ribbon beneath the index. Align a tag’s centre stitch.',80,513,1130,43,26,ivory);
 }else{
  // Each stop occupies one equal horizontal phase, despite the chart's meandering line.
  const y=t=>278+54*Math.sin(t*Math.PI*3-.6),pts=Array.from({length:181},(_,i)=>[80+1440*i/180,y(i/180)]);
  svg(p,'Static / chart contour engraving',66,105,1468,340,`<g fill="none" stroke="${sepia}" stroke-opacity=".14" stroke-width="1">${Array.from({length:6},(_,i)=>`<path d="M0 ${65+i*10}C270 ${-80+i*19} 420 ${190+i*10} 710 ${80+i*10}S1170 ${30+i*9} 1468 ${90+i*7}"/><path d="M0 ${240+i*9}C310 ${160+i*11} 510 ${360+i*10} 860 ${245+i*9}S1240 ${310+i*7} 1468 ${245+i*10}"/>`).join('')}</g>`);
  svg(p,'One continuous sounding route',70,200,1460,160,`<path d="${pathLine(pts.map(([x,yy])=>[x-70,yy-200]))}" fill="none" stroke="${ink}" stroke-width="3" stroke-dasharray="4 6" stroke-linecap="round"/>`);
  for(let i=0;i<8;i++){
   const t=(i+.5)/8,x=80+1440*t,yy=y(t),above=i%2===0,z=board(p,'Zone / '+tags[i]+' / chart bearing',x-90,yy-120,180,240);
   const iconY=above?11:145,labelY=above?54:188;
   icon(z,i,73,iconY,34);text(z,'Native tag name',tags[i],0,labelY,180,36,25,ink,true,'center');
   svg(z,'Static / bearing leader',89,above?94:126,2,20,`<path d="M1 0V20" stroke="${sepia}" stroke-opacity=".7"/>`);
   const samples=Array.from({length:9},(_,j)=>{const tt=t+(.18/8)*(j/8-.5);return [80+1440*tt-x+90,y(tt)-yy+120];});
   svg(z,'Precision / sounding target',0,0,180,240,`<path d="${pathLine(samples)}" stroke="${gold}" stroke-width="5" fill="none" stroke-linecap="round"/><circle cx="90" cy="120" r="7" fill="${paper}" stroke="${ink}" stroke-width="1.7"/>`);
   zones.push({tag:tags[i],id:z.id,fraction:.125,visible:true,precisionFraction:.18});
  }
  const t=.04,x=80+1440*t,yy=y(t),m=svg(p,'Marker / independent chart boat',x-20,yy-31,40,48,`<path d="M7 27H35L29 38H12ZM21 2V25M20 4L6 23H20ZM24 8L34 23H24Z" fill="${ink}" stroke="${paper}" stroke-width="1.4"/><path d="M4 43H34" stroke="${sepia}" stroke-width="1.5"/>`);markerId=m.id;guideCount=8;
  text(p,'Native / instruction','Stop the boat at a bearing. Its gold sounding marks precision.',80,513,1130,43,26,ink);
 }
 const control=stop(p,v===1);
 const result={id:p.id,variant:v,title:titles[v],state:'initial',width:1600,height:600,zones,markerId,guideCount,visibleTags:zones.filter(z=>z.visible).map(z=>z.tag),stopId:control.id,complete:true,palettePolicy:'Shared ink, parchment and restrained gold; no per-tag colours.',interactionProposal:['Needle follows a crescent','Ribbon moves beneath fixed index','Boat follows chart route'][v]};state.bars[v]=result;return result;
}
if(ACTION==='initialize'){
 if(state&&penpot.currentPage.getShapeById(state.reviewId))return state;
 state=storage.tagBarR049={revision:'MFJ-R049',fileId:penpot.currentFile.id,pageId:penpot.currentPage.id,pageName:penpot.currentPage.name,bars:{},rowIds:[]};
 const r=board(null,'MFJ-R049 / Fishing instruments / three new forms',38200,25000,1840,2780);state.reviewId=r.id;rect(r,'Review / sea-ink background',0,0,1840,2780,'#111E23');
 text(r,'Review title','THREE WAYS TO READ THE SEA',100,55,1640,90,52,paper,true);
 text(r,'Review introduction','Initial-state explorations · one palette · different forms and motion cues',103,165,1640,55,29,'#C8BBA3');
 for(let i=0;i<3;i++){const row=board(r,`${i+1} / ${titles[i]} / initial only`,120,300+i*790,1600,735);state.rowIds.push(row.id);text(row,'Review / title',`${String(i+1).padStart(2,'0')} — ${titles[i].toUpperCase()}`,0,0,1600,47,33,paper,true);text(row,'Review / concept',captions[i],0,61,1600,42,27,'#C8BBA3');}
 text(r,'Review / scope','Visual proposals only. Eight equal tag phases; no odds or Unity changes. Alternate motion paths are not implemented.',120,2700,1600,45,24,'#C8BBA3');penpot.selection=[r];penpot.viewport.zoomIntoView([r]);return state;
}
if(!state)throw Error('Initialize first.');
if(ACTION.startsWith('bar:'))return create(+ACTION.split(':')[1]);
if(ACTION==='audit')return Object.values(state.bars).map(b=>({id:b.id,exists:!!penpot.currentPage.getShapeById(b.id),state:b.state,logicalTagCount:b.zones.length,visibleCount:b.visibleTags.length,equalPhases:b.zones.every(z=>z.fraction===.125),markerExists:!!penpot.currentPage.getShapeById(b.markerId),zonesExist:b.zones.every(z=>!!penpot.currentPage.getShapeById(z.id))}));
if(ACTION==='finish'){const r=penpot.currentPage.getShapeById(state.reviewId);penpot.selection=[r];penpot.viewport.zoomIntoView([r]);return state;}
throw Error('Unknown action');
