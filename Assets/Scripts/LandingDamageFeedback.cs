using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Makes a strike's points read as damage to the encounter creature. When the strike's points transfer into accumulated Landing
/// Points (the score payout), the creature card on the right flinches away from the Strike panel with a hurt tint, the moving fish
/// thrashes in the timing glass, and a bright damage chip marks the part of the draining Resistance bar just lost.
/// Severity follows the share of Resistance the strike dealt. Presentation only: it samples the score sequence's clock, composes on
/// top of the collision pose each frame, never changes points, and restores everything it touches.
/// </summary>
public sealed class LandingDamageFeedback : MonoBehaviour
{
    [SerializeField] private LandingCardCollision collision;
    [SerializeField] private RectTransform creatureCard, fish;
    [SerializeField] private LandingPanelFlash creatureFlash;
    [SerializeField] private Image chip;
    /// <summary>Length of the hurt reaction (knockback, tint, thrash); the white trail follows LandingHealthTrail instead.</summary>
    public const float Seconds = .36f;
    public static readonly Color HurtTint = new Color32(255, 112, 92, 255), ChipColor = new Color32(255, 244, 206, 255);
    private Vector3 chipScale = Vector3.one;
    private bool chipCaptured;
    public float ChipOpacity => chip != null && chip.gameObject.activeSelf ? chip.color.a : 0;

    /// <summary>0..1 severity: the share of Resistance this strike dealt, with a floor for any damage and a lift for the finishing blow.</summary>
    public static float Severity(decimal before, decimal after, decimal resistance)
    {
        if (after <= before || resistance <= 0) return 0;
        float share = (float)((after - before) / resistance);
        float value = Mathf.Clamp01(.25f + share * 1.5f);
        return after >= resistance ? Mathf.Max(value, .85f) : value;
    }

    /// <summary>Knockback away from the attacking Strike panel (positive x) with a short decaying shake; zero outside the reaction.</summary>
    public static Vector2 Knockback(float age, float severity)
    {
        if (age < 0 || age > Seconds || severity <= 0) return Vector2.zero;
        float push = (1 - Mathf.Exp(-age * 90)) * Mathf.Exp(-age * 11) * (1 - age / Seconds);
        float shake = Mathf.Sin(age * 95) * Mathf.Exp(-age * 14) * (1 - age / Seconds);
        return new Vector2((8 + 20 * severity) * push + (2 + 4 * severity) * shake, (1.5f + 2 * severity) * Mathf.Sin(age * 131) * Mathf.Exp(-age * 16));
    }

    /// <summary>Tilt in degrees as the card is knocked back (its top falls away from the attacker, clockwise).</summary>
    public static float Tilt(float age, float severity)
    {
        if (age < 0 || age > Seconds || severity <= 0) return 0;
        return -(1.5f + 4 * severity) * (1 - Mathf.Exp(-age * 70)) * Mathf.Exp(-age * 12) * (1 - age / Seconds);
    }

    /// <summary>Card scale multipliers: a brief horizontal squash from the blow, area roughly preserved.</summary>
    public static Vector2 Squash(float age, float severity)
    {
        if (age < 0 || age > Seconds || severity <= 0) return Vector2.one;
        float k = .07f * severity * Mathf.Exp(-age * 18) * Mathf.Cos(age * 30) * (1 - age / Seconds);
        return new Vector2(1 - k, 1 + k * .7f);
    }

    /// <summary>Hurt tint over the creature panel: a sharp attack and fast decay; softer with reduced motion.</summary>
    public static float HurtFlash(float age, float severity, bool reduced)
    {
        if (age < 0 || age > Seconds || severity <= 0) return 0;
        float peak = (reduced ? .3f : .45f) + (reduced ? .1f : .3f) * severity;
        return age < .018f ? peak * age / .018f : peak * Mathf.Exp(-(age - .018f) * 15);
    }

