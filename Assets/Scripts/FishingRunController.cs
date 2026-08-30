using System;
using System.Text;
using UnityEngine;

public sealed class FishingRunController : MonoBehaviour
{
    [Header("Start Run Settings")]
    [SerializeField] private bool startRunOnAwake = true;
    [Min(0)]
    [SerializeField] private int startingLineCapacity = 10;
    [Min(0)]
    [SerializeField] private int startingDepth;
    [SerializeField] private string startingBiomeId = "Coastal";
    [Min(1)]
    [SerializeField] private int startingHandSize = 4;
    [Min(1)]
    [SerializeField] private int depthStepPerDescend = 1;
    [SerializeField] private int randomSeed;
    [SerializeField] private bool useRandomSeed = true;

    [Header("Starting Cards")]
    [SerializeField] private CardDefinition[] startingTechniqueDeck = Array.Empty<CardDefinition>();
    [SerializeField] private CardDefinition[] encounterPool = Array.Empty<CardDefinition>();

    [Header("Optional Views")]
    [SerializeField] private CardView currentEncounterView;
    [SerializeField] private CardView[] techniqueHandViews = Array.Empty<CardView>();
    [SerializeField] private CatchChainView catchChainView;

    [Header("Debug Actions")]
    [SerializeField] private int debugTechniqueHandIndex;
    [SerializeField] private int debugCatchChainIndex;

    [Header("Run State")]
    [SerializeField] private bool runActive;
    [SerializeField] private string currentBiomeId;
    [SerializeField] private int currentDepth;
    [SerializeField] private int lineCapacity;
    [SerializeField] private EncounterRuntime encounterRuntime = new EncounterRuntime();
    [SerializeField] private CatchChainRuntime catchChainRuntime = new CatchChainRuntime();
    [SerializeField] private TechniqueDeckRuntime techniqueDeckRuntime = new TechniqueDeckRuntime();

    [Header("Last Surface Result")]
    [SerializeField] private FishingRunResult lastSurfaceResult = new FishingRunResult();

    private System.Random random;

    public bool RunActive => runActive;
    public string CurrentBiomeId => currentBiomeId;
    public int CurrentDepth => currentDepth;
    public int LineCapacity => lineCapacity;
    public CardDefinition CurrentEncounter => encounterRuntime.CurrentEncounter;
    public EncounterState CurrentEncounterState => encounterRuntime.CurrentState;
    public CardDefinition HookedEncounter => encounterRuntime.HookedEncounter;
    public HookedEffectRecord[] HookedEffectRecords => encounterRuntime.HookedEffectRecords;
    public ActiveCatchEffectRecord[] ActiveCatchEffectRecords => catchChainRuntime.ActiveEffectRecords;
    public CardDefinition[] CatchChain => catchChainRuntime.Cards;
    public CardDefinition[] TechniqueHand => techniqueDeckRuntime.Hand;
    public CardDefinition[] TechniqueDrawPile => techniqueDeckRuntime.DrawPile;
    public CardDefinition[] TechniqueDiscardPile => techniqueDeckRuntime.DiscardPile;
    public CardDefinition[] LastHaul => lastSurfaceResult.Haul;
    public int LastHaulValue => lastSurfaceResult.HaulValue;
    public int LastSurfaceDepth => lastSurfaceResult.SurfaceDepth;
    public int LastSurfaceLineLoad => lastSurfaceResult.SurfaceLineLoad;
    public bool LastSurfaceWasOverloaded => lastSurfaceResult.WasOverloaded;
    public int CurrentLineLoad => catchChainRuntime.CurrentLineLoad;

    /// <summary>
    /// Initializes runtime owners and starts the run automatically when configured.
    /// </summary>
    private void Awake()
    {
        EnsureRuntimeObjects();

        if (startRunOnAwake)
        {
            StartRun();
        }
    }

