function board(parent,name,x,y,w,h){const s=penpot.createBoard();s.name=name;s.resize(w,h);s.fills=[];if(parent)parent.appendChild(s);s.x=(parent?.x||0)+x;s.y=(parent?.y||0)+y;return s;}
function rect(parent,name,x,y,w,h,c,opacity=1,r=0){const s=penpot.createRectangle();parent.appendChild(s);s.name=name;s.resize(w,h);s.x=parent.x+x;s.y=parent.y+y;s.fills=[{fillColor:c,fillOpacity:opacity}];s.borderRadius=r;return s;}
function text(parent,name,content,x,y,w,h,size,c=ink,heading=false,align='left'){const s=penpot.createText(content);parent.appendChild(s);s.name=name;const f=heading?headingFont:bodyFont;f.applyToText(s,f.variants.find(v=>v.fontWeight==='400'&&v.fontStyle==='normal'));s.fontSize=String(size);s.fills=[{fillColor:c,fillOpacity:1}];s.resize(w,h);s.x=parent.x+x;s.y=parent.y+y;s.align=align;return s;}
function svg(parent,name,x,y,w,h,paths,view=`0 0 ${w} ${h}`){const s=penpot.createShapeFromSvg(`<svg xmlns="http://www.w3.org/2000/svg" width="${w}" height="${h}" viewBox="${view}">${paths}</svg>`);parent.appendChild(s);s.name=name;s.x=parent.x+x;s.y=parent.y+y;return s;}
const glyphs=[
 '<path d="M5 16C11 7 22 7 28 16C22 25 11 25 5 16ZM5 16L1 10V22ZM22 14h.1"/>',
 '<path d="M3 12Q16 2 29 12L25 26H7ZM5 13L10 20L14 13L18 20L23 13M11 7l-3 -3M21 7l3 -3"/>',
 '<path d="M4 9Q10 2 17 9Q10 16 4 9L1 5v8ZM17 17Q23 10 30 17Q23 24 17 17L14 13v8ZM4 25Q10 18 17 25Q10 32 4 25L1 21v8Z"/>',
 '<path d="M10 16Q16 9 22 16Q16 23 10 16L7 12v8ZM3 6H0v20h3M29 6h3v20h-3"/>',
 '<path d="M10 10V7a6 6 0 0 1 12 0v3M7 11H25L29 29H3Z"/>',
 '<circle cx="16" cy="5" r="3"/><path d="M16 8V28M8 12h16M3 19Q3 29 16 29Q29 29 29 19M3 19l-2 5M3 19l5 2M29 19l2 5M29 19l-5 2"/>',
 '<path d="M16 2L29 8V17Q27 27 16 31Q5 27 3 17V8ZM16 7V26M8 11h16M9 17h14M12 23h8"/>',
 '<path d="M25 3Q8 3 8 16Q8 29 25 29Q17 24 17 16Q17 8 25 3ZM1 10h4M0 17h5M1 24h4"/>'
];
function icon(parent,i,x,y,size,c=ink){return svg(parent,'Tag glyph / '+tags[i],x,y,size,size,`<g fill="none" stroke="${c}" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round">${glyphs[i]}</g>`,'0 0 32 32');}
