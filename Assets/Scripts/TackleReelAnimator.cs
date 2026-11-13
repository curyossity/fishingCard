using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>Animates a reusable four-symbol cylindrical reel toward an already resolved authoritative tag.</summary>
public sealed class TackleReelAnimator : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image symbolTemplate;
    [SerializeField] private TMP_Text fallbackTemplate, labelTemplate;
    [SerializeField, Min(1)] private float radius = 172;
    [SerializeField] private TackleReelSurface surface;
    [Header("Elastic Stop")]
    [SerializeField, Range(0, .7f)] private float stopOvershootInSymbols = .45f;
    [SerializeField, Range(.15f, .6f)] private float settleFraction = .34f;
    [SerializeField, Range(.2f, 1)] private float elasticPeriod = .47f;
    private readonly List<TackleReelFace> faces = new List<TackleReelFace>();
    private string[] tags = Array.Empty<string>();
    private float phase, from, destination, elapsed, duration;
    private int target;
    public bool IsSpinning { get; private set; }
    public float Phase => phase;

    /// <summary>Authors the reel's native templates without replacing existing source artwork.</summary>
    public void Configure(UnityEngine.UI.Image symbol, TMP_Text fallback, TMP_Text label)
    { symbolTemplate = symbol; fallbackTemplate = fallback; labelTemplate = label; }

    /// <summary>Creates only the expedition's four active symbols and starts a deterministic visual trajectory.</summary>
    public void Spin(IReadOnlyList<string> activeTags, string result, Func<string, Sprite> illustration, float seconds)
    {
        EnsureFaces(activeTags, illustration);
        target = Array.FindIndex(tags, tag => string.Equals(tag, result, StringComparison.OrdinalIgnoreCase));
        if (target < 0) throw new InvalidOperationException("The resolved reel tag is outside the expedition symbols.");
        from = phase;
        float end = -target * Mathf.PI * .5f;
        destination = from + Mathf.Repeat(end - from, 2 * Mathf.PI) + 4 * Mathf.PI;
        elapsed = 0; duration = Mathf.Max(.2f, seconds); IsSpinning = true;
        Draw();
    }

    /// <summary>Decelerates beyond the selected face, then recoils elastically using unscaled time and the existing stagger duration.</summary>
    public void Advance(float delta)
    {
        if (!IsSpinning) return;
        elapsed += Mathf.Max(0, delta);
        float t = Mathf.Clamp01(elapsed / duration);
        float settle = Mathf.Clamp(settleFraction, .15f, .6f);
        float overshoot = Mathf.Clamp(stopOvershootInSymbols, 0, .7f) * Mathf.PI * .5f;
        if (t < 1 - settle)
        {
            float travel = t / (1 - settle);
            phase = Mathf.LerpUnclamped(from, destination + overshoot, 1 - Mathf.Pow(1 - travel, 3));
        }
        else phase = destination + overshoot * SpringReturn((t - 1 + settle) / settle);
        if (t >= 1) { phase = Mathf.Repeat(destination, 2 * Mathf.PI); IsSpinning = false; }
        Draw();
    }

    /// <summary>Returns from a zero-velocity overshoot with a damped elastic recoil and an exact, smooth endpoint.</summary>
    private float SpringReturn(float progress)
    {
        float t = Mathf.Clamp01(progress);
        const float damping = 6;
        float frequency = 2 * Mathf.PI / Mathf.Clamp(elasticPeriod, .2f, 1);
        // The sine term makes the turning point's velocity zero, matching the end of the forward deceleration.
        float oscillation = Mathf.Exp(-damping * t) * (Mathf.Cos(frequency * t) + damping / frequency * Mathf.Sin(frequency * t));
        float tail = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.7f, 1, t));
        return oscillation * tail;
    }

    /// <summary>Retains source templates and reuses generated faces until the active symbol set changes.</summary>
    private void EnsureFaces(IReadOnlyList<string> activeTags, Func<string, Sprite> illustration)
    {
        bool same = tags.Length == activeTags.Count;
        for (int i = 0; i < tags.Length && same; i++) same &= tags[i] == activeTags[i];
        if (same) return;
        foreach (var face in faces) Destroy(face.gameObject);
        faces.Clear(); tags = new string[activeTags.Count]; phase = 0; target = 0;
        for (int i = 0; i < tags.Length; i++)
        {
            tags[i] = activeTags[i];
            var row = new GameObject("Reel face " + tags[i], typeof(RectTransform), typeof(TackleReelFace));
            var rect = (RectTransform)row.transform; rect.SetParent(transform, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f); rect.sizeDelta = new Vector2(320, 365);
            var icon = Instantiate(symbolTemplate, rect); var fallback = Instantiate(fallbackTemplate, rect); var label = Instantiate(labelTemplate, rect);
            icon.sprite = illustration(tags[i]); icon.enabled = icon.sprite != null; icon.gameObject.SetActive(true);
            fallback.text = label.text = tags[i].ToUpperInvariant(); fallback.gameObject.SetActive(icon.sprite == null); label.gameObject.SetActive(true);
            var face = row.GetComponent<TackleReelFace>(); face.Initialize((RectTransform)transform, icon, fallback, label, radius); faces.Add(face);
        }
        symbolTemplate.gameObject.SetActive(false); fallbackTemplate.gameObject.SetActive(false); labelTemplate.gameObject.SetActive(false);
    }

    /// <summary>Keeps paper, dividers, plaques and native faces on one continuous cylinder, including its visible neighboring edges.</summary>
    private void Draw()
    {
        if (surface != null) surface.ApplyRotation(phase, radius);
        for (int i = 0; i < faces.Count; i++)
            faces[i].SetAngle(phase + i * Mathf.PI * .5f, true);
    }

    /// <summary>Cancels visual work immediately on phase exit without changing the resolved runtime result.</summary>
    private void OnDisable() { IsSpinning = false; }
}
