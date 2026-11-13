using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>Deterministic Landing mechanics/scoring checks independent of artwork, input scheduling and player saves.</summary>
public static class LandingChallengeChecks
{
    private static int checks;
    /// <summary>Exercises overlap, attempts, scoring attribution, positional snapshots, motion and full-value failure rewards.</summary>
    public static int ValidateRules()
    {
        checks = 0;
        var tuning = Settings();
        Check(LandingChallengeRuntime.Grade(.5, .04, .5, .24, .08) == LandingAccuracy.Perfect, "Centered fish was not Perfect.");
        Check(LandingChallengeRuntime.Grade(.59, .04, .5, .24, .08) == LandingAccuracy.Good, "Outer overlap was not Good.");
        Check(LandingChallengeRuntime.Grade(.8, .04, .5, .24, .08) == LandingAccuracy.Miss, "Outside fish was not Miss.");
        Check(LandingChallengeRuntime.Grade(.625, .25, .5, .5, .0) == LandingAccuracy.Perfect, "Inclusive overlap did not use the fish interval.");
        Check(LandingScoring.Resolve(LandingAccuracy.Good, tuning, Array.Empty<LandingRigMember>()).Points == 10, "Good base differs from tuning.");
        Check(LandingScoring.Resolve(LandingAccuracy.Perfect, tuning, Array.Empty<LandingRigMember>()).Points == 20, "Perfect base differs from tuning.");
        var bonus = Member(1, "Bonus", CatchAttachmentRole.Catch, new[] { "Schooling" }, new LandingRigRule("Flat", LandingAccuracyFilter.Successful, 5, 1));
        var multiple = Member(2, "Multiplier", CatchAttachmentRole.Bait, new[] { "Fish" }, new LandingRigRule("Double", LandingAccuracyFilter.Perfect, 0, 2));
        var build = new[] { bonus, multiple };
        var score = LandingScoring.Resolve(LandingAccuracy.Perfect, tuning, build);
        Check(score.Base == 20 && score.Bonuses == 5 && score.Multiplier == 2 && score.Points == 50, "Flat-before-product model is wrong.");
        Check(score.Contributions.Count == 2 && score.Contributions[0].SourceId == 1 && score.Contributions[1].SourceId == 2, "Contribution identity/order changed.");
        Check(LandingScoring.Resolve(LandingAccuracy.Miss, tuning, build).Points == 0, "Regular bonuses scored on a Miss.");
        var explicitMiss = Member(3, "Miss exception", CatchAttachmentRole.Bait, Array.Empty<string>(), new LandingRigRule("Explicit Miss", LandingAccuracyFilter.Miss, 3, 2));
        Check(LandingScoring.Resolve(LandingAccuracy.Miss, tuning, new[] { bonus, explicitMiss }).Points == 6, "Explicit Miss rule was ignored or gained ordinary bonuses.");
        var fraction = Member(4, "Fraction", CatchAttachmentRole.Catch, Array.Empty<string>(), new LandingRigRule("Fraction", LandingAccuracyFilter.Successful, .5f, 1.5f));
        Check(LandingScoring.Resolve(LandingAccuracy.Good, tuning, new[] { fraction }).Points == 15.75m, "Fractional scoring was truncated.");
        var source = Member(5, "School source", CatchAttachmentRole.Bait, new[] { "Schooling" }, new LandingRigRule("Connected", LandingAccuracyFilter.Successful, 3, 1, null, "Schooling"));
        var other = Member(6, "Other", CatchAttachmentRole.Catch, new[] { "Schooling", "Fish" });
        var third = Member(7, "Third", CatchAttachmentRole.Bait, new[] { "Schooling" });
        Check(LandingScoring.Resolve(LandingAccuracy.Good, tuning, new[] { other, source, third }).Points == 16, "Both-role contiguous group count is wrong.");
        Check(LandingScoring.Resolve(LandingAccuracy.Good, tuning, new[] { source, multiple, third }).Points == 10, "Connected group crossed a nonmatching member.");
        var adjacent = Member(8, "Adjacency", CatchAttachmentRole.Bait, new[] { "Predator" }, new LandingRigRule("Neighbor", LandingAccuracyFilter.Perfect, 0, 1.5f, "Fish"));
        Check(LandingScoring.Resolve(LandingAccuracy.Perfect, tuning, new[] { adjacent, other }).Points == 30, "Neighbor condition did not match Catch.");
        Check(LandingScoring.Resolve(LandingAccuracy.Good, tuning, new[] { adjacent, other }).Points == 10, "Perfect-only condition affected Good.");
        Check(LandingScoring.Resolve(LandingAccuracy.Perfect, tuning, new[] { adjacent, third }).Points == 20, "Missing neighbor condition activated.");
        var copiedTags = new[] { "Fish" }; var copiedRules = new[] { new LandingRigRule("Copy", LandingAccuracyFilter.Successful, 1, 1) };
        var copied = new LandingRigMember(9, "Snapshot", CatchAttachmentRole.Catch, copiedTags, copiedRules);
        copiedTags[0] = "Heavy"; copiedRules[0] = null;
        Check(copied.HasTag("Fish") && copied.Rules[0] != null, "Caller mutated an encounter snapshot.");

        var runtime = new LandingChallengeRuntime(); double clock = 0;
        runtime.Begin(12, 40, tuning, Array.Empty<LandingRigMember>(), new System.Random(10), clock);
        long oldOccurrence = runtime.Occurrence, oldRevision = runtime.Revision;
        Strike(runtime, LandingAccuracy.Good, ref clock);
        Check(runtime.Points == 10 && runtime.AttemptsRemaining == 3, "Good strike did not consume one attempt.");
        Check(!runtime.TryStrike(oldOccurrence, oldRevision, clock + 1), "Stale strike accepted.");
        Check(!runtime.TryStrike(runtime.Occurrence, runtime.Revision, clock), "Scoring freeze allowed another strike.");
        double fish = runtime.FishPosition, zone = runtime.ZonePosition;
        runtime.AdvanceTo(clock + .1);
        Check(runtime.FishPosition == fish && runtime.ZonePosition == zone, "Indicators moved during scoring freeze.");
        clock += .23; runtime.AdvanceTo(clock);
        Strike(runtime, LandingAccuracy.Miss, ref clock); Strike(runtime, LandingAccuracy.Perfect, ref clock); Strike(runtime, LandingAccuracy.Good, ref clock);
        Check(runtime.State == LandingChallengeState.Succeeded && runtime.Points == 40 && runtime.AttemptsRemaining == 0, "Final-attempt success lost to failure.");
        Check(!runtime.TryStrike(runtime.Occurrence, runtime.Revision, clock + 10), "Terminal success accepted a strike.");
        runtime.Begin(13, 20, tuning, Array.Empty<LandingRigMember>(), new System.Random(3), clock);
        Strike(runtime, LandingAccuracy.Perfect, ref clock);
        Check(runtime.State == LandingChallengeState.Succeeded && runtime.AttemptsRemaining == 3, "Early success did not end immediately.");
        runtime.Begin(14, 999, tuning, Array.Empty<LandingRigMember>(), new System.Random(4), clock);
        for (int i = 0; i < 4; i++) Strike(runtime, LandingAccuracy.Miss, ref clock);
        Check(runtime.State == LandingChallengeState.Failed && runtime.Points == 0 && runtime.AttemptsRemaining == 0, "Misses did not exhaust attempts.");
        runtime.Reset(); Check(runtime.Points == 0 && runtime.LastScore == null && runtime.Rig.Count == 0 && runtime.Occurrence != oldOccurrence, "Reset retained encounter state.");

        var patterns = new HashSet<LandingMovementPattern>(); bool sawWarning = false;
        for (int seed = 0; seed < 12; seed++)
        {
            runtime.Begin(seed, 40, tuning, Array.Empty<LandingRigMember>(), new System.Random(seed), 0); patterns.Add(runtime.Pattern);
            bool bounded = true;
            for (int tick = 0; tick < 200; tick++)
            {
                runtime.AdvanceTo(tick * .05);
                bounded &= runtime.FishPosition >= runtime.FishWidth * .5 - 1e-9 && runtime.FishPosition <= 1 - runtime.FishWidth * .5 + 1e-9
                    && runtime.ZonePosition >= runtime.GoodWidth * .5 - 1e-9 && runtime.ZonePosition <= 1 - runtime.GoodWidth * .5 + 1e-9;
                sawWarning |= runtime.IsTelegraphingBurst;
            }
            Check(bounded, "Movement escaped the track.");
            var otherRuntime = new LandingChallengeRuntime(); otherRuntime.Begin(seed, 40, tuning, Array.Empty<LandingRigMember>(), new System.Random(seed), 0);
            otherRuntime.AdvanceTo(9.95);
            Check(Math.Abs(runtime.FishPosition - otherRuntime.FishPosition) < 1e-8 && Math.Abs(runtime.ZonePosition - otherRuntime.ZonePosition) < 1e-8, "Motion depends on frame subdivision.");
        }
        Check(patterns.Count == 3 && sawWarning, "Pattern selection/telegraph did not cover all three behaviors.");
        Check(tuning.Resistance(0, 0) == 40 && tuning.Resistance(120, 3) == 150, "Resistance ignored profile growth or creature override.");
        ValidateRewards();
        return checks;
    }

