import json
from Build import call,folder
result=call('execute_code',{'code':(folder/'CreateDesign.js').read_text(encoding='utf-8')})
(folder/'PenpotDesign.json').write_text(json.dumps(result.get('result',result),indent=2),encoding='utf-8')
print(json.dumps(result))
