using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>Verifies quest evidence, persistence, result integration and reward isolation using disposable state.</summary>
public static class DiscoveryJournalChecks
{
    private static string Output = "Docs/Validation/DiscoveryJournal/";
    private static int assertions;
    public static string Status { get; private set; }

    /// <summary>Exercises progression boundaries against the real card-instance/result snapshot contract.</summary>
    public static void ValidateRules()
    {
        assertions = 0;
        var catalog = DiscoveryQuestCatalog.Load(AssetDatabase.LoadAssetAtPath<TextAsset>(JournalHarborBuilder.Root + "DiscoveryQuests.json"));
        var school = catalog.quests.Single(q => q.id == "the_perfect_school");
        var collector = catalog.quests.Single(q => q.id == "the_collector");
        var risk = catalog.quests.Single(q => q.id == "risk_taker");
        var creature = ScriptableObject.CreateInstance<CardDefinition>();
        var environment = ScriptableObject.CreateInstance<CardDefinition>();
        try
        {
            var definition = new SerializedObject(creature); definition.FindProperty("cardType").enumValueIndex = (int)CardType.Creature; definition.ApplyModifiedPropertiesWithoutUndo();
            var eventDefinition = new SerializedObject(environment); eventDefinition.FindProperty("cardType").enumValueIndex = (int)CardType.Environment; eventDefinition.ApplyModifiedPropertiesWithoutUndo();
            CardInstance[] four = Enumerable.Range(0, 4).Select(i => new CardInstance(i, creature, null, i % 2 == 0 ? CatchAttachmentRole.Catch : CatchAttachmentRole.Bait)).ToArray();
            foreach (var card in four) card.SetRuntimeBaseTags(new[] { "Fish", "Schooling" });
            four[3].SetRuntimeBaseTags(new[] { "Schooling", "Small", "Elusive", "Armored" });
            var state = new DiscoveryJournalProgress();
            Require(!state.Record("empty", new FishingRunResult(), 3), "Unfinished result advanced quests.");
            Require(!state.Record("failed", Result(four, true, true), 3), "Line break advanced quests.");
            state.Record("partial-a", Result(four.Take(2).ToArray()), 3);
            state.Record("partial-b", Result(four.Take(2).ToArray()), 3);
            Require(state.GetProgress(school) == 2, "Single-expedition creatures accumulated across runs.");
            var result = Result(four, false, true);
            int gold = result.GoldAwarded, haul = result.HaulValue;
            Require(state.Record("success-a", result, 3), "Successful evidence was ignored.");
            Require(state.GetProgress(school) == 4, "Mixed Catch/Bait Schooling did not complete.");
            Require(state.GetProgress(collector) == 5, "Distinct effective tags did not complete.");
            Require(state.GetProgress(risk) == 1, "Successful overload did not count once.");
            Require(!state.Record("success-a", result, 3) && state.GetProgress(risk) == 1, "Duplicate result counted twice.");
            state = DiscoveryJournalProgress.FromJson(state.ToJson());
            Require(!state.Record("success-a", result, 3), "Save reload lost expedition deduplication.");
            state.Record("success-b", result, 3); state.Record("success-c", result, 3); state.Record("success-d", result, 3);
            Require(state.GetProgress(risk) == 3, "Three separate overloads did not complete or exceed target.");
            Require(DiscoveryJournalProgress.FromJson(state.ToJson()).GetProgress(collector) == 5, "Completion did not survive serialization.");
            Require(result.GoldAwarded == gold && result.HaulValue == haul, "Quest evaluation modified rewards or haul.");
            var excluded = new DiscoveryJournalProgress();
            var nonCreature = new CardInstance(99, environment); nonCreature.SetRuntimeBaseTags(new[] { "Schooling", "Heavy" });
            excluded.Record("event", Result(new[] { nonCreature }), 3);
            Require(excluded.GetProgress(school) == 0 && excluded.GetProgress(collector) == 0, "Non-creature counted toward quests.");
            // Released/lost histories are explicitly separate from returned attachments.
            var removed = new FishingRunResult(); removed.Record(Array.Empty<CardInstance>(), four, four, 5, 0, 10, false);
            excluded.Record("removed", removed, 3);
            Require(excluded.GetProgress(school) == 0, "Released or lost creatures counted.");
            Require(Rejects("{\"version\":99}") && Rejects("not json"), "Unsupported/corrupt save accepted.");
        }
        finally { UnityEngine.Object.DestroyImmediate(creature); UnityEngine.Object.DestroyImmediate(environment); }
        Directory.CreateDirectory(Output); File.WriteAllText(Output + "RulesVerification.json", "{\"passed\":true,\"assertions\":" + assertions + "}");
    }

    /// <summary>Creates a completed result with explicit load and line-break outcomes.</summary>
    private static FishingRunResult Result(CardInstance[] cards, bool broken = false, bool overloaded = false)
    {
        var result = new FishingRunResult(); result.Record(cards, null, null, 8, overloaded ? 11 : 5, 10, broken); return result;
    }

    /// <summary>Checks that invalid saves cannot silently reset and overwrite existing objective evidence.</summary>
    private static bool Rejects(string json)
    {
        try { DiscoveryJournalProgress.FromJson(json); return false; } catch (Exception) { return true; }
    }

