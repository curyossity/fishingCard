"""Compare an isolated full-water render with its identical water-disabled pose."""
from pathlib import Path
import json
import sys

import numpy as np
from PIL import Image

folder = Path(sys.argv[1] if len(sys.argv) > 1 else "Docs/Validation/StrikeWater/Contained")
full = np.asarray(Image.open(folder / "Full.png").convert("RGB"))
empty = np.asarray(Image.open(folder / "WaterOff.png").convert("RGB"))
y, x = np.nonzero(np.any(full != empty, axis=2))
polygon = np.array(json.loads((folder / "GlassPolygon.json").read_text()))
area = np.sum(polygon[:, 0] * np.roll(polygon[:, 1], -1) - polygon[:, 1] * np.roll(polygon[:, 0], -1))
direction = 1 if area > 0 else -1
inside = np.ones(len(x), dtype=bool)
for first, second in zip(polygon, np.roll(polygon, -1, axis=0)):
    edge = second - first
    distance = direction * (edge[0] * (y + .5 - first[1]) - edge[1] * (x + .5 - first[0])) / np.linalg.norm(edge)
    inside &= distance >= -.75  # Subpixel raster coverage at the projected polygon edge.
outside = int(np.count_nonzero(~inside))
assert len(x) > 0, "The comparison contains no water."
assert outside == 0, f"Water changes {outside} pixels outside the glass."
report = dict(changedPixels=len(x), pixelsOutsideGlass=outside, rasterTolerancePixels=.75, frameOutsideGlassUnchanged=True)
(folder / "PixelVerification.json").write_text(json.dumps(report, indent=2) + "\n")
print(json.dumps(report))
