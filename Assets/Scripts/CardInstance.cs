using System;
using UnityEngine;

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

    public int InstanceId => instanceId;
    public CardDefinition Definition => definition;
    public int CurrentWeight => currentWeight;
    public int CurrentValue => currentValue;
    public int PermanentWeightModifier => permanentWeightModifier;
    public int PermanentValueModifier => permanentValueModifier;
    public int BaseValue => baseValue;
    public int WeightModifier => definition == null ? 0 : currentWeight - definition.Weight;
    public int ValueModifier => definition == null ? 0 : currentValue - baseValue;

    /// <summary>
    /// Creates an empty runtime card for Unity serialization.
    /// </summary>
    public CardInstance()
    {
    }

    /// <summary>
    /// Creates one independently mutable runtime copy of a card definition.
    /// </summary>
    public CardInstance(int instanceId, CardDefinition definition, System.Random random = null)
    {
        this.instanceId = instanceId;
        this.definition = definition;
        baseValue = definition == null ? 0 : definition.RollCaughtValue(random);
        ResetCurrentStats();
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

    /// <summary>
    /// Applies a runtime weight change without allowing Line Load contribution below zero.
    /// </summary>
    public void ModifyWeight(int amount)
    {
        currentWeight = Mathf.Max(0, currentWeight + amount);
    }

    /// <summary>
    /// Applies a runtime value change without allowing haul value below zero.
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
        CardInstance snapshot = new CardInstance(instanceId, definition);
        snapshot.baseValue = baseValue;
        snapshot.permanentWeightModifier = permanentWeightModifier;
        snapshot.permanentValueModifier = permanentValueModifier;
        snapshot.currentWeight = currentWeight;
        snapshot.currentValue = currentValue;
        return snapshot;
    }
}