    /// <summary>
    /// Initializes capacity, runtime systems, biome, depth, and the first encounter.
    /// </summary>
    [ContextMenu("Run/Start Run")]
    public void StartRun()
    {
        EnsureRuntimeObjects();

        int seed = useRandomSeed ? Environment.TickCount : randomSeed;
        random = new System.Random(seed);

        runActive = true;
        lineCapacity = startingLineCapacity;
        currentBiomeId = startingBiomeId;
        currentDepth = Mathf.Max(0, startingDepth);

        encounterRuntime.Reset();
        catchChainRuntime.Reset();
        techniqueDeckRuntime.Initialize(startingTechniqueDeck, startingHandSize, random, LogRuntimeWarning);
        lastSurfaceResult.Reset();

        RevealEncounterAtCurrentDepth();
        RefreshViews();
        Debug.Log(BuildStartRunSummary(seed), this);
    }

    /// <summary>
    /// Applies a Technique card from the current hand to the Hooked reaction window.
    /// </summary>
    public bool TryUseTechniqueCard(int handIndex)
    {
        if (!runActive)
        {
            Debug.LogWarning("Cannot use a Technique card before a run has started.", this);
            return false;
        }

        if (encounterRuntime.HookedEncounter == null)
        {
            Debug.LogWarning("There is no Hooked encounter for a Technique card to affect.", this);
            return false;
        }

        if (handIndex < 0 || handIndex >= techniqueDeckRuntime.Hand.Length)
        {
            Debug.LogWarning($"Technique hand index is out of range: {handIndex}.", this);
            return false;
        }

        CardDefinition techniqueCard = techniqueDeckRuntime.Hand[handIndex];

        if (techniqueCard == null)
        {
            Debug.LogWarning($"Technique hand slot {handIndex} is empty.", this);
            return false;
        }

        if (techniqueCard.CardType != CardType.Technique)
        {
            Debug.LogWarning($"Card is not a Technique card: {techniqueCard.DisplayName}.", this);
            return false;
        }

        int addedEffects = encounterRuntime.AddTechniqueEffects(techniqueCard);
        Debug.Log($"Technique card used on Hooked encounter: {techniqueCard.DisplayName}. Tracked effects added: {addedEffects}.", this);
        return true;
    }

    /// <summary>
    /// Resolves Descend by committing a Hooked catch, moving deeper, and revealing the next encounter.
    /// </summary>
    public bool TryDescend()
    {
        if (!runActive)
        {
            Debug.LogWarning("Cannot Descend before a run has started.", this);
            return false;
        }

        CardDefinition caughtCard = encounterRuntime.TakeHookedEncounter();

        if (caughtCard != null)
        {
            catchChainRuntime.Add(caughtCard);
        }

        // The next reveal uses the new depth so data-driven depth ranges take effect immediately.
        currentDepth += Mathf.Max(1, depthStepPerDescend);
        techniqueDeckRuntime.Refill(startingHandSize);
        RevealEncounterAtCurrentDepth();

        RefreshViews();
        Debug.Log(BuildDescendSummary(caughtCard), this);
        return true;
    }

    /// <summary>
    /// Releases one caught card without advancing depth or resolving the Hooked encounter.
    /// </summary>
    public bool TryReleaseCatch(int catchIndex)
    {
        if (!runActive)
        {
            Debug.LogWarning("Cannot Release a catch before a run has started.", this);
            return false;
        }

        bool released = catchChainRuntime.TryRelease(
            catchIndex,
            out CardDefinition releasedCard,
            out int previousLineLoad,
            out string validationMessage);

        if (!released)
        {
            Debug.LogWarning(validationMessage, this);
            return false;
        }

        RefreshViews();
        Debug.Log(BuildReleaseSummary(releasedCard, previousLineLoad), this);
        return true;
    }

    /// <summary>
    /// Surfaces with the attached Catch Chain, records the haul, and ends the run.
    /// </summary>
    public bool TrySurface()
    {
        if (!runActive)
        {
            Debug.LogWarning("Cannot Surface before a run has started.", this);
            return false;
        }

        // The Hooked encounter is intentionally excluded because it has not entered the Catch Chain.
        lastSurfaceResult.Record(
            catchChainRuntime.Cards,
            currentDepth,
            catchChainRuntime.CurrentLineLoad,
            lineCapacity);

        string surfaceSummary = BuildSurfaceSummary();
        EndActiveRun();

        Debug.Log(surfaceSummary, this);
        return true;
    }

