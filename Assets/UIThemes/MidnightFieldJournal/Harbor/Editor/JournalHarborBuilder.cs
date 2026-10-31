using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Imports the current Penpot hub sources and builds native, editable journal pages in the isolated candidate scene.</summary>
public static class JournalHarborBuilder
{
    public const string Root = "Assets/UIThemes/MidnightFieldJournal/Harbor/";
    public const string PrefabPath = Root + "Prefabs/JournalHarbor.prefab";
    [Serializable] private sealed class Layout { public Page[] pages; public Resource[] resources; }
    [Serializable] private sealed class Page { public string id; public Node[] nodes; }
    [Serializable] private sealed class Resource { public string id, path; }
    [Serializable] private sealed class Node
    {
        public string kind, name, resource, text, color, family, align, role;
        public float x, y, width, height, opacity = 1, fontSize;
    }
    private static readonly Color Ink = new Color32(16, 38, 46, 255);

    /// <summary>Builds the current authored hub prefab and installs it into the clean comparison scene.</summary>
    [MenuItem("Fishing Cards/UI/Midnight Field Journal/Install Harbor Screens")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Build Harbor outside Play Mode.");
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != JournalComparisonSceneBuilder.ScenePath || scene.isDirty)
            throw new InvalidOperationException("Open the clean journal comparison scene before installing the Harbor.");
        BuildPrefab();
        Install(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save the integrated journal scene.");
    }

