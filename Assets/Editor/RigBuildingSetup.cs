using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Authors the independent Build screen from existing approved art, with no Catch/Bait controls.</summary>
public static class RigBuildingSetup
{
    public const string PrefabPath = TackleDiscoverArtSetup.Root + "Prefabs/BuildCanvas.prefab";
    private const string Root = TackleDiscoverArtSetup.Root;
    private const string Source = "Docs/UIThemes/TackleApparatusConcept/AssetLibrary/";
    private static readonly Color Cream = new Color32(222, 211, 165, 255);
    [Serializable] private sealed class Manifest { public Entry[] assets; }
    [Serializable] private sealed class Entry { public string name, exportPath; public float[] unityBorderLBRT; }

    /// <summary>Builds and installs the third phase without rebuilding Discover reels or altering Landing tuning.</summary>
    [MenuItem("Fishing Cards/Rig Building/Install Build Phase")]
    public static void InstallCurrentScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Install Build outside Play Mode.");
        var scene = SceneManager.GetActiveScene();
        if (scene.path != JournalComparisonSceneBuilder.ScenePath) throw new InvalidOperationException("Open the comparison scene.");
        BuildAssets(); Install(scene); EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save Build bindings.");
        AssetDatabase.SaveAssets();
    }

    /// <summary>Retains the view when Harbor/comparison authoring regenerates the scene.</summary>
    public static void Install(Scene scene)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null) BuildAssets();
        var runView = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<FishingRunView>(true)).Single();
        var build = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<RigBuildingView>(true)).SingleOrDefault();
        if (build == null) build = ((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), scene)).GetComponent<RigBuildingView>();
        var data = new SerializedObject(runView); Bind(data, "rigBuildingView", build); data.ApplyModifiedPropertiesWithoutUndo(); build.gameObject.SetActive(false);
    }

    /// <summary>Imports original placement ornaments and assembles reusable, native-interactive UI.</summary>
    public static void BuildAssets()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(TackleDiscoverArtSetup.RigPrefabPath) == null) TackleDiscoverArtSetup.BuildAssets();
        Directory.CreateDirectory(Root + "Art/Placement");
        foreach (var entry in JsonUtility.FromJson<Manifest>(File.ReadAllText(Source + "Manifest.json")).assets.Where(e => e.exportPath.StartsWith("Exports/Placement/", StringComparison.Ordinal)))
        {
            string path = Root + "Art/Placement/" + Path.GetFileName(entry.exportPath);
            byte[] bytes = File.ReadAllBytes(Source + entry.exportPath);
            if (!File.Exists(path) || !File.ReadAllBytes(path).SequenceEqual(bytes)) File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point; importer.wrapMode = TextureWrapMode.Clamp; importer.spritePixelsPerUnit = 100;
            importer.mipmapEnabled = false; importer.isReadable = false; importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 2048; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spriteBorder = entry.unityBorderLBRT == null || entry.unityBorderLBRT.Length == 0 ? Vector4.zero
                : entry.unityBorderLBRT.Length == 4 ? new Vector4(entry.unityBorderLBRT[0], entry.unityBorderLBRT[1], entry.unityBorderLBRT[2], entry.unityBorderLBRT[3])
                : throw new InvalidOperationException("Expected four border values for " + entry.name);
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings); settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings); importer.SaveAndReimport();
        }
        var root = new GameObject("Tackle Build Canvas", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster), typeof(RigBuildingView));
        try
        {
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 85;
            root.AddComponent<UnityEngine.UI.Image>().color = Color.clear;
            var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
            var stage = Rect(root.transform, "1920x1080 Stage", new Rect(0, 0, 1920, 1080));
            stage.anchorMin = stage.anchorMax = stage.pivot = new Vector2(.5f, .5f); stage.anchoredPosition = Vector2.zero;
            var data = new SerializedObject(root.GetComponent<RigBuildingView>());
            Art(stage, "desk-background", new Rect(0, 0, 1920, 1080)); Art(stage, "rig-cabinet", new Rect(24, 44, 380, 995));
            Art(stage, "title-plaque", new Rect(570, 28, 780, 116), 1.11111111f); Art(stage, "haul-plaque", new Rect(1430, 29, 345, 116));
            Art(stage, "machine-housing", new Rect(490, 163, 1310, 657)); Art(stage, "control-console", new Rect(492, 897, 1310, 165), 1.53846154f);
            Art(stage, "message-strip", new Rect(525, 836, 1140, 54), 1.66666667f);
            Bind(data, "heading", Label(stage, "BuildHeading", "BUILD", new Rect(590, 65, 740, 47), 25));
            Bind(data, "haul", Label(stage, "Haul", "HAUL 0", new Rect(1450, 70, 300, 40), 26));
            Label(stage, "RigTitle", "YOUR RIG", new Rect(70, 105, 290, 42), 23);
            Label(stage, "NewSpeciesCaption", "NEW SPECIES", new Rect(625, 210, 450, 42), 23);
            var pending = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(TackleDiscoverArtSetup.RigPrefabPath));
            pending.transform.SetParent(stage, false); ((RectTransform)pending.transform).anchoredPosition = new Vector2(690, -275);
            Bind(data, "pendingCard", pending.GetComponent<TackleRigCardView>());
            Art(stage, "portrait-frame", new Rect(1090, 255, 590, 265), 1.25f);
            var details = Label(stage, "PrimaryAbility", "", new Rect(1120, 280, 530, 205), 22, true);
            details.color = new Color32(55, 59, 44, 255); Bind(data, "details", details);
            Label(stage, "Direction", "ROD  >  LINE TIP", new Rect(630, 535, 1010, 34), 19);
            Art(stage, "insertion-rail", new Rect(570, 570, 1140, 220), 1.2f);
            var rail = Scroll(stage, "Insertion positions", new Rect(580, 575, 1120, 215), false, out var railContent);
            Bind(data, "railScroll", rail); Bind(data, "railContent", railContent);
            var rig = Scroll(stage, "Committed rig", new Rect(62, 189, 315, 825), true, out var rigContent);
            Bind(data, "rigScroll", rig); Bind(data, "rigContent", rigContent);
            Bind(data, "emptyRig", Label(stage, "EmptyRig", "EMPTY RIG\nPlace your first species", new Rect(80, 240, 280, 170), 24, true));
            Bind(data, "cardPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(TackleDiscoverArtSetup.RigPrefabPath).GetComponent<TackleRigCardView>());
            var gap = Rect(railContent, "Insertion Template", new Rect(0, 0, 60, 140));
            var gapImage = gap.gameObject.AddComponent<UnityEngine.UI.Image>(); gapImage.sprite = Sprite("insertion-socket-idle");
            var gapButton = gap.gameObject.AddComponent<UnityEngine.UI.Button>(); gapButton.targetGraphic = gapImage;
            Label(gap, "Position number", "1", new Rect(5, 48, 50, 45), 24); gap.gameObject.SetActive(false); Bind(data, "gapTemplate", gapButton);
            Bind(data, "socketIdle", Sprite("insertion-socket-idle")); Bind(data, "socketSelected", Sprite("insertion-socket-selected")); Bind(data, "socketBlocked", Sprite("insertion-blocked"));
            Bind(data, "message", Label(stage, "PlacementMessage", "", new Rect(548, 843, 1100, 42), 19, true));
            Art(stage, "readout-glass-wide", new Rect(520, 913, 600, 132), 1.2f);
            var preview = Scroll(stage, "Preview details", new Rect(545, 930, 550, 95), true, out var previewContent);
            Bind(data, "previewScroll", preview); Bind(data, "previewLabel", Label(previewContent, "Projected rig values", "", new Rect(0, 0, 550, 95), 20, true));
            var button = Rect(stage, "Commit placement", new Rect(1140, 913, 610, 132));
            var hit = button.gameObject.AddComponent<UnityEngine.UI.Image>(); hit.color = Color.clear;
            var commit = button.gameObject.AddComponent<UnityEngine.UI.Button>(); commit.targetGraphic = hit; commit.transition = UnityEngine.UI.Selectable.Transition.None;
            var visual = Rect(button, "Visual", new Rect(0, 0, 610, 132)); visual.gameObject.AddComponent<CanvasGroup>();
            Art(visual, "button-primary", new Rect(0, 0, 610, 132), 1.42f);
            var text = Label(visual, "CommitLabel", "PLACE CREATURE", new Rect(20, 25, 570, 75), 30); text.color = new Color32(55, 59, 44, 255);
            var focus = Art(button, "focus-corners", new Rect(0, 0, 610, 132), 1).gameObject; focus.SetActive(false);
            var feedback = new SerializedObject(button.gameObject.AddComponent<TackleButtonFeedback>());
            Bind(feedback, "visual", visual); Bind(feedback, "focusCorners", focus); feedback.ApplyModifiedPropertiesWithoutUndo();
            Bind(data, "commit", commit); data.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    /// <summary>Creates a clipped, explicitly sized content area, matching the existing theme's pooling/scrolling pattern.</summary>
    private static UnityEngine.UI.ScrollRect Scroll(Transform parent, string name, Rect bounds, bool vertical, out RectTransform content)
    {
        var root = Rect(parent, name, bounds); var scroll = root.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();
        var viewport = Rect(root, "Viewport", new Rect(0, 0, bounds.width, bounds.height)); viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
        var image = viewport.gameObject.AddComponent<UnityEngine.UI.Image>(); image.color = Color.clear;
        content = Rect(viewport, "Content", new Rect(0, 0, bounds.width, bounds.height)); scroll.viewport = viewport; scroll.content = content;
        scroll.vertical = vertical; scroll.horizontal = !vertical; scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped; return scroll;
    }
    /// <summary>Loads an exact approved resource from its own phase or the common library.</summary>
    private static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Root + "Art/Placement/" + name + ".png")
        ?? AssetDatabase.LoadAssetAtPath<Sprite>(Root + "Art/Common/" + name + ".png");
    /// <summary>Preserves native borders for scalable frames and keeps decorative art non-intercepting.</summary>
    private static UnityEngine.UI.Image Art(Transform parent, string name, Rect bounds, float ppu = 0)
    {
        var image = Rect(parent, name, bounds).gameObject.AddComponent<UnityEngine.UI.Image>(); image.sprite = Sprite(name); image.raycastTarget = false;
        if (image.sprite == null) throw new InvalidOperationException("Missing approved Build resource: " + name);
        if (ppu > 0) { image.type = UnityEngine.UI.Image.Type.Sliced; image.pixelsPerUnitMultiplier = ppu; } return image;
    }
    /// <summary>Creates a top-left anchored rectangle in the approved reference composition.</summary>
    private static RectTransform Rect(Transform parent, string name, Rect bounds)
    {
        var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform; rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1); rect.anchoredPosition = new Vector2(bounds.x, -bounds.y); rect.sizeDelta = bounds.size; return rect;
    }
    /// <summary>Uses the licensed theme fonts and bounded native text, never printed rules or role choices.</summary>
    private static TMP_Text Label(Transform parent, string name, string value, Rect bounds, float size, bool body = false)
    {
        var text = Rect(parent, name, bounds).gameObject.AddComponent<TextMeshProUGUI>();
        text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Root + "Fonts/" + (body ? "VT323" : "PressStart2P") + ".asset");
        text.text = value; text.fontSize = size; text.enableAutoSizing = !body; text.fontSizeMin = 12; text.fontSizeMax = size;
        text.color = Cream; text.alignment = body ? TextAlignmentOptions.TopLeft : TextAlignmentOptions.Midline;
        text.textWrappingMode = TextWrappingModes.Normal; text.raycastTarget = false; text.richText = false; return text;
    }
    /// <summary>Serializes explicit view references without runtime object searches.</summary>
    private static void Bind(SerializedObject data, string field, UnityEngine.Object value) => data.FindProperty(field).objectReferenceValue = value;
}
