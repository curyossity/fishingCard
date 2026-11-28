"""Round 2 of the Landing sound revision: health-drain and rig-ram alternatives, auditioned in context.

Mixes the installed runtime takes (Assets/Audio/Deepwater at their manifest gains) with the round-2 candidates in
Docs/Audio/landing-revision/round2 on the game's real strike schedule (schedule.json). Candidates are level-matched to the
cue they would replace, so the comparison is about timbre, not loudness. The health drain starts GAP seconds after the
slam contact so it no longer overlaps the slam. Writes mono 48 kHz WAVs and a listening page; nothing under Assets changes.

Run: .local-tools/earshot/venv/Scripts/python.exe -I Tools/Audio/render_landing_round2.py
"""
import sys
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parent))
import render_landing_revision as base  # noqa: E402

RATE = base.RATE
ROUND = base.OUT / "round2"
PHRASES = ROUND / "phrases"
GAP = .2  # seconds between the slam contact and the start of the Resistance drain (sound and bar)
OPTIONS = (1, 2, 3)


def peak_index(audio):
    return int(np.abs(audio).argmax())


def hit_level(audio):
    """RMS (dB) of the contact: 20 ms before the loudest moment to 100 ms after it."""
    p = peak_index(audio)
    window = audio[max(0, p - int(.02 * RATE)): p + int(.1 * RATE)]
    return 20 * np.log10(np.sqrt(np.mean(window ** 2)) + 1e-12)


def body_level(audio):
    """RMS (dB) over the audible part (within 40 dB of the peak)."""
    loud = np.abs(audio) > np.abs(audio).max() * .01
    idx = np.nonzero(loud)[0]
    window = audio[idx[0]: idx[-1] + 1]
    return 20 * np.log10(np.sqrt(np.mean(window ** 2)) + 1e-12)


def matched(path, reference, measure):
    """Loads a candidate scaled so its measured level equals the installed reference's in-game level."""
    audio = base.mono(path)
    return audio * base.db(measure(reference) - measure(audio))


def ram(kind, option):
    return matched(ROUND / f"{kind}_{option}.wav", base.take(kind), hit_level)


def drain(option):
    return matched(ROUND / f"HealthDrain_{option}.wav", base.take("HealthDrain"), body_level)


def strike(ram_option=None, drain_option=None, gap=GAP, with_drain=True):
    """A Perfect strike with three rig rams, the final slam and the health drain."""
    steps, payout = base.schedule["steps"], base.schedule["payout"]
    mix = base.Mix(base.schedule["duration"] + gap + .4)
    mix.add(base.take("Strike"), 0)
    mix.add(base.take("Perfect"), steps[0]["impact"])
    for i, step in enumerate(steps[1:]):
        mix.add(base.take("Travel", i), step["launch"])
        kind = "Multiplier" if step["multiply"] else "Addition"
        pitch = 1 + i * .04
        if ram_option is None:
            mix.add(base.take(kind, i), step["impact"], pitch)
        else:
            audio = ram(kind, ram_option)
            mix.add(audio, step["impact"] - peak_index(audio) / RATE / pitch, pitch)
    tier = ["PayoutSmall", "PayoutMedium", "PayoutBig"][base.schedule["tier"]]
    mix.add(base.take(tier), payout)
    slam = base.take("SlamImpact")
    mix.add(slam, payout - peak_index(slam) / RATE)
    if with_drain:
        audio = base.take("HealthDrain") if drain_option is None else drain(drain_option)
        mix.add(audio, payout + gap)
    return mix


def write(mix, name):
    peak = np.abs(mix.buffer).max()
    if peak > .98:
        mix.buffer *= .98 / peak
    base.sf.write(PHRASES / name, mix.buffer, RATE, subtype="PCM_16")


def main():
    PHRASES.mkdir(parents=True, exist_ok=True)
    write(strike(gap=.03), "Strike-Now.wav")
    for option in OPTIONS:
        write(strike(drain_option=option), f"Drain-{option}.wav")
        write(strike(ram_option=option, with_drain=False), f"Ram-{option}.wav")
    write(strike(with_drain=False), "Ram-Now.wav")
    print("phrases written to", PHRASES)


if __name__ == "__main__":
    main()
