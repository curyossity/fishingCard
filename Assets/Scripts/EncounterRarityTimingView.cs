using System;
using UnityEngine;

/// <summary>Presentation boundary for the controller-owned creature timing phase.</summary>
public abstract class EncounterRarityTimingView : MonoBehaviour
{
    /// <summary>Displays one occurrence and forwards a token-checked Stop command.</summary>
    public abstract void Present(EncounterRarityTiming timing, Func<long, bool> stop, BiomeDefinition biome, int depth);
}
