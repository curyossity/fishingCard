using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;

/// <summary>Imports the approved independent Discover sprites and builds the live 1920x1080 Tackle Apparatus presentation.</summary>
public static class TackleDiscoverArtSetup
{
    public const string Root = "Assets/UIThemes/TackleApparatus/";
    public const string PrefabPath = Root + "Prefabs/DiscoverCanvas.prefab";
    public const string RigPrefabPath = Root + "Prefabs/RigCard.prefab";
    private const string Source = "Docs/UIThemes/TackleApparatusConcept/AssetLibrary/";
    private static readonly Color Cream = new Color32(222, 211, 165, 255);
    private static readonly Color Ink = new Color32(55, 59, 44, 255);
    private static TMP_FontAsset displayFont, bodyFont;
    private static Dictionary<string, Sprite> sprites;
    [Serializable] private sealed class Manifest { public Asset[] assets; }
    [Serializable] private sealed class Asset { public string name, exportPath; public string[] uses; public float[] unityBorderLBRT; public bool alpha; }

    /// <summary>Installs the approved art in the saved comparison scene without altering the Harbor or other phases.</summary>
    [MenuItem("Fishing Cards/Slot Machine/Integrate Approved Discover Art")]
    public static void InstallCurrentScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != JournalComparisonSceneBuilder.ScenePath)
            throw new InvalidOperationException("Open the comparison scene outside Play Mode.");
        BuildAssets();
        Install(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save Discover artwork integration.");
        AssetDatabase.SaveAssets();
    }

    /// <summary>Preserves the artwork binding when the comparison scene or Harbor is regenerated.</summary>
    public static void Install(Scene scene)
    {
        var owner = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<FishingRunView>(true)).Single();
        var canvas = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<TackleDiscoverPresentation>(true)).SingleOrDefault();
        if (canvas == null)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), scene);
            canvas = instance.GetComponent<TackleDiscoverPresentation>();
        }
        var data = new SerializedObject(owner);
        var previous = data.FindProperty("slotMachineView").objectReferenceValue as SlotMachineView;
        if (previous != null && previous.gameObject != canvas.gameObject) previous.gameObject.SetActive(false);
        data.FindProperty("slotMachineView").objectReferenceValue = canvas.GetComponent<SlotMachineView>();
        data.ApplyModifiedPropertiesWithoutUndo();
        canvas.gameObject.SetActive(false);
    }

    /// <summary>Copies approved exports byte-for-byte, imports exact borders and builds reusable serialized art prefabs.</summary>
    public static void BuildAssets()
    {
        EnsureFolder(Root + "Art/Common"); EnsureFolder(Root + "Art/Discover");
        EnsureFolder(Root + "Fonts"); EnsureFolder(Root + "Prefabs");
        Manifest manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(Source + "Manifest.json"));
        sprites = new Dictionary<string, Sprite>();
        foreach (Asset asset in manifest.assets.Where(a => a.uses.Contains("discover")))
        {
            string path = Root + "Art/" + asset.exportPath.Substring("Exports/".Length);
            Copy(Source + asset.exportPath, path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100; importer.spritePivot = new Vector2(.5f, .5f);
            importer.spriteBorder = asset.unityBorderLBRT == null || asset.unityBorderLBRT.Length == 0 ? Vector4.zero
                : new Vector4(asset.unityBorderLBRT[0], asset.unityBorderLBRT[1], asset.unityBorderLBRT[2], asset.unityBorderLBRT[3]);
            importer.filterMode = FilterMode.Point; importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = false; importer.isReadable = false; importer.sRGBTexture = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput; importer.alphaIsTransparency = asset.alpha;
            importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings); importer.SaveAndReimport();
            sprites.Add(asset.name, AssetDatabase.LoadAssetAtPath<Sprite>(path));
        }
        foreach (string file in new[] { "PressStart2P-Regular.ttf", "VT323-Regular.ttf", "pressstart2p-OFL.txt", "vt323-OFL.txt" })
        { Copy(Source + "Fonts/" + file, Root + "Fonts/" + file); AssetDatabase.ImportAsset(Root + "Fonts/" + file); }
        displayFont = Font("PressStart2P-Regular", "PressStart2P");
        bodyFont = Font("VT323-Regular", "VT323");
        TackleReelSymbolSetup.Import();
        BuildRigCard();
        BuildDiscover();
        Copy(Source + "Manifest.json", Root + "SourceManifest.json");
        AssetDatabase.SaveAssets();
    }

    /// <summary>Authors the shared rig card with supplied face/portrait/clip siblings and independent live labels.</summary>
    private static void BuildRigCard()
    {
        var root = Rect("Tackle Rig Card", null, new Rect(0, 0, 315, 275));
        try
        {
            var view = root.gameObject.AddComponent<TackleRigCardView>();
            var data = new SerializedObject(view);
            Bind(data, "face", Art(root, "rig-card-ivory", new Rect(0, 0, 315, 244), 1.35135135f));
            Bind(data, "catchFace", sprites["rig-card-ivory"]); Bind(data, "baitFace", sprites["rig-card-sage"]);
            Bind(data, "portrait", Art(root, "portrait-sardine", new Rect(30, 68, 255, 105), 0, true));
            Art(root, "card-clip", new Rect(130, -20, 55, 34), 0, true);
            Art(root, "rope-vertical", new Rect(147, 242, 20, 40), 0, true);
            Bind(data, "nameLabel", Text(root, "NameLabel", "", new Rect(20, 25, 275, 32), 21, Cream, false, 12));
            Bind(data, "roleLabel", Text(root, "RoleLabel", "", new Rect(44, 198, 158, 30), 19, Cream, true));
            Bind(data, "valueLabel", Text(root, "SaleValueLabel", "", new Rect(212, 197, 58, 32), 22, Cream, false, 10));
            var portraits = data.FindProperty("portraits"); portraits.arraySize = 4;
            string[] ids = { "creature_sardine", "creature_mackerel", "creature_glass_minnow", "creature_garfish" };
            string[] names = { "portrait-sardine", "portrait-mackerel", "portrait-glass-minnow", "portrait-garfish" };
            for (int i = 0; i < 4; i++)
            {
                portraits.GetArrayElementAtIndex(i).FindPropertyRelative("creatureId").stringValue = ids[i];
                portraits.GetArrayElementAtIndex(i).FindPropertyRelative("sprite").objectReferenceValue = sprites[names[i]];
            }
            data.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root.gameObject, RigPrefabPath);
        }
        finally { UnityEngine.Object.DestroyImmediate(root.gameObject); }
    }

    /// <summary>Recreates the approved Discover composition with native interactive overlays rather than a flattened screenshot.</summary>
    private static void BuildDiscover()
    {
        var root = new GameObject("Tackle Discover Canvas", typeof(RectTransform), typeof(Canvas),
            typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster), typeof(SlotMachineView), typeof(TackleDiscoverPresentation));
        try
        {
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 70;
            var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
            var letterbox = root.AddComponent<UnityEngine.UI.Image>(); letterbox.color = new Color32(14, 17, 15, 255);
            var stage = Rect("1920x1080 Stage", root.transform, new Rect(0, 0, 1920, 1080));
            stage.anchorMin = stage.anchorMax = stage.pivot = new Vector2(.5f, .5f); stage.anchoredPosition = Vector2.zero;
            var view = new SerializedObject(root.GetComponent<SlotMachineView>());
            var art = new SerializedObject(root.GetComponent<TackleDiscoverPresentation>());
            Bind(view, "artwork", root.GetComponent<TackleDiscoverPresentation>()); Bind(view, "discoverContent", stage.gameObject);
            Art(stage, "desk-background", new Rect(0, 0, 1920, 1080));
            Art(stage, "rig-cabinet", new Rect(24, 44, 380, 995));
            Art(stage, "title-plaque", new Rect(570, 28, 780, 116), 1.11111111f);
            Art(stage, "haul-plaque", new Rect(1430, 29, 345, 116));
            Art(stage, "machine-housing", new Rect(490, 163, 1310, 657));
            Art(stage, "control-console", new Rect(492, 897, 1310, 165), 1.53846154f);
            Art(stage, "message-strip", new Rect(525, 836, 1140, 54), 1.66666667f);
            Bind(art, "biomeLabel", Text(stage, "BiomePhaseLabel", "", new Rect(590, 65, 740, 47), 28, Cream, false, 18));
            Bind(art, "haulLabel", Text(stage, "HaulLabel", "", new Rect(1450, 70, 300, 40), 26, new Color32(215, 172, 85, 255), false, 16));
            Text(stage, "RigTitle", "YOUR RIG", new Rect(70, 105, 290, 42), 23, Cream);
            Bind(view, "heading", Text(stage, "MachineTitle", "ENCOUNTER TAGS", new Rect(700, 231, 950, 44), 27, Cream));
            Bind(view, "resultDescription", Text(stage, "ResultLabel", "", new Rect(570, 846, 1050, 35), 22, new Color32(215, 172, 85, 255), false, 15));
            var faces = view.FindProperty("reelLabels"); faces.arraySize = 3;
            var locks = view.FindProperty("lockButtons"); locks.arraySize = 3;
            var lockTexts = view.FindProperty("lockLabels"); lockTexts.arraySize = 3;
            var icons = art.FindProperty("reelSymbols"); icons.arraySize = 3;
            var fallbacks = art.FindProperty("fallbackSymbols"); fallbacks.arraySize = 3;
            var levers = art.FindProperty("lockLevers"); levers.arraySize = 3;
            for (int i = 0; i < 3; i++)
            {
                float x = 625 + i * 345;
                Art(stage, "reel-drum", new Rect(x, 305, 320, 365));
                icons.GetArrayElementAtIndex(i).objectReferenceValue = Art(stage, "tag-schooling", new Rect(x + 54, 384, 212, 98), 0, true);
                fallbacks.GetArrayElementAtIndex(i).objectReferenceValue = Text(stage, "Symbol " + i, "", new Rect(x + 35, 386, 250, 96), 26, Ink, false, 18);
                faces.GetArrayElementAtIndex(i).objectReferenceValue = Text(stage, "TagLabel " + i, "", new Rect(x + 30, 565, 260, 34), 22, Ink);
                var lever = Art(stage, "lock-lever-open", new Rect(x + 38, 694, 70, 74), 0, true);
                levers.GetArrayElementAtIndex(i).objectReferenceValue = lever;
                var button = Button(stage, "Lock " + i, "LOCK", new Rect(x + 120, 710, 160, 45), "status-plaque", 4, 17, Cream);
                locks.GetArrayElementAtIndex(i).objectReferenceValue = button;
                lockTexts.GetArrayElementAtIndex(i).objectReferenceValue = button.GetComponentInChildren<TMP_Text>();
            }
            Bind(art, "openLever", sprites["lock-lever-open"]); Bind(art, "lockedLever", sprites["lock-lever-locked"]);
            Bind(art, "matchOn", sprites["match-lamps-on"]); Bind(art, "matchOff", sprites["match-lamps-off"]);
            Bind(art, "matchLamps", Art(stage, "match-lamps-off", new Rect(864, 670, 197, 50), 0, true));
            TackleReelSymbolSetup.Bind(art);
            Bind(view, "respinButton", Button(stage, "Respin", "RESPIN", new Rect(545, 920, 505, 116), "button-secondary", 1.42857143f, 26, Cream));
            var accept = Button(stage, "Accept", "ACCEPT TAGS", new Rect(1120, 920, 600, 116), "button-primary", 1.42857143f, 26, Ink);
            Bind(view, "acceptButton", accept); Bind(art, "initialFocus", accept);
            var viewport = Rect("Rig Viewport", stage, new Rect(62, 189, 315, 825));
            viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            var scroll = viewport.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();
            var content = Rect("Ordered Rig", viewport, new Rect(0, 0, 315, 825));
            scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false; scroll.vertical = true;
            scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 45;
            var hit = viewport.gameObject.AddComponent<UnityEngine.UI.Image>(); hit.color = Color.clear;
            Bind(art, "rigContent", content); Bind(art, "rigScroll", scroll);
            Bind(art, "rigCardPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(RigPrefabPath).GetComponent<TackleRigCardView>());
            Bind(art, "emptyRigLabel", Text(viewport, "Empty Rig", "NO CREATURES\nATTACHED", new Rect(15, 280, 285, 150), 34, Cream, false, 24, true));
            view.ApplyModifiedPropertiesWithoutUndo(); art.ApplyModifiedPropertiesWithoutUndo();
            AddReelAnimation(root);
            TackleReelSurfaceSetup.Configure(root);
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    /// <summary>Adds cylinder rendering to the existing prefab without rebuilding its artwork or changing scene overrides.</summary>
    [MenuItem("Fishing Cards/Slot Machine/Install Cylindrical Reel Animation")]
    public static void InstallReelAnimation()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode before installing reel animation.");
        var root = PrefabUtility.LoadPrefabContents(PrefabPath);
        try { AddReelAnimation(root); TackleReelSurfaceSetup.Configure(root); PrefabUtility.SaveAsPrefabAsset(root, PrefabPath); }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
    }

    /// <summary>Retains the original templates and composes three clipped cylinder surfaces around native tag graphics.</summary>
    private static void AddReelAnimation(GameObject root)
    {
        var view = new SerializedObject(root.GetComponent<SlotMachineView>());
        var art = new SerializedObject(root.GetComponent<TackleDiscoverPresentation>());
        var reels = art.FindProperty("reels"); reels.arraySize = 3;
        var stage = root.transform.Find("1920x1080 Stage");
        for (int i = 0; i < 3; i++)
        {
            var existing = stage.Find("Cylindrical Reel " + i);
            if (existing != null) { reels.GetArrayElementAtIndex(i).objectReferenceValue = existing.GetComponent<TackleReelAnimator>(); continue; }
            var cylinder = Rect("Cylindrical Reel " + i, stage, new Rect(625 + i * 345, 305, 320, 365));
            cylinder.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            var animator = cylinder.gameObject.AddComponent<TackleReelAnimator>();
            var icon = (UnityEngine.UI.Image)art.FindProperty("reelSymbols").GetArrayElementAtIndex(i).objectReferenceValue;
            var fallback = (TMP_Text)art.FindProperty("fallbackSymbols").GetArrayElementAtIndex(i).objectReferenceValue;
            var label = (TMP_Text)view.FindProperty("reelLabels").GetArrayElementAtIndex(i).objectReferenceValue;
            MoveReelTemplate(icon.rectTransform, cylinder, new Vector2(0, 55));
            MoveReelTemplate(fallback.rectTransform, cylinder, new Vector2(0, 55));
            MoveReelTemplate(label.rectTransform, cylinder, new Vector2(0, -95));
            animator.Configure(icon, fallback, label);
            reels.GetArrayElementAtIndex(i).objectReferenceValue = animator;
        }
        art.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>Keeps authored dimensions and native font settings while centering each source field on its wheel.</summary>
    private static void MoveReelTemplate(RectTransform template, RectTransform cylinder, Vector2 position)
    {
        template.SetParent(cylinder, false);
        template.anchorMin = template.anchorMax = template.pivot = new Vector2(.5f, .5f);
        template.anchoredPosition = position;
        template.gameObject.SetActive(false);
    }

    /// <summary>Creates a stable rectangular hit target with separate artwork, press motion and focus treatment.</summary>
    private static UnityEngine.UI.Button Button(Transform parent, string name, string label, Rect bounds,
        string sprite, float ppu, float size, Color color)
    {
        var root = Rect(name, parent, bounds);
        var hit = root.gameObject.AddComponent<UnityEngine.UI.Image>(); hit.color = Color.clear;
        var button = root.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = hit;
        button.transition = UnityEngine.UI.Selectable.Transition.None;
        var visual = Rect("Visual", root, new Rect(0, 0, bounds.width, bounds.height)); visual.gameObject.AddComponent<CanvasGroup>();
        Art(visual, sprite, new Rect(0, 0, bounds.width, bounds.height), ppu);
        Text(visual, "Label", label, new Rect(5, 3, bounds.width - 10, bounds.height - 6), size, color);
        var focus = Art(root, "focus-corners", new Rect(0, 0, bounds.width, bounds.height), 1).gameObject; focus.SetActive(false);
        var feedback = new SerializedObject(root.gameObject.AddComponent<TackleButtonFeedback>());
        Bind(feedback, "visual", visual); Bind(feedback, "focusCorners", focus); feedback.ApplyModifiedPropertiesWithoutUndo();
        return button;
    }

    /// <summary>Creates native sliced frames or aspect-preserved illustrations with independent text and hit regions.</summary>
    private static UnityEngine.UI.Image Art(Transform parent, string name, Rect bounds, float ppu = 0, bool preserve = false)
    {
        var image = Rect(name, parent, bounds).gameObject.AddComponent<UnityEngine.UI.Image>();
        image.sprite = sprites[name]; image.raycastTarget = false; image.preserveAspect = preserve;
        image.type = ppu > 0 ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
        if (ppu > 0) image.pixelsPerUnitMultiplier = ppu;
        return image;
    }

    /// <summary>Creates top-left layout coordinates from the approved 1920x1080 composition.</summary>
    private static RectTransform Rect(string name, Transform parent, Rect bounds)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(bounds.x, -bounds.y); rect.sizeDelta = bounds.size;
        return rect;
    }

    /// <summary>Creates a live licensed-font field; optional fitting protects long authored names without changing safe regions.</summary>
    private static TMP_Text Text(Transform parent, string name, string value, Rect bounds, float size, Color color,
        bool left = false, float minimum = 0, bool body = false)
    {
        var text = Rect(name, parent, bounds).gameObject.AddComponent<TextMeshProUGUI>();
        text.font = body ? bodyFont : displayFont; text.text = value; text.fontSize = size; text.color = color;
        text.alignment = left ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.Midline;
        text.textWrappingMode = TextWrappingModes.Normal; text.raycastTarget = false; text.richText = false;
        if (minimum > 0) { text.enableAutoSizing = true; text.fontSizeMin = minimum; text.fontSizeMax = size; }
        return text;
    }

    /// <summary>Embeds a static ASCII atlas with its own material and no dependency on the previous theme.</summary>
    private static TMP_FontAsset Font(string source, string name)
    {
        string path = Root + "Fonts/" + name + ".asset";
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        if (font != null) return font;
        var input = AssetDatabase.LoadAssetAtPath<Font>(Root + "Fonts/" + source + ".ttf");
        font = TMP_FontAsset.CreateFontAsset(input, 64, 8, GlyphRenderMode.SDF16, 2048, 2048, AtlasPopulationMode.Dynamic, false);
        font.name = name;
        string characters = new string(Enumerable.Range(32, 95).Select(i => (char)i).ToArray());
        if (!font.TryAddCharacters(characters, out string missing)) throw new InvalidOperationException("Missing font glyphs: " + missing);
        font.atlasPopulationMode = AtlasPopulationMode.Static;
        AssetDatabase.CreateAsset(font, path);
        AssetDatabase.AddObjectToAsset(font.material, font);
        foreach (Texture2D texture in font.atlasTextures) AssetDatabase.AddObjectToAsset(texture, font);
        return font;
    }

    /// <summary>Creates asset folders through Unity so every new folder keeps its generated metadata.</summary>
    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/'); EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }

    /// <summary>Preserves original exported pixels and existing target GUIDs during repeatable imports.</summary>
    private static void Copy(string source, string destination)
    {
        if (!File.Exists(destination) || !File.ReadAllBytes(source).SequenceEqual(File.ReadAllBytes(destination))) File.Copy(source, destination, true);
    }

    /// <summary>Persists an explicit native component or asset reference.</summary>
    private static void Bind(SerializedObject data, string field, UnityEngine.Object value)
    { data.FindProperty(field).objectReferenceValue = value; }
}
