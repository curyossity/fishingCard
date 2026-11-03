import math,random,json,shutil
from pathlib import Path
root=Path('Docs/UIThemes/MidnightFieldJournal/Revisions/MFJ-R066');(root/'Art').mkdir(parents=True,exist_ok=True)
shutil.copyfile('C:/Users/user/.codex/generated_images/01a12664-7dcf-7183-b8bc-4582ea9b6f12/exec-0dbf509c-7361-4906-b3a6-a5cd0fac4254.png',root/'Art/GeneratedConceptOnly.png')
rng=random.Random(6601)
def grain(n,x0,y0,w,h):
 out=[]
 for _ in range(n):
  x=x0+rng.random()*w;y=y0+rng.random()*h;s=.7+rng.random()*1.5
  out.append(f'M{x:.2f} {y:.2f}l{s:.2f} .3l-.4 {s:.2f}l{-s:.2f} -.6Z')
 return ''.join(out)
def spool(x,y,scale=1,angle=0):
 thread=''.join(f'<path d="M{35+i*3} 13Q{45+i*3} 48 {35+i*3} 87"/>' for i in range(21))
 hatch=''.join(f'M{20+i*3} 19l-11 18M{21+i*3} 65l-10 16' for i in range(9))
 return f'''<g transform="translate({x} {y}) rotate({angle} 75 50) scale({scale})"><path d="M28 10L105 10Q127 49 105 90L28 90Z" fill="#B69A6A" stroke="#37464A" stroke-width="2"/><g stroke="#4C5650" stroke-width="1" fill="none">{thread}</g><path d="M34 14L104 14M34 87L104 87" stroke="#D5B885" stroke-width="2"/><ellipse cx="30" cy="50" rx="27" ry="43" fill="#DDC395" stroke="#35454A" stroke-width="2"/><ellipse cx="30" cy="50" rx="21" ry="35" fill="none" stroke="#7D704F" stroke-width="1.5"/><ellipse cx="30" cy="50" rx="9" ry="18" fill="#34464A" stroke="#A38758" stroke-width="2"/><path d="{hatch}" fill="none" stroke="#5F6553" stroke-width=".9"/><path d="M13 17Q36 0 48 24M12 84Q33 98 47 78" fill="none" stroke="#F3DEB0" stroke-width="1.2"/><ellipse cx="108" cy="50" rx="22" ry="40" fill="none" stroke="#536057" stroke-width="1.5"/></g>'''
def float_art(x,y,scale=1,angle=0):
 lines=''.join(f'M{25+i*4} 25Q{20+i*4} 43 {25+i*4} 65' for i in range(6))
 return f'''<g transform="translate({x} {y}) rotate({angle} 36 48) scale({scale})"><path d="M36 0V20M36 74V96" stroke="#384B4C" stroke-width="2"/><path d="M36 18C5 20 6 71 36 77C66 71 67 20 36 18Z" fill="#E5CBA0" stroke="#374A4B" stroke-width="2"/><path d="M12 45Q36 53 60 45L57 61Q36 71 15 61Z" fill="#9E8154" stroke="#48564F" stroke-width="1"/><path d="{lines}" stroke="#756E50" stroke-width=".7" fill="none"/><path d="M23 28Q14 39 19 52" stroke="#F3E6C7" stroke-width="2" fill="none"/></g>'''
def grass(x,y,w=100,h=240,flip=False):
 pieces=[f'<path d="M{w/2} {h}Q{w*.3} {h*.55} {w*.6} 0" fill="none" stroke="#5D6756" stroke-width="2"/>']
 for i in range(8):
  yy=h*(.15+i*.10);xx=w*(.49-.12*math.sin(i));side=-1 if i%2 else 1;tipx=xx+side*w*(.26+.04*(i%3));tipy=yy-h*.13
  pieces.append(f'<path d="M{xx} {yy+23}Q{tipx} {yy+10} {tipx} {tipy}Q{xx} {tipy+10} {xx} {yy+23}Z" fill="#667262" fill-opacity=".56" stroke="#536454" stroke-width="1"/><path d="M{xx} {yy+23}L{tipx} {tipy}" stroke="#9D8355" stroke-width=".7"/>')
 for i in range(5):
  xx=w*(.22+i*.14);yy=h*(.1+i*.12)
  pieces.append(f'<path d="M{w/2} {h}Q{xx} {yy+80} {xx} {yy}" fill="none" stroke="#71705A" stroke-width="1.2"/><ellipse cx="{xx}" cy="{yy}" rx="5" ry="10" transform="rotate({i*12-20} {xx} {yy})" fill="#AE8F59" stroke="#535D50" stroke-width="1"/><path d="M{xx-2} {yy-7}l3 13" stroke="#EDD4A4" stroke-width=".7"/>')
 return f'<g transform="translate({x} {y})'+(f' translate({w} 0) scale(-1 1)' if flip else '')+'">'+''.join(pieces)+'</g>'
