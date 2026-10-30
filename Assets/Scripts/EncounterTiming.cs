using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Shared hidden-encounter clock, occurrence identity and stopped-feedback lifecycle.</summary>
public abstract class EncounterTiming
{
    protected CardDefinition[] Candidates { get; private set; } = Array.Empty<CardDefinition>();
    private float elapsed, speed, hold, stoppedElapsed;
    private bool stopped;
    private long occurrence;
    public bool Active => Candidates.Length > 0;
    public bool Stopped => Active && stopped;
    public bool ReadyToReveal => Stopped && stoppedElapsed >= hold;
    public long Occurrence => occurrence;
    public float Position => Mathf.PingPong(elapsed * speed, 1);
    public float FeedbackProgress => hold <= 0 ? 1 : Mathf.Clamp01(stoppedElapsed / hold);
    public abstract int EligibleSpeciesCount { get; }
    public abstract string ResultLabel { get; }
    public abstract bool CelebrateStop { get; }

    /// <summary>Reserves distinct species and resets phase state without resetting run-owned tag balance.</summary>
    protected void Reserve(CardDefinition[] cards, float traversalsPerSecond, float resultHoldSeconds)
    {
        Reset();
        var unique = new List<CardDefinition>();
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (CardDefinition card in cards ?? Array.Empty<CardDefinition>())
            if (card != null && !unique.Contains(card)
                && (string.IsNullOrEmpty(card.UniqueId) || ids.Add(card.UniqueId))) unique.Add(card);
        Candidates = unique.ToArray();
        speed = Mathf.Max(.05f, traversalsPerSecond);
        hold = Mathf.Max(0, resultHoldSeconds);
    }

    /// <summary>Advances only the marker or stopped hold; reveal and visual travel are separately gated.</summary>
    public bool Tick(float delta)
    {
        if (!Active) return false;
        if (!stopped) elapsed += Mathf.Max(0, delta); else stoppedElapsed += Mathf.Max(0, delta);
        return ReadyToReveal;
    }

    /// <summary>Captures one exact stop and rejects repeated or stale occurrence callbacks.</summary>
    public bool TryStop(long expectedOccurrence)
    {
        if (!Active || stopped || occurrence != expectedOccurrence) return false;
        CaptureStop(Position);
        stopped = true;
        return true;
    }

    /// <summary>Lets a concrete timing rule capture its outcome without presentation-owned decisions.</summary>
    protected abstract void CaptureStop(float position);

    /// <summary>Samples the reserved pool under the captured outcome's selection rule.</summary>
    public abstract CardDefinition Choose(System.Random random);

    /// <summary>Invalidates phase callbacks while retaining separately owned per-run state.</summary>
    public virtual void Reset()
    {
        occurrence++;
        Candidates = Array.Empty<CardDefinition>();
        elapsed = stoppedElapsed = 0;
        stopped = false;
    }
}
