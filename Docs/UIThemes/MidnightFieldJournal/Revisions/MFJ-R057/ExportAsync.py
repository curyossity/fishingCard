"""Allow Penpot's lossless render to finish beyond the MCP request timeout."""
import base64,json,sys,time
from ApplySharpFrames import call,folder,targets

kind=sys.argv[1];name=kind.title()+'4K.png';shape=targets[kind]
code='const key='+json.dumps(kind)+';const id='+json.dumps(shape)+';'+('if(storage.r057PngExports)delete storage.r057PngExports[key];' if '--fresh' in sys.argv else '')+r'''
storage.r057PngExports??={};
if(!storage.r057PngExports[key]){
 storage.r057PngExports[key]={status:'rendering'};
 penpot.currentPage.getShapeById(id).export({type:'png',scale:1}).then(bytes=>{storage.r057PngExports[key]={status:'ready',bytes};}).catch(error=>{storage.r057PngExports[key]={status:'error',error:String(error)};});
}
return {status:storage.r057PngExports[key].status};
'''
print(json.dumps(call('execute_code',{'code':code})),flush=True)
for attempt in range(90):
    result=call('execute_code',{'code':'const s=storage.r057PngExports['+json.dumps(kind)+'];return {status:s.status,error:s.error,length:s.bytes?.length};'})['result']
    if result['status']=='error':raise RuntimeError(result['error'])
    if result['status']=='ready':break
    time.sleep(3)
else:raise RuntimeError('Lossless render has not completed; render is retained in plugin storage')
path=folder/name
with path.open('wb') as output:
    for offset in range(0,result['length'],262144):
        code='const bytes=storage.r057PngExports['+json.dumps(kind)+'].bytes.slice('+str(offset)+','+str(offset+262144)+r''');let binary='';for(let i=0;i<bytes.length;i+=8192)binary+=String.fromCharCode(...bytes.slice(i,i+8192));return {base64:btoa(binary)};'''
        data=call('execute_code',{'code':code})['result']
        output.write(base64.b64decode(data['base64']))
print(json.dumps({'file':str(path),'bytes':path.stat().st_size}),flush=True)
