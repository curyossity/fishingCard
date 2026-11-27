using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>Validates the creature hurt reaction and Resistance damage chip through real strikes with persistence disabled, and films review frames.</summary>
public static class LandingDamageFeedbackChecks
{
    public const string Output = "Docs/Validation/DamageFeedback/";
    private const BindingFlags Hidden = BindingFlags.Static | BindingFlags.NonPublic, Field = BindingFlags.Instance | BindingFlags.NonPublic;
    public static string Result { get; private set; } = "Not run";
    private static int checks;

    /// <summary>Pure reaction rules: severity follows the Resistance share, every channel decays to rest, the chip holds then fades.</summary>
    [MenuItem("Fishing Cards/Validation/Landing Damage Feedback (Timing)")]
    public static int ValidateTiming()
    {
        checks = 0;
        Check(LandingDamageFeedback.Severity(10, 10, 40) == 0 && LandingDamageFeedback.Severity(0, 20, 0) == 0, "A strike without damage reacts.");
        float small = LandingDamageFeedback.Severity(0, 4, 100), large = LandingDamageFeedback.Severity(0, 40, 100), finish = LandingDamageFeedback.Severity(90, 101, 100);
        Check(small > 0 && small < large && finish >= .85f, "Severity does not scale with the Resistance share or lift the finishing blow.");
        float after = LandingDamageFeedback.Seconds + .001f;
        Check(LandingDamageFeedback.Knockback(after, 1) == Vector2.zero && LandingDamageFeedback.Tilt(after, 1) == 0 && LandingDamageFeedback.Squash(after, 1) == Vector2.one
            && LandingDamageFeedback.HurtFlash(after, 1, false) == 0 && LandingDamageFeedback.FishThrash(after, 1) == 0, "Hurt reaction does not return to rest.");
        Check(LandingDamageFeedback.Knockback(.05f, 1).x > 8 && LandingDamageFeedback.Knockback(.05f, .2f).x < LandingDamageFeedback.Knockback(.05f, 1).x, "Creature card is not knocked away from the Strike panel, scaled by severity.");
        var span = LandingDamageFeedback.ChipSpan(10, 30, 40);
        Check(Mathf.Approximately(span.x, .25f) && Mathf.Approximately(span.y, .75f), "Damage chip does not mark the lost part of the draining bar.");
        Check(LandingDamageFeedback.HurtFlash(.018f, 1, true) < LandingDamageFeedback.HurtFlash(.018f, 1, false), "Reduced motion does not soften the hurt flash.");
        Check(LandingDamageFeedback.Seconds <= .36f, "Reaction outlasts the shortest post-payout feedback tail.");
        // Health trail: red eases down first; the white trail holds on the old value, then eases down after it, never ahead of it.
        foreach (float intensity in new[] { 0f, .5f, 1f })
        {
            bool lags = true; float previousTrail = 0;
            for (float t = 0; t <= LandingHealthTrail.End(intensity) + .05f; t += 1 / 240f)
            {
                float red = LandingHealthTrail.Red(t, intensity), trail = LandingHealthTrail.Trail(t, intensity);
                lags &= trail <= red + 1e-5f && trail >= previousTrail; previousTrail = trail;
            }
            Check(lags && LandingHealthTrail.Red(LandingHealthTrail.RedSeconds(intensity), intensity) == 1 && LandingHealthTrail.Trail(LandingHealthTrail.TrailStart(intensity), intensity) == 0
                && LandingHealthTrail.Trail(LandingHealthTrail.End(intensity), intensity) == 1, "Health trail does not ease the red down first and then the white after it.");
        }
        var trailAtHit = LandingDamageFeedback.Trail(10, 30, 40, .02f, 0);
        Check(trailAtHit.y == .75f && trailAtHit.x < .75f && trailAtHit.x > .25f, "White trail does not hold the old Resistance while the red drains.");
        // Control waits for the whole drain on a damaging strike; a zero-point Miss keeps its short tail.
        var damaging = new LandingScoreSequence(new LandingScore(LandingAccuracy.Perfect, 20, 0, 1, Array.Empty<LandingContribution>()));
        var miss = new LandingScoreSequence(new LandingScore(LandingAccuracy.Miss, 0, 0, 1, Array.Empty<LandingContribution>()));
        Check(damaging.Duration - damaging.PayoutAt >= LandingHealthTrail.End(damaging.Intensity) && miss.Duration - miss.PayoutAt < LandingHealthTrail.End(0),
            "Feedback releases control before the health bar settles, or delays a Miss.");
        return checks;
    }

