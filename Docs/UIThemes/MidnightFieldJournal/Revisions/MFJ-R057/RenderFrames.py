"""Rasterize authored nine-slice UI at its actual 4K display size using Chromium."""
import json,subprocess,tempfile,sys
from pathlib import Path

folder=Path(__file__).resolve().parent;sx=3840/1672;sy=2160/941
def shell(name,key,x,y,w,h,outer=False):return dict(name=name,master=key,x=x,y=y,w=w,h=h,outer=outer)
def layout(kind):
    cells=[shell('perimeter','Dark',0,0,1672,941,True),shell('wordmark','Paper',68,25,400,73),shell('navigation','Dark',510,20,760,77),shell('gold','Paper',1350,25,249,73)]
    if kind=='harbor':
        for i,x in enumerate([62,588,1102]):
            cells += [shell('destination'+str(i),'Dark',x,534,496 if i==1 else 506,226),shell('destination-title'+str(i),'Paper',x+9,540,478 if i==1 else 488,53)]
        cells += [shell('loadout','Dark',65,779,1080,102),shell('primary','Paper',1173,778,407,100)]
    else:
        cells += [shell('cabinet','Dark',65,289,1016,515),shell('filters','Paper',73,294,1000,57),shell('detail','Dark',1105,289,503,515),shell('detail-heading','Paper',1113,294,487,57),shell('primary','Paper',1153,731,416,62),shell('loadout','Dark',65,819,1070,82),shell('return','Paper',1160,819,440,82)]
        if kind=='shop':
            for i,x in enumerate([94,424,759]):
                cells += [shell('equipment'+str(i),'Dark',x,359,318 if i==1 else 308,370),shell('equipment-art'+str(i),'Paper',x+5,364,308 if i==1 else 298,178),shell('equipment-title'+str(i),'Paper',x+5,545,308 if i==1 else 298,46)]
            cells += [shell('detail-art','Paper',1112,352,489,191)]
    return cells
def box(x,y,w,h,extra=''):
    return f'position:absolute;left:{round(x*sx)}px;top:{round(y*sy)}px;width:{round(w*sx)}px;height:{round(h*sy)}px;{extra}'
def icon(kind,x,y,size):
    paths={'anchor':'M50 12V79M40 22A10 10 0 1 0 60 22A10 10 0 1 0 40 22M24 44H76M10 63Q12 88 50 90Q88 88 90 63M10 63L8 78M10 63L24 69M90 63L92 78M90 63L76 69','book':'M50 23Q28 6 8 20V82Q28 73 50 89Q72 73 92 82V20Q72 6 50 23V89M18 33L39 38M18 46L39 51M61 38L82 33M61 51L82 46','cards':'M14 19L60 9L80 77L35 89ZM34 14L82 22L76 88M40 42L53 34L64 52L50 63Z','rod':'M12 91L87 9M15 85L23 92M87 9Q67 16 77 58Q86 95 60 83M37 63A11 11 0 1 0 51 72A11 11 0 1 0 37 63','coin':'M12 50A38 38 0 1 0 88 50A38 38 0 1 0 12 50M20 50A30 30 0 1 0 80 50A30 30 0 1 0 20 50M43 36L61 45L55 64L37 55Z'}
    return f'<svg viewBox="0 0 100 100" style="{box(x,y,size,size)}"><path d="{paths[kind]}" fill="none" stroke="#9A6A35" stroke-width="3.3" stroke-linecap="round" stroke-linejoin="round"/></svg>'

if __name__=='__main__':
    kind=sys.argv[1];cells=layout(kind);parts=['<!doctype html><html><head><meta charset="utf-8"><style>html,body{margin:0;width:3840px;height:2160px;overflow:hidden;background:transparent}*{box-sizing:border-box}</style></head><body>']
    masks=[(0,0,1672,105),(0,0,60,941),(1612,0,60,941),(0,906,1672,35),(48,526 if kind=='harbor' else 285,1575,386 if kind=='harbor' else 628)]
    for x,y,w,h in masks:parts.append(f'<div style="{box(x,y,w,h,"background:#10262E;")}"></div>')
    for c in cells:
        w=round(c['w']*sx);h=round(c['h']*sy);edge=min(220,int(w*.2),int(h*.43));c.update(target=[w,h],sourceBorders=[300,500,560,500],targetBorders=[edge*.6,edge,edge*1.12,edge],sourceCorner=500,targetCorner=edge,cornerScale=edge/500)
        fill='' if c['outer'] else ' fill'
        uri=(folder/'Art'/f'{c["master"]}FrameMaster.png').as_uri()
        border_width=f'{edge*.6}px {edge}px {edge*1.12}px {edge}px'
        body='transparent'
        style=box(c['x'],c['y'],c['w'],c['h'],f'background:{body};background-clip:padding-box;border-style:solid;border-color:transparent;border-width:{border_width};border-image:url("{uri}") 300 500 560 500{fill} / {border_width} / 0 stretch;')
        parts.append(f'<div data-component="{c["name"]}" style=\'{style}\'></div>')
    parts += [icon('anchor',106,36,48),icon('coin',1372,40,42),icon('anchor',1205 if kind=='harbor' else 1188,802 if kind=='harbor' else 841,48),icon('book' if kind=='collection' else 'rod',95,797 if kind=='harbor' else 840,65)]
    if kind=='harbor':
        for x,key in zip([62,588,1102],['rod','cards','book']):parts.append(icon(key,x+34,548,42))
    parts.append('</body></html>');html=folder/(kind.title()+'Frames.html');html.write_text(''.join(parts),encoding='utf-8')
    out=folder/(kind.title()+'Frames4K.png')
    chrome=Path('C:/Program Files/Google/Chrome/Application/chrome.exe')
    profile=Path(tempfile.mkdtemp(prefix='fishing-ui-r057-'))
    args=[str(chrome),'--headless','--disable-gpu','--disable-gpu-sandbox','--in-process-gpu','--no-sandbox','--hide-scrollbars','--no-first-run','--no-default-browser-check','--allow-file-access-from-files','--force-device-scale-factor=1','--default-background-color=00000000','--window-size=3840,2160','--virtual-time-budget=2500','--user-data-dir='+str(profile),'--screenshot='+str(out),html.as_uri()]
    result=subprocess.run(args,capture_output=True,text=True,timeout=45)
    if not out.exists():raise RuntimeError(result.stderr[-1500:])
    (folder/(kind.title()+'FrameGeometry.json')).write_text(json.dumps(cells,indent=2),encoding='utf-8')
    print(json.dumps({'file':str(out),'bytes':out.stat().st_size,'components':len(cells),'cornerUpscaling':False}))
