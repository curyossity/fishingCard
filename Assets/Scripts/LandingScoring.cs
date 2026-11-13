using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public enum LandingAccuracy { Miss, Good, Perfect }
public enum LandingAccuracyFilter { Successful, Good, Perfect, Miss }

/// <summary>An explicitly authored role ability; empty current creature lists grant no new abilities.</summary>
[Serializable]
public sealed class LandingRigEffectDefinition
{
    [UnityEngine.SerializeField] private string label;
    [UnityEngine.SerializeField] private LandingAccuracyFilter accuracy = LandingAccuracyFilter.Successful;
    [UnityEngine.SerializeField, UnityEngine.Min(0)] private float bonus;
    [UnityEngine.SerializeField, UnityEngine.Min(0)] private float multiplier = 1;
    [UnityEngine.SerializeField] private string adjacentTag;
    [UnityEngine.SerializeField] private string perContiguousOtherTag;

    /// <summary>Copies the role ability into encounter-owned immutable data; only an explicit Miss filter can score on misses.</summary>
    public LandingRigRule Snapshot()
    { return new LandingRigRule(label, accuracy, bonus, multiplier, adjacentTag, perContiguousOtherTag); }
}

/// <summary>Immutable generic scoring rule, separate from legacy weight/haul effects and illustrative species abilities.</summary>
public sealed class LandingRigRule
{
    public string Label { get; }
    public LandingAccuracyFilter Accuracy { get; }
    public decimal Bonus { get; }
    public decimal Multiplier { get; }
    public string AdjacentTag { get; }
    public string ContiguousTag { get; }

    /// <summary>Validates nonnegative finite scoring modifiers before strikes can consume attempts.</summary>
    public LandingRigRule(string label, LandingAccuracyFilter accuracy, float bonus, float multiplier, string adjacentTag = null, string contiguousTag = null)
    {
        if (float.IsNaN(bonus) || float.IsInfinity(bonus) || bonus < 0 || float.IsNaN(multiplier)
            || float.IsInfinity(multiplier) || multiplier < 0 || !Enum.IsDefined(typeof(LandingAccuracyFilter), accuracy))
            throw new ArgumentException("Invalid landing scoring ability.");
        Label = string.IsNullOrWhiteSpace(label) ? "Rig effect" : label;
        Accuracy = accuracy; Bonus = (decimal)bonus; Multiplier = (decimal)multiplier;
        AdjacentTag = adjacentTag?.Trim(); ContiguousTag = contiguousTag?.Trim();
    }

    /// <summary>Checks the exact accuracy window, excluding Miss from ordinary successful-strike bonuses.</summary>
    public bool Matches(LandingAccuracy result)
    {
        return Accuracy == LandingAccuracyFilter.Successful ? result != LandingAccuracy.Miss
            : Accuracy == LandingAccuracyFilter.Good ? result == LandingAccuracy.Good
            : Accuracy == LandingAccuracyFilter.Perfect ? result == LandingAccuracy.Perfect : result == LandingAccuracy.Miss;
    }
}

/// <summary>One already attached rig member's stable identity, tags and selected-role scoring rules.</summary>
public sealed class LandingRigMember
{
    public int Id { get; }
    public string Name { get; }
    public CatchAttachmentRole Role { get; }
    public IReadOnlyList<string> Tags { get; }
    public IReadOnlyList<LandingRigRule> Rules { get; }

    /// <summary>Copies arrays so later view/definition edits cannot change the encounter's scoring snapshot.</summary>
    public LandingRigMember(int id, string name, CatchAttachmentRole role, string[] tags, LandingRigRule[] rules)
    {
        Id = id; Name = name ?? "Creature"; Role = role;
        Tags = Array.AsReadOnly((string[])(tags ?? Array.Empty<string>()).Clone());
        Rules = Array.AsReadOnly((LandingRigRule[])(rules ?? Array.Empty<LandingRigRule>()).Clone());
    }

    /// <summary>Compares tags case-insensitively for both Catch and Bait members.</summary>
    public bool HasTag(string tag)
    {
        foreach (string current in Tags) if (string.Equals(current, tag, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }
}

/// <summary>Attribution from one committed creature, retained with the strike for quick feedback and card highlighting.</summary>
public sealed class LandingContribution
{
    public int SourceId { get; }
    public string Label { get; }
    public decimal Bonus { get; }
    public decimal Multiplier { get; }
    /// <summary>Records a resolved contribution without retaining a live rig instance or rerunning its effect.</summary>
    public LandingContribution(int sourceId, string label, decimal bonus, decimal multiplier)
    { SourceId = sourceId; Label = label; Bonus = bonus; Multiplier = multiplier; }
}

/// <summary>Immutable strike result; scoring resolves before presentation and is never repeated by animation.</summary>
public sealed class LandingScore
{
    public LandingAccuracy Accuracy { get; }
    public decimal Base { get; }
    public decimal Bonuses { get; }
    public decimal Multiplier { get; }
    public decimal Points { get; }
    public IReadOnlyList<LandingContribution> Contributions { get; }
    /// <summary>Captures all score components without rounding away fractional multipliers.</summary>
    public LandingScore(LandingAccuracy accuracy, decimal basis, decimal bonuses, decimal multiplier, LandingContribution[] contributions)
    {
        Accuracy = accuracy; Base = basis; Bonuses = bonuses; Multiplier = multiplier;
        Points = checked((basis + bonuses) * multiplier);
        Contributions = Array.AsReadOnly((LandingContribution[])(contributions ?? Array.Empty<LandingContribution>()).Clone());
    }
}

/// <summary>Pure positional scoring over an encounter snapshot; the encountered creature is never part of that snapshot.</summary>
public static class LandingScoring
{
    /// <summary>Applies flat bonuses first and multiplies eligible multipliers in committed rig/effect order.</summary>
    public static LandingScore Resolve(LandingAccuracy accuracy, LandingSettings settings, IReadOnlyList<LandingRigMember> rig)
    {
        decimal basis = accuracy == LandingAccuracy.Perfect ? settings.PerfectPoints : accuracy == LandingAccuracy.Good ? settings.GoodPoints : 0;
        decimal bonus = 0, multiplier = 1;
        var contributions = new List<LandingContribution>();
        for (int i = 0; i < rig.Count; i++)
        {
            LandingRigMember member = rig[i];
            foreach (var rule in member.Rules)
            {
                if (!rule.Matches(accuracy)) continue;
                if (!string.IsNullOrWhiteSpace(rule.AdjacentTag)
                    && !(i > 0 && rig[i - 1].HasTag(rule.AdjacentTag))
                    && !(i + 1 < rig.Count && rig[i + 1].HasTag(rule.AdjacentTag))) continue;
                int count = 1;
                if (!string.IsNullOrWhiteSpace(rule.ContiguousTag))
                {
                    count = 0;
                    if (member.HasTag(rule.ContiguousTag))
                    {
                        for (int j = i - 1; j >= 0 && rig[j].HasTag(rule.ContiguousTag); j--) count++;
                        for (int j = i + 1; j < rig.Count && rig[j].HasTag(rule.ContiguousTag); j++) count++;
                    }
                    if (count == 0) continue;
                }
                decimal amount = checked(rule.Bonus * count);
                bonus = checked(bonus + amount); multiplier = checked(multiplier * rule.Multiplier);
                if (amount != 0 || rule.Multiplier != 1)
                    contributions.Add(new LandingContribution(member.Id, member.Name + ": " + rule.Label, amount, rule.Multiplier));
            }
        }
        return new LandingScore(accuracy, basis, bonus, multiplier, contributions.ToArray());
    }
}
