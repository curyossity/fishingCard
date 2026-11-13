using System;
using System.Collections.Generic;

public enum LandingMovementPattern { Steady, Burst, Erratic }
public enum LandingChallengeState { Inactive, Active, Succeeded, Failed }

/// <summary>Owns encounter-local timing, attempts and scoring independently of Unity UI and artwork.</summary>
public sealed class LandingChallengeRuntime
{
    private LandingSettings settings;
    private IReadOnlyList<LandingRigMember> rig = Array.Empty<LandingRigMember>();
    private double lastClock, motionTime, fishPhase, feedbackRemaining;
    public long Occurrence { get; private set; }
    public long Revision { get; private set; }
    public int EncounterRevision { get; private set; }
    public LandingChallengeState State { get; private set; }
    public LandingMovementPattern Pattern { get; private set; }
    public decimal Resistance { get; private set; }
    public decimal Points { get; private set; }
    public int AttemptsRemaining { get; private set; }
    public LandingScore LastScore { get; private set; }
    public string Error { get; private set; }
    public double FishPosition { get; private set; }
    public double ZonePosition { get; private set; }
    public double FishWidth => settings?.FishWidth ?? 0;
    public double GoodWidth => settings?.GoodWidth ?? 0;
    public double PerfectWidth => settings?.PerfectWidth ?? 0;
    public bool IsTelegraphingBurst { get; private set; }
    public bool CanStrike => State == LandingChallengeState.Active && feedbackRemaining <= 0 && Error == null;
    public IReadOnlyList<LandingRigMember> Rig => rig;

    /// <summary>Starts a fresh occurrence from immutable tuning and only previously attached rig members.</summary>
    public void Begin(int encounterRevision, decimal resistance, LandingSettings tuning, LandingRigMember[] members, Random random, double clock)
    {
        if (tuning == null || random == null || resistance <= 0 || !Finite(clock) || clock < 0)
            throw new ArgumentException("Landing requires validated tuning, Resistance, randomness and a finite clock.");
        var snapshot = (LandingRigMember[])(members ?? Array.Empty<LandingRigMember>()).Clone();
        var ids = new HashSet<int>();
        foreach (var member in snapshot)
            if (member == null || !ids.Add(member.Id)) throw new ArgumentException("Rig snapshot requires unique attached instance identities.");
        Reset();
        settings = tuning; rig = Array.AsReadOnly(snapshot); Resistance = resistance;
        EncounterRevision = encounterRevision; AttemptsRemaining = tuning.Attempts;
        Pattern = (LandingMovementPattern)random.Next(3); fishPhase = random.NextDouble() * 4;
        lastClock = clock; State = LandingChallengeState.Active; UpdatePositions();
    }

    /// <summary>Advances motion from an absolute monotonic clock, excluding the brief scoring freeze even across a long frame.</summary>
    public void AdvanceTo(double clock)
    {
        if (State != LandingChallengeState.Active || !Finite(clock) || clock < lastClock) return;
        double delta = clock - lastClock; lastClock = clock;
        double frozen = Math.Min(delta, feedbackRemaining);
        feedbackRemaining -= frozen; motionTime += delta - frozen;
        UpdatePositions();
    }

    /// <summary>Evaluates exact strike-time overlap, computes the complete score, then consumes one attempt and resolves a terminal outcome.</summary>
    public bool TryStrike(long occurrence, long revision, double clock)
    {
        if (occurrence != Occurrence || revision != Revision || State != LandingChallengeState.Active
            || !Finite(clock) || clock < lastClock || Error != null) return false;
        AdvanceTo(clock);
        if (!CanStrike) return false;
        LandingScore score; decimal total;
        try { score = LandingScoring.Resolve(Grade(FishPosition, FishWidth, ZonePosition, GoodWidth, PerfectWidth), settings, rig); total = checked(Points + score.Points); }
        catch (OverflowException) { Error = "Landing scoring exceeds supported decimal precision. Check authored bonuses and multipliers."; return false; }
        LastScore = score; Points = total; AttemptsRemaining--; Revision++;
        feedbackRemaining = settings.FeedbackSeconds;
        // Success takes precedence on the final attempt; presentation never delays the authoritative outcome.
        if (Points >= Resistance) State = LandingChallengeState.Succeeded;
        else if (AttemptsRemaining == 0) State = LandingChallengeState.Failed;
        return true;
    }

    /// <summary>Classifies inclusive interval overlap with Perfect priority; a Miss lies wholly outside the wider Good region.</summary>
    public static LandingAccuracy Grade(double fish, double fishWidth, double zone, double goodWidth, double perfectWidth)
    {
        double distance = Math.Abs(fish - zone);
        if (distance <= (fishWidth + perfectWidth) * .5) return LandingAccuracy.Perfect;
        return distance <= (fishWidth + goodWidth) * .5 ? LandingAccuracy.Good : LandingAccuracy.Miss;
    }

    /// <summary>Invalidates stale commands and clears all encounter-local points, effects and movement state.</summary>
    public void Reset()
    {
        Occurrence++; Revision++; State = LandingChallengeState.Inactive;
        settings = null; rig = Array.Empty<LandingRigMember>(); Resistance = Points = 0;
        AttemptsRemaining = 0; LastScore = null; Error = null;
        motionTime = lastClock = feedbackRemaining = fishPhase = 0;
        FishPosition = ZonePosition = .5; IsTelegraphingBurst = false;
    }

    /// <summary>Samples deterministic learnable motion; random selection and phase occur only once at encounter start.</summary>
    private void UpdatePositions()
    {
        ZonePosition = settings.GoodWidth * .5 + (1 - settings.GoodWidth)
            * (.5 + .5 * Math.Sin(motionTime * 2 * Math.PI / settings.ZonePeriod));
        double t = motionTime + fishPhase;
        double position;
        IsTelegraphingBurst = false;
        if (Pattern == LandingMovementPattern.Steady) position = Reflect(t * settings.FishSpeed);
        else if (Pattern == LandingMovementPattern.Burst)
        {
            var movement = settings.Motion;
            double normal = movement.NormalSeconds, warning = movement.WarningSeconds, burst = movement.BurstSeconds;
            double cycle = normal + warning + burst;
            double phase = t % cycle;
            IsTelegraphingBurst = phase >= normal && phase < normal + warning;
            double distance = Math.Floor(t / cycle) * (normal + warning * movement.WarningMultiplier + burst * movement.BurstMultiplier);
            distance += Math.Min(phase, normal);
            distance += Math.Min(Math.Max(phase - normal, 0), warning) * movement.WarningMultiplier;
            distance += Math.Max(phase - normal - warning, 0) * movement.BurstMultiplier;
            position = Reflect(distance * settings.FishSpeed);
        }
        else
        {
            var keys = settings.Motion.ErraticPositions;
            double cursor = t / settings.Motion.ErraticStepSeconds;
            int segment = (int)(Math.Floor(cursor) % (keys.Count - 1));
            double blend = cursor - Math.Floor(cursor);
            position = keys[segment] + (keys[segment + 1] - keys[segment]) * blend;
        }
        FishPosition = settings.FishWidth * .5 + position * (1 - settings.FishWidth);
    }

    /// <summary>Reflects an analytical travel distance at each track endpoint without frame-step overshoot.</summary>
    private static double Reflect(double distance) { return 1 - Math.Abs(distance % 2 - 1); }
    /// <summary>Rejects invalid timestamps without modifying the encounter clock.</summary>
    private static bool Finite(double value) { return !double.IsNaN(value) && !double.IsInfinity(value); }
}
