#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>Opt-in development-player checks; never creates an object during ordinary gameplay.</summary>
public sealed class JournalPlayerValidation : MonoBehaviour
{
    [Serializable] private sealed class Sample
    {
        public int width, height, handWidth, handHeight, frames;
        public float averageFrameMs, p95FrameMs;
        public long totalAllocatedBytes, graphicsDriverBytes, textureBytes, handRendersDuringIdle, batches, setPassCalls;
        public bool batchesAvailable, setPassAvailable;
    }
    [Serializable] private sealed class Report
    {
        public bool passed;
        public int assertions;
        public string unity, graphics, failure, scope = "Development Windows player; real scene, layout/raycast/focus, idle frame sampling, Surface/restart and original-scene smoke";
        public List<Sample> samples = new List<Sample>();
        public List<string> errors = new List<string>();
    }
    private readonly Report report = new Report();
    private string output;
    private bool finished;
    private Action<int, int> resize;
    private Action completion;
    private bool oldBackground;
    private int oldTargetRate;
    public static string Status { get; private set; } = "Not started";

#if UNITY_EDITOR
    /// <summary>Runs the same output checks using an Editor-owned temporary Game View size.</summary>
    public static void StartEditor(string directory, Action<int, int> setSize, Action completed)
    {
        if (!Application.isPlaying || FindFirstObjectByType<JournalPlayerValidation>() != null) throw new InvalidOperationException("Start once in Play Mode.");
        var obj = new GameObject("Opt-in Journal Output Validation"); DontDestroyOnLoad(obj);
        var runner = obj.AddComponent<JournalPlayerValidation>(); runner.output = directory; runner.resize = setSize; runner.completion = completed;
        runner.report.scope = "Actual Editor Play Mode output; fixed Game View sizes, raycast/focus, idle frame sampling, Surface/restart and original-scene smoke. Not standalone player evidence.";
        runner.Begin();
    }
#endif