    /// <summary>Finds a valid future alignment using the model's real motion, without setting its positions or bypassing scoring.</summary>
    public static void Strike(LandingChallengeRuntime runtime, LandingAccuracy accuracy, ref double clock)
    {
        double end = clock + 60;
        while (clock < end)
        {
            clock += .005; runtime.AdvanceTo(clock);
            if (runtime.CanStrike && LandingChallengeRuntime.Grade(runtime.FishPosition, runtime.FishWidth, runtime.ZonePosition, runtime.GoodWidth, runtime.PerfectWidth) == accuracy)
            { Check(runtime.TryStrike(runtime.Occurrence, runtime.Revision, clock), "Legal strike rejected."); return; }
        }
        throw new InvalidOperationException("No requested alignment found in the motion fixture.");
    }

    /// <summary>Checks role-aware full-value snapshots and duplicate payout protection without touching a player wallet.</summary>
    private static void ValidateRewards()
    {
        var card = ScriptableObject.CreateInstance<CardDefinition>();
        try
        {
            var data = new SerializedObject(card); data.FindProperty("value").intValue = 11; data.ApplyModifiedPropertiesWithoutUndo();
            var catchCard = new CardInstance(1, card, null, CatchAttachmentRole.Catch);
            var bait = new CardInstance(2, card, null, CatchAttachmentRole.Bait);
            var result = new FishingRunResult(); result.RecordLandingFailure(new[] { catchCard, bait }, 3);
            Check(result.HaulValue == 11 && result.Haul.Length == 2 && result.EndedByLandingFailure && !result.EndedByLineBreak && !result.WasOverloaded, "Failure applied a sale penalty, Bait value, or capacity rule.");
            var wallet = new RunRewardRuntime(); wallet.BeginRun();
            Check(wallet.AwardGold(result) == 11 && wallet.AwardGold(result) == 0 && wallet.TotalGold == 11, "Failure payout was lost or awarded twice.");
            Check(!new DiscoveryJournalProgress().Record("landing-test", result, 1), "Landing failure counted as successful surfacing.");
        }
        finally { UnityEngine.Object.DestroyImmediate(card); }
    }

    /// <summary>Creates the documented prototype configuration for isolated model checks.</summary>
    public static LandingSettings Settings() { return new LandingSettings(4, 40, 10, 10, 20, .24, .08, .035, 3.4, .26, .22); }
    /// <summary>Creates immutable test rig data without assigning illustrative abilities to real creature assets.</summary>
    private static LandingRigMember Member(int id, string name, CatchAttachmentRole role, string[] tags, params LandingRigRule[] rules)
    { return new LandingRigMember(id, name, role, tags, rules); }
    /// <summary>Fails on the first violated invariant and records meaningful completed checks.</summary>
    private static void Check(bool value, string message) { if (!value) throw new InvalidOperationException(message); checks++; }
}
