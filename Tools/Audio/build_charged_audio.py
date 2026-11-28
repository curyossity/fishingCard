"""Install the user-selected Charged Cabinet bank with the audition's exact calibration.

The raw recipe renders offline from Editor-only source material. Calibration is
split between available PCM headroom and cue gain, so AudioSource never clamps a
gain above one and the runtime keeps the audition's relative levels.
"""
import argparse
import hashlib
import json
from pathlib import Path

import numpy as np
import soundfile as sf

from render_sound_revision import RATE, ROOT, db, render_bank

APPROVAL = ROOT / "Assets/Editor/AudioSources/DeepwaterRevision/ChargedApproval.json"
OUTPUT = ROOT / "Assets/Audio/Deepwater"
EVIDENCE = ROOT / "Docs/Validation/ChargedCabinet"


def record_selection():
    """Freeze the user's selected source hashes and comparison trim for reproducible production builds."""
    preview = ROOT / "Docs/Validation/AudioRevision"
    raw = json.loads((preview / "charged/manifest.json").read_text(encoding="utf-8"))
    levels = json.loads((preview / "ComparisonLevels.json").read_text(encoding="utf-8"))["charged"]
    approval = dict(selectedOn="2026-11-21", selection="User chose A — Charged cabinet in the native-animation comparison.",
                    comparisonTrimDb=levels["comparisonTrimDb"], pitchGrowth=.28,
                    cues=raw["cues"], comparisonLevels=levels["final"])
    APPROVAL.write_text(json.dumps(approval, indent=2) + "\n", encoding="utf-8")
    print("Recorded the selected Charged Cabinet source and mix calibration.")


def main():
    approval = json.loads(APPROVAL.read_text(encoding="utf-8"))
    raw_rows = render_bank("charged", ROOT / "Temp/DeepwaterChargedRaw")
    approved = {row["cue"] + "/" + Path(row["file"]).name: row for row in approval["cues"]}
    trim = approval["comparisonTrimDb"]
    rows, calibration = [], []
    for row in raw_rows:
        raw_path = ROOT / row["file"]
        key = row["cue"] + "/" + raw_path.name
        if hashlib.sha256(raw_path.read_bytes()).hexdigest() != approved[key]["sha256"]:
            raise ValueError("Recipe/source differs from the selected audition: " + key)
        audio, rate = sf.read(raw_path)
        if rate != RATE:
            raise ValueError("Source rate differs from the selected audition.")
        peak = float(np.abs(audio).max())
        headroom = 20 * np.log10(.639 / max(.0001, peak))
        bake = min(trim, headroom)
        gain = row["gainDb"] + trim - bake
        if gain > -.001:
            raise ValueError("Cue calibration would be clamped by AudioSource: " + key)
        final = audio * db(bake)
        path = OUTPUT / raw_path.name
        sf.write(path, final, RATE, subtype="PCM_16")
        expected = audio * db(row["gainDb"] + trim)
        encoded, _ = sf.read(path)
        actual = encoded * db(gain)
        error = float(np.abs(expected - actual).max())
        if error > .00004:
            raise ValueError("Quantized cue no longer matches audition calibration: " + key)
        target = dict(row, file=path.relative_to(ROOT).as_posix(), gainDb=float(gain),
                      pitchGrowth=.28 if row["cue"] in ("Addition", "Multiplier") else 1.,
                      intensityGainDb=0., sha256=hashlib.sha256(path.read_bytes()).hexdigest())
        rows.append(target)
        calibration.append(dict(cue=row["cue"], take=path.name, pcmGainDb=float(bake),
                                voiceGainDb=float(gain), effectiveSampleError=error))
    manifest = dict(version=3, direction="charged", selectedOn=approval["selectedOn"],
                    sampleRate=RATE, auditionTrimDb=trim, cues=rows)
    (OUTPUT / "manifest.json").write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
    EVIDENCE.mkdir(parents=True, exist_ok=True)
    (EVIDENCE / "Calibration.json").write_text(json.dumps(dict(passed=True, auditionTrimDb=trim, cues=calibration), indent=2) + "\n", encoding="utf-8")
    print(json.dumps(dict(cues=len({r["cue"] for r in rows}), takes=len(rows),
                         voiceGainRange=[min(r["gainDb"] for r in rows), max(r["gainDb"] for r in rows)],
                         maximumEffectiveSampleError=max(r["effectiveSampleError"] for r in calibration))))


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--record-selection", action="store_true")
    args = parser.parse_args()
    if args.record_selection:
        record_selection()
    else:
        main()
        # The 28 November Landing revision cues live beside the approved family; rewriting the manifest must keep them.
        from build_landing_revision_audio import main as landing_revision
        landing_revision()
