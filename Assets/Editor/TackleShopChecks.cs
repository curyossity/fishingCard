using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Verifies equipment transactions, complete expedition benefits and actual shop input with isolated save data.</summary>
public static class TackleShopChecks
{
    private static string Output = "Docs/Validation/TackleShop/";
    private static int assertions;
    public static string Status { get; private set; }

    /// <summary>Checks purchase invariants, failure atomicity, persistence and first-Catch modifier ownership.</summary>
    public static void ValidateRules(string output = "Docs/Validation/TackleShop/")
    {
        Output = output;
        assertions = 0; Directory.CreateDirectory(Output);
        var catalog = AssetDatabase.LoadAssetAtPath<TackleShopCatalog>(JournalTackleShopSetup.CatalogPath); catalog.Validate();
        string saved = null;
        var shop = new TackleShopRuntime(catalog, 1000, 0, () => saved, json => saved = json);
        Require(catalog.Items.Select(x => x.Price).SequenceEqual(new[] { 200, 200, 250 }), "Approved prices differ.");
        string reason;
        Require(!shop.TryPurchase("driftwood_rod", 199, 0, false, out reason), "Insufficient gold allowed.");
        Require(!shop.TryPurchase("driftwood_rod", 1000, 0, true, out reason), "Purchase during expedition allowed.");
        Require(!shop.TryPurchase("unknown", 1000, 0, false, out reason), "Unknown item allowed.");
        Require(shop.TryPurchase("driftwood_rod", 1000, 0, false, out reason) && shop.Gold == 800 && shop.Owns("driftwood_rod") && shop.Equipped == null, "Purchase or separate equip failed.");
        Require(!shop.TryPurchase("driftwood_rod", 800, 0, false, out reason) && shop.Gold == 800, "Duplicate purchase deducted gold.");
        Require(!shop.TryEquip("barbed_hook", 800, 0, false, out reason), "Unowned item equipped.");
        Require(shop.TryEquip("driftwood_rod", 800, 0, false, out reason), "Owned item could not equip.");
        shop = new TackleShopRuntime(catalog, 0, 0, () => saved, json => saved = json);
        Require(shop.Gold == 800 && shop.EquippedId == "driftwood_rod", "Wallet/loadout reload failed.");
        Require(shop.TryPurchase("tideglass_reel", 800, 0, false, out reason) && shop.TryEquip("tideglass_reel", 600, 0, false, out reason) && shop.Owns("driftwood_rod") && shop.EquippedId == "tideglass_reel", "Single loadout selection failed.");
        Require(!shop.TryEquip("driftwood_rod", 600, 0, true, out reason), "Active equipment changed.");
        Require(shop.SaveWallet(900, 2), "Ordinary gold/upgrade save failed.");
        var reloaded = new TackleShopRuntime(catalog, 0, 0, () => saved, _ => { });
        Require(reloaded.Gold == 900 && reloaded.LineCapacityUpgrades == 2, "Existing capacity purchase persistence failed.");
        bool fail = true;
        var failure = new TackleShopRuntime(catalog, 500, 0, () => null, _ => { if (fail) throw new IOException("Deliberate test failure"); });
        Require(!failure.TryPurchase("barbed_hook", 500, 0, false, out reason) && !failure.Owns("barbed_hook") && failure.Gold == 500, "Failed save partially applied transaction.");
        fail = false; Require(failure.TryPurchase("barbed_hook", 500, 0, false, out reason) && failure.Gold == 250, "Failed save could not retry.");
        bool wrote = false;
        var corrupt = new TackleShopRuntime(catalog, 0, 0, () => "{\"version\":99}", _ => wrote = true);
        Require(!corrupt.TryPurchase("driftwood_rod", 999, 0, false, out reason) && !wrote, "Unsupported save overwritten.");

        var cod = FindCard("creature_coastal_cod"); var chain = new CatchChainRuntime(); var resolver = new EffectResolver(); var equipment = new RunEquipmentRuntime();
        equipment.BeginRun(catalog.Find("barbed_hook")); chain.BeforeAttachmentEffects += equipment.BeforeAttachment;
        for (int i = 0; i < 3; i++)
        {
            var preview = chain.CreateResolvedCatchPreview(cod, resolver, CatchAttachmentRole.Catch, equipment.PreviewCatchBonus);
            Require(preview.CurrentValue == cod.Value + 1 && equipment.RemainingCatchBonuses == 3, "Preview consumed equipment or missed value.");
        }
        var bait = chain.Add(cod, resolver, CatchAttachmentRole.Bait);
        Require(equipment.RemainingCatchBonuses == 3 && bait.CurrentValue == cod.Value, "Bait consumed a grant.");
        chain.TryChangeCreatureMode(bait.InstanceId, resolver);
        Require(equipment.RemainingCatchBonuses == 3 && bait.CurrentValue == cod.Value, "Mode change consumed a grant.");
        var first = chain.Add(cod, resolver); chain.Add(cod, resolver); chain.Add(cod, resolver); var fourth = chain.Add(cod, resolver);
        Require(first.CurrentValue == cod.Value + 1 && fourth.CurrentValue == cod.Value && equipment.RemainingCatchBonuses == 0, "First-three Catch boundary failed.");
        Require(new CardStatBreakdown(first, CreatureStat.Value, 1).Reasons.Any(x => x.Name == "Barbed Hook" && x.Amount == 1), "Equipment attribution missing.");
        chain.TryChangeCreatureMode(first.InstanceId, resolver); chain.TryChangeCreatureMode(first.InstanceId, resolver);
        Require(first.CurrentValue == cod.Value + 1, "Awarded bonus lost or repeated after mode changes.");
        chain.TryReleaseTogether(new[] { first.InstanceId }, resolver); var next = chain.Add(cod, resolver);
        Require(next.CurrentValue == cod.Value && equipment.RemainingCatchBonuses == 0, "Release refunded equipment charge.");
        equipment.BeginRun(catalog.Find("barbed_hook")); Require(equipment.RemainingCatchBonuses == 3, "New expedition did not reset charges.");
        equipment.BeginRun(catalog.Find("driftwood_rod")); Require(equipment.CapacityBonus == 1 && equipment.OpeningDrawBonus == 0, "Rod benefit wrong.");
        equipment.BeginRun(catalog.Find("tideglass_reel")); Require(equipment.OpeningDrawBonus == 2 && equipment.CapacityBonus == 0, "Reel benefit stacks incorrectly.");
        var deck = new TechniqueDeckRuntime(); var techniques = Enumerable.Repeat(FindCard("cut_loose"), 12).ToArray();
        deck.Initialize(techniques, 6, new System.Random(1), null);
        Require(deck.Hand.Length == 6 && deck.DrawTechniques(1, new System.Random(1)) == 1, "Draw above the six-card opening hand failed.");
        deck.TryDiscardTechnique(0); Require(deck.TryRecoverDiscardedTechnique(0) && deck.Hand.Length == 7, "Six-card recovery failed.");
        deck.Initialize(techniques, 4, new System.Random(1), null); Require(deck.Hand.Length == 4, "Default opening draw changed.");
        File.WriteAllText(Output + "RulesVerification.json", "{\"passed\":true,\"assertions\":" + assertions + "}");
    }

