"""Compare the installed Unity score exports against the user's selected audition."""
import json
from pathlib import Path

import numpy as np
import soundfile as sf

from render_sound_revision import ROOT, meter

OUTPUT = ROOT / "Docs/Validation/ChargedCabinet"


def main():
    approval = json.loads((ROOT / "Assets/Editor/AudioSources/DeepwaterRevision/ChargedApproval.json").read_text(encoding="utf-8"))
    manifest = json.loads((ROOT / "Assets/Audio/Deepwater/manifest.json").read_text(encoding="utf-8"))
    cues = {row["cue"]: row for row in manifest["cues"]}
    results = []
    for tier in ["Strong", "Exceptional"]:
        runtime_path = OUTPUT / f"Runtime{tier}.wav"
        measured = meter(runtime_path)
        approved = approval["comparisonLevels"][tier]
        timeline = json.loads((OUTPUT / f"Runtime{tier}.json").read_text(encoding="utf-8"))
        impacts = [event for event in timeline["events"] if event["kind"] in ("Addition", "Multiplier")]
        assert all(1 <= event["pitch"] <= 1.281 for event in impacts)
        assert all(event["gain"] <= 1 for event in timeline["events"])
        assert measured["truePeak"] < -1
        difference = measured["lufs"] - approved["lufs"]
        assert abs(difference) < .35, f"{tier} differs from the selected comparison level: {difference} LU"
        results.append(dict(tier=tier, installed=measured, audition=approved, loudnessDifferenceLu=difference))
    # Effective source waveforms must retain the calibration, including a once-through loop.
    errors = []
    for row in manifest["cues"]:
        installed, rate = sf.read(ROOT / row["file"])
        name = Path(row["file"]).name
        reference, ref_rate = sf.read(ROOT / "Docs/Validation/AudioRevision/charged" / name)
        raw = next(item for item in approval["cues"] if Path(item["file"]).name == name)
        expected = reference * 10 ** ((raw["gainDb"] + approval["comparisonTrimDb"]) / 20)
        actual = installed * 10 ** (row["gainDb"] / 20)
        assert rate == ref_rate and len(actual) == len(expected)
        error = float(np.max(np.abs(expected - actual)))
        assert error < .00004
        errors.append(error)
    report = dict(passed=True, direction="charged", takes=len(errors), maximumEffectiveSampleError=max(errors), mixes=results)
    (OUTPUT / "AuditionParity.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
