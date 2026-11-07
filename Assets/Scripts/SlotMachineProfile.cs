using System;
using UnityEngine;

/// <summary>Editable provisional rarity and secondary-tag balancing; no live expedition state.</summary>
[CreateAssetMenu(menuName = "Fishing Cards/Slot Machine Profile")]
public sealed class SlotMachineProfile : ScriptableObject
{
    [Tooltip("Relative Common / Uncommon / Rare / Legendary weights. Provisional balancing values.")]
    [SerializeField] private int[] baselineRarityWeights = { 70, 22, 7, 1 };
    [SerializeField] private int[] pairRarityWeights = { 55, 28, 14, 3 };
    [SerializeField] private int[] tripleRarityWeights = { 35, 35, 23, 7 };
    [Min(2)] [SerializeField] private int secondaryTagWeight = 2;

    /// <summary>Freezes authored balancing for one expedition; invalid profiles cannot start a run.</summary>
    public SlotSelectionSettings CreateSettings()
    {
        return new SlotSelectionSettings(baselineRarityWeights, pairRarityWeights, tripleRarityWeights, secondaryTagWeight);
    }
}

/// <summary>Immutable expedition-owned selection settings, independently testable from Unity assets.</summary>
public sealed class SlotSelectionSettings
{
    private readonly int[][] rarityWeights;
    public int SecondaryTagWeight { get; }

    /// <summary>Copies positive-support rarity tables and verifies the pair/triple rarity improvements.</summary>
    public SlotSelectionSettings(int[] baseline, int[] pair, int[] triple, int secondaryWeight)
    {
        if (secondaryWeight < 2) throw new ArgumentOutOfRangeException(nameof(secondaryWeight));
        SecondaryTagWeight = secondaryWeight;
        rarityWeights = new[] { CopyWeights(baseline), CopyWeights(pair), CopyWeights(triple) };
        bool pairImproves = false, tripleImproves = false;
        for (int threshold = 1; threshold < 4; threshold++)
        {
            double baselineShare = UpperShare(rarityWeights[0], threshold);
            double pairShare = UpperShare(rarityWeights[1], threshold);
            double tripleShare = UpperShare(rarityWeights[2], threshold);
            if (pairShare < baselineShare || tripleShare < pairShare)
                throw new ArgumentException("Pair and triple tables must progressively improve higher-rarity probabilities.");
            pairImproves |= pairShare > baselineShare;
            tripleImproves |= tripleShare > pairShare;
        }
        if (!pairImproves || !tripleImproves) throw new ArgumentException("Each match table must improve at least one higher-rarity probability.");
    }

    /// <summary>Rolls the requested rarity before inspecting which tiers contain suitable creatures.</summary>
    public CardRarity RollRarity(SlotMatchKind kind, System.Random random)
    {
        int[] weights = rarityWeights[(int)kind];
        int total = 0;
        foreach (int weight in weights) total = checked(total + weight);
        int ticket = random.Next(total);
        for (int i = 0; i < weights.Length; i++)
        {
            ticket -= weights[i];
            if (ticket < 0) return (CardRarity)i;
        }
        throw new InvalidOperationException("Rarity weights did not resolve a ticket.");
    }

    /// <summary>Validates table shape and overflow before storing a defensive copy.</summary>
    private static int[] CopyWeights(int[] source)
    {
        if (source == null || source.Length != 4) throw new ArgumentException("Four rarity weights are required.");
        int total = 0;
        foreach (int weight in source)
        {
            if (weight < 0) throw new ArgumentException("Rarity weights cannot be negative.");
            total = checked(total + weight);
        }
        if (total == 0) throw new ArgumentException("A rarity table needs positive total weight.");
        return (int[])source.Clone();
    }

    /// <summary>Computes cumulative higher-tier probability for monotonic balancing validation.</summary>
    private static double UpperShare(int[] weights, int threshold)
    {
        double total = 0, upper = 0;
        for (int i = 0; i < weights.Length; i++) { total += weights[i]; if (i >= threshold) upper += weights[i]; }
        return upper / total;
    }
}

public enum SlotMatchKind { ThreeDifferent, Pair, Triple }

/// <summary>Tag-only interpretation of three faces; independent of rarity and creature selection.</summary>
public readonly struct SlotMatch
{
    public SlotMatchKind Kind { get; }
    public string GuaranteedTag { get; }
    public string PreferredTag { get; }

    /// <summary>Finds the repeated tag, preserving the unmatched face as a pair's secondary preference.</summary>
    public SlotMatch(TagSlotMachineRuntime.Result result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        Kind = SlotMatchKind.ThreeDifferent;
        GuaranteedTag = PreferredTag = null;
        foreach (string tag in result.Tags)
        {
            int count = result.Count(tag);
            if (count < 2) continue;
            Kind = count == 3 ? SlotMatchKind.Triple : SlotMatchKind.Pair;
            GuaranteedTag = tag;
            if (count == 2)
                foreach (string other in result.Tags)
                    if (!string.Equals(tag, other, StringComparison.OrdinalIgnoreCase)) PreferredTag = other;
            break;
        }
    }
}
