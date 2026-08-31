using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class CatchChainRuntime
{
    [SerializeField] private CardInstance[] catches = Array.Empty<CardInstance>();
    [SerializeField] private ActiveCatchEffectRecord[] activeEffectRecords = Array.Empty<ActiveCatchEffectRecord>();
    [SerializeField] private int nextInstanceId = 1;

    public CardInstance[] Catches => catches;
    public ActiveCatchEffectRecord[] ActiveEffectRecords => activeEffectRecords;

    /// <summary>
    /// Calculates the Line Load contributed by all catches still attached to the line.
    /// </summary>
    public int CurrentLineLoad
    {
        get
        {
            int load = 0;

            for (int i = 0; i < catches.Length; i++)
            {
                if (catches[i] != null)
                {
                    load += catches[i].CurrentWeight;
                }
            }

            return load;
        }
    }

    /// <summary>
    /// Adds a committed encounter and tracks its catch-related effects.
    /// </summary>
    public void Add(CardDefinition caughtCard, EffectResolver effectResolver)
    {
        if (caughtCard == null)
        {
            return;
        }

        CardInstance caughtInstance = new CardInstance(nextInstanceId, caughtCard);
        nextInstanceId++;
        catches = AppendCatch(catches, caughtInstance);
        RebuildActiveEffectRecords();
        effectResolver.ResolveCatchChain(catches, activeEffectRecords);
    }

    /// <summary>
    /// Releases one catch and reports the removed card and its previous Line Load.
    /// </summary>
    public bool TryRelease(
        int catchIndex,
        EffectResolver effectResolver,
        out CardInstance releasedCatch,
        out int previousLineLoad,
        out string validationMessage)
    {
        releasedCatch = null;
        previousLineLoad = CurrentLineLoad;
        validationMessage = string.Empty;

        if (catchIndex < 0 || catchIndex >= catches.Length)
        {
            validationMessage = $"Catch Chain index is out of range: {catchIndex}.";
            return false;
        }

        releasedCatch = catches[catchIndex];

        if (releasedCatch == null)
        {
            validationMessage = $"Catch Chain slot {catchIndex} is empty.";
            return false;
        }

        catches = RemoveCatchAt(catches, catchIndex);
        RebuildActiveEffectRecords();
        effectResolver.ResolveCatchChain(catches, activeEffectRecords);
        return true;
    }

    /// <summary>
    /// Returns a separate snapshot of all catches currently attached to the line.
    /// </summary>
    public CardInstance[] CreateSnapshot()
    {
        CardInstance[] result = new CardInstance[catches.Length];

        for (int i = 0; i < catches.Length; i++)
        {
            result[i] = catches[i]?.CreateSnapshot();
        }

        return result;
    }

    /// <summary>
    /// Clears all Catch Chain cards and active effect records.
    /// </summary>
    public void Reset()
    {
        catches = Array.Empty<CardInstance>();
        activeEffectRecords = Array.Empty<ActiveCatchEffectRecord>();
        nextInstanceId = 1;
    }

    /// <summary>
    /// Tracks effects that become relevant when a card enters the Catch Chain.
    /// </summary>
    private void AddCatchEffects(CardInstance caughtInstance, int catchIndex)
    {
        AddActiveEffects(caughtInstance, CardEffectTrigger.WhenCaught, catchIndex);
        AddActiveEffects(caughtInstance, CardEffectTrigger.WhileAttached, catchIndex);
    }

    /// <summary>
    /// Rebuilds active effect records from catches that remain attached.
    /// </summary>
    private void RebuildActiveEffectRecords()
    {
        activeEffectRecords = Array.Empty<ActiveCatchEffectRecord>();

        // Rebuilding also preserves the correct number of records for repeated card definitions.
        for (int i = 0; i < catches.Length; i++)
        {
            AddCatchEffects(catches[i], i);
        }
    }

    /// <summary>
    /// Adds catch effects matching one active trigger.
    /// </summary>
    private void AddActiveEffects(CardInstance sourceInstance, CardEffectTrigger trigger, int catchIndex)
    {
        CardDefinition sourceCard = sourceInstance?.Definition;

        if (sourceCard == null || sourceCard.Effects == null)
        {
            return;
        }

        List<ActiveCatchEffectRecord> records = new List<ActiveCatchEffectRecord>(activeEffectRecords);

        for (int i = 0; i < sourceCard.Effects.Length; i++)
        {
            CardEffectDefinition effect = sourceCard.Effects[i];

            if (effect == null || effect.Trigger != trigger)
            {
                continue;
            }

            records.Add(new ActiveCatchEffectRecord(sourceInstance, effect, trigger, catchIndex));
        }

        activeEffectRecords = records.ToArray();
    }

    /// <summary>
    /// Returns a new catch-instance array with one catch appended.
    /// </summary>
    private static CardInstance[] AppendCatch(CardInstance[] source, CardInstance caughtInstance)
    {
        CardInstance[] result = new CardInstance[source.Length + 1];

        for (int i = 0; i < source.Length; i++)
        {
            result[i] = source[i];
        }

        result[result.Length - 1] = caughtInstance;
        return result;
    }

    /// <summary>
    /// Returns a new catch-instance array without the catch at the requested index.
    /// </summary>
    private static CardInstance[] RemoveCatchAt(CardInstance[] source, int removeIndex)
    {
        CardInstance[] result = new CardInstance[source.Length - 1];
        int resultIndex = 0;

        for (int i = 0; i < source.Length; i++)
        {
            if (i == removeIndex)
            {
                continue;
            }

            result[resultIndex] = source[i];
            resultIndex++;
        }

        return result;
    }
}