    /// <summary>Horizontal thrash of the moving fish in the timing glass while the clock is held.</summary>
    public static float FishThrash(float age, float severity)
    {
        if (age < 0 || age > Seconds || severity <= 0) return 0;
        return (3 + 7 * severity) * Mathf.Sin(age * 70) * Mathf.Exp(-age * 9) * (1 - age / Seconds);
    }

    /// <summary>
    /// The white damage trail on the draining bar, as remaining-Resistance anchors (x = draining red edge, y = trail edge). The
    /// red eases from the old value to the new one; the trail holds on the old value, then eases down until it meets the red.
    /// </summary>
    public static Vector2 Trail(decimal before, decimal after, decimal resistance, float age, float intensity)
    {
        var span = ChipSpan(before, after, resistance);
        float red = Mathf.Lerp(span.y, span.x, LandingHealthTrail.Red(age, intensity));
        float trail = Mathf.Lerp(span.y, span.x, LandingHealthTrail.Trail(age, intensity));
        return new Vector2(red, trail);
    }

    /// <summary>Samples the reaction at a score-sequence time; call after the collision has written its own pose for the frame.</summary>
    public void Render(LandingScoreSequence sequence, LandingStrikeReceipt receipt, float elapsed, bool reduced)
    {
        if (sequence == null || receipt == null) { Clear(); return; }
        float age = elapsed - sequence.PayoutAt;
        float severity = Severity(receipt.PointsBefore, receipt.PointsAfter, receipt.Resistance);
        RenderChip(receipt, age, severity, sequence.Intensity, reduced);
        if (severity <= 0 || age < 0 || age > Seconds) return;
        if (creatureFlash != null) creatureFlash.Show(HurtFlash(age, severity, reduced), HurtTint);
        if (reduced || collision == null || !collision.IsCaptured) return;
        // The collision resets the card to its rest pose every frame before posing it, so this composes rather than accumulates.
        Vector2 knock = Knockback(age, severity), squash = Squash(age, severity);
        creatureCard.anchoredPosition += knock;
        creatureCard.localRotation *= Quaternion.Euler(0, 0, Tilt(age, severity));
        creatureCard.localScale = Vector3.Scale(creatureCard.localScale, new Vector3(squash.x, squash.y, 1));
        if (fish != null) fish.anchoredPosition += Vector2.right * FishThrash(age, severity);
    }

    /// <summary>Remaining-Resistance share the chip spans: from what is left after the hit up to what was left before it.</summary>
    public static Vector2 ChipSpan(decimal before, decimal after, decimal resistance)
    {
        if (resistance <= 0) return Vector2.zero;
        return new Vector2(Mathf.Clamp01(1 - (float)(after / resistance)), Mathf.Clamp01(1 - (float)(before / resistance)));
    }

    /// <summary>Draws the white trail between the draining red edge and the trail edge; it disappears once they meet.</summary>
    private void RenderChip(LandingStrikeReceipt receipt, float age, float severity, float intensity, bool reduced)
    {
        if (chip == null) return;
        var trail = severity > 0 && age >= 0 ? Trail(receipt.PointsBefore, receipt.PointsAfter, receipt.Resistance, age, intensity) : Vector2.zero;
        bool visible = trail.y - trail.x > .002f;
        chip.gameObject.SetActive(visible);
        if (!visible) return;
        if (!chipCaptured) { chipScale = chip.rectTransform.localScale; chipCaptured = true; }
        var rect = chip.rectTransform;
        rect.anchorMin = new Vector2(trail.x, 0); rect.anchorMax = new Vector2(trail.y, 1);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        chip.color = ChipColor;
        // A quick vertical swell sells the hit; reduced motion keeps the chip still.
        float swell = reduced || age < 0 ? 1 : 1 + .35f * Mathf.Exp(-age * 20);
        rect.localScale = new Vector3(chipScale.x, chipScale.y * swell, chipScale.z);
    }

    /// <summary>Hides the chip and restores its scale; the collision restores the card and flash it owns.</summary>
    public void Clear()
    {
        if (chip != null) { chip.gameObject.SetActive(false); if (chipCaptured) chip.rectTransform.localScale = chipScale; }
    }

    private void OnDisable() => Clear();
}
