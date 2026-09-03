using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class EncounterVarietyRuntime
{
    private const int MaximumRecentEncounterHistory = 8;
    private const int MaximumConsecutiveCreatureEncounters = 2;

    [SerializeField] private CardDefinition[] recentEncounterHistory = Array.Empty<CardDefinition>();
    [SerializeField] private string recentEncounterSequenceSummary;
    [SerializeField] private int consecutiveCreatureEncounters;
    [SerializeField] private string lastRepetitionRuleSummary;

    public CardDefinition[] RecentEncounterHistory => recentEncounterHistory;
    public string RecentEncounterSequenceSummary => recentEncounterSequenceSummary;
    public int ConsecutiveCreatureEncounters => consecutiveCreatureEncounters;
    public string LastRepetitionRuleSummary => lastRepetitionRuleSummary;

    /// <summary>
    /// Clears the previous rule report before evaluating a new encounter selection.
    /// </summary>
    public void BeginSelection()
    {
        lastRepetitionRuleSummary = "None";
    }

    /// <summary>
    /// Removes avoidable immediate repeats and breaks long ordinary-creature streaks.
    /// </summary>
    public void ApplyRepetitionRules(List<CardDefinition> candidates)
    {
        if (candidates == null || candidates.Count <= 1)
        {
            return;
        }

        CardDefinition previousEncounter = GetMostRecentEncounter();

        if (previousEncounter != null && ContainsDifferentEncounter(candidates, previousEncounter))
        {
            RemoveEncounter(candidates, previousEncounter);
            lastRepetitionRuleSummary = $"Prevented immediate repeat of {previousEncounter.DisplayName}";
        }

        if (consecutiveCreatureEncounters < MaximumConsecutiveCreatureEncounters
            || !ContainsNonCreatureEncounter(candidates))
        {
            return;
        }

        RemoveOrdinaryCreatures(candidates);
        lastRepetitionRuleSummary = lastRepetitionRuleSummary == "None"
            ? "Creature streak capped at 2"
            : $"{lastRepetitionRuleSummary}; creature streak capped at 2";
    }

    /// <summary>
    /// Records a selected encounter for streak rules and an Inspector-visible sequence summary.
    /// </summary>
    public void RecordSelectedEncounter(CardDefinition encounter)
    {
        if (encounter == null)
        {
            return;
        }

        consecutiveCreatureEncounters = IsOrdinaryCreature(encounter)
            ? consecutiveCreatureEncounters + 1
            : 0;

        int existingCount = recentEncounterHistory == null ? 0 : recentEncounterHistory.Length;
        int retainedCount = Math.Min(existingCount, MaximumRecentEncounterHistory - 1);
        CardDefinition[] updatedHistory = new CardDefinition[retainedCount + 1];
        int sourceStartIndex = existingCount - retainedCount;

        for (int i = 0; i < retainedCount; i++)
        {
            updatedHistory[i] = recentEncounterHistory[sourceStartIndex + i];
        }

        updatedHistory[updatedHistory.Length - 1] = encounter;
        recentEncounterHistory = updatedHistory;
        UpdateEncounterSequenceSummary();
    }

    /// <summary>
    /// Reports that an authored chain intentionally took priority over repetition filtering.
    /// </summary>
    public void RecordAuthoredChainFollowUp()
    {
        lastRepetitionRuleSummary = "Authored chain follow-up";
    }

    /// <summary>
    /// Clears all sequence memory and Inspector diagnostics for a new run.
    /// </summary>
    public void Reset()
    {
        recentEncounterHistory = Array.Empty<CardDefinition>();
        recentEncounterSequenceSummary = string.Empty;
        consecutiveCreatureEncounters = 0;
        lastRepetitionRuleSummary = string.Empty;
    }

    /// <summary>
    /// Returns the most recently selected encounter, or null before the first reveal.
    /// </summary>
    private CardDefinition GetMostRecentEncounter()
    {
        return recentEncounterHistory == null || recentEncounterHistory.Length == 0
            ? null
            : recentEncounterHistory[recentEncounterHistory.Length - 1];
    }

    /// <summary>
    /// Checks whether at least one candidate differs from the previous encounter.
    /// </summary>
    private static bool ContainsDifferentEncounter(List<CardDefinition> candidates, CardDefinition previousEncounter)
    {
        for (int i = 0; i < candidates.Count; i++)
        {
            if (candidates[i] != previousEncounter)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Removes every pool entry for one encounter definition.
    /// </summary>
    private static void RemoveEncounter(List<CardDefinition> candidates, CardDefinition encounter)
    {
        for (int i = candidates.Count - 1; i >= 0; i--)
        {
            if (candidates[i] == encounter)
            {
                candidates.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Checks whether a non-creature candidate can interrupt an ordinary-creature streak.
    /// </summary>
    private static bool ContainsNonCreatureEncounter(List<CardDefinition> candidates)
    {
        for (int i = 0; i < candidates.Count; i++)
        {
            if (!IsOrdinaryCreature(candidates[i]))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Removes ordinary creatures while retaining treasure, events, and Apex encounters.
    /// </summary>
    private static void RemoveOrdinaryCreatures(List<CardDefinition> candidates)
    {
        for (int i = candidates.Count - 1; i >= 0; i--)
        {
            if (IsOrdinaryCreature(candidates[i]))
            {
                candidates.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Builds a compact category-and-name history for Play Mode inspection.
    /// </summary>
    private void UpdateEncounterSequenceSummary()
    {
        string[] labels = new string[recentEncounterHistory.Length];

        for (int i = 0; i < recentEncounterHistory.Length; i++)
        {
            CardDefinition encounter = recentEncounterHistory[i];
            labels[i] = encounter == null
                ? "None"
                : $"{GetEncounterCategoryLabel(encounter)}: {encounter.DisplayName}";
        }

        recentEncounterSequenceSummary = string.Join(" -> ", labels);
    }

    /// <summary>
    /// Returns the useful sequence category for an encounter-history label.
    /// </summary>
    private static string GetEncounterCategoryLabel(CardDefinition encounter)
    {
        if (encounter.CardType == CardType.ApexEncounter)
        {
            return "Apex";
        }

        if (encounter.Rarity == CardRarity.Rare)
        {
            return $"Rare {encounter.CardType}";
        }

        return encounter.CardType.ToString();
    }

    /// <summary>
    /// Checks whether an encounter belongs to the ordinary creature category limited by streak rules.
    /// </summary>
    private static bool IsOrdinaryCreature(CardDefinition card)
    {
        return card != null && card.CardType == CardType.Creature;
    }
}
