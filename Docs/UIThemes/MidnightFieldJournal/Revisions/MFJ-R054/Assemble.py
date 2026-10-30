import json
from Bridge import call,folder
art=json.loads((folder/'ImportedArt.json').read_text(encoding='utf-8'))
code='const ART='+json.dumps(art['shapeId'])+';\n'+(folder/'CreateDesign.js').read_text(encoding='utf-8')
result=call('execute_code',{'code':code})
state=result.get('result',result)
(folder/'PenpotDesign.json').write_text(json.dumps(state,indent=2),encoding='utf-8')
print(json.dumps(state))
