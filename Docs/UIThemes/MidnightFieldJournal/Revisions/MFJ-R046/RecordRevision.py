"""Record the Penpot timing-bar proposal without changing Unity rules or imported checkpoints."""
import hashlib
import json
from pathlib import Path

root=Path('Docs/UIThemes/MidnightFieldJournal');folder=root/'Revisions/MFJ-R046'
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def write(p,x):p.write_text(json.dumps(x,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
digest=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
ledger=read(root/'DesignChangeLedger.json');index=read(root/'PendingUnityChanges.json');d=read(folder/'PenpotDesign.json')
assert ledger['nextRevisionId']=='MFJ-R046'
assert not any(c['id']=='MFJ-R046' for c in ledger['changes'])
assert d['stopPreservesExactPosition'] and d['refined']
for rows in [d['baseDistribution'],d['upgradeDistribution'],d['modifiedDistribution']]:assert abs(sum(r[1] for r in rows)-1)<1e-9
assert all(r[0]!='Miss' for r in d['baseDistribution'])
for r in d['records']:
    assert abs(sum(z['fraction'] for z in r['zones'])-1)<1e-9
    assert all(z['fraction']>0 for z in r['zones'])
for name in ['DesignReview.png','InContext4K.png','ReusableParts.png']:
    assert (folder/name).read_bytes().startswith(b'\x89PNG\r\n\x1a\n')
checkpoint=read(root/ledger['currentUnityCheckpoint'])
assert all(digest(Path(f['path']))==f['sha256'] for f in checkpoint['files']),'Unity checkpoint changed'
write(folder/'Assets.json',{'revision':'MFJ-R046','status':'ready_for_review','unityImported':False,
    'creation':'Native Penpot vectors/text/patterns; existing clean parchment and approved creature examples reused.',
    'newRasterAssets':[],'frameBorders':d['sourceFrameBorders'],'panelCanvas':[1160,340],'trackCanvas':[1048,62],
    'fonts':['Cinzel','Source Serif 4'],'separateElements':['frame','zone fills','clipped patterns','marker','motion trail','labels','emblems','stop button','zone wash','stop burst'],
    'widthsAndTimingsAreIllustrative':True,'gameplayImplemented':False,
    'reviewExports':[{'path':(folder/name).as_posix(),'sha256':digest(folder/name)} for name in ['DesignReview.png','InContext4K.png','ReusableParts.png']]})
write(folder/'Verification.json',{'status':'design_verified','visualInspection':['DesignReview.png','InContext4K.png','ReusableParts.png'],
    'baseHasThreePositiveZonesAndNoFailure':True,'allZoneWidthsSumToOne':True,'tinyZonesUseExternalLabels':True,
    'redundantRarityCues':['colour','pattern','emblem','result label'],'motionFrameCount':len(d['motionBoardIds']),
    'stopPreservesExactNormalizedPosition':True,'baseRarityReveals':['Common Cod','Uncommon Small Octopus','Rare Moonlit Ray'],
    'exampleBiomeDepth':'Coastal Waters, 12 m','eligibleExamples':True,
    'prototypeInteractionPlayedInViewer':False,'continuousTimingOrRandomSelectionImplemented':False,
    'unityCheckpointPreserved':ledger['currentUnityCheckpoint'],'unityFilesVerifiedUnchanged':len(checkpoint['files']),'unityImported':False})
entry={'id':'MFJ-R046','date':'2026-10-25',
    'request':'Design a themed reusable rarity timing bar in Penpot, including base rarities, optional Legendary/Miss zones, dynamic zone widths, moving/stopped marker and interaction mockup; no gameplay implementation.',
    'component':['Fishing rarity timing bar'],'status':'ready_for_review','supersedes':[],
    'penpotSources':d,'affectedPenpotInstances':[d['reviewBoardId'],d['contextBoardId'],d['partsBoardId'],*d['motionBoardIds'],*d['revealBoardIds'].values()],
    'changeKinds':['new visual proposal','editable timing instrument','rarity-zone variants','frame-based interaction prototype'],
    'before':'No rarity-timing instrument; current runtime encounter selection is unchanged.',
    'after':'1160x340 parchment timing instrument with separately resizable patterned zones, contrast-outlined marker, external rarity labels, Stop button and restrained rarity feedback. Base, upgrade, resize, Legendary and Miss studies plus in-context assembly and timed-frame prototype.',
    'unityAssetIds':['RarityTimingFrame (proposed)','RarityTimingMarker (proposed)','RarityZonePatterns (proposed)'],
    'unityPaths':['Assets/UIThemes/MidnightFieldJournal/Art/RarityTiming/ (proposed, not created)'],
    'dependentConsumers':['Future timing-phase HUD','Future normalized rarity-zone layout and marker view','Future runtime encounter eligibility/random selection owner'],
    'configurationChanges':'Visual proposal only. User supplied a new mechanic, but the existing GDD/runtime are not rewritten. Resolve gameplay integration separately; no hardcoded design odds, upgrade strengths, timing or fallback policy.',
    'exportRequirements':{'designResolution':[3840,2160],'panelCanvas':[1160,340],'trackCanvas':[1048,62],
        'dynamicFieldsSeparate':True,'nativeVectorAndText':True,'normalizedZoneBoundaries':True,'slices':True,'frameBorders':d['sourceFrameBorders']},
    'approvalEvidence':'User explicitly requested a Penpot visual design and interaction mockup, excluding gameplay implementation.',
    'exportEvidence':{'review':'Revisions/MFJ-R046/DesignReview.png','context':'Revisions/MFJ-R046/InContext4K.png','parts':'Revisions/MFJ-R046/ReusableParts.png','geometry':'Revisions/MFJ-R046/PenpotDesign.json'},
    'importEvidence':None,'verificationEvidence':{'report':'Revisions/MFJ-R046/Verification.json'},
    'remainingWork':['Visual review','Future gameplay design: rarity eligibility gaps, event/Apex scheduling and replacements, timing/upgrade balance','Unity implementation/import only when separately requested'],
    'unityImported':False,'integrationStatus':'not_imported','synchronizationCheckpoint':None}
assert all(k in entry for k in ledger['requiredEntryFields'])
ledger['changes'].append(entry);ledger['nextRevisionId']='MFJ-R047';write(root/'DesignChangeLedger.json',ledger)
index['generatedDate']='2026-10-25';index['pending'].append({'id':'MFJ-R046','status':'ready_for_review','component':['Fishing rarity timing bar'],'manifest':'Revisions/MFJ-R046/Assets.json','unityImported':False});write(root/'PendingUnityChanges.json',index)
summary='''

## MFJ-R046 — Rarity timing bar (Penpot only)

**Ready for review; not implemented or imported into Unity.** Parchment timing instrument with separate Common/Uncommon/Rare zones, optional tiny Legendary/Miss sections, external colour/pattern/emblem labels, independent outlined marker, Stop control and restrained stop feedback. Review states include zone resizing and Legendary/Miss results; a timed-frame prototype freezes the current marker position and leads to existing eligible creature examples. Widths and frame timing are illustrative. [Previews and handoff](Revisions/MFJ-R046/README.md).

This is the user's new visual/mechanic proposal, not an update to the current runtime's encounter rules. One pending design revision; 38 completed integrations and seven superseded drafts remain preserved. Latest Unity baseline remains MFJ-SYNC-007. Gameplay integration needs its own explicit task.
'''
for name in ['Design Changes.md','Pending Unity Changes.md','VisualContract.md']:
    p=root/name;t=p.read_text(encoding='utf-8')
    if name=='Design Changes.md':
        a=t.index('## Pending Unity changes');b=t.index('## Change history',a)
        t=t[:a]+'## Pending Unity changes\n\n**One pending proposal: MFJ-R046 — rarity timing bar, ready for visual review. No gameplay implementation requested.** 38 completed imports and seven superseded drafts preserved. [Pending index](PendingUnityChanges.json) · [Current Unity checkpoint](Sync007/UnityCheckpoint.json).\n\n'+t[b:]
    elif name=='Pending Unity Changes.md':
        a=t.index('## Explicit creature role choice')
        t='# Penpot changes awaiting Unity integration\n\n**Current: 1 pending visual proposal (MFJ-R046); 38 imported and verified; 7 superseded drafts.** Rarity timing remains a Penpot-only proposal. [Pending index](PendingUnityChanges.json) · [Full ledger](DesignChangeLedger.json).\n\nLatest baseline: [MFJ-SYNC-007](Sync007/UnityCheckpoint.json). Next revision: **MFJ-R047**. Preserve all current Unity behavior until gameplay integration is requested.\n\n'+t[a:]
    else:
        t=t.replace('0 pending,38 completed,7 superseded','1 pending visual proposal,38 completed,7 superseded',1)
        t=t.replace('[Current integration and verification](Sync006/README.md)','[Current integration and verification](Sync007/README.md)',1)
        t=t.replace('[Next import baseline](DisplayedStatColours/UnityCheckpoint.json)','[Next import baseline](Sync007/UnityCheckpoint.json)',1)
    p.write_text(t.rstrip()+summary,encoding='utf-8')
print('R046 recorded: 1 pending Penpot proposal; Unity checkpoint hashes unchanged.')
