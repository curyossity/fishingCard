using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Shows a strike's damage on the creature's side. At every ram, the hit's own effect pops out of the impact point on the creature
/// card as a floating number (+20 for the accuracy base, +3 for an addition, x2 for a multiplier), coloured by kind, sized by
/// strength, rising and drifting away before it fades. At the end the strike's total slams onto the creature's portrait: it drops
/// in large from above and lands exactly at the payout, the moment the hurt reaction and the Resistance bar drain begin. Floating
/// numbers still in the air clear as the slam comes down. Presentation only: every value comes from the frozen score sequence,
/// sampled statelessly from its clock, and nothing here changes points.
/// </summary>
public sealed class LandingDamageNumbers : MonoBehaviour
{
    [SerializeField] private RectTransform layer;
    [SerializeField] private RectTransform popupTemplate;
    [SerializeField] private CanvasGroup slam;
    [SerializeField] private TMP_Text slamValue;
    [SerializeField] private LandingCardCollision collision;
    [SerializeField] private RectTransform slamTarget;
    public const float Life = .7f, FadeAt = .45f, Rise = 70, MaxPool = 4;
    /// <summary>Slam: drop before the payout, the landing punch, the hold on the portrait and the fade while the bar drains.</summary>
    public const float SlamDrop = .2f, SlamPunch = .14f, SlamHold = .3f, SlamFade = .25f, SlamHeight = 110, SlamStartScale = 2.3f;
    public static readonly Color BaseColor = new Color32(242, 230, 200, 255), AddColor = new Color32(240, 205, 133, 255), MultiplyColor = new Color32(150, 225, 190, 255);
    /// <summary>Bright gold: stands out against both the parchment and the red hurt flash it lands with.</summary>
    public static readonly Color SlamColor = new Color32(255, 224, 102, 255);
    private readonly List<(RectTransform root, TMP_Text face, TMP_Text shadow)> pool = new List<(RectTransform, TMP_Text, TMP_Text)>();
    private Vector2 origin, slamRest;
    private bool active, restCaptured;

    /// <summary>Text of the floating number for a score step: the accuracy base, an addition or a multiplier.</summary>
    public static string Label(LandingScoreSequence sequence, int step)
    {
        if (step == 0) return "+" + LandingScoreRush.Format(sequence.Score.Base);
        var s = sequence.Steps[step];
        return s.Multiply ? "×" + LandingScoreRush.FormatMultiplier(s.Amount) : "+" + LandingScoreRush.Format(s.Amount);
    }

    public static Color ColorFor(LandingScoreSequence sequence, int step) => step == 0 ? BaseColor : sequence.Steps[step].Multiply ? MultiplyColor : AddColor;

    /// <summary>Whether a step produces a floating number (a zero-point base, such as an ordinary Miss, does not).</summary>
    public static bool Shows(LandingScoreSequence sequence, int step) => step > 0 || sequence.Score.Base > 0;

    /// <summary>Pop scale: bursts past full size on contact, then settles; bigger hits read bigger.</summary>
    public static float Pop(float age, float strength)
    {
        float size = 1 + .45f * Mathf.Clamp01(strength);
        if (age < 0) return 0;
        if (age < .06f) return size * Mathf.Lerp(.3f, 1.35f, age / .06f);
        if (age < .16f) return size * Mathf.Lerp(1.35f, 1, Mathf.SmoothStep(0, 1, (age - .06f) / .1f));
        return size;
    }

    /// <summary>Offset from the impact point (stage pixels, y up): rises with an ease-out and drifts away from the attacker.</summary>
    public static Vector2 Drift(float age, int step)
    {
        float u = Mathf.Clamp01(age / Life), rise = 1 - (1 - u) * (1 - u);
        // A fixed fan of angles keeps consecutive numbers from stacking without consuming any randomness.
        float spread = new[] { 22f, 44f, 10f, 34f }[step % 4];
        return new Vector2(spread * rise, Rise * rise + (step % 3 - 1) * 10);
    }

    public static float Opacity(float age) => age < 0 || age >= Life ? 0 : age < FadeAt ? 1 : 1 - (age - FadeAt) / (Life - FadeAt);

    /// <summary>Floating numbers give way to the slam: full until it starts dropping, gone by the time it lands.</summary>
    public static float ClearForSlam(float elapsed, float payoutAt) => 1 - Mathf.Clamp01((elapsed - (payoutAt - SlamDrop)) / (SlamDrop * .7f));

    /// <summary>Slam drop progress 0..1 (ease-in, accelerating like a falling weight); exactly 1 at the payout.</summary>
    public static float SlamFall(float elapsed, float payoutAt)
    {
        float u = Mathf.Clamp01((elapsed - (payoutAt - SlamDrop)) / SlamDrop);
        return u * u * u;
    }

