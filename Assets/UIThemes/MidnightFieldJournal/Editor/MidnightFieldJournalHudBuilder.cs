using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>Authors the journal gameplay composition while retaining shared run-view ownership.</summary>
public static partial class MidnightFieldJournalPrefabBuilder
{
    /// <summary>Converts 4K top-left design geometry into child anchors.</summary>
    private static Rect ScreenBounds(float x, float y, float w, float h) { return new Rect(x / 3840, 1 - (y + h) / 2160, w / 3840, h / 2160); }
    /// <summary>Creates a fixed-composition viewport and persistent overlay layers on the isolated candidate canvas.</summary>
    private static void BuildGameplayCanvas()
    {
        GameObject root = Root("MidnightFieldJournalCanvas", 3840, 2160);
        Canvas canvas = root.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = root.AddComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = theme.ReferenceResolution; scaler.matchWidthOrHeight = .5f;
        root.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        var overscan = Region("Aspect Matte", root.transform, new Rect(0, 0, 1, 1)).gameObject.AddComponent<UnityEngine.UI.Image>();
        overscan.color = new Color32(17, 30, 37, 255); overscan.raycastTarget = false;
        RectTransform design = Region("Gameplay 16x9 Viewport", root.transform, new Rect(0, 0, 1, 1));
        var fitter = design.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>(); fitter.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent; fitter.aspectRatio = 16f / 9;
        RectTransform background = Region("Background", design, new Rect(0, 0, 1, 1)); background.gameObject.AddComponent<CanvasGroup>();
        Picture("Native 4K Paper Background", background, "PaperBackground4K", new Rect(0, 0, 1, 1));
        Picture("Illustrated Kelp Background", background, "Background", new Rect(0, 0, 1, 1)).color = new Color(1, 1, 1, .45f);
        RectTransform main = Region("Main Content", design, new Rect(0, 0, 1, 1)); main.gameObject.AddComponent<CanvasGroup>();
        RectTransform header = Region("Top Navigation", design, new Rect(0, 0, 1, 1)); header.gameObject.AddComponent<CanvasGroup>();
        TMP_Text biome = ScreenLabel("Biome Name", header, 120, 26, 1000, 80, 43, Paper, true);
        biome.alignment = TextAlignmentOptions.MidlineLeft;
        TMP_Text depth = ScreenLabel("Current Depth", header, 1540, 26, 620, 80, 43, Paper, true);
        TMP_Text deck = ScreenLabel("Deck Count", header, 2910, 26, 400, 80, 36, Paper, true);
        var settingsRoot = Region("Settings Button", header, ScreenBounds(3410, 28, 300, 72));
        var settingsFace = settingsRoot.gameObject.AddComponent<UnityEngine.UI.Image>(); settingsFace.color = new Color(0, 0, 0, .15f);
        var settings = settingsRoot.gameObject.AddComponent<UnityEngine.UI.Button>(); settings.targetGraphic = settingsFace;
        ScreenLabel("Settings Label", settingsRoot, 0, 0, 3840, 2160, 29, Paper, true).text = "SETTINGS";
        Edge(settingsRoot, "Settings Edge", Copper, 1, 2, 8);
        var settingsFocus = Edge(settingsRoot, "Settings Focus", theme.BaitAccent, 1, 3, 8); settingsFocus.enabled = false;
        var settingsFeedback = settingsRoot.gameObject.AddComponent<JournalActionFeedback>(); var focusData = new SerializedObject(settingsFeedback);
        Bind(focusData, "focus", settingsFocus); focusData.ApplyModifiedPropertiesWithoutUndo();
        Rule(header, 120, 112, 3590);
        GameObject chain = Instance("CatchChain", main, ScreenBounds(105, 170, 855, 1540));
        RectTransform encounter = Region("Encounter Panel", main, ScreenBounds(1238, 113, 1064, 1324));
        RectTransform actions = Region("Run Controls", main, new Rect(0, 0, 1, 1));
        ScreenLabel("Actions Heading", actions, 2780, 185, 880, 80, 40, Copper, true).text = "YOUR NEXT MOVE";
        var descend = PlaceAction("Descend", actions, 2780, 255);
        var surface = PlaceAction("Surface", actions, 2780, 645);
        ScreenLabel("Descend Hint", actions, 2780, 575, 880, 60, 31, Paper).text = "Continue deeper";
        ScreenLabel("Surface Hint", actions, 2780, 972, 880, 60, 31, Paper).text = "Bring your Catch home";
        Rule(actions, 2840, 1095, 760);
        ScreenLabel("Depth Guide Heading", actions, 2840, 1115, 760, 70, 34, Copper, true).text = "DEPTH GUIDE";
        RectTransform tiers = Region("Biome Tiers", actions, ScreenBounds(2840, 1195, 760, 210));
        TMP_Text prototype = Label("Tier Template", tiers, new Rect(0, 0, 1, 1), 30, Paper); prototype.enableAutoSizing = true; prototype.fontSizeMin = 20;
        prototype.alignment = TextAlignmentOptions.MidlineLeft; prototype.gameObject.SetActive(false);
        GameObject load = Instance("LineLoadMeter", main, ScreenBounds(145, 1745, 760, 150));
        Rule(main, 165, 1923, 700);
        ScreenLabel("Haul Label", main, 165, 1950, 440, 80, 34, Copper, true).text = "CURRENT HAUL";
        TMP_Text haul = ScreenLabel("Haul Value", main, 670, 1945, 195, 90, 54, Paper);
        TMP_Text trail = ScreenLabel("Encounter Trail Preview", main, 1050, 1430, 1500, 100, 30, Paper);
        trail.enableAutoSizing = true; trail.fontSizeMin = 22; trail.gameObject.SetActive(false);
        GameObject hand = Instance("TechniqueHand", design, ScreenBounds(980, 1294, 2500, 866)); hand.AddComponent<CanvasGroup>();
        // Hand pile counts occupy the spare action-bar row, clear of both the depth guide and inspect text.
        SetRegion(hand.transform.Find("Pile Counts").GetComponent<RectTransform>(), new Rect(.744f, 1.22f, .30f, .058f));
        Canvas tooltip = Overlay(root.transform, "Tooltip Layer", 100);
        Canvas transition = Overlay(root.transform, "Transition Layer", 200);
        Canvas modal = Overlay(root.transform, "Modal Layer", 300);
        var hud = root.AddComponent<JournalGameplayHud>(); var data = new SerializedObject(hud);
        Bind(data, "biomeText", biome); Bind(data, "depthText", depth); Bind(data, "deckText", deck); Bind(data, "haulText", haul); Bind(data, "trailText", trail);
        Bind(data, "descendButton", descend); Bind(data, "surfaceButton", surface); Bind(data, "settingsButton", settings);
        Bind(data, "meter", load.GetComponent<LineLoadMeterView>()); Bind(data, "tierPrototype", prototype); Bind(data, "tierRoot", tiers);
        data.FindProperty("activeColor").colorValue = theme.BaitAccent; data.FindProperty("inactiveColor").colorValue = Paper; data.ApplyModifiedPropertiesWithoutUndo();
        var view = root.AddComponent<FishingRunView>(); data = new SerializedObject(view);
        Bind(data, "hudPresentation", hud); Bind(data, "creatureCardPrefab", AssetDatabase.LoadAssetAtPath<CreatureCardView>(Prefabs + "CreatureCard.prefab"));
        Bind(data, "navigationFont", title); Bind(data, "bodyFont", body);
        Bind(data, "backgroundRegion", background); Bind(data, "topNavigationBar", header); Bind(data, "mainContent", main);
        Bind(data, "catchRigPanel", chain.transform); Bind(data, "encounterPanel", encounter); Bind(data, "runControlsPanel", actions); Bind(data, "techniqueHand", hand.transform);
        Bind(data, "backgroundGroup", background.GetComponent<CanvasGroup>()); Bind(data, "topNavigationGroup", header.GetComponent<CanvasGroup>());
        Bind(data, "mainContentGroup", main.GetComponent<CanvasGroup>()); Bind(data, "techniqueHandGroup", hand.GetComponent<CanvasGroup>());
        Bind(data, "tooltipLayer", tooltip); Bind(data, "transitionLayer", transition); Bind(data, "modalLayer", modal); data.ApplyModifiedPropertiesWithoutUndo();
        BuildModals(root, view, modal, header.GetComponent<CanvasGroup>(), main.GetComponent<CanvasGroup>(), hand.GetComponent<CanvasGroup>(), descend);
        // The HUD alone owns effective load; individual catch weights cannot reconstruct Steady Line relief.
        Save(root, "MidnightFieldJournalCanvas");
    }

