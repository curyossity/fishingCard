using System;
using UnityEngine;

public enum LineLoadRiskOutcome
{
    NotOverloaded,
    Held,
    CatchLost
}

[Serializable]
public sealed class LineLoadRiskRuntime
{
    [Header("Risk Tuning")]
    [Range(0f, 1f)]
    [SerializeField] private float baseBreakChance = 0.10f;
    [Range(0f, 1f)]
    [SerializeField] private float breakChancePerExcessLoad = 0.08f;
    [Range(0f, 1f)]
    [SerializeField] private float maximumBreakChance = 0.65f;

    [Header("Last Check")]
    [SerializeField] private LineLoadRiskOutcome lastOutcome;
    [SerializeField] private int lastExcessLoad;
    [SerializeField] private float lastBreakChance;
    [SerializeField] private float lastRoll;
    [SerializeField] private int lastSelectedCatchIndex = -1;

    public LineLoadRiskOutcome LastOutcome => lastOutcome;
    public int LastExcessLoad => lastExcessLoad;
    public float LastBreakChance => lastBreakChance;
    public float LastRoll => lastRoll;
    public int LastSelectedCatchIndex => lastSelectedCatchIndex;

    /// <summary>
    /// Evaluates overload strain and returns the Catch Chain index that breaks free, or -1 when the line holds.
    /// </summary>
    public int Evaluate(int currentLoad, int capacity, int catchCount, System.Random random)
    {
        lastExcessLoad = Mathf.Max(0, currentLoad - capacity);
        lastSelectedCatchIndex = -1;

        if (lastExcessLoad == 0 || catchCount <= 0)
        {
            lastOutcome = LineLoadRiskOutcome.NotOverloaded;
            lastBreakChance = 0f;
            lastRoll = 0f;
            return -1;
        }

        float configuredMaximum = Mathf.Clamp01(maximumBreakChance);
        float calculatedChance = baseBreakChance + breakChancePerExcessLoad * lastExcessLoad;
        lastBreakChance = Mathf.Clamp(calculatedChance, 0f, configuredMaximum);
        lastRoll = (float)random.NextDouble();

        if (lastRoll >= lastBreakChance)
        {
            lastOutcome = LineLoadRiskOutcome.Held;
            return -1;
        }

        lastOutcome = LineLoadRiskOutcome.CatchLost;
        lastSelectedCatchIndex = random.Next(catchCount);
        return lastSelectedCatchIndex;
    }

    /// <summary>
    /// Clears the previous overload check while preserving Inspector-authored tuning.
    /// </summary>
    public void Reset()
    {
        lastOutcome = LineLoadRiskOutcome.NotOverloaded;
        lastExcessLoad = 0;
        lastBreakChance = 0f;
        lastRoll = 0f;
        lastSelectedCatchIndex = -1;
    }
}
