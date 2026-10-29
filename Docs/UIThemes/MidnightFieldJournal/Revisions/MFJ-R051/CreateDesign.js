// Selected parchment layout: anchor cursor over the bar and in-channel fading precision areas.
const tags=['Fish','Predator','Schooling','Small','Heavy','Anchored','Armored','Elusive'];
const ink='#293D40',paper='#EEE0C3',sepia='#95734D',gold='#B28A46',ivory='#F4E9D3';
const headingFont=penpot.fonts.findByName('Cinzel'),bodyFont=penpot.fonts.findByName('Source Serif 4');
let state=storage.tagBarR051;
function createPrecision(z){const w=z.width*.18,p=penpot.createPath();p.name='Precision / upward fading area';p.d=`M0 60C${w*.15} 44 ${w*.3} 22 ${w*.5} 0C${w*.7} 22 ${w*.85} 44 ${w} 60Z`;z.appendChild(p);p.x=z.x+(z.width-w)/2;p.y=z.y+56;p.strokes=[];p.fills=[{fillOpacity:1,fillColorGradient:{type:'linear',startX:.5,startY:1,endX:.5,endY:0,width:1,stops:[{color:gold,opacity:.62,offset:0},{color:gold,opacity:.24,offset:.38},{color:gold,opacity:.04,offset:.72},{color:gold,opacity:0,offset:1}]}}];p.sendToBack();return p;}
if(ACTION==='initialize'){
 if(state&&penpot.currentPage.getShapeById(state.reviewId))return state;
 state=storage.tagBarR051={revision:'MFJ-R051',fileId:penpot.currentFile.id,pageId:penpot.currentPage.id,pageName:penpot.currentPage.name,sourceRevision:'MFJ-R050',sourceBarId:SOURCE.id,bars:{}};
 const r=board(null,'MFJ-R051 / Lantern Focus / anchor cursor and rising precision',42300,25000,1840,740);state.reviewId=r.id;
 rect(r,'Review / sea-ink background',0,0,1840,740,'#111E23');
 text(r,'Review title','LANTERN FOCUS — ANCHOR & LIGHT',100,48,1640,80,49,paper,true);
 text(r,'Review summary','An anchor cursor over the bar · eight rising precision areas · light parchment',103,145,1640,48,26,'#C8BBA3');
 text(r,'Review scope','Selected design refinement · initial state only · independent editable parts · no Unity changes',120,660,1600,44,25,'#C8BBA3');
 penpot.selection=[r];penpot.viewport.zoomIntoView([r]);return state;
}
if(!state)throw Error('Initialize first.');
if(ACTION==='refine'){
 if(state.bars['0']?.complete)return state.bars['0'];
 if(state.bars['0'])return {id:state.bars['0'].id,incomplete:true};
 const r=penpot.currentPage.getShapeById(state.reviewId),p=penpot.currentPage.getShapeById(SOURCE.id).clone();r.appendChild(p);p.x=r.x+120;p.y=r.y+210;p.name='Initial / Lantern Focus / anchor cursor and rising precision / current';state.bars['0']={id:p.id,complete:false};
 const track=p.children.find(s=>s.name==='Track / eight equal normalized regions'),zones=[];
 for(let i=0;i<8;i++){
  const z=track.children.find(s=>s.name==='Zone / '+tags[i]);
  for(const s of [...z.children])if(s.name.startsWith('Precision /'))s.remove();
  const area=createPrecision(z);
  zones.push({tag:tags[i],id:z.id,fraction:.125,precisionFraction:.18,precisionId:area.id,precisionOffsetY:56,precisionHeight:60});
 }
 for(const s of [...p.children])if(s.name.startsWith('Marker / independent'))s.remove();
 const position=.37,x=80+1440*position;
 const anchor='M26 172V202M14 181H38M6 187C6 199 16 205 26 205C36 205 46 199 46 187M6 187L3 194M6 187L13 190M46 187L49 194M46 187L39 190';
 const marker=svg(p,'Marker / independent anchor cursor / overlay',x-26,88,52,210,`<path d="M26 25V160" fill="none" stroke="${ivory}" stroke-width="7"/><path d="M26 25V160" fill="none" stroke="${ink}" stroke-width="3.5"/><circle cx="26" cy="16" r="11" fill="${paper}" stroke="${ivory}" stroke-width="6"/><circle cx="26" cy="16" r="11" fill="${paper}" stroke="${ink}" stroke-width="3"/><circle cx="26" cy="16" r="3" fill="${ink}"/><g fill="none" stroke-linecap="round" stroke-linejoin="round"><circle cx="26" cy="166" r="6" stroke="${ivory}" stroke-width="7"/><path d="${anchor}" stroke="${ivory}" stroke-width="7"/><circle cx="26" cy="166" r="6" stroke="${ink}" stroke-width="3.5"/><path d="${anchor}" stroke="${ink}" stroke-width="3.5"/></g>`);
 marker.bringToFront();
 const b={id:p.id,state:'initial',width:p.width,height:p.height,trackId:track.id,trackWidth:1440,zones,markerId:marker.id,markerPosition:position,markerStyle:'Ivory-edged dark line overlays the bar, with a nautical eyelet above and anchor below',activeTagHighlight:false,precisionPresentation:'Eight tapered ochre highlights rise from the channel bottom and fade near text height',complete:true};state.bars['0']=b;
 penpot.selection=[r];penpot.viewport.zoomIntoView([r]);return b;
}
if(ACTION==='audit'){
 const b=state.bars['0'],p=penpot.currentPage.getShapeById(b.id),track=penpot.currentPage.getShapeById(b.trackId),marker=penpot.currentPage.getShapeById(b.markerId);
 return {id:p.id,width:p.width,height:p.height,marker:{id:marker.id,width:marker.width,height:marker.height,visible:marker.visible,overlay:marker.parentIndex>track.parentIndex,top:marker.y-p.y,bottom:marker.y+marker.height-p.y},zones:b.zones.map(z=>{const s=penpot.currentPage.getShapeById(z.id),a=penpot.currentPage.getShapeById(z.precisionId);return {tag:z.tag,width:s.width,precisionWidth:a.width,precisionHeight:a.height,precisionTop:a.y-s.y,precisionBottom:a.y+a.height-s.y,centred:Math.abs(a.x-s.x-(s.width-a.width)/2)<.02,behindText:a.parentIndex<s.children.find(t=>t.name==='Native tag name').parentIndex,gradientStops:a.fills[0]?.fillColorGradient?.stops,oldBracketPresent:s.children.some(t=>t.name.includes('bracket')),activeHighlight:s.children.some(t=>t.name.startsWith('Feedback /'))};})};
}
if(ACTION==='finish'){const r=penpot.currentPage.getShapeById(state.reviewId);penpot.selection=[r];penpot.viewport.zoomIntoView([r]);return state;}
throw Error('Unknown action');
