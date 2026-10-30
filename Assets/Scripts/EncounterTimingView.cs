using System;
using UnityEngine;

/// <summary>Presentation boundary for the controller-owned creature timing phase.</summary>
public abstract class EncounterTimingView : MonoBehaviour
{
    /// <summary>Allows an optional occurrence-bound entrance to finish before marker motion and Stop input.</summary>
    public virtual bool HasCompletedEntrance(long occurrence) { return true; }

    /// <summary>Allows an optional occurrence-bound visual exit to finish before the creature is revealed.</summary>
    public virtual bool HasCompletedExit(long occurrence) { return true; }

    /// <summary>Displays one occurrence and forwards a token-checked Stop command.</summary>
    public abstract void Present(EncounterTiming timing, Func<long, bool> stop, BiomeDefinition biome, int depth);
}
