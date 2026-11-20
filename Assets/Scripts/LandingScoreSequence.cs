using System;
using System.Collections.Generic;

/// <summary>Pure presentation schedule over an already resolved score; it never evaluates creature rules.</summary>
public sealed class LandingScoreSequence
{
    public readonly struct Step
    {
        public readonly int SourceId;
        public readonly decimal Value, Amount;
        public readonly bool Multiply;
        public readonly IReadOnlyList<int> SupportingIds;
        public readonly float Time, LaunchAt, ImpactAt;
        /// <summary>Records an exact operation and the time at which its presentation begins.</summary>
        public Step(int sourceId, decimal value, decimal amount, bool multiply, float time, float span = 0, IReadOnlyList<int> supportingIds = null)
        { SourceId = sourceId; Value = value; Amount = amount; Multiply = multiply; Time = time; LaunchAt = time + span * .2f; ImpactAt = time + span * .72f; SupportingIds = supportingIds ?? Array.Empty<int>(); }
    }

    public IReadOnlyList<Step> Steps { get; }
    public float PayoutAt { get; }
    public float Duration { get; }
    public float Intensity { get; }
    public LandingScore Score { get; }

    /// <summary>Schedules every addition before every multiplier, with a bounded duration even for a large rig.</summary>
    public LandingScoreSequence(LandingScore score)
    {
        Score = score ?? throw new ArgumentNullException(nameof(score));
        double amplification = (double)(score.Points / Math.Max(1, score.Base));
        Intensity = score.Points <= 0 ? 0 : (float)Math.Max(0, Math.Min(1, Math.Log10(Math.Max(1, amplification)) / 1.5));
        int count = 0;
        foreach (var item in score.Contributions) { if (item.Bonus != 0) count++; if (item.Multiplier != 1) count++; }
        float spacing = count == 0 ? 0 : Math.Min(.30f + Intensity * .10f, 1.38f / (count + .75f));
        var steps = new List<Step> { new Step(-1, score.Base, score.Base, false, .06f) };
        decimal subtotal = score.Base, product = 1;
        foreach (var item in score.Contributions)
            if (item.Bonus != 0)
            {
                subtotal = checked(subtotal + item.Bonus);
                steps.Add(new Step(item.SourceId, subtotal, item.Bonus, false, .06f + steps.Count * spacing, spacing, item.SupportingIds));
            }
        foreach (var item in score.Contributions)
            if (item.Multiplier != 1)
            {
                product = checked(product * item.Multiplier);
                // A later zero multiplier can make the final score representable even if a prefix is too large.
                decimal value = product > 1 && subtotal > decimal.MaxValue / product ? decimal.MaxValue : subtotal * product;
                steps.Add(new Step(item.SourceId, value, item.Multiplier, true, .06f + steps.Count * spacing, spacing, item.SupportingIds));
            }
        Steps = steps.AsReadOnly();
        PayoutAt = steps[steps.Count - 1].ImpactAt + .20f + Intensity * .14f;
        Duration = PayoutAt + .65f + Intensity * .3f;
    }

    /// <summary>Finds the current operation without consuming animation state or changing the score.</summary>
    public int StepAt(float elapsed)
    {
        int index = 0;
        for (int i = 1; i < Steps.Count && Steps[i].Time <= elapsed; i++) index = i;
        return index;
    }

    /// <summary>Finds the last arrived operation; launched projectiles cannot increase the visible score early.</summary>
    public int ImpactStepAt(float elapsed)
    {
        int index = 0;
        for (int i = 1; i < Steps.Count && Steps[i].ImpactAt <= elapsed; i++) index = i;
        return index;
    }
    /// <summary>Returns the exact resolved prefix that has visually reached the readout.</summary>
    public decimal ValueAt(float elapsed) => Steps[ImpactStepAt(elapsed)].Value;
}
