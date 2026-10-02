using System;
using UnityEngine;

public enum LineLoadRiskOutcome
{
    NotOverloaded,
    Held,
    LineBroken
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

    public LineLoadRiskOutcome LastOutcome => lastOutcome;
    public int LastExcessLoad => lastExcessLoad;
    public float LastBreakChance => lastBreakChance;
    public float LastRoll => lastRoll;

    /// <summary>
    /// Evaluates overload strain and reports whether the line breaks.
    /// </summary>
    public bool Evaluate(int currentLoad, int capacity, int catchCount, System.Random random)
    {
        lastExcessLoad = Mathf.Max(0, currentLoad - capacity);

        if (lastExcessLoad == 0 || catchCount <= 0)
        {
            lastOutcome = LineLoadRiskOutcome.NotOverloaded;
            lastBreakChance = 0f;
            lastRoll = 0f;
            return false;
        }

        float configuredMaximum = Mathf.Clamp01(maximumBreakChance);
        float calculatedChance = baseBreakChance + breakChancePerExcessLoad * lastExcessLoad;
        lastBreakChance = Mathf.Clamp(calculatedChance, 0f, configuredMaximum);
        lastRoll = (float)random.NextDouble();

        if (lastRoll >= lastBreakChance)
        {
            lastOutcome = LineLoadRiskOutcome.Held;
            return false;
        }

        lastOutcome = LineLoadRiskOutcome.LineBroken;
        return true;
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
    }
}
