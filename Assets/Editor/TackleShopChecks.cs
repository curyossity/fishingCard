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
        Require(catalog.Items.Length == 9 && catalog.Items.Select(x => x.Price).SequenceEqual(new[] { 100, 250, 350, 150, 300, 250, 100, 250, 300 }), "Approved prices differ.");
        string reason;
        Require(shop.EquippedItems.Length == 0 && catalog.Items.All(x => !shop.Owns(x.Id)), "New players received equipment.");
        foreach (var item in catalog.Items)
        {
            var boundary = new TackleShopRuntime(catalog, item.Price - 1, 0, () => null, _ => { });
            Require(!boundary.TryPurchase(item.Id, item.Price - 1, 0, false, out reason), "Below-price purchase allowed: " + item.Id);
            boundary = new TackleShopRuntime(catalog, item.Price, 0, () => null, _ => { });
            Require(boundary.TryPurchase(item.Id, item.Price, 0, false, out reason) && boundary.Gold == 0
                && boundary.Owns(item.Id) && boundary.EquippedItems.Length == 0 && catalog.Find(item.Id).Price == item.Price,
                "Exact-price purchase, optional equip or fixed pricing failed: " + item.Id);
        }
        Require(!shop.TryPurchase("driftwood_rod", 99, 0, false, out reason), "Insufficient gold allowed.");
        Require(!shop.TryPurchase("driftwood_rod", 1000, 0, true, out reason), "Purchase during expedition allowed.");
        Require(!shop.TryPurchase("unknown", 1000, 0, false, out reason), "Unknown item allowed.");
        Require(shop.TryPurchase("driftwood_rod", 1000, 0, false, out reason) && shop.Gold == 900 && shop.Owns("driftwood_rod") && shop.EquippedItems.Length == 0, "Purchase or separate equip failed.");
        Require(!shop.TryPurchase("driftwood_rod", 900, 0, false, out reason) && shop.Gold == 900, "Duplicate purchase deducted gold.");
        Require(!shop.TryEquip("barbed_hook", 900, 0, false, out reason), "Unowned item equipped.");
        Require(shop.TryEquip("driftwood_rod", 900, 0, false, out reason), "Owned item could not equip.");
        shop = new TackleShopRuntime(catalog, 0, 0, () => saved, json => saved = json);
        Require(shop.Gold == 900 && shop.IsEquipped("driftwood_rod"), "Wallet/loadout reload failed.");
        Require(shop.TryPurchase("tideglass_reel", 900, 0, false, out reason) && shop.TryEquip("tideglass_reel", 750, 0, false, out reason) && shop.IsEquipped("driftwood_rod") && shop.IsEquipped("tideglass_reel"), "Category loadout selection failed.");
        Require(shop.TryEquip("tideglass_reel", 750, 0, false, out reason) && shop.EquippedItems.Length == 2, "Repeated equip duplicated a category.");
        Require(shop.TryUnequip(EquipmentSlot.Rod, 750, 0, false, out reason) && !shop.IsEquipped("driftwood_rod") && shop.IsEquipped("tideglass_reel"), "Unequip cleared another category.");
        var migrated = new TackleShopRuntime(catalog, 0, 0, () => "{\"version\":1,\"gold\":123,\"lineCapacityUpgrades\":2,\"owned\":[\"barbed_hook\"],\"equipped\":\"barbed_hook\"}", _ => { });
        Require(migrated.Gold == 123 && migrated.LineCapacityUpgrades == 2 && migrated.GetEquipped(EquipmentSlot.Hook)?.Id == "barbed_hook", "Legacy save migration lost progression.");
        Require(!shop.TryEquip("driftwood_rod", 750, 0, true, out reason), "Active equipment changed.");
        Require(shop.SaveWallet(900, 2), "Ordinary gold/upgrade save failed.");
        var reloaded = new TackleShopRuntime(catalog, 0, 0, () => saved, _ => { });
        Require(reloaded.Gold == 900 && reloaded.LineCapacityUpgrades == 2, "Existing capacity purchase persistence failed.");
        bool fail = true;
        var failure = new TackleShopRuntime(catalog, 500, 0, () => null, _ => { if (fail) throw new IOException("Deliberate test failure"); });
        Require(!failure.TryPurchase("barbed_hook", 500, 0, false, out reason) && !failure.Owns("barbed_hook") && failure.Gold == 500, "Failed save partially applied transaction.");
        fail = false; Require(failure.TryPurchase("barbed_hook", 500, 0, false, out reason) && failure.Gold == 400, "Failed save could not retry.");
        bool wrote = false;
        var corrupt = new TackleShopRuntime(catalog, 0, 0, () => "{\"version\":99}", _ => wrote = true);
        Require(!corrupt.TryPurchase("driftwood_rod", 999, 0, false, out reason) && !wrote, "Unsupported save overwritten.");

        string allSaved = null;
        var all = new TackleShopRuntime(catalog, 10000, 0, () => allSaved, json => allSaved = json);
        foreach (var item in catalog.Items.Reverse())
        {
            Require(all.TryPurchase(item.Id, all.Gold, 0, false, out reason), "Catalog item purchase failed: " + item.Id);
            var otherSlots = all.EquippedItems.Where(x => x.Slot != item.Slot).Select(x => x.Id).ToArray();
            Require(all.TryEquip(item.Id, all.Gold, 0, false, out reason)
                && otherSlots.All(all.IsEquipped) && all.EquippedItems.Count(x => x.Slot == item.Slot) == 1,
                "Same-category replacement disturbed the loadout: " + item.Id);
        }
        all = new TackleShopRuntime(catalog, 0, 0, () => allSaved, _ => { });
        Require(catalog.Items.All(x => all.Owns(x.Id)) && all.EquippedItems.Length == 3 && all.Gold == 7950,
            "Nine-item ownership/loadout failed to reload.");

        EquipmentPassiveChecks.Validate(Output + "PassiveEngine/");
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
        Require(controller.TotalGold == 1100 && controller.TackleShop.Owns("driftwood_rod") && controller.TackleShop.EquippedItems.Length == 0, "Pointer purchase failed.");
        Click(flow, "Purchase or Equip"); Require(controller.TackleShop.IsEquipped("driftwood_rod"), "Pointer equip failed.");
        string reason;
        Require(!controller.TryPurchaseEquipment("driftwood_rod", out reason) && controller.TotalGold == 1100, "Duplicate purchase allowed.");
        flow.Navigate(0); Require(flow.TryStartNewRun(), "Rod run did not start.");
        int baseline = (int)Field(controller, "startingLineCapacity").GetValue(controller);
        Require(controller.LineCapacity == baseline && controller.TechniqueHand.Length == 4, "Rod start benefit wrong.");
        Require(!controller.TryPurchaseEquipment("tideglass_reel", out reason) && !controller.TryEquipEquipment("", out reason), "Active-run shop mutation allowed.");
        Require(controller.TrySurface(), "Rod Surface failed.");
        var modal = UnityEngine.Object.FindAnyObjectByType<JournalResultModal>();
        float deadline = Time.realtimeSinceStartup + 20; while (!modal.IsVisible && Time.realtimeSinceStartup < deadline) yield return null;
        Require(modal.IsVisible, "Rod result missing."); modal.NextRunButton.onClick.Invoke(); flow.Navigate(1); yield return null;
        flow.GetComponentInChildren<JournalTackleShopView>(true).Filter(2); yield return null;
        Click(flow, "Inspect TIDEGLASS REEL"); Click(flow, "Purchase or Equip"); Click(flow, "Purchase or Equip");
        Require(controller.TotalGold == 950 && controller.TackleShop.IsEquipped("tideglass_reel"), "Reel purchase/equip failed.");
        flow.Navigate(0); Require(flow.TryStartNewRun(), "Reel run did not start.");
        Require(controller.TechniqueHand.Length == 5 && controller.LineCapacity == baseline, "Rod and Reel combined benefits failed.");
        var liveDeck = (TechniqueDeckRuntime)Field(controller, "techniqueDeckRuntime").GetValue(controller);
        Require(liveDeck.DrawTechniques(20, new System.Random(1)) == 7 && liveDeck.Hand.Length == 12,
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
        flow.GetComponentInChildren<JournalTackleShopView>(true).Filter(3); yield return null;
        Click(flow, "Inspect BARBED HOOK"); Click(flow, "Purchase or Equip"); Click(flow, "Purchase or Equip");
        Require(controller.TotalGold == 850 && controller.TackleShop.IsEquipped("barbed_hook"), "Hook purchase/equip failed.");
        flow.Navigate(0); Require(flow.TryStartNewRun(), "Hook run did not start.");
        var chain = (CatchChainRuntime)Field(controller, "catchChainRuntime").GetValue(controller); var resolver = (EffectResolver)Field(controller, "effectResolver").GetValue(controller);
        var cod = FindCard("creature_coastal_cod"); for (int i = 0; i < 4; i++) chain.Add(cod, resolver);
        Require(chain.Catches[0].CurrentValue == cod.Value && chain.Catches[1].CurrentValue == cod.Value && chain.Catches[2].CurrentValue == cod.Value + 2 && chain.Catches[3].CurrentValue == cod.Value, "Controller Hook grants failed.");
        Require(controller.TechniqueHand.Length == 5 && controller.LineCapacity == baseline, "Combined equipment benefits missing.");
        Require(controller.TrySurface(), "Hook Surface failed.");
        deadline = Time.realtimeSinceStartup + 20; while (!modal.IsVisible && Time.realtimeSinceStartup < deadline) yield return null;
        Require(modal.IsVisible && controller.LastHaulValue == cod.Value * 4 + 2 && controller.TotalGold == 850 + controller.LastHaulValue, "Hook sale/gold failed.");
        var keys = JsonUtility.FromJson<Keys>(File.ReadAllText(Output + "TestKeys.json"));
        var reload = new TackleShopRuntime(controller.TackleShop.Catalog, 0, 0, () => PlayerPrefs.GetString(keys.progression), _ => { });
        Require(reload.Gold == controller.TotalGold && reload.EquippedItems.Length == 3 && new[] { "driftwood_rod", "tideglass_reel", "barbed_hook" }.All(reload.Owns), "Persistent ownership/wallet/loadout failed.");
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
