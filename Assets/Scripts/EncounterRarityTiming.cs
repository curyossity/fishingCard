using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Owns a hidden encounter's eligibility, marker and exact stopped outcome.</summary>
public sealed class EncounterRarityTiming
{
    public readonly struct Zone
    {
        public readonly CardRarity Rarity;
        public readonly bool IsMiss;
        public readonly float Start, End;
        public string Label => IsMiss ? "Miss" : Rarity.ToString();
        /// <summary>Defines one normalized interval and its outcome.</summary>
        public Zone(CardRarity rarity, bool miss, float start, float end)
        { Rarity = rarity; IsMiss = miss; Start = start; End = end; }
    }
    private CardDefinition[] candidates = Array.Empty<CardDefinition>();
    private Zone[] zones = Array.Empty<Zone>();
    private float elapsed, speed, hold, stoppedElapsed;
    private bool stopped;
    private long occurrence;
    public bool Active => candidates.Length > 0;
    public bool Stopped => Active && stopped;
    public bool ReadyToReveal => Stopped && stoppedElapsed >= hold;
    public long Occurrence => occurrence;
    public float Position => Mathf.PingPong(elapsed * speed, 1);
    public float FeedbackProgress => hold <= 0 ? 1 : Mathf.Clamp01(stoppedElapsed / hold);
    public IReadOnlyList<Zone> Zones => zones;
    public Zone Outcome { get; private set; }
    /// <summary>Counts equal species entries under the locked ceiling for authoritative draw diagnostics.</summary>
    public int EligibleSpeciesCount
    {
        get
        {
            if (!Stopped || Outcome.IsMiss) return 0;
            int count=0;
            foreach (CardDefinition card in candidates) if ((int)card.Rarity <= (int)Outcome.Rarity) count++;
            return count;
        }
    }

    /// <summary>Gives unavailable rarity space to Common where possible; fixed promises keep their identity.</summary>
    public void Begin(CardDefinition[] cards, RarityTimingProfile profile, bool fixedEncounter)
    {
        Reset();
        var unique = new List<CardDefinition>(); var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (CardDefinition card in cards ?? Array.Empty<CardDefinition>())
            if (card != null && !unique.Contains(card)
                && (string.IsNullOrEmpty(card.UniqueId) || ids.Add(card.UniqueId))) unique.Add(card);
        candidates = unique.ToArray();
        speed = Mathf.Max(.05f, profile.traversalsPerSecond); hold = Mathf.Max(0, profile.resultHoldSeconds);
        var available = new List<CardRarity>(); var widths = new List<float>(); float total = 0, unavailableWidth = 0;
        foreach (CardRarity rarity in Enum.GetValues(typeof(CardRarity)))
        {
            float width = fixedEncounter ? 1 : Mathf.Max(0, profile.Width(rarity));
            if (!Array.Exists(candidates, c => c.Rarity == rarity))
            { if (!fixedEncounter) unavailableWidth += width; continue; }
            if (width <= 0) continue;
            available.Add(rarity); widths.Add(width); total += width;
        }
        // Keep configured non-Common widths stable when a depth has no Rare (or other rarity) creatures.
        int commonIndex = available.IndexOf(CardRarity.Common);
        if (commonIndex >= 0) { widths[commonIndex] += unavailableWidth; total += unavailableWidth; }
        // A pool containing only currently disabled rarities must still produce an encounter.
        if (available.Count == 0)
            foreach (CardRarity rarity in Enum.GetValues(typeof(CardRarity)))
                if (Array.Exists(candidates, c => c.Rarity == rarity)) { available.Add(rarity); widths.Add(1); total++; }
        float missWidth = fixedEncounter ? 0 : Mathf.Max(0, profile.miss); total += missWidth;
        var result = new List<Zone>(); float start = 0;
        for (int i = 0; i < available.Count; i++)
        { float end = start + widths[i] / total; result.Add(new Zone(available[i], false, start, end)); start = end; }
        if (missWidth > 0) result.Add(new Zone(default, true, start, 1));
        else if (result.Count > 0) { Zone last = result[result.Count - 1]; result[result.Count - 1] = new Zone(last.Rarity, false, last.Start, 1); }
        zones = result.ToArray();
    }

    /// <summary>Advances marker motion or stopped feedback, independent of presentation frame rate.</summary>
    public bool Tick(float delta)
    {
        if (!Active) return false;
        if (!stopped) elapsed += Mathf.Max(0, delta); else stoppedElapsed += Mathf.Max(0, delta);
        return stopped && stoppedElapsed >= hold;
    }

    /// <summary>Locks exactly one occurrence, rejecting stale callbacks and repeated presses.</summary>
    public bool TryStop(long expectedOccurrence)
    {
        if (!Active || stopped || expectedOccurrence != occurrence) return false;
        float position = Position; Outcome = zones[zones.Length - 1];
        foreach (Zone zone in zones) if (position < zone.End) { Outcome = zone; break; }
        stopped = true; return true;
    }

    /// <summary>Samples uniformly across distinct creatures at or below the stopped rarity ceiling.</summary>
    public CardDefinition Choose(System.Random random)
    {
        if (!Stopped || Outcome.IsMiss) return null;
        var indices = new List<int>();
        for (int i = 0; i < candidates.Length; i++)
            if ((int)candidates[i].Rarity <= (int)Outcome.Rarity) indices.Add(i);
        return candidates[indices[random.Next(indices.Count)]];
    }

    /// <summary>Invalidates pending input and clears hidden state at a phase/run boundary.</summary>
    public void Reset()
    { occurrence++; candidates = Array.Empty<CardDefinition>(); zones = Array.Empty<Zone>(); elapsed = stoppedElapsed = 0; stopped = false; }
}
