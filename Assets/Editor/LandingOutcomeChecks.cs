using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Validates the Landing outcome beat (Landed / Escaped with haul cash-out) and the Build haul count-up through real Discover,
/// Landing and Build commands with persistence disabled, and films review frames.
/// </summary>
public static class LandingOutcomeChecks
{
    public const string Output = "Docs/Validation/LandingOutcome/";
    private const BindingFlags Hidden = BindingFlags.Static | BindingFlags.NonPublic;
    public static string Result { get; private set; } = "Not run";
    private static int checks;

    /// <summary>Pure schedule rules: rest poses at both ends, bounded durations, an exact payout total and decaying punches.</summary>
    [MenuItem("Fishing Cards/Validation/Landing Outcome (Timing)")]
    public static int ValidateTiming()
    {
        checks = 0;
        var landed = LandingOutcomeTiming.For(LandingOutcomeKind.Landed);
        Check(landed.Duration <= 1 && landed.Lift(0) == 0 && landed.Lift(landed.Duration) == 0 && landed.Scale(landed.Duration) == Vector2.one, "Landed beat does not start and end at rest.");
        Check(landed.Lift((LandingOutcomeTiming.YankEnd + LandingOutcomeTiming.HangEnd) * .5f) > 40 && landed.Lift(LandingOutcomeTiming.DipEnd * .9f) < 0, "Landed card is not dipped and then yanked up.");
        Check(Mathf.Abs(landed.Lift(LandingOutcomeTiming.LandAt)) < .5f && landed.Flash(LandingOutcomeTiming.LandAt + .01f) > .3f && landed.Scale(LandingOutcomeTiming.LandAt).y < .95f, "Landed card does not slam home with a flash and squash.");
        Check(landed.FaceOpacity(.5f, false) == 1 && float.IsPositiveInfinity(landed.StampHold), "Landed card or stamp disappears before the shutter.");
        Check(landed.StageOffset(LandingOutcomeTiming.LandAt + .3f) == Vector2.zero, "Landed stage impulse does not settle.");
        var values = new[] { 7, 12, 15, 9 }; int total = values.Sum();
        var escaped = LandingOutcomeTiming.For(LandingOutcomeKind.Escaped, values.Length);
        Check(escaped.FaceOpacity(escaped.StampAt, false) < .01f && escaped.FaceOpacity(escaped.StampAt, true) < .01f, "Escaped creature is still visible when ESCAPED stamps.");
        Check(escaped.StampAt + escaped.StampHold + LandingRarityBanner.FoldSeconds <= LandingOutcomeTiming.PayoutAt + .001f, "ESCAPED stamp overlaps the payout readout.");
        int previous = -1; bool monotonic = true;
        for (float t = 0; t <= escaped.Duration; t += 1 / 120f) { int v = escaped.Payout(t, values, total); monotonic &= v >= previous; previous = v; }
        Check(monotonic && escaped.Payout(LandingOutcomeTiming.PayoutAt - .01f, values, total) == 0 && escaped.Payout(escaped.PayoutEnd, values, total) == total, "Payout tally is not a monotonic roll ending exactly on the paid total.");
        Check(escaped.ShowsExpeditionOver(LandingOutcomeTiming.PayoutAt) && !escaped.ShowsExpeditionOver(escaped.StampAt), "Header does not name the escape before stating the expedition is over.");
        Check(escaped.CardOpacity(0, escaped.ClearAt + LandingOutcomeTiming.CardFadeSeconds + .001f) == 0 && escaped.CardOpacity(3, LandingOutcomeTiming.PayoutAt) == 1, "Cashed-out cards do not clear in order.");
        Check(escaped.PayoutPunch(escaped.PayoutEnd + LandingOutcomeTiming.PunchSeconds + .01f) == 1 && TackleHaulCount.Punch(1) == 1, "Punches do not return to exactly 1.");
        var longHaul = LandingOutcomeTiming.For(LandingOutcomeKind.Escaped, 40);
        Check(escaped.Duration <= LandingOutcomeTiming.TypicalMaximum && longHaul.Duration < 4.5f, "Escape beat is too long.");
        Check(LandingOutcomeTiming.For(LandingOutcomeKind.Escaped, 0).Duration < escaped.Duration && LandingOutcomeTiming.For(LandingOutcomeKind.Escaped, 0).Payout(5, null, 0) == 0, "Empty haul is not handled.");
        Check(TackleHaulCount.Value(32, 39, 0) == 32 && TackleHaulCount.Value(32, 39, 1) == 39 && TackleHaulCount.Value(32, 39, .5f) is int mid && mid > 32 && mid < 39, "Haul count-up does not roll between its ends.");
        return checks;
    }

