"""Erases the two original painted Landing panels from landing-machine.png, leaving the machine's own empty interior recess.

The combat cards (Strike gauge and specimen plate) are separate moving layers, so the machine underneath must show its recess, not
the old cards or a flat placeholder block. The panels sit inside a dark recess channel (~25-30 px wide) whose pixels next to the
panels are darkened by the old panels' drop shadows. The fill rectangle therefore swallows those shadows, and its edge colours are
sampled from the clean middle of the channel. A Coons patch blends those border profiles inward in both directions (keeping the
recess's own shading: darker under the top bevel), and quiet 2x2-clustered grain matches the surface. Everything outside the fill
rectangle, including every alpha value, is copied byte-for-byte.

Usage: .local-tools/earshot/venv/Scripts/python.exe -I Tools/Art/make_empty_landing_machine.py <source.png> <output.png>
"""
import sys

import numpy as np
from PIL import Image

# Fill rectangle in source pixels (x0, y0, x1, y1) and the clean channel bands sampled for each edge.
X0, Y0, X1, Y1 = 205, 180, 1530, 517
LEFT, RIGHT, TOP, BOTTOM = (196, 204), (1530, 1538), (175, 180), (517, 522)
CUT = 26  # chamfer of the recess window's corners, in source pixels
RNG = np.random.default_rng(7)


def smooth(values, radius):
    """Box blur along axis 0 of an (n, 3) profile."""
    kernel = np.ones(radius * 2 + 1) / (radius * 2 + 1)
    pad = np.pad(values, ((radius, radius), (0, 0)), mode="edge")
    return np.stack([np.convolve(pad[:, c], kernel, mode="valid") for c in range(values.shape[1])], axis=1)


def main(source, output):
    img = np.array(Image.open(source).convert("RGBA")).astype(float)
    rgb = img[..., :3]
    h, w = Y1 - Y0, X1 - X0
    top = smooth(np.median(rgb[TOP[0]:TOP[1], X0:X1], axis=0), 8)
    bottom = smooth(np.median(rgb[BOTTOM[0]:BOTTOM[1], X0:X1], axis=0), 8)
    # Side profiles skip the rows beside the recess's chamfered corners (bright bevel pixels) and hold their nearest clean value.
    rows = np.clip(np.arange(Y0, Y1), Y0 + 24, Y1 - 14)
    left = smooth(np.median(rgb[rows][:, LEFT[0]:LEFT[1]], axis=1), 8)
    right = smooth(np.median(rgb[rows][:, RIGHT[0]:RIGHT[1]], axis=1), 8)
    u = np.linspace(0, 1, w)[None, :, None]
    v = np.linspace(0, 1, h)[:, None, None]
    c00, c10, c01, c11 = top[0], top[-1], bottom[0], bottom[-1]
    # Coons patch: ruled surfaces from both directions minus the bilinear corner surface.
    base = ((1 - v) * top[None] + v * bottom[None]) + ((1 - u) * left[:, None] + u * right[:, None]) \
        - ((1 - u) * (1 - v) * c00 + u * (1 - v) * c10 + (1 - u) * v * c01 + u * v * c11)
    # Quiet grain in 2x2 clusters, the same scale as the cabinet's painted texture.
    noise = np.kron(RNG.normal(0, 1, ((h + 1) // 2, (w + 1) // 2)), np.ones((2, 2)))[:h, :w] * 1.6
    # The recess window has chamfered corners; the fill follows them so the original bevel survives there.
    yy, xx = np.mgrid[0:h, 0:w]
    corner = np.minimum(xx, w - 1 - xx) + np.minimum(yy, h - 1 - yy)
    keep = (corner < CUT)[..., None]
    out = img.copy()
    out[Y0:Y1, X0:X1, :3] = np.where(keep, rgb[Y0:Y1, X0:X1], np.clip(base + noise[..., None], 0, 255))
    Image.fromarray(out.round().astype(np.uint8), "RGBA").save(output)
    print("filled", (X0, Y0, X1, Y1), "->", output)


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