    /// <summary>
    /// Inspector context-menu wrapper for testing the Descend action.
    /// </summary>
    [ContextMenu("Run/Descend")]
    private void UseDebugDescend()
    {
        TryDescend();
    }

    /// <summary>
    /// Inspector context-menu wrapper for releasing a Catch Chain card by index.
    /// </summary>
    [ContextMenu("Run/Release Debug Catch")]
    private void UseDebugReleaseCatch()
    {
        TryReleaseCatch(debugCatchChainIndex);
    }

    /// <summary>
    /// Inspector context-menu wrapper for testing the Surface action.
    /// </summary>
    [ContextMenu("Run/Surface")]
    private void UseDebugSurface()
    {
        TrySurface();
    }

    /// <summary>
    /// Inspector context-menu wrapper for applying a Technique card by hand index.
    /// </summary>
    [ContextMenu("Run/Use Debug Technique Card")]
    private void UseDebugTechniqueCard()
    {
        TryUseTechniqueCard(debugTechniqueHandIndex);
    }

    /// <summary>
    /// Ensures serialized runtime owners exist when older scene data does not contain them yet.
    /// </summary>
    private void EnsureRuntimeObjects()
    {
        if (encounterRuntime == null)
        {
            encounterRuntime = new EncounterRuntime();
        }

        if (catchChainRuntime == null)
        {
            catchChainRuntime = new CatchChainRuntime();
        }

        if (techniqueDeckRuntime == null)
        {
            techniqueDeckRuntime = new TechniqueDeckRuntime();
        }

        if (lastSurfaceResult == null)
        {
            lastSurfaceResult = new FishingRunResult();
        }
    }

    /// <summary>
    /// Reveals an encounter for the current biome and depth, reporting an empty candidate pool.
    /// </summary>
    private void RevealEncounterAtCurrentDepth()
    {
        bool revealed = encounterRuntime.Reveal(encounterPool, currentBiomeId, currentDepth, random);

        if (!revealed)
        {
            Debug.LogWarning($"No valid encounter found for biome '{currentBiomeId}' at depth {currentDepth}.", this);
        }
    }

    /// <summary>
    /// Clears state that exists only while a fishing run is active.
    /// </summary>
    private void EndActiveRun()
    {
        runActive = false;
        encounterRuntime.Reset();
        catchChainRuntime.Reset();
        techniqueDeckRuntime.Reset();
        RefreshViews();
    }

    /// <summary>
    /// Updates optional card views from the current runtime owners.
    /// </summary>
    private void RefreshViews()
    {
        if (currentEncounterView != null)
        {
            currentEncounterView.SetCard(encounterRuntime.CurrentEncounter, encounterRuntime.CurrentState);
        }

        if (catchChainView != null)
        {
            catchChainView.Refresh(catchChainRuntime.Cards, catchChainRuntime.ActiveEffectRecords);
        }

        if (techniqueHandViews == null)
        {
            return;
        }

        for (int i = 0; i < techniqueHandViews.Length; i++)
        {
            if (techniqueHandViews[i] == null)
            {
                continue;
            }

            CardDefinition card = i < techniqueDeckRuntime.Hand.Length ? techniqueDeckRuntime.Hand[i] : null;
            techniqueHandViews[i].SetCard(card);
        }
    }

    /// <summary>
    /// Reports warnings raised by plain runtime classes through this Unity component.
    /// </summary>
    private void LogRuntimeWarning(string message)
    {
        Debug.LogWarning(message, this);
    }

