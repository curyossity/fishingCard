using System;
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

    public int LineCapacityUpgradeCost => lineCapacityUpgradeCost;
    public int LineCapacityPerUpgrade => lineCapacityPerUpgrade;
    public int MaximumLineCapacityUpgrades => maximumLineCapacityUpgrades;
    public int PurchasedLineCapacityUpgrades => purchasedLineCapacityUpgrades;
    public int CurrentLineCapacityBonus => purchasedLineCapacityUpgrades * lineCapacityPerUpgrade;
    public string LastPurchaseSummary => lastPurchaseSummary;

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
}
