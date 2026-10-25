"""Record the verified selective R046 import without rewriting historical checkpoints."""
from pathlib import Path
import hashlib
import json
import subprocess

ROOT = Path(__file__).resolve().parents[4]
BASE = ROOT / "Docs/UIThemes/MidnightFieldJournal"
OUT = BASE / "Sync008"


def read(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def write(path, value):
    path.write_text(json.dumps(value, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


previous = read(BASE / "Sync007/UnityCheckpoint.json")
paths = {entry["path"] for entry in previous["files"]}
paths.update({"Assets/Scripts/EncounterRuntime.cs", "Assets/Scripts/EncounterRuntime.cs.meta"})
paths.update(str(p.relative_to(ROOT)).replace("\\", "/") for p in (ROOT / "Assets/UIThemes/MidnightFieldJournal").rglob("*") if p.is_file())
for name in ("EncounterRarityTiming", "EncounterRarityTimingView", "EncounterRuntime.RarityTiming", "RarityTimingProfile"):
    for suffix in (".cs", ".cs.meta"):
        paths.add("Assets/Scripts/" + name + suffix)
files = [{"path": p, "sha256": sha(ROOT / p)} for p in sorted(paths) if (ROOT / p).is_file()]
old_hashes = {e["path"]: e["sha256"] for e in previous["files"]}
changed = [e["path"] for e in files if e["path"] in old_hashes and e["sha256"] != old_hashes[e["path"]]]
allowed = {
    "Assets/Scripts/EncounterRuntime.cs", "Assets/Scripts/FishingRunController.cs", "Assets/Scripts/FishingRunView.cs",
    "Assets/UIThemes/MidnightFieldJournal/Editor/JournalComparisonSceneBuilder.cs",
    "Assets/UIThemes/MidnightFieldJournal/Editor/MidnightFieldJournalHudBuilder.cs",
    "Assets/UIThemes/MidnightFieldJournal/Prefabs/MidnightFieldJournalCanvas.prefab",
    "Assets/UIThemes/MidnightFieldJournal/Scenes/MidnightFieldJournalComparison.unity",
}
unexpected = set(changed) - allowed
if unexpected:
    raise RuntimeError("Unrelated checkpoint files changed: " + str(sorted(unexpected)))
protected = ["Assets/Scenes/SampleScene.unity", "ProjectSettings/EditorBuildSettings.asset"]
for path in protected:
    if (ROOT / path).read_bytes().replace(b"\r\n", b"\n") != subprocess.check_output(["git", "show", "HEAD:" + path], cwd=ROOT).replace(b"\r\n", b"\n"):
        raise RuntimeError("Protected original scene/build configuration changed: " + path)
frame = "Assets/UIThemes/MidnightFieldJournal/Art/Rarity/RarityBarFrame.png"
write(OUT / "SourceManifest.json", {
    "revision": "MFJ-R046", "fileId": "9e056cfe-f83f-81f8-8008-cc523ff45211",
    "pageId": "bae680cd-9225-8018-8008-cddd6a6f3825", "shapeId": "bae680cd-9225-8018-8008-d8641d512f22",
    "source": "Approved blank frame exported directly from Penpot; no screenshot slicing or generated card text",
    "path": frame, "sha256": sha(ROOT / frame), "nativeSize": [1160, 340], "spriteBorder": [64, 64, 64, 64],
    "dynamicElements": ["zones", "patterns", "legends", "marker", "stop button", "heading", "feedback"],
})
production = read(OUT / "ProductionValidation.json")
if not production["status"].startswith("Passed") or production["assertions"] != 22:
    raise RuntimeError("Production validation is incomplete")
write(OUT / "Verification.json", {
    "status": "verified", "date": "2026-10-25", "timingAssertions": 46, "productionAssertions": 22,
    "gameplaySuites": {"ApprovedTechnique": 39, "CoastalSupport": 62, "PredatorSchoolRay": 87,
        "ShellFeedingExchange": 87, "BassUrchinOctopus": 101, "LobsterSquidEel": 80, "CoastalEvent": 57, "EventPersistence": 45},
    "consoleErrors": 0, "productionRuntimeWarnings": 0,
    "existingCompilationWarnings": "Older scripts use deprecated FindFirstObjectByType; the new timing scripts use FindAnyObjectByType.",
    "captureResolution": [3840, 2160],
    "protectedPreviousCheckpointFilesUnchanged": True, "changedPreviousFiles": changed,
    "scope": "Isolated comparison scene; legacy scene/build configuration preserved",
})
write(OUT / "UnityCheckpoint.json", {
    "checkpoint": "MFJ-SYNC-008", "date": "2026-10-25", "previousCheckpoint": "../Sync007/UnityCheckpoint.json",
    "revisions": ["MFJ-R046"], "kind": "Creature rarity timing and approved R046 instrument", "files": files,
})
ledger = read(BASE / "DesignChangeLedger.json")
entry = next(e for e in ledger["changes"] if e["id"] == "MFJ-R046")
entry.update(status="verified", unityImported=True, integrationStatus="verified", synchronizationCheckpoint="MFJ-SYNC-008",
    approvalEvidence="User requested integration: stop the bar before creature reveal; repeat after Catch/Bait and Descend; events bypass timing.",
    importEvidence={"manifest": "Sync008/SourceManifest.json", "checkpoint": "Sync008/UnityCheckpoint.json"},
    verificationEvidence={"report": "Sync008/Verification.json"}, remainingWork=[])
ledger["currentUnityCheckpoint"] = "Sync008/UnityCheckpoint.json"
if not any(e["id"] == "MFJ-SYNC-008" for e in ledger["synchronizationCheckpoints"]):
    ledger["synchronizationCheckpoints"].append({"id": "MFJ-SYNC-008", "date": "2026-10-25", "revisions": ["MFJ-R046"], "status": "verified", "manifest": "Sync008/UnityCheckpoint.json", "verification": "Sync008/Verification.json"})
write(BASE / "DesignChangeLedger.json", ledger)
index = read(BASE / "PendingUnityChanges.json")
index["pending"] = [e for e in index["pending"] if e["id"] != "MFJ-R046"]
if not any(e["id"] == "MFJ-R046" for e in index["completed"]):
    index["completed"].append({"id": "MFJ-R046", "checkpoint": "MFJ-SYNC-008"})
index["currentUnityCheckpoint"] = "Sync008/UnityCheckpoint.json"
write(BASE / "PendingUnityChanges.json", index)
manifest = read(BASE / "Revisions/MFJ-R046/Assets.json")
manifest.update(status="verified", unityImported=True, synchronizationCheckpoint="MFJ-SYNC-008", unityImportManifest="../../Sync008/SourceManifest.json")
write(BASE / "Revisions/MFJ-R046/Assets.json", manifest)

for name in ("Design Changes.md", "Pending Unity Changes.md"):
    path = BASE / name
    text = path.read_text(encoding="utf-8")
    lines = text.splitlines()
    if name == "Design Changes.md":
        lines = ["**No pending changes: 39 imported and verified; seven superseded drafts.** MFJ-R046 is integrated as [MFJ-SYNC-008](Sync008/README.md). [Pending index](PendingUnityChanges.json)." if line.startswith("**One pending proposal:") else line for line in lines]
        lines = ["| MFJ-R046 | Creature rarity timing bar, reusable R046 instrument and stop-before-reveal flow. | Imported and verified at MFJ-SYNC-008. | Events bypass timing; fixed encounter promises preserved. |" if line.startswith("| MFJ-R046 |") else line for line in lines]
    else:
        lines = ["**Current: 0 pending; 39 imported and verified; 7 superseded drafts.** R046 is integrated. [Pending index](PendingUnityChanges.json) · [Full ledger](DesignChangeLedger.json)." if line.startswith("**Current:") else line for line in lines]
        lines = ["Latest baseline: [MFJ-SYNC-008](Sync008/UnityCheckpoint.json). Next revision: **MFJ-R047**." if line.startswith("Latest baseline:") else line for line in lines]
    text = "\n".join(lines) + "\n"
    if "## MFJ-R046 integration — MFJ-SYNC-008" not in text:
        text += "\n## MFJ-R046 integration — MFJ-SYNC-008\n\nImported and verified on 25 October 2026. Creature timing waits for Stop before reveal; events bypass the bar. Reusable 4K presentation, configuration, gameplay validation and source hashes are recorded in [Sync008](Sync008/README.md). Earlier design-only statements describe historical status. No pending import remains.\n"
    path.write_text(text, encoding="utf-8")
path = BASE / "VisualContract.md"
text = path.read_text(encoding="utf-8")
start = text.index("## MFJ-R046")
text = text[:start] + """## MFJ-R046 — Integrated as MFJ-SYNC-008

**Imported and verified.** The approved parchment timing instrument temporarily replaces the central encounter during creature selection. Its sliced frame has fixed 64px corners; native zones/patterns, external legends, independent outlined compass marker, Stop and stopped-result wash/burst remain editable and reusable. No creature information is visible before Stop and its brief feedback hold. Events bypass timing. Available rarity intervals normalize dynamically to the full track; queued/trail/chain/Apex promises retain their identity through a fixed-rarity interval. [Import and 4K verification](Sync008/README.md).

**No pending changes remain: 39 completed imports, seven superseded drafts.** Future selective integration uses [MFJ-SYNC-008](Sync008/UnityCheckpoint.json). The original Penpot design study and previous checkpoints remain historical evidence.
"""
path.write_text(text, encoding="utf-8")
path = BASE / "Revisions/MFJ-R046/README.md"
text = path.read_text(encoding="utf-8")
if "**Integrated and verified as" not in text:
    text = text.replace("Penpot-only visual proposal, 25 October 2026.", "**Integrated and verified as [MFJ-SYNC-008](../../Sync008/README.md) on 25 October 2026.** The design-only notes below preserve the original proposal.\n\nOriginal Penpot-only visual proposal, 25 October 2026.", 1)
path.write_text(text, encoding="utf-8")
print(json.dumps({"checkpointFiles": len(files), "changedPreviousFiles": changed, "pending": len(index["pending"]), "completed": len(index["completed"])}))
