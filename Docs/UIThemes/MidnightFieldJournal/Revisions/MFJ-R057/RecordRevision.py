"""Record verified progress without treating generation/export as a Unity import."""
import hashlib,json,struct,sys
from pathlib import Path

folder=Path(__file__).resolve().parent;root=folder.parent.parent;project=folder.parents[4]
def read(path):return json.loads(path.read_text(encoding='utf-8'))
def write(path,data):
    p=path.with_suffix(path.suffix+'.tmp');p.write_text(json.dumps(data,indent=2,ensure_ascii=False)+'\n',encoding='utf-8');p.replace(path)
complete='--complete' in sys.argv
design=read(folder/'PenpotDesign.json') if (folder/'PenpotDesign.json').exists() else {}
if complete:
    assert set(design.get('screens',{}))=={'harbor','shop','collection'}
    assert all(s.get('method')=='native-4k-fixed-corner-render' for s in design['screens'].values())
    assert all(f['cornerScale']<=1 for s in design['screens'].values() for f in s['shells'])
    for name in ['Harbor4K.png','Shop4K.png','Collection4K.png']:
        assert struct.unpack('>II',(folder/name).read_bytes()[16:24])==(3840,2160)
    live=read(folder/'PenpotVerification.json')
    assert live['allForegroundsPreserved'] and live['allFramesNative4K']
status='ready_for_review' if complete else 'in_progress'
sources=[]
for p in (folder/'Art').glob('*.png'):
    data=p.read_bytes();sources.append({'file':str(p.relative_to(folder)),'dimensions':struct.unpack('>II',data[16:24]),'sha256':hashlib.sha256(data).hexdigest()})
ledger=read(root/'DesignChangeLedger.json');template=next(c for c in ledger['changes'] if c['id']=='MFJ-R056')
entry={**template,'id':'MFJ-R057','date':'2026-10-30','status':status,
 'request':'Fix blurry UI frames on the new Harbor/shop screens for sharp 4K presentation.',
 'component':['Harbor framing','Tackle Shop framing','Technique Collection framing','Reusable dark and parchment frame masters'],
 'supersedes':[],'penpotSources':design,'affectedPenpotInstances':['75555645-d407-8067-8008-de8ff58e2639','75555645-d407-8067-8008-de9b9c9710d1','75555645-d407-8067-8008-de9d62501389'],
 'changeKinds':['resolution_correction','independent_frame_masters','nine_slice_construction'],
 'before':'UI framing was baked into 1672x941 full-screen art and enlarged about 2.3x. 4K JPEG exports did not add detail.',
 'after':'Independent detailed frame masters assembled with fixed/downsampled corners and separately resized edges/fills; text, equipment and exact Technique sources retained.' if complete else 'New frame masters and assembly tools are saved. Penpot assembly/visual verification remains in progress; do not import these screens yet.',
 'exportRequirements':['Use final verified R057 frame sources for R053/R055/R056. Do not import the rejected Vectors study. Scenic illustrations remain lower-resolution review art; this is a frame correction, not a claim that the entire backdrop is native 4K.'],
 'approvalEvidence':['User requested correction of blurry frames at 4K.'],
 'exportEvidence':{'folder':'Revisions/MFJ-R057','sources':sources,'files':['Harbor4K.png','Shop4K.png','Collection4K.png'] if complete else []},
 'verificationEvidence':{'report':'Revisions/MFJ-R057/Verification.json'},
 'remainingWork':['Visual review. No Unity integration requested.'] if complete else ['Keep Penpot foreground with plugin open, finish all three screen assemblies, inspect rendered frames and save lossless 4K exports.']}
ledger['changes']=[c for c in ledger['changes'] if c['id']!='MFJ-R057']+[entry];ledger['nextRevisionId']='MFJ-R058';write(root/'DesignChangeLedger.json',ledger)
pending=read(root/'PendingUnityChanges.json');pending['pending']=[p for p in pending['pending'] if p['id']!='MFJ-R057']+[{'id':'MFJ-R057','status':status,'unityImported':False,'source':'Revisions/MFJ-R057/README.md'}];write(root/'PendingUnityChanges.json',pending)
for filename,marker,summary in [
 ('Design Changes.md','**Three pending designs:', '**Four pending revisions: R053 Harbor, R055 Tackle Shop, R056 Technique Collection and R057 sharp-frame correction.** R057 is '+status+'. Do not import earlier baked frame images in place of its replacements. All 40 completed revisions and 13 superseded studies remain unchanged; Unity checkpoint is MFJ-SYNC-009. [Frame correction](Revisions/MFJ-R057/README.md).'),
 ('VisualContract.md','**MFJ-SYNC-009 is the latest Unity checkpoint.**','**MFJ-SYNC-009 is the latest Unity checkpoint.** R053/R055/R056 remain pending, with [R057 sharp-frame correction](Revisions/MFJ-R057/README.md) '+status+'. Use its verified standalone frame construction rather than enlarged full-screen review images. 40 completed revisions and 13 superseded studies remain unchanged. No Unity import.')]:
    p=root/filename;s=p.read_text(encoding='utf-8')
    if marker not in s:marker='**Four pending revisions:' if filename=='Design Changes.md' else marker
    a=s.index(marker);z=s.index('\n',a);s=s[:a]+summary+s[z:]
    if filename=='Design Changes.md' and '## MFJ-R057' not in s:s+='\n\n## MFJ-R057 — Sharp 4K frames\n\nReplace magnified full-screen raster framing with separately generated reusable nine-slice frame masters. Preserve existing native card fields and illustrations. See the verification report for completion; no Unity import. [Revision](Revisions/MFJ-R057/README.md).\n'
    p.write_text(s,encoding='utf-8')
before=read(folder.parent/'MFJ-R052/UnityBefore.json');expected={p['path']:p['sha256'] for p in before['files']}
actual={p.relative_to(project).as_posix():hashlib.sha256(p.read_bytes()).hexdigest() for p in (project/'Assets').rglob('*') if p.is_file()}
diff=[k for k in expected.keys()|actual.keys() if expected.get(k)!=actual.get(k)]
geometry=[f for s in design.get('screens',{}).values() for f in s.get('shells',[])]
captures=[{'file':name,'dimensions':struct.unpack('>II',(folder/name).read_bytes()[16:24]),'sha256':hashlib.sha256((folder/name).read_bytes()).hexdigest()} for name in ['Harbor4K.png','Shop4K.png','Collection4K.png']] if complete else []
write(folder/'Verification.json',{'revision':'MFJ-R057','status':status,'assemblyAndVisualReviewComplete':complete,'masterSources':sources,'frameComponents':len(geometry),'maximumDecorativeCornerScale':max((f.get('cornerScale',0) for f in geometry),default=0),'nativeFrameCanvas':[3840,2160],'sourceBordersTopRightBottomLeft':[300,500,560,500],'foregroundIdsPreserved':all(bool(s.get('foregroundIds')) for s in design.get('screens',{}).values()),'captures':captures,'visualReview':'All three lossless Penpot exports inspected. Corrected Harbor footer frame alignment. Existing text, Technique cards and equipment remain in place. No enlarged decorative corners.' if complete else 'Pending','scenicArtNative4K':False,'unityImported':False,'unityAssetsComparison':{'checkedFiles':len(actual),'differences':diff,'unchanged':not diff}})
print(json.dumps({'status':status,'unityAssetDifferences':diff}))
