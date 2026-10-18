using System;
using UnityEngine;

/// <summary>Optional result skin; reward, progression, purchase legality and next-run state remain controller-owned.</summary>
public abstract class RunResultPresentation : MonoBehaviour
{
    /// <summary>Displays existing result/progression data and forwards legal commands.</summary>
    public abstract void Refresh(bool runActive, FishingRunResult result, int totalGold, RunProgressionRuntime progression,
        bool canPurchaseUpgrade, string upgradeReason, bool canUnlockTechnique, string techniqueReason,
        Func<bool> purchaseUpgrade, Func<bool> unlockTechnique, Action startNextRun);
    /// <summary>Clears result visibility before the first completed run.</summary>
    public abstract void Hide();
}
