using System;
using System.Linq;

/// <summary>Maps a resolved score timeline to wall time, stretching only the final creature contact when that impact is major.</summary>
public sealed class LandingImpactTiming
{
    private readonly struct Hold
    {
        public readonly int Index;
        public readonly float Start, Length, Extra;
        /// <summary>Freezes one bounded slow-contact interval.</summary>
        public Hold(int index, float start, float length, float extra) { Index = index; Start = start; Length = length; Extra = extra; }
    }
    private readonly Hold[] holds;
    public float Duration { get; }
    public int FocusCount => holds.Length;

    /// <summary>Selects only the final major creature impact and respects the remaining feedback budget without slowing dashes or returns.</summary>
    public LandingImpactTiming(LandingScoreSequence sequence)
    {
        int last = sequence.Steps.Count - 1;
        var final = sequence.Steps[last];
        var selected = last > 0 && final.Strength >= .75f && final.Gain > 0 ? new[] { last } : Array.Empty<int>();
        float requested = selected.Sum(i => ContactHold(sequence.Steps[i]) * (1 / .28f - 1));
        float budget = Math.Max(0, LandingScoreSequence.MaximumDuration - .05f - sequence.Duration);
        float fraction = requested > 0 ? Math.Min(1, budget / requested) : 0;
        holds = selected.Select(i => new Hold(i, sequence.Steps[i].ImpactAt, ContactHold(sequence.Steps[i]), ContactHold(sequence.Steps[i]) * (1 / .28f - 1) * fraction)).ToArray();
        Duration = sequence.Duration + holds.Sum(h => h.Extra);
    }

    /// <summary>Matches the collision's compressed contact phase, ending exactly where the attacker starts returning.</summary>
    public static float ContactHold(LandingScoreSequence.Step step) => .075f + step.Strength * .030f;
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
