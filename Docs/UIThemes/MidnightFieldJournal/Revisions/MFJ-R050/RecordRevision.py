"""Record the selected refinement and archive unchosen concept studies."""
import hashlib
import json
from pathlib import Path
import re

folder=Path(__file__).resolve().parent
root=folder.parents[1];repo=root.parents[2]
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def write(p,d):
    t=p.with_suffix(p.suffix+'.tmp');t.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8');t.replace(p)
state=read(folder/'PenpotDesign.json');b=state['bars'].get('0',{})
audit=read(folder/'Action-audit.json') if (folder/'Action-audit.json').exists() else {}
review=read(folder/'VisualReview.json') if (folder/'VisualReview.json').exists() else {'passed':False}
before=read(folder/'UnityBefore.json')
modified=[f['path'] for f in before['files'] if not (repo/f['path']).exists() or hashlib.sha256((repo/f['path']).read_bytes()).hexdigest()!=f['sha256']]
exports=['DesignReview.png','LanternFocusParchment.png']
checks={'oneCompleteRefinement':len(state['bars'])==1 and b.get('complete',False),
        'eightPermanentCentres':len(audit.get('zones',[]))==8 and all(z['precisionVisible'] and z['centred'] and abs(z['width']-180)<.01 and abs(z['precisionWidth']-32.4)<.01 for z in audit.get('zones',[])),
        'noActiveTagHighlight':bool(audit) and not any(z['activeHighlight'] for z in audit['zones']),
        'lightSharedPalette':bool(audit) and not audit['darkWashPresent'] and all(z['tagColour'].lower()=='#293d40' for z in audit['zones']),
        'largerVisibleMarker':bool(audit) and audit['marker']['visible'] and audit['marker']['width']>=39 and audit['marker']['height']>=215,
        'exportsExist':all((folder/f).exists() for f in exports),'visualReviewPassed':review['passed'],'unityFilesUnchanged':not modified}
status='ready_for_review' if all(checks.values()) else 'in_progress'
write(folder/'Verification.json',{'revision':'MFJ-R050','date':'2026-10-28','checks':checks,'unityCheckpointUnchanged':before['checkpoint'],'unityFileCount':len(before['files']),'modifiedUnityFiles':modified,'visualReview':review})
old_ids=['MFJ-R047','MFJ-R048','MFJ-R049']
entry={'id':'MFJ-R050','date':'2026-10-28','request':'Continue with R048 Lantern Focus: light parchment, all eight centres marked, no current-tag highlight, more visible marker.',
       'component':['Selected Lantern Focus tag bar'],'status':status,'supersedes':old_ids,
       'penpotSources':state,'affectedPenpotInstances':[state['reviewId']]+([b['id']] if b else []),
       'changeKinds':['selected_concept_refinement','palette','precision_guides','marker_visibility'],
       'before':'Dark Lantern Focus highlighted one tag and showed only that tag’s precision guide with a small marker.',
       'after':'Light parchment, uniform ink tags, eight permanent centre brackets, no active-tag fill, larger dual-ended dark cursor. Original studies preserved.',
       'unityAssetIds':[],'unityPaths':[],'dependentConsumers':[],'configurationChanges':[],
       'exportRequirements':['Only the selected R050 design may enter a later import checklist; superseded studies remain historical.'],
       'approvalEvidence':['User selected 03 — The Lantern Focus and specified this refinement; the R049 alternatives were rejected.'],
       'exportEvidence':{'folder':'Revisions/MFJ-R050','files':exports if checks['exportsExist'] else []},'importEvidence':{},
       'verificationEvidence':{'report':'Revisions/MFJ-R050/Verification.json'},
       'remainingWork':['Unity integration is not part of this Penpot design task.'] if status=='ready_for_review' else ['Complete native and rendered visual checks.'],
       'unityImported':False,'integrationStatus':'not_imported','synchronizationCheckpoint':None}
ledger=read(root/'DesignChangeLedger.json')
for c in ledger['changes']:
    if c['id'] in old_ids:
        c.update(status='superseded',integrationStatus='superseded',supersededBy='MFJ-R050',remainingWork=[])
        c['supersessionReason']='User selected the R048 Lantern Focus concept for refinement as R050; other proposals are historical, not import candidates.'
