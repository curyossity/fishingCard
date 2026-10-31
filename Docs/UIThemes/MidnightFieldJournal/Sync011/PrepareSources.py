"""Copy only the approved R058 resources and preserve its actual Penpot field geometry."""
import hashlib,json,shutil
from pathlib import Path

folder=Path(__file__).resolve().parent
project=folder.parents[3]
source=folder.parent/'Revisions/MFJ-R058'
target=project/'Assets/UIThemes/MidnightFieldJournal/Harbor/Journal'
target.mkdir(parents=True,exist_ok=True)
if not (folder/'Before.json').exists():
    files=[p for p in (project/'Assets').rglob('*') if p.is_file()]+[project/'ProjectSettings/EditorBuildSettings.asset']
    (folder/'Before.json').write_text(json.dumps({'files':[{'path':p.relative_to(project).as_posix(),'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in files]},indent=2),encoding='utf-8')
design=json.loads((source/'PenpotDesign.json').read_text(encoding='utf-8'))
live=json.loads((source/'PenpotVerification.json').read_text(encoding='utf-8'))
geometry=json.loads((source/'FrameGeometry.json').read_text(encoding='utf-8'))
resources=[]
for key,path in [(x['key'],source/x['file']) for x in geometry]+[('JournalDesk',source/'Art/JournalDesk.png')]:
    dest=target/'Art'/(key+'.png');dest.parent.mkdir(exist_ok=True);shutil.copyfile(path,dest)
    resources.append({'id':key,'path':dest.relative_to(project).as_posix(),'source':path.relative_to(project).as_posix(),'sha256':hashlib.sha256(dest.read_bytes()).hexdigest()})
nodes=[]
for f in design['frames']:
    nodes.append(dict(kind='image',name='Frame / '+f['key'],resource=f['key'],x=f['x'],y=f['y'],width=f['displayPixels'][0],height=f['displayPixels'][1]))
nodes.append(dict(kind='image',name='Journal Illustration',resource='JournalDesk',x=232.5,y=365,width=810,height=1080))
for t in live['text']:
    text=t['text'];row=next((i for i,r in enumerate(design['rows']) if t['id']==r['statusId']),-1)
    color='#F1DFB7'
    if t['y']<225:color='#243638' if text in ['JOURNAL','920'] and t['x']<1100 or text=='920' else '#C5AE89'
    if t['y']<225 and text=='JOURNAL' and t['x']>1100:color='#F1DFB7'
    if text.startswith(('01','02','03')):color='#9A6A35'
    if text in [q['title'].upper() for q in json.loads((source/'QuestCopy.json').read_text(encoding='utf-8'))]:color='#243638'
    if text in ['OBJECTIVE','REWARD · NOT YET GRANTED']:color='#D8B57F'
    if text in ['THE FIELD JOURNAL','PROGRESS IS RECORDED']:color='#E0BC80'
    if text.startswith('Discover the sea.'):color='#C5AE89'
    if text=='BACK TO HARBOR':color='#243638'
    if row>=0:color='#C4D9C4' if text=='COMPLETE' else '#E0BC80'
    align='center' if text in ['920','HARBOR','TACKLE SHOP','TECHNIQUES','JOURNAL','THE FIELD JOURNAL','BACK TO HARBOR'] and t['x']>1100 or text.startswith(('Every expedition','Discover the sea.')) or text=='THE FIELD JOURNAL' else 'left'
    role='status' if row>=0 else 'gold' if text=='920' else 'save' if text.startswith('Rewards are listed for reference;') else ''
    if role=='gold':text='0'
    nodes.append(dict(kind='text',name='Progress' if row>=0 else 'Gold Balance' if role=='gold' else 'Field / '+text.replace('\n',' '),text=text,x=t['x'],y=t['y'],width=t['width'],height=t['height'],fontSize=float(t['fontSize']),family=t['font'],color=color,align=align,role=role,questIndex=row))
nodes.append(dict(kind='rect',name='Active Journal Tab',x=2460,y=183,width=200,height=3,color='#B88D52',opacity=1))
for i in range(3):nodes.append(dict(kind='rect',name='Quest Divider',x=2360,y=470+i*540,width=2,height=240,color='#B88D52',opacity=.48))
for name,x,y,size,key,color,index in [('Journal',205,88,105,'Book','#9A6A35',-1),('Gold',3150,92,95,'Coin','#9A6A35',-1),('Footer',220,1960,100,'Book','#9A6A35',-1),('Back',2725,1960,100,'Anchor','#9A6A35',-1),('Completion',1460,1233,50,'Check','#C4D9C4',1)]:
    nodes.append(dict(kind='emblem',name=name+' Emblem',x=x,y=y,width=size,height=size,resource=key,color=color,questIndex=index))
layout={'width':3840,'height':2160,'resources':resources,'nodes':nodes,'quests':design['rows']}
(target/'JournalLayout.json').write_text(json.dumps(layout,indent=2,ensure_ascii=False),encoding='utf-8')
(folder/'ImportedSources.json').write_text(json.dumps(resources,indent=2),encoding='utf-8')
print(json.dumps({'resources':len(resources),'nodes':len(nodes),'target':str(target)}))
