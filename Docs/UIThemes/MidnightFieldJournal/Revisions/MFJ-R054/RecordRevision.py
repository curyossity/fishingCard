"""Record only the new Tackle Shop study without changing completed imports."""
import hashlib,json,struct
from pathlib import Path

folder=Path(__file__).resolve().parent
root=folder.parent.parent
project=folder.parents[4]
def read(path):return json.loads(path.read_text(encoding='utf-8'))
def write(path,data):
    tmp=path.with_suffix(path.suffix+'.tmp')
    tmp.write_text(json.dumps(data,indent=2,ensure_ascii=False)+'\n',encoding='utf-8');tmp.replace(path)
design=read(folder/'PenpotDesign.json')
if 'boardId' not in design:raise RuntimeError('Penpot creation not verified')
source=folder/'Art/TackleShopOrnateBlank.png'
dimensions=struct.unpack('>II',source.read_bytes()[16:24])
ledger=read(root/'DesignChangeLedger.json')
previous=next(c for c in ledger['changes'] if c['id']=='MFJ-R053')
entry={**previous,
 'id':'MFJ-R054','date':'2026-10-30',
 'request':'Create the expanded Tackle Shop with the same ornate card-style instructions as the revised Harbor.',
 'component':['Tackle Shop expanded view'],'supersedes':[],
 'penpotSources':design,'affectedPenpotInstances':[design['boardId']],
 'changeKinds':['expanded_shop_screen','ornate_frame','independent_equipment_art','editable_shop_fields'],
 'before':'Only the simpler, visually superseded R052 expanded shop exists.',
 'after':'One ornate workshop screen matching R053, with independent equipment art, editable ownership and gold fields, a selected item panel and single-slot loadout.',
 'exportRequirements':['Visual review only; independently recreate decorative shells and scenery at production resolution after approval. Do not slice the composition. Keep native labels, state and equipment illustrations separate.'],
 'approvalEvidence':['User requested the Tackle Shop with the same instructions after the main Harbor. Visual approval remains pending.'],
 'exportEvidence':{'folder':'Revisions/MFJ-R054','files':['TackleShop.jpg','TackleShop4K.jpg'],'sourceRasterDimensions':list(dimensions),'boardDimensions':[3840,2160]},
 'verificationEvidence':{'report':'Revisions/MFJ-R054/Verification.json'},
 'remainingWork':['Visual review. Equipment examples and prices are illustrative, not approved gameplay definitions. Production asset separation follows style approval.']
}
ledger['changes']=[c for c in ledger['changes'] if c['id']!='MFJ-R054']+[entry]
ledger['nextRevisionId']='MFJ-R055'
write(root/'DesignChangeLedger.json',ledger)
pending=read(root/'PendingUnityChanges.json')
pending['pending']=[p for p in pending['pending'] if p['id']!='MFJ-R054']+[{'id':'MFJ-R054','status':'ready_for_review','unityImported':False,'source':'Revisions/MFJ-R054/README.md'}]
write(root/'PendingUnityChanges.json',pending)
p=root/'Design Changes.md';s=p.read_text(encoding='utf-8')
start=s.index('**One pending design: MFJ-R053');end=s.index('\n',start)
s=s[:start]+'**Two pending designs: MFJ-R053 ornate Harbor and MFJ-R054 matching Tackle Shop, ready for review.** R052 remains superseded for visual direction. All 40 completed revisions and 12 superseded studies retain their statuses. Unity checkpoint remains MFJ-SYNC-009. [Harbor](Revisions/MFJ-R053/README.md) · [Tackle Shop](Revisions/MFJ-R054/README.md) · [Pending index](PendingUnityChanges.json).'+s[end:]
s+='\n\n## MFJ-R054 — Matching ornate Tackle Shop\n\nOne expanded shop screen with detailed parchment/copper framing, navy information panels, separate equipment illustrations and native editable labels. Shows equipped, purchasable and journal-locked states with a one-item loadout. Illustrative design data only; no new equipment mechanics. Pending review, not integrated. [Review](Revisions/MFJ-R054/README.md).\n'
p.write_text(s,encoding='utf-8')
p=root/'VisualContract.md';s=p.read_text(encoding='utf-8')
start=s.index('**MFJ-SYNC-009 is the latest Unity checkpoint.**');end=s.index('\n',start)
s=s[:start]+'**MFJ-SYNC-009 is the latest Unity checkpoint.** 40 revisions remain imported and verified; 12 studies are superseded. [MFJ-R053 ornate Harbor](Revisions/MFJ-R053/README.md) and [MFJ-R054 matching Tackle Shop](Revisions/MFJ-R054/README.md) are pending visual review. R052 remains historical structure only. These are visual studies, not separated production asset sets; equipment art and text in R054 are independent. No new Unity import. [Current checkpoint](Sync009/UnityCheckpoint.json).'+s[end:]
p.write_text(s,encoding='utf-8')
before=read(folder.parent/'MFJ-R052/UnityBefore.json')
expected={f['path']:f['sha256'] for f in before['files']}
actual={p.relative_to(project).as_posix():hashlib.sha256(p.read_bytes()).hexdigest() for p in (project/'Assets').rglob('*') if p.is_file()}
delta=[k for k in expected.keys()|actual.keys() if expected.get(k)!=actual.get(k)]
write(folder/'Verification.json',{'revision':'MFJ-R054','screenCount':1,'boardSize':[3840,2160],'sourceRasterDimensions':dimensions,'editableTextFields':len(design['editableTextFields']),'independentEquipmentArtwork':len(design['independentEquipmentArtwork']),'visualReview':'Penpot export inspected for text fit, illustration containment and decoration collisions.','productionAssetSeparation':'Equipment artwork and native text separate; decorative composition remains review-only.','unityImported':False,'unityAssetsComparison':{'checkedFiles':len(actual),'differences':delta,'unchanged':not delta},'priorHarborRetained':True})
print(json.dumps({'pending':[p['id'] for p in pending['pending']],'unityAssetDifferences':delta}))
