"""Compose two Penpot concept sources from approved independent artwork; no Unity files are written."""
import json, struct
from pathlib import Path

root=Path(__file__).resolve().parents[4]
out=Path(__file__).resolve().parent
kit=root/'Docs/UIThemes/TackleApparatusConcept/AssetLibrary'
manifest=json.loads((kit/'Manifest.json').read_text(encoding='utf-8-sig'))
assets={a['name']:a for a in manifest['assets']}
scenes=[]
cream='#DED3A5'; gold='#E9BC63'; ink='#373B2C'; muted='#B6BDA0'

def add(kind, ident, x,y,w,h,**extra):
    scene['commands'].append(dict(kind=kind,id=ident,x=x,y=y,w=w,h=h,**extra))
def image(name,x,y,w,h,ppu=None,ident=None):
    item=assets.get(name)
    path=(kit/item['exportPath']) if item else root/('Assets/UIThemes/TackleApparatus/Art/Creatures/creature_'+name+'.png')
    size=struct.unpack('>II',path.read_bytes()[16:24]); sw,sh=size
    border=item.get('unityBorderLBRT') if item else None
    if not ppu or not border:
        add('image',ident or name,x,y,w,h,src=str(path),sx=0,sy=0,sw=sw,sh=sh); return
    left,bottom,right,top=border
    xs=[0,left,sw-right,sw]; ys=[0,top,sh-bottom,sh]
    dx=[x,x+left/ppu,x+w-right/ppu,x+w]; dy=[y,y+top/ppu,y+h-bottom/ppu,y+h]
    for r in range(3):
        for c in range(3):
            add('image',(ident or name)+f'-slice-{r}-{c}',dx[c],dy[r],dx[c+1]-dx[c],dy[r+1]-dy[r],src=str(path),sx=xs[c],sy=ys[r],sw=xs[c+1]-xs[c],sh=ys[r+1]-ys[r])
def text(ident,value,x,y,w,size=24,color=cream,body=False,align='center'):
    add('text',ident,x,y,w,size+8,text=value,size=size,color=color,font='body' if body else 'display',align=align,shadow=not body and color!=ink)
def fit_creature(name,x,y,w,h,ident):
    path=root/('Assets/UIThemes/TackleApparatus/Art/Creatures/creature_'+name+'.png')
    sw,sh=struct.unpack('>II',path.read_bytes()[16:24]); scale=min(w/sw,h/sh)
    image(name,x+(w-sw*scale)/2,y+(h-sh*scale)/2,sw*scale,sh*scale,ident=ident)
def card(name,label,level,value,x,y,small=False,highlight=False):
    w,h=(205,177) if small else (315,244)
    image('rig-card-ivory',x,y,w,h,2.0833333333 if small else 1.35135135,ident=label+'-blank-card')
    if small:
        image('card-clip',x+75,y-20,55,34)
        text(label+'-name',name.replace('_',' ').upper(),x+24,y+18,w-48,14)
        fit_creature(name,x+23,y+48,w-46,75,label+'-sprite')
        text(label+'-level','LV '+str(level),x+28,y+144,90,15,align='left')
        text(label+'-sale-value',str(value),x+143,y+144,36,17)
    else:
        image('card-clip',x+130,y-23,55,34)
        text(label+'-name',name.replace('_',' ').upper(),x+51,y+27,w-102,21)
        fit_creature(name,x+30,y+70,w-60,105,label+'-sprite')
        text(label+'-level','LV '+str(level),x+44,y+202,140,18,align='left')
        text(label+'-sale-value',str(value),x+211,y+201,58,22)
        image('rope-vertical',x+147,y+244,20,35)
    if highlight:
        image('focus-corners',x-9,y-9,w+18,h+18,1.8,ident='upgrade-highlight')

