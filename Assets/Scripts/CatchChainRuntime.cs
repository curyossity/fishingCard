using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class CatchChainRuntime
{
    [SerializeField] private CardDefinition[] cards = Array.Empty<CardDefinition>();
    [SerializeField] private ActiveCatchEffectRecord[] activeEffectRecords = Array.Empty<ActiveCatchEffectRecord>();

    public CardDefinition[] Cards => cards;
    public ActiveCatchEffectRecord[] ActiveEffectRecords => activeEffectRecords;

    /// <summary>
    /// Calculates the Line Load contributed by all catches still attached to the line.
    /// </summary>
    public int CurrentLineLoad
    {
        get
        {
            int load = 0;

            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] != null)
                {
                    load += cards[i].Weight;
                }
            }

            return load;
        }
    }

    /// <summary>
    /// Adds a committed encounter and tracks its catch-related effects.
    /// </summary>
    public void Add(CardDefinition caughtCard)
    {
        if (caughtCard == null)
        {
            return;
        }

        cards = AppendCard(cards, caughtCard);
        AddCatchEffects(caughtCard);
    }

    /// <summary>
    /// Releases one catch and reports the removed card and its previous Line Load.
    /// </summary>
    public bool TryRelease(
        int catchIndex,
        out CardDefinition releasedCard,
        out int previousLineLoad,
        out string validationMessage)
    {
        releasedCard = null;
        previousLineLoad = CurrentLineLoad;
        validationMessage = string.Empty;

        if (catchIndex < 0 || catchIndex >= cards.Length)
        {
            validationMessage = $"Catch Chain index is out of range: {catchIndex}.";
            return false;
        }

        releasedCard = cards[catchIndex];

        if (releasedCard == null)
        {
            validationMessage = $"Catch Chain slot {catchIndex} is empty.";
            return false;
        }

        cards = RemoveCardAt(cards, catchIndex);
        RebuildActiveEffectRecords();
        return true;
    }

    /// <summary>
    /// Returns a separate snapshot of all catches currently attached to the line.
    /// </summary>
    public CardDefinition[] CreateSnapshot()
    {
        CardDefinition[] result = new CardDefinition[cards.Length];

        for (int i = 0; i < cards.Length; i++)
        {
            result[i] = cards[i];
        }

        return result;
    }

    /// <summary>
    /// Clears all Catch Chain cards and active effect records.
    /// </summary>
    public void Reset()
    {
        cards = Array.Empty<CardDefinition>();
        activeEffectRecords = Array.Empty<ActiveCatchEffectRecord>();
    }

    /// <summary>
    /// Tracks effects that become relevant when a card enters the Catch Chain.
    /// </summary>
    private void AddCatchEffects(CardDefinition caughtCard)
    {
        AddActiveEffects(caughtCard, CardEffectTrigger.WhenCaught);
        AddActiveEffects(caughtCard, CardEffectTrigger.WhileAttached);
    }

    /// <summary>
    /// Rebuilds active effect records from catches that remain attached.
    /// </summary>
    private void RebuildActiveEffectRecords()
    {
        activeEffectRecords = Array.Empty<ActiveCatchEffectRecord>();

        // Rebuilding also preserves the correct number of records for repeated card definitions.
        for (int i = 0; i < cards.Length; i++)
        {
            AddCatchEffects(cards[i]);
        }
    }

    /// <summary>
    /// Adds catch effects matching one active trigger.
    /// </summary>
    private void AddActiveEffects(CardDefinition sourceCard, CardEffectTrigger trigger)
    {
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

            records.Add(new ActiveCatchEffectRecord(sourceCard, effect, trigger));
        }

        activeEffectRecords = records.ToArray();
    }

    /// <summary>
    /// Returns a new card array with one card appended.
    /// </summary>
    private static CardDefinition[] AppendCard(CardDefinition[] source, CardDefinition card)
    {
        CardDefinition[] result = new CardDefinition[source.Length + 1];

        for (int i = 0; i < source.Length; i++)
        {
            result[i] = source[i];
        }

        result[result.Length - 1] = card;
        return result;
    }

    /// <summary>
    /// Returns a new card array without the card at the requested index.
    /// </summary>
    private static CardDefinition[] RemoveCardAt(CardDefinition[] source, int removeIndex)
    {
        CardDefinition[] result = new CardDefinition[source.Length - 1];
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
