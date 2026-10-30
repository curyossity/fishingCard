using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Run-owned eight-tag balance and occurrence-owned tag/precision selection.</summary>
public sealed class EncounterTagTiming : EncounterTiming
{
    public readonly struct Zone
    {
        public readonly string Tag;
        public readonly float Start, End, PrecisionStart, PrecisionEnd;
        /// <summary>Defines a tag span and its proportional centre area.</summary>
        public Zone(string tag, float start, float end, float precisionFraction)
        {
            Tag = tag; Start = start; End = end;
            float half = (end - start) * precisionFraction * .5f, centre = (start + end) * .5f;
            PrecisionStart = centre - half; PrecisionEnd = centre + half;
        }
    }
    private readonly float[] rawWeights = new float[CreatureTagVocabulary.All.Length];
    private Zone[] zones = Array.Empty<Zone>();
    private float[] nextShares = Array.Empty<float>();
    private TagTimingProfile profile;
    private bool balanceInitialized;
    public IReadOnlyList<Zone> Zones => zones;
    public IReadOnlyList<float> NextShares => nextShares;
    public Zone Outcome { get; private set; }
    public bool PrecisionHit { get; private set; }
    public bool UsesFallbackPool { get; private set; }
    public bool FixedEncounter { get; private set; }
    public float ResizeSeconds => profile == null ? 0 : profile.resizeSeconds;
    public override string ResultLabel => Outcome.Tag + (PrecisionHit ? " · Precision" : "");
    public override bool CelebrateStop => Stopped && PrecisionHit;
    public override int EligibleSpeciesCount => Stopped ? SelectionPool().Count : 0;

    /// <summary>Reserves the full eligible species snapshot while keeping all eight tags visible.</summary>
    public void Begin(CardDefinition[] cards, TagTimingProfile settings, bool fixedEncounter)
    {
        Reserve(cards, settings.traversalsPerSecond, settings.resultHoldSeconds);
        profile = settings; FixedEncounter = fixedEncounter;
        if (!balanceInitialized) ResetBalance();
        float[] shares = Normalize(rawWeights, settings.minimumShare);
        zones = new Zone[rawWeights.Length];
        float start = 0;
        for (int i = 0; i < zones.Length; i++)
        {
            float end = i == zones.Length - 1 ? 1 : start + shares[i];
            zones[i] = new Zone(CreatureTagVocabulary.All[i], start, end, Mathf.Clamp01(settings.precisionFraction));
            start = end;
        }
        nextShares = shares;
    }

    /// <summary>Locks the tag and precision, then applies one run-balance change for that selected tag only.</summary>
    protected override void CaptureStop(float position)
    {
        int selected = zones.Length - 1;
        for (int i = 0; i < zones.Length; i++) if (position < zones[i].End) { selected = i; break; }
        Outcome = zones[selected];
        PrecisionHit = position >= Outcome.PrecisionStart && position <= Outcome.PrecisionEnd;
        UsesFallbackPool = !Array.Exists(Candidates, c => HasTag(c, Outcome.Tag));
        for (int i = 0; i < rawWeights.Length; i++)
            rawWeights[i] = i == selected
                ? Mathf.Max(.000001f, rawWeights[i] * (1 - Mathf.Clamp01(profile.selectedShrink)))
                : Mathf.Min(1f / rawWeights.Length, rawWeights[i] + Mathf.Max(0, profile.recoveryPerSelection));
        // Do not accumulate invisible fatigue after a displayed zone has reached its floor.
        float others = 0;
        for (int i = 0; i < rawWeights.Length; i++) if (i != selected) others += rawWeights[i];
        float floor = Mathf.Clamp(profile.minimumShare, 0, 1f / rawWeights.Length);
        rawWeights[selected] = Mathf.Max(rawWeights[selected], others * floor / (1 - floor));
        nextShares = Normalize(rawWeights, profile.minimumShare);
    }

    /// <summary>Draws uniformly normally, or with the approved per-species rarity multipliers on precision.</summary>
    public override CardDefinition Choose(System.Random random)
    {
        if (!Stopped) return null;
        List<CardDefinition> pool = SelectionPool();
        if (!PrecisionHit) return pool[random.Next(pool.Count)];
        int total = 0;
        foreach (CardDefinition card in pool) total += profile.PrecisionWeight(card.Rarity);
        int roll = random.Next(total);
        foreach (CardDefinition card in pool)
        {
            roll -= profile.PrecisionWeight(card.Rarity);
            if (roll < 0) return card;
        }
        return pool[pool.Count - 1];
    }

    /// <summary>Builds only the stopped tag pool; empty tags use the full already-eligible snapshot.</summary>
    private List<CardDefinition> SelectionPool()
    {
        var result = new List<CardDefinition>();
        foreach (CardDefinition card in Candidates) if (HasTag(card, Outcome.Tag)) result.Add(card);
        if (result.Count == 0) result.AddRange(Candidates);
        return result;
    }

    /// <summary>Matches authored tags without changing definitions or using attached-creature tag modifiers.</summary>
    private static bool HasTag(CardDefinition card, string tag)
    { return Array.Exists(card.Tags ?? Array.Empty<string>(), t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase)); }

    /// <summary>Projects raw recovery weights to a full normalized track with a final minimum share.</summary>
    private static float[] Normalize(float[] weights, float minimum)
    {
        int count = weights.Length;
        var shares = new float[count]; var clamped = new bool[count];
        minimum = Mathf.Clamp(minimum, 0, 1f / count);
        float remaining = 1;
        for (int pass = 0; pass <= count; pass++)
        {
            float total = 0;
            for (int i = 0; i < count; i++) if (!clamped[i]) total += weights[i];
            bool changed = false;
            float passBudget = remaining;
            for (int i = 0; i < count; i++)
            {
                if (clamped[i]) continue;
                float proposed = total > 0 ? weights[i] / total * passBudget : passBudget / count;
                if (proposed + .0000001f < minimum)
                { shares[i] = minimum; clamped[i] = true; remaining -= minimum; changed = true; }
            }
            if (changed) continue;
            for (int i = 0; i < count; i++) if (!clamped[i]) shares[i] = total > 0 ? weights[i] / total * remaining : remaining / count;
            break;
        }
        return shares;
    }

    /// <summary>Clears only phase state; balance deliberately survives new creatures and biome transitions.</summary>
    public override void Reset()
    {
        base.Reset(); zones = Array.Empty<Zone>(); nextShares = Array.Empty<float>();
        PrecisionHit = UsesFallbackPool = FixedEncounter = false;
    }

    /// <summary>Starts all eight raw recovery baselines at12.5%; normalization may expand other spans afterward.</summary>
    private void ResetBalance()
    { for (int i = 0; i < rawWeights.Length; i++) rawWeights[i] = 1f / rawWeights.Length; balanceInitialized = true; }

    /// <summary>Clears fatigue at run end/restart, not at ordinary phase or biome boundaries.</summary>
    public void ResetRun() { Reset(); ResetBalance(); }
}
