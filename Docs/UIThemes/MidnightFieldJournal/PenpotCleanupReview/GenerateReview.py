"""Generate a review proposal only. Never edits or deletes anything in Penpot."""
import json,re
from pathlib import Path
from collections import Counter

folder=Path(__file__).resolve().parent
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
write=lambda p,d:p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
inventory=read(folder/'LiveInventory.json')
links=read(folder/'PrototypeLinks.json')
overlay=folder/'OverlayLinks.json'
if overlay.exists():
    extra=read(overlay)
    replacements={(x['shapeId'],x['type']):x for x in extra['links']}
    links['links']=[replacements.get((x['shapeId'],x['type']),x) for x in links['links']]
write(folder/'PrototypeLinks.json',links)

items=[]
for page in inventory['pages']:
    for shape in page['roots']:
        name=shape['name']; sid=shape['id']
        decision='KEEP_CURRENT'; reason='Current component, current card design, or independent reusable source.'
        if 'Fantasy Card' in page['name']:
            decision='REMOVE_CANDIDATE'; reason='MCP connection demo; unrelated to the fishing game.'
        elif 'Gameplay Reference' in page['name']:
            decision='ARCHIVE_OPTIONAL'; reason='Legacy nautical UI, replaced by Midnight Field Journal; retain only if the old style is still wanted as reference.'
        elif '(Static)' in page['name']:
            decision='KEEP_REFERENCE'; reason='Original approved atmosphere/style mockup; reference only, not a runtime asset.'
        elif name.startswith('GAMEPLAY'):
            if name.startswith('GAMEPLAY G'):
                decision='KEEP_REFERENCE'; reason='Most recent general assembled gameplay reference before subsequent tooltip/tag-bar studies; not a complete latest-state gameplay screen.'
            else:
                decision='ARCHIVE_OPTIONAL'; reason='Older assembled comparison/prototype. Current component masters are kept separately; this screen is not the current Unity layout.'
        if page['name']=='Concept B — Reusable Assembly (4K)':
            if re.search(r'(?:MFJ-|/ )R(?:025|035|036|042|043|044|046|047|048|049|050|052|054)\b',name) or name.startswith('R057 comparison'):
                decision='REMOVE_CANDIDATE'; reason='Retired design, rejected alternative, or obsolete comparison; successor is preserved.'
            if name.startswith('GAMEPLAY H') or name.startswith(('GAMEPLAY I','GAMEPLAY J','GAMEPLAY K','GAMEPLAY L')):
                decision='REMOVE_CANDIDATE'; reason='Demonstrates the retired R042 tooltip or replaced R046 rarity-bar mechanic.'
            if name.startswith('PROTOTYPE / Hover modified Value'):
                decision='REMOVE_CANDIDATE'; reason='Prototype for retired R042 tooltip; R045 is current.'
            if name.startswith(('SOURCE ASSETS','COMPONENT REVIEW')):
                decision='KEEP_SHARED'; reason='Mixed source sheet contains reusable originals; extract required pieces before considering any whole-board deletion.'
            if name.startswith(('CATCH CHAIN CARD v2','CATCH CHAIN —','TECHNIQUES —')):
                decision='KEEP_CURRENT'; reason='Approved detailed chain/Technique appearance; the later fan refines placement rather than replacing this artwork.'
            if name.startswith('MFJ-R026'):
                decision='KEEP_CURRENT'; reason='Despite R026 naming, these boards were updated in place for accepted R027; frame and role artwork are reused.'
            if shape['type']=='rectangle':
                if name in ['HarborPanorama.png','TideglassReel.png','DriftwoodRod.png','BarbedHook.png']:
                    decision='REMOVE_CANDIDATE'; reason='Loose source from the replaced R052/old equipment study; current ornate scenery/engraved items are preserved.'
                elif sid.startswith('d76f6cbf'):
                    decision='CONSOLIDATE_SOURCE'; reason='Duplicate loose Technique artwork from the early Harbor study; keep a canonical current copy before removing this drop.'
                else:
                    decision='KEEP_SHARED'; reason='Loose current engraved equipment, Technique artwork/frame or R057 frame master; organize with reusable source assets.'
        items.append({**shape,'pageId':page['id'],'pageName':page['name'],'decision':decision,'reason':reason})

