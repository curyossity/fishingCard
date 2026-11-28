"""Renders in-context audition phrases for the Landing sound revision (Docs/Audio/landing-revision).

Each phrase mixes the approved Charged Cabinet runtime takes (Assets/Audio/Deepwater, at their manifest gains) with one candidate
set (A or B) for the new cues, on the game's real schedule (schedule.json exported from LandingScoreSequence). New cues are placed
so their measured peak meets the visible contact. Writes mono 48 kHz WAVs and a listening page; nothing under Assets changes.

Run: .local-tools/earshot/venv/Scripts/python.exe -I Tools/Audio/render_landing_revision.py
"""
import json
from pathlib import Path

import numpy as np
import soundfile as sf

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "Docs/Audio/landing-revision"
AUDITION = OUT / "audition"
RATE = 48000
# Proposed starting gains (dB) for the new cues; the slam sits under the tiered payout, the drain well beneath everything.
NEW_GAIN = {"ShutterClose": -13, "ShutterOpen": -17, "Stamp": -11, "Landed": -8, "SlamImpact": -9, "HealthDrain": -21}
manifest = json.loads((ROOT / "Assets/Audio/Deepwater/manifest.json").read_text(encoding="utf-8"))
schedule = json.loads((OUT / "schedule.json").read_text(encoding="utf-8"))


def db(value):
    return 10 ** (value / 20)


def mono(path):
    audio, rate = sf.read(path)
    if audio.ndim > 1:
        audio = audio.mean(axis=1)
    assert rate == RATE, path
    return audio


def take(cue, index=0):
    rows = [row for row in manifest["cues"] if row["cue"] == cue]
    row = rows[index % len(rows)]
    return mono(ROOT / row["file"]) * db(row["gainDb"])


def candidate(cue, variant):
    audio = mono(AUDITION / f"{cue}_{variant}.wav")
    return audio * db(NEW_GAIN[cue]), int(np.abs(audio).argmax())


class Mix:
    def __init__(self, seconds):
        self.buffer = np.zeros(int(seconds * RATE))

    def add(self, audio, at, pitch=1.0):
        if pitch != 1.0:
            positions = np.arange(0, len(audio) - 1, pitch)
            audio = np.interp(positions, np.arange(len(audio)), audio)
        start = int(round(at * RATE))
        if start < 0:
            audio, start = audio[-start:], 0
        end = min(len(self.buffer), start + len(audio))
        self.buffer[start:end] += audio[: end - start]

    def hit(self, cue, variant, at):
        """Places a candidate so its loudest moment meets the visible contact at `at`."""
        audio, peak = candidate(cue, variant)
        self.add(audio, at - peak / RATE)

    def write(self, name):
        peak = np.abs(self.buffer).max()
        if peak > .98:
            self.buffer *= .98 / peak
        sf.write(OUT / "phrases" / name, self.buffer, RATE, subtype="PCM_16")


def strike(variant, with_new):
    """A Perfect strike with three rig contributors, the final slam and the health drain."""
    steps, payout = schedule["steps"], schedule["payout"]
    mix = Mix(schedule["duration"] + .4)
    mix.add(take("Strike"), 0)
    mix.add(take("Perfect"), steps[0]["impact"])
    for i, step in enumerate(steps[1:]):
        mix.add(take("Travel", i), step["launch"])
        cue = "Multiplier" if step["multiply"] else "Addition"
        mix.add(take(cue, i), step["impact"], 1 + i * .04)
    tier = ["PayoutSmall", "PayoutMedium", "PayoutBig"][schedule["tier"]]
    mix.add(take(tier), payout)
    if with_new:
        mix.hit("SlamImpact", variant, payout)
        mix.add(candidate("HealthDrain", variant)[0], payout + .03)
    return mix


def landed(variant):
    """LANDED: yank (approved Travel) at the dip's end, the lock-in at the slam, the stamp on landing."""
    mix = Mix(1.2)
    mix.add(take("Travel"), .08, 1.1)
    mix.hit("Landed", variant, .38)
    mix.hit("Stamp", variant, .38)
    return mix


def escaped(variant, cards=4):
    """ESCAPED: approved Failure as the creature sinks, the stamp, then the haul cash-out and HAUL PAID."""
    mix = Mix(3.6)
    mix.add(take("Failure"), 0)
    mix.hit("Stamp", variant, .66)
    step = min(.22, max(.03, 1.6 / cards))
    for i in range(cards):
        mix.add(take("Preview", i), 1.5 + i * step, min(1.5, 1 + i * .04))
    mix.add(take("PayoutSmall"), 1.5 + cards * step, .94)
    return mix


def shutter(variant):
    """Phase change: close (landing at 0.18 s), then the roll back up from 0.26 s."""
    mix = Mix(1.0)
    mix.hit("ShutterClose", variant, .18)
    open_audio, _ = candidate("ShutterOpen", variant)
    mix.add(open_audio, .26)
    return mix


def reveal(variant):
    """Rare entrance: approved Reveal pitched for Rare, then the banner stamp as the face lands."""
    mix = Mix(1.4)
    settle = .22 + .46
    mix.add(take("Reveal"), settle, 1.12)
    mix.hit("Stamp", variant, settle)
    return mix


def main():
    (OUT / "phrases").mkdir(parents=True, exist_ok=True)
    strike("A", False).write("Strike-Current.wav")
    for variant in "AB":
        strike(variant, True).write(f"Strike-{variant}.wav")
        landed(variant).write(f"Landed-{variant}.wav")
        escaped(variant).write(f"Escaped-{variant}.wav")
        shutter(variant).write(f"Shutter-{variant}.wav")
        reveal(variant).write(f"Reveal-{variant}.wav")
    print("phrases written to", OUT / "phrases")


if __name__ == "__main__":
    main()