    /// <summary>Starts the live sequence; requires a fresh comparison Play session.</summary>
    [MenuItem("Fishing Cards/Validation/Landing Outcome (Live)")]
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
        owner.TryPrepareSlotTags(new[] { "Armored", "Predator", "Schooling", "Anchored" }); harbor.TryStartNewRun();
        ((UnityEngine.UI.Button)new SerializedObject(harbor.SlotPreparation).FindProperty("confirm").objectReferenceValue).onClick.Invoke();
        yield return null;
        var landingView = UnityEngine.Object.FindAnyObjectByType<LandingChallengeView>(FindObjectsInactive.Include);
        var viewData = new SerializedObject(landingView);
        var strike = (UnityEngine.UI.Button)viewData.FindProperty("strike").objectReferenceValue;
        var returnButton = (UnityEngine.UI.Button)viewData.FindProperty("returnToHarbor").objectReferenceValue;
        var presentation = landingView.GetComponentInChildren<TackleLandingPresentation>(true);
        var outcome = presentation.Outcome; var outcomeData = new SerializedObject(outcome);
        var card = (RectTransform)outcomeData.FindProperty("creatureCard").objectReferenceValue;
        var fade = (CanvasGroup)outcomeData.FindProperty("creatureFade").objectReferenceValue;
        var banner = (LandingRarityBanner)outcomeData.FindProperty("banner").objectReferenceValue;
        var payout = (CanvasGroup)outcomeData.FindProperty("payout").objectReferenceValue;
        var header = (TMPro.TMP_Text)new SerializedObject(presentation).FindProperty("creature").objectReferenceValue;
        var transition = UnityEngine.Object.FindAnyObjectByType<TacklePhaseTransition>(FindObjectsInactive.Include);

        // 1. New species landed: the beat holds Build, cannot be bypassed, then skips into Build.
        yield return Encounter(owner, "Sardine", true);
        Check(owner.LandingOutcomePending && outcome.IsPlaying && outcome.Timing.Kind == LandingOutcomeKind.Landed, "A successful landing did not start the Landed beat.");
        Check(landingView.gameObject.activeInHierarchy && UnityEngine.Object.FindAnyObjectByType<RigBuildingView>() == null, "Build opened before the Landed beat.");
        Check(strike.gameObject.activeSelf && strike.interactable && !returnButton.gameObject.activeSelf, "Landed beat does not offer only Skip.");
        Check(!owner.TryCommitRigPlacement(owner.BuildOccurrence, owner.Rig.Revision, 0), "Placement was accepted during the Landed beat.");
        owner.HoldLandingOutcomeForReview(); outcome.Seek(0); // Age 0 is the resting pose.
        Vector2 cardRest = card.anchoredPosition;
        yield return Film(outcome, "Landed", new[] { .05f, .16f, .3f, .4f, .9f });
        Check(banner.IsShowing && banner.IsOutcome && banner.Outcome == LandingOutcomeKind.Landed && header.text == "SARDINE LANDED", "LANDED stamp or header missing.");
        strike.onClick.Invoke(); yield return null;
        Check(!owner.LandingOutcomePending && owner.AwaitingRigPlacement && UnityEngine.Object.FindAnyObjectByType<RigBuildingView>() != null, "Skip did not open Build.");
        Check(transition == null || transition.IsPlaying, "Landed-to-Build did not use the phase transition.");
        Check(card.anchoredPosition == cardRest && fade.alpha == 1 && !banner.IsShowing, "Landing left the creature panel or stamp displaced on exit.");
        TacklePhaseTransitionChecks.Settle();

