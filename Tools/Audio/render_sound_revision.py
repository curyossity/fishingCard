"""Render two deliberately contrasting sound directions over the game's frozen scoring schedules.

All sources are local. No network or API credentials. The preview keeps the game's
native animation and matches comparison levels before judging timbre.
"""
from __future__ import annotations

from functools import lru_cache
import hashlib
import json
from pathlib import Path
import re
import subprocess

import numpy as np
from scipy import signal
import soundfile as sf

ROOT = Path(__file__).resolve().parents[2]
SOURCES = ROOT / "Assets/Editor/AudioSources/DeepwaterRevision"
PREVIEW = ROOT / "Docs/Validation/AudioRevision"
RATE = 48000
FFMPEG = ROOT / ".local-tools/earshot/ffmpeg/ffmpeg.exe"


def db(value):
    return 10 ** (value / 20)


def edge(audio, attack=.001, release=.012):
    audio = np.array(audio, dtype=np.float64, copy=True)
    a, b = min(len(audio), round(attack * RATE)), min(len(audio), round(release * RATE))
    if a:
        audio[:a] *= np.sin(np.linspace(0, np.pi / 2, a)) ** 2
    if b:
        audio[-b:] *= np.sin(np.linspace(np.pi / 2, 0, b)) ** 2
    return audio


@lru_cache(maxsize=64)
def raw(name):
    path = SOURCES / (name + ".wav")
    if name.startswith("card_"):
        path = ROOT / "Assets/Editor/AudioSources/Deepwater/Recordings" / f"card-place-{name[-1]}.ogg"
    audio, rate = sf.read(path, always_2d=True)
    return signal.resample_poly(audio.mean(axis=1), RATE, rate)


def part(name, duration, pitch=0, gain=0, at=0, start=None, low=7000, high=65, reverse=False, stretch=False):
    audio = raw(name).copy()
    if start is None:
        # Trim pre-contact silence. Do not turn peak alignment into a missing attack.
        envelope = np.convolve(np.abs(audio), np.ones(48) / 48, mode="same")
        starts = np.flatnonzero(envelope > max(.0001, envelope.max() * .035))
        offset = max(0, starts[0] - 96) if len(starts) else 0
    else:
        offset = round(start * RATE)
    audio = audio[offset:]
    if name == "pinball":
        audio = audio[:round(.29 * RATE)]
    if pitch:
        audio = signal.resample(audio, round(len(audio) / 2 ** (pitch / 12)))
    if stretch:
        audio = signal.resample(audio, round(duration * RATE))
    audio = signal.sosfilt(signal.butter(2, high, "highpass", fs=RATE, output="sos"), audio)
    audio = signal.sosfilt(signal.butter(2, low, "lowpass", fs=RATE, output="sos"), audio)
    audio = audio[:round(duration * RATE)]
    if reverse:
        audio = audio[::-1]
    audio = edge(audio)
    audio *= .65 / max(.001, np.abs(audio).max()) * db(gain)
    return at, audio


def compose(duration, layers):
    audio = np.zeros(round(duration * RATE))
    for at, audio_part in layers:
        begin = round(at * RATE)
        count = min(len(audio_part), len(audio) - begin)
        if count > 0:
            audio[begin:begin + count] += audio_part[:count]
    return edge(audio)


