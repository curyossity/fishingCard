"""Build editable SVG review boards from native shapes/text and the unchanged game capture."""
from pathlib import Path
from html import escape
import base64
import json

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[3]
ART = ROOT / 'Assets/UIThemes/TackleApparatus'
CAPTURE = ROOT / 'Docs/Validation/FiveTagAbilities/Build/Build-1920x1080.png'
W = 460
INK = '#F2E5C4'
GOLD = '#E9BC63'
MUTED = '#BEBAA8'
GREEN = '#B9DDC0'
AMBER = '#F1CA83'


def encoded(path, mime):
    return 'data:' + mime + ';base64,' + base64.b64encode(path.read_bytes()).decode()


def fonts():
    return '<style>@font-face{font-family:VT323;src:url(' + encoded(ART / 'Fonts/VT323-Regular.ttf', 'font/ttf') + ')}@font-face{font-family:"Press Start 2P";src:url(' + encoded(ART / 'Fonts/PressStart2P-Regular.ttf', 'font/ttf') + ')}</style>'


def text(x, y, content, size=28, fill=INK, family='VT323', anchor='start'):
    family = 'Press Start 2P' if family == 'PressStart' else family
    family = '&quot;' + family + '&quot;' if ' ' in family else family
    return f'<text x="{x}" y="{y}" font-family="{family}" font-size="{size}" fill="{fill}" text-anchor="{anchor}">{content}</text>'


def line(x1, y1, x2, y2, fill='#7C673D'):
    return f'<path d="M{x1} {y1}H{x2}" stroke="{fill}" stroke-width="1"/>' if y1 == y2 else f'<path d="M{x1} {y1}L{x2} {y2}" stroke="{fill}" stroke-width="1"/>'


def shape(h, name):
    # A stepped silhouette, fine brass bevel and enamel insert, constructed as editable paths.
    d = f'M12 0H448V4H456V12H460V{h-12}H456V{h-4}H448V{h}H12V{h-4}H4V{h-12}H0V12H4V4H12Z'
    out = f'<g id="{name}"><path d="{d}" transform="translate(5 7)" fill="#050B0D" opacity=".72"/>'
    out += f'<path d="{d}" fill="#302C20" stroke="#171B18" stroke-width="4"/>'
    out += f'<path d="{d}" fill="none" stroke="#B08B47" stroke-width="2"/>'
    out += f'<rect x="7" y="7" width="446" height="{h-14}" rx="6" fill="#1A2525" stroke="#65583D" stroke-width="2"/>'
    out += f'<rect x="12" y="12" width="436" height="{h-24}" rx="3" fill="#202725" stroke="#0B1516" stroke-width="2"/>'
    out += '<path d="M17 13H443" stroke="#DAC18A" stroke-width="1"/>'
    out += f'<path d="M17 {h-13}H443" stroke="#0A1011" stroke-width="2"/>'
    out += '<rect x="15" y="15" width="430" height="70" fill="#29312C"/>'
    out += line(17, 87, 443, 87, '#9D8049')
    out += line(17, 89, 443, 89, '#080F11')
    for x, y in [(12, 12), (448, 12), (12, h-12), (448, h-12)]:
        out += f'<g id="{name}-rivet-{x}-{y}"><rect x="{x-3}" y="{y-3}" width="6" height="6" fill="#C49C57"/><rect x="{x-1}" y="{y-1}" width="3" height="3" fill="#514830"/></g>'
    return out


DATA = {
    'rig': dict(name='CLEANER SHRIMP', rarity='UNCOMMON', tag='Armored', level=1, value=8, ability='Fresh Start', h=282,
                body=['Your first Miss grants', '<tspan fill="#E9BC63">+1 attempt</tspan>, once per encounter.'],
                note='Miss still scores 0 Landing Points.'),
    'preview': dict(name='GLASS MINNOW', rarity='COMMON', tag='Schooling', level=1, value=5, ability='Diverse Company', h=370,
                    body=['Successful strikes score <tspan fill="#E9BC63">×1.25</tspan>', 'with two adjacent neighbors', 'of different tags.'],
                    context='AFTER PLACEMENT', result='×1.25 on successful strikes', reason='Predator + Anchored neighbors.', color=GREEN),
    'pending': dict(name='ACORN BARNACLE', rarity='COMMON', tag='Anchored', level=1, value=6, ability='Balance Point', h=370,
                    body=['Gain <tspan fill="#E9BC63">+1 attempt</tspan> when exactly', 'at the center of your rig,', 'with creatures on both sides.'],
                    context='AFTER PLACEMENT', result='No extra attempt here', reason='Needs equal numbers on both sides.', color=AMBER),
    'scaling': dict(name='MACKEREL', rarity='COMMON', tag='Schooling', level=2, value=32, ability='Shared Fortune', h=370,
                    body=['Gain <tspan fill="#E9BC63">+6 sale value</tspan> per other', 'contiguous Schooling creature.'],
                    context='IN THIS RIG', result='+12 sale value', reason='2 other connected Schooling species.', color=GREEN),
}


