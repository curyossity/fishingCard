using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>Validates resolved Catch Chain binding, pooled input, scrolling and independent hardware in a disposable scene.</summary>
public static class JournalCatchChainChecks
{
    private const string Root = MidnightFieldJournalAssetSetup.Root;
    private const string Evidence = "Docs/UIThemes/MidnightFieldJournal/";
    private static int assertions;
    private static GameObject smokeRoot;
    private static CatchChainView smokeView;
    private static CatchChainRuntime smokeRuntime;
    private static Action<int> smokeCallback;
    private static int smokeClicks, smokeSelected;
    private static bool smokeRunInBackground;
    [Serializable] private sealed class Report
    {
        public bool passed, activeSceneUnchanged;
        public int assertions;
        public string scope = "Isolated real-state component checks and 4K/1080p captures; full playable-scene integration remains UI-07";
        public string[] captures = { "UI03CatchChain4K.png", "UI03CatchChain1080p.png", "UI03LongChain4K.png" };
    }

    /// <summary>Exercises actual Awake wiring, graphic hit testing, wheel scrolling and pooling in a running Editor.</summary>
    public static void RunPlayModeSmoke()
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Enter Play Mode before this smoke check.");
        assertions = 0;
        if (smokeRoot != null) throw new InvalidOperationException("Finish the existing smoke check first.");
        smokeRunInBackground = Application.runInBackground;
        Application.runInBackground = true;
        GameObject root = new GameObject("Isolated Journal Chain Smoke", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
        smokeRoot = root;
        try
        {
            Canvas canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 30000;
            var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(3840, 2160); scaler.matchWidthOrHeight = .5f;
            CatchChainView view = Instance<CatchChainView>("CatchChain", root.transform, 200, 170, 855, 1540);
            JournalCatchChainPresentation chain = view.GetComponent<JournalCatchChainPresentation>();
            CatchChainRuntime runtime = new CatchChainRuntime(); EffectResolver resolver = new EffectResolver();
            for (int i = 0; i < 12; i++) runtime.Add(Card("Sardine"), resolver);
            smokeClicks = 0; smokeSelected = -1; smokeCallback = i => { smokeClicks++; smokeSelected = i; };
            smokeView = view; smokeRuntime = runtime;
            Refresh(view, runtime, smokeCallback);
        }
        catch { UnityEngine.Object.Destroy(root); smokeRoot = null; Application.runInBackground = smokeRunInBackground; throw; }
    }