    /// <summary>Starts an actual controller Surface test with an isolated PlayerPrefs key, never the player's journal.</summary>
    public static void BeginLiveCheck(string output = "Docs/Validation/DiscoveryJournal/")
    {
        Output = output;
        Directory.CreateDirectory(Output);
        if (!Application.isPlaying) throw new InvalidOperationException("Enter Play Mode first.");
        var flow = UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>();
        var tracker = flow.GetComponent<DiscoveryJournalTracker>();
        string key = "FishingCards.DiscoveryJournal.Test." + Guid.NewGuid().ToString("N");
        typeof(DiscoveryJournalTracker).GetField("saveKey", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(tracker, key);
        Status = "Running"; flow.StartCoroutine(Guard(Live(flow, tracker, key), key));
    }

    /// <summary>Reports coroutine failures and always removes the isolated test save.</summary>
    private static IEnumerator Guard(IEnumerator routine, string key)
    {
        while (true)
        {
            bool more;
            try { more = routine.MoveNext(); }
            catch (Exception error) { Status = "Failed: " + error; PlayerPrefs.DeleteKey(key); PlayerPrefs.Save(); Debug.LogException(error); yield break; }
            if (!more) break;
            yield return routine.Current;
        }
        PlayerPrefs.DeleteKey(key); PlayerPrefs.Save(); Status = "Passed";
        Directory.CreateDirectory(Output); File.WriteAllText(Output + "LiveProgressVerification.json", "{\"passed\":true,\"message\":\"Actual Surface updates Journal, persists evidence, grants no quest rewards and returns to Harbor.\"}");
    }

    /// <summary>Uses actual Surface/event notification and inspects persistent data, UI and unchanged progression.</summary>
    private static IEnumerator Live(JournalHarborFlow flow, DiscoveryJournalTracker tracker, string key)
    {
        var controller = flow.Controller;
        Require(flow.TryStartNewRun(), "Could not start expedition.");
        var chain = (CatchChainRuntime)typeof(FishingRunController).GetField("catchChainRuntime", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(controller);
        var resolver = (EffectResolver)typeof(FishingRunController).GetField("effectResolver", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(controller);
        var sardine = AssetDatabase.FindAssets("t:CardDefinition").Select(g => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).First(c => c.UniqueId == "creature_sardine");
        for (int i = 0; i < 4; i++) chain.Add(sardine, resolver, i % 2 == 0 ? CatchAttachmentRole.Catch : CatchAttachmentRole.Bait);
        int deck = controller.SavedTechniqueDeck.Length, capacity = controller.ProgressionLineCapacityBonus, gold = controller.TotalGold;
        Require(controller.TrySurface(), "Actual Surface rejected.");
        var quest = tracker.Quests.First(q => q.id == "the_perfect_school");
        Require(tracker.GetProgress(quest) == 4, "Controller result did not update tracker.");
        Require(PlayerPrefs.HasKey(key) && DiscoveryJournalProgress.FromJson(PlayerPrefs.GetString(key)).GetProgress(quest) == 4, "Completion was not persisted.");
        var reloadObject = new GameObject("Disposable Journal Reload"); reloadObject.SetActive(false);
        try
        {
            var reloaded = reloadObject.AddComponent<DiscoveryJournalTracker>(); var data = new SerializedObject(reloaded);
            data.FindProperty("saveKey").stringValue = key;
            data.FindProperty("catalogSource").objectReferenceValue = AssetDatabase.LoadAssetAtPath<TextAsset>(JournalHarborBuilder.Root + "DiscoveryQuests.json");
            data.ApplyModifiedPropertiesWithoutUndo();
            Require(reloaded.GetProgress(quest) == 4, "Fresh tracker did not load saved completion.");
        }
        finally { UnityEngine.Object.Destroy(reloadObject); }
        Require(controller.SavedTechniqueDeck.Length == deck && controller.ProgressionLineCapacityBonus == capacity, "Quest completion granted progression.");
        Require(controller.TotalGold - gold == controller.LastGoldAwarded, "Quest completion granted extra gold.");
        var modal = UnityEngine.Object.FindAnyObjectByType<JournalResultModal>(); float deadline = Time.realtimeSinceStartup + 20;
        while (!modal.IsVisible && Time.realtimeSinceStartup < deadline) yield return null;
        Require(modal.IsVisible, "Results missing."); modal.NextRunButton.onClick.Invoke(); flow.Navigate(3); yield return null;
        Require(flow.CurrentPage == 3 && flow.GetComponentsInChildren<TMP_Text>().Any(t => t.text == "COMPLETE"), "Journal did not show completed quest.");
        Require(flow.GetComponentsInChildren<TMP_Text>().Count(t => t.text == "REWARD · NOT YET GRANTED") == 3, "Reward descriptions imply a grant.");
        flow.Navigate(0);
    }

    /// <summary>Stops verification on the first violated gameplay or presentation boundary.</summary>
    private static void Require(bool condition, string message) { assertions++; if (!condition) throw new InvalidOperationException(message); }
}
