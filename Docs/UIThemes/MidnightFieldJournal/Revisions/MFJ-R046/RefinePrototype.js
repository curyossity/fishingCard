// Preserve the exact stopped marker position and route every base rarity to an existing eligible example.
if(storage.rarityR046.refined)return storage.rarityR046;
const d=storage.rarityR046,page=penpot.currentPage;
const find=id=>page.getShapeById(id);
const rareReveal=find(d.revealBoardId);rareReveal.x=42000;rareReveal.y=17000;
function revealVariant(name,id,x){
 const b=rareReveal.clone();page.root.appendChild(b);b.name=name;b.x=x;b.y=17000;
 for(const child of b.children)if(child.name.startsWith('Dynamic / existing Moonlit Ray'))child.remove();
 const card=find(id).clone();b.appendChild(card);card.x=b.x+1250;card.y=b.y+110;card.hidden=false;return b;
}
const commonReveal=revealVariant('GAMEPLAY K / Common Cod reveal / R046','bae680cd-9225-8018-8008-cf09f41c7320',46500);
const uncommonReveal=revealVariant('GAMEPLAY L / Uncommon Small Octopus reveal / R046','bae680cd-9225-8018-8008-cee1b56dd362',51000);
const reveals={Common:commonReveal,Uncommon:uncommonReveal,Rare:rareReveal};
const cache={},freezes=[];
for(const k of Object.keys(d.resultBoardIds)){
 const template=find(d.resultBoardIds[k]);
 for(const interaction of template.interactions)if(interaction.trigger==='after-delay')interaction.remove();
 template.addInteraction('after-delay',{type:'navigate-to',destination:reveals[k]},850);
 const r=d.records.find(r=>r.id===template.id);cache[k+':'+r.position]=template;
}
let j=0;
for(const id of d.motionBoardIds){
 const r=d.records.find(r=>r.id===id),k=r.position<.68?'Common':r.position<.92?'Uncommon':'Rare',key=k+':'+r.position;
 if(!cache[key]){
  const b=find(d.resultBoardIds[k]).clone();page.root.appendChild(b);b.x=32000+(j%7)*1320;b.y=23600+Math.floor(j/7)*560;b.name='PROTOTYPE / R046 / Exact stop '+k+' '+r.position;
  const marker=b.children.find(s=>s.name.startsWith('Dynamic / marker /'));marker.x=b.x+56+1048*r.position-19;
  const burst=b.children.find(s=>s.name.startsWith('Feedback / stop burst'));if(burst)burst.x=b.x+56+1048*r.position-35;
  cache[key]=b;freezes.push({id:b.id,rarity:k,position:r.position,markerId:marker.id});j++;
 }
 const button=find(r.buttonId);for(const i of button.interactions)i.remove();button.addInteraction('click',{type:'navigate-to',destination:cache[key]});
}
const scene=find(d.contextBoardId);
// Existing scene's stop also locks at its displayed .79 position.
const scenePanel=scene.children.find(s=>s.name==='MFJ-R046 / In-context timing bar');
if(scenePanel){const button=scenePanel.children.find(s=>s.name.startsWith('Control / Stop'));for(const i of button.interactions)i.remove();button.addInteraction('click',{type:'navigate-to',destination:cache['Uncommon:0.79']});}
function headerDepth(s){if(s.type==='text'&&s.characters?.includes('180'))s.characters=s.characters.replace(/180/g,'12');for(const c of s.children??[])headerDepth(c);}
for(const b of [scene,...Object.values(reveals)])for(const c of b.children)if(c.name.startsWith('01 / Header'))headerDepth(c);
// Darken result headings for parchment contrast, keeping the more saturated zone fill separate.
const labelColours={Common:'#355950',Uncommon:'#2D5D74',Rare:'#845515',Legendary:'#69415F',Miss:'#304448'};
for(const r of d.records.filter(r=>r.result)){
 const p=find(r.id),heading=p.children.find(s=>s.name==='Dynamic / phase heading');if(heading)heading.fills=[{fillColor:labelColours[r.result],fillOpacity:1}];
}
for(const b of Object.values(cache)){const h=b.children.find(s=>s.name==='Dynamic / phase heading');if(h){const k=b.name.includes('Rare')?'Rare':b.name.includes('Uncommon')?'Uncommon':'Common';h.fills=[{fillColor:labelColours[k],fillOpacity:1}];}}
d.revealBoardIds=Object.fromEntries(Object.entries(reveals).map(([k,b])=>[k,b.id]));d.exactFreezeFrames=freezes;d.stopPreservesExactPosition=true;d.refined=true;
penpot.selection=[find(d.reviewBoardId)];penpot.viewport.zoomIntoView(penpot.selection);return d;
