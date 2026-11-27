using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Native-geometry roller shutter shaped exactly to the Tackle machine body. The body silhouette (traced from the shared housing
/// artwork, excluding the external pipes) is stored as horizontal spans per row; the shutter is drawn row by row inside those spans.
/// Its strip is bent over a drum behind the machine's top edge, so slats near the top are foreshortened and darker and the shutter
/// always appears to come over from behind the cabinet rather than appearing from nowhere.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class TackleShutterGraphic : MaskableGraphic
{
    public const float DrumRadius = 90, SlatHeight = 30, BarHeight = 26, Row = 1.5f;
    [SerializeField, HideInInspector] private int rows;
    /// <summary>Span index range per row: spans for row i are spanStart[i] .. spanStart[i + 1] - 1. Rows run top to bottom.</summary>
    [SerializeField, HideInInspector] private int[] spanStart = Array.Empty<int>();
    /// <summary>Horizontal extents as fractions of the graphic's width (x = left, y = right).</summary>
    [SerializeField, HideInInspector] private Vector2[] spans = Array.Empty<Vector2>();
    private float extent;

    public bool HasSilhouette => rows > 0 && spanStart.Length == rows + 1;
    /// <summary>How far the shutter has dropped, 0 (rolled up) to 1 (fully closed on the machine's bottom edge).</summary>
    public float Extent { get => extent; set { value = Mathf.Clamp01(value); if (value == extent) return; extent = value; SetVerticesDirty(); } }

#if UNITY_EDITOR
    /// <summary>Editor authoring: stores the traced body silhouette (rows top to bottom, spans as width fractions).</summary>
    public void SetSilhouette(int rowCount, int[] starts, Vector2[] extents) { rows = rowCount; spanStart = starts; spans = extents; SetVerticesDirty(); }
#endif

    /// <summary>Screen depth below the top edge for a point at strip distance s from where it leaves the drum.</summary>
    public static float Depth(float s)
    {
        float wrap = DrumRadius * Mathf.PI * .5f;
        return s <= wrap ? DrumRadius * (1 - Mathf.Cos(s / DrumRadius)) : DrumRadius + s - wrap;
    }

    /// <summary>Inverse of Depth: the strip distance that reaches a given visible depth.</summary>
    public static float StripLength(float depth)
    {
        if (depth <= 0) return 0;
        return depth <= DrumRadius ? DrumRadius * Mathf.Acos(1 - depth / DrumRadius) : DrumRadius * Mathf.PI * .5f + depth - DrumRadius;
    }

    private static float Facing(float s) => s >= DrumRadius * Mathf.PI * .5f ? 1 : Mathf.Sin(Mathf.Max(0, s) / DrumRadius);
    /// <summary>Light falls off steeply as the strip turns away over the drum, so the top reads as curving behind the cabinet.</summary>
    private static float Shade(float s) => Mathf.Lerp(.08f, 1, Mathf.Pow(Facing(s), 1.6f));
    /// <summary>Deterministic 0..1 noise per slat, so wear marks stay fixed to their slat as it moves.</summary>
    private static float Hash(int slat, int salt) { uint h = (uint)(slat * 73856093) ^ (uint)(salt * 19349663); h ^= h >> 13; h *= 0x5bd1e995; h ^= h >> 15; return (h & 0xffff) / 65535f; }

    private static readonly Color SlatFace = new Color32(66, 88, 82, 255), SlatLight = new Color32(116, 142, 128, 255), SlatBevel = new Color32(46, 64, 59, 255);
    private static readonly Color Groove = new Color32(16, 22, 21, 255), Brass = new Color32(178, 128, 56, 255), BrassLight = new Color32(236, 192, 112, 255);
    private static readonly Color BrassDark = new Color32(88, 58, 24, 255), Rim = new Color32(12, 16, 15, 255), RimBrass = new Color32(150, 108, 50, 255);

    /// <summary>Base colour of the strip at distance s (bottom bar at the strip's end, slats above it).</summary>
    private static Color StripColor(float s, float strip)
    {
        float fromBar = strip - s;
        if (fromBar <= BarHeight)
        {
            if (fromBar < 4) return BrassDark;
            if (fromBar > BarHeight - 3) return Groove;
            if (fromBar > BarHeight - 6) return BrassLight;
            return Brass;
        }
        float within = (fromBar - BarHeight) % SlatHeight; // 0 at the bottom of a slat
        if (within < 3) return Groove;
        if (within < 6) return SlatBevel;
        if (within > SlatHeight - 2) return SlatLight;
        return SlatFace;
    }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear(); if (extent <= 0 || !HasSilhouette) return;
        Rect r = rectTransform.rect;
        float depth = extent * r.height, strip = StripLength(depth);
        // Row by row: each screen row maps back to a strip distance, which gives its colour and shading.
        for (float d = 0; d < depth; d += Row)
        {
            float d1 = Mathf.Min(depth, d + Row), s = StripLength((d + d1) * .5f);
            Color c = StripColor(s, strip); float shade = Shade(s);
            c = new Color(c.r * shade, c.g * shade, c.b * shade, 1);
            RowSpans(mesh, r, d, d1, c, true);
        }
        // Wear marks follow the slats as they travel.
        float slatTop = strip - BarHeight;
        for (float end = slatTop; end > 0; end -= SlatHeight)
        {
            float start = end - SlatHeight; int slat = Mathf.RoundToInt((slatTop - end) / SlatHeight);
            for (int m = 0; m < 3; m++)
            {
                float fx = Mathf.Lerp(.08f, .92f, Hash(slat, m)), width = (14 + 46 * Hash(slat, m + 7)) / r.width;
                float s0 = start + 8 + 10 * Hash(slat, m + 13), s1 = s0 + 3 + 6 * Hash(slat, m + 19);
                Mark(mesh, r, fx - width * .5f, fx + width * .5f, s0, s1, m == 0 ? new Color(.55f, .33f, .14f, .38f) : new Color(.12f, .17f, .16f, .32f));
            }
        }
        // Pull handle and the soft shadow the bar casts onto the machine below it.
        Mark(mesh, r, .5f - 56 / r.width, .5f + 56 / r.width, strip - BarHeight + 9, strip - BarHeight + 18, BrassDark);
        if (depth < r.height - 1)
            for (float d = depth; d < Mathf.Min(r.height, depth + 16); d += Row)
                RowSpans(mesh, r, d, Mathf.Min(r.height, d + Row), new Color(0, 0, 0, .45f * (1 - (d - depth) / 16)), false);
    }

    /// <summary>Fills the body silhouette between two depths, optionally with a dark rim and brass trim at each span's ends.</summary>
    private void RowSpans(VertexHelper mesh, Rect r, float d0, float d1, Color color, bool trim)
    {
        int row = Mathf.Clamp(Mathf.FloorToInt((d0 + d1) * .5f / r.height * rows), 0, rows - 1);
        float top = r.yMax - d0, bottom = r.yMax - d1;
        for (int i = spanStart[row]; i < spanStart[row + 1]; i++)
        {
            float x0 = r.xMin + spans[i].x * r.width, x1 = r.xMin + spans[i].y * r.width;
            if (x1 - x0 < .5f) continue;
            Quad(mesh, x0, x1, top, bottom, color);
            if (!trim || x1 - x0 < 12) continue;
            // Rim and brass trim keep the silhouette edge reading as a fitted cover, shaded with the drum curve.
            float shade = Mathf.Clamp01(Mathf.Max(color.r, color.g) / Mathf.Max(.001f, Mathf.Max(SlatFace.r, SlatFace.g)));
            var brass = new Color(RimBrass.r * shade, RimBrass.g * shade, RimBrass.b * shade, 1);
            Quad(mesh, x0, x0 + 2, top, bottom, Rim); Quad(mesh, x0 + 2, x0 + 4, top, bottom, brass);
            Quad(mesh, x1 - 4, x1 - 2, top, bottom, brass); Quad(mesh, x1 - 2, x1, top, bottom, Rim);
            // Subtle inward darkening at both sides, like the top curve but gentler: the slats seem to turn away into side
            // channels, so the shutter sits in the machine instead of hovering on it.
            float fade = Mathf.Min(SideShadeWidth, (x1 - x0) * .25f);
            SideShade(mesh, x0 + 4, x0 + 4 + fade, top, bottom, SideShadeAlpha, 0);
            SideShade(mesh, x1 - 4 - fade, x1 - 4, top, bottom, 0, SideShadeAlpha);
        }
    }

    public const float SideShadeWidth = 44, SideShadeAlpha = .62f;

    /// <summary>Horizontal black gradient between two alphas, used for the side curvature.</summary>
    private static void SideShade(VertexHelper mesh, float x0, float x1, float top, float bottom, float leftAlpha, float rightAlpha)
    {
        if (x1 - x0 < .5f) return;
        Color left = new Color(0, 0, 0, leftAlpha), right = new Color(0, 0, 0, rightAlpha);
        int i = mesh.currentVertCount;
        mesh.AddVert(new Vector3(x0, top), left, Vector2.zero); mesh.AddVert(new Vector3(x1, top), right, Vector2.zero);
        mesh.AddVert(new Vector3(x1, bottom), right, Vector2.zero); mesh.AddVert(new Vector3(x0, bottom), left, Vector2.zero);
        mesh.AddTriangle(i, i + 1, i + 2); mesh.AddTriangle(i, i + 2, i + 3);
    }

    /// <summary>Translucent mark between strip distances s0..s1 and width fractions fx0..fx1, clipped to the silhouette.</summary>
    private void Mark(VertexHelper mesh, Rect r, float fx0, float fx1, float s0, float s1, Color color)
    {
        s0 = Mathf.Max(0, s0); if (s1 <= s0) return;
        float d0 = Depth(s0), d1 = Depth(s1), shade = Shade((s0 + s1) * .5f);
        var c = new Color(color.r * shade, color.g * shade, color.b * shade, color.a);
        for (float d = d0; d < d1; d += Row)
        {
            float e = Mathf.Min(d1, d + Row);
            int row = Mathf.Clamp(Mathf.FloorToInt((d + e) * .5f / r.height * rows), 0, rows - 1);
            for (int i = spanStart[row]; i < spanStart[row + 1]; i++)
            {
                float a = Mathf.Max(fx0, spans[i].x + 8 / r.width), b = Mathf.Min(fx1, spans[i].y - 8 / r.width);
                if (b > a) Quad(mesh, r.xMin + a * r.width, r.xMin + b * r.width, r.yMax - d, r.yMax - e, c);
            }
        }
    }

    private static void Quad(VertexHelper mesh, float x0, float x1, float top, float bottom, Color color)
    {
        int i = mesh.currentVertCount;
        mesh.AddVert(new Vector3(x0, top), color, Vector2.zero); mesh.AddVert(new Vector3(x1, top), color, Vector2.zero);
        mesh.AddVert(new Vector3(x1, bottom), color, Vector2.zero); mesh.AddVert(new Vector3(x0, bottom), color, Vector2.zero);
        mesh.AddTriangle(i, i + 1, i + 2); mesh.AddTriangle(i, i + 2, i + 3);
    }
}
