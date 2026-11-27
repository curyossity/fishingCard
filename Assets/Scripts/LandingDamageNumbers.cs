using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Shows a strike's damage on the creature's side. At every ram, the hit's own effect pops out of the impact point on the creature
/// card as a floating number (+20 for the accuracy base, +3 for an addition, x2 for a multiplier), coloured by kind, sized by
/// strength, rising and drifting away before it fades. A DAMAGE counter above the creature card follows the strike's running total
/// and, at the payout, drops into the Resistance bar just as the bar starts to drain. Presentation only: every value comes from the
/// frozen score sequence, sampled statelessly from its clock, and nothing here changes points.
/// </summary>
public sealed class LandingDamageNumbers : MonoBehaviour
{
    [SerializeField] private RectTransform layer;
    [SerializeField] private RectTransform popupTemplate;
    [SerializeField] private CanvasGroup counter;
    [SerializeField] private TMP_Text counterValue;
    [SerializeField] private LandingCardCollision collision;
    [SerializeField] private RectTransform healthBar;
    public const float Life = .7f, FadeAt = .45f, Rise = 70, FlightSeconds = .16f, MaxPool = 4;
    public static readonly Color BaseColor = new Color32(242, 230, 200, 255), AddColor = new Color32(240, 205, 133, 255), MultiplyColor = new Color32(150, 225, 190, 255);
    private readonly List<(RectTransform root, TMP_Text face, TMP_Text shadow)> pool = new List<(RectTransform, TMP_Text, TMP_Text)>();
    private Vector2 origin, counterRest, barTarget;
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

    /// <summary>Counter flight into the Resistance bar, 0..1 (ease-in), ending exactly at the payout when the bar starts to drain.</summary>
    public static float Flight(float elapsed, float payoutAt)
    {
        float u = Mathf.Clamp01((elapsed - (payoutAt - FlightSeconds)) / FlightSeconds);
        return u * u;
    }

    /// <summary>Captures the rest-pose impact point and counter path for one strike; call after the collision has captured its rest.</summary>
    public void Begin(LandingScoreSequence sequence)
    {
        Clear();
        if (layer == null || collision == null) return;
        origin = layer.InverseTransformPoint(collision.ContactPoint) + new Vector3(18, 18);
        if (!restCaptured && counter != null) { counterRest = ((RectTransform)counter.transform).anchoredPosition; restCaptured = true; }
        if (healthBar != null && counter != null)
        {
            var parent = (RectTransform)counter.transform.parent;
            Vector2 local = parent.InverseTransformPoint(healthBar.TransformPoint(healthBar.rect.center));
            // Anchored positions are measured from the counter's anchor; convert the bar's local point into that frame.
            var rect = (RectTransform)counter.transform;
            barTarget = counterRest + (local - (Vector2)parent.InverseTransformPoint(rect.TransformPoint(rect.rect.center)));
        }
        active = true;
    }

    /// <summary>Samples every floating number and the counter at a score-sequence time.</summary>
    public void Render(LandingScoreSequence sequence, float elapsed, bool reduced)
    {
        if (!active || sequence == null) return;
        int slot = 0;
        for (int i = 0; i < sequence.Steps.Count && slot < MaxPool; i++)
        {
            if (!Shows(sequence, i)) continue;
            float age = elapsed - sequence.Steps[i].ImpactAt;
            if (age < 0 || age >= Life) continue;
            var popup = Popup(slot++);
            popup.root.gameObject.SetActive(true);
            popup.face.text = Label(sequence, i); if (popup.shadow != null) popup.shadow.text = popup.face.text;
            popup.face.color = ColorFor(sequence, i);
            float alpha = Opacity(age);
            popup.face.alpha = alpha; if (popup.shadow != null) popup.shadow.alpha = alpha * .85f;
            float strength = i == 0 ? .3f : sequence.Steps[i].Strength;
            popup.root.anchoredPosition = origin + (reduced ? Vector2.zero : Drift(age, i));
            popup.root.localScale = Vector3.one * (reduced ? 1 + .45f * strength : Pop(age, strength));
        }
        for (int i = slot; i < pool.Count; i++) pool[i].root.gameObject.SetActive(false);
        RenderCounter(sequence, elapsed, reduced);
    }

    /// <summary>Running total above the creature card; at the payout it drops into the Resistance bar.</summary>
    private void RenderCounter(LandingScoreSequence sequence, float elapsed, bool reduced)
    {
        if (counter == null) return;
        int first = Shows(sequence, 0) ? 0 : 1;
        bool scoring = sequence.Score.Points > 0 && first < sequence.Steps.Count;
        float start = scoring ? sequence.Steps[first].ImpactAt : float.MaxValue;
        if (!scoring || elapsed < start || elapsed >= sequence.PayoutAt) { counter.alpha = 0; return; }
        counterValue.text = LandingScoreRush.Format(sequence.ValueAt(elapsed));
        float flight = Flight(elapsed, sequence.PayoutAt), fadeIn = Mathf.Clamp01((elapsed - start) / .08f);
        var rect = (RectTransform)counter.transform;
        if (reduced)
        {
            rect.anchoredPosition = counterRest; rect.localScale = Vector3.one;
            counter.alpha = fadeIn * (1 - flight);
            return;
        }
        rect.anchoredPosition = Vector2.Lerp(counterRest, barTarget, flight);
        rect.localScale = Vector3.one * Mathf.Lerp(1, .55f, flight);
        counter.alpha = fadeIn * Mathf.Lerp(1, .35f, flight);
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

    /// <summary>Hides every number and returns the counter to its rest pose.</summary>
    public void Clear()
    {
        active = false;
        foreach (var popup in pool) if (popup.root != null) popup.root.gameObject.SetActive(false);
        if (counter != null)
        {
            counter.alpha = 0;
            var rect = (RectTransform)counter.transform;
            if (restCaptured) rect.anchoredPosition = counterRest;
            rect.localScale = Vector3.one;
        }
    }

    private void OnDisable() => Clear();
}
