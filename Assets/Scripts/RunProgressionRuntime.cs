using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class RunProgressionRuntime
{
    [Header("Line Capacity Upgrade")]
    [Min(0)]
    [SerializeField] private int lineCapacityUpgradeCost = 10;
    [Min(1)]
    [SerializeField] private int lineCapacityPerUpgrade = 2;
    [Min(1)]
    [SerializeField] private int maximumLineCapacityUpgrades = 3;

    [Header("Progression State")]
    [SerializeField] private int purchasedLineCapacityUpgrades;
    [SerializeField] private bool purchasedUpgradeAfterLastRun;
    [SerializeField] private string lastPurchaseSummary;

    [Header("Technique Unlock")]
    [SerializeField] private CardDefinition unlockableTechniqueCard;
    [Min(0)]
    [SerializeField] private int techniqueUnlockCost = 12;
    [SerializeField] private bool techniqueCardUnlocked;
    [SerializeField] private bool techniqueDeckInitialized;
    [SerializeField] private CardDefinition[] savedTechniqueDeck = Array.Empty<CardDefinition>();

    public int LineCapacityUpgradeCost => lineCapacityUpgradeCost;
    public int LineCapacityPerUpgrade => lineCapacityPerUpgrade;
    public int MaximumLineCapacityUpgrades => maximumLineCapacityUpgrades;
    public int PurchasedLineCapacityUpgrades => purchasedLineCapacityUpgrades;
    public int CurrentLineCapacityBonus => purchasedLineCapacityUpgrades * lineCapacityPerUpgrade;
    public string LastPurchaseSummary => lastPurchaseSummary;
    public CardDefinition UnlockableTechniqueCard => unlockableTechniqueCard;
    public int TechniqueUnlockCost => techniqueUnlockCost;
    public bool TechniqueCardUnlocked => techniqueCardUnlocked;
    public CardDefinition[] SavedTechniqueDeck => savedTechniqueDeck;

    /// <summary>
    /// Creates the session deck once from valid authored Technique cards without overwriting later changes.
    /// </summary>
    public void InitializeTechniqueDeck(CardDefinition[] authoredStartingDeck)
    {
        if (techniqueDeckInitialized)
        {
            return;
        }

        List<CardDefinition> validCards = new List<CardDefinition>();
        CardDefinition[] sourceDeck = authoredStartingDeck ?? Array.Empty<CardDefinition>();

        for (int i = 0; i < sourceDeck.Length; i++)
        {
            CardDefinition card = sourceDeck[i];

            if (card != null && card.CardType == CardType.Technique)
            {
                validCards.Add(card);
            }
        }

        savedTechniqueDeck = validCards.ToArray();
        techniqueCardUnlocked = ContainsCard(savedTechniqueDeck, unlockableTechniqueCard);
        techniqueDeckInitialized = true;
    }

    /// <summary>
    /// Clears the per-result purchase limit while preserving acquired upgrades.
    /// </summary>
    public void BeginRun()
    {
        purchasedUpgradeAfterLastRun = false;
        lastPurchaseSummary = string.Empty;
    }

    /// <summary>
    /// Reports whether the player can buy the next Line Capacity upgrade between runs.
    /// </summary>
    public bool CanPurchaseLineCapacityUpgrade(
        RunRewardRuntime rewards,
        bool runActive,
        bool hasCompletedRun,
        out string restrictionReason)
    {
        restrictionReason = string.Empty;

        if (runActive || !hasCompletedRun)
        {
            restrictionReason = "Available after completing a run";
            return false;
        }

        if (purchasedUpgradeAfterLastRun)
        {
            restrictionReason = "One upgrade may be purchased after each run";
            return false;
        }

        if (purchasedLineCapacityUpgrades >= maximumLineCapacityUpgrades)
        {
            restrictionReason = "Maximum Line Capacity upgrades reached";
            return false;
        }

        if (rewards == null || rewards.TotalGold < lineCapacityUpgradeCost)
        {
            restrictionReason = $"Requires {lineCapacityUpgradeCost} Gold";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Purchases one Line Capacity upgrade and removes its cost from the Gold wallet.
    /// </summary>
    public bool TryPurchaseLineCapacityUpgrade(
        RunRewardRuntime rewards,
        bool runActive,
        bool hasCompletedRun,
        out string resultSummary)
    {
        if (!CanPurchaseLineCapacityUpgrade(rewards, runActive, hasCompletedRun, out resultSummary))
        {
            return false;
        }

        if (!rewards.TrySpendGold(lineCapacityUpgradeCost))
        {
            resultSummary = "Gold could not be spent";
            return false;
        }

        purchasedLineCapacityUpgrades++;
        purchasedUpgradeAfterLastRun = true;
        lastPurchaseSummary = $"Line Capacity increased by {lineCapacityPerUpgrade}. "
            + $"Future runs receive +{CurrentLineCapacityBonus} total capacity.";
        resultSummary = lastPurchaseSummary;
        return true;
    }

    /// <summary>
    /// Reports whether the configured Technique card can be unlocked between runs.
    /// </summary>
    public bool CanUnlockTechniqueCard(
        RunRewardRuntime rewards,
        bool runActive,
        bool hasCompletedRun,
        out string restrictionReason)
    {
        restrictionReason = string.Empty;

        if (runActive || !hasCompletedRun)
        {
            restrictionReason = "Available after completing a run";
            return false;
        }

        if (unlockableTechniqueCard == null || unlockableTechniqueCard.CardType != CardType.Technique)
        {
            restrictionReason = "No valid Technique unlock is configured";
            return false;
        }

        if (techniqueCardUnlocked || ContainsCard(savedTechniqueDeck, unlockableTechniqueCard))
        {
            restrictionReason = "Technique already unlocked";
            return false;
        }

        if (rewards == null || rewards.TotalGold < techniqueUnlockCost)
        {
            restrictionReason = $"Requires {techniqueUnlockCost} Gold";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Spends Gold to unlock and append the configured Technique card to future run decks.
    /// </summary>
    public bool TryUnlockTechniqueCard(
        RunRewardRuntime rewards,
        bool runActive,
        bool hasCompletedRun,
        out string resultSummary)
    {
        if (!CanUnlockTechniqueCard(rewards, runActive, hasCompletedRun, out resultSummary))
        {
            return false;
        }

        if (!rewards.TrySpendGold(techniqueUnlockCost))
        {
            resultSummary = "Gold could not be spent";
            return false;
        }

        savedTechniqueDeck = AppendCard(savedTechniqueDeck, unlockableTechniqueCard);
        techniqueCardUnlocked = true;
        resultSummary = $"{unlockableTechniqueCard.DisplayName} unlocked. "
            + $"Future runs use {savedTechniqueDeck.Length} Technique cards.";
        return true;
    }

    /// <summary>
    /// Checks whether a deck already contains the requested card definition or stable card ID.
    /// </summary>
    private static bool ContainsCard(CardDefinition[] deck, CardDefinition requestedCard)
    {
        if (requestedCard == null || deck == null)
        {
            return false;
        }

        for (int i = 0; i < deck.Length; i++)
        {
            CardDefinition card = deck[i];

            if (card == requestedCard
                || (card != null
                    && !string.IsNullOrWhiteSpace(card.UniqueId)
                    && string.Equals(card.UniqueId, requestedCard.UniqueId, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns a new saved deck with one unlocked Technique card appended.
    /// </summary>
    private static CardDefinition[] AppendCard(CardDefinition[] deck, CardDefinition card)
    {
        CardDefinition[] source = deck ?? Array.Empty<CardDefinition>();
        CardDefinition[] result = new CardDefinition[source.Length + 1];

        for (int i = 0; i < source.Length; i++)
        {
            result[i] = source[i];
        }

        result[result.Length - 1] = card;
        return result;
    }
}
