using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

/// <summary>Imports the approved creature library and binds one authoritative illustration per creature definition.</summary>
public static class CreatureSpriteSetup
{
    public const string Source = "Docs/UIThemes/TackleApparatusConcept/CreatureSprites/";
    public const string Destination = TackleDiscoverArtSetup.Root + "Art/Creatures/";
    private const int Padding = 4;

    [Serializable] public sealed class Library { public Entry[] sprites; }
    [Serializable] public sealed class Entry
    {
        public string id, name, definition, filename, sha256;
        public int width, height;
        public int[] bounds;
    }
    [Serializable] private sealed class ImportRecord
    {
        public string id, sourceSha256, importedSha256, assetPath;
        public int[] sourceBoundsTopLeft;
        public int transparentPadding;
    }
    [Serializable] private sealed class ImportManifest { public ImportRecord[] sprites; }

    /// <summary>Updates existing prefab previews without rebuilding unrelated reels, typography or scene bindings.</summary>
    [MenuItem("Fishing Cards/Artwork/Integrate Creature Sprites")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Import creature artwork outside Play Mode.");
        Import();
        BindPreview(TackleDiscoverArtSetup.RigPrefabPath, typeof(TackleRigCardView), "creature_sardine");
        BindPreview(LandingChallengeSetup.PrefabPath, typeof(TackleLandingPresentation), "creature_garfish");
        AssetDatabase.SaveAssets();
    }

    /// <summary>Verifies the complete roster before importing; trims only transparent margins without resampling source pixels.</summary>
    public static void Import()
    {
        var entries = ReadLibrary();
        var definitions = AssetDatabase.FindAssets("t:CardDefinition", new[] { "Assets/Cards" })
            .Select(g => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(c => c.CardType == CardType.Creature || c.CardType == CardType.ApexEncounter).ToArray();
        if (entries.Length != definitions.Length || entries.Select(e => e.id).Distinct().Count() != entries.Length)
            throw new InvalidOperationException("Creature sprite roster does not cover the current creature definitions exactly.");
        foreach (var entry in entries)
        {
            if (entry.filename != entry.id + ".png" || !definitions.Any(c => c.UniqueId == entry.id && AssetDatabase.GetAssetPath(c) == entry.definition))
                throw new InvalidOperationException("Invalid sprite identity or definition: " + entry.id);
            if (Hash(File.ReadAllBytes(Source + "Sprites/" + entry.filename)) != entry.sha256)
                throw new InvalidOperationException("Approved creature source changed: " + entry.filename);
        }

        Directory.CreateDirectory(Destination);
        var records = new ImportRecord[entries.Length];
        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            string path = Destination + entry.filename;
            byte[] pixels = Crop(entry);
            if (!File.Exists(path) || !File.ReadAllBytes(path).SequenceEqual(pixels)) File.WriteAllBytes(path, pixels);
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
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) throw new InvalidOperationException("Creature sprite failed to import: " + entry.id);
            BindDefinition(AssetDatabase.LoadAssetAtPath<CardDefinition>(entry.definition), sprite);
            records[i] = new ImportRecord { id = entry.id, assetPath = path, sourceSha256 = entry.sha256,
                importedSha256 = Hash(pixels), sourceBoundsTopLeft = entry.bounds, transparentPadding = Padding };
        }
        string manifest = TackleDiscoverArtSetup.Root + "CreatureSpriteImports.json";
        File.WriteAllText(manifest, JsonUtility.ToJson(new ImportManifest { sprites = records }, true) + "\n");
        AssetDatabase.ImportAsset(manifest);
        AssetDatabase.SaveAssets();
    }

    /// <summary>Reads only selected production sprites; discarded generation variants are never imported.</summary>
    public static Entry[] ReadLibrary() => JsonUtility.FromJson<Library>(File.ReadAllText(Source + "Manifest.json")).sprites;

    /// <summary>Centers aspect-fit silhouettes inside the existing portrait rectangle without moving or resizing that rectangle.</summary>
    public static void CenterPortrait(UnityEngine.UI.Image image)
    {
        var rect = image.rectTransform; var center = new Vector2(.5f, .5f);
        rect.anchoredPosition += Vector2.Scale(center - rect.pivot, rect.rect.size);
        rect.pivot = center; image.preserveAspect = true;
    }

    /// <summary>Preserves RGBA samples inside the recorded alpha bounds, adding a clear four-pixel border.</summary>
    private static byte[] Crop(Entry entry)
    {
        var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        Texture2D result = null;
        try
        {
            if (!source.LoadImage(File.ReadAllBytes(Source + "Sprites/" + entry.filename)) || source.width != entry.width || source.height != entry.height)
                throw new InvalidOperationException("Unexpected source dimensions: " + entry.id);
            int x = entry.bounds[0], top = entry.bounds[1], w = entry.bounds[2], h = entry.bounds[3];
            if (x < 0 || top < 0 || w <= 0 || h <= 0 || x + w > source.width || top + h > source.height)
                throw new InvalidOperationException("Invalid alpha bounds: " + entry.id);
            int width = w + Padding * 2, height = h + Padding * 2;
            var input = source.GetPixels32(); var output = new Color32[width * height];
            int bottom = source.height - top - h;
            for (int row = 0; row < h; row++)
                Array.Copy(input, (bottom + row) * source.width + x, output, (row + Padding) * width + Padding, w);
            result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            result.SetPixels32(output); result.Apply(); return result.EncodeToPNG();
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(source);
            if (result != null) UnityEngine.Object.DestroyImmediate(result);
        }
    }

    /// <summary>Replaces illustration fields and old illustration-specific transforms; all gameplay fields remain untouched.</summary>
    private static void BindDefinition(CardDefinition definition, Sprite sprite)
    {
        var data = new SerializedObject(definition);
        data.FindProperty("artwork").objectReferenceValue = sprite;
        data.FindProperty("encounterArtwork").objectReferenceValue = sprite;
        data.FindProperty("artworkLayout").enumValueIndex = (int)CardArtworkLayout.MaskedRegion;
        data.FindProperty("encounterArtworkScale").floatValue = 1;
        data.FindProperty("encounterArtworkRotation").floatValue = 0;
        data.FindProperty("encounterArtworkOffset").vector2Value = Vector2.zero;
        // The old Squid face baked its illustration into a full card. Use the reusable face and live fields.
        data.FindProperty("cardFaceArtwork").objectReferenceValue = null;
        data.FindProperty("cardFaceIncludesName").boolValue = false;
        data.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>Replaces only the prefab's inactive preview image; live views bind the definition for every species.</summary>
    private static void BindPreview(string path, Type viewType, string creatureId)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) return;
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var data = new SerializedObject(root.GetComponent(viewType));
            var image = (UnityEngine.UI.Image)data.FindProperty("portrait").objectReferenceValue;
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Destination + creatureId + ".png");
            CenterPortrait(image);
            if (viewType == typeof(TackleRigCardView))
            {
                var name = (TMPro.TMP_Text)data.FindProperty("nameLabel").objectReferenceValue;
                name.rectTransform.anchoredPosition = new Vector2(52.5f, -25); name.rectTransform.sizeDelta = new Vector2(210, 32);
                name.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
                name.enableAutoSizing = true; name.fontSizeMin = 12; name.fontSizeMax = 21;
            }
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    /// <summary>Records exact source and derived-image identities for repeatable asset provenance.</summary>
    private static string Hash(byte[] bytes)
    {
        using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
    }
}
