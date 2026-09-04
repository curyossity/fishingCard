using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class BiomeApexRuntime
{
    [SerializeField] private BiomeApexState state;
    [SerializeField] private int boundaryDepth = -1;
    [SerializeField] private CardDefinition selectedApex;
    [SerializeField] private int validPossibilityCount;
    [SerializeField] private string lastSelectionSummary;

    public BiomeApexState State => state;
    public int BoundaryDepth => boundaryDepth;
    public CardDefinition SelectedApex => selectedApex;
    public int ValidPossibilityCount => validPossibilityCount;
    public string LastSelectionSummary => lastSelectionSummary;
    public bool HasReachedBoundary => state != BiomeApexState.NotReached;

    /// <summary>
    /// Selects one valid Apex the first time the run reaches a biome's boundary.
    /// </summary>
    public bool TrySelectApex(
        BiomeDefinition biome,
        int currentDepth,
        System.Random random,
        out CardDefinition apex)
    {
        apex = null;

        if (HasReachedBoundary || biome == null || random == null)
        {
            return false;
        }

        int authoredBoundaryDepth = biome.GetApexBoundaryDepth();

        if (authoredBoundaryDepth < 0 || currentDepth < authoredBoundaryDepth)
        {
            return false;
        }

        boundaryDepth = authoredBoundaryDepth;
        List<CardDefinition> candidates = BuildValidCandidates(biome, authoredBoundaryDepth);
        validPossibilityCount = candidates.Count;

        if (candidates.Count == 0)
        {
            state = BiomeApexState.Unavailable;
            lastSelectionSummary = $"No valid Apex possibilities at boundary depth {authoredBoundaryDepth}.";
            return false;
        }

        selectedApex = candidates[random.Next(candidates.Count)];
        state = BiomeApexState.Hooked;
        lastSelectionSummary = $"Selected {selectedApex.DisplayName} from {candidates.Count} Apex possibilities at depth {authoredBoundaryDepth}.";
        apex = selectedApex;
        return true;
    }

    /// <summary>
    /// Reports whether the supplied Hooked card is the currently selected biome Apex.
    /// </summary>
    public bool IsCurrentApex(CardDefinition encounter)
    {
        return state == BiomeApexState.Hooked
            && selectedApex != null
            && encounter == selectedApex;
    }

    /// <summary>
    /// Records whether a Technique kept, replaced, or avoided the selected Apex.
    /// </summary>
    public void RecordTechniqueResolution(CardDefinition previousEncounter, CardDefinition currentEncounter)
    {
        if (!IsCurrentApex(previousEncounter) || currentEncounter == previousEncounter)
        {
            return;
        }

        if (currentEncounter == null)
        {
            state = BiomeApexState.Avoided;
            lastSelectionSummary = $"{previousEncounter.DisplayName} was avoided with a Technique card.";
            return;
        }

        if (currentEncounter.CardType == CardType.ApexEncounter)
        {
            selectedApex = currentEncounter;
            lastSelectionSummary = $"{previousEncounter.DisplayName} was replaced by {currentEncounter.DisplayName}.";
        }
    }

    /// <summary>
    /// Records that the selected Apex entered the Catch Chain through Descend.
    /// </summary>
    public void RecordCommittedApex(CardDefinition committedEncounter)
    {
        if (!IsCurrentApex(committedEncounter))
        {
            return;
        }

        state = BiomeApexState.Caught;
        lastSelectionSummary = $"{committedEncounter.DisplayName} entered the Catch Chain.";
    }

    /// <summary>
    /// Clears all one-run Apex selection and resolution state.
    /// </summary>
    public void Reset()
    {
        state = BiomeApexState.NotReached;
        boundaryDepth = -1;
        selectedApex = null;
        validPossibilityCount = 0;
        lastSelectionSummary = string.Empty;
    }

    /// <summary>
    /// Builds the valid Apex candidate list from biome-authored possibilities.
    /// </summary>
    private static List<CardDefinition> BuildValidCandidates(BiomeDefinition biome, int boundaryDepth)
    {
        List<CardDefinition> candidates = new List<CardDefinition>();
        CardDefinition[] possibilities = biome.ApexEncounters;

        if (possibilities == null)
        {
            return candidates;
        }

        for (int i = 0; i < possibilities.Length; i++)
        {
            CardDefinition card = possibilities[i];

            if (card != null
                && card.CardType == CardType.ApexEncounter
                && card.IsAvailableInBiome(biome.BiomeId)
                && card.IsAvailableAtDepth(boundaryDepth))
            {
                candidates.Add(card);
            }
        }

        return candidates;
    }
}

public enum BiomeApexState
{
    NotReached,
    Hooked,
    Avoided,
    Caught,
    Unavailable
}
