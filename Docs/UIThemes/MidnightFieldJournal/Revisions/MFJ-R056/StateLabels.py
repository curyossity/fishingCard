import json
from Build import call,folder
code=r"""
const state=JSON.parse(penpot.currentPage.getPluginData('MFJ-R056')),b=penpot.currentPage.getShapeById(state.boardId);
for(const data of state.cardInstances){
 const c=penpot.currentPage.getShapeById(data.boardId);
 if(data.illustrativeState==='locked'){
  c.opacity=1;
  c.children.filter(s=>s.name==='Exact active-game resource / PrizeHook').forEach(s=>s.opacity=.4);
 }
}
const labels=b.children.filter(s=>s.type==='text'&&(/Editable \/ (\d IN DECK|OWNED · NOT IN DECK|LOCKED · 120 GOLD)/.test(s.name)));
for(const t of labels){
 const name='Native / state label backing / '+t.id;
 const r=penpot.createRectangle();b.appendChild(r);r.name=name;r.resize(t.width,t.height);r.x=t.x;r.y=t.y;r.borderRadius=5;r.fills=[{fillColor:'#10262E',fillOpacity:.95}];
 const top=t.clone();b.appendChild(top);top.x=t.x;top.y=t.y;state.editableTextFields=state.editableTextFields.map(id=>id===t.id?top.id:id);t.remove();
}
penpot.currentPage.setPluginData('MFJ-R056',JSON.stringify(state));penpot.selection=[b];penpot.viewport.zoomIntoView([b]);return state;
"""
r=call('execute_code',{'code':code});state=r.get('result',r)
(folder/'PenpotDesign.json').write_text(json.dumps(state,indent=2),encoding='utf-8')
print(json.dumps({'boardId':state['boardId'],'cardCount':len(state['cardInstances'])}))