        // 2. Placement receipt: the haul plaque rolls from the previous haul to the committed haul.
        var build = UnityEngine.Object.FindAnyObjectByType<RigBuildingView>(); var buildData = new SerializedObject(build);
        var haulLabel = (TMPro.TMP_Text)buildData.FindProperty("haul").objectReferenceValue;
        ((UnityEngine.UI.Button)buildData.FindProperty("commit").objectReferenceValue).onClick.Invoke();
        yield return SampleHaul(owner, haulLabel, "Placement");
        yield return WaitForDiscover(owner);

        // 3. Second species, landed straight through (deadline path, no skip).
        yield return Encounter(owner, "Cod", true);
        double deadline = Time.realtimeSinceStartupAsDouble + LandingOutcomeTiming.LandedDuration + 1;
        while (owner.LandingOutcomePending && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
        Check(!owner.LandingOutcomePending && owner.AwaitingRigPlacement, "Landed beat did not end at its own deadline.");
        TacklePhaseTransitionChecks.Settle(); yield return null;
        build = UnityEngine.Object.FindAnyObjectByType<RigBuildingView>(); buildData = new SerializedObject(build);
        ((UnityEngine.UI.Button)buildData.FindProperty("commit").objectReferenceValue).onClick.Invoke();
        yield return WaitForDiscover(owner);

        // 4. Duplicate: the automatic upgrade waits for the Landed beat, then its haul counts up.
        var sardine = Card("Sardine"); int level = owner.Rig.FindSpecies(sardine.UniqueId).Level;
        yield return Encounter(owner, "Sardine", true);
        Check(owner.LandingOutcomePending && owner.Rig.FindSpecies(sardine.UniqueId).Level == level, "Duplicate upgraded before its Landed beat.");
        strike.onClick.Invoke(); yield return null;
        Check(owner.Rig.FindSpecies(sardine.UniqueId).Level == level + 1 && owner.RigUpgradeFeedbackVisible, "Duplicate did not upgrade automatically after the Landed beat.");
        TacklePhaseTransitionChecks.Settle();
        build = UnityEngine.Object.FindAnyObjectByType<RigBuildingView>();
        yield return SampleHaul(owner, (TMPro.TMP_Text)new SerializedObject(build).FindProperty("haul").objectReferenceValue, "Upgrade");
        yield return WaitForDiscover(owner);

        // 5. Escape: the expedition is lost, the haul is paid once, then cashed out card by card.
        int gold = owner.TotalGold, haul = owner.Rig.HaulValue, members = owner.Rig.Members.Length;
        yield return Encounter(owner, "Cod", false);
        Check(!owner.RunActive && owner.LandingFailureVisible && owner.LandingOutcomePending && outcome.Timing.Kind == LandingOutcomeKind.Escaped, "Failure did not start the Escaped beat.");
        Check(owner.TotalGold == gold + haul && owner.LastHaulValue == haul && owner.Rig.Members.Length == 0, "Failure settlement changed or waited for the beat.");
        Check(!returnButton.gameObject.activeSelf && strike.gameObject.activeSelf, "Return to Harbor is offered before the payout.");
        returnButton.onClick.Invoke();
        Check(owner.LandingFailureVisible && landingView.gameObject.activeInHierarchy, "Return to Harbor bypassed the payout.");
        var shown = presentation.GetComponentsInChildren<TackleRigCardView>().Where(c => c.gameObject.activeSelf && c.BoundInstance != null).ToArray();
        Check(shown.Length == members && outcome.Timing.Cards == owner.LastHaul.Length, "The paid haul is not shown for its cash-out.");
        // Reduced motion: no thrash or sink, only a fade; numbers still count.
        var rush = presentation.GetComponent<LandingScoreRush>();
        var reduced = (UnityEngine.UI.Toggle)new SerializedObject(rush).FindProperty("reducedMotionToggle").objectReferenceValue;
        bool preference = reduced.isOn; reduced.SetIsOnWithoutNotify(true);
        owner.HoldLandingOutcomeForReview(); outcome.Seek(0); cardRest = card.anchoredPosition; var rotationRest = card.localRotation;
        try
        {
            outcome.Seek(.25f);
            Check(card.anchoredPosition == cardRest && card.localRotation == rotationRest && fade.alpha > 0 && fade.alpha < 1, "Reduced motion moved the escaping card or skipped its fade.");
        }
        finally { reduced.SetIsOnWithoutNotify(preference); }
        outcome.Seek(.5f); yield return null;
        Check(header.text == "COD ESCAPED", "Header does not name the escaped creature.");
        var timing = outcome.Timing;
        yield return Film(outcome, "Escaped", new[] { .17f, .5f, .9f, timing.CardAt(Mathf.Max(0, timing.Cards - 1)) + .05f, timing.Duration - .05f });
        yield return null; yield return null;
        Check(header.text == "EXPEDITION OVER" && outcome.DisplayedPayout == haul && payout.alpha == 1, "Payout readout or header is wrong at the end of the tally.");
        strike.onClick.Invoke(); yield return null;
        Check(!owner.LandingOutcomePending && returnButton.gameObject.activeSelf && returnButton.interactable, "Return to Harbor did not appear after the payout.");
        Check(outcome.DisplayedPayout == haul && payout.alpha == 1 && fade.alpha == 0 && owner.TotalGold == gold + haul, "Skipped payout did not land on the paid total exactly once.");
        Check(presentation.GetComponentsInChildren<TackleRigCardView>().All(c => !c.gameObject.activeSelf), "Rig column was not cleared after the cash-out.");
        SlotMachineSceneChecks.Capture(presentation.GetComponent<Canvas>(), "Escaped-Final.png", 1920, 1080, Output);
        returnButton.onClick.Invoke(); yield return null;
        Check(harbor.HubVisible && !landingView.gameObject.activeInHierarchy && fade.alpha == 1 && payout.alpha == 0, "Returning to Harbor left the escape visuals behind.");

        Result = "Passed " + checks + " live Landing outcome checks";
        File.WriteAllText(Output + "LiveVerification.json", "{\"passed\":true,\"assertions\":" + checks + "}");
        Debug.Log(Result);
    }

