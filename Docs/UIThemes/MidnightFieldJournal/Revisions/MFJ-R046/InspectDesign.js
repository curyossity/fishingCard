const d=storage.rarityR046,page=penpot.currentPage;
if(!d?.refined)return {ready:false};
const get=id=>page.getShapeById(id),checks=[];
for(const id of d.motionBoardIds){
 const record=d.records.find(r=>r.id===id),button=get(record.buttonId);
 const click=button.interactions.find(i=>i.trigger==='click');
 const target=click.action.destination;
 const marker=target.children.find(s=>s.name.startsWith('Dynamic / marker /'));
 const normalized=(marker.x-target.x+19-56)/1048;
 checks.push({motionId:id,targetId:target.id,sourcePosition:record.position,stoppedPosition:normalized,exact:Math.abs(normalized-record.position)<.00001});
}
if(!page.flows.some(f=>f.name==='R046 — Rarity timing'))page.createFlow('R046 — Rarity timing',get(d.prototypeStartId));
const roots=[d.reviewBoardId,d.contextBoardId,d.partsBoardId,...Object.values(d.revealBoardIds),...d.motionBoardIds,...Object.values(d.resultBoardIds),...d.exactFreezeFrames.map(x=>x.id)].map(get);
const overlaps=[];for(let i=0;i<roots.length;i++)for(let j=i+1;j<roots.length;j++){
 const a=roots[i],b=roots[j];if(a.id===b.id)continue;
 if(Math.min(a.x+a.width,b.x+b.width)-Math.max(a.x,b.x)>1&&Math.min(a.y+a.height,b.y+b.height)-Math.max(a.y,b.y)>1)overlaps.push([a.id,b.id]);
}
const review=get(d.reviewBoardId);penpot.selection=[review];penpot.viewport.zoomIntoView([review]);
storage.rarityR046Audit={ready:true,checks,overlappingTopLevelBoards:overlaps,flowName:'R046 — Rarity timing',prototypeViewerPlayed:false};
return storage.rarityR046Audit;
