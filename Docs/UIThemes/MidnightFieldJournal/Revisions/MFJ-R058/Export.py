"""Export the actual Penpot board asynchronously without printing image buffers."""
import base64
import json
import sys
import time
from Bridge import call, folder

design=json.loads((folder/'PenpotDesign.json').read_text(encoding='utf-8'))
scale=float(sys.argv[1]) if len(sys.argv)>1 else .5
key='R058-'+str(scale)
code='const key='+json.dumps(key)+';const id='+json.dumps(design['boardId'])+';'+r'''
storage.journalExports??={};
if(!storage.journalExports[key]){
 storage.journalExports[key]={status:'rendering'};
 penpot.currentPage.getShapeById(id).export({type:'png',scale:SCALE}).then(bytes=>{storage.journalExports[key]={status:'ready',bytes};}).catch(error=>{storage.journalExports[key]={status:'error',error:String(error)};});
}
return {status:storage.journalExports[key].status};
'''.replace('SCALE',str(scale))
print(json.dumps(call('execute_code',{'code':code})),flush=True)
for attempt in range(50):
    state=call('execute_code',{'code':'const s=storage.journalExports['+json.dumps(key)+'];return {status:s.status,error:s.error,length:s.bytes?.length};'})
    if state['status']=='ready':break
    if state['status']=='error':raise RuntimeError(state['error'])
    time.sleep(2)
else:raise RuntimeError('Export is still rendering in plugin storage.')
path=folder/('Journal4K.png' if scale==1 else 'JournalPreview.png')
with path.open('wb') as output:
    for offset in range(0,state['length'],262144):
        code='const a=storage.journalExports['+json.dumps(key)+'].bytes.slice('+str(offset)+','+str(offset+262144)+');let s="";for(let i=0;i<a.length;i+=8192)s+=String.fromCharCode(...a.slice(i,i+8192));return {base64:btoa(s)};'
        data=call('execute_code',{'code':code});output.write(base64.b64decode(data['base64']))
print(json.dumps({'file':str(path),'bytes':path.stat().st_size}),flush=True)
