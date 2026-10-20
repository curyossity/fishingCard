/// <summary>Authored stat baselines and selected role, keyed to one revealed encounter occurrence.</summary>
public readonly struct EncounterStatFeedback
{
    public readonly int EncounterRevision;
    public readonly int BaselineWeight;
    public readonly int BaselineValue;
    public readonly bool IsBait;

    /// <summary>Includes every effect source while identifying Bait's intrinsic zero sale value as a neutral role change.</summary>
    public EncounterStatFeedback(int encounterRevision, int baselineWeight, int baselineValue, bool isBait = false)
    {
        EncounterRevision = encounterRevision;
        BaselineWeight = baselineWeight;
        BaselineValue = baselineValue;
        IsBait = isBait;
    }
}
