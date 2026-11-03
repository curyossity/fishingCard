const s=storage.r063,{b,X,Y,C,content,board,text,image,svg}=s;
const specs=[{key:'rod',name:'Driftwood Rod',state:'200 GOLD',type:'ROD'},{key:'reel',name:'Tideglass Reel',state:'OWNED',type:'REEL'},{key:'hook',name:'Barbed Hook',state:'250 GOLD',type:'HOOK'}];
const row=ROW_INDEX;
for(let col=0;col<3;col++){
 const item=specs[col],x=155+col*740,y=1080+row*650,index=row*3+col;
 const tile=board('Placeholder '+(index+1)+' / '+item.name,X+x,Y+y,680,590,content);
 image(tile,'compact',x,y,680,389);
 image(tile,item.key,x+125,y+55,430,245,true);
 text(tile,'Item name',item.name,x+20,y+414,640,55,36,C.ivory,false,'center');
 text(tile,'Ownership or price',item.state,x+20,y+481,640,48,30,item.key==='reel'?C.green:C.gold,false,'center');
 text(tile,'Item category',item.type,x+20,y+540,640,40,24,C.muted,false,'center');
 if(index===1)svg(tile,'Selected item outline','<rect x="2" y="2" width="676" height="385" rx="23" fill="none" stroke="#E7C58D" stroke-width="4"/>',x,y,680,389);
 s.catalog.push({id:tile.id,item:item.key,index,placeholder:true});
}
return {row,itemCount:s.catalog.length};
