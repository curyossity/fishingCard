using System;
using UnityEngine;

/// <summary>Owns one biome-local promised creature slot independently of authored encounter chains.</summary>
[Serializable]
public sealed class EventEncounterTrailRuntime
{
    [SerializeField] private CardDefinition creature;
    [SerializeField] private BiomeDefinition biome;
    [SerializeField] private int destinationDepth;
    [SerializeField] private CardDefinition lastPromisedCreature;
    [SerializeField] private int lastConsumedDepth;
    [SerializeField] private bool lastReplaced;
    public bool HasPending => creature != null;
    public CardDefinition Creature => creature;
    public int DestinationDepth => destinationDepth;
    public string Preview => HasPending
        ? creature.DisplayName + " at depth " + destinationDepth + " | Weight " + creature.Weight + " | Value " + creature.Value
        : lastPromisedCreature == null ? string.Empty
        : lastPromisedCreature.DisplayName + " trail at depth " + lastConsumedDepth + (lastReplaced ? ": replaced" : ": reached")
            + " | Weight " + lastPromisedCreature.Weight + " | Value " + lastPromisedCreature.Value;

    /// <summary>Requires an ordinary slot and an eligible species in that slot's pool.</summary>
    public static bool IsLegalSlot(BiomeDefinition biome, int depth, CardDefinition creature)
    {
        int boundary = biome?.GetApexBoundaryDepth() ?? -1;
        return biome != null && depth >= 0 && (boundary < 0 || depth < boundary)
            && biome.GetDepthTier(depth) != null && creature?.CardType == CardType.Creature
            && creature.IsAvailableInBiome(biome.BiomeId) && creature.IsAvailableAtDepth(depth)
            && Array.IndexOf(biome.GetEncounterPool(depth), creature) >= 0;
    }

    /// <summary>Creates at most one promise; duplicate or invalid forced events do nothing.</summary>
    public bool TrySchedule(BiomeDefinition currentBiome, int currentDepth, CardDefinition promised, int distance)
    {
        if (HasPending || distance <= 0 || currentDepth > int.MaxValue - distance
            || !IsLegalSlot(currentBiome, currentDepth + distance, promised)) return false;
        biome = currentBiome; creature = promised; destinationDepth = currentDepth + distance;
        lastPromisedCreature = null; return true;
    }

    /// <summary>Consumes a due normal slot once, including when a queued choice takes precedence.</summary>
    public CardDefinition TakeForSlot(BiomeDefinition currentBiome, int depth, int selectionDepth, bool replaced)
    {
        lastPromisedCreature = null;
        if (!HasPending) return null;
        if (biome != currentBiome) { Reset(); return null; }
        if (depth < destinationDepth && selectionDepth < destinationDepth) return null;
        if (!IsLegalSlot(biome, selectionDepth, creature))
        {
            int passed = Math.Max(depth, selectionDepth);
            if (passed == int.MaxValue) { Reset(); return null; }
            int next = FindNextLegalSlot(passed + 1);
            if (next < 0) { Reset(); return null; }
            destinationDepth = next;
            return null;
        }
        destinationDepth = Math.Max(depth, selectionDepth);
        lastPromisedCreature = creature; lastConsumedDepth = destinationDepth; lastReplaced = replaced;
        CardDefinition selected = creature; creature = null;
        return replaced ? null : selected;
    }

    /// <summary>Finds the first remaining eligible interval without scanning an unbounded biome depth by depth.</summary>
    private int FindNextLegalSlot(int start)
    {
        System.Collections.Generic.SortedSet<int> candidates = new System.Collections.Generic.SortedSet<int>();
        candidates.Add(start);
        candidates.Add(Math.Max(start, creature.MinimumDepth));
        foreach (BiomeDepthTierDefinition tier in biome.DepthTiers ?? Array.Empty<BiomeDepthTierDefinition>())
        {
            if (tier == null) continue;
            if (tier.MinimumDepth >= start) candidates.Add(tier.MinimumDepth);
            if (tier.MaximumDepth >= start && tier.MaximumDepth < int.MaxValue) candidates.Add(tier.MaximumDepth + 1);
        }
        foreach (int candidate in candidates)
            if (IsLegalSlot(biome, candidate, creature)) return candidate;
        return -1;
    }

    /// <summary>Marks a consumed promised encounter as replaced or avoided without recreating the trail.</summary>
    public void MarkReplaced(CardDefinition current)
    {
        if (lastPromisedCreature != null && current == lastPromisedCreature) lastReplaced = true;
    }

    /// <summary>Clears promises and preview presentation on biome or run ending.</summary>
    public void Reset() { creature = null; biome = null; lastPromisedCreature = null; destinationDepth = 0; }
}
