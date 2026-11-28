using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Authors the Tackle phase-transition overlay prefab and binds it to the comparison scene's FishingRunView.</summary>
public static class TacklePhaseTransitionSetup
{
    public const string PrefabPath = TackleDiscoverArtSetup.Root + "Prefabs/PhaseTransitionCanvas.prefab";
    /// <summary>Above every Tackle phase canvas (Discover 70, Build 85, Landing 400), below Harbor tag preparation (500).</summary>
    public const int SortingOrder = 450;

    [MenuItem("Fishing Cards/Tackle/Install Phase Transitions")]
    public static void InstallCurrentScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Install outside Play Mode.");
        var scene = SceneManager.GetActiveScene();
        if (scene.path != JournalComparisonSceneBuilder.ScenePath) throw new InvalidOperationException("Open the comparison scene.");
        BuildPrefab(); Install(scene); EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save phase-transition binding.");
        AssetDatabase.SaveAssets();
    }

    /// <summary>Idempotent scene binding, retained by full slot/scene regeneration.</summary>
    public static void Install(Scene scene)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null) BuildPrefab();
        var runView = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<FishingRunView>(true)).Single();
        var transition = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<TacklePhaseTransition>(true)).SingleOrDefault();
        if (transition == null) transition = ((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), scene)).GetComponent<TacklePhaseTransition>();
        var data = new SerializedObject(runView); data.FindProperty("phaseTransition").objectReferenceValue = transition; data.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>Creates or refreshes the overlay: full-screen snapshot, masked interior window with its sliding copy and scrim, and an input blocker.</summary>
    public static void BuildPrefab()
    {
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        var root = existing != null ? PrefabUtility.LoadPrefabContents(PrefabPath)
            : new GameObject("PhaseTransitionCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(TacklePhaseTransition));
        try
        {
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = SortingOrder; canvas.enabled = false;
            var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            // Jolt container: the held outgoing frame and the shutter window move together when the shutter lands.
            var content = Child(root.transform, "Content"); Stretch(content); content.SetAsFirstSibling();
            var full = Raw(content, "Full snapshot", false);
            var legacyFull = root.transform.Find("Full snapshot"); if (legacyFull != null) UnityEngine.Object.DestroyImmediate(legacyFull.gameObject);
            var legacyWindow = root.transform.Find("Interior window"); if (legacyWindow != null) UnityEngine.Object.DestroyImmediate(legacyWindow.gameObject);
            var window = Child(content, "Interior window"); if (window.GetComponent<RectMask2D>() == null) window.gameObject.AddComponent<RectMask2D>();
            foreach (string old in new[] { "Interior snapshot", "Lip", "Scrim", "Holder" }) { var gone = window.Find(old); if (gone != null) UnityEngine.Object.DestroyImmediate(gone.gameObject); }
            var shutter = window.Find("Shutter")?.GetComponent<TackleShutterGraphic>();
            if (shutter == null) shutter = new GameObject("Shutter", typeof(RectTransform), typeof(CanvasRenderer), typeof(TackleShutterGraphic)).GetComponent<TackleShutterGraphic>();
            shutter.rectTransform.SetParent(window, false); Stretch(shutter.rectTransform); shutter.raycastTarget = false; shutter.color = Color.white;
            full.rectTransform.SetAsFirstSibling();
            var blocker = Raw(root.transform, "Input blocker", true); blocker.color = Color.clear; blocker.rectTransform.SetAsLastSibling();
            full.gameObject.SetActive(false); window.gameObject.SetActive(false); blocker.gameObject.SetActive(false);
            var data = new SerializedObject(root.GetComponent<TacklePhaseTransition>());
            data.FindProperty("overlay").objectReferenceValue = canvas;
            data.FindProperty("content").objectReferenceValue = content;
            data.FindProperty("fullSnapshot").objectReferenceValue = full;
            data.FindProperty("blocker").objectReferenceValue = blocker;
            data.FindProperty("interiorWindow").objectReferenceValue = window;
            data.FindProperty("shutter").objectReferenceValue = shutter;
            data.FindProperty("bodyBounds").rectValue = TraceBody(shutter);
            data.ApplyModifiedPropertiesWithoutUndo();
            DeepwaterAudioSetup.ConfigureTransition(root);
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally
        {
            if (existing != null) PrefabUtility.UnloadPrefabContents(root); else UnityEngine.Object.DestroyImmediate(root);
        }
    }

    /// <summary>
    /// Regular body outline measured from the shared housing artwork (landing-machine.png, 1739x904; Discover, Landing and Build use
    /// pixel-identical outer shells), in texture pixels from the top-left: the cabinet's straight left edge, its innermost straight
    /// right edge, its main top line and bottom, and chamfers matching its cut corners. Pipes, knobs, the raised top sections and the
    /// right-side bulges stay outside the shutter as hardware.
    /// </summary>
    public const int HousingWidth = 1739, HousingHeight = 904, BodyLeft = 120, BodyRight = 1652, BodyTop = 31, BodyBottom = 887, TopChamfer = 46, BottomChamfer = 14;

    /// <summary>Stores the chamfered body outline on the shutter as one span per texture row and returns its bounds in stage pixels.</summary>
    public static Rect TraceBody(TackleShutterGraphic shutter)
    {
        int rows = BodyBottom - BodyTop; float width = BodyRight - BodyLeft;
        var starts = new int[rows + 1]; var spans = new Vector2[rows];
        for (int i = 0; i < rows; i++)
        {
            float inset = Mathf.Max(0, TopChamfer - (i + .5f), BottomChamfer - (rows - i - .5f));
            starts[i] = i; spans[i] = new Vector2(inset / width, 1 - inset / width);
        }
        starts[rows] = rows;
        shutter.SetSilhouette(rows, starts, spans);
        Rect stage = TackleMachineGeometry.StageBounds; float sx = stage.width / HousingWidth, sy = stage.height / HousingHeight;
        return new Rect(stage.x + BodyLeft * sx, stage.y + BodyTop * sy, width * sx, rows * sy);
    }

    private static RectTransform Child(Transform parent, string name)
    {
        var rect = parent.Find(name) as RectTransform;
        if (rect == null) rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
        rect.SetParent(parent, false); return rect;
    }

    private static RawImage Raw(Transform parent, string name, bool raycast)
    {
        var image = parent.Find(name)?.GetComponent<RawImage>();
        if (image == null) image = new GameObject(name, typeof(RectTransform), typeof(RawImage)).GetComponent<RawImage>();
        image.rectTransform.SetParent(parent, false); Stretch(image.rectTransform); image.raycastTarget = raycast; image.color = Color.white;
        return image;
    }

    private static void Stretch(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; rect.pivot = new Vector2(.5f, .5f); }
}