    /// <summary>Builds the three authored page hierarchies with independent sprites, TMP fields and navigation targets.</summary>
    private static void BuildPrefab()
    {
        Layout layout = JsonUtility.FromJson<Layout>(File.ReadAllText(Root + "HarborLayout.json"));
        if (layout.pages.Length != 3) throw new InvalidOperationException("Expected three Harbor pages.");
        foreach (Resource resource in layout.resources.Where(r => r.path.StartsWith(Root + "Art/", StringComparison.Ordinal))) Import(resource.path);
        var sprites = layout.resources.ToDictionary(r => r.id, r => AssetDatabase.LoadAssetAtPath<Sprite>(r.path));
        if (sprites.Any(p => p.Value == null)) throw new InvalidOperationException("Missing Harbor presentation resource.");
        var theme = AssetDatabase.LoadAssetAtPath<MidnightFieldJournalTheme>(MidnightFieldJournalAssetSetup.ThemePath);
        if (theme == null || theme.DisplayFont == null || theme.BodyFont == null) throw new InvalidOperationException("Existing journal fonts are required.");
        MidnightFieldJournalAssetSetup.EnsureFolder(Root + "Prefabs/");
        var root = new GameObject("JournalHarbor", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster), typeof(JournalHarborFlow));
        try
        {
            Canvas canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 80;
            var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(3840, 2160); scaler.matchWidthOrHeight = .5f;
            RectTransform content = Stretch("Hub Content", root.transform);
            var backdrop = content.gameObject.AddComponent<UnityEngine.UI.Image>(); backdrop.color = Ink; backdrop.raycastTarget = false;
            RectTransform stage = Stretch("16x9 Design Stage", content);
            var aspect = stage.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>(); aspect.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent; aspect.aspectRatio = 16f / 9f;
            JournalHarborFlow flow = root.GetComponent<JournalHarborFlow>();
            var tracker = root.AddComponent<DiscoveryJournalTracker>();
            var questSource = AssetDatabase.LoadAssetAtPath<TextAsset>(Root + "DiscoveryQuests.json");
            var trackerData = new SerializedObject(tracker); trackerData.FindProperty("catalogSource").objectReferenceValue = questSource; trackerData.ApplyModifiedPropertiesWithoutUndo();
            var pages = new List<GameObject>(); var gold = new List<TMP_Text>();
            foreach (Page source in layout.pages)
            {
                RectTransform page = Stretch(source.id, stage); pages.Add(page.gameObject);
                foreach (Node node in source.nodes)
                {
                    RectTransform target = Place(node.name, page, new Rect(node.x, node.y, node.width, node.height));
                    if (node.kind == "text")
                    {
                        var text = target.gameObject.AddComponent<TextMeshProUGUI>();
                        text.font = node.family == "Cinzel" ? theme.DisplayFont : theme.BodyFont;
                        text.text = node.text; text.fontSize = node.fontSize; text.enableAutoSizing = true;
                        text.fontSizeMin = node.fontSize * .8f; text.fontSizeMax = node.fontSize;
                        text.alignment = node.align == "left" ? TextAlignmentOptions.TopLeft : node.align == "right" ? TextAlignmentOptions.TopRight : TextAlignmentOptions.Top;
                        text.margin = Vector4.zero; text.richText = false; text.raycastTarget = false;
                        text.textWrappingMode = TextWrappingModes.Normal; text.color = ColorOf(node.color, node.opacity);
                        if (node.role == "gold") { text.name = "Gold Balance"; gold.Add(text); }
                    }
                    else
                    {
                        var image = target.gameObject.AddComponent<UnityEngine.UI.Image>(); image.raycastTarget = false;
                        image.color = node.kind == "image" ? new Color(1, 1, 1, node.opacity) : ColorOf(node.color, node.opacity);
                        if (node.kind == "image") { image.sprite = sprites[node.resource]; image.useSpriteMesh = false; }
                    }
                }
                // Match the stable 16:9 source coordinates; scalable stage keeps hit areas aligned on other aspect ratios.
                Navigate(page, flow, "Harbor", new Rect(580, 38, 150, 55), 0);
                Navigate(page, flow, "Tackle Shop", new Rect(737, 38, 150, 55), 1);
                Navigate(page, flow, "Technique Collection", new Rect(894, 38, 150, 55), 2);
                Navigate(page, flow, "Discovery Journal", new Rect(1051, 38, 150, 55), 3);
                if (source.id == "harbor")
                {
                    Navigate(page, flow, "Open Tackle Shop", new Rect(62, 534, 506, 226), 1);
                    Navigate(page, flow, "Open Technique Collection", new Rect(588, 534, 496, 226), 2);
                    Navigate(page, flow, "Open Discovery Journal", new Rect(1102, 534, 506, 226), 3);
                    var sail = Hit(page, "Set Sail", new Rect(1173, 778, 407, 100));
                    UnityEventTools.AddPersistentListener(sail.onClick, flow.StartNewRun);
                }
                else
                {
                    Navigate(page, flow, "Back to Harbor", new Rect(1160, 819, 440, 82), 0);
                    Inactive(page, source.id == "shop" ? "Purchasing unavailable" : "Deck editing unavailable", new Rect(1153, 731, 416, 62));
                }
                page.gameObject.SetActive(source.id == "harbor");
            }
            RectTransform journal = JournalDiscoveryPageBuilder.Build(stage, tracker, questSource, theme, flow);
            pages.Add(journal.gameObject);
            gold.Add(journal.GetComponent<JournalDiscoveryPage>().GoldLabel);
            journal.gameObject.SetActive(false);
            var data = new SerializedObject(flow);
            data.FindProperty("hubContent").objectReferenceValue = content.gameObject;
            SetArray(data.FindProperty("pages"), pages.Cast<UnityEngine.Object>().ToArray());
            SetArray(data.FindProperty("goldLabels"), gold.Cast<UnityEngine.Object>().ToArray());
            data.ApplyModifiedPropertiesWithoutUndo();
            JournalTackleShopSetup.Bind(root);
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    /// <summary>Attaches only this hub to the candidate scene and delegates run setup to its existing controller.</summary>
    public static void Install(Scene scene)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null) return;
        var controller = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<FishingRunController>(true)).Single();
        var runView = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<FishingRunView>(true)).Single();
        foreach (var previous in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<JournalHarborFlow>(true)).ToArray()) UnityEngine.Object.DestroyImmediate(previous.gameObject);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        var flow = instance.GetComponent<JournalHarborFlow>(); var data = new SerializedObject(flow);
        data.FindProperty("controller").objectReferenceValue = controller;
        data.FindProperty("runCanvas").objectReferenceValue = runView.gameObject;
        data.FindProperty("resultModal").objectReferenceValue = runView.GetComponentInChildren<JournalResultModal>(true);
        data.ApplyModifiedPropertiesWithoutUndo(); PrefabUtility.RecordPrefabInstancePropertyModifications(flow);
        var tracker = instance.GetComponent<DiscoveryJournalTracker>(); var trackerData = new SerializedObject(tracker);
        trackerData.FindProperty("controller").objectReferenceValue = controller; trackerData.ApplyModifiedPropertiesWithoutUndo(); PrefabUtility.RecordPrefabInstancePropertyModifications(tracker);
        var run = new SerializedObject(controller); run.FindProperty("startRunOnAwake").boolValue = false; run.ApplyModifiedPropertiesWithoutUndo();
        var equipment = AssetDatabase.LoadAssetAtPath<TackleShopCatalog>(JournalTackleShopSetup.CatalogPath);
        if (equipment != null) { run.Update(); run.FindProperty("tackleShopCatalog").objectReferenceValue = equipment; run.ApplyModifiedPropertiesWithoutUndo(); }
        EditorUtility.SetDirty(controller);
    }

    /// <summary>Preserves source pixels at 4K instead of applying Unity's default 2048 texture reduction or lossy compression.</summary>
    private static void Import(string path)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
        importer.maxTextureSize = 4096; importer.npotScale = TextureImporterNPOTScale.None;
        importer.mipmapEnabled = false; importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true; importer.filterMode = FilterMode.Bilinear; importer.wrapMode = TextureWrapMode.Clamp;
        importer.SaveAndReimport();
    }

    /// <summary>Creates a stretch container for existing canvas/layout ownership.</summary>
    private static RectTransform Stretch(string name, Transform parent)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; return rect;
    }

    /// <summary>Places an authored field in the 3840x2160 stage using top-left source geometry.</summary>
    private static RectTransform Place(string name, Transform parent, Rect bounds)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1); rect.anchoredPosition = new Vector2(bounds.x, -bounds.y); rect.sizeDelta = bounds.size; return rect;
    }

    /// <summary>Creates a pointer and navigation target with restrained wash feedback and no added hover frame.</summary>
    private static UnityEngine.UI.Button Hit(Transform parent, string name, Rect bounds)
    {
        bounds = new Rect(bounds.x * 3840 / 1672, bounds.y * 2160 / 941, bounds.width * 3840 / 1672, bounds.height * 2160 / 941);
        var rect = Place(name, parent, bounds); var graphic = rect.gameObject.AddComponent<UnityEngine.UI.Image>(); graphic.color = Color.white;
        var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = graphic;
        var colors = button.colors; colors.normalColor = Color.clear; colors.highlightedColor = new Color(1, .94f, .78f, .07f);
        colors.selectedColor = new Color(1, .94f, .78f, .09f); colors.pressedColor = new Color(1, .94f, .78f, .14f); colors.disabledColor = Color.clear; button.colors = colors;
        return button;
    }

    /// <summary>Wires an authored location to this prefab's own flow.</summary>
    private static void Navigate(Transform parent, JournalHarborFlow flow, string name, Rect bounds, int page)
    {
        var button = Hit(parent, name, bounds); UnityEventTools.AddIntPersistentListener(button.onClick, flow.Navigate, page);
    }

    /// <summary>Excludes unimplemented actions from pointer and keyboard navigation.</summary>
    private static void Inactive(Transform parent, string name, Rect bounds) { Hit(parent, name, bounds).interactable = false; }

    /// <summary>Converts authored sRGB text colours without modifying theme-wide materials.</summary>
    private static Color ColorOf(string value, float opacity) { ColorUtility.TryParseHtmlString(value, out Color color); color.a = opacity; return color; }

    /// <summary>Persists stable scene/prefab references through the existing SerializedObject workflow.</summary>
    private static void SetArray(SerializedProperty field, UnityEngine.Object[] objects)
    {
        field.arraySize = objects.Length; for (int i = 0; i < objects.Length; i++) field.GetArrayElementAtIndex(i).objectReferenceValue = objects[i];
    }
}
