const s=storage.r064,{X,Y,R,C,content,board,text,image,svg}=s,row=ROW_INDEX;
const spec=[{key:'rod',name:'Driftwood Rod',type:'ROD',status:'200 GOLD'},{key:'reel',name:'Tideglass Reel',type:'REEL',status:'OWNED'},{key:'hook',name:'Barbed Hook',type:'HOOK',status:'250 GOLD'}];
for(let col=0;col<3;col++){
 const item=spec[col],x=255+740*col,y=1090+row*665,w=500,h=w/R,index=row*3+col,card=board('Portrait catalog card / '+(index+1)+' / '+item.key,X+x,Y+y,w,h,content);
 svg(card,'Native engraved equipment card',`<rect x="3" y="6" width="494" height="${h-9}" rx="21" fill="#B69B74" fill-opacity=".25"/><rect x="2" y="2" width="496" height="${h-9}" rx="20" fill="#F1E4CA" stroke="#8D704F" stroke-width="2"/><rect x="14" y="14" width="472" height="${h-33}" rx="12" fill="none" stroke="#B69C74" stroke-width="1"/><g fill="none" stroke="#9B805B" stroke-width="1.5"><path d="M30 72Q28 29 72 30M428 ${h-33}Q473 ${h-31} 470 ${h-75}"/><path d="M34 50Q56 54 54 31M447 ${h-31}Q447 ${h-54} 471 ${h-52}"/></g>`,x,y,w,h);
 text(card,'Category',item.type,x+60,y+38,380,36,23,C.muted,false,'center');
 text(card,'Name',item.name,x+32,y+88,436,56,34,C.ink,false,'center');
 image(card,item.key,x+70,y+166,360,230,true);
 svg(card,'Native card footer rule','<path d="M0 1H330" stroke="#A7916D" stroke-width="1"/>',x+85,y+429,330,3);
 text(card,'Price or ownership',item.status,x+55,y+466,390,48,31,item.key==='reel'?C.green:C.ink,false,'center');
 if(index===1)svg(card,'Selected card keyline',`<rect x="1" y="1" width="498" height="${h-3}" rx="20" stroke="#45685E" stroke-width="4" fill="none"/>`,x,y,w,h);
 s.catalog.push({id:card.id,item:item.key,index,placeholder:true,width:w,height:h});
}
return {row,count:s.catalog.length};
