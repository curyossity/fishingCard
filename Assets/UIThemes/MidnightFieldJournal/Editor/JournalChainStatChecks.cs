using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Verifies serial stat feedback on the real compact-card chain with overflow and pooled removal.</summary>
public static class JournalChainStatChecks
{
    private const string DefaultOutput = "Docs/UIThemes/MidnightFieldJournal/ChainStatFeedback/";
    private static string Output;
    private static JournalModalHost runner;
    private static GameObject fixture;
    private static bool oldBackground;
    private static int assertions;
    private static readonly List<int> order = new List<int>();
    public static string Status { get; private set; }
    [Serializable] private sealed class Report { public bool passed; public int assertions; public string error; public int[] order; public int pulsedRecipients; public float peakScale, peakFlash; }
    private static int pulsedRecipients;
    private static float peakScale, peakFlash;

    /// <summary>Starts finite checks without editing authored scenes or assets.</summary>
    public static void Start(string outputFolder = null)
    {
        if (!Application.isPlaying || runner != null) throw new InvalidOperationException("Start once in Play Mode.");
        oldBackground = Application.runInBackground; Application.runInBackground = true;
        assertions = 0; order.Clear(); Status = "Running";
        Output = string.IsNullOrWhiteSpace(outputFolder) ? DefaultOutput : outputFolder;
        pulsedRecipients = 0; peakScale = 1; peakFlash = 0;
        runner = new GameObject("Chain Feedback Checks").AddComponent<JournalModalHost>();
        runner.StartCoroutine(Guarded());
    }

    /// <summary>Restores background execution and reports the first regression.</summary>
    private static IEnumerator Guarded()
    {
        IEnumerator check = Check(); Exception error = null;
        while (true)
        {
            bool more;
            try { more = check.MoveNext(); } catch (Exception e) { error = e; break; }
            if (!more) break; yield return check.Current;
        }
        Directory.CreateDirectory(Output);
        File.WriteAllText(Output + "PlayModeVerification.json", JsonUtility.ToJson(new Report {
            passed = error == null, assertions = assertions, error = error?.ToString(), order = order.ToArray(),
            pulsedRecipients = pulsedRecipients, peakScale = peakScale, peakFlash = peakFlash }, true));
        if (fixture != null) UnityEngine.Object.Destroy(fixture);
        Application.runInBackground = oldBackground;
        UnityEngine.Object.Destroy(runner.gameObject); runner = null;
        Status = error == null ? "Passed" : "Failed: " + error;
        if (error != null) Debug.LogException(error);
    }

