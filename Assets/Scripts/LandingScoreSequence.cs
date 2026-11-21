using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Pure presentation schedule over an already resolved score; it never evaluates creature rules.</summary>
public sealed class LandingScoreSequence
{
    public readonly struct Step
    {
        public readonly int SourceId;
        public readonly decimal Value, Amount;
        public readonly bool Multiply;
        public readonly float Time;
        public readonly float LaunchAt, ImpactAt, EndAt;
        public readonly IReadOnlyList<int> SourceIds;
        /// <summary>Records an exact operation and the time at which its presentation begins.</summary>
        public Step(int sourceId, decimal value, decimal amount, bool multiply, float time, float span = .3f, int[] sources = null)
        {
            SourceId = sourceId; Value = value; Amount = amount; Multiply = multiply; Time = time;
            LaunchAt = time + span * .30f; ImpactAt = time + span * .76f; EndAt = time + span;
            SourceIds = Array.AsReadOnly(sources ?? new[] { sourceId });
        }
    }

    public IReadOnlyList<Step> Steps { get; }
    public float PayoutAt { get; }
    public float Duration { get; }
    public float Intensity { get; }
    public LandingScore Score { get; }
    public int OperationCount { get; }
    public int Tier => Intensity >= .75f ? 2 : Intensity >= .3f ? 1 : 0;

    /// <summary>Schedules every addition before every multiplier, with a bounded duration even for a large rig.</summary>
    public LandingScoreSequence(LandingScore score)
    {
        Score = score ?? throw new ArgumentNullException(nameof(score));
        double amplification = (double)(score.Points / Math.Max(1, score.Base));
        Intensity = score.Points <= 0 ? 0 : (float)Math.Max(0, Math.Min(1, Math.Log10(Math.Max(1, amplification)) / 1.5));
        var additions = score.Contributions.Where(c => c.Bonus != 0).ToArray();
        var multipliers = score.Contributions.Where(c => c.Multiplier != 1).ToArray();
        OperationCount = additions.Length + multipliers.Length;
        var steps = new List<Step> { new Step(-1, score.Base, score.Base, false, 0, .10f) };
        decimal subtotal = score.Base, product = 1;
        float time = .18f;
        // Uncapped rigs retain every operation, but present related operations together instead of unreadable rapid flashes.
        int batchSize = Math.Max(1, (int)Math.Ceiling(OperationCount / 8.0));
        for (int offset = 0; offset < additions.Length; offset += batchSize)
        {
            var group = additions.Skip(offset).Take(batchSize).ToArray(); decimal amount = 0;
            foreach (var item in group) { subtotal = checked(subtotal + item.Bonus); amount += item.Bonus; }
            float span = Math.Max(.18f, .34f - steps.Count * .025f);
            steps.Add(new Step(group[0].SourceId, subtotal, amount, false, time, span, group.Select(c => c.SourceId).ToArray()));
            time += span;
        }
        for (int offset = 0; offset < multipliers.Length; offset += batchSize)
        {
            var group = multipliers.Skip(offset).Take(batchSize).ToArray(); decimal amount = 1;
            foreach (var item in group)
            {
                product = checked(product * item.Multiplier);
                amount = amount > 1 && item.Multiplier > 1 && amount > decimal.MaxValue / item.Multiplier ? decimal.MaxValue : amount * item.Multiplier;
            }
            decimal value = product > 1 && subtotal > decimal.MaxValue / product ? decimal.MaxValue : subtotal * product;
            float span = .32f + (offset == 0 ? .08f : 0) + (offset + batchSize >= multipliers.Length ? Intensity * .14f : 0);
            steps.Add(new Step(group[0].SourceId, value, amount, true, time, span, group.Select(c => c.SourceId).ToArray()));
            time += span;
        }
        Steps = steps.AsReadOnly();
        PayoutAt = time + .10f + Intensity * .24f;
        Duration = PayoutAt + .36f + Intensity * .34f;
    }

    /// <summary>Finds the current operation without consuming animation state or changing the score.</summary>
    public int StepAt(float elapsed)
    {
        int index = 0;
        for (int i = 1; i < Steps.Count && Steps[i].Time <= elapsed; i++) index = i;
        return index;
    }
    /// <summary>Returns only delivered values; a travelling projectile cannot alter the score display.</summary>
    public decimal ValueAt(float elapsed) => elapsed < Steps[0].ImpactAt ? 0 : Steps[ImpactStepAt(elapsed)].Value;
    /// <summary>Finds the last arrival independently of the currently preparing source.</summary>
    public int ImpactStepAt(float elapsed)
    {
        int index = 0;
        for (int i = 1; i < Steps.Count && Steps[i].ImpactAt <= elapsed; i++) index = i;
        return index;
    }
    /// <summary>Uses a bounded pentatonic rise rather than continuously pitching mechanical samples into squeaks.</summary>
    public static float NotePitch(int index) => (float)Math.Pow(2, new[] { 0, 3, 5, 7, 10, 12 }[Math.Min(5, Math.Max(0, index))] / 12.0);
}
