using System;

/// <summary>Frozen acquisition information for the brief upgrade display, independent of the next encounter.</summary>
public sealed class RigAcquisitionReceipt
{
    public long Occurrence { get; }
    public CardInstance Member { get; }
    public CardInstance[] Members { get; }
    public int PreviousLevel { get; }
    public int PreviousHaul { get; }
    public int Haul { get; }
    public string BiomeName { get; }

    /// <summary>Copies the acquired rig so refreshes and later encounters cannot change the displayed result.</summary>
    public RigAcquisitionReceipt(long occurrence, CardInstance member, CardInstance[] members, int previousLevel, int previousHaul, int haul, string biomeName)
    {
        Occurrence = occurrence; Member = member.CreateSnapshot(); PreviousLevel = previousLevel;
        PreviousHaul = previousHaul; Haul = haul; BiomeName = biomeName;
        Members = Array.ConvertAll(members, item => item.CreateSnapshot());
    }
}
