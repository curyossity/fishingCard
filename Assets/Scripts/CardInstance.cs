using System;
using System.Collections.Generic;
using UnityEngine;

public enum CatchAttachmentRole
{
    Catch,
    Bait
}

[Serializable]
public sealed class CardInstance
{
    [SerializeField] private int instanceId;
    [SerializeField] private CardDefinition definition;
    [SerializeField] private int permanentWeightModifier;
    [SerializeField] private int permanentValueModifier;
    [SerializeField] private int baseValue;
    [SerializeField] private int currentWeight;
    [SerializeField] private int currentValue;
    [SerializeField] private CatchAttachmentRole attachmentRole;
    [SerializeField] private int attachmentBiomeVisit;
    [SerializeField] private int[] removedEffectIndices = Array.Empty<int>();
    [SerializeField] private List<AppliedCatchModifier> appliedModifiers = new List<AppliedCatchModifier>();

    [SerializeField] private bool hasRuntimeTagOverride;
    [SerializeField] private string[] runtimeBaseTags = Array.Empty<string>();
    [SerializeField] private string[] effectiveTags = Array.Empty<string>();
    [SerializeField] private List<SelectedEffectTargets> selectedEffectTargets = new List<SelectedEffectTargets>();

    public string[] Tags => effectiveTags;
    public int InstanceId => instanceId;
    public CardDefinition Definition => definition;
    public int CurrentWeight => currentWeight;
    public int CurrentValue => currentValue;
    public int HaulValueContribution => IsBait ? 0 : currentValue;
    public int PermanentWeightModifier => permanentWeightModifier;
    public int PermanentValueModifier => permanentValueModifier;
    public int BaseValue => baseValue;
    public int WeightModifier => definition == null ? 0 : currentWeight - definition.Weight;
    public int ValueModifier => definition == null ? 0 : currentValue - baseValue;
    public CatchAttachmentRole AttachmentRole => attachmentRole;
    public bool IsBait => attachmentRole == CatchAttachmentRole.Bait;
    public AppliedCatchModifier[] AppliedModifiers => appliedModifiers.ToArray();
    public int AttachmentBiomeVisit => attachmentBiomeVisit;
    public CardEffectDefinition[] ActiveEffects
    {
        get
        {
            CardEffectDefinition[] authored = definition?.GetEffects(attachmentRole) ?? Array.Empty<CardEffectDefinition>();
            if (removedEffectIndices.Length == 0)
            {
                return authored;
            }

            CardEffectDefinition[] active = (CardEffectDefinition[])authored.Clone();
            for (int i = 0; i < active.Length; i++)
            {
                if (Array.IndexOf(removedEffectIndices, i) >= 0)
                {
                    active[i] = null;
                }
            }

            return active;
        }
    }

    /// <summary>Records which biome visit committed this copy, independent of depth and biome identity.</summary>
    public void SetAttachmentBiomeVisit(int visit)
    {
        attachmentBiomeVisit = visit;
    }

    /// <summary>Removes one exact authored effect from this copy for the rest of its attached lifetime.</summary>
    public bool TryRemoveEffect(CardEffectDefinition effect)
    {
        CardEffectDefinition[] active = ActiveEffects;
        int index = Array.IndexOf(active, effect);
        if (effect == null || index < 0)
        {
            return false;
        }

        int[] updated = new int[removedEffectIndices.Length + 1];
        Array.Copy(removedEffectIndices, updated, removedEffectIndices.Length);
        updated[updated.Length - 1] = index;
        removedEffectIndices = updated;
        return true;
    }
    public bool HidesOwnValueDuringRun => HasActiveEffect(
        CardEffectType.HideEncounterInformation,
        CardEffectTarget.Self);
    public bool PreventsReleaseWhileAttached => HasActiveEffect(
        CardEffectType.PreventRelease,
        CardEffectTarget.Self);

    /// <summary>
    /// Creates an empty runtime card for Unity serialization.
    /// </summary>
    public CardInstance()
    {
    }

