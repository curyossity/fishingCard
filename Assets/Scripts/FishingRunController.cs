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
    [SerializeField] private BiomeDefinition startingBiome;
    [Min(1)]
    [SerializeField] private int startingHandSize = 4;
    [Min(1)]
    [SerializeField] private int depthStepPerDescend = 1;
    [SerializeField] private int randomSeed;
    [SerializeField] private bool useRandomSeed = true;

    [Header("Starting Technique Cards")]
    [SerializeField] private CardDefinition[] startingTechniqueDeck = Array.Empty<CardDefinition>();

    [Header("Optional Views")]
    [SerializeField] private CardView currentEncounterView;
    [SerializeField] private CardView[] techniqueHandViews = Array.Empty<CardView>();
    [SerializeField] private TechniqueHandView techniqueHandView;
    [SerializeField] private CatchChainView catchChainView;
    [SerializeField] private RunResultView runResultView;

    [Header("Debug Actions")]
    [SerializeField] private int debugTechniqueHandIndex;
    [SerializeField] private int debugCatchChainIndex;
    [SerializeField] private CardDefinition debugCatchCard;
    [SerializeField] private CatchChainScenarioDefinition debugScenario;

    [Header("Run State")]
    [SerializeField] private bool runActive;
    [SerializeField] private BiomeDefinition currentBiome;
    [SerializeField] private int currentDepth;
    [SerializeField] private int lineCapacity;
    [SerializeField] private bool currentEncounterInformationHidden;
    [SerializeField] private EncounterRuntime encounterRuntime = new EncounterRuntime();
    [SerializeField] private BiomeApexRuntime biomeApexRuntime = new BiomeApexRuntime();
    [SerializeField] private CatchChainRuntime catchChainRuntime = new CatchChainRuntime();
    [SerializeField] private TechniqueDeckRuntime techniqueDeckRuntime = new TechniqueDeckRuntime();
    [SerializeField] private TechniqueEffectRuntime techniqueEffectRuntime = new TechniqueEffectRuntime();

    [Header("Line Load Risk")]
    [SerializeField] private LineLoadRiskRuntime lineLoadRiskRuntime = new LineLoadRiskRuntime();

    [Header("Last Surface Result")]
    [SerializeField] private FishingRunResult lastSurfaceResult = new FishingRunResult();

    [Header("Run Rewards")]
    [SerializeField] private RunRewardRuntime runRewardRuntime = new RunRewardRuntime();
    [SerializeField] private RunProgressionRuntime runProgressionRuntime = new RunProgressionRuntime();

    private System.Random random;
    private EffectResolver effectResolver;

    public bool RunActive => runActive;
    public BiomeDefinition CurrentBiome => currentBiome;
    public string CurrentBiomeId => currentBiome == null ? string.Empty : currentBiome.BiomeId;
    public BiomeDepthTierDefinition CurrentDepthTier => currentBiome?.GetDepthTier(currentDepth);
    public int CurrentDepth => currentDepth;
    public int LineCapacity => lineCapacity;
    public CardDefinition CurrentEncounter => encounterRuntime.CurrentEncounter;
    public EncounterState CurrentEncounterState => encounterRuntime.CurrentState;
    public CardDefinition HookedEncounter => encounterRuntime.HookedEncounter;
    public HookedEffectRecord[] HookedEffectRecords => encounterRuntime.HookedEffectRecords;
    public ActiveCatchEffectRecord[] ActiveCatchEffectRecords => catchChainRuntime.ActiveEffectRecords;
    public CardInstance[] CatchChain => catchChainRuntime.Catches;
    public CardDefinition[] TechniqueHand => techniqueDeckRuntime.Hand;
    public CardDefinition[] TechniqueDrawPile => techniqueDeckRuntime.DrawPile;
    public CardDefinition[] TechniqueDiscardPile => techniqueDeckRuntime.DiscardPile;
    public CardEffectDefinition[] PendingDescendTechniqueEffects => techniqueEffectRuntime.PendingDescendEffects;
    public CardEffectDefinition[] PendingEncounterTechniqueEffects => techniqueEffectRuntime.PendingEncounterEffects;
    public CardInstance[] LastHaul => lastSurfaceResult.Haul;
    public int LastHaulValue => lastSurfaceResult.HaulValue;
    public int LastSurfaceDepth => lastSurfaceResult.SurfaceDepth;
    public int LastSurfaceLineLoad => lastSurfaceResult.SurfaceLineLoad;
    public bool LastSurfaceWasOverloaded => lastSurfaceResult.WasOverloaded;
    public int CurrentLineLoad => catchChainRuntime.CurrentLineLoad;
    public bool CurrentEncounterInformationHidden => currentEncounterInformationHidden;
    public BiomeApexState CurrentBiomeApexState => biomeApexRuntime.State;
    public CardDefinition SelectedBiomeApex => biomeApexRuntime.SelectedApex;
    public bool NextWatersPresented => biomeApexRuntime.NextWatersPresented;
    public CardInstance[] LastReleasedCatches => lastSurfaceResult.ReleasedCatches;
    public CardInstance[] LastLostCatches => lastSurfaceResult.LostCatches;
    public int LastGoldAwarded => lastSurfaceResult.GoldAwarded;
    public int TotalGold => runRewardRuntime.TotalGold;
    public int ProgressionLineCapacityBonus => runProgressionRuntime.CurrentLineCapacityBonus;

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
        lineCapacity = Mathf.Max(0, startingLineCapacity + runProgressionRuntime.CurrentLineCapacityBonus);
        currentBiome = startingBiome;
        currentDepth = Mathf.Max(0, startingDepth);

        encounterRuntime.Reset();
        biomeApexRuntime.Reset();
        catchChainRuntime.Reset();
        techniqueDeckRuntime.Initialize(startingTechniqueDeck, startingHandSize, random, LogRuntimeWarning);
        techniqueEffectRuntime.Reset();
        lineLoadRiskRuntime.Reset();
        lastSurfaceResult.Reset();
        runRewardRuntime.BeginRun();
        runProgressionRuntime.BeginRun();

        RevealEncounterAtCurrentDepth();
        RefreshViews();
        Debug.Log(BuildStartRunSummary(seed), this);
    }

    /// <summary>
    /// Applies a Technique card from the current hand to the Hooked reaction window.
    /// </summary>
    public bool TryUseTechniqueCard(int handIndex)
    {
        if (!CanUseTechniqueCard(handIndex, out string restrictionReason))
        {
            Debug.LogWarning(restrictionReason, this);
            return false;
        }

        techniqueDeckRuntime.TryGetHandCard(handIndex, out CardDefinition techniqueCard, out _);
        CardDefinition encounterBeforeTechnique = encounterRuntime.CurrentEncounter;
        int encounterSelectionDepth = GetEffectiveEncounterSelectionDepth();
        CardDefinition[] currentEncounterPool = GetTechniqueEncounterPool(encounterSelectionDepth);
        bool consumed = techniqueDeckRuntime.TryUseCard(
            handIndex,
            startingHandSize,
            random,
            out _,
            out string deckValidationMessage);

        if (!consumed)
        {
            Debug.LogWarning(deckValidationMessage, this);
            return false;
        }

        bool applied = techniqueEffectRuntime.ApplyCard(
            techniqueCard,
            encounterRuntime,
            catchChainRuntime,
            effectResolver,
            currentEncounterPool,
            currentBiome?.EncounterChains,
            CurrentBiomeId,
            currentDepth,
            random,
            currentEncounterInformationHidden,
            out string effectSummary);

        if (!applied)
        {
            Debug.LogWarning($"Technique card was consumed but its validated effect did not resolve: {techniqueCard.DisplayName}.", this);
            return false;
        }

        biomeApexRuntime.RecordTechniqueResolution(encounterBeforeTechnique, encounterRuntime.CurrentEncounter);

        RefreshViews();
        Debug.Log($"Technique card used: {techniqueCard.DisplayName} | {effectSummary}. "
            + $"Draw: {techniqueDeckRuntime.DrawPile.Length}. "
            + $"Discard: {techniqueDeckRuntime.DiscardPile.Length}.", this);
        return true;
    }

    /// <summary>
    /// Reports whether a Technique hand slot can affect the current Hooked encounter.
    /// </summary>
    public bool CanUseTechniqueCard(int handIndex, out string restrictionReason)
    {
        restrictionReason = string.Empty;

        if (!runActive)
        {
            restrictionReason = "Run inactive";
            return false;
        }

        if (!techniqueDeckRuntime.TryGetHandCard(handIndex, out CardDefinition techniqueCard, out restrictionReason))
        {
            return false;
        }

        if (encounterRuntime.HookedEncounter == null)
        {
            restrictionReason = "No Hooked encounter";
            return false;
        }

        int encounterSelectionDepth = GetEffectiveEncounterSelectionDepth();
        return techniqueEffectRuntime.CanUseCard(
            techniqueCard,
            encounterRuntime,
            catchChainRuntime,
            currentEncounterInformationHidden,
            GetTechniqueEncounterPool(encounterSelectionDepth),
            CurrentBiomeId,
            currentDepth,
            out restrictionReason);
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

        if (biomeApexRuntime.NextWatersPresented)
        {
            Debug.LogWarning("No further biome content is available. Release catches or Surface with the haul.", this);
            return false;
        }

        CardDefinition caughtCard = encounterRuntime.TakeHookedEncounter();
        CardInstance committedCatch = null;

        if (caughtCard != null)
        {
            committedCatch = catchChainRuntime.Add(caughtCard, effectResolver);
            biomeApexRuntime.RecordCommittedApex(caughtCard);
        }

        TechniqueDescendResolution techniqueResolution = techniqueEffectRuntime.ResolveNextDescend(
            committedCatch,
            catchChainRuntime,
            effectResolver,
            lineCapacity);
        int effectiveCapacity = Mathf.Max(0, lineCapacity + techniqueResolution.CapacityBonus);
        CardInstance strainReleasedCatch = ResolveOverloadRisk(effectiveCapacity);

        // The next reveal uses the new depth so data-driven depth ranges take effect immediately.
        currentDepth += Mathf.Max(1, depthStepPerDescend + techniqueResolution.AdditionalDepth);
        techniqueDeckRuntime.Refill(startingHandSize, random);
        RevealEncounterAtCurrentDepth();

        RefreshViews();
        Debug.Log(BuildDescendSummary(caughtCard, strainReleasedCatch, techniqueResolution, effectiveCapacity), this);
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
            effectResolver,
            out CardInstance releasedCatch,
            out int previousLineLoad,
            out string validationMessage);

        if (!released)
        {
            Debug.LogWarning(validationMessage, this);
            return false;
        }

        RefreshViews();
        Debug.Log(BuildReleaseSummary(releasedCatch, previousLineLoad), this);
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

        int surfaceStartingLoad = catchChainRuntime.CurrentLineLoad;
        CardInstance strainReleasedCatch = ResolveOverloadRisk(lineCapacity);

        // The Hooked encounter is intentionally excluded because it has not entered the Catch Chain.
        lastSurfaceResult.Record(
            catchChainRuntime.Catches,
            catchChainRuntime.ReleasedCatches,
            catchChainRuntime.LostCatches,
            currentDepth,
            surfaceStartingLoad,
            lineCapacity);
        runRewardRuntime.AwardGold(lastSurfaceResult);

        string surfaceSummary = BuildSurfaceSummary(strainReleasedCatch);
        EndActiveRun();

        Debug.Log(surfaceSummary, this);
        return true;
    }

    /// <summary>
    /// Purchases the between-run Line Capacity upgrade when the completed run and wallet allow it.
    /// </summary>
    public bool TryPurchaseLineCapacityUpgrade()
    {
        bool purchased = runProgressionRuntime.TryPurchaseLineCapacityUpgrade(
            runRewardRuntime,
            runActive,
            lastSurfaceResult.HasResult,
            out string resultSummary);

        if (!purchased)
        {
            Debug.LogWarning(resultSummary, this);
            return false;
        }

        RefreshViews();
        Debug.Log($"PROGRESSION PURCHASED | {resultSummary} | Gold Remaining: {runRewardRuntime.TotalGold}", this);
        return true;
    }

    /// <summary>
    /// Replaces the current run setup with a repeatable Catch Chain decision scenario.
    /// </summary>
    public bool LoadDebugScenario(CatchChainScenarioDefinition scenario)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Enter Play Mode before loading a debug scenario.", this);
            return false;
        }

        if (scenario == null)
        {
            Debug.LogWarning("Assign a debug scenario before loading it.", this);
            return false;
        }

        if (!runActive)
        {
            StartRun();
        }

        lineCapacity = Mathf.Max(0, scenario.LineCapacity);
        currentDepth = Mathf.Max(0, scenario.Depth);
        encounterRuntime.Reset();
        biomeApexRuntime.Reset();
        catchChainRuntime.Reset();
        techniqueEffectRuntime.Reset();

        CardDefinition[] scenarioCatches = scenario.StartingCatches ?? Array.Empty<CardDefinition>();

        for (int i = 0; i < scenarioCatches.Length; i++)
        {
            catchChainRuntime.Add(scenarioCatches[i], effectResolver);
        }

        encounterRuntime.SetCurrentEncounter(scenario.CurrentEncounter);
        lineLoadRiskRuntime.Reset();
        RefreshViews();
        Debug.Log(BuildDebugScenarioSummary(scenario), this);
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
    /// Inspector context-menu wrapper for adding a specific card through the real Catch Chain runtime.
    /// </summary>
    [ContextMenu("Run/Add Debug Catch")]
    private void AddDebugCatch()
    {
        if (!runActive)
        {
            Debug.LogWarning("Cannot add a debug catch before a run has started.", this);
            return;
        }

        if (debugCatchCard == null)
        {
            Debug.LogWarning("Assign Debug Catch Card before adding a debug catch.", this);
            return;
        }

        catchChainRuntime.Add(debugCatchCard, effectResolver);
        RefreshViews();
        Debug.Log($"Debug catch added: {debugCatchCard.DisplayName}. Line Load: {CurrentLineLoad} / {lineCapacity}.", this);
    }

    /// <summary>
    /// Inspector context-menu wrapper for loading the configured decision scenario.
    /// </summary>
    [ContextMenu("Run/Load Debug Scenario")]
    private void LoadConfiguredDebugScenario()
    {
        LoadDebugScenario(debugScenario);
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

        if (biomeApexRuntime == null)
        {
            biomeApexRuntime = new BiomeApexRuntime();
        }

        if (catchChainRuntime == null)
        {
            catchChainRuntime = new CatchChainRuntime();
        }

        if (techniqueDeckRuntime == null)
        {
            techniqueDeckRuntime = new TechniqueDeckRuntime();
        }

        if (techniqueEffectRuntime == null)
        {
            techniqueEffectRuntime = new TechniqueEffectRuntime();
        }

        if (lastSurfaceResult == null)
        {
            lastSurfaceResult = new FishingRunResult();
        }

        if (lineLoadRiskRuntime == null)
        {
            lineLoadRiskRuntime = new LineLoadRiskRuntime();
        }

        if (runRewardRuntime == null)
        {
            runRewardRuntime = new RunRewardRuntime();
        }

        if (runProgressionRuntime == null)
        {
            runProgressionRuntime = new RunProgressionRuntime();
        }

        if (effectResolver == null)
        {
            effectResolver = new EffectResolver();
        }
    }

    /// <summary>
    /// Reveals an encounter for the current biome and depth, reporting an empty candidate pool.
    /// </summary>
    private void RevealEncounterAtCurrentDepth()
    {
        int selectionDepth = GetEffectiveEncounterSelectionDepth();

        if (TryRevealBiomeApex(selectionDepth))
        {
            techniqueEffectRuntime.CompleteEncounterReveal();
            return;
        }

        if (TryPresentNextWaters())
        {
            techniqueEffectRuntime.CompleteEncounterReveal();
            return;
        }

        if (biomeApexRuntime.HasReachedBoundary
            && selectionDepth >= biomeApexRuntime.BoundaryDepth)
        {
            encounterRuntime.SetCurrentEncounter(null);
            techniqueEffectRuntime.CompleteEncounterReveal();

            if (biomeApexRuntime.State == BiomeApexState.Unavailable)
            {
                Debug.LogWarning(biomeApexRuntime.LastSelectionSummary, this);
            }

            return;
        }

        bool revealed = encounterRuntime.Reveal(
            GetEncounterPool(selectionDepth),
            CurrentBiomeId,
            selectionDepth,
            random,
            catchChainRuntime.ActiveEffectRecords,
            effectResolver,
            techniqueEffectRuntime.PendingEncounterEffects,
            null,
            currentBiome?.EncounterChains);

        techniqueEffectRuntime.CompleteEncounterReveal();

        if (!revealed)
        {
            Debug.LogWarning($"No valid encounter found for biome '{CurrentBiomeId}' at depth {currentDepth}.", this);
        }
    }

    /// <summary>
    /// Selects and reveals the run's one biome Apex when the depth boundary is crossed.
    /// </summary>
    private bool TryRevealBiomeApex(int selectionDepth)
    {
        if (!biomeApexRuntime.TrySelectApex(currentBiome, selectionDepth, random, out CardDefinition apex))
        {
            return false;
        }

        if (!encounterRuntime.RevealApex(apex))
        {
            Debug.LogWarning($"Could not reveal selected biome Apex: {apex.DisplayName}.", this);
            return false;
        }

        Debug.Log($"BIOME APEX REVEALED | {biomeApexRuntime.LastSelectionSummary}", this);
        return true;
    }

    /// <summary>
    /// Presents the biome-authored next-waters card after the selected Apex is caught or avoided.
    /// </summary>
    private bool TryPresentNextWaters()
    {
        if (!biomeApexRuntime.CanPresentNextWaters)
        {
            return false;
        }

        CardDefinition nextWatersEncounter = currentBiome?.NextWatersEncounter;

        if (nextWatersEncounter == null)
        {
            Debug.LogWarning($"Biome '{CurrentBiomeId}' has no next-waters encounter configured.", this);
            return false;
        }

        encounterRuntime.SetCurrentEncounter(nextWatersEncounter);
        biomeApexRuntime.RecordNextWatersPresented();

        // Run-owned systems stay intact so the haul and attached effects cross the boundary together.
        Debug.Log(
            $"NEXT WATERS | {nextWatersEncounter.DisplayName} | "
            + $"Catch Chain: {catchChainRuntime.Catches.Length} | "
            + $"Line Load: {catchChainRuntime.CurrentLineLoad}/{lineCapacity}",
            this);
        return true;
    }

    /// <summary>
    /// Calculates the depth used to choose the next tier and filter its encounter cards.
    /// </summary>
    private int GetEffectiveEncounterSelectionDepth()
    {
        return Mathf.Max(0, currentDepth + techniqueEffectRuntime.GetNextEncounterDepthOffset());
    }

    /// <summary>
    /// Returns the current biome's encounter subset for an effective selection depth.
    /// </summary>
    private CardDefinition[] GetEncounterPool(int selectionDepth)
    {
        return currentBiome == null
            ? Array.Empty<CardDefinition>()
            : currentBiome.GetEncounterPool(selectionDepth);
    }

    /// <summary>
    /// Returns Apex alternatives during its reaction window and the regular tier pool otherwise.
    /// </summary>
    private CardDefinition[] GetTechniqueEncounterPool(int selectionDepth)
    {
        if (currentBiome != null && biomeApexRuntime.IsCurrentApex(encounterRuntime.HookedEncounter))
        {
            return currentBiome.ApexEncounters ?? Array.Empty<CardDefinition>();
        }

        return GetEncounterPool(selectionDepth);
    }

    /// <summary>
    /// Clears state that exists only while a fishing run is active.
    /// </summary>
    private void EndActiveRun()
    {
        runActive = false;
        encounterRuntime.Reset();
        biomeApexRuntime.Reset();
        catchChainRuntime.Reset();
        techniqueDeckRuntime.Reset();
        techniqueEffectRuntime.Reset();
        RefreshViews();
    }

    /// <summary>
    /// Resolves the current overload risk and releases the randomly selected catch when the line breaks.
    /// </summary>
    private CardInstance ResolveOverloadRisk(int effectiveCapacity)
    {
        int releaseIndex = lineLoadRiskRuntime.Evaluate(
            catchChainRuntime.CurrentLineLoad,
            effectiveCapacity,
            catchChainRuntime.Catches.Length,
            random);

        if (releaseIndex < 0)
        {
            return null;
        }

        bool released = catchChainRuntime.TryRelease(
            releaseIndex,
            effectResolver,
            out CardInstance releasedCatch,
            out _,
            out string validationMessage,
            CatchRemovalReason.LineStrain);

        if (!released)
        {
            Debug.LogWarning($"Overload strain could not release a catch: {validationMessage}", this);
            return null;
        }

        return releasedCatch;
    }

    /// <summary>
    /// Updates optional card views from the current runtime owners.
    /// </summary>
    private void RefreshViews()
    {
        currentEncounterInformationHidden = effectResolver != null
            && effectResolver.HidesEncounterInformation(catchChainRuntime.ActiveEffectRecords)
            && !techniqueEffectRuntime.RevealsCurrentEncounter;

        if (currentEncounterView != null)
        {
            currentEncounterView.SetCard(
                encounterRuntime.CurrentEncounter,
                encounterRuntime.CurrentState,
                CurrentEncounterInformationHidden);
        }

        if (catchChainView != null)
        {
            catchChainView.Refresh(
                catchChainRuntime.Catches,
                catchChainRuntime.ActiveEffectRecords,
                catchChainRuntime.CurrentLineLoad,
                lineCapacity);
        }

        if (techniqueHandView != null)
        {
            CardDefinition[] hand = techniqueDeckRuntime.Hand;
            bool[] playableSlots = new bool[hand.Length];
            string[] restrictionReasons = new string[hand.Length];

            for (int i = 0; i < hand.Length; i++)
            {
                playableSlots[i] = CanUseTechniqueCard(i, out restrictionReasons[i]);
            }

            techniqueHandView.Refresh(
                hand,
                playableSlots,
                restrictionReasons,
                techniqueDeckRuntime.DrawPile.Length,
                techniqueDeckRuntime.DiscardPile.Length,
                TryUseTechniqueCard);
        }

        if (runResultView != null)
        {
            bool canPurchaseUpgrade = runProgressionRuntime.CanPurchaseLineCapacityUpgrade(
                runRewardRuntime,
                runActive,
                lastSurfaceResult.HasResult,
                out string upgradeRestrictionReason);
            runResultView.Refresh(
                runActive,
                lastSurfaceResult,
                runRewardRuntime.TotalGold,
                runProgressionRuntime.CurrentLineCapacityBonus,
                runProgressionRuntime.LineCapacityUpgradeCost,
                runProgressionRuntime.LineCapacityPerUpgrade,
                runProgressionRuntime.PurchasedLineCapacityUpgrades,
                runProgressionRuntime.MaximumLineCapacityUpgrades,
                canPurchaseUpgrade,
                upgradeRestrictionReason,
                TryPurchaseLineCapacityUpgrade,
                StartRun);
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
        summary.AppendLine($"Biome: {(currentBiome == null ? "none" : currentBiome.DisplayName)}");
        summary.AppendLine($"Depth Tier: {(CurrentDepthTier == null ? "none" : CurrentDepthTier.DisplayName)}");
        summary.AppendLine($"Depth: {currentDepth}");
        summary.AppendLine($"Line Capacity: {lineCapacity}");
        summary.AppendLine($"Catch Chain: {catchChainRuntime.Catches.Length} cards");
        summary.AppendLine($"Technique Hand: {techniqueDeckRuntime.Hand.Length} cards");
        summary.AppendLine($"Technique Draw Pile: {techniqueDeckRuntime.DrawPile.Length} cards");
        summary.Append("First Encounter: ");
        summary.Append(encounterRuntime.CurrentEncounter == null ? "none" : encounterRuntime.CurrentEncounter.DisplayName);
        summary.AppendLine();
        summary.Append("Encounter State: ");
        summary.AppendLine(encounterRuntime.CurrentState.ToString());
        summary.AppendLine($"Biome Apex State: {biomeApexRuntime.State}");
        summary.Append("Recent Encounter Sequence: ");
        summary.Append(encounterRuntime.RecentEncounterSequenceSummary);
        return summary.ToString();
    }

    /// <summary>
    /// Builds a Console-friendly summary of a loaded decision scenario.
    /// </summary>
    private string BuildDebugScenarioSummary(CatchChainScenarioDefinition scenario)
    {
        string encounterName = scenario.CurrentEncounter == null
            ? "none"
            : scenario.CurrentEncounter.DisplayName;

        return $"Debug scenario loaded: {scenario.ScenarioName}\n"
            + $"Observe: {scenario.DecisionToObserve}\n"
            + $"Depth: {currentDepth} | Line Load: {CurrentLineLoad} / {lineCapacity} | "
            + $"Catches: {catchChainRuntime.Catches.Length} | Current Encounter: {encounterName}";
    }

    /// <summary>
    /// Builds the Descend log from catch, depth, load, and encounter state.
    /// </summary>
    private string BuildDescendSummary(
        CardDefinition caughtCard,
        CardInstance strainReleasedCatch,
        TechniqueDescendResolution techniqueResolution,
        int effectiveCapacity)
    {
        StringBuilder summary = new StringBuilder();
        summary.AppendLine("Descend resolved.");
        summary.Append("Caught: ");
        summary.Append(caughtCard == null ? "none" : caughtCard.DisplayName);
        summary.AppendLine();
        summary.AppendLine($"Depth: {currentDepth}");
        summary.AppendLine($"Depth Tier: {(CurrentDepthTier == null ? "none" : CurrentDepthTier.DisplayName)}");
        summary.AppendLine($"Biome Apex State: {biomeApexRuntime.State}");
        summary.AppendLine($"Line Load: {catchChainRuntime.CurrentLineLoad} / {lineCapacity}");
        summary.AppendLine($"Catch Chain: {catchChainRuntime.Catches.Length} cards");
        summary.AppendLine($"Active Catch Effects: {catchChainRuntime.ActiveEffectRecords.Length}");
        summary.AppendLine($"Encounter Information: {(CurrentEncounterInformationHidden ? "Hidden" : "Visible")}");
        summary.AppendLine($"Encounter Selection Weight: {encounterRuntime.LastSelectedEncounterWeight} / {encounterRuntime.LastTotalEncounterWeight}");
        summary.AppendLine($"Technique Hand: {techniqueDeckRuntime.Hand.Length} cards");
        summary.AppendLine($"Technique Depth Bonus: {techniqueResolution.AdditionalDepth}");
        summary.AppendLine($"Temporary Capacity: {effectiveCapacity} ({techniqueResolution.CapacityBonus:+#;-#;0})");
        summary.AppendLine($"Committed Catch: Weight {techniqueResolution.CommittedWeightChange:+#;-#;0}, "
            + $"Value {techniqueResolution.CommittedValueChange:+#;-#;0}, "
            + $"Overload Reward {techniqueResolution.OverloadValueReward:+#;-#;0}");
        AppendOverloadRiskSummary(summary, strainReleasedCatch);
        summary.Append("Next Encounter: ");
        summary.Append(encounterRuntime.CurrentEncounter == null ? "none" : encounterRuntime.CurrentEncounter.DisplayName);
        summary.AppendLine();
        summary.Append("Encounter State: ");
        summary.AppendLine(encounterRuntime.CurrentState.ToString());
        summary.AppendLine($"Encounter Variety Rule: {encounterRuntime.LastRepetitionRuleSummary}");
        summary.Append("Recent Encounter Sequence: ");
        summary.Append(encounterRuntime.RecentEncounterSequenceSummary);
        return summary.ToString();
    }

    /// <summary>
    /// Builds a compact Release log showing the lost card and immediate load change.
    /// </summary>
    private string BuildReleaseSummary(CardInstance releasedCatch, int previousLineLoad)
    {
        string encounterName = encounterRuntime.CurrentEncounter == null
            ? "none"
            : encounterRuntime.CurrentEncounter.DisplayName;

        string releasedName = releasedCatch?.Definition == null ? "unknown catch" : releasedCatch.Definition.DisplayName;
        int releasedValue = releasedCatch == null ? 0 : releasedCatch.CurrentValue;

        return $"Release resolved | Released: {releasedName} | Lost Value: {releasedValue} | "
            + $"Line Load: {previousLineLoad} -> {catchChainRuntime.CurrentLineLoad} / {lineCapacity} | Depth: {currentDepth} | "
            + $"Current Encounter: {encounterName} ({encounterRuntime.CurrentState})";
    }

    /// <summary>
    /// Builds the end-of-run summary from the stored Surface result.
    /// </summary>
    private string BuildSurfaceSummary(CardInstance strainReleasedCatch)
    {
        string loadStatus = lastSurfaceResult.WasOverloaded ? "Overloaded" : "Within Capacity";
        StringBuilder summary = new StringBuilder();

        // Unity shows this complete first line even while the Console entry is collapsed.
        summary.AppendLine($"Surface resolved | Haul: {lastSurfaceResult.Haul.Length} cards | "
            + $"Value: {lastSurfaceResult.HaulValue} | Load: {lastSurfaceResult.SurfaceLineLoad} / "
            + $"{lastSurfaceResult.LineCapacity} | Depth: {lastSurfaceResult.SurfaceDepth} | {loadStatus}");
        summary.AppendLine($"Gold Awarded: {lastSurfaceResult.GoldAwarded} | Total Gold: {runRewardRuntime.TotalGold}");
        summary.Append("Successful Haul: ");

        if (lastSurfaceResult.Haul.Length == 0)
        {
            summary.Append("none");
            AppendRunRemovalSummary(summary);
            AppendOverloadRiskSummary(summary, strainReleasedCatch);
            return summary.ToString();
        }

        for (int i = 0; i < lastSurfaceResult.Haul.Length; i++)
        {
            if (i > 0)
            {
                summary.Append(", ");
            }

            CardInstance caughtInstance = lastSurfaceResult.Haul[i];
            summary.Append(caughtInstance?.Definition == null ? "unknown card" : caughtInstance.Definition.DisplayName);
        }

        AppendRunRemovalSummary(summary);
        AppendOverloadRiskSummary(summary, strainReleasedCatch);

        return summary.ToString();
    }

    /// <summary>
    /// Appends the released and involuntarily lost catch histories to the Surface summary.
    /// </summary>
    private void AppendRunRemovalSummary(StringBuilder summary)
    {
        summary.AppendLine();
        summary.Append("Released: ");
        AppendCatchNames(summary, lastSurfaceResult.ReleasedCatches);
        summary.AppendLine();
        summary.Append("Lost: ");
        AppendCatchNames(summary, lastSurfaceResult.LostCatches);
    }

    /// <summary>
    /// Appends a comma-separated catch list, or none when the supplied history is empty.
    /// </summary>
    private static void AppendCatchNames(StringBuilder summary, CardInstance[] catches)
    {
        CardInstance[] safeCatches = catches ?? Array.Empty<CardInstance>();

        if (safeCatches.Length == 0)
        {
            summary.Append("none");
            return;
        }

        for (int i = 0; i < safeCatches.Length; i++)
        {
            if (i > 0)
            {
                summary.Append(", ");
            }

            summary.Append(safeCatches[i]?.Definition == null
                ? "unknown card"
                : safeCatches[i].Definition.DisplayName);
        }
    }

    /// <summary>
    /// Appends the most recent overload check and any lost catch to an action summary.
    /// </summary>
    private void AppendOverloadRiskSummary(StringBuilder summary, CardInstance strainReleasedCatch)
    {
        if (lineLoadRiskRuntime.LastOutcome == LineLoadRiskOutcome.NotOverloaded)
        {
            return;
        }

        if (summary.Length > 0 && summary[summary.Length - 1] != '\n')
        {
            summary.AppendLine();
        }

        summary.Append("Overload Risk: ");

        if (lineLoadRiskRuntime.LastOutcome == LineLoadRiskOutcome.Held)
        {
            summary.AppendLine($"Line held at {lineLoadRiskRuntime.LastBreakChance:P0} break chance");
            return;
        }

        string releasedName = strainReleasedCatch?.Definition == null
            ? "unknown catch"
            : strainReleasedCatch.Definition.DisplayName;
        summary.AppendLine($"Line strain released {releasedName} at {lineLoadRiskRuntime.LastBreakChance:P0} break chance");
    }
}
