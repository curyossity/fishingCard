// Penpot-only design source. Execute once in the connected existing assembly page.
if (storage.modifierR042) return storage.modifierR042;
const page = penpotUtils.getPageById('bae680cd-9225-8018-8008-cddd6a6f3825');
penpot.openPage(page);
const ink = '#24383A', muted = '#695846', copper = '#A8753E', green = '#18703A', red = '#AE2C26';
const titleFont = penpot.fonts.findByName('Cinzel');
const bodyFont = penpot.fonts.findByName('Source Serif 4');
const source = penpotUtils.findShapeById('bae680cd-9225-8018-8008-d1c2678546b4');
function board(name, x, y, w, h, parent) {
  const b = penpot.createBoard(); b.name = name; b.resize(w,h); b.fills=[];
  if(parent) parent.appendChild(b); b.x=(parent?.x||0)+x; b.y=(parent?.y||0)+y; return b;
}
function text(parent, name, content, x,y,w,h,size,color=ink,heading=false,bold=false,align='left') {
  const t=penpot.createText(content); t.name=name; parent.appendChild(t);
  const f=heading?titleFont:bodyFont; f.applyToText(t,f.variants.find(v=>v.fontWeight===(bold?'700':'400')&&v.fontStyle==='normal'));
  t.fontSize=String(size); t.fills=[{fillColor:color,fillOpacity:1}]; t.align=align;
  t.resize(w,h);t.x=parent.x+x;t.y=parent.y+y;return t;
}
function rect(parent,name,x,y,w,h,color,opacity=1) {
  const r=penpot.createRectangle(); r.name=name;parent.appendChild(r);r.resize(w,h);
  r.x=parent.x+x;r.y=parent.y+y;r.fills=[{fillColor:color,fillOpacity:opacity}];return r;
}
function rule(parent,y) {rect(parent,'Static / engraved divider',146,y,608,1.5,copper,.6);}
function emblem(parent,x,y,kind) {
  const paths=kind==='creature'
    ? '<path d="M5 20Q17 4 32 20Q17 36 5 20ZM32 20L40 11V29Z"/><circle cx="13" cy="18" r="1.5" fill="#A8753E"/>'
    :kind==='technique'?'<path d="M12 6V25Q12 36 25 33Q35 30 32 20L28 25M12 6H20M24 9L31 3M28 14L37 11"/>'
    :kind==='environment'?'<path d="M9 35V10M3 16L9 10L15 16M30 31V5M24 11L30 5L36 11M4 27Q10 22 16 27M24 22Q30 17 36 22"/>'
    :'<path d="M21 4L40 36H2ZM21 14V24M21 29V30"/>';
  const g=penpot.createShapeFromSvg('<svg xmlns="http://www.w3.org/2000/svg" width="42" height="42" viewBox="0 0 42 42"><g fill="none" stroke="#A8753E" stroke-width="1.8" stroke-linejoin="round" stroke-linecap="round">'+paths+'</g></svg>');
  g.name='Static / '+kind+' source emblem';parent.appendChild(g);g.x=parent.x+x;g.y=parent.y+y;return g;
}
const sourceBoard=board('MFJ-R042 / Independent modifier-tooltip frame',35200,2200,900,1020);
sourceBoard.appendChild(source);source.name='Static / independent engraved parchment master';source.resize(900,1020);source.x=sourceBoard.x;source.y=sourceBoard.y;
const review=board('MFJ-R042 / Stat modifier tooltips / editable review',32000,9300,2480,1370);
rect(review,'Review background',0,0,2480,1370,'#111E23');
text(review,'Review heading','THE STORY BEHIND EACH NUMBER',70,42,2300,75,44,'#E8D4AE',true);
text(review,'Review interaction note','Hover a modified stat to inspect its sources. Every amount and source below is editable.',70,123,2300,52,30,'#C9BDA5');
const configs=[{
  key:'value',title:'VALUE',creature:'LIONFISH',base:30,current:42,net:12,
  rows:[
    {name:'Prize Hook',source:'TECHNIQUE',amount:8,kind:'technique',detail:'Applied when this creature entered Catch.'},
    {name:'Venomous Spines',source:'CREATURE · LIONFISH',amount:-5,kind:'creature',detail:'One deliberate creature Release.'},
    {name:'Moonlit Reflection',source:'CREATURE · MOONLIT RAY BAIT',amount:9,kind:'creature',detail:'Value transferred from a released Garfish.'}
  ]
},{
  key:'weight',title:'WEIGHT',creature:'COD',base:2,current:4,net:2,
  rows:[
    {name:'Tangle',source:'CREATURE · JELLYFISH CATCH',amount:1,kind:'creature',detail:'Extra weight carried by this creature.'},
    {name:'Prize Hook',source:'TECHNIQUE',amount:1,kind:'technique',detail:'Applied when this creature entered Catch.'},
    {name:'Reef Snag',source:'HAZARD',amount:1,kind:'event',detail:'Until the current biome ends.'},
    {name:'Gentle Upwelling',source:'ENVIRONMENT',amount:-1,kind:'environment',detail:'Retained after switching from Bait to Catch.'}
  ]
}];
const tips=[];
for(let i=0;i<configs.length;i++) {
  const c=configs[i], b=board('MFJ-R042 / '+c.title+' modifier tooltip / '+c.creature,90+i*1230,235,900,1020,review);
  const paper=source.clone();b.appendChild(paper);paper.x=b.x;paper.y=b.y;
  paper.shadows=[{style:'drop-shadow',offsetX:0,offsetY:12,blur:24,spread:0,color:{color:'#000000',opacity:.32}}];
  text(b,'Dynamic / stat heading',c.title+' BREAKDOWN',220,112,460,48,32,ink,true,false,'center');
  text(b,'Dynamic / creature name',c.creature,260,168,380,38,25,muted,true,false,'center');
  text(b,'Static / default label','DEFAULT',170,240,220,32,21,muted,true,false,'center');
  text(b,'Dynamic / base amount',String(c.base),170,275,220,72,58,ink,false,false,'center');
  text(b,'Static / calculation arrow','→',405,278,90,64,48,copper,false,false,'center');
  const resultColour=c.key==='value'?green:red;
  text(b,'Static / current label','CURRENT',510,240,220,32,21,muted,true,false,'center');
  text(b,'Dynamic / resolved amount',String(c.current),510,271,220,80,64,resultColour,false,true,'center');
  rule(b,364);
  const start=394, pitch=c.rows.length===3?132:108;
  c.rows.forEach((row,j)=>{
    const y=start+j*pitch;
    emblem(b,142,y+13,row.kind);
    text(b,'Dynamic / source '+(j+1),row.source,205,y,494,27,18,muted,true);
    text(b,'Dynamic / effect '+(j+1),row.name,205,y+31,478,44,30,ink,false,true);
    text(b,'Dynamic / explanation '+(j+1),row.detail,205,y+76,490,39,23,muted);
    const benefit=c.key==='value'?row.amount>0:row.amount<0;
    text(b,'Dynamic / signed modifier '+(j+1),(row.amount>0?'+':'−')+Math.abs(row.amount),690,y+31,83,46,36,benefit?green:red,false,true,'right');
  });
  rule(b,844);
  text(b,'Static / net label','NET CHANGE',325,866,205,42,22,muted,true);
  text(b,'Dynamic / net amount','+'+c.net,530,856,90,58,40,resultColour,false,true,'right');
  tips.push(b);
}
const game=storage.game.clone();game.name='GAMEPLAY H / Value modifier tooltip / MFJ-R042';game.x=32000;game.y=6500;
for(const s of game.children) if(s.name.includes('Main Creature Card')||s.name.includes('Hover tooltip')||s.name.includes('pointer over'))s.hidden=true;
const lion=penpotUtils.findShapeById('bae680cd-9225-8018-8008-cf1b7bc27fec').clone();game.appendChild(lion);lion.x=game.x+1250;lion.y=game.y+110;
lion.name='Current Lionfish / editable / modified stat example';
const statValue=penpotUtils.findShape(s=>s.name==='Dynamic / sale Value',lion);
statValue.characters='42';statValue.fills=[{fillColor:green,fillOpacity:1}];statValue.fontWeight='700';
const statWeight=penpotUtils.findShape(s=>s.name==='Dynamic / effective Weight',lion);
statWeight.characters='3';statWeight.fills=[{fillColor:red,fillOpacity:1}];statWeight.fontWeight='700';
const tipPreview=tips[0].clone();game.appendChild(tipPreview);tipPreview.name='Visible example / hover modified Value';tipPreview.x=game.x+2120;tipPreview.y=game.y+520;
const proto=game.clone();proto.name='PROTOTYPE / Hover modified Value / MFJ-R042';proto.x=36000;proto.y=6500;
const shown=penpotUtils.findShape(s=>s.name==='Visible example / hover modified Value',proto);shown.hidden=true;
for(const b of [game,proto]) for(const child of b.children.filter(s=>s.hidden)) child.remove();
const overlay=tips[0].clone();page.root.appendChild(overlay);overlay.name='OVERLAY / Value modifiers / MFJ-R042';overlay.x=35200;overlay.y=3450;
const protoValue=penpotUtils.findShape(s=>s.name==='Dynamic / sale Value'&&s.characters==='42',proto);
protoValue.addInteraction('mouse-enter',{type:'open-overlay',destination:overlay,position:'manual',manualPositionLocation:{x:2120,y:520},closeWhenClickOutside:true,addBackgroundOverlay:false});
protoValue.addInteraction('mouse-leave',{type:'close-overlay',destination:overlay,animation:{type:'dissolve',duration:100,easing:'linear'}});
storage.modifierR042={revision:'MFJ-R042',fileId:penpot.currentFile.id,pageId:page.id,reviewBoardId:review.id,sourceBoardId:sourceBoard.id,frameId:source.id,valueTooltipId:tips[0].id,weightTooltipId:tips[1].id,gameplayBoardId:game.id,prototypeBoardId:proto.id,overlayBoardId:overlay.id,hoverTargetId:protoValue.id,configs};
penpot.selection=[review];penpot.viewport.zoomIntoView([review]);
return storage.modifierR042;
