"""Export exact native R045 icon shapes through the connected local Penpot MCP session."""
import json
import os
import pathlib
import subprocess
import sys

root=pathlib.Path.cwd()
response=json.loads((pathlib.Path(os.environ['TEMP'])/'stat-icons-ready.json').read_text(encoding='utf-16'))
icons=json.loads(response['result']['content'][0]['text'])['result']
bridge=pathlib.Path(os.environ['TEMP'])/'penpot-modifier-tooltip-mcp.py'
for name,shape in icons.items():
    filename=name.lower().replace(' ','_')+'.png'
    target=root/'Assets/UIThemes/MidnightFieldJournal/Art/Tooltips/StatIcons'/filename
    args={'name':'export_shape','arguments':{'shapeId':shape,'format':'png','filePath':target.as_posix()}}
    p=subprocess.run([sys.executable,str(bridge)],input=json.dumps(args),text=True,capture_output=True)
    if p.returncode or not target.exists(): raise RuntimeError(p.stdout+p.stderr)
    print('Exported '+filename,flush=True)
(root/'Docs/UIThemes/MidnightFieldJournal/Sync007/IconSources.json').write_text(json.dumps(icons,indent=2),encoding='utf-8')