def tooltip(key, x=0, y=0):
    d = DATA[key]
    out = f'<g id="tooltip-{key}" transform="translate({x} {y})">' + shape(d['h'], 'editable-tooltip-frame-'+key)
    out += text(27, 41, d['name'], 16, INK, 'PressStart')
    out += text(432, 39, d['rarity'], 18, MUTED, anchor='end')
    out += text(27, 72, d['tag'], 24, GOLD)
    out += text(174, 72, 'Lv ' + str(d['level']), 24)
    out += text(432, 72, 'SALE ' + str(d['value']), 24, INK, anchor='end')
    out += text(27, 126, d['ability'], 29, GOLD)
    for i, row in enumerate(d['body']):
        out += text(27, 164 + i*31, row, 28)
    if 'context' in d:
        out += line(27, 251, 433, 251)
        out += text(27, 278, d['context'], 21, MUTED)
        out += f'<rect x="27" y="290" width="3" height="49" fill="{d["color"]}"/>'
        out += text(41, 313, d['result'], 28, d['color'])
        out += text(41, 342, d['reason'], 22, MUTED)
    else:
        out += line(27, 221, 433, 221)
        out += text(27, 251, d['note'], 23, MUTED)
    return out + '</g></g>'


def svg(w, h, content):
    return f'<svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" width="{w}" height="{h}" viewBox="0 0 {w} {h}"><title>Deepwater — Creature hover details</title>' + fonts() + content + '</svg>'


def focus(x, y, w, h):
    return f'<g id="hovered-card-outline"><rect x="{x-4}" y="{y-4}" width="{w+8}" height="{h+8}" rx="7" fill="none" stroke="#191C17" stroke-width="6"/><rect x="{x-3}" y="{y-3}" width="{w+6}" height="{h+6}" rx="6" fill="none" stroke="{GOLD}" stroke-width="3"/></g>'


def background():
    return f'<image id="unchanged-runtime-reference" x="0" y="0" width="1920" height="1080" xlink:href="{encoded(CAPTURE, "image/png")}"/>'


def write(name, body):
    (HERE / name).write_text(body, encoding='utf-8')


