using System;
using System.Collections.Generic;
using UnityEngine;

public enum DiscoveryQuestMetric { SchoolingOnSuccessfulSurface, DistinctTagsOnSuccessfulSurface, OverloadedSuccessfulExpeditions }

/// <summary>Authored objective and descriptive reward; deliberately contains no reward-grant command.</summary>
[Serializable]
public sealed class DiscoveryQuestDefinition
{
    public string id, title, requirement, reward;
    public DiscoveryQuestMetric metric;
    public int target;
}

/// <summary>Loads the stable, data-authored discovery objectives independently of their UI.</summary>
[Serializable]
public sealed class DiscoveryQuestCatalog
{
    public DiscoveryQuestDefinition[] quests;

    /// <summary>Rejects invalid identities and targets rather than silently presenting untrackable objectives.</summary>
    public static DiscoveryQuestCatalog Load(TextAsset source)
    {
        if (source == null) throw new InvalidOperationException("Discovery quest catalog is missing.");
        var catalog = JsonUtility.FromJson<DiscoveryQuestCatalog>(source.text);
        if (catalog?.quests == null || catalog.quests.Length == 0) throw new InvalidOperationException("Discovery quest catalog is empty.");
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var quest in catalog.quests)
            if (quest == null || string.IsNullOrWhiteSpace(quest.id) || !ids.Add(quest.id) || quest.target < 1
                || !Enum.IsDefined(typeof(DiscoveryQuestMetric), quest.metric)
                || string.IsNullOrWhiteSpace(quest.title) || string.IsNullOrWhiteSpace(quest.requirement) || string.IsNullOrWhiteSpace(quest.reward))
                throw new InvalidOperationException("Invalid discovery quest definition.");
        return catalog;
    }
}
