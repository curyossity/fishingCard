"""Installs the user-selected Landing revision cues (28 November 2026) into the Charged Cabinet bank.

Selection (Docs/Audio/landing-revision): ShutterClose B, ShutterOpen B, Stamp B, Landed A, SlamImpact A, HealthDrain A (round 1);
round 2 replaces the score contacts with the "card body-check" ram: Addition and Multiplier (ram option 1), and the first
strike hit in the same family (Good = the + slap, Perfect = the slap with a small electrical accent). Each chosen audition take
(made locally with Earshot recipes) is converted to mono 48 kHz PCM 16 with a 0.639 sample peak, as every bank take is, and the
cue gain absorbs the change so the in-game level equals the audited phrase. Rows are merged into
Assets/Audio/Deepwater/manifest.json (replacing every earlier row for these cues), with each take's measured contact time
(`impactAt`) so callers can schedule its loudest moment on the visible contact. Revision takes are written under their own file
names, so the approved Charged Cabinet files stay untouched on disk. `build_charged_audio.py` reapplies this step after it
rewrites the manifest.

Run: .local-tools/earshot/venv/Scripts/python.exe -I Tools/Audio/build_landing_revision_audio.py
"""
import hashlib
import json
from pathlib import Path

import numpy as np
import soundfile as sf

ROOT = Path(__file__).resolve().parents[2]
REVISION = ROOT / "Docs/Audio/landing-revision"
OUTPUT = ROOT / "Assets/Audio/Deepwater"
APPROVAL = ROOT / "Assets/Editor/AudioSources/DeepwaterRevision/LandingRevisionApproval.json"
RATE, PEAK = 48000, .639
# cue: (chosen take under REVISION, bank file, bus, audited in-game gain dB, priority, cooldown s, pitch growth)
# Round-2 gains match each new contact's level to the approved take it replaces (render_landing_round2.hit_level).
SELECTION = {
    "ShutterClose": ("audition/ShutterClose_B.wav", "ShutterClose_1.wav", "Motion", -13, 210, .1, 1.0),
    "ShutterOpen": ("audition/ShutterOpen_B.wav", "ShutterOpen_1.wav", "Motion", -17, 210, .1, 1.0),
    "Stamp": ("audition/Stamp_B.wav", "Stamp_1.wav", "Score", -11, 120, .08, 1.0),
    "Landed": ("audition/Landed_A.wav", "Landed_1.wav", "Score", -8, 70, 0, 1.0),
    "SlamImpact": ("audition/SlamImpact_A.wav", "SlamImpact_1.wav", "Score", -9, 60, 0, 1.0),
    # Installed but not scheduled: the drain sound is switched off until it is revised.
    "HealthDrain": ("audition/HealthDrain_A.wav", "HealthDrain_1.wav", "Motion", -21, 200, .1, 1.0),
    "Addition": ("round2/Addition_1.wav", "RamAddition_1.wav", "Score", -12.63, 90, 0, .28),
    "Multiplier": ("round2/Multiplier_1.wav", "RamMultiplier_1.wav", "Score", -11.2, 65, 0, .28),
    "Good": ("round2/Addition_1.wav", "RamGood_1.wav", "Score", -14.54, 80, 0, 1.0),
    "Perfect": ("round2/Perfect_1.wav", "RamPerfect_1.wav", "Score", -10.46, 65, 0, 1.0),
}


def build():
    rows, approval = [], []
    for cue, (take, file, bus, level, priority, cooldown, growth) in SELECTION.items():
        source = REVISION / take
        audio, rate = sf.read(source)
        if audio.ndim > 1:
            audio = audio.mean(axis=1)
        if rate != RATE:
            raise ValueError(f"{source} is not {RATE} Hz")
        peak = float(np.abs(audio).max())
        bake = PEAK / peak
        gain = level - 20 * np.log10(bake)
        if gain > -.001:
            raise ValueError(f"{cue} would need gain above unity")
        path = OUTPUT / file
        sf.write(path, audio * bake, RATE, subtype="PCM_16")
        written, _ = sf.read(path)
        impact = float(np.abs(written).argmax()) / RATE
        rows.append(dict(cue=cue, file=path.relative_to(ROOT).as_posix(), category=bus, gainDb=float(gain), priority=priority,
                         cooldown=cooldown, duration=round(len(written) / RATE, 3), loop=False,
                         sha256=hashlib.sha256(path.read_bytes()).hexdigest(), pitchGrowth=growth, intensityGainDb=0.0,
                         impactAt=round(impact, 4)))
        approval.append(dict(cue=cue, source=source.relative_to(ROOT).as_posix(),
                             sourceSha256=hashlib.sha256(source.read_bytes()).hexdigest(), auditedGainDb=level))
    return rows, approval


def merge(rows):
    manifest_path = OUTPUT / "manifest.json"
    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    names = {row["cue"] for row in rows}
    manifest["cues"] = [row for row in manifest["cues"] if row["cue"] not in names] + rows
    manifest["landingRevision"] = "2026-11-28"
    manifest_path.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")


def main():
    rows, approval = build()
    merge(rows)
    APPROVAL.write_text(json.dumps(dict(selectedOn="2026-11-28", selection="User picks from Docs/Audio/landing-revision audition (round 1) and round2 (ram option 1).",
                                        tool="Earshot 1.0.0 local recipes (public-domain recording models); no hosted service or key.",
                                        cues=approval), indent=2) + "\n", encoding="utf-8")
    print(json.dumps([dict(cue=r["cue"], gainDb=round(r["gainDb"], 2), impactAt=r["impactAt"]) for r in rows]))


if __name__ == "__main__":
    main()
