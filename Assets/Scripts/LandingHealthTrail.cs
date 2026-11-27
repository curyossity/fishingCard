using UnityEngine;

/// <summary>
/// Schedule of the creature's Resistance bar after a strike's points land (ages measured from the score payout). The red fill
/// eases down from the old Resistance to the new one; a white damage trail stays at the old value, holds briefly, then eases
/// down until it meets the red. The strike's feedback lasts until the trail has finished, so control never returns while the
/// bar is still moving. Shared by the score presentation (red), the damage chip (white) and the score sequence (tail length).
/// </summary>
public static class LandingHealthTrail
{
    /// <summary>White trail hold after the red has settled, its own drain, and a short settle before control returns.</summary>
    public const float Hold = .15f, DrainSeconds = .35f, Settle = .06f;
    /// <summary>Post-payout tail used before the health trail existed (readout fade, impact recovery); still the minimum.</summary>
    public static float BaseTail(float intensity) => .36f + intensity * .34f;

    /// <summary>Seconds the red fill takes to reach the new Resistance; bigger hits drain a little longer.</summary>
    public static float RedSeconds(float intensity) => .45f + intensity * .18f;

    /// <summary>Red drain progress 0..1 (ease-out): fast at first, settling onto the new value.</summary>
    public static float Red(float age, float intensity)
    {
        if (age <= 0) return 0;
        float t = Mathf.Clamp01(age / RedSeconds(intensity));
        return 1 - (1 - t) * (1 - t) * (1 - t);
    }

    public static float TrailStart(float intensity) => RedSeconds(intensity) + Hold;
    public static float End(float intensity) => TrailStart(intensity) + DrainSeconds;

    /// <summary>
    /// White trail progress 0..1 (ease in-out): stays on the old value, then slides down to the new one. It only starts once
    /// the red has settled, and it is clamped to the red's progress, so whatever the tuning it can never pass the red mid-drain.
    /// </summary>
    public static float Trail(float age, float intensity)
    {
        float u = Mathf.Clamp01((age - TrailStart(intensity)) / DrainSeconds);
        return Mathf.Min(u * u * (3 - 2 * u), Red(age, intensity));
    }

    /// <summary>Feedback tail after the payout: long enough for the whole drain when the strike dealt damage.</summary>
    public static float Tail(float intensity, bool damage) => damage ? Mathf.Max(BaseTail(intensity), End(intensity) + Settle) : BaseTail(intensity);
}
