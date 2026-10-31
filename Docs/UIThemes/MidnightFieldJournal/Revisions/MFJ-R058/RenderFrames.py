"""Render independent reusable ornaments from the approved masters at their exact 4K display sizes."""
from concurrent.futures import ThreadPoolExecutor
import hashlib
import json
from pathlib import Path
import subprocess
import tempfile

folder = Path(__file__).resolve().parent
masters = folder.parent / 'MFJ-R057/Art'
frames = [
    dict(key='perimeter', master='Dark', w=3840, h=2160, outer=True),
    dict(key='wordmark', master='Paper', w=919, h=168),
    dict(key='navigation', master='Dark', w=1745, h=177),
    dict(key='gold', master='Paper', w=572, h=168),
    dict(key='sidebar', master='Dark', w=975, h=1580),
    dict(key='illustration', master='Paper', w=895, h=1130),
    dict(key='quest', master='Dark', w=2480, h=500),
    dict(key='questTitle', master='Paper', w=2440, h=115),
    dict(key='footer', master='Dark', w=2500, h=188),
    dict(key='return', master='Paper', w=1010, h=188),
]

def render(frame):
    key, w, h = frame['key'], frame['w'], frame['h']
    edge = min(220, int(w * .2), int(h * .43))
    borders = f'{edge*.6}px {edge}px {edge*1.12}px {edge}px'
    source = (masters / (frame['master'] + 'FrameMaster.png')).as_uri()
    fill = '' if frame.get('outer') else ' fill'
    html = folder / 'Frames' / (key + '.html')
    png = html.with_suffix('.png')
    html.parent.mkdir(exist_ok=True)
    html.write_text(f'''<!doctype html><html><head><style>html,body{{margin:0;width:{w}px;height:{h}px;background:transparent;overflow:hidden}}.frame{{box-sizing:border-box;width:{w}px;height:{h}px;border-style:solid;border-color:transparent;border-width:{borders};border-image:url("{source}") 300 500 560 500{fill} / {borders} / 0 stretch;}}</style></head><body><div class="frame"></div></body></html>''', encoding='utf-8')
    profile = Path(tempfile.mkdtemp(prefix='fishing-journal-r058-'))
    args = ['C:/Program Files/Google/Chrome/Application/chrome.exe', '--headless', '--disable-gpu', '--disable-gpu-sandbox', '--in-process-gpu', '--no-sandbox', '--hide-scrollbars', '--no-first-run', '--no-default-browser-check', '--allow-file-access-from-files', '--force-device-scale-factor=1', '--default-background-color=00000000', f'--window-size={w},{max(200,h)}', '--virtual-time-budget=800', '--user-data-dir=' + str(profile), '--screenshot=' + str(png), html.as_uri()]
    subprocess.run(args, capture_output=True, timeout=40, check=True)
    # Chromium minimum viewport height is 200. Use a viewport screenshot of precisely the authored frame via CDP when shorter.
    if h < 200:
        frame['viewportHeight'] = 200
        frame['imageDisplayHeight'] = 200
    else:
        frame['imageDisplayHeight'] = h
    frame.update(sourceBorders=[300,500,560,500], cornerScale=edge/500, targetBorders=[edge*.6,edge,edge*1.12,edge], source=str(Path('../MFJ-R057/Art')/(frame['master']+'FrameMaster.png')), file='Frames/'+key+'.png', sha256=hashlib.sha256(png.read_bytes()).hexdigest())
    return frame

if __name__ == '__main__':
    with ThreadPoolExecutor(max_workers=2) as pool:
        result = list(pool.map(render, frames))
    (folder/'FrameGeometry.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
    print(json.dumps({'frames':len(result),'largestCornerScale':max(f['cornerScale'] for f in result)}))
