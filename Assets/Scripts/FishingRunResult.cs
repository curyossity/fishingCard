using System;
using UnityEngine;

[Serializable]
public sealed class FishingRunResult
{
    [SerializeField] private CardInstance[] haul = Array.Empty<CardInstance>();
    [SerializeField] private int haulValue;
    [SerializeField] private int surfaceDepth;
    [SerializeField] private int surfaceLineLoad;
    [SerializeField] private int lineCapacity;
    [SerializeField] private bool wasOverloaded;

    public CardInstance[] Haul => haul;
    public int HaulValue => haulValue;
    public int SurfaceDepth => surfaceDepth;
    public int SurfaceLineLoad => surfaceLineLoad;
    public int LineCapacity => lineCapacity;
    public bool WasOverloaded => wasOverloaded;

    /// <summary>
    /// Records the attached catches and run values present when Surface begins.
    /// </summary>
    public void Record(CardInstance[] successfulHaul, int depth, int load, int capacity)
    {
        haul = CopyCards(successfulHaul);
        haulValue = CalculateValue(haul);
        surfaceDepth = depth;
        surfaceLineLoad = load;
        lineCapacity = capacity;
        wasOverloaded = surfaceLineLoad > lineCapacity;
    }

    /// <summary>
    /// Clears the previous Surface result when a new run begins.
    /// </summary>
    public void Reset()
    {
        haul = Array.Empty<CardInstance>();
        haulValue = 0;
        surfaceDepth = 0;
        surfaceLineLoad = 0;
        lineCapacity = 0;
        wasOverloaded = false;
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
