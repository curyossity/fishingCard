using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Persistent objective evidence. Single-expedition records use maxima, never sums across expeditions.</summary>
[Serializable]
public sealed class DiscoveryJournalProgress
{
    [SerializeField] private int version = 1;
    [SerializeField] private int bestSchooling;
    [SerializeField] private int bestDistinctTags;
    [SerializeField] private List<string> overloadedExpeditions = new List<string>();

    /// <summary>Reads capped progress for an authored objective without exposing mutable save state.</summary>
    public int GetProgress(DiscoveryQuestDefinition quest)
    {
        int amount;
        switch (quest.metric)
        {
            case DiscoveryQuestMetric.SchoolingOnSuccessfulSurface: amount = bestSchooling; break;
            case DiscoveryQuestMetric.DistinctTagsOnSuccessfulSurface: amount = bestDistinctTags; break;
            case DiscoveryQuestMetric.OverloadedSuccessfulExpeditions: amount = overloadedExpeditions.Count; break;
            default: throw new ArgumentOutOfRangeException(nameof(quest));
        }
        return Mathf.Min(quest.target, amount);
    }

    /// <summary>Records surviving creature tags before run cleanup; broken lines and unresolved results never qualify.</summary>
    public bool Record(string expeditionId, FishingRunResult result, int overloadedTarget)
    {
        if (string.IsNullOrEmpty(expeditionId) || result == null || !result.HasResult || result.EndedByLineBreak) return false;
        int schooling = 0;
        var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var card in result.Haul)
        {
            if (card?.Definition == null || !card.Definition.RequiresAttachmentChoice) continue;
            bool isSchooling = false;
            foreach (string tag in card.Tags)
            {
                if (string.IsNullOrWhiteSpace(tag)) continue;
                tags.Add(tag);
                if (string.Equals(tag, "Schooling", StringComparison.OrdinalIgnoreCase)) isSchooling = true;
            }
            if (isSchooling) schooling++;
        }
        bool changed = schooling > bestSchooling || tags.Count > bestDistinctTags;
        bestSchooling = Math.Max(bestSchooling, schooling);
        bestDistinctTags = Math.Max(bestDistinctTags, tags.Count);
        // Stable expedition IDs make repeated completion notifications and save reloads idempotent.
        if (result.WasOverloaded && overloadedExpeditions.Count < overloadedTarget && !overloadedExpeditions.Contains(expeditionId))
        {
            overloadedExpeditions.Add(expeditionId);
            changed = true;
        }
        return changed;
    }

    /// <summary>Serializes only evidence; no gold, equipment, deck or reward state can be granted here.</summary>
    public string ToJson() { return JsonUtility.ToJson(this); }

    /// <summary>Validates saved evidence and refuses unsupported versions instead of overwriting them.</summary>
    public static DiscoveryJournalProgress FromJson(string json)
    {
        var progress = JsonUtility.FromJson<DiscoveryJournalProgress>(json);
        if (progress == null || progress.version != 1 || progress.bestSchooling < 0 || progress.bestDistinctTags < 0
            || progress.overloadedExpeditions == null) throw new InvalidOperationException("Invalid or unsupported discovery journal save.");
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (string id in progress.overloadedExpeditions)
            if (string.IsNullOrWhiteSpace(id) || !ids.Add(id)) throw new InvalidOperationException("Invalid discovery expedition history.");
        return progress;
    }
}
