using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>Checks visible integer pacing, stat polarity, masking and actual Catch Chain preview integration.</summary>
public static class JournalStatFeedbackChecks
{
    private const string DefaultOutput = "Docs/UIThemes/MidnightFieldJournal/StatFeedback/";
    private static string Output;
    private static JournalModalHost runner;
    private static GameObject fixture;
    private static bool oldBackground;
    private static float oldTimeScale;
    private static int assertions;
    private static string captureDirectory;
    public static string Status { get; private set; } = "Not started";
    [Serializable] private sealed class Report { public bool passed; public int assertions; public string error; public int[] valueSequence; public string captureDirectory; public float maximumStepScale, finalBeatScale; public bool positiveTilt, negativeTilt; }
    private static readonly List<int> sequence = new List<int>();
    private static float maximumStepScale, finalBeatScale;
    private static bool positiveTilt, negativeTilt;

    /// <summary>Runs a finite Play Mode check, preserving source assets and saved scenes.</summary>
    public static void Start(string outputFolder = null)
    {
        if (!Application.isPlaying || runner != null) throw new InvalidOperationException("Enter Play Mode and start once.");
        oldBackground = Application.runInBackground; oldTimeScale = Time.timeScale;
        Application.runInBackground = true; assertions = 0; sequence.Clear(); Status = "Running";
        Output = string.IsNullOrWhiteSpace(outputFolder) ? DefaultOutput : outputFolder;
        Directory.CreateDirectory(Output); maximumStepScale = finalBeatScale = 0; positiveTilt = negativeTilt = false;
        captureDirectory = Output + "Capture-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff") + "/";
        Directory.CreateDirectory(captureDirectory);
        runner = new GameObject("Stat Feedback Verification").AddComponent<JournalModalHost>(); runner.StartCoroutine(Guarded());
    }

    /// <summary>Reports failures and restores test-owned timing and objects.</summary>
    private static IEnumerator Guarded()
    {
        IEnumerator check = Check(); Exception failure = null;
        while (true)
        {
            bool more;
            try { more = check.MoveNext(); } catch (Exception e) { failure = e; break; }
            if (!more) break; yield return check.Current;
        }
        File.WriteAllText(Output + "PlayModeVerification.json", JsonUtility.ToJson(new Report { passed = failure == null,
            assertions = assertions, error = failure?.ToString(), valueSequence = sequence.ToArray(), captureDirectory = captureDirectory,
            maximumStepScale = maximumStepScale, finalBeatScale = finalBeatScale, positiveTilt = positiveTilt, negativeTilt = negativeTilt }, true));
        Application.runInBackground = oldBackground; Time.timeScale = oldTimeScale;
        if (fixture != null) UnityEngine.Object.Destroy(fixture);
        UnityEngine.Object.Destroy(runner.gameObject); runner = null;
        Status = failure == null ? "Passed" : "Failed: " + failure;
        if (failure != null) Debug.LogException(failure);
    }

    /// <summary>Exercises the opted-in prefab before verifying its real controller data path.</summary>
    private static IEnumerator Check()
    {
        fixture = new GameObject("Stat Feedback Fixture", typeof(RectTransform), typeof(Canvas));
        fixture.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var card = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UIThemes/MidnightFieldJournal/Prefabs/CreatureCard.prefab"), fixture.transform).GetComponent<CreatureCardView>();
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var weight = (TMP_Text)typeof(CreatureCardView).GetField("weightText", flags).GetValue(card);
        var value = (TMP_Text)typeof(CreatureCardView).GetField("valueText", flags).GetValue(card);
        var cod = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Coastal Cod.asset");
        var urchin = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Sea Urchin.asset");
        Color normal = value.color;
        Action<int, int, int, int, int> bind = (rev, bw, bv, w, v) => {
            card.SetCard(cod, w, v, false); card.PresentStatFeedback(new EncounterStatFeedback(rev, bw, bv)); };
        yield return null; yield return null; yield return null;
        Time.timeScale = 0;
        bind(1, 2, 8, 2, 12);
        Require(value.text == "8", "Modified stat skipped its baseline");
        Require(value.color.g > value.color.r && value.rectTransform.localScale.x > 1.2f, "Positive value lacks green enlargement");
        Require(weight.rectTransform.localScale == Vector3.one, "Unmodified Weight was emphasised");
        float began = Time.realtimeSinceStartup, deadline = began + 5;
        while (Time.realtimeSinceStartup < deadline)
        {
            int shown = int.Parse(value.text);
            if (shown > 8 && shown < 12) maximumStepScale = Mathf.Max(maximumStepScale, value.rectTransform.localScale.x);
            float tilt = Mathf.DeltaAngle(0, value.rectTransform.localEulerAngles.z);
            positiveTilt |= tilt > .5f; negativeTilt |= tilt < -.5f;
            if (sequence.Count == 0 || sequence[sequence.Count - 1] != shown) sequence.Add(shown);
            if (shown == 12) { finalBeatScale = value.rectTransform.localScale.x; break; }
            bind(1, 2, 8, 2, 12); yield return null;
        }
        Require(string.Join(",", sequence) == "8,9,10,11,12", "Integer sequence skipped or restarted under repeated refresh");
        Require(Time.realtimeSinceStartup - began >= .6f, "Counting was too fast to notice");
        Require(value.color.g > value.color.r && value.rectTransform.localScale.x > 1.2f, "Final modified number lost emphasis");
        Require(maximumStepScale > 1.35f, "Counting steps lack a noticeable pop");
        Require(positiveTilt && negativeTilt, "Counting beats did not alternate their tilt");
        Require(finalBeatScale > maximumStepScale + .04f, "Final landing was not stronger than ordinary beats");
        yield return new WaitForSecondsRealtime(.45f);
        Require(Mathf.Abs(value.rectTransform.localScale.x - 1.22f) < .001f
            && Quaternion.Angle(value.rectTransform.localRotation, Quaternion.identity) < .01f, "Punch did not settle to the persistent stat style");
        bind(1, 2, 8, 2, 8);
        Require(value.text == "12" && value.color.r > value.color.g, "Removing a value buff did not count down visibly");
        yield return new WaitForSecondsRealtime(1.2f);
        Require(value.text == "8" && value.color == normal && value.rectTransform.localScale == Vector3.one, "Baseline recovery did not restore defaults");
        bind(2, 3, 12, 1, 8);
        Require(weight.text == "3" && weight.color.g > weight.color.r, "Weight reduction was not green or missed baseline");
        Require(value.text == "12" && value.color.r > value.color.g, "Value penalty was not red");
        yield return new WaitForSecondsRealtime(1.2f);
        Require(weight.text == "1" && value.text == "8", "Decreasing stats failed to finish");
        bind(3, 1, 8, 3, 8);
        Require(weight.color.r > weight.color.g && weight.rectTransform.localScale.x > 1.2f, "Weight penalty was not red/enlarged");
        bind(4, 2, 8, 2, 12); yield return new WaitForSecondsRealtime(.3f);
        string before = value.text; bind(4, 2, 8, 2, 10);
        Require(value.text == before, "Retarget snapped to a new baseline or target");
        bind(4, 2, 12, 2, 12);
        Require(value.text == before && value.color.g > value.color.r, "A rising non-chain baseline incorrectly reversed the remaining count's colour");
        bind(5, 2, 8, 2, 12);
        Require(value.text == "8", "Same-species new occurrence did not restart at its own baseline");
        card.SetCard(cod, 2, 0, false); card.SelectEffectChoice(EncounterEffectChoice.Bait, false);
        card.PresentStatFeedback(new EncounterStatFeedback(6, 2, 0));
        Require(value.text == "0" && value.color == normal && value.rectTransform.localScale == Vector3.one, "Bait's zero sale value was treated as a penalty");
        card.SetCard(cod, 3, 20, true, true); card.PresentStatFeedback(new EncounterStatFeedback(7, 2, 8));
        yield return new WaitForSecondsRealtime(.4f);
        Require(weight.text == "?" && value.text == "?" && value.color == normal, "Animation exposed hidden information");
        card.SetCard(cod, 2, 18, false); yield return new WaitForSecondsRealtime(.4f);
        Require(value.text == "18" && value.rectTransform.localScale == Vector3.one, "Unanimated snapshot was overwritten by an old counter");
        bind(8, 2, 8, 2, 12); card.SetStatFeedbackPaused(true);
        yield return new WaitForSecondsRealtime(.5f); Require(value.text == "8", "Incoming transition consumed unseen count steps");
        card.SetStatFeedbackPaused(false); card.CompleteStatFeedback();
        Require(value.text == "12" && value.rectTransform.localScale == Vector3.one, "Outgoing snapshot did not freeze authoritative values");
        fixture.SetActive(false);
        Time.timeScale = oldTimeScale;

        var controller = UnityEngine.Object.FindFirstObjectByType<FishingRunController>();
        var view = UnityEngine.Object.FindFirstObjectByType<FishingRunView>();
        var chain = (CatchChainRuntime)typeof(FishingRunController).GetField("catchChainRuntime", flags).GetValue(controller);
        var resolver = (EffectResolver)typeof(FishingRunController).GetField("effectResolver", flags).GetValue(controller);
        var runtime = (EncounterRuntime)typeof(FishingRunController).GetField("encounterRuntime", flags).GetValue(controller);
        var refresh = typeof(FishingRunController).GetMethod("RefreshViews", flags);
        controller.StartRun(); controller.TryDescend();
        while (view.IsEncounterTransitionPlaying) yield return null;
        chain.Add(AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Spiny Lobster.asset"), resolver, CatchAttachmentRole.Bait);
        Action<CardInstance> pending = p => p.AddPermanentModifiers(1, 8);
        var resolved = chain.CreateResolvedCatchPreview(urchin, resolver, CatchAttachmentRole.Catch, pending);
        var baseline = chain.CreateResolvedCatchPreview(urchin, resolver, CatchAttachmentRole.Catch, pending, false);
        Require(baseline.CurrentWeight == 2 && resolved.CurrentWeight == 1, "Baseline failed to isolate Lobster from pending Technique modifiers");
        Require(baseline.CurrentValue == resolved.CurrentValue && chain.Catches.Length == 1, "Presentation baseline changed unrelated value or live chain");
        runtime.SetCurrentEncounter(urchin); refresh.Invoke(controller, null);
        var current = (CreatureCardView)typeof(FishingRunView).GetField("encounterCardView", flags).GetValue(view);
        var currentWeight = (TMP_Text)typeof(CreatureCardView).GetField("weightText", flags).GetValue(current);
        Require(currentWeight.text == "1" && currentWeight.color.g > currentWeight.color.r, "Controller preview did not animate Lobster's reduction from base Weight");
        yield return new WaitForSecondsRealtime(.5f);
        Require(currentWeight.text == "0", "Controller stat did not reach authoritative Weight");
        ScreenCapture.CaptureScreenshot(captureDirectory + "WeightReduction4K.png"); yield return null;
        chain.Reset(); var garfish = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Garfish.asset");
        chain.Add(AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Tidepool Eel.asset"), resolver);
        runtime.SetCurrentEncounter(garfish); refresh.Invoke(controller, null);
        current = (CreatureCardView)typeof(FishingRunView).GetField("encounterCardView", flags).GetValue(view);
        var currentValue = (TMP_Text)typeof(CreatureCardView).GetField("valueText", flags).GetValue(current);
        Require(currentValue.text == "9" && currentValue.color.g > currentValue.color.r, "Garfish's chain-dependent own effect missed feedback");
        yield return new WaitForSecondsRealtime(.9f);
        Require(currentValue.text == "12", "Garfish failed to reach chain-resolved Value");
        ScreenCapture.CaptureScreenshot(captureDirectory + "ValueIncrease4K.png"); yield return null;
        controller.StartRun(); yield return null;
        Require(controller.IsOpeningBoatVisible, "Verification failed to reset run");
    }

    /// <summary>Stops on the first visual or integration regression.</summary>
    private static void Require(bool value, string message) { assertions++; if (!value) throw new InvalidOperationException(message); }
}
