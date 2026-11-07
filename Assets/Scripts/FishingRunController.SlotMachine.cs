using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class FishingRunController
{
    [Header("Discover Slot Machine")]
    [SerializeField] private SlotMachineProfile slotMachineProfile;
    [SerializeField] private string[] preparedSlotTags = Array.Empty<string>();
    private string[] expeditionSlotTags = Array.Empty<string>();
    private SlotSelectionSettings expeditionSlotSettings;
    private bool slotCommandInProgress;
    private string slotPreparationFailure;
    public bool UsesSlotMachine => slotMachineProfile != null;
    public TagSlotMachineRuntime SlotMachine => encounterRuntime.SlotMachine;
    public bool AwaitingLandingChallenge => encounterRuntime.AwaitingLandingChallenge;
    public string SlotFailure => encounterRuntime.SlotFailure ?? slotPreparationFailure;
    public IReadOnlyList<string> PreparedSlotTags => Array.AsReadOnly(preparedSlotTags);
    public IReadOnlyList<string> ExpeditionSlotTags => Array.AsReadOnly(expeditionSlotTags);
    public bool HasPreparedSlotTags => EncounterRuntime.TryValidateActiveTags(preparedSlotTags, out _);
    public event Action<CardDefinition, int> CreatureReadyForLanding;
    // Progression supplies unlock evidence; an authored requirement is never granted by default.
    public Func<string, bool> CreatureUnlockIsGranted { get; set; }

    /// <summary>Changes the Harbor's four-tag preparation only while no expedition owns its frozen snapshot.</summary>
    public bool TryPrepareSlotTags(string[] requested)
    {
        if (runActive || !EncounterRuntime.TryValidateActiveTags(requested, out string[] canonical)) return false;
        preparedSlotTags = canonical;
        slotPreparationFailure = null;
        return true;
    }

    /// <summary>Freezes four player-selected symbols and editable rarity tuning before any run state is cleared.</summary>
    private bool PrepareSlotExpedition()
    {
        if (!UsesSlotMachine) return true;
        if (runActive) return false;
        if (!EncounterRuntime.TryValidateActiveTags(preparedSlotTags, out string[] canonical))
        { slotPreparationFailure = "Select four active tags in the Harbor before starting."; return false; }
        try { expeditionSlotSettings = slotMachineProfile.CreateSettings(); }
        catch (ArgumentException error) { slotPreparationFailure = error.Message; return false; }
        catch (OverflowException error) { slotPreparationFailure = error.Message; return false; }
        expeditionSlotTags = canonical;
        CardDefinition[] eligible = SlotCreatureSelector.EligiblePool(startingBiome?.GetStandardCreaturePool(),
            startingBiome?.BiomeId, IsSlotCreatureUnlocked);
        foreach (string tag in canonical)
            if (!Array.Exists(eligible, card => SlotCreatureSelector.HasTag(card, tag)))
            { slotPreparationFailure = "No eligible " + tag + " creature is available in the starting biome."; return false; }
        slotPreparationFailure = null;
        return true;
    }

    /// <summary>Preserves explicit creature unlock requirements independently of matching or rarity fallback.</summary>
    private bool IsSlotCreatureUnlocked(CardDefinition card)
    {
        return string.IsNullOrWhiteSpace(card.RequiredCreatureUnlockId)
            || CreatureUnlockIsGranted?.Invoke(card.RequiredCreatureUnlockId) == true;
    }

    /// <summary>Starts standard Discover from all normal biome creatures; special Apex conditions retain their own path.</summary>
    private void PrepareSlotEncounterAtCurrentDepth()
    {
        creatureEffectRuntime.BeginEncounter();
        runEquipment.BeginEncounter();
        int depth = GetEffectiveEncounterSelectionDepth();
        if (TryRevealBiomeApex(depth) || TryPresentNextWaters()) return;
        if (encounterRuntime.BeginSlotEncounter(currentBiome?.GetStandardCreaturePool(), CurrentBiomeId,
            expeditionSlotTags, expeditionSlotSettings, random, IsSlotCreatureUnlocked)) return;
        Debug.LogWarning(encounterRuntime.SlotFailure, this);
    }

    /// <summary>Changes one reel lock while rejecting stale UI, transition input and attempts to lock two reels.</summary>
    public bool TrySetSlotReelLocked(long revision, int reel, bool value)
    {
        if (!CoreActionInputReady || slotCommandInProgress || !SlotMachine.TrySetLocked(revision, reel, value)) return false;
        RefreshViews();
        return true;
    }

    /// <summary>Uses the single optional respin and leaves its fixed result visible until the player accepts.</summary>
    public bool TryRespinSlotMachine(long revision)
    {
        if (!CoreActionInputReady || slotCommandInProgress || !SlotMachine.HasSpun) return false;
        slotCommandInProgress = true;
        try
        {
            return SlotMachine.TrySpin(revision, random);
        }
        finally { slotCommandInProgress = false; RefreshViews(); }
    }

    /// <summary>Accepts either the initial result or the fixed respin result and reveals the creature once.</summary>
    public bool TryAcceptSlotMachine(long revision)
    {
        if (!CoreActionInputReady || slotCommandInProgress) return false;
        slotCommandInProgress = true;
        try { return CompleteSlotSelection(revision); }
        finally { slotCommandInProgress = false; RefreshViews(); }
    }

    /// <summary>Publishes a landing handoff without catching a creature, spending an attempt or awarding points.</summary>
    private bool CompleteSlotSelection(long revision)
    {
        if (!encounterRuntime.CompleteSlotEncounter(revision, random)) return false;
        techniqueEffectRuntime.CompleteEncounterReveal();
        CreatureReadyForLanding?.Invoke(CurrentEncounter, CurrentEncounterRevision);
        return true;
    }
}
