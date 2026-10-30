"""Author resolution-independent engraved UI shells; no raster resizing or tracing."""
import json,math,random
from pathlib import Path

folder=Path(__file__).resolve().parent
PAPER='#EAD4AE';INK='#122830';COPPER='#A56C37';GOLD='#C1975D';LIGHT='#F3E3C9'

def path(d,stroke=GOLD,width=.65,fill='none',opacity=1):
    return f'<path d="{d}" fill="{fill}" stroke="{stroke}" stroke-width="{width}" opacity="{opacity}" stroke-linecap="round" stroke-linejoin="round"/>'

def rect(x,y,w,h,fill,stroke='none',sw=0,r=10):
    return f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{r}" fill="{fill}" stroke="{stroke}" stroke-width="{sw}"/>'

def vegetation(x,y,height,width,flip=1,seed=3):
    """Engrave a multi-stem kelp spray with fine veins and irregular leaflets."""
    rng=random.Random(seed);stems=[];leaves=[];veins=[];seedpods=[]
    for stem in range(5):
        angle=(stem-2)*.19
        length=height*(.58+.42*rng.random());bend=width*(stem-2)*.15
        def point(t):return x+flip*(math.sin(angle)*length*t+bend*t*t),y-length*t
        a=point(0);z=point(1)
        stems.append(f'M{a[0]:.2f},{a[1]:.2f} Q{x+flip*bend*.1:.2f},{y-length*.55:.2f} {z[0]:.2f},{z[1]:.2f}')
        for j in range(2,14):
            t=j/15;px,py=point(t);spread=width*.18*(1-t*.6)*rng.uniform(.7,1.2)
            for sign in [-1,1]:
                ex=px+sign*spread;ey=py-length*.09*rng.uniform(.6,1.2);cx=px+sign*spread*.6;cy=py-length*.025
                leaves.append(f'M{px:.2f},{py:.2f} Q{cx:.2f},{cy:.2f} {ex:.2f},{ey:.2f} Q{px+sign*spread*.26:.2f},{ey+length*.015:.2f} {px:.2f},{py:.2f}Z')
                veins.append(f'M{px:.2f},{py:.2f} L{ex:.2f},{ey:.2f}')
        for t in [.84,.91,.97]:
            px,py=point(t);seedpods.append(f'M{px-1:.2f},{py:.2f} a1.2,1.8 15 1,0 2.4,0 a1.2,1.8 15 1,0 -2.4,0')
    return path(' '.join(leaves),COPPER,.42,'#9F8558',.8)+path(' '.join(stems),LIGHT,.85)+path(' '.join(veins),LIGHT,.35,opacity=.85)+path(' '.join(seedpods),GOLD,.5,GOLD)

def rock(x,y,size,seed):
    rng=random.Random(seed)
    d=f'M{x-size:.1f},{y} Q{x-size*.9:.1f},{y-size*.7:.1f} {x-size*.3:.1f},{y-size*.8:.1f} Q{x+size*.2:.1f},{y-size*1.15:.1f} {x+size*.6:.1f},{y-size*.5:.1f} L{x+size},{y}Z'
    hatch=[]
    for i in range(55):
        px=x+rng.uniform(-size*.67,size*.65);py=y-rng.uniform(.08,.6)*size
        hatch.append(f'M{px:.1f},{py:.1f} l{rng.uniform(1,5):.1f},{rng.uniform(-3,1):.1f}')
    return path(d,GOLD,.9,'#354047')+path(' '.join(hatch),'#A5A392',.35,opacity=.8)

def corner(x,y,signx=1,signy=1):
    d='M0 36 Q0 0 36 0 M4 30 Q7 7 30 4 M7 24 Q15 7 24 7 M2 15 Q14 18 17 3 M7 7 Q23 20 34 6 M8 8 Q20 23 6 34 M16 6 Q18 13 26 16 M6 16 Q13 18 16 26 M7 7 L2 2 M27 3 L32 1 M3 27 L1 32'
    return f'<g transform="translate({x},{y}) scale({signx},{signy})">'+path(d,COPPER,.8)+path('M5 5 Q14 11 18 18 M11 1 L15 6 M1 11 L6 15',LIGHT,.45)+'</g>'

def shell(x,y,w,h,light=False,botanical=True,seed=1,heading=None):
    fill=PAPER if light else INK
    out=rect(x-3,y-3,w+6,h+6,'#07171D','#07171D',2,16)
    out+=rect(x,y,w,h,fill,COPPER,2.3,13)
    out+=rect(x+3,y+3,w-6,h-6,'none',LIGHT,1.2,11)
    out+=rect(x+6,y+6,w-12,h-12,'none',COPPER,.65,9)
    out+=rect(x+9,y+9,w-18,h-18,'none',GOLD,.6,8)
    # Actual vector grain: short, irregular engraved marks, no embedded texture.
    rng=random.Random(seed);grain=[]
    for i in range(int(w*h/260)):
        px=rng.uniform(x+14,x+w-14);py=rng.uniform(y+14,y+h-14)
        grain.append(f'M{px:.1f},{py:.1f} l{rng.uniform(1,5):.1f},{rng.uniform(-2,2):.1f}')
    out+=path(' '.join(grain),COPPER if light else '#748081',.35,opacity=.12 if light else .1)
    if heading:
        hh=heading
        out+=rect(x+10,y+10,w-20,hh,PAPER,COPPER,.8,5)
        out+=path(f'M{x+15},{y+hh+11} H{x+w-15}',LIGHT,.7)
    out+=corner(x+10,y+10)+corner(x+w-10,y+10,-1,1)+corner(x+10,y+h-10,1,-1)+corner(x+w-10,y+h-10,-1,-1)
    if botanical and not light:
        size=min(h*.55,130,w*.21)
        out+=vegetation(x+24,y+h-13,size,min(w*.13,45),1,seed)
        out+=vegetation(x+w-23,y+h-13,size*1.05,min(w*.13,45),-1,seed+4)
        if h>160:
            out+=rock(x+25,y+h-12,18,seed)+rock(x+w-28,y+h-12,22,seed+1)
    # Small engraved pins at precise joins.
    for px,py in [(x+13,y+13),(x+w-13,y+13),(x+13,y+h-13),(x+w-13,y+h-13)]:
        out+=f'<circle cx="{px}" cy="{py}" r="2" fill="{GOLD}" stroke="{COPPER}" stroke-width=".6"/>'
    return out

