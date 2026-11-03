const p=penpot.currentPage,d=JSON.parse(p.getSharedPluginData('fishing-cards-design','MFJ-R065'));function walk(a){return (a.children||[]).flatMap(c=>[c,...walk(c)]);}
for(const spec of d.screens){const root=p.getShapeById(spec.id),all=walk(root),detail=all.find(c=>c.name==='Enlarged equipment card / Reel');
 const type=detail.children.find(c=>c.name==='Equipment type');type.y=detail.y+90;type.fontSize='24';type.resize(780,34);
 const owner=detail.children.find(c=>c.name==='Ownership state');owner.y=detail.y+777;owner.fontSize='24';owner.resize(760,38);
 for(const card of all.filter(c=>c.name.startsWith('Portrait catalog card /'))){
  const cat=card.children.find(c=>c.name==='Category');cat.y=card.y+43;cat.fontSize='17';cat.resize(380,25);
  const art=card.children.find(c=>c.name.startsWith('Independent illustration /'));const scale=Math.min(330/art.width,200/art.height);art.resize(art.width*scale,art.height*scale);art.x=card.x+(card.width-art.width)/2;art.y=card.y+150+(200-art.height)/2;
 }
 const hint=all.find(c=>c.name==='Catalog scroll hint');hint.x=root.x+300;
}
return {revision:d.revision,refinement:'Native category labels clear top rope motif; ownership clears separator knot; catalog illustrations inset within their art region.'};
