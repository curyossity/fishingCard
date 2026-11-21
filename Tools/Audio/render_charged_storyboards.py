"""Render whole Discover/Build phrases with the installed bank, preserving its mix."""
import json
import re

import numpy as np
from scipy import signal
import soundfile as sf

from render_sound_revision import ROOT, RATE, compose, db, edge, meter

OUTPUT = ROOT / "Docs/Validation/ChargedCabinet"


def main():
    prefab = (ROOT / "Assets/UIThemes/TackleApparatus/Prefabs/DiscoverCanvas.prefab").read_text(encoding="utf-8")
    duration = float(re.search(r"^  spinDuration: ([0-9.]+)$", prefab, re.M).group(1))
    stagger = float(re.search(r"^  stopStagger: ([0-9.]+)$", prefab, re.M).group(1))
    settles = [float(value) for value in re.findall(r"^  settleFraction: ([0-9.]+)$", prefab, re.M)]
    brake_times = np.array([(duration + i * stagger) * (1 - settles[i]) for i in range(3)])
    settle_at = duration + 2 * stagger
    rows = json.loads((ROOT / "Assets/Audio/Deepwater/manifest.json").read_text(encoding="utf-8"))["cues"]
    selected = {name: next(row for row in rows if row["cue"] == name) for name in {row["cue"] for row in rows}}
    report = {}
    for name in ["DiscoverOrdinary", "DiscoverTriple", "BuildAndUpgrade"]:
        reference = json.loads((ROOT / "Docs/Validation/AudioDirection" / (name + ".json")).read_text(encoding="utf-8"))
        layers = []
        # Keep the authored sequence, but derive its contacts/result from the active animation rather than old delays.
        if name.startswith("Discover"):
            for event in reference["events"]:
                if event["cue"].startswith("SlotStop"):
                    event["time"] = float(brake_times[int(event["cue"][-1]) - 1])
                elif event["cue"] in ("Pair", "Triple"):
                    event["time"] = settle_at
        for event in reference["events"]:
            cue = selected[event["cue"]]
            audio, _ = sf.read(ROOT / cue["file"])
            if event["cue"] == "SlotRoll":
                count = round((max(brake_times) + .04 - event["time"]) * RATE)
                times = np.arange(count) / RATE + event["time"]
                speed = np.mean(np.maximum(0, 1 - times[:, None] / brake_times[None, :]) ** 2, axis=1)
                pitch = .78 + .22 * np.sqrt(speed)
                position = np.cumsum(pitch) % len(audio)
                audio = np.interp(position, np.arange(len(audio) + 1), np.append(audio, audio[0]))
                audio = edge(audio * np.sqrt(speed), .04, .04)
            if event["pitch"] != 1:
                audio = signal.resample(audio, round(len(audio) / event["pitch"]))
            layers.append((event["time"], audio * db(cue["gainDb"])))
        length = {"DiscoverOrdinary": 1.95, "DiscoverTriple": 2.1, "BuildAndUpgrade": 2.8}[name]
        path = OUTPUT / (name + ".wav")
        sf.write(path, compose(length, layers), RATE, subtype="PCM_16")
        report[name] = meter(path)
        assert report[name]["truePeak"] < -1, "Storyboard exceeds output headroom: " + name
        reference["note"] = "Authored interaction storyboard with the installed calibrated bank; not device loopback."
        (OUTPUT / (name + ".json")).write_text(json.dumps(reference, indent=2) + "\n", encoding="utf-8")
    (OUTPUT / "StoryboardLevels.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report))


if __name__ == "__main__":
    main()
