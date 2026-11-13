using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Authors the moving paper layer independently of the preserved drum rim and native symbol fields.</summary>
public static class TackleReelSurfaceSetup
{
    public const string Source = "Docs/UIThemes/TackleApparatusConcept/ReelSurface/";
    public const string ArtRoot = TackleDiscoverArtSetup.Root + "Art/ReelSurface/";
    private const string MaterialPath = TackleDiscoverArtSetup.Root + "Materials/RotatingReelSurface.mat";
    private const string ShaderPath = TackleDiscoverArtSetup.Root + "Shaders/ReelSurface.shader";

    /// <summary>Imports the flat paper unchanged and creates a GUID-stable material with explicit source-art sampling regions.</summary>
    private static Material ImportMaterial()
    {
        Directory.CreateDirectory(ArtRoot);
        Directory.CreateDirectory(TackleDiscoverArtSetup.Root + "Materials");
        string paperPath = ArtRoot + "reel-paper-flat.png";
        File.Copy(Source + "reel-paper-flat.png", paperPath, true);
        AssetDatabase.ImportAsset(paperPath, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(paperPath);
        importer.textureType = TextureImporterType.Default; importer.sRGBTexture = true;
        importer.filterMode = FilterMode.Point; importer.wrapMode = TextureWrapMode.Clamp;
        importer.mipmapEnabled = false; importer.isReadable = false; importer.alphaSource = TextureImporterAlphaSource.None;
        importer.textureCompression = TextureImporterCompression.Uncompressed; importer.npotScale = TextureImporterNPOTScale.None;
        importer.maxTextureSize = 2048; importer.SaveAndReimport();
        var shader = AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);
        if (shader == null) throw new InvalidOperationException("The rotating reel shader has not been imported.");
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, MaterialPath); }
        material.shader = shader;
        material.SetTexture("_PaperTex", AssetDatabase.LoadAssetAtPath<Texture2D>(paperPath));
        // Original 412x490 drum texels: only these decorative crops rotate; their surrounding rim never does.
        material.SetVector("_PlaqueUV", new Vector4(49f / 412, 85f / 490, 316f / 412, 79f / 490));
        material.SetVector("_DividerUV", new Vector4(34f / 412, 390f / 490, 344f / 412, 10f / 490));
        material.SetVector("_Plaque", new Vector4(123, 26, -95, 0));
        material.SetVector("_Dividers", new Vector4(134, 2.8f, 112, -48));
        material.SetFloat("_Phase", 0); material.SetFloat("_Radius", 172);
        material.SetVector("_CylinderSize", new Vector4(320, 365, 0, 0));
        EditorUtility.SetDirty(material);
        return material;
    }

    /// <summary>Updates only the reel background layers, retaining the existing symbols, controls, timing and scene bindings.</summary>
    [MenuItem("Fishing Cards/Slot Machine/Install Rotating Reel Surface")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode before installing the reel surface.");
        var root = PrefabUtility.LoadPrefabContents(TackleDiscoverArtSetup.PrefabPath);
        try { Configure(root); PrefabUtility.SaveAsPrefabAsset(root, TackleDiscoverArtSetup.PrefabPath); }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
    }

    /// <summary>Composes a private-material surface behind each native cylinder; rerunning this authoring operation is idempotent.</summary>
    public static void Configure(GameObject root)
    {
        Material material = ImportMaterial();
        Texture drum = AssetDatabase.LoadAssetAtPath<Sprite>(TackleDiscoverArtSetup.Root + "Art/Discover/reel-drum.png").texture;
        var stage = root.transform.Find("1920x1080 Stage");
        foreach (var original in stage.GetComponentsInChildren<UnityEngine.UI.Image>(true)
            .Where(image => image.sprite != null && image.sprite.name == "reel-drum")) original.gameObject.SetActive(false);
        foreach (var reel in root.GetComponentsInChildren<TackleReelAnimator>(true))
        {
            var renderer = reel.GetComponentInChildren<TackleReelSurface>(true);
            if (renderer == null)
            {
                var child = new GameObject("Rotating Paper and Decorations", typeof(RectTransform), typeof(UnityEngine.UI.RawImage), typeof(TackleReelSurface));
                var rect = (RectTransform)child.transform; rect.SetParent(reel.transform, false);
                rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
                renderer = child.GetComponent<TackleReelSurface>();
            }
            renderer.transform.SetAsFirstSibling();
            var image = renderer.GetComponent<UnityEngine.UI.RawImage>(); image.texture = drum; image.raycastTarget = false; image.material = material;
            renderer.Configure(image, material);
            var data = new SerializedObject(reel); data.FindProperty("surface").objectReferenceValue = renderer; data.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    /// <summary>Checks source preservation, complete native-cylinder bindings and a compiled shader in the serialized prefab.</summary>
    public static int Validate()
    {
        int checks = 0;
        Action<bool, string> check = (condition, message) => { if (!condition) throw new InvalidOperationException(message); checks++; };
        check(File.ReadAllBytes(Source + "reel-paper-flat.png").SequenceEqual(File.ReadAllBytes(ArtRoot + "reel-paper-flat.png")), "Paper source was altered.");
        var shader = AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);
        check(shader != null && !ShaderUtil.ShaderHasError(shader), "Reel shader did not compile.");
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TackleDiscoverArtSetup.PrefabPath);
        var reels = prefab.GetComponentsInChildren<TackleReelAnimator>(true);
        check(reels.Length == 3 && prefab.GetComponentsInChildren<TackleReelSurface>(true).Length == 3, "Expected three independent reel surfaces.");
        foreach (var reel in reels)
        {
            var data = new SerializedObject(reel); var renderer = data.FindProperty("surface").objectReferenceValue as TackleReelSurface;
            check(renderer != null && renderer.transform.parent == reel.transform, "Reel surface is not owned by its cylinder.");
            var image = renderer.GetComponent<UnityEngine.UI.RawImage>();
            check(image != null && image.texture != null && image.material.shader == shader && !image.raycastTarget,
                "Reel surface material, source or input pass-through is invalid.");
        }
        foreach (var image in prefab.GetComponentsInChildren<UnityEngine.UI.Image>(true).Where(i => i.sprite != null && i.sprite.name == "reel-drum"))
            check(!image.gameObject.activeSelf, "A baked static divider/plaque remains behind the moving layer.");
        return checks;
    }
}
