using UnityEngine;

/// <summary>Samples vertical identity turns, distinguishing encounter/rig boundaries from ordinary contributor changes.</summary>
public static class LandingCardHandoff
{
    public readonly struct Frame
    {
        public readonly bool Active;
        public readonly float Progress;
        public readonly int Turns, VisibleStep;
        public float FacePhase => Progress >= 1 ? 1 : Progress * Turns % 1;

        /// <summary>Freezes one sampled handoff so identity, projection and opacity share the same clock.</summary>
        public Frame(float start, float duration, float elapsed, int turns, int outgoing, int incoming)
        {
            Active = duration > 0 && elapsed >= start && elapsed < start + duration;
            Progress = duration > 0 ? Mathf.Clamp01((elapsed - start) / duration) : 1;
            Turns = turns;
            // Float evaluation paths must agree on the fully hidden middle turn.
            VisibleStep = Active && elapsed + .00001f < start + duration * .5f ? outgoing : incoming;
        }
    }

    /// <summary>Uses three turns to enter/leave rig scoring, one between contributors, and none for unchanged identities.</summary>
    public static Frame Sample(LandingScoreSequence sequence, float elapsed)
    {
        if (sequence.HasContributors)
        {
            if (elapsed >= sequence.EntryAt && elapsed < sequence.Steps[1].Time)
                return new Frame(sequence.EntryAt, LandingScoreSequence.BoundaryTurnDuration, elapsed, 3, 0, 1);
            if (elapsed >= sequence.ReturnAt)
                return new Frame(sequence.ReturnAt, LandingScoreSequence.BoundaryTurnDuration, elapsed, 3, sequence.Steps.Count - 1, 0);
        }
        int index = sequence.StepAt(elapsed);
        return new Frame(sequence.Steps[index].Time, DurationFor(sequence, index), elapsed, 1, index - 1, index);
    }

    /// <summary>Skips the separately staged first source and unchanged identities, shortening only compressed grouped windups.</summary>
    public static float DurationFor(LandingScoreSequence sequence, int index)
    {
        if (index <= 1) return 0;
        var incoming = sequence.Steps[index]; var outgoing = sequence.Steps[index - 1];
        bool same = incoming.SourceIds.Count == outgoing.SourceIds.Count;
        for (int i = 0; same && i < incoming.SourceIds.Count; i++) same = incoming.SourceIds[i] == outgoing.SourceIds[i];
        return same ? 0 : Mathf.Min(.14f, (incoming.LaunchAt - incoming.Time) * .60f);
    }

    /// <summary>Conceals labels around the swap and fades them with the reduced-motion face.</summary>
    public static float CaptionOpacity(float progress) => Mathf.SmoothStep(0, 1, Mathf.Clamp01((Mathf.Abs(progress - .5f) - .06f) / .44f));

    /// <summary>Hides the narrow card edge at the identity swap; reduced motion uses a stationary full-face fade.</summary>
    public static float FaceOpacity(float progress, bool reduced) => reduced ? CaptionOpacity(progress)
        : Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.04f, .15f, Mathf.Abs(progress - .5f)));
}