ledger['changes']=[c for c in ledger['changes'] if c['id']!='MFJ-R050']+[entry];ledger['nextRevisionId']='MFJ-R051';write(root/'DesignChangeLedger.json',ledger)
index=read(root/'PendingUnityChanges.json');index['generatedDate']='2026-10-28'
index['pending']=[{'id':c['id'],'status':c['status'],'unityImported':False,'source':'Revisions/'+c['id']+'/README.md'} for c in ledger['changes'] if not c['unityImported'] and c['status']!='superseded']
index['superseded']=[{'id':c['id'],'supersededBy':c.get('supersededBy'),'unityImported':c['unityImported']} for c in ledger['changes'] if c['status']=='superseded']
index['counts']={'pending':len(index['pending']),'completed':len(index['completed']),'superseded':len(index['superseded'])};write(root/'PendingUnityChanges.json',index)
p=root/'Pending Unity Changes.md';s=p.read_text(encoding='utf-8')
s=re.sub(r'\*\*Current:.*?\n','**Current: 1 pending selected design; 39 imported and verified; 10 superseded drafts/studies.** R050 is '+status+'. R047/R048/R049 are historical and excluded from import. [Pending index](PendingUnityChanges.json) · [Full ledger](DesignChangeLedger.json).\n',s,count=1)
s=s.replace('Next revision: **MFJ-R050**.','Next revision: **MFJ-R051**.')
if '## Selected Lantern Focus — MFJ-R050' not in s:s+='\n## Selected Lantern Focus — MFJ-R050\n\nLight parchment, eight permanent centre marks, no active-tag wash and a larger dark cursor. This is the only pending tag-bar design. Earlier concepts remain as historical Penpot boards. [Design and verification](Revisions/MFJ-R050/README.md). Unity remains unchanged.\n'
p.write_text(s,encoding='utf-8')
p=root/'Design Changes.md';s=p.read_text(encoding='utf-8')
s=re.sub(r'(## Pending Unity changes\s*\n)\*\*.*?\n',r'\1**One pending selected design: MFJ-R050** ('+status+'). R047/R048/R049 are superseded; all39 prior integrations remain verified. [Pending index](PendingUnityChanges.json).\n',s,count=1)
for old_id in old_ids:
    def archive_row(m):
        parts=m.group(0).rstrip('\n').split('|');parts[3]=' Superseded by MFJ-R050; not imported. ';parts[4]=' Historical study retained; selected Lantern Focus continues in R050. ';return '|'.join(parts)+'\n'
    s=re.sub(r'^\| '+old_id+r' \|.*\n',archive_row,s,flags=re.MULTILINE)
row='| MFJ-R050 | Selected Lantern Focus: parchment palette, eight centre brackets, no tag highlight and a larger dark cursor. | '+status+'; Penpot only. | Not imported; [evidence](Revisions/MFJ-R050/README.md). |\n'
if '| MFJ-R050 |' in s:s=re.sub(r'^\| MFJ-R050 \|.*\n',lambda m:row,s,flags=re.MULTILINE)
else:
    m=list(re.finditer(r'^\| MFJ-R049 \|.*\n',s,re.MULTILINE))[-1];s=s[:m.end()]+row+s[m.end():]
p.write_text(s,encoding='utf-8')
p=root/'VisualContract.md';s=p.read_text(encoding='utf-8')
s=s.replace('Three pending Penpot-only studies, MFJ-R047/R048/R049, explore tag-timing visuals and instrument forms; none replaces the live rarity bar.','One pending selected Penpot refinement, MFJ-R050, continues Lantern Focus in light parchment with all precision centres marked and a larger cursor. R047/R048/R049 are superseded historical studies. The live Unity rarity bar is unchanged.')
p.write_text(s,encoding='utf-8')
for old_id in old_ids:
    p=root/'Revisions'/old_id/'README.md';s=p.read_text(encoding='utf-8')
    if '**Superseded by MFJ-R050.**' not in s:
        first,rest=s.split('\n',1);s=first+'\n\n**Superseded by MFJ-R050.** Retained as historical design evidence; excluded from future import. The selected Lantern Focus is continued in [R050](../MFJ-R050/README.md). Earlier status notes below describe the historical review.\n'+rest
        p.write_text(s,encoding='utf-8')
print(json.dumps({'status':status,'counts':index['counts'],'checks':checks}))
