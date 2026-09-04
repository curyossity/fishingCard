using System;
using UnityEngine;

[Serializable]
public sealed class FishingRunResult
{
    [SerializeField] private CardInstance[] haul = Array.Empty<CardInstance>();
    [SerializeField] private CardInstance[] releasedCatches = Array.Empty<CardInstance>();
    [SerializeField] private CardInstance[] lostCatches = Array.Empty<CardInstance>();
    [SerializeField] private int haulValue;
    [SerializeField] private int goldAwarded;
    [SerializeField] private int surfaceDepth;
    [SerializeField] private int surfaceLineLoad;
    [SerializeField] private int lineCapacity;
    [SerializeField] private bool wasOverloaded;
    [SerializeField] private bool hasResult;

    public CardInstance[] Haul => haul;
    public CardInstance[] ReleasedCatches => releasedCatches;
    public CardInstance[] LostCatches => lostCatches;
    public int HaulValue => haulValue;
    public int GoldAwarded => goldAwarded;
    public int SurfaceDepth => surfaceDepth;
    public int SurfaceLineLoad => surfaceLineLoad;
    public int LineCapacity => lineCapacity;
    public bool WasOverloaded => wasOverloaded;
    public bool HasResult => hasResult;

    /// <summary>
    /// Records the surviving haul, removal histories, and run values when Surface resolves.
    /// </summary>
    public void Record(
        CardInstance[] successfulHaul,
        CardInstance[] releasedDuringRun,
        CardInstance[] lostDuringRun,
        int depth,
        int load,
        int capacity)
    {
        haul = CopyCards(successfulHaul);
        releasedCatches = CopyCards(releasedDuringRun);
        lostCatches = CopyCards(lostDuringRun);
        haulValue = CalculateValue(haul);
        surfaceDepth = depth;
        surfaceLineLoad = load;
        lineCapacity = capacity;
        wasOverloaded = surfaceLineLoad > lineCapacity;
        hasResult = true;
    }

    /// <summary>
    /// Stores the currency granted for this completed Surface result.
    /// </summary>
    public void RecordGoldAward(int amount)
    {
        goldAwarded = Mathf.Max(0, amount);
    }

    /// <summary>
    /// Clears the previous Surface result when a new run begins.
    /// </summary>
    public void Reset()
    {
        haul = Array.Empty<CardInstance>();
        releasedCatches = Array.Empty<CardInstance>();
        lostCatches = Array.Empty<CardInstance>();
        haulValue = 0;
        goldAwarded = 0;
        surfaceDepth = 0;
        surfaceLineLoad = 0;
        lineCapacity = 0;
        wasOverloaded = false;
        hasResult = false;
    }

    /// <summary>
    /// Copies catch instances into a separate result snapshot.
    /// </summary>
    private static CardInstance[] CopyCards(CardInstance[] cards)
    {
        CardInstance[] source = cards ?? Array.Empty<CardInstance>();
        CardInstance[] result = new CardInstance[source.Length];

        for (int i = 0; i < source.Length; i++)
        {
            result[i] = source[i]?.CreateSnapshot();
        }

        return result;
    }

    /// <summary>
    /// Calculates the total resolved value of all non-null haul catches.
    /// </summary>
    private static int CalculateValue(CardInstance[] cards)
    {
        int totalValue = 0;

        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] != null)
            {
                totalValue += cards[i].CurrentValue;
            }
        }

        return totalValue;
    }
}
