"""Import independent native-sized frame assets and assemble an editable Penpot Journal."""
import hashlib
import json
from pathlib import Path
import sys
from Bridge import call, folder

project = folder.parents[4]
snapshot = folder / 'UnityBefore.json'
if not snapshot.exists():
    files = [p for p in (project/'Assets').rglob('*') if p.is_file()]
    snapshot.write_text(json.dumps({'files':[{'path':p.relative_to(project).as_posix(),'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in files]},indent=2),encoding='utf-8')
geometry = json.loads((folder/'FrameGeometry.json').read_text(encoding='utf-8'))
state_path = folder/'ImportedArt.json'
art = json.loads(state_path.read_text(encoding='utf-8')) if state_path.exists() else {}
for index, item in enumerate(geometry):
    key = item['key']
    if key in art:
        continue
    result = call('import_image', {'filePath':str(folder/item['file']),'x':60000,'y':50000+index*250,'width':item['w'],'height':item['imageDisplayHeight']})
    art[key] = result['shapeId']
    state_path.write_text(json.dumps(art,indent=2),encoding='utf-8')
    print('Imported '+key,flush=True)
if '--frames-only' in sys.argv:
    sys.exit(0)
if 'illustrationArt' not in art:
    path = folder/'Art/JournalDesk.png'
    if not path.exists():
        raise RuntimeError('The independent Journal illustration is not ready.')
    result = call('import_image',{'filePath':str(path),'x':60000,'y':53000,'width':835,'height':1080})
    art['illustrationArt'] = result['shapeId']
    state_path.write_text(json.dumps(art,indent=2),encoding='utf-8')
quests = json.loads((project/'Assets/UIThemes/MidnightFieldJournal/Harbor/DiscoveryQuests.json').read_text(encoding='utf-8'))['quests']
(folder/'QuestCopy.json').write_text(json.dumps(quests,indent=2,ensure_ascii=False),encoding='utf-8')
code = 'const ART='+json.dumps(art)+';const GEOMETRY='+json.dumps(geometry)+';const QUESTS='+json.dumps(quests)+';\n'+(folder/'CreateDesign.js').read_text(encoding='utf-8')
design = call('execute_code',{'code':code})
(folder/'PenpotDesign.json').write_text(json.dumps(design,indent=2),encoding='utf-8')
print(json.dumps({'boardId':design['boardId'],'boardName':design['boardName']}))
