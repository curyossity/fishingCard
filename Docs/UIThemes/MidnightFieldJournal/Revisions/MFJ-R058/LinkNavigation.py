"""Link the existing current hub screens to the newly designed Journal without changing their visual assets."""
import json
from Bridge import call, folder

design=json.loads((folder/'PenpotDesign.json').read_text(encoding='utf-8'))
code='const ID='+json.dumps(design['boardId'])+';'+r'''
const destination=penpot.currentPage.getShapeById(ID),changes=[];
for(const id of ['75555645-d407-8067-8008-de8ff58e2639','75555645-d407-8067-8008-de9b9c9710d1','75555645-d407-8067-8008-de9d62501389']){
 const board=penpot.currentPage.getShapeById(id);
 const links=penpotUtils.findShapes(s=>s.type==='text'&&['JOURNAL','DISCOVERY JOURNAL'].includes(s.characters.trim().toUpperCase()),board);
 for(const t of links){for(const old of [...(t.interactions||[])])t.removeInteraction(old);t.addInteraction('click',{type:'navigate-to',destination,preserveScrollPosition:false});changes.push({boardId:id,shapeId:t.id});}
}
penpot.selection=[destination];penpot.viewport.zoomIntoView([destination]);return {links:changes,selectedBoardId:destination.id};
'''
result=call('execute_code',{'code':code})
(folder/'Navigation.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
print(json.dumps(result))