def icon(kind,x,y,size,color=COPPER):
    d={
      'anchor':'M50 7V79M38 18A12 12 0 1 0 62 18A12 12 0 1 0 38 18M24 44H76M9 62Q12 89 50 91Q88 89 91 62M9 62L7 78M9 62L24 68M91 62L93 78M91 62L76 68',
      'rod':'M12 91L87 9M15 85L23 92M87 9Q67 16 77 58Q86 95 60 83M37 63A11 11 0 1 0 51 72A11 11 0 1 0 37 63',
      'book':'M50 21Q28 5 7 19V83Q28 73 50 90Q72 73 93 83V19Q72 5 50 21V90M17 31L38 36M17 44L38 49M17 57L38 62M62 36L83 31M62 49L83 44M62 62L83 57',
      'cards':'M14 19L60 9L80 77L35 89ZM34 14L82 22L76 88M40 42L53 34L64 52L50 63Z',
      'coin':'M12 50A38 38 0 1 0 88 50A38 38 0 1 0 12 50M20 50A30 30 0 1 0 80 50A30 30 0 1 0 20 50M28 50A22 22 0 1 0 72 50A22 22 0 1 0 28 50M43 36L61 45L55 64L37 55Z'
    }[kind]
    return f'<g transform="translate({x},{y}) scale({size/100})">'+path(d,color,3)+'</g>'

def page_edges():
    # Opaque bands hide the enlarged raster perimeter before the new etching.
    out=rect(0,0,1672,21,INK,r=0)+rect(0,0,21,941,INK,r=0)+rect(1651,0,21,941,INK,r=0)+rect(0,920,1672,21,INK,r=0)
    for inset,color,width in [(7,COPPER,3),(10,LIGHT,1.2),(14,GOLD,1),(18,COPPER,.7)]:
        out+=rect(inset,inset,1672-2*inset,941-2*inset,'none',color,width,24)
    for x,sx in [(21,1),(1651,-1)]:
        for y in [280,570,916]:out+=vegetation(x,y,170,24,sx,int(x+y))
    for x,y,sx,sy in [(20,20,1,1),(1652,20,-1,1),(20,921,1,-1),(1652,921,-1,-1)]:
        out+=corner(x,y,sx,sy)
    return out

def header():
    out=rect(25,20,1622,82,INK,r=0)
    out+=shell(69,29,397,64,True,False,8)
    out+=shell(514,24,755,69,False,False,11)
    out+=shell(1351,29,244,64,True,False,3)
    out+=f'<circle cx="130" cy="60" r="38" fill="{PAPER}" stroke="{COPPER}" stroke-width="2"/><circle cx="130" cy="60" r="32" fill="none" stroke="{COPPER}" stroke-width="1"/>'
    out+=icon('anchor',106,36,48)+icon('coin',1372,40,42)
    out+=vegetation(75,96,64,35,1,12)+vegetation(461,96,64,35,-1,15)
    return out

def footer(kind):
    return shell(65,819,1070 if kind!='harbor' else 1080,82,False,True,55)+shell(1160,819,440,82,True,False,56)+icon('book' if kind=='collection' else 'rod',90,839,66,GOLD)+icon('anchor',1188,841,48)

def make(kind):
    out=header()
    if kind=='harbor':
        out+=rect(49,526,1570,389,INK,r=0)
        for i,(x,w) in enumerate([(62,506),(588,496),(1102,506)]):
            out+=shell(x,534,w,226,False,True,i+10,heading=57)
            out+=icon(['rod','cards','book'][i],x+34,548,42)
        out+=footer(kind)
    else:
        out+=rect(49,285,1570,628,INK,r=0)
        out+=shell(65,289,1016,515,False,kind=='collection',31,heading=56)
        out+=shell(1105,289,503,515,False,True,32,heading=56)
        if kind=='shop':
            for i,x in enumerate([94,424,759]):
                out+=shell(x,359,308 if i!=1 else 318,370,False,True,40+i)
                out+=shell(x+5,364,298 if i!=1 else 308,178,True,False,60+i)
                out+=shell(x+5,545,298 if i!=1 else 308,46,True,False,70+i)
            out+=shell(1112,352,489,191,True,False,80)
            out+=path('M1168 617H1548M1168 679H1548',GOLD,.9)
        out+=shell(1153,731,416,62,True,False,81)
        out+=footer(kind)
    out+=page_edges()
    return '<svg xmlns="http://www.w3.org/2000/svg" width="3840" height="2160" viewBox="0 0 1672 941">'+out+'</svg>'

if __name__=='__main__':
    for kind in ['harbor','shop','collection']:
        path_out=folder/'Vectors'/f'{kind.title()}Frames.svg';path_out.write_text(make(kind),encoding='utf-8')
        print(json.dumps({'file':str(path_out),'bytes':path_out.stat().st_size,'width':3840,'height':2160,'embeddedRasterImages':0}))
