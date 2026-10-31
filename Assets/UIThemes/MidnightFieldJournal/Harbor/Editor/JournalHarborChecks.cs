using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>Checks native source quality, actual hub rendering and the live hub-to-expedition loop.</summary>
public static class JournalHarborChecks
{
    private static string Output = "Docs/Validation/DiscoveryJournal/";
    private static int assertions;
    public static string Status { get; private set; } = "Not started";

    /// <summary>Checks the installed scene/prefab and renders each actual Unity page at 4K.</summary>
    public static void ValidateAndCapture(string output = "Docs/Validation/DiscoveryJournal/")
    {
        Output = output;
        if (Application.isPlaying) throw new InvalidOperationException("Validate authored assets outside Play Mode.");
        Directory.CreateDirectory(Output); assertions = 0;
        var scene = SceneManager.GetActiveScene();
        Require(scene.path == JournalComparisonSceneBuilder.ScenePath && !scene.isDirty, "Expected clean integrated scene.");
        var installed = UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>();
        Require(installed != null && installed.Controller != null, "Scene hub/controller binding missing.");
        Require(!new SerializedObject(installed.Controller).FindProperty("startRunOnAwake").boolValue, "A hidden run would start before Set Sail.");
        Require(scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<EventSystem>(true)).Count() == 1, "Expected one EventSystem.");
        foreach (string page in new[] { "harbor", "shop", "collection" })
        {
            string path = JournalHarborBuilder.Root + "Art/" + page + "Frames.png";
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path); var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            Require(texture.width == 3840 && texture.height == 2160, "4K frame was reduced: " + page);
            Require(importer.textureCompression == TextureImporterCompression.Uncompressed && !importer.mipmapEnabled && importer.maxTextureSize >= 3840, "Lossy frame import: " + page);
        }
        Scene preview = EditorSceneManager.NewPreviewScene();
        GameObject root = null, cameraObject = null;
        try
        {
            root = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(JournalHarborBuilder.PrefabPath)); SceneManager.MoveGameObjectToScene(root, preview);
            var canvas = root.GetComponent<Canvas>(); var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>();
            Require(scaler.referenceResolution == new Vector2(3840, 2160), "Hub reference resolution changed.");
            var stage = root.transform.Find("Hub Content/16x9 Design Stage"); Require(stage != null, "Letterboxed stage missing.");
            cameraObject = new GameObject("Harbor capture camera", typeof(Camera)); SceneManager.MoveGameObjectToScene(cameraObject, preview);
            var camera = cameraObject.GetComponent<Camera>(); camera.scene = preview; camera.orthographic = true; camera.orthographicSize = 1080;
            camera.transform.position = new Vector3(0, 0, -100); camera.nearClipPlane = .1f; camera.farClipPlane = 2000;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color32(16, 38, 46, 255);
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 10;
            for (int i = 0; i < stage.childCount; i++)
            {
                for (int j = 0; j < stage.childCount; j++) stage.GetChild(j).gameObject.SetActive(i == j);
                var page = stage.GetChild(i);
                foreach (var button in page.GetComponentsInChildren<UnityEngine.UI.Button>())
                    if (button.interactable) Require(button.onClick.GetPersistentEventCount() == 1, "Unwired route: " + button.name);
                Capture(camera, root, 3840, 2160, page.name + "-Unity4K.png");
                Capture(camera, root, 1920, 1080, page.name + "-Unity1080p.png");
            }
        }
        finally
        {
            if (root != null) UnityEngine.Object.DestroyImmediate(root);
            if (cameraObject != null) UnityEngine.Object.DestroyImmediate(cameraObject);
            EditorSceneManager.ClosePreviewScene(preview);
        }
        Require(!scene.isDirty, "Capture dirtied the active scene.");
        File.WriteAllText(Output + "ComponentVerification.json", "{\"passed\":true,\"assertions\":" + assertions + "}");
    }

    /// <summary>Renders native UI without replacing it with a precomposed screenshot.</summary>
    private static void Capture(Camera camera, GameObject root, int width, int height, string name)
    {
        var target = new RenderTexture(width, height, 24); var old = RenderTexture.active; Texture2D texture = null;
        try
        {
            camera.targetTexture = target; Canvas.ForceUpdateCanvases();
            foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>()) { text.ForceMeshUpdate(true, true); Require(!text.isTextOverflowing, "Text overflow: " + text.name + " / " + text.text); }
            camera.Render(); RenderTexture.active = target; texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0); texture.Apply(); File.WriteAllBytes(Output + name, texture.EncodeToPNG());
        }
        finally { RenderTexture.active = old; camera.targetTexture = null; target.Release(); UnityEngine.Object.DestroyImmediate(target); if (texture != null) UnityEngine.Object.DestroyImmediate(texture); }
    }

    /// <summary>Starts a live pointer/navigation/new-run/surface/return verification in the actual scene.</summary>
    public static void BeginProductionSmoke(string output = "Docs/Validation/DiscoveryJournal/")
    {
        Output = output;
        Directory.CreateDirectory(Output);
        if (!Application.isPlaying) throw new InvalidOperationException("Enter the integrated comparison scene's Play Mode.");
        assertions = 0; Status = "Running";
        var flow = UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>();
        if (flow == null) throw new InvalidOperationException("Harbor flow missing.");
        flow.StartCoroutine(Guard(Smoke(flow)));
    }

    /// <summary>Reports failures from a frame-based verification instead of leaving an ambiguous running status.</summary>
    private static IEnumerator Guard(IEnumerator routine)
    {
        while (true)
        {
            bool more;
            try { more = routine.MoveNext(); }
            catch (Exception error) { Status = "Failed: " + error.Message; File.WriteAllText(Output + "PlayVerification.json", JsonUtility.ToJson(new Report { passed = false, assertions = assertions, message = error.ToString() }, true)); Debug.LogException(error); yield break; }
            if (!more) break;
            yield return routine.Current;
        }
        Status = "Passed"; File.WriteAllText(Output + "PlayVerification.json", JsonUtility.ToJson(new Report { passed = true, assertions = assertions, message = "Actual pointer raycast, routes, fresh run, Surface/results, Harbor return and second fresh run passed." }, true));
    }

    /// <summary>Uses actual uGUI pointer dispatch and existing controller commands without altering card or progression data.</summary>
    private static IEnumerator Smoke(JournalHarborFlow flow)
    {
        yield return null;
        var controller = flow.Controller;
        Require(flow.HubVisible && flow.CurrentPage == 0 && !controller.RunActive, "Game did not start at Harbor.");
        Click(flow, "Open Tackle Shop"); Require(flow.CurrentPage == 1, "Shop route failed.");
        yield return null; // Allow the newly enabled page to register with the GraphicRaycaster.
        Click(flow, "Technique Collection"); Require(flow.CurrentPage == 2, "Collection route failed.");
        yield return null;
        Click(flow, "Back to Harbor"); Require(flow.CurrentPage == 0, "Harbor return failed.");
        yield return null;
        Click(flow, "Open Discovery Journal"); Require(flow.CurrentPage == 3, "Journal destination failed.");
        yield return null;
        Click(flow, "Back to Harbor"); Require(flow.CurrentPage == 0, "Journal return failed.");
        yield return null;
        Click(flow, "Set Sail");
        Require(!flow.HubVisible && controller.RunActive && controller.IsOpeningBoatVisible, "Set Sail did not start the existing boat run.");
        Require(controller.TechniqueHand.Length == 4 && controller.CatchChain.Length == 0, "Fresh run was not clean.");
        Require(!flow.TryStartNewRun(), "Duplicate Set Sail restarted an active run.");
        Require(controller.TrySurface(), "Existing Surface command failed.");
        var modal = UnityEngine.Object.FindAnyObjectByType<JournalResultModal>();
        float deadline = Time.realtimeSinceStartup + 20;
        while (!modal.IsVisible && Time.realtimeSinceStartup < deadline) yield return null;
        Require(modal.IsVisible && !controller.RunActive, "Surface did not finish at results.");
        Require(modal.NextRunButton.GetComponentInChildren<TMP_Text>().text == "RETURN TO HARBOR", "Results did not offer Harbor.");
        modal.NextRunButton.onClick.Invoke(); Require(flow.HubVisible && flow.CurrentPage == 0, "Results-to-Harbor failed.");
        yield return null;
        Click(flow, "Set Sail");
        Require(controller.RunActive && controller.IsOpeningBoatVisible && controller.CatchChain.Length == 0 && controller.TechniqueHand.Length == 4, "Second expedition inherited stale state.");
        Require(controller.TryDescend(), "Fresh expedition could not Descend.");
        yield return new WaitForSecondsRealtime(2);
        Require(controller.RunActive && controller.CurrentDepth > 0, "Run did not advance after Descend.");
        Require(controller.TrySurface(), "Surface after first descent failed.");
        deadline = Time.realtimeSinceStartup + 20;
        while (!modal.IsVisible && Time.realtimeSinceStartup < deadline) yield return null;
        Require(modal.IsVisible, "Second run results missing.");
        modal.NextRunButton.onClick.Invoke(); Require(flow.HubVisible, "Second result could not return to Harbor.");
    }

    /// <summary>Checks the live raycast hit before dispatching a pointer click at the authored button centre.</summary>
    private static void Click(JournalHarborFlow flow, string name)
    {
        var button = flow.GetComponentsInChildren<UnityEngine.UI.Button>().Single(b => b.name == name && b.gameObject.activeInHierarchy);
        Canvas.ForceUpdateCanvases(); var rect = button.GetComponent<RectTransform>();
        Vector2 position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
        var pointer = new PointerEventData(EventSystem.current) { position = position, button = PointerEventData.InputButton.Left };
        var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer, hits);
        Require(hits.Count > 0 && hits[0].gameObject.GetComponentInParent<UnityEngine.UI.Button>() == button, "Pointer target misaligned or blocked: " + name);
        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
    }

    /// <summary>Stops at the first broken interaction or rendering contract.</summary>
    private static void Require(bool condition, string message) { assertions++; if (!condition) throw new InvalidOperationException(message); }
    [Serializable] private sealed class Report { public bool passed; public int assertions; public string message; }
}
