# Water contained inside the glass

Active presentation revision, 24 November 2026. [Watch the full-to-empty preview](Compare.html) or inspect the [full-level detail](Detail.png). The initial strike-water version was committed and pushed as `ec2ebcf` before this fix.

The timing track's rectangular mask includes the artwork's angled frame corners. The original rectangular fill could therefore cover those corners at full water. The water now uses an eight-sided glass shape with 18-reference-pixel beveled corners and a four-pixel inset. The aiming track, fish and accuracy-zone bounds are unchanged.

`LandingStrikeWater` clips both body and foam against the beveled floor and roof. It splits strips exactly at bevel transitions and intersects partial water surfaces with the sloping floor, so rising/draining water also remains inside. The bounded mesh has at most 656 vertices, creates no transient objects or per-frame managed allocations, and retains the existing mask and draw order.

Fill still reaches full at 50% of original Resistance, accumulates the current strike's running total, and drains over two-thirds of a second after the final contribution. Collision, sound, number punch, splash, reduced motion and cleanup behavior remain unchanged.

**729 checks pass:** 196 native checks per fixture, 64 prefab/binding checks, 209 score/schedule checks and 64 Landing/model checks. Geometry checks now enforce the diagonal glass bounds rather than only the rectangular track. Prefab checks enforce the four-pixel clearance and reject the former rectangular corners. Both weak and powerful receipts retain all existing scoring, animation, input, masking and interruption checks.

A second independent check compares a full-water native frame against the identical frozen pose with the water component disabled. [PixelVerification.json](PixelVerification.json) records 70,918 changed pixels and **zero changed pixels outside the glass polygon**, with 0.75-pixel edge-raster tolerance. The frame outside the opening is untouched. [Full.png](Full.png), [WaterOff.png](WaterOff.png), [Half.png](Half.png) and the projected [GlassPolygon.json](GlassPolygon.json) retain the evidence. Reproduce with the free local Python environment:

```powershell
.local-tools/earshot/venv/Scripts/python.exe Tools/Rendering/check_strike_water_clip.py
```

The motion preview contains 80 consecutive native frames at 60 fps, covering original sequence time 2.8-4.13 seconds: the final collision, full water, drain and empty glass. Audio is trimmed from the matching frozen runtime schedule. Unity reports zero errors and the isolated Play session is stopped. GUID/meta integrity covers 2,175 files with zero errors/warnings. Neither player-controller commands nor player saves are touched.