    /// <summary>Starts the live sequence; requires a fresh comparison Play session.</summary>
    [MenuItem("Fishing Cards/Validation/Landing Damage Feedback (Live)")]
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
        typeof(RigBuildingSceneChecks).GetMethod("ForceNextCreature", Hidden).Invoke(null, new object[] { owner, Card("Cod") });
        yield return (IEnumerator)typeof(RigBuildingSceneChecks).GetMethod("Accept", Hidden).Invoke(null, new object[] { owner });
        yield return null;
        var presentation = UnityEngine.Object.FindAnyObjectByType<TackleLandingPresentation>();
        var rush = presentation.GetComponent<LandingScoreRush>(); var damage = presentation.GetComponent<LandingDamageFeedback>();
        var data = new SerializedObject(damage);
        var card = (RectTransform)data.FindProperty("creatureCard").objectReferenceValue;
        var flash = (LandingPanelFlash)data.FindProperty("creatureFlash").objectReferenceValue;
        var chip = (UnityEngine.UI.Image)data.FindProperty("chip").objectReferenceValue;
        var rushData = new SerializedObject(rush);
        var reduced = (UnityEngine.UI.Toggle)rushData.FindProperty("reducedMotionToggle").objectReferenceValue;
        var soundToggle = (UnityEngine.UI.Toggle)rushData.FindProperty("soundToggle").objectReferenceValue;
        bool reducedPreference = reduced.isOn, soundPreference = soundToggle.isOn;
        reduced.SetIsOnWithoutNotify(false);
        Vector2 cardRest = card.anchoredPosition; Quaternion rotationRest = card.localRotation; Vector3 scaleRest = card.localScale;
        try
        {
            // A real Perfect that does not finish the creature: points transfer as damage and the challenge continues.
            yield return Strike(owner, LandingAccuracy.Perfect);
            var receipt = owner.LastLandingStrike;
            Check(owner.LandingFeedbackPending && owner.Landing.State == LandingChallengeState.Active && receipt.PointsAfter > receipt.PointsBefore, "Fixture strike did not deal non-finishing damage.");
            typeof(FishingRunController).GetField("landingFeedbackUntil", Field).SetValue(owner, double.MaxValue); // Hold the gate for frame capture.
            soundToggle.SetIsOnWithoutNotify(false);
            var sequence = rush.Sequence; float payout = sequence.PayoutAt;
            float severity = LandingDamageFeedback.Severity(receipt.PointsBefore, receipt.PointsAfter, receipt.Resistance);
            yield return SeekRush(rush, payout - .05f);
            Check(!chip.gameObject.activeSelf, "Damage chip appeared before the points transferred.");
            yield return SeekRush(rush, payout + .04f);
            Check(card.anchoredPosition.x > cardRest.x + 3, "Creature card did not flinch away from the Strike panel.");
            Check(flash.color.r > .9f && flash.color.g < .6f && flash.color.a > .2f, "Creature panel did not take a hurt tint.");
            float from = 1 - (float)(receipt.PointsAfter / receipt.Resistance), to = 1 - (float)(receipt.PointsBefore / receipt.Resistance);
            float intensity = sequence.Intensity;
            // Early: the red is draining and the white trail still holds the old Resistance.
            Check(chip.gameObject.activeSelf && Mathf.Abs(chip.rectTransform.anchorMax.x - to) < .001f && chip.rectTransform.anchorMin.x > from + .001f && chip.rectTransform.anchorMin.x < to - .001f,
                "White trail does not hold the old Resistance while the red drains.");
            yield return SeekRush(rush, payout + LandingHealthTrail.TrailStart(intensity) - .02f);
            Check(chip.gameObject.activeSelf && Mathf.Abs(chip.rectTransform.anchorMin.x - from) < .001f && Mathf.Abs(chip.rectTransform.anchorMax.x - to) < .001f, "Red did not settle on the new Resistance before the trail drains.");
            yield return SeekRush(rush, payout + LandingHealthTrail.TrailStart(intensity) + LandingHealthTrail.DrainSeconds * .5f);
            Check(chip.gameObject.activeSelf && chip.rectTransform.anchorMax.x < to - .01f && chip.rectTransform.anchorMax.x > from + .01f && owner.LandingFeedbackPending, "White trail does not ease down while control is still held.");
            Check(sequence.Duration - payout >= LandingHealthTrail.End(intensity), "Feedback gate can release control before the trail settles.");
            reduced.SetIsOnWithoutNotify(true);
            yield return SeekRush(rush, payout + .04f);
            Check((card.anchoredPosition - cardRest).sqrMagnitude < .01f && card.localRotation == rotationRest && flash.color.a > 0 && chip.gameObject.activeSelf, "Reduced motion moved the card or dropped the tint and chip.");
            reduced.SetIsOnWithoutNotify(false);
            // The rush ends itself at its sequence duration (after the trail settles), so every seek stays inside that tail.
            float end = LandingHealthTrail.End(intensity);
            yield return Film(rush, "Strike", payout - .12f, payout + end + .04f, severity);
            yield return SeekRush(rush, payout + end + .02f);
            Check(!chip.gameObject.activeSelf && (card.anchoredPosition - cardRest).sqrMagnitude < .01f, "Reaction or trail did not settle before the feedback tail ended.");
            owner.CompleteLandingFeedback(); yield return null;
            Check(!chip.gameObject.activeSelf && (card.anchoredPosition - cardRest).sqrMagnitude < .01f && card.localScale == scaleRest && flash.color.a == 0, "Skipping left the reaction or chip behind.");

            // A Miss deals no damage: no flinch, tint or chip.
            yield return Strike(owner, LandingAccuracy.Miss);
            receipt = owner.LastLandingStrike;
            Check(receipt.PointsAfter == receipt.PointsBefore, "Fixture Miss dealt damage.");
            typeof(FishingRunController).GetField("landingFeedbackUntil", Field).SetValue(owner, double.MaxValue);
            yield return SeekRush(rush, rush.Sequence.PayoutAt + .04f);
            Check(!chip.gameObject.activeSelf && (card.anchoredPosition - cardRest).sqrMagnitude < .01f, "A Miss produced a damage reaction.");
            owner.CompleteLandingFeedback(); yield return null;
        }
        finally { reduced.SetIsOnWithoutNotify(reducedPreference); soundToggle.SetIsOnWithoutNotify(soundPreference); }

