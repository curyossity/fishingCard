using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Biome", menuName = "Fishing Cards/Biome Definition")]
public sealed class BiomeDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string biomeId;
    [SerializeField] private string displayName;
    [TextArea(2, 4)]
    [SerializeField] private string coreIdentity;
    [TextArea(2, 5)]
    [SerializeField] private string strategicTension;
    [SerializeField] private string[] signatureTags = Array.Empty<string>();

    [Header("Depth Structure")]
    [SerializeField] private BiomeDepthTierDefinition[] depthTiers = Array.Empty<BiomeDepthTierDefinition>();
    [SerializeField] private EncounterChainDefinition[] encounterChains = Array.Empty<EncounterChainDefinition>();

    [Header("Apex")]
    [SerializeField] private CardDefinition[] apexEncounters = Array.Empty<CardDefinition>();

    public string BiomeId => biomeId;
    public string DisplayName => displayName;
    public string CoreIdentity => coreIdentity;
    public string StrategicTension => strategicTension;
    public string[] SignatureTags => signatureTags;
    public BiomeDepthTierDefinition[] DepthTiers => depthTiers;
    public EncounterChainDefinition[] EncounterChains => encounterChains;
    public CardDefinition[] ApexEncounters => apexEncounters;

    /// <summary>
    /// Returns the authored tier containing a depth, or null when the biome has ended.
    /// </summary>
    public BiomeDepthTierDefinition GetDepthTier(int depth)
    {
        if (depthTiers == null)
        {
            return null;
        }

        for (int i = 0; i < depthTiers.Length; i++)
        {
            BiomeDepthTierDefinition tier = depthTiers[i];

            if (tier != null && tier.ContainsDepth(depth))
            {
                return tier;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the encounter subset authored for a depth, or an empty pool outside all tiers.
    /// </summary>
    public CardDefinition[] GetEncounterPool(int depth)
    {
        BiomeDepthTierDefinition tier = GetDepthTier(depth);
        return tier?.EncounterPool ?? Array.Empty<CardDefinition>();
    }

    /// <summary>
    /// Returns the first depth after all finite tiers, or -1 when no Apex boundary exists.
    /// </summary>
    public int GetApexBoundaryDepth()
    {
        if (depthTiers == null || depthTiers.Length == 0)
        {
            return -1;
        }

        int deepestTierDepth = -1;

        for (int i = 0; i < depthTiers.Length; i++)
        {
            BiomeDepthTierDefinition tier = depthTiers[i];

            if (tier == null)
            {
                continue;
            }

            // An unlimited tier has no later boundary at which an Apex can trigger.
            if (tier.MaximumDepth < 0)
            {
                return -1;
            }

            deepestTierDepth = Math.Max(deepestTierDepth, tier.MaximumDepth);
        }

        return deepestTierDepth < 0 ? -1 : deepestTierDepth + 1;
    }
}

[Serializable]
public sealed class BiomeDepthTierDefinition
{
    [SerializeField] private string tierId;
    [SerializeField] private string displayName;
    [Min(0)]
    [SerializeField] private int minimumDepth;
    [SerializeField] private int maximumDepth = -1;
    [TextArea(2, 5)]
    [SerializeField] private string designIntent;
    [SerializeField] private CardDefinition[] encounterPool = Array.Empty<CardDefinition>();

    public string TierId => tierId;
    public string DisplayName => displayName;
    public int MinimumDepth => minimumDepth;
    public int MaximumDepth => maximumDepth;
    public string DesignIntent => designIntent;
    public CardDefinition[] EncounterPool => encounterPool;

    /// <summary>
    /// Checks whether a run depth falls inside this tier's inclusive authored range.
    /// </summary>
    public bool ContainsDepth(int depth)
    {
        return depth >= minimumDepth && (maximumDepth < 0 || depth <= maximumDepth);
    }
}
