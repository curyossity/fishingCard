from Bridge import call,folder
import json
code="""
const state=JSON.parse(penpot.currentPage.getPluginData('MFJ-R054'));
const b=penpot.currentPage.getShapeById(state.boardId),sx=3840/1672,sy=2160/941;
for(const s of b.children){
 if(['Editable / In your collection','Editable / 180 GOLD','Editable / Complete a discovery'].includes(s.name))s.y=b.y+648*sy;
 if(s.name==='Editable / CURRENT LOADOUT · 1 / 1'||s.name==='Editable / Driftwood Rod equipped')s.x=b.x+225*sx;
}
function line(name,x,y,w){if(b.children.some(s=>s.name===name))return;const s=penpot.createRectangle();b.appendChild(s);s.name=name;s.resize(w*sx,1.5*sy);s.x=b.x+x*sx;s.y=b.y+y*sy;s.fills=[{fillColor:'#B88D52',fillOpacity:1}];}
line('Native / current location underline',760,79,100);
line('Native / active equipment filter underline',138,337,140);
penpot.selection=[b];penpot.viewport.zoomIntoView([b]);return {boardId:b.id,selected:true};
"""
print(call('execute_code',{'code':code}))
