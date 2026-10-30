const saved=penpot.currentPage.getPluginData('MFJ-R057');
const state=saved?JSON.parse(saved):{revision:'MFJ-R057',fileId:penpot.currentFile.id,pageId:penpot.currentPage.id,pageName:penpot.currentPage.name,screens:{},unityImported:false};
const b=penpot.currentPage.getShapeById(ID),sx=3840/1672,sy=2160/941;
if(state.screens[KIND]?.method==='fixed-corner-nine-slice')return state;
const old=b.children.find(s=>s.width>3800&&s.height>2100);
if(!old)throw Error('Background missing');
const foreground=[...b.children].filter(s=>s.id!==old.id);
let backup;
if(!penpot.currentPage.root.children.some(s=>s.name==='R057 comparison / original raster framing / '+KIND)){
 backup=b.clone();backup.name='R057 comparison / original raster framing / '+KIND;backup.x=b.x;backup.y=b.y+6000;
}
const root=penpot.createBoard();b.appendChild(root);root.name='MFJ-R057 / Sharp 4K independent frames / '+KIND;root.resize(3840,2160);root.x=b.x;root.y=b.y;root.fills=[];
const shells=[];
function cover(x,y,w,h){const r=penpot.createRectangle();root.appendChild(r);r.resize(w*sx,h*sy);r.x=b.x+x*sx;r.y=b.y+y*sy;r.fills=[{fillColor:'#10262E',fillOpacity:1}];r.name='Backing / hides enlarged review-frame pixels';}
function shell(name,key,x,y,w,h,outer=false){
 const m=MASTERS[key],frame=penpot.createBoard();root.appendChild(frame);frame.name='Reusable 9-slice / '+name;frame.fills=[];frame.resize(w*sx,h*sy);frame.x=b.x+x*sx;frame.y=b.y+y*sy;
 const src=300,edge=Math.min(220,Math.floor(w*sx*.2),Math.floor(h*sy*.43));
 const dx=[0,edge,w*sx-edge,w*sx],dy=[0,edge,h*sy-edge,h*sy];
 const ux=[0,src,m.width-src,m.width],uy=[0,src,m.height-src,m.height];
 for(let row=0;row<3;row++)for(let col=0;col<3;col++){
  if(outer&&row===1&&col===1)continue;
  const cw=dx[col+1]-dx[col],ch=dy[row+1]-dy[row];
  const clip=penpot.createBoard();frame.appendChild(clip);clip.name='Slice '+row+','+col;clip.fills=[];clip.resize(cw,ch);clip.x=frame.x+dx[col];clip.y=frame.y+dy[row];clip.clipContent=true;
  const image=penpot.currentPage.getShapeById(m.id).clone();clip.appendChild(image);image.name='Standalone '+key+' frame master / clipped, not cut from mockup';
  const kx=cw/(ux[col+1]-ux[col]),ky=ch/(uy[row+1]-uy[row]);
  image.resize(m.width*kx,m.height*ky);image.x=clip.x-ux[col]*kx;image.y=clip.y-uy[row]*ky;
 }
 shells.push({name,boardId:frame.id,master:key,target:[Math.round(w*sx),Math.round(h*sy)],sourceCorner:src,targetCorner:edge,cornerScale:edge/src});
 return frame;
}
// Retain the scenic illustration only; replace baked header, panel and perimeter framing.
cover(0,0,1672,105);cover(0,0,60,941);cover(1612,0,60,941);cover(0,906,1672,35);
shell('screen perimeter','Dark',0,0,1672,941,true);
shell('Harbor wordmark','Paper',68,25,400,73);
shell('navigation','Dark',510,20,760,77);
shell('gold balance','Paper',1350,25,249,73);
if(KIND==='harbor'){
 cover(48,526,1574,386);
 [62,588,1102].forEach((x,i)=>{shell('destination '+i,'Dark',x,534,i===1?496:506,226);shell('destination title '+i,'Paper',x+9,540,i===1?478:488,53);});
 shell('loadout','Dark',65,819,1080,82);shell('Set Sail','Paper',1160,819,440,82);
}else{
 cover(48,285,1575,628);
 shell('collection cabinet','Dark',65,289,1016,515);
 shell('cabinet filters','Paper',73,294,1000,57);
 shell('detail or deck','Dark',1105,289,503,515);
 shell('detail heading','Paper',1113,294,487,57);
 shell('primary action','Paper',1153,731,416,62);
 shell('loadout','Dark',65,819,1070,82);shell('return to Harbor','Paper',1160,819,440,82);
 if(KIND==='shop'){
  [94,424,759].forEach((x,i)=>{
   shell('equipment '+i,'Dark',x,359,i===1?318:308,370);
   shell('equipment art '+i,'Paper',x+5,364,i===1?308:298,178);
   shell('equipment title '+i,'Paper',x+5,545,i===1?308:298,46);
  });
  shell('selected equipment artwork','Paper',1112,352,489,191);
 }
}
for(const s of foreground)b.appendChild(s);
state.screens[KIND]={boardId:b.id,backupBoardId:backup?.id||null,frameLayerId:root.id,sourceArtId:old.id,foregroundIds:foreground.map(s=>s.id),method:'fixed-corner-nine-slice',shells,masterSourceCorner:300};
penpot.currentPage.setPluginData('MFJ-R057',JSON.stringify(state));penpot.selection=[b];penpot.viewport.zoomIntoView([b]);return state;
