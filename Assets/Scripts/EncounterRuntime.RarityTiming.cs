using System.Collections.Generic;

public sealed partial class EncounterRuntime
{
    public RarityTimingProfile TimingProfile { get; set; }
    public EncounterRarityTiming RarityTiming { get; } = new EncounterRarityTiming();
    private EncounterChainDefinition[] timingChains;
    private bool timingChainFollowUp;

    /// <summary>Reserves a creature slot without reveal reactions, concealment consumption or history.</summary>
    private bool TryDeferCreature(CardDefinition selected, List<CardDefinition> cards,
        EncounterChainDefinition[] chains, bool chainFollowUp = false)
    {
        if (TimingProfile == null || selected == null
            || (selected.CardType != CardType.Creature && selected.CardType != CardType.ApexEncounter)) return false;
        var eligible = new List<CardDefinition>();
        if (cards == null) eligible.Add(selected);
        else for (int i = 0; i < cards.Count; i++)
            if (cards[i].CardType == CardType.Creature) eligible.Add(cards[i]);
        SetCurrentEncounter(null);
        timingChains = chains; timingChainFollowUp = chainFollowUp;
        RarityTiming.Begin(eligible.ToArray(), TimingProfile, cards == null);
        return true;
    }

    /// <summary>Commits the stopped selection through the normal reveal and reaction path.</summary>
    public bool CompleteRarityTiming(System.Random random)
    {
        if (!RarityTiming.ReadyToReveal) return false;
        CardDefinition selected = RarityTiming.Choose(random);
        lastSelectedEncounterWeight = selected != null ? 1 : 0;
        lastTotalEncounterWeight = RarityTiming.EligibleSpeciesCount;
        lastCandidateWeightSummary = selected != null
            ? "Uniform creature draw up to " + RarityTiming.Outcome.Label + ": " + lastTotalEncounterWeight + " species"
            : "Miss: no creature selected";
        RarityTiming.Reset();
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
}
