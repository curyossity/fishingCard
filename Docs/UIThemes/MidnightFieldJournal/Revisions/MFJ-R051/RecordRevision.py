"""Record the next selected refinement while retaining immutable Unity integration history."""
import hashlib
import json
from pathlib import Path
import re

folder=Path(__file__).resolve().parent;root=folder.parents[1];repo=root.parents[2]
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def write(p,d):
    t=p.with_suffix(p.suffix+'.tmp');t.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8');t.replace(p)
state=read(folder/'PenpotDesign.json');b=state['bars'].get('0',{})
audit=read(folder/'Action-audit.json') if (folder/'Action-audit.json').exists() else {}
review=read(folder/'VisualReview.json') if (folder/'VisualReview.json').exists() else {'passed':False}
before=read(folder/'UnityBefore.json')
modified=[f['path'] for f in before['files'] if not (repo/f['path']).exists() or hashlib.sha256((repo/f['path']).read_bytes()).hexdigest()!=f['sha256']]
exports=['DesignReview.png','LanternAnchorPrecision.png']
checks={'oneCompleteRefinement':len(state['bars'])==1 and b.get('complete',False),
        'eightInBarPrecisionAreas':len(audit.get('zones',[]))==8 and all(z['centred'] and z['behindText'] and abs(z['precisionWidth']-32.4)<.02 and z['precisionTop']==56 and z['precisionBottom']==116 for z in audit.get('zones',[])),
        'nativeUpwardFade':bool(audit) and all(z.get('gradientStops') and z['gradientStops'][0]['opacity']==.62 and z['gradientStops'][-1]['opacity']==0 for z in audit['zones']),
        'oldBracketsRemoved':bool(audit) and not any(z['oldBracketPresent'] for z in audit['zones']),
        'noActiveTagHighlight':bool(audit) and not any(z['activeHighlight'] for z in audit['zones']),
        'visibleOverlayMarker':bool(audit) and audit['marker']['visible'] and audit['marker']['overlay'] and audit['marker']['width']>=51 and audit['marker']['top']<118 and audit['marker']['bottom']>234,
        'exportsExist':all((folder/f).exists() for f in exports),'visualReviewPassed':review['passed'],'unityFilesUnchanged':not modified}
status='ready_for_review' if all(checks.values()) else 'in_progress'
write(folder/'Verification.json',{'revision':'MFJ-R051','date':'2026-10-28','checks':checks,'unityCheckpointUnchanged':before['checkpoint'],'unityFileCount':len(before['files']),'modifiedUnityFiles':modified,'visualReview':review})
ledger=read(root/'DesignChangeLedger.json');previous=next(c for c in ledger['changes'] if c['id']=='MFJ-R050')
entry=json.loads(json.dumps(previous).replace('MFJ-R050','MFJ-R051'))
for key in ['supersededBy','supersessionReason']:entry.pop(key,None)
entry.update(id='MFJ-R051',date='2026-10-28',status=status,integrationStatus='not_imported',supersedes=['MFJ-R050'],
             request='Replace the cursor with a visible over-bar marker ending in an anchor; move precision areas into the bar as upward tapering highlights fading near text height.',
             penpotSources=state,affectedPenpotInstances=[state['reviewId'],b['id']],
             changeKinds=['marker_redesign','precision_placement','gradient_fade'],
             before='R050 had two separate cursor pointers outside the channel and centre brackets below the bar.',
             after='Independent anchor cursor overlays the channel; eight tapered ochre precision areas rise from its bottom edge and fade beneath native text. No active-tag fill.',
             approvalEvidence=['User requested the anchor-bottom overlay marker and upward-fading in-bar precision areas.'],
             exportEvidence={'folder':'Revisions/MFJ-R051','files':exports if checks['exportsExist'] else []},
             remainingWork=['Unity integration is outside this Penpot refinement task.'] if status=='ready_for_review' else ['Finish exported visual checks.'])
