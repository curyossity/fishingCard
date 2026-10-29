// Refinement of the selected R048 Lantern Focus; no Unity changes.
const tags=['Fish','Predator','Schooling','Small','Heavy','Anchored','Armored','Elusive'];
const ink='#293D40',paper='#EEE0C3',sepia='#95734D',gold='#A77C3C',ivory='#F4E9D3';
const headingFont=penpot.fonts.findByName('Cinzel'),bodyFont=penpot.fonts.findByName('Source Serif 4');
let state=storage.tagBarR050;
if(ACTION==='initialize'){
 if(state&&penpot.currentPage.getShapeById(state.reviewId))return state;
 state=storage.tagBarR050={revision:'MFJ-R050',fileId:penpot.currentFile.id,pageId:penpot.currentPage.id,pageName:penpot.currentPage.name,sourceRevision:'MFJ-R048',sourceBarId:SOURCE.id,bars:{}};
 const r=board(null,'MFJ-R050 / Lantern Focus / light parchment refinement',40250,25000,1840,740);state.reviewId=r.id;
 rect(r,'Review / sea-ink background',0,0,1840,740,'#111E23');
 text(r,'Review title','LANTERN FOCUS — PARCHMENT',100,48,1640,80,52,paper,true);
 text(r,'Review summary','Eight marked centres · one shared palette · no tag highlight · a clear independent cursor',103,145,1640,48,26,'#C8BBA3');
 text(r,'Review scope','Selected Lantern Focus refinement · initial state only · editable native parts · no Unity changes',120,660,1600,44,25,'#C8BBA3');
 penpot.selection=[r];penpot.viewport.zoomIntoView([r]);return state;
}
if(!state)throw Error('Initialize first.');
if(ACTION==='refine'){
 if(state.bars['0']?.complete)return state.bars['0'];
 if(state.bars['0'])return {id:state.bars['0'].id,incomplete:true};
 const r=penpot.currentPage.getShapeById(state.reviewId),source=penpot.currentPage.getShapeById(SOURCE.id),p=source.clone();r.appendChild(p);p.x=r.x+120;p.y=r.y+210;p.name='Initial / Lantern Focus / light parchment / current design';
 state.bars['0']={id:p.id,complete:false};
 const grain=p.children.find(s=>s.name==='Static / parchment grain');grain.children.find(s=>s.name==='Sea-ink wash')?.remove();
 p.children.find(s=>s.name==='Native / heading').fills=[{fillColor:ink,fillOpacity:1}];
 p.children.find(s=>s.name==='Native / phase').fills=[{fillColor:sepia,fillOpacity:1}];
 p.children.find(s=>s.name==='Native / instruction').remove();
 text(p,'Native / instruction','Stop on a tag. Aim for its marked centre.',80,322,1120,42,26,ink);
 const track=p.children.find(s=>s.name==='Track / eight equal normalized regions');
 const channel=track.children.find(s=>s.name==='One continuous dark channel');channel.name='Shared parchment channel / no active-tag fill';channel.fills=[{fillColor:'#E4D3B1',fillOpacity:.55}];
 const zones=[];
 for(let i=0;i<8;i++){
  const z=track.children.find(s=>s.name==='Zone / '+tags[i]);
  for(const s of [...z.children]){
   if(s.name.startsWith('Feedback /')||s.name.startsWith('Precision /')||s.name.startsWith('Tag glyph /'))s.remove();
   else if(s.name==='Native tag name')s.fills=[{fillColor:ink,fillOpacity:1}];
   else if(s.name==='Boundary / quiet divider')s.fills=[{fillColor:ink,fillOpacity:.18}];
  }
  icon(z,i,74,20,32,ink);
  const w=z.width*.18,x=(z.width-w)/2;
  const precision=rect(z,'Precision / permanent centre section',x,125,w,12,gold,.15);
  const outline=svg(z,'Precision / permanent centre bracket',x,124,w,17,`<path d="M1 1V12H${w-1}V1M${w/2} 8V16" fill="none" stroke="${gold}" stroke-width="1.7"/>`);
  zones.push({tag:tags[i],id:z.id,fraction:.125,precisionFraction:.18,precisionId:precision.id,precisionOutlineId:outline.id});
 }
 for(const s of [...p.children])if(s.name.startsWith('Marker / independent'))s.remove();
 const position=.32,cursorX=80+1440*position;
 const cursor=svg(p,'Marker / independent high-contrast cursor',cursorX-20,84,40,216,`<g fill="${ink}" stroke="${ivory}" stroke-width="2.5" stroke-linejoin="round"><path d="M20 1L39 20L20 39L1 20Z"/><path d="M20 190L37 213H3Z"/></g><path d="M20 39V46M20 184V190" stroke="${ink}" stroke-width="4"/>`);
 const stop=p.children.find(s=>s.name==='Control / Stop / independent')||p.children.find(s=>s.name==='Control / Stop / native');
 stop.children.find(s=>s.name==='Stop face').fills=[{fillColor:ink,fillOpacity:1}];
 (stop.children.find(s=>s.name==='Stop label')||stop.children.find(s=>s.name==='Native Stop label')).fills=[{fillColor:paper,fillOpacity:1}];
 stop.children.find(s=>s.name==='Stop emblem').fills=[{fillColor:paper,fillOpacity:1}];
 const oldRule=stop.children.find(s=>s.name==='Stop / etched inner rule')||stop.children.find(s=>s.name==='Fine inner rule');oldRule?.remove();
 svg(stop,'Stop / parchment inner rule',4,4,252,54,`<rect x="1" y="1" width="250" height="52" rx="3" fill="none" stroke="${paper}" stroke-opacity=".5"/>`);
 const b={id:p.id,state:'initial',width:p.width,height:p.height,trackId:track.id,trackWidth:1440,zones,markerId:cursor.id,markerPosition:position,markerSize:{width:40,height:216},activeTagHighlight:false,precisionPresentation:'Eight permanent centre brackets below the tag labels',palettePolicy:'Light parchment, shared ink glyphs/text, restrained ochre centre marks',complete:true};state.bars['0']=b;
 penpot.selection=[r];penpot.viewport.zoomIntoView([r]);return b;
}
if(ACTION==='audit'){
 const b=state.bars['0'],p=penpot.currentPage.getShapeById(b.id),track=penpot.currentPage.getShapeById(b.trackId),marker=penpot.currentPage.getShapeById(b.markerId);
 return {id:p.id,width:p.width,height:p.height,marker:{id:marker.id,width:marker.width,height:marker.height,visible:marker.visible},darkWashPresent:p.children.find(s=>s.name==='Static / parchment grain').children.some(s=>s.name==='Sea-ink wash'),zones:b.zones.map(z=>{const s=penpot.currentPage.getShapeById(z.id),c=penpot.currentPage.getShapeById(z.precisionId);return {tag:z.tag,width:s.width,precisionWidth:c.width,precisionVisible:c.visible&&c.fills.some(f=>f.fillOpacity>0),centred:Math.abs(c.x-s.x-(s.width-c.width)/2)<.01,tagColour:s.children.find(t=>t.name==='Native tag name').fills[0].fillColor,activeHighlight:s.children.some(t=>t.name.startsWith('Feedback /'))};})};
}
if(ACTION==='finish'){const r=penpot.currentPage.getShapeById(state.reviewId);penpot.selection=[r];penpot.viewport.zoomIntoView([r]);return state;}
throw Error('Unknown action');
