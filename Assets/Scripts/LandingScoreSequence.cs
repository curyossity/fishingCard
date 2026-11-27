using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Pure presentation schedule over an already resolved score; it never evaluates creature rules.</summary>
public sealed class LandingScoreSequence
{
    public const float MaximumDuration = 7.5f;
    /// <summary>One Strike-panel turn: entering and leaving rig scoring take the same single turn as a change between contributors.</summary>
    public const float BoundaryTurnDuration = .14f;
    public readonly struct Step
    {
        public readonly int SourceId;
        public readonly decimal Value, Amount;
        public readonly decimal PreviousValue, Gain;
        public readonly float Strength;
        public readonly bool Multiply;
        public readonly float Time;
        public readonly float LaunchAt, ImpactAt, EndAt;
        public readonly IReadOnlyList<int> SourceIds;
        /// <summary>Freezes an exact operation, delivered gain and strength-dependent presentation deadlines.</summary>
        public Step(int sourceId, decimal value, decimal amount, bool multiply, float time, float span = .3f, int[] sources = null, decimal previousValue = 0, float strength = 0)
        {
            SourceId = sourceId; Value = value; Amount = amount; Multiply = multiply; Time = time;
            PreviousValue = previousValue; Gain = value - previousValue; Strength = strength;
            // Reserve a short accelerating dash and enough recovery for a complete rigid-card rebound.
            ImpactAt = time + span * .58f;
            LaunchAt = ImpactAt - (sourceId < 0 ? .110f : .100f + .030f * strength); EndAt = time + span;
            SourceIds = Array.AsReadOnly(sources ?? new[] { sourceId });
        }
    }

    public IReadOnlyList<Step> Steps { get; }
    public float PayoutAt { get; }
    public float EntryAt { get; }
    public float ReturnAt { get; }
    public bool HasContributors => Steps.Count > 1;
    public float Duration { get; }
    public float Intensity { get; }
    public LandingScore Score { get; }
    public int OperationCount { get; }
    public int Tier => Intensity >= .75f ? 2 : Intensity >= .3f ? 1 : 0;

    /// <summary>Schedules canonical additions before multipliers with gain-based emphasis and bounded grouped pacing.</summary>
    public LandingScoreSequence(LandingScore score)
    {
        Score = score ?? throw new ArgumentNullException(nameof(score));
        double amplification = (double)(score.Points / Math.Max(1, score.Base));
        Intensity = score.Points <= 0 ? 0 : (float)Math.Max(0, Math.Min(1, Math.Log10(Math.Max(1, amplification)) / 1.5));
        var additions = score.Contributions.Where(c => c.Bonus != 0).ToArray();
        var multipliers = score.Contributions.Where(c => c.Multiplier != 1).ToArray();
        OperationCount = additions.Length + multipliers.Length;
        bool scoring = score.Points > 0;
        var steps = new List<Step> { new Step(-1, score.Base, score.Base, false, 0, scoring ? .52f : .28f) };
        decimal subtotal = score.Base, product = 1;
        float time = scoring ? .60f : .34f;
        // Uncapped rigs retain every operation, but present related operations together instead of unreadable rapid flashes.
        int batchSize = Math.Max(1, (int)Math.Ceiling(OperationCount / 8.0));
        for (int offset = 0; offset < additions.Length; offset += batchSize)
        {
            var group = additions.Skip(offset).Take(batchSize).ToArray(); decimal amount = 0;
            foreach (var item in group) { subtotal = checked(subtotal + item.Bonus); amount += item.Bonus; }
            decimal previous = steps[steps.Count - 1].Value;
            float strength = StrengthFor(previous, subtotal, score.Base);
            float span = Math.Max(.50f, .58f + strength * .18f - (steps.Count - 1) * .009f);
            steps.Add(new Step(group[0].SourceId, subtotal, amount, false, time, span, group.Select(c => c.SourceId).ToArray(), previous, strength));
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
            decimal previous = steps[steps.Count - 1].Value;
            float strength = StrengthFor(previous, value, score.Base);
            float span = .68f + strength * .22f;
            steps.Add(new Step(group[0].SourceId, value, amount, true, time, span, group.Select(c => c.SourceId).ToArray(), previous, strength));
            time += span;
        }
        // Compress only time above the readable per-beat floor, retaining arithmetic and every grouped identity.
        const float minimumSpan = .50f, modifierBudget = 4.76f;
        float boundaryTime = steps.Count > 1 ? BoundaryTurnDuration : 0;
        // Keep collision pacing independent of the encounter transition speed.
        float extra = steps.Skip(1).Sum(s => Math.Max(0, s.EndAt - s.Time - minimumSpan));
        float available = Math.Max(0, modifierBudget - (steps.Count - 1) * minimumSpan);
        float tempo = extra > available ? available / extra : 1;
        EntryAt = scoring ? .60f : .34f;
        time = EntryAt + boundaryTime;
        for (int i = 1; i < steps.Count; i++)
        {
            var step = steps[i]; float span = minimumSpan + (step.EndAt - step.Time - minimumSpan) * tempo;
            steps[i] = new Step(step.SourceId, step.Value, step.Amount, step.Multiply, time, span, step.SourceIds.ToArray(), step.PreviousValue, step.Strength);
            time += span;
        }
        Steps = steps.AsReadOnly();
        ReturnAt = time;
        PayoutAt = ReturnAt + boundaryTime + .10f + Intensity * .24f;
        // A damaging strike holds control until the Resistance bar's red drain and white trail have both finished.
        Duration = PayoutAt + LandingHealthTrail.Tail(Intensity, score.Points > 0);
    }

    /// <summary>Balances relative growth and points gained against the accuracy base, so nominally small multipliers cannot fake a huge impact.</summary>
    private static float StrengthFor(decimal before, decimal after, decimal basis)
    {
        double gain = Math.Max(0, (double)(after - before));
        double relative = Math.Min(1, Math.Log10(1 + gain / Math.Max(1, (double)before)) / Math.Log10(11));
        double absolute = Math.Min(1, Math.Log10(1 + gain / Math.Max(1, (double)basis)) / Math.Log10(51));
        return (float)(relative * .4 + absolute * .6);
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