        Result = "Passed " + checks + " live damage feedback checks";
        File.WriteAllText(Output + "LiveVerification.json", "{\"passed\":true,\"assertions\":" + checks + "}");
        Debug.Log(Result);
    }

    /// <summary>Re-bases the rush clock so its own Tick renders the requested sequence time this frame, then waits for that frame.</summary>
    private static IEnumerator SeekRush(LandingScoreRush rush, float sequenceTime)
    {
        double real = rush.Timing.ToReal(sequenceTime);
        typeof(LandingScoreRush).GetField("started", Field).SetValue(rush, Time.realtimeSinceStartupAsDouble - real);
        yield return new WaitForEndOfFrame();
    }

    /// <summary>Captures the payout window at 60 fps of sequence time plus five stills.</summary>
    private static IEnumerator Film(LandingScoreRush rush, string name, float from, float to, float severity)
    {
        string frames = "Temp/DamageFeedbackPreview/" + name + "/";
        if (Directory.Exists(frames)) Directory.Delete(frames, true); Directory.CreateDirectory(frames);
        int count = Mathf.CeilToInt((to - from) * 60); float payout = rush.Sequence.PayoutAt;
        float strength = rush.Sequence.Intensity, trail = LandingHealthTrail.TrailStart(strength);
        // Before the hit, red draining under the held white trail, red settled, white trail draining, settled.
        var stills = new[] { payout - .05f, payout + .05f, payout + trail - .03f, payout + trail + LandingHealthTrail.DrainSeconds * .5f, payout + LandingHealthTrail.End(strength) + .02f }
            .Select(t => Mathf.RoundToInt((t - from) * 60)).ToArray();
        for (int i = 0; i < count; i++)
        {
            yield return SeekRush(rush, from + i / 60f);
            var shot = ScreenCapture.CaptureScreenshotAsTexture(); byte[] png = shot.EncodeToPNG(); UnityEngine.Object.Destroy(shot);
            File.WriteAllBytes(frames + i.ToString("000") + ".png", png);
            for (int s = 0; s < stills.Length; s++) if (stills[s] == i) File.WriteAllBytes(Output + name + "-" + (s + 1) + ".png", png);
        }
    }

    /// <summary>Waits for an observed alignment and uses the guarded real strike command.</summary>
    private static IEnumerator Strike(FishingRunController owner, LandingAccuracy accuracy)
    {
        var model = owner.Landing; double until = Time.realtimeSinceStartupAsDouble + 30; long revision = model.Revision;
        while (model.Revision == revision && Time.realtimeSinceStartupAsDouble < until)
        {
            if (!owner.LandingFeedbackPending) model.AdvanceTo(Time.realtimeSinceStartupAsDouble);
            double distance = Math.Abs(model.FishPosition - model.ZonePosition);
            if (!owner.LandingFeedbackPending && model.CanStrike && (accuracy == LandingAccuracy.Perfect ? distance < model.PerfectWidth * .15 : distance > (model.GoodWidth + model.FishWidth) * .5 + .05))
                owner.TryStrikeLanding(model.Occurrence, model.Revision);
            if (model.Revision == revision) yield return null;
        }
        Check(model.Revision > revision, "Timed out waiting for a " + accuracy + " strike.");
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
                Debug.LogError("Damage feedback check failed: " + inner); yield break;
            }
            if (current is IEnumerator nested) { stack.Push(nested); continue; }
            yield return current;
        }
    }

    private static CardDefinition Card(string name) => AssetDatabase.FindAssets("t:CardDefinition", new[] { "Assets/Cards" })
        .Select(g => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).Single(c => c.DisplayName == name);
    private static void Check(bool valid, string message) { if (!valid) { Result = "FAILED: " + message; throw new InvalidOperationException(message); } checks++; }
}
