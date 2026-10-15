using System;
using UnityEngine;

/// <summary>Specifies an explicit authored lifetime without assuming encounter or biome expiry rules.</summary>
[Serializable]
public sealed class EventEffectLifetime
{
    public const string BiomeEnd = "BiomeEnd";
    [SerializeField] private string boundaryId;
    [SerializeField] private int boundaryCount;
    public string BoundaryId => boundaryId;
    public int BoundaryCount => boundaryCount;
    public bool HasBoundary => !string.IsNullOrEmpty(boundaryId);

    /// <summary>Creates a lifetime that remains until run cleanup or an applied target leaves the chain.</summary>
    public static EventEffectLifetime UntilRunEnds() { return new EventEffectLifetime(); }

    /// <summary>Creates a lifetime counted at a specifically authored boundary emitted by runtime coordination.</summary>
    public static EventEffectLifetime AtBoundary(string id, int count)
    {
        if (string.IsNullOrWhiteSpace(id) || count <= 0) throw new ArgumentException("A named boundary and positive count are required.");
        return new EventEffectLifetime { boundaryId = id, boundaryCount = count };
    }
}

/// <summary>Read-only public view of one pending or applied event effect; the persistence owner controls its lifetime.</summary>
[Serializable]
public sealed class PersistentEventEffect
{
    [SerializeField] private long id;
    [SerializeField] private long sourceOccurrenceId;
    [SerializeField] private CardDefinition source;
    [SerializeField] private CardEffectDefinition effect;
    [SerializeField] private string boundaryId;
    [SerializeField] private int remainingBoundaries;
    [SerializeField] private int targetInstanceId = -1;
    public long Id => id;
    public long SourceOccurrenceId => sourceOccurrenceId;
    public CardDefinition Source => source;
    public CardEffectDefinition Effect => effect;
    public string BoundaryId => boundaryId;
    public int RemainingBoundaries => remainingBoundaries;
    public int TargetInstanceId => targetInstanceId;
    public bool IsPending => targetInstanceId < 0;

    /// <summary>Copies lifetime data into an independent occurrence instead of mutating a shared descriptor.</summary>
    internal PersistentEventEffect(long id, long origin, CardDefinition source, CardEffectDefinition effect, EventEffectLifetime lifetime)
    {
        this.id = id; sourceOccurrenceId = origin; this.source = source; this.effect = effect;
        boundaryId = lifetime.BoundaryId; remainingBoundaries = lifetime.BoundaryCount;
    }

    /// <summary>Applies pending ownership to a target without restarting its boundary count.</summary>
    internal void AttachTo(int instanceId) { targetInstanceId = instanceId; }

    /// <summary>Consumes one matching authored boundary and reports expiry.</summary>
    internal bool ObserveBoundary(string id)
    {
        if (string.IsNullOrEmpty(boundaryId) || boundaryId != id) return false;
        remainingBoundaries--; return remainingBoundaries <= 0;
    }
}
