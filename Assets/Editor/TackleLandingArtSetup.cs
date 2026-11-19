using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>Imports the integrated Landing variant and authors live controls inside its shared machine silhouette.</summary>
public static class TackleLandingArtSetup
{
    private const string Root = TackleDiscoverArtSetup.Root;
    private const string Source = "Docs/UIThemes/TackleApparatusConcept/AssetLibrary/";
    public const string IndicatorPath = Root + "Art/Landing/Indicators/generic-fish.png";
    public const string HousingPath = Root + "Art/Landing/Integrated/landing-machine.png";
    public const string HousingSource = "Docs/UIThemes/TackleApparatusConcept/LandingIntegrated/landing-machine.png";
    private static readonly Color Cream = new Color32(222, 211, 165, 255), Ink = new Color32(55, 59, 44, 255);
    private static Dictionary<string, Sprite> sprites;
    private static TMP_FontAsset displayFont, bodyFont;
    [Serializable] private sealed class Manifest { public Asset[] assets; }
    [Serializable] private sealed class Asset { public string name, exportPath; public string[] uses; public float[] unityBorderLBRT; public bool alpha; }
    [Serializable] private sealed class HousingContract { public string baseSha256,outputSha256; }

    /// <summary>Copies only approved Landing/shared exports at original size and keeps existing GUIDs and tuning.</summary>
    public static void BuildAssets()
    {
        CreatureSpriteSetup.Import();
        sprites = new Dictionary<string, Sprite>();
        var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(Source + "Manifest.json"));
        foreach (var asset in manifest.assets.Where(a => a.uses.Contains("landing")))
        {
            string path = Root + "Art/" + asset.exportPath.Substring("Exports/".Length);
            Folder(Path.GetDirectoryName(path).Replace('\\', '/'));
            if (!File.Exists(path) || !File.ReadAllBytes(path).SequenceEqual(File.ReadAllBytes(Source + asset.exportPath))) File.Copy(Source + asset.exportPath, path, true);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100; importer.spritePivot = new Vector2(.5f, .5f);
            importer.spriteBorder = asset.unityBorderLBRT == null || asset.unityBorderLBRT.Length == 0 ? Vector4.zero
                : new Vector4(asset.unityBorderLBRT[0], asset.unityBorderLBRT[1], asset.unityBorderLBRT[2], asset.unityBorderLBRT[3]);
            importer.filterMode = FilterMode.Point; importer.wrapMode = TextureWrapMode.Clamp; importer.mipmapEnabled = false; importer.isReadable = false;
            importer.sRGBTexture = true; importer.alphaSource = TextureImporterAlphaSource.FromInput; importer.alphaIsTransparency = asset.alpha;
            importer.npotScale = TextureImporterNPOTScale.None; importer.maxTextureSize = 2048; importer.textureCompression = TextureImporterCompression.Uncompressed;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings); settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings); importer.SaveAndReimport(); sprites.Add(asset.name, AssetDatabase.LoadAssetAtPath<Sprite>(path));
        }
        ImportIndicator();
        ImportHousing();
        displayFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Root + "Fonts/PressStart2P.asset");
        bodyFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Root + "Fonts/VT323.asset");
        if (displayFont == null || bodyFont == null || AssetDatabase.LoadAssetAtPath<GameObject>(TackleDiscoverArtSetup.RigPrefabPath) == null)
            throw new InvalidOperationException("Install the shared Tackle Apparatus art and fonts before Landing.");
        BuildCanvas(); AssetDatabase.SaveAssets();
    }

    /// <summary>Replaces the plain prefab contents while retaining its asset GUID and the existing Landing gameplay/input boundary.</summary>
    private static void BuildCanvas()
    {
        var root = new GameObject("Tackle Landing Canvas", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler),
            typeof(UnityEngine.UI.GraphicRaycaster), typeof(LandingChallengeView), typeof(TackleLandingPresentation));
        try
        {
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay; root.GetComponent<Canvas>().sortingOrder = 400;
            var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
            root.AddComponent<UnityEngine.UI.Image>().color = new Color32(14, 17, 15, 255);
            var stage = Rect("1920x1080 Stage", root.transform, new Rect(0, 0, 1920, 1080));
            stage.anchorMin = stage.anchorMax = stage.pivot = new Vector2(.5f, .5f); stage.anchoredPosition = Vector2.zero;
            var input = new SerializedObject(root.GetComponent<LandingChallengeView>());
            var data = new SerializedObject(root.GetComponent<TackleLandingPresentation>());
            Bind(input, "artwork", root.GetComponent<TackleLandingPresentation>());
            Art(stage, "desk-background", new Rect(0, 0, 1920, 1080)); Art(stage, "rig-cabinet", new Rect(24, 44, 380, 995));
            Art(stage, "title-plaque", new Rect(570, 28, 780, 116), 1.11111111f); Art(stage, "haul-plaque", new Rect(1430, 29, 345, 116));
            Art(stage, "landing-machine", TackleMachineGeometry.StageBounds); Art(stage, "control-console", new Rect(492, 897, 1310, 165), 1.53846154f);
            Art(stage, "message-strip", new Rect(525, 836, 1140, 54), 1.66666667f);
            Bind(data, "phase", Text(stage, "BiomePhase", "", new Rect(590, 65, 740, 47), 28, Cream, 18));
            Bind(data, "haul", Text(stage, "Haul", "", new Rect(1450, 70, 300, 40), 26, new Color32(215, 172, 85, 255), 16));
            Text(stage, "RigTitle", "YOUR RIG", new Rect(70, 105, 290, 42), 23, Cream);
            Bind(data, "creature", Text(stage, "Creature", "", new Rect(700, 229, 950, 50), 27, Cream, 16));
            var portrait = Art(stage, "portrait-garfish", new Rect(680, 334, 400, 116), 0, true);
            portrait.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CreatureSpriteSetup.Destination + "creature_garfish.png");
            CreatureSpriteSetup.CenterPortrait(portrait);
            Bind(data, "portrait", portrait);
            Bind(data, "tags", Text(stage, "CreatureTags", "", new Rect(705, 484, 360, 28), 20, Cream, 14, true));
            Text(stage, "PointsCaption", "LANDING POINTS", new Rect(1150, 326, 205, 28), 20, Cream, 14, true);
            Text(stage, "ResistanceCaption", "RESISTANCE", new Rect(1385, 326, 220, 28), 20, Cream, 14, true);
            Bind(data, "points", Text(stage, "Points", "0", new Rect(1148, 365, 210, 76), 56, Cream, 20));
            Text(stage, "Separator", "/", new Rect(1358, 380, 32, 55), 38, Cream);
            Bind(data, "resistance", Text(stage, "Resistance", "40", new Rect(1390, 365, 215, 76), 56, Cream, 20));
            var bar = Rect("Progress Clip", stage, new Rect(1160, 496, 436, 10));
            bar.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            var fill = Art(bar, "score-segment", new Rect(0, 0, 436, 10)).rectTransform;
            fill.anchorMin = Vector2.zero; fill.anchorMax = Vector2.one; fill.offsetMin = fill.offsetMax = Vector2.zero;
            Bind(data, "progressFill", fill);
            var track = Rect("Track Interior", stage, new Rect(652, 614, 965, 84));
            track.gameObject.AddComponent<UnityEngine.UI.RectMask2D>(); Bind(data, "track", track);
            var good = Motion(track, "landing-zone", 84); var perfect = Motion(track, "perfect-band", 84);
            Bind(data, "goodZone", good); Bind(data, "perfectZone", perfect);
            var marker = Art(track, "generic-fish", new Rect(0, 0, 53, 35));
            var fish = marker.rectTransform; fish.name = "Fish Indicator";
            fish.anchorMin = fish.anchorMax = fish.pivot = new Vector2(.5f, .5f); fish.anchoredPosition = Vector2.zero;
            Bind(data, "fish", fish); Bind(data, "fishMarker", marker);
            var lampContent = Rect("Attempt Lamps", stage, new Rect(655, 734, 252, 55)); Bind(data, "lampContent", lampContent);
            var lamp = Art(lampContent, "lamp-off", new Rect(0, 0, 55, 55), 0, true); lamp.gameObject.SetActive(false);
            Bind(data, "lampTemplate", lamp); Bind(data, "lampOn", sprites["lamp-on"]); Bind(data, "lampOff", sprites["lamp-off"]);
            Bind(data, "attempts", Text(stage, "Attempts", "", new Rect(942, 748, 300, 30), 20, Cream, 14));
            Bind(data, "movement", Text(stage, "Behavior", "", new Rect(1318, 748, 270, 30), 22, new Color32(175, 203, 167, 255), 15));
            Bind(data, "message", Text(stage, "Instruction", "", new Rect(555, 844, 1080, 35), 24, Cream, 17, true));
            Art(stage, "readout-glass-wide", new Rect(545, 920, 505, 116), 1.33333333f);
            Bind(data, "grade", Text(stage, "Grade", "", new Rect(570, 933, 455, 30), 22, Cream, 12));
            var score = Scroll(stage, "Score Attribution", new Rect(570, 971, 455, 48), out RectTransform scoreContent);
            var detail = Text(scoreContent, "Breakdown", "", new Rect(0, 0, 445, 48), 23, new Color32(175, 203, 167, 255), 0, true);
            detail.alignment = TextAlignmentOptions.TopLeft;
            detail.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>().verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            score.content = detail.rectTransform; Bind(data, "breakdown", detail); Bind(data, "scoreScroll", score);
            var strike = Button(stage, "Strike", "STRIKE", new Rect(1120, 920, 600, 116), 30);
            var key = Art(strike.transform.Find("Visual"), "keycap", new Rect(440, 30, 128, 48), 4);
            Text(key.transform, "Binding", "SPACE", new Rect(5, 9, 118, 28), 18, Cream);
            var strikeLabel = strike.GetComponentInChildren<TMP_Text>(); strikeLabel.rectTransform.sizeDelta = new Vector2(390, 100);
            Bind(input, "strike", strike);
            var back = Button(stage, "Return", "RETURN TO HARBOR", new Rect(1120, 920, 600, 116), 23); back.gameObject.SetActive(false); Bind(input, "returnToHarbor", back);
            var feedback = data.FindProperty("buttons"); feedback.arraySize = 2;
            feedback.GetArrayElementAtIndex(0).objectReferenceValue = strike.GetComponent<TackleButtonFeedback>(); feedback.GetArrayElementAtIndex(1).objectReferenceValue = back.GetComponent<TackleButtonFeedback>();
            var rigScroll = Scroll(stage, "Rig Scroll", new Rect(62, 189, 315, 825), out RectTransform rigContent);
            Bind(data, "rigScroll", rigScroll); Bind(data, "rigContent", rigContent);
            Bind(data, "rigCardPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(TackleDiscoverArtSetup.RigPrefabPath).GetComponent<TackleRigCardView>());
            Bind(data, "emptyRig", Text(stage, "EmptyRig", "", new Rect(80, 225, 280, 150), 21, Cream, 15, true));
            input.ApplyModifiedPropertiesWithoutUndo(); data.ApplyModifiedPropertiesWithoutUndo();
            TackleCardTooltipSetup.ConfigurePhase(root);
            PrefabUtility.SaveAsPrefabAsset(root, LandingChallengeSetup.PrefabPath);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    /// <summary>Creates centered independent zone layers so normalized model widths match the visible regions exactly.</summary>
    private static RectTransform Motion(Transform track, string sprite, float height)
    {
        var image = Art(track, sprite, new Rect(0, 0, 120, height)); var rect = image.rectTransform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f); rect.anchoredPosition = Vector2.zero; return rect;
    }
    /// <summary>Imports the original generic pixel marker separately from the preserved Penpot exports.</summary>
    private static void ImportIndicator()
    {
        const string original = "Docs/UIThemes/TackleApparatusConcept/LandingIndicator/generic-fish.png";
        Folder(Root + "Art/Landing/Indicators");
        if (!File.Exists(IndicatorPath) || !File.ReadAllBytes(IndicatorPath).SequenceEqual(File.ReadAllBytes(original))) File.Copy(original, IndicatorPath, true);
        AssetDatabase.ImportAsset(IndicatorPath, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(IndicatorPath);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100; importer.spritePivot = new Vector2(.5f, .5f); importer.spriteBorder = Vector4.zero;
        importer.filterMode = FilterMode.Point; importer.wrapMode = TextureWrapMode.Clamp;
        importer.mipmapEnabled = false; importer.isReadable = false; importer.alphaIsTransparency = true;
        importer.npotScale = TextureImporterNPOTScale.None; importer.textureCompression = TextureImporterCompression.Uncompressed;
        var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings); settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings); importer.SaveAndReimport();
        sprites.Add("generic-fish", AssetDatabase.LoadAssetAtPath<Sprite>(IndicatorPath));
    }

    /// <summary>Imports the complete normalized Landing housing without cropping, slicing or altering the shared exterior.</summary>
    private static void ImportHousing()
    {
        var contract=JsonUtility.FromJson<HousingContract>(File.ReadAllText("Docs/UIThemes/TackleApparatusConcept/LandingIntegrated/Normalization.json"));
        if(HashFile(RigBuildingSetup.ArtPath+"build-machine.png")!=contract.baseSha256||HashFile(HousingSource)!=contract.outputSha256)
            throw new InvalidOperationException("The Landing housing must be normalized against the current Build shell before import.");
        Folder(Root+"Art/Landing/Integrated");
        if(!File.Exists(HousingPath)||!File.ReadAllBytes(HousingPath).SequenceEqual(File.ReadAllBytes(HousingSource)))File.Copy(HousingSource,HousingPath,true);
        AssetDatabase.ImportAsset(HousingPath,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(HousingPath);
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=100;
        importer.filterMode=FilterMode.Point;importer.wrapMode=TextureWrapMode.Clamp;importer.mipmapEnabled=false;importer.isReadable=false;
        importer.sRGBTexture=true;importer.alphaSource=TextureImporterAlphaSource.FromInput;importer.alphaIsTransparency=true;
        importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.spriteBorder=Vector4.zero;
        var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);importer.SaveAndReimport();
        sprites.Add("landing-machine",AssetDatabase.LoadAssetAtPath<Sprite>(HousingPath));
    }

    /// <summary>Checks the immutable normalization inputs so authoring cannot silently import a drifting phase silhouette.</summary>
    private static string HashFile(string path)
    {
        using(var hash=System.Security.Cryptography.SHA256.Create())
            return BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(path))).Replace("-","").ToLowerInvariant();
    }
    /// <summary>Creates clipped scroll content without a visible placeholder background or an artificial item cap.</summary>
    private static UnityEngine.UI.ScrollRect Scroll(Transform parent, string name, Rect bounds, out RectTransform content)
    {
        var root = Rect(name, parent, bounds); var scroll = root.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();
        var viewport = Rect("Viewport", root, new Rect(0, 0, bounds.width, bounds.height)); viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
        var hit = viewport.gameObject.AddComponent<UnityEngine.UI.Image>(); hit.color = Color.clear;
        content = Rect("Content", viewport, new Rect(0, 0, bounds.width, bounds.height)); scroll.viewport = viewport; scroll.content = content;
        scroll.horizontal = false; scroll.vertical = true; scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped; return scroll;
    }
    /// <summary>Authors the same independent press/focus/disabled feedback used by the approved Discover controls.</summary>
    private static UnityEngine.UI.Button Button(Transform parent, string name, string label, Rect bounds, float size)
    {
        var root = Rect(name, parent, bounds); var hit = root.gameObject.AddComponent<UnityEngine.UI.Image>(); hit.color = Color.clear;
        var button = root.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = hit; button.transition = UnityEngine.UI.Selectable.Transition.None;
        var visual = Rect("Visual", root, new Rect(0, 0, bounds.width, bounds.height)); visual.gameObject.AddComponent<CanvasGroup>();
        Art(visual, "button-primary", new Rect(0, 0, bounds.width, bounds.height), 1.42857143f);
        Text(visual, "Label", label, new Rect(15, 3, bounds.width - 30, bounds.height - 6), size, Ink, 16);
        var focus = Art(root, "focus-corners", new Rect(0, 0, bounds.width, bounds.height), 1).gameObject; focus.SetActive(false);
        var data = new SerializedObject(root.gameObject.AddComponent<TackleButtonFeedback>()); Bind(data, "visual", visual); Bind(data, "focusCorners", focus); data.ApplyModifiedPropertiesWithoutUndo(); return button;
    }
    /// <summary>Uses supplied sprite borders and independent live text, preserving original exported pixels.</summary>
    private static UnityEngine.UI.Image Art(Transform parent, string name, Rect bounds, float ppu = 0, bool preserve = false)
    {
        var image = Rect(name, parent, bounds).gameObject.AddComponent<UnityEngine.UI.Image>(); image.sprite = sprites[name]; image.raycastTarget = false; image.preserveAspect = preserve;
        image.type = ppu > 0 ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
        if (ppu > 0) image.pixelsPerUnitMultiplier = ppu; return image;
    }
    /// <summary>Converts approved top-left design coordinates into anchored Unity UI geometry.</summary>
    private static RectTransform Rect(string name, Transform parent, Rect bounds)
    {
        var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform; rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1); rect.anchoredPosition = new Vector2(bounds.x, -bounds.y); rect.sizeDelta = bounds.size; return rect;
    }
    /// <summary>Authors readable licensed pixel-font fields without baking functional text into sprites.</summary>
    private static TMP_Text Text(Transform parent, string name, string value, Rect bounds, float size, Color color, float min = 0, bool body = false)
    {
        var text = Rect(name, parent, bounds).gameObject.AddComponent<TextMeshProUGUI>(); text.font = body ? bodyFont : displayFont;
        text.text = value; text.fontSize = size; text.color = color; text.alignment = TextAlignmentOptions.Midline;
        text.textWrappingMode = TextWrappingModes.Normal; text.raycastTarget = false; text.richText = false;
        if (min > 0) { text.enableAutoSizing = true; text.fontSizeMin = min; text.fontSizeMax = size; } return text;
    }
    /// <summary>Creates folders using Unity's metadata-aware authoring path.</summary>
    private static void Folder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return; Folder(Path.GetDirectoryName(path).Replace('\\', '/')); AssetDatabase.CreateFolder(Path.GetDirectoryName(path).Replace('\\', '/'), Path.GetFileName(path));
    }
    /// <summary>Persists component references without embedding scene objects in the reusable prefab.</summary>
    private static void Bind(SerializedObject data, string field, UnityEngine.Object value) { data.FindProperty(field).objectReferenceValue = value; }
}
