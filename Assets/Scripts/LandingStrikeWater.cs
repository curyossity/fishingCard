using UnityEngine;
using UnityEngine.UI;

/// <summary>Fills the timing glass from delivered strike points, independently of aiming geometry and persistent encounter progress.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class LandingStrikeWater : MaskableGraphic
{
    public const float DrainSeconds = 2f / 3f;
    private const float RiseSeconds = .065f, DrainDelay = .08f;
    [SerializeField] private Color bottom = new Color(.055f, .24f, .31f, .78f);
    [SerializeField] private Color surface = new Color(.25f, .62f, .67f, .80f);
    [SerializeField] private Color foam = new Color(.69f, .90f, .85f, .90f);
    private float clock;
    private bool flat;
    public float Level { get; private set; }
    public bool FlatSurface => flat;
    public float CornerCut => Mathf.Min(18, Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) * .5f);

    /// <summary>Identifies the inset beveled glass opening rather than the track's rectangular aiming bounds.</summary>
    public bool ContainsGlassPoint(Vector2 point)
    {
        Rect bounds = rectTransform.rect;
        float cut = CutAt(bounds, point.x);
        return point.x >= bounds.xMin && point.x <= bounds.xMax && point.y >= bounds.yMin + cut && point.y <= bounds.yMax - cut;
    }

    /// <summary>Maps half the original Resistance to full glass, saturating before decimal division can overflow.</summary>
    public static float DamageLevel(decimal points, decimal resistance)
    {
        decimal full = resistance / 2;
        if (points <= 0 || full <= 0) return 0;
        return points >= full ? 1 : (float)(points / full);
    }

    /// <summary>Accumulates canonical running totals at contact, then drains once after the final delivered operation.</summary>
    public static float LevelAt(LandingScoreSequence sequence, decimal resistance, float elapsed, bool reduced)
    {
        var hit = sequence.Steps[sequence.ImpactStepAt(elapsed)];
        if (elapsed < hit.ImpactAt) return 0;
        float previous = DamageLevel(hit.PreviousValue, resistance), next = DamageLevel(hit.Value, resistance);
        float rise = reduced ? 1 : Mathf.SmoothStep(0, 1, Mathf.Clamp01((elapsed - hit.ImpactAt) / RiseSeconds));
        float level = Mathf.Lerp(previous, next, rise);
        float drainAt = sequence.Steps[sequence.Steps.Count - 1].ImpactAt + DrainDelay;
        return level * (1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01((elapsed - drainAt) / DrainSeconds)));
    }

    /// <summary>Samples the existing unscaled presentation clock without changing score, targets, marker positions or input.</summary>
    public void Render(LandingScoreSequence sequence, decimal resistance, float elapsed, bool reduced)
    {
        if (!isActiveAndEnabled) { Clear(); return; }
        Level = LevelAt(sequence, resistance, elapsed, reduced); clock = elapsed; flat = reduced; SetVerticesDirty();
    }

    /// <summary>Clips water and foam to the beveled opening underneath unchanged fish and accuracy bands.</summary>
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear(); if (Level <= .0001f) return;
        Rect bounds = rectTransform.rect;
        float amplitude = flat ? 0 : 2 * Mathf.Sin(Level * Mathf.PI);
        const int segments = 80;
        for (int i = 0; i < segments; i++)
        {
            float x0 = Mathf.Lerp(bounds.xMin, bounds.xMax, i / (float)segments);
            float x1 = Mathf.Lerp(bounds.xMin, bounds.xMax, (i + 1) / (float)segments);
            // Split exactly at the bevel changes so chords cannot bridge across the glass corners.
            float left = bounds.xMin + CornerCut, right = bounds.xMax - CornerCut;
            if (x0 < left && x1 > left) { Strip(mesh, bounds, x0, left, amplitude); x0 = left; }
            if (x0 < right && x1 > right) { Strip(mesh, bounds, x0, right, amplitude); x0 = right; }
            Strip(mesh, bounds, x0, x1, amplitude);
        }
    }

    /// <summary>Measures the diagonal inset at one horizontal position in the eight-sided glass.</summary>
    private float CutAt(Rect bounds, float x) => Mathf.Max(0, CornerCut - Mathf.Min(x - bounds.xMin, bounds.xMax - x));

    /// <summary>Intersects one linear surface segment with the beveled floor and roof, including partial fills.</summary>
    private void Strip(VertexHelper mesh, Rect bounds, float x0, float x1, float amplitude)
    {
        float h0 = Height(bounds, x0, amplitude), h1 = Height(bounds, x1, amplitude);
        float floor0 = bounds.yMin + CutAt(bounds, x0), floor1 = bounds.yMin + CutAt(bounds, x1);
        float depth0 = h0 - floor0, depth1 = h1 - floor1;
        if (depth0 <= 0 && depth1 <= 0) return;
        if (depth0 < 0)
        {
            float crossing = depth0 / (depth0 - depth1);
            x0 = Mathf.Lerp(x0, x1, crossing); h0 = Mathf.Lerp(h0, h1, crossing); floor0 = bounds.yMin + CutAt(bounds, x0);
        }
        else if (depth1 < 0)
        {
            float crossing = depth1 / (depth1 - depth0);
            x1 = Mathf.Lerp(x1, x0, crossing); h1 = Mathf.Lerp(h1, h0, crossing); floor1 = bounds.yMin + CutAt(bounds, x1);
        }
        float top0 = Mathf.Max(floor0, Mathf.Min(h0, bounds.yMax - CutAt(bounds, x0)));
        float top1 = Mathf.Max(floor1, Mathf.Min(h1, bounds.yMax - CutAt(bounds, x1)));
        Quad(mesh, x0, x1, floor0, floor1, top0, top1, bottom, surface);
        Quad(mesh, x0, x1, Mathf.Max(floor0, top0 - 1.5f), Mathf.Max(floor1, top1 - 1.5f), top0, top1, foam, foam);
    }

    /// <summary>Keeps wave displacement inside the glass and reduces it to zero at empty/full.</summary>
    private float Height(Rect bounds, float x, float amplitude) => Mathf.Clamp(bounds.yMin + Level * bounds.height
        + amplitude * (Mathf.Sin(x * .035f - clock * 7) * .65f + Mathf.Sin(x * .017f + clock * 4) * .35f), bounds.yMin, bounds.yMax);

    /// <summary>Writes one gradient water strip without transient objects or per-frame managed allocations.</summary>
    private static void Quad(VertexHelper mesh, float x0, float x1, float lower0, float lower1, float upper0, float upper1, Color low, Color high)
    {
        int first = mesh.currentVertCount;
        mesh.AddVert(new Vector2(x0, lower0), low, Vector2.zero); mesh.AddVert(new Vector2(x0, upper0), high, Vector2.zero);
        mesh.AddVert(new Vector2(x1, upper1), high, Vector2.zero); mesh.AddVert(new Vector2(x1, lower1), low, Vector2.zero);
        mesh.AddTriangle(first, first + 1, first + 2); mesh.AddTriangle(first, first + 2, first + 3);
    }

    /// <summary>Leaves the glass empty on completion, skip, replacement and phase exit.</summary>
    public void Clear() { Level = 0; clock = 0; SetVerticesDirty(); }
    /// <summary>Restores the empty state when independently disabled, while preserving normal uGUI cleanup.</summary>
    protected override void OnDisable() { Clear(); base.OnDisable(); }
}
