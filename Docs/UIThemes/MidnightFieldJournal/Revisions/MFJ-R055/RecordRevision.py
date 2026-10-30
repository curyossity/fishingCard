import hashlib,json,struct
from pathlib import Path

folder=Path(__file__).resolve().parent
root=folder.parent.parent
project=folder.parents[4]
def read(path):return json.loads(path.read_text(encoding='utf-8'))
def write(path,data):
    tmp=path.with_suffix(path.suffix+'.tmp')
    tmp.write_text(json.dumps(data,indent=2,ensure_ascii=False)+'\n',encoding='utf-8');tmp.replace(path)
state=read(folder/'PenpotDesign.json')
if len(state.get('equipmentArtwork',[]))!=4:raise RuntimeError('Expected three shop item illustrations and one selected detail image')
sources=[]
for p in sorted((folder/'Art').glob('*.png')):
    data=p.read_bytes()
    width,height=struct.unpack('>II',data[16:24])
    sources.append({'file':str(p.relative_to(folder)),'width':width,'height':height,'hasAlphaChannel':data[25] in (4,6),'sha256':hashlib.sha256(data).hexdigest()})
ledger=read(root/'DesignChangeLedger.json')
prev=next(c for c in ledger['changes'] if c['id']=='MFJ-R054')
entry={**prev,'id':'MFJ-R055','date':'2026-10-30',
 'request':'Recreate shop equipment illustrations to match the creature cards, keeping the shop layout.',
 'component':['Driftwood Rod illustration','Tideglass Reel illustration','Barbed Hook illustration','Tackle Shop current review board'],
 'status':'ready_for_review','supersedes':['MFJ-R054'],
 'penpotSources':state,'affectedPenpotInstances':[state['boardId']],
 'changeKinds':['replacement_equipment_illustrations'],
 'before':'R054 equipment illustrations looked like rendered inventory objects and did not share the creature-card illustration style.',
 'after':'Three new separate transparent etched illustrations with muted naturalist washes; replaced all four shop equipment instances while preserving the layout and text.',
 'exportRequirements':['Use the independent R055 item sources, not the R052 item art. Retain R054/R053 layout and production decorative-asset boundaries.'],
 'approvalEvidence':['User liked the shop and requested new item artwork referencing the creature cards. New artwork remains pending review.'],
 'exportEvidence':{'folder':'Revisions/MFJ-R055','files':['TackleShop.jpg','TackleShop4K.jpg'],'sources':sources},
 'verificationEvidence':{'report':'Revisions/MFJ-R055/Verification.json'},
 'remainingWork':['Review replacement artwork. No Unity integration requested. Decorative shop shells remain review-only composition.']}
prev.update(status='superseded',supersededBy='MFJ-R055')
prev['remainingWork']=['Layout retained by R055. Use R055 equipment sources and review board for future integration; keep this version as comparison only.']
ledger['changes']=[c for c in ledger['changes'] if c['id']!='MFJ-R055']+[entry]
ledger['nextRevisionId']='MFJ-R056';write(root/'DesignChangeLedger.json',ledger)
pending=read(root/'PendingUnityChanges.json')
pending['pending']=[p for p in pending['pending'] if p['id'] not in ['MFJ-R054','MFJ-R055']]+[{'id':'MFJ-R055','status':'ready_for_review','unityImported':False,'source':'Revisions/MFJ-R055/README.md'}]
if not any(p['id']=='MFJ-R054' for p in pending['superseded']):pending['superseded'].append({'id':'MFJ-R054','supersededBy':'MFJ-R055','unityImported':False})
write(root/'PendingUnityChanges.json',pending)
for filename,start_marker,replacement in [
 ('Design Changes.md','**Two pending designs:', '**Two pending designs: MFJ-R053 ornate Harbor and MFJ-R055 Tackle Shop with recreated engraved equipment.** R055 retains R054 layout and replaces its item art. All 40 completed revisions remain unchanged; 13 studies are superseded. Unity checkpoint remains MFJ-SYNC-009. [Current shop](Revisions/MFJ-R055/README.md) · [Pending index](PendingUnityChanges.json).'),
 ('VisualContract.md','**MFJ-SYNC-009 is the latest Unity checkpoint.**','**MFJ-SYNC-009 is the latest Unity checkpoint.** 40 revisions remain imported and verified; 13 studies are superseded. [R053 Harbor](Revisions/MFJ-R053/README.md) and [R055 Tackle Shop with engraved equipment](Revisions/MFJ-R055/README.md) are pending. R055 preserves R054 layout and replaces its item art with independent transparent naturalist illustrations. R052 and R054 remain comparisons, not future import sources. No new Unity import.')]:
    p=root/filename;s=p.read_text(encoding='utf-8');start=s.index(start_marker);end=s.index('\n',start);s=s[:start]+replacement+s[end:]
    if filename=='Design Changes.md':s+='\n\n## MFJ-R055 — Recreated shop equipment\n\nNew transparent engraved Rod, Reel and Hook based on approved creature art. R054 layout preserved in the new review board, including replacement of the selected Reel detail. Pending review, not integrated. [Review and source files](Revisions/MFJ-R055/README.md).\n'
    p.write_text(s,encoding='utf-8')
p=folder.parent/'MFJ-R054/README.md';s=p.read_text(encoding='utf-8')
if not s.startswith('> Equipment artwork superseded'):p.write_text('> Equipment artwork superseded by [MFJ-R055](../MFJ-R055/README.md). Its current shop board retains this layout; use the new item illustrations for future integration.\n\n'+s,encoding='utf-8')
before=read(folder.parent/'MFJ-R052/UnityBefore.json');expected={f['path']:f['sha256'] for f in before['files']}
actual={p.relative_to(project).as_posix():hashlib.sha256(p.read_bytes()).hexdigest() for p in (project/'Assets').rglob('*') if p.is_file()}
delta=[k for k in expected.keys()|actual.keys() if expected.get(k)!=actual.get(k)]
write(folder/'Verification.json',{'revision':'MFJ-R055','screenCount':1,'independentItemInstances':4,'uniqueNewIllustrations':3,'sources':sources,'visualReview':'All item silhouettes fit without cropping; new etching style and restrained washes match creature art. Shop text, borders and layout preserved.','unityImported':False,'unityAssetsComparison':{'checkedFiles':len(actual),'differences':delta,'unchanged':not delta}})
print(json.dumps({'pending':[p['id'] for p in pending['pending']],'unityAssetDifferences':delta,'sources':sources}))
