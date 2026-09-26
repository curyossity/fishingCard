using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class FishingRunViewPrefabBuilder
{
    private const string PrefabPath = "Assets/Prefabs/UI/FishingRunView.prefab";
    private const string CreatureCardPrefabPath = "Assets/Prefabs/UI/Components/CreatureCardView.prefab";
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const string ValidationDirectory = "Docs/Validation/Phase2";

    private const string GameplaySortingLayer = "Gameplay UI";
    private const string TooltipSortingLayer = "Tooltips";
    private const string TransitionSortingLayer = "Transitions";
    private const string ModalSortingLayer = "Modals";

    /// <summary>
    /// Rebuilds the reusable run-view prefab and installs an instance in the gameplay scene.
    /// </summary>
    [MenuItem("Fishing Cards/UI/Rebuild Fishing Run View")]
    public static void Build()
    {
        try
        {
            EnsureProjectFolders();
            EnsureSortingLayers();
            BuildPrefab();
            InstallPrefabInScene();
            ValidateAndCapture();
            AssetDatabase.SaveAssets();
            Debug.Log("FishingRunView prefab, scene instance, and Phase 2 validation were rebuilt successfully.");

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            Directory.CreateDirectory(ValidationDirectory);
            File.WriteAllText(Path.Combine(ValidationDirectory, "failure.txt"), exception.ToString());

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }

            throw;
        }
    }

    /// <summary>
    /// Creates the authored root hierarchy and serializes all stable view bindings.
    /// </summary>
    private static void BuildPrefab()
    {
        GameObject root = CreateUiObject("FishingRunView", null);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(1920f, 1080f);

        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        SetCanvasSortingLayer(canvas, GameplaySortingLayer);
        canvas.sortingOrder = 0;

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        root.AddComponent<GraphicRaycaster>();

        FishingRunView runView = root.AddComponent<FishingRunView>();

        RectTransform background = CreateStretchRegion("Background", rootRect);
        CanvasGroup backgroundGroup = background.gameObject.AddComponent<CanvasGroup>();
        BuildBackground(background);

        RectTransform topNavigation = CreateRegion(
            "TopNavigationBar",
            rootRect,
            new Vector2(0.015f, 0.915f),
            new Vector2(0.985f, 0.985f));
        CanvasGroup topNavigationGroup = topNavigation.gameObject.AddComponent<CanvasGroup>();
        AddSlicedImage(topNavigation, "Top Navigation Surface", "Frames/top-navigation-base-9slice.png", Color.white);

        RectTransform mainContent = CreateRegion(
            "MainContent",
            rootRect,
            new Vector2(0.015f, 0.29f),
            new Vector2(0.985f, 0.90f));
        CanvasGroup mainContentGroup = mainContent.gameObject.AddComponent<CanvasGroup>();

        RectTransform catchRigPanel = CreateRegion(
            "CatchRigPanel",
            mainContent,
            Vector2.zero,
            new Vector2(0.22f, 1f));
        AddContactShadow(catchRigPanel);

        RectTransform encounterPanel = CreateRegion(
            "EncounterPanel",
            mainContent,
            new Vector2(0.285f, 0f),
            new Vector2(0.715f, 1f));
        AddContactShadow(encounterPanel);

        RectTransform runControlsPanel = CreateRegion(
            "RunControlsPanel",
            mainContent,
            new Vector2(0.78f, 0f),
            Vector2.one);
        AddContactShadow(runControlsPanel);

        AddDivider(mainContent, "Left Column Divider", 0.2525f);
        AddDivider(mainContent, "Right Column Divider", 0.7475f);

        RectTransform techniqueHand = CreateRegion(
            "TechniqueHand",
            rootRect,
            new Vector2(0.225f, 0.02f),
            new Vector2(0.775f, 0.275f));
        CanvasGroup techniqueHandGroup = techniqueHand.gameObject.AddComponent<CanvasGroup>();
        AddContactShadow(techniqueHand);

        Canvas tooltipLayer = CreateOrderedLayer("TooltipLayer", rootRect, TooltipSortingLayer, 100, true);
        Canvas transitionLayer = CreateOrderedLayer("TransitionLayer", rootRect, TransitionSortingLayer, 200, false);
        Canvas modalLayer = CreateOrderedLayer("ModalLayer", rootRect, ModalSortingLayer, 300, true);

        CatchChainView catchChainView = catchRigPanel.gameObject.AddComponent<CatchChainView>();
        TechniqueHandView techniqueHandView = techniqueHand.gameObject.AddComponent<TechniqueHandView>();
        RunResultView runResultView = modalLayer.gameObject.AddComponent<RunResultView>();

        SetSerializedBoolean(catchChainView, "fillParentRegion", true);
        SetSerializedBoolean(techniqueHandView, "fillParentRegion", true);
        ConfigureRunView(
            runView,
            background,
            topNavigation,
            mainContent,
            catchRigPanel,
            encounterPanel,
            runControlsPanel,
            techniqueHand,
            tooltipLayer,
            transitionLayer,
            modalLayer,
            backgroundGroup,
            topNavigationGroup,
            mainContentGroup,
            techniqueHandGroup);

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        UnityEngine.Object.DestroyImmediate(root);
    }

    /// <summary>
    /// Replaces the prototype canvas with the reusable prefab and reconnects controller views.
    /// </summary>
    private static void InstallPrefabInScene()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        FishingRunController controller = UnityEngine.Object.FindAnyObjectByType<FishingRunController>();

        if (controller == null)
        {
            throw new InvalidOperationException("SampleScene does not contain a FishingRunController.");
        }

        FishingRunView existingView = UnityEngine.Object.FindAnyObjectByType<FishingRunView>();
        GameObject oldRoot = existingView == null ? null : existingView.gameObject;
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        instance.name = "FishingRunView";

        if (oldRoot != null && oldRoot != instance)
        {
            UnityEngine.Object.DestroyImmediate(oldRoot);
        }

        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("techniqueHandView").objectReferenceValue =
            instance.GetComponentInChildren<TechniqueHandView>(true);
        serializedController.FindProperty("catchChainView").objectReferenceValue =
            instance.GetComponentInChildren<CatchChainView>(true);
        serializedController.FindProperty("runResultView").objectReferenceValue =
            instance.GetComponentInChildren<RunResultView>(true);
        serializedController.FindProperty("fishingRunView").objectReferenceValue =
            instance.GetComponent<FishingRunView>();
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    /// <summary>
    /// Validates the prefab contract and captures the static foundation at target aspect ratios.
    /// </summary>
    private static void ValidateAndCapture()
    {
        Directory.CreateDirectory(ValidationDirectory);
        string failurePath = Path.Combine(ValidationDirectory, "failure.txt");
        if (File.Exists(failurePath))
        {
            File.Delete(failurePath);
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            throw new InvalidOperationException("FishingRunView prefab was not created.");
        }

        CanvasScaler scaler = prefab.GetComponent<CanvasScaler>();
        Require(scaler != null, "FishingRunView requires a CanvasScaler.");
        Require(scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize, "CanvasScaler must scale with screen size.");
        Require(Vector2.Distance(scaler.referenceResolution, new Vector2(1920f, 1080f)) < 0.01f, "CanvasScaler reference resolution must be 1920x1080.");
        Require(Mathf.Abs(scaler.matchWidthOrHeight - 0.5f) < 0.001f, "CanvasScaler match must be 0.5.");

        string[] requiredPaths =
        {
            "Background",
            "TopNavigationBar",
            "MainContent",
            "MainContent/CatchRigPanel",
            "MainContent/EncounterPanel",
            "MainContent/RunControlsPanel",
            "TechniqueHand",
            "TooltipLayer",
            "TransitionLayer",
            "ModalLayer"
        };

        foreach (string path in requiredPaths)
        {
            Require(prefab.transform.Find(path) != null, "Missing prefab region: " + path);
        }

        ValidateOrderedLayer(prefab, "TooltipLayer", TooltipSortingLayer, 100);
        ValidateOrderedLayer(prefab, "TransitionLayer", TransitionSortingLayer, 200);
        ValidateOrderedLayer(prefab, "ModalLayer", ModalSortingLayer, 300);
        Require(prefab.GetComponentsInChildren<CanvasGroup>(true).Length >= 4, "Major sections require independent CanvasGroups.");

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        FishingRunController controller = UnityEngine.Object.FindAnyObjectByType<FishingRunController>();
        FishingRunView sceneView = UnityEngine.Object.FindAnyObjectByType<FishingRunView>();
        Require(controller != null && sceneView != null, "SampleScene must contain the controller and prefab view instance.");

        CaptureFoundation(1920, 1080, "foundation-1920x1080.png");
        CaptureFoundation(1366, 768, "foundation-1366x768.png");
        CaptureFoundation(1280, 800, "foundation-1280x800.png");
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        string result = "{\"unity\":\"" + Application.unityVersion
            + "\",\"prefab\":\"FishingRunView\",\"referenceResolution\":\"1920x1080\""
            + ",\"sortingLayers\":4,\"captures\":3,\"status\":\"passed\"}";
        File.WriteAllText(Path.Combine(ValidationDirectory, "result.json"), result);
    }

    /// <summary>
    /// Renders the static prefab foundation through a camera-backed Canvas.
    /// </summary>
    private static void CaptureFoundation(int width, int height, string filename)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        Canvas canvas = instance.GetComponent<Canvas>();

        GameObject cameraObject = new GameObject("Validation Camera", typeof(Camera));
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.orthographic = true;
        camera.nearClipPlane = -10f;
        camera.farClipPlane = 10f;

        RenderTexture target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        camera.targetTexture = target;
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 1f;

        Canvas.ForceUpdateCanvases();
        camera.Render();
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = target;
        Texture2D capture = new Texture2D(width, height, TextureFormat.RGB24, false);
        capture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
        capture.Apply();
        File.WriteAllBytes(Path.Combine(ValidationDirectory, filename), capture.EncodeToPNG());
        RenderTexture.active = previous;

        UnityEngine.Object.DestroyImmediate(capture);
        camera.targetTexture = null;
        target.Release();
        UnityEngine.Object.DestroyImmediate(target);
        UnityEngine.Object.DestroyImmediate(cameraObject);
        UnityEngine.Object.DestroyImmediate(instance);
    }

    /// <summary>
    /// Adds the supplied complete nautical-chart background at its authored 16:9 aspect ratio.
    /// </summary>
    private static void BuildBackground(RectTransform parent)
    {
        RectTransform artwork = CreateStretchRegion("Artwork", parent);
        AspectRatioFitter fitter = artwork.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fitter.aspectRatio = 16f / 9f;

        AddSimpleImage(
            artwork,
            "Nautical Chart Background",
            "Backgrounds/fishing-ui-background-1920x1080.png",
            Color.white);
    }

    /// <summary>
    /// Writes the stable hierarchy references into the runtime view component.
    /// </summary>
    private static void ConfigureRunView(
        FishingRunView runView,
        RectTransform background,
        RectTransform topNavigation,
        RectTransform mainContent,
        RectTransform catchRigPanel,
        RectTransform encounterPanel,
        RectTransform runControlsPanel,
        RectTransform techniqueHand,
        Canvas tooltipLayer,
        Canvas transitionLayer,
        Canvas modalLayer,
        CanvasGroup backgroundGroup,
        CanvasGroup topNavigationGroup,
        CanvasGroup mainContentGroup,
        CanvasGroup techniqueHandGroup)
    {
        SerializedObject serializedView = new SerializedObject(runView);
        SetObjectReference(
            serializedView,
            "creatureCardPrefab",
            AssetDatabase.LoadAssetAtPath<CreatureCardView>(CreatureCardPrefabPath));
        SetObjectReference(serializedView, "fallbackCreatureCardFace", LoadSprite("Cards/Creature/creature-card-base.png"));
        SetObjectReference(serializedView, "rarityHookSprite", LoadSprite("Markers/anchor-filled.png"));
        SetObjectReference(serializedView, "backgroundRegion", background);
        SetObjectReference(serializedView, "topNavigationBar", topNavigation);
        SetObjectReference(serializedView, "mainContent", mainContent);
        SetObjectReference(serializedView, "catchRigPanel", catchRigPanel);
        SetObjectReference(serializedView, "encounterPanel", encounterPanel);
        SetObjectReference(serializedView, "runControlsPanel", runControlsPanel);
        SetObjectReference(serializedView, "techniqueHand", techniqueHand);
        SetObjectReference(serializedView, "tooltipLayer", tooltipLayer);
        SetObjectReference(serializedView, "transitionLayer", transitionLayer);
        SetObjectReference(serializedView, "modalLayer", modalLayer);
        SetObjectReference(serializedView, "backgroundGroup", backgroundGroup);
        SetObjectReference(serializedView, "topNavigationGroup", topNavigationGroup);
        SetObjectReference(serializedView, "mainContentGroup", mainContentGroup);
        SetObjectReference(serializedView, "techniqueHandGroup", techniqueHandGroup);
        serializedView.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>
    /// Adds a sorting layer when it is not already present in project settings.
    /// </summary>
    private static void EnsureSortingLayers()
    {
        UnityEngine.Object tagManagerAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset").First();
        SerializedObject tagManager = new SerializedObject(tagManagerAsset);
        SerializedProperty layers = tagManager.FindProperty("m_SortingLayers");
        string[] names = { GameplaySortingLayer, TooltipSortingLayer, TransitionSortingLayer, ModalSortingLayer };

        foreach (string name in names)
        {
            bool exists = Enumerable.Range(0, layers.arraySize)
                .Any(index => layers.GetArrayElementAtIndex(index).FindPropertyRelative("name").stringValue == name);
            if (exists)
            {
                continue;
            }

            int index = layers.arraySize;
            layers.InsertArrayElementAtIndex(index);
            SerializedProperty layer = layers.GetArrayElementAtIndex(index);
            layer.FindPropertyRelative("name").stringValue = name;
            layer.FindPropertyRelative("uniqueID").longValue = unchecked((uint)Animator.StringToHash("FishingCards." + name));
            layer.FindPropertyRelative("locked").boolValue = false;
        }

        tagManager.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// Creates a full-screen child Canvas with deterministic render ordering.
    /// </summary>
    private static Canvas CreateOrderedLayer(
        string name,
        RectTransform parent,
        string sortingLayer,
        int sortingOrder,
        bool receivesInput)
    {
        RectTransform rect = CreateStretchRegion(name, parent);
        Canvas canvas = rect.gameObject.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        SetCanvasSortingLayer(canvas, sortingLayer);
        canvas.sortingOrder = sortingOrder;

        if (receivesInput)
        {
            rect.gameObject.AddComponent<GraphicRaycaster>();
        }

        return canvas;
    }

    /// <summary>
    /// Verifies one overlay layer's sorting contract.
    /// </summary>
    private static void ValidateOrderedLayer(GameObject root, string path, string sortingLayer, int order)
    {
        Canvas canvas = root.transform.Find(path).GetComponent<Canvas>();
        Require(canvas != null && canvas.overrideSorting, path + " must override Canvas sorting.");
        Require(GetSortingLayerId(sortingLayer) != 0, "Missing project sorting layer: " + sortingLayer);
        Require(canvas.sortingOrder == order, path + " has the wrong sorting order.");
    }

    private static RectTransform CreateStretchRegion(string name, Transform parent)
    {
        return CreateRegion(name, parent, Vector2.zero, Vector2.one);
    }

    private static RectTransform CreateRegion(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        RectTransform rect = CreateUiObject(name, parent).GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return rect;
    }

    private static GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.layer = LayerMask.NameToLayer("UI");
        if (parent != null)
        {
            gameObject.transform.SetParent(parent, false);
        }

        return gameObject;
    }

    private static Image AddSimpleImage(Transform parent, string name, string relativePath, Color color)
    {
        RectTransform rect = CreateStretchRegion(name, parent);
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = LoadSprite(relativePath);
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static Image AddSlicedImage(Transform parent, string name, string relativePath, Color color)
    {
        Image image = AddSimpleImage(parent, name, relativePath, color);
        image.type = Image.Type.Sliced;
        return image;
    }

    private static void AddDivider(RectTransform parent, string name, float anchorX)
    {
        RectTransform rect = CreateRegion(name, parent, new Vector2(anchorX, 0f), new Vector2(anchorX, 1f));
        rect.sizeDelta = new Vector2(18f, 0f);
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = LoadSprite("Frames/main-column-divider.png");
        image.preserveAspect = false;
        image.raycastTarget = false;
    }

    private static void AddContactShadow(RectTransform parent)
    {
        RectTransform rect = CreateRegion("Contact Shadow", parent, new Vector2(0.03f, 0.01f), new Vector2(0.97f, 0.99f));
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = LoadSprite("Effects/contact-shadow-soft.png");
        image.color = new Color(1f, 1f, 1f, 0.42f);
        image.raycastTarget = false;
    }

    private static Sprite LoadSprite(string relativePath)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/FishingUIAssets/" + relativePath);
        if (sprite == null)
        {
            throw new InvalidOperationException("Missing UI sprite: " + relativePath);
        }

        return sprite;
    }

    private static void SetObjectReference(SerializedObject target, string propertyName, UnityEngine.Object value)
    {
        SerializedProperty property = target.FindProperty(propertyName);
        if (property == null)
        {
            throw new InvalidOperationException("Missing serialized property: " + propertyName);
        }

        property.objectReferenceValue = value;
    }

    private static void SetSerializedBoolean(UnityEngine.Object target, string propertyName, bool value)
    {
        SerializedObject serializedTarget = new SerializedObject(target);
        SerializedProperty property = serializedTarget.FindProperty(propertyName);
        Require(property != null, "Missing serialized property: " + propertyName);
        property.boolValue = value;
        serializedTarget.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetCanvasSortingLayer(Canvas canvas, string sortingLayerName)
    {
        int sortingLayerId = GetSortingLayerId(sortingLayerName);
        SerializedObject serializedCanvas = new SerializedObject(canvas);
        SerializedProperty sortingLayerProperty = serializedCanvas.FindProperty("m_SortingLayerID");
        Require(sortingLayerProperty != null, "Canvas sorting-layer property is unavailable.");
        sortingLayerProperty.longValue = unchecked((uint)sortingLayerId);
        serializedCanvas.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void EnsureProjectFolders()
    {
        Directory.CreateDirectory("Assets/Prefabs/UI");
        Directory.CreateDirectory(ValidationDirectory);
    }

    private static int GetSortingLayerId(string sortingLayerName)
    {
        int sortingLayerId = SortingLayer.NameToID(sortingLayerName);
        if (sortingLayerId == 0 && sortingLayerName != "Default")
        {
            throw new InvalidOperationException("Unity has not imported sorting layer: " + sortingLayerName);
        }

        return sortingLayerId;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
