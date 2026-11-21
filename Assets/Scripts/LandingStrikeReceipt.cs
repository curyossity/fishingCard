using System;
using System.Collections.Generic;

/// <summary>Frozen last-hit information survives rig cleanup, upgrades and the following encounter.</summary>
public sealed class LandingStrikeReceipt
{
    public LandingScore Score { get; }
    public IReadOnlyList<CardInstance> Members { get; }
    public IReadOnlyList<LandingRigMember> Rules { get; }
    public long Occurrence { get; }
    public long Revision { get; }
    public bool ProtectedMiss { get; }
    public decimal PointsBefore { get; }
    public decimal PointsAfter { get; }
    public decimal Resistance { get; }
    /// <summary>Normalized marker center at the accepted strike, independent of subsequent motion or encounters.</summary>
    public double FishPosition { get; }

    /// <summary>Captures only committed pre-acquisition members; the landed creature cannot credit itself.</summary>
    public LandingStrikeReceipt(LandingChallengeRuntime landing, CardInstance[] members)
    {
        Score = landing.LastScore; Occurrence = landing.Occurrence; Revision = landing.Revision;
        ProtectedMiss = landing.LastMissProtected;
        FishPosition = landing.FishPosition;
        PointsAfter = landing.Points; PointsBefore = PointsAfter - (Score?.Points ?? 0); Resistance = landing.Resistance;
        Members = Array.AsReadOnly(Array.ConvertAll(members ?? Array.Empty<CardInstance>(), member => member.CreateSnapshot()));
        var rules = new LandingRigMember[landing.Rig.Count];
        for (int i = 0; i < rules.Length; i++) rules[i] = landing.Rig[i];
        Rules = Array.AsReadOnly(rules);
    }
}
