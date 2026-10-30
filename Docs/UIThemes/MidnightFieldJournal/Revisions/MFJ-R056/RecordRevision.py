"""Append the Collection design without altering completed Unity checkpoints."""
import hashlib,json,struct
from pathlib import Path

folder=Path(__file__).resolve().parent;root=folder.parent.parent;project=folder.parents[4]
def read(path):return json.loads(path.read_text(encoding='utf-8'))
def write(path,data):
    temp=path.with_suffix(path.suffix+'.tmp');temp.write_text(json.dumps(data,indent=2,ensure_ascii=False)+'\n',encoding='utf-8');temp.replace(path)
design=read(folder/'PenpotDesign.json');sources=read(folder/'GameCardSources.json')
assert len(design['cardInstances'])==8
checks=[]
for key,s in sources.items():
    checks.append({'resource':key,'matchesActiveGameResource':hashlib.sha256((project/s['source']).read_bytes()).hexdigest()==s['sha256']==hashlib.sha256((folder/s['file']).read_bytes()).hexdigest()})
assert all(c['matchesActiveGameResource'] for c in checks)
ledger=read(root/'DesignChangeLedger.json');template=next(c for c in ledger['changes'] if c['id']=='MFJ-R055')
entry={**template,'id':'MFJ-R056','date':'2026-10-30',
 'request':'Design the Technique Collection in the revised Harbor style and reuse actual in-run Technique card faces; user clarified Event cards was a typo.',
 'component':['Technique Collection expanded view','Reusable collection card instances','Starting deck visual editor'],
 'supersedes':[],'penpotSources':design,'affectedPenpotInstances':[design['boardId']],
 'changeKinds':['new_collection_screen','exact_in_game_card_resources','native_collection_fields'],
 'before':'The historical R052 collection used simplified card layouts and did not match the revised Harbor framing.',
 'after':'Ornate Collection screen containing all eight active Technique designs using exact frame/art copies with native names and rules, external ownership labels and adjacent starting-deck list.',
 'exportRequirements':['Retain native per-card fields and exact existing game frame/art. Recreate outer decorative shells independently after approval; do not slice the review composition.'],
 'approvalEvidence':['User requested Collection with same instructions as revised Harbor and shop. User clarified Technique cards, not Event cards.'],
 'exportEvidence':{'folder':'Revisions/MFJ-R056','files':['TechniqueCollection.jpg','TechniqueCollection4K.jpg'],'cardSources':'Revisions/MFJ-R056/GameCardSources.json','sourceIdentityChecks':checks},
 'verificationEvidence':{'report':'Revisions/MFJ-R056/Verification.json'},
 'remainingWork':['Visual review. Ownership, example price and alternate deck are illustrative states. No new gameplay definitions or Unity integration.']}
ledger['changes']=[c for c in ledger['changes'] if c['id']!='MFJ-R056']+[entry];ledger['nextRevisionId']='MFJ-R057';write(root/'DesignChangeLedger.json',ledger)
pending=read(root/'PendingUnityChanges.json');pending['pending']=[p for p in pending['pending'] if p['id']!='MFJ-R056']+[{'id':'MFJ-R056','status':'ready_for_review','unityImported':False,'source':'Revisions/MFJ-R056/README.md'}];write(root/'PendingUnityChanges.json',pending)
for filename,marker,replacement in [
 ('Design Changes.md','**Two pending designs:','**Three pending designs: MFJ-R053 Harbor, MFJ-R055 Tackle Shop and MFJ-R056 Technique Collection.** The Collection uses exact active in-run Technique frame/art sources with editable text. All 40 completed revisions and 13 superseded studies retain their statuses; Unity checkpoint remains MFJ-SYNC-009. [Collection review](Revisions/MFJ-R056/README.md) · [Pending index](PendingUnityChanges.json).'),
 ('VisualContract.md','**MFJ-SYNC-009 is the latest Unity checkpoint.**','**MFJ-SYNC-009 is the latest Unity checkpoint.** 40 revisions remain imported and verified; 13 studies are superseded. Pending review: [R053 Harbor](Revisions/MFJ-R053/README.md), [R055 Tackle Shop](Revisions/MFJ-R055/README.md) and [R056 Technique Collection](Revisions/MFJ-R056/README.md). R056 reuses exact active in-run Technique frame and illustration resources with native names/rules, in an upright collection grid. It does not use Event cards. No new Unity import.')]:
    p=root/filename;text=p.read_text(encoding='utf-8');start=text.index(marker);end=text.index('\n',start);text=text[:start]+replacement+text[end:]
    if filename=='Design Changes.md':text+='\n\n## MFJ-R056 — Technique Collection\n\nNew ornate collection with all eight active Technique faces using exact game frame/art and separate native fields. External owned/in-deck/locked state and starting-deck editor are visual only; data is illustrative. Pending review, not imported. [Review and source identity evidence](Revisions/MFJ-R056/README.md).\n'
    p.write_text(text,encoding='utf-8')
before=read(folder.parent/'MFJ-R052/UnityBefore.json');expected={f['path']:f['sha256'] for f in before['files']}
actual={p.relative_to(project).as_posix():hashlib.sha256(p.read_bytes()).hexdigest() for p in (project/'Assets').rglob('*') if p.is_file()}
delta=[k for k in expected.keys()|actual.keys() if expected.get(k)!=actual.get(k)]
dimensions=struct.unpack('>II',(folder/'Art/CollectionBlank.png').read_bytes()[16:24])
write(folder/'Verification.json',{'revision':'MFJ-R056','screenCount':1,'boardSize':[3840,2160],'generatedBackdropSize':dimensions,'techniqueCards':len(design['cardInstances']),'editableTextFields':len(design['editableTextFields']),'sourceIdentityChecks':checks,'visualReview':'Export visually inspected: exact game frames/art, native titles/rules and external state labels; collection and deck fit ornate panels.','unityImported':False,'unityAssetsComparison':{'checkedFiles':len(actual),'differences':delta,'unchanged':not delta}})
print(json.dumps({'pending':[p['id'] for p in pending['pending']],'sourceIdentityChecks':len(checks),'unityAssetDifferences':delta}))
