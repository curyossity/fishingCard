using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Owns a hidden encounter's eligibility, marker and exact stopped outcome.</summary>
public sealed class EncounterRarityTiming : EncounterTiming
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
    private Zone[] zones = Array.Empty<Zone>();
    public IReadOnlyList<Zone> Zones => zones;
    public Zone Outcome { get; private set; }
    public override string ResultLabel => Outcome.Label;
    public override bool CelebrateStop
    {
        get
        {
            if (!Stopped || Outcome.IsMiss) return false;
            foreach (Zone zone in zones) if (!zone.IsMiss && (int)zone.Rarity > (int)Outcome.Rarity) return false;
            return true;
        }
    }
    /// <summary>Counts equal species entries under the locked ceiling for authoritative draw diagnostics.</summary>
    public override int EligibleSpeciesCount
    {
        get
        {
            if (!Stopped || Outcome.IsMiss) return 0;
            int count=0;
            foreach (CardDefinition card in Candidates) if ((int)card.Rarity <= (int)Outcome.Rarity) count++;
            return count;
        }
    }

    /// <summary>Gives unavailable rarity space to Common where possible; fixed promises keep their identity.</summary>
    public void Begin(CardDefinition[] cards, RarityTimingProfile profile, bool fixedEncounter)
    {
        Reserve(cards, profile.traversalsPerSecond, profile.resultHoldSeconds);
        var available = new List<CardRarity>(); var widths = new List<float>(); float total = 0, unavailableWidth = 0;
        foreach (CardRarity rarity in Enum.GetValues(typeof(CardRarity)))
        {
            float width = fixedEncounter ? 1 : Mathf.Max(0, profile.Width(rarity));
            if (!Array.Exists(Candidates, c => c.Rarity == rarity))
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
                if (Array.Exists(Candidates, c => c.Rarity == rarity)) { available.Add(rarity); widths.Add(1); total++; }
        float missWidth = fixedEncounter ? 0 : Mathf.Max(0, profile.miss); total += missWidth;
        var result = new List<Zone>(); float start = 0;
        for (int i = 0; i < available.Count; i++)
        { float end = start + widths[i] / total; result.Add(new Zone(available[i], false, start, end)); start = end; }
        if (missWidth > 0) result.Add(new Zone(default, true, start, 1));
        else if (result.Count > 0) { Zone last = result[result.Count - 1]; result[result.Count - 1] = new Zone(last.Rarity, false, last.Start, 1); }
        zones = result.ToArray();
    }

    /// <summary>Captures the rarity interval containing the shared clock's exact stopped position.</summary>
    protected override void CaptureStop(float position)
    {
        Outcome = zones[zones.Length - 1];
        foreach (Zone zone in zones) if (position < zone.End) { Outcome = zone; break; }
    }

    /// <summary>Samples uniformly across distinct creatures at or below the stopped rarity ceiling.</summary>
    public override CardDefinition Choose(System.Random random)
    {
        if (!Stopped || Outcome.IsMiss) return null;
        var indices = new List<int>();
        for (int i = 0; i < Candidates.Length; i++)
            if ((int)Candidates[i].Rarity <= (int)Outcome.Rarity) indices.Add(i);
        return Candidates[indices[random.Next(indices.Count)]];
    }

    /// <summary>Invalidates pending input and clears hidden state at a phase/run boundary.</summary>
    public override void Reset()
    { base.Reset(); zones = Array.Empty<Zone>(); }
}
