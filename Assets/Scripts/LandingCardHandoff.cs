using UnityEngine;

/// <summary>Samples a contributor handoff inside existing preparation time without changing score or sound deadlines.</summary>
public static class LandingCardHandoff
{
    /// <summary>Skips unchanged identities and shortens the turn only when a grouped windup has less room.</summary>
    public static float DurationFor(LandingScoreSequence sequence, int index)
    {
        if (index <= 0) return 0;
        var incoming = sequence.Steps[index]; var outgoing = sequence.Steps[index - 1];
        bool same = incoming.SourceIds.Count == outgoing.SourceIds.Count;
        for (int i = 0; same && i < incoming.SourceIds.Count; i++) same = incoming.SourceIds[i] == outgoing.SourceIds[i];
        return same ? 0 : Mathf.Min(.14f, (incoming.LaunchAt - incoming.Time) * .60f);
    }

    /// <summary>Uses the same absolute progress for identity selection, card projection and native labels.</summary>
    public static float Progress(LandingScoreSequence sequence, int index, float elapsed)
    {
        float duration = DurationFor(sequence, index);
        return duration <= 0 ? 1 : Mathf.Clamp01((elapsed - sequence.Steps[index].Time) / duration);
    }

    /// <summary>Keeps the outgoing face until the hidden midpoint, including deterministic captures that skip earlier frames.</summary>
    public static int VisibleStep(LandingScoreSequence sequence, int index, float elapsed)
    {
        // A ten-microsecond tolerance keeps the hidden swap stable across float evaluation paths.
        float duration = DurationFor(sequence, index);
        return duration > 0 && elapsed + .00001f < sequence.Steps[index].Time + duration * .5f ? index - 1 : index;
    }

    /// <summary>Conceals labels around the swap and fades them with the reduced-motion face.</summary>
    public static float CaptionOpacity(float progress) => Mathf.SmoothStep(0, 1, Mathf.Clamp01((Mathf.Abs(progress - .5f) - .06f) / .44f));

    /// <summary>Hides the narrow card edge at the identity swap; reduced motion uses a stationary full-face fade.</summary>
    public static float FaceOpacity(float progress, bool reduced) => reduced ? CaptionOpacity(progress)
        : Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.04f, .15f, Mathf.Abs(progress - .5f)));
}
