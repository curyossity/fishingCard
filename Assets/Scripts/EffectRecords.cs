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
    [SerializeField] private CardDefinition sourceCard;
    [SerializeField] private CardEffectDefinition effect;
    [SerializeField] private CardEffectTrigger activeTrigger;

    public CardDefinition SourceCard => sourceCard;
    public CardEffectDefinition Effect => effect;
    public CardEffectTrigger ActiveTrigger => activeTrigger;

    /// <summary>
    /// Creates an empty record for Unity serialization.
    /// </summary>
    public ActiveCatchEffectRecord()
    {
    }

    /// <summary>
    /// Creates a runtime record for an effect attached to the current Catch Chain.
    /// </summary>
    public ActiveCatchEffectRecord(CardDefinition sourceCard, CardEffectDefinition effect, CardEffectTrigger activeTrigger)
    {
        this.sourceCard = sourceCard;
        this.effect = effect;
        this.activeTrigger = activeTrigger;
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
