import json
from Build import call,folder
code="""
const state=JSON.parse(penpot.currentPage.getPluginData('MFJ-R056'));
const b=penpot.currentPage.getShapeById(state.boardId),sx=3840/1672,sy=2160/941;
for(let i=0;i<state.cardInstances.length;i++){
 const data=state.cardInstances[i],c=penpot.currentPage.getShapeById(data.boardId);
 const x=112.5+(i%4)*240,y=358+Math.floor(i/4)*221;
 const factor=190/205;
 for(const s of c.children){
  const rx=s.x-c.x,ry=s.y-c.y;
  s.resize(s.width*factor,s.height*factor);s.x=c.x+rx*factor;s.y=c.y+ry*factor;
  if(s.type==='text')s.fontSize=String(Number(s.fontSize)*factor);
 }
 c.resize(190*sx,190*sx*640/620);c.x=b.x+x*sx;c.y=b.y+y*sy;
 const actualRules=c.children.filter(s=>s.type==='text').at(-1);
 actualRules.fontSize=String(28*190*sx/620);
 if(data.id==='clear_water')actualRules.characters='Reveal a hidden Hooked\\ncreature; otherwise remove\\n1 pending concealment.';
 const stateText=data.illustrativeState==='locked'?'LOCKED · 120 GOLD':data.illustrativeState==='owned'?'OWNED · NOT IN DECK':data.deckCopies+' IN DECK';
 const badge=b.children.filter(s=>s.name==='Editable / '+stateText).sort((a,z)=>a.x-z.x||a.y-z.y).find(s=>Math.abs((s.x-b.x)/sx-(105+(i%4)*240-3))<2&&Math.abs((s.y-b.y)/sy-(354+Math.floor(i/4)*233+210))<2);
 if(badge){badge.x=b.x+(x-10)*sx;badge.y=b.y+(y+198)*sy;badge.resize(210*sx,20*sy);}
}
for(const s of b.children){
 if(s.name?.startsWith('Editable / ')&&Math.abs((s.x-b.x)/sx-1163)<2&&(s.y-b.y)/sy>=486&&(s.y-b.y)/sy<720){s.x=b.x+1215*sx;s.resize(190*sx,s.height);}
 if(s.name==='Editable / ⌑')s.characters='—';
}
penpot.selection=[b];penpot.viewport.zoomIntoView([b]);return {boardId:b.id,cards:state.cardInstances.length};
"""
print(call('execute_code',{'code':code}))
