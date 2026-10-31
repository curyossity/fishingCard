"""Keep editable status fields clear of the detailed lower-corner engraving."""
import json
from Bridge import call, folder
design=json.loads((folder/'PenpotDesign.json').read_text(encoding='utf-8'))
code='const D='+json.dumps(design)+';'+r'''
const b=penpot.currentPage.getShapeById(D.boardId);
D.rows.forEach((r,i)=>{
 const row=penpot.currentPage.getShapeById(r.boardId),status=penpot.currentPage.getShapeById(r.statusId);
 status.x=row.x+(i===1?330:280);status.resize(850,54);
 for(const s of row.children){
  if(s.name==='Native emblem / completed quest')s.x=row.x+260;
  if(s.name.startsWith('Native / expedition progress ')){const n=Number(s.name.split(' ').at(-1))-1;s.x=row.x+280+n*260;s.y=row.y+455;s.resize(240,6);}
 }
});
const caption=penpotUtils.findShape(s=>s.type==='text'&&s.characters.startsWith('Discover the sea.'),b);caption.y=b.y+1745;
if(storage.journalExports){delete storage.journalExports['R058-0.5'];delete storage.journalExports['R058-1.0'];}
penpot.selection=[b];penpot.viewport.zoomIntoView([b]);return {polished:true,boardId:b.id};
'''
print(json.dumps(call('execute_code',{'code':code})))
