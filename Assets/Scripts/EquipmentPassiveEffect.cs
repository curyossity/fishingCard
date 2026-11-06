using System;
using UnityEngine;

/// <summary>Reusable passive behavior, independent of an equipment item's identity or artwork.</summary>
public enum EquipmentPassiveKind
{
    ReduceFirstAttachmentWeights,
    ReduceHeaviestWeight,
    CatchValueWhileOverloaded,
    DrawAtBiomeStart,
    DrawAfterEmptyHandTechnique,
    FilterAfterBaitAttachment,
    ValueEveryNthCatch,
    ReduceBaitWeightsWithMinimumCount,
    CatchValueWithMinimumBaitCount
}

/// <summary>Authors a passive's magnitude and attachment/count threshold without runtime counters.</summary>
[Serializable]
public sealed class EquipmentPassiveEffect
{
    [SerializeField] private EquipmentPassiveKind kind;
    [SerializeField] private int amount = 1;
    [SerializeField] private int threshold = 1;
    public EquipmentPassiveKind Kind => kind;
    public int Amount => amount;
    public int Threshold => threshold;
}
