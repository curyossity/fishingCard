using System;
using UnityEngine;

/// <summary>Connects completed expeditions to persistent discovery evidence without executing rewards.</summary>
public sealed class DiscoveryJournalTracker : MonoBehaviour
{
    [SerializeField] private FishingRunController controller;
    [SerializeField] private TextAsset catalogSource;
    [SerializeField] private string saveKey = "FishingCards.DiscoveryJournal.v1";
    private DiscoveryQuestCatalog catalog;
    private DiscoveryJournalProgress progress;
    private bool loadFailed;
    private bool pendingSave;
    public string SaveError { get; private set; }
    public DiscoveryQuestDefinition[] Quests { get { EnsureLoaded(); return catalog.quests; } }

    /// <summary>Subscribes while the always-active hub root exists, including during expeditions.</summary>
    private void OnEnable()
    {
        if (controller != null) controller.RunCompleted += OnRunCompleted;
    }

    /// <summary>Releases the completion subscription on scene unload or disabling.</summary>
    private void OnDisable()
    {
        if (controller != null) controller.RunCompleted -= OnRunCompleted;
        TrySave();
    }

    /// <summary>Returns saved progress even when no expedition has started in this session.</summary>
    public int GetProgress(DiscoveryQuestDefinition quest) { EnsureLoaded(); return progress.GetProgress(quest); }

    /// <summary>Initializes once; unreadable existing saves remain untouched and expose an explicit error.</summary>
    private void EnsureLoaded()
    {
        if (catalog != null) return;
        catalog = DiscoveryQuestCatalog.Load(catalogSource);
        progress = new DiscoveryJournalProgress();
        try
        {
            if (PlayerPrefs.HasKey(saveKey)) progress = DiscoveryJournalProgress.FromJson(PlayerPrefs.GetString(saveKey));
        }
        catch (Exception error)
        {
            loadFailed = true;
            SaveError = "Journal save could not be loaded. Existing saved data has been preserved.";
            Debug.LogError(SaveError + " " + error.Message, this);
        }
    }

    /// <summary>Saves only changed objective evidence, once per actual completed expedition.</summary>
    private void OnRunCompleted(string expeditionId, FishingRunResult result)
    {
        EnsureLoaded();
        int overloadedTarget = 0;
        foreach (var quest in catalog.quests)
            if (quest.metric == DiscoveryQuestMetric.OverloadedSuccessfulExpeditions) overloadedTarget = Math.Max(overloadedTarget, quest.target);
        pendingSave |= progress.Record(expeditionId, result, overloadedTarget);
        TrySave();
    }

    /// <summary>Retries unsaved evidence on later completions or scene exit while preserving unreadable saves.</summary>
    private void TrySave()
    {
        if (!pendingSave || loadFailed) return;
        try
        {
            PlayerPrefs.SetString(saveKey, progress.ToJson());
            PlayerPrefs.Save();
            pendingSave = false;
            SaveError = null;
        }
        catch (Exception error)
        {
            SaveError = "Journal progress is available this session but could not be saved.";
            Debug.LogError(SaveError + " " + error.Message, this);
        }
    }
}