def build():
    HERE.mkdir(parents=True, exist_ok=True)
    for key in DATA:
        write(f'tooltip-{key}.svg', svg(W+8, DATA[key]['h']+8, tooltip(key)))
    write('01-left-rig.svg', svg(1920, 1080, background() + focus(72, 486, 299, 237) + tooltip('rig', 396, 260)))
    write('02-insertion-preview.svg', svg(1920, 1080, background() + focus(1338, 606, 207, 177) + tooltip('preview', 1214, 216)))

    sheet = '<rect width="1920" height="1200" fill="#101C20"/>'
    sheet += text(80, 74, 'DEEPWATER / CREATURE DETAILS', 24, GOLD, 'PressStart')
    sheet += text(80, 120, 'One reusable hover panel. Current-level rules first; placement consequences only when relevant.', 29)
    labels = [('rig',80,'01  /  LEFT RIG'),('preview',730,'02  /  INSERTION PREVIEW'),('pending',1380,'03  /  POSITION CONDITION')]
    for key,x,label in labels:
        sheet += text(x, 184, label, 24, MUTED) + tooltip(key,x,208)
    sheet += text(80, 652, '04  /  UPGRADED + RESOLVED VALUE', 24, MUTED) + tooltip('scaling',80,676)
    sheet += '<g id="optional-keyword-help" transform="translate(610 713)"><rect width="350" height="175" rx="5" fill="#26322E" stroke="#907548"/>'
    sheet += text(24, 36, 'CONTIGUOUS', 24, GOLD)
    for i,row in enumerate(['An uninterrupted group of', 'matching creatures. Another', 'tag breaks the connection.']):
        sheet += text(24, 75+i*29, row, 25)
    sheet += '</g>'
    sheet += text(610, 937, 'Only explain terms used by the ability.', 25, MUTED)
    sheet += text(1090, 704, 'INTERACTION CONTRACT', 25, GOLD)
    notes = ['Hover: 180 ms delay; focus: immediate.', 'Anchor to the card, not the moving cursor.', 'Left rig: open to the right.', 'Insertion tray: open above, flip at edges.', 'Keep the hovered card and gap buttons clear.', 'Escape dismisses; scroll/phase change hides.', 'No actions, reorder handles or triangle tail.']
    for i,row in enumerate(notes): sheet += text(1090,749+i*38,row,26)
    sheet += line(80, 1104, 1840, 1104)
    sheet += text(80, 1144, '460 px panel / 28 px body at 1080p / native editable text / supplied typefaces', 25, MUTED)
    write('00-component-sheet.svg', svg(1920,1200,sheet))

    html = '''<!doctype html><html lang="en"><meta charset="utf-8"><title>Deepwater — Card tooltip review</title>
<style>body{margin:0;background:#101c20;color:#f2e5c4;font:18px system-ui}header{padding:18px 24px;display:flex;flex-wrap:wrap;align-items:center;gap:24px;border-bottom:1px solid #75603d}strong{color:#e9bc63}header span{color:#bebaa8;font-size:14px}button{font:inherit;color:inherit;background:#26322e;border:1px solid #907548;padding:9px 14px;border-radius:4px;cursor:pointer}button:focus-visible{outline:3px solid #e9bc63;outline-offset:3px}.stage{position:relative;width:min(100%,1920px);aspect-ratio:16/9;margin:24px auto}.stage>img{width:100%;display:block}.hotspot{position:absolute;padding:0;background:none;border:0;border-radius:6px}.hotspot:hover,.hotspot:focus-visible{outline:3px solid #e9bc63;outline-offset:3px}.tip{position:absolute;width:24.375%;height:auto;pointer-events:none;display:none}.tip img{display:block;width:100%}.tip.open{display:block}.rig{left:20.625%;top:24.074%}.preview{left:63.23%;top:20%}.pending{left:59%;top:20%}.sheet{display:block;width:min(100%,1600px);margin:40px auto}footer{padding:0 24px 30px;color:#bebaa8;font-size:14px}a{color:#e9bc63}</style>
<header><strong>DEEPWATER / HOVER DETAILS</strong><span>Design prototype — not installed in Unity or Penpot</span><button data-show="rig">Left rig</button><button data-show="preview">Insertion tray</button><button data-show="pending">Pending creature</button><button id="hide">Hide</button></header>
<div class="stage"><img src="../../../Validation/FiveTagAbilities/Build/Build-1920x1080.png" alt="Existing Deepwater insertion screen">
<button class="hotspot" style="left:3.75%;top:45%;width:15.57%;height:21.94%" data-tip="rig" aria-label="Inspect Cleaner Shrimp"></button>
<button class="hotspot" style="left:69.6875%;top:56.11%;width:10.78%;height:16.39%" data-tip="preview" aria-label="Inspect Glass Minnow in the insertion preview"></button>
<button class="hotspot" style="left:34.48%;top:28.43%;width:23.18%;height:21.3%" data-tip="pending" aria-label="Inspect Acorn Barnacle before placement"></button>
<div id="rig" class="tip rig"><img src="tooltip-rig.svg" alt="Cleaner Shrimp: Fresh Start"></div>
<div id="preview" class="tip preview"><img src="tooltip-preview.svg" alt="Glass Minnow: Diverse Company, after placement"></div>
<div id="pending" class="tip pending"><img src="tooltip-pending.svg" alt="Acorn Barnacle: Balance Point, not centered here"></div></div>
<footer>Hover a highlighted source card or use Tab to inspect. Escape dismisses. Preview values are illustrative and do not change game state. <a href="00-component-sheet.svg">Open the editable component sheet</a>.</footer><img class="sheet" src="00-component-sheet.svg" alt="Tooltip components and behavior specifications">
<script>let timer,hiddenTarget;function hide(){clearTimeout(timer);document.querySelectorAll('.tip').forEach(t=>t.classList.remove('open'))}function show(id){hide();document.getElementById(id).classList.add('open')}document.querySelectorAll('[data-tip]').forEach(b=>{b.addEventListener('pointerenter',()=>{hiddenTarget=null;clearTimeout(timer);timer=setTimeout(()=>show(b.dataset.tip),180)});b.addEventListener('pointerleave',()=>{clearTimeout(timer);timer=setTimeout(hide,80)});b.addEventListener('focus',()=>show(b.dataset.tip));b.addEventListener('blur',hide);b.addEventListener('click',()=>show(b.dataset.tip))});document.querySelectorAll('[data-show]').forEach(b=>b.onclick=()=>show(b.dataset.show));document.getElementById('hide').onclick=hide;document.addEventListener('keydown',e=>{if(e.key==='Escape')hide()});window.addEventListener('resize',hide);window.addEventListener('scroll',hide,{passive:true});</script></html>'''
    write('Review.html', html)
    write('DesignManifest.json',json.dumps(dict(status='Prepared locally; Penpot connector unavailable',date='2026-11-19',referenceResolution=[1920,1080],panelWidth=W,bodyFont='VT323',bodyFontSize=28,titleFont='Press Start 2P',hoverDelayMs=180,hideDelayMs=80,sourceCapture=str(CAPTURE.relative_to(ROOT)).replace('\\','/'),boards=['00-component-sheet.svg','01-left-rig.svg','02-insertion-preview.svg']),indent=2)+'\n')
    print('Created editable tooltip components, two contextual boards and an interactive review.')


if __name__ == '__main__':
    build()
