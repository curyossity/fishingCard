using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Validates the floating damage numbers, the final damage slam and the identity-only Strike panel through a real strike with rig
/// contributors (persistence disabled), and films review frames.
/// </summary>
public static class LandingDamageNumbersChecks
{
    public const string Output = "Docs/Validation/DamageNumbers/";
    private const BindingFlags Hidden = BindingFlags.Static | BindingFlags.NonPublic, Field = BindingFlags.Instance | BindingFlags.NonPublic;
    public static string Result { get; private set; } = "Not run";
    private static int checks;

    /// <summary>Pure rules: labels per step kind, a pop that settles, drift that fans out, a fade that ends, a slam that lands on the payout.</summary>
    [MenuItem("Fishing Cards/Validation/Landing Damage Numbers (Timing)")]
    public static int ValidateTiming()
    {
        checks = 0;
        var sequence = new LandingScoreSequence(new LandingScore(LandingAccuracy.Perfect, 20, 5, 2,
            new[] { new LandingContribution(4, "Add", 5, 1), new LandingContribution(8, "Multiply", 0, 2) }));
        Check(LandingDamageNumbers.Label(sequence, 0) == "+20" && LandingDamageNumbers.Label(sequence, 1) == "+5" && LandingDamageNumbers.Label(sequence, 2) == "×2", "Floating numbers do not name the hit's own effect.");
        Check(LandingDamageNumbers.ColorFor(sequence, 0) != LandingDamageNumbers.ColorFor(sequence, 1) && LandingDamageNumbers.ColorFor(sequence, 1) != LandingDamageNumbers.ColorFor(sequence, 2), "Base, addition and multiplier numbers share a colour.");
        var miss = new LandingScoreSequence(new LandingScore(LandingAccuracy.Miss, 0, 0, 1, Array.Empty<LandingContribution>()));
        Check(!LandingDamageNumbers.Shows(miss, 0) && LandingDamageNumbers.Shows(sequence, 0), "A zero-point Miss floats a number.");
        Check(LandingDamageNumbers.Pop(.059f, 1) > LandingDamageNumbers.Pop(.5f, 1) && LandingDamageNumbers.Pop(.01f, 1) < LandingDamageNumbers.Pop(.059f, 1) && Mathf.Approximately(LandingDamageNumbers.Pop(.5f, 0), 1) && LandingDamageNumbers.Pop(.5f, 1) > LandingDamageNumbers.Pop(.5f, 0), "Numbers do not pop and settle, or ignore strength.");
        Check(LandingDamageNumbers.Drift(.6f, 0).y > 40 && LandingDamageNumbers.Drift(.6f, 1).x != LandingDamageNumbers.Drift(.6f, 2).x, "Numbers do not rise or fan apart.");
        Check(LandingDamageNumbers.Opacity(0) == 1 && LandingDamageNumbers.Opacity(LandingDamageNumbers.Life) == 0, "Numbers do not appear on contact and fade out.");
        float payout = sequence.PayoutAt;
        Check(LandingDamageNumbers.SlamFall(payout - LandingDamageNumbers.SlamDrop, payout) == 0 && LandingDamageNumbers.SlamFall(payout, payout) == 1
            && LandingDamageNumbers.SlamScale(payout - LandingDamageNumbers.SlamDrop, payout) > 2 && LandingDamageNumbers.SlamScale(payout + LandingDamageNumbers.SlamPunch + .01f, payout) == 1,
            "The slam does not drop large and land at full size exactly at the payout.");
        Check(LandingDamageNumbers.SlamOpacity(payout - LandingDamageNumbers.SlamDrop - .01f, payout) == 0 && LandingDamageNumbers.SlamOpacity(payout, payout) == 1
            && LandingDamageNumbers.SlamOpacity(payout + LandingDamageNumbers.SlamHold + LandingDamageNumbers.SlamFade, payout) == 0, "The slam does not appear, hold and fade.");
        Check(payout + LandingDamageNumbers.SlamHold + LandingDamageNumbers.SlamFade <= sequence.Duration, "The slam outlasts the strike's feedback.");
        Check(LandingDamageNumbers.ClearForSlam(payout - LandingDamageNumbers.SlamDrop, payout) == 1 && LandingDamageNumbers.ClearForSlam(payout, payout) == 0, "Floating numbers do not clear for the slam.");
        Check(payout - LandingDamageNumbers.SlamDrop >= sequence.Steps.Last().ImpactAt, "The slam drops before the last ram.");
        return checks;
    }