    /// <summary>Requires both a development build and an explicit absolute output-directory argument.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void StartIfRequested()
    {
        if (Application.isEditor || !Debug.isDebugBuild || FindFirstObjectByType<JournalPlayerValidation>() != null) return;
        string[] args = Environment.GetCommandLineArgs(); int index = Array.IndexOf(args, "-journal-smoke-output");
        if (index < 0 || index + 1 >= args.Length || !Path.IsPathRooted(args[index + 1])) return;
        var obj = new GameObject("Opt-in Journal Player Validation"); DontDestroyOnLoad(obj);
        var runner = obj.AddComponent<JournalPlayerValidation>(); runner.output = args[index + 1]; runner.Begin();
    }
    /// <summary>Captures and restores presentation-only execution settings for a bounded validation run.</summary>
    private void Begin()
    {
        Directory.CreateDirectory(output); oldBackground = Application.runInBackground; oldTargetRate = Application.targetFrameRate;
        Application.runInBackground = true; Application.targetFrameRate = 60;
        Application.logMessageReceived += RecordError; Status = "Running"; StartCoroutine(Guarded());
    }
    /// <summary>Records player exceptions and guarantees an exit report even when a check fails.</summary>
    private IEnumerator Guarded()
    {
        report.unity = Application.unityVersion; report.graphics = SystemInfo.graphicsDeviceName + " / " + SystemInfo.graphicsDeviceType;
        var sequence = Run(); Exception failure = null;
        while (true)
        {
            bool more;
            try { more = sequence.MoveNext(); } catch (Exception error) { failure = error; break; }
            if (!more) break;
            yield return sequence.Current;
        }
        report.failure = failure?.ToString(); report.passed = failure == null && report.errors.Count == 0;
        File.WriteAllText(Path.Combine(output, Application.isEditor ? "UI08EditorOutputValidation.json" : "UI08PlayerValidation.json"), JsonUtility.ToJson(report, true));
        finished = true; Application.logMessageReceived -= RecordError;
        Status = report.passed ? "Passed: " + report.assertions : "Failed: " + report.failure;
        Application.runInBackground = oldBackground; Application.targetFrameRate = oldTargetRate; completion?.Invoke();
        if (Application.isEditor) Destroy(gameObject); else Application.Quit(report.passed ? 0 : 1);
    }
    /// <summary>Captures actual output pixels at several aspects, then exercises both scene variants.</summary>
    private IEnumerator Run()
    {
        yield return null; yield return null;
        var controller = FindFirstObjectByType<FishingRunController>(); var hud = FindFirstObjectByType<JournalGameplayHud>();
        var hand = FindFirstObjectByType<JournalTechniqueHandPresentation>();
        Require(controller != null && controller.RunActive && hud != null && hand != null, "Candidate failed to start.");
        Require(controller.TechniqueHand.Length == 4 && controller.TechniqueDrawPile.Length == 8, "Starter deck changed in player.");
        int[,] sizes = { {3840,2160}, {1920,1080}, {3440,1440}, {1920,1200} };
        for (int s = 0; s < sizes.GetLength(0); s++)
        {
            if (resize != null) resize(sizes[s, 0], sizes[s, 1]); else Screen.SetResolution(sizes[s, 0], sizes[s, 1], FullScreenMode.Windowed);
            for (int frame = 0; frame < 12; frame++) yield return null;
            Require(Screen.width == sizes[s, 0] && Screen.height == sizes[s, 1], "Player output was clamped by the display/window manager.");
            Canvas.ForceUpdateCanvases();
            CheckInside(hud.DescendButton.GetComponent<RectTransform>()); CheckInside(hud.SurfaceButton.GetComponent<RectTransform>());
            CheckInside(hand.Surface.rectTransform);
            foreach (var button in new[] { hud.DescendButton, hud.SurfaceButton, hud.SettingsButton }) CheckHit(button);
            for (int i = 0; i < controller.TechniqueHand.Length; i++)
            {
                RectTransform rect = hand.Cards[i].GetComponent<RectTransform>();
                Vector3 pixel = hand.ProjectionCamera.WorldToScreenPoint(rect.TransformPoint(rect.rect.center));
                RectTransform surface = hand.Surface.rectTransform;
                Vector2 uv = new Vector2(pixel.x / hand.Target.width, pixel.y / hand.Target.height);
                Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, surface.TransformPoint(surface.rect.min + Vector2.Scale(uv, surface.rect.size)));
                var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = screen }, hits);
                Require(hits.Count > 0 && hits[0].gameObject.GetComponentInParent<TechniqueCardView>() == hand.Cards[i], "Transformed hand target mismatch: " + i);
            }
            EventSystem.current.SetSelectedGameObject(hand.Cards[0].gameObject); yield return null;
            Require(hand.Poses[0].Inspecting, "Navigation selection did not inspect card.");
            EventSystem.current.SetSelectedGameObject(null); yield return null;
            yield return new WaitForEndOfFrame();
            Texture2D pixels = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(output, (Application.isEditor ? "UI08Gameplay" : "UI08Player") + Screen.width + "x" + Screen.height + ".png"), pixels.EncodeToPNG()); Destroy(pixels);
            for (int frame = 0; frame < 12; frame++) yield return null;
            var sample = new Sample { width = Screen.width, height = Screen.height, handWidth = hand.Target.width, handHeight = hand.Target.height };
            long renders = hand.RenderedFrameCount;
            var times = new List<float>();
            using (var batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count"))
            using (var passes = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count"))
            {
                for (int frame = 0; frame < 120; frame++) { yield return null; times.Add(Time.unscaledDeltaTime * 1000); }
                sample.batchesAvailable = batches.Valid; sample.setPassAvailable = passes.Valid;
                sample.batches = batches.Valid ? batches.LastValue : -1; sample.setPassCalls = passes.Valid ? passes.LastValue : -1;
            }
            times.Sort(); sample.frames = times.Count; sample.averageFrameMs = times.Average(); sample.p95FrameMs = times[(int)(times.Count * .95f)];
            sample.handRendersDuringIdle = hand.RenderedFrameCount - renders;
            sample.totalAllocatedBytes = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
            sample.graphicsDriverBytes = UnityEngine.Profiling.Profiler.GetAllocatedMemoryForGraphicsDriver();
            sample.textureBytes = Resources.FindObjectsOfTypeAll<Texture>().Sum(t => UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t));
            Require(sample.handRendersDuringIdle == 0, "Idle hand rendered repeatedly."); report.samples.Add(sample);
        }
        if (resize != null) resize(1920, 1080); else Screen.SetResolution(1920,1080, FullScreenMode.Windowed); yield return null;
        for (int step = 0; step < 3; step++)
        {
            Require(controller.TryDescend(), "Candidate refused a legal Descend.");
            float until = Time.realtimeSinceStartup + 8;
            while ((!controller.CanDescend || controller.HasPendingCreatureChoice) && controller.RunActive && Time.realtimeSinceStartup < until)
            { ResolveCandidateChoice(); yield return null; }
            Require(controller.RunActive && controller.CanSurface, "Candidate action did not finish.");
        }
        Require(controller.TrySurface(), "Candidate refused Surface.");
        float deadline = Time.realtimeSinceStartup + 20;
        var result = FindFirstObjectByType<JournalResultModal>(FindObjectsInactive.Include);
        while (!result.IsVisible && Time.realtimeSinceStartup < deadline) { ResolveCandidateChoice(); yield return null; }
        Require(result.IsVisible && !controller.RunActive, "Candidate result timed out.");
        Submit(result.NextRunButton); Require(controller.RunActive && controller.CurrentDepth == 0, "Candidate restart failed.");
        SceneManager.LoadScene("SampleScene"); yield return null; yield return null;
        controller = FindFirstObjectByType<FishingRunController>();
        Require(controller != null && controller.RunActive && FindFirstObjectByType<JournalGameplayHud>() == null, "Original scene did not start independently.");
        Require(controller.TechniqueHand.Length == 4 && controller.TrySurface(), "Original scene could not Surface.");
        deadline = Time.realtimeSinceStartup + 20;
        var oldView = FindFirstObjectByType<FishingRunView>();
        while ((controller.RunActive || oldView.IsSurfaceTransitionPlaying) && Time.realtimeSinceStartup < deadline) yield return null;
        Require(!controller.RunActive && !oldView.IsSurfaceTransitionPlaying, "Original Surface timed out.");
        controller.StartRun(); Require(controller.RunActive && controller.TechniqueHand.Length == 4, "Original restart failed.");
    }
    /// <summary>Chooses a displayed legal optional/last row without relying on internal choice state.</summary>
    private static void ResolveCandidateChoice()
    {
        var choice = FindFirstObjectByType<JournalChoiceModal>(FindObjectsInactive.Include); if (choice == null || !choice.IsVisible) return;
        var active = choice.Rows.Where(r => r.gameObject.activeInHierarchy).ToArray();
        var decline = active.FirstOrDefault(r => r.GetComponentsInChildren<TMP_Text>().Any(t => t.text.StartsWith("Decline", StringComparison.OrdinalIgnoreCase) || t.text.StartsWith("Skip", StringComparison.OrdinalIgnoreCase)));
        Submit(decline ?? active.Last());
    }
    /// <summary>Confirms actual EventSystem ordering for a visible action face.</summary>
    private void CheckHit(UnityEngine.UI.Button button)
    {
        var rect = button.GetComponent<RectTransform>(); var hits = new List<RaycastResult>();
        var pointer = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center)) };
        EventSystem.current.RaycastAll(pointer, hits);
        Require(hits.Count > 0 && hits[0].gameObject.GetComponentInParent<UnityEngine.UI.Button>() == button, "Action blocked: " + button.name);
    }
    /// <summary>Rejects primary regions extending beyond the actual player output.</summary>
    private void CheckInside(RectTransform rect)
    {
        var corners = new Vector3[4]; rect.GetWorldCorners(corners);
        Require(corners.All(c => c.x >= -1 && c.y >= -1 && c.x <= Screen.width + 1 && c.y <= Screen.height + 1), "Region outside screen: " + rect.name);
    }
    /// <summary>Uses the same selected-submit path as navigation input.</summary>
    private static void Submit(UnityEngine.UI.Button button) { ExecuteEvents.Execute(button.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler); }
    /// <summary>Records exceptions without changing normal game logging.</summary>
    private void RecordError(string message, string trace, LogType type) { if (!finished && (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)) report.errors.Add(message + "\n" + trace); }
    /// <summary>Stops validation on a failed functional contract.</summary>
    private void Require(bool condition, string message) { report.assertions++; if (!condition) throw new InvalidOperationException(message); }
}
#endif
