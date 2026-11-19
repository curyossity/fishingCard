using System;
using System.Collections.Generic;
using UnityEngine;

public enum RigAbilityKind { LandingBonus, LandingMultiplier, ValueBonus, ValueMultiplier, ExtraAttempts, MissProtection, BlockInsertionBefore, BlockInsertionAfter, PerfectBase, GoodWidth, PerfectWidth, FishSlowInGood }
public enum RigAbilityTrigger { Continuous, Acquired, Upgraded, Strike }
public enum RigPositionCount { Once, Adjacent, ContiguousOther, Before, After, Other, DifferentNeighbors, Center, LineTip, DistinctTagsBefore, DistinctOtherTags }
public enum RigValueTarget { Self, Adjacent, Before, After, All, UntilNextSameTag }
public enum RigStrikeCondition { Always, FirstSuccessful, FirstPerfect, PreviousPerfect, PreviousMiss, HalfResistance, FinalAttempt, HigherRarity, EncounterTag, FirstGood, FirstMiss, TwoSuccessful }

/// <summary>One authored creature ability with related components, stepped level progression and optional coefficient caps.</summary>
[Serializable]
public sealed class CreatureAbilityDefinition
{
    [SerializeField] private string name;
    [SerializeField, TextArea, Tooltip("Use {amount}, {multiplier}, {percent} and {attempts} to keep short descriptions aligned with current-level tuning.")] private string description;
    [SerializeField] private CreatureAbilityEffectDefinition[] effects = Array.Empty<CreatureAbilityEffectDefinition>();

    /// <summary>Copies authoring into immutable expedition data; empty abilities grant no invented effects.</summary>
    public RigAbilitySpec Snapshot()
    {
        var rules = new List<RigAbilityEffect>();
        foreach (var effect in effects ?? Array.Empty<CreatureAbilityEffectDefinition>())
            if (effect != null) rules.Add(effect.Snapshot());
        return new RigAbilitySpec(name, description, rules.ToArray());
    }
}

/// <summary>Data-driven component of a single ability; multiplier amounts are additive growth above x1.</summary>
[Serializable]
public sealed class CreatureAbilityEffectDefinition
{
    [SerializeField] private RigAbilityKind kind;
    [SerializeField] private RigAbilityTrigger trigger;
    [SerializeField] private LandingAccuracyFilter accuracy;
    [SerializeField, Min(0)] private float amount, amountPerLevel;
    [SerializeField] private RigPositionCount count;
    [SerializeField] private string tag;
    [SerializeField] private bool perPosition = true;
    [SerializeField] private RigValueTarget valueTarget;
    [SerializeField] private string targetTag;
    [SerializeField] private RigStrikeCondition condition;
    [SerializeField] private string encounterTag;
    [SerializeField, Min(0)] private int triggerLimit;
    [SerializeField, Min(1)] private int levelsPerIncrement = 1;
    [SerializeField, Min(0)] private float maximumAmount;

    /// <summary>Validates effect boundaries before any rig or reward state can change.</summary>
    public RigAbilityEffect Snapshot() => new RigAbilityEffect(kind, trigger, accuracy, amount, amountPerLevel, count, tag, perPosition, valueTarget, targetTag,
        condition, encounterTag, triggerLimit, levelsPerIncrement == 0 ? 1 : levelsPerIncrement, maximumAmount);
}

/// <summary>Frozen primary ability; its components are shared safely by independent previews and result snapshots.</summary>
public sealed class RigAbilitySpec
{
    public string Name { get; }
    public string Description { get; }
    public IReadOnlyList<RigAbilityEffect> Effects { get; }
    /// <summary>Copies components so authoring changes cannot mutate an attached creature's progression.</summary>
    public RigAbilitySpec(string name, string description, RigAbilityEffect[] effects)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "Ability unassigned" : name;
        Description = description ?? "";
        Effects = Array.AsReadOnly((RigAbilityEffect[])(effects ?? Array.Empty<RigAbilityEffect>()).Clone());
    }
}

/// <summary>Immutable ability component with explicit scope, trigger and upgrade scaling.</summary>
public sealed class RigAbilityEffect
{
    public RigAbilityKind Kind { get; }
    public RigAbilityTrigger Trigger { get; }
    public LandingAccuracyFilter Accuracy { get; }
    public decimal BaseAmount { get; }
    public decimal PerLevel { get; }
    public RigPositionCount Count { get; }
    public string Tag { get; }
    public bool PerPosition { get; }
    public RigValueTarget ValueTarget { get; }
    public string TargetTag { get; }
    public RigStrikeCondition Condition { get; }
    public string EncounterTag { get; }
    public int TriggerLimit { get; }
    public int LevelsPerIncrement { get; }
    public decimal MaximumAmount { get; }

