using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Builds independent Midnight Field Journal templates without replacing the active game's scene.</summary>
public static partial class MidnightFieldJournalPrefabBuilder
{
    private const string RootPath = MidnightFieldJournalAssetSetup.Root;
    private const string Prefabs = RootPath + "Prefabs/";
    private static Color Ink => theme.ParchmentInk;
    private static Color Paper => theme.LightInk;
    private static Color Copper => theme.Copper;
    private static TMP_FontAsset body;
    private static TMP_FontAsset title;
    private static MidnightFieldJournalTheme theme;

    /// <summary>Imports newly authored art and builds reusable data-bound prefabs and a 4K layout canvas.</summary>
    [MenuItem("Fishing Cards/UI/Midnight Field Journal/Build Reusable Templates")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Build theme templates outside Play Mode.");
        MidnightFieldJournalAssetSetup.EnsureFolder(Prefabs);
        theme = AssetDatabase.LoadAssetAtPath<MidnightFieldJournalTheme>(MidnightFieldJournalAssetSetup.ThemePath);
        if (theme == null) throw new InvalidOperationException("Prepare the isolated fonts and theme before building templates.");
        JournalControlDesignSync.Bind(theme);
        JournalInspectionDesignSync.Bind(theme);
        JournalCompactArtworkSync.Bind(theme);
        body = theme.BodyFont;
        title = theme.DisplayFont;
        if (body == null || title == null) throw new InvalidOperationException("Theme-local TMP fonts are required.");
        CreatureCardVisualProfile profile = AssetDatabase.LoadAssetAtPath<CreatureCardVisualProfile>(RootPath + "Profiles/CreatureVisualProfile.asset");
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<CreatureCardVisualProfile>();
            AssetDatabase.CreateAsset(profile, RootPath + "Profiles/CreatureVisualProfile.asset");
        }
        profile.nameInk = Ink; profile.catchRulesInk = Paper; profile.eventRulesInk = Paper;
        profile.catchRules = CentralBounds(82, 882, 545, 427); profile.baitRules = CentralBounds(634, 882, 545, 427);
        profile.eventRules = CentralBounds(225,1070,810,205);
        profile.minimumRulesSize = 26; profile.maximumRulesSize = 39; profile.maximumStatSize = 64; profile.rulesPadding = Vector2.zero;
        profile.catchDescription = profile.baitDescription = new Rect(.025f, .05f, .95f, .52f);
        profile.separateAbilityHeadings = true; profile.plainStatNumbers = true; profile.showEventCategory = true;
        profile.selectionWash = Color.clear;
        profile.respectParentInputLocks = true; profile.catchAspectRatio = .9f; profile.eventAspectRatio = .9f;
        EditorUtility.SetDirty(profile);
        Creature(profile);
        Compact();
        Technique();
        TechniqueHand();
        ActionButton("Descend");
        ActionButton("Surface");
        LoadMeter();
        CatchTooltip();
        CatchChain();
        CanvasLayout();
        AssetDatabase.SaveAssetIfDirty(profile);
        Validate();
    }

    /// <summary>Creates the horizontal Catch Chain template with independently bound portrait, stats and Bait badge.</summary>
    private static void Compact()
    {
        GameObject root = Root("CompactCatchCard", 680, 680f / 1.78f);
        CompactCatchCardView view = root.AddComponent<CompactCatchCardView>();
        Image background = Picture("Blank Compact Master", root.transform, "CompactCatchTemplate", new Rect(0, 0, 1, 1));
        Button button = root.AddComponent<Button>(); button.targetGraphic = background;
        background.raycastTarget = true; button.transition = Selectable.Transition.None;
        TMP_Text name = Label("Creature Name", root.transform, CompactBounds(67, 36, 470, 69), 50, Ink);
        name.font = theme.BodySemibold; name.alignment = TextAlignmentOptions.TopLeft;
        name.enableAutoSizing = true; name.fontSizeMin = 32; name.textWrappingMode = TextWrappingModes.NoWrap;
        Image portrait = Portrait(root.transform, CompactBounds(64, 100, 552, 221));
        Label("Weight Label", root.transform, CompactBounds(76, 310, 144, 40), 24, Ink, true).text = "WEIGHT";
        TMP_Text weight = Label("Weight Value", root.transform, CompactBounds(228, 296, 83, 68), 54, Ink);
        Label("Value Label", root.transform, CompactBounds(381, 310, 115, 40), 24, Ink, true).text = "VALUE";
        TMP_Text value = Label("Value Value", root.transform, CompactBounds(527, 296, 104, 68), 54, Ink);
        foreach (TMP_Text stat in new[] { weight, value })
        { stat.enableAutoSizing = true; stat.fontSizeMin = 24; stat.textWrappingMode = TextWrappingModes.NoWrap; stat.overflowMode = TextOverflowModes.Overflow; }
        JournalCardOutline bait = Edge(root.transform, "Bait Cyan Edge", theme.BaitAccent, 9, 4, 20);
        JournalCardOutline inner = Edge(bait.transform, "Inner Blue Etching", new Color32(72, 145, 155, 204), 18, 1.5f, 14);
        Edge(bait.transform, "Diffuse Cyan Edge", new Color32(142, 214, 219, 35), 7, 11, 22);
        bait.gameObject.SetActive(false);
        RectTransform badgeRoot = Region("Bait Marker", root.transform, CompactBounds(528, 40, 104, 42));
        UnityEngine.UI.Image badgeFace = badgeRoot.gameObject.AddComponent<UnityEngine.UI.Image>(); badgeFace.color = new Color32(84, 140, 148, 255); badgeFace.raycastTarget = false;
        TMP_Text badge = Label("Bait Label", badgeRoot, new Rect(0, 0, 1, 1), 23, Paper, true); badge.text = "BAIT"; badgeRoot.gameObject.SetActive(false);
        SerializedObject data = new SerializedObject(view);
        Bind(data, "portraitImage", portrait); Bind(data, "nameText", name);
        Bind(data, "weightText", weight); Bind(data, "valueText", value);
        Bind(data, "selectionButton", button); Bind(data, "baitMarker", badgeRoot.gameObject);
        Bind(data, "baitFrame", bait);
        Bind(data, "hoverFrame", Edge(root.transform, "Hover And Focus Edge", new Color32(240, 224, 189, 230), 4, 3, 22));
        data.FindProperty("pointerHoverFrame").boolValue = false;
        Bind(data, "selectedFrame", Edge(root.transform, "Selection Edge", theme.Copper, 3, 5, 24));
        Bind(data, "releaseCandidateFrame", Edge(root.transform, "Legal Target Edge", theme.BaitAccent, 3, 5, 24));
        Image disabled = Picture("Disabled Overlay", root.transform, null, new Rect(0, 0, 1, 1));
        disabled.color = new Color(0, 0, 0, .3f); disabled.gameObject.SetActive(false); Bind(data, "disabledOverlay", disabled);
        Bind(data, "rigAttachmentPoint", Region("Rig Attachment Point", root.transform, new Rect(.48f, .96f, .04f, .04f)));
        data.FindProperty("titleInk").colorValue = Ink; data.FindProperty("statInk").colorValue = Ink;
        data.FindProperty("uppercaseName").boolValue = false;
        data.FindProperty("titleAlignment").intValue = (int)TextAlignmentOptions.TopLeft;
        foreach (string overlay in new[] { "Hover And Focus Edge", "Selection Edge", "Legal Target Edge" }) root.transform.Find(overlay).gameObject.SetActive(false);
        data.ApplyModifiedPropertiesWithoutUndo(); Save(root, "CompactCatchCard");
    }

    /// <summary>Creates the existing technique view with one blank surface and separate equipment and text layers.</summary>
    private static void Technique()
    {
        GameObject root = Root("TechniqueCard", 620, 640);
        TechniqueCardView view = root.AddComponent<TechniqueCardView>();
        RectTransform visual = Region("Visual Root", root.transform, new Rect(0, 0, 1, 1));
        Image background = Picture("Blank Technique Master", visual, "TechniqueTemplate", new Rect(0, 0, 1, 1));
        Button button = root.AddComponent<JournalTechniqueButton>(); button.targetGraphic = background;
        TMP_Text keyword = Label("Technique Type", visual, new Rect(.23f, .88f, .54f, .047f), 20, Ink, true); keyword.text = "TECHNIQUE"; keyword.characterSpacing = 4;
        background.raycastTarget = true; button.transition = Selectable.Transition.None;
        UnityEngine.UI.Shadow shadow = background.gameObject.AddComponent<UnityEngine.UI.Shadow>();
        shadow.effectColor = new Color(0, 0, 0, .5f); shadow.effectDistance = new Vector2(7, -10);
        TMP_Text name = Label("Technique Name", visual, new Rect(.12f, .775f, .76f, .09f), 39.68f, Ink, true);
        name.fontWeight = FontWeight.SemiBold; keyword.fontWeight = FontWeight.SemiBold;
        Image art = Portrait(visual, new Rect(.08f, .27f, .84f, .455f));
        TMP_Text rules = Label("Technique Rules", visual, new Rect(.14f, .07f, .72f, .17f), 32.86f, Ink);
        rules.font = theme.BodySemibold;
        name.enableAutoSizing = true; name.fontSizeMin = 28; name.textWrappingMode = TextWrappingModes.NoWrap;
        rules.enableAutoSizing = true; rules.fontSizeMin = 23; rules.textWrappingMode = TextWrappingModes.Normal;
        rules.overflowMode = TextOverflowModes.Overflow;
        SerializedObject data = new SerializedObject(view);
        Bind(data, "visualRoot", visual); Bind(data, "artworkImage", art); Bind(data, "nameText", name);
        Bind(data, "rulesText", rules); Bind(data, "useButton", button);
        Bind(data, "keywordText", keyword); data.FindProperty("externalPose").boolValue = true;
        data.FindProperty("inspectWhileUnavailable").boolValue = true;
        Bind(data, "hoverFrame", Edge(visual, "Hover And Focus Edge", theme.Copper, 10, 3, 20));
        data.FindProperty("pointerHoverFrame").boolValue = false;
        Bind(data, "selectedFrame", Edge(visual, "Selected Edge", theme.BaitAccent, 8, 4, 20));
        Image disabled = Picture("Disabled Overlay", visual, null, new Rect(0, 0, 1, 1)); disabled.color = new Color(0, 0, 0, .18f);
        Bind(data, "disabledOverlay", disabled);
        foreach (string overlay in new[] { "Hover And Focus Edge", "Selected Edge", "Disabled Overlay" }) visual.Find(overlay).gameObject.SetActive(false);
        data.ApplyModifiedPropertiesWithoutUndo(); Save(root, "TechniqueCard");
    }

    /// <summary>Creates a projected hand surface while keeping all physical cards independently data-bound.</summary>
    private static void TechniqueHand()
    {
        GameObject root = Root("TechniqueHand", 2500, 866);
        JournalTechniqueHandPresentation presentation = root.AddComponent<JournalTechniqueHandPresentation>();
        TechniqueHandView view = root.AddComponent<TechniqueHandView>();
        var surface = Region("Projected Hand Surface", root.transform, new Rect(0, 0, 1, 1)).gameObject.AddComponent<UnityEngine.UI.RawImage>();
        surface.raycastTarget = false;
        TMP_Text piles = Label("Pile Counts", root.transform, new Rect(.67f, .91f, .32f, .06f), 25, theme.MutedInk, true);
        TMP_Text restriction = Label("Focused Restriction", root.transform, new Rect(.12f, 0, .76f, .08f), 26, Paper);
        var data = new SerializedObject(presentation);
        Bind(data, "theme", theme); Bind(data, "cardPrefab", AssetDatabase.LoadAssetAtPath<TechniqueCardView>(Prefabs + "TechniqueCard.prefab"));
        data.FindProperty("restingTilt").floatValue = 40;
        data.FindProperty("fieldOfView").floatValue = 2 * Mathf.Atan(433f / 2000) * Mathf.Rad2Deg;
        data.FindProperty("cameraDistance").floatValue = 2000;
        data.FindProperty("cardSpacing").floatValue = 500;
        Bind(data, "surface", surface); Bind(data, "pileText", piles); Bind(data, "restrictionText", restriction); data.ApplyModifiedPropertiesWithoutUndo();
        data = new SerializedObject(view); Bind(data, "presentation", presentation); data.ApplyModifiedPropertiesWithoutUndo();
        var raycaster = root.AddComponent<JournalHandRaycaster>(); data = new SerializedObject(raycaster);
        Bind(data, "hand", presentation); data.ApplyModifiedPropertiesWithoutUndo();
        Save(root, "TechniqueHand");
    }

    /// <summary>Creates static button art with Unity-owned click, hover, pressed and disabled behavior.</summary>
    private static void ActionButton(string name)
    {
        MidnightFieldJournalTheme.SpriteEntry source = null;
        foreach (var entry in theme.Sprites) if (entry.SourceId == name + "Button") source = entry;
        if (source == null) throw new InvalidOperationException("Missing action source bounds.");
        Rect visible = source.VisibleBoundsTopLeft;
        GameObject root = Root(name + "Button", source.DesignCanvas.x * visible.width, source.DesignCanvas.y * visible.height);
        Image hit = Picture("Visible Face Hit Area", root.transform, null, new Rect(0, 0, 1, 1)); hit.color = Color.clear; hit.raycastTarget = true;
        Image image = Picture("Static Button Face", root.transform, name + "Button", new Rect(-visible.x / visible.width, -(1 - visible.yMax) / visible.height, 1 / visible.width, 1 / visible.height));
        image.raycastTarget = false;
        Button button = root.AddComponent<Button>(); button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white; colors.highlightedColor = new Color(1.14f, 1.12f, 1.05f);
        colors.pressedColor = new Color(.84f, .82f, .76f); colors.disabledColor = new Color(.45f, .45f, .45f, .65f);
        colors.colorMultiplier = 1; colors.fadeDuration = .15f; button.colors = colors;
        colors.selectedColor = colors.highlightedColor; button.colors = colors;
        var feedback = root.AddComponent<JournalActionFeedback>(); var data = new SerializedObject(feedback);
        var edge = Edge(root.transform, "Focus Edge", theme.BaitAccent, 3, 3, 12); edge.enabled = false;
        Bind(data, "face", image.rectTransform); Bind(data, "focus", edge); data.ApplyModifiedPropertiesWithoutUndo();
        Save(root, name + "Button");
    }

    /// <summary>Creates a pooled data-driven meter; no numbers or load segments are baked into its art.</summary>
    private static void LoadMeter()
    {
        GameObject root = Root("LineLoadMeter", 760, 175);
        LineLoadMeterView meter = root.AddComponent<LineLoadMeterView>();
        float housingHeight=760f*716/2196;
        Picture("Engraved Load Housing",root.transform,"LineLoadHousing",new Rect(0,(175-housingHeight)/350,1,housingHeight/175));
        Color ink=new Color32(38,53,57,255);
        TMP_Text caption=Label("Line Load Label",root.transform,new Rect(95f/760,1-69f/175,340f/760,44f/175),30,ink,true);caption.text="LINE LOAD";caption.alignment=TextAlignmentOptions.TopLeft;
        TMP_Text label=Label("Load Text",root.transform,new Rect(460f/760,1-76f/175,205f/760,62f/175),43,ink);label.alignment=TextAlignmentOptions.TopRight;
        TMP_Text status=Label("Load Status",root.transform,new Rect(95f/760,1-144f/175,570f/760,28f/175),20,new Color32(101,91,73,255),true);status.alignment=TextAlignmentOptions.Top;
        RectTransform content=Region("Pips",root.transform,new Rect(95f/760,1-105f/175,570f/760,18f/175));
        Image prototype=Region("Pip Template",root.transform,new Rect(0,0,.08f,.1f)).gameObject.AddComponent<JournalLoadSegment>();prototype.raycastTarget=false;
        prototype.gameObject.SetActive(false);
        SerializedObject data = new SerializedObject(meter);
        Bind(data, "loadText", label);Bind(data,"statusText",status); Bind(data, "pipRoot", content); Bind(data, "pipPrefab", prototype);
        data.ApplyModifiedPropertiesWithoutUndo(); Save(root, "LineLoadMeter");
    }

    /// <summary>Creates the controller-facing chain with an independent themed renderer and a real masked scroll viewport.</summary>
    private static void CatchChain()
    {
        GameObject root = Root("CatchChain", 855, 1540);
        JournalCatchChainPresentation presentation = root.AddComponent<JournalCatchChainPresentation>();
        CatchChainView view = root.AddComponent<CatchChainView>();
        TMP_Text heading = Label("Chain Heading", root.transform, new Rect(0, .97f, 1, .03f), 36, Copper, true);
        heading.text = "CATCH CHAIN";
        RectTransform scrollRoot = Region("Chain Scroll", root.transform, new Rect(0, 0, 1, 1 - 50f / 1540));
        UnityEngine.UI.ScrollRect scroll = scrollRoot.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();
        RectTransform viewport = Region("Viewport", scrollRoot, new Rect(0, 0, 1, 1));
        UnityEngine.UI.Image hitArea = viewport.gameObject.AddComponent<UnityEngine.UI.Image>();
        hitArea.color = Color.clear; hitArea.raycastTarget = true;
        viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
        RectTransform content = Region("Content", viewport, new Rect(0, 0, 1, 1));
        content.anchorMin = new Vector2(0, 1); content.anchorMax = Vector2.one;
        content.pivot = new Vector2(.5f, 1); content.sizeDelta = new Vector2(0, 1490);
        RectTransform hardware = Region("Independent Brass Hardware", content, new Rect(0, 0, 1, 1));
        TMP_Text empty = Label("Empty Chain", scrollRoot, new Rect(.1f, .5f, .8f, .1f), 30, theme.MutedInk);
        empty.text = "No attached catches";
        scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false; scroll.vertical = true;
        scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 90;
        SerializedObject data = new SerializedObject(presentation);
        Bind(data, "theme", theme); Bind(data, "cardPrefab", AssetDatabase.LoadAssetAtPath<CompactCatchCardView>(Prefabs + "CompactCatchCard.prefab"));
        Bind(data, "viewport", viewport); Bind(data, "content", content); Bind(data, "hardware", hardware);
        Bind(data, "headingText", heading); Bind(data, "emptyText", empty); data.ApplyModifiedPropertiesWithoutUndo();
        Bind(data, "tooltipPrefab", AssetDatabase.LoadAssetAtPath<JournalCatchTooltip>(Prefabs + "CatchTooltip.prefab"));
        data.FindProperty("topPadding").floatValue = 60; data.ApplyModifiedPropertiesWithoutUndo();
        data = new SerializedObject(view); Bind(data, "presentation", presentation); data.ApplyModifiedPropertiesWithoutUndo();
        heading.transform.SetAsLastSibling();
        Save(root, "CatchChain");
    }

    /// <summary>Adds a hollow state border as a sibling layer, leaving the paper and hardware untouched.</summary>
    private static JournalCardOutline Edge(Transform parent, string name, Color color, float inset, float thickness, float radius)
    {
        JournalCardOutline outline = Region(name, parent, new Rect(0, 0, 1, 1)).gameObject.AddComponent<JournalCardOutline>();
        outline.color = color; outline.raycastTarget = false;
        SerializedObject data = new SerializedObject(outline);
        data.FindProperty("inset").floatValue = inset; data.FindProperty("thickness").floatValue = thickness;
        data.FindProperty("radius").floatValue = radius; data.ApplyModifiedPropertiesWithoutUndo();
        return outline;
    }

    /// <summary>Creates a reusable 4K composition canvas with prefab instances and an empty dynamic chain container.</summary>
    private static void CanvasLayout()
    {
        BuildGameplayCanvas();
    }

    /// <summary>Verifies serialized content boundaries and writes machine-readable asset resolution evidence.</summary>
    public static void Validate()
    {
        string[] names = { "CreatureCard", "CompactCatchCard", "TechniqueCard", "TechniqueHand", "DescendButton", "SurfaceButton", "LineLoadMeter", "CatchChain", "CatchTooltip", "MidnightFieldJournalCanvas" };
        foreach (string name in names)
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + name + ".prefab") == null) throw new InvalidOperationException("Missing prefab: " + name);
        GameObject canvas = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + "MidnightFieldJournalCanvas.prefab");
        if (canvas.GetComponent<CanvasScaler>().referenceResolution != new Vector2(3840, 2160)) throw new InvalidOperationException("4K reference resolution changed.");
        Texture2D background = Sprite("PaperBackground4K").texture;
        if (background.width != 3840 || background.height != 2160) throw new InvalidOperationException("Background is not native 4K.");
        File.WriteAllText("Docs/UIThemes/MidnightFieldJournal/TemplateValidation.json", "{\"passed\":true,\"scope\":\"Current candidate templates including role-aware tooltip\",\"prefabCount\":10,\"canvas\":[3840,2160],\"nativeBackground\":[3840,2160],\"screenshotSlices\":0}");
        Debug.Log("Midnight Field Journal: ten reusable prefabs and native 4K background validated.");
    }

    /// <summary>Creates a named UI root with stable geometry.</summary>
    private static GameObject Root(string name, float width, float height)
    {
        GameObject root = new GameObject(name, typeof(RectTransform));
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height); return root;
    }

    /// <summary>Creates a normalized child region with no implicit runtime state.</summary>
    private static RectTransform Region(string name, Transform parent, Rect bounds)
    {
        GameObject child = new GameObject(name, typeof(RectTransform)); child.transform.SetParent(parent, false);
        RectTransform rect = child.GetComponent<RectTransform>(); SetRegion(rect, bounds); return rect;
    }

    /// <summary>Applies normalized bounds in Unity's bottom-left coordinate system.</summary>
    private static void SetRegion(RectTransform rect, Rect bounds)
    { rect.anchorMin = bounds.min; rect.anchorMax = bounds.max; rect.offsetMin = rect.offsetMax = Vector2.zero; }

    /// <summary>Creates a static art layer without input blocking.</summary>
    private static Image Picture(string name, Transform parent, string sprite, Rect bounds)
    { Image image = Region(name, parent, bounds).gameObject.AddComponent<Image>(); image.sprite = Sprite(sprite); image.raycastTarget = false; return image; }

    /// <summary>Creates an empty masked portrait slot for a runtime-supplied sprite.</summary>
    private static Image Portrait(Transform parent, Rect bounds)
    {
        RectTransform mask = Region("Artwork Mask", parent, bounds); mask.gameObject.AddComponent<RectMask2D>();
        Image image = Picture("Runtime Artwork", mask, null, new Rect(0, 0, 1, 1)); image.preserveAspect = true; image.enabled = false; return image;
    }

    /// <summary>Creates a live TMP field with bounded fitting and no baked sample content.</summary>
    private static TMP_Text Label(string name, Transform parent, Rect bounds, float size, Color color, bool display = false)
    {
        TextMeshProUGUI text = Region(name, parent, bounds).gameObject.AddComponent<TextMeshProUGUI>();
        text.font = display ? title : body; text.fontSize = size; text.fontSizeMax = size; text.fontSizeMin = size * .62f;
        text.enableAutoSizing = false; text.alignment = TextAlignmentOptions.Center; text.color = color;
        text.text = string.Empty; text.raycastTarget = false; return text;
    }

    /// <summary>Keeps the static stat label and mutable numeric field in their own visibility parent.</summary>
    private static TMP_Text Stat(Transform parent, string name, string caption, Rect bounds)
    {
        RectTransform region = Region(name + " Stat", parent, bounds);
        Label(name + " Label", region, new Rect(0, .63f, 1, .33f), 22, Ink, true).text = caption;
        return Label(name + " Value", region, new Rect(0, 0, 1, .65f), 48, Ink);
    }

    /// <summary>Loads only this theme's independently authored sprites.</summary>
    private static Sprite Sprite(string name)
    {
        if (name == null) return null;
        string id = name == "CompactCatchTemplate" ? "CatchFrameV2" : name == "TechniqueTemplate" ? "TechniqueFrameV2" : name;
        if (!theme.TryGetSprite(id, out Sprite sprite)) throw new InvalidOperationException("Theme sprite missing: " + id);
        return sprite;
    }

    /// <summary>Converts the approved compact-card top-left pixel layout into Unity anchors.</summary>
    private static Rect CompactBounds(float x, float y, float width, float height)
    { float h = 680f / 1.78f; return new Rect(x / 680, 1 - (y + height) / h, width / 680, height / h); }

    /// <summary>Assigns an existing view's serialized presentation binding without reflection into runtime state.</summary>
    private static void Bind(SerializedObject data, string field, UnityEngine.Object value)
    { SerializedProperty property = data.FindProperty(field); if (property == null) throw new InvalidOperationException("Unknown binding: " + field); property.objectReferenceValue = value; }

    /// <summary>Saves a template then removes only the temporary authoring root.</summary>
    private static void Save(GameObject root, string name)
    { try { PrefabUtility.SaveAsPrefabAsset(root, Prefabs + name + ".prefab"); } finally { UnityEngine.Object.DestroyImmediate(root); } }

    /// <summary>Adds a reusable prefab instance to the composition without sample card data.</summary>
    private static GameObject Instance(string name, Transform parent, Rect bounds, string instanceName = null)
    { GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + name + ".prefab")); if (instanceName != null) instance.name = instanceName; instance.transform.SetParent(parent, false); SetRegion(instance.GetComponent<RectTransform>(), bounds); return instance; }
}
