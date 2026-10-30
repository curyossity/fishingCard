using System.Collections.Generic;

public sealed partial class EncounterRuntime
{
    public RarityTimingProfile TimingProfile { get; set; }
    public TagTimingProfile TagTimingProfile { get; set; }
    public EncounterRarityTiming RarityTiming { get; } = new EncounterRarityTiming();
    public EncounterTagTiming TagTiming { get; } = new EncounterTagTiming();
    public EncounterTiming Timing => TagTiming.Active ? (EncounterTiming)TagTiming : RarityTiming;
    private EncounterChainDefinition[] timingChains;
    private bool timingChainFollowUp;

    /// <summary>Reserves a creature slot without reveal reactions, concealment consumption or history.</summary>
    private bool TryDeferCreature(CardDefinition selected, List<CardDefinition> cards,
        EncounterChainDefinition[] chains, bool chainFollowUp = false)
    {
        if ((TimingProfile == null && TagTimingProfile == null) || selected == null
            || (selected.CardType != CardType.Creature && selected.CardType != CardType.ApexEncounter)) return false;
        var eligible = new List<CardDefinition>();
        if (cards == null) eligible.Add(selected);
        else for (int i = 0; i < cards.Count; i++)
            if (cards[i].CardType == CardType.Creature) eligible.Add(cards[i]);
        SetCurrentEncounter(null);
        timingChains = chains; timingChainFollowUp = chainFollowUp;
        if (TagTimingProfile != null)
        { RarityTiming.Reset(); TagTiming.Begin(eligible.ToArray(), TagTimingProfile, cards == null); }
        else
        { TagTiming.Reset(); RarityTiming.Begin(eligible.ToArray(), TimingProfile, cards == null); }
        return true;
    }

    /// <summary>Commits the stopped selection through the normal reveal and reaction path.</summary>
    public bool CompleteTiming(System.Random random)
    {
        EncounterTiming timing = Timing;
        if (!timing.ReadyToReveal) return false;
        CardDefinition selected = timing.Choose(random);
        lastSelectedEncounterWeight = selected != null ? 1 : 0;
        lastTotalEncounterWeight = timing.EligibleSpeciesCount;
        lastCandidateWeightSummary = selected != null
            ? (timing is EncounterTagTiming ? "Tag draw " : "Uniform creature draw up to ") + timing.ResultLabel + ": " + lastTotalEncounterWeight + " species"
            : "Miss: no creature selected";
        timing.Reset();
        if (selected != null)
        {
            if (timingChainFollowUp) encounterVarietyRuntime.RecordAuthoredChainFollowUp();
            encounterVarietyRuntime.RecordSelectedEncounter(selected);
            BeginEncounterChain(selected, timingChains);
            SetCurrentEncounter(selected);
        }
        timingChains = null; timingChainFollowUp = false;
        return true;
    }

    /// <summary>Retains the legacy rarity fixture entry point while both modes share reveal commitment.</summary>
    public bool CompleteRarityTiming(System.Random random) { return CompleteTiming(random); }
}
