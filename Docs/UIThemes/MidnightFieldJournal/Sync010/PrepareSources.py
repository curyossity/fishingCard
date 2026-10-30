"""Snapshot the project and copy only current approved Harbor presentation sources."""
import hashlib,json,os,shutil,subprocess
from pathlib import Path

project=Path(__file__).resolve().parents[4]
folder=Path(__file__).resolve().parent;theme=folder.parent
destination=project/'Assets/UIThemes/MidnightFieldJournal/Harbor'
folder.mkdir(exist_ok=True)
snapshot=folder/'Before.json'
if not snapshot.exists():
    files=[p for p in (project/'Assets').rglob('*') if p.is_file()]+[project/'ProjectSettings/EditorBuildSettings.asset']
    snapshot.write_text(json.dumps({'files':[{'path':p.relative_to(project).as_posix(),'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in files]},indent=2),encoding='utf-8')

assets={
 'harborBackground':'Revisions/MFJ-R053/Art/HarborOrnateBlank.png',
 'shopBackground':'Revisions/MFJ-R054/Art/TackleShopOrnateBlank.png',
 'collectionBackground':'Revisions/MFJ-R056/Art/CollectionBlank.png',
 'harborFrames':'Revisions/MFJ-R057/HarborFrames4K.png',
 'shopFrames':'Revisions/MFJ-R057/ShopFrames4K.png',
 'collectionFrames':'Revisions/MFJ-R057/CollectionFrames4K.png',
 'rod':'Revisions/MFJ-R055/Art/RodEngraved.png',
 'reel':'Revisions/MFJ-R055/Art/ReelEngraved.png',
 'hook':'Revisions/MFJ-R055/Art/HookEngraved.png'
}
mapping={}
for key,relative in assets.items():
    src=theme/relative;dst=destination/'Art'/(key+'.png');dst.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(src,dst)
    mapping[key]=dst.relative_to(project).as_posix()
mapping['frame']='Assets/UIThemes/MidnightFieldJournal/Art/Cards/Technique/TechniqueFrame.png'
for key in ['CutLoose','SlipPast','ClearWater','HoldSteady','Rerig','SteadyLine','Recast','PrizeHook']:
    mapping[key]='Assets/UIThemes/MidnightFieldJournal/Art/Techniques/'+key+'.png'
code=r'''
const ids={harbor:'75555645-d407-8067-8008-de8ff58e2639',shop:'75555645-d407-8067-8008-de9b9c9710d1',collection:'75555645-d407-8067-8008-de9d62501389'};
const pages=[];
for(const [key,id] of Object.entries(ids)){
 const b=penpot.currentPage.getShapeById(id),nodes=[];
 function walk(s,opacity=1){
  opacity*=s.opacity??1;
  let resource='';
  if(s!==b&&s.width>3800&&s.height>2100)resource=key+(s.name.startsWith('MFJ-R057 /')?'Frames':'Background');
  else if(s.name.startsWith('Independent engraved equipment /'))resource=s.name.includes('Driftwood')?'rod':s.name.includes('Barbed')?'hook':'reel';
  else if(s.name.startsWith('Exact active-game resource / '))resource=s.name.split(' / ').at(-1);
  const fill=s.fills?.[0],bounds={x:s.x-b.x,y:s.y-b.y,width:s.width,height:s.height};
  if(resource){nodes.push({kind:'image',name:s.name,resource,...bounds,opacity});return;}
  if(s.type==='text'){nodes.push({kind:'text',name:s.name,text:s.characters,...bounds,opacity,color:fill?.fillColor||'#F1DFB7',family:s.fontFamily,fontSize:Number(s.fontSize),align:s.align,role:s.characters==='920'?'gold':''});return;}
  if(fill?.fillColor&&s!==b)nodes.push({kind:'rect',name:s.name,...bounds,opacity:opacity*(fill.fillOpacity??1),color:fill.fillColor});
  for(const c of s.children||[])walk(c,opacity);
 }
 walk(b);pages.push({id:key,boardId:id,nodes});
}
return {schemaVersion:1,width:3840,height:2160,pages};
'''
result=subprocess.run(['python',os.environ['TEMP']+'/penpot-modifier-tooltip-mcp.py'],input=json.dumps({'name':'execute_code','arguments':{'code':code}}),capture_output=True,text=True,check=True)
raw=json.loads(result.stdout);content=raw['result']['content'][0]['text']
try:data=json.loads(content)['result']
except (ValueError,KeyError):raise RuntimeError(content[:1000])
data['resources']=[{'id':key,'path':path} for key,path in mapping.items()]
for page in data['pages']:
    for node in page['nodes']:
        if node['kind']=='text' and node['text']=='LOADOUT SAVED':node['text']='LOADOUT PREVIEW'
write=json.dumps(data,ensure_ascii=False,indent=2)
(folder/'PenpotLayout.json').write_text(write,encoding='utf-8')
(destination/'HarborLayout.json').write_text(write,encoding='utf-8')
manifest=[{'id':key,'source':str(theme/relative),'target':mapping[key],'sha256':hashlib.sha256((theme/relative).read_bytes()).hexdigest()} for key,relative in assets.items()]
(folder/'ImportedSources.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print(json.dumps({'pages':len(data['pages']),'nodes':[len(p['nodes']) for p in data['pages']],'copiedAssets':len(manifest)}))
