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
    ('build-machine',(490,163,1310,657),'machine-housing'),
    ('build-console',(492,897,1310,165),'control-console'),
    ('build-message',(525,836,1140,54),'message-strip'),
    ('build-portrait',(630,295,455,225),'portrait-frame'),
    ('build-effect',(1100,285,565,235),'single-creature-details-frame'),
    ('build-rail-heading',(680,521,950,47),'status-plaque'),
    ('build-readout',(545,920,505,116),'readout-glass-wide'),
    ('build-place',(1120,920,600,116),'button-primary'),
    ('build-upgraded',(1120,920,600,116),'button-secondary'),
    ('build-rig-card',(62,209,315,244),'rig-mackerel-blank-card')]:
    pool=source[1]['commands'] if name=='build-upgraded' else commands
    scene(name,bounds,[c for c in pool if c['kind']=='image' and c['id'].startswith(prefix)])

def image(ident,src,sx,sy,sw,sh,x,y,w,h):
    return dict(kind='image',id=ident,src=str(src),sx=sx,sy=sy,sw=sw,sh=sh,x=x,y=y,w=w,h=h)

def transparent_asset(ident,relative,x,y,w,h):
    path=kit/'Exports'/relative
    sw,sh=struct.unpack('>II',path.read_bytes()[16:24])
    return image(ident,path,0,0,sw,sh,x,y,w,h)

def rail_layers():
    """Fit the transparent page 08 rail without stretching its border over the card area."""
    path=kit/'Exports/Placement/insertion-rail.png'
    source_x=[0,93,529,622]
    source_y=[0,49,172,221]
    target_x=[550,590,1645,1685]
    target_y=[562,592,801,831]
    return [image('rail-'+str(row)+'-'+str(col),path,
        source_x[col],source_y[row],source_x[col+1]-source_x[col],source_y[row+1]-source_y[row],
        target_x[col],target_y[row],target_x[col+1]-target_x[col],target_y[row+1]-target_y[row])
        for row in range(3) for col in range(3)]

scene('build-tray',(550,562,1135,269),rail_layers())
# The three-card decoration uses independent transparent rope/clip masters at native UI positions.
# No screen crops, erased creature fields or wood patches belong in these runtime masters.
three=rail_layers()
for x in [807,1111]:
    three.append(transparent_asset('rail-rope-'+str(x),'Common/rope-horizontal.png',x,581,304,48))
for x in [697,1001,1305]:
    three.append(transparent_asset('rail-clip-'+str(x),'Common/card-clip.png',x+83,578,55,34))
scene('build-tray-three',(550,562,1135,269),three)

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
        if command['kind']=='image' and not Path(command['src']).resolve().is_relative_to((kit/'Exports').resolve()):
            raise ValueError('Runtime artwork must use an independent approved export: '+exported['name'])

(target/'Composition.json').write_text(json.dumps(scenes,indent=2))
manifest=dict(referenceResolution=[1920,1080],penpotFile='3681d377-bc58-8110-8008-e7a38adc2112',
    penpotPages=['389319c8-6b5a-80a0-8008-f36e5768d320','389319c8-6b5a-80a0-8008-f371b5b35d7e'],
    referenceSha256=hashlib.sha256(reference.read_bytes()).hexdigest(),
    assets=[dict(name=s['name'],width=s['width'],height=s['height'],sha256=hashlib.sha256((target/(s['name']+'.png')).read_bytes()).hexdigest() if (target/(s['name']+'.png')).exists() else None) for s in scenes],
    notes='Only independent AssetLibrary exports are sampled; no composed-screen pixels. No creature, level, sale value, effect text or insertion controls are printed into runtime art.')
(target/'Manifest.json').write_text(json.dumps(manifest,indent=2))
print(target)
