using TMPro;
using UnityEngine;

/// <summary>Displays discovery evidence and descriptive rewards, without granting or claiming unlocks.</summary>
public sealed class JournalDiscoveryPage : MonoBehaviour
{
    [SerializeField] private DiscoveryJournalTracker tracker;
    [System.Serializable] private sealed class QuestRow
    {
        public TMP_Text status;
        public UnityEngine.UI.Graphic completionMark;
        public UnityEngine.UI.Image[] segments;
    }
    [SerializeField] private QuestRow[] rows;
    [SerializeField] private TMP_Text saveStatus;
    [SerializeField] private TMP_Text goldLabel;
    public TMP_Text GoldLabel => goldLabel;

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
            bool complete=amount>=quests[i].target;
            var row=rows[i]; row.status.text=FormatProgress(quests[i],amount);
            row.status.color=complete?new Color32(196,217,196,255):new Color32(224,188,128,255);
            if(row.completionMark!=null)
            {
                row.completionMark.enabled=complete;
                var position=row.status.rectTransform.anchoredPosition; position.x=complete?1530:1480; row.status.rectTransform.anchoredPosition=position;
            }
            for(int j=0;j<row.segments.Length;j++) row.segments[j].color=j<amount?new Color32(216,181,127,255):new Color32(114,128,128,102);
        }
        saveStatus.text = string.IsNullOrEmpty(tracker.SaveError)
            ? "Rewards are listed for reference; nothing is granted yet."
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
