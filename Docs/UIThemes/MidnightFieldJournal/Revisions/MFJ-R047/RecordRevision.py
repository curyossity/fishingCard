"""Record the Penpot-only increment without altering any Unity checkpoint."""
import hashlib
import json
import re
from pathlib import Path

folder = Path(__file__).resolve().parent
root = folder.parents[1]
repo = root.parents[2]

def read(path):
    return json.loads(path.read_text(encoding='utf-8'))

def write(path, value):
    temporary = path.with_suffix(path.suffix + '.tmp')
    temporary.write_text(json.dumps(value, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    temporary.replace(path)

state = read(folder / 'PenpotDesign.json')
bars = list(state['bars'].values())
complete = len(bars) == 6 and all(b.get('complete') for b in bars)
exports = ['DesignReview.png', 'FieldJournal.png', 'NightSounder.png', 'NaturalistsLedger.png']
review_path = folder / 'VisualReview.json'
visual_review = read(review_path) if review_path.exists() else {'passed': False}
ready = complete and all((folder / name).exists() for name in exports) and visual_review['passed']
status = 'ready_for_review' if ready else 'in_progress'
before = read(folder / 'UnityBefore.json')
modified = [f['path'] for f in before['files'] if not (repo / f['path']).exists() or hashlib.sha256((repo / f['path']).read_bytes()).hexdigest() != f['sha256']]
checks = {'sixCompleteBars': complete, 'eightZonesPerBar': complete and all(len(b['zones']) == 8 for b in bars), 'widthsSumToOne': complete and all(abs(sum(z['fraction'] for z in b['zones']) - 1) < 1e-8 for b in bars), 'initialEqual': complete and all(all(z['fraction'] == .125 for z in b['zones']) for b in bars if b['state'] == 'initial'), 'finalMinimumFivePercent': complete and all(z['fraction'] >= .05 for b in bars for z in b['zones']), 'proportionalPrecision': complete and all(abs(z['precisionWidth'] / z['width'] - .18) < 1e-8 for b in bars for z in b['zones']), 'exportsExist': ready, 'unityFilesUnchanged': not modified}
audit = read(folder / 'Action-audit.json')
checks['nativeGeometry'] = len(audit) == 6 and all(b['markerCount'] == 1 and len(b['zones']) == 8 and all(z['precisionCentered'] and abs(z['width']-z['expected']) < .01 for z in b['zones']) for b in audit)
checks['visualReviewPassed'] = visual_review['passed']
write(folder / 'Verification.json', {'revision': 'MFJ-R047', 'verificationDate': '2026-10-28', 'checks': checks, 'unityCheckpointUnchanged': before['checkpoint'], 'unityFileCount': len(before['files']), 'modifiedUnityFiles': modified, 'visualReview': visual_review})
ledger = read(root / 'DesignChangeLedger.json')
entry = {'id': 'MFJ-R047', 'date': '2026-10-27', 'request': 'Three static Dynamic Fishing Tag Bar variations, each with one initial and one mid-run state; Penpot only.', 'component': ['Dynamic Fishing Tag Bar studies'], 'status': status, 'supersedes': [], 'penpotSources': state, 'affectedPenpotInstances': [state['reviewId']] + [b['id'] for b in bars], 'changeKinds': ['new_design_alternatives', 'native_editable_ui'], 'before': 'Existing Unity rarity bar remains active and unchanged.', 'after': 'Six static bars on Tag Bar — Three UI Studies.' if complete else 'Dedicated review page created; bars awaiting plugin reconnection.', 'unityAssetIds': [], 'unityPaths': [], 'dependentConsumers': [], 'configurationChanges': [], 'exportRequirements': ['Design review PNGs only; no Unity import authorized.'], 'approvalEvidence': ['User confirmed 12.5% as recovery baseline before normalization; normalized zones may exceed it.'], 'exportEvidence': {'folder': 'Revisions/MFJ-R047', 'files': exports if ready else []}, 'importEvidence': {}, 'verificationEvidence': {'report': 'Revisions/MFJ-R047/Verification.json'}, 'remainingWork': ['Choose a variation before any gameplay implementation.'] if ready else ['Reconnect plugin, create six bars, export and visually inspect.'], 'unityImported': False, 'integrationStatus': 'not_imported', 'synchronizationCheckpoint': None}
ledger['changes'] = [c for c in ledger['changes'] if c['id'] != entry['id']] + [entry]
entry['after'] = 'Six static editable bars on MFJ-R047 / Dynamic Fishing Tag Bar / three static variations, on Concept B — Reusable Assembly (4K).'
entry['penpotSources']['pageName'] = 'Concept B — Reusable Assembly (4K)'
entry['verificationDate'] = '2026-10-28'
if complete and not ready:
    entry['remainingWork'] = ['Finish rendered visual checks: Penpot PNG/SVG exports are timing out. All six native bars are created and their geometry is verified.', 'Choose a variation before any gameplay implementation.']
ledger['nextRevisionId'] = 'MFJ-R048'
write(root / 'DesignChangeLedger.json', ledger)
index = read(root / 'PendingUnityChanges.json')
index['generatedDate'] = '2026-10-28'
index['pending'] = [{'id': c['id'], 'status': c['status'], 'unityImported': False, 'source': 'Revisions/'+c['id']+'/README.md'} for c in ledger['changes'] if not c['unityImported'] and c['status'] != 'superseded']
index['counts'] = {'pending': len(index['pending']), 'completed': len(index['completed']), 'superseded': len(index['superseded'])}
write(root / 'PendingUnityChanges.json', index)
pending_md = root / 'Pending Unity Changes.md'
content = pending_md.read_text(encoding='utf-8')
content = re.sub(r'\*\*Current:.*?\n', '**Current: 1 pending design study; 39 imported and verified; 7 superseded drafts.** R047 is '+status+'. [Pending index](PendingUnityChanges.json) · [Full ledger](DesignChangeLedger.json).\n', content, count=1)
content = re.sub(r'Latest baseline:.*?Next revision:.*?\n', 'Latest baseline: [MFJ-SYNC-008.5](RarityBarEntrance/UnityCheckpoint.json). Next revision: **MFJ-R048**.\n', content, count=1)
if '## Dynamic Fishing Tag Bar — MFJ-R047' not in content:
    content += '\n## Dynamic Fishing Tag Bar — MFJ-R047\n\nThree alternatives with initial and mid-run static states. Penpot-only exploration; the existing Unity rarity bar remains active. [Design and evidence](Revisions/MFJ-R047/README.md). No variation has been selected for implementation.\n'
pending_md.write_text(content, encoding='utf-8')
history = root / 'Design Changes.md'
content = history.read_text(encoding='utf-8')
content = re.sub(r'(## Pending Unity changes\s*\n)\*\*(?:No pending changes:|One pending design study:).*?\n', r'\1**One pending design study: MFJ-R047** ('+status+'). All 39 previously integrated revisions remain verified; seven drafts are superseded. [Pending index](PendingUnityChanges.json).\n', content, count=1)
if '| MFJ-R047 |' not in content:
    row = '| MFJ-R047 | Three Dynamic Fishing Tag Bar alternatives, each initial + mid-run, with separate native zones/precision/icons/marker/text. | '+status+'; design only. | Not imported; [design evidence](Revisions/MFJ-R047/README.md). |\n'
    matches = list(re.finditer(r'^\| MFJ-R046 \|.*\n', content, re.MULTILINE))
    if matches:
        position = matches[-1].end()
        content = content[:position] + row + content[position:]
    else:
        content += '\n## Dynamic Fishing Tag Bar — MFJ-R047\n\n'+row
else:
    content = re.sub(r'\| MFJ-R047 \|.*', '| MFJ-R047 | Three Dynamic Fishing Tag Bar alternatives, each initial + mid-run, with separate native zones/precision/icons/marker/text. | '+status+'; design only. | Not imported; [design evidence](Revisions/MFJ-R047/README.md). |', content)
history.write_text(content, encoding='utf-8')
contract = root / 'VisualContract.md'
content = contract.read_text(encoding='utf-8')
content = re.sub(r'\*\*MFJ-SYNC-007 is the latest imported checkpoint\.\*\*.*?\n', '**MFJ-SYNC-008.5 is the latest Unity checkpoint.** [Current baseline](RarityBarEntrance/UnityCheckpoint.json). One pending Penpot-only study, MFJ-R047, offers three tag-bar alternatives; it does not replace the live rarity bar. 39 revisions remain imported and verified; seven drafts are superseded. [Design log](Design%20Changes.md). Earlier sections describe historical geometry where superseded.\n', content, count=1)
contract.write_text(content, encoding='utf-8')
print(json.dumps({'status': status, 'counts': index['counts'], 'checks': checks}))
