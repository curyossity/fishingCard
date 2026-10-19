from pathlib import Path
import json,base64,subprocess
root=Path('Docs/UIThemes/MidnightFieldJournal/Revisions/MFJ-R028');temp=Path('Temp/PenpotMcp/r028_render');temp.mkdir(exist_ok=True)
cards=[{'id':id,'x':x,'y':y,'angle':a,'order':i} for i,(id,x,y,a) in enumerate(zip(['cut_loose','slip_past','steady_line','prize_hook'],[500,1000,1500,2000],[480,400,400,480],[-15,-5,5,15]))]
config={'canvas':[2500,866],'perspective':2000,'tiltX':40,'cardSize':[620,640],'transformOrigin':[1250,433],'cards':cards,'purpose':'Penpot layout preview only; reuse native individual Unity views at future import, never the flattened preview.'}
(root/'FanLayout.json').write_text(json.dumps(config,indent=2)+'\n')
def html(single=None,dark=False):
 bits=[]
 for i,c in enumerate(cards):
  if single is not None and i!=single:continue
  uri='data:image/png;base64,'+base64.b64encode((root/'Sources'/f'Card{i}.png').read_bytes()).decode()
  bits.append(f'<img src="{uri}" style="left:{c["x"]-310}px;top:{c["y"]-320}px;transform:rotateZ({c["angle"]}deg);z-index:{i};">')
 return '<!doctype html><style>*{box-sizing:border-box}html,body{margin:0;width:2500px;height:866px;overflow:hidden;background:'+('#101c22' if dark else 'transparent')+'}.camera{position:absolute;width:2500px;height:866px;perspective:2000px;perspective-origin:50% 50%}.table{position:absolute;inset:0;transform:rotateX(40deg);transform-style:preserve-3d}img{position:absolute;width:620px;height:640px;filter:drop-shadow(0px 14px 10px #0009);transform-origin:50% 50%;}</style><div class="camera"><div class="table">'+''.join(bits)+'</div></div>'
exe='C:/Program Files/Google/Chrome/Application/chrome.exe'
for name,single,dark in [('FanPreview',None,True),('Card0Projected',0,False),('Card1Projected',1,False),('Card2Projected',2,False),('Card3Projected',3,False)]:
 page=temp/(name+'.html');page.write_text(html(single,dark),encoding='utf-8')
 dest=(root/(name+'.png')).resolve()
 z=subprocess.run([exe,'--headless=new','--disable-gpu','--no-sandbox','--no-first-run','--no-default-browser-check','--hide-scrollbars','--force-device-scale-factor=1','--window-size=2500,866','--default-background-color=00000000','--user-data-dir='+str((temp/('ChromeProfile_'+name)).resolve()),'--screenshot='+str(dest),page.resolve().as_uri()],capture_output=True,timeout=55)
 assert dest.exists(),z.stderr.decode(errors='replace')[-2000:]
 print(name,dest.stat().st_size,flush=True)