    /// <summary>Attaches journal presentations to existing view contracts without introducing runtime game owners.</summary>
    private static void BuildModals(GameObject root, FishingRunView view, Canvas modal, CanvasGroup header, CanvasGroup main, CanvasGroup hand, UnityEngine.UI.Button descend)
    {
        var host = modal.gameObject.AddComponent<JournalModalHost>(); var data = new SerializedObject(host);
        var groups = data.FindProperty("gameplayGroups"); groups.arraySize = 3;
        groups.GetArrayElementAtIndex(0).objectReferenceValue = header; groups.GetArrayElementAtIndex(1).objectReferenceValue = main; groups.GetArrayElementAtIndex(2).objectReferenceValue = hand;
        Bind(data, "resumeFocus", descend); data.ApplyModifiedPropertiesWithoutUndo();
        var choices = Region("Journal Choices", modal.transform, new Rect(0, 0, 1, 1)).gameObject.AddComponent<JournalChoiceModal>();
        data = new SerializedObject(choices); Bind(data, "theme", theme); Bind(data, "host", host); data.ApplyModifiedPropertiesWithoutUndo();
        data = new SerializedObject(view); Bind(data, "choicePresentation", choices); data.ApplyModifiedPropertiesWithoutUndo();
        var resultRoot = Region("Journal Results", modal.transform, new Rect(0, 0, 1, 1));
        var results = resultRoot.gameObject.AddComponent<JournalResultModal>(); data = new SerializedObject(results); Bind(data, "theme", theme); Bind(data, "host", host); data.ApplyModifiedPropertiesWithoutUndo();
        var resultView = resultRoot.gameObject.AddComponent<RunResultView>(); data = new SerializedObject(resultView); Bind(data, "presentation", results); data.ApplyModifiedPropertiesWithoutUndo();
        var settings = Region("Journal Settings", modal.transform, new Rect(0, 0, 1, 1)).gameObject.AddComponent<JournalSettingsModal>();
        data = new SerializedObject(settings); Bind(data, "theme", theme); Bind(data, "host", host); Bind(data, "view", view); data.ApplyModifiedPropertiesWithoutUndo();
    }
    /// <summary>Creates an independently sorted overlay which remains available when gameplay sections hide.</summary>
    private static Canvas Overlay(Transform parent, string name, int order)
    {
        var rect = Region(name, parent, new Rect(0, 0, 1, 1)); Canvas canvas = rect.gameObject.AddComponent<Canvas>(); canvas.overrideSorting = true; canvas.sortingOrder = order;
        rect.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>(); return canvas;
    }
    /// <summary>Preserves source face placement while excluding its transparent margins from the button hit area.</summary>
    private static UnityEngine.UI.Button PlaceAction(string name, Transform parent, float x, float y)
    {
        MidnightFieldJournalTheme.SpriteEntry source = null; foreach (var entry in theme.Sprites) if (entry.SourceId == name + "Button") source = entry;
        Rect v = source.VisibleBoundsTopLeft; Vector2 size = source.DesignCanvas;
        GameObject obj = Instance(name + "Button", parent, ScreenBounds(x + v.x * size.x, y + v.y * size.y, v.width * size.x, v.height * size.y));
        return obj.GetComponent<UnityEngine.UI.Button>();
    }
    /// <summary>Adds a typography surface at measured design coordinates.</summary>
    private static TMP_Text ScreenLabel(string name, Transform parent, float x, float y, float w, float h, float size, Color ink, bool display = false)
    { var text = Label(name, parent, ScreenBounds(x, y, w, h), size, ink, display); text.enableAutoSizing = true; text.fontSizeMin = size * .65f; text.textWrappingMode = TextWrappingModes.NoWrap; return text; }
    /// <summary>Draws a noninteractive journal divider.</summary>
    private static void Rule(Transform parent, float x, float y, float width)
    { var image = Region("Journal Divider", parent, ScreenBounds(x, y, width, 2)).gameObject.AddComponent<UnityEngine.UI.Image>(); image.color = Copper; image.raycastTarget = false; }
}
