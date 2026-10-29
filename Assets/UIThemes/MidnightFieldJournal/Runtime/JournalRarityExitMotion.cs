using UnityEngine;

/// <summary>Shared one-second elastic motion matching amplitude 1 and period 0.74.</summary>
public static class JournalRarityExitMotion
{
    public const float Duration = 1f;

    /// <summary>Evaluates normalized elastic progress, retaining the negative wind-up before the upward exit.</summary>
    public static float Evaluate(float progress)
    {
        float t = Mathf.Clamp01(progress);
        if (t == 0 || t == 1) return t;
        return Mathf.Pow(2, 10 * (t - 1)) * Mathf.Cos((1 - t) * 2 * Mathf.PI / .74f);
    }

    /// <summary>Mirrors the exit in space and time: below the viewport to rest, with elastic settling.</summary>
    public static Vector2 EntranceOffset(float progress,float viewportHeight)
    { return Vector2.down*Mathf.Max(0,viewportHeight)*Evaluate(1-Mathf.Clamp01(progress)); }
}
