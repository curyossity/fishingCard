using System;
using UnityEngine;

/// <summary>Authored prototype tuning; an expedition receives a validated immutable copy.</summary>
[CreateAssetMenu(fileName = "LandingChallenge", menuName = "Fishing Cards/Landing Challenge Profile")]
public sealed class LandingChallengeProfile : ScriptableObject
{
    [SerializeField, Min(1)] private int attempts = 4;
    [SerializeField, Min(1)] private int defaultResistance = 40;
    [SerializeField, Min(0)] private int resistancePerDepth = 10;
    [SerializeField, Min(0)] private float goodPoints = 10, perfectPoints = 20;
    [SerializeField, Range(.01f, 1)] private float goodWidth = .24f, perfectWidth = .08f, fishWidth = .035f;
    [SerializeField, Min(.1f)] private float zonePeriod = 3.4f;
    [SerializeField, Min(.01f)] private float fishSpeed = .26f;
    [SerializeField, Min(.1f)] private float feedbackSeconds = .22f;
    [Header("Movement Prototype")]
    [SerializeField, Min(.01f)] private float burstNormalSeconds = 1.8f, burstWarningSeconds = .35f, burstSeconds = .45f;
    [SerializeField, Min(0)] private float warningSpeedMultiplier = .25f, burstSpeedMultiplier = 2.6f;
    [SerializeField, Min(.01f)] private float erraticStepSeconds = .65f;
    [SerializeField] private float[] erraticPositions = { .12f, .75f, .75f, .4f, .65f, .18f, .18f, .5f, .12f };

    /// <summary>Rejects invalid authoring before a run can change its current state.</summary>
    public LandingSettings CreateSettings()
    {
        return new LandingSettings(attempts, defaultResistance, resistancePerDepth, goodPoints, perfectPoints,
            goodWidth, perfectWidth, fishWidth, zonePeriod, fishSpeed, feedbackSeconds,
            new LandingMotionSettings(burstNormalSeconds, burstWarningSeconds, burstSeconds, warningSpeedMultiplier,
                burstSpeedMultiplier, erraticStepSeconds, Array.ConvertAll(erraticPositions, value => (double)value)));
    }
}

/// <summary>Frozen challenge tuning; widths and speeds are normalized to the track rather than pixels.</summary>
public sealed class LandingSettings
{
    public int Attempts { get; }
    public int DefaultResistance { get; }
    public int ResistancePerDepth { get; }
    public decimal GoodPoints { get; }
    public decimal PerfectPoints { get; }
    public double GoodWidth { get; }
    public double PerfectWidth { get; }
    public double FishWidth { get; }
    public double ZonePeriod { get; }
    public double FishSpeed { get; }
    public double FeedbackSeconds { get; }
    public LandingMotionSettings Motion { get; }

    /// <summary>Creates an immutable configuration and validates nested zones, finite motion and nonnegative scoring.</summary>
    public LandingSettings(int attempts, int resistance, int resistancePerDepth, float goodPoints, float perfectPoints,
        double goodWidth, double perfectWidth, double fishWidth, double zonePeriod, double fishSpeed, double feedbackSeconds, LandingMotionSettings motion = null)
    {
        if (attempts < 1 || resistance < 1 || resistancePerDepth < 0 || !Finite(goodPoints) || !Finite(perfectPoints)
            || goodPoints < 0 || perfectPoints < goodPoints || !Finite(goodWidth) || !Finite(perfectWidth) || !Finite(fishWidth)
            || perfectWidth <= 0 || perfectWidth > goodWidth || goodWidth >= 1 || fishWidth <= 0 || fishWidth >= 1
            || !Finite(zonePeriod) || zonePeriod <= 0 || !Finite(fishSpeed) || fishSpeed <= 0
            || !Finite(feedbackSeconds) || feedbackSeconds < 0)
            throw new ArgumentException("Landing tuning requires finite positive movement, nested zones, attempts and Resistance.");
        Attempts = attempts; DefaultResistance = resistance; ResistancePerDepth = resistancePerDepth;
        GoodPoints = (decimal)goodPoints; PerfectPoints = (decimal)perfectPoints;
        GoodWidth = goodWidth; PerfectWidth = perfectWidth; FishWidth = fishWidth;
        ZonePeriod = zonePeriod; FishSpeed = fishSpeed; FeedbackSeconds = feedbackSeconds;
        Motion = motion ?? LandingMotionSettings.Prototype();
    }

    /// <summary>Resolves an optional creature-specific base independently of Weight, using editable prototype depth growth.</summary>
    public decimal Resistance(int creatureBase, int progressedDepth)
    { return (creatureBase > 0 ? creatureBase : DefaultResistance) + (decimal)Math.Max(0, progressedDepth) * ResistancePerDepth; }

    /// <summary>Identifies invalid authored floating-point values before entering a challenge.</summary>
    private static bool Finite(double value) { return !double.IsNaN(value) && !double.IsInfinity(value); }
}

/// <summary>Immutable analytical movement parameters, including copied loop keys for learnable erratic motion.</summary>
public sealed class LandingMotionSettings
{
    public double NormalSeconds { get; }
    public double WarningSeconds { get; }
    public double BurstSeconds { get; }
    public double WarningMultiplier { get; }
    public double BurstMultiplier { get; }
    public double ErraticStepSeconds { get; }
    public System.Collections.Generic.IReadOnlyList<double> ErraticPositions { get; }
    /// <summary>Validates finite motion and a continuous normalized loop before any challenge is entered.</summary>
    public LandingMotionSettings(double normal, double warning, double burst, double warningMultiplier, double burstMultiplier, double step, double[] positions)
    {
        foreach (double value in new[] { normal, warning, burst, warningMultiplier, burstMultiplier, step })
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0) throw new ArgumentException("Movement values must be finite and nonnegative.");
        if (normal <= 0 || warning <= 0 || burst <= 0 || step <= 0 || positions == null || positions.Length < 2
            || positions[0] != positions[positions.Length - 1]) throw new ArgumentException("Movement needs positive periods and a closed erratic loop.");
        foreach (double value in positions)
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0 || value > 1) throw new ArgumentException("Erratic keys must stay within the track.");
        NormalSeconds = normal; WarningSeconds = warning; BurstSeconds = burst;
        WarningMultiplier = warningMultiplier; BurstMultiplier = burstMultiplier; ErraticStepSeconds = step;
        ErraticPositions = Array.AsReadOnly((double[])positions.Clone());
    }
    /// <summary>Supplies explicit prototype defaults for isolated model fixtures and callers without an authored asset.</summary>
    public static LandingMotionSettings Prototype()
    { return new LandingMotionSettings(1.8, .35, .45, .25, 2.6, .65, new[] { .12, .75, .75, .4, .65, .18, .18, .5, .12 }); }
}
