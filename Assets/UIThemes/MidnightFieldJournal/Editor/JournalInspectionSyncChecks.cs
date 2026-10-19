using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>Verifies the current tooltip and variable-count fan using real prefabs, runtime instances and projection.</summary>
public static class JournalInspectionSyncChecks
{
    private const string Root = MidnightFieldJournalAssetSetup.Root;
    private const string Output = "Docs/UIThemes/MidnightFieldJournal/Sync003/";
    private static int assertions;
    private static GameObject smokeRoot;
    private static JournalCatchChainPresentation smokeChain;
    private static CardInstance[] smokeAttachments;
    private static bool previousBackground;
    private static TechniqueHandView largeView;
    private static int largeCount;
    private static readonly System.Collections.Generic.List<int> largeClicks = new System.Collections.Generic.List<int>();

    /// <summary>Checks live content, empty effects, runtime tags and bounded projected hands beyond the current deck limit.</summary>
    public static void Run()
    {
        assertions = 0; Directory.CreateDirectory(Output);
        Scene active = SceneManager.GetActiveScene(); bool dirty = active.isDirty;
        Scene preview = EditorSceneManager.NewPreviewScene(); JournalTechniqueHandPresentation hand = null;
        try
        {
            var root = new GameObject("Inspection sync validation", typeof(RectTransform), typeof(Canvas)); SceneManager.MoveGameObjectToScene(root, preview);
            root.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace; root.GetComponent<RectTransform>().sizeDelta = new Vector2(3840, 2160);
            var tooltip = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<JournalCatchTooltip>(Root + "Prefabs/CatchTooltip.prefab"), root.transform);
            tooltip.transform.localPosition = Vector3.zero;
            var crab = new CardInstance(1, Card("Clinging Crab")); tooltip.Bind(crab);
            Require(tooltip.DisplayedAbility == "Iron Grip" && tooltip.DisplayedDescription.Contains("deliberately"), "Wrong Catch effect.");
            Require(!tooltip.GetComponentsInChildren<Transform>(true).Any(t => t.name.ToLowerInvariant().Contains("pointer")), "Obsolete pointer present.");
            Require(tooltip.GetComponentsInChildren<UnityEngine.UI.Graphic>(true).All(g => !g.raycastTarget), "Tooltip intercepts input.");
            CaptureTooltip(root, tooltip, preview, "TooltipCrab4K.png");
            var minnow = new CardInstance(2, Card("Glass Minnow"), null, CatchAttachmentRole.Bait); minnow.GrantTag("Elusive"); tooltip.Bind(minnow);
            Require(tooltip.DisplayedAbility == "Shoal Signal" && tooltip.DisplayedDescription.Contains("Schooling"), "Wrong Bait effect.");
            Require(tooltip.GetComponentsInChildren<TMP_Text>().Any(t => t.text == "Elusive"), "Effective granted tag missing.");
            CaptureTooltip(root, tooltip, preview, "TooltipBait4K.png");
            minnow.SetAttachmentRole(CatchAttachmentRole.Catch); tooltip.Bind(minnow);
            Require(tooltip.DisplayedAbility == "No effect" && tooltip.DisplayedDescription == "", "Effectless Catch leaked Bait rules.");
            tooltip.gameObject.SetActive(false);
            var view = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<TechniqueHandView>(Root + "Prefabs/TechniqueHand.prefab"), root.transform);
            var handRect = view.GetComponent<RectTransform>(); handRect.sizeDelta = new Vector2(2500, 866); hand = view.GetComponent<JournalTechniqueHandPresentation>();
            foreach (int count in new[] { 0, 1, 2, 3, 4, 5, 6, 8, 12, 20 })
            {
                var cards = Enumerable.Range(0, count).Select(i => Card(new[] { "Cut Loose", "Slip Past", "Steady Line", "Prize Hook" }[i % 4])).ToArray();
                view.Refresh(cards, cards.Select(c => true).ToArray(), cards.Select(c => "").ToArray(), 8, 0, i => true, count);
                hand.RenderNow(); Require(hand.Cards.Count(c => c.gameObject.activeSelf) == count, "Wrong physical hand count " + count);
                for (int i = 0; i < count; i++)
                {
                    var rect = hand.Cards[i].GetComponent<RectTransform>(); var corners = new Vector3[4]; rect.GetWorldCorners(corners);
                    var projected = corners.Select(hand.ProjectionCamera.WorldToScreenPoint).ToArray();
                    Require(projected.All(p => p.x > 0 && p.x < hand.Target.width && p.y > 0 && p.y < hand.Target.height), "Fan clips at count " + count);
                    Require(Vector3.Distance(rect.localPosition, hand.Poses[i].RestHit.localPosition) < .01f, "Stable hit plane detached.");
                    Require(Mathf.Abs(rect.localPosition.y) < 80, "Arc grows with count.");
                    if (i > 0) Require(rect.localPosition.x > hand.Cards[i - 1].transform.localPosition.x, "Physical hand order reversed.");
                }
                if (new[] { 1, 2, 3, 4, 6, 8, 12 }.Contains(count)) Capture(hand.Target, "Fan" + count + "Cards4K.png");
                if (count == 4)
                {
                    var a = hand.Cards[0].GetComponent<RectTransform>(); var b = hand.Cards[1].GetComponent<RectTransform>();
                    Require(Mathf.Abs(b.localPosition.y + 17) < .01f && Mathf.Abs(a.localPosition.y + 67) < .01f, "Approved heights changed.");
                    Require(Top(hand, a) < Top(hand, b), "Outer cards project above the middle pair.");
                    var pose = hand.Poses[1]; var rest = pose.transform.localRotation; pose.OnPointerEnter(null); hand.RenderNow();
                    Capture(hand.Target, "FanInspect4K.png"); pose.OnPointerExit(null); hand.RenderNow();
                    Require(Quaternion.Angle(rest, pose.transform.localRotation) < .01f, "Inspect pose did not restore fan.");
                }
            }
            for (int count = 1; count <= 64; count++)
                for (int i = 0; i < count; i++)
                {
                    JournalTechniqueFan.Evaluate(i, count, 500, 40, out var a, out var ar);
                    JournalTechniqueFan.Evaluate(count - i - 1, count, 500, 40, out var b, out var br);
                    Require(Mathf.Abs(a.x + b.x) < .01f && Mathf.Abs(a.y - b.y) < .01f && Mathf.Abs(a.x) <= 750.01f && a.y > -80, "Asymmetric/unbounded fan count " + count);
                }
        }
        finally { if (hand != null) hand.ReleaseResources(); EditorSceneManager.ClosePreviewScene(preview); }
        Require(SceneManager.GetActiveScene() == active && active.isDirty == dirty, "Isolated checks changed open scene.");
        File.WriteAllText(Output + "ComponentVerification.json", "{\"passed\":true,\"assertions\":" + assertions + ",\"renderedHandCounts\":[0,1,2,3,4,5,6,8,12,20],\"symmetricHandCounts\":64}");
        Debug.Log("Inspection/fan checks passed: " + assertions);
    }

    /// <summary>Begins actual rendered-frame hover/focus and scroll/lock lifecycle verification.</summary>
    public static void StartSmoke()
    {
        if (!Application.isPlaying || smokeRoot != null) throw new InvalidOperationException("Start once in Play Mode.");
        previousBackground = Application.runInBackground; Application.runInBackground = true;
        smokeRoot = new GameObject("Journal inspection smoke", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(CanvasGroup));
        smokeRoot.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay; smokeRoot.GetComponent<Canvas>().sortingOrder = 40000;
        var scaler = smokeRoot.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(3840, 2160);
        smokeChain = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<JournalCatchChainPresentation>(Root + "Prefabs/CatchChain.prefab"), smokeRoot.transform);
        var rect = smokeChain.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1); rect.anchoredPosition = new Vector2(100, -150);
        smokeAttachments = new[] { new CardInstance(91, Card("Clinging Crab")), new CardInstance(92, Card("Glass Minnow"), null, CatchAttachmentRole.Bait) };
        smokeChain.Refresh(smokeAttachments, Array.Empty<ActiveCatchEffectRecord>(), 3, 12, -1, i => { }); Canvas.ForceUpdateCanvases();
        smokeChain.Cards[0].GetComponent<JournalCatchInspectionTrigger>().OnSelect(new BaseEventData(EventSystem.current));
    }

    /// <summary>Checks the tooltip after a real frame, then arms scrolling dismissal for the next frame.</summary>
    public static void CheckSmokeVisible()
    {
        var tooltip = smokeRoot.GetComponentInChildren<JournalCatchTooltip>(); Require(tooltip != null && tooltip.DisplayedAbility == "Iron Grip", "Navigation tooltip did not open.");
        smokeAttachments[0].GrantTag("Elusive"); smokeChain.Refresh(smokeAttachments, Array.Empty<ActiveCatchEffectRecord>(), 3, 12, -1, i => { });
        Require(tooltip.GetComponentsInChildren<TMP_Text>().Any(t => t.text == "Elusive"), "Visible tooltip did not refresh live tags.");
        var scroll = smokeChain.GetComponentInChildren<UnityEngine.UI.ScrollRect>(); scroll.content.anchoredPosition += new Vector2(0, 10);
    }

    /// <summary>Verifies dismissal, immediate focus reopening and parent-lock dismissal.</summary>
    public static void CheckSmokeScroll()
    {
        Require(smokeRoot.GetComponentInChildren<JournalCatchTooltip>() == null, "Tooltip survived scrolling.");
        smokeChain.Cards[1].GetComponent<JournalCatchInspectionTrigger>().OnSelect(new BaseEventData(EventSystem.current));
    }

    /// <summary>Arms a real parent CanvasGroup lock after verifying a different active role.</summary>
    public static void CheckSmokeBait()
    {
        Require(smokeRoot.GetComponentInChildren<JournalCatchTooltip>()?.DisplayedAbility == "Shoal Signal", "Focus did not rebind to Bait.");
        smokeRoot.GetComponent<CanvasGroup>().interactable = false;
    }

    /// <summary>Restores the prior Editor ticking setting and disposes all ephemeral smoke fixtures.</summary>
    public static void FinishSmoke()
    {
        try { Require(smokeRoot.GetComponentInChildren<JournalCatchTooltip>() == null, "Tooltip survived input lock."); File.WriteAllText(Output + "PlayModeTooltip.json", "{\"passed\":true,\"focus\":true,\"liveTags\":true,\"scrollDismissal\":true,\"roleRebind\":true,\"inputLock\":true}"); }
        finally { UnityEngine.Object.Destroy(smokeRoot); smokeRoot = null; smokeChain = null; Application.runInBackground = previousBackground; }
    }

    /// <summary>Creates a rendered larger-hand fixture so exposed card regions can be tested through the real raycaster.</summary>
    public static void StartLargeInput(int count)
    {
        if (!Application.isPlaying || smokeRoot != null) throw new InvalidOperationException("Start once in Play Mode.");
        previousBackground = Application.runInBackground; Application.runInBackground = true; largeCount = count; largeClicks.Clear();
        smokeRoot = new GameObject("Large hand input", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler));
        smokeRoot.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay; smokeRoot.GetComponent<Canvas>().sortingOrder = 40000;
        var scaler = smokeRoot.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(3840, 2160);
        largeView = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<TechniqueHandView>(Root + "Prefabs/TechniqueHand.prefab"), smokeRoot.transform);
        var cards = Enumerable.Range(0, count).Select(i => Card("Cut Loose")).ToArray();
        largeView.Refresh(cards, cards.Select(c => true).ToArray(), cards.Select(c => "").ToArray(), 0, 0, i => { largeClicks.Add(i); return true; }, count);
    }

    /// <summary>Finds an exposed projected region for every occurrence and clicks it through the guarded real button.</summary>
    public static void FinishLargeInput()
    {
        try
        {
            var hand = largeView.GetComponent<JournalTechniqueHandPresentation>();
            for (int i = 0; i < largeCount; i++)
            {
                EventSystem.current.SetSelectedGameObject(null); foreach (var pose in hand.Poses) pose.ResetInspection(); hand.RenderNow(); Canvas.ForceUpdateCanvases();
                var card = hand.Cards[i]; var rect = card.GetComponent<RectTransform>(); bool found = false;
                foreach (float u in new[] { .05f, .1f, .2f, .4f, .6f, .8f, .95f })
                {
                    if (found) break;
                    foreach (float v in new[] { .2f, .4f, .6f, .8f })
                    {
                        var local = rect.rect.min + Vector2.Scale(rect.rect.size, new Vector2(u, v));
                        var pixel = hand.ProjectionCamera.WorldToScreenPoint(rect.TransformPoint(local));
                        var pointer = new PointerEventData(EventSystem.current) { position = pixel, button = PointerEventData.InputButton.Left };
                        var hits = new System.Collections.Generic.List<RaycastResult>(); hand.RaycastProjected(pointer, pixel, hits);
                        if (hits.Count == 0 || hits[0].gameObject.GetComponentInParent<TechniqueCardView>() != card) continue;
                        ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, pointer, ExecuteEvents.pointerDownHandler);
                        ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, pointer, ExecuteEvents.pointerClickHandler);
                        Require(largeClicks.Count == i + 1 && largeClicks[i] == i, "Large-hand click selected wrong occurrence."); found = true; break;
                    }
                }
                Require(found, "No exposed input region for occurrence " + i + " / " + largeCount);
            }
            File.WriteAllText(Output + "Hand" + largeCount + "Input.json", "{\"passed\":true,\"count\":" + largeCount + ",\"allOccurrencesRaycastAndClicked\":true}");
        }
        finally { UnityEngine.Object.Destroy(smokeRoot); smokeRoot = null; largeView = null; Application.runInBackground = previousBackground; }
    }

    /// <summary>Measures the projected far edge in camera coordinates, including card rotation.</summary>
    private static float Top(JournalTechniqueHandPresentation hand, RectTransform card) { var p = new Vector3[4]; card.GetWorldCorners(p); return p.Select(hand.ProjectionCamera.WorldToScreenPoint).Max(v => v.y); }
    /// <summary>Loads existing data without changing any authored gameplay field.</summary>
    private static CardDefinition Card(string name) { var c = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/" + name + ".asset"); if (c == null) throw new InvalidOperationException(name); return c; }
    /// <summary>Renders the native tooltip at its actual 4K footprint in a disposable preview scene.</summary>
    private static void CaptureTooltip(GameObject root, JournalCatchTooltip tooltip, Scene scene, string file)
    {
        Canvas.ForceUpdateCanvases(); var o = new GameObject("Tooltip capture camera", typeof(Camera)); SceneManager.MoveGameObjectToScene(o, scene);
        var camera = o.GetComponent<Camera>(); camera.scene = scene; camera.orthographic = true; camera.orthographicSize = 320; camera.transform.position = new Vector3(0, 0, -1000); camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color32(15, 27, 33, 255);
        var target = new RenderTexture(900, 640, 24); camera.targetTexture = target; camera.Render(); Capture(target, file); camera.targetTexture = null; target.Release(); UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(o);
    }
    /// <summary>Saves a rendered view only as review evidence, never as a runtime asset.</summary>
    private static void Capture(RenderTexture source, string file)
    {
        var previous = RenderTexture.active; var resolved = RenderTexture.GetTemporary(source.width, source.height, 0); Texture2D tex = null;
        try { Graphics.Blit(source, resolved); RenderTexture.active = resolved; tex = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false); tex.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0); tex.Apply(); File.WriteAllBytes(Output + file, tex.EncodeToPNG()); }
        finally { RenderTexture.active = previous; RenderTexture.ReleaseTemporary(resolved); if (tex != null) UnityEngine.Object.DestroyImmediate(tex); }
    }
    /// <summary>Stops verification at a failed visual or lifecycle contract.</summary>
    private static void Require(bool condition, string message) { assertions++; if (!condition) throw new InvalidOperationException(message); }
}
