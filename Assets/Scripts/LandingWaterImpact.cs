using UnityEngine;

/// <summary>Samples custom PixelFX water sheets and a score-digit punch on the existing collision clock.</summary>
public sealed class LandingWaterImpact : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.RawImage spray;
    [SerializeField] private RectTransform number;
    [SerializeField] private LandingCardCollision collision;
    [SerializeField] private Texture2D[] sheets;
    [SerializeField] private int frameCount, columns, rows;
    [SerializeField] private float fps;
    private Vector3 numberRest;
    private bool captured;
    public int ActiveFrame { get; private set; } = -1;
    public int ActiveTier { get; private set; } = -1;
    public float NumberScale => captured ? number.localScale.x / numberRest.x : 1;

    /// <summary>Captures the native number scale after a previous receipt has restored its presentation.</summary>
    public void Begin() { Clear(); numberRest = number.localScale; captured = true; }

    /// <summary>Shows only delivered gains, suppressing spray during turns, misses and reduced motion.</summary>
    public void Render(LandingScoreSequence sequence, float elapsed, bool reduced)
    {
        if (!captured || reduced || LandingCardHandoff.Sample(sequence, elapsed).Active) { ResetVisuals(); return; }
        int index = sequence.ImpactStepAt(elapsed); var step = sequence.Steps[index];
        if (sequence.StepAt(elapsed) != index) { ResetVisuals(); return; }
        float age = elapsed - step.ImpactAt;
        if (age < 0 || age >= (frameCount - 1) / fps || step.Gain <= 0) { ResetVisuals(); return; }
        float strength = index == 0 ? .24f : step.Strength;
        ActiveTier = strength >= .7f ? 2 : strength >= .28f ? 1 : 0;
        ActiveFrame = Mathf.Min(frameCount - 1, Mathf.FloorToInt(age * fps));
        spray.texture = sheets[ActiveTier];
        spray.uvRect = new Rect((ActiveFrame % columns) / (float)columns, 1 - (ActiveFrame / columns + 1) / (float)rows, 1f / columns, 1f / rows);
        spray.rectTransform.position = collision.ContactPoint;
        spray.enabled = true;
        float punch = (.045f + strength * .095f) * (1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01(age / .16f)));
        number.localScale = numberRest * (1 + punch);
    }

    /// <summary>Clears visual output without consuming the receipt, including a live reduced-motion change.</summary>
    public void ResetVisuals()
    {
        ActiveFrame = ActiveTier = -1;
        if (spray != null) spray.enabled = false;
        if (captured) number.localScale = numberRest;
    }
    /// <summary>Restores the number and clears the sheet on skip, replacement, completion or phase exit.</summary>
    public void Clear() { ResetVisuals(); captured = false; }
    /// <summary>Cannot retain a spray or stretched number when disabled independently.</summary>
    private void OnDisable() => Clear();
}