# A retained prototype must not lose its destination just because an old label matched a cleanup rule.
by_id={x['id']:x for x in items}
promotions=[]
while True:
    changed=False
    for link in links['links']:
        source=by_id.get(link['rootId']); target=by_id.get(link.get('destinationRootId'))
        if source and target and source['decision'] not in ['REMOVE_CANDIDATE','ARCHIVE_OPTIONAL'] and target['decision'] in ['REMOVE_CANDIDATE','ARCHIVE_OPTIONAL']:
            target['decision']='KEEP_SHARED'; target['reason']='Required destination of a retained prototype: '+source['name']+'.'
            promotions.append({'sourceId':source['id'],'destinationId':target['id']});changed=True
    if not changed:break
counts=Counter(x['decision'] for x in items)
pages=[]
for page in inventory['pages']:
    decision='KEEP_AND_ORGANIZE'
    if not page['roots']:decision='REMOVE_EMPTY_PAGE_CANDIDATE'
    elif 'Fantasy Card' in page['name']:decision='REMOVE_DEMO_PAGE_CANDIDATE'
    elif 'Gameplay Reference' in page['name']:decision='ARCHIVE_LEGACY_PAGE_OPTIONAL'
    elif '(Static)' in page['name']:decision='KEEP_ONE_STYLE_REFERENCE'
    pages.append({'id':page['id'],'name':page['name'],'topLevelItems':len(page['roots']),'proposal':decision})
review={'date':'2026-10-31','file':inventory['file'],'readOnly':True,'deletionAuthorized':False,
        'counts':dict(counts),'pages':pages,'items':items,'prototypeDependencyPromotions':promotions,
        'prototypeLinksChecked':len(links['links']),'hierarchyNodesChecked':sum(x['nodeCount'] for x in links['roots']),
        'truncatedHierarchyScans':[x['id'] for x in links['roots'] if x['truncated']],
        'localLibraryComponents':0,'note':'Review proposal. Recheck file revision and prototype targets immediately before any later approved cleanup.'}
write(folder/'Review.json',review)

lines=['# Penpot cleanup — review before deletion','',
       '**Read-only audit: 31 October 2026. Nothing has been moved, renamed or deleted.**',
       f"File: **{inventory['file']['name']}**, revision **{inventory['file']['revision']}**. Five pages, {len(items)} top-level items; {review['hierarchyNodesChecked']:,} hierarchy nodes and {review['prototypeLinksChecked']} prototype interactions checked. The local component library is empty: these are native boards/groups and image resources, not formal library components.",'',
       'The current Unity integration records identify accepted designs, but `verified/imported` alone does not mean a design is still current. R035/R036 and R046 were integrated historically and later replaced. Several accepted tooltip boards still say R026 because R027 updated them in place.','',
       '## Proposal at a glance','',
       '| Classification | Top-level items | Meaning |','| --- | ---: | --- |']
labels={'KEEP_CURRENT':'Keep: accepted current designs and component review/source boards.',
        'KEEP_SHARED':'Keep: shared or mixed reusable resources and required prototype destinations.',
        'KEEP_REFERENCE':'Keep as reference: original mood mockup and one general assembled gameplay view.',
        'CONSOLIDATE_SOURCE':'Duplicate source drops: retain a canonical copy, then remove the duplicate.',
        'ARCHIVE_OPTIONAL':'Unused legacy/comparison screens: proposed archive or removal after review.',
        'REMOVE_CANDIDATE':'Unused/replaced designs proposed for removal; not approved for deletion yet.'}