previous.update(status='superseded',integrationStatus='superseded',supersededBy='MFJ-R051',supersessionReason='Selected design refined with an anchor cursor and in-bar fading precision areas.',remainingWork=[])
ledger['changes']=[c for c in ledger['changes'] if c['id']!='MFJ-R051']+[entry];ledger['nextRevisionId']='MFJ-R052';write(root/'DesignChangeLedger.json',ledger)
index=read(root/'PendingUnityChanges.json');index['generatedDate']='2026-10-28'
index['pending']=[{'id':c['id'],'status':c['status'],'unityImported':False,'source':'Revisions/'+c['id']+'/README.md'} for c in ledger['changes'] if not c['unityImported'] and c['status']!='superseded']
index['superseded']=[{'id':c['id'],'supersededBy':c.get('supersededBy'),'unityImported':c['unityImported']} for c in ledger['changes'] if c['status']=='superseded']
index['counts']={'pending':len(index['pending']),'completed':len(index['completed']),'superseded':len(index['superseded'])};write(root/'PendingUnityChanges.json',index)
p=root/'Pending Unity Changes.md';s=p.read_text(encoding='utf-8')
s=re.sub(r'\*\*Current:.*?\n','**Current: 1 pending selected design; 39 imported and verified; 11 superseded drafts/studies.** R051 is '+status+'. R050 is superseded. [Pending index](PendingUnityChanges.json) · [Full ledger](DesignChangeLedger.json).\n',s,count=1)
s=s.replace('Next revision: **MFJ-R051**.','Next revision: **MFJ-R052**.')
if '## Anchor cursor and rising precision — MFJ-R051' not in s:s+='\n## Anchor cursor and rising precision — MFJ-R051\n\nThe selected parchment bar now uses an overlay cursor ending in an anchor, with all eight precision highlights inside the channel, rising and fading from its bottom edge. R050 is historical; only R051 remains pending. [Design and verification](Revisions/MFJ-R051/README.md). Unity is unchanged.\n'
p.write_text(s,encoding='utf-8')
p=root/'Design Changes.md';s=p.read_text(encoding='utf-8')
s=re.sub(r'(## Pending Unity changes\s*\n)\*\*.*?\n',r'\1**One pending selected design: MFJ-R051** ('+status+'). R050 is superseded; all39 prior integrations remain verified. [Pending index](PendingUnityChanges.json).\n',s,count=1)
def archive_row(m):
    a=m.group(0).rstrip('\n').split('|');a[3]=' Superseded by MFJ-R051; not imported. ';a[4]=' Earlier selected refinement retained as historical evidence. ';return '|'.join(a)+'\n'
s=re.sub(r'^\| MFJ-R050 \|.*\n',archive_row,s,flags=re.MULTILINE)
row='| MFJ-R051 | Anchor-bottom cursor over the bar; eight in-channel tapered precision highlights fading upward. | '+status+'; Penpot only. | Not imported; [evidence](Revisions/MFJ-R051/README.md). |\n'
if '| MFJ-R051 |' in s:s=re.sub(r'^\| MFJ-R051 \|.*\n',lambda m:row,s,flags=re.MULTILINE)
else:
    m=list(re.finditer(r'^\| MFJ-R050 \|.*\n',s,re.MULTILINE))[-1];s=s[:m.end()]+row+s[m.end():]
p.write_text(s,encoding='utf-8')
p=root/'VisualContract.md';s=p.read_text(encoding='utf-8').replace('One pending selected Penpot refinement, MFJ-R050, continues Lantern Focus in light parchment with all precision centres marked and a larger cursor.','One pending selected Penpot refinement, MFJ-R051, continues Lantern Focus in light parchment with a visible anchor cursor over the bar and eight upward-fading precision areas inside it.')
p.write_text(s,encoding='utf-8')
p=root/'Revisions/MFJ-R050/README.md';s=p.read_text(encoding='utf-8')
if '**Superseded by MFJ-R051.**' not in s:
    first,rest=s.split('\n',1);p.write_text(first+'\n\n**Superseded by MFJ-R051.** Historical refinement only; the current anchor cursor and in-bar fading precision design is [R051](../MFJ-R051/README.md). Earlier review notes below describe the previous state.\n'+rest,encoding='utf-8')
print(json.dumps({'status':status,'counts':index['counts'],'checks':checks}))
