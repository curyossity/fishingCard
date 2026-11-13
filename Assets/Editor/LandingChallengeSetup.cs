using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Installs an artwork-free, reusable Landing canvas and editable prototype profile.</summary>
public static class LandingChallengeSetup
{
    public const string Root = "Assets/LandingChallenge/";
    public const string ProfilePath = Root + "LandingChallenge.asset";
    public const string PrefabPath = Root + "LandingControls.prefab";

    /// <summary>Installs the Landing increment in the comparison scene, preserving Discover art and all other authored assets.</summary>
    [MenuItem("Fishing Cards/Landing Challenge/Install Plain Controls")]
    public static void InstallCurrentScene()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != JournalComparisonSceneBuilder.ScenePath)
            throw new InvalidOperationException("Open the comparison scene outside Play Mode.");
        Install(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save Landing integration.");
        AssetDatabase.SaveAssets();
    }

    /// <summary>Reuses the same profile, controls and Harbor route when the scene or Harbor is regenerated.</summary>
    public static void Install(Scene scene)
    {
        BuildAssets();
        var prefab = PrefabUtility.LoadPrefabContents(PrefabPath);
        try { EnsureScoreScroll(prefab); PrefabUtility.SaveAsPrefabAsset(prefab, PrefabPath); }
        finally { PrefabUtility.UnloadPrefabContents(prefab); }
        var controller = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<FishingRunController>(true)).Single();
        var view = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<FishingRunView>(true)).Single();
        var harbor = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<JournalHarborFlow>(true)).Single();
        var controls = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<LandingChallengeView>(true)).SingleOrDefault();
        if (controls == null) controls = ((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), scene)).GetComponent<LandingChallengeView>();
        var data = new SerializedObject(controller);
        if (data.FindProperty("landingChallengeProfile").objectReferenceValue == null)
            data.FindProperty("landingChallengeProfile").objectReferenceValue = AssetDatabase.LoadAssetAtPath<LandingChallengeProfile>(ProfilePath);
        data.ApplyModifiedPropertiesWithoutUndo();
        var viewData = new SerializedObject(view); Bind(viewData, "landingChallengeView", controls); viewData.ApplyModifiedPropertiesWithoutUndo();
        for (int i = controls.ReturnedToHarbor.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEditor.Events.UnityEventTools.RemovePersistentListener(controls.ReturnedToHarbor, i);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(controls.ReturnedToHarbor, harbor.ReturnToHarbor);
        EditorUtility.SetDirty(controls); PrefabUtility.RecordPrefabInstancePropertyModifications(controls);
        controls.gameObject.SetActive(false);
    }

    /// <summary>Creates native colored rectangles and text only; existing approved Landing artwork is deliberately not imported.</summary>
    private static void BuildAssets()
    {
        if (!AssetDatabase.IsValidFolder(Root.TrimEnd('/'))) AssetDatabase.CreateFolder("Assets", "LandingChallenge");
        if (AssetDatabase.LoadAssetAtPath<LandingChallengeProfile>(ProfilePath) == null)
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<LandingChallengeProfile>(), ProfilePath);
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null) return;
        var root = new GameObject("Plain Landing Canvas", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster), typeof(LandingChallengeView));
        try
        {
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 400;
            var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
            root.AddComponent<UnityEngine.UI.Image>().color = new Color(.025f, .045f, .065f);
            var stage = Rect(root.transform, "1920x1080 Stage", 0, 0, 1920, 1080);
            var data = new SerializedObject(root.GetComponent<LandingChallengeView>());
            Bind(data, "creature", Label(stage, "Creature", "LANDING", 200, 440, 1300, 70, 40));
            Bind(data, "progress", Label(stage, "Points", "", 200, 355, 1300, 55, 29));
            Bind(data, "attempts", Label(stage, "Attempts", "", 200, 285, 650, 50, 25));
            Bind(data, "behavior", Label(stage, "Movement", "", 650, 285, 480, 50, 24));
            var track = Rect(stage, "Fishing Track", 200, 120, 1250, 170); track.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(.10f, .16f, .20f);
            Bind(data, "track", track);
            var good = Rect(track, "Good Region", 0, 0, 300, 150); good.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(.16f, .55f, .39f);
            var perfect = Rect(track, "Perfect Region", 0, 0, 100, 150); perfect.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(.85f, .65f, .15f);
            var fish = Rect(track, "Fish Indicator", 0, 0, 44, 170); fish.gameObject.AddComponent<UnityEngine.UI.Image>().color = Color.white;
            Bind(data, "goodZone", good); Bind(data, "perfectZone", perfect); Bind(data, "fish", fish);
            Label(stage, "Legend", "GREEN: GOOD     GOLD: PERFECT     WHITE: FISH", 200, 10, 1250, 40, 23);
            var bar = Rect(stage, "Points Bar", 200, -48, 1250, 28); bar.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(.1f, .14f, .17f);
            var fill = Rect(bar, "Points Fill", 0, 0, 1250, 28); fill.anchorMin = Vector2.zero; fill.anchorMax = Vector2.one;
            fill.offsetMin = fill.offsetMax = Vector2.zero; fill.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(.25f, .7f, .55f); Bind(data, "progressFill", fill);
            Bind(data, "feedback", Label(stage, "Feedback", "", 200, -105, 1300, 60, 27));
            Bind(data, "breakdown", Label(stage, "Score Breakdown", "", 200, -280, 1250, 260, 24));
            Bind(data, "strike", Button(stage, "Strike", "STRIKE / SPACE", 200, -465));
            Bind(data, "returnToHarbor", Button(stage, "Return", "RETURN TO HARBOR", 200, -465));
            var scrollRoot = Rect(stage, "Rig Scroll", -720, -20, 350, 850);
            var scroll = scrollRoot.gameObject.AddComponent<UnityEngine.UI.ScrollRect>(); scroll.horizontal = false; scroll.vertical = true;
            var viewport = Rect(scrollRoot, "Viewport", 0, 0, 350, 850); viewport.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(.06f, .1f, .13f);
            viewport.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic = true;
            var rigText = Label(viewport, "Current Rig", "CURRENT RIG", 0, 0, 320, 850, 23);
            rigText.alignment = TextAlignmentOptions.TopLeft; rigText.rectTransform.anchorMin = rigText.rectTransform.anchorMax = new Vector2(.5f, 1);
            rigText.rectTransform.pivot = new Vector2(.5f, 1); rigText.rectTransform.anchoredPosition = new Vector2(0, -15);
            var fitter = rigText.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>(); fitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport; scroll.content = rigText.rectTransform; scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
            Bind(data, "rig", rigText);
            foreach (var image in root.GetComponentsInChildren<UnityEngine.UI.Image>()) image.raycastTarget = false;
            root.GetComponent<UnityEngine.UI.Image>().raycastTarget = true;
            foreach (var button in root.GetComponentsInChildren<UnityEngine.UI.Button>()) button.GetComponent<UnityEngine.UI.Image>().raycastTarget = true;
            viewport.GetComponent<UnityEngine.UI.Image>().raycastTarget = true;
            data.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    /// <summary>Keeps arbitrarily long contribution lists clipped and scrollable without limiting the rig or score sources.</summary>
    private static void EnsureScoreScroll(GameObject root)
    {
        var data = new SerializedObject(root.GetComponent<LandingChallengeView>());
        var text = (TMP_Text)data.FindProperty("breakdown").objectReferenceValue;
        if (text.GetComponentInParent<UnityEngine.UI.ScrollRect>() != null) return;
        var scrollRoot = Rect(text.transform.parent, "Score Scroll", 200, -280, 1250, 260);
        var scroll = scrollRoot.gameObject.AddComponent<UnityEngine.UI.ScrollRect>(); scroll.horizontal = false;
        var viewport = Rect(scrollRoot, "Viewport", 0, 0, 1250, 260);
        viewport.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(.035f, .065f, .09f);
        viewport.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic = true;
        text.rectTransform.SetParent(viewport, false); text.rectTransform.anchorMin = text.rectTransform.anchorMax = new Vector2(.5f, 1);
        text.rectTransform.pivot = new Vector2(.5f, 1); text.rectTransform.anchoredPosition = new Vector2(0, -10); text.rectTransform.sizeDelta = new Vector2(1200, 260);
        text.alignment = TextAlignmentOptions.TopLeft;
        text.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>().verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        scroll.viewport = viewport; scroll.content = text.rectTransform; scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
    }

    /// <summary>Creates a center-anchored native rectangle in the fixed design stage.</summary>
    private static RectTransform Rect(Transform parent, string name, float x, float y, float width, float height)
    {
        var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform; rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f); rect.anchoredPosition = new Vector2(x, y); rect.sizeDelta = new Vector2(width, height); return rect;
    }
    /// <summary>Authors readable native labels independently from any art asset.</summary>
    private static TMP_Text Label(Transform parent, string name, string text, float x, float y, float width, float height, float size)
    {
        var label = Rect(parent, name, x, y, width, height).gameObject.AddComponent<TextMeshProUGUI>();
        label.font = TMP_Settings.defaultFontAsset; label.text = text; label.fontSize = size; label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false; return label;
    }
    /// <summary>Authors one accessible native button; no generated art or card-face changes are required.</summary>
    private static UnityEngine.UI.Button Button(Transform parent, string name, string text, float x, float y)
    {
        var rect = Rect(parent, name, x, y, 650, 75); var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>(); image.color = new Color(.15f, .3f, .4f);
        var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = image; Label(rect, "Label", text, 0, 0, 620, 65, 26); return button;
    }
    /// <summary>Serializes references without embedding scene dependencies in the reusable prefab.</summary>
    private static void Bind(SerializedObject data, string field, UnityEngine.Object value) { data.FindProperty(field).objectReferenceValue = value; }
}