svg=f'''<svg xmlns="http://www.w3.org/2000/svg" width="3840" height="2160" viewBox="0 0 3840 2160">
<defs><linearGradient id="navy" x1="0" y1="0" x2="1" y2="1"><stop stop-color="#101D24"/><stop offset="1" stop-color="#14232A"/></linearGradient><linearGradient id="paper" x1="0" y1="0" x2=".8" y2="1"><stop stop-color="#F0E3C6"/><stop offset=".6" stop-color="#E9D6B2"/><stop offset="1" stop-color="#E4CDA8"/></linearGradient></defs>
<rect width="3840" height="2160" fill="url(#navy)"/>
<path d="{grain(650,0,0,3840,2160)}" fill="#A7B0A5" opacity=".065"/>
<path d="M81 66L350 69L700 65L1150 68L1580 65L2000 70L2504 65L2510 330L2505 750L2510 1210L2506 1590L2510 2090L2140 2095L1700 2091L1230 2095L760 2090L300 2095L70 2090L74 1680L70 1200L75 770L70 330Z" fill="url(#paper)" stroke="#A78B61" stroke-width="1"/>
<path d="{grain(650,88,85,2390,1985)}" fill="#806D4D" opacity=".075"/>
<g fill="none" stroke="#736C53" stroke-width="1.5"><path d="M113 100Q1200 87 2465 100M115 150Q105 1100 117 2078Q1280 2083 2460 2078Q2475 1180 2462 151"/><path d="M105 166Q109 1080 107 2008M2453 170Q2465 1120 2452 2006" stroke-opacity=".55"/></g>
<g fill="none" stroke="#8B7650" stroke-width="1.4"><path d="M113 143C90 115 109 89 136 113C158 139 99 154 110 126C124 97 169 110 183 137C199 162 147 167 153 143C158 126 189 128 198 139"/><path d="M2459 144C2490 124 2484 100 2455 113C2421 125 2473 158 2480 135C2488 110 2436 100 2423 131C2408 157 2375 152 2383 135C2390 121 2420 137 2436 142"/><path d="M111 487C90 471 91 451 112 457C132 467 129 497 110 511M2459 784C2482 770 2488 797 2470 805C2447 815 2440 784 2458 775M112 1430C81 1410 94 1378 111 1390C137 1409 123 1444 108 1452"/></g>
<g fill="none" stroke="#756747" stroke-width="1.4"><path d="M131 2050C240 2113 292 2044 252 2026C211 2006 198 2086 284 2076C388 2065 392 2030 355 2031C324 2029 331 2080 421 2070M2420 2051C2299 2090 2244 2073 2272 2042C2299 2014 2319 2077 2248 2079C2174 2081 2140 2048 2169 2042C2200 2036 2190 2078 2085 2074"/><path d="M118 2037Q123 2047 133 2056M2458 2037Q2445 2043 2432 2055" stroke="#B09769" stroke-width="2"/></g>
<path d="M3810 2118Q3700 2146 3675 2115Q3658 2087 3692 2094Q3720 2110 3684 2133Q3590 2152 3548 2132" fill="none" stroke="#9D855B" stroke-width="1.4"/>
</svg>'''
(root/'Art/ShopBackground4K.svg').write_text(svg,encoding='utf-8')
code="const p=penpot.currentPage;const a=penpot.createShapeFromSvg("+json.dumps(svg)+");a.name='MFJ-R066 / Original native 4K shop background';a.x=88800;a.y=27000;storage.r066Background=a;return {id:a.id,width:a.width,height:a.height};"
Path('Temp/PenpotR066CreateBackground.js').write_text(code,encoding='utf-8')
print(json.dumps({'nativeCanvas':[3840,2160],'svgBytes':len(svg.encode()),'oldAssetsUsed':False}))
