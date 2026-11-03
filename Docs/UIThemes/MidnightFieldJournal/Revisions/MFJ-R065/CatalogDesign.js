const s=storage.r065,{b,X,Y,master,walk,pic}=s,row=ROW_INDEX;
const cards=walk(b).filter(c=>c.name.startsWith('Portrait catalog card /')).slice(row*3,row*3+3);
for(const card of cards){const old=card.children.find(c=>c.name==='Native engraved equipment card');if(old)old.remove();const x=card.x-X,y=card.y-Y,frame=pic(card,master,'Equipment family / illustrated card frame',x,y,card.width,card.height);frame.sendToBack();
 for(const c of [...card.children]){
  if(c.name==='Category'){c.y=Y+y+34;c.fontSize='21';}
  if(c.name==='Name'){c.y=Y+y+69;c.fontSize='30';c.resize(436,48);}
  if(c.name==='Native card footer rule')c.hidden=true;
  if(c.name==='Price or ownership'){c.y=Y+y+453;c.fontSize='29';c.fills=[{fillColor:c.characters==='OWNED'?'#BED2B1':'#F0DFBD',fillOpacity:1}];}
 }
}
return {row,cards:cards.length};
