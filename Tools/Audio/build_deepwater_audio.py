"""Compatibility entry point for the selected bank, with the rejected acoustic recipe retained for archiving.

No network, credentials, oscillator patches or generative service. Rebuild with the
Earshot venv's Python; stem generation is documented separately in Sources.md.
"""
from __future__ import annotations

import hashlib
import json
import sys
from pathlib import Path

import numpy as np
from scipy import signal
import soundfile as sf

ROOT = Path(__file__).resolve().parents[2]
SOURCES = ROOT / "Assets/Editor/AudioSources/Deepwater"
OUTPUT = ROOT / "Assets/Audio/Deepwater"
REVIEW = ROOT / "Docs/Validation/AudioDirection"
RATE = 48000
MANIFEST = []
RECORDINGS = json.loads((SOURCES / "Recordings/sources.json").read_text(encoding="utf-8"))["recordings"]
CARDS = json.loads((SOURCES / "Recordings/cards.json").read_text(encoding="utf-8"))["recordings"]


def db(value):
    return 10 ** (value / 20)


def fade(audio, attack=0.0015, release=0.015):
    audio = np.array(audio, dtype=np.float64, copy=True)
    a, b = min(len(audio), int(attack * RATE)), min(len(audio), int(release * RATE))
    if a:
        audio[:a] *= np.sin(np.linspace(0, np.pi / 2, a)) ** 2
    if b:
        audio[-b:] *= np.sin(np.linspace(np.pi / 2, 0, b)) ** 2
    return audio


def stem(name, length=None, semitones=0, lowpass=6000, highpass=65, reverse=False):
    source_path = SOURCES / (name + ".wav")
    if name.startswith("card_") or name == "shuffle":
        source_path = SOURCES / "Recordings" / next(r["file"] for r in CARDS if r["voice"] == name)
    if name in ("harp", "pizzicato"):
        target = (62 if name == "harp" else 50) + semitones
        recording = min((r for r in RECORDINGS if r["voice"] == name), key=lambda r: abs(r["midi"] - target))
        semitones = target - recording["midi"]
        source_path = SOURCES / "Recordings" / recording["file"]
    audio, rate = sf.read(source_path, always_2d=True)
    audio = audio.mean(axis=1)
    audio = signal.resample_poly(audio, RATE, rate)
    # Remove leading silence without moving a recorded attack's natural peak.
    envelope = np.convolve(np.abs(audio), np.ones(48) / 48, mode="same")
    active = np.flatnonzero(envelope > max(0.0001, envelope.max() * 0.009))
    if name.startswith("card_"):
        # Keep the physical contact at the input frame, rather than a long pre-contact approach.
        active = np.flatnonzero(envelope > max(.0001, envelope.max() * .16))
    if len(active):
        audio = audio[max(0, active[0] - 96):]
    if semitones:
        audio = signal.resample(audio, max(1, round(len(audio) / 2 ** (semitones / 12))))
    audio = signal.sosfilt(signal.butter(2, highpass, "highpass", fs=RATE, output="sos"), audio)
    audio = signal.sosfilt(signal.butter(2, lowpass, "lowpass", fs=RATE, output="sos"), audio)
    if length:
        audio = audio[:round(length * RATE)]
    if reverse:
        audio = audio[::-1]
    audio = fade(audio)
    return audio / max(0.001, np.abs(audio).max()) * 0.7


def layer(name, length, semitones=0, gain=0, at=0, **kwargs):
    return at, stem(name, length, semitones, **kwargs) * db(gain)


def mix(length, layers):
    audio = np.zeros(round(length * RATE))
    for at, part in layers:
        start = max(0, round(at * RATE))
        size = min(len(part), len(audio) - start)
        if size > 0:
            audio[start:start + size] += part[:size]
    return fade(audio)


def cue(name, duration, layers, category, gain, priority=100, cooldown=0, variant=0, loop=False):
    audio = mix(duration, layers)
    audio *= min(1, 0.64 / max(0.001, np.abs(audio).max()))
    if loop:
        # Circular overlap removes the file seam; no zero-amplitude dip every lap.
        overlap = int(RATE * 0.035)
        window = np.linspace(0, 1, overlap)
        audio[:overlap] = audio[:overlap] * window + audio[-overlap:] * (1 - window)
        audio = audio[:-overlap]
        audio[-1] = audio[0]
    path = OUTPUT / f"{name}_{variant + 1}.wav"
    sf.write(path, audio, RATE, subtype="PCM_16")
    MANIFEST.append(dict(cue=name, file=path.relative_to(ROOT).as_posix(),
                         category=category, gainDb=gain, priority=priority,
                         cooldown=cooldown, duration=len(audio) / RATE,
                         sha256=hashlib.sha256(path.read_bytes()).hexdigest(), loop=loop))