    /// <summary>
    /// Creates one independently mutable runtime copy of a card definition.
    /// </summary>
    public CardInstance(
        int instanceId,
        CardDefinition definition,
        System.Random random = null,
        CatchAttachmentRole attachmentRole = CatchAttachmentRole.Catch)
    {
        this.instanceId = instanceId;
        this.definition = definition;
        this.attachmentRole = attachmentRole;
        baseValue = definition == null ? 0 : definition.RollCaughtValue(random);
        ResetCurrentStats();
        ResetEffectiveTags();
    }

    /// <summary>Changes this copy's base tags without modifying its authored definition; the chain must then recalculate.</summary>
    public void SetRuntimeBaseTags(string[] tags)
    {
        hasRuntimeTagOverride = true;
        runtimeBaseTags = CreatureTagVocabulary.Normalize(tags);
        ResetEffectiveTags();
    }

    /// <summary>Rebuilds effective tags from native/runtime base tags before applying surviving grants.</summary>
    public void ResetEffectiveTags()
    {
        effectiveTags = (string[])(hasRuntimeTagOverride ? runtimeBaseTags
            : definition?.Tags ?? Array.Empty<string>()).Clone();
    }

    /// <summary>Grants a tag once, preserving native tags and grants from other active sources on rebuild.</summary>
    public void GrantTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag) || HasTag(tag)) return;
        List<string> tags = new List<string>(effectiveTags) { tag };
        effectiveTags = CreatureTagVocabulary.Normalize(tags.ToArray());
    }

    /// <summary>Reports whether this attached copy currently has the requested effective tag.</summary>
    public bool HasTag(string tag)
    {
        return Array.Exists(effectiveTags, t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Stores a one-time selection by stable target instance IDs for one role effect.</summary>
    public void SetEffectTargets(int effectIndex, int[] targetIds)
    {
        selectedEffectTargets.RemoveAll(t => t.EffectIndex == effectIndex && t.Role == attachmentRole);
        selectedEffectTargets.Add(new SelectedEffectTargets
        {
            EffectIndex = effectIndex, Role = attachmentRole,
            TargetIds = (int[])(targetIds ?? Array.Empty<int>()).Clone()
        });
    }

    /// <summary>Tests the frozen target selection for the currently active role effect.</summary>
    public bool IsSelectedEffectTarget(CardEffectDefinition effect, int targetId)
    {
        int index = Array.IndexOf(ActiveEffects, effect);
        return selectedEffectTargets.Exists(t => t.EffectIndex == index && t.Role == attachmentRole
            && Array.IndexOf(t.TargetIds, targetId) >= 0);
    }

    /// <summary>
    /// Restores current stats to the immutable values authored on the card definition.
    /// </summary>
    public void ResetCurrentStats()
    {
        currentWeight = definition == null ? 0 : Mathf.Max(0, definition.Weight + permanentWeightModifier);
        currentValue = definition == null ? 0 : Mathf.Max(0, baseValue + permanentValueModifier);
    }

    /// <summary>
    /// Changes this catch copy's lasting base modifiers before persistent effects are recalculated.
    /// </summary>
    public void AddPermanentModifiers(int weightAmount, int valueAmount)
    {
        permanentWeightModifier += weightAmount;
        permanentValueModifier += valueAmount;
    }

    /// <summary>Applies a lasting authored modifier while retaining its origin for later cleansing.</summary>
    public void AddPermanentModifiers(int weightAmount, int valueAmount, CardEffectDefinition effect, CardDefinition source)
    {
        AddPermanentModifiers(weightAmount, valueAmount);
        if (effect != null && (weightAmount != 0 || valueAmount != 0))
        {
            appliedModifiers.Add(new AppliedCatchModifier(effect, source, weightAmount, valueAmount));
        }
    }

    /// <summary>Removes one exact lasting modifier contribution while preserving all other mutations.</summary>
    public bool TryRemoveAppliedModifier(AppliedCatchModifier modifier)
    {
        if (modifier == null || modifier.Effect?.EffectTone != CardEffectTone.Negative
            || !appliedModifiers.Remove(modifier))
        {
            return false;
        }

        permanentWeightModifier -= modifier.WeightAmount;
        permanentValueModifier -= modifier.ValueAmount;
        return true;
    }

    /// <summary>
    /// Applies a runtime weight change without allowing Line Load contribution below zero.
    /// </summary>
    public void ModifyWeight(int amount)
    {
        currentWeight = Mathf.Max(0, currentWeight + amount);
    }

    /// <summary>
    /// Applies a runtime value change without allowing the resolved card value below zero.
    /// Bait retains this resolved value for effect calculations but contributes zero haul value.
    /// </summary>
    public void ModifyValue(int amount)
    {
        currentValue = Mathf.Max(0, currentValue + amount);
    }

    /// <summary>
    /// Creates an independent snapshot that preserves this copy's resolved stats.
    /// </summary>
    public CardInstance CreateSnapshot()
    {
        CardInstance snapshot = new CardInstance(instanceId, definition, null, attachmentRole);
        snapshot.hasRuntimeTagOverride = hasRuntimeTagOverride;
        snapshot.runtimeBaseTags = (string[])runtimeBaseTags.Clone();
        snapshot.effectiveTags = (string[])effectiveTags.Clone();
        foreach (SelectedEffectTargets selection in selectedEffectTargets)
        {
            snapshot.selectedEffectTargets.Add(new SelectedEffectTargets
            {
                EffectIndex = selection.EffectIndex, Role = selection.Role,
                TargetIds = (int[])selection.TargetIds.Clone()
            });
        }
        snapshot.baseValue = baseValue;
        snapshot.permanentWeightModifier = permanentWeightModifier;
        snapshot.permanentValueModifier = permanentValueModifier;
        snapshot.currentWeight = currentWeight;
        snapshot.currentValue = currentValue;
        snapshot.attachmentBiomeVisit = attachmentBiomeVisit;
        snapshot.removedEffectIndices = (int[])removedEffectIndices.Clone();
        snapshot.appliedModifiers = new List<AppliedCatchModifier>(appliedModifiers);
        return snapshot;
    }

    [Serializable]
    private sealed class SelectedEffectTargets
    {
        public int EffectIndex;
        public CatchAttachmentRole Role;
        public int[] TargetIds = Array.Empty<int>();
    }

    /// <summary>Checks this copy's surviving role effects, including cleansed self restrictions.</summary>
    private bool HasActiveEffect(CardEffectType effectType, CardEffectTarget target)
    {
        CardEffectDefinition[] activeEffects = ActiveEffects;
        for (int i = 0; i < activeEffects.Length; i++)
        {
            CardEffectDefinition effect = activeEffects[i];
            if (effect != null && effect.EffectType == effectType && effect.Target == target)
            {
                return true;
            }
        }

        return false;
    }
}

/// <summary>Records one resolved lasting stat effect independently of its source remaining attached.</summary>
[Serializable]
public sealed class AppliedCatchModifier
{
    [SerializeField] private CardEffectDefinition effect;
    [SerializeField] private CardDefinition sourceCard;
    [SerializeField] private int weightAmount;
    [SerializeField] private int valueAmount;
    public CardEffectDefinition Effect => effect;
    public CardDefinition SourceCard => sourceCard;
    public int WeightAmount => weightAmount;
    public int ValueAmount => valueAmount;

    /// <summary>Captures one effect's actual applied contribution for exact removal and snapshots.</summary>
    public AppliedCatchModifier(CardEffectDefinition effect, CardDefinition sourceCard, int weightAmount, int valueAmount)
    {
        this.effect = effect;
        this.sourceCard = sourceCard;
        this.weightAmount = weightAmount;
        this.valueAmount = valueAmount;
    }
}
