// Refine existing R043 instances in place; preserve R042 comparison design.
if(storage.modifierR044) return storage.modifierR044;
const old=storage.modifierR043;
const changes={removedScrollbars:[],portraits:[]};
const ids=[...old.records.map(r=>r.id),old.prototypeTopId,old.prototypeBottomId];
function walk(s){
 if(s.name.startsWith('Scrollbar /')){changes.removedScrollbars.push(s.id);s.remove();return;}
 if(s.name.startsWith('Dynamic / approved ') && s.name.endsWith(' portrait')){
  const parent=s.parent,scale=Math.min(48/s.width,48/s.height),w=s.width*scale,h=s.height*scale;
  s.resize(w,h);s.x=parent.x+(parent.width-w)/2;s.y=parent.y+(parent.height-h)/2;
  changes.portraits.push({id:s.id,name:s.name,width:w,height:h,parentId:parent.id,fit:'contain',padding:4});
 }
 for(const c of s.children??[])walk(c);
}
for(const id of ids)walk(penpotUtils.findShapeById(id));
const review=penpotUtils.findShapeById(old.scrollReviewBoardId);
for(const c of review.children)if(c.name==='Review note')c.characters='Four sources. Three visible at once. Scroll to see more; no scrollbar.';
storage.modifierR044={...old,revision:'MFJ-R044',updatesRevision:'MFJ-R043',changes,scrollbar:false,portraitFit:'contain',portraitPadding:4,unityImported:false,status:'ready_for_review'};
penpot.selection=[penpotUtils.findShapeById(old.reviewBoardId)];penpot.viewport.zoomIntoView(penpot.selection);
return storage.modifierR044;