for level in [1,3]:
    upgrade=level==3
    scene=dict(name='Level 3 - Automatic Upgrade' if upgrade else 'Level 1 - New Species',file='Sardine-Level-3-Upgrade' if upgrade else 'Sardine-Level-1-New',width=1920,height=1080,commands=[])
    scenes.append(scene)
    image('desk-background',0,0,1920,1080)
    image('rig-cabinet',24,44,380,995)
    image('title-plaque',570,28,780,116,1.11111111)
    image('haul-plaque',1430,29,345,116)
    image('machine-housing',490,163,1310,657)
    image('control-console',492,897,1310,165,1.53846154)
    image('message-strip',525,836,1140,54,1.66666667)
    text('phase','SHALLOWS / UPGRADE' if upgrade else 'SHALLOWS / BUILD',590,72,740,30)
    text('haul','HAUL 36' if upgrade else 'HAUL 15',1450,76,300,27,gold)
    text('rig-heading','YOUR RIG',70,114,290,24)
    if upgrade:
        card('sardine','rig-sardine',3,21,62,209)
        card('mackerel','rig-mackerel',1,10,62,484)
        card('glass_minnow','rig-glass-minnow',1,5,62,759)
    else:
        card('mackerel','rig-mackerel',1,10,62,209)
        card('glass_minnow','rig-glass-minnow',1,5,62,484)
        text('new-rig-caption','New species adds',94,832,250,29,muted,True)
        text('new-rig-caption-2','a new position.',94,868,250,29,muted,True)
    text('landed-name','SARDINE LANDED',690,223,960,28)
    image('portrait-frame',630,295,455,225,1.25)
    fit_creature('sardine',655,326,405,116,'landed-sardine-portrait')
    text('creature-tags','FISH / SCHOOLING / SMALL',666,474,383,25,cream,True)
    # The former two role panels are one informational plate; level and current effect/value remain explicit.
    image('role-panel-selected',1100,285,565,235,1.45,ident='acquisition-information-plate')
    text('acquisition-status','DUPLICATE / +1' if upgrade else 'NEW SPECIES',1140,325,240,17,gold)
    text('result-level','LEVEL 2 > 3' if upgrade else 'LEVEL 1',1400,325,225,18,gold)
    text('sale-caption','SALE VALUE',1132,378,205,17)
    text('resolved-sale-value','21' if upgrade else '7',1132,410,205,42)
    text('copy-value-explanation','14 + 7 = 21' if upgrade else '1 copy x 7',1132,467,205,23,cream,True)
    text('ability-name','SCHOOLING',1360,378,266,18)
    text('resolved-ability-amount','+0.6x' if upgrade else '+0.2x',1360,410,266,33,gold)
    text('ability-condition-1','per other connected',1360,447,266,22,cream,True)
    text('ability-condition-2','Schooling species',1360,470,266,22,cream,True)
    image('status-plaque',680,531,950,47,2.6)
    text('rail-heading','UPGRADE IN PLACE / ORDER PRESERVED' if upgrade else 'CHOOSE INSERTION POINT',695,545,920,20)
    image('insertion-rail',622,584,1040,217,1.38888889)
    if upgrade:
        image('rope-horizontal',802,622,600,16)
        card('sardine','rail-sardine',3,21,740,617,True,True)
        card('mackerel','rail-mackerel',1,10,1010,617,True)
        card('glass_minnow','rail-glass-minnow',1,5,1280,617,True)
        image('status-plaque',753,585,180,37,3)
        text('upgrade-slot-badge','+1 LEVEL',765,597,156,15,gold)
        message='Sardine stays in position 1. Level and ability upgraded automatically.'
    else:
        image('rope-horizontal',810,622,580,16)
        card('mackerel','rail-mackerel',1,10,790,617,True)
        card('glass_minnow','rail-glass-minnow',1,5,1240,617,True)
        for x,selected in [(682,False),(1084,True),(1545,False)]:
            image('insertion-socket-selected' if selected else 'insertion-socket-idle',x,677,62,62)
            image('insertion-plus',x+13,690,36,36)
        image('caret-down',1100,644,30,22)
        message='Between Mackerel and Glass Minnow. Placement is permanent.'
    text('placement-message',message,550,854,1085,25,cream,True)
    image('readout-glass-wide',545,920,505,116,1.3333333)
    text('haul-delta-caption','HAUL CHANGE' if upgrade else 'HAUL PREVIEW',565,939,465,20)
    text('haul-delta','29 > 36' if upgrade else '15 > 22',565,978,465,31)
    if upgrade:
        image('button-secondary',1120,920,600,116,1.42857143)
        text('automatic-upgrade-status','UPGRADE APPLIED',1140,945,560,25,gold)
        text('automatic-continuation','Continuing automatically...',1140,982,560,24,cream,True)
    else:
        image('button-primary',1120,920,600,116,1.42857143)
        text('commit-action','PLACE & CONTINUE',1140,962,560,27,ink)

(out/'Composition.json').write_text(json.dumps(scenes,indent=2),encoding='utf-8')
print(json.dumps({'screens':len(scenes),'commands':[len(s['commands']) for s in scenes],'folder':str(out)}))
