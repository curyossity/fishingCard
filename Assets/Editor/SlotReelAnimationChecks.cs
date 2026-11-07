using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>Exercises real frame-driven cylinder motion and the controller's presentation input boundary in Play Mode.</summary>
public static class SlotReelAnimationChecks
{
    public static string Result { get; private set; } = "Not run";
    private static int checks;
    private const string Output = "Docs/Validation/SlotReelAnimation/";

    /// <summary>Starts from the Harbor without modifying saves or supplying a fabricated landing result.</summary>
    public static void Begin()
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Enter a fresh comparison Play Mode session.");
        Result = "Running"; checks = 0;
        UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>().Controller.StartCoroutine(Validate());
    }

    /// <summary>Validates initial motion, curvature, one stationary locked reel, stable final review and explicit acceptance.</summary>
    private static IEnumerator Validate()
    {
        var harbor = UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>(); var controller = harbor.Controller;
        Check(!controller.RunActive, "Expected a fresh Harbor.");
        Check(controller.TryPrepareSlotTags(new[] { "Fish", "Predator", "Schooling", "Small" }), "Could not prepare four tags.");
        harbor.TryStartNewRun();
        var preparation = new SerializedObject(harbor.GetComponentInChildren<SlotTagPreparationView>(true));
        ((UnityEngine.UI.Button)preparation.FindProperty("confirm").objectReferenceValue).onClick.Invoke();
        var view = UnityEngine.Object.FindAnyObjectByType<SlotMachineView>();
        var reels = view.GetComponentsInChildren<TackleReelAnimator>();
        Check(reels.Length == 3 && view.IsAnimating && reels.All(r => r.IsSpinning), "Initial spin did not animate all wheels.");
        string[] initial = Enumerable.Range(0, 3).Select(controller.SlotMachine.GetTag).ToArray();
        long revision = controller.SlotMachine.Revision;
        Check(!controller.TryAcceptSlotMachine(revision), "Accept bypassed initial animation.");
        Check(!controller.TrySetSlotReelLocked(revision, 0, true) && !controller.TryRespinSlotMachine(revision), "Spin input was not gated.");
        float phase = reels[0].Phase;
        yield return new WaitForSecondsRealtime(.18f);
        Check(reels[0].Phase != phase, "Wheel phase did not advance across frames.");
        Check(initial.SequenceEqual(Enumerable.Range(0, 3).Select(controller.SlotMachine.GetTag)), "Animation rerolled model faces.");
        SlotMachineSceneChecks.Capture(view.GetComponent<Canvas>(), "InitialSpinning.png", output: Output);
        float timeout = Time.realtimeSinceStartup + 5;
        while (view.IsAnimating && Time.realtimeSinceStartup < timeout) yield return null;
        Check(!view.IsAnimating && reels.All(r => !r.IsSpinning), "Initial reels did not settle.");
        Check(controller.CurrentEncounter == null && controller.SlotMachine.Active, "Settling accepted the result automatically.");
        var face = reels[0].GetComponentsInChildren<TackleReelFace>().Single();
        Vector3 cylinderCenter = ((RectTransform)reels[0].transform).rect.center;
        Color32 color = Color.white;
        Vector3 center = face.Project(reels[0].transform, cylinderCenter, ref color);
        Color32 edgeColor = Color.white;
        Vector3 upper = face.Project(reels[0].transform, cylinderCenter + new Vector3(0, 150, 0), ref edgeColor);
        Check(Mathf.Abs(center.y - cylinderCenter.y) < .01f && Mathf.Abs(upper.y - cylinderCenter.y - 150) < .01f, "Settled cylinder shifted the approved face geometry.");
        Check(edgeColor.r < color.r, "Cylinder lighting did not darken its curved edges.");
        Check(controller.TrySetSlotReelLocked(controller.SlotMachine.Revision, 1, true), "Could not lock settled middle reel.");
        float lockedPhase = reels[1].Phase;
        string lockedTag = controller.SlotMachine.GetTag(1);
        Check(controller.TryRespinSlotMachine(controller.SlotMachine.Revision), "Respin failed.");
        string[] final = Enumerable.Range(0, 3).Select(controller.SlotMachine.GetTag).ToArray();
        Check(reels[0].IsSpinning && !reels[1].IsSpinning && reels[2].IsSpinning, "Locked wheel moved or unlocked wheel did not spin.");
        Check(!controller.TryAcceptSlotMachine(controller.SlotMachine.Revision), "Accept bypassed respin animation.");
        yield return new WaitForSecondsRealtime(.2f);
        Check(reels[1].Phase == lockedPhase && controller.SlotMachine.GetTag(1) == lockedTag, "Locked wheel changed during respin.");
        SlotMachineSceneChecks.Capture(view.GetComponent<Canvas>(), "LockedRespin.png", output: Output);
        timeout = Time.realtimeSinceStartup + 5;
        while (view.IsAnimating && Time.realtimeSinceStartup < timeout) yield return null;
        Check(!view.IsAnimating, "Respin never finished.");
        Check(final.SequenceEqual(Enumerable.Range(0, 3).Select(controller.SlotMachine.GetTag)), "Visual respin changed its authoritative result.");
        Check(controller.SlotMachine.RespinsRemaining == 0 && !controller.TryRespinSlotMachine(controller.SlotMachine.Revision), "A second respin was allowed.");
        var data = new SerializedObject(view);
        var accept = (UnityEngine.UI.Button)data.FindProperty("acceptButton").objectReferenceValue;
        Check(accept.interactable, "Accept did not enable after settling.");
        for (int i = 0; i < 3; i++)
        {
            Check(!((UnityEngine.UI.Button)data.FindProperty("lockButtons").GetArrayElementAtIndex(i).objectReferenceValue).interactable, "Final lock remained enabled.");
            var label = reels[i].GetComponentsInChildren<TMP_Text>().Last();
            Check(string.Equals(label.text, final[i], StringComparison.OrdinalIgnoreCase), "Visible caption differs from final tag.");
            Check(label.textInfo.meshInfo[0].colors32.Take(label.textInfo.meshInfo[0].vertexCount).Any(c => c.a > 200), "Final caption is transparent.");
        }
        SlotMachineSceneChecks.Capture(view.GetComponent<Canvas>(), "FinalReview1080.png", output: Output);
        SlotMachineSceneChecks.Capture(view.GetComponent<Canvas>(), "FinalReviewUltrawide.png", 2560, 1080, Output);
        yield return new WaitForSecondsRealtime(.3f);
        Check(controller.SlotMachine.Active && controller.CurrentEncounter == null, "Final review advanced without Accept.");
        Check(!controller.TryAcceptSlotMachine(revision), "Stale initial acceptance succeeded.");
        accept.onClick.Invoke();
        Check(controller.AwaitingLandingChallenge && controller.CurrentEncounter != null && !view.gameObject.activeInHierarchy, "Accept did not hand off to Land.");
        Check(reels.All(r => !r.IsSpinning) && controller.CatchChain.Length == 0, "Animation persisted after phase exit or attached a creature.");
        Result = "Passed " + checks + " live animation checks";
        Debug.Log(Result);
    }

    /// <summary>Stops immediately with a visible result on any failed boundary or presentation invariant.</summary>
    private static void Check(bool condition, string message)
    {
        if (!condition) { Result = "FAILED: " + message; throw new InvalidOperationException(message); }
        checks++;
    }

    /// <summary>Settles authored animations for existing synchronous rule/control fixtures; motion has its own frame-driven check.</summary>
    public static void SettleForControlFixture(SlotMachineView view, FishingRunController controller)
    {
        foreach (var reel in view.GetComponentsInChildren<TackleReelAnimator>()) reel.Advance(10);
        view.Present(controller);
    }
}
