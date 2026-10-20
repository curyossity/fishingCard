using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>Checks visible integer pacing, role changes, masking and resolved feedback from chain, Technique and event effects.</summary>
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
    [Serializable] private sealed class Report { public bool passed; public int assertions; public string error; public int[] valueSequence; public string captureDirectory; public float maximumStepScale, finalBeatScale, countDuration; public bool positiveTilt, negativeTilt; }
    private static float countDuration;
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
            maximumStepScale = maximumStepScale, finalBeatScale = finalBeatScale, countDuration = countDuration, positiveTilt = positiveTilt, negativeTilt = negativeTilt }, true));
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
        FontStyles normalStyle = value.fontStyle;
        FontWeight normalWeight = value.fontWeight;
        Action<int, int, int, int, int> bind = (rev, bw, bv, w, v) => {
            card.SetCard(cod, w, v, false); card.PresentStatFeedback(new EncounterStatFeedback(rev, bw, bv)); };
        yield return null; yield return null; yield return null;
        Time.timeScale = 0;
        bind(1, 2, 8, 2, 12);
        Require(value.text == "8", "Modified stat skipped its baseline");
        Require(value.color.g > value.color.r && value.rectTransform.localScale.x > 1.2f, "Positive value lacks green enlargement");
        Require((value.fontStyle & FontStyles.Bold) != 0 && value.fontWeight == FontWeight.Bold,
            "Manipulated stat was not bold during the animation");
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
        countDuration = Time.realtimeSinceStartup - began;
        Require(countDuration >= .15f && countDuration < .5f, "Short count missed the slightly slower rapid timing");
        Require(value.color.g > value.color.r && value.rectTransform.localScale.x > 1.2f, "Final modified number lost emphasis");
        Require(maximumStepScale > 1.35f, "Counting steps lack a noticeable pop");
        Require(positiveTilt && negativeTilt, "Counting beats did not alternate their tilt");
        Require(finalBeatScale > maximumStepScale + .04f, "Final landing was not stronger than ordinary beats");
        yield return new WaitForSecondsRealtime(.45f);
        Require(Mathf.Abs(value.rectTransform.localScale.x - 1.22f) < .001f
            && Quaternion.Angle(value.rectTransform.localRotation, Quaternion.identity) < .01f, "Punch did not settle to the persistent stat style");
        Require((value.fontStyle & FontStyles.Bold) != 0 && value.fontWeight == FontWeight.Bold,
            "Modified result lost its bold weight after settling");
        bind(1, 2, 8, 2, 8);
        Require(value.text == "12" && value.color.g > value.color.r, "Returning a buff to default changed colour before reaching default");
        Require((value.fontStyle & FontStyles.Bold) != 0, "Return-to-default count was not bold");
        yield return new WaitForSecondsRealtime(1.2f);
        Require(value.text == "8" && value.color == normal && value.rectTransform.localScale == Vector3.one, "Baseline recovery did not restore defaults");
        Require(value.fontStyle == normalStyle && value.fontWeight == normalWeight, "Default result retained bold styling");
        bind(2, 3, 12, 1, 8);
        Require(weight.text == "3" && weight.color.g > weight.color.r, "Weight reduction was not green or missed baseline");
        Require(value.text == "12" && value.color.r > value.color.g, "Value penalty was not red");
        yield return new WaitForSecondsRealtime(1.2f);
        Require(weight.text == "1" && value.text == "8", "Decreasing stats failed to finish");
        bind(3, 1, 8, 3, 8);
        Require(weight.color.r > weight.color.g && weight.rectTransform.localScale.x > 1.2f, "Weight penalty was not red/enlarged");
        bind(4, 2, 8, 2, 12); yield return new WaitForSecondsRealtime(.06f);
        string before = value.text; bind(4, 2, 8, 2, 10);
        Require(value.text == before, "Retarget snapped to a new baseline or target");
        bind(4, 2, 12, 2, 12);
        Require(value.text == before && value.color.r > value.color.g, "Displayed Value did not use the updated baseline for colour");
        bind(5, 2, 8, 2, 12);
        Require(value.text == "8", "Same-species new occurrence did not restart at its own baseline");
        card.SetCard(cod, 2, 0, false); card.SelectEffectChoice(EncounterEffectChoice.Bait, false);
        card.PresentStatFeedback(new EncounterStatFeedback(5, 2, 0, true));
        Require(value.text == "0" && value.color == normal && value.rectTransform.localScale == Vector3.one, "Bait's zero sale value was treated as a penalty");
        Require(value.fontStyle == normalStyle && value.fontWeight == normalWeight, "Bait zero retained modified bold styling");
        yield return new WaitForSecondsRealtime(.4f);
        Require(value.text == "0" && Quaternion.Angle(value.rectTransform.localRotation, Quaternion.identity) < .01f, "Bait retained a Catch count or punch");
        card.SetCard(cod, 2, 12, false); card.SelectEffectChoice(EncounterEffectChoice.Catch, false);
        card.PresentStatFeedback(new EncounterStatFeedback(5, 2, 8));
        Require(value.text == "8" && value.color.g > value.color.r, "Returning to Catch counted from Bait zero instead of authored Value");
        card.SetCard(cod, 3, 20, true, true); card.PresentStatFeedback(new EncounterStatFeedback(7, 2, 8));
        yield return new WaitForSecondsRealtime(.4f);
        Require(weight.text == "?" && value.text == "?" && value.color == normal, "Animation exposed hidden information");
        Require(value.fontStyle == normalStyle && value.fontWeight == normalWeight, "Hidden Value retained modified bold styling");
        card.SetCard(cod, 2, 18, false); yield return new WaitForSecondsRealtime(.4f);
        Require(value.text == "18" && value.rectTransform.localScale == Vector3.one, "Unanimated snapshot was overwritten by an old counter");
        Require(value.fontStyle == normalStyle && value.fontWeight == normalWeight, "Static snapshot inherited animated bold styling");
        bind(8, 2, 8, 2, 12); card.SetStatFeedbackPaused(true);
        yield return new WaitForSecondsRealtime(.5f); Require(value.text == "8", "Incoming transition consumed unseen count steps");
        card.SetStatFeedbackPaused(false); card.CompleteStatFeedback();
        Require(value.text == "12" && value.rectTransform.localScale == Vector3.one, "Outgoing snapshot did not freeze authoritative values");
        bind(9, 2, 8, 2, 10008);
        float largeStart = Time.realtimeSinceStartup; int changes = 0, last = 8;
        while (Time.realtimeSinceStartup - largeStart < 1.5f && value.text != "10008")
        {
            int shown = int.Parse(value.text);
            if (shown != last) { changes++; last = shown; }
            yield return null;
        }
        Require(value.text == "10008" && changes > 1, "Large bonus was instant or took too long to count");
        Require(changes <= 25, "Large bonus retained an unbounded integer-by-integer sequence");
        IEnumerator colourChecks = CheckDisplayedColours(card, cod, weight, value);
        while (colourChecks.MoveNext()) yield return colourChecks.Current;
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
        Require(resolved.CurrentWeight == 1, "Combined chain and pending Technique Weight preview changed");
        Require(resolved.CurrentValue == urchin.Value + 8 && chain.Catches.Length == 1, "Combined preview lost Technique Value or changed the live chain");
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
        chain.Reset(); runtime.SetCurrentEncounter(cod); refresh.Invoke(controller, null);
        var deck = (TechniqueDeckRuntime)typeof(FishingRunController).GetField("techniqueDeckRuntime", flags).GetValue(controller);
        var techniques = (TechniqueEffectRuntime)typeof(FishingRunController).GetField("techniqueEffectRuntime", flags).GetValue(controller);
        typeof(TechniqueDeckRuntime).GetField("hand", flags).SetValue(deck,
            new[] { AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Prize Hook.asset") });
        Require(controller.TryUseTechniqueCard(0), "Prize Hook could not be played in the real reaction window");
        Require(currentWeight.text == cod.Weight.ToString() && currentWeight.color.r > currentWeight.color.g,
            "Technique Weight penalty did not start red at authored Weight");
        Require(currentValue.text == cod.Value.ToString() && currentValue.color.g > currentValue.color.r,
            "Technique Value bonus did not start green at authored Value");
        yield return new WaitForSecondsRealtime(.5f);
        Require(currentWeight.text == (cod.Weight + 1).ToString() && int.Parse(currentValue.text) > cod.Value,
            "Technique stat modifiers did not count through visible integers");
        Require(controller.TryChooseEncounterRole(EncounterEffectChoice.Bait), "Could not choose Bait during Technique preview");
        Require(currentValue.text == "0" && currentValue.color == normal && currentValue.rectTransform.localScale == Vector3.one,
            "Real Bait selection counted down or retained Technique emphasis");
        Require(controller.TryChooseEncounterRole(EncounterEffectChoice.Catch), "Could not restore Catch");
        Require(currentValue.text == cod.Value.ToString(), "Real Catch return counted from zero");
        yield return new WaitForSecondsRealtime(1.9f);
        Require(currentValue.text == (cod.Value + 8).ToString() && techniques.Approved.PendingPrizeCount == 1,
            "Prize preview missed its final Value or consumed the grant");
        ScreenCapture.CaptureScreenshot(captureDirectory + "TechniqueStats4K.png"); yield return null;
        techniques.Approved.Reset();
        var persistence = (EventPersistenceRuntime)typeof(FishingRunController).GetField("eventPersistenceRuntime", flags).GetValue(controller);
        var upwelling = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Gentle Upwelling.asset");
        persistence.QueuePending(999, upwelling, upwelling.Effects[0], EventEffectLifetime.UntilRunEnds());
        Require(controller.TryChooseEncounterRole(EncounterEffectChoice.Bait), "Could not choose Bait for Upwelling");
        Require(currentWeight.text == (cod.Weight + 1).ToString() && currentWeight.color.r > currentWeight.color.g
            && currentValue.text == "0" && currentValue.color == normal,
            "Event retarget changed the still-overweight number to green early or coloured Bait zero");
        yield return new WaitForSecondsRealtime(.8f);
        Require(currentWeight.text == Mathf.Max(0, cod.Weight - 1).ToString() && currentWeight.color.g > currentWeight.color.r
            && persistence.Effects[0].IsPending,
            "Upwelling preview missed beneficial Weight/colour or consumed its pending effect");
        controller.StartRun(); yield return null;
        Require(controller.IsOpeningBoatVisible, "Verification failed to reset run");
    }

    /// <summary>Checks both stats frame by frame through same-side changes, baseline returns and crossings in both directions.</summary>
    private static IEnumerator CheckDisplayedColours(CreatureCardView card, CardDefinition definition, TMP_Text weight, TMP_Text value)
    {
        var animator = card.GetComponent<CreatureCardStatAnimator>(); animator.ResetPresentation();
        Color weightNormal = weight.color, valueNormal = value.color;
        int[] destinations = { 12, 10, 14, 8, 4, 6, 2, 12, 4, 8 };
        for (int stat = 0; stat < 2; stat++)
        {
            bool lowerIsBetter = stat == 0;
            TMP_Text text = lowerIsBetter ? weight : value;
            Color normal = lowerIsBetter ? weightNormal : valueNormal;
            foreach (int target in destinations)
            {
                Action bind = () => {
                    card.SetCard(definition, lowerIsBetter ? target : 8, lowerIsBetter ? 8 : target, false);
                    card.PresentStatFeedback(new EncounterStatFeedback(100 + stat, 8, 8));
                };
                bind(); bool correct = true; int samples = 0;
                float deadline = Time.realtimeSinceStartup + 2;
                do
                {
                    int shown = int.Parse(text.text);
                    int direction = shown.CompareTo(8);
                    if (direction == 0 && target != 8) direction = target.CompareTo(8);
                    int benefit = lowerIsBetter ? -direction : direction;
                    correct &= benefit == 0 ? text.color == normal
                        : benefit > 0 ? text.color.g > text.color.r : text.color.r > text.color.g;
                    samples++;
                    if (!animator.IsAnimating) break;
                    yield return null; bind();
                } while (Time.realtimeSinceStartup < deadline);
                Require(correct && text.text == target.ToString() && samples > 1,
                    (lowerIsBetter ? "Weight" : "Value") + " colour changed early, flashed neutral, or missed default at target " + target);
            }
        }
    }

    /// <summary>Stops on the first visual or integration regression.</summary>
    private static void Require(bool value, string message) { assertions++; if (!value) throw new InvalidOperationException(message); }
}
