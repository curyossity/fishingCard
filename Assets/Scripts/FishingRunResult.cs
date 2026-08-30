using System;
using UnityEngine;

[Serializable]
public sealed class FishingRunResult
{
    [SerializeField] private CardDefinition[] haul = Array.Empty<CardDefinition>();
    [SerializeField] private int haulValue;
    [SerializeField] private int surfaceDepth;
    [SerializeField] private int surfaceLineLoad;
    [SerializeField] private int lineCapacity;
    [SerializeField] private bool wasOverloaded;

    public CardDefinition[] Haul => haul;
    public int HaulValue => haulValue;
    public int SurfaceDepth => surfaceDepth;
    public int SurfaceLineLoad => surfaceLineLoad;
    public int LineCapacity => lineCapacity;
    public bool WasOverloaded => wasOverloaded;

    /// <summary>
    /// Records the attached catches and run values present when Surface begins.
    /// </summary>
    public void Record(CardDefinition[] successfulHaul, int depth, int load, int capacity)
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
        haul = Array.Empty<CardDefinition>();
        haulValue = 0;
        surfaceDepth = 0;
        surfaceLineLoad = 0;
        lineCapacity = 0;
        wasOverloaded = false;
    }

    /// <summary>
    /// Copies card references into a separate result array.
    /// </summary>
    private static CardDefinition[] CopyCards(CardDefinition[] cards)
    {
        CardDefinition[] source = cards ?? Array.Empty<CardDefinition>();
        CardDefinition[] result = new CardDefinition[source.Length];

        for (int i = 0; i < source.Length; i++)
        {
            result[i] = source[i];
        }

        return result;
    }

    /// <summary>
    /// Calculates the total base value of all non-null haul cards.
    /// </summary>
    private static int CalculateValue(CardDefinition[] cards)
    {
        int totalValue = 0;

        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] != null)
            {
                totalValue += cards[i].Value;
            }
        }

        return totalValue;
    }
}