    /// <summary>Slam scale: large while falling, squashing past full size on impact, then settling on exactly 1.</summary>
    public static float SlamScale(float elapsed, float payoutAt)
    {
        float age = elapsed - payoutAt;
        if (age < 0) return Mathf.Lerp(SlamStartScale, 1, SlamFall(elapsed, payoutAt));
        if (age > SlamPunch) return 1;
        return 1 + .3f * Mathf.Exp(-age * 18) * Mathf.Cos(age * 30) * (1 - age / SlamPunch);
    }

    /// <summary>Slam opacity: fades in during the drop, holds on the portrait, then fades while the bar drains.</summary>
    public static float SlamOpacity(float elapsed, float payoutAt)
    {
        float start = payoutAt - SlamDrop, end = payoutAt + SlamHold;
        if (elapsed < start || elapsed >= end + SlamFade) return 0;
        if (elapsed < start + .06f) return (elapsed - start) / .06f;
        return elapsed < end ? 1 : 1 - (elapsed - end) / SlamFade;
    }

    /// <summary>Captures the rest-pose impact point and slam target for one strike; call after the collision has captured its rest.</summary>
    public void Begin(LandingScoreSequence sequence)
    {
        Clear();
        if (layer == null || collision == null) return;
        origin = layer.InverseTransformPoint(collision.ContactPoint) + new Vector3(18, 18);
        if (slam != null && slamTarget != null)
        {
            var rect = (RectTransform)slam.transform;
            Vector2 target = layer.InverseTransformPoint(slamTarget.TransformPoint(slamTarget.rect.center));
            slamRest = target; restCaptured = true; rect.anchoredPosition = target;
        }
        active = true;
    }

    /// <summary>Samples every floating number and the final slam at a score-sequence time.</summary>
    public void Render(LandingScoreSequence sequence, float elapsed, bool reduced)
    {
        if (!active || sequence == null) return;
        int slot = 0;
        bool slams = sequence.Score.Points > 0;
        float clear = slams ? ClearForSlam(elapsed, sequence.PayoutAt) : 1;
        for (int i = 0; i < sequence.Steps.Count && slot < MaxPool; i++)
        {
            if (!Shows(sequence, i)) continue;
            float age = elapsed - sequence.Steps[i].ImpactAt;
            if (age < 0 || age >= Life) continue;
            float alpha = Opacity(age) * clear;
            if (alpha <= 0) continue;
            var popup = Popup(slot++);
            popup.root.gameObject.SetActive(true);
            popup.face.text = Label(sequence, i); if (popup.shadow != null) popup.shadow.text = popup.face.text;
            popup.face.color = ColorFor(sequence, i);
            popup.face.alpha = alpha; if (popup.shadow != null) popup.shadow.alpha = alpha * .85f;
            float strength = i == 0 ? .3f : sequence.Steps[i].Strength;
            popup.root.anchoredPosition = origin + (reduced ? Vector2.zero : Drift(age, i));
            popup.root.localScale = Vector3.one * (reduced ? 1 + .45f * strength : Pop(age, strength));
        }
        for (int i = slot; i < pool.Count; i++) pool[i].root.gameObject.SetActive(false);
        RenderSlam(sequence, elapsed, reduced, slams);
    }

    /// <summary>The strike's total drops onto the creature's portrait and lands exactly as the bar starts to drain.</summary>
    private void RenderSlam(LandingScoreSequence sequence, float elapsed, bool reduced, bool slams)
    {
        if (slam == null) return;
        float alpha = slams ? SlamOpacity(elapsed, sequence.PayoutAt) : 0;
        slam.alpha = alpha;
        if (alpha <= 0) return;
        slamValue.text = LandingScoreRush.Format(sequence.Score.Points);
        var rect = (RectTransform)slam.transform;
        if (reduced) { rect.anchoredPosition = slamRest; rect.localScale = Vector3.one; return; }
        rect.anchoredPosition = slamRest + Vector2.up * (SlamHeight * (1 - SlamFall(elapsed, sequence.PayoutAt)));
        rect.localScale = Vector3.one * SlamScale(elapsed, sequence.PayoutAt);
    }

    private (RectTransform root, TMP_Text face, TMP_Text shadow) Popup(int index)
    {
        while (pool.Count <= index)
        {
            var root = Instantiate(popupTemplate, layer); root.name = "Floating damage " + pool.Count;
            pool.Add((root, root.Find("Face").GetComponent<TMP_Text>(), root.Find("Shadow")?.GetComponent<TMP_Text>()));
        }
        return pool[index];
    }

    /// <summary>Hides every number and the slam, restoring the slam's rest pose.</summary>
    public void Clear()
    {
        active = false;
        foreach (var popup in pool) if (popup.root != null) popup.root.gameObject.SetActive(false);
        if (slam != null)
        {
            slam.alpha = 0;
            var rect = (RectTransform)slam.transform;
            if (restCaptured) rect.anchoredPosition = slamRest;
            rect.localScale = Vector3.one;
        }
    }

    private void OnDisable() => Clear();
}
