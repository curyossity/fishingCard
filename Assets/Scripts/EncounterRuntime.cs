using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class EncounterRuntime
{
    [SerializeField] private CardDefinition currentEncounter;
    [SerializeField] private EncounterState currentState;
    [SerializeField] private CardDefinition hookedEncounter;
    [SerializeField] private HookedEffectRecord[] hookedEffectRecords = Array.Empty<HookedEffectRecord>();

    public CardDefinition CurrentEncounter => currentEncounter;
    public EncounterState CurrentState => currentState;
    public CardDefinition HookedEncounter => hookedEncounter;
    public HookedEffectRecord[] HookedEffectRecords => hookedEffectRecords;

    /// <summary>
    /// Selects and activates a valid encounter for the current biome and depth.
    /// </summary>
    public bool Reveal(CardDefinition[] encounterPool, string biomeId, int depth, System.Random random)
    {
        List<CardDefinition> candidates = new List<CardDefinition>();

        if (encounterPool != null)
        {
            for (int i = 0; i < encounterPool.Length; i++)
            {
                CardDefinition card = encounterPool[i];

                if (card == null || !IsEncounterCard(card))
                {
                    continue;
                }

                if (card.IsAvailableInBiome(biomeId) && card.IsAvailableAtDepth(depth))
                {
                    candidates.Add(card);
                }
            }
        }

        if (candidates.Count == 0)
        {
            SetCurrentEncounter(null);
            return false;
        }

        SetCurrentEncounter(candidates[random.Next(candidates.Count)]);
        return true;
    }

    /// <summary>
    /// Removes and returns the Hooked encounter so it can enter the Catch Chain.
    /// </summary>
    public CardDefinition TakeHookedEncounter()
    {
        if (hookedEncounter == null)
        {
            return null;
        }

        CardDefinition caughtCard = hookedEncounter;
        hookedEncounter = null;
        hookedEffectRecords = Array.Empty<HookedEffectRecord>();
        currentState = EncounterState.Caught;
        return caughtCard;
    }

    /// <summary>
    /// Adds Technique effects that can influence the current Hooked encounter.
    /// </summary>
    public int AddTechniqueEffects(CardDefinition techniqueCard)
    {
        List<HookedEffectRecord> records = new List<HookedEffectRecord>(hookedEffectRecords);
        int originalCount = records.Count;

        AddRelevantEffectRecords(records, techniqueCard, HookedEffectSource.TechniqueCard);
        hookedEffectRecords = records.ToArray();
        return records.Count - originalCount;
    }

    /// <summary>
    /// Clears the current encounter and its reaction-window state.
    /// </summary>
    public void Reset()
    {
        currentEncounter = null;
        currentState = EncounterState.None;
        hookedEncounter = null;
        hookedEffectRecords = Array.Empty<HookedEffectRecord>();
    }

    /// <summary>
    /// Applies encounter and Hooked state for a newly selected card.
    /// </summary>
    private void SetCurrentEncounter(CardDefinition encounter)
    {
        currentEncounter = encounter;

        if (currentEncounter == null)
        {
            hookedEncounter = null;
            currentState = EncounterState.None;
            RebuildHookedEffectRecords();
            return;
        }

        if (IsCatchableEncounter(currentEncounter))
        {
            hookedEncounter = currentEncounter;
            currentState = EncounterState.Hooked;
            RebuildHookedEffectRecords();
            return;
        }

        hookedEncounter = null;
        currentState = EncounterState.Encountered;
        RebuildHookedEffectRecords();
    }

    /// <summary>
    /// Rebuilds effects that can influence the current Hooked reaction window.
    /// </summary>
    private void RebuildHookedEffectRecords()
    {
        if (hookedEncounter == null)
        {
            hookedEffectRecords = Array.Empty<HookedEffectRecord>();
            return;
        }

        List<HookedEffectRecord> records = new List<HookedEffectRecord>();
        AddRelevantEffectRecords(records, hookedEncounter, HookedEffectSource.HookedEncounter);
        hookedEffectRecords = records.ToArray();
    }

    /// <summary>
    /// Adds effects from a source card when they match the current Hooked encounter.
    /// </summary>
    private void AddRelevantEffectRecords(
        List<HookedEffectRecord> records,
        CardDefinition sourceCard,
        HookedEffectSource sourceType)
    {
        if (sourceCard == null || sourceCard.Effects == null)
        {
            return;
        }

        for (int i = 0; i < sourceCard.Effects.Length; i++)
        {
            CardEffectDefinition effect = sourceCard.Effects[i];

            if (DoesEffectAffectHookedEncounter(effect, hookedEncounter))
            {
                records.Add(new HookedEffectRecord(sourceCard, effect, sourceType));
            }
        }
    }

    /// <summary>
    /// Checks whether an effect targets and matches the current Hooked encounter.
    /// </summary>
    private static bool DoesEffectAffectHookedEncounter(CardEffectDefinition effect, CardDefinition targetEncounter)
    {
        if (effect == null || targetEncounter == null)
        {
            return false;
        }

        bool targetsHookedCard = effect.Target == CardEffectTarget.HookedEncounter
            || effect.Target == CardEffectTarget.CurrentEncounter
            || effect.Target == CardEffectTarget.NextDescend;

        return targetsHookedCard && RequiredTagsMatch(effect.RequiredTags, targetEncounter);
    }

    /// <summary>
    /// Checks whether every required effect tag matches the target card.
    /// </summary>
    private static bool RequiredTagsMatch(string[] requiredTags, CardDefinition targetCard)
    {
        if (requiredTags == null || requiredTags.Length == 0)
        {
            return true;
        }

        if (targetCard == null)
        {
            return false;
        }

        for (int i = 0; i < requiredTags.Length; i++)
        {
            if (string.Equals(requiredTags[i], targetCard.CardType.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!HasTag(targetCard.Tags, requiredTags[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks whether a tag collection contains a requested tag, ignoring case.
    /// </summary>
    private static bool HasTag(string[] tags, string requiredTag)
    {
        if (tags == null || tags.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < tags.Length; i++)
        {
            if (string.Equals(requiredTag, tags[i], StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks whether a card belongs to the encounter-facing side of the game.
    /// </summary>
    private static bool IsEncounterCard(CardDefinition card)
    {
        // Apex uses encounter behavior, while later content systems keep it out of ordinary pools.
        return card.CardType == CardType.Creature
            || card.CardType == CardType.Treasure
            || card.CardType == CardType.Hazard
            || card.CardType == CardType.Environment
            || card.CardType == CardType.Opportunity
            || card.CardType == CardType.ApexEncounter
            || card.CardType == CardType.Encounter;
    }

    /// <summary>
    /// Checks whether a revealed encounter should enter the Hooked reaction state.
    /// </summary>
    private static bool IsCatchableEncounter(CardDefinition card)
    {
        return card != null
            && (card.CardType == CardType.Creature || card.CardType == CardType.ApexEncounter);
    }
}