    /// <summary>
    /// Builds the startup log from the initialized runtime systems.
    /// </summary>
    private string BuildStartRunSummary(int seed)
    {
        StringBuilder summary = new StringBuilder();
        summary.AppendLine("Run started.");
        summary.AppendLine($"Seed: {seed}");
        summary.AppendLine($"Biome: {currentBiomeId}");
        summary.AppendLine($"Depth: {currentDepth}");
        summary.AppendLine($"Line Capacity: {lineCapacity}");
        summary.AppendLine($"Catch Chain: {catchChainRuntime.Cards.Length} cards");
        summary.AppendLine($"Technique Hand: {techniqueDeckRuntime.Hand.Length} cards");
        summary.AppendLine($"Technique Draw Pile: {techniqueDeckRuntime.DrawPile.Length} cards");
        summary.Append("First Encounter: ");
        summary.Append(encounterRuntime.CurrentEncounter == null ? "none" : encounterRuntime.CurrentEncounter.DisplayName);
        summary.AppendLine();
        summary.Append("Encounter State: ");
        summary.Append(encounterRuntime.CurrentState);
        return summary.ToString();
    }

    /// <summary>
    /// Builds the Descend log from catch, depth, load, and encounter state.
    /// </summary>
    private string BuildDescendSummary(CardDefinition caughtCard)
    {
        StringBuilder summary = new StringBuilder();
        summary.AppendLine("Descend resolved.");
        summary.Append("Caught: ");
        summary.Append(caughtCard == null ? "none" : caughtCard.DisplayName);
        summary.AppendLine();
        summary.AppendLine($"Depth: {currentDepth}");
        summary.AppendLine($"Line Load: {catchChainRuntime.CurrentLineLoad} / {lineCapacity}");
        summary.AppendLine($"Catch Chain: {catchChainRuntime.Cards.Length} cards");
        summary.AppendLine($"Active Catch Effects: {catchChainRuntime.ActiveEffectRecords.Length}");
        summary.AppendLine($"Technique Hand: {techniqueDeckRuntime.Hand.Length} cards");
        summary.Append("Next Encounter: ");
        summary.Append(encounterRuntime.CurrentEncounter == null ? "none" : encounterRuntime.CurrentEncounter.DisplayName);
        summary.AppendLine();
        summary.Append("Encounter State: ");
        summary.Append(encounterRuntime.CurrentState);
        return summary.ToString();
    }

    /// <summary>
    /// Builds a compact Release log showing the lost card and immediate load change.
    /// </summary>
    private string BuildReleaseSummary(CardDefinition releasedCard, int previousLineLoad)
    {
        string encounterName = encounterRuntime.CurrentEncounter == null
            ? "none"
            : encounterRuntime.CurrentEncounter.DisplayName;

        return $"Release resolved | Released: {releasedCard.DisplayName} | Lost Value: {releasedCard.Value} | "
            + $"Line Load: {previousLineLoad} -> {catchChainRuntime.CurrentLineLoad} / {lineCapacity} | Depth: {currentDepth} | "
            + $"Current Encounter: {encounterName} ({encounterRuntime.CurrentState})";
    }

    /// <summary>
    /// Builds the end-of-run summary from the stored Surface result.
    /// </summary>
    private string BuildSurfaceSummary()
    {
        string loadStatus = lastSurfaceResult.WasOverloaded ? "Overloaded" : "Within Capacity";
        StringBuilder summary = new StringBuilder();

        // Unity shows this complete first line even while the Console entry is collapsed.
        summary.AppendLine($"Surface resolved | Haul: {lastSurfaceResult.Haul.Length} cards | "
            + $"Value: {lastSurfaceResult.HaulValue} | Load: {lastSurfaceResult.SurfaceLineLoad} / "
            + $"{lastSurfaceResult.LineCapacity} | Depth: {lastSurfaceResult.SurfaceDepth} | {loadStatus}");
        summary.Append("Successful Haul: ");

        if (lastSurfaceResult.Haul.Length == 0)
        {
            summary.Append("none");
            return summary.ToString();
        }

        for (int i = 0; i < lastSurfaceResult.Haul.Length; i++)
        {
            if (i > 0)
            {
                summary.Append(", ");
            }

            CardDefinition card = lastSurfaceResult.Haul[i];
            summary.Append(card == null ? "unknown card" : card.DisplayName);
        }

        return summary.ToString();
    }
}
