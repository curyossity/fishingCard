"""Record R051 only after live Unity verification; retain immutable earlier checkpoints."""
import hashlib
import json
import pathlib
import subprocess

ROOT = pathlib.Path(__file__).resolve().parents[4]
DOCS = ROOT / 'Docs/UIThemes/MidnightFieldJournal'
HERE = pathlib.Path(__file__).resolve().parent


def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def write(path, data):
    temporary = path.with_suffix(path.suffix + '.tmp')
    temporary.write_text(json.dumps(data, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    temporary.replace(path)


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def polish(text):
    for before, after in {
        'October2026': 'October 2026', 'approved29': 'approved 29', 'verified29': 'verified 29',
        'occupies12.5': 'occupies 12.5', 'weight1/2/4/8': 'weight 1/2/4/8',
        'span18%': 'span 18%', 'by0.85': 'by 0.85', 'recover0.01': 'recover 0.01',
        'their0.125': 'their 0.125', 'fill100%': 'fill 100%', 'final5%': 'final 5%',
        'The12.5%': 'The 12.5%', 'over0.3': 'over 0.3', 'amplitude1': 'amplitude 1',
        'period0.74': 'period 0.74', 'for0.65': 'for 0.65', 'and4K': 'and 4K',
        '**40 revisions': '** 40 revisions', ';11 are': '; 11 are',
        'spans.40 revisions': 'spans. 40 revisions',
        'verified.40 revisions': 'verified. 40 revisions', 'the1600': 'the 1600',
        'its4K': 'its 4K', 'a5%': 'a 5%',
    }.items():
        text = text.replace(before, after)
    return text


def update_documents():
    timing = """The journal comparison scene uses the approved R051 Lantern Focus tag bar before creature reveals, replacing the earlier rarity ceiling bar. Descend commits the selected Catch/Bait and advances normally; the next creature remains hidden until Stop, its feedback hold and the upward exit complete. Events reveal directly. Catch/Bait has no default. Technique replacements of already revealed creatures do not start another timing phase.

All eight tags remain visible in canonical order: Fish, Predator, Schooling, Small, Heavy, Anchored, Armored, Elusive. Initially each occupies12.5%. A normal hit samples uniformly from distinct biome/depth-eligible species with that tag, without rarity weights, encounter-weight modifiers or immediate-repeat filtering. On precision, each Common/Uncommon/Rare/Legendary species receives relative weight1/2/4/8. If the stopped tag has no eligible creatures, use the full eligible species pool with the same normal/precision policy. Category/event pacing remains separate. Queued selections, trails, authored chains and scheduled Apex encounters keep their promised identity; their tag bar cannot replace that promise.

Each tag has a centred precision span18% of its width. After one accepted Stop, multiply only the selected tag's raw weight by0.85; other tags recover0.01 toward their0.125 raw baseline. Normalize the resulting weights to fill100%, with a final5% minimum. The12.5% baseline is applied before normalization; displayed zones may exceed it. Repeated hits at the floor do not accumulate invisible fatigue. Run balance survives subsequent creatures and biome transitions; ending/restarting the run restores equal spans. Widths resize smoothly over0.3 seconds without changing the captured hit.

Preserve the previous one-second entrance from below with mirrored elastic settling and one-second upward elastic-in exit (amplitude1, period0.74). Marker motion and Stop remain locked until entrance finishes. Stop holds for0.65 seconds; precision hits reuse the existing gold/ivory particles and marker pulse. Reveal reactions and concealment assignment wait until the exit finishes. Descend and Technique play stay locked during timing; Surface can cancel it. TagTiming.asset authors speed, hold, precision fraction, fatigue/recovery and rarity multipliers. No new creature, Technique or upgrade content is introduced. Legacy scenes without timing profiles retain direct reveals; the old rarity profile/prefab remain available as backup.
"""
    path = ROOT / 'Docs/GDD_IMPLEMENTATION_BRIEF.md'
    text = path.read_text(encoding='utf-8-sig')
    start = text.index('## Approved creature timing flow')
    end = text.index('This brief summarizes', start)
    text = text[:start] + '## Approved creature timing flow (29 October2026)\n\n' + timing + '\n' + text[end:]
    path.write_text(polish(text), encoding='utf-8')
    path = ROOT / 'Docs/Fishing_Card_Roguelike_GDD_with_image/Fishing_Card_Roguelike_GDD.md'
    text = path.read_text(encoding='utf-8-sig')
    start = text.index('## Rarity timing rules (') if '## Rarity timing rules (' in text else text.index('## Tag timing rules (')
    end = text.index('Caught creatures do not immediately', start)
    text = text[:start] + '## Tag timing rules (approved29 October2026)\n\n' + timing + '\n' + text[end:]
    start = text.index('3.  Select the next encounter category')
    end = text.index('4.  Resolve automatic reveal reactions', start)
    text = text[:start] + "3.  Select the next encounter category based on depth, biome, current catches\n    and active modifiers. Before a creature is revealed, show the tag timing\n    bar. Stop selects a tag; its centre precision area boosts higher-rarity\n    species odds. Draw an eligible creature with that tag, falling back to\n    all eligible species if unavailable. Reveal after stopped feedback and\n    the instrument's upward exit. Events reveal directly. The journal\n    comparison scene enables timing; legacy scenes retain direct reveals.\n\n" + text[end:]
    path.write_text(polish(text), encoding='utf-8')
    path = ROOT / 'Docs/Architecture.md'
    text = path.read_text(encoding='utf-8-sig')
    start = text.index('### Creature encounter rarity timing') if '### Creature encounter rarity timing' in text else text.index('### Creature encounter tag timing')
    end = text.index('### Midnight Field Journal candidate-theme resources', start)
    section = """### Creature encounter tag timing

`EncounterTiming` owns the shared distinct-species snapshot, ping-pong clock, occurrence tokens, Stop and feedback hold. `EncounterTagTiming` owns per-run raw tag balance and per-occurrence normalized zones, exact tag/precision outcome, fallback and normal/precision species sampling. `TagTimingProfile` authors configuration only. `EncounterRuntime` reserves a creature without publishing identity or dispatching reveal/concealment/history reactions, then commits through the existing reveal path after Stop. Category pacing remains separate; all species candidates are biome/depth filtered before repetition filtering. Fixed School Call/trail/chain/Apex promises retain identity. Events bypass timing. `Reset(true)` preserves run balance across biomes; ordinary run cleanup calls `ResetRun`. `EncounterRarityTiming` remains a legacy implementation of the same lifecycle, with no duplicate controller clock.

`FishingRunController.Timing` is the sole active instrument boundary. The controller owns Stop input, Descend/Technique locks and delayed reveal. `EncounterTimingView` supplies optional occurrence-bound entry/exit gates; absent/disabled views do not strand gameplay. `FishingRunView` forwards this generic state through a migrated serialized reference. `JournalEncounterTimingView` owns the shared one-second elastic entry/exit, focus, stopped pulse and particles. `JournalRarityExitMotion` remains the common curve. `JournalTagTimingView` renders R051: separate sliced paper/frame, eight native glyph/text spans, proportional tapered precision meshes, overlay anchor and Stop. It smoothly lerps widths to the next balance while keeping the stopped cursor within its captured span. Narrow zones retain glyphs and show their name in the phase field when traversed; no tag-specific colours or current-zone fill are added.

`JournalTagTimingBuilder` owns isolated profile/prefab creation and targeted canvas replacement. Normal candidate canvas/scene regeneration binds tag timing. The original scene/build entries and rarity assets remain intact. Shared concealment, encounter promises, chain effects and card renderers are unchanged. [MFJ-SYNC-009 rules, live verification and4K captures](UIThemes/MidnightFieldJournal/Sync009/README.md).

"""
    text = text[:start] + section + text[end:]
    text = text.replace('`JournalRarityTimingView` creates the emitter lazily', '`JournalEncounterTimingView` creates the emitter lazily')
    text = text.replace('for a stop on the highest displayed non-Miss rarity.', 'for precision tag hits, or the highest displayed non-Miss rarity in legacy mode.')
    path.write_text(polish(text), encoding='utf-8')
    path = DOCS / 'VisualContract.md'
    text = path.read_text(encoding='utf-8-sig')
    start = text.index('**MFJ-SYNC-008.5') if '**MFJ-SYNC-008.5' in text else text.index('**MFJ-SYNC-009')
    end = text.index('\n', start)
    text = text[:start] + '**MFJ-SYNC-009 is the latest Unity checkpoint.** [Current baseline](Sync009/UnityCheckpoint.json). R051 Lantern Focus is integrated and verified: light parchment, overlay anchor cursor, eight proportional rising precision lights and dynamically resizing tag spans.40 revisions are imported and verified;11 are superseded. No pending imports remain. [Integration evidence](Sync009/README.md). Earlier sections retain historical geometry.' + text[end:]
    if '## R051 tag timing integration' not in text:
        text += '\n## R051 tag timing integration\n\nThe candidate now uses the1600×400 Lantern Focus instrument centred in its4K gameplay viewport. Paper and engraved frame are independent sliced exports; marker, monochrome tag glyphs, native labels, precision gradients, Stop and feedback are separate uGUI elements. No assembled mockup is sliced. Eight zones normalize at runtime with a5% minimum. Small spans retain their glyph; a full tag name appears in the phase field when needed. No active-zone wash is shown. Precision hits reuse the gold/ivory particle celebration and exact old entrance/exit animations. See [Sync009](Sync009/README.md).\n'
    path.write_text(polish(text), encoding='utf-8')
    path = DOCS / 'Design Changes.md'
    text = path.read_text(encoding='utf-8-sig')
    text = text.replace('**One pending selected design: MFJ-R051** (ready_for_review). R050 is superseded; all39 prior integrations remain verified. [Pending index](PendingUnityChanges.json).', '**No pending imports. MFJ-R051 is imported and verified at MFJ-SYNC-009.**40 revisions are completed;11 are superseded. [Integration evidence](Sync009/README.md) · [Pending index](PendingUnityChanges.json).')
    lines = text.splitlines()
    for i, line in enumerate(lines):
        if line.startswith('| MFJ-R051 |'):
            columns = line.split('|')
            columns[3] = ' Imported and verified at MFJ-SYNC-009. '
            columns[4] = ' R051 replaces the candidate rarity bar; separate native dynamic elements and independent blank exports. [Evidence](Sync009/README.md). '
            lines[i] = '|'.join(columns)
    path.write_text(polish('\n'.join(lines)+'\n'), encoding='utf-8')
    path = DOCS / 'Revisions/MFJ-R051/README.md'
    text = path.read_text(encoding='utf-8-sig')
    if '**Unity integration:' not in text:
        text = text.replace('\n\n', '\n\n**Unity integration: imported and verified29 October2026 at [MFJ-SYNC-009](../../Sync009/README.md). No pending work remains for this revision.** The original design-only notes below are retained as historical evidence.\n\n', 1)
    path.write_text(polish(text), encoding='utf-8')


def main():
    report = read(HERE / 'PlayModeVerification.json')
    if not report['status'].startswith('Passed:'):
        raise RuntimeError('Live Play Mode verification must pass first')
    protected = ['Assets/Scenes/SampleScene.unity', 'ProjectSettings/EditorBuildSettings.asset']
    for name in protected:
        original = subprocess.check_output(['git', 'show', 'HEAD:' + name], cwd=ROOT)
        if original.replace(b'\r\n', b'\n') != (ROOT / name).read_bytes().replace(b'\r\n', b'\n'):
            raise RuntimeError('Protected legacy file changed: ' + name)
    before = read(DOCS / 'RarityBarEntrance/UnityCheckpoint.json')
    changed = []
    missing = []
    files = {}
    for entry in before['files']:
        path = ROOT / entry['path']
        if not path.exists():
            missing.append(entry['path'])
            continue
        sha = digest(path)
        files[entry['path']] = sha
        if sha != entry['sha256']:
            changed.append(entry['path'])
    # Shared data, illustrations, fonts and every other prefab must remain untouched.
    allowed = {
        'Assets/Scripts/EncounterRarityTiming.cs', 'Assets/Scripts/EncounterRuntime.RarityTiming.cs',
        'Assets/Scripts/EncounterRuntime.cs', 'Assets/Scripts/FishingRunController.cs', 'Assets/Scripts/FishingRunView.cs',
        'Assets/UIThemes/MidnightFieldJournal/Editor/JournalComparisonSceneBuilder.cs',
        'Assets/UIThemes/MidnightFieldJournal/Editor/JournalRarityTimingBuilder.cs',
        'Assets/UIThemes/MidnightFieldJournal/Editor/MidnightFieldJournalHudBuilder.cs',
        'Assets/UIThemes/MidnightFieldJournal/Runtime/JournalRarityTimingView.cs',
        'Assets/UIThemes/MidnightFieldJournal/Prefabs/MidnightFieldJournalCanvas.prefab',
        'Assets/UIThemes/MidnightFieldJournal/Scenes/MidnightFieldJournalComparison.unity',
    }
    renamed = {'Assets/Scripts/EncounterRarityTimingView.cs', 'Assets/Scripts/EncounterRarityTimingView.cs.meta'}
    unexpected = set(changed) - allowed
    if unexpected or set(missing) - renamed:
        raise RuntimeError('Unexpected previous-baseline changes: ' + repr((unexpected, missing)))
    for name in ['EncounterTiming', 'EncounterTimingView', 'EncounterTagTiming', 'TagTimingProfile']:
        for suffix in ['.cs', '.cs.meta']:
            path = ROOT / ('Assets/Scripts/' + name + suffix)
            files[path.relative_to(ROOT).as_posix()] = digest(path)
    theme = ROOT / 'Assets/UIThemes/MidnightFieldJournal'
    for path in theme.rglob('*'):
        if path.is_file():
            files[path.relative_to(ROOT).as_posix()] = digest(path)
    verification = {
        'status': 'verified', 'date': '2026-10-29', 'revision': 'MFJ-R051',
        'rulesAssertions': 2955, 'legacyRulesAssertions': 75, 'elasticExitAssertions': 6,
        'elasticEntranceAssertions': 8, 'particleAssertions': 14, 'playMode': report,
        'visualChecks': ['4K and 1080p moving instrument', 'precision stop feedback', '5% minimum span and proportional lights', 'event bypass'],
        'protectedFiles': [{'path': name, 'sha256': digest(ROOT / name)} for name in protected],
        'previousBaselineChangedFiles': changed, 'renamedPreviousPaths': missing,
        'unchangedPreviousBaselineFiles': len(before['files']) - len(changed) - len(missing),
        'scope': 'Targeted candidate canvas timing replacement; no card data, illustrations, fonts, other templates or legacy scene/build changes.',
    }
    write(HERE / 'Verification.json', verification)
    write(HERE / 'UnityCheckpoint.json', {
        'checkpoint': 'MFJ-SYNC-009', 'date': '2026-10-29',
        'previousCheckpoint': '../RarityBarEntrance/UnityCheckpoint.json', 'revisions': ['MFJ-R051'],
        'kind': 'R051 Lantern Focus tag selection, precision rarity boost, fatigue and shared elastic timing animations',
        'verification': 'Verification.json', 'files': [{'path': p, 'sha256': h} for p, h in sorted(files.items())],
    })
    write(HERE / 'SourceManifest.json', {
        'revision': 'MFJ-R051', 'fileId': '9e056cfe-f83f-81f8-8008-cc523ff45211',
        'pageId': 'bae680cd-9225-8018-8008-cddd6a6f3825',
        'approvedBarId': '0ee2d4ff-05c9-80cd-8008-dc54a3d51c76',
        'frameExportBoardId': '0ee2d4ff-05c9-80cd-8008-dd5a7a1e59e6',
        'paperExportShapeId': '0ee2d4ff-05c9-80cd-8008-dc54a3d51c77',
        'exports': [{'path': n, 'width': 1600, 'height': 400, 'sha256': digest(HERE / n)} for n in ['TagBarFrame.png', 'TagBarPaper.png']],
        'construction': 'Independent blank engraved frame and independent clean paper exported from native R051 shapes. No assembled screenshot sliced. Tags, text, precision lights, anchor, Stop and feedback are live native Unity elements.',
        'profile': 'Assets/UIThemes/MidnightFieldJournal/Profiles/TagTiming.asset',
        'prefab': 'Assets/UIThemes/MidnightFieldJournal/Prefabs/TagTiming.prefab',
    })
    ledger_path = DOCS / 'DesignChangeLedger.json'
    ledger = read(ledger_path)
    entry = next(c for c in ledger['changes'] if c['id'] == 'MFJ-R051')
    entry.update(status='verified', unityImported=True, integrationStatus='verified', synchronizationCheckpoint='MFJ-SYNC-009', remainingWork=[])
    entry['unityPaths'] = ['Assets/UIThemes/MidnightFieldJournal/' + p for p in ['Art/TagTiming/', 'Prefabs/TagTiming.prefab', 'Profiles/TagTiming.asset', 'Prefabs/MidnightFieldJournalCanvas.prefab', 'Scenes/MidnightFieldJournalComparison.unity']]
    entry['importEvidence'] = {'checkpoint': 'Sync009/UnityCheckpoint.json', 'sourceManifest': 'Sync009/SourceManifest.json', 'date': '2026-10-29'}
    entry['verificationEvidence']['unityReport'] = 'Sync009/Verification.json'
    entry['exportEvidence']['unitySourceManifest'] = 'Sync009/SourceManifest.json'
    entry['dependentConsumers'] = ['FishingRunController', 'FishingRunView', 'MidnightFieldJournalCanvas', 'JournalComparisonSceneBuilder']
    entry['exportRequirements'] = ['Satisfied by independent R051 frame/paper exports; superseded studies were not imported.']
    entry['configurationChanges'] = ['Candidate uses TagTiming.asset; rarity profile retained as legacy backup.', 'Uniform normal species odds; precision weights1/2/4/8; unavailable tags fall back to all eligible species.']
    ledger['currentUnityCheckpoint'] = 'Sync009/UnityCheckpoint.json'
    ledger['synchronizationCheckpoints'] = [c for c in ledger['synchronizationCheckpoints'] if c['id'] != 'MFJ-SYNC-009'] + [{
        'id': 'MFJ-SYNC-009', 'date': '2026-10-29', 'revisions': ['MFJ-R051'], 'status': 'verified',
        'manifest': 'Sync009/UnityCheckpoint.json', 'verification': 'Sync009/Verification.json',
    }]
    write(ledger_path, ledger)
    pending_path = DOCS / 'PendingUnityChanges.json'
    pending = read(pending_path)
    pending['generatedDate'] = '2026-10-29'
    pending['currentUnityCheckpoint'] = 'Sync009/UnityCheckpoint.json'
    pending['pending'] = [c for c in pending['pending'] if c['id'] != 'MFJ-R051']
    pending['completed'] = [c for c in pending['completed'] if c['id'] != 'MFJ-R051'] + [{'id': 'MFJ-R051', 'checkpoint': 'MFJ-SYNC-009'}]
    pending['counts'] = {key: len(pending[key]) for key in ['pending', 'completed', 'superseded']}
    write(pending_path, pending)
    update_documents()
    print(json.dumps({'checkpoint': 'MFJ-SYNC-009', 'pending': len(pending['pending']), 'completed': len(pending['completed']), 'files': len(files), 'unchanged': verification['unchangedPreviousBaselineFiles']}))


if __name__ == '__main__':
    main()
