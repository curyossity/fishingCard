const p=penpot.currentPage,ns='fishing-cards-design';
if(p.getSharedPluginData(ns,'MFJ-R067'))throw Error('R067 already exists; inspect before retry');
const prev=JSON.parse(p.getSharedPluginData(ns,'MFJ-R066'));
const find=(root,name)=>penpotUtils.findShape(s=>s.name===name,root);
const source=penpot.createBoard();source.name='MFJ-R067 / Approved concept background / PREVIEW — source 1672 x 941';source.resize(3840,2160);source.x=88800;source.y=32000;source.clipContent=true;source.fills=[];
p.getShapeById('92f68b17-9997-80de-8008-dfc1d70ae173').appendChild(source);
const art=p.getShapeById('5de0315f-b85e-80a3-8008-e1835da54fa4');source.appendChild(art);art.name='Approved GeneratedConceptOnly / raster preview — not native 4K';art.resize(3840,2160);art.x=source.x;art.y=source.y;
const screens=[];
for(let i=0;i<prev.screens.length;i++){
 const old=p.getShapeById(prev.screens[i].id),b=old.clone();b.x=80000+i*4200;b.y=32000;b.name='MFJ-R067 / Tackle Shop / Approved concept'+(i?' — Catalog Scrolled':' — Current Review');b.setSharedPluginData(ns,'MFJ-R066','');
 const children=[b,...penpotUtils.findShapes(()=>true,b)];for(const c of children)for(const interaction of [...c.interactions])c.removeInteraction(interaction);
 const oldBg=find(b,'Fixed / Original native 4K background');oldBg.hidden=true;
 const bg=source.clone();b.appendChild(bg);bg.name='Fixed / Approved concept background — preview';bg.x=b.x;bg.y=b.y;bg.fixedWhenScrolling=true;bg.sendToBack();
 screens.push(b);
}
find(screens[0],'Catalog / Next scroll state').addInteraction('click',{type:'navigate-to',destination:screens[1]});find(screens[1],'Catalog / Previous scroll state').addInteraction('click',{type:'navigate-to',destination:screens[0]});
for(const b of screens)find(b,'Navigation / Harbor').addInteraction('click',{type:'navigate-to',destination:p.getShapeById('75555645-d407-8067-8008-de8ff58e2639')});
const d={revision:'MFJ-R067',fileId:penpot.currentFile.id,pageId:p.id,pageName:p.name,size:[3840,2160],screens:screens.map((b,i)=>({id:b.id,state:i?'Catalog scrolled':'Current review',position:[b.x,b.y],scrollOffset:i?1090:0})),initialId:screens[0].id,backgroundSourceBoardId:source.id,backgroundImageId:art.id,backgroundSource:'Art/ApprovedConceptBackground.png',backgroundNativePixels:[1672,941],displayPixels:[3840,2160],status:'visual_review_only',unityImported:false,integrationStatus:'blocked_native_4k_art',baseRevision:'MFJ-R066',change:'Use the exact GeneratedConceptOnly image requested by the user, replacing the rejected flat reconstruction for visual review.',qualityLimitation:'The board is 3840 x 2160 but the approved raster is 1672 x 941, enlarged approximately 2.30x. This is not a native 4K asset and must not be marked ready for final 4K Unity import.',unchanged:['Cards, slots, catalog and detail layout','All UI text remains separate/editable','Unity assets, code, gameplay and saves'],prototype:prev.prototype};
p.setSharedPluginData(ns,d.revision,JSON.stringify(d));for(const b of screens)b.setSharedPluginData(ns,d.revision,JSON.stringify(d));
prev.status='superseded';prev.supersededBy=d.revision;p.setSharedPluginData(ns,'MFJ-R066',JSON.stringify(prev));
penpot.selection=[screens[0]];penpot.viewport.zoomIntoView([screens[0]]);return d;