    /// <summary>Completes input checks after the canvas has rendered a frame and acquired actual graphic depths.</summary>
    public static void FinishPlayModeSmoke()
    {
        if (!Application.isPlaying || smokeRoot == null) throw new InvalidOperationException("Start the smoke check in Play Mode first.");
        GameObject root = smokeRoot; CatchChainView view = smokeView; CatchChainRuntime runtime = smokeRuntime;
        JournalCatchChainPresentation chain = view.GetComponent<JournalCatchChainPresentation>();
        Action<int> callback = smokeCallback;
        try
        {
            EventSystem events = EventSystem.current;
            Require(events != null, "Running scene requires an EventSystem.");
            CompactCatchCardView first = chain.Cards[0];
            RectTransform rect = first.GetComponent<RectTransform>();
            var pointer = new PointerEventData(events) { position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center)), button = PointerEventData.InputButton.Left };
            var hits = new System.Collections.Generic.List<RaycastResult>();
            root.GetComponent<UnityEngine.UI.GraphicRaycaster>().Raycast(pointer, hits);
            Require(hits.Any(hit => hit.gameObject.transform.IsChildOf(first.transform)), "Visible card cannot be raycast.");
            GameObject receiver = ExecuteEvents.ExecuteHierarchy(hits.First(hit => hit.gameObject.transform.IsChildOf(first.transform)).gameObject, pointer, ExecuteEvents.pointerClickHandler);
            Require(receiver == first.gameObject && smokeClicks == 1 && smokeSelected == 0, "Actual Awake/raycast click did not forward once.");
            for (int i = 0; i < 5; i++) Refresh(view, runtime, callback);
            Click(first, events); Require(smokeClicks == 2, "Repeated refresh duplicated actual runtime listeners.");
            var scroll = view.GetComponentInChildren<UnityEngine.UI.ScrollRect>();
            float before = scroll.content.anchoredPosition.y;
            ExecuteEvents.Execute(scroll.gameObject, new PointerEventData(events) { scrollDelta = new Vector2(0, -1) }, ExecuteEvents.scrollHandler);
            Require(scroll.content.anchoredPosition.y > before, "Wheel input failed to scroll the long chain.");
            float offset = scroll.content.anchoredPosition.y; Refresh(view, runtime, callback);
            Require(Mathf.Abs(offset - scroll.content.anchoredPosition.y) < .1f, "Runtime refresh lost the scroll offset.");
            var last = chain.Cards[11]; Click(last, events); Require(smokeSelected == 11 && smokeClicks == 3, "Callback used a stale pooled index.");
            view.Refresh(runtime.Catches.Take(1).ToArray(), null, 1, 8, -1, callback);
            Require(chain.Cards.Count(c => c.gameObject.activeSelf) == 1, "Runtime pool failed to shrink.");
            view.Refresh(runtime.Catches, runtime.ActiveEffectRecords, runtime.CurrentLineLoad, 8, -1, callback);
            Require(chain.Cards[11] == last && last.gameObject.activeSelf, "Runtime pool failed to reuse a reactivated view.");
            Click(last, events); Require(smokeSelected == 11 && smokeClicks == 4, "Reactivated view duplicated its handler.");
            File.WriteAllText(Evidence + "UI03PlayModeValidation.json", "{\"passed\":true,\"assertions\":" + assertions + ",\"scope\":\"Temporary chain in real Play Mode: Awake, raycast, click, wheel input, scroll retention, pooled reactivation; no full-run visual integration claim\"}");
            Debug.Log("Journal chain Play Mode smoke passed: " + assertions + " assertions.");
        }
        finally
        {
            root.SetActive(false); UnityEngine.Object.Destroy(root);
            smokeRoot = null; smokeView = null; smokeRuntime = null; smokeCallback = null;
            Application.runInBackground = smokeRunInBackground;
        }
    }

    /// <summary>Runs component checks without saving, replacing or installing anything into the open scene.</summary>
    [MenuItem("Fishing Cards/UI/Midnight Field Journal/Validate Catch Chain")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Run isolated checks outside Play Mode.");
        assertions = 0;
        Scene active = SceneManager.GetActiveScene(); string activePath = active.path; bool dirty = active.isDirty;
        Scene preview = EditorSceneManager.NewPreviewScene();
        try
        {
            GameObject canvasObject = new GameObject("Chain component check", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.GraphicRaycaster));
            SceneManager.MoveGameObjectToScene(canvasObject, preview);
            Canvas canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace;
            canvasObject.GetComponent<RectTransform>().sizeDelta = new Vector2(3840, 2160);
            GameObject cameraObject = new GameObject("Chain check camera", typeof(Camera)); SceneManager.MoveGameObjectToScene(cameraObject, preview);
            Camera camera = cameraObject.GetComponent<Camera>(); camera.scene = preview; camera.orthographic = true; camera.orthographicSize = 1080;
            camera.transform.position = new Vector3(0, 0, -100); camera.nearClipPlane = .1f; camera.farClipPlane = 2000;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color32(17, 30, 37, 255); canvas.worldCamera = camera;
            GameObject eventsObject = new GameObject("Chain check EventSystem", typeof(EventSystem)); SceneManager.MoveGameObjectToScene(eventsObject, preview);
            EventSystem events = eventsObject.GetComponent<EventSystem>();
            CatchChainView view = Instance<CatchChainView>("CatchChain", canvas.transform, 200, 170, 855, 1540);
            JournalCatchChainPresentation chain = view.GetComponent<JournalCatchChainPresentation>();
            LineLoadMeterView meter = Instance<LineLoadMeterView>("LineLoadMeter", canvas.transform, 245, 1760, 760, 150);
            SerializedObject data = new SerializedObject(chain); data.FindProperty("loadMeter").objectReferenceValue = meter; data.ApplyModifiedPropertiesWithoutUndo();
            var scroll = view.GetComponentInChildren<UnityEngine.UI.ScrollRect>();
            Require(scroll != null && scroll.viewport.GetComponent<UnityEngine.UI.RectMask2D>() != null, "Chain needs a real masked ScrollRect.");
            Require(scroll.content.parent == scroll.viewport && !scroll.horizontal && scroll.vertical, "Scroll hierarchy or axes invalid.");
            CatchChainRuntime runtime = new CatchChainRuntime(); EffectResolver resolver = new EffectResolver();
            runtime.Add(Card("Sardine"), resolver);
            runtime.Add(Card("Clinging Crab"), resolver);
            runtime.Add(Card("Glass Minnow"), resolver, CatchAttachmentRole.Bait);
            int selected = -1, callbacks = 0;
            Action<int> select = i => { selected = i; callbacks++; };
            Refresh(view, runtime, select);
            Require(chain.Cards.Count == 3 && chain.Links.Count(l => l.gameObject.activeSelf) == 4, "Three attachments need four independent links.");
            AssertFields(chain.Cards[0], "Sardine", "1", "7", false);
            AssertFields(chain.Cards[1], "Clinging Crab", "2", "14", false);
            AssertFields(chain.Cards[2], "Glass Minnow", "1", "0", true);
            Require(Mathf.Abs(chain.Cards[1].GetComponent<RectTransform>().anchoredPosition.y - chain.Cards[0].GetComponent<RectTransform>().anchoredPosition.y + 450) < .1f, "Reference pitch changed.");
            Rect crabBounds = chain.Cards[1].transform.Find("Artwork Mask").GetComponent<RectTransform>().rect;
            Require(Mathf.Abs(crabBounds.width - 440) < .1f && Mathf.Abs(crabBounds.height - 250) < .1f, "Crab framing differs from approved crop.");
            foreach (var link in chain.Links)
                Require(!link.raycastTarget && link.color == Color.white && link.sprite.name == "BrassConnector", "Hardware is interactive, tinted or outdated.");
            ValidateEyelets(chain);
            // Edit-mode prefab instances do not receive Awake; initialize just that listener explicitly for callback checks.
            foreach (var card in chain.Cards) card.SendMessage("Awake");
            CompactCatchCardView first = chain.Cards[0];
            for (int i = 0; i < 4; i++) Refresh(view, runtime, select);
            Require(chain.Cards[0] == first, "Refresh replaced a physical card view.");
            Click(first, events); Require(selected == 0 && callbacks == 1, "Refresh duplicated or lost its callback.");
            first.SetState(CompactCatchCardState.Disabled); Click(first, events); Require(callbacks == 1, "Disabled card forwarded input.");
            first.SetState(CompactCatchCardState.ReleaseCandidate);
            Require(first.transform.Find("Legal Target Edge").gameObject.activeSelf, "Legal-target feedback absent.");
            first.SetState(CompactCatchCardState.Normal);
            ExecuteEvents.Execute(first.gameObject, new BaseEventData(events), ExecuteEvents.selectHandler);
            Require(first.transform.Find("Hover And Focus Edge").gameObject.activeSelf, "Navigation focus absent.");
            ExecuteEvents.Execute(first.gameObject, new BaseEventData(events), ExecuteEvents.deselectHandler);
            Require(!first.transform.Find("Hover And Focus Edge").gameObject.activeSelf, "Navigation focus did not clear.");
            Capture(canvas, camera, 3840, 2160, "UI03CatchChain4K.png");
            Capture(canvas, camera, 1920, 1080, "UI03CatchChain1080p.png");
            runtime.Catches[0].AddPermanentModifiers(2, 9992); runtime.Recalculate(resolver);
            Refresh(view, runtime, select);
            AssertFields(first, "Sardine", "3", "9999", false);
            runtime.Catches[0].SetAttachmentRole(CatchAttachmentRole.Bait); runtime.Recalculate(resolver);
            Refresh(view, runtime, select); AssertFields(first, "Sardine", "3", "0", true);
            runtime.Catches[0].SetAttachmentRole(CatchAttachmentRole.Catch); runtime.Recalculate(resolver);
            Refresh(view, runtime, select); AssertFields(first, "Sardine", "3", "9999", false);
            runtime.Catches[0].AddPermanentModifiers(-3, 0); runtime.Recalculate(resolver);
            Refresh(view, runtime, select); AssertFields(first, "Sardine", "0", "9999", false);
            for (int i = 0; i < 17; i++) runtime.Add(Card("Sardine"), resolver);
            Refresh(view, runtime, select); Canvas.ForceUpdateCanvases();
            Require(chain.Cards.Count == 20 && chain.Links.Count(l => l.gameObject.activeSelf) == 21, "Long chain truncated.");
            Require(scroll.content.rect.height > scroll.viewport.rect.height * 4, "Long content cannot scroll.");
            scroll.content.anchoredPosition = new Vector2(0, 1700);
            Refresh(view, runtime, select);
            Require(Mathf.Abs(scroll.content.anchoredPosition.y - 1700) < .1f, "Stat refresh reset scroll position.");
            Capture(canvas, camera, 3840, 2160, "UI03LongChain4K.png");
            var copy = runtime.Catches.Take(1).ToArray(); view.Refresh(copy, null, 3, 0, -1, select);
            Require(chain.Cards.Count(c => c.gameObject.activeSelf) == 1 && chain.Links.Count(l => l.gameObject.activeSelf) == 2, "Shrinking left stale cards or links.");
            Require(Mathf.Abs(scroll.content.anchoredPosition.y) < .1f, "Shrinking failed to clamp scroll.");
            view.Refresh(null, null, 0, 0, -1, select);
            Require(chain.Cards.All(c => !c.gameObject.activeSelf) && chain.Links.All(l => !l.gameObject.activeSelf), "Empty chain retained content.");
            view.Refresh(new[] { new CardInstance(99, Card("Garfish")) }, null, 1, 8, -1, select);
            Require(!first.transform.Find("Artwork Mask/Runtime Artwork").GetComponent<UnityEngine.UI.Image>().enabled, "Missing art retained another species.");
            meter.SetLoad(40, 32); Canvas.ForceUpdateCanvases();
            Require(meter.transform.Find("Pips").GetComponentsInChildren<UnityEngine.UI.Image>().Length == 32, "Capacity change left missing pips.");
            foreach (var pip in meter.transform.Find("Pips").GetComponentsInChildren<UnityEngine.UI.Image>()) Require(pip.preserveAspect, "Load pip stretched.");
            string[] dependencies = AssetDatabase.GetDependencies(Root + "Prefabs/CatchChain.prefab", true);
            Require(!dependencies.Any(p => p.StartsWith("Assets/FishingUIAssets/") || p.StartsWith("Assets/Prefabs/UI/")), "Chain depends on old visual resources.");
            var legacy = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/FishingRunView.prefab").GetComponentInChildren<CatchChainView>(true);
            Require(legacy != null && new SerializedObject(legacy).FindProperty("presentation").objectReferenceValue == null, "Old view acquired the new presenter.");
            CompactCatchCardView oldCard = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<CompactCatchCardView>("Assets/Prefabs/UI/Components/CompactCatchCard.prefab"), canvas.transform);
            oldCard.SetCard(new CardInstance(100, Card("Glass Minnow"), null, CatchAttachmentRole.Bait));
            Require(oldCard.transform.Find("Bait Marker").gameObject.activeSelf, "Existing runtime Bait badge fallback regressed.");
            SerializedObject oldData = new SerializedObject(oldCard);
            Require(((TMP_Text)oldData.FindProperty("nameText").objectReferenceValue).text == "GLASS MINNOW", "Legacy uppercase default changed.");
            Require(((TMP_Text)oldData.FindProperty("valueText").objectReferenceValue).text == "0", "Legacy Bait Value regressed.");
            oldCard.SetState(CompactCatchCardState.Disabled);
            Require(!oldCard.GetComponent<UnityEngine.UI.Button>().interactable, "Legacy disabled input regressed.");
        }
        finally { EditorSceneManager.ClosePreviewScene(preview); }
        bool unchanged = SceneManager.GetActiveScene().path == activePath && SceneManager.GetActiveScene().isDirty == dirty;
        Require(unchanged, "Checks changed the open scene.");
        File.WriteAllText(Evidence + "UI03Validation.json", JsonUtility.ToJson(new Report { passed = true, activeSceneUnchanged = unchanged, assertions = assertions }, true));
        Debug.Log("Journal Catch Chain checks passed: " + assertions + " assertions.");
    }

    /// <summary>Uses the same resolved-state entry point as FishingRunController.</summary>
    private static void Refresh(CatchChainView view, CatchChainRuntime runtime, Action<int> select)
    { view.Refresh(runtime.Catches, runtime.ActiveEffectRecords, runtime.CurrentLineLoad, 8, -1, select); Canvas.ForceUpdateCanvases(); }

    /// <summary>Loads existing authored cards without changing their definitions.</summary>
    private static CardDefinition Card(string name)
    { CardDefinition card = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/" + name + ".asset"); Require(card != null, "Missing card " + name); return card; }

    /// <summary>Checks visible final values and explicit role styling, not base-value comparisons.</summary>
    private static void AssertFields(CompactCatchCardView card, string name, string weight, string value, bool bait)
    {
        Require(card.transform.Find("Creature Name").GetComponent<TMP_Text>().text == name, "Wrong name.");
        foreach (var pair in new[] { new[] { "Weight Value", weight }, new[] { "Value Value", value } })
        {
            TMP_Text text = card.transform.Find(pair[0]).GetComponent<TMP_Text>(); text.ForceMeshUpdate(true, true);
            Require(text.text == pair[1] && text.textInfo.characterCount == pair[1].Length && text.textInfo.meshInfo.Any(m => m.vertexCount > 0), "Stat glyphs absent: " + pair[0]);
        }
        Require(card.transform.Find("Bait Marker").gameObject.activeSelf == bait && card.transform.Find("Bait Cyan Edge").gameObject.activeSelf == bait, "Wrong role styling.");
    }

    /// <summary>Checks the connector's measured eyelet locations against adjacent card edges.</summary>
    private static void ValidateEyelets(JournalCatchChainPresentation chain)
    {
        RectTransform a = chain.Cards[0].GetComponent<RectTransform>(), b = chain.Cards[1].GetComponent<RectTransform>();
        RectTransform link = chain.Links[1].rectTransform;
        Require(Mathf.Abs(-link.anchoredPosition.y + link.rect.height * .151f - (-a.anchoredPosition.y + a.rect.height - 14)) < .1f, "Upper eyelet detached.");
        Require(Mathf.Abs(-link.anchoredPosition.y + link.rect.height * .845f - (-b.anchoredPosition.y + 14)) < .1f, "Lower eyelet detached.");
    }

    /// <summary>Delivers a real Button pointer event rather than directly invoking the controller delegate.</summary>
    private static void Click(CompactCatchCardView card, EventSystem events)
    { ExecuteEvents.Execute(card.gameObject, new PointerEventData(events) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler); }

    /// <summary>Places a real prefab in the isolated preview canvas.</summary>
    private static T Instance<T>(string name, Transform parent, float x, float y, float w, float h) where T : Component
    {
        GameObject obj = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Prefabs/" + name + ".prefab"), parent);
        RectTransform rect = obj.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1); rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(w, h);
        return obj.GetComponent<T>();
    }

    /// <summary>Captures actual component pixels at a chosen output size without saving a scene.</summary>
    private static void Capture(Canvas canvas, Camera camera, int width, int height, string file)
    {
        RenderTexture target = new RenderTexture(width, height, 24); Texture2D image = null; RenderTexture previous = RenderTexture.active;
        try
        {
            Canvas.ForceUpdateCanvases(); foreach (TMP_Text text in canvas.GetComponentsInChildren<TMP_Text>()) text.ForceMeshUpdate(true, true);
            camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
            image = new Texture2D(width, height, TextureFormat.RGB24, false); image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply();
            File.WriteAllBytes(Evidence + file, image.EncodeToPNG());
        }
        finally { camera.targetTexture = null; RenderTexture.active = previous; if (image != null) UnityEngine.Object.DestroyImmediate(image); target.Release(); UnityEngine.Object.DestroyImmediate(target); }
    }

    /// <summary>Stops at the first failed contract with a useful Editor error.</summary>
    private static void Require(bool condition, string message) { assertions++; if (!condition) throw new InvalidOperationException(message); }
}
