using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>Validates the Tackle phase transitions through a real Discover → Landing → Build → Discover sequence with persistence disabled.</summary>
public static class TacklePhaseTransitionChecks
{
    public const string Output = "Docs/Validation/PhaseTransitions/";
    private const BindingFlags Hidden = BindingFlags.Static | BindingFlags.NonPublic;
    public static string Result { get; private set; } = "Not run";
    private static int checks;

    /// <summary>Confirms the overlay prefab and scene binding: hidden by default, above every phase canvas, below Harbor preparation.</summary>
    [MenuItem("Fishing Cards/Validation/Tackle Phase Transitions (Assets)")]
    public static int ValidateAssets()
    {
        checks = 0;
        var root = AssetDatabase.LoadAssetAtPath<GameObject>(TacklePhaseTransitionSetup.PrefabPath);
        Check(root != null, "Phase-transition prefab missing.");
        var transition = root.GetComponent<TacklePhaseTransition>(); var data = new SerializedObject(transition);
        foreach (string field in new[] { "overlay", "content", "fullSnapshot", "blocker", "interiorWindow", "shutter" })
            Check(data.FindProperty(field).objectReferenceValue != null, "Transition field '" + field + "' is unbound.");
        var canvas = root.GetComponent<Canvas>();
        Check(!canvas.enabled && canvas.renderMode == RenderMode.ScreenSpaceOverlay, "Overlay is visible or not a screen overlay at rest.");
        Check(canvas.sortingOrder > 400 && canvas.sortingOrder < 500, "Overlay does not sit above Landing and below Harbor preparation.");
        Check(((Component)data.FindProperty("interiorWindow").objectReferenceValue).GetComponent<UnityEngine.UI.RectMask2D>() != null, "Interior window does not clip the shutter.");
        Check(((UnityEngine.UI.RawImage)data.FindProperty("blocker").objectReferenceValue).raycastTarget
            && !((UnityEngine.UI.RawImage)data.FindProperty("fullSnapshot").objectReferenceValue).raycastTarget, "Only the blocker should intercept input.");
        Check(TacklePhaseTransition.ReducedDuration < TacklePhaseTransition.Duration && TacklePhaseTransition.Duration <= .5f, "Transition is too long.");
        // Shutter motion: rolled up at both ends, fully closed when the held frame is swapped, jolt only around the landing.
        Check(TacklePhaseTransition.Extent(0) == 0 && TacklePhaseTransition.Extent(TacklePhaseTransition.Duration) == 0, "Shutter does not start and end rolled up.");
        Check(TacklePhaseTransition.Extent(TacklePhaseTransition.CloseSeconds) >= .999f && TacklePhaseTransition.Extent(TacklePhaseTransition.SwapAt) > .96f, "Window is not covered when the phase swaps.");
        Check(TacklePhaseTransition.Jolt(.05f) == 0 && TacklePhaseTransition.Jolt(TacklePhaseTransition.Duration - .01f) == 0, "Jolt outside the landing.");
        // Drum curve: depth is continuous and monotonic, and the top of the strip is foreshortened.
        float previous = -1; bool monotonic = true;
        for (float s = 0; s < 120; s += .5f) { float d = TackleShutterGraphic.Depth(s); monotonic &= d > previous; previous = d; }
        Check(monotonic && TackleShutterGraphic.Depth(2) < 1 && Mathf.Abs(TackleShutterGraphic.StripLength(TackleShutterGraphic.Depth(60)) - 60) < .01f, "Shutter drum mapping is wrong.");
        var bounds = transition.BodyBounds; var shell = TackleMachineGeometry.StageBounds;
        Check(bounds.xMin >= shell.xMin && bounds.xMax <= shell.xMax + .01f && bounds.yMin >= shell.yMin - .01f && bounds.yMax <= shell.yMax + .01f && bounds.width > shell.width * .85f,
            "Shutter does not cover the machine body inside the shared shell.");
        var shutterGraphic = (TackleShutterGraphic)data.FindProperty("shutter").objectReferenceValue;
        Check(shutterGraphic.HasSilhouette, "Shutter has no traced body silhouette.");
        return checks;
    }

