// Penpot-only rarity timing proposal. Widths and motion timings are illustrative, not gameplay balance.
if(storage.rarityR046)return storage.rarityR046;
const page=penpotUtils.getPageById('bae680cd-9225-8018-8008-cddd6a6f3825');penpot.openPage(page);
const paper=penpotUtils.findShapeById('bae680cd-9225-8018-8008-d1c2678546b4');
const reference=penpotUtils.findShapeById('bae680cd-9225-8018-8008-d0a91363085a');
const ray=penpotUtils.findShapeById('bae680cd-9225-8018-8008-cf1bf647bdf1');
const ink='#304448',sepia='#9B713F',cream='#F0E2C2',muted='#715D44',dark='#111E23';
const title=penpot.fonts.findByName('Cinzel'),body=penpot.fonts.findByName('Source Serif 4');
function board(n,x,y,w,h,p){const s=penpot.createBoard();s.name=n;s.resize(w,h);s.fills=[];if(p)p.appendChild(s);s.x=(p?.x||0)+x;s.y=(p?.y||0)+y;return s;}
function rect(p,n,x,y,w,h,c,a=1,r=0){const s=penpot.createRectangle();p.appendChild(s);s.name=n;s.resize(w,h);s.x=p.x+x;s.y=p.y+y;s.fills=[{fillColor:c,fillOpacity:a}];s.borderRadius=r;return s;}
function text(p,n,c,x,y,w,h,size=24,color=ink,bold=false,heading=false,align='left'){const s=penpot.createText(c);p.appendChild(s);s.name=n;const f=heading?title:body;f.applyToText(s,f.variants.find(v=>v.fontWeight===(bold?'700':'400')&&v.fontStyle==='normal'));s.fontSize=String(size);s.fills=[{fillColor:color,fillOpacity:1}];s.resize(w,h);s.x=p.x+x;s.y=p.y+y;s.align=align;return s;}
function svg(p,n,x,y,w,h,paths,view=`0 0 ${w} ${h}`){const s=penpot.createShapeFromSvg(`<svg xmlns="http://www.w3.org/2000/svg" width="${w}" height="${h}" viewBox="${view}">${paths}</svg>`);p.appendChild(s);s.name=n;s.x=p.x+x;s.y=p.y+y;return s;}
const corner=`<g fill="none" stroke="${sepia}" stroke-linecap="round"><path d="M64 4H22Q4 4 4 22V64M64 11H23Q11 11 11 23V64" stroke-width="1.2"/><path d="M19 42Q10 29 24 20Q37 12 44 26L34 40Q28 47 19 42Z" stroke-width="1"/><path d="M23 39L22 24M28 40L28 21M33 39L34 22M36 35L40 26M20 45Q29 50 40 39M44 19Q55 20 52 30Q49 36 44 31Q40 26 46 25" stroke-width=".75"/><path d="M16 49L20 46M46 16L50 14" stroke-width=".7"/></g>`;
function frame(p,w=1160,h=340){
 const bg=board('Static / clean parchment / scalable centre',0,0,w,h,p);bg.clipContent=true;bg.borderRadius=20;
 const im=paper.clone();bg.appendChild(im);im.resize(1190*w/550,1322*h/642);im.x=bg.x-320*w/550;im.y=bg.y-340*h/642;im.name='Static / clean paper grain only';
 for(const[x,y,sx,sy]of[[0,0,1,1],[w-64,0,-1,1],[0,h-64,1,-1],[w-64,h-64,-1,-1]])svg(p,'Static / fixed engraved corner',x,y,64,64,`<g transform="translate(${sx<0?64:0} ${sy<0?64:0}) scale(${sx} ${sy})">${corner}</g>`);
 for(const y of[4,11,h-11,h-4])rect(p,'Static / stretchable horizontal rule',64,y,w-128,1,sepia);
 for(const x of[4,11,w-11,w-4])rect(p,'Static / stretchable vertical rule',x,64,1,h-128,sepia);
}
const zones={Common:{color:'#60847F',symbol:'wave'},Uncommon:{color:'#467C98',symbol:'double-wave'},Rare:{color:'#CAA24F',symbol:'star'},Legendary:{color:'#93617E',symbol:'diamond'},Miss:{color:'#D5C9AC',symbol:'gap'}};
function emblem(p,kind,x,y,color,size=25){
 let path=kind==='wave'?'<path d="M2 13Q8 5 14 13T26 13"/>':kind==='double-wave'?'<path d="M2 9Q8 1 14 9T26 9M2 19Q8 11 14 19T26 19"/>':kind==='star'?'<path d="M14 2L17 10L26 14L17 17L14 26L10 17L2 14L10 10Z"/>':kind==='diamond'?'<path d="M14 1L26 14L14 27L2 14ZM14 7L20 14L14 21L8 14Z"/>':'<path d="M2 14H9M19 14H26M13 7L17 14L13 21"/>';
 return svg(p,'Dynamic / '+kind+' rarity emblem',x,y,size,size,`<g fill="none" stroke="${color}" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">${path}</g>`,'0 0 28 28');
}
const base=[['Common',.68],['Uncommon',.24],['Rare',.08]],upgrade=[['Common',.62],['Uncommon',.25],['Rare',.08],['Legendary',.03],['Miss',.02]],wider=[['Common',.58],['Uncommon',.28],['Rare',.14]];
const records=[];
function panel(parent,x,y,name,distribution=base,position=.36,result=null,buttonState='normal'){
 const p=board(name,x,y,1160,340,parent);frame(p);
 text(p,'Dynamic / phase heading',result?result==='Miss'?'ENCOUNTER SKIPPED':result.toUpperCase()+' ENCOUNTER':'ENCOUNTER RARITY',56,28,900,40,26,result?zones[result].color:ink,false,true);
 const track=board('Dynamic / zone track / normalized widths',56,108,1048,62,p);track.clipContent=true;track.borderRadius=7;
 const zoneRecords=[];let start=0;
 for(const[k,portion]of distribution){
  const z=board('Zone / '+k+' / resizable',start*1048,0,portion*1048,62,track);z.clipContent=true;
  rect(z,'Dynamic / '+k+' colour fill',0,0,z.width,62,zones[k].color);
  let pattern='';
  if(k==='Common'||k==='Uncommon')for(let i=-12;i<z.width+30;i+=32){pattern+=`<path d="M${i} 38q8 -10 16 0t16 0"/>`;if(k==='Uncommon')pattern+=`<path d="M${i} 23q8 -10 16 0t16 0"/>`;}
  else if(k==='Rare')for(let i=12;i<z.width;i+=28)pattern+=`<path d="M${i} 15v12M${i-6} 21h12M${i} 35v12M${i-6} 41h12"/>`;
  else if(k==='Legendary')for(let i=0;i<z.width;i+=20)pattern+=`<path d="M${i+10} 4l8 12l-8 12l-8 -12ZM${i+10} 34l8 12l-8 12l-8 -12Z"/>`;
  else for(let i=-30;i<z.width+60;i+=12)pattern+=`<path d="M${i} 62l36 -62"/>`;
  svg(z,'Dynamic / '+k+' pattern / clipped independently',0,0,Math.max(1,z.width),62,`<g fill="none" stroke="${k==='Miss'?ink:cream}" stroke-opacity="${k==='Miss'?'.55':'.38'}" stroke-width="1.1">${pattern}</g>`);
  if(result===k)rect(z,'Feedback / stopped-zone ivory wash',0,0,z.width,62,cream,.23);
  zoneRecords.push({name:k,fraction:portion,shapeId:z.id,start,end:start+portion,width:z.width});start+=portion;
 }
 svg(p,'Static / track border and depth-like ticks',54,106,1052,66,`<rect x="1" y="1" width="1050" height="64" rx="8" fill="none" stroke="${ink}" stroke-width="2"/><path d="${Array.from({length:31},(_,i)=>`M${18+i*34} 2v${i%5===0?10:5}`).join('')}" fill="none" stroke="${cream}" stroke-opacity=".7" stroke-width="1"/>`);
 const marker=board('Dynamic / marker / independent normalized position',56+1048*position-19,79,38,117,p);
 if(!result){for(const[d,a]of[[-12,.24],[-23,.10]])rect(marker,'Feedback / optional motion trail',19+d,29,2,62,cream,a);}
 rect(marker,'Marker / dark outline',16,27,6,66,ink);rect(marker,'Marker / bright stop line',18,27,2,66,'#FFF4D7');
 svg(marker,'Marker / parchment compass head',0,0,38,31,`<path d="M19 0L35 12L19 30L3 12Z" fill="${cream}" stroke="${ink}" stroke-width="1.7"/><path d="M19 5L28 12L19 22L10 12Z" fill="none" stroke="${sepia}" stroke-width="1"/><path d="M19 9v9M14.5 13.5h9" stroke="${ink}" stroke-width="1"/>`);
 svg(marker,'Marker / lower anchor point',10,94,18,20,`<path d="M9 1L16 8L9 18L2 8Z" fill="${cream}" stroke="${ink}" stroke-width="1.5"/>`);
 if(result){const halo=svg(p,'Feedback / stop burst / optional short pulse',56+1048*position-35,57,70,70,`<g stroke="${result==='Miss'?ink:sepia}" fill="none" stroke-width="1.4">${Array.from({length:8},(_,i)=>{const a=i*Math.PI/4;return `<path d="M${35+25*Math.cos(a)} ${35+25*Math.sin(a)}L${35+33*Math.cos(a)} ${35+33*Math.sin(a)}"/>`;}).join('')}</g>`);halo.opacity=.75;}
 const legend=board('Dynamic / external rarity labels / never squeezed into tiny zones',56,194,1048,36,p);
 distribution.forEach(([k],i)=>{const slot=1048/distribution.length;emblem(legend,zones[k].symbol,i*slot,2,k==='Miss'?ink:zones[k].color,25);text(legend,'Label / '+k,k.toUpperCase(),i*slot+35,1,slot-40,34,distribution.length>3?20:23,ink,false,true);});
 const note=result?(result==='Miss'?'Open water. No encounter, no penalty.':'The creature reveals next. Then choose Catch or Bait.'):'Stop the marker to determine your next encounter.';
 text(p,'Dynamic / instruction',note,56,260,708,44,23,muted);
 const button=board('Control / Stop / '+buttonState,808,248,296,66,p);rect(button,'Button / ink face',0,0,296,66,result?'#6D6048':buttonState==='hover'?'#47635E':ink,1,8);
 svg(button,'Button / etched inner rule',4,4,288,58,`<rect x="1" y="1" width="286" height="56" rx="5" fill="none" stroke="${cream}" stroke-opacity=".55" stroke-width="1"/>`);
 rect(button,'Button / stop emblem',47,25,14,14,cream);text(button,'Button / native label',result?'STOPPED':'STOP',76,15,174,43,27,cream,false,true,'center');
 const r={id:p.id,trackId:track.id,markerId:marker.id,buttonId:button.id,position,result,zones:zoneRecords,width:1160,height:340};records.push(r);return {p,button,r};
}
const review=board('MFJ-R046 / Rarity timing bar / design review',32000,17000,3200,2200);rect(review,'Review background',0,0,3200,2200,dark);
text(review,'Review title','THE NEXT THING BELOW',95,65,3000,95,64,cream,false,true);
text(review,'Review subtitle','Rarity timing · parchment, sea ink and engraved marks · reusable 4K elements',100,170,3000,60,34,'#C9BDA5');
text(review,'Base caption','01 / BASE BAR — NO FAILURE ZONE',100,275,1400,50,28,cream,false,true);
const basic=panel(review,100,350,'MFJ-R046 / Base / sweeping marker');
text(review,'Rare caption','02 / STOPPED — RARE',1660,275,1400,50,28,cream,false,true);
panel(review,1660,350,'MFJ-R046 / Rare / locked feedback',base,.96,'Rare');
text(review,'Upgrade caption','03 / OPTIONAL LEGENDARY + MISS',100,820,1400,50,28,cream,false,true);
panel(review,100,895,'MFJ-R046 / Upgraded / tiny extra zones',upgrade,.72,null,'hover');
text(review,'Modifier caption','04 / ZONES RESIZED BY A RUN EFFECT',1660,820,1400,50,28,cream,false,true);
panel(review,1660,895,'MFJ-R046 / Modified / expanded Rare zone',wider,.91,'Rare');
text(review,'Legendary caption','05 / LEGENDARY — SPECIAL INK + DIAMONDS',100,1365,1400,50,28,cream,false,true);
panel(review,100,1440,'MFJ-R046 / Legendary / locked feedback',upgrade,.966,'Legendary');
text(review,'Miss caption','06 / MISS — OPEN WATER, NOT FAILURE',1660,1365,1400,50,28,cream,false,true);
panel(review,1660,1440,'MFJ-R046 / Miss / encounter skipped',upgrade,.991,'Miss');
text(review,'Review disclaimer','Illustrative widths and timing only. No encounter odds or Unity gameplay have changed.',100,1880,2950,60,30,'#C9BDA5');
text(review,'Review hierarchy note','Tiny sections keep their labels outside the track. Colour + pattern + emblem identify every rarity.',100,1960,2950,60,30,'#C9BDA5');
// Prototype frames simulate marker travel. Stop maps to the zone at the currently shown position.
const positions=[.04,.20,.36,.52,.67,.79,.9,.96,.9,.79,.67,.52,.36,.20];
const motion=positions.map((v,i)=>panel(null,32000+(i%7)*1320,19600+Math.floor(i/7)*560,'PROTOTYPE / R046 / Sweep '+String(i+1).padStart(2,'0'),base,v));
const outcomes={};for(const[k,pos]of[['Common',.36],['Uncommon',.79],['Rare',.96]])outcomes[k]=panel(null,32000+Object.keys(outcomes).length*1320,20900,'PROTOTYPE / R046 / Stopped '+k,base,pos,k);
const reveal=reference.clone();page.root.appendChild(reveal);reveal.name='GAMEPLAY J / Rare creature revealed after timing / R046';reveal.x=36500;reveal.y=19600;
function cleanGameplay(b){for(const s of b.children){if(/Main Creature|Previous main|tooltip|pointer/i.test(s.name))s.hidden=true;}}
cleanGameplay(reveal);
const rayInstance=ray.clone();reveal.appendChild(rayInstance);rayInstance.x=reveal.x+1250;rayInstance.y=reveal.y+110;rayInstance.hidden=false;rayInstance.name='Dynamic / existing Moonlit Ray example / creature reveal';
const scene=reference.clone();page.root.appendChild(scene);scene.name='GAMEPLAY I / Rarity timing phase / R046';scene.x=36500;scene.y=17000;cleanGameplay(scene);
text(scene,'Dynamic / timing-stage heading','SOMETHING STIRS BELOW',1300,470,1160,80,46,cream,false,true,'center');
text(scene,'Dynamic / biome + destination context','Coastal Waters · 12 m',1300,565,1160,50,30,'#C9BDA5',false,false,'center');
const scenePanel=panel(scene,1300,710,'MFJ-R046 / In-context timing bar',base,.79);
scenePanel.button.addInteraction('click',{type:'navigate-to',destination:outcomes.Uncommon.p});
for(let i=0;i<motion.length;i++){
 const v=positions[i],k=v<.68?'Common':v<.92?'Uncommon':'Rare';
 motion[i].p.addInteraction('after-delay',{type:'navigate-to',destination:motion[(i+1)%motion.length].p},140);
 motion[i].button.addInteraction('click',{type:'navigate-to',destination:outcomes[k].p});
}
outcomes.Rare.p.addInteraction('after-delay',{type:'navigate-to',destination:reveal},850);
for(const k of['Common','Uncommon'])text(outcomes[k].p,'Mockup / replay control','REPLAY STUDY',56,310,500,27,16,muted).addInteraction('click',{type:'navigate-to',destination:motion[0].p});
// Make the review's base STOP an easy entry point into the running prototype.
basic.button.addInteraction('click',{type:'navigate-to',destination:motion[0].p});
const parts=board('MFJ-R046 / Reusable parts / native Unity handoff',32000,21900,3200,1400);rect(parts,'Review background',0,0,3200,1400,dark);
text(parts,'Parts heading','SEPARATE PARTS, ONE INSTRUMENT',100,70,2950,90,54,cream,false,true);
const blank=board('Source / blank nine-slice parchment surround',100,250,1160,340,parts);frame(blank);
text(parts,'Frame note','Static surround: 64px fixed corners; straight edges and paper centre resize.',100,630,1300,90,29,'#C9BDA5');
const exploded=panel(parts,1660,250,'Source / editable bar with independent children',upgrade,.96);
text(parts,'Parts note','Separate zone fills + clipped patterns · marker + trail · legends · Stop button · result pulse',1660,630,1400,150,29,'#C9BDA5');
text(parts,'Motion tokens','MOTION INTENT',100,880,1300,50,30,cream,false,true);
text(parts,'Motion detail','Steady horizontal travel. Freeze immediately on Stop.\nBrief marker punch + zone wash; rarity name appears.\nSubtle Rare/Legendary burst; no whole-screen flash.',100,960,1450,220,30,'#C9BDA5');
text(parts,'Implementation boundary','PROTOTYPE BOUNDARY',1660,880,1400,50,30,cream,false,true);
text(parts,'Boundary detail','Penpot uses discrete timed frames, not a live timing game.\nContinuous motion, random selection and eligibility belong to Unity later.\nNo upgrade, speed or width values here establish final balance.',1660,960,1400,220,30,'#C9BDA5');
storage.rarityR046={revision:'MFJ-R046',fileId:penpot.currentFile.id,pageId:page.id,reviewBoardId:review.id,contextBoardId:scene.id,revealBoardId:reveal.id,partsBoardId:parts.id,blankFrameId:blank.id,prototypeStartId:motion[0].p.id,motionBoardIds:motion.map(x=>x.p.id),resultBoardIds:Object.fromEntries(Object.entries(outcomes).map(([k,v])=>[k,v.p.id])),records,baseDistribution:base,upgradeDistribution:upgrade,modifiedDistribution:wider,prototypeFrameDelayMs:140,sourceFrameBorders:{left:64,right:64,top:64,bottom:64},status:'ready_for_review',unityImported:false};
penpot.selection=[review];penpot.viewport.zoomIntoView([review]);return storage.rarityR046;
