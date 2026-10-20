/// <summary>Presentation baseline resolved without Catch Chain influences, keyed to one revealed encounter occurrence.</summary>
public readonly struct EncounterStatFeedback
{
    public readonly int EncounterRevision;
    public readonly int BaselineWeight;
    public readonly int BaselineValue;

    /// <summary>Preserves non-chain modifiers in the baseline so role choices and Techniques do not masquerade as chain effects.</summary>
    public EncounterStatFeedback(int encounterRevision, int baselineWeight, int baselineValue)
    {
        EncounterRevision = encounterRevision;
        BaselineWeight = baselineWeight;
        BaselineValue = baselineValue;
    }
}