def build():
    OUTPUT.mkdir(parents=True, exist_ok=True)
    REVIEW.mkdir(parents=True, exist_ok=True)
    for i in range(3):
        card, rim, shaker = f"card_{i + 1}", f"rim_{i + 1}", f"shaker_{i + 1}"
        cue("TagSelect", .15, [layer(card, .13, gain=-4), layer("pizzicato", .13, 12, -15)], "Interface", -13, 180, .07, i)
        cue("TagDeselect", .13, [layer(card, .10, -2, -6)], "Interface", -15, 180, .07, i)
        cue("Preview", .13, [layer(card, .12, gain=-5)], "Interface", -16, 180, .075, i)
        cue("SlotTick", .055, [layer(shaker, .05, lowpass=4200, highpass=900)], "Motion", -25, 220, .045, i)
        cue("Addition", .23, [layer("harp", .22, 0, -3), layer(rim, .04, gain=-13)], "Score", -10, 90, 0, i)
        cue("Multiplier", .32, [layer("harp", .31, 7, -3), layer("pizzicato", .25, 12, -5), layer("kick", .11, gain=-13), layer(card, .055, gain=-20)], "Score", -8, 65, 0, i)
        cue("Attach", .26, [layer(card, .17, gain=-4), layer(rim, .10, -4, -10), layer("pizzicato", .23, 0, -11)], "Score", -9, 70, 0, i)
    cue("SlotStart", .25, [layer("shuffle", .17, gain=-8), layer("pizzicato", .24, gain=-6), layer("kick", .1, gain=-10)], "Motion", -9, 110)
    roll = []
    for i in range(12):
        roll.append(layer(f"shaker_{i % 3 + 1}", .09, gain=-8 - (i % 2) * 4, at=i * .075, lowpass=2900, highpass=700))
    cue("SlotRoll", .935, roll, "Motion", -27, 240, loop=True)
    for i, note in enumerate([0, 5, 7]):
        cue(f"SlotStop{i + 1}", .20, [layer(f"rim_{i + 1}", .08, gain=-8), layer("harp", .19, note, -7), layer("kick", .09, gain=-18)], "Motion", -10, 100)
    cue("SlotLock", .16, [layer("rim_1", .07, gain=-9), layer("pizzicato", .15, 12, -7)], "Interface", -13, 160)
    cue("SlotUnlock", .15, [layer("card_2", .10, gain=-9), layer("pizzicato", .13, 7, -10)], "Interface", -15, 160)
    cue("Pair", .29, [layer("harp", .20, 7, -5), layer("harp", .20, 12, -6, .075)], "Score", -13, 80)
    cue("Triple", .55, [layer("pizzicato", .30, 0, -7), layer("harp", .28, 7, -7), layer("harp", .31, 12, -5, .09), layer("harp", .30, 19, -10, .20)], "Score", -9, 60)
    cue("Reveal", .30, [layer("card_3", .22, gain=-8), layer("harp", .28, -5, -10)], "Interface", -15, 160)
    cue("Strike", .10, [layer("rim_1", .06, -5, -9), layer("card_1", .08, gain=-14)], "Motion", -12, 100)
    cue("Good", .22, [layer("pizzicato", .20, 12, -3), layer("harp", .20, 0, -10)], "Score", -10, 80)
    cue("Perfect", .30, [layer("harp", .28, 12, -4), layer("pizzicato", .24, 12, -5), layer("harp", .23, 7, -14)], "Score", -8, 65)
    cue("Miss", .18, [layer("card_3", .14, -7, -7), layer("pizzicato", .12, -5, -15, lowpass=800)], "Motion", -15, 120)
    cue("Travel", .12, [layer("harp", .12, 7, -9, reverse=True, lowpass=2200), layer("card_1", .07, gain=-22)], "Motion", -22, 200)
    cue("PayoutSmall", .24, [layer("pizzicato", .23, 0, -5), layer("harp", .22, 0, -8)], "Score", -12, 60)
    cue("PayoutMedium", .39, [layer("pizzicato", .30, 0, -6), layer("harp", .28, 7, -6), layer("harp", .25, 12, -8, .08)], "Score", -9, 45)
    cue("PayoutBig", .65, [layer("kick", .20, gain=-13), layer("pizzicato", .37, 0, -7), layer("harp", .48, 0, -9), layer("harp", .38, 7, -8, .045), layer("harp", .40, 12, -6, .11), layer("harp", .35, 19, -12, .23)], "Score", -6, 35)
    cue("Failure", .52, [layer("pizzicato", .36, -5, -6), layer("card_2", .22, -5, -12), layer("harp", .30, -12, -15, .10)], "Score", -14, 50)
    cue("BuildTravel", .24, [layer("card_1", .23, gain=-9)], "Motion", -19, 200)
    cue("Upgrade", .65, [layer("card_2", .12, gain=-13), layer("pizzicato", .28, 12, -7), layer("harp", .30, 7, -9), layer("harp", .30, 12, -7, .11), layer("harp", .29, 19, -10, .23)], "Score", -9, 50)
    (OUTPUT / "manifest.json").write_text(json.dumps(dict(version=1, sampleRate=RATE, cues=MANIFEST), indent=2) + "\n", encoding="utf-8")
    # An audition is a complete interaction, rather than an isolated success beep.
    audition()
    print(json.dumps(dict(files=len(MANIFEST), cues=len({m['cue'] for m in MANIFEST}), megabytes=round(sum((ROOT / m['file']).stat().st_size for m in MANIFEST) / 1e6, 2))))


