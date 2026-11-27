"""Draws the Landing combat card frames as true pixel art on a native grid, then scales them 2x with nearest neighbour.

Outputs (transparent PNG, text and numbers stay native in Unity):
  creature-card.png   - "specimen plate": parchment clamped in a rust-iron holder with brass brackets, a name tab and a
                        riveted Resistance gauge (dark glass trough; the red fill is a native Unity image).
  strike-panel.png    - "strike gauge": teal machine-steel housing, brass bezel and header tab, dark display window and a
                        recessed label slot.
  hook-icon.png       - brass hook glyph for the ready state of the strike panel.
  bar-fill.png        - Resistance fill segment (9-slice), red with a highlight row.
  layout.json         - native-pixel rectangles of every live region, for the Unity setup.

Palette ramps are sampled from the existing painted assets (portrait-frame.png, score-readout.png, progress-trough.png).
Light comes from the top left. Run with the local venv: .local-tools/earshot/venv/Scripts/python.exe -I Tools/Art/make_combat_cards.py <out_dir>
"""
import json
import sys
from pathlib import Path

import numpy as np
from PIL import Image

SCALE = 2
RNG = np.random.default_rng(1127)

def hexes(*values):
    return [tuple(int(v[i:i + 2], 16) for i in (1, 3, 5)) for v in values]

# Ramps run dark -> light.
PARCHMENT = hexes("#614C38", "#B48D67", "#DBB082", "#E6C094", "#EAC9A0", "#EDCBA4", "#F3D8B0")
BRASS = hexes("#3A2A16", "#5D4627", "#9C6A2D", "#C9A358", "#DDB969", "#F1D98C")
RUST = hexes("#1E0E08", "#3A1A10", "#5A2A1A", "#7A3A24", "#94482C", "#B05D3E")
STEEL = hexes("#0E1715", "#1B2B28", "#263A36", "#36504A", "#4E6B63", "#6F8C83")
INK = hexes("#060807", "#0A0C0B", "#141310", "#1B1A17", "#24211D", "#2A2620", "#332E26")
OUTLINE = (20, 12, 8)


class Canvas:
    def __init__(self, width, height):
        self.w, self.h = width, height
        self.rgba = np.zeros((height, width, 4), np.uint8)
        self.regions = {}

    def put(self, mask, ramp, level, jitter=0.0, alpha=255):
        """Fills a mask with one ramp, offset per pixel by level (0..len-1 float) plus clustered noise."""
        if not mask.any():
            return
        noise = cluster_noise(self.w, self.h) * jitter
        lv = np.clip(np.rint((level if np.ndim(level) else np.full((self.h, self.w), level)) + noise), 0, len(ramp) - 1).astype(int)
        colors = np.array(ramp, np.uint8)[lv]
        self.rgba[mask, :3] = colors[mask]
        self.rgba[mask, 3] = alpha

    def color(self, mask, rgb, alpha=255):
        self.rgba[mask, :3] = rgb
        self.rgba[mask, 3] = alpha

    def save(self, path):
        img = Image.fromarray(self.rgba, "RGBA")
        img.resize((self.w * SCALE, self.h * SCALE), Image.NEAREST).save(path)


