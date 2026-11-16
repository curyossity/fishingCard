using System;
using System.Collections.Generic;
using UnityEngine;

public enum RigAbilityKind { LandingBonus, LandingMultiplier, ValueBonus, ValueMultiplier, ExtraAttempts, MissProtection, BlockInsertionBefore, BlockInsertionAfter }
public enum RigAbilityTrigger { Continuous, Acquired, Upgraded }
public enum RigPositionCount { Once, Adjacent, ContiguousOther, Before, After, Other }
public enum RigValueTarget { Self, Adjacent, Before, After, All }

/// <summary>One authored creature ability, with related components and an explicit linear level progression.</summary>
[Serializable]
public sealed class CreatureAbilityDefinition
{
    [SerializeField] private string name;
    [SerializeField, TextArea] private string description;
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

    /// <summary>Validates effect boundaries before any rig or reward state can change.</summary>
    public RigAbilityEffect Snapshot() => new RigAbilityEffect(kind, trigger, accuracy, amount, amountPerLevel, count, tag, perPosition, valueTarget, targetTag);
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

    /// <summary>Rejects invalid enum/numeric authoring and unsupported triggers rather than silently ignoring it.</summary>
    public RigAbilityEffect(RigAbilityKind kind, RigAbilityTrigger trigger, LandingAccuracyFilter accuracy, float amount, float perLevel,
        RigPositionCount count = RigPositionCount.Once, string tag = null, bool perPosition = true,
        RigValueTarget target = RigValueTarget.Self, string targetTag = null)
    {
        if (!Enum.IsDefined(typeof(RigAbilityKind), kind) || !Enum.IsDefined(typeof(RigAbilityTrigger), trigger)
            || !Enum.IsDefined(typeof(RigPositionCount), count) || !Enum.IsDefined(typeof(RigValueTarget), target)
            || !Enum.IsDefined(typeof(LandingAccuracyFilter), accuracy) || float.IsNaN(amount) || float.IsInfinity(amount)
            || float.IsNaN(perLevel) || float.IsInfinity(perLevel) || amount < 0 || perLevel < 0)
            throw new ArgumentException("Creature abilities require supported operations and finite nonnegative scaling.");
        if (trigger != RigAbilityTrigger.Continuous && (kind != RigAbilityKind.ValueBonus || target != RigValueTarget.Self))
            throw new ArgumentException("Acquired/Upgraded triggers currently support self sale-value grants only.");
        if (kind != RigAbilityKind.LandingBonus && kind != RigAbilityKind.LandingMultiplier && (amount % 1 != 0 || perLevel % 1 != 0))
            throw new ArgumentException("Value, attempt and restriction components require whole-unit scaling; no rounding rule is implied.");
        Kind = kind; Trigger = trigger; Accuracy = accuracy; BaseAmount = (decimal)amount; PerLevel = (decimal)perLevel;
        Count = count; Tag = tag?.Trim(); PerPosition = perPosition; ValueTarget = target; TargetTag = targetTag?.Trim();
    }
    /// <summary>Levels have no gameplay cap; checked arithmetic rejects unsupported numeric overflow atomically.</summary>
    public decimal Amount(int level) => checked(BaseAmount + PerLevel * (level - 1));
}