    /// <summary>Starts pointer-driven purchases and real expedition tests using isolated progression/Journal keys.</summary>
    public static void BeginLiveCheck(string output = "Docs/Validation/TackleShop/")
    {
        Output = output;
        if (!Application.isPlaying) throw new InvalidOperationException("Enter Play Mode.");
        var flow = UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>(); var controller = flow.Controller;
        string key = "FishingCards.EquipmentTest." + Guid.NewGuid().ToString("N"), journalKey = key + ".Journal";
        var catalog = AssetDatabase.LoadAssetAtPath<TackleShopCatalog>(JournalTackleShopSetup.CatalogPath);
        var shop = new TackleShopRuntime(catalog, 1200, 0, () => null, json => { PlayerPrefs.SetString(key, json); PlayerPrefs.Save(); });
        Field(controller, "tackleShop").SetValue(controller, shop);
        ((RunRewardRuntime)Field(controller, "runRewardRuntime").GetValue(controller)).RestoreBalance(1200);
        ((RunProgressionRuntime)Field(controller, "runProgressionRuntime").GetValue(controller)).RestoreCapacityPurchases(0);
        var tracker = flow.GetComponent<DiscoveryJournalTracker>(); Field(tracker, "saveKey").SetValue(tracker, journalKey);
        Directory.CreateDirectory(Output); File.WriteAllText(Output + "TestKeys.json", JsonUtility.ToJson(new Keys { progression = key, journal = journalKey }));
        assertions = 0; Status = "Running"; flow.StartCoroutine(Guard(Live(flow)));
    }

