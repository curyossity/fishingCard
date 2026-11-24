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

    /// <summary>Builds translucent water with a restrained surface wave underneath the existing fish and accuracy bands.</summary>
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
            float y0 = Height(bounds, x0, amplitude), y1 = Height(bounds, x1, amplitude);
            Quad(mesh, x0, x1, bounds.yMin, bounds.yMin, y0, y1, bottom, surface);
            Quad(mesh, x0, x1, Mathf.Max(bounds.yMin, y0 - 1.5f), Mathf.Max(bounds.yMin, y1 - 1.5f), y0, y1, foam, foam);
        }
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