def audition():
    def render(name, events, duration):
        audio = np.zeros((round(duration * RATE), 2))
        frozen = []
        for at, name_, pitch, gain, pan in events:
            meta = next(x for x in MANIFEST if x["cue"] == name_)
            clip, _ = sf.read(ROOT / meta["file"])
            if name_ == "SlotRoll":
                frames = round((1.35 - at) * RATE)
                clip = fade(np.tile(clip, int(np.ceil(frames / len(clip))))[:frames], .04, .04)
            if pitch != 1:
                clip = signal.resample(clip, round(len(clip) / pitch))
            clip *= db(meta["gainDb"] + gain)
            start, size = round(at * RATE), min(len(clip), len(audio) - round(at * RATE))
            stereo = np.column_stack((clip * np.cos((pan + 1) * np.pi / 4), clip * np.sin((pan + 1) * np.pi / 4)))
            audio[start:start + size] += stereo[:size]
            frozen.append(dict(time=at, cue=name_, pitch=pitch, gainDb=meta["gainDb"] + gain, pan=pan))
        sf.write(REVIEW / (name + ".wav"), audio, RATE, subtype="PCM_16")
        (REVIEW / (name + ".json")).write_text(json.dumps(dict(note="Authored sound storyboard; actual runtime schedules are exported separately by Unity checks.", events=frozen), indent=2) + "\n", encoding="utf-8")

    launch = [(0, "SlotStart", 1, 0, 0), (.02, "SlotRoll", 1, 0, 0)]
    launch += [(i * .14 + .12, "SlotTick", 1, 0, (-.12 if i % 2 else .12)) for i in range(5)]
    launch += [(t, f"SlotStop{i + 1}", 1, 0, (i - 1) * .16) for i, t in enumerate([1.05, 1.20, 1.35])]
    render("DiscoverTriple", launch + [(1.44, "Triple", 1, 0, 0)], 2.1)
    render("DiscoverOrdinary", launch, 1.95)
    e = [(0, "Strike", 1, 0, 0), (.076, "Perfect", 1, 0, 0)]
    for i in range(3):
        e += [(.18 + i * .29 + .29 * .3, "Travel", 1, 0, -.1), (.18 + i * .29 + .29 * .76, "Addition", 2 ** ([0, 3, 5][i] / 12), 0, 0)]
    for i in range(3):
        e += [(1.05 + i * .37 + .37 * .3, "Travel", 1, 0, -.1), (1.05 + i * .37 + .37 * .76, "Multiplier", 2 ** ([7, 10, 12][i] / 12), 0, 0)]
    e += [(2.45, "PayoutBig", 1, 0, 0)]
    render("GrowingRig", e, 3.2)
    render("BuildAndUpgrade", [(0, "Preview", 1, 0, 0), (.4, "Preview", 1, 0, 0), (.85, "BuildTravel", 1, 0, 0), (1.17, "Attach", 1, 0, 0), (2, "Upgrade", 1, 0, 0)], 2.8)


if __name__ == "__main__":
    if "--archive-acoustic" in sys.argv:
        OUTPUT = ROOT / "Docs/Validation/AudioDirection/ArchivedBank"
        REVIEW = ROOT / "Temp/ArchivedAcousticStoryboards"
        build()
    else:
        from build_charged_audio import main
        main()
