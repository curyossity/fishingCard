using System;
using UnityEngine;

public enum HookedEffectSource
{
    HookedEncounter,
    TechniqueCard
}

[Serializable]
public sealed class ActiveCatchEffectRecord
{
    [SerializeField] private CardInstance sourceInstance;
    [SerializeField] private CardEffectDefinition effect;
    [SerializeField] private CardEffectTrigger activeTrigger;
    [SerializeField] private int sourceCatchIndex;

    public CardInstance SourceInstance => sourceInstance;
    public CardDefinition SourceCard => sourceInstance?.Definition;
    public CardEffectDefinition Effect => effect;
    public CardEffectTrigger ActiveTrigger => activeTrigger;
    public int SourceCatchIndex => sourceCatchIndex;

    /// <summary>
    /// Creates an empty record for Unity serialization.
    /// </summary>
    public ActiveCatchEffectRecord()
    {
    }

    /// <summary>
    /// Creates a runtime record for an effect and the exact catch copy that provides it.
    /// </summary>
    public ActiveCatchEffectRecord(
        CardInstance sourceInstance,
        CardEffectDefinition effect,
        CardEffectTrigger activeTrigger,
        int sourceCatchIndex)
    {
        this.sourceInstance = sourceInstance;
        this.effect = effect;
        this.activeTrigger = activeTrigger;
        this.sourceCatchIndex = sourceCatchIndex;
    }
}

[Serializable]
public sealed class HookedEffectRecord
{
    [SerializeField] private CardDefinition sourceCard;
    [SerializeField] private CardEffectDefinition effect;
    [SerializeField] private HookedEffectSource sourceType;

    public CardDefinition SourceCard => sourceCard;
    public CardEffectDefinition Effect => effect;
    public HookedEffectSource SourceType => sourceType;

    /// <summary>
    /// Creates an empty record for Unity serialization.
    /// </summary>
    public HookedEffectRecord()
    {
    }

    /// <summary>
    /// Creates a runtime record for an effect that can influence the Hooked encounter.
    /// </summary>
    public HookedEffectRecord(CardDefinition sourceCard, CardEffectDefinition effect, HookedEffectSource sourceType)
    {
        this.sourceCard = sourceCard;
        this.effect = effect;
        this.sourceType = sourceType;
    }
}
