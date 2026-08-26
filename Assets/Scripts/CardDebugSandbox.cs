using System;
using System.Text;
using UnityEngine;

public sealed class CardDebugSandbox : MonoBehaviour
{
    [Header("Debug Inputs")]
    [SerializeField] private CardDefinition debugEncounterCard;
    [SerializeField] private CardDefinition debugTechniqueCard;
    [SerializeField] private CardDefinition debugCatchCard;
    [SerializeField] private int debugDepth;
    [SerializeField] private int debugLineCapacity = 10;

    [Header("Debug State")]
    [SerializeField] private CardDefinition currentEncounter;
    [SerializeField] private CardDefinition[] techniqueHand = Array.Empty<CardDefinition>();
    [SerializeField] private CardDefinition[] catchChain = Array.Empty<CardDefinition>();
    [SerializeField] private int currentDepth;
    [SerializeField] private int lineCapacity = 10;

    public CardDefinition CurrentEncounter => currentEncounter;
    public CardDefinition[] TechniqueHand => techniqueHand;
    public CardDefinition[] CatchChain => catchChain;
    public int CurrentDepth => currentDepth;
    public int LineCapacity => lineCapacity;

    /// <summary>
    /// Calculates the current load from attached debug catches.
    /// </summary>
    public int CurrentLineLoad
    {
        get
        {
            int load = 0;

            for (int i = 0; i < catchChain.Length; i++)
            {
                if (catchChain[i] != null)
                {
                    load += catchChain[i].Weight;
                }
            }

            return load;
        }
    }

    /// <summary>
    /// Replaces the current debug encounter with a specific card.
    /// </summary>
    public void SpawnSpecificEncounter(CardDefinition card)
    {
        currentEncounter = card;
        Debug.Log(card == null ? "Debug encounter cleared." : $"Debug encounter spawned: {card.BuildDebugSummary()}", this);
    }

    /// <summary>
    /// Adds a specific Technique card to the debug hand.
    /// </summary>
    public void GiveTechniqueCard(CardDefinition card)
    {
        if (card == null)
        {
            Debug.LogWarning("Cannot add a null technique card.", this);
            return;
        }

        AddCard(ref techniqueHand, card);
        Debug.Log($"Technique card added: {card.DisplayName}", this);
    }

    /// <summary>
    /// Sets the debug depth while clamping it to a non-negative value.
    /// </summary>
    public void ChangeDepth(int depth)
    {
        currentDepth = Mathf.Max(0, depth);
        Debug.Log($"Depth set to {currentDepth}.", this);
    }

    /// <summary>
    /// Sets the debug Line Capacity while clamping it to a non-negative value.
    /// </summary>
    public void ChangeLineCapacity(int capacity)
    {
        lineCapacity = Mathf.Max(0, capacity);
        Debug.Log($"Line Capacity set to {lineCapacity}. Current Load: {CurrentLineLoad}.", this);
    }

    /// <summary>
    /// Adds a card to the debug Catch Chain so load and active effects can be inspected.
    /// </summary>
    public void AddCatch(CardDefinition card)
    {
        if (card == null)
        {
            Debug.LogWarning("Cannot add a null catch.", this);
            return;
        }

        AddCard(ref catchChain, card);
        Debug.Log($"Catch added: {card.DisplayName}. Load: {CurrentLineLoad}/{lineCapacity}.", this);
    }

    /// <summary>
    /// Removes the first matching card from the debug Catch Chain.
    /// </summary>
    public void RemoveCatch(CardDefinition card)
    {
        if (card == null)
        {
            Debug.LogWarning("Cannot remove a null catch.", this);
            return;
        }

        if (RemoveCard(ref catchChain, card))
        {
            Debug.Log($"Catch removed: {card.DisplayName}. Load: {CurrentLineLoad}/{lineCapacity}.", this);
            return;
        }

        Debug.LogWarning($"Catch was not attached: {card.DisplayName}.", this);
    }

    /// <summary>
    /// Logs the current debug run state and all visible effect definitions.
    /// </summary>
    public void PrintActiveEffects()
    {
        StringBuilder output = new StringBuilder();
        output.AppendLine($"Depth: {currentDepth}");
        output.AppendLine($"Line Load: {CurrentLineLoad}/{lineCapacity}");
        output.AppendLine(currentEncounter == null ? "Current Encounter: none" : $"Current Encounter: {currentEncounter.DisplayName}");
        AppendCardEffects(output, "Encounter Effects", currentEncounter);
        AppendCardListEffects(output, "Technique Hand Effects", techniqueHand);
        AppendCardListEffects(output, "Catch Chain Effects", catchChain);
        Debug.Log(output.ToString(), this);
    }

