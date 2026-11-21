"""Check revision audition assets and record local source provenance; no network or credentials."""
import hashlib
import json
from pathlib import Path
import re

import numpy as np
import soundfile as sf

ROOT = Path(__file__).resolve().parents[2]
PREVIEW = ROOT / "Docs/Validation/AudioRevision"
SOURCE = ROOT / "Assets/Editor/AudioSources/DeepwaterRevision"
IDS = {"pinball": "pinball/play", "button_1": "switch/push", "button_2": "switch/push", "button_3": "switch/push", "ring": "coin/flip", "impact": "kick/punch", "spring": "jump/spring", "glint": "note/glass_pluck", "travel": "whoosh/fast", "weight": "slam/title", "flare": "shine/match", "pulse": "note/pluck_bass"}


def main():
    provenance = dict(earshotRevision="7a7f7c5c70d4a763a1eca4b3666af7c77e313f97",
                      method="Guarded local Earshot MCP make; default seed 0; switch/push n=3. No hosted calls, uploads or API keys.",
                      sources=[dict(file=p.name, catalogueId=IDS[p.stem], sha256=hashlib.sha256(p.read_bytes()).hexdigest()) for p in SOURCE.glob("*.wav")])
    (SOURCE / "sources.json").write_text(json.dumps(provenance, indent=2) + "\n", encoding="utf-8")
    upstream = ROOT / ".local-tools/earshot/source/earshot-7a7f7c5c70d4a763a1eca4b3666af7c77e313f97"
    (SOURCE / "Earshot-LICENSE.txt").write_bytes((upstream / "LICENSE").read_bytes())
    credits = []
    for kind in ["resyn", "drums"]:
        credits += [line for line in (upstream / "earshot/data" / kind / "CREDITS.md").read_text(encoding="utf-8").splitlines() if any(design in line for design in IDS.values())]
    (SOURCE / "Model Credits.md").write_text("# Credits of fitted model source families\n\nOther selected designs are procedural; this list is not a claim that every rendered source is an original recording.\n\n" + "\n".join(credits) + "\n", encoding="utf-8")
    checked = 0
    for direction in ["charged", "playful"]:
        manifest = json.loads((PREVIEW / direction / "manifest.json").read_text(encoding="utf-8"))
        assert len(manifest["cues"]) == 42
        assert len({row["cue"] for row in manifest["cues"]}) == 28
        for row in manifest["cues"]:
            path = ROOT / row["file"]
            audio, rate = sf.read(path)
            assert rate == 48000 and audio.ndim == 1
            assert .01 < np.max(np.abs(audio)) <= .65
            assert np.isfinite(audio).all()
            assert abs(audio[0] - audio[-1]) < .001 if row["loop"] else abs(audio[0]) < .001 and abs(audio[-1]) < .001
            assert hashlib.sha256(path.read_bytes()).hexdigest() == row["sha256"]
            checked += 1
    levels = json.loads((PREVIEW / "ComparisonLevels.json").read_text(encoding="utf-8"))
    for data in levels.values():
        assert abs(data["final"]["Exceptional"]["lufs"] + 24.8) < .1
        assert all(m["truePeak"] < -1 for m in data["final"].values())
    page = (PREVIEW / "Compare.html").read_text(encoding="utf-8")
    for filename in re.findall(r'<script src="([^"]+)"', page):
        assert (PREVIEW / filename).resolve().is_file()
    for direction in ["current", "charged", "playful"]:
        for tier in ["Ordinary", "Strong", "Exceptional"]:
            assert (PREVIEW / (direction + "-" + tier + ".wav")).is_file()
    # Extract inline JavaScript for an optional local parser check without launching a browser or service.
    inline = "\n".join(re.findall(r"<script>(.*?)</script>", page, re.S))
    (ROOT / "Temp").mkdir(exist_ok=True)
    (ROOT / "Temp/DeepwaterSoundComparison.js").write_text(inline, encoding="utf-8")
    result = dict(passed=True, candidateWavs=checked, cueFamiliesPerCandidate=28, comparisonMixes=9, humanListening=False, runtimeBindingsChanged=False)
    (PREVIEW / "Verification.json").write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(result))


if __name__ == "__main__":
    main()
