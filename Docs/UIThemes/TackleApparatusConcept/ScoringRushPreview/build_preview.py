"""Embed optimized copies of existing art in a self-contained animation concept; never modify Unity assets."""
from pathlib import Path
import base64
import io
import json
from PIL import Image

HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[3]
ART=ROOT/'Assets/UIThemes/TackleApparatus'

def data(path, max_size=None, photo=False):
    with Image.open(path) as original:
        image=original.copy()
        if max_size:
            image.thumbnail(max_size,Image.Resampling.LANCZOS)
        buffer=io.BytesIO()
        image.save(buffer,format='WEBP',quality=78 if photo else 88,method=6)
    return 'data:image/webp;base64,'+base64.b64encode(buffer.getvalue()).decode()

assets={
    'backdrop':data(ROOT/'Docs/Validation/LandingIntegrated/Landing-1920x1080.png',(1600,900),True),
    'card':data(ART/'Art/Common/rig-card-ivory.png',(420,340)),
    'sardine':data(ART/'Art/Creatures/creature_sardine.png',(400,200)),
    'cuttlefish':data(ART/'Art/Creatures/creature_cuttlefish.png',(400,200)),
    'tuna':data(ART/'Art/Creatures/apex_stormjaw_tuna.png',(400,200)),
    'fish':data(ART/'Art/Landing/Indicators/generic-fish.png',(100,70)),
}
def font(name):return 'data:font/ttf;base64,'+base64.b64encode((ART/'Fonts'/name).read_bytes()).decode()
fragment=(HERE/'score-rush.template.html').read_text(encoding='utf-8')
fragment=fragment.replace('__ASSETS__',json.dumps(assets,separators=(',',':'))).replace('__PIXEL_FONT__',font('PressStart2P-Regular.ttf')).replace('__BODY_FONT__',font('VT323-Regular.ttf'))
assert len(fragment.encode())<1024*1024,'Inline preview exceeds size budget'
(HERE/'score-rush.html').write_text(fragment,encoding='utf-8')
print(json.dumps({'bytes':len(fragment.encode()),'images':len(assets),'path':str(HERE/'score-rush.html')}))
