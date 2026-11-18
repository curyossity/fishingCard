using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Versions Discover's housing independently of its slot rules, cylindrical reels and original artwork.</summary>
public static class TackleDiscoverHousingSetup
{
    public const string Source = "Docs/UIThemes/TackleApparatusConcept/DiscoverIntegrated/";
    public const string HousingPath = TackleDiscoverArtSetup.Root + "Art/Discover/Integrated/discover-machine.png";
    public const string FullSizeHousingPath = TackleDiscoverArtSetup.Root + "Art/Discover/Integrated/discover-machine-full-size.png";
    public const string OriginalPrefab = TackleDiscoverArtSetup.Root + "Prefabs/DiscoverCanvas-Original.prefab";
    public const string CompactPrefab = TackleDiscoverArtSetup.Root + "Prefabs/DiscoverCanvas-Compact.prefab";
    private const string SelectionPath = TackleDiscoverArtSetup.Root + "DiscoverHousingSelection.json";
    // Native socket columns share a lifted baseline that clears the raised central lamp fascia at full reel size.
    private static readonly Vector2[] FullSizeSocketCenters = { new Vector2(440,423), new Vector2(860,423), new Vector2(1290,423) };
    [Serializable] private sealed class Selection { public bool unifiedHousing = true, fullSizeReels; }
    [Serializable] private sealed class Contract { public string outputSha256; public int alphaDifferences, protectedVisibleColorDifferences; }

    /// <summary>Retains the selected version across full Discover authoring and Editor restarts.</summary>
    public static bool UsesUnifiedHousing => !File.Exists(SelectionPath) || JsonUtility.FromJson<Selection>(File.ReadAllText(SelectionPath)).unifiedHousing;
    /// <summary>Distinguishes full-size wheels while retaining compatibility with the earlier compact selection file.</summary>
    public static bool UsesFullSizeReels => UsesUnifiedHousing && File.Exists(SelectionPath) && JsonUtility.FromJson<Selection>(File.ReadAllText(SelectionPath)).fullSizeReels;
    public static string SelectedHousingPath => UsesFullSizeReels ? FullSizeHousingPath : HousingPath;
    public static string SelectedSource => Source + (UsesFullSizeReels ? "FullSize/" : "");

    /// <summary>Installs the shared-shell version while preserving the complete previous animated prefab once.</summary>
    [MenuItem("Fishing Cards/Slot Machine/Housing/Use Unified Housing (Compact Wheels)")]
    public static void InstallCurrentScene()
    { InstallVariant(false); }

    /// <summary>Installs original-size wheels inside the unified outer silhouette and preserves the compact version.</summary>
    [MenuItem("Fishing Cards/Slot Machine/Housing/Use Unified Housing (Original Wheel Size)")]
    public static void InstallFullSizeCurrentScene()
    { InstallVariant(true); }

    /// <summary>Versions only the Discover composition and keeps the existing controller/scene binding.</summary>
    private static void InstallVariant(bool fullSize)
    {
        RequireScene();
        if (AssetDatabase.LoadAssetAtPath<GameObject>(OriginalPrefab) == null &&
            !AssetDatabase.CopyAsset(TackleDiscoverArtSetup.PrefabPath, OriginalPrefab))
            throw new InvalidOperationException("Could not preserve the original Discover prefab.");
        if (fullSize && UsesUnifiedHousing && !UsesFullSizeReels && AssetDatabase.LoadAssetAtPath<GameObject>(CompactPrefab) == null &&
            !AssetDatabase.CopyAsset(TackleDiscoverArtSetup.PrefabPath, CompactPrefab))
            throw new InvalidOperationException("Could not preserve the compact Discover prefab.");
        Select(true, fullSize);
        var root = PrefabUtility.LoadPrefabContents(TackleDiscoverArtSetup.PrefabPath);
        try { Configure(root); PrefabUtility.SaveAsPrefabAsset(root, TackleDiscoverArtSetup.PrefabPath); }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        SaveScene();
    }

    /// <summary>Restores the archived functional composition without replacing its original sprite or material sources.</summary>
    [MenuItem("Fishing Cards/Slot Machine/Housing/Restore Original Housing")]
    public static void RestoreOriginal()
    {
        RequireScene();
        if (AssetDatabase.LoadAssetAtPath<GameObject>(OriginalPrefab) == null)
            throw new InvalidOperationException("The original Discover prefab has not been archived.");
        var root = PrefabUtility.LoadPrefabContents(OriginalPrefab);
        try { PrefabUtility.SaveAsPrefabAsset(root, TackleDiscoverArtSetup.PrefabPath); }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        Select(false); SaveScene();
    }