    /// <summary>Runs actual uGUI input and existing controller commands, leaving normal player save data untouched.</summary>
    private static IEnumerator Live(JournalHarborFlow flow)
    {
        var controller = flow.Controller; flow.Navigate(1); yield return new WaitForSecondsRealtime(.5f);
        Click(flow, "Inspect DRIFTWOOD ROD"); Click(flow, "Purchase or Equip");
        Require(controller.TotalGold == 1000 && controller.TackleShop.Owns("driftwood_rod") && controller.TackleShop.Equipped == null, "Pointer purchase failed.");
        Click(flow, "Purchase or Equip"); Require(controller.TackleShop.EquippedId == "driftwood_rod", "Pointer equip failed.");
        string reason;
        Require(!controller.TryPurchaseEquipment("driftwood_rod", out reason) && controller.TotalGold == 1000, "Duplicate purchase allowed.");
        flow.Navigate(0); Require(flow.TryStartNewRun(), "Rod run did not start.");
        int baseline = (int)Field(controller, "startingLineCapacity").GetValue(controller);
        Require(controller.LineCapacity == baseline + 1 && controller.TechniqueHand.Length == 4, "Rod start benefit wrong.");
        Require(!controller.TryPurchaseEquipment("tideglass_reel", out reason) && !controller.TryEquipEquipment("", out reason), "Active-run shop mutation allowed.");
        Require(controller.TrySurface(), "Rod Surface failed.");
        var modal = UnityEngine.Object.FindAnyObjectByType<JournalResultModal>();
        float deadline = Time.realtimeSinceStartup + 20; while (!modal.IsVisible && Time.realtimeSinceStartup < deadline) yield return null;
        Require(modal.IsVisible, "Rod result missing."); modal.NextRunButton.onClick.Invoke(); flow.Navigate(1); yield return null;
        Click(flow, "Inspect TIDEGLASS REEL"); Click(flow, "Purchase or Equip"); Click(flow, "Purchase or Equip");
        Require(controller.TotalGold == 800 && controller.TackleShop.EquippedId == "tideglass_reel", "Reel purchase/equip failed.");
        flow.Navigate(0); Require(flow.TryStartNewRun(), "Reel run did not start.");
        Require(controller.TechniqueHand.Length == 6 && controller.LineCapacity == baseline, "Reel opening draw or one-slot tradeoff failed.");
        var liveDeck = (TechniqueDeckRuntime)Field(controller, "techniqueDeckRuntime").GetValue(controller);
        Require(liveDeck.DrawTechniques(20, new System.Random(1)) == 6 && liveDeck.Hand.Length == 12,
            "Effect draws above the Reel opening hand were capped or invented cards.");
        controller.GetType().GetMethod("RefreshViews", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(controller, null);
        yield return new WaitForSecondsRealtime(.5f);
        var handView = UnityEngine.Object.FindAnyObjectByType<JournalTechniqueHandPresentation>();
        Require(handView.Cards.Count(card => card.gameObject.activeInHierarchy) == 12
            && handView.Poses.Count == 12, "Larger hand lost visible or inspectable slots.");
        ScreenCapture.CaptureScreenshot(Output + "UnlimitedTwelveCardHand4K.png");
        yield return null;
        Require(controller.TrySurface(), "Reel Surface failed.");
        deadline = Time.realtimeSinceStartup + 20; while (!modal.IsVisible && Time.realtimeSinceStartup < deadline) yield return null;
        Require(modal.IsVisible, "Reel result missing."); modal.NextRunButton.onClick.Invoke(); flow.Navigate(1); yield return null;
        Click(flow, "Inspect BARBED HOOK"); Click(flow, "Purchase or Equip"); Click(flow, "Purchase or Equip");
        Require(controller.TotalGold == 550 && controller.TackleShop.EquippedId == "barbed_hook", "Hook purchase/equip failed.");
        flow.Navigate(0); Require(flow.TryStartNewRun(), "Hook run did not start.");
        var chain = (CatchChainRuntime)Field(controller, "catchChainRuntime").GetValue(controller); var resolver = (EffectResolver)Field(controller, "effectResolver").GetValue(controller);
        var cod = FindCard("creature_coastal_cod"); for (int i = 0; i < 4; i++) chain.Add(cod, resolver);
        Require(chain.Catches.Take(3).All(c => c.CurrentValue == cod.Value + 1) && chain.Catches[3].CurrentValue == cod.Value, "Controller Hook grants failed.");
        Require(controller.TechniqueHand.Length == 4 && controller.LineCapacity == baseline, "Unequipped bonuses leaked.");
        Require(controller.TrySurface(), "Hook Surface failed.");
        deadline = Time.realtimeSinceStartup + 20; while (!modal.IsVisible && Time.realtimeSinceStartup < deadline) yield return null;
        Require(modal.IsVisible && controller.LastHaulValue == cod.Value * 4 + 3 && controller.TotalGold == 550 + controller.LastHaulValue, "Hook sale/gold failed.");
        var keys = JsonUtility.FromJson<Keys>(File.ReadAllText(Output + "TestKeys.json"));
        var reload = new TackleShopRuntime(controller.TackleShop.Catalog, 0, 0, () => PlayerPrefs.GetString(keys.progression), _ => { });
        Require(reload.Gold == controller.TotalGold && reload.EquippedId == "barbed_hook" && reload.Catalog.Items.All(i => reload.Owns(i.Id)), "Persistent ownership/wallet/loadout failed.");
        modal.NextRunButton.onClick.Invoke(); flow.Navigate(1);
    }

    /// <summary>Reports frame-based failures without silently treating a partial run as verified.</summary>
    private static IEnumerator Guard(IEnumerator routine)
    {
        while (true)
        {
            bool more;
            try { more = routine.MoveNext(); }
            catch (Exception error) { Status = "Failed: " + error; Debug.LogException(error); yield break; }
            if (!more) break; yield return routine.Current;
        }
        Status = "Passed"; File.WriteAllText(Output + "PlayVerification.json", "{\"passed\":true,\"assertions\":" + assertions + "}");
    }

    /// <summary>Removes only the recorded isolated test keys after leaving Play Mode and its final save flush.</summary>
    public static void CleanupTestSaves(string output = "Docs/Validation/TackleShop/")
    {
        Output = output;
        if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode before cleaning test keys.");
        var keys = JsonUtility.FromJson<Keys>(File.ReadAllText(Output + "TestKeys.json"));
        if (!keys.progression.StartsWith("FishingCards.EquipmentTest.", StringComparison.Ordinal) || keys.journal != keys.progression + ".Journal") throw new InvalidOperationException("Unexpected test key.");
        PlayerPrefs.DeleteKey(keys.progression); PlayerPrefs.DeleteKey(keys.journal); PlayerPrefs.Save();
    }

    /// <summary>Checks the authored live pointer target before dispatching an actual Button click.</summary>
    private static void Click(JournalHarborFlow flow, string name)
    {
        Canvas.ForceUpdateCanvases(); var button = flow.GetComponentsInChildren<UnityEngine.UI.Button>().Single(b => b.name == name);
        var rect = button.GetComponent<RectTransform>(); var pointer = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center)), button = PointerEventData.InputButton.Left };
        var hits = new System.Collections.Generic.List<RaycastResult>(); EventSystem.current.RaycastAll(pointer, hits);
        Require(hits.Count > 0 && hits[0].gameObject.GetComponentInParent<UnityEngine.UI.Button>() == button && button.interactable, "Shop pointer target blocked: " + name);
        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
    }

    /// <summary>Finds an exact approved card identity for effect integration fixtures.</summary>
    private static CardDefinition FindCard(string id) { return AssetDatabase.FindAssets("t:CardDefinition").Select(g => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).First(c => c.UniqueId == id); }
    /// <summary>Accesses existing runtime owners only for isolated verification setup.</summary>
    private static FieldInfo Field(object owner, string name) { return owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic); }
    /// <summary>Stops on the first violated transaction or effect boundary.</summary>
    private static void Require(bool condition, string message) { assertions++; if (!condition) throw new InvalidOperationException(message); }
    [Serializable] private sealed class Keys { public string progression, journal; }
}