    [MenuItem("Fishing Cards/Validation/Landing Damage Numbers (Live)")]
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
        // Disposable fixture rig, seeded through the transactional rig owner before the encounter is accepted.
        foreach (string name in new[] { "Sardine", "Cuttlefish", "Stormjaw Tuna" })
            Check(owner.Rig.TryAcquire(owner.Rig.Revision, Card(name), owner.Rig.Members.Length, out _, out _), "Could not seed the contributor fixture.");
        typeof(RigBuildingSceneChecks).GetMethod("ForceNextCreature", Hidden).Invoke(null, new object[] { owner, Card("Cod") });
        yield return (IEnumerator)typeof(RigBuildingSceneChecks).GetMethod("Accept", Hidden).Invoke(null, new object[] { owner });
        yield return null;
        var presentation = UnityEngine.Object.FindAnyObjectByType<TackleLandingPresentation>();
        var rush = presentation.GetComponent<LandingScoreRush>(); var numbers = presentation.GetComponent<LandingDamageNumbers>();
        var data = new SerializedObject(numbers);
        var slam = (CanvasGroup)data.FindProperty("slam").objectReferenceValue;
        var slamValue = (TMPro.TMP_Text)data.FindProperty("slamValue").objectReferenceValue;
        var portrait = (RectTransform)data.FindProperty("slamTarget").objectReferenceValue;
        var layer = (RectTransform)data.FindProperty("layer").objectReferenceValue;
        var panel = new SerializedObject(presentation.GetComponent<LandingStrikePanel>());
        var face = (GameObject)panel.FindProperty("face").objectReferenceValue;
        var label = (TMPro.TMP_Text)panel.FindProperty("label").objectReferenceValue;
        var rushData = new SerializedObject(rush);
        var soundToggle = (UnityEngine.UI.Toggle)rushData.FindProperty("soundToggle").objectReferenceValue;
        bool sound = soundToggle.isOn;
        try
        {
            yield return Strike(owner);
            var sequence = rush.Sequence;
            Check(owner.LandingFeedbackPending && sequence.HasContributors, "Fixture strike has no rig contributors.");
            typeof(FishingRunController).GetField("landingFeedbackUntil", Field).SetValue(owner, double.MaxValue);
            soundToggle.SetIsOnWithoutNotify(false);
            foreach (string part in new[] { "value", "operation", "caption", "glow" })
                Check(!((Component)rushData.FindProperty(part).objectReferenceValue).gameObject.activeSelf, "The Strike panel still shows its own " + part + ".");
            // Base ram: the strike face (hook and grade) on the left, "+20" floating off the creature, no slam yet.
            var first = sequence.Steps[0];
            yield return Seek(rush, first.ImpactAt + .05f);
            Check(face.activeSelf && !ContributorShown(presentation), "The accuracy hit does not show the hook and grade.");
            Check(Visible(layer).Contains(LandingDamageNumbers.Label(sequence, 0)), "The base ram did not float its number.");
            Check(slam.alpha == 0, "The final slam shows during the rams.");
            // Each contributor ram: only the creature on the left, its own effect floating, no slam yet.
            for (int i = 1; i < sequence.Steps.Count; i++)
            {
                var step = sequence.Steps[i];
                yield return Seek(rush, step.ImpactAt + .05f);
                Check(!face.activeSelf && ContributorShown(presentation), "A contributor ram does not show only the rig creature.");
                Check(Visible(layer).Contains(LandingDamageNumbers.Label(sequence, i)), "Contributor ram " + i + " did not float its effect.");
                Check(slam.alpha == 0, "The final slam shows during the rams.");
            }
            // The total slams onto the creature's portrait exactly at the payout, as the bar starts to drain; floats have cleared.
            yield return Seek(rush, sequence.PayoutAt - LandingDamageNumbers.SlamDrop * .5f);
            var rect = (RectTransform)slam.transform; float midScale = rect.localScale.x;
            yield return Seek(rush, sequence.PayoutAt + .005f);
            Vector2 target = ((RectTransform)rect.parent).InverseTransformPoint(portrait.TransformPoint(portrait.rect.center));
            Check(slam.alpha > .9f && slamValue.text == LandingScoreRush.Format(sequence.Score.Points) && midScale > 1.1f, "The total did not drop in large and land.");
            // The card has already begun recoiling from the impact (knockback), so allow for that offset.
            Check((rect.anchoredPosition - target).sqrMagnitude < 30 * 30, "The slam did not land on the creature's portrait.");
            Check(Visible(layer).Count == 0, "Floating numbers did not clear for the slam.");
            Check(owner.LandingFeedbackPending, "Control returned before the slam landed.");
            yield return Film(rush, "Combo", sequence.Steps[0].ImpactAt - .1f, sequence.PayoutAt + LandingDamageNumbers.SlamHold + LandingDamageNumbers.SlamFade);
            owner.CompleteLandingFeedback(); yield return null;
            Check(Visible(layer).Count == 0 && slam.alpha == 0 && label.text != "", "Skipping left floating numbers or the slam behind.");
        }
        finally { soundToggle.SetIsOnWithoutNotify(sound); }
        Result = "Passed " + checks + " live damage number checks";
        File.WriteAllText(Output + "LiveVerification.json", "{\"passed\":true,\"assertions\":" + checks + "}");
        Debug.Log(Result);
    }

    private static bool ContributorShown(TackleLandingPresentation presentation) => presentation.GetComponent<LandingScoringPortrait>().IsShowingContributors;

    private static List<string> Visible(RectTransform layer) => layer.GetComponentsInChildren<TMPro.TMP_Text>()
        .Where(t => t.name == "Face" && t.transform.parent.name.StartsWith("Floating damage ") && t.alpha > .05f).Select(t => t.text).ToList();

    private static IEnumerator Seek(LandingScoreRush rush, float sequenceTime)
    {
        typeof(LandingScoreRush).GetField("started", Field).SetValue(rush, Time.realtimeSinceStartupAsDouble - rush.Timing.ToReal(sequenceTime));
        yield return new WaitForEndOfFrame();
    }

    private static IEnumerator Film(LandingScoreRush rush, string name, float from, float to)
    {
        string frames = "Temp/DamageNumbersPreview/" + name + "/";
        if (Directory.Exists(frames)) Directory.Delete(frames, true); Directory.CreateDirectory(frames);
        int count = Mathf.CeilToInt((to - from) * 60);
        var seq = rush.Sequence;
        var stills = new List<float> { seq.Steps[0].ImpactAt + .08f };
        for (int i = 1; i < seq.Steps.Count && stills.Count < 4; i++) stills.Add(seq.Steps[i].ImpactAt + .08f);
        stills.Add(seq.PayoutAt - LandingDamageNumbers.SlamDrop * .4f); stills.Add(seq.PayoutAt + .03f);
        var frameAt = stills.Select(t => Mathf.RoundToInt((t - from) * 60)).ToArray();
        for (int i = 0; i < count; i++)
        {
            yield return Seek(rush, from + i / 60f);
            var shot = ScreenCapture.CaptureScreenshotAsTexture(); byte[] png = shot.EncodeToPNG(); UnityEngine.Object.Destroy(shot);
            File.WriteAllBytes(frames + i.ToString("000") + ".png", png);
            for (int s = 0; s < frameAt.Length; s++) if (frameAt[s] == i) File.WriteAllBytes(Output + name + "-" + (s + 1) + ".png", png);
        }
    }

    private static IEnumerator Strike(FishingRunController owner)
    {
        var model = owner.Landing; double until = Time.realtimeSinceStartupAsDouble + 30; long revision = model.Revision;
        while (model.Revision == revision && Time.realtimeSinceStartupAsDouble < until)
        {
            if (!owner.LandingFeedbackPending) model.AdvanceTo(Time.realtimeSinceStartupAsDouble);
            if (!owner.LandingFeedbackPending && model.CanStrike && Math.Abs(model.FishPosition - model.ZonePosition) < model.PerfectWidth * .15) owner.TryStrikeLanding(model.Occurrence, model.Revision);
            if (model.Revision == revision) yield return null;
        }
        Check(model.Revision > revision, "Timed out waiting for a Perfect strike.");
        yield return null;
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
                Debug.LogError("Damage number check failed: " + inner); yield break;
            }
            if (current is IEnumerator nested) { stack.Push(nested); continue; }
            yield return current;
        }
    }

    private static CardDefinition Card(string name) => AssetDatabase.FindAssets("t:CardDefinition", new[] { "Assets/Cards" })
        .Select(g => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).Single(c => c.DisplayName == name);
    private static void Check(bool valid, string message) { if (!valid) { Result = "FAILED: " + message; throw new InvalidOperationException(message); } checks++; }
}
