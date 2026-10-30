using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>Authors a native Journal page using the existing paper/ink modal treatment.</summary>
public static class JournalDiscoveryPageBuilder
{
    /// <summary>Creates reusable objective rows; the tracker supplies progress only during play.</summary>
    public static RectTransform Build(Transform parent, DiscoveryJournalTracker tracker, TextAsset catalogSource, MidnightFieldJournalTheme theme)
    {
        var catalog = DiscoveryQuestCatalog.Load(catalogSource);
        var page = JournalModalLayout.Rect("journal", parent, new Rect(0, 0, 1, 1));
        var paper = JournalModalLayout.Rect("Paper", page, new Rect(.025f, .12f, .95f, .74f)).gameObject.AddComponent<UnityEngine.UI.Image>();
        paper.color = theme.LightInk; paper.raycastTarget = false;
        var edge = JournalModalLayout.Rect("Paper Edge", paper.transform, new Rect(0, 0, 1, 1)).gameObject.AddComponent<JournalCardOutline>(); edge.color = theme.Copper; edge.raycastTarget = false;
        JournalModalLayout.Text("Discovery Journal", page, new Rect(.06f, .77f, .88f, .065f), theme, 66, true).text = "DISCOVERY JOURNAL";
        JournalModalLayout.Text("Introduction", page, new Rect(.06f, .72f, .88f, .045f), theme, 34).text = "Successful returns tell your story. Catch and Bait both count; line breaks do not.";
        var progressLabels = new List<TMP_Text>(); var fills = new List<UnityEngine.UI.Image>();
        for (int i = 0; i < catalog.quests.Length; i++)
        {
            var quest = catalog.quests[i];
            var row = JournalModalLayout.Rect(quest.id, page, new Rect(.06f, .53f - i * .18f, .88f, .16f));
            var outline = row.gameObject.AddComponent<JournalCardOutline>(); outline.color = theme.Copper; outline.raycastTarget = false;
            JournalModalLayout.Text("Quest", row, new Rect(.025f, .66f, .42f, .29f), theme, 45, true).text = quest.title;
            JournalModalLayout.Text("Requirement", row, new Rect(.025f, .31f, .43f, .34f), theme, 35).text = quest.requirement;
            var progress = JournalModalLayout.Text("Progress", row, new Rect(.025f, .09f, .43f, .20f), theme, 29);
            progress.text = JournalDiscoveryPage.FormatProgress(quest, 0);
            progressLabels.Add(progress);
            JournalModalLayout.Text("Reward Heading", row, new Rect(.51f, .70f, .46f, .22f), theme, 29, true).text = "REWARD · NOT GRANTED";
            JournalModalLayout.Text("Reward", row, new Rect(.51f, .10f, .46f, .58f), theme, 35).text = quest.reward;
            var track = JournalModalLayout.Rect("Progress Track", row, new Rect(.025f, .045f, .43f, .022f)).gameObject.AddComponent<UnityEngine.UI.Image>();
            track.color = new Color(.4f, .35f, .25f, .5f); track.raycastTarget = false;
            var fill = JournalModalLayout.Rect("Progress Fill", track.transform, new Rect(0, 0, 1, 1)).gameObject.AddComponent<UnityEngine.UI.Image>();
            fill.color = theme.Copper; fill.raycastTarget = false; fill.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            fill.type = UnityEngine.UI.Image.Type.Filled; fill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal; fill.fillAmount = 0; fills.Add(fill);
            track.gameObject.SetActive(quest.metric == DiscoveryQuestMetric.OverloadedSuccessfulExpeditions);
        }
        var status = JournalModalLayout.Text("Save Status", page, new Rect(.06f, .13f, .88f, .035f), theme, 30);
        status.text = "Progress is saved. Rewards are listed for reference and are not granted yet.";
        var view = page.gameObject.AddComponent<JournalDiscoveryPage>(); var data = new SerializedObject(view);
        data.FindProperty("tracker").objectReferenceValue = tracker;
        data.FindProperty("saveStatus").objectReferenceValue = status;
        var labels = data.FindProperty("progressLabels"); labels.arraySize = progressLabels.Count;
        var bars = data.FindProperty("progressFills"); bars.arraySize = fills.Count;
        for (int i = 0; i < progressLabels.Count; i++) { labels.GetArrayElementAtIndex(i).objectReferenceValue = progressLabels[i]; bars.GetArrayElementAtIndex(i).objectReferenceValue = fills[i]; }
        data.ApplyModifiedPropertiesWithoutUndo();
        return page;
    }

    /// <summary>Updates only the existing prefab's progress labels and tracks, preserving all layout and object identities.</summary>
    public static void SyncProgressPresentation()
    {
        var root = PrefabUtility.LoadPrefabContents(JournalHarborBuilder.PrefabPath);
        try
        {
            var catalog = DiscoveryQuestCatalog.Load(AssetDatabase.LoadAssetAtPath<TextAsset>(JournalHarborBuilder.Root + "DiscoveryQuests.json"));
            var page = root.GetComponentInChildren<JournalDiscoveryPage>(true);
            var data = new SerializedObject(page);
            for (int i = 0; i < catalog.quests.Length; i++)
            {
                var label = (TMP_Text)data.FindProperty("progressLabels").GetArrayElementAtIndex(i).objectReferenceValue;
                var fill = (UnityEngine.UI.Image)data.FindProperty("progressFills").GetArrayElementAtIndex(i).objectReferenceValue;
                label.text = JournalDiscoveryPage.FormatProgress(catalog.quests[i], 0);
                fill.transform.parent.gameObject.SetActive(catalog.quests[i].metric == DiscoveryQuestMetric.OverloadedSuccessfulExpeditions);
            }
            PrefabUtility.SaveAsPrefabAsset(root, JournalHarborBuilder.PrefabPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
}
