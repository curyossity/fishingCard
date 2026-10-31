using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Authors the approved equipment data and binds the existing ornate Tackle Shop without replacing its artwork.</summary>
public static class JournalTackleShopSetup
{
    public const string Root = JournalHarborBuilder.Root + "TackleShop/";
    public const string CatalogPath = Root + "TackleShopCatalog.asset";

    /// <summary>Creates GUID-stable equipment definitions and installs live controls in the candidate hub.</summary>
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Install outside Play Mode.");
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.path != JournalComparisonSceneBuilder.ScenePath || scene.isDirty) throw new InvalidOperationException("Open the clean candidate scene.");
        CreateCatalog();
        var root = PrefabUtility.LoadPrefabContents(JournalHarborBuilder.PrefabPath);
        try { Bind(root); PrefabUtility.SaveAsPrefabAsset(root, JournalHarborBuilder.PrefabPath); }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        var controller = UnityEngine.Object.FindAnyObjectByType<FishingRunController>(); var data = new SerializedObject(controller);
        data.FindProperty("tackleShopCatalog").objectReferenceValue = AssetDatabase.LoadAssetAtPath<TackleShopCatalog>(CatalogPath);
        data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(controller); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
    }

    /// <summary>Restores this binding during normal hub prefab regeneration.</summary>
    public static void Bind(GameObject root)
    {
        if (AssetDatabase.LoadAssetAtPath<TackleShopCatalog>(CatalogPath) == null) return;
        var theme = AssetDatabase.LoadAssetAtPath<MidnightFieldJournalTheme>(MidnightFieldJournalAssetSetup.ThemePath);
        var flow = root.GetComponent<JournalHarborFlow>(); var stage = root.transform.Find("Hub Content/16x9 Design Stage");
        Transform shop = stage.Find("shop"), harbor = stage.Find("harbor"), collection = stage.Find("collection");
        var view = shop.GetComponent<JournalTackleShopView>() ?? shop.gameObject.AddComponent<JournalTackleShopView>();
        string[] names = { "DRIFTWOOD ROD", "TIDEGLASS REEL", "BARBED HOOK" };
        string[] oldStates = { "EQUIPPED", "AVAILABLE", "JOURNAL LOCKED" }, oldPrices = { "In your collection", "180 GOLD", "Complete a discovery" };
        var states = new TMP_Text[3]; var effects = new TMP_Text[3]; var buttons = new UnityEngine.UI.Button[3]; var portraits = new Sprite[3];
        for (int i = 0; i < 3; i++)
        {
            states[i] = FindText(shop, "Editable / " + oldStates[i]); effects[i] = FindText(shop, "Editable / " + oldPrices[i]);
            var rect = effects[i].rectTransform; rect.sizeDelta = new Vector2(620, 175); effects[i].fontSizeMax = 35; effects[i].fontSizeMin = 29; effects[i].fontSize = 35;
            buttons[i] = Button(shop, "Inspect " + names[i], new Rect(216 + i * 769.4f, 824, i == 1 ? 730 : 707, 850));
            buttons[i].onClick.RemoveAllListeners(); while (buttons[i].onClick.GetPersistentEventCount() > 0) UnityEventTools.RemovePersistentListener(buttons[i].onClick, 0);
            UnityEventTools.AddIntPersistentListener(buttons[i].onClick, view.Select, i);
            portraits[i] = shop.GetComponentsInChildren<UnityEngine.UI.Image>(true).Where(x => x.name.StartsWith("Independent engraved equipment /", StringComparison.Ordinal) && x.name.Contains(i == 0 ? "Driftwood" : i == 1 ? "Tideglass" : "Barbed")).OrderBy(x => x.rectTransform.anchoredPosition.x).First().sprite;
        }
        string[] categories = { "ALL EQUIPMENT", "RODS", "REELS", "HOOKS" };
        for (int i = 0; i < categories.Length; i++)
        {
            var tab = FindText(shop, "Editable / " + categories[i]); var r = tab.rectTransform;
            var button = Button(shop, "Browse " + categories[i], new Rect(r.anchoredPosition.x, -r.anchoredPosition.y, r.sizeDelta.x, r.sizeDelta.y));
            while (button.onClick.GetPersistentEventCount() > 0) UnityEventTools.RemovePersistentListener(button.onClick, 0);
            UnityEventTools.AddIntPersistentListener(button.onClick, view.Select, Math.Max(0, i - 1));
        }
        var inactive = shop.Find("Purchasing unavailable"); if (inactive != null) UnityEngine.Object.DestroyImmediate(inactive.gameObject);
        var action = Button(shop, "Purchase or Equip", new Rect(2648, 1678, 955, 143));
        while (action.onClick.GetPersistentEventCount() > 0) UnityEventTools.RemovePersistentListener(action.onClick, 0);
        UnityEventTools.AddPersistentListener(action.onClick, view.Activate);
        var detailEffect = FindText(shop, "Editable / Unlock with gold. Equip one item for your next expedition.");
        detailEffect.rectTransform.anchoredPosition = new Vector2(2648, -1400); detailEffect.rectTransform.sizeDelta = new Vector2(942, 215); detailEffect.fontSizeMax = 41; detailEffect.fontSizeMin = 33;
        FindText(shop, "Editable / Unlocking does not replace your loadout.").text = "Buy once. Equip between expeditions.";
        var message = shop.GetComponentsInChildren<TMP_Text>(true).First(x => x.name.Contains("The right tool")); message.fontSizeMax = 39; message.fontSizeMin = 29;
        var detailArt = shop.GetComponentsInChildren<UnityEngine.UI.Image>(true).First(x => x.name.StartsWith("Independent engraved equipment /", StringComparison.Ordinal) && x.rectTransform.anchoredPosition.x > 2500);
        var data = new SerializedObject(view);
        data.FindProperty("flow").objectReferenceValue = flow;
        Set(data, "cardStates", states); Set(data, "cardEffects", effects); Set(data, "itemButtons", buttons); Set(data, "portraits", portraits);
        Set(data, "loadoutLabels", new[] { FindText(shop, "Editable / Driftwood Rod equipped"), harbor.GetComponentsInChildren<TMP_Text>(true).First(x => x.name.StartsWith("Editable / Driftwood Rod", StringComparison.Ordinal)) });
        Set(data, "loadoutHeadings", new[] { shop.GetComponentsInChildren<TMP_Text>(true).First(x => x.name.StartsWith("Editable / CURRENT LOADOUT", StringComparison.Ordinal)) });
        data.FindProperty("detailCategory").objectReferenceValue = FindText(shop, "Editable / REEL / PERMANENT UNLOCK");
        data.FindProperty("detailName").objectReferenceValue = shop.GetComponentsInChildren<TMP_Text>(true).Where(x => x.name == "Editable / TIDEGLASS REEL").OrderByDescending(x => x.rectTransform.anchoredPosition.x).First();
        data.FindProperty("detailEffect").objectReferenceValue = detailEffect;
        data.FindProperty("detailState").objectReferenceValue = FindText(shop, "Editable / ONE EQUIPMENT SLOT");
        data.FindProperty("actionLabel").objectReferenceValue = shop.GetComponentsInChildren<TMP_Text>(true).First(x => x.name.StartsWith("Editable / UNLOCK", StringComparison.Ordinal));
        data.FindProperty("message").objectReferenceValue = message; data.FindProperty("action").objectReferenceValue = action; data.FindProperty("detailArt").objectReferenceValue = detailArt;
        data.FindProperty("collectionHand").objectReferenceValue = collection.GetComponentsInChildren<TMP_Text>(true).First(x => x.text.StartsWith("Starting hand:", StringComparison.Ordinal));
        data.ApplyModifiedPropertiesWithoutUndo();
        // Authored edit-mode defaults reflect unowned equipment without reading or changing player saves.
        var catalog = AssetDatabase.LoadAssetAtPath<TackleShopCatalog>(CatalogPath); var items = catalog.Items;
        for (int i = 0; i < 3; i++) { states[i].text = items[i].Price + " GOLD"; states[i].color = new Color(.81f, .66f, .40f); effects[i].text = items[i].Description; }
        FindText(shop, "Editable / ONE EQUIPMENT SLOT").text = "PERMANENT UNLOCK"; detailEffect.text = items[0].Description;
        ((TMP_Text)data.FindProperty("detailName").objectReferenceValue).text = items[0].DisplayName.ToUpperInvariant();
        ((TMP_Text)data.FindProperty("detailCategory").objectReferenceValue).text = "CAPACITY-FOCUSED · 200 GOLD";
        ((TMP_Text)data.FindProperty("actionLabel").objectReferenceValue).text = "BUY · 200 GOLD";
        detailArt.sprite = portraits[0]; detailArt.preserveAspect = true; message.text = "Buy once. Equip one item for your next expedition.";
        var loadouts = data.FindProperty("loadoutLabels");
        for (int i = 0; i < loadouts.arraySize; i++) ((TMP_Text)loadouts.GetArrayElementAtIndex(i).objectReferenceValue).text = "No equipment selected";
        ((TMP_Text)data.FindProperty("loadoutHeadings").GetArrayElementAtIndex(0).objectReferenceValue).text = "CURRENT LOADOUT · 0 / 1";
    }

    /// <summary>Creates only the three approved prices/benefits and exact source identities used by stat inspection.</summary>
    private static void CreateCatalog()
    {
        MidnightFieldJournalAssetSetup.EnsureFolder(Root);
        string[] ids = { "driftwood_rod", "tideglass_reel", "barbed_hook" }, names = { "Driftwood Rod", "Tideglass Reel", "Barbed Hook" }, categories = { "Capacity-focused", "Technique-focused", "Profit-focused" };
        string[] descriptions = { "Start each expedition with +1 Line Capacity.", "Draw 2 additional Technique cards at the start of each expedition.", "The first 3 creatures you Catch each expedition gain +1 sale value each while attached." };
        string[] art = { "rod", "reel", "hook" }; int[] prices = { 200, 200, 250 }, amounts = { 1, 2, 1 };
        var sources = new CardDefinition[3];
        for (int i = 0; i < 3; i++)
        {
            string path = Root + ids[i] + ".asset"; var source = AssetDatabase.LoadAssetAtPath<CardDefinition>(path);
            if (source == null) { source = ScriptableObject.CreateInstance<CardDefinition>(); AssetDatabase.CreateAsset(source, path); }
            var data = new SerializedObject(source); data.FindProperty("uniqueId").stringValue = "equipment_" + ids[i]; data.FindProperty("displayName").stringValue = names[i];
            data.FindProperty("cardType").enumValueIndex = (int)CardType.Equipment; data.FindProperty("rulesText").stringValue = descriptions[i];
            data.FindProperty("artwork").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(JournalHarborBuilder.Root + "Art/" + art[i] + ".png");
            var effects = data.FindProperty("effects"); effects.arraySize = i == 2 ? 1 : 0;
            if (i == 2)
            {
                var effect = effects.GetArrayElementAtIndex(0);
                effect.FindPropertyRelative("effectId").stringValue = "equipment_" + ids[i]; effect.FindPropertyRelative("displayName").stringValue = names[i];
                effect.FindPropertyRelative("effectType").enumValueIndex = (int)CardEffectType.ModifyCatchValue; effect.FindPropertyRelative("trigger").enumValueIndex = (int)CardEffectTrigger.WhenCaught;
                effect.FindPropertyRelative("target").enumValueIndex = (int)CardEffectTarget.Self; effect.FindPropertyRelative("effectTone").enumValueIndex = (int)CardEffectTone.Positive; effect.FindPropertyRelative("amount").intValue = 1;
            }
            data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(source); AssetDatabase.SaveAssetIfDirty(source); sources[i] = source;
        }
        var catalog = AssetDatabase.LoadAssetAtPath<TackleShopCatalog>(CatalogPath);
        if (catalog == null) { catalog = ScriptableObject.CreateInstance<TackleShopCatalog>(); AssetDatabase.CreateAsset(catalog, CatalogPath); }
        var c = new SerializedObject(catalog); var items = c.FindProperty("items"); items.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            var item = items.GetArrayElementAtIndex(i); item.FindPropertyRelative("id").stringValue = ids[i]; item.FindPropertyRelative("displayName").stringValue = names[i]; item.FindPropertyRelative("category").stringValue = categories[i];
            item.FindPropertyRelative("description").stringValue = descriptions[i]; item.FindPropertyRelative("price").intValue = prices[i]; item.FindPropertyRelative("amount").intValue = amounts[i];
            item.FindPropertyRelative("catchLimit").intValue = i == 2 ? 3 : 0; item.FindPropertyRelative("benefit").enumValueIndex = i; item.FindPropertyRelative("sourceCard").objectReferenceValue = sources[i];
        }
        c.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssetIfDirty(catalog); catalog.Validate();
    }

    /// <summary>Finds a stable authored native label rather than replacing adjacent UI.</summary>
    private static TMP_Text FindText(Transform parent, string name) { return parent.GetComponentsInChildren<TMP_Text>(true).First(x => x.name == name); }

    /// <summary>Persists a typed reference array through the existing prefab serialization workflow.</summary>
    private static void Set(SerializedObject data, string name, UnityEngine.Object[] values)
    {
        var field = data.FindProperty(name); field.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) field.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
    }

    /// <summary>Adds reusable selection/purchase hit regions with the existing non-framed hover feedback.</summary>
    private static UnityEngine.UI.Button Button(Transform parent, string name, Rect bounds)
    {
        var existing = parent.Find(name); if (existing != null) return existing.GetComponent<UnityEngine.UI.Button>();
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1); rect.anchoredPosition = new Vector2(bounds.x, -bounds.y); rect.sizeDelta = bounds.size;
        var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>(); image.color = Color.white;
        var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = image; var colors = button.colors;
        colors.normalColor = Color.clear; colors.highlightedColor = new Color(1, .94f, .78f, .06f); colors.selectedColor = new Color(1, .94f, .78f, .08f); colors.pressedColor = new Color(1, .94f, .78f, .13f); button.colors = colors; return button;
    }
}