    /// <summary>Rejects invalid enum/numeric authoring and unsupported triggers rather than silently ignoring it.</summary>
    public RigAbilityEffect(RigAbilityKind kind, RigAbilityTrigger trigger, LandingAccuracyFilter accuracy, float amount, float perLevel,
        RigPositionCount count = RigPositionCount.Once, string tag = null, bool perPosition = true,
        RigValueTarget target = RigValueTarget.Self, string targetTag = null, RigStrikeCondition condition = RigStrikeCondition.Always,
        string encounterTag = null, int triggerLimit = 0, int levelsPerIncrement = 1, float maximumAmount = 0)
    {
        if (!Enum.IsDefined(typeof(RigAbilityKind), kind) || !Enum.IsDefined(typeof(RigAbilityTrigger), trigger)
            || !Enum.IsDefined(typeof(RigPositionCount), count) || !Enum.IsDefined(typeof(RigValueTarget), target)
            || !Enum.IsDefined(typeof(LandingAccuracyFilter), accuracy) || float.IsNaN(amount) || float.IsInfinity(amount)
            || float.IsNaN(perLevel) || float.IsInfinity(perLevel) || amount < 0 || perLevel < 0)
            throw new ArgumentException("Creature abilities require supported operations and finite nonnegative scaling.");
        if (!Enum.IsDefined(typeof(RigStrikeCondition), condition) || triggerLimit < 0 || levelsPerIncrement < 1
            || float.IsNaN(maximumAmount) || float.IsInfinity(maximumAmount) || maximumAmount < 0 || maximumAmount > 0 && maximumAmount < amount)
            throw new ArgumentException("Invalid ability condition, trigger limit or level progression.");
        if (trigger != RigAbilityTrigger.Continuous && trigger != RigAbilityTrigger.Strike && (kind != RigAbilityKind.ValueBonus || target != RigValueTarget.Self))
            throw new ArgumentException("Acquired/Upgraded triggers currently support self sale-value grants only.");
        if (trigger == RigAbilityTrigger.Strike && kind != RigAbilityKind.ExtraAttempts)
            throw new ArgumentException("Strike grants support additional attempts only.");
        if (trigger == RigAbilityTrigger.Strike && triggerLimit < 1)
            throw new ArgumentException("Strike attempt grants require a finite per-encounter trigger limit.");
        bool scoring = kind == RigAbilityKind.LandingBonus || kind == RigAbilityKind.LandingMultiplier || kind == RigAbilityKind.PerfectBase;
        if ((condition != RigStrikeCondition.Always || triggerLimit > 0) && !scoring && trigger != RigAbilityTrigger.Strike)
            throw new ArgumentException("Encounter conditions and trigger limits require a scoring rule or strike grant.");
        if (scoring && trigger != RigAbilityTrigger.Continuous)
            throw new ArgumentException("Scoring rules must use the continuous strike evaluator.");
        bool wholeUnits = kind == RigAbilityKind.ValueBonus || kind == RigAbilityKind.ExtraAttempts || kind == RigAbilityKind.MissProtection
            || kind == RigAbilityKind.BlockInsertionBefore || kind == RigAbilityKind.BlockInsertionAfter;
        if (wholeUnits && (amount % 1 != 0 || perLevel % 1 != 0 || maximumAmount % 1 != 0))
            throw new ArgumentException("Value, attempt and restriction components require whole-unit scaling; no rounding rule is implied.");
        if (kind == RigAbilityKind.ValueMultiplier && (amount % 1 != 0 || perLevel % 1 != 0))
            throw new ArgumentException("Fractional sale multipliers need an explicit economy rounding policy.");
        if ((kind == RigAbilityKind.GoodWidth || kind == RigAbilityKind.PerfectWidth || kind == RigAbilityKind.FishSlowInGood)
            && (trigger != RigAbilityTrigger.Continuous || maximumAmount <= 0 || maximumAmount > .95f))
            throw new ArgumentException("Motion and zone assistance require a continuous rule capped at 95% or less.");
        if (condition == RigStrikeCondition.EncounterTag && !Array.Exists(CreatureTagVocabulary.All, t => string.Equals(t, encounterTag, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("Encounter conditions require a supported creature tag.");
        Kind = kind; Trigger = trigger; Accuracy = accuracy; BaseAmount = (decimal)amount; PerLevel = (decimal)perLevel;
        Count = count; Tag = tag?.Trim(); PerPosition = perPosition; ValueTarget = target; TargetTag = targetTag?.Trim();
        Condition = condition; EncounterTag = encounterTag; TriggerLimit = triggerLimit; LevelsPerIncrement = levelsPerIncrement; MaximumAmount = (decimal)maximumAmount;
    }
    /// <summary>Levels have no gameplay cap; checked arithmetic rejects unsupported numeric overflow atomically.</summary>
    public decimal Amount(int level)
    {
        if (level < 1) throw new ArgumentOutOfRangeException(nameof(level));
        decimal value = checked(BaseAmount + PerLevel * ((level - 1) / LevelsPerIncrement));
        return MaximumAmount > 0 ? Math.Min(value, MaximumAmount) : value;
    }
}
