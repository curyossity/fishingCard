using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Checks the theme's resource boundary and renders isolated import/font evidence.</summary>
public static class MidnightFieldJournalAssetValidation
{
    private const string Root = MidnightFieldJournalAssetSetup.Root;
    private const string Evidence = "Docs/UIThemes/MidnightFieldJournal/";
    [Serializable] private sealed class Report
    {
        public bool passed, prefabGuidsStable, activeSceneUnchanged;
        public string scope = "Current isolated imports/fonts/templates; component behavior has separate validation; no full playable-scene certification";
        public int sprites, fonts, prefabs;
        public long textureBytes, fontAtlasBytes;
        public string[] externalSharedDependencies;
        public List<string> failures = new List<string>();
        public List<string> warnings = new List<string>();
    }

    /// <summary>Validates native texture sizes, local font dependencies, stable regeneration and missing-art boundaries.</summary>
    [MenuItem("Fishing Cards/UI/Midnight Field Journal/Validate Isolated Resources")]
    public static void Validate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Validate assets outside Play Mode.");
        Report report = new Report();
        Scene active = SceneManager.GetActiveScene(); string scenePath = active.path; bool sceneDirty = active.isDirty;
        MidnightFieldJournalTheme theme = AssetDatabase.LoadAssetAtPath<MidnightFieldJournalTheme>(MidnightFieldJournalAssetSetup.ThemePath);
        if (theme == null) throw new InvalidOperationException("Theme profile is missing.");
        foreach (MidnightFieldJournalAssetSetup.Source source in MidnightFieldJournalAssetSetup.LoadCatalog().sprites)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(source.path);
            TextureImporter importer = AssetImporter.GetAtPath(source.path) as TextureImporter;
            if (sprite == null || importer == null) { report.failures.Add("Missing sprite: " + source.id); continue; }
            if (sprite.texture.width != source.width || sprite.texture.height != source.height) report.failures.Add("Native dimensions changed: " + source.id);
            if (importer.isReadable || importer.npotScale != TextureImporterNPOTScale.None || importer.textureCompression != TextureImporterCompression.Uncompressed || importer.wrapMode != TextureWrapMode.Clamp) report.failures.Add("Import contract mismatch: " + source.id);
            if (source.path.Contains("/Techniques/") || source.path.Contains("/Cards/Technique/") || source.path.Contains("/Cards/CompactCatch/") || source.path.Contains("/Creatures/"))
                if (!importer.mipmapEnabled || importer.filterMode != FilterMode.Trilinear) report.failures.Add("Card minification settings missing: " + source.id);
            report.sprites++; report.textureBytes += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(sprite.texture);
        }
        foreach (string guid in AssetDatabase.FindAssets("t:TMP_FontAsset", new[] { Root + "Fonts/TMP" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid); TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (font == null) continue;
            report.fonts++;
            if (font.isMultiAtlasTexturesEnabled) report.failures.Add("Unbounded font atlas: " + path);
            bool dynamic = font.atlasPopulationMode == AtlasPopulationMode.Dynamic;
            if (dynamic && !new SerializedObject(font).FindProperty("m_ClearDynamicDataOnBuild").boolValue) report.failures.Add("Dynamic atlas not cleared on build: " + path);
            if (!dynamic && (!font.HasCharacters("0123456789?ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz") || font.faceInfo.scale != 1)) report.failures.Add("Static glyph/scale contract failed: " + path);
            if (!dynamic && font.fontFeatureTable.glyphPairAdjustmentRecords.Any(p => !font.glyphTable.Any(g => g.index == p.firstAdjustmentRecord.glyphIndex) || !font.glyphTable.Any(g => g.index == p.secondAdjustmentRecord.glyphIndex))) report.failures.Add("Static kerning refers to unbaked glyphs: " + path);
            foreach (Texture2D atlas in font.atlasTextures) report.fontAtlasBytes += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(atlas);
        }
        string[] prefabs = AssetDatabase.FindAssets("t:Prefab", new[] { Root + "Prefabs" }).Select(AssetDatabase.GUIDToAssetPath).ToArray();
        Dictionary<string, string> ids = prefabs.ToDictionary(p => p, AssetDatabase.AssetPathToGUID);
        MidnightFieldJournalPrefabBuilder.Build();
        report.prefabs = prefabs.Length;
        report.prefabGuidsStable = ids.All(p => AssetDatabase.AssetPathToGUID(p.Key) == p.Value);
        if (!report.prefabGuidsStable) report.failures.Add("Template regeneration changed a prefab GUID.");
        if (theme.Sprites.Count != 20 || report.sprites != 20 || report.fonts != 8 || report.prefabs != 10) report.failures.Add("Unexpected asset/font/template count.");
        if (theme.CatchTooltipFrame == null) report.failures.Add("Detailed inspection frame is missing.");
        if (theme.TryGetArtwork("clear_water", out Sprite unused)) report.failures.Add("Missing art incorrectly substituted for Clear Water.");
        string[] dependencies = AssetDatabase.GetDependencies(prefabs.Concat(new[] { MidnightFieldJournalAssetSetup.ThemePath }).ToArray(), true);
        report.externalSharedDependencies = dependencies.Where(p => !p.StartsWith(Root, StringComparison.Ordinal)).OrderBy(p => p).ToArray();
        foreach (string dependency in report.externalSharedDependencies)
            if (dependency.StartsWith("Assets/FishingUIAssets/", StringComparison.Ordinal) || dependency.StartsWith("Assets/Prefabs/UI/", StringComparison.Ordinal)) report.failures.Add("Old-theme art/font/prefab dependency: " + dependency);
        report.activeSceneUnchanged = SceneManager.GetActiveScene().path == scenePath && SceneManager.GetActiveScene().isDirty == sceneDirty;
        if (!report.activeSceneUnchanged) report.failures.Add("Asset preparation changed active scene state.");
        report.warnings.Add("Illustrated kelp source remains 1672 x 941 over native 4K paper; full-screen 4K review remains UI-08.");
        report.warnings.Add("Thirty active card identities still have no selected new-theme illustration; no wrong-card fallback was assigned.");
        report.warnings.Add("Central card and projected-hand behavior have separate component evidence; full gameplay scene, modal composition and build validation remain later tasks.");
        report.passed = report.failures.Count == 0;
        File.WriteAllText(Evidence + "ResourceValidation.json", JsonUtility.ToJson(report, true));
        if (!report.passed) throw new InvalidOperationException(string.Join("\n", report.failures));
        Debug.Log("Midnight Field Journal: resources, font isolation and repeatable template generation validated.");
    }

    /// <summary>Renders resource evidence in a disposable preview scene, leaving the user's scene untouched.</summary>
    [MenuItem("Fishing Cards/UI/Midnight Field Journal/Capture Resource Sheet")]
    public static void CaptureResourceSheet()
    {
        MidnightFieldJournalTheme theme = AssetDatabase.LoadAssetAtPath<MidnightFieldJournalTheme>(MidnightFieldJournalAssetSetup.ThemePath);
        Scene preview = EditorSceneManager.NewPreviewScene();
        RenderTexture target = new RenderTexture(3840, 2160, 24); Texture2D output = null;
        RenderTexture previous = RenderTexture.active;
        try
        {
            GameObject cameraObject = new GameObject("Theme Asset Preview Camera", typeof(Camera)); SceneManager.MoveGameObjectToScene(cameraObject, preview);
            Camera camera = cameraObject.GetComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 1080;
            camera.scene = preview; camera.transform.position = new Vector3(0, 0, -100); camera.nearClipPlane = .1f; camera.farClipPlane = 2000;
            camera.targetTexture = target; camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color32(17, 30, 37, 255);
            GameObject root = new GameObject("Theme Resource Sheet", typeof(RectTransform), typeof(Canvas)); SceneManager.MoveGameObjectToScene(root, preview);
            Canvas canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace; canvas.worldCamera = camera;
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(3840, 2160);
            SpriteImage(root.transform, theme, "PaperBackground4K", 0, 0, 3840, 2160);
            SpriteImage(root.transform, theme, "Background", 0, 0, 3840, 2160).color = new Color(1, 1, 1, .45f);
            Label(root.transform, "MIDNIGHT FIELD JOURNAL — RESOURCE CHECK", 100, 60, 3600, 100, 61, theme.DisplayFont);
            Label(root.transform, "UI-02 imported sources and blank templates • layout and gameplay are validated in later tasks", 100, 170, 3600, 80, 34, theme.BodyFont);
            GameObject compact = Prefab(root.transform, "CompactCatchCard", 100, 330, 850, 850 / 1.78f);
            Populate(compact, theme, "SardineV2", new Dictionary<string, string> { { "Creature Name", "Sardine" }, { "Weight Value", "1" }, { "Value Value", "7" } });
            GameObject technique = Prefab(root.transform, "TechniqueCard", 1100, 300, 775, 800);
            Populate(technique, theme, "CutLooseV2", new Dictionary<string, string> { { "Technique Name", "CUT LOOSE" }, { "Technique Rules", "Release 1 attached\ncreature." } });
            GameObject technique2 = Prefab(root.transform, "TechniqueCard", 1960, 300, 775, 800);
            Populate(technique2, theme, "SteadyLineV2", new Dictionary<string, string> { { "Technique Name", "STEADY LINE" }, { "Technique Rules", "Reduce Line Load by 2\nfor your next Descend or Surface." } });
            Prefab(root.transform, "DescendButton", 2830, 380, 850, 335);
            Prefab(root.transform, "SurfaceButton", 2830, 760, 850, 340);
            SpriteImage(root.transform, theme, "BrassConnectorV3", 420, 850, 180, 270);
            for (int n = 0; n < 8; n++) SpriteImage(root.transform, theme, n < 4 ? "LoadFilled" : "LoadEmpty", 100 + n * 100, 1180, 80, 60);
            string[] samples = { "Cinzel Regular", "Cinzel Semibold", "Cinzel Bold", "Source Serif 4 Regular", "Source Serif 4 Semibold", "Source Serif 4 Bold" };
            for (int n = 0; n < samples.Length; n++)
            {
                TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Root + "Fonts/TMP/" + samples[n] + ".asset");
                Label(root.transform, samples[n] + "  /  Catch & Bait  /  0123456789 ?", 100, 1350 + n * 115, 3600, 90, 52, font);
            }
            Canvas.ForceUpdateCanvases(); foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>()) text.ForceMeshUpdate(true, true);
            camera.Render(); RenderTexture.active = target; output = new Texture2D(3840, 2160, TextureFormat.RGB24, false);
            output.ReadPixels(new Rect(0, 0, 3840, 2160), 0, 0); output.Apply(); File.WriteAllBytes(Evidence + "UI02Assets4K.png", output.EncodeToPNG());
        }
        finally
        {
            RenderTexture.active = previous; if (output != null) UnityEngine.Object.DestroyImmediate(output);
            target.Release(); UnityEngine.Object.DestroyImmediate(target); EditorSceneManager.ClosePreviewScene(preview);
        }
    }

    /// <summary>Instances a template only within the disposable preview hierarchy.</summary>
    private static GameObject Prefab(Transform parent, string name, float x, float y, float width, float height)
    {
        GameObject instance = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Prefabs/" + name + ".prefab"), parent);
        Place(instance.GetComponent<RectTransform>(), x, y, width, height); return instance;
    }

    /// <summary>Fills presentation-only sample fields without writing to prefab or gameplay assets.</summary>
    private static void Populate(GameObject root, MidnightFieldJournalTheme theme, string art, Dictionary<string, string> text)
    {
        foreach (TMP_Text field in root.GetComponentsInChildren<TMP_Text>(true)) if (text.TryGetValue(field.name, out string value)) field.text = value;
        foreach (UnityEngine.UI.Image image in root.GetComponentsInChildren<UnityEngine.UI.Image>(true))
            if (image.name == "Runtime Artwork") { theme.TryGetSprite(art, out Sprite sprite); image.sprite = sprite; image.enabled = true; }
    }

    /// <summary>Adds a noninteractive source image at design-pixel coordinates.</summary>
    private static UnityEngine.UI.Image SpriteImage(Transform parent, MidnightFieldJournalTheme theme, string key, float x, float y, float width, float height)
    {
        GameObject obj = new GameObject(key, typeof(RectTransform), typeof(UnityEngine.UI.Image)); obj.transform.SetParent(parent, false);
        Place(obj.GetComponent<RectTransform>(), x, y, width, height); theme.TryGetSprite(key, out Sprite sprite);
        UnityEngine.UI.Image image = obj.GetComponent<UnityEngine.UI.Image>(); image.sprite = sprite; image.raycastTarget = false; return image;
    }

    /// <summary>Adds a font sample with explicit glyph size and color rather than runtime fitting.</summary>
    private static void Label(Transform parent, string value, float x, float y, float width, float height, float size, TMP_FontAsset font)
    {
        GameObject obj = new GameObject("Font Sample", typeof(RectTransform)); obj.transform.SetParent(parent, false);
        Place(obj.GetComponent<RectTransform>(), x, y, width, height); TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.font = font; text.fontSize = size; text.text = value; text.color = new Color32(240, 224, 189, 255); text.raycastTarget = false;
    }

    /// <summary>Places preview content using the contract's top-left reference coordinates.</summary>
    private static void Place(RectTransform rect, float x, float y, float width, float height)
    { rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 1); rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(width, height); }
}
