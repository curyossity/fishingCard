"""Render original review-only slot sequences. No game audio, prefab or runtime file is modified."""
from pathlib import Path
import array
import json
import math
import random
import wave

RATE = 44100
OUT = Path(__file__).resolve().parent
NOTES = (293.6648, 369.9944, 440.0)


def pluck(target, at, hz, gain, length=.16, warmth=1.0):
    """A short rounded tonal contact with independently damped upper partials."""
    start = round(at * RATE)
    for i in range(round(length * RATE)):
        if start + i >= len(target):
            break
        t = i / RATE
        attack = 1 - math.exp(-t / .0015)
        tail = min(1.0, (length - t) / .018)
        phase = 2 * math.pi * hz * (t + .00009 * (1 - math.exp(-t / .006)))
        value = math.sin(phase) * math.exp(-t / (.054 * warmth))
        value += .22 * math.sin(phase * 2.002) * math.exp(-t / .023)
        value += .055 * math.sin(phase * 4.014) * math.exp(-t / .011)
        target[start + i] += gain * value * attack * tail


def contact(target, at, gain, seed, length=.026):
    """A soft broad-band attack supporting a tonal contact without a metallic sample."""
    rng, low = random.Random(seed), 0.0
    start = round(at * RATE)
    for i in range(round(length * RATE)):
        if start + i >= len(target):
            break
        t = i / RATE
        low += .28 * (rng.uniform(-1, 1) - low)
        envelope = (1 - math.exp(-t / .0007)) * math.exp(-t / .0055)
        target[start + i] += gain * low * envelope


def travel(target, starts, ends):
    """A quiet air bed loses density as the wheels settle, leaving space for the result."""
    rng, smooth, slower = random.Random(1934), 0.0, 0.0
    for i in range(round(max(ends) * RATE)):
        t = i / RATE
        active = sum(begin <= t < end for begin, end in zip(starts, ends))
        if not active:
            continue
        smooth += .14 * (rng.uniform(-1, 1) - smooth)
        slower += .014 * (smooth - slower)
        envelope = min(1, t / .05) * min(1, (max(ends) - t) / .09)
        flutter = .82 + .18 * math.sin(2 * math.pi * (20 * t - 5 * t * t))
        target[i] += (smooth - slower) * .042 * active / 3 * envelope * flutter


def crossings(duration, tag):
    """Mirror the existing forward three-turn ease; the later spring is intentionally left quiet."""
    destination = 4 * math.pi + ((-tag * math.pi / 2) % (2 * math.pi))
    travel_time = duration * .66
    overshoot = .45 * math.pi / 2
    threshold = math.pi / 2
    result = []
    last = -1.0
    while threshold < destination + overshoot:
        fraction = threshold / (destination + overshoot)
        t = travel_time * (1 - (1 - fraction) ** (1 / 3))
        if t - last >= .065:
            result.append(t)
            last = t
        threshold += math.pi / 2
    return result


def render(name, result, locked=False):
    """Compose one complete launch/spin/settle/result phrase with no imported sound material."""
    target = [0.0] * round(2.3 * RATE)
    stops = [1.05, 1.20] if locked else [1.05, 1.20, 1.35]
    wheels = [0, 2] if locked else [0, 1, 2]
    event = []
    travel(target, [0] * len(stops), stops)
    pluck(target, 0, 146.8324, .26, .19, 1.15)
    contact(target, 0, .32, 11)
    event.append({'at': 0, 'event': 'launch'})
    for wheel, stop in zip(wheels, stops):
        for n, t in enumerate(crossings(stop, wheel)):
            contact(target, t, .105, wheel * 53 + n)
            pluck(target, t, 587.3295 + wheel * 8, .012, .035, .45)
        pluck(target, stop, NOTES[wheel], .23 + wheel * .025, .18)
        contact(target, stop, .19, wheel + 81)
        event.append({'at': stop, 'event': 'reel-stop', 'wheel': wheel})
    finish = max(stops) + .055
    if result == 'pair':
        for offset, hz, gain in ((0, 587.3295, .21), (.095, 880.0, .22)):
            pluck(target, finish + offset, hz, gain, .24, 1.05)
    elif result == 'triple':
        for n, hz in enumerate((440.0, 587.3295, 739.9888, 1174.659)):
            pluck(target, finish + n * .06, hz, .20 + n * .013, .26, 1.25)
        pluck(target, finish + .18, 293.6648, .13, .28, 1.25)
    if result != 'ordinary':
        event.append({'at': finish, 'event': result + '-accent'})
    # Common gain across all variations keeps the real result hierarchy intact.
    pcm = array.array('h', (round(math.tanh(v * 1.8) * .72 * 32767) for v in target))
    pcm[0] = pcm[-1] = 0
    with wave.open(str(OUT / (name + '.wav')), 'wb') as output:
        output.setnchannels(1)
        output.setsampwidth(2)
        output.setframerate(RATE)
        output.writeframes(pcm.tobytes())
    peak = max(abs(value) for value in pcm) / 32768
    rms = math.sqrt(sum((value / 32768) ** 2 for value in pcm) / len(pcm))
    assert 0 < peak < .90
    return {'file': name + '.wav', 'duration': len(pcm) / RATE,
            'samplePeakDbfs': round(20 * math.log10(peak), 2),
            'rmsDbfs': round(20 * math.log10(rms), 2), 'events': event}


if __name__ == '__main__':
    previews = [render('Ordinary', 'ordinary'), render('Pair', 'pair'),
                render('Triple', 'triple'), render('Locked-Respin', 'pair', True)]
    (OUT / 'Verification.json').write_text(json.dumps({
        'status': 'review-only; not installed; not auditioned by agent',
        'material': 'original procedural synthesis; no reference-game recordings',
        'previews': previews}, indent=2) + '\n', encoding='utf-8')
    print(json.dumps([{'file': item['file'], 'samplePeakDbfs': item['samplePeakDbfs']} for item in previews]))
