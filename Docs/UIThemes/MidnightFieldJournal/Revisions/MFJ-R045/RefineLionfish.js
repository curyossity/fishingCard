const d=storage.modifierR045;
const svg='<svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 64 64"><path fill="#304448" d="M7 34Q11 25 22 25L18 10L29 23L31 6L36 23L45 11L44 27Q48 28 51 31L61 22L58 35L61 48L49 39Q39 46 27 42L19 54L20 41Q11 40 7 34Z"/><circle cx="17" cy="32" r="2.2" fill="#F3E7CB"/></svg>';
let count=0;
function walk(s){if(s.name==='Icon / Lionfish / flat ink / no backing'){const x=s.x,y=s.y,p=s.parent,n=s.name;s.remove();const g=penpot.createShapeFromSvg(svg);p.appendChild(g);g.name=n;g.x=x;g.y=y;count++;return;}for(const c of s.children??[])walk(c);}
for(const r of d.records.filter(r=>r.stat==='Value'&&r.rows.length>1))walk(penpotUtils.findShapeById(r.id));
return {refinedLionfishIcons:count};
