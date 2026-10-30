using TMPro;
using UnityEngine;

/// <summary>Displays discovery evidence and descriptive rewards, without granting or claiming unlocks.</summary>
public sealed class JournalDiscoveryPage : MonoBehaviour
{
    [SerializeField] private DiscoveryJournalTracker tracker;
    [SerializeField] private TMP_Text[] progressLabels;
    [SerializeField] private UnityEngine.UI.Image[] progressFills;
    [SerializeField] private TMP_Text saveStatus;

    /// <summary>Refreshes saved progress whenever the player opens the Journal.</summary>
    private void OnEnable() { Refresh(); }

    /// <summary>Renders completed objectives as tracked completion, never as granted rewards.</summary>
    public void Refresh()
    {
        if (tracker == null) return;
        var quests = tracker.Quests;
        for (int i = 0; i < quests.Length; i++)
        {
            int amount = tracker.GetProgress(quests[i]);
            progressLabels[i].text = FormatProgress(quests[i], amount);
            progressFills[i].transform.parent.gameObject.SetActive(quests[i].metric == DiscoveryQuestMetric.OverloadedSuccessfulExpeditions);
            progressFills[i].fillAmount = (float)amount / quests[i].target;
        }
        saveStatus.text = string.IsNullOrEmpty(tracker.SaveError)
            ? "Progress is saved. Rewards are listed for reference and are not granted yet."
            : tracker.SaveError;
    }

    /// <summary>Shows partial counts only for objectives that accumulate across expeditions.</summary>
    public static string FormatProgress(DiscoveryQuestDefinition quest, int amount)
    {
        string status = amount >= quest.target ? "COMPLETE" : "INCOMPLETE";
        if (quest.metric != DiscoveryQuestMetric.OverloadedSuccessfulExpeditions) return status;
        return amount + " / " + quest.target + " successful expeditions" + (amount >= quest.target ? " · COMPLETE" : "");
    }
}