def render_bank(direction, folder):
    folder.mkdir(parents=True, exist_ok=True)
    brighter = direction == "playful"
    rows = []

    def cue(name, duration, layers, bus="Score", gain=-10, priority=90, cooldown=0, take=0, loop=False):
        audio = compose(duration, layers)
        # Reserve headroom, while preserving the body of quieter cues.
        audio *= min(1, .64 / max(.001, np.abs(audio).max()))
        if loop:
            n = round(.035 * RATE); t = np.linspace(0, 1, n)
            audio[:n] = audio[:n] * t + audio[-n:] * (1 - t)
            audio = audio[:-n]; audio[-1] = audio[0]
        path = folder / f"{name}_{take + 1}.wav"
        sf.write(path, audio, RATE, subtype="PCM_16")
        rows.append(dict(cue=name, file=path.relative_to(ROOT).as_posix(), category=bus, gainDb=gain,
                         priority=priority, cooldown=cooldown, duration=len(audio) / RATE, loop=loop,
                         sha256=hashlib.sha256(path.read_bytes()).hexdigest()))

    def contact(i=0, gain=-7):
        return part(f"button_{i % 3 + 1}", .065, pitch=-2 if not brighter else 2, gain=gain, low=4800)

    def body(gain=-12, pitch=0, duration=.12, at=0):
        return part("impact", duration, pitch=pitch, gain=gain, at=at, low=1400)

    def glint(duration=.15, pitch=0, gain=-12, at=0):
        if brighter:
            return part("glint", duration, pitch=pitch, gain=gain, at=at, low=6500, high=180)
        # A damped contact with an electrical body, rather than a literal coin jingle.
        return at, compose(duration, [part("ring", min(duration, .047), pitch, gain - 7, low=3400, high=750),
                                       part("pulse", min(duration, .13), pitch + 12, gain - 5, low=2400, high=120)])

    def release(big=False):
        layers = [part("pinball", .16, pitch=-5, gain=-9, start=1.32), body(-8, duration=.2), glint(.25, -5, -7)]
        if brighter:
            layers += [glint(.26, 7, -9, .03), glint(.25, 12, -13, .09)]
        else:
            layers += [part("flare", .21, -4, -16, at=.015, low=4200), part("pulse", .18, -12, -16, low=450)]
        if big:
            # A short spray of score sparks, not a precomposed musical fanfare.
            for j, at in enumerate([.045, .088, .126, .16, .197, .228]):
                layers.append(glint(.09, [0, 3, 7, 5, 10, 12][j], -14 - j * .8, at))
        return layers

    for i in range(3):
        cue("TagSelect", .105, [contact(i, -6), part(f"card_{i + 1}", .10, gain=-12)], "Interface", -14, 180, .07, i)
        cue("TagDeselect", .10, [part(f"card_{i + 1}", .09, pitch=-2, gain=-6)], "Interface", -15, 180, .07, i)
        cue("Preview", .10, [part(f"card_{i + 1}", .095, gain=-4), contact(i, -18)], "Interface", -16, 180, .075, i)
        cue("SlotTick", .045, [part(f"button_{i + 1}", .043, gain=-9, low=3000, high=500)], "Motion", -23, 220, .045, i)
        cue("Addition", .18, [contact(i, -14), glint(.135, gain=-4), part("pulse", .10, 0, -19, low=750)], "Score", -10, 90, 0, i)
        multiplier = [part("pinball", .10, -3, -11, start=1.32), body(-12), glint(.18, 5 if brighter else -5, -7), part("flare", .12, gain=-13, low=3800)]
        if brighter:
            multiplier.append(glint(.13, 12, -14, .025))
        else:
            multiplier.append(part("spring", .095, -12, -18, low=1600))
        cue("Multiplier", .25, multiplier, "Score", -8, 65, 0, i)
        cue("Attach", .21, [part(f"card_{i + 1}", .15, gain=-8), contact(i, -6), part("spring", .085, -12, -17, at=.025, low=2100)], "Score", -10, 70, 0, i)

    cue("SlotStart", .23, [part("pinball", .135, -5, -7, start=1.32), part("travel", .15, -7, -17, low=2300), body(-11)], "Motion", -9, 110)
    cue("SlotRoll", .935, [part("travel", .935, -5, -5, low=1800, high=250, stretch=True), part("spring", .935, -12, -22, low=1000, stretch=True)], "Motion", -26, 240, loop=True)
    for i in range(3):
        cue(f"SlotStop{i + 1}", .15, [part("pinball", .13, -3 + i * 2, -7, start=1.32), body(-16), glint(.08, i * 2, -20 if not brighter else -13)], "Motion", -10, 100)
    cue("SlotLock", .12, [contact(0, -5), part("pinball", .055, -7, -15, at=.035, start=1.32)], "Interface", -13, 160)
    cue("SlotUnlock", .11, [contact(1, -9), part("card_2", .075, -2, -10)], "Interface", -15, 160)
    cue("Pair", .23, [glint(.17, 0, -6), glint(.12, 7, -12, .055), contact(1, -18)], "Score", -13, 80)
    cue("Triple", .44, release(), "Score", -9, 60)
    cue("Reveal", .20, [part("card_3", .15, gain=-5), part("travel", .12, -5, -15, low=2500)], "Interface", -15, 160)
    cue("Strike", .08, [contact(0, -6), body(-15, duration=.07)], "Motion", -12, 100)
    cue("Good", .16, [part("pinball", .08, -4, -9, start=1.32), body(-15), glint(.12, -5, -11)], "Score", -10, 80)
    cue("Perfect", .21, [contact(0, -12), glint(.18, 0, -5), body(-12), part("flare", .08, 0, -20, low=4000)], "Score", -8, 65)
    cue("Miss", .14, [part("pinball", .11, -12, -11, start=1.32, low=1000), part("card_3", .06, -5, -15)], "Motion", -14, 120)
    cue("Travel", .12, [part("travel", .12, -3, -4, low=3500, high=300), part("flare", .10, -5, -17, low=4200, reverse=True)], "Motion", -21, 200)
    cue("PayoutSmall", .20, [part("pinball", .09, -7, -12, start=1.32), glint(.14, -5, -12), body(-15)], "Score", -12, 60)
    cue("PayoutMedium", .32, release(), "Score", -9, 45)
    cue("PayoutBig", .57, release(True), "Score", -6, 35)
    cue("Failure", .34, [part("spring", .22, -15, -15, low=1500), part("pinball", .12, -12, -9, start=1.32, low=900)], "Score", -14, 50)
    cue("BuildTravel", .22, [part("card_1", .16, gain=-6), part("travel", .20, -5, -18, low=2800)], "Motion", -19, 200)
    cue("Upgrade", .45, [contact(0, -9), body(-15), glint(.22, 0, -6), part("flare", .14, gain=-14), glint(.16, 7, -14, .065)], "Score", -9, 50)
    (folder / "manifest.json").write_text(json.dumps(dict(version=2, direction=direction, sampleRate=RATE, cues=rows), indent=2) + "\n", encoding="utf-8")
    return rows


