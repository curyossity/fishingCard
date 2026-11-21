using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Designs the expedition's independent reel-preparation bench with reusable Tackle art and native controls.</summary>
public static class TackleTagPreparationSetup
{
    private const string ArtRoot = TackleDiscoverArtSetup.Root + "Art/Common/";
    private static readonly Color Cream = new Color32(233, 220, 177, 255);
    private static readonly string[] Philosophies = {
        "Cooperation and\nshared strength.", "Exploit openings.\nDeliver big payoffs.", "Build strength\nthrough placement.",
        "Reliable strength\nand recovery.", "Master timing.\nReward precision." };

    /// <summary>Installs only tag preparation, leaving the rest of the Harbor and expedition presentation intact.</summary>
    [MenuItem("Fishing Cards/UI/Tackle Apparatus/Install Tag Preparation")]
    public static void InstallCurrentScene()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.isDirty)
            throw new InvalidOperationException("Open a saved game scene outside Play Mode.");
        BuildPrefab(); Install(scene); EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save tag preparation bindings.");
        AssetDatabase.SaveAssets();
    }

    /// <summary>Binds the GUID-stable preparation prefab as a standalone canvas, independent of the old Harbor's scale.</summary>
    public static void Install(Scene scene)
    {
        var harbor = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<JournalHarborFlow>(true)).Single();
        var preparation = harbor.SlotPreparation ?? scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<SlotTagPreparationView>(true)).SingleOrDefault();
        if (preparation == null)
            preparation = ((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(SlotMachineSetup.PreparationPath), scene)).GetComponent<SlotTagPreparationView>();
        if (preparation.transform.parent != null) preparation.transform.SetParent(null, false);
        preparation.transform.localScale = Vector3.one;
        var harborData = new SerializedObject(harbor); Bind(harborData, "slotPreparation", preparation);
        var hub = (GameObject)harborData.FindProperty("hubContent").objectReferenceValue;
        var blocker = hub.GetComponent<CanvasGroup>() ?? hub.AddComponent<CanvasGroup>();
        var data = new SerializedObject(preparation); Bind(data, "blockedHarborContent", blocker); data.ApplyModifiedPropertiesWithoutUndo();
        harborData.ApplyModifiedPropertiesWithoutUndo();
        PrefabUtility.RecordPrefabInstancePropertyModifications(harbor); PrefabUtility.RecordPrefabInstancePropertyModifications(preparation);
        preparation.gameObject.SetActive(false);
    }

    /// <summary>Creates the reviewed 1920 by 1080 design before any Harbor scene integration.</summary>
    public static void BuildPrefab()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Author preparation outside Play Mode.");
        var root = new GameObject("Tackle Tag Preparation", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler),
            typeof(UnityEngine.UI.GraphicRaycaster), typeof(SlotTagPreparationView), typeof(TackleTagPreparationPresentation));
        try
        {
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 500;
            var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
            root.AddComponent<UnityEngine.UI.Image>().color = new Color32(14, 17, 15, 255);
            var stage = Rect(root.transform, "1920x1080 Stage", new Rect(0, 0, 1920, 1080));
            stage.anchorMin = stage.anchorMax = stage.pivot = new Vector2(.5f, .5f); stage.anchoredPosition = Vector2.zero;
            Art(stage, "Desk", "desk-background", new Rect(0, 0, 1920, 1080));
            Art(stage, "Preparation title plate", "title-plaque", new Rect(430, 38, 1060, 147));
            Text(stage, "Heading", "PREPARE THE REELS", new Rect(450, 88, 1020, 47), 30, true);
            Text(stage, "Choose four instruction", "Choose 4 tags for this expedition", new Rect(400, 205, 1120, 48), 38);
            Art(stage, "Selection rail", "rope-horizontal", new Rect(80, 296, 1760, 20));
            var view = new SerializedObject(root.GetComponent<SlotTagPreparationView>());
            var art = new SerializedObject(root.GetComponent<TackleTagPreparationPresentation>());
            Bind(view, "artwork", root.GetComponent<TackleTagPreparationPresentation>());
            var buttons = view.FindProperty("tagButtons"); var labels = view.FindProperty("tagLabels"); var plates = art.FindProperty("plates");
            buttons.arraySize = labels.arraySize = plates.arraySize = CreatureTagVocabulary.All.Length;
            for (int i = 0; i < CreatureTagVocabulary.All.Length; i++)
            {
                string tag = CreatureTagVocabulary.All[i]; float x = 100 + i * 350;
                var button = Button(stage, tag, new Rect(x, 332, 320, 251), "rig-card-ivory", out var visual, out var feedback);
                var name = Text(visual, "Tag name", tag.ToUpperInvariant(), new Rect(40, 20, 240, 37), 18, true);
                var symbol = Rect(visual, "Tag illustration", new Rect(42, 65, 236, 123)).gameObject.AddComponent<UnityEngine.UI.Image>();
                symbol.sprite = TagSprite(tag); symbol.preserveAspect = true; symbol.raycastTarget = false;
                var lamp = Art(visual, "Selection lamp", "lamp-on", new Rect(31, 204, 35, 35));
                var state = Text(visual, "Selection state", "IN YOUR REELS", new Rect(70, 206, 221, 31), 27);
                Art(stage, tag + " clip", "card-clip", new Rect(x + 120, 298, 80, 46));
                Art(stage, tag + " philosophy plate", "status-plaque", new Rect(x, 607, 320, 92));
                var philosophy = Text(stage, tag + " philosophy", Philosophies[i], new Rect(x + 18, 621, 284, 66), 27);
                philosophy.textWrappingMode = TextWrappingModes.NoWrap; philosophy.lineSpacing = 3;
                buttons.GetArrayElementAtIndex(i).objectReferenceValue = button; labels.GetArrayElementAtIndex(i).objectReferenceValue = name;
                var plate = plates.GetArrayElementAtIndex(i); plate.FindPropertyRelative("tag").stringValue = tag;
                Bind(plate, "face", visual.GetComponent<UnityEngine.UI.Image>()); Bind(plate, "symbol", symbol);
                Bind(plate, "lamp", lamp); Bind(plate, "state", state); Bind(plate, "feedback", feedback);
            }
            var instruction = Text(stage, "Draft instruction", "Remove a tag to choose a different one.", new Rect(400, 735, 1120, 44), 32);
            Bind(art, "instruction", instruction);
            var probability = Text(stage, "Reel probability", "Each chosen tag: 25% per reel · Fixed for this expedition", new Rect(240, 790, 1440, 40), 31);
            var pool = Text(stage, "Creature pool explanation", "Creatures with other tags can still appear.", new Rect(300, 836, 1320, 34), 27);
            var guidance = art.FindProperty("guidance"); guidance.arraySize = 3;
            guidance.GetArrayElementAtIndex(0).objectReferenceValue = instruction.gameObject;
            guidance.GetArrayElementAtIndex(1).objectReferenceValue = probability.gameObject;
            guidance.GetArrayElementAtIndex(2).objectReferenceValue = pool.gameObject;
            var failure = Text(stage, "Start failure", "", new Rect(300, 726, 1320, 145), 30);
            failure.enableAutoSizing = true; failure.fontSizeMin = 22; failure.fontSizeMax = 30;
            failure.gameObject.SetActive(false); Bind(art, "failure", failure);
            Art(stage, "Preparation console", "control-console", new Rect(155, 885, 1610, 171));
            var back = Button(stage, "Back", new Rect(209, 925, 330, 94), "button-secondary", out var backVisual, out _);
            Text(backVisual, "Back caption", "BACK", new Rect(16, 24, 298, 42), 25, true); Bind(view, "cancel", back);
            Art(stage, "Selection readout", "readout-glass-wide", new Rect(620, 915, 580, 113));
            var status = Text(stage, "Selected count", "4 / 4 SELECTED", new Rect(645, 928, 530, 37), 27, true); Bind(view, "status", status);
            var lamps = art.FindProperty("readyLamps"); lamps.arraySize = 4;
            for (int i = 0; i < 4; i++) lamps.GetArrayElementAtIndex(i).objectReferenceValue = Art(stage, "Ready lamp " + i, "lamp-on", new Rect(827 + i * 43, 974, 32, 32));
            var confirm = Button(stage, "Set Sail", new Rect(1270, 925, 447, 94), "button-primary", out var confirmVisual, out var confirmFeedback);
            Text(confirmVisual, "Set Sail caption", "SET SAIL", new Rect(16, 24, 415, 42), 27, true); Bind(view, "confirm", confirm); Bind(art, "confirmFeedback", confirmFeedback);
            Bind(art, "selectedFace", Sprite("rig-card-ivory")); Bind(art, "unselectedFace", Sprite("rig-card-sage"));
            Bind(art, "lampOn", Sprite("lamp-on")); Bind(art, "lampOff", Sprite("lamp-off"));
            view.ApplyModifiedPropertiesWithoutUndo(); art.ApplyModifiedPropertiesWithoutUndo();
            DeepwaterAudioSetup.ConfigurePreparation(root);
            PrefabUtility.SaveAsPrefabAsset(root, SlotMachineSetup.PreparationPath);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
        AssetDatabase.SaveAssets();
    }

    /// <summary>Uses the same identity illustrations as the actual Discover reels.</summary>
    private static Sprite TagSprite(string tag)
    {
        string path = tag == "Schooling" ? ArtRoot + "tag-schooling.png" : TackleReelSymbolSetup.Destination + "tag-" + tag.ToLowerInvariant() + ".png";
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        return sprite != null ? sprite : throw new InvalidOperationException("Missing tag illustration: " + path);
    }
    /// <summary>Loads preserved theme images without changing their import settings or pixels.</summary>
    private static Sprite Sprite(string name)
    { var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArtRoot + name + ".png"); return sprite != null ? sprite : throw new InvalidOperationException("Missing Tackle art: " + name); }
    /// <summary>Creates top-left reference geometry inside the centered stage.</summary>
    private static RectTransform Rect(Transform parent, string name, Rect bounds)
    {
        var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform; rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1); rect.anchoredPosition = new Vector2(bounds.x, -bounds.y); rect.sizeDelta = bounds.size; return rect;
    }
    /// <summary>Places an independent supplied-art layer that never intercepts input.</summary>
    private static UnityEngine.UI.Image Art(Transform parent, string name, string sprite, Rect bounds)
    { var image = Rect(parent, name, bounds).gameObject.AddComponent<UnityEngine.UI.Image>(); image.sprite = Sprite(sprite); image.raycastTarget = false; return image; }
    /// <summary>Creates readable native theme text instead of baking game vocabulary into images.</summary>
    private static TMP_Text Text(Transform parent, string name, string content, Rect bounds, float size, bool display = false)
    {
        var text = Rect(parent, name, bounds).gameObject.AddComponent<TextMeshProUGUI>();
        text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TackleDiscoverArtSetup.Root + "Fonts/" + (display ? "PressStart2P" : "VT323") + ".asset");
        text.text = content; text.fontSize = size; text.color = Cream; text.raycastTarget = false; text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal; return text;
    }
    /// <summary>Uses the expedition's native press motion and independent pointer/gamepad focus corners.</summary>
    private static UnityEngine.UI.Button Button(Transform parent, string name, Rect bounds, string sprite, out RectTransform visual, out TackleButtonFeedback feedback)
    {
        var rect = Rect(parent, name, bounds); var hit = rect.gameObject.AddComponent<UnityEngine.UI.Image>(); hit.color = Color.clear;
        var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = hit; button.transition = UnityEngine.UI.Selectable.Transition.None;
        visual = Art(rect, "Visual", sprite, new Rect(0, 0, bounds.width, bounds.height)).rectTransform; visual.gameObject.AddComponent<CanvasGroup>();
        var focus = Art(rect, "Focus", "focus-corners", new Rect(-6, -6, bounds.width + 12, bounds.height + 12)); focus.gameObject.SetActive(false);
        feedback = rect.gameObject.AddComponent<TackleButtonFeedback>(); var data = new SerializedObject(feedback); Bind(data, "visual", visual); Bind(data, "focusCorners", focus.gameObject); data.ApplyModifiedPropertiesWithoutUndo();
        return button;
    }
    /// <summary>Stores explicit component references on a serialized component.</summary>
    private static void Bind(SerializedObject data, string property, UnityEngine.Object value) => data.FindProperty(property).objectReferenceValue = value;
    /// <summary>Stores one tag plate's native fields without runtime hierarchy searches.</summary>
    private static void Bind(SerializedProperty data, string property, UnityEngine.Object value) => data.FindPropertyRelative(property).objectReferenceValue = value;
}
