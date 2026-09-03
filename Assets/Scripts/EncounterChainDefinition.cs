using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Encounter Chain", menuName = "Fishing Cards/Encounter Chain Definition")]
public sealed class EncounterChainDefinition : ScriptableObject
{
    [SerializeField] private string chainId;
    [SerializeField] private string displayName;
    [TextArea(2, 5)]
    [SerializeField] private string narrativeIntent;
    [SerializeField] private CardDefinition[] sequence = Array.Empty<CardDefinition>();

    public string ChainId => chainId;
    public string DisplayName => displayName;
    public string NarrativeIntent => narrativeIntent;
    public CardDefinition[] Sequence => sequence;

    /// <summary>
    /// Checks whether a selected encounter is the authored trigger at the start of this chain.
    /// </summary>
    public bool StartsWith(CardDefinition encounter)
    {
        return encounter != null && sequence != null && sequence.Length > 1 && sequence[0] == encounter;
    }

    /// <summary>
    /// Returns a chain card by sequence index without exposing invalid array access.
    /// </summary>
    public bool TryGetCard(int sequenceIndex, out CardDefinition encounter)
    {
        encounter = null;

        if (sequence == null || sequenceIndex < 0 || sequenceIndex >= sequence.Length)
        {
            return false;
        }

        encounter = sequence[sequenceIndex];
        return encounter != null;
    }
}
