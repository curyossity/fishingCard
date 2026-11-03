const p=penpot.currentPage,d=JSON.parse(p.getSharedPluginData('fishing-cards-design','MFJ-R061'));
function descendants(s){return (s.children||[]).flatMap(c=>[c,...descendants(c)]);}
const results=[];
for(const s of d.screens){const b=p.getShapeById(s.id),all=descendants(b);
 for(const t of all.filter(t=>t.type==='text')){
  if(t.name.startsWith('Ownership /'))t.y=b.y+1683;
  if(t.name==='Footer / permanent ownership'){t.x=b.x+420;t.resize(2200,t.height);}
  if(t.name==='Slot limit guidance'){t.x=b.x+300;t.resize(820,t.height);t.fontSize='25';}
  if(t.name.startsWith('Action / unequip'))t.x-=40;
 }
 results.push({state:s.state,id:b.id,size:[b.width,b.height],slotCategories:s.slots.map(id=>p.getShapeById(id).name),catalog:s.catalog,nativeTextCount:all.filter(t=>t.type==='text').length,primaryInteractions:p.getShapeById(s.primaryActionId).interactions.map(i=>({trigger:i.trigger,action:i.action})),bounds:b.children.every(c=>c.x>=b.x-1&&c.y>=b.y-1&&c.x+c.width<=b.x+b.width+1&&c.y+c.height<=b.y+b.height+1)});
}
penpot.selection=[p.getShapeById(d.screens[0].id)];penpot.viewport.zoomIntoView(penpot.selection);
return {revision:d.revision,oldR055Preserved:!!p.getShapeById('75555645-d407-8067-8008-de9b9c9710d1'),screens:results,sourceDimensions:[p.getShapeById(d.workshopArtId).width,p.getShapeById(d.workshopArtId).height]};
