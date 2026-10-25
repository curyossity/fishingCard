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
    [SerializeField] private List<string> removedRoleEffectKeys = new List<string>();
    [SerializeField] private List<AppliedCatchModifier> appliedModifiers = new List<AppliedCatchModifier>();
    [NonSerialized] private List<CardStatContribution> resolvedStatContributions = new List<CardStatContribution>();
    public IReadOnlyList<CardStatContribution> ResolvedStatContributions => resolvedStatContributions;

    [SerializeField] private bool hasRuntimeTagOverride;
    [SerializeField] private string[] runtimeBaseTags = Array.Empty<string>();
    [SerializeField] private string[] effectiveTags = Array.Empty<string>();
    [SerializeField] private List<SelectedEffectTargets> selectedEffectTargets = new List<SelectedEffectTargets>();

    [SerializeField] private List<string> consumedAttachmentActivations = new List<string>();

    [SerializeField] private bool hazardProtectionInitialized;
    [SerializeField] private bool hazardProtectionAvailable;

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
            if (removedEffectIndices.Length == 0 && removedRoleEffectKeys.Count == 0)
            {
                return authored;
            }

            CardEffectDefinition[] active = (CardEffectDefinition[])authored.Clone();
            for (int i = 0; i < active.Length; i++)
            {
                if (Array.IndexOf(removedEffectIndices, i) >= 0
                    || removedRoleEffectKeys.Contains(attachmentRole + ":" + i))
                {
                    active[i] = null;
                }
            }

            return active;
        }
    }

    /// <summary>Consumes a card's one-time role ability even when no offer exists or the player declines.</summary>
    public bool TryConsumeAttachmentActivation(int effectIndex)
    {
        string key = attachmentRole + ":" + effectIndex;
        if (consumedAttachmentActivations.Contains(key)) return false;
        consumedAttachmentActivations.Add(key);
        return true;
    }

    /// <summary>Records which biome visit committed this copy, independent of depth and biome identity.</summary>
    public void SetAttachmentBiomeVisit(int visit)
    {
        attachmentBiomeVisit = visit;
    }

    /// <summary>Removes one exact authored effect from this copy's current role for the rest of its attached lifetime.</summary>
    public bool TryRemoveEffect(CardEffectDefinition effect)
    {
        CardEffectDefinition[] active = ActiveEffects;
        int index = Array.IndexOf(active, effect);
        if (effect == null || index < 0)
        {
            return false;
        }

        MigrateRemovedRoleEffects();
        removedRoleEffectKeys.Add(attachmentRole + ":" + index);
        return true;
    }
    /// <summary>Associates legacy cleansed indices with their current role before any role mutation.</summary>
    private void MigrateRemovedRoleEffects()
    {
        foreach (int index in removedEffectIndices) removedRoleEffectKeys.Add(attachmentRole + ":" + index);
        removedEffectIndices = Array.Empty<int>();
    }

    /// <summary>Changes role state while preserving permanent modifiers and expiring one-use Bait protection; the chain must recalculate.</summary>
    public void SetAttachmentRole(CatchAttachmentRole role)
    {
        MigrateRemovedRoleEffects();
        RefreshHazardProtection();
        attachmentRole = role;
        RefreshHazardProtection();
    }

    /// <summary>Initializes one protection on first Bait attachment and expires it immediately outside Bait mode.</summary>
    public void RefreshHazardProtection()
    {
        bool protectedBait = IsBait && HasActiveEffect(CardEffectType.CancelNextHazard, CardEffectTarget.Self);
        if (hazardProtectionInitialized)
        {
            if (!protectedBait) hazardProtectionAvailable = false;
        }
        else if (protectedBait)
        {
            hazardProtectionInitialized = true;
            hazardProtectionAvailable = true;
        }
    }

    /// <summary>Consumes the single unused protection without allowing role changes to restore it.</summary>
    public bool TryConsumeHazardProtection()
    {
        RefreshHazardProtection();
        if (!hazardProtectionAvailable) return false;
        hazardProtectionAvailable = false;
        return true;
    }

    public bool HasUnusedHazardProtection => hazardProtectionAvailable && IsBait
        && HasActiveEffect(CardEffectType.CancelNextHazard, CardEffectTarget.Self);
    public bool PreventsTechniqueTargeting => !IsBait
        && HasActiveEffect(CardEffectType.PreventTechniqueTargeting, CardEffectTarget.Self);
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
        RefreshHazardProtection();
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

    /// <summary>Returns the combined tracked event contribution applied after ongoing weight interactions.</summary>
    public int EventWeightModifier
    {
        get
        {
            int amount = 0;
            foreach (AppliedCatchModifier modifier in appliedModifiers)
                if (modifier.EventEffectId > 0) amount += modifier.WeightAmount;
            return amount;
        }
    }

    /// <summary>
    /// Restores base and lasting stats; event weight contributions apply after ongoing creature interactions.
    /// </summary>
    public void ResetCurrentStats()
    {
        currentWeight = definition == null ? 0 : Mathf.Max(0, definition.Weight + permanentWeightModifier - EventWeightModifier);
        currentValue = definition == null ? 0 : Mathf.Max(0, baseValue + permanentValueModifier);
        if (resolvedStatContributions == null) resolvedStatContributions = new List<CardStatContribution>();
        resolvedStatContributions.Clear();
        int weight = 0, value = 0;
        foreach (AppliedCatchModifier modifier in appliedModifiers)
        {
            int w = modifier.EventEffectId > 0 ? 0 : modifier.WeightAmount;
            resolvedStatContributions.Add(new CardStatContribution(modifier.SourceCard, modifier.Effect, w, modifier.ValueAmount));
            weight += w; value += modifier.ValueAmount;
        }
        int untrackedWeight = permanentWeightModifier - EventWeightModifier - weight;
        int untrackedValue = permanentValueModifier - value;
        if (untrackedWeight != 0 || untrackedValue != 0)
            resolvedStatContributions.Add(new CardStatContribution(null, null, untrackedWeight, untrackedValue));
        ReconcileStatFloors();
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
        return modifier?.Effect?.EffectTone == CardEffectTone.Negative && TryRemoveTrackedModifier(modifier);
    }

    /// <summary>Tracks one independently expiring event modifier without changing authored stats or resetting it on role changes.</summary>
    public AppliedCatchModifier AddEventModifier(long eventEffectId, int weightAmount, int valueAmount,
        CardEffectDefinition effect, CardDefinition source)
    {
        if (eventEffectId <= 0 || effect == null || source == null) throw new ArgumentException("Event modifier provenance is required.");
        if (Array.Exists(AppliedModifiers, m => m.EventEffectId == eventEffectId))
            throw new InvalidOperationException("Event modifier is already present.");
        AddPermanentModifiers(weightAmount, valueAmount);
        AppliedCatchModifier modifier = new AppliedCatchModifier(effect, source, weightAmount, valueAmount, eventEffectId);
        appliedModifiers.Add(modifier);
        return modifier;
    }

    /// <summary>Expires one exact tracked contribution regardless of tone, preserving unrelated modifiers.</summary>
    public bool TryRemoveTrackedModifier(AppliedCatchModifier modifier)
    {
        if (modifier == null || !appliedModifiers.Remove(modifier)) return false;
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

    /// <summary>Records the effective, zero-clamped change at the same point where gameplay resolves it.</summary>
    public void ModifyWeight(int amount, CardDefinition source, CardEffectDefinition effect)
    {
        int before = currentWeight; ModifyWeight(amount);
        if (currentWeight != before) resolvedStatContributions.Add(new CardStatContribution(source, effect, currentWeight - before, 0));
    }

    /// <summary>Applies the existing combined event-weight rule while retaining each event's independent contribution.</summary>
    public void ApplyEventWeightModifiers()
    {
        ModifyWeight(EventWeightModifier);
        foreach (AppliedCatchModifier modifier in appliedModifiers)
            if (modifier.EventEffectId > 0 && modifier.WeightAmount != 0)
                resolvedStatContributions.Add(new CardStatContribution(modifier.SourceCard, modifier.Effect, modifier.WeightAmount, 0));
        ReconcileStatFloors();
    }

    /// <summary>Attributes only effective reductions when combined lasting or event modifiers encounter the zero floor.</summary>
    private void ReconcileStatFloors()
    {
        int weight = definition?.Weight ?? 0, value = baseValue;
        foreach (CardStatContribution c in resolvedStatContributions) { weight += c.Weight; value += c.Value; }
        int weightOverflow = currentWeight - weight, valueOverflow = currentValue - value;
        for (int i = resolvedStatContributions.Count - 1; i >= 0 && (weightOverflow > 0 || valueOverflow > 0); i--)
        {
            CardStatContribution c = resolvedStatContributions[i];
            int w = Mathf.Min(Mathf.Max(0, -c.Weight), Mathf.Max(0, weightOverflow));
            int v = Mathf.Min(Mathf.Max(0, -c.Value), Mathf.Max(0, valueOverflow));
            resolvedStatContributions[i] = new CardStatContribution(c.Source, c.Effect, c.Weight + w, c.Value + v);
            weightOverflow -= w; valueOverflow -= v;
        }
    }

    /// <summary>
    /// Applies a runtime value change without allowing the resolved card value below zero.
    /// Bait retains this resolved value for effect calculations but contributes zero haul value.
    /// </summary>
    public void ModifyValue(int amount)
    {
        currentValue = Mathf.Max(0, currentValue + amount);
    }

    /// <summary>Records an ongoing source's actual contribution without duplicating its calculation in presentation.</summary>
    public void ModifyValue(int amount, CardDefinition source, CardEffectDefinition effect)
    {
        int before = currentValue; ModifyValue(amount);
        if (currentValue != before) resolvedStatContributions.Add(new CardStatContribution(source, effect, 0, currentValue - before));
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
        snapshot.consumedAttachmentActivations = new List<string>(consumedAttachmentActivations);
        snapshot.hazardProtectionInitialized = hazardProtectionInitialized;
        snapshot.hazardProtectionAvailable = hazardProtectionAvailable;
        snapshot.baseValue = baseValue;
        snapshot.permanentWeightModifier = permanentWeightModifier;
        snapshot.permanentValueModifier = permanentValueModifier;
        snapshot.currentWeight = currentWeight;
        snapshot.currentValue = currentValue;
        snapshot.attachmentBiomeVisit = attachmentBiomeVisit;
        snapshot.removedEffectIndices = (int[])removedEffectIndices.Clone();
        snapshot.removedRoleEffectKeys = new List<string>(removedRoleEffectKeys);
        snapshot.appliedModifiers = new List<AppliedCatchModifier>(appliedModifiers);
        snapshot.resolvedStatContributions = new List<CardStatContribution>(resolvedStatContributions);
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
    [SerializeField] private long eventEffectId;
    public long EventEffectId => eventEffectId;
    public CardEffectDefinition Effect => effect;
    public CardDefinition SourceCard => sourceCard;
    public int WeightAmount => weightAmount;
    public int ValueAmount => valueAmount;

    /// <summary>Captures one effect's actual applied contribution for exact removal and snapshots.</summary>
    public AppliedCatchModifier(CardEffectDefinition effect, CardDefinition sourceCard, int weightAmount, int valueAmount, long eventEffectId = 0)
    {
        this.eventEffectId = eventEffectId;
        this.effect = effect;
        this.sourceCard = sourceCard;
        this.weightAmount = weightAmount;
        this.valueAmount = valueAmount;
    }
}
