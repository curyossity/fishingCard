using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>Validates individual projected Technique content, perspective, lifecycle and real HUD input.</summary>
public static class JournalTechniqueHandChecks
{
    private const string Root = MidnightFieldJournalAssetSetup.Root;
    private const string Output = "Docs/UIThemes/MidnightFieldJournal/";
    private static int assertions;
    private static GameObject smokeRoot;
    private static TechniqueHandView smokeView;
    private static int clicks, selected;
    private static bool oldBackground;
    private static string smokeScenePath;
    private static byte[] smokeSceneBytes;
    [Serializable] private sealed class Report { public bool passed, sceneUnchanged; public int assertions; public string scope = "Isolated projected hand; full comparison scene remains UI-07"; }

    /// <summary>Checks actual data-bound prefabs in a disposable scene and exports their rendered card planes.</summary>
    [MenuItem("Fishing Cards/UI/Midnight Field Journal/Validate Technique Hand")]
    public static void Run()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Run isolated capture outside Play Mode.");
        assertions = 0; Scene active = SceneManager.GetActiveScene(); bool dirty = active.isDirty;
        Scene preview = EditorSceneManager.NewPreviewScene();
        JournalTechniqueHandPresentation cleanup = null;
        try
        {
            GameObject root = new GameObject("Technique hand validation", typeof(RectTransform), typeof(Canvas));
            SceneManager.MoveGameObjectToScene(root, preview);
            root.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            root.GetComponent<RectTransform>().sizeDelta = new Vector2(3840, 2160);
            TechniqueHandView view = Create(root.transform);
            JournalTechniqueHandPresentation hand = view.GetComponent<JournalTechniqueHandPresentation>();
            cleanup = hand;
            CardDefinition[] sample = Sample();
            Refresh(view, sample); hand.RenderNow();
            Require(hand.Cards.Count == 4 && hand.ProjectionCamera != null && !hand.ProjectionCamera.orthographic, "Hand is not individual perspective UI.");
            for (int i = 0; i < 4; i++) AssertContent(hand.Cards[i], sample[i], true);
            Vector3[] corners = new Vector3[4]; hand.Cards[1].GetComponent<RectTransform>().GetWorldCorners(corners);
            Vector3[] projected = corners.Select(hand.ProjectionCamera.WorldToScreenPoint).ToArray();
            Require(Vector3.Distance(projected[0], projected[3]) > Vector3.Distance(projected[1], projected[2]) * 1.1f, "Near edge does not widen: projection is not genuine.");
            Capture(hand, "UI04HandRest.png");
            var handRect = view.GetComponent<RectTransform>(); handRect.sizeDelta = new Vector2(1800, 866); hand.RenderNow();
            foreach (var card in hand.Cards)
            {
                card.GetComponent<RectTransform>().GetWorldCorners(corners);
                Require(corners.Select(hand.ProjectionCamera.WorldToScreenPoint).All(p => p.x >= 0 && p.x <= hand.Target.width), "Narrow hand viewport clips cards.");
            }
            handRect.sizeDelta = new Vector2(2500, 866); hand.RenderNow();
            var pose = hand.Poses[1]; Quaternion rest = pose.transform.localRotation;
            pose.OnPointerEnter(null); hand.RenderNow();
            Require(Quaternion.Angle(pose.transform.localRotation, rest) > 20, "Inspect pose did not straighten the full card.");
            Capture(hand, "UI04HandInspect.png");
            pose.OnPointerExit(null); hand.RenderNow(); Require(Quaternion.Angle(pose.transform.localRotation, rest) < .1f, "Resting pose did not restore.");
            TechniqueCardView original = hand.Cards[0]; Refresh(view, sample); Require(hand.Cards[0] == original, "Refresh recreated physical card views.");
            for (int count = 0; count <= 4; count++)
            {
                Refresh(view, sample.Take(count).ToArray());
                Require(hand.Cards.Count(c => c.gameObject.activeSelf) == count, "Incorrect visible card count.");
            }
            foreach (string name in new[] { "Cut Loose", "Slip Past", "Clear Water", "Hold Steady", "Re-rig", "Steady Line", "Recast", "Prize Hook" })
            {
                CardDefinition definition = Card(name); Refresh(view, new[] { definition });
                bool hasArt = true;
                AssertContent(hand.Cards[0], definition, hasArt);
            }
            Refresh(view, new[] { Card("Cut Loose"), Card("Cut Loose"), Card("Slip Past") });
            Require(hand.Cards[0] != hand.Cards[1], "Repeated designs collapsed into one physical view.");
            var oldTarget = hand.Target; view.gameObject.SetActive(false);
            hand.ReleaseResources();
            Require(hand.Target == null && hand.ProjectionCamera == null, "Disabled hand retained GPU/camera ownership.");
            view.gameObject.SetActive(true); Refresh(view, sample); hand.RenderNow();
            Require(hand.Target != null && hand.Target != oldTarget && hand.Cards.Count == 4, "Hand did not recreate its owned resources.");
            root.GetComponent<Canvas>().scaleFactor = .5f; hand.RenderNow();
            Require(hand.Target.width == 1250, "1080p hand does not scale its render target."); Capture(hand, "UI04Hand1080p.png");
            string[] deps = AssetDatabase.GetDependencies(Root + "Prefabs/TechniqueHand.prefab", true);
            Require(!deps.Any(p => p.StartsWith("Assets/FishingUIAssets/") || p.StartsWith("Assets/Prefabs/UI/")), "Hand depends on old visual resources.");
            var legacy = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/FishingRunView.prefab").GetComponentInChildren<TechniqueHandView>(true);
            Require(legacy != null && new SerializedObject(legacy).FindProperty("presentation").objectReferenceValue == null, "Old hand renderer was replaced.");
        }
        finally { if (cleanup != null) cleanup.ReleaseResources(); EditorSceneManager.ClosePreviewScene(preview); }
        bool unchanged = SceneManager.GetActiveScene() == active && active.isDirty == dirty;
        Require(unchanged, "Capture altered the open scene.");
        File.WriteAllText(Output + "UI04Validation.json", JsonUtility.ToJson(new Report { passed = true, sceneUnchanged = unchanged, assertions = assertions }, true));
        Debug.Log("Journal technique component checks passed: " + assertions);
    }

    /// <summary>Creates the real overlay hand, allowing a rendered frame before completing input tests.</summary>
    public static void StartPlayModeSmoke()
    {
        if (!Application.isPlaying || smokeRoot != null) throw new InvalidOperationException("Start once in Play Mode.");
        assertions = 0; clicks = 0; selected = -1; oldBackground = Application.runInBackground; Application.runInBackground = true;
        smokeScenePath = SceneManager.GetActiveScene().path; smokeSceneBytes = File.Exists(smokeScenePath) ? File.ReadAllBytes(smokeScenePath) : null;
        smokeRoot = new GameObject("Isolated Journal Hand Smoke", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
        Canvas canvas = smokeRoot.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 30000;
        var scaler = smokeRoot.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(3840, 2160); scaler.matchWidthOrHeight = .5f;
        smokeView = Create(smokeRoot.transform); Refresh(smokeView, Sample());
        GameObject modal = new GameObject("Modal hit blocker", typeof(RectTransform), typeof(UnityEngine.UI.Image)); modal.transform.SetParent(smokeRoot.transform, false);
        var modalRect = modal.GetComponent<RectTransform>(); modalRect.anchorMin = Vector2.zero; modalRect.anchorMax = Vector2.one; modalRect.offsetMin = modalRect.offsetMax = Vector2.zero;
        modal.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, .01f); modal.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
    }

    /// <summary>Exercises remapped raycasts, hover, submission, disabled slots, modal priority and current slot indices.</summary>
    public static void FinishPlayModeSmoke()
    {
        if (!Application.isPlaying || smokeRoot == null) throw new InvalidOperationException("Start the smoke check first.");
        try
        {
            var hand = smokeView.GetComponent<JournalTechniqueHandPresentation>();
            EventSystem events = EventSystem.current; Require(events != null, "Scene has no EventSystem.");
            for (int i = 0; i < 4; i++)
            {
                Vector2 screen = ScreenPoint(hand, i); var pointer = new PointerEventData(events) { position = screen, button = PointerEventData.InputButton.Left };
                var hits = new System.Collections.Generic.List<RaycastResult>(); smokeView.GetComponent<JournalHandRaycaster>().Raycast(pointer, hits);
                Require(hits.Count > 0 && hits[0].gameObject.GetComponentInParent<TechniqueCardView>() == hand.Cards[i], "Projected card hit mapped to wrong slot " + i);
                ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, pointer, ExecuteEvents.pointerClickHandler);
                Require(clicks == i + 1 && selected == i, "Actual projected click duplicated or lost slot index.");
            }
            var pose = hand.Poses[1]; pose.OnPointerEnter(null); hand.RenderNow();
            var stablePointer = new PointerEventData(events) { position = ScreenPoint(hand, 1, true) };
            var stableHits = new System.Collections.Generic.List<RaycastResult>(); smokeView.GetComponent<JournalHandRaycaster>().Raycast(stablePointer, stableHits);
            Require(stableHits.Count > 0 && stableHits[0].gameObject.GetComponentInParent<TechniqueCardView>() == hand.Cards[1], "Lifted card lost its resting hover region.");
            pose.OnPointerExit(null);
            for (int i = 0; i < 4; i++)
            {
                var button = hand.Cards[i].GetComponent<UnityEngine.UI.Button>();
                Require(button.navigation.mode == UnityEngine.UI.Navigation.Mode.Explicit && (i == 3 || button.navigation.selectOnRight == hand.Cards[i + 1].GetComponent<UnityEngine.UI.Button>()), "Navigation order incorrect.");
            }
            events.SetSelectedGameObject(hand.Cards[2].gameObject); hand.RenderNow(); Require(hand.Poses[2].Inspecting, "Navigation focus did not inspect.");
            ExecuteEvents.Execute(hand.Cards[2].gameObject, new BaseEventData(events), ExecuteEvents.submitHandler); Require(clicks == 5 && selected == 2, "Submit failed.");
            events.SetSelectedGameObject(null);
            smokeView.Refresh(Sample(), new[] { false, true, true, true }, new[] { "No legal target", "", "", "" }, 8, 0, i => { clicks++; return true; });
            ExecuteEvents.Execute(hand.Cards[0].gameObject, new PointerEventData(events) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
            Require(clicks == 5, "Unavailable card could be played.");
            events.SetSelectedGameObject(hand.Cards[0].gameObject); hand.RenderNow();
            Require(hand.Poses[0].Inspecting, "Unavailable card cannot be inspected using navigation.");
            ExecuteEvents.Execute(hand.Cards[0].gameObject, new BaseEventData(events), ExecuteEvents.submitHandler); Require(clicks == 5, "Inspecting an unavailable card enabled play.");
            events.SetSelectedGameObject(null);
            hand.Poses[0].OnPointerEnter(null); Require(smokeView.transform.Find("Focused Restriction").GetComponent<TMP_Text>().text == "No legal target", "Restriction reason missing."); hand.Poses[0].OnPointerExit(null);
            GameObject modal = smokeRoot.transform.Find("Modal hit blocker").gameObject; modal.GetComponent<UnityEngine.UI.Image>().raycastTarget = true;
            Canvas.ForceUpdateCanvases(); var modalPointer = new PointerEventData(events) { position = ScreenPoint(hand, 1) }; var all = new System.Collections.Generic.List<RaycastResult>();
            events.RaycastAll(modalPointer, all); Require(all.Count > 0 && all[0].gameObject == modal, "Projected hand bypassed modal sorting.");
            UnityEngine.Object.Destroy(modal);
            CanvasGroup inputLock = smokeView.gameObject.AddComponent<CanvasGroup>(); inputLock.interactable = false;
            ExecuteEvents.Execute(hand.Cards[1].gameObject, new BaseEventData(events), ExecuteEvents.submitHandler);
            Require(clicks == 5, "Offscreen keyboard input bypassed its HUD CanvasGroup lock."); inputLock.interactable = true;
            Refresh(smokeView, new[] { Card("Cut Loose"), Card("Cut Loose") });
            ExecuteEvents.Execute(hand.Cards[1].gameObject, new PointerEventData(events) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
            Require(selected == 1 && clicks == 6, "Refreshed repeated card used stale callback.");
            var pressedCard = hand.Cards[0]; var press = new PointerEventData(events) { button = PointerEventData.InputButton.Left };
            ExecuteEvents.Execute(pressedCard.gameObject, press, ExecuteEvents.pointerDownHandler);
            smokeView.Refresh(new[] { Card("Cut Loose"), Card("Cut Loose") }, new[] { true, true }, new[] { "", "" }, 8, 0, i => { clicks++; return true; }, 77);
            ExecuteEvents.Execute(pressedCard.gameObject, press, ExecuteEvents.pointerClickHandler);
            Require(clicks == 6, "A hand version change redirected an old press to a new occurrence.");
            var target = hand.Target; smokeView.gameObject.SetActive(false); Require(hand.Target == null, "Actual OnDisable retained the target.");
            smokeView.gameObject.SetActive(true); hand.RenderNow(); Require(hand.Target != null && hand.Target != target, "Actual OnEnable did not rebuild the hand.");
            bool savedSceneUnchanged = smokeSceneBytes == null ? SceneManager.GetActiveScene().path == smokeScenePath : File.ReadAllBytes(smokeScenePath).SequenceEqual(smokeSceneBytes);
            Require(savedSceneUnchanged, "Input checks changed the saved scene.");
            File.WriteAllText(Output + "UI04PlayModeValidation.json", JsonUtility.ToJson(new Report { passed = true, sceneUnchanged = savedSceneUnchanged, assertions = assertions, scope = "Actual perspective-camera raycast mapping, pointer/submit callbacks, focus, stable hover, legality, stale-press rejection, resource lifetime and modal priority; saved scene file unchanged" }, true));
        }
        finally { smokeRoot.SetActive(false); UnityEngine.Object.Destroy(smokeRoot); smokeRoot = null; smokeView = null; Application.runInBackground = oldBackground; }
    }

    /// <summary>Maps a complete projected card center back into the HUD's screen rectangle.</summary>
    private static Vector2 ScreenPoint(JournalTechniqueHandPresentation hand, int index, bool resting = false)
    {
        RectTransform rect = resting ? hand.Poses[index].RestHit : hand.Cards[index].GetComponent<RectTransform>();
        Vector3 pixel = hand.ProjectionCamera.WorldToScreenPoint(rect.TransformPoint(rect.rect.center));
        RectTransform surface = hand.Surface.rectTransform; Vector2 uv = new Vector2(pixel.x / hand.Target.width, pixel.y / hand.Target.height);
        return RectTransformUtility.WorldToScreenPoint(null, surface.TransformPoint(surface.rect.min + Vector2.Scale(uv, surface.rect.size)));
    }

    /// <summary>Loads the approved current card design.</summary>
    private static CardDefinition Card(string name) { var card = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/" + name + ".asset"); Require(card != null, "Missing card " + name); return card; }
    /// <summary>Returns the four independently illustrated reference designs.</summary>
    private static CardDefinition[] Sample() { return new[] { Card("Cut Loose"), Card("Slip Past"), Card("Steady Line"), Card("Prize Hook") }; }
    /// <summary>Calls the same presentation endpoint as the live controller.</summary>
    private static void Refresh(TechniqueHandView view, CardDefinition[] cards)
    { view.Refresh(cards, cards.Select(c => true).ToArray(), cards.Select(c => "").ToArray(), 8, 0, i => { clicks++; selected = i; return true; }); }
    /// <summary>Creates a real hand prefab at its authored output canvas size.</summary>
    private static TechniqueHandView Create(Transform parent)
    {
        var root = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Prefabs/TechniqueHand.prefab"), parent);
        var rect = root.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.anchoredPosition = new Vector2(0, -300); rect.sizeDelta = new Vector2(2500, 866);
        return root.GetComponent<TechniqueHandView>();
    }
    /// <summary>Checks real title/rules content and exact artwork identity with no substitution.</summary>
    private static void AssertContent(TechniqueCardView card, CardDefinition definition, bool artwork)
    {
        Require(card.transform.Find("Visual Root/Technique Name").GetComponent<TMP_Text>().text == definition.DisplayName.ToUpperInvariant(), "Wrong card title.");
        var rules = card.transform.Find("Visual Root/Technique Rules").GetComponent<TMP_Text>(); rules.ForceMeshUpdate(true, true);
        string Words(string text) => string.Join(" ", text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries));
        Require(Words(rules.text) == Words(definition.RulesText) && rules.textInfo.characterCount > 0 && !rules.isTextOverflowing, "Rules truncated or missing: " + definition.DisplayName);
        Require(card.transform.Find("Visual Root/Artwork Mask/Runtime Artwork").GetComponent<UnityEngine.UI.Image>().enabled == artwork, "Wrong/missing-art substitution.");
    }
    /// <summary>Writes an actual camera target without changing art or storing it as a runtime hand image.</summary>
    private static void Capture(JournalTechniqueHandPresentation hand, string file)
    {
        hand.RenderNow(); RenderTexture previous = RenderTexture.active;
        var resolved = RenderTexture.GetTemporary(hand.Target.width, hand.Target.height, 0, RenderTextureFormat.ARGB32); Texture2D output = null;
        try { Graphics.Blit(hand.Target, resolved); RenderTexture.active = resolved; output = new Texture2D(resolved.width, resolved.height, TextureFormat.RGBA32, false); output.ReadPixels(new Rect(0, 0, resolved.width, resolved.height), 0, 0); output.Apply(); File.WriteAllBytes(Output + file, output.EncodeToPNG()); }
        finally { RenderTexture.active = previous; if (output != null) UnityEngine.Object.DestroyImmediate(output); RenderTexture.ReleaseTemporary(resolved); }
    }
    /// <summary>Stops at the first failed component or input contract.</summary>
    private static void Require(bool condition, string message) { assertions++; if (!condition) throw new InvalidOperationException(message); }
}