    /// <summary>Starts the live sequence; requires a fresh comparison Play session.</summary>
    [MenuItem("Fishing Cards/Validation/Tackle Phase Transitions (Live)")]
    public static void Begin()
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Enter a fresh comparison Play session.");
        Result = "Running"; checks = 0; var owner = UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>().Controller;
        Directory.CreateDirectory(Output); File.WriteAllText(Output + "LiveVerification.json", "{\"passed\":false,\"running\":true}");
        owner.SetDebugSaveResetPending(true); Application.runInBackground = true; owner.StartCoroutine(Guarded(Run()));
    }

    private static IEnumerator Run()
    {
        var harbor = UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>(); var owner = harbor.Controller;
        var transition = UnityEngine.Object.FindAnyObjectByType<TacklePhaseTransition>(FindObjectsInactive.Include);
        Check(transition != null && transition.isActiveAndEnabled, "Phase transition is not installed in the scene.");
        var overlay = transition.GetComponent<Canvas>();
        owner.TryPrepareSlotTags(new[] { "Armored", "Predator", "Schooling", "Anchored" }); harbor.TryStartNewRun();
        ((UnityEngine.UI.Button)new SerializedObject(harbor.SlotPreparation).FindProperty("confirm").objectReferenceValue).onClick.Invoke();
        yield return null;
        Check(!transition.IsPlaying, "Harbor-to-Discover entry played a machine transition.");

        // Discover → Landing.
        var ready = typeof(FishingRunController).GetProperty("CoreActionInputReady", BindingFlags.Instance | BindingFlags.NonPublic);
        double deadline = Time.realtimeSinceStartupAsDouble + 15;
        while (!(bool)ready.GetValue(owner) && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
        typeof(RigBuildingSceneChecks).GetMethod("ForceNextCreature", Hidden).Invoke(null, new object[] { owner, Card("Sardine") });
        SlotReelAnimationChecks.SettleForControlFixture(UnityEngine.Object.FindAnyObjectByType<SlotMachineView>(), owner);
        yield return null;
        double accepted = Time.realtimeSinceStartupAsDouble;
        Check(owner.TryAcceptSlotMachine(owner.SlotMachine.Revision), "Could not accept Discover.");
        Check(transition.IsPlaying && overlay.enabled, "Discover-to-Landing did not play a transition.");
        Check(owner.LandingRevealStartedAt - accepted >= transition.LeadSeconds - .05, "Creature entrance starts behind the transition.");
        Check(owner.LandingRevealPending && UnityEngine.Object.FindAnyObjectByType<LandingCreatureReveal>().IsPlaying, "Creature entrance is not waiting behind the transition.");
        yield return Film(transition, "DiscoverToLanding");
        Check(!transition.IsPlaying && !overlay.enabled, "Transition overlay was left on screen.");
        owner.CompleteLandingReveal();

        // Landing → Build (new species).
        yield return (IEnumerator)typeof(RigBuildingSceneChecks).GetMethod("Land", Hidden).Invoke(null, new object[] { owner, true });
        Check(owner.AwaitingRigPlacement && UnityEngine.Object.FindAnyObjectByType<RigBuildingView>() != null, "Landing did not open Build.");
        Check(transition.IsPlaying, "Landing-to-Build did not play a transition.");
        yield return Film(transition, "LandingToBuild");

        // Build → Discover after placement feedback.
        Check(owner.TryCommitRigPlacement(owner.BuildOccurrence, owner.Rig.Revision, 0), "Placement was rejected.");
        double until = Time.realtimeSinceStartupAsDouble + 5;
        while (owner.RigAcquisitionFeedbackVisible && Time.realtimeSinceStartupAsDouble < until) yield return null;
        Check(owner.SlotMachine.Active && transition.IsPlaying, "Build-to-Discover did not play a transition.");
        yield return Film(transition, "BuildToDiscover");
        Check(!transition.IsPlaying && !overlay.enabled, "Final transition left the overlay on.");

        Result = "Passed " + checks + " live phase-transition checks";
        File.WriteAllText(Output + "LiveVerification.json", "{\"passed\":true,\"assertions\":" + checks + "}");
        Debug.Log(Result);
    }

    /// <summary>
    /// Freezes the transition at exact ages and captures the composited screen (every overlay canvas): five stills for review and a
    /// 60 fps sequence under Temp/ for an optional preview video. Captures are slow, so the transition is seeked, never raced.
    /// </summary>
    private static IEnumerator Film(TacklePhaseTransition transition, string name)
    {
        string frames = "Temp/PhaseTransitionPreview/" + name + "/";
        if (Directory.Exists(frames)) Directory.Delete(frames, true); Directory.CreateDirectory(frames);
        int count = Mathf.CeilToInt(TacklePhaseTransition.Duration * 60) + 1;
        int[] stills = { 6, 11, 14, 21, count - 1 }; int still = 0;
        for (int i = 0; i < count; i++)
        {
            transition.Seek(i / 60f);
            yield return new WaitForEndOfFrame();
            var shot = ScreenCapture.CaptureScreenshotAsTexture();
            byte[] png = shot.EncodeToPNG(); UnityEngine.Object.Destroy(shot);
            File.WriteAllBytes(frames + i.ToString("000") + ".png", png);
            if (still < stills.Length && i == stills[still]) { File.WriteAllBytes(Output + name + "-" + (++still) + ".png", png); }
        }
        transition.Release(); transition.Finish();
        yield return null;
        Check(!transition.IsPlaying, name + " transition never finished.");
    }

    private static IEnumerator Guarded(IEnumerator body)
    {
        var stack = new System.Collections.Generic.Stack<IEnumerator>(); stack.Push(body);
        while (stack.Count > 0)
        {
            object current;
            try
            {
                if (!stack.Peek().MoveNext()) { stack.Pop(); continue; }
                current = stack.Peek().Current;
            }
            catch (Exception error)
            {
                var inner = error is TargetInvocationException && error.InnerException != null ? error.InnerException : error;
                Result = "FAILED: " + inner.Message; File.WriteAllText(Output + "LiveVerification.json", "{\"passed\":false}");
                Debug.LogError("Phase transition check failed: " + inner); yield break;
            }
            if (current is IEnumerator nested) { stack.Push(nested); continue; }
            yield return current;
        }
    }

    /// <summary>
    /// Ends any running phase transition so a check that raycasts the at-rest layout is not hit by its brief input blocker.
    /// Cosmetic only; mirrors how strike checks skip the creature entrance.
    /// </summary>
    internal static void Settle()
    {
        var transition = UnityEngine.Object.FindAnyObjectByType<TacklePhaseTransition>();
        if (transition != null) transition.Finish();
    }

    private static CardDefinition Card(string name) => AssetDatabase.FindAssets("t:CardDefinition", new[] { "Assets/Cards" })
        .Select(g => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).Single(c => c.DisplayName == name);
    private static void Check(bool valid, string message) { if (!valid) { Result = "FAILED: " + message; throw new InvalidOperationException(message); } checks++; }
}