def mix_events(bank, events, duration):
    by_name = {name: [r for r in bank if r["cue"] == name] for name in {r["cue"] for r in bank}}
    takes = {}; layers = []
    for e in events:
        name = e["kind"]; take = takes.get(name, 0); takes[name] = take + 1
        row = by_name[name][take % len(by_name[name])]
        clip, _ = sf.read(ROOT / row["file"])
        # Preserve exact event times, with a restrained sound-design rise in the darker direction.
        pitch = e.get("pitch", 1)
        if bank[0].get("direction") == "charged" and name in ("Addition", "Multiplier"):
            pitch = 1 + (pitch - 1) * .28
        if name == "Travel":
            pitch = e.get("pitch", 1)
        clip = signal.resample(clip, max(1, round(len(clip) / pitch)))
        gain = db(row["gainDb"])
        if name in ("Addition", "Multiplier"):
            gain *= db(e.get("boost", 0))
        layers.append((e["t"], clip * gain))
    return compose(duration, layers)


def meter(path):
    result = subprocess.run([str(FFMPEG), "-hide_banner", "-nostats", "-i", str(path), "-af", "loudnorm=I=-24:TP=-1:LRA=20:dual_mono=true:print_format=json", "-f", "null", "-"], capture_output=True, text=True, check=True)
    value = json.loads(re.search(r'\{\s*"input_i".*?\}', result.stderr, re.S).group())
    return {"lufs": float(value["input_i"]), "truePeak": float(value["input_tp"])}


def preview():
    PREVIEW.mkdir(parents=True, exist_ok=True)
    timelines = {n: json.loads((ROOT / "Docs/Validation/AudioDirection" / ("Runtime" + n + ".json")).read_text(encoding="utf-8")) for n in ["Ordinary", "Strong", "Exceptional"]}
    ordinary = timelines["Ordinary"]
    # Native archive's ordinary clip is Perfect 20, so make its comparison audio agree.
    for event in ordinary["events"]:
        if event["kind"] == "Good":
            event["kind"] = "Perfect"; event["file"] = "Assets/Audio/Deepwater/Perfect_1.wav"; event["gain"] = db(-8)
    meters = {}; snapshots = {}
    for direction in ["current", "charged", "playful"]:
        # Preserve the rejected reference after the production bank changes to the selected direction.
        if direction == "current" and (PREVIEW / "ComparisonLevels.json").exists() and all((PREVIEW / f"current-{name}.wav").exists() for name in timelines):
            meters[direction] = json.loads((PREVIEW / "ComparisonLevels.json").read_text(encoding="utf-8"))["current"]
            continue
        bank = json.loads((ROOT / "Assets/Audio/Deepwater/manifest.json").read_text(encoding="utf-8"))["cues"] if direction == "current" else render_bank(direction, PREVIEW / direction)
        for row in bank:
            row["direction"] = direction
        rendered = {}
        for name, timeline in timelines.items():
            events = timeline["events"]
            if direction == "current" and name != "Ordinary":
                audio, _ = sf.read(ROOT / "Docs/Validation/AudioDirection" / f"Runtime{name}.wav")
            else:
                audio = mix_events(bank, events, timeline["duration"] + .5)
            rendered[name] = audio
            sf.write(PREVIEW / f"{direction}-{name}.wav", audio, RATE, subtype="PCM_16")
        measure = meter(PREVIEW / f"{direction}-Exceptional.wav")
        # One static trim per direction preserves ordinary-to-exceptional dynamics.
        trim = min(-24.8 - measure["lufs"], -1.2 - measure["truePeak"])
        meters[direction] = {"raw": measure, "comparisonTrimDb": trim, "final": {}}
        for name, audio in rendered.items():
            path = PREVIEW / f"{direction}-{name}.wav"
            sf.write(path, audio * db(trim), RATE, subtype="PCM_16")
            meters[direction]["final"][name] = meter(path)
        snapshots[direction] = bank
    (PREVIEW / "ComparisonLevels.json").write_text(json.dumps(meters, indent=2) + "\n", encoding="utf-8")
    (PREVIEW / "timelines.js").write_text("const soundTimelines=" + json.dumps(timelines) + ";\n", encoding="utf-8")
    print(json.dumps(meters, indent=2))


if __name__ == "__main__":
    preview()