for key in labels:lines.append(f'| {key} | {counts[key]} | {labels[key]} |')
lines+=['','The empty Page 1 is an additional page-removal candidate and is not counted as a top-level item. An entire page is not a deletion candidate merely because some boards on it are old.','',
        '## What we currently use — keep','',
        '| Area | Keep |','| --- | --- |',
        '| Main creature cards | R002 main card/master plus all 18 approved creature illustrations and editable card examples, R003–R019. |',
        '| Events | R020–R024: Reef Snag, Silt Burst, Gentle Upwelling, Loose Knot, Fresh Shell Fragments; event master, illustration sources and rosters. |',
        '| Catch Chain | Detailed v2 chain cards, brass connectors and their source artwork. |',
        '| Techniques | All eight approved Technique illustrations/frames; engraved initial four plus R031–R034; R030 editable/projected fan. |',
        '| Catch Chain tooltip | R027 accepted no-pointer design, including the boards/master still named R026 and its working prototype. |',
        '| Modified-stat tooltip | R045 compact flat-icon frame, blank 9-slice master and arrow-only scroll states/prototypes. |',
        '| Actions / Line Load | R037–R038 parchment/card-style controls and reusable sources. |',
        '| Opening / Depth / Hand backing | R039 boat, R040 depth guide, R041 backing sheet, including independent artwork. |',
        '| Timing bar | R051 Lantern Focus with anchor cursor and rising precision highlights. |',
        '| Harbor | R053 board with the R057 sharp 4K framing update. |',
        '| Tackle Shop | R055 board and engraved rod/reel/hook artwork with R057 sharp 4K framing. |',
        '| Technique Collection | R056 and its actual in-run card artwork with R057 sharp 4K framing. |',
        '| Journal | R058 ornate Journal and separate Journal sources. |',
        '| Shared masters | R057 DarkFrameMaster/PaperFrameMaster; current equipment/Technique image resources; mixed original source sheets until their used parts are separated. |','',
        '## What is no longer used — proposed cleanup groups','',
        '| Group | Why it is no longer current |','| --- | --- |',
        '| Page 1 | Empty. |',
        '| Fantasy Card — MCP Verification | Connection-test Moonlit Relic demo, unrelated to the game. |',
        '| Gameplay Reference — Editable | Entire legacy nautical-style UI. Optional archive if this original style should remain available as reference. |',
        '| Extra GAMEPLAY A/B/C/D/E/F comparisons | Earlier assembled reference versions. Preserve component masters separately; retain G as one general reference. |',
        '| SUPERSEDED / MFJ-R025 tooltips | Replaced by the accepted R027 engraved tooltip. |',
        '| R035 Line Load and R036 marine hardware buttons | Replaced by R037–R038 parchment controls. |',
        '| R042–R044 modifier-tooltip studies, overlays and prototypes | Replaced by R045. R044 reused boards whose labels still say R043. |',
        '| R046 rarity-bar review, animation states, parts and GAMEPLAY I/J/K/L | Rarity mechanic replaced by the tag bar. Existing Unity animation behavior does not require retaining every Penpot storyboard frame. |',
        '| R047 / R048 / R049 tag-bar studies | Rejected/unselected alternatives and mid-run comparisons. |',
        '| R050 Lantern Focus refinement | Replaced by R051 anchor cursor/rising precision version. |',
        '| R052 Harbor/Shop/Collection/Journal/library | First hub suite, replaced by later ornate screens. |',
        '| R054 Tackle Shop and old loose equipment/panorama drops | Replaced by R055 engraved items and current ornate scenery. |',
        '| R057 comparison / original raster framing — Harbor, Shop, Collection | Blurry pre-correction copies, not the sharp 4K current screens. |','',
        '## Proposed organization — no changes applied','',
        '1. **Current Screens:** Harbor, Tackle Shop, Technique Collection, Journal, and one clearly labelled gameplay reference.',
        '2. **Current Cards:** creature/event/Technique collections and their editable templates.',
        '3. **Current Components:** Catch Chain, controls, timing bar, tooltips, depth guide and backing sheet.',
        '4. **Source Art & Masters:** independent artwork and reusable frames, separate from screen review boards.',
        '5. **Archive:** original static mockup and any old comparisons you elect to retain; remove the approved obsolete groups after review.','',
        'There is no single gameplay board that combines every latest component: G predates the final stat tooltip and tag bar. Before deleting all comparison screens, assemble one up-to-date gameplay reference from the kept parts. This is a proposed follow-up, not work performed by this audit.','',
        '## Complete item-by-item review','',
        'The table below uses actual live names and IDs. IDs distinguish duplicates with the same filename. Full machine-readable classification and reasons are in [Review.json](Review.json); the untouched live inventory is in [LiveInventory.json](LiveInventory.json). Prototype connections are in [PrototypeLinks.json](PrototypeLinks.json).','']
for page in pages:
    lines+=['### '+page['name'],'',f"Page ID: `{page['id']}` · **{page['proposal']}**",'']
    subset=[x for x in items if x['pageId']==page['id']]
    if not subset:lines+=['No shapes.',''];continue
    lines+=['| Actual Penpot item | Recommendation | ID |','| --- | --- | --- |']
    for item in subset:lines.append('| '+item['name'].replace('|','\\|')+' | '+item['decision']+' | `'+item['id']+'` |')
    lines.append('')
lines+=['## Before any deletion','',
        'No deletion is authorized by this list. After the user reviews it, export a backup of the file, recheck its revision/targets, and use the exact reviewed IDs. Do not delete Unity assets, revision exports or source/license records as part of Penpot cleanup. Keep required prototype destinations with their retained source boards.','']
(folder/'README.md').write_text('\n'.join(lines),encoding='utf-8')
print(json.dumps({'pages':len(pages),'items':len(items),'counts':dict(counts),'dependencyPromotions':promotions},ensure_ascii=False))
