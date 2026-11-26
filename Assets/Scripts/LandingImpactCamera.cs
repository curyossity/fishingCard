using UnityEngine;

/// <summary>Owns the overlay stage's virtual camera pose, framing both colliding cards and composing the collision's shake.</summary>
public sealed class LandingImpactCamera : MonoBehaviour
{
    [SerializeField] private RectTransform stage, creatureCard, scoreCard;
    [SerializeField, Range(1, 2)] private float maximumZoom = 1.65f;
    private Vector2 rest;
    private Vector3 scale, localRest;
    private Rect frame;
    private bool captured;
    public float Zoom { get; private set; } = 1;
    public Rect FramedCards => frame;
    private readonly Vector3[] corners = new Vector3[4];

    /// <summary>Captures a stable composition at contact so the camera does not chase individual recoil frames.</summary>
    public void Begin(float contactDistance)
    {
        Clear(); rest = stage.anchoredPosition; scale = stage.localScale; localRest = stage.localPosition;
        Vector2 minimum = new Vector2(float.PositiveInfinity, float.PositiveInfinity), maximum = -minimum;
        foreach (var card in new[] { creatureCard, scoreCard })
        {
            card.GetWorldCorners(corners);
            foreach (var corner in corners)
            {
                Vector2 point = stage.InverseTransformPoint(corner);
                if (card == creatureCard) point += Vector2.right * contactDistance;
                minimum = Vector2.Min(minimum, point); maximum = Vector2.Max(maximum, point);
            }
        }
        frame = Rect.MinMaxRect(minimum.x - 35, minimum.y - 25, maximum.x + 35, maximum.y + 25); captured = true;
    }

    /// <summary>Pushes in at contact, holds through slow compression and initial rebound, then eases out at normal-speed recovery.</summary>
    public void Render(LandingScoreSequence sequence, LandingImpactTiming timing, float elapsed, Vector2 shake, bool reduced)
    {
        ResetMotion(); if (!captured || reduced || !isActiveAndEnabled) return;
        int index = sequence.ImpactStepAt(elapsed); var hit = sequence.Steps[index];
        float weight = 0;
        if (timing.Focuses(index) && elapsed >= hit.ImpactAt && elapsed < hit.EndAt)
        {
            float recoveryAt = LandingImpactTiming.SlowEnd(hit);
            weight = elapsed < recoveryAt
                ? Mathf.SmoothStep(0, 1, Mathf.Clamp01((timing.ToReal(elapsed) - timing.ToReal(hit.ImpactAt)) / .07f))
                : 1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01((elapsed - recoveryAt) / Mathf.Min(.22f, hit.EndAt - recoveryAt)));
        }
        var viewport = (RectTransform)stage.parent;
        float fit = Mathf.Min(maximumZoom, viewport.rect.width / (frame.width * Mathf.Abs(scale.x) + 120), viewport.rect.height / (frame.height * Mathf.Abs(scale.y) + 100));
        Zoom = Mathf.Lerp(1, Mathf.Max(1, fit), weight);
        stage.localScale = scale * Zoom;
        Vector2 focus = (Vector2)localRest + Vector2.Scale(frame.center, (Vector2)scale * Zoom);
        stage.anchoredPosition = rest + (viewport.rect.center - focus) * weight + shake * Zoom;
    }

    /// <summary>Applies an unzoomed stage offset for non-scoring moments (the creature reveal), capturing the rest pose on first use.</summary>
    public void RenderOffset(Vector2 offset)
    {
        if (!captured) { rest = stage.anchoredPosition; scale = stage.localScale; localRest = stage.localPosition; frame = default; captured = true; }
        Zoom = 1; stage.localScale = scale; stage.anchoredPosition = rest + offset;
    }

    /// <summary>Restores the authored stage pose immediately, including when reduced motion is enabled mid-shot.</summary>
    public void ResetMotion() { Zoom = 1; if (!captured) return; stage.anchoredPosition = rest; stage.localScale = scale; }
    /// <summary>Releases all camera ownership on skip, replacement, completion and phase exit.</summary>
    public void Clear() { ResetMotion(); captured = false; }
    /// <summary>Prevents independent disable from leaving the cabinet zoomed or displaced.</summary>
    private void OnDisable() => Clear();
}