def cluster_noise(w, h):
    """Blocky value noise: 2x2 clusters read as hand-placed pixels rather than per-pixel static."""
    base = RNG.normal(0, 1, ((h + 1) // 2, (w + 1) // 2))
    return np.kron(base, np.ones((2, 2)))[:h, :w] * 0.45 + RNG.normal(0, 1, (h, w)) * 0.2


def octagon(w, h, x0, y0, x1, y1, cut):
    """Mask of a rectangle [x0,x1) x [y0,y1) with 45-degree chamfered corners."""
    yy, xx = np.mgrid[0:h, 0:w]
    inside = (xx >= x0) & (xx < x1) & (yy >= y0) & (yy < y1)
    dx = np.minimum(xx - x0, x1 - 1 - xx)
    dy = np.minimum(yy - y0, y1 - 1 - yy)
    return inside & (dx + dy >= cut)


def rect(w, h, x0, y0, x1, y1):
    yy, xx = np.mgrid[0:h, 0:w]
    return (xx >= x0) & (xx < x1) & (yy >= y0) & (yy < y1)


def edges(mask):
    """Top-left lit edge, bottom-right shaded edge and the outer outline ring of a mask."""
    up = np.zeros_like(mask); up[1:] = mask[:-1]
    down = np.zeros_like(mask); down[:-1] = mask[1:]
    left = np.zeros_like(mask); left[:, 1:] = mask[:, :-1]
    right = np.zeros_like(mask); right[:, :-1] = mask[:, 1:]
    top_lit = mask & (~up | ~left)
    bottom_dark = mask & (~down | ~right)
    grown = mask | up | down | left | right
    return top_lit & ~bottom_dark, bottom_dark, grown & ~mask


def shrink(mask, n=1):
    for _ in range(n):
        m = mask.copy()
        m[1:] &= mask[:-1]; m[:-1] &= mask[1:]; m[:, 1:] &= mask[:, :-1]; m[:, :-1] &= mask[:, 1:]
        mask = m
    return mask


def bevel_band(c, outer, inner, ramp, base, jitter=0.6):
    """A frame band between two masks: outline, two-step top-left light, two-step bottom-right shade, and a dark inner lip."""
    band = outer & ~inner
    c.put(band, ramp, base, jitter)
    lit, dark, ring = edges(outer)
    top = len(ramp) - 1
    c.put(lit & band, ramp, top, 0.1)
    lit2, dark2, _ = edges(shrink(outer))
    c.put(lit2 & band, ramp, min(base + 1, top), 0.2)
    c.put(dark & band, ramp, max(base - 2, 0), 0.1)
    c.put(dark2 & band, ramp, max(base - 1, 0), 0.2)
    c.color(ring, OUTLINE)
    # Inner lip: the band steps down into whatever it holds.
    _, _, iring = edges(inner)
    c.put(iring & band, ramp, 0, 0)
    _, _, iring2 = edges(inner | iring)
    c.put(iring2 & band, ramp, max(base - 1, 0), 0.1)


def rivet(c, cx, cy, ramp=BRASS, r=1.6):
    yy, xx = np.mgrid[0:c.h, 0:c.w]
    d = np.hypot(xx - cx, yy - cy)
    c.color(d < r + 1.0, OUTLINE)
    c.put(d < r + .2, ramp, 3, 0)
    c.put((d < r * .7) & (xx < cx) & (yy < cy), ramp, 5, 0)
    c.put((d < r + .2) & (xx > cx) & (yy > cy), ramp, 1, 0)


def plate(c, x0, y0, x1, y1, ramp, base, cut=1, inset_ramp=INK, inset=True, pad=2):
    """A small raised plate with a dark inset slot for native text."""
    outer = octagon(c.w, c.h, x0, y0, x1, y1, cut)
    c.color(edges(outer)[2], OUTLINE)
    c.put(outer, ramp, base, 0.4)
    lit, dark, _ = edges(outer)
    c.put(lit, ramp, min(base + 2, len(ramp) - 1), 0)
    c.put(dark, ramp, max(base - 2, 0), 0)
    if inset:
        slot = octagon(c.w, c.h, x0 + pad, y0 + 2, x1 - pad, y1 - 2, 1)
        c.put(slot, inset_ramp, 3, 0.5)
        l2, d2, _ = edges(slot)
        c.put(l2, inset_ramp, 1, 0)
        return (x0 + pad, y0 + 2, x1 - pad, y1 - 2)
    return (x0, y0, x1, y1)


def wear(c, mask, ramp, count, darker=True, size=(1, 3)):
    """Scattered scuffs: short horizontal clusters a shade off the local surface."""
    ys, xs = np.nonzero(mask)
    if len(xs) == 0:
        return
    for i in RNG.choice(len(xs), size=count, replace=False):
        length = RNG.integers(size[0], size[1] + 1)
        for k in range(length):
            x, y = xs[i] + k, ys[i]
            if 0 <= x < c.w and mask[y, x]:
                current = np.array(c.rgba[y, x, :3], int)
                idx = int(np.argmin([np.abs(np.array(r) - current).sum() for r in ramp]))
                c.rgba[y, x, :3] = ramp[max(0, idx - 1) if darker else min(len(ramp) - 1, idx + 1)]


def bracket(c, x, y, sx, sy, arm=9):
    """Brass L clamp at a parchment corner; sx/sy point the arms into the card."""
    mask = np.zeros((c.h, c.w), bool)
    for t in range(arm):
        for th in range(3):
            for px, py in ((x + sx * t, y + sy * th), (x + sx * th, y + sy * t)):
                if 0 <= px < c.w and 0 <= py < c.h:
                    mask[py, px] = True
    c.color(edges(mask)[2], OUTLINE)
    c.put(mask, BRASS, 3, 0.3)
    lit, dark, _ = edges(mask)
    c.put(lit, BRASS, 5, 0)
    c.put(dark, BRASS, 1, 0)
    rivet(c, x + sx * 1 + .5, y + sy * 1 + .5, r=1.1)


def creature_card(out):
    W, H = 214, 116
    c = Canvas(W, H)
    holder = octagon(W, H, 0, 4, W, H, 8)
    window = octagon(W, H, 8, 12, W - 8, 86, 4)
    gauge = rect(W, H, 8, 90, W - 8, H - 8)
    # Rust-iron holder: the opponent is clamped in darker, harder metal than the player's machine.
    bevel_band(c, holder, window, RUST, 2.6, 0.7)
    wear(c, holder & ~window, RUST, 110, darker=False)
    wear(c, holder & ~window, RUST, 50, darker=True)
    # Riveted divider between the specimen and its gauge.
    c.put(rect(W, H, 8, 86, W - 8, 87), RUST, 0, 0)
    c.put(rect(W, H, 8, 87, W - 8, 88), RUST, 5, 0.2)
    # Parchment specimen, recessed under the holder: darker under the top frame, edges aged, a few stains.
    yy, xx = np.mgrid[0:H, 0:W]
    edge = np.minimum.reduce([xx - 8, W - 9 - xx, yy - 12, 85 - yy]).astype(float)
    shade = 5.0 - np.clip(4 - edge, 0, 4) * 0.55 - np.clip(3 - (yy - 12), 0, 3) * 0.5
    for sx, sy, sr in ((34, 72, 9), (178, 24, 7), (150, 76, 6)):
        shade -= np.clip(1 - np.hypot(xx - sx, (yy - sy) * 1.4) / sr, 0, 1) * 0.9
    c.put(window, PARCHMENT, shade, 0.8)
    wear(c, window, PARCHMENT, 70, darker=True, size=(1, 2))
    wear(c, window, PARCHMENT, 30, darker=False, size=(1, 3))
    # Resistance gauge: brass-rimmed dark glass trough with engraved quarter notches.
    trough = octagon(W, H, 18, 95, W - 18, 107, 2)
    rim = octagon(W, H, 16, 93, W - 16, 109, 3)
    c.color(edges(rim)[2], OUTLINE)
    c.put(rim & ~trough, BRASS, 3, 0.3)
    rl, rd, _ = edges(rim)
    c.put(rl & ~trough, BRASS, 5, 0); c.put(rd & ~trough, BRASS, 1, 0)
    c.put(edges(trough)[2] & rim, BRASS, 0, 0)
    c.put(trough, INK, 1.3, 0.4)
    c.put(rect(W, H, 19, 96, W - 19, 97) & trough, INK, 0, 0)
    c.put(rect(W, H, 19, 105, W - 19, 106) & trough, INK, 4, 0)
    for q in (0.25, 0.5, 0.75):
        x = int(round(18 + (W - 36) * q))
        c.put(rect(W, H, x, 93, x + 1, 95), BRASS, 0, 0)
        c.put(rect(W, H, x, 107, x + 1, 109), BRASS, 0, 0)
    rivet(c, 11.5, 101); rivet(c, W - 11.5, 101)
    for y in (30, 60):
        rivet(c, 4, y, ramp=BRASS, r=1.2); rivet(c, W - 4, y, ramp=BRASS, r=1.2)
    # Brass corner clamps hold the specimen.
    for x, y, sx, sy in ((8, 12, 1, 1), (W - 9, 12, -1, 1), (8, 85, 1, -1), (W - 9, 85, -1, -1)):
        bracket(c, x, y, sx, sy, arm=12)
    # Name tab riding on the top edge, for the creature's tag.
    tag = plate(c, 64, 0, W - 64, 14, BRASS, 3, cut=3, pad=9)
    rivet(c, 68.5, 7, r=1.1); rivet(c, W - 68.5, 7, r=1.1)
    c.regions = {"portrait": [14, 16, W - 14, 84], "tag": list(tag), "trough": [19, 96, W - 19, 106]}
    c.save(out / "creature-card.png")
    return {"size": [W, H], "regions": c.regions}


def strike_panel(out):
    W, H = 226, 110
    c = Canvas(W, H)
    housing = octagon(W, H, 0, 4, W, H, 8)
    inner = octagon(W, H, 7, 11, W - 7, H - 7, 5)
    display = octagon(W, H, 11, 15, W - 11, 80, 3)
    # Teal machine-steel housing: the player's own apparatus, the same metal as the cabinet.
    bevel_band(c, housing, inner, STEEL, 3, 0.8)
    wear(c, housing & ~inner, STEEL, 100, darker=False)
    wear(c, housing & ~inner, STEEL, 40, darker=True)
    # Brass faceplate around the display and label slot.
    c.put(inner, BRASS, 2.7, 0.3)
    fl, fd, _ = edges(inner)
    c.put(fl, BRASS, 5, 0); c.put(fd, BRASS, 1, 0)
    wear(c, inner & ~display, BRASS, 22, darker=True)
    # Dark display window: soft vignette, a glass sheen along the top, stepped bezel shadow.
    yy, xx = np.mgrid[0:H, 0:W]
    vignette = 5.4 - np.clip(np.hypot((xx - W / 2) / 104, (yy - 47) / 38) - .3, 0, 1) * 3.4
    c.put(display, INK, vignette, 0.5)
    c.put(display & (yy == 16), INK, 0, 0)
    c.put(display & (yy == 18) & (xx > 22) & (xx < 78), INK, 6, 0.2)
    c.put(display & (yy == 19) & (xx > 26) & (xx < 60), INK, 6, 0.2)
    c.color(edges(display)[2] & ~display, OUTLINE)
    # Recessed label slot.
    slot = octagon(W, H, 20, 86, W - 20, 99, 2)
    c.color(edges(slot)[2], OUTLINE)
    c.put(slot, INK, 3, 0.4)
    c.put(edges(slot)[0], INK, 1, 0)
    rivet(c, 13.5, 92.5); rivet(c, W - 13.5, 92.5)
    for y in (34, 64):
        rivet(c, 3.5, y, ramp=BRASS, r=1.2); rivet(c, W - 3.5, y, ramp=BRASS, r=1.2)
    # Header tab for the caption.
    tab = plate(c, 64, 0, W - 64, 14, BRASS, 3, cut=3, pad=9)
    rivet(c, 68.5, 7, r=1.1); rivet(c, W - 68.5, 7, r=1.1)
    c.regions = {"display": [13, 19, W - 13, 78], "caption": list(tab), "label": [22, 88, W - 22, 97]}
    c.save(out / "strike-panel.png")
    return {"size": [W, H], "regions": c.regions}


def hook_icon(out):
    W, H = 16, 26
    c = Canvas(W, H)
    pts = set()
    yy, xx = np.mgrid[0:H, 0:W]
    eye = (np.hypot(xx - 10, yy - 3.2) < 2.8) & (np.hypot(xx - 10, yy - 3.2) > 1.2)
    shank = rect(W, H, 9, 6, 11, 18)
    bend = (np.hypot(xx - 6, yy - 18) < 5.0) & (np.hypot(xx - 6, yy - 18) > 2.8) & (yy >= 18)
    point = rect(W, H, 1, 13, 3, 19)
    barb = (xx - 1 == (yy - 13) // 1) & (yy >= 13) & (yy <= 15) & (xx <= 4)
    mask = eye | shank | bend | point | barb
    c.color(edges(mask)[2], OUTLINE)
    c.put(mask, BRASS, 3, 0.2)
    lit, dark, _ = edges(mask)
    c.put(lit, BRASS, 5, 0); c.put(dark, BRASS, 1, 0)
    c.save(out / "hook-icon.png")
    return {"size": [W, H]}


def bar_fill(out):
    W, H = 6, 10
    c = Canvas(W, H)
    red = hexes("#4A140A", "#7C2414", "#A8371F", "#C8482C", "#E06A4A", "#F29A7E")
    for y in range(H):
        level = [5, 4, 3, 3, 3, 2, 2, 2, 1, 0][y]
        c.put(rect(W, H, 0, y, W, y + 1), red, level, 0)
    c.save(out / "bar-fill.png")
    return {"size": [W, H], "slice": [1, 1, 1, 1]}


def main():
    out = Path(sys.argv[1] if len(sys.argv) > 1 else "Temp/CombatCards")
    out.mkdir(parents=True, exist_ok=True)
    layout = {"scale": SCALE, "creature-card": creature_card(out), "strike-panel": strike_panel(out),
              "hook-icon": hook_icon(out), "bar-fill": bar_fill(out)}
    (out / "layout.json").write_text(json.dumps(layout, indent=2))
    print(json.dumps(layout))


if __name__ == "__main__":
    main()
