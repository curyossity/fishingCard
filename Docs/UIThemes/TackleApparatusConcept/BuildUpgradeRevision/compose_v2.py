"""Refine the two concepts with page 04's original tray and a single effect plate."""
from pathlib import Path
exec(Path(__file__).with_name('compose.py').read_text().split('for level in [1,3]:')[0])
out=Path(__file__).resolve().parent/'Refinement'
out.mkdir(exist_ok=True)
assets['status-plaque']=dict(assets['status-plaque'],unityBorderLBRT=[55,55,55,55])

def reference_crop(ident,sx,sy,sw,sh,x=None,y=None,w=None,h=None):
    add('image',ident,sx if x is None else x,sy if y is None else y,sw if w is None else w,sh if h is None else h,src=str(root/'Docs/UIThemes/TackleApparatusConcept/RigPlacement.png'),sx=sx,sy=sy,sw=sw,sh=sh)

def blank_patch(asset,ident,sx,sy,sw,sh,x,y,w,h):
    add('image',ident,x,y,w,h,src=str(kit/assets[asset]['exportPath']),sx=sx,sy=sy,sw=sw,sh=sh)

def original_rail_card(name,label,level,value,x,y,sage=False):
    # Page 04's original edges and clips remain intact; replace only dynamic fields.
    if sage:reference_crop(label+'-original-ivory-frame',697,601,221,190,x,y,221,190)
    asset='rig-card-ivory'
    blank_patch(asset,label+'-portrait-paper',42,84,330,156,x+21,y+50,184,90)
    blank_patch(asset,label+'-name-well',62,31,292,49,x+36,y+20,151,26)
    blank_patch(asset,label+'-footer-well',50,247,316,50,x+34,y+143,154,32)
    text(label+'-name',name.replace('_',' ').upper(),x+37,y+25,149,21,cream,True)
    fit_creature(name,x+24,y+57,176,77,label+'-portrait')
    text(label+'-level','LV '+str(level),x+42,y+151,88,22,cream,True,align='left')
    text(label+'-sale',str(value),x+141,y+150,38,24,cream,True)

for level in [1,3]:
    upgrade=level==3
    scene=dict(name='Level 3 - Refined' if upgrade else 'Level 1 - Refined',file='Sardine-Level-3-Refined' if upgrade else 'Sardine-Level-1-Refined',width=1920,height=1080,commands=[])
    scenes.append(scene)
    image('desk-background',0,0,1920,1080)
    image('rig-cabinet',24,44,380,995)
    image('title-plaque',570,28,780,116,1.11111111)
    image('haul-plaque',1430,29,345,116)
    image('machine-housing',490,163,1310,657)
    image('control-console',492,897,1310,165,1.53846154)
    image('message-strip',525,836,1140,54,1.66666667)
    text('phase','SHALLOWS / BUILD',590,72,740,30)
    text('haul','HAUL 36' if upgrade else 'HAUL 31',1450,76,300,27,gold)
    text('rig-heading','YOUR RIG',70,114,290,24)
    if upgrade:
        card('sardine','rig-sardine',3,21,62,209)
        card('mackerel','rig-mackerel',1,10,62,484)
        card('glass_minnow','rig-glass-minnow',1,5,62,759)
    else:
        card('mackerel','rig-mackerel',1,10,62,209)
        card('glass_minnow','rig-glass-minnow',1,5,62,484)
        card('sea_bass','rig-sea-bass',1,16,62,759)
    text('landed-name','SARDINE LANDED',690,223,960,28)
    image('portrait-frame',630,295,455,225,1.25)
    fit_creature('sardine',655,326,405,116,'landed-sardine-portrait')
    text('creature-tags','FISH / SCHOOLING / SMALL',666,474,383,25,cream,True)
    image('status-plaque',1100,285,565,235,1.45,ident='single-creature-details-frame')
    text('result-level','LV 2 > 3' if upgrade else 'LV 1',1160,322,230,24,gold,align='left')
    if upgrade:text('level-increase','+1',1354,325,48,26,gold,True)
    text('sale-caption','SALE',1443,326,77,25,cream,True)
    text('resolved-sale-value','21' if upgrade else '7',1526,319,95,32,gold)
    text('effect-line-1','Successful strikes gain',1160,383,455,33,cream,True,align='left')
    text('resolved-ability-amount','+0.6x' if upgrade else '+0.2x',1160,426,132,23,gold,align='left')
    text('effect-line-2','per other contiguous',1302,427,313,29,cream,True,align='left')
    text('effect-line-3','Schooling creature.',1160,465,455,33,cream,True,align='left')
    reference_crop('page-04-insertion-section',550,562,1135,269)
    scene['commands'][-1]['mask']=[30,0,1105,0,1135,30,1135,239,1105,269,30,269,0,239,0,30]
    if upgrade:
        original_rail_card('sardine','rail-sardine',3,21,697,601)
        original_rail_card('mackerel','rail-mackerel',1,10,1001,601,True)
        original_rail_card('glass_minnow','rail-glass-minnow',1,5,1305,601)
        for x in [613,924,1226,1542]:
            reference_crop('wood-inactive-gap-'+str(x),1617,659,18,94,x,659,80,94)
        reference_crop('wood-no-insertion-caret',1617,629,18,35,1542,629,80,35)
        image('focus-corners',691,597,235,198,1.8,ident='upgraded-sardine-highlight')
        message='Sardine +1 level'
    else:
        original_rail_card('mackerel','rail-mackerel',1,10,697,601)
        original_rail_card('glass_minnow','rail-glass-minnow',1,5,1001,601,True)
        original_rail_card('sea_bass','rail-sea-bass',1,16,1305,601)
        message='After Sea Bass'
    image('status-plaque',680,521,950,47,2.6)
    text('rail-heading','YOUR RIG' if upgrade else 'CHOOSE INSERTION POINT',695,535,920,20)
    text('placement-message',message,550,854,1085,25,cream,True)
    image('readout-glass-wide',545,920,505,116,1.3333333)
    text('haul-delta-caption','HAUL CHANGE' if upgrade else 'HAUL PREVIEW',565,939,465,20)
    text('haul-delta','29 > 36' if upgrade else '31 > 38',565,978,465,31)
    if upgrade:
        image('button-secondary',1120,920,600,116,1.42857143)
        text('automatic-upgrade-status','UPGRADED',1140,945,560,25,gold)
        text('automatic-continuation','Next encounter...',1140,982,560,24,cream,True)
    else:
        image('button-primary',1120,920,600,116,1.42857143)
        text('commit-action','PLACE & CONTINUE',1140,962,560,27,ink)

(out/'Composition.json').write_text(json.dumps(scenes,indent=2),encoding='utf-8')
print(out)
