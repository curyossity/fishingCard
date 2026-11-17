"""Separate approved decorative artwork from all live species, labels and interactions."""
import json, copy, hashlib, struct
from pathlib import Path

folder=Path(__file__).resolve().parent
target=folder/'Refinement'/'RuntimeAssets'
target.mkdir(exist_ok=True)
root=folder.parents[3]
source=json.loads((folder/'Refinement'/'Composition.json').read_text())
commands=source[0]['commands']
kit=root/'Docs/UIThemes/TackleApparatusConcept/AssetLibrary'
reference=root/'Docs/UIThemes/TackleApparatusConcept/RigPlacement.png'
integrated_source=folder/'IntegratedMachine/integrated-build-machine-v2.png'
scenes=[]

def scene(name, bounds, layers):
    x,y,w,h=bounds
    layers=copy.deepcopy(layers)
    for c in layers:c['x']-=x;c['y']-=y
    scenes.append(dict(name=name,file=name,width=w,height=h,transparent=True,rasterOnly=True,commands=layers))

for name,bounds,prefix in [
    ('build-desk',(0,0,1920,1080),'desk-background'),
    ('build-cabinet',(24,44,380,995),'rig-cabinet'),
    ('build-title',(570,28,780,116),'title-plaque'),
    ('build-haul',(1430,29,345,116),'haul-plaque'),
    ('build-console',(492,897,1310,165),'control-console'),
    ('build-message',(525,836,1140,54),'message-strip'),
    ('build-rail-heading',(680,521,950,47),'status-plaque'),
    ('build-readout',(545,920,505,116),'readout-glass-wide'),
    ('build-place',(1120,920,600,116),'button-primary'),
    ('build-upgraded',(1120,920,600,116),'button-secondary'),
    ('build-rig-card',(62,209,315,244),'rig-mackerel-blank-card')]:
    pool=source[1]['commands'] if name=='build-upgraded' else commands
    scene(name,bounds,[c for c in pool if c['kind']=='image' and c['id'].startswith(prefix)])

# Keep the complete authored housing at native source resolution. The Unity stage scales it
# uniformly once; no screenshot cutouts, stretched wood or separately overlaid tray are used.
machine_width,machine_height=struct.unpack('>II',integrated_source.read_bytes()[16:24])
scenes.append(dict(name='build-machine',file='build-machine',width=machine_width,height=machine_height,
    transparent=True,rasterOnly=True,copySource=str(integrated_source),commands=[]))

def image(ident,src,sx,sy,sw,sh,x,y,w,h):
    return dict(kind='image',id=ident,src=str(src),sx=sx,sy=sy,sw=sw,sh=sh,x=x,y=y,w=w,h=h)

def transparent_asset(ident,relative,x,y,w,h):
    path=kit/'Exports'/relative
    sw,sh=struct.unpack('>II',path.read_bytes()[16:24])
    return image(ident,path,0,0,sw,sh,x,y,w,h)

# Page 08 sockets and the common arrow have real alpha. Never crop controls from a screen.
# Preserve the existing 80x94 native hit target and the approved visual center.
for state in ['idle','selected']:
    scene('build-gap-'+state,(0,0,80,94),[
        transparent_asset(state+'-socket','Placement/insertion-socket-'+state+'.png',0,0,74,73),
        transparent_asset('plus','Placement/insertion-plus.png',25,24,24,24)])
scene('build-selection-caret',(0,0,80,35),[
    transparent_asset('selected-arrow','Common/caret-down.png',22,8,30,20)])

scene('build-rail-card',(697,601,221,190),[
    transparent_asset('blank-ivory-card','Common/rig-card-ivory.png',697,601,221,190)])

for exported in scenes:
    for command in exported['commands']:
        if (command['kind']=='image' and not Path(command['src']).resolve().is_relative_to((kit/'Exports').resolve())):
            raise ValueError('Runtime artwork must use an independent approved export: '+exported['name'])

(target/'Composition.json').write_text(json.dumps(scenes,indent=2))
manifest=dict(referenceResolution=[1920,1080],penpotFile='3681d377-bc58-8110-8008-e7a38adc2112',
    penpotPages=['389319c8-6b5a-80a0-8008-f36e5768d320','389319c8-6b5a-80a0-8008-f371b5b35d7e'],
    referenceSha256=hashlib.sha256(reference.read_bytes()).hexdigest(),
    assets=[dict(name=s['name'],width=s['width'],height=s['height'],sha256=hashlib.sha256((target/(s['name']+'.png')).read_bytes()).hexdigest() if (target/(s['name']+'.png')).exists() else None) for s in scenes],
    integratedMachineSource='IntegratedMachine/integrated-build-machine-v2.png',
    integratedMachineSha256=hashlib.sha256(integrated_source.read_bytes()).hexdigest(),
    notes='One complete authored machine housing with its built-in wooden recess, copied byte-for-byte; independent AssetLibrary exports for the remaining ornaments. No screenshot crops, stretched wood or separate tray background.')
(target/'Manifest.json').write_text(json.dumps(manifest,indent=2))
print(target)
