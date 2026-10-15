using System;
using System.Collections.Generic;
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
    [SerializeField] private FishingRunView fishingRunView;

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
    [SerializeField] private int selectedCatchIndex = -1;
    [SerializeField] private bool currentEncounterInformationHidden;
    [SerializeField] private EncounterRuntime encounterRuntime = new EncounterRuntime();
    [SerializeField] private BiomeApexRuntime biomeApexRuntime = new BiomeApexRuntime();
    [SerializeField] private CatchChainRuntime catchChainRuntime = new CatchChainRuntime();
    [SerializeField] private TechniqueDeckRuntime techniqueDeckRuntime = new TechniqueDeckRuntime();
    [SerializeField] private TechniqueEffectRuntime techniqueEffectRuntime = new TechniqueEffectRuntime();
    [SerializeField] private EventPersistenceRuntime eventPersistenceRuntime = new EventPersistenceRuntime();

    [Header("Line Load Risk")]
    [SerializeField] private LineLoadRiskRuntime lineLoadRiskRuntime = new LineLoadRiskRuntime();

    [Header("Last Surface Result")]
    [SerializeField] private FishingRunResult lastSurfaceResult = new FishingRunResult();

    [Header("Run Rewards")]
    [SerializeField] private RunRewardRuntime runRewardRuntime = new RunRewardRuntime();
    [SerializeField] private RunProgressionRuntime runProgressionRuntime = new RunProgressionRuntime();

    [SerializeField] private EventEffectRuntime eventEffectRuntime = new EventEffectRuntime();
    [SerializeField] private ConfirmedEventAction confirmedEventAction;
    [SerializeField] private long confirmedEventOccurrenceId;
    [SerializeField] private bool eventConsequencesComplete;
    [SerializeField] private string eventActionFailure;
    private bool eventActionConfirmationInProgress;
    private bool eventActionAdvanceInProgress;
    public ConfirmedEventAction PendingEventAction => confirmedEventAction;
    public bool HasConfirmedEventAction => confirmedEventAction != ConfirmedEventAction.None;
    public string EventActionFailure => eventActionFailure;
    private bool EventActionInputLocked => HasConfirmedEventAction || eventActionConfirmationInProgress || eventActionAdvanceInProgress;

    private System.Random random;
    private EffectResolver effectResolver;
    private bool runEndCleanupPending;
    private CardInstance[] endedRunPresentationCatches = Array.Empty<CardInstance>();
    private ActiveCatchEffectRecord[] endedRunPresentationEffects = Array.Empty<ActiveCatchEffectRecord>();
    private int endedRunPresentationLoad;
    private CardInstance[] DisplayedCatches => !runActive && runEndCleanupPending ? endedRunPresentationCatches : catchChainRuntime.Catches;
    private ActiveCatchEffectRecord[] DisplayedCatchEffects => !runActive && runEndCleanupPending ? endedRunPresentationEffects : catchChainRuntime.ActiveEffectRecords;
    private int DisplayedLineLoad => !runActive && runEndCleanupPending ? endedRunPresentationLoad : catchChainRuntime.CurrentLineLoad;
    [SerializeField] private CreatureEffectRuntime creatureEffectRuntime = new CreatureEffectRuntime();
    [SerializeField] private bool descendCompletionPending;
    [SerializeField] private bool biomeEntryRevealPending;
    [SerializeField] private int pendingCommittedCatchId = -1;
    private int selectedSwapTargetId = -1;
    [SerializeField] private bool techniqueRefillPending;

    private bool IsFailedConfirmedEventAction => HasConfirmedEventAction && !string.IsNullOrEmpty(eventActionFailure);
    public CreatureEffectChoiceKind PendingCreatureChoice => IsFailedConfirmedEventAction || eventEffectRuntime.IsDispatching
        ? CreatureEffectChoiceKind.None : creatureEffectRuntime.ChoiceKind;
    public CardDefinition[] OfferedCreatures => creatureEffectRuntime.OfferedCreatures;
    public CardInstance[] EligibleSwapCatches => creatureEffectRuntime.GetSwapTargets(catchChainRuntime.Catches);
    public NegativeEffectOption[] RemovableNegativeEffects => NegativeEffectRuntime.GetOptions(
        catchChainRuntime, encounterRuntime, techniqueEffectRuntime, effectResolver, eventPersistenceRuntime);
    public bool HasPendingCreatureChoice => PendingCreatureChoice != CreatureEffectChoiceKind.None;

    private bool CoreActionInputReady => runActive && !EventActionInputLocked
        && !encounterRuntime.IsRevealReactionInProgress && !techniqueRefillPending
        && !HasPendingCreatureChoice && !descendCompletionPending && !biomeEntryRevealPending;

    public bool CanDescend => CoreActionInputReady && (!biomeApexRuntime.NextWatersPresented
        || currentBiome?.NextBiome?.GetDepthTier(currentBiome.NextBiome.StartingDepth) != null);
    public bool CanSurface => CoreActionInputReady;
    public bool CanReleaseSelectedCatch => CanReleaseCatch(selectedCatchIndex);

    /// <summary>Reports legal Release availability without mutating an attached card or resolving reactions.</summary>
    public bool CanReleaseCatch(int catchIndex)
    {
        CardInstance[] catches = catchChainRuntime.Catches;
        return CoreActionInputReady && catchIndex >= 0 && catchIndex < catches.Length
            && catches[catchIndex] != null && !catches[catchIndex].PreventsReleaseWhileAttached;
    }

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
    public bool LastRunEndedByLineBreak => lastSurfaceResult.EndedByLineBreak;
    public int CurrentLineLoad => catchChainRuntime.CurrentLineLoad;
    public bool CurrentEncounterInformationHidden => currentEncounterInformationHidden;
    public bool CurrentEncounterIdentityHidden => encounterRuntime.IsCreatureConcealed;
    private bool TechniqueRevealInformationHidden => effectResolver != null
        && effectResolver.HidesEncounterInformation(catchChainRuntime.ActiveEffectRecords)
        && !techniqueEffectRuntime.RevealsCurrentEncounter;
    public BiomeApexState CurrentBiomeApexState => biomeApexRuntime.State;
    public CardDefinition SelectedBiomeApex => biomeApexRuntime.SelectedApex;
    public bool NextWatersPresented => biomeApexRuntime.NextWatersPresented;
    public CardInstance[] LastReleasedCatches => lastSurfaceResult.ReleasedCatches;
    public CardInstance[] LastLostCatches => lastSurfaceResult.LostCatches;
    public int LastGoldAwarded => lastSurfaceResult.GoldAwarded;
    public int TotalGold => runRewardRuntime.TotalGold;
    public int ProgressionLineCapacityBonus => runProgressionRuntime.CurrentLineCapacityBonus;
    public CardDefinition[] SavedTechniqueDeck => runProgressionRuntime.SavedTechniqueDeck;
    public bool UnlockableTechniqueCardUnlocked => runProgressionRuntime.TechniqueCardUnlocked;
    public int SelectedCatchIndex => selectedCatchIndex;

    /// <summary>
    /// Initializes runtime owners and starts the run automatically when configured.
    /// </summary>
    private void Awake()
    {
        EnsureRuntimeObjects();
        if (fishingRunView != null)
        {
            fishingRunView.SurfaceTransitionCompleted += HandleSurfaceTransitionCompleted;
            fishingRunView.EncounterEffectChoiceChanged += HandleEncounterEffectChoiceChanged;
        }

        if (startRunOnAwake)
        {
            StartRun();
        }
    }

    private void OnDestroy()
    {
        if (catchChainRuntime != null)
        {
            catchChainRuntime.DeliberatelyReleased -= HandleDeliberateRelease;
            if (creatureEffectRuntime != null)
                catchChainRuntime.TechniqueTargetSelected -= creatureEffectRuntime.RecordSelectedTechniqueTarget;
        }
        if (encounterRuntime != null)
        {
            encounterRuntime.HazardCancelled -= HandleHazardCancelled;
            if (creatureEffectRuntime != null) encounterRuntime.CreatureRevealed -= creatureEffectRuntime.RecordCreatureReveal;
        }
        if (fishingRunView != null)
        {
            fishingRunView.SurfaceTransitionCompleted -= HandleSurfaceTransitionCompleted;
            fishingRunView.EncounterEffectChoiceChanged -= HandleEncounterEffectChoiceChanged;
        }
    }

    /// <summary>Refreshes the selected role's preview without resolving either effect before Descend.</summary>
    private void HandleEncounterEffectChoiceChanged(EncounterEffectChoice choice)
    {
        if (runActive)
        {
            RefreshViews();
        }
    }

    /// <summary>Refreshes result presentation after the view finishes replaying the ascent.</summary>
    private void HandleSurfaceTransitionCompleted()
    {
        ClearEndedRunState();
        RefreshViews();
    }

    /// <summary>
    /// Initializes capacity, runtime systems, biome, depth, and the first encounter.
    /// </summary>
    [ContextMenu("Run/Start Run")]
    public void StartRun()
    {
        if (eventActionConfirmationInProgress || eventActionAdvanceInProgress) return;
        EnsureRuntimeObjects();
        ResetEventActionState();
        ClearEndedRunState();

        int seed = useRandomSeed ? Environment.TickCount : randomSeed;
        random = new System.Random(seed);
        runProgressionRuntime.InitializeTechniqueDeck(startingTechniqueDeck);

        runActive = true;
        runEndCleanupPending = false;
        creatureEffectRuntime.Reset();
        descendCompletionPending = false;
        techniqueRefillPending = false;
        biomeEntryRevealPending = false;
        pendingCommittedCatchId = -1;
        selectedSwapTargetId = -1;
        lineCapacity = Mathf.Max(0, startingLineCapacity + runProgressionRuntime.CurrentLineCapacityBonus);
        currentBiome = startingBiome;
        currentDepth = Mathf.Max(0, startingDepth);
        selectedCatchIndex = -1;

        encounterRuntime.Reset();
        biomeApexRuntime.Reset();
        eventPersistenceRuntime.Reset();
        catchChainRuntime.Reset();
        techniqueDeckRuntime.Initialize(
            runProgressionRuntime.SavedTechniqueDeck,
            startingHandSize,
            random,
            LogRuntimeWarning);
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
    /// Applies a legal Technique in the catchable/event reaction window, then finishes reactions before hand refill.
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
            out string deckValidationMessage,
            false);

        if (!consumed)
        {
            Debug.LogWarning(deckValidationMessage, this);
            return false;
        }

        techniqueRefillPending = true;
        creatureEffectRuntime.BeginTechniquePlay(techniqueCard, techniqueDeckRuntime.LastDiscardOccurrenceId, catchChainRuntime.Catches);
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
            TechniqueRevealInformationHidden,
            out string effectSummary);

        creatureEffectRuntime.FinishReleaseResolution();
        creatureEffectRuntime.FinishTechniquePlay();
        PrepareCreatureChoice();
        ResumeAfterCreatureChoices();

        if (!applied)
        {
            Debug.LogWarning($"Technique card was consumed but its validated effect did not resolve: {techniqueCard.DisplayName}.", this);
            return false;
        }

        biomeApexRuntime.RecordTechniqueResolution(encounterBeforeTechnique, encounterRuntime.CurrentEncounter);
        if (encounterBeforeTechnique != encounterRuntime.CurrentEncounter) eventActionFailure = string.Empty;

        RefreshViews();
        Debug.Log($"Technique card used: {techniqueCard.DisplayName} | {effectSummary}. "
            + $"Draw: {techniqueDeckRuntime.DrawPile.Length}. "
            + $"Discard: {techniqueDeckRuntime.DiscardPile.Length}.", this);
        return true;
    }

    /// <summary>
    /// Validates an open catchable/event Technique window and the selected card's actual targets.
    /// </summary>
    public bool CanUseTechniqueCard(int handIndex, out string restrictionReason)
    {
        restrictionReason = string.Empty;

        if (!runActive)
        {
            restrictionReason = "Run inactive";
            return false;
        }

        if (HasPendingCreatureChoice)
        {
            restrictionReason = "Resolve the creature effect choice first";
            return false;
        }

        if (!techniqueDeckRuntime.TryGetHandCard(handIndex, out CardDefinition techniqueCard, out restrictionReason))
        {
            return false;
        }

        if (EventActionInputLocked || descendCompletionPending || biomeEntryRevealPending || techniqueRefillPending)
        {
            restrictionReason = "Finish the current action first";
            return false;
        }

        if (!encounterRuntime.HasTechniqueReactionWindow)
        {
            restrictionReason = "No open Technique window";
            return false;
        }

        int encounterSelectionDepth = GetEffectiveEncounterSelectionDepth();
        return techniqueEffectRuntime.CanUseCard(
            techniqueCard,
            encounterRuntime,
            catchChainRuntime,
            TechniqueRevealInformationHidden,
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

        if (!CanDescend)
        {
            return false;
        }

        if (biomeApexRuntime.NextWatersPresented)
        {
            return TryEnterNextBiome();
        }

        if (!TryConfirmEventAction(ConfirmedEventAction.Descend, out bool handled)) return false;
        if (handled) return true;
        CommitCurrentEncounterForDescend();
        ResumeAfterCreatureChoices();
        RefreshViews();
        return true;
    }

    /// <summary>Commits only a catchable encounter, preserving the normal attachment and suspended Descend path.</summary>
    private void CommitCurrentEncounterForDescend()
    {
        CatchAttachmentRole attachmentRole = fishingRunView != null
            && fishingRunView.SelectedEncounterEffectChoice == EncounterEffectChoice.Bait
                ? CatchAttachmentRole.Bait
                : CatchAttachmentRole.Catch;
        bool concealedCommit = encounterRuntime.IsCreatureConcealed;
        CardDefinition caughtCard = encounterRuntime.TakeHookedEncounter();
        if (concealedCommit) RefreshViews();
        CardInstance committedCatch = null;
        catchChainRuntime.CompleteEncounter(effectResolver);

        if (caughtCard != null)
        {
            committedCatch = catchChainRuntime.Add(caughtCard, effectResolver, attachmentRole, random);
            biomeApexRuntime.RecordCommittedApex(caughtCard);
            creatureEffectRuntime.RecordAttachment(committedCatch, false, catchChainRuntime.Catches);
        }

        pendingCommittedCatchId = committedCatch?.InstanceId ?? -1;
        descendCompletionPending = true;
        PrepareCreatureChoice();
    }

    /// <summary>Finishes the suspended Descend only after immediate attachment choices are resolved.</summary>
    private void CompletePendingDescend()
    {
        CardInstance committedCatch = Array.Find(catchChainRuntime.Catches, card => card?.InstanceId == pendingCommittedCatchId);
        pendingCommittedCatchId = -1;
        descendCompletionPending = false;
        TechniqueDescendResolution techniqueResolution = techniqueEffectRuntime.ResolveNextDescend(
            committedCatch,
            catchChainRuntime,
            effectResolver,
            lineCapacity);
        int effectiveCapacity = Mathf.Max(0, lineCapacity + techniqueResolution.CapacityBonus);
        int overloadStartingLoad = catchChainRuntime.CurrentLineLoad;
        CardInstance[] strainLostCatches = ResolveOverloadRisk(effectiveCapacity);
        selectedCatchIndex = -1;

        if (lineLoadRiskRuntime.LastOutcome == LineLoadRiskOutcome.LineBroken)
        {
            CompleteRun(overloadStartingLoad, effectiveCapacity, true);
            Debug.Log(BuildSurfaceSummary(strainLostCatches), this);
            return;
        }

        // The next reveal uses the new depth so data-driven depth ranges take effect immediately.
        currentDepth += Mathf.Max(1, depthStepPerDescend + techniqueResolution.AdditionalDepth);
        techniqueDeckRuntime.Refill(startingHandSize, random);
        RevealEncounterAtCurrentDepth();

        RefreshViews();
        Debug.Log(BuildDescendSummary(committedCatch, strainLostCatches, techniqueResolution, effectiveCapacity), this);
    }

    /// <summary>Enters the authored destination after the Apex transition, retaining all run-owned build state.</summary>
    public bool TryEnterNextBiome()
    {
        BiomeDefinition next = currentBiome?.NextBiome;
        if (!CoreActionInputReady || !biomeApexRuntime.NextWatersPresented
            || next == null || next.GetDepthTier(next.StartingDepth) == null)
        {
            return false;
        }

        EndBiomeEventEffects();
        currentBiome = next;
        currentDepth = next.StartingDepth;
        selectedCatchIndex = -1;
        selectedSwapTargetId = -1;
        encounterRuntime.Reset(true);
        biomeApexRuntime.Reset();
        creatureEffectRuntime.BeginBiome(catchChainRuntime.Catches);
        biomeEntryRevealPending = true;
        PrepareCreatureChoice();
        ResumeAfterCreatureChoices();
        RefreshViews();
        return true;
    }

    /// <summary>Removes the selected active negative occurrence and continues the suspended action.</summary>
    public bool TryChooseNegativeEffect(int optionIndex)
    {
        if (IsFailedConfirmedEventAction || eventActionConfirmationInProgress
            || eventActionAdvanceInProgress || eventEffectRuntime.IsDispatching) return false;
        if (!runActive || PendingCreatureChoice != CreatureEffectChoiceKind.RemoveNegativeEffect)
        {
            return false;
        }

        NegativeEffectOption[] options = RemovableNegativeEffects;
        if (optionIndex < 0 || optionIndex >= options.Length || !options[optionIndex].TryRemove())
        {
            return false;
        }

        creatureEffectRuntime.CompleteChoice();
        selectedSwapTargetId = -1;
        PrepareCreatureChoice();
        ResumeAfterCreatureChoices();
        RefreshViews();
        return true;
    }

    /// <summary>Resolves an offered replacement for an original pre-entry creature Catch.</summary>
    public bool TryChooseCreatureSwap(int catchInstanceId, int offerIndex)
    {
        if (IsFailedConfirmedEventAction || eventEffectRuntime.IsDispatching) return false;
        if (!runActive || !creatureEffectRuntime.TrySwap(
            catchInstanceId, offerIndex, catchChainRuntime, effectResolver, random))
        {
            return false;
        }

        selectedSwapTargetId = -1;
        selectedCatchIndex = -1;
        PrepareCreatureChoice();
        ResumeAfterCreatureChoices();
        RefreshViews();
        return true;
    }

    /// <summary>Resolves an event tie or optional Release while the confirmed action stays locked.</summary>
    public bool TryChooseEventTarget(int targetId)
    {
        if (!runActive || IsFailedConfirmedEventAction || eventActionConfirmationInProgress
            || eventActionAdvanceInProgress || eventEffectRuntime.IsDispatching) return false;
        bool selected = PendingCreatureChoice == CreatureEffectChoiceKind.EventSnagTarget
            ? creatureEffectRuntime.TryChooseEventSnag(targetId, catchChainRuntime)
            : creatureEffectRuntime.TryChooseEventRelease(targetId, catchChainRuntime, effectResolver);
        if (!selected) return false;
        selectedCatchIndex = -1;
        PrepareCreatureChoice(); ResumeAfterCreatureChoices(); RefreshViews(); return true;
    }

    /// <summary>Chooses a legal target for an immediate creature Release.</summary>
    public bool TryChooseCreatureRelease(int targetInstanceId)
    {
        if (IsFailedConfirmedEventAction || eventEffectRuntime.IsDispatching) return false;
        if (!runActive || !creatureEffectRuntime.TryRelease(targetInstanceId, catchChainRuntime, effectResolver))
        {
            return false;
        }
        selectedCatchIndex = -1;
        PrepareCreatureChoice();
        ResumeAfterCreatureChoices();
        RefreshViews();
        return true;
    }

    /// <summary>Returns the selected discard occurrence to hand before normal refill resumes.</summary>
    public bool TryChooseRecoveredTechnique(int discardIndex)
    {
        if (IsFailedConfirmedEventAction || eventEffectRuntime.IsDispatching) return false;
        if (!runActive || PendingCreatureChoice != CreatureEffectChoiceKind.RecoverTechnique
            || !techniqueDeckRuntime.TryRecoverDiscardedTechnique(discardIndex))
        {
            return false;
        }
        creatureEffectRuntime.CompleteChoice();
        PrepareCreatureChoice();
        ResumeAfterCreatureChoices();
        RefreshViews();
        return true;
    }

    /// <summary>Chooses one Buoyant Bell recipient or finishes the optional selection.</summary>
    public bool TryChooseCreatureWeightTarget(int targetInstanceId)
    {
        if (IsFailedConfirmedEventAction || eventEffectRuntime.IsDispatching) return false;
        if (!runActive || !creatureEffectRuntime.TryChooseWeightTarget(targetInstanceId, catchChainRuntime, effectResolver)) return false;
        PrepareCreatureChoice();
        ResumeAfterCreatureChoices();
        RefreshViews();
        return true;
    }

    /// <summary>Resolves or declines Shoal Signal before the suspended attachment action resumes.</summary>
    public bool TryChooseCreatureRedraw(int handIndex)
    {
        if (IsFailedConfirmedEventAction || eventEffectRuntime.IsDispatching) return false;
        if (!runActive || !creatureEffectRuntime.TryChooseRedraw(handIndex, techniqueDeckRuntime, random)) return false;
        PrepareCreatureChoice();
        ResumeAfterCreatureChoices();
        RefreshViews();
        return true;
    }

    /// <summary>Previews the next reveal destination without advancing depth or consuming queued Technique modifiers.</summary>
    private NextEncounterDestination GetNextEncounterDestination()
    {
        int depth = currentDepth;
        if (descendCompletionPending)
        {
            int additional = 0;
            foreach (CardEffectDefinition effect in techniqueEffectRuntime.PendingDescendEffects)
            {
                if (effect?.EffectType == CardEffectType.ModifyDescendDistance) additional += effect.Amount;
            }
            depth += Mathf.Max(1, depthStepPerDescend + additional);
        }
        int selectionDepth = Mathf.Max(0, depth + techniqueEffectRuntime.GetNextEncounterDepthOffset());
        return GetRevealDestination(selectionDepth);
    }

    /// <summary>Reserves Apex and next-waters scheduling before any creature's chosen normal encounter can be used.</summary>
    private NextEncounterDestination GetRevealDestination(int selectionDepth)
    {
        int boundary = currentBiome?.GetApexBoundaryDepth() ?? -1;
        bool scheduled = biomeApexRuntime.CanPresentNextWaters
            || (boundary >= 0 && selectionDepth >= boundary);
        return new NextEncounterDestination(currentBiome, selectionDepth, !scheduled);
    }

    /// <summary>Resolves or declines the shared optional Lionfish release.</summary>
    public bool TryChoosePredatorRelease(int targetId)
    {
        if (IsFailedConfirmedEventAction || eventEffectRuntime.IsDispatching) return false;
        if (!runActive || !creatureEffectRuntime.TryChoosePredatorRelease(targetId, catchChainRuntime, effectResolver)) return false;
        selectedCatchIndex = -1;
        PrepareCreatureChoice();
        ResumeAfterCreatureChoices();
        RefreshViews();
        return true;
    }

    /// <summary>Queues the School Call choice and resumes the current action at its normal reveal time.</summary>
    public bool TryChooseNextCreatureEncounter(int offerIndex)
    {
        if (IsFailedConfirmedEventAction || eventEffectRuntime.IsDispatching) return false;
        if (!runActive || !creatureEffectRuntime.TryChooseNextEncounter(offerIndex)) return false;
        PrepareCreatureChoice();
        ResumeAfterCreatureChoices();
        RefreshViews();
        return true;
    }

    /// <summary>Transfers a released Catch's captured Value after the entire releasing ability completes.</summary>
    public bool TryChooseCreatureValueTransfer(int targetId)
    {
        if (IsFailedConfirmedEventAction || eventEffectRuntime.IsDispatching) return false;
        if (!runActive || !creatureEffectRuntime.TryChooseValueTransfer(targetId, catchChainRuntime, effectResolver)) return false;
        PrepareCreatureChoice();
        ResumeAfterCreatureChoices();
        RefreshViews();
        return true;
    }

    /// <summary>Activates or declines the shared Predator-triggered Sardine sacrifice.</summary>
    public bool TryActivateFeedingFrenzy(int sourceId)
    {
        if (IsFailedConfirmedEventAction || eventEffectRuntime.IsDispatching) return false;
        if (!runActive || !creatureEffectRuntime.TryActivateFeedingFrenzy(sourceId, catchChainRuntime, effectResolver)) return false;
        selectedCatchIndex = -1;
        PrepareCreatureChoice();
        ResumeAfterCreatureChoices();
        RefreshViews();
        return true;
    }

    /// <summary>Discards one technique for room, or finishes the activated capacity-limited draw.</summary>
    public bool TryChooseFeedingDraw(int handIndex)
    {
        if (IsFailedConfirmedEventAction || eventEffectRuntime.IsDispatching) return false;
        if (!runActive || !creatureEffectRuntime.TryChooseFeedingDraw(handIndex, techniqueDeckRuntime, random)) return false;
        PrepareCreatureChoice();
        ResumeAfterCreatureChoices();
        RefreshViews();
        return true;
    }

    /// <summary>Selects the hand half of a sheltered exchange without moving a card, or declines it.</summary>
    public bool TryChooseExchangeHand(int handIndex)
    {
        if (IsFailedConfirmedEventAction || eventEffectRuntime.IsDispatching) return false;
        if (!runActive || !creatureEffectRuntime.TryChooseExchangeHand(handIndex, techniqueDeckRuntime)) return false;
        PrepareCreatureChoice();
        ResumeAfterCreatureChoices();
        RefreshViews();
        return true;
    }

    /// <summary>Completes both validated exchange selections atomically before the suspended attachment action resumes.</summary>
    public bool TryChooseExchangeDiscard(int discardIndex)
    {
        if (IsFailedConfirmedEventAction || eventEffectRuntime.IsDispatching) return false;
        if (!runActive || !creatureEffectRuntime.TryChooseExchangeDiscard(discardIndex, techniqueDeckRuntime)) return false;
        PrepareCreatureChoice();
        ResumeAfterCreatureChoices();
        RefreshViews();
        return true;
    }

    /// <summary>Resolves the mandatory Sea Bass Catch discard before the suspended attachment finishes.</summary>
    public bool TryChooseRequiredTechniqueDiscard(int handIndex)
    {
        if (IsFailedConfirmedEventAction || eventEffectRuntime.IsDispatching) return false;
        if (!runActive || !creatureEffectRuntime.TryChooseRequiredDiscard(handIndex, techniqueDeckRuntime)) return false;
        PrepareCreatureChoice();
        ResumeAfterCreatureChoices();
        RefreshViews();
        return true;
    }

    /// <summary>Selects one Small creature without removing it, or finishes the simultaneous release selection.</summary>
    public bool TryChooseSelectedCreatureRelease(int targetId)
    {
        if (IsFailedConfirmedEventAction || eventEffectRuntime.IsDispatching) return false;
        if (!runActive || !creatureEffectRuntime.TryChooseSelectedRelease(targetId, catchChainRuntime, effectResolver)) return false;
        selectedCatchIndex = -1;
        PrepareCreatureChoice();
        ResumeAfterCreatureChoices();
        RefreshViews();
        return true;
    }

    /// <summary>Selects Helping Arms' Catch half without changing modes, or declines the activation.</summary>
    public bool TryChooseModeSwapCatch(int catchId)
    {
        if (IsFailedConfirmedEventAction || eventEffectRuntime.IsDispatching) return false;
        if (!runActive || !creatureEffectRuntime.TryChooseModeSwapCatch(catchId, catchChainRuntime)) return false;
        PrepareCreatureChoice();
        ResumeAfterCreatureChoices();
        RefreshViews();
        return true;
    }

    /// <summary>Completes the Helping Arms pair and activates the new Catch before the new Bait.</summary>
    public bool TryChooseModeSwapBait(int baitId)
    {
        if (IsFailedConfirmedEventAction || eventEffectRuntime.IsDispatching) return false;
        if (!runActive || !creatureEffectRuntime.TryChooseModeSwapBait(baitId, catchChainRuntime, effectResolver)) return false;
        selectedCatchIndex = -1;
        PrepareCreatureChoice();
        ResumeAfterCreatureChoices();
        RefreshViews();
        return true;
    }

    /// <summary>Discards one hand technique for exact Spiny Grip recovery, or declines its consumed opportunity.</summary>
    public bool TryChoosePlayedTechniqueRecovery(int handIndex)
    {
        if (IsFailedConfirmedEventAction || eventEffectRuntime.IsDispatching) return false;
        if (!runActive || !creatureEffectRuntime.TryChoosePlayedTechniqueRecovery(handIndex, techniqueDeckRuntime)) return false;
        PrepareCreatureChoice();
        ResumeAfterCreatureChoices();
        RefreshViews();
        return true;
    }

    /// <summary>Records the complete Hazard cancellation only after its reveal has been discarded without effects.</summary>
    private void HandleHazardCancelled(CardDefinition hazard)
    {
        catchChainRuntime.RecordHazardCancelled(hazard, effectResolver);
    }

    /// <summary>Resolves the immediate Squid skip decision before any new Technique or core action can be played.</summary>
    public bool TryChooseEncounterSkip(bool skip)
    {
        if (IsFailedConfirmedEventAction || eventEffectRuntime.IsDispatching) return false;
        if (!runActive || !creatureEffectRuntime.TryChooseEncounterSkip(skip, encounterRuntime)) return false;
        PrepareCreatureChoice();
        ResumeAfterCreatureChoices();
        RefreshViews();
        return true;
    }

    /// <summary>Releases a chosen legal Elusive Bait for Slippery Exit, or declines the immediate opportunity.</summary>
    public bool TryChooseMatchingBaitRelease(int targetId)
    {
        if (IsFailedConfirmedEventAction || eventEffectRuntime.IsDispatching) return false;
        if (!runActive || !creatureEffectRuntime.TryChooseMatchingBaitRelease(targetId, catchChainRuntime, effectResolver)) return false;
        selectedCatchIndex = -1;
        PrepareCreatureChoice();
        ResumeAfterCreatureChoices();
        RefreshViews();
        return true;
    }

    /// <summary>Records Release reactions without interrupting the resolving ability.</summary>
    private void HandleDeliberateRelease(CardInstance released, CatchRemovalReason reason)
    {
        creatureEffectRuntime.RecordDeliberateRelease(released, reason, catchChainRuntime.Catches);
    }

    /// <summary>Prepares queued choices once, skipping requests with no eligible targets or offers.</summary>
    private void PrepareCreatureChoice()
    {
        if (!runActive || creatureEffectRuntime == null || IsFailedConfirmedEventAction
            || eventActionConfirmationInProgress || eventEffectRuntime.IsDispatching)
        {
            return;
        }

        creatureEffectRuntime.PrepareChoice(catchChainRuntime, encounterRuntime, currentBiome,
            techniqueEffectRuntime, effectResolver, random, () => RemovableNegativeEffects.Length, techniqueDeckRuntime, GetNextEncounterDestination());
    }

    /// <summary>Resumes either attachment resolution or the first biome reveal after all choices finish.</summary>
    private void ResumeAfterCreatureChoices()
    {
        if (HasPendingCreatureChoice || !runActive)
        {
            return;
        }

        if (eventActionAdvanceInProgress || eventActionConfirmationInProgress) return;
        if (HasConfirmedEventAction)
        {
            AdvanceConfirmedEventAction();
            return;
        }

        if (techniqueRefillPending)
        {
            techniqueRefillPending = false;
            techniqueDeckRuntime.Refill(startingHandSize, random);
        }

        if (descendCompletionPending)
        {
            CompletePendingDescend();
        }
        else if (biomeEntryRevealPending)
        {
            biomeEntryRevealPending = false;
            RevealEncounterAtCurrentDepth();
        }
    }

    /// <summary>Registers a specifically approved effect implementation without guessing behavior for legacy event data.</summary>
    public void RegisterEventEffectHandler(IEventEffectHandler handler)
    {
        EnsureRuntimeObjects();
        eventEffectRuntime.RegisterHandler(handler);
    }

    /// <summary>Validates/claims an event and commits the requested action, or leaves a failed preflight unconfirmed.</summary>
    private bool TryConfirmEventAction(ConfirmedEventAction action, out bool handled)
    {
        EventEncounterOccurrence occurrence = encounterRuntime.CurrentEventOccurrence;
        handled = occurrence != null && (occurrence.State == EventOccurrenceState.Unresolved
            || occurrence.State == EventOccurrenceState.Cancelled || occurrence.State == EventOccurrenceState.Resolving);
        if (!handled) { eventActionFailure = string.Empty; return true; }
        if (!encounterRuntime.HasEventReactionWindow) return false;
        eventActionConfirmationInProgress = true;
        try
        {
            eventActionFailure = string.Empty;
            bool cancelled = occurrence.State == EventOccurrenceState.Cancelled;
            if (cancelled)
            {
                if (!encounterRuntime.TryCloseCancelledEventWindow(occurrence.Id)) return false;
            }
            else if (!eventEffectRuntime.TryBeginResolution(CreateEventResolutionContext(), out string reason))
            {
                eventActionFailure = reason;
                Debug.LogWarning("Event action was not confirmed: " + reason, this);
                return false;
            }
            confirmedEventAction = action;
            confirmedEventOccurrenceId = occurrence.Id;
            eventConsequencesComplete = cancelled;
        }
        finally { eventActionConfirmationInProgress = false; }
        AdvanceConfirmedEventAction();
        return true;
    }

    /// <summary>Finishes event dispatch/creature work, then performs the committed action exactly once without reopening Techniques.</summary>
    private void AdvanceConfirmedEventAction()
    {
        if (!runActive || !HasConfirmedEventAction || IsFailedConfirmedEventAction
            || eventActionAdvanceInProgress || HasPendingCreatureChoice) return;
        eventActionAdvanceInProgress = true;
        bool actionFinished = false;
        try
        {
            if (encounterRuntime.CurrentEventOccurrence?.Id != confirmedEventOccurrenceId)
            {
                RecordEventActionFailure("Confirmed event ownership changed");
                return;
            }
            if (!eventConsequencesComplete)
            {
                EventResolutionContext context = CreateEventResolutionContext();
                EventResolutionProgress progress;
                do
                {
                    progress = eventEffectRuntime.AdvanceResolution(context);
                    if (progress == EventResolutionProgress.Failed)
                    {
                        RecordEventActionFailure(eventEffectRuntime.FailureReason);
                        return;
                    }
                    PrepareCreatureChoice();
                    if (HasPendingCreatureChoice) return;
                } while (progress == EventResolutionProgress.WaitingForCreatureReactions);
                if (progress != EventResolutionProgress.Completed)
                {
                    RecordEventActionFailure("Event did not reach its completed boundary");
                    return;
                }
                eventConsequencesComplete = true;
            }
            if (confirmedEventAction == ConfirmedEventAction.Descend)
            {
                if (!descendCompletionPending) CommitCurrentEncounterForDescend();
                PrepareCreatureChoice();
                if (HasPendingCreatureChoice) return;
                CompletePendingDescend();
                actionFinished = true;
            }
            else if (confirmedEventAction == ConfirmedEventAction.Surface)
            {
                PerformSurface();
                actionFinished = true;
            }
        }
        finally
        {
            if (actionFinished) ClearConfirmedEventAction();
            eventActionAdvanceInProgress = false;
            RefreshViews();
        }
    }

    /// <summary>Reconstructs dispatch context using the same per-run owners, including persistence.</summary>
    private EventResolutionContext CreateEventResolutionContext()
    {
        return new EventResolutionContext(encounterRuntime, catchChainRuntime, creatureEffectRuntime,
            techniqueDeckRuntime, effectResolver, random, eventPersistenceRuntime, currentBiome, currentDepth);
    }

    /// <summary>Preserves a failed confirmed action instead of retrying partially applied event consequences.</summary>
    private void RecordEventActionFailure(string reason)
    {
        if (eventActionFailure == reason) return;
        eventActionFailure = reason;
        Debug.LogWarning("Confirmed event action stopped: " + reason, this);
    }

    /// <summary>Clears the completed action latch without changing the next encounter's lifecycle.</summary>
    private void ClearConfirmedEventAction()
    {
        confirmedEventAction = ConfirmedEventAction.None;
        confirmedEventOccurrenceId = 0;
        eventConsequencesComplete = false;
    }

    /// <summary>Aborts pending dispatch/action state when replacing or ending a run.</summary>
    private void ResetEventActionState()
    {
        eventEffectRuntime.Reset();
        ClearConfirmedEventAction();
        eventActionFailure = string.Empty;
    }

    /// <summary>Supplies modal labels and callbacks from the runtime queue without adding card-face state.</summary>
    private void RefreshCreatureChoiceView()
    {
        if (fishingRunView == null)
        {
            return;
        }

        if (!runActive || !HasPendingCreatureChoice)
        {
            fishingRunView.HideCreatureEffectChoice();
            return;
        }

        if (PendingCreatureChoice == CreatureEffectChoiceKind.EventSnagTarget
            || PendingCreatureChoice == CreatureEffectChoiceKind.EventCreatureRelease)
        {
            bool snag = PendingCreatureChoice == CreatureEffectChoiceKind.EventSnagTarget;
            CardInstance[] targets = snag ? creatureEffectRuntime.GetEventSnagTargets(catchChainRuntime.Catches)
                : creatureEffectRuntime.GetEventReleaseTargets(catchChainRuntime.Catches);
            List<string> labels = new List<string>(); List<string> details = new List<string>(); List<Sprite> art = new List<Sprite>();
            foreach (CardInstance card in targets)
            {
                labels.Add(card.Definition.DisplayName + " (" + card.AttachmentRole + ")");
                details.Add("Weight " + card.CurrentWeight); art.Add(card.Definition.Artwork);
            }
            if (!snag) { labels.Add("Decline"); details.Add("Keep all attached creatures."); art.Add(null); }
            fishingRunView.ShowCreatureEffectChoice(creatureEffectRuntime.ChoiceName,
                snag ? "Choose one of your heaviest non-Armored creatures." : "You may release one attached creature.",
                labels.ToArray(), details.ToArray(), art.ToArray(),
                index => TryChooseEventTarget(index < targets.Length ? targets[index].InstanceId : -1));
            return;
        }

        string abilityName = string.IsNullOrWhiteSpace(creatureEffectRuntime.ChoiceName)
            ? creatureEffectRuntime.GetChoiceSource(catchChainRuntime.Catches)?.Definition?.DisplayName ?? "Creature effect"
            : creatureEffectRuntime.ChoiceName;
        if (PendingCreatureChoice == CreatureEffectChoiceKind.RemoveNegativeEffect)
        {
            NegativeEffectOption[] options = RemovableNegativeEffects;
            string[] labels = Array.ConvertAll(options, option => option.Label);
            string[] details = Array.ConvertAll(options, option => option.Description);
            Sprite[] art = Array.ConvertAll(options, option => option.SourceCard?.Artwork);
            fishingRunView.ShowCreatureEffectChoice(abilityName, "Choose one negative effect to remove.",
                labels, details, art, index => TryChooseNegativeEffect(index));
            return;
        }

        if (PendingCreatureChoice == CreatureEffectChoiceKind.ReleaseAttachedCard)
        {
            CardInstance[] releases = creatureEffectRuntime.GetReleaseTargets(catchChainRuntime.Catches);
            fishingRunView.ShowCreatureEffectChoice(abilityName, "Choose another attached card to release.",
                Array.ConvertAll(releases, card => card.Definition.DisplayName + " (" + card.AttachmentRole + ")"),
                Array.ConvertAll(releases, card => "Weight " + card.CurrentWeight + " · Value "
                    + (card.HidesOwnValueDuringRun && !card.IsBait ? "?" : card.HaulValueContribution.ToString())),
                Array.ConvertAll(releases, card => card.Definition.Artwork),
                index => TryChooseCreatureRelease(releases[index].InstanceId));
            return;
        }

        if (PendingCreatureChoice == CreatureEffectChoiceKind.RecoverTechnique)
        {
            CardDefinition[] discarded = techniqueDeckRuntime.DiscardPile;
            fishingRunView.ShowCreatureEffectChoice(abilityName, "Choose a discarded technique to return to hand.",
                Array.ConvertAll(discarded, card => card.DisplayName),
                Array.ConvertAll(discarded, card => card.RulesText),
                Array.ConvertAll(discarded, card => card.Artwork),
                index => TryChooseRecoveredTechnique(index));
            return;
        }

        if (PendingCreatureChoice == CreatureEffectChoiceKind.RedrawTechnique)
        {
            CardDefinition[] hand = techniqueDeckRuntime.Hand;
            string[] labels = new string[hand.Length + 1];
            string[] details = new string[hand.Length + 1];
            Sprite[] art = new Sprite[hand.Length + 1];
            labels[0] = "Decline redraw";
            details[0] = "Keep your current hand.";
            for (int i = 0; i < hand.Length; i++)
            {
                labels[i + 1] = hand[i].DisplayName;
                details[i + 1] = hand[i].RulesText;
                art[i + 1] = hand[i].Artwork;
            }
            fishingRunView.ShowCreatureEffectChoice(abilityName, "You may discard one technique and draw its replacement.",
                labels, details, art, index => TryChooseCreatureRedraw(index - 1));
            return;
        }

        if (PendingCreatureChoice == CreatureEffectChoiceKind.SelectWeightTargets)
        {
            CardInstance[] eligible = creatureEffectRuntime.GetWeightTargets(catchChainRuntime.Catches);
            string[] labels = new string[eligible.Length + 1];
            string[] details = new string[eligible.Length + 1];
            Sprite[] art = new Sprite[eligible.Length + 1];
            labels[0] = "Finish selection (" + creatureEffectRuntime.SelectedWeightTargetCount + "/2)";
            details[0] = "Unused selections cannot be saved.";
            for (int i = 0; i < eligible.Length; i++)
            {
                labels[i + 1] = eligible[i].Definition.DisplayName + " (" + eligible[i].AttachmentRole + ")";
                details[i + 1] = "Weight " + eligible[i].CurrentWeight + " · Reduce by 1 while attached and Armored.";
                art[i + 1] = eligible[i].Definition.Artwork;
            }
            fishingRunView.ShowCreatureEffectChoice(abilityName, "Choose up to two distinct Armored creatures.",
                labels, details, art, index => TryChooseCreatureWeightTarget(index == 0 ? -1 : eligible[index - 1].InstanceId));
            return;
        }

        if (PendingCreatureChoice == CreatureEffectChoiceKind.OptionalPredatorRelease)
        {
            CardInstance[] eligible = creatureEffectRuntime.GetPredatorReleaseTargets(catchChainRuntime.Catches);
            string[] labels = new string[eligible.Length + 1];
            string[] details = new string[eligible.Length + 1];
            Sprite[] art = new Sprite[eligible.Length + 1];
            labels[0] = "Decline release";
            details[0] = "Keep all attached creatures.";
            for (int i = 0; i < eligible.Length; i++)
            {
                labels[i + 1] = eligible[i].Definition.DisplayName + " (" + eligible[i].AttachmentRole + ")";
                details[i + 1] = "Weight " + eligible[i].CurrentWeight + " · Deliberately release this Small creature.";
                art[i + 1] = eligible[i].Definition.Artwork;
            }
            fishingRunView.ShowCreatureEffectChoice(abilityName, "You may release another Small creature.",
                labels, details, art, index => TryChoosePredatorRelease(index == 0 ? -1 : eligible[index - 1].InstanceId));
            return;
        }
        if (PendingCreatureChoice == CreatureEffectChoiceKind.ChooseNextEncounter)
        {
            CardDefinition[] schoolOffers = OfferedCreatures;
            string[] labels = new string[schoolOffers.Length + 1];
            string[] details = new string[schoolOffers.Length + 1];
            Sprite[] art = new Sprite[schoolOffers.Length + 1];
            labels[0] = "Decline selection";
            details[0] = "Keep the next encounter draw or earlier selection.";
            for (int i = 0; i < schoolOffers.Length; i++)
            {
                labels[i + 1] = schoolOffers[i].DisplayName;
                details[i + 1] = "Choose as your next encounter · Weight " + schoolOffers[i].Weight;
                art[i + 1] = schoolOffers[i].Artwork;
            }
            fishingRunView.ShowCreatureEffectChoice(abilityName, "Choose your next Schooling encounter.",
                labels, details, art, index => TryChooseNextCreatureEncounter(index - 1));
            return;
        }
        if (PendingCreatureChoice == CreatureEffectChoiceKind.TransferValue)
        {
            CardInstance[] eligible = creatureEffectRuntime.GetTransferTargets(catchChainRuntime.Catches);
            fishingRunView.ShowCreatureEffectChoice(abilityName, "Choose a Catch to gain " + creatureEffectRuntime.TransferValue + " Value.",
                Array.ConvertAll(eligible, card => card.Definition.DisplayName),
                Array.ConvertAll(eligible, card => "Value " + (card.HidesOwnValueDuringRun ? "?" : card.CurrentValue.ToString())),
                Array.ConvertAll(eligible, card => card.Definition.Artwork),
                index => TryChooseCreatureValueTransfer(eligible[index].InstanceId));
            return;
        }

        if (PendingCreatureChoice == CreatureEffectChoiceKind.FeedingFrenzy)
        {
            CardInstance[] sources = creatureEffectRuntime.GetFeedingFrenzySources(catchChainRuntime.Catches);
            string[] labels = new string[sources.Length + 1];
            string[] details = new string[sources.Length + 1];
            Sprite[] art = new Sprite[sources.Length + 1];
            labels[0] = "Decline sacrifice";
            details[0] = "Keep your Sardine Baits.";
            for (int i = 0; i < sources.Length; i++)
            {
                labels[i + 1] = sources[i].Definition.DisplayName + " · Bait " + (i + 1);
                details[i + 1] = "Release this Bait, then draw up to 2 techniques. You may discard for room.";
                art[i + 1] = sources[i].Definition.Artwork;
            }
            fishingRunView.ShowCreatureEffectChoice(abilityName, "Release one Sardine Bait to draw techniques?",
                labels, details, art, index => TryActivateFeedingFrenzy(index == 0 ? -1 : sources[index - 1].InstanceId));
            return;
        }
        if (PendingCreatureChoice == CreatureEffectChoiceKind.DrawTechniques)
        {
            CardDefinition[] cards = techniqueDeckRuntime.Hand;
            string[] labels = new string[cards.Length + 1];
            string[] details = new string[cards.Length + 1];
            Sprite[] art = new Sprite[cards.Length + 1];
            int room = Math.Min(creatureEffectRuntime.PendingDrawCount, Math.Max(0, 4 - cards.Length));
            labels[0] = "Finish · draw up to " + room;
            details[0] = "Draw only as many techniques as fit in four slots.";
            for (int i = 0; i < cards.Length; i++)
            {
                labels[i + 1] = "Discard " + cards[i].DisplayName;
                details[i + 1] = "Make room without playing this technique.";
                art[i + 1] = cards[i].Artwork;
            }
            fishingRunView.ShowCreatureEffectChoice(abilityName, "You may discard for room before drawing " + creatureEffectRuntime.PendingDrawCount + ".",
                labels, details, art, index => TryChooseFeedingDraw(index - 1));
            return;
        }
        if (PendingCreatureChoice == CreatureEffectChoiceKind.ExchangeTechniques)
        {
            bool choosingHand = creatureEffectRuntime.SelectedExchangeHandIndex < 0;
            CardDefinition[] cards = choosingHand ? techniqueDeckRuntime.Hand : techniqueDeckRuntime.DiscardPile;
            string[] labels = new string[cards.Length + 1];
            string[] details = new string[cards.Length + 1];
            Sprite[] art = new Sprite[cards.Length + 1];
            labels[0] = "Decline exchange";
            details[0] = "Keep your hand and discard pile.";
            for (int i = 0; i < cards.Length; i++)
            {
                labels[i + 1] = cards[i].DisplayName;
                details[i + 1] = choosingHand ? "Choose this hand card to discard." : "Return this already-discarded technique to hand.";
                art[i + 1] = cards[i].Artwork;
            }
            fishingRunView.ShowCreatureEffectChoice(abilityName,
                choosingHand ? "Choose a technique in hand." : "Choose a technique in your discard pile.",
                labels, details, art, index =>
                {
                    if (index == 0) TryChooseExchangeHand(-1);
                    else if (choosingHand) TryChooseExchangeHand(index - 1);
                    else TryChooseExchangeDiscard(index - 1);
                });
            return;
        }

        if (PendingCreatureChoice == CreatureEffectChoiceKind.DiscardTechnique
            || PendingCreatureChoice == CreatureEffectChoiceKind.RecoverPlayedTechnique)
        {
            bool required = PendingCreatureChoice == CreatureEffectChoiceKind.DiscardTechnique;
            CardDefinition[] cards = techniqueDeckRuntime.Hand;
            int offset = required ? 0 : 1;
            string[] labels = new string[cards.Length + offset];
            string[] details = new string[labels.Length];
            Sprite[] art = new Sprite[labels.Length];
            if (!required)
            {
                labels[0] = "Decline recovery";
                details[0] = "Keep your hand. This recovery has been consumed.";
            }
            for (int i = 0; i < cards.Length; i++)
            {
                labels[i + offset] = "Discard " + cards[i].DisplayName;
                details[i + offset] = required ? "Discard without playing this technique." : "Make room to return the technique just played.";
                art[i + offset] = cards[i].Artwork;
            }
            fishingRunView.ShowCreatureEffectChoice(abilityName,
                required ? "Choose one technique to discard." : "Discard one technique for room, or decline recovery.",
                labels, details, art, index =>
                {
                    if (required) TryChooseRequiredTechniqueDiscard(index);
                    else TryChoosePlayedTechniqueRecovery(index - 1);
                });
            return;
        }
        if (PendingCreatureChoice == CreatureEffectChoiceKind.SelectReleaseTargets)
        {
            CardInstance[] targets = creatureEffectRuntime.GetSelectedReleaseTargets(catchChainRuntime.Catches);
            string[] labels = new string[targets.Length + 1];
            string[] details = new string[labels.Length];
            Sprite[] art = new Sprite[labels.Length];
            labels[0] = creatureEffectRuntime.SelectedReleaseCount == 0 ? "Decline releases" : "Release selected creature";
            details[0] = "Finish selecting; unused releases cannot be saved.";
            for (int i = 0; i < targets.Length; i++)
            {
                labels[i + 1] = targets[i].Definition.DisplayName + " (" + targets[i].AttachmentRole + ")";
                details[i + 1] = "Weight " + targets[i].CurrentWeight + " · Value "
                    + (targets[i].IsBait ? "0" : targets[i].HidesOwnValueDuringRun ? "?" : targets[i].CurrentValue.ToString())
                    + ". All chosen creatures leave together.";
                art[i + 1] = targets[i].Definition.Artwork;
            }
            fishingRunView.ShowCreatureEffectChoice(abilityName,
                "Choose up to 2 creatures · Selected " + creatureEffectRuntime.SelectedReleaseCount,
                labels, details, art, index => TryChooseSelectedCreatureRelease(index == 0 ? -1 : targets[index - 1].InstanceId));
            return;
        }
        if (PendingCreatureChoice == CreatureEffectChoiceKind.SwapCreatureModes)
        {
            bool choosingCatch = creatureEffectRuntime.SelectedModeSwapCatchId < 0;
            CardInstance[] targets = creatureEffectRuntime.GetModeSwapTargets(catchChainRuntime, !choosingCatch);
            string[] labels = new string[targets.Length + 1];
            string[] details = new string[labels.Length];
            Sprite[] art = new Sprite[labels.Length];
            labels[0] = "Decline swap";
            details[0] = "Keep both creatures in their current modes.";
            for (int i = 0; i < targets.Length; i++)
            {
                labels[i + 1] = targets[i].Definition.DisplayName;
                details[i + 1] = (choosingCatch ? "Switch Catch to Bait." : "Switch Bait to Catch.")
                    + " Current Weight " + targets[i].CurrentWeight + " · Value "
                    + (targets[i].HidesOwnValueDuringRun ? "?" : targets[i].CurrentValue.ToString());
                art[i + 1] = targets[i].Definition.Artwork;
            }
            fishingRunView.ShowCreatureEffectChoice(abilityName,
                choosingCatch ? "Choose another creature Catch." : "Choose another creature Bait.",
                labels, details, art, index =>
                {
                    if (index == 0) TryChooseModeSwapCatch(-1);
                    else if (choosingCatch) TryChooseModeSwapCatch(targets[index - 1].InstanceId);
                    else TryChooseModeSwapBait(targets[index - 1].InstanceId);
                });
            return;
        }

        if (PendingCreatureChoice == CreatureEffectChoiceKind.EncounterSkip)
        {
            fishingRunView.ShowCreatureEffectChoice(abilityName, "Skip this creature before playing techniques?",
                new[] { "Skip creature", "Keep creature" },
                new[] { "Discard without attaching; depth stays unchanged.", "Continue with this creature. The skip offer is spent." },
                new Sprite[2], index => TryChooseEncounterSkip(index == 0));
            return;
        }
        if (PendingCreatureChoice == CreatureEffectChoiceKind.ReleaseMatchingBait)
        {
            CardInstance[] eligible = creatureEffectRuntime.GetMatchingBaitReleaseTargets(catchChainRuntime.Catches);
            string[] labels = new string[eligible.Length + 1];
            string[] details = new string[labels.Length];
            Sprite[] art = new Sprite[labels.Length];
            labels[0] = "Decline release";
            details[0] = "Keep your Elusive Baits.";
            for (int i = 0; i < eligible.Length; i++)
            {
                labels[i + 1] = eligible[i].Definition.DisplayName;
                details[i + 1] = "Release this Elusive Bait · Weight " + eligible[i].CurrentWeight;
                art[i + 1] = eligible[i].Definition.Artwork;
            }
            fishingRunView.ShowCreatureEffectChoice(abilityName, "You may release another Elusive Bait.",
                labels, details, art, index => TryChooseMatchingBaitRelease(index == 0 ? -1 : eligible[index - 1].InstanceId));
            return;
        }

        CardInstance[] swapTargets = EligibleSwapCatches;
        CardInstance selected = Array.Find(swapTargets, card => card.InstanceId == selectedSwapTargetId);
        if (selected == null)
        {
            selectedSwapTargetId = -1;
            fishingRunView.ShowCreatureEffectChoice(abilityName, "Choose a Catch to replace.",
                Array.ConvertAll(swapTargets, card => card.Definition.DisplayName),
                Array.ConvertAll(swapTargets, card => "Weight " + card.CurrentWeight + " · Value "
                    + (card.HidesOwnValueDuringRun ? "?" : card.HaulValueContribution.ToString())),
                Array.ConvertAll(swapTargets, card => card.Definition.Artwork),
                index =>
                {
                    selectedSwapTargetId = swapTargets[index].InstanceId;
                    RefreshViews();
                });
            return;
        }

        CardDefinition[] offers = OfferedCreatures;
        string[] offerLabels = new string[offers.Length + 1];
        string[] offerDetails = new string[offers.Length + 1];
        Sprite[] offerArt = new Sprite[offers.Length + 1];
        for (int i = 0; i < offers.Length; i++)
        {
            CardDefinition card = offers[i];
            offerLabels[i] = card.DisplayName;
            string value = card.HidesOwnValueDuringRunForRole(CatchAttachmentRole.Catch)
                ? "?"
                : card.RandomizesValueWhenCaught
                    ? card.MinimumCaughtValue + "–" + card.MaximumCaughtValue
                    : card.Value.ToString();
            offerDetails[i] = "Weight " + card.Weight + " · Value " + value + " · " + card.RulesText;
            offerArt[i] = card.Artwork;
        }

        offerLabels[offers.Length] = "Choose a different Catch";
        offerDetails[offers.Length] = "Keep the same creature offers.";
        fishingRunView.ShowCreatureEffectChoice("Cleaning Station", "Replace " + selected.Definition.DisplayName + " with:",
            offerLabels, offerDetails, offerArt,
            index =>
            {
                if (index == offers.Length)
                {
                    selectedSwapTargetId = -1;
                    RefreshViews();
                }
                else
                {
                    TryChooseCreatureSwap(selectedSwapTargetId, index);
                }
            });
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

        if (!CanReleaseCatch(catchIndex))
        {
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

        selectedCatchIndex = -1;
        creatureEffectRuntime.FinishReleaseResolution();
        PrepareCreatureChoice();
        ResumeAfterCreatureChoices();
        RefreshViews();
        Debug.Log(BuildReleaseSummary(releasedCatch, previousLineLoad), this);
        return true;
    }

    /// <summary>
    /// Selects or deselects an attached catch for the player-facing Release action.
    /// </summary>
    public void SelectCatchForRelease(int catchIndex)
    {
        if (!CoreActionInputReady) return;
        if (!runActive || catchIndex < 0 || catchIndex >= catchChainRuntime.Catches.Length)
        {
            selectedCatchIndex = -1;
            RefreshViews();
            return;
        }

        selectedCatchIndex = selectedCatchIndex == catchIndex ? -1 : catchIndex;
        RefreshViews();
    }

    /// <summary>
    /// Releases the catch currently selected in the player-facing Catch Chain.
    /// </summary>
    public bool TryReleaseSelectedCatch()
    {
        if (selectedCatchIndex < 0 || selectedCatchIndex >= catchChainRuntime.Catches.Length)
        {
            Debug.LogWarning("Select an attached catch before using Release.", this);
            return false;
        }

        return TryReleaseCatch(selectedCatchIndex);
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

        if (!CanSurface)
        {
            return false;
        }

        if (!TryConfirmEventAction(ConfirmedEventAction.Surface, out bool handled)) return false;
        if (handled) return true;
        PerformSurface();
        return true;
    }

    /// <summary>Evaluates the return and records the haul only after any event consequences and reactions finish.</summary>
    private void PerformSurface()
    {
        int surfaceStartingLoad = catchChainRuntime.CurrentLineLoad;
        CardInstance[] strainLostCatches = ResolveOverloadRisk(lineCapacity);
        bool lineBroke = lineLoadRiskRuntime.LastOutcome == LineLoadRiskOutcome.LineBroken;

        CompleteRun(surfaceStartingLoad, lineCapacity, lineBroke);
        string surfaceSummary = BuildSurfaceSummary(strainLostCatches);

        Debug.Log(surfaceSummary, this);
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
    /// Unlocks the configured Technique card and adds it to the saved deck for future runs.
    /// </summary>
    public bool TryUnlockTechniqueCard()
    {
        bool unlocked = runProgressionRuntime.TryUnlockTechniqueCard(
            runRewardRuntime,
            runActive,
            lastSurfaceResult.HasResult,
            out string resultSummary);

        if (!unlocked)
        {
            Debug.LogWarning(resultSummary, this);
            return false;
        }

        RefreshViews();
        Debug.Log($"TECHNIQUE UNLOCKED | {resultSummary} | Gold Remaining: {runRewardRuntime.TotalGold}", this);
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

        if (eventActionConfirmationInProgress || eventActionAdvanceInProgress) return false;
        ResetEventActionState();
        lineCapacity = Mathf.Max(0, scenario.LineCapacity);
        currentDepth = Mathf.Max(0, scenario.Depth);
        selectedCatchIndex = -1;
        encounterRuntime.Reset();
        biomeApexRuntime.Reset();
        eventPersistenceRuntime.Reset();
        catchChainRuntime.Reset();
        techniqueEffectRuntime.Reset();

        creatureEffectRuntime.Reset();
        descendCompletionPending = false;
        biomeEntryRevealPending = false;
        pendingCommittedCatchId = -1;
        selectedSwapTargetId = -1;
        CardDefinition[] scenarioCatches = scenario.StartingCatches ?? Array.Empty<CardDefinition>();

        for (int i = 0; i < scenarioCatches.Length; i++)
        {
            creatureEffectRuntime.RecordAttachment(catchChainRuntime.Add(scenarioCatches[i], effectResolver, random));
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

        creatureEffectRuntime.RecordAttachment(catchChainRuntime.Add(debugCatchCard, effectResolver, random));
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

        if (creatureEffectRuntime == null)
        {
            creatureEffectRuntime = new CreatureEffectRuntime();
        }

        catchChainRuntime.DeliberatelyReleased -= HandleDeliberateRelease;
        catchChainRuntime.DeliberatelyReleased += HandleDeliberateRelease;
        catchChainRuntime.TechniqueTargetSelected -= creatureEffectRuntime.RecordSelectedTechniqueTarget;
        catchChainRuntime.TechniqueTargetSelected += creatureEffectRuntime.RecordSelectedTechniqueTarget;
        creatureEffectRuntime.EncounterEffects = encounterRuntime;
        encounterRuntime.HazardCancellation = catchChainRuntime.TryCancelHazard;
        encounterRuntime.HazardCancelled -= HandleHazardCancelled;
        encounterRuntime.HazardCancelled += HandleHazardCancelled;
        encounterRuntime.CreatureRevealed -= creatureEffectRuntime.RecordCreatureReveal;
        encounterRuntime.CreatureRevealed += creatureEffectRuntime.RecordCreatureReveal;

        if (effectResolver == null)
        {
            effectResolver = new EffectResolver();
        }
        if (eventEffectRuntime == null) eventEffectRuntime = new EventEffectRuntime();
        if (eventPersistenceRuntime == null) eventPersistenceRuntime = new EventPersistenceRuntime();
        eventPersistenceRuntime.Bind(catchChainRuntime, effectResolver);
        CoastalEventHandlers.Register(eventEffectRuntime);
        creatureEffectRuntime.EventPersistence = eventPersistenceRuntime;
        catchChainRuntime.BeforeAttachmentEffects -= eventPersistenceRuntime.ApplyPendingBaitWeightReductions;
        catchChainRuntime.BeforeAttachmentEffects += eventPersistenceRuntime.ApplyPendingBaitWeightReductions;
        encounterRuntime.EventAvailability = IsEventAvailable;
    }

    /// <summary>Filters conditional trail events before random selection or Technique replacement.</summary>
    private bool IsEventAvailable(CardDefinition card, int selectionDepth)
    {
        CardEffectDefinition effect = Array.Find(card.Effects ?? Array.Empty<CardEffectDefinition>(),
            e => e?.EffectType == CardEffectType.ScheduleCreatureTrail);
        return effect == null || !encounterRuntime.Trail.HasPending && encounterRuntime.ActiveEncounterChain == null
            && EventEncounterTrailRuntime.IsLegalSlot(currentBiome, currentDepth + effect.Amount, effect.ReplacementCard);
    }

    /// <summary>Expires biome-scoped modifiers once, retaining cross-biome pending effects.</summary>
    private void EndBiomeEventEffects()
    {
        eventPersistenceRuntime.NotifyBoundary(eventPersistenceRuntime.Epoch, EventEffectLifetime.BiomeEnd,
            creatureEffectRuntime.BiomeVisit + 1L);
        encounterRuntime.Trail.Reset();
    }

    /// <summary>
    /// Reveals an encounter for the current biome and depth, reporting an empty candidate pool.
    /// </summary>
    private void RevealEncounterAtCurrentDepth()
    {
        creatureEffectRuntime.BeginEncounter();
        int selectionDepth = GetEffectiveEncounterSelectionDepth();
        CardDefinition chosen = creatureEffectRuntime.TakeQueuedEncounter(GetRevealDestination(selectionDepth));

        if (TryRevealBiomeApex(selectionDepth))
        {
            encounterRuntime.Trail.Reset();
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
            encounterRuntime.Trail.Reset();
            encounterRuntime.SetCurrentEncounter(null);
            techniqueEffectRuntime.CompleteEncounterReveal();

            if (biomeApexRuntime.State == BiomeApexState.Unavailable)
            {
                Debug.LogWarning(biomeApexRuntime.LastSelectionSummary, this);
            }

            return;
        }

        CardDefinition promised = encounterRuntime.Trail.TakeForSlot(currentBiome, currentDepth, selectionDepth, chosen != null);
        if (chosen == null) chosen = promised;
        bool revealed = chosen != null
            ? encounterRuntime.RevealSelectedCreature(chosen, CurrentBiomeId, selectionDepth,
                catchChainRuntime.ActiveEffectRecords, effectResolver, techniqueEffectRuntime.PendingEncounterEffects,
                currentBiome?.EncounterChains)
            : encounterRuntime.Reveal(
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

        EndBiomeEventEffects();
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
        CaptureEndedRunPresentation();
        runActive = false;
        selectedCatchIndex = -1;
        runEndCleanupPending = true;
        ResetEndedRunRuntimeState();
        RefreshViews();
        if (fishingRunView == null || !fishingRunView.IsSurfaceTransitionPlaying)
        {
            ClearEndedRunState();
            RefreshViews();
        }
    }

    /// <summary>Freezes the visible rig before clearing gameplay owners; effect records reference only presentation copies.</summary>
    private void CaptureEndedRunPresentation()
    {
        endedRunPresentationLoad = catchChainRuntime.CurrentLineLoad;
        endedRunPresentationCatches = Array.ConvertAll(catchChainRuntime.Catches, card => card?.CreateSnapshot());
        List<ActiveCatchEffectRecord> records = new List<ActiveCatchEffectRecord>();
        foreach (ActiveCatchEffectRecord record in catchChainRuntime.ActiveEffectRecords)
        {
            CardInstance snapshot = Array.Find(endedRunPresentationCatches, card => card != null
                && card.InstanceId == record.SourceInstance?.InstanceId);
            if (snapshot != null) records.Add(new ActiveCatchEffectRecord(snapshot, record.Effect, record.ActiveTrigger, record.SourceCatchIndex));
        }
        endedRunPresentationEffects = records.ToArray();
    }

    /// <summary>Clears all live event, trail, deck, and creature ownership immediately when the run ends.</summary>
    private void ResetEndedRunRuntimeState()
    {
        ResetEventActionState();
        encounterRuntime.Reset();
        biomeApexRuntime.Reset();
        eventPersistenceRuntime.Reset();
        catchChainRuntime.Reset();
        techniqueDeckRuntime.Reset();
        techniqueEffectRuntime.Reset();
        creatureEffectRuntime.Reset();
        descendCompletionPending = false;
        biomeEntryRevealPending = false;
        techniqueRefillPending = false;
        pendingCommittedCatchId = -1;
        selectedSwapTargetId = -1;
    }

    /// <summary>Releases presentation-only copies after ascent or before a new run without touching current gameplay state.</summary>
    private void ClearEndedRunState()
    {
        runEndCleanupPending = false;
        endedRunPresentationCatches = Array.Empty<CardInstance>();
        endedRunPresentationEffects = Array.Empty<ActiveCatchEffectRecord>();
        endedRunPresentationLoad = 0;
    }

    /// <summary>
    /// Resolves overload risk and randomly loses up to half of the Catch Chain when the line breaks.
    /// </summary>
    private CardInstance[] ResolveOverloadRisk(int effectiveCapacity)
    {
        bool lineBroke = lineLoadRiskRuntime.Evaluate(
            catchChainRuntime.CurrentLineLoad,
            effectiveCapacity,
            catchChainRuntime.Catches.Length,
            random);

        if (!lineBroke)
        {
            return Array.Empty<CardInstance>();
        }

        return catchChainRuntime.LoseRandomCatchesAfterLineBreak(
            effectResolver,
            random);
    }

    /// <summary>Snapshots rewards and begins the shared ascent presentation for any run-ending outcome.</summary>
    private void CompleteRun(int startingLoad, int evaluatedCapacity, bool endedByLineBreak)
    {
        // The Hooked encounter is intentionally excluded because it has not entered the Catch Chain.
        lastSurfaceResult.Record(
            catchChainRuntime.Catches,
            catchChainRuntime.ReleasedCatches,
            catchChainRuntime.LostCatches,
            currentDepth,
            startingLoad,
            evaluatedCapacity,
            endedByLineBreak);
        runRewardRuntime.AwardGold(lastSurfaceResult);
        EndActiveRun();
    }

    /// <summary>
    /// Updates optional card views from the current runtime owners.
    /// </summary>
    private void RefreshViews()
    {
        PrepareCreatureChoice();
        RefreshCreatureChoiceView();
        currentEncounterInformationHidden = CurrentEncounterIdentityHidden || (effectResolver != null
            && effectResolver.HidesEncounterInformation(catchChainRuntime.ActiveEffectRecords)
            && !techniqueEffectRuntime.RevealsCurrentEncounter);

        if (currentEncounterView != null)
        {
            currentEncounterView.SetCard(
                encounterRuntime.CurrentEncounter,
                encounterRuntime.CurrentState,
                CurrentEncounterInformationHidden, CurrentEncounterIdentityHidden);
        }

        if (catchChainView != null)
        {
            catchChainView.Refresh(
                DisplayedCatches,
                DisplayedCatchEffects,
                DisplayedLineLoad,
                lineCapacity,
                selectedCatchIndex,
                SelectCatchForRelease);
        }

        if (fishingRunView != null)
        {
            CardDefinition currentEncounter = encounterRuntime.CurrentEncounter;
            CatchAttachmentRole previewRole = fishingRunView.SelectedEncounterEffectChoice == EncounterEffectChoice.Bait
                ? CatchAttachmentRole.Bait
                : CatchAttachmentRole.Catch;
            CardInstance encounterPreview = catchChainRuntime.CreateResolvedCatchPreview(
                currentEncounter,
                effectResolver,
                previewRole);
            int resolvedEncounterWeight = 0;
            int resolvedEncounterValue = 0;

            if (encounterPreview != null)
            {
                eventPersistenceRuntime.PreviewPendingBaitWeightReductions(encounterPreview);
                resolvedEncounterWeight = encounterPreview.CurrentWeight;
                resolvedEncounterValue = encounterPreview.HaulValueContribution;
            }
            else if (currentEncounter != null)
            {
                resolvedEncounterWeight = currentEncounter.Weight;
                resolvedEncounterValue = previewRole == CatchAttachmentRole.Bait ? 0 : currentEncounter.Value;
            }

            fishingRunView.Refresh(
                runActive,
                currentBiome,
                currentDepth,
                currentEncounter,
                resolvedEncounterWeight,
                resolvedEncounterValue,
                CurrentEncounterInformationHidden,
                lineCapacity,
                DisplayedLineLoad,
                techniqueDeckRuntime.DrawPile.Length,
                selectedCatchIndex,
                CanDescend,
                CanReleaseSelectedCatch,
                CanSurface,
                TryDescend,
                TryReleaseSelectedCatch,
                TrySurface,
                CurrentEncounterIdentityHidden);
            fishingRunView.SetTrailPreview(runActive ? encounterRuntime.Trail.Preview : string.Empty);
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
            bool resultPresentationBlocked = fishingRunView != null
                && fishingRunView.IsSurfaceTransitionPlaying;
            bool canPurchaseUpgrade = runProgressionRuntime.CanPurchaseLineCapacityUpgrade(
                runRewardRuntime,
                runActive,
                lastSurfaceResult.HasResult,
                out string upgradeRestrictionReason);
            bool canUnlockTechnique = runProgressionRuntime.CanUnlockTechniqueCard(
                runRewardRuntime,
                runActive,
                lastSurfaceResult.HasResult,
                out string techniqueRestrictionReason);
            runResultView.Refresh(
                runActive || resultPresentationBlocked,
                lastSurfaceResult,
                runRewardRuntime.TotalGold,
                runProgressionRuntime,
                canPurchaseUpgrade,
                upgradeRestrictionReason,
                canUnlockTechnique,
                techniqueRestrictionReason,
                TryPurchaseLineCapacityUpgrade,
                TryUnlockTechniqueCard,
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
        CardInstance committedCatch,
        CardInstance[] strainLostCatches,
        TechniqueDescendResolution techniqueResolution,
        int effectiveCapacity)
    {
        StringBuilder summary = new StringBuilder();
        summary.AppendLine("Descend resolved.");
        summary.Append("Caught: ");
        summary.Append(committedCatch?.Definition == null ? "none" : committedCatch.Definition.DisplayName);
        if (committedCatch != null)
        {
            summary.Append($" ({committedCatch.AttachmentRole})");
        }
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
        AppendOverloadRiskSummary(summary, strainLostCatches);
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
        string releasedValue = releasedCatch != null && !releasedCatch.IsBait && releasedCatch.HidesOwnValueDuringRun
            ? "?"
            : (releasedCatch?.HaulValueContribution ?? 0).ToString();

        return $"Release resolved | Released: {releasedName} | Lost Value: {releasedValue} | "
            + $"Line Load: {previousLineLoad} -> {catchChainRuntime.CurrentLineLoad} / {lineCapacity} | Depth: {currentDepth} | "
            + $"Current Encounter: {encounterName} ({encounterRuntime.CurrentState})";
    }

    /// <summary>
    /// Builds the end-of-run summary from the stored Surface result.
    /// </summary>
    private string BuildSurfaceSummary(CardInstance[] strainLostCatches)
    {
        string loadStatus = lastSurfaceResult.WasOverloaded ? "Overloaded" : "Within Capacity";
        string completionLabel = lastSurfaceResult.EndedByLineBreak ? "Line break resolved" : "Surface resolved";
        StringBuilder summary = new StringBuilder();

        // Unity shows this complete first line even while the Console entry is collapsed.
        summary.AppendLine($"{completionLabel} | Haul: {lastSurfaceResult.Haul.Length} cards | "
            + $"Value: {lastSurfaceResult.HaulValue} | Load: {lastSurfaceResult.SurfaceLineLoad} / "
            + $"{lastSurfaceResult.LineCapacity} | Depth: {lastSurfaceResult.SurfaceDepth} | {loadStatus}");
        summary.AppendLine($"Gold Awarded: {lastSurfaceResult.GoldAwarded} | Total Gold: {runRewardRuntime.TotalGold}");
        summary.Append("Successful Haul: ");

        if (lastSurfaceResult.Haul.Length == 0)
        {
            summary.Append("none");
            AppendRunRemovalSummary(summary);
            AppendOverloadRiskSummary(summary, strainLostCatches);
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
        AppendOverloadRiskSummary(summary, strainLostCatches);

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
    /// Appends the most recent overload check and any lost catches to an action summary.
    /// </summary>
    private void AppendOverloadRiskSummary(StringBuilder summary, CardInstance[] strainLostCatches)
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

        CardInstance[] safeLosses = strainLostCatches ?? Array.Empty<CardInstance>();
        summary.Append($"Line broke at {lineLoadRiskRuntime.LastBreakChance:P0} break chance; lost ");
        AppendCatchNames(summary, safeLosses);
        summary.AppendLine("; forced Surface");
    }
}
