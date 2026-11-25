using System;
using System.Linq;

/// <summary>Maps a resolved score timeline to wall time, stretching the final major impact's approach, contact and first rebound.</summary>
public sealed class LandingImpactTiming
{
    private readonly struct Hold
    {
        public readonly int Index;
        public readonly float Start, Length, Extra;
        /// <summary>Freezes one bounded slow-motion interval.</summary>
        public Hold(int index, float start, float length, float extra) { Index = index; Start = start; Length = length; Extra = extra; }
    }
    private readonly Hold[] holds;
    public float Duration { get; }
    public int FocusCount => holds.Length;

    /// <summary>Selects only the final major creature impact and bounds its extended slow-motion window within the feedback budget.</summary>
    public LandingImpactTiming(LandingScoreSequence sequence)
    {
        int last = sequence.Steps.Count - 1;
        var final = sequence.Steps[last];
        var selected = last > 0 && final.Strength >= .75f && final.Gain > 0 ? new[] { last } : Array.Empty<int>();
        float requested = selected.Sum(i => (SlowEnd(sequence.Steps[i]) - SlowStart(sequence.Steps[i])) * (1 / .28f - 1));
        float budget = Math.Max(0, LandingScoreSequence.MaximumDuration - .05f - sequence.Duration);
        float fraction = requested > 0 ? Math.Min(1, budget / requested) : 0;
        holds = selected.Select(i => new Hold(i, SlowStart(sequence.Steps[i]), SlowEnd(sequence.Steps[i]) - SlowStart(sequence.Steps[i]),
            (SlowEnd(sequence.Steps[i]) - SlowStart(sequence.Steps[i])) * (1 / .28f - 1) * fraction)).ToArray();
        Duration = sequence.Duration + holds.Sum(h => h.Extra);
    }

    /// <summary>Matches the collision's compressed contact phase, ending exactly where the attacker starts returning.</summary>
    public static float ContactHold(LandingScoreSequence.Step step) => .075f + step.Strength * .030f;
    /// <summary>Slows the last 30 ms of the canonical dash while retaining its normal-speed launch and preparation.</summary>
    public static float SlowStart(LandingScoreSequence.Step step) => step.ImpactAt - Math.Min(.030f, (step.ImpactAt - step.LaunchAt) * .3f);
    /// <summary>Lingers into the first 25 ms of rebound, retaining at least three quarters of return for normal-speed recovery.</summary>
    public static float SlowEnd(LandingScoreSequence.Step step) => step.ImpactAt + ContactHold(step)
        + Math.Min(.025f, Math.Max(0, step.EndAt - step.ImpactAt - ContactHold(step)) * .25f);
    /// <summary>Reports whether this is the receipt's qualifying final creature impact.</summary>
    public bool Focuses(int index) { foreach (var hold in holds) if (hold.Index == index) return true; return false; }

    /// <summary>Converts canonical deadlines to real deadlines for scheduled audio and the controller's cosmetic gate.</summary>
    public float ToReal(float sequenceTime)
    {
        float extra = 0;
        foreach (var hold in holds)
        {
            if (sequenceTime <= hold.Start) break;
            if (sequenceTime < hold.Start + hold.Length) return sequenceTime + extra + (sequenceTime - hold.Start) / hold.Length * hold.Extra;
            extra += hold.Extra;
        }
        return sequenceTime + extra;
    }

    /// <summary>Samples animation time from an unscaled wall clock without touching Unity time scale or gameplay.</summary>
    public float ToSequence(float realTime)
    {
        float extra = 0;
        foreach (var hold in holds)
        {
            float start = hold.Start + extra;
            if (realTime <= start) break;
            if (realTime < start + hold.Length + hold.Extra) return hold.Start + (realTime - start) * hold.Length / (hold.Length + hold.Extra);
            extra += hold.Extra;
        }
        return realTime - extra;
    }
}
