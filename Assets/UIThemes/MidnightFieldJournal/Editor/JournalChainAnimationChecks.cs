using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Verifies animated newest-catch following through the real prefab in Play Mode.</summary>
public static class JournalChainAnimationChecks
{
    private const string Output = "Docs/UIThemes/MidnightFieldJournal/SmoothCatchChain/";
    private static JournalModalHost runner;
    private static GameObject canvas;
    private static bool oldBackground;
    private static float oldTimeScale;
    private static int assertions;
    public static string Status { get; private set; } = "Not started";
    [Serializable] private sealed class Report { public bool passed; public int assertions; public string error; }

    /// <summary>Starts a finite test without modifying the saved scene or gameplay state.</summary>
    public static void Start()
    {
        if (!Application.isPlaying || runner != null) throw new InvalidOperationException("Enter Play Mode and start once.");
        oldBackground = Application.runInBackground; oldTimeScale = Time.timeScale;
        Application.runInBackground = true; assertions = 0; Status = "Running";
        runner = new GameObject("Chain Animation Verification").AddComponent<JournalModalHost>();
        runner.StartCoroutine(Guarded());
    }

    /// <summary>Records the outcome and restores test-owned state even on failure.</summary>
    private static IEnumerator Guarded()
    {
        var sequence = Check(); Exception failure = null;
        while (true)
        {
            bool more;
            try { more = sequence.MoveNext(); } catch (Exception e) { failure = e; break; }
            if (!more) break;
            yield return sequence.Current;
        }
        Directory.CreateDirectory(Output);
        File.WriteAllText(Output + "PlayModeVerification.json", JsonUtility.ToJson(new Report { passed = failure == null, assertions = assertions, error = failure?.ToString() }, true));
        Time.timeScale = oldTimeScale; Application.runInBackground = oldBackground;
        UnityEngine.Object.Destroy(canvas); UnityEngine.Object.Destroy(runner.gameObject); runner = null;
        Status = failure == null ? "Passed" : "Failed: " + failure;
        if (failure != null) Debug.LogException(failure);
    }

    /// <summary>Checks intermediate motion, repeated refresh, retargeting, manual interruption and reset.</summary>
    private static IEnumerator Check()
    {
        canvas = new GameObject("Chain Animation Test Canvas", typeof(RectTransform), typeof(Canvas));
        canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var root = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UIThemes/MidnightFieldJournal/Prefabs/CatchChain.prefab"), canvas.transform);
        var view = root.GetComponent<CatchChainView>(); var scroll = root.GetComponentInChildren<JournalCatchChainScrollRect>();
        var card = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Coastal Cod.asset");
        var catches = Enumerable.Range(1, 16).Select(i => new CardInstance(i, card)).ToArray();
        Action<int> refresh = n => { view.Refresh(catches.Take(n).ToArray(), null, n * 2, 32, -1, i => { }); Canvas.ForceUpdateCanvases(); };
        Func<float> y = () => scroll.content.anchoredPosition.y;
        Func<float> end = () => Mathf.Max(0, scroll.content.rect.height - scroll.viewport.rect.height);
        var pointer = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left };
        refresh(8); yield return null; yield return null; yield return null;
        Require(Mathf.Abs(y() - end()) < .5f, "Initial chain not positioned at end");
        Time.timeScale = 0;
        float start = y(); refresh(9); float target = end();
        Require(Mathf.Abs(y() - start) < .5f && target > start, "Append snapped instead of animating");
        bool intermediate = false; float previous = y();
        for (int i = 0; i < 240 && y() < target - .1f; i++)
        {
            refresh(9); yield return null;
            Require(y() >= previous - .1f && y() <= target + .1f, "Animation reversed or overshot");
            intermediate |= y() > start + .1f && y() < target - .1f;
            previous = y();
        }
        Require(intermediate && Mathf.Abs(y() - target) < .1f, "Unscaled animation never passed through intermediate positions or finished under repeated refresh");
        Require(scroll.FollowingNewest, "Animation lost follow intent");
        refresh(10); yield return null; float moving = y(); refresh(11);
        Require(Mathf.Abs(y() - moving) < .1f, "Retarget snapped");
        yield return new WaitForSecondsRealtime(.45f);
        Require(Mathf.Abs(y() - end()) < .1f, "Retarget failed to reveal latest catch");
        refresh(12); yield return null; pointer.scrollDelta = new Vector2(0, 3); scroll.OnScroll(pointer);
        float manual = y(); yield return new WaitForSecondsRealtime(.45f); refresh(13);
        Require(!scroll.FollowingNewest && Mathf.Abs(y() - manual) < .5f, "Wheel did not cancel follow or retain inspection offset");
        pointer.scrollDelta = new Vector2(0, -10000); scroll.OnScroll(pointer); refresh(14); yield return null;
        Require(scroll.FollowingNewest, "Return to end did not resume following");
        scroll.OnInitializePotentialDrag(pointer); scroll.OnBeginDrag(pointer); manual = y();
        yield return new WaitForSecondsRealtime(.45f); refresh(15);
        Require(!scroll.FollowingNewest && Mathf.Abs(y() - manual) < .5f, "Drag did not interrupt animation");
        scroll.OnEndDrag(pointer); refresh(0);
        Require(scroll.FollowingNewest && Mathf.Abs(y()) < .1f, "Empty chain did not reset");
        refresh(8); refresh(9); root.SetActive(false); root.SetActive(true); refresh(9);
        Require(Mathf.Abs(y() - end()) < .1f, "Re-enabled chain retained unfinished motion");
        refresh(10); refresh(1); yield return new WaitForSecondsRealtime(.45f);
        Require(Mathf.Abs(y()) < .1f, "Shrink retained stale animation");
    }

    /// <summary>Stops on the first regression.</summary>
    private static void Require(bool value, string message) { assertions++; if (!value) throw new InvalidOperationException(message); }
}
