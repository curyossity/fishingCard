using System;
using System.Collections.Generic;

public sealed partial class EncounterRuntime
{
    public TagSlotMachineRuntime SlotMachine { get; } = new TagSlotMachineRuntime();
    public bool AwaitingLandingChallenge { get; private set; }
    public SlotCreatureSelector.Selection LastSlotSelection { get; private set; }
    public string SlotFailure { get; private set; }
    private CardDefinition[] slotCandidates = Array.Empty<CardDefinition>();
    private SlotSelectionSettings slotSettings;
    private string slotBiome;
    private Func<CardDefinition, bool> slotUnlocked;

    /// <summary>Validates four canonical active tags without selecting replacements on the player's behalf.</summary>
    public static bool TryValidateActiveTags(string[] requested, out string[] canonical)
    {
        canonical = CreatureTagVocabulary.Normalize(requested);
        return requested != null && requested.Length == 4 && canonical.Length == 4;
    }

    /// <summary>Reserves the biome-wide unlocked creature pool and performs the three independent initial rolls.</summary>
    public bool BeginSlotEncounter(CardDefinition[] pool, string biomeId, string[] activeTags,
        SlotSelectionSettings settings, System.Random random, Func<CardDefinition, bool> unlocked)
    {
        EnsureEncounterCanBeRevealed();
        if (settings == null || random == null || !TryValidateActiveTags(activeTags, out string[] canonical))
            throw new ArgumentException("Discover requires settings, randomness and four distinct available tags.");
        CardDefinition[] eligible = SlotCreatureSelector.EligiblePool(pool, biomeId, unlocked);
        foreach (string tag in canonical)
            if (!Array.Exists(eligible, card => SlotCreatureSelector.HasTag(card, tag)))
            {
                SlotFailure = "No eligible " + tag + " creature in this biome. Check biome content and unlocks.";
                return false;
            }
        SetCurrentEncounter(null);
        CancelActiveEncounterChain();
        TagTiming.ResetRun(); RarityTiming.Reset();
        AwaitingLandingChallenge = false;
        SlotFailure = null;
        LastSlotSelection = default;
        slotCandidates = eligible;
        slotSettings = settings;
        slotBiome = biomeId;
        slotUnlocked = unlocked;
        var symbols = new TagSlotMachineRuntime.Symbol[4];
        for (int i = 0; i < 4; i++) symbols[i] = new TagSlotMachineRuntime.Symbol(canonical[i], 1);
        SlotMachine.Begin(new IReadOnlyList<TagSlotMachineRuntime.Symbol>[] { symbols, symbols, symbols }, 1);
        SlotMachine.TrySpin(SlotMachine.Revision, random);
        return true;
    }

    /// <summary>Commits the initial accepted or final respun result once, then exposes a pending landing encounter.</summary>
    public bool CompleteSlotEncounter(long revision, System.Random random)
    {
        if (SlotMachine.Revision != revision || !SlotMachine.CanAccept) return false;
        var tags = new[] { SlotMachine.GetTag(0), SlotMachine.GetTag(1), SlotMachine.GetTag(2) };
        var snapshot = new TagSlotMachineRuntime.Result(SlotMachine.Occurrence, tags);
        CardDefinition[] eligible = SlotCreatureSelector.EligiblePool(slotCandidates, slotBiome, slotUnlocked);
        if (!SlotCreatureSelector.TrySelect(eligible, new SlotMatch(snapshot), slotSettings, random,
            out SlotCreatureSelector.Selection selection, out string failure))
        { SlotFailure = failure; return false; }
        if (!SlotMachine.TryAccept(revision, out _)) return false;
        LastSlotSelection = selection;
        SlotFailure = null;
        EnsureEncounterVarietyRuntime();
        encounterVarietyRuntime.BeginSelection();
        encounterVarietyRuntime.RecordSelectedEncounter(selection.Creature);
        lastSelectedEncounterWeight = 1;
        lastTotalEncounterWeight = eligible.Length;
        lastCandidateWeightSummary = "Slot " + selection.Match.Kind + ": " + selection.Creature.DisplayName
            + "; requested " + selection.RequestedRarity + ", resolved " + selection.ResolvedRarity;
        // Mark the gate before reveal callbacks: selection never authorizes acquiring this creature.
        AwaitingLandingChallenge = true;
        activeRevealCallbackDepth++;
        try { ApplyRevealedEncounter(selection.Creature, true); }
        finally { activeRevealCallbackDepth--; }
        return true;
    }

    /// <summary>Discards all per-encounter selection ownership and invalidates callbacks at expedition/biome reset.</summary>
    private void ResetSlotMachine()
    {
        SlotMachine.Reset();
        slotCandidates = Array.Empty<CardDefinition>();
        slotSettings = null;
        slotUnlocked = null;
        slotBiome = null;
        SlotFailure = null;
        AwaitingLandingChallenge = false;
        LastSlotSelection = default;
    }
}