    /// <summary>Forces one creature through real Discover acceptance and real strikes, leaving the outcome beat running.</summary>
    private static IEnumerator Encounter(FishingRunController owner, string creature, bool succeed)
    {
        typeof(RigBuildingSceneChecks).GetMethod("ForceNextCreature", Hidden).Invoke(null, new object[] { owner, Card(creature) });
        yield return (IEnumerator)typeof(RigBuildingSceneChecks).GetMethod("Accept", Hidden).Invoke(null, new object[] { owner });
        var model = owner.Landing; double deadline = Time.realtimeSinceStartupAsDouble + 90;
        while (model.State == LandingChallengeState.Active && Time.realtimeSinceStartupAsDouble < deadline)
        {
            if (!owner.LandingFeedbackPending) model.AdvanceTo(Time.realtimeSinceStartupAsDouble);
            double distance = Math.Abs(model.FishPosition - model.ZonePosition);
            bool aligned = succeed ? distance < model.PerfectWidth * .15 : distance > (model.GoodWidth + model.FishWidth) * .5 + .05;
            if (model.CanStrike && aligned) owner.TryStrikeLanding(model.Occurrence, model.Revision);
            yield return null;
        }
        Check(model.State == (succeed ? LandingChallengeState.Succeeded : LandingChallengeState.Failed), creature + " did not reach the intended outcome.");
        owner.CompleteLandingFeedback(); yield return null;
    }