    /// <summary>Fits the existing native-coordinate wheel assemblies into the common shell without changing their projection math.</summary>
    public static void Configure(GameObject root)
    {
        if (!UsesUnifiedHousing) return;
        var sprite = ImportHousing();
        var stage = root.transform.Find("1920x1080 Stage");
        var housing = stage.Find("machine-housing").GetComponent<UnityEngine.UI.Image>();
        housing.sprite = sprite; housing.type = UnityEngine.UI.Image.Type.Simple; housing.preserveAspect = false;
        Place(housing.rectTransform, TackleMachineGeometry.StageBounds);
        for (int i = 0; i < 3; i++)
        {
            // Uniform parent scaling preserves the shader radius, native face offsets and elastic settling.
            var wheel = (RectTransform)stage.Find("Cylindrical Reel " + i);
            wheel.localScale = Vector3.one * (UsesFullSizeReels ? 1 : .72f);
            if (UsesFullSizeReels)
            {
                var bounds = TackleMachineGeometry.StageBounds;
                var center = bounds.position + Vector2.Scale(FullSizeSocketCenters[i], new Vector2(bounds.width / 1739, bounds.height / 904));
                wheel.anchoredPosition = new Vector2(center.x - wheel.sizeDelta.x * .5f, -center.y + wheel.sizeDelta.y * .5f);
            }
            else wheel.anchoredPosition = new Vector2(703 + i * 324, -295);
        }
        var art = new SerializedObject(root.GetComponent<TackleDiscoverPresentation>());
        for (int i = 0; i < 3; i++)
        {
            var lever = (UnityEngine.UI.Image)art.FindProperty("lockLevers").GetArrayElementAtIndex(i).objectReferenceValue;
            Place(lever.rectTransform, UsesFullSizeReels ? new Rect(719 + i * 324,732,48,48) : new Rect(719 + i * 324, 649, 55, 58));
            var button = stage.Find("Lock " + i);
            Place((RectTransform)button, new Rect(787 + i * 324, UsesFullSizeReels ? 735 : 655, 135, 45));
            // The whole housing supplies the mounting frame; retain only native text, focus and hit feedback.
            foreach (var image in button.GetComponentsInChildren<UnityEngine.UI.Image>(true))
                if (image.sprite != null && image.sprite.name == "status-plaque") image.gameObject.SetActive(false);
        }
        var lamps = (UnityEngine.UI.Image)art.FindProperty("matchLamps").objectReferenceValue;
        Place(lamps.rectTransform, UsesFullSizeReels ? new Rect(1043,683,197,35) : new Rect(1083,733,114,42)); lamps.preserveAspect = true;
        TackleReelApertureSetup.Configure(root, UsesFullSizeReels);
        TackleReelAnimationSetup.Configure(root);
    }

    /// <summary>Imports the normalized 1739x904 source with its proven shared alpha and protected exterior contract.</summary>
    private static Sprite ImportHousing()
    {
        var contract = JsonUtility.FromJson<Contract>(File.ReadAllText(SelectedSource + "Normalization.json"));
        byte[] bytes = File.ReadAllBytes(SelectedSource + "discover-machine.png");
        using (var sha = SHA256.Create())
            if (BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant() != contract.outputSha256
                || contract.alphaDifferences != 0 || contract.protectedVisibleColorDifferences != 0)
                throw new InvalidOperationException("Discover housing does not match its normalized source contract.");
        string target = SelectedHousingPath;
        Directory.CreateDirectory(Path.GetDirectoryName(target));
        if (!File.Exists(target) || !File.ReadAllBytes(target).SequenceEqual(bytes)) File.WriteAllBytes(target, bytes);
        AssetDatabase.ImportAsset(target, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(target);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100; importer.spriteBorder = Vector4.zero;
        importer.filterMode = FilterMode.Point; importer.wrapMode = TextureWrapMode.Clamp;
        importer.mipmapEnabled = false; importer.isReadable = false; importer.sRGBTexture = true;
        importer.alphaSource = TextureImporterAlphaSource.FromInput; importer.alphaIsTransparency = true;
        importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 2048;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings); importer.SaveAndReimport();
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(target);
        if (sprite.texture.width != 1739 || sprite.texture.height != 904) throw new InvalidOperationException("Discover housing was resized.");
        return sprite;
    }

    /// <summary>Writes the project-owned version selection without relying on per-user Editor preferences.</summary>
    private static void Select(bool unified, bool fullSize = false)
    {
        File.WriteAllText(SelectionPath, JsonUtility.ToJson(new Selection { unifiedHousing = unified, fullSizeReels = fullSize }, true) + "\n");
        AssetDatabase.ImportAsset(SelectionPath);
    }

    /// <summary>Rejects edits to a running or unrelated scene before modifying a prefab or selection.</summary>
    private static void RequireScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().path != JournalComparisonSceneBuilder.ScenePath)
            throw new InvalidOperationException("Open the comparison scene outside Play Mode.");
    }

    /// <summary>Preserves the live controller binding and saves only the active comparison scene.</summary>
    private static void SaveScene()
    {
        var scene = SceneManager.GetActiveScene(); TackleDiscoverArtSetup.Install(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save Discover housing selection.");
        AssetDatabase.SaveAssets();
    }

    /// <summary>Positions a top-left anchored native UI element in the shared stage coordinate space.</summary>
    private static void Place(RectTransform rect, Rect bounds)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(bounds.x, -bounds.y); rect.sizeDelta = bounds.size;
    }
}