    /// <summary>
    /// Clears the debug run state back to a fresh starting point.
    /// </summary>
    public void RestartRunInstantly()
    {
        currentEncounter = null;
        techniqueHand = Array.Empty<CardDefinition>();
        catchChain = Array.Empty<CardDefinition>();
        currentDepth = 0;
        lineCapacity = debugLineCapacity;
        Debug.Log("Debug run state restarted.", this);
    }

    /// <summary>
    /// Inspector context-menu wrapper for spawning the configured debug encounter.
    /// </summary>
    [ContextMenu("Debug/Spawn Specific Encounter")]
    private void SpawnDebugEncounter()
    {
        SpawnSpecificEncounter(debugEncounterCard);
    }

    /// <summary>
    /// Inspector context-menu wrapper for adding the configured debug Technique card.
    /// </summary>
    [ContextMenu("Debug/Give Specific Technique Card")]
    private void GiveDebugTechniqueCard()
    {
        GiveTechniqueCard(debugTechniqueCard);
    }

    /// <summary>
    /// Inspector context-menu wrapper for applying the configured debug depth.
    /// </summary>
    [ContextMenu("Debug/Change Depth Manually")]
    private void ChangeDebugDepth()
    {
        ChangeDepth(debugDepth);
    }

    /// <summary>
    /// Inspector context-menu wrapper for applying the configured debug Line Capacity.
    /// </summary>
    [ContextMenu("Debug/Change Line Capacity Manually")]
    private void ChangeDebugLineCapacity()
    {
        ChangeLineCapacity(debugLineCapacity);
    }

    /// <summary>
    /// Inspector context-menu wrapper for adding the configured debug catch.
    /// </summary>
    [ContextMenu("Debug/Add Catch Manually")]
    private void AddDebugCatch()
    {
        AddCatch(debugCatchCard);
    }

    /// <summary>
    /// Inspector context-menu wrapper for removing the configured debug catch.
    /// </summary>
    [ContextMenu("Debug/Remove Catch Manually")]
    private void RemoveDebugCatch()
    {
        RemoveCatch(debugCatchCard);
    }

    /// <summary>
    /// Inspector context-menu wrapper for logging debug effects.
    /// </summary>
    [ContextMenu("Debug/Print Active Effects")]
    private void PrintDebugActiveEffects()
    {
        PrintActiveEffects();
    }

    /// <summary>
    /// Inspector context-menu wrapper for resetting the debug run state.
    /// </summary>
    [ContextMenu("Debug/Restart Run Instantly")]
    private void RestartDebugRun()
    {
        RestartRunInstantly();
    }

    /// <summary>
    /// Appends a card to an array-backed debug list.
    /// </summary>
    private static void AddCard(ref CardDefinition[] cards, CardDefinition card)
    {
        int originalLength = cards == null ? 0 : cards.Length;
        CardDefinition[] updatedCards = new CardDefinition[originalLength + 1];

        for (int i = 0; i < originalLength; i++)
        {
            updatedCards[i] = cards[i];
        }

        updatedCards[originalLength] = card;
        cards = updatedCards;
    }

    /// <summary>
    /// Removes the first matching card from an array-backed debug list.
    /// </summary>
    private static bool RemoveCard(ref CardDefinition[] cards, CardDefinition card)
    {
        if (cards == null || cards.Length == 0)
        {
            return false;
        }

        int removeIndex = -1;
        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] == card)
            {
                removeIndex = i;
                break;
            }
        }

        if (removeIndex < 0)
        {
            return false;
        }

        CardDefinition[] updatedCards = new CardDefinition[cards.Length - 1];
        int writeIndex = 0;

        for (int i = 0; i < cards.Length; i++)
        {
            if (i == removeIndex)
            {
                continue;
            }

            updatedCards[writeIndex] = cards[i];
            writeIndex++;
        }

        cards = updatedCards;
        return true;
    }

    /// <summary>
    /// Appends effect summaries for each card in a debug card list.
    /// </summary>
    private static void AppendCardListEffects(StringBuilder output, string heading, CardDefinition[] cards)
    {
        output.AppendLine(heading + ":");

        if (cards == null || cards.Length == 0)
        {
            output.AppendLine("- none");
            return;
        }

        for (int i = 0; i < cards.Length; i++)
        {
            AppendCardEffects(output, cards[i] == null ? "null" : cards[i].DisplayName, cards[i]);
        }
    }

    /// <summary>
    /// Appends effect summaries for a single card.
    /// </summary>
    private static void AppendCardEffects(StringBuilder output, string heading, CardDefinition card)
    {
        output.AppendLine(heading + ":");

        if (card == null || card.Effects == null || card.Effects.Length == 0)
        {
            output.AppendLine("- none");
            return;
        }

        for (int i = 0; i < card.Effects.Length; i++)
        {
            output.Append("- ");
            output.AppendLine(card.Effects[i].BuildDebugSummary());
        }
    }
}
