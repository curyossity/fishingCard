using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "New Catch Chain Scenario",
    menuName = "Fishing Cards/Debug/Catch Chain Scenario")]
public sealed class CatchChainScenarioDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string scenarioName;
    [TextArea(2, 5)]
    [SerializeField] private string decisionToObserve;

    [Header("Run Setup")]
    [Min(0)]
    [SerializeField] private int lineCapacity = 10;
    [Min(0)]
    [SerializeField] private int depth;
    [SerializeField] private CardDefinition[] startingCatches = Array.Empty<CardDefinition>();
    [SerializeField] private CardDefinition currentEncounter;

    public string ScenarioName => scenarioName;
    public string DecisionToObserve => decisionToObserve;
    public int LineCapacity => lineCapacity;
    public int Depth => depth;
    public CardDefinition[] StartingCatches => startingCatches;
    public CardDefinition CurrentEncounter => currentEncounter;
}
