"""Separate approved decorative artwork from all live species, labels and interactions."""
import json, copy, hashlib
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

tray=copy.deepcopy(next(c for c in commands if c['id']=='page-04-insertion-section'))
layers=[tray]
# Clear the old printed cards, role labels, clips and sockets. Retain the approved perimeter.
layers.append(image('clean-wood',kit/'Exports/Placement/insertion-rail.png',95,67,430,100,588,580,1053,218))
layers.append(image('clean-top',reference,940,562,70,25,610,562,1010,25))
scene('build-tray',(550,562,1135,269),layers)

# The approved three-card view retains its original wood and rope pixels. Erase data underneath
# the independent masters, rather than turning the concept into a printed game-state background.
three=[tray]+[c for c in source[1]['commands'] if c['id'].startswith('wood-inactive-gap-') or c['id']=='wood-no-insertion-caret']
for x in [697,1001,1305]:
    three.append(image('clear-card-'+str(x),kit/'Exports/Placement/insertion-rail.png',95,67,430,100,x,601,221,190))
scene('build-tray-three',(550,562,1135,269),three)
scene('build-gap-idle',(0,0,80,94),[image('idle-socket',reference,613,659,80,94,0,0,80,94)])
scene('build-gap-selected',(0,0,80,94),[image('selected-socket',reference,1542,659,80,94,0,0,80,94)])
scene('build-selection-caret',(0,0,80,35),[image('selected-arrow',reference,1542,629,80,35,0,0,80,35)])

blank=[image('page-04-original-card',reference,697,601,221,190,697,601,221,190)]
blank += [c for c in source[1]['commands'] if c['id'] in ['rail-sardine-portrait-paper','rail-sardine-name-well','rail-sardine-footer-well']]
scene('build-rail-card',(697,601,221,190),blank)

(target/'Composition.json').write_text(json.dumps(scenes,indent=2))
manifest=dict(referenceResolution=[1920,1080],penpotFile='3681d377-bc58-8110-8008-e7a38adc2112',
    penpotPages=['389319c8-6b5a-80a0-8008-f36e5768d320','389319c8-6b5a-80a0-8008-f371b5b35d7e'],
    referenceSha256=hashlib.sha256(reference.read_bytes()).hexdigest(),
    assets=[dict(name=s['name'],width=s['width'],height=s['height'],sha256=hashlib.sha256((target/(s['name']+'.png')).read_bytes()).hexdigest() if (target/(s['name']+'.png')).exists() else None) for s in scenes],
    notes='Only decorative masters. No creature, level, sale value, effect text or insertion controls are printed into runtime art.')
(target/'Manifest.json').write_text(json.dumps(manifest,indent=2))
print(target)
