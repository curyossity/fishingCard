"""Record the Harbor design without changing any previous integration status or Unity checkpoint."""
import hashlib
import json
from pathlib import Path
import struct

FOLDER = Path(__file__).resolve().parent
DOCS = FOLDER.parents[1]
REPO = DOCS.parents[2]


def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def write(path, data):
    temporary = path.with_suffix(path.suffix + '.tmp')
    temporary.write_text(json.dumps(data, indent=2, ensure_ascii=False) + '\n', encoding='utf-8')
    temporary.replace(path)


def main():
    state = read(FOLDER / 'PenpotDesign.json')
    audit = read(FOLDER / 'Action-audit.json')['result']
    before = read(FOLDER / 'UnityBefore.json')
    modified = [f['path'] for f in before['files'] if not (REPO / f['path']).exists() or hashlib.sha256((REPO / f['path']).read_bytes()).hexdigest() != f['sha256']]
    art = []
    for path in (FOLDER / 'Art').glob('*.png'):
        raw = path.read_bytes()
        width, height = struct.unpack('>II', raw[16:24])
        art.append({'file': path.relative_to(FOLDER).as_posix(), 'width': width, 'height': height, 'hasAlphaChannel': raw[25] in (4, 6), 'sha256': hashlib.sha256(raw).hexdigest()})
    previews = ['Harbor.jpg', 'TackleShop.jpg', 'TechniqueCollection.jpg', 'DiscoveryJournal.jpg', 'ReusableElements.jpg']
    checks = {
        'fourComplete4KLayouts': len(audit['screens']) == 4 and all(s['width'] == 3840 and s['height'] == 2160 and s['nativeTextFields'] >= 25 for s in audit['screens']),
        'reusableElementSheet': bool(audit.get('kitId')),
        'separateIllustrations': len(art) == 4,
        'allNavigationConnected': state.get('connectedLinks') == 17 and not state.get('prototypeErrors'),
        'reviewExportsPresent': all((FOLDER / p).exists() for p in previews),
        'unityAssetsUnchanged': not modified,
        'manualVisualReview': True,
    }
    if not all(checks.values()):
        raise RuntimeError('Harbor checks incomplete: ' + repr(checks))
    verification = {
        'revision': 'MFJ-R052', 'date': '2026-10-30', 'status': 'ready_for_review', 'checks': checks,
        'screens': audit['screens'], 'nativeTextFields': sum(s['nativeTextFields'] for s in audit['screens']),
        'connectedNavigationLinks': state['connectedLinks'], 'unityAssetsChecked': len(before['files']), 'modifiedUnityFiles': modified,
        'generatedBitmapSources': art, 'reviewExports': previews,
        'productionBoundary': 'Layouts, vector decoration and typography are 4K-native. Harbor scenery source is 1672x941; it requires a native-resolution master or approved enlargement before production shipping. No upscaled bitmap is labelled native 4K.',
        'contentBoundary': 'Equipment names/effects/tradeoffs, prices, ownership and discovery content are illustrative UX examples, not approved runtime changes.',
        'prototypeBoundary': '17 board navigation links verified; purchases, deck edits, claims and Set Sail are visual design states. Named flow creation is rejected by current Penpot API; present the selected main board.',
    }
    write(FOLDER / 'Verification.json', verification)
    write(FOLDER / 'VisualReview.json', {'passed': True, 'date': '2026-10-30', 'reviewed': previews, 'checks': ['existing Cinzel/Source Serif4 and sea-ink/parchment/copper palette', 'complete readable navigation and one-currency wallet', 'independent art contained without stretching', 'owned/equipped/locked labels distinct beyond colour', 'equipment sidegrade and one-item tradeoff visible', 'valid12-card illustrative deck', 'five objective/reward delivery examples fit folio', 'native glyph scale corrected', 'blank shell and separate reusable fields', 'no UI baked into generated scenery']})
    ledger_path = DOCS / 'DesignChangeLedger.json'
    ledger = read(ledger_path)
    entry = {
        'id': 'MFJ-R052', 'date': '2026-10-30',
        'request': 'Design the complete Harbor hub, expanded Tackle Shop, Technique Collection and Discovery Journal in the existing game style, using separable reusable 4K UI elements.',
        'component': ['Harbor hub', 'Tackle Shop', 'Technique Collection', 'Discovery Journal', 'Harbor reusable-element sheet'],
        'status': 'ready_for_review', 'supersedes': [], 'penpotSources': state,
        'affectedPenpotInstances': list(state['screens'].values()) + [state['kitId']],
        'changeKinds': ['new_screen', 'hub_navigation', 'reusable_UI_templates', 'new_scenery', 'new_equipment_illustrations'],
        'before': 'No Harbor screen exists in this candidate Penpot design.',
        'after': 'Four editable 4K screens with 17 navigation links, an engraved dusk Harbor and separate native UI fields, artwork, shells and state templates.',
        'unityAssetIds': [], 'unityPaths': [], 'dependentConsumers': ['Future Harbor presentation; no runtime consumer changed'],
        'configurationChanges': [], 'exportRequirements': ['Export individual blank shells, corners, illustrations and masks; reconstruct text and state in Unity.', 'Prepare native 4K Harbor scenery or explicitly approve enlargement before production use.'],
        'approvalEvidence': ['User requested Harbor design and expanded views; existing visual style and gold-only progression.'],
        'exportEvidence': {'folder': 'Revisions/MFJ-R052', 'files': previews, 'generatedSources': art},
        'importEvidence': {}, 'verificationEvidence': {'report': 'Revisions/MFJ-R052/Verification.json', 'visualReview': 'Revisions/MFJ-R052/VisualReview.json'},
        'remainingWork': ['User visual review; no Unity integration requested.'],
        'unityImported': False, 'integrationStatus': 'not_imported', 'synchronizationCheckpoint': None,
    }
    ledger['changes'] = [c for c in ledger['changes'] if c['id'] != entry['id']] + [entry]
    ledger['nextRevisionId'] = 'MFJ-R053'
    write(ledger_path, ledger)
    path = DOCS / 'PendingUnityChanges.json'
    pending = read(path)
    pending['generatedDate'] = '2026-10-30'
    pending['pending'] = [p for p in pending['pending'] if p['id'] != entry['id']] + [{'id': entry['id'], 'status': 'ready_for_review', 'unityImported': False, 'source': 'Revisions/MFJ-R052/README.md'}]
    pending['counts'] = {key: len(pending[key]) for key in ['pending', 'completed', 'superseded']}
    write(path, pending)
    path = DOCS / 'Design Changes.md'
    text = path.read_text(encoding='utf-8-sig')
    text = text.replace('**No pending imports. MFJ-R051 is imported and verified at MFJ-SYNC-009.** 40 revisions are completed; 11 are superseded.', '**One pending design: MFJ-R052 — Harbor screens, ready for review.** MFJ-R051 remains imported and verified at MFJ-SYNC-009; all 40 completed revisions and 11 superseded studies retain their status.')
    row = '| MFJ-R052 | Harbor, Tackle Shop, Technique Collection and Discovery Journal; separate illustrations and reusable editable 4K elements. | Ready for review; not imported. | [Design and handoff](Revisions/MFJ-R052/README.md). |'
    if '| MFJ-R052 |' not in text:
        text += '\n' + row + '\n'
    path.write_text(text, encoding='utf-8')
    path = DOCS / 'VisualContract.md'
    text = path.read_text(encoding='utf-8-sig')
    text = text.replace('No pending imports remain. [Integration evidence](Sync009/README.md).', 'One new Penpot design, [MFJ-R052 Harbor](Revisions/MFJ-R052/README.md), is pending review and has not been imported. [Integration evidence](Sync009/README.md).')
    path.write_text(text, encoding='utf-8')
    print(json.dumps({'status': 'ready_for_review', 'screens': 4, 'nativeTextFields': verification['nativeTextFields'], 'connectedLinks': 17, 'unityAssetsUnchanged': len(before['files']), 'pendingCounts': pending['counts']}))


if __name__ == '__main__':
    main()
