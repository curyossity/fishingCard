import json
from Build import call,folder
code="""
const state=JSON.parse(penpot.currentPage.getPluginData('MFJ-R053'));
const b=penpot.currentPage.getShapeById(state.boardId);
const duplicate=penpot.currentPage.getShapeById('75555645-d407-8067-8008-de8ff4703f89');
if(duplicate)duplicate.remove();
const subtitle=b.children.find(s=>s.name==='Editable / COASTAL WATERS');
if(subtitle){state.editableTextFields=state.editableTextFields.filter(id=>id!==subtitle.id);subtitle.remove();}
penpot.currentPage.setPluginData('MFJ-R053',JSON.stringify(state));
penpot.selection=[b];penpot.viewport.zoomIntoView([b]);return state;
"""
r=call('execute_code',{'code':code})
(folder/'PenpotDesign.json').write_text(json.dumps(r.get('result',r),indent=2),encoding='utf-8')
print(json.dumps({'saved':True}))