    /// <summary>Exercises all recipients, scrolling, refresh retention, role changes and manual interruption.</summary>
    private static IEnumerator Check()
    {
        fixture = new GameObject("Chain Feedback Fixture", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler));
        fixture.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = fixture.GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(3840, 2160);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UIThemes/MidnightFieldJournal/Prefabs/CatchChain.prefab");
        var obj = UnityEngine.Object.Instantiate(prefab, fixture.transform);
        var rect = (RectTransform)obj.transform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(100, -100); rect.sizeDelta = new Vector2(855, 1500);
        var presentation = obj.GetComponent<JournalCatchChainPresentation>();
        var scroll = obj.GetComponentInChildren<JournalCatchChainScrollRect>();
        var chain = new CatchChainRuntime(); var resolver = new EffectResolver();
        var cod = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Coastal Cod.asset");
        for (int i = 0; i < 7; i++) chain.Add(cod, resolver, i == 4 ? CatchAttachmentRole.Bait : CatchAttachmentRole.Catch);
        int selected = -1;
        Action refresh = () => presentation.Refresh(chain.Catches, chain.ActiveEffectRecords, 0, 10, -1, i => selected = i);
        refresh(); yield return null; Canvas.ForceUpdateCanvases();
        Require(presentation.AnimatingCard == null, "Unmodified cards unexpectedly queued");
        for (int i = 0; i < 7; i++) chain.Catches[i].AddPermanentModifiers(i % 2 == 0 ? 1 : -1, 2);
        chain.Recalculate(resolver); refresh();
        var seen = new HashSet<int>(); CompactCatchCardView previous = null;
        var pulsed = new HashSet<int>();
        bool countDuringPulse = false, connectorMoved = false, capturedPulse = false;
        var connectorPositions = new Vector3[presentation.Links.Count];
        for (int i = 0; i < connectorPositions.Length; i++) connectorPositions[i] = presentation.Links[i].rectTransform.anchoredPosition3D;
        float deadline = Time.realtimeSinceStartup + 22;
        float maximumOffset = scroll.content.anchoredPosition.y;
        string captures = Output + "Capture-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff") + "/";
        Directory.CreateDirectory(captures);
        while (Time.realtimeSinceStartup < deadline)
        {
            refresh();
            maximumOffset = Mathf.Max(maximumOffset, scroll.content.anchoredPosition.y);
            CompactCatchCardView active = presentation.AnimatingCard;
            if (active != null)
            {
                var pulse = active.GetComponentInChildren<JournalCatchCardPulse>();
                if (pulse != null && pulse.IsPlaying)
                {
                    pulsed.Add(active.BoundInstance.InstanceId);
                    peakScale = Mathf.Max(peakScale, pulse.MotionScale); peakFlash = Mathf.Max(peakFlash, pulse.color.a);
                    countDuringPulse |= Text(active, "weightText").text != cod.Weight.ToString()
                        || Text(active, "valueText").text != (active.BoundInstance.IsBait ? "0" : cod.Value.ToString());
                    for (int i = 0; i < connectorPositions.Length; i++)
                        connectorMoved |= presentation.Links[i].rectTransform.anchoredPosition3D != connectorPositions[i]
                            || presentation.Links[i].rectTransform.localScale != Vector3.one;
                    if (!capturedPulse && pulse.MotionScale > 1.04f)
                    {
                        ScreenCapture.CaptureScreenshot(captures + "CardPopAndFlash4K.png"); capturedPulse = true;
                    }
                }
            }
            if (active != null && active != previous)
            {
                if (previous != null) Require(!previous.HasPendingStatFeedback, "Next card began before the prior spring finished");
                Require(seen.Add(active.BoundInstance.InstanceId), "A completed card replayed after refresh");
                order.Add(active.BoundInstance.InstanceId); previous = active;
                foreach (var queued in presentation.Cards)
                    if (queued != active && !seen.Contains(queued.BoundInstance.InstanceId))
                        Require(Text(queued, "weightText").text == cod.Weight.ToString()
                            && Text(queued, "weightText").rectTransform.localScale == Vector3.one
                            && (Text(queued, "weightText").fontStyle & FontStyles.Bold) == 0,
                            "A later creature began counting, enlarging or becoming bold before its turn");
            }
            if (seen.Count == 7 && presentation.AnimatingCard == null) break;
            yield return null;
        }
        Require(order.Count == 7, "Not every affected Catch/Bait completed");
        pulsedRecipients = pulsed.Count;
        Require(pulsedRecipients == 7, "Not every recipient received a card-level prelude");
        Require(!countDuringPulse, "Numbers started before the card pop/flash completed");
        Require(!connectorMoved, "Card pop moved or scaled chain hardware");
        Require(peakScale > 1.04f && peakScale <= 1.046f && peakFlash > .16f && peakFlash <= .181f,
            "Card pop/flash was absent or exceeded its subtle amplitude");
        for (int i = 0; i < 7; i++) Require(order[i] == chain.Catches[6 - i].InstanceId, "Feedback did not run newest to oldest");
        Require(scroll.content.anchoredPosition.y < maximumOffset - 500, "Overflow chain did not scroll toward old recipients: "
            + maximumOffset + " -> " + scroll.content.anchoredPosition.y + "; extent " + scroll.content.rect.height + "/" + scroll.viewport.rect.height);
        ScreenCapture.CaptureScreenshot(captures + "SettledOldest4K.png"); yield return null;
        foreach (var card in presentation.Cards)
        {
            var weight = Text(card, "weightText"); var value = Text(card, "valueText");
            Require(weight.text == card.BoundInstance.CurrentWeight.ToString(), "Final Weight was not authoritative");
            Require(value.text == card.BoundInstance.HaulValueContribution.ToString(), "Final Value was not authoritative");
            Require((weight.fontStyle & FontStyles.Bold) != 0 && weight.fontWeight == FontWeight.Bold,
                "Modified compact Weight lost persistent bold styling");
            Require(((value.fontStyle & FontStyles.Bold) != 0) == !card.BoundInstance.IsBait,
                "Modified compact Value or neutral Bait zero had incorrect bold styling");
            if (card.BoundInstance.IsBait) Require(value.rectTransform.localScale == Vector3.one, "Bait zero was animated/enlarged");
            var pulse = card.GetComponentInChildren<JournalCatchCardPulse>();
            Require(pulse != null && !pulse.IsPlaying && pulse.MotionScale == 1 && pulse.color.a == 0 && !pulse.raycastTarget,
                "Card pulse did not restore resting visuals or blocked input");
        }
        var retained = presentation.Cards[3]; var retainedInstance = retained.BoundInstance;
        retained.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        Require(selected == 3, "Wrapping visuals broke the card's selection callback");
        chain.TryReleaseTogether(new[] { chain.Catches[0].InstanceId }, resolver); refresh(); yield return null;
        Require(ReferenceEquals(presentation.Cards[2], retained) && ReferenceEquals(retained.BoundInstance, retainedInstance),
            "Removal shifted an animation onto another creature");
        Require(presentation.AnimatingCard == null, "Removal replayed unchanged survivors");
        chain.Catches[5].AddPermanentModifiers(0, 3); chain.Recalculate(resolver); refresh(); yield return null;
        Require(presentation.AnimatingCard != null, "Later effect did not queue feedback");
        float pulseDeadline = Time.realtimeSinceStartup + 2;
        while (Time.realtimeSinceStartup < pulseDeadline && (presentation.AnimatingCard.GetComponentInChildren<JournalCatchCardPulse>() == null
            || !presentation.AnimatingCard.GetComponentInChildren<JournalCatchCardPulse>().IsPlaying)) yield return null;
        Require(presentation.AnimatingCard.GetComponentInChildren<JournalCatchCardPulse>().IsPlaying,
            "Manual interruption fixture missed the active pulse");
        scroll.OnScroll(new PointerEventData(EventSystem.current) { scrollDelta = new Vector2(0, 1) });
        yield return null; yield return null;
        Require(presentation.AnimatingCard == null, "Manual scrolling fought automatic feedback");
        refresh(); yield return null;
        Require(presentation.AnimatingCard == null, "Manual interruption replayed on refresh");
        foreach (var pulse in presentation.GetComponentsInChildren<JournalCatchCardPulse>())
            Require(!pulse.IsPlaying && pulse.MotionScale == 1 && pulse.color.a == 0, "Manual scrolling retained a card pop/flash");
        chain.Catches[5].AddPermanentModifiers(0, 2); chain.Recalculate(resolver); refresh(); yield return null;
        Require(presentation.AnimatingCard != null, "Feedback did not resume for a new change after manual inspection");
        pulseDeadline = Time.realtimeSinceStartup + 2;
        while (Time.realtimeSinceStartup < pulseDeadline && !presentation.AnimatingCard.GetComponentInChildren<JournalCatchCardPulse>().IsPlaying) yield return null;
        var removedPulse = presentation.AnimatingCard.GetComponentInChildren<JournalCatchCardPulse>();
        Require(removedPulse.IsPlaying, "Removal fixture missed the active pulse");
        var removedActive = presentation.AnimatingCard.BoundInstance;
        chain.TryReleaseTogether(new[] { removedActive.InstanceId }, resolver); refresh(); yield return null;
        Require(presentation.AnimatingCard == null || !ReferenceEquals(presentation.AnimatingCard.BoundInstance, removedActive),
            "Removed active recipient blocked the queue");
        Require(!removedPulse.IsPlaying && removedPulse.MotionScale == 1 && removedPulse.color.a == 0, "Removal left a stale card pulse");
        chain.Catches[0].AddPermanentModifiers(0, 3); chain.Recalculate(resolver);
        presentation.SetStatFeedbackEnabled(false); refresh(); yield return null;
        Require(presentation.AnimatingCard == null && Text(presentation.Cards[0], "valueText").text == chain.Catches[0].HaulValueContribution.ToString(),
            "Ended-run presentation replayed feedback or retained a partial count");
        chain.Reset(); refresh(); yield return null;
        Require(presentation.AnimatingCard == null && !scroll.HasFeedbackFocus, "Empty chain retained stale queue/focus");
    }

    /// <summary>Reads actual prefab numeric text without relying on child names.</summary>
    private static TMP_Text Text(CompactCatchCardView card, string field)
    { return (TMP_Text)typeof(CompactCatchCardView).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(card); }

    /// <summary>Stops at the first ordering, identity or presentation violation.</summary>
    private static void Require(bool condition, string message)
    { assertions++; if (!condition) throw new InvalidOperationException(message); }
}
