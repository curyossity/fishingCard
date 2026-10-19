using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>Checks assembled journal HUD data, layering, button geometry and rendered input in isolation.</summary>
public static class JournalGameplayHudChecks
{
    private const string Root = MidnightFieldJournalAssetSetup.Root;
    private const string Output = "Docs/UIThemes/MidnightFieldJournal/";
    private static int assertions, descendClicks, surfaceClicks, settingsClicks;
    private static GameObject smoke;
    private static bool oldBackground;
    private static byte[] sceneBytes;
    private static string scenePath;
    [Serializable] private sealed class Report { public bool passed, sceneUnchanged; public int assertions; }

    /// <summary>Validates the actual composed prefab and captures it without installing a game scene.</summary>
    [MenuItem("Fishing Cards/UI/Midnight Field Journal/Validate Gameplay HUD")]
    public static void Run() { ValidateComponent(true); }

    /// <summary>Checks bindings independently of image export when a historical preview is open in an external viewer.</summary>
    public static void ValidateComponent(bool capture)
    {
        if (Application.isPlaying) throw new InvalidOperationException("Capture in Edit Mode.");
        assertions = 0; Scene active = SceneManager.GetActiveScene(); bool dirty = active.isDirty;
        Scene scene = EditorSceneManager.NewPreviewScene(); GameObject obj = null;
        try
        {
            obj = UnityEngine.Object.Instantiate(Prefab()); SceneManager.MoveGameObjectToScene(obj, scene);
            Canvas canvas = obj.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace; obj.GetComponent<RectTransform>().sizeDelta = new Vector2(3840, 2160);
            obj.transform.position = Vector3.zero;
            obj.GetComponent<UnityEngine.UI.CanvasScaler>().enabled = false;
            Refresh(obj, true); var hud = obj.GetComponent<JournalGameplayHud>(); var view = obj.GetComponent<FishingRunView>();
            Require(hud.BiomeText.text == Biome().DisplayName.ToUpperInvariant() && hud.DepthText.text == "DEPTH  8 m" && hud.DeckText.text == "DECK  7", "Navigation is sample data.");
            Require(Label(obj, "Haul Value").text == "21" && Label(obj, "Load Text").text == "4 / 8", "Load/haul not controller values.");
            Require(obj.GetComponentsInChildren<CreatureCardView>(true).Length == 2, "Encounter transition slots duplicated.");
            for (int i = 0; i < 5; i++) Refresh(obj, true);
            Require(obj.GetComponentsInChildren<CreatureCardView>(true).Length == 2, "Refresh rebuilt encounters.");
            Require(view.TooltipLayer.sortingOrder < view.TransitionLayer.sortingOrder && view.TransitionLayer.sortingOrder < view.ModalLayer.sortingOrder, "Overlay ordering incorrect.");
            Require(!obj.GetComponentsInChildren<Transform>(true).Any(t => t.name.Contains("Tension") || t.name.Contains("Release Button") || t.name.Contains("Run number")), "Retired/sample controls generated.");
            Require(hud.DescendButton.targetGraphic.raycastTarget == false && hud.DescendButton.GetComponentsInChildren<UnityEngine.UI.Image>().Any(i => i.raycastTarget && i.color.a == 0), "Action padding intercepts input.");
            var legacy = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/FishingRunView.prefab").GetComponent<FishingRunView>();
            Require(new SerializedObject(legacy).FindProperty("hudPresentation").objectReferenceValue == null, "Legacy HUD overwritten.");
            view.SetTrailPreview("Spiny Lobster at depth 11"); Require(Label(obj, "Encounter Trail Preview").text.Contains("Spiny Lobster"), "Learned trail missing."); view.SetTrailPreview("");
            hud.Refresh(Biome(), 8, 12, 8, 0, null);
            Require(Label(obj, "Haul Value").text == "?" && Label(obj, "Load Text").text == "12 / 8", "Hidden haul or overload suppressed.");
            var meter = obj.GetComponentInChildren<LineLoadMeterView>(); var pipRoot = (RectTransform)new SerializedObject(meter).FindProperty("pipRoot").objectReferenceValue;
            Require(pipRoot.GetComponentsInChildren<UnityEngine.UI.Image>().Length == 8, "Capacity pip count is fixed to concept.");
            var meterData = new SerializedObject(meter); var danger = meterData.FindProperty("dangerColor").colorValue;
            Require(pipRoot.GetComponentsInChildren<UnityEngine.UI.Image>().All(p => p.color == danger), "Overload danger segments missing.");
            hud.Refresh(Biome(), 8, 6, 8, 7, 21);
            Require(!pipRoot.GetComponentsInChildren<UnityEngine.UI.Image>().Any(p => p.color == danger), "Invented sub-capacity danger threshold.");
            var tierRoot = hud.GetComponentInChildren<JournalDepthGuide>().GetComponent<RectTransform>();
            Require(tierRoot.GetComponentsInChildren<JournalDepthTierView>().Length == Biome().DepthTiers.Length, "Tier rows do not follow authored biome data.");
            hud.Refresh(null, 0, 0, 0, 0, 0); Require(pipRoot.GetComponentsInChildren<UnityEngine.UI.Image>().Length == 0 && Label(obj, "Haul Value").text == "0", "Empty load/haul missing.");
            Require(tierRoot.GetComponentsInChildren<TMP_Text>().Length == 0, "Old biome guide leaked after clearing.");
            Refresh(obj, true); Populate(obj);
            if (capture) { Capture(obj, scene, 3840, 2160, "UI06Gameplay4K.png"); Capture(obj, scene, 1920, 1080, "UI06Gameplay1080p.png"); }
            string[] deps = AssetDatabase.GetDependencies(Root + "Prefabs/MidnightFieldJournalCanvas.prefab", true);
            Require(!deps.Any(p => p.StartsWith("Assets/FishingUIAssets/") || p.StartsWith("Assets/Prefabs/UI/")), "HUD depends on old visual assets.");
        }
        finally { if (obj != null) obj.GetComponentInChildren<JournalTechniqueHandPresentation>(true)?.ReleaseResources(); EditorSceneManager.ClosePreviewScene(scene); }
        bool unchanged = SceneManager.GetActiveScene() == active && active.isDirty == dirty; Require(unchanged, "Preview changed open scene.");
        File.WriteAllText(Output + "UI06Validation.json", JsonUtility.ToJson(new Report { passed = true, sceneUnchanged = unchanged, assertions = assertions }, true));
    }
    /// <summary>Creates a transient complete HUD and lets real canvas graphic depths settle for input testing.</summary>
    public static void StartPlayModeSmoke()
    {
        if (!Application.isPlaying || smoke != null) throw new InvalidOperationException("Start once in Play Mode.");
        assertions = descendClicks = surfaceClicks = settingsClicks = 0; oldBackground = Application.runInBackground; Application.runInBackground = true;
        scenePath = SceneManager.GetActiveScene().path; sceneBytes = File.ReadAllBytes(scenePath);
        smoke = UnityEngine.Object.Instantiate(Prefab()); smoke.GetComponent<Canvas>().sortingOrder = 30000;
        foreach (Canvas overlay in smoke.GetComponentsInChildren<Canvas>()) if (overlay != smoke.GetComponent<Canvas>() && overlay.overrideSorting) overlay.sortingOrder += 30000;
        Refresh(smoke, true); Populate(smoke);
        smoke.GetComponent<FishingRunView>().SettingsRequested += () => settingsClicks++;
        GameObject modal = new GameObject("Modal Test Blocker", typeof(RectTransform), typeof(UnityEngine.UI.Image)); modal.transform.SetParent(smoke.GetComponent<FishingRunView>().ModalLayer.transform, false);
        var rect = modal.GetComponent<RectTransform>(); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
        modal.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, .01f); modal.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
    }
    /// <summary>Checks actual action hits, transparent padding, command availability, settings and modal sorting.</summary>
    public static void FinishPlayModeSmoke()
    {
        try
        {
            var hud = smoke.GetComponent<JournalGameplayHud>(); var view = smoke.GetComponent<FishingRunView>(); var events = EventSystem.current;
            Require(events != null, "Missing EventSystem.");
            foreach (var button in new[] { hud.DescendButton, hud.SurfaceButton, hud.SettingsButton })
            {
                var rect = button.GetComponent<RectTransform>(); var pointer = new PointerEventData(events) { button = PointerEventData.InputButton.Left, position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center)) };
                var hits = new System.Collections.Generic.List<RaycastResult>(); events.RaycastAll(pointer, hits);
                Require(hits.Count > 0 && hits[0].gameObject.GetComponentInParent<UnityEngine.UI.Button>() == button, "Action hit target blocked: " + button.name);
                ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, pointer, ExecuteEvents.pointerClickHandler);
            }
            Require(descendClicks == 1 && surfaceClicks == 1 && settingsClicks == 1, "Callbacks missing or duplicated.");
            var settings = smoke.GetComponentInChildren<JournalSettingsModal>(true);
            if (settings != null && settings.IsVisible) ExecuteEvents.Execute(settings.CloseButton.gameObject, new BaseEventData(events), ExecuteEvents.submitHandler);
            var current = (CreatureCardView)typeof(FishingRunView).GetField("encounterCardView", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(view);
            var baitRegion = (RectTransform)new SerializedObject(current).FindProperty("baitRulesRegion").objectReferenceValue;
            Require(baitRegion.GetComponent<UnityEngine.UI.Button>().IsInteractable(), "Authored role button blocked by transition parent at rest.");
            ExecuteEvents.Execute(baitRegion.gameObject, new BaseEventData(events), ExecuteEvents.submitHandler);
            Require(view.SelectedEncounterEffectChoice == EncounterEffectChoice.Bait, "Assembled role selection failed.");
            Refresh(smoke, false); ExecuteEvents.Execute(hud.SurfaceButton.gameObject, new BaseEventData(events), ExecuteEvents.submitHandler); Require(surfaceClicks == 1, "Disabled action dispatched.");
            Require(!baitRegion.GetComponent<UnityEngine.UI.Button>().IsInteractable(), "Locked role button ignores parent state.");
            Refresh(smoke, true);
            ExecuteEvents.Execute(hud.SurfaceButton.gameObject, new BaseEventData(events), ExecuteEvents.submitHandler); Require(surfaceClicks == 2, "Navigation submit missing.");
            var buttonRect = hud.DescendButton.GetComponent<RectTransform>();
            var paddingPointer = new PointerEventData(events) { position = RectTransformUtility.WorldToScreenPoint(null, buttonRect.TransformPoint(new Vector2(0, buttonRect.rect.yMax + 30))) };
            var paddingHits = new System.Collections.Generic.List<RaycastResult>(); events.RaycastAll(paddingPointer, paddingHits);
            Require(!paddingHits.Any(h => h.gameObject.GetComponentInParent<UnityEngine.UI.Button>() == hud.DescendButton), "Transparent margin is clickable.");
            GameObject modal = view.ModalLayer.transform.Find("Modal Test Blocker").gameObject; modal.GetComponent<UnityEngine.UI.Image>().raycastTarget = true;
            var modalPointer = new PointerEventData(events) { position = RectTransformUtility.WorldToScreenPoint(null, buttonRect.TransformPoint(buttonRect.rect.center)) }; var modalHits = new System.Collections.Generic.List<RaycastResult>(); events.RaycastAll(modalPointer, modalHits);
            Require(modalHits.Count > 0 && modalHits[0].gameObject == modal, "Modal does not block HUD.");
            bool unchanged = sceneBytes.SequenceEqual(File.ReadAllBytes(scenePath)); Require(unchanged, "Saved scene changed.");
            File.WriteAllText(Output + "UI06PlayModeValidation.json", JsonUtility.ToJson(new Report { passed = true, sceneUnchanged = unchanged, assertions = assertions }, true));
        }
        finally { Application.runInBackground = oldBackground; UnityEngine.Object.Destroy(smoke); smoke = null; }
    }
    /// <summary>Refreshes the existing run-view endpoint with representative runtime data and current command delegates.</summary>
    private static void Refresh(GameObject obj, bool available)
    { obj.GetComponent<FishingRunView>().Refresh(true, Biome(), 8, Card("Small Octopus"), 1, 9, false, 8, 4, 7, -1, available, false, available, () => { descendClicks++; return true; }, null, () => { surfaceClicks++; return true; }, false, 21); }
    /// <summary>Populates real reusable chain/hand views; no samples are saved into prefabs.</summary>
    private static void Populate(GameObject obj)
    {
        obj.GetComponentInChildren<CatchChainView>().Refresh(new[] { new CardInstance(1, Card("Sardine")), new CardInstance(2, Card("Clinging Crab")), new CardInstance(3, Card("Glass Minnow"), null, CatchAttachmentRole.Bait) }, null, 4, 8, -1, i => { });
        obj.GetComponentInChildren<TechniqueHandView>().Refresh(new[] { Card("Cut Loose"), Card("Slip Past"), Card("Steady Line"), Card("Prize Hook") }, new[] { true, true, true, true }, new[] { "", "", "", "" }, 7, 1, i => true, 1);
        obj.GetComponentInChildren<JournalTechniqueHandPresentation>().RenderNow();
    }
    /// <summary>Exports real component pixels at the target resolution.</summary>
    private static void Capture(GameObject obj, Scene scene, int width, int height, string file)
    {
        var cameraObj = new GameObject("Journal HUD Capture", typeof(Camera)); SceneManager.MoveGameObjectToScene(cameraObj, scene); var camera = cameraObj.GetComponent<Camera>();
        camera.scene = scene; camera.orthographic = true; camera.orthographicSize = 1080; camera.transform.position = new Vector3(0, 0, -100); camera.nearClipPlane = .1f; camera.farClipPlane = 2000; camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black;
        var target = new RenderTexture(width, height, 24); var old = RenderTexture.active; Texture2D pixels = null;
        try
        {
            camera.targetTexture = target; obj.GetComponent<Canvas>().worldCamera = camera;
            foreach (Canvas overlay in obj.GetComponentsInChildren<Canvas>()) overlay.worldCamera = camera;
            Canvas.ForceUpdateCanvases(); obj.GetComponentInChildren<JournalTechniqueHandPresentation>().RenderNow();
            foreach (TMP_Text text in obj.GetComponentsInChildren<TMP_Text>()) text.ForceMeshUpdate(true, true);
            camera.Render(); RenderTexture.active = target; pixels = new Texture2D(width, height, TextureFormat.RGBA32, false); pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0); pixels.Apply(); File.WriteAllBytes(Output + file, pixels.EncodeToPNG());
        }
        finally { RenderTexture.active = old; camera.targetTexture = null; target.Release(); UnityEngine.Object.DestroyImmediate(target); if (pixels != null) UnityEngine.Object.DestroyImmediate(pixels); UnityEngine.Object.DestroyImmediate(cameraObj); }
    }
    /// <summary>Finds one dynamic or static label on the authored hierarchy.</summary>
    private static TMP_Text Label(GameObject obj, string name) { return obj.GetComponentsInChildren<TMP_Text>(true).First(t => t.name == name); }
    /// <summary>Loads the isolated candidate prefab.</summary>
    private static GameObject Prefab() { return AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Prefabs/MidnightFieldJournalCanvas.prefab"); }
    /// <summary>Finds production card data by its display identity.</summary>
    private static CardDefinition Card(string name) { return AssetDatabase.FindAssets("t:CardDefinition", new[] { "Assets/Cards" }).Select(g => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).First(c => c.DisplayName == name); }
    /// <summary>Uses the actual authored coastal tier definitions.</summary>
    private static BiomeDefinition Biome() { return AssetDatabase.FindAssets("t:BiomeDefinition", new[] { "Assets/Biomes" }).Select(g => AssetDatabase.LoadAssetAtPath<BiomeDefinition>(AssetDatabase.GUIDToAssetPath(g))).First(b => b.BiomeId == "Coastal"); }
    /// <summary>Stops on the first failed presentation contract.</summary>
    private static void Require(bool condition, string message) { assertions++; if (!condition) throw new InvalidOperationException(message); }
}
