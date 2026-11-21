"""Render whole Discover/Build phrases with the installed bank, preserving its mix."""
import json

import numpy as np
from scipy import signal
import soundfile as sf

from render_sound_revision import ROOT, RATE, compose, db, edge, meter

OUTPUT = ROOT / "Docs/Validation/ChargedCabinet"


def main():
    rows = json.loads((ROOT / "Assets/Audio/Deepwater/manifest.json").read_text(encoding="utf-8"))["cues"]
    selected = {name: next(row for row in rows if row["cue"] == name) for name in {row["cue"] for row in rows}}
    report = {}
    for name in ["DiscoverOrdinary", "DiscoverTriple", "BuildAndUpgrade"]:
        reference = json.loads((ROOT / "Docs/Validation/AudioDirection" / (name + ".json")).read_text(encoding="utf-8"))
        layers = []
        for event in reference["events"]:
            cue = selected[event["cue"]]
            audio, _ = sf.read(ROOT / cue["file"])
            if event["cue"] == "SlotRoll":
                count = round((1.35 - event["time"]) * RATE)
                audio = edge(np.tile(audio, int(np.ceil(count / len(audio))))[:count], .04, .04)
            if event["pitch"] != 1:
                audio = signal.resample(audio, round(len(audio) / event["pitch"]))
            layers.append((event["time"], audio * db(cue["gainDb"])))
        duration = {"DiscoverOrdinary": 1.95, "DiscoverTriple": 2.1, "BuildAndUpgrade": 2.8}[name]
        path = OUTPUT / (name + ".wav")
        sf.write(path, compose(duration, layers), RATE, subtype="PCM_16")
        report[name] = meter(path)
        assert report[name]["truePeak"] < -1, "Storyboard exceeds output headroom: " + name
        reference["note"] = "Authored interaction storyboard with the installed calibrated bank; not device loopback."
        (OUTPUT / (name + ".json")).write_text(json.dumps(reference, indent=2) + "\n", encoding="utf-8")
    (OUTPUT / "StoryboardLevels.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report))


if __name__ == "__main__":
    main()
