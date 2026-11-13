using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Imports the supplemental reel illustrations and preserves their mappings through Discover regeneration.</summary>
public static class TackleReelSymbolSetup
{
    public const string Source = "Docs/UIThemes/TackleApparatusConcept/ReelSymbols/";
    public const string Destination = TackleDiscoverArtSetup.Root + "Art/Discover/Symbols/";
    private static readonly string[] Tags = { "Fish", "Predator", "Schooling", "Small", "Heavy", "Anchored", "Armored", "Elusive" };
    [Serializable] private sealed class Manifest { public Entry[] entries; }
    [Serializable] private sealed class Entry { public string tag, file, sha256; public int width, height; }

    /// <summary>Copies generated originals unchanged and applies the same point-filtered sprite import contract as the existing reel art.</summary>
    public static void Import()
    {
        Directory.CreateDirectory(Destination);
        var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(Source + "Manifest.json"));
        foreach (var entry in manifest.entries)
        {
            string path = Destination + entry.file;
            File.Copy(Source + "Source/" + entry.file, path, true);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100; importer.spritePivot = new Vector2(.5f, .5f); importer.spriteBorder = Vector4.zero;
            importer.filterMode = FilterMode.Point; importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = false; importer.isReadable = false; importer.sRGBTexture = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput; importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings); importer.SaveAndReimport();
        }
    }

    /// <summary>Writes all eight tag mappings while retaining the original Fish and Schooling sprites.</summary>
    public static void Bind(SerializedObject presentation)
    {
        var mapping = presentation.FindProperty("tagArtwork"); mapping.arraySize = Tags.Length;
        for (int i = 0; i < Tags.Length; i++)
        {
            string path = Tags[i] == "Fish" ? TackleDiscoverArtSetup.Root + "Art/Common/portrait-mackerel.png"
                : Tags[i] == "Schooling" ? TackleDiscoverArtSetup.Root + "Art/Common/tag-schooling.png"
                : Destination + "tag-" + Tags[i].ToLowerInvariant() + ".png";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) throw new InvalidOperationException("Missing reel illustration: " + path);
            var entry = mapping.GetArrayElementAtIndex(i);
            entry.FindPropertyRelative("tag").stringValue = Tags[i];
            entry.FindPropertyRelative("sprite").objectReferenceValue = sprite;
        }
    }

    /// <summary>Adds the new symbols to the existing prefab without rebuilding artwork, animation or scene overrides.</summary>
    [MenuItem("Fishing Cards/Slot Machine/Install Remaining Reel Symbols")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode before importing reel symbols.");
        Import();
        var prefab = PrefabUtility.LoadPrefabContents(TackleDiscoverArtSetup.PrefabPath);
        try
        {
            var data = new SerializedObject(prefab.GetComponent<TackleDiscoverPresentation>());
            Bind(data); data.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(prefab, TackleDiscoverArtSetup.PrefabPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(prefab); }
        AssetDatabase.SaveAssets();
    }

    /// <summary>Checks generated-byte provenance, usable alpha, intact padding, sprite settings and complete tag coverage.</summary>
    public static int Validate()
    {
        int checks = 0;
        Action<bool, string> check = (condition, message) => { if (!condition) throw new InvalidOperationException(message); checks++; };
        var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(Source + "Manifest.json"));
        check(manifest.entries.Length == 6, "Expected six supplemental symbols.");
        foreach (var entry in manifest.entries)
        {
            byte[] bytes = File.ReadAllBytes(Destination + entry.file);
            using (var hash = System.Security.Cryptography.SHA256.Create())
                check(BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant() == entry.sha256,
                    "Generated symbol pixels changed: " + entry.tag);
            var texture = new Texture2D(2, 2);
            try
            {
                check(ImageConversion.LoadImage(texture, bytes) && texture.width == entry.width && texture.height == entry.height,
                    "Invalid symbol dimensions: " + entry.tag);
                var pixels = texture.GetPixels32(); bool clear = false, opaque = false;
                foreach (var pixel in pixels) { clear |= pixel.a == 0; opaque |= pixel.a == 255; }
                check(clear && opaque, "Missing usable alpha: " + entry.tag);
                bool padded = true;
                // Generated cutouts can retain invisible one-unit alpha noise; visible contour pixels must stay clear of every edge.
                for (int x = 0; x < texture.width; x++) padded &= pixels[x].a <= 1 && pixels[(texture.height - 1) * texture.width + x].a <= 1;
                for (int y = 0; y < texture.height; y++) padded &= pixels[y * texture.width].a <= 1 && pixels[(y + 1) * texture.width - 1].a <= 1;
                check(padded, "Symbol touches the canvas edge: " + entry.tag);
            }
            finally { UnityEngine.Object.DestroyImmediate(texture); }
            var importer = (TextureImporter)AssetImporter.GetAtPath(Destination + entry.file);
            check(importer.textureType == TextureImporterType.Sprite && importer.spriteImportMode == SpriteImportMode.Single
                && importer.filterMode == FilterMode.Point && !importer.mipmapEnabled && !importer.isReadable
                && importer.textureCompression == TextureImporterCompression.Uncompressed && importer.alphaIsTransparency,
                "Incorrect pixel-art import settings: " + entry.tag);
        }
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TackleDiscoverArtSetup.PrefabPath);
        var mapping = new SerializedObject(prefab.GetComponent<TackleDiscoverPresentation>()).FindProperty("tagArtwork");
        check(mapping.arraySize == Tags.Length, "Incomplete reel illustration mappings.");
        for (int i = 0; i < Tags.Length; i++)
            check(mapping.GetArrayElementAtIndex(i).FindPropertyRelative("tag").stringValue == Tags[i]
                && mapping.GetArrayElementAtIndex(i).FindPropertyRelative("sprite").objectReferenceValue != null,
                "Missing tag illustration: " + Tags[i]);
        return checks;
    }

    /// <summary>Renders every mapped symbol at its actual reel size on the approved paper/desk art without altering a run.</summary>
    public static void CaptureFamily()
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Enter Play Mode to capture through the active render pipeline.");
        var root = new GameObject("Reel symbol family preview", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler));
        try
        {
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 1000;
            var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
            var stage = PreviewRect(root.transform, "1920x1080 Stage", Vector2.zero, new Vector2(1920, 1080));
            var background = PreviewRect(stage, "Desk", Vector2.zero, new Vector2(1920, 1080)).gameObject.AddComponent<UnityEngine.UI.Image>();
            background.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(TackleDiscoverArtSetup.Root + "Art/Common/desk-background.png");
            var font = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(TackleDiscoverArtSetup.Root + "Fonts/PressStart2P.asset");
            var title = PreviewRect(stage, "Title", new Vector2(0, 470), new Vector2(1600, 60)).gameObject.AddComponent<TMPro.TextMeshProUGUI>();
            title.font = font; title.fontSize = 30; title.alignment = TMPro.TextAlignmentOptions.Center;
            title.color = new Color32(222, 211, 165, 255); title.text = "REEL SYMBOLS / ACTUAL GAME SIZE";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TackleDiscoverArtSetup.PrefabPath);
            var mapping = new SerializedObject(prefab.GetComponent<TackleDiscoverPresentation>()).FindProperty("tagArtwork");
            for (int i = 0; i < mapping.arraySize; i++)
            {
                var cell = PreviewRect(stage, Tags[i], new Vector2(-525 + 350 * (i % 4), 210 - 400 * (i / 4)), new Vector2(320, 365));
                var paper = cell.gameObject.AddComponent<UnityEngine.UI.Image>();
                paper.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(TackleDiscoverArtSetup.Root + "Art/Discover/reel-drum.png");
                var icon = PreviewRect(cell, "Symbol", new Vector2(0, 55), new Vector2(212, 98)).gameObject.AddComponent<UnityEngine.UI.Image>();
                icon.sprite = (Sprite)mapping.GetArrayElementAtIndex(i).FindPropertyRelative("sprite").objectReferenceValue;
                icon.preserveAspect = true;
                var label = PreviewRect(cell, "Native tag label", new Vector2(0, -95), new Vector2(260, 34)).gameObject.AddComponent<TMPro.TextMeshProUGUI>();
                label.font = font; label.fontSize = 22; label.alignment = TMPro.TextAlignmentOptions.Center;
                label.color = new Color32(55, 59, 44, 255); label.text = Tags[i].ToUpperInvariant();
            }
            SlotMachineSceneChecks.Capture(canvas, "ReelSymbolFamily1080.png", output: "Docs/Validation/TackleReelSymbols/");
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    /// <summary>Creates a centered native UI rectangle for the asset review sheet only.</summary>
    private static RectTransform PreviewRect(Transform parent, string name, Vector2 position, Vector2 size)
    {
        var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
        rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = position; rect.sizeDelta = size; return rect;
    }
}
