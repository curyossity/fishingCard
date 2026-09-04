using System;
using UnityEngine;

[Serializable]
public sealed class RunRewardRuntime
{
    [Min(0)]
    [SerializeField] private int goldPerHaulValue = 1;
    [SerializeField] private int totalGold;
    [SerializeField] private int lastGoldAwarded;

    public int GoldPerHaulValue => goldPerHaulValue;
    public int TotalGold => totalGold;
    public int LastGoldAwarded => lastGoldAwarded;

    /// <summary>
    /// Converts a completed haul's resolved value into Gold and adds it to the current wallet.
    /// </summary>
    public int AwardGold(FishingRunResult result)
    {
        if (result == null || !result.HasResult)
        {
            lastGoldAwarded = 0;
            return 0;
        }

        lastGoldAwarded = Mathf.Max(0, result.HaulValue * goldPerHaulValue);
        totalGold += lastGoldAwarded;
        result.RecordGoldAward(lastGoldAwarded);
        return lastGoldAwarded;
    }

    /// <summary>
    /// Clears the previous run's award while preserving accumulated Gold.
    /// </summary>
    public void BeginRun()
    {
        lastGoldAwarded = 0;
    }
}