    /// <summary>Samples the Build haul plaque through a receipt: it must roll upward from the previous haul to the committed haul.</summary>
    private static IEnumerator SampleHaul(FishingRunController owner, TMPro.TMP_Text label, string name)
    {
        var receipt = owner.LastRigAcquisition; var seen = new List<int>(); bool captured = false;
        double deadline = Time.realtimeSinceStartupAsDouble + 3;
        while (owner.RigAcquisitionFeedbackVisible && Time.realtimeSinceStartupAsDouble < deadline)
        {
            if (int.TryParse(label.text.Replace("HAUL", "").Trim(), out int value) && (seen.Count == 0 || seen[seen.Count - 1] != value)) seen.Add(value);
            if (!captured && seen.Count >= 3 && value != receipt.Haul)
            { captured = true; SlotMachineSceneChecks.Capture(label.canvas.rootCanvas, "Build-" + name + "-Counting.png", 1920, 1080, Output); }
            yield return null;
        }
        Check(seen.Count > 0 && seen[0] == receipt.PreviousHaul && seen[seen.Count - 1] == receipt.Haul, name + " haul did not count from " + receipt.PreviousHaul + " to " + receipt.Haul + ".");
        Check(seen.Zip(seen.Skip(1), (a, b) => b > a).All(up => up) && (receipt.Haul - receipt.PreviousHaul < 3 || seen.Count >= 3), name + " haul jumped instead of rolling: " + string.Join(",", seen));
    }

    private static IEnumerator WaitForDiscover(FishingRunController owner)
    {
        double deadline = Time.realtimeSinceStartupAsDouble + 5;
        while ((owner.RigAcquisitionFeedbackVisible || !owner.SlotMachine.Active) && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
        Check(owner.SlotMachine.Active, "Build did not hand back to Discover.");
        TacklePhaseTransitionChecks.Settle(); yield return null;
    }

    /// <summary>Seeks the held beat at exact 60 fps ages, capturing the composited screen plus five review stills.</summary>
    private static IEnumerator Film(LandingOutcome outcome, string name, float[] stills)
    {
        string frames = "Temp/LandingOutcomePreview/" + name + "/";
        if (Directory.Exists(frames)) Directory.Delete(frames, true); Directory.CreateDirectory(frames);
        int count = Mathf.CeilToInt(outcome.Timing.Duration * 60);
        var still = stills.Select(t => Mathf.Clamp(Mathf.RoundToInt(t * 60), 0, count - 1)).ToArray();
        for (int i = 0; i < count; i++)
        {
            outcome.Seek(i / 60f);
            yield return new WaitForEndOfFrame();
            var shot = ScreenCapture.CaptureScreenshotAsTexture(); byte[] png = shot.EncodeToPNG(); UnityEngine.Object.Destroy(shot);
            File.WriteAllBytes(frames + i.ToString("000") + ".png", png);
            for (int s = 0; s < still.Length; s++) if (still[s] == i) File.WriteAllBytes(Output + name + "-" + (s + 1) + ".png", png);
        }
        outcome.Seek(outcome.Timing.Duration - .01f); outcome.Release();
    }

    private static IEnumerator Guarded(IEnumerator body)
    {
        var stack = new Stack<IEnumerator>(); stack.Push(body);
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
                Debug.LogError("Landing outcome check failed: " + inner); yield break;
            }
            if (current is IEnumerator nested) { stack.Push(nested); continue; }
            yield return current;
        }
    }

    private static CardDefinition Card(string name) => AssetDatabase.FindAssets("t:CardDefinition", new[] { "Assets/Cards" })
        .Select(g => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).Single(c => c.DisplayName == name);
    private static void Check(bool valid, string message) { if (!valid) { Result = "FAILED: " + message; throw new InvalidOperationException(message); } checks++; }
}
