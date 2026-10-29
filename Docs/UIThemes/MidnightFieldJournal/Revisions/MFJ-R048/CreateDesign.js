// Three initial-state visual alternatives only; no runtime or gameplay changes.
const tags=['Fish','Predator','Schooling','Small','Heavy','Anchored','Armored','Elusive'];
const ink='#293D40',paper='#EEE0C3',sepia='#95734D',gold='#B58A49',night='#17282C',ivory='#F4E9D3';
const titles=['The Engraved Ruler','The Tackle Labels','The Lantern Focus'];
const captions=['One continuous rail. Names sit where you aim; precision is a fine edge notch.',
 'Eight paper tackle labels on one fishing line. A short gold stitch marks each centre.',
 'One active tag at a time. Only the tag under the marker reveals its precision guide.'];
const headingFont=penpot.fonts.findByName('Cinzel'),bodyFont=penpot.fonts.findByName('Source Serif 4');
let state=storage.tagBarR048;
function board(parent,name,x,y,w,h){const s=penpot.createBoard();s.name=name;s.resize(w,h);s.fills=[];if(parent)parent.appendChild(s);s.x=(parent?.x||0)+x;s.y=(parent?.y||0)+y;return s;}
function rect(parent,name,x,y,w,h,c,opacity=1,r=0){const s=penpot.createRectangle();parent.appendChild(s);s.name=name;s.resize(w,h);s.x=parent.x+x;s.y=parent.y+y;s.fills=[{fillColor:c,fillOpacity:opacity}];s.borderRadius=r;return s;}
function text(parent,name,content,x,y,w,h,size,c=ink,heading=false,align='left'){const s=penpot.createText(content);parent.appendChild(s);s.name=name;const f=heading?headingFont:bodyFont;f.applyToText(s,f.variants.find(v=>v.fontWeight==='400'&&v.fontStyle==='normal'));s.fontSize=String(size);s.fills=[{fillColor:c,fillOpacity:1}];s.resize(w,h);s.x=parent.x+x;s.y=parent.y+y;s.align=align;return s;}
function svg(parent,name,x,y,w,h,paths,view=`0 0 ${w} ${h}`){const s=penpot.createShapeFromSvg(`<svg xmlns="http://www.w3.org/2000/svg" width="${w}" height="${h}" viewBox="${view}">${paths}</svg>`);parent.appendChild(s);s.name=name;s.x=parent.x+x;s.y=parent.y+y;return s;}
const glyphs=[
 '<path d="M5 16C11 7 22 7 28 16C22 25 11 25 5 16ZM5 16L1 10V22ZM22 14h.1"/>',
 '<path d="M3 12Q16 2 29 12L25 26H7ZM5 13L10 20L14 13L18 20L23 13M11 7l-3 -3M21 7l3 -3"/>',
 '<path d="M4 9Q10 2 17 9Q10 16 4 9L1 5v8ZM17 17Q23 10 30 17Q23 24 17 17L14 13v8ZM4 25Q10 18 17 25Q10 32 4 25L1 21v8Z"/>',
 '<path d="M10 16Q16 9 22 16Q16 23 10 16L7 12v8ZM3 6H0v20h3M29 6h3v20h-3"/>',
 '<path d="M10 10V7a6 6 0 0 1 12 0v3M7 11H25L29 29H3Z"/>',
 '<circle cx="16" cy="5" r="3"/><path d="M16 8V28M8 12h16M3 19Q3 29 16 29Q29 29 29 19M3 19l-2 5M3 19l5 2M29 19l2 5M29 19l-5 2"/>',
 '<path d="M16 2L29 8V17Q27 27 16 31Q5 27 3 17V8ZM16 7V26M8 11h16M9 17h14M12 23h8"/>',
 '<path d="M25 3Q8 3 8 16Q8 29 25 29Q17 24 17 16Q17 8 25 3ZM1 10h4M0 17h5M1 24h4"/>'
];
function icon(parent,i,x,y,size,c=ink){return svg(parent,'Tag glyph / '+tags[i],x,y,size,size,`<g fill="none" stroke="${c}" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round">${glyphs[i]}</g>`,'0 0 32 32');}
function frame(parent,dark=false){
 const crop=board(parent,'Static / parchment grain',0,0,1600,400);crop.clipContent=true;crop.borderRadius=24;
 const src=penpot.currentPage.getShapeById('bae680cd-9225-8018-8008-d1c2678546b4'),im=src.clone();crop.appendChild(im);im.resize(1190*1600/550,1322*400/642);im.x=crop.x-320*1600/550;im.y=crop.y-340*400/642;im.name='Approved clean paper texture';
 if(dark)rect(crop,'Sea-ink wash',0,0,1600,400,night,.97);
 const edge=dark?'#9D8966':sepia;
 svg(parent,'Static / quiet engraved frame',4,4,1592,392,`<rect x="1" y="1" width="1590" height="390" rx="21" fill="none" stroke="${edge}" stroke-width="2"/><rect x="9" y="9" width="1574" height="374" rx="15" fill="none" stroke="${edge}" stroke-width=".7"/>`);
 const corner='<path d="M0 42V0H42M7 34V7H34M15 30Q16 16 30 15M21 14Q17 7 24 5Q32 8 28 16"/>';
 for(const[x,y,sx,sy]of[[23,23,1,1],[1535,23,-1,1],[23,335,1,-1],[1535,335,-1,-1]])svg(parent,'Static / fixed engraved corner',x,y,42,42,`<g transform="translate(${sx<0?42:0} ${sy<0?42:0}) scale(${sx} ${sy})" fill="none" stroke="${edge}" stroke-width="1">${corner}</g>`);
}
function stop(parent,dark=false){const s=board(parent,'Control / Stop / native',1260,312,260,62);rect(s,'Stop face',0,0,260,62,dark?paper:ink,1,5);svg(s,'Fine inner rule',5,5,250,52,`<rect x="1" y="1" width="248" height="50" rx="3" fill="none" stroke="${dark?ink:paper}" stroke-opacity=".55"/>`);rect(s,'Stop emblem',32,25,12,12,dark?ink:paper);text(s,'Stop label','STOP',66,12,160,40,27,dark?ink:paper,true,'center');return s;}
function marker(parent,x,y,h,dark=false,float=false){
 const stroke=dark?ivory:ink;
 const path=float?`<path d="M15 1V20M15 20C-3 29 1 47 15 56C29 47 33 29 15 20Z" fill="${ivory}" stroke="${ink}" stroke-width="2"/><path d="M5 37H25" stroke="${gold}" stroke-width="3"/>`:`<path d="M15 1L28 15L15 29L2 15Z" fill="${dark?ivory:paper}" stroke="${stroke}" stroke-width="2"/><path d="M15 28V${h-4}" stroke="${stroke}" stroke-width="5"/><path d="M15 28V${h-4}" stroke="${dark?night:ivory}" stroke-width="2"/>`;
 return svg(parent,'Marker / independent '+(float?'fishing float':'ivory needle'),x-15,y,30,h,path);
}
function createBar(v){
 if(state.bars[v]?.complete)return state.bars[v];
 if(state.bars[v])return {incomplete:true,id:state.bars[v].id};
 const row=penpot.currentPage.getShapeById(state.rowIds[v]);
 const p=board(row,'Initial / '+titles[v]+' / native reusable bar',0,122,1600,400);state.bars[v]={id:p.id,complete:false};
 const dark=v===2;frame(p,dark);const fg=dark?ivory:ink;
 text(p,'Native / heading',['READ THE WATER','CHOOSE YOUR CATCH','FOLLOW THE LIGHT'][v],80,31,1150,50,34,fg,true);
 text(p,'Native / phase','CHOOSE A TAG',1225,40,290,37,21,dark?'#CCBDA1':sepia,true,'right');
 const track=board(p,'Track / eight equal normalized regions',80,v===0?125:v===1?92:118,1440,v===0?120:v===1?186:154);
 const zones=[],w=180,precisionFraction=.18;
 if(v===0){
  rect(track,'One continuous sea-ink rail',0,0,1440,24,ink,1,5);
  for(let i=0;i<8;i++){
   const z=board(track,'Zone / '+tags[i],i*w,0,w,120);z.clipContent=true;
   if(i)rect(z,'Boundary / quiet hairline',0,0,1.5,24,paper,.85);
   const precision=rect(z,'Precision / quiet gold edge notch',(w-w*.18)/2,20,w*.18,4,gold);
   icon(z,i,73,44,34);text(z,'Native tag name',tags[i],0,83,w,36,26,ink,true,'center');
   zones.push({tag:tags[i],id:z.id,precisionId:precision.id,precisionVisible:true,fraction:.125});
  }
  marker(p,80+1440*.29,91,78);
 }else if(v===1){
  svg(track,'One continuous fishing line',0,8,1440,12,`<path d="M0 6H1440" stroke="${sepia}" stroke-width="3"/><path d="M0 3H1440" stroke="${ivory}" stroke-width="1"/>`);
  rect(track,'One continuous timing rail',0,140,1440,5,ink,1,2);
  for(let i=0;i<8;i++){
   const z=board(track,'Zone / '+tags[i],i*w,0,w,186);z.clipContent=true;
   const labelFace=svg(z,'Independent / tackle label face',8,29,164,95,`<path d="M11 1H153L163 12V83L153 94H11L1 83V12Z" fill="${paper}" stroke="${sepia}" stroke-width="1.5"/><path d="M14 6H150L158 15V80L150 89H14L6 80V15Z" fill="none" stroke="${sepia}" stroke-opacity=".35"/>`);
   svg(z,'Static / paper fastening',82,8,16,29,`<path d="M8 0V21" stroke="${sepia}" stroke-width="1.5"/><circle cx="8" cy="23" r="3" fill="${ink}"/>`);
   icon(z,i,74,41,32);text(z,'Native tag name',tags[i],9,81,162,35,25,ink,true,'center');
   if(i)rect(z,'Boundary / rail tick',0,135,1.5,15,sepia,.75);
   const precision=rect(z,'Precision / gold stitch',(w-w*.18)/2,140,w*.18,5,gold);
   zones.push({tag:tags[i],id:z.id,precisionId:precision.id,precisionVisible:true,fraction:.125});
  }
  marker(p,80+1440*.29,225,58,false,true);
 }else{
  rect(track,'One continuous dark channel',0,0,1440,116,'#101E22',1,7);
  svg(track,'Channel / thin outline',0,0,1440,116,`<rect x="1" y="1" width="1438" height="114" rx="7" fill="none" stroke="#A49477" stroke-width="1"/>`);
  for(let i=0;i<8;i++){
   const z=board(track,'Zone / '+tags[i],i*w,0,w,154);z.clipContent=true;const active=i===0;
   if(active)rect(z,'Feedback / active tag paper wash',3,3,w-6,110,paper,1,5);
   if(i)rect(z,'Boundary / quiet divider',0,15,1,86,ivory,.16);
   icon(z,i,74,20,32,active?ink:'#CBBFA8');text(z,'Native tag name',tags[i],0,66,w,38,25,active?ink:ivory,true,'center');
   const precision=rect(z,'Precision / reveal only under marker',(w-w*.18)/2,130,w*.18,3,'#D4B06E',active?1:0);
   if(active)svg(z,'Precision / active centre bracket',w/2-18,123,36,12,`<path d="M1 1V10H35V1M18 6V15" fill="none" stroke="#D4B06E" stroke-width="1.4"/>`);
   zones.push({tag:tags[i],id:z.id,precisionId:precision.id,precisionVisible:active,fraction:.125});
  }
  marker(p,80+1440*.07,87,40,true);
 }
 text(p,'Native / instruction',['Stop on a tag. Aim for its fine gold notch.','Stop the float beneath a tag. Aim for the gold stitch.','Stop on a tag. Its centre guide appears as you pass.'][v],80,322,1120,42,26,fg);
 const control=stop(p,dark);
 const result={id:p.id,variant:v,title:titles[v],state:'initial',width:1600,height:400,trackId:track.id,trackWidth:1440,tagColour:ink,palettePolicy:'One shared tag palette; gold is reserved for precision, not tag identity.',precisionFraction,zones,markerPosition:v===2?.07:.29,precisionPresentation:['Fine edge notch','Short stitch on fishing line','Only current zone exposes guide'][v],stopId:control.id,complete:true};state.bars[v]=result;return result;
}
if(ACTION==='initialize'){
 if(state&&penpot.currentPage.getShapeById(state.reviewId))return state;
 state=storage.tagBarR048={revision:'MFJ-R048',fileId:penpot.currentFile.id,pageId:penpot.currentPage.id,pageName:penpot.currentPage.name,bars:{},rowIds:[]};
 const review=board(null,'MFJ-R048 / Quiet tag bars / three initial-state ideas',36000,25000,1840,2160);state.reviewId=review.id;rect(review,'Review / sea-ink background',0,0,1840,2160,'#111E23');
 text(review,'Review title','A QUIETER CAST',100,55,1640,90,64,paper,true);
 text(review,'Review introduction','Three initial-state directions · one shared palette · eight equal tags',103,165,1640,55,30,'#C8BBA3');
 for(let i=0;i<3;i++){const row=board(review,`${i+1} / ${titles[i]} / initial only`,120,300+i*590,1600,530);state.rowIds.push(row.id);text(row,'Review / direction title',`${String(i+1).padStart(2,'0')} — ${titles[i].toUpperCase()}`,0,0,1600,47,33,paper,true);text(row,'Review / direction rationale',captions[i],0,61,1600,40,28,'#C8BBA3');}
 text(review,'Review / visual-only note','Initial state only: eight equal 12.5% spans. Precision remains 18% of each span for visual comparison.',120,2080,1600,45,25,'#C8BBA3');
 penpot.selection=[review];penpot.viewport.zoomIntoView([review]);return state;
}
if(!state)throw Error('Initialize the comparison first.');
if(penpot.currentPage.id!==state.pageId)throw Error('Open the existing reusable assembly page before continuing.');
if(ACTION.startsWith('bar:'))return createBar(+ACTION.split(':')[1]);
if(ACTION==='audit')return Object.values(state.bars).map(b=>{const p=penpot.currentPage.getShapeById(b.id);return {id:p.id,initialOnly:b.state==='initial',width:p.width,height:p.height,markers:p.children.filter(s=>s.name.startsWith('Marker / independent')).length,zones:b.zones.map(z=>{const s=penpot.currentPage.getShapeById(z.id),r=penpot.currentPage.getShapeById(z.precisionId);return {tag:z.tag,width:s.width,precisionWidth:r.width,precisionVisible:z.precisionVisible,centred:Math.abs(r.x-s.x-(s.width-r.width)/2)<.01};})};});
if(ACTION==='finish'){const review=penpot.currentPage.getShapeById(state.reviewId);penpot.selection=[review];penpot.viewport.zoomIntoView([review]);return state;}
throw Error('Unknown action');
