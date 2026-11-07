using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Validates approved image imports and live artwork bindings independently from encounter-rule tests.</summary>
public static class TackleDiscoverArtChecks
{
    /// <summary>Checks unchanged exported bytes, native import settings, licensed fonts and serialized runtime fields.</summary>
    public static int ValidateAssets()
    {
        int checks = 0;
        Action<bool, string> assert = (value, message) =>
        { if (!value) throw new InvalidOperationException(message); checks++; };
        string root = TackleDiscoverArtSetup.Root;
        string source = "Docs/UIThemes/TackleApparatusConcept/AssetLibrary/Exports/";
        var files = Directory.GetFiles(root + "Art", "*.png", SearchOption.AllDirectories);
        assert(files.Length == 37, "Discover asset selection changed unexpectedly.");
        foreach (string file in files)
        {
            string path = file.Replace('\\', '/');
            string original = source + path.Substring((root + "Art/").Length);
            assert(File.ReadAllBytes(path).SequenceEqual(File.ReadAllBytes(original)), "Approved image pixels changed: " + path);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            assert(importer != null && importer.textureType == TextureImporterType.Sprite
                && importer.spriteImportMode == SpriteImportMode.Single, "Invalid sprite import: " + path);
            assert(importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && !importer.isReadable
                && importer.textureCompression == TextureImporterCompression.Uncompressed && importer.maxTextureSize == 2048,
                "Import settings do not preserve the initial artwork: " + path);
        }
        foreach (string name in new[] { "PressStart2P", "VT323" })
            assert(AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(root + "Fonts/" + name + ".asset") != null, "Missing embedded font: " + name);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TackleDiscoverArtSetup.PrefabPath);
        assert(prefab != null, "Discover prefab was not built.");
        var canvas = prefab.GetComponent<Canvas>();
        var scaler = prefab.GetComponent<UnityEngine.UI.CanvasScaler>();
        assert(canvas.renderMode == RenderMode.ScreenSpaceOverlay && canvas.sortingOrder == 70
            && scaler.referenceResolution == new Vector2(1920, 1080)
            && scaler.screenMatchMode == UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand, "Unexpected canvas scaling/layering.");
        var data = new SerializedObject(prefab.GetComponent<SlotMachineView>());
        assert(data.FindProperty("artwork").objectReferenceValue != null, "Live slot commands are not bound to the artwork.");
        assert(data.FindProperty("lockButtons").arraySize == 3 && data.FindProperty("reelLabels").arraySize == 3,
            "Three reels/locks were not authored.");
        foreach (string name in new[] { "respinButton", "acceptButton", "heading", "resultDescription" })
            assert(data.FindProperty(name).objectReferenceValue != null, "Missing live field: " + name);
        var presentation = new SerializedObject(prefab.GetComponent<TackleDiscoverPresentation>());
        assert(presentation.FindProperty("tagArtwork").arraySize == 2, "Unavailable tag illustrations were silently substituted.");
        assert(presentation.FindProperty("rigCardPrefab").objectReferenceValue != null
            && presentation.FindProperty("rigScroll").objectReferenceValue != null, "Scrollable rig was not bound.");
        foreach (var image in prefab.GetComponentsInChildren<UnityEngine.UI.Image>(true))
            if (image.sprite != null)
                assert(AssetDatabase.GetAssetPath(image.sprite).StartsWith(root, StringComparison.Ordinal), "Another artwork theme leaked into Discover.");
        return checks;
    }

    /// <summary>Checks real UI raycasts, focus/press/disabled states and long-rig reuse without changing gameplay ownership.</summary>
    public static int ValidateLiveArtwork()
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Enter a fresh comparison Play Mode session.");
        const string output = "Docs/Validation/TackleDiscoverArt/";
        int checks = 0;
        Action<bool, string> assert = (value, message) =>
        { if (!value) throw new InvalidOperationException(message); checks++; };
        var harbor = UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>();
        var controller = harbor.Controller;
        assert(!controller.RunActive, "Artwork check requires a fresh Harbor session.");
        controller.TryPrepareSlotTags(new[] { "Fish", "Predator", "Schooling", "Small" });
        harbor.TryStartNewRun();
        var preparation = new SerializedObject(harbor.GetComponentInChildren<SlotTagPreparationView>(true));
        ((UnityEngine.UI.Button)preparation.FindProperty("confirm").objectReferenceValue).onClick.Invoke();
        var view = UnityEngine.Object.FindAnyObjectByType<SlotMachineView>();
        var artwork = view.GetComponent<TackleDiscoverPresentation>();
        var viewData = new SerializedObject(view); var artData = new SerializedObject(artwork);
        var accept = (UnityEngine.UI.Button)viewData.FindProperty("acceptButton").objectReferenceValue;
        var respin = (UnityEngine.UI.Button)viewData.FindProperty("respinButton").objectReferenceValue;
        var firstLock = (UnityEngine.UI.Button)viewData.FindProperty("lockButtons").GetArrayElementAtIndex(0).objectReferenceValue;
        Canvas.ForceUpdateCanvases();
        foreach (var button in new[] { accept, firstLock })
        {
            var rect = (RectTransform)button.transform;
            var pointer = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
            { position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center)) };
            var hits = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
            UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointer, hits);
            assert(hits.Count > 0 && hits[0].gameObject == button.gameObject, "Artwork intercepts button input: " + button.name);
        }
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(accept.gameObject);
        var feedback = new SerializedObject(accept.GetComponent<TackleButtonFeedback>());
        assert(((GameObject)feedback.FindProperty("focusCorners").objectReferenceValue).activeSelf, "Keyboard focus corners absent.");
        var visual = (RectTransform)feedback.FindProperty("visual").objectReferenceValue;
        Vector2 origin = visual.anchoredPosition;
        var press = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
        accept.GetComponent<TackleButtonFeedback>().OnPointerDown(press);
        assert(visual.anchoredPosition == origin + Vector2.down * 4, "Pressed artwork did not move four pixels.");
        accept.GetComponent<TackleButtonFeedback>().OnPointerUp(press);
        assert(visual.anchoredPosition == origin, "Pressed artwork did not restore its position.");
        assert(!respin.interactable && respin.GetComponentInChildren<CanvasGroup>().alpha < .5f, "Disabled respin was not visibly disabled.");
        firstLock.onClick.Invoke();
        var lever = (UnityEngine.UI.Image)artData.FindProperty("lockLevers").GetArrayElementAtIndex(0).objectReferenceValue;
        assert(lever.sprite == artData.FindProperty("lockedLever").objectReferenceValue, "Locked lever artwork did not follow state.");
        assert(respin.interactable && respin.GetComponentInChildren<CanvasGroup>().alpha == 1, "Locked result did not enable respin artwork.");
        SlotMachineSceneChecks.Capture(view.GetComponent<Canvas>(), "Locked1080.png", output: output);
        firstLock.onClick.Invoke();
        assert(lever.sprite == artData.FindProperty("openLever").objectReferenceValue, "Unlocked lever artwork was not restored.");
        var definitions = AssetDatabase.FindAssets("t:CardDefinition", new[] { "Assets/Cards" })
            .Select(id => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(id))).ToArray();
        string[] ids = { "creature_sardine", "creature_mackerel", "creature_glass_minnow" };
        var clones = new System.Collections.Generic.List<CardDefinition>();
        var refresh = typeof(TackleDiscoverPresentation).GetMethod("RefreshRig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        try
        {
            for (int i = 0; i < ids.Length; i++)
            {
                var source = definitions.First(card => card.UniqueId == ids[i]);
                var clone = UnityEngine.Object.Instantiate(source); clones.Add(clone);
                var data = new SerializedObject(clone); data.FindProperty("value").intValue = i == 0 ? 7 : 5;
                data.FindProperty("effects").arraySize = 0; data.FindProperty("baitEffects").arraySize = 0;
                data.ApplyModifiedPropertiesWithoutUndo();
            }
            var rig = Enumerable.Range(0, 8).Select(i => new CardInstance(1000 + i, clones[i % 3], null,
                i % 3 == 1 ? CatchAttachmentRole.Bait : CatchAttachmentRole.Catch)).ToArray();
            refresh.Invoke(artwork, new object[] { rig.Take(3).ToArray() });
            Canvas.ForceUpdateCanvases();
            var haul = (TMPro.TMP_Text)artData.FindProperty("haulLabel").objectReferenceValue;
            assert(haul.text == "HAUL 12", "Fixture Catch/Bait value display is incorrect.");
            SlotMachineSceneChecks.Capture(view.GetComponent<Canvas>(), "RigFixture1080.png", output: output);
            refresh.Invoke(artwork, new object[] { rig });
            Canvas.ForceUpdateCanvases();
            var content = (RectTransform)artData.FindProperty("rigContent").objectReferenceValue;
            var cards = content.GetComponentsInChildren<TackleRigCardView>();
            assert(cards.Length == 8, "Rig was capped at the three illustrated rows.");
            var remembered = cards.First(card => ReferenceEquals(card.BoundInstance, rig[1]));
            var scroll = (UnityEngine.UI.ScrollRect)artData.FindProperty("rigScroll").objectReferenceValue;
            assert(content.rect.height > scroll.viewport.rect.height, "Long rig did not extend beyond its viewport.");
            scroll.verticalNormalizedPosition = 0;
            assert(content.anchoredPosition.y > 0, "Long rig did not scroll.");
            refresh.Invoke(artwork, new object[] { rig.Skip(1).ToArray() });
            assert(remembered.gameObject.activeSelf && ReferenceEquals(remembered.BoundInstance, rig[1]), "Removal transferred a physical card to another instance.");
            refresh.Invoke(artwork, new object[] { Array.Empty<CardInstance>() });
            assert(controller.CatchChain.Length == 0 && haul.text == "HAUL 0", "Artwork fixture mutated the actual rig.");
            foreach (var size in new[] { new Vector2Int(1920, 1080), new Vector2Int(1280, 800), new Vector2Int(2560, 1080) })
                SlotMachineSceneChecks.Capture(view.GetComponent<Canvas>(), "Discover-" + size.x + "x" + size.y + ".png", size.x, size.y, output);
        }
        finally
        {
            refresh.Invoke(artwork, new object[] { controller.CatchChain });
            foreach (var clone in clones) UnityEngine.Object.DestroyImmediate(clone);
        }
        return checks;
    }
}
