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
    [SerializeField] private int lastSelectedEncounterWeight;
    [SerializeField] private int lastTotalEncounterWeight;
    [SerializeField] private string lastCandidateWeightSummary;
    [SerializeField] private EncounterChainDefinition activeEncounterChain;
    [SerializeField] private int nextEncounterChainIndex;
    [SerializeField] private EncounterVarietyRuntime encounterVarietyRuntime = new EncounterVarietyRuntime();

    public CardDefinition CurrentEncounter => currentEncounter;
    public EncounterState CurrentState => currentState;
    public CardDefinition HookedEncounter => hookedEncounter;
    public HookedEffectRecord[] HookedEffectRecords => hookedEffectRecords;
    public int LastSelectedEncounterWeight => lastSelectedEncounterWeight;
    public int LastTotalEncounterWeight => lastTotalEncounterWeight;
    public string LastCandidateWeightSummary => lastCandidateWeightSummary;
    public EncounterChainDefinition ActiveEncounterChain => activeEncounterChain;
    public int NextEncounterChainIndex => nextEncounterChainIndex;
    public CardDefinition[] RecentEncounterHistory => encounterVarietyRuntime == null
        ? Array.Empty<CardDefinition>()
        : encounterVarietyRuntime.RecentEncounterHistory;
    public string RecentEncounterSequenceSummary => encounterVarietyRuntime == null
        ? string.Empty
        : encounterVarietyRuntime.RecentEncounterSequenceSummary;
    public int ConsecutiveCreatureEncounters => encounterVarietyRuntime == null
        ? 0
        : encounterVarietyRuntime.ConsecutiveCreatureEncounters;
    public string LastRepetitionRuleSummary => encounterVarietyRuntime == null
        ? string.Empty
        : encounterVarietyRuntime.LastRepetitionRuleSummary;

    /// <summary>
    /// Selects and activates a valid encounter for the current biome and depth.
    /// </summary>
    public bool Reveal(
        CardDefinition[] encounterPool,
        string biomeId,
        int depth,
        System.Random random,
        ActiveCatchEffectRecord[] activeCatchEffects,
        EffectResolver effectResolver,
        CardEffectDefinition[] temporaryEncounterEffects = null,
        CardDefinition excludedEncounter = null,
        EncounterChainDefinition[] encounterChains = null)
    {
        List<CardDefinition> candidates = new List<CardDefinition>();
        List<int> candidateWeights = new List<int>();
        List<string> candidateWeightLabels = new List<string>();
        EnsureEncounterVarietyRuntime();
        lastSelectedEncounterWeight = 0;
        lastTotalEncounterWeight = 0;
        lastCandidateWeightSummary = string.Empty;
        encounterVarietyRuntime.BeginSelection();

        if (TryRevealActiveChain(biomeId, depth))
        {
            return true;
        }

        if (encounterPool != null)
        {
            for (int i = 0; i < encounterPool.Length; i++)
            {
                CardDefinition card = encounterPool[i];

                if (card == null || card == excludedEncounter || !IsEncounterCard(card))
                {
                    continue;
                }

                if (card.IsAvailableInBiome(biomeId) && card.IsAvailableAtDepth(depth))
                {
                    candidates.Add(card);
                }
            }
        }

        encounterVarietyRuntime.ApplyRepetitionRules(candidates);

        if (candidates.Count == 0)
        {
            SetCurrentEncounter(null);
            return false;
        }

        for (int i = 0; i < candidates.Count; i++)
        {
            CardDefinition candidate = candidates[i];
            int selectionWeight = effectResolver.GetEncounterSelectionWeight(
                candidate,
                activeCatchEffects,
                temporaryEncounterEffects);
            candidateWeights.Add(selectionWeight);
            candidateWeightLabels.Add($"{candidate.DisplayName}: {selectionWeight}");
            lastTotalEncounterWeight += selectionWeight;
        }

        int selectedIndex = SelectWeightedIndex(candidateWeights, lastTotalEncounterWeight, random);
        lastCandidateWeightSummary = string.Join(", ", candidateWeightLabels);
        lastSelectedEncounterWeight = candidateWeights[selectedIndex];
        SetCurrentEncounter(candidates[selectedIndex]);
        encounterVarietyRuntime.RecordSelectedEncounter(currentEncounter);
        BeginEncounterChain(currentEncounter, encounterChains);
        return true;
    }

    /// <summary>
    /// Activates a preselected biome Apex through the normal Hooked encounter state.
    /// </summary>
    public bool RevealApex(CardDefinition apexEncounter)
    {
        if (apexEncounter == null || apexEncounter.CardType != CardType.ApexEncounter)
        {
            return false;
        }

        EnsureEncounterVarietyRuntime();
        CancelActiveEncounterChain();
        lastSelectedEncounterWeight = 1;
        lastTotalEncounterWeight = 1;
        lastCandidateWeightSummary = $"Biome Apex: {apexEncounter.DisplayName}";
        encounterVarietyRuntime.BeginSelection();
        SetCurrentEncounter(apexEncounter);
        encounterVarietyRuntime.RecordSelectedEncounter(apexEncounter);
        return true;
    }

    /// <summary>
    /// Cancels a queued follow-up when the encounter that started it is avoided or replaced.
    /// </summary>
    public void CancelActiveEncounterChain()
    {
        activeEncounterChain = null;
        nextEncounterChainIndex = 0;
    }

    /// <summary>
    /// Reports whether the pool contains another valid encounter for replacement at a depth.
    /// </summary>
    public bool HasAlternativeEncounter(
        CardDefinition[] encounterPool,
        string biomeId,
        int depth,
        CardDefinition excludedEncounter)
    {
        if (encounterPool == null)
        {
            return false;
        }

        for (int i = 0; i < encounterPool.Length; i++)
        {
            CardDefinition card = encounterPool[i];

            if (card != null
                && card != excludedEncounter
                && IsEncounterCard(card)
                && card.IsAvailableInBiome(biomeId)
                && card.IsAvailableAtDepth(depth))
            {
                return true;
            }
        }

        return false;
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
    /// Counts effects on a Technique card that can currently target the Hooked encounter.
    /// </summary>
    public int CountApplicableTechniqueEffects(CardDefinition techniqueCard)
    {
        if (techniqueCard == null || techniqueCard.Effects == null || hookedEncounter == null)
        {
            return 0;
        }

        int applicableEffects = 0;

        for (int i = 0; i < techniqueCard.Effects.Length; i++)
        {
            if (DoesEffectAffectHookedEncounter(techniqueCard.Effects[i], hookedEncounter))
            {
                applicableEffects++;
            }
        }

        return applicableEffects;
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
        lastSelectedEncounterWeight = 0;
        lastTotalEncounterWeight = 0;
        lastCandidateWeightSummary = string.Empty;
        EnsureEncounterVarietyRuntime();
        encounterVarietyRuntime.Reset();
        CancelActiveEncounterChain();
    }

    /// <summary>
    /// Restores the variety runtime when loading scene data created before it existed.
    /// </summary>
    private void EnsureEncounterVarietyRuntime()
    {
        if (encounterVarietyRuntime == null)
        {
            encounterVarietyRuntime = new EncounterVarietyRuntime();
        }
    }

    /// <summary>
    /// Selects one candidate from integer weights, falling back to uniform selection when all weights are zero.
    /// </summary>
    private static int SelectWeightedIndex(List<int> weights, int totalWeight, System.Random random)
    {
        if (totalWeight <= 0)
        {
            return random.Next(weights.Count);
        }

        int roll = random.Next(totalWeight);
        int cumulativeWeight = 0;

        for (int i = 0; i < weights.Count; i++)
        {
            cumulativeWeight += weights[i];

            if (roll < cumulativeWeight)
            {
                return i;
            }
        }

        return weights.Count - 1;
    }

    /// <summary>
    /// Reveals the next valid card in an active short encounter sequence.
    /// </summary>
    private bool TryRevealActiveChain(string biomeId, int depth)
    {
        if (activeEncounterChain == null
            || !activeEncounterChain.TryGetCard(nextEncounterChainIndex, out CardDefinition chainedEncounter))
        {
            CancelActiveEncounterChain();
            return false;
        }

        if (!IsEncounterCard(chainedEncounter)
            || !chainedEncounter.IsAvailableInBiome(biomeId)
            || !chainedEncounter.IsAvailableAtDepth(depth))
        {
            CancelActiveEncounterChain();
            return false;
        }

        nextEncounterChainIndex++;

        if (!activeEncounterChain.TryGetCard(nextEncounterChainIndex, out _))
        {
            EncounterChainDefinition completedChain = activeEncounterChain;
            CancelActiveEncounterChain();
            lastCandidateWeightSummary = $"Chain: {completedChain.DisplayName}";
        }
        else
        {
            lastCandidateWeightSummary = $"Chain: {activeEncounterChain.DisplayName}";
        }

        lastSelectedEncounterWeight = 1;
        lastTotalEncounterWeight = 1;
        encounterVarietyRuntime.RecordAuthoredChainFollowUp();
        SetCurrentEncounter(chainedEncounter);
        encounterVarietyRuntime.RecordSelectedEncounter(chainedEncounter);
        return true;
    }

    /// <summary>
    /// Queues the second card when a randomly selected encounter starts an authored chain.
    /// </summary>
    private void BeginEncounterChain(CardDefinition selectedEncounter, EncounterChainDefinition[] encounterChains)
    {
        if (encounterChains == null)
        {
            return;
        }

        for (int i = 0; i < encounterChains.Length; i++)
        {
            EncounterChainDefinition chain = encounterChains[i];

            if (chain != null && chain.StartsWith(selectedEncounter))
            {
                activeEncounterChain = chain;
                nextEncounterChainIndex = 1;
                return;
            }
        }
    }

    /// <summary>
    /// Applies encounter and Hooked state for an explicitly supplied encounter card.
    /// </summary>
    public void SetCurrentEncounter(CardDefinition encounter)
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
            && (card.CardType == CardType.Creature
                || card.CardType == CardType.Treasure
                || card.CardType == CardType.ApexEncounter);
    }

}
