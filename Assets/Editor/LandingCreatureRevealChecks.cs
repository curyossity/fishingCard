using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>Validates the rarity-scaled creature entrance: pure pacing rules, prefab bindings, and the live gate in a fresh Play session.</summary>
public static class LandingCreatureRevealChecks
{
    public const string Output = "Docs/Validation/CreatureReveal/";
    private const BindingFlags Fixture = BindingFlags.Static | BindingFlags.NonPublic;
    public static string Result { get; private set; } = "Not run";
    private static int checks;

    /// <summary>Runs the edit-mode rules and prefab checks from the Validation menu.</summary>
    [MenuItem("Fishing Cards/Validation/Landing Creature Reveal (Rules and Assets)")]
    public static void RunEditMode()
    {
        int rules = ValidateRules(), assets = ValidateAssets();
        Directory.CreateDirectory(Output);
        File.WriteAllText(Output + "RulesAndAssets.json", "{\"passed\":true,\"rules\":" + rules + ",\"assets\":" + assets + "}");
        Debug.Log("Creature reveal: " + rules + " rule and " + assets + " asset assertions passed.");
    }

    /// <summary>Checks pacing, monotonic emphasis by rarity, exact rest values and the reduced-motion fade, independently of rendering.</summary>
    public static int ValidateRules()
    {
        checks = 0;
        var tiers = ((CardRarity[])Enum.GetValues(typeof(CardRarity))).Select(LandingRevealTiming.For).ToArray();
        for (int i = 0; i < tiers.Length; i++)
        {
            var t = tiers[i];
            Check(t.Duration > 0 && t.Duration <= LandingRevealTiming.MaximumDuration, t.Rarity + " reveal is unbounded or empty.");
            Check(t.FaceOpacity(0, false) == 0 && t.FaceScale(0) < .05f, t.Rarity + " face is visible before its entrance.");
            Check(Mathf.Abs(t.FaceScale(t.SettleAt) - 1) < .001f && t.FaceOpacity(t.SettleAt, false) == 1, t.Rarity + " face does not land face-up.");
            Check(t.SettleScale(t.Duration) == 1 && t.SettleScale(t.Duration - .0001f) - 1 < .001f, t.Rarity + " pop does not return exactly to rest.");
            Check(t.NameOpacity(t.SettleAt - .01f) == 0 && t.NameOpacity(t.Duration) == 1, t.Rarity + " name appears before the face lands.");
            Check(t.StageOffset(t.SettleAt + .33f) == Vector2.zero && t.FlashOpacity(t.SettleAt + .06f) == 0, t.Rarity + " shake or flash outlives the settle.");
            Check(t.FaceOpacity(t.SettleAt, true) == 1 && t.FaceOpacity(t.Anticipation, true) == 0, t.Rarity + " reduced-motion fade has the wrong endpoints.");
            float previous = -1; bool monotonic = true;
            for (float e = 0; e <= t.Duration; e += .01f) { float a = t.FaceOpacity(e, true); monotonic &= a >= previous - .0001f; previous = a; }
            Check(monotonic, t.Rarity + " reduced-motion fade flickers.");
            Check(LandingRarityBanner.Announces(t.Rarity) == (t.Rarity >= CardRarity.Rare) && (LandingRarityBanner.Word(t.Rarity) != "") == LandingRarityBanner.Announces(t.Rarity),
                t.Rarity + " banner announcement policy is wrong.");
            if (i > 0)
            {
                var lower = tiers[i - 1];
                Check(t.Duration > lower.Duration && t.Turns >= lower.Turns && t.Shake >= lower.Shake && t.Flash >= lower.Flash,
                    t.Rarity + " is not more emphatic than " + lower.Rarity + ".");
            }
        }
        // Routine creatures stay light: no anticipation, flash or stage motion below Rare.
        foreach (var t in tiers.Where(t => t.Rarity < CardRarity.Rare))
            Check(t.Anticipation == 0 && t.Flash == 0 && t.Shake == 0 && t.StageOffset(t.SettleAt + .02f) == Vector2.zero, t.Rarity + " is over-juiced.");
        // Banner: stamps past rest and settles exactly, holds, then folds and disappears; Legendary alone glints.
        foreach (var rarity in new[] { CardRarity.Rare, CardRarity.Legendary })
        {
            float hold = LandingRarityBanner.Hold(rarity), life = LandingRarityBanner.Lifetime(rarity);
            Check(LandingRarityBanner.StampScale(0) > 1.2f && LandingRarityBanner.StampScale(LandingRarityBanner.StampSeconds) == 1, rarity + " stamp does not slam and settle exactly.");
            Check(Mathf.Abs(LandingRarityBanner.StampRotation(LandingRarityBanner.StampSeconds) - LandingRarityBanner.Tilt) < .001f, rarity + " banner does not settle at its tilt.");
            Check(LandingRarityBanner.Fold(hold, rarity) == 1 && LandingRarityBanner.Fold(life, rarity) < .02f, rarity + " banner does not hold then fold.");
            Check(LandingRarityBanner.Opacity(hold * .5f, rarity, false) == 1 && LandingRarityBanner.Opacity(life, rarity, false) == 0
                && LandingRarityBanner.Opacity(-.01f, rarity, true) == 0 && LandingRarityBanner.Opacity(life, rarity, true) == 0, rarity + " banner visibility window is wrong.");
            Check(life < 2, rarity + " banner lingers too long over play.");
        }
        Check(float.IsNaN(LandingRarityBanner.Glint(.4f, CardRarity.Rare)) && !float.IsNaN(LandingRarityBanner.Glint(.4f, CardRarity.Legendary))
            && float.IsNaN(LandingRarityBanner.Glint(1, CardRarity.Legendary)), "Glint is not Legendary-only and bounded.");
        // Flashes stay below the photosensitivity threshold: one per reveal.
        foreach (var t in tiers)
        {
            int pulses = 0; bool on = false;
            for (float e = 0; e <= t.Duration; e += .002f) { bool lit = t.FlashOpacity(e) > .05f; if (lit && !on) pulses++; on = lit; }
            Check(pulses <= 1, t.Rarity + " flashes more than once.");
        }
        return checks;
    }

    /// <summary>Confirms the installed prefab binds the reveal to the existing collision panel, flash, camera, preferences and presentation.</summary>
    public static int ValidateAssets()
    {
        checks = 0;
        var root = AssetDatabase.LoadAssetAtPath<GameObject>(LandingChallengeSetup.PrefabPath);
        Check(root != null, "Landing prefab missing.");
        var reveal = root.GetComponentInChildren<LandingCreatureReveal>(true);
        Check(reveal != null, "Creature reveal is not installed; run Fishing Cards/Landing Feedback/Install Creature Reveal.");
        var data = new SerializedObject(reveal); var collision = new SerializedObject(root.GetComponentInChildren<LandingCardCollision>(true));
        var banner = (LandingRarityBanner)data.FindProperty("banner").objectReferenceValue;
        Check(banner != null, "Rarity banner is not installed.");
        var bannerData = new SerializedObject(banner);
        foreach (string field in new[] { "group", "face", "shadow", "preferences" })
            Check(bannerData.FindProperty(field).objectReferenceValue != null, "Banner field '" + field + "' is unbound.");
        Check(!banner.raycastTarget && !((CanvasGroup)bannerData.FindProperty("group").objectReferenceValue).blocksRaycasts, "Banner intercepts input.");
        Check(banner.transform.parent == collision.FindProperty("stage").objectReferenceValue && banner.transform.GetSiblingIndex() > ((Component)collision.FindProperty("creatureCard").objectReferenceValue).transform.GetSiblingIndex(),
            "Banner is not drawn above the creature card.");
        foreach (string field in new[] { "creatureCard", "creatureFade", "creatureFlash", "creatureName", "impactCamera", "preferences", "sound" })
            Check(data.FindProperty(field).objectReferenceValue != null, "Reveal field '" + field + "' is unbound.");
        foreach (string field in new[] { "creatureCard", "creatureFade", "creatureFlash" })
            Check(data.FindProperty(field).objectReferenceValue == collision.FindProperty(field).objectReferenceValue, "Reveal does not share the collision's " + field + ".");
        var view = new SerializedObject(root.GetComponentInChildren<TackleLandingPresentation>(true));
        Check(view.FindProperty("reveal").objectReferenceValue == reveal, "Presentation does not reference the reveal.");
        Check(view.FindProperty("creature").objectReferenceValue == data.FindProperty("creatureName").objectReferenceValue, "Reveal fades a different name label.");
        return checks;
    }

    /// <summary>Starts the live check; requires a fresh Play session of the comparison scene.</summary>
    [MenuItem("Fishing Cards/Validation/Landing Creature Reveal (Live)")]
    public static void Begin()
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Enter a fresh comparison Play session.");
        Result = "Running"; checks = 0; var owner = UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>().Controller;
        Directory.CreateDirectory(Output); File.WriteAllText(Output + "LiveVerification.json", "{\"passed\":false,\"running\":true}");
        owner.SetDebugSaveResetPending(true); Application.runInBackground = true; owner.StartCoroutine(Guarded(Run()));
    }

    /// <summary>Records a failure instead of leaving the result file in its running state.</summary>
    private static IEnumerator Guarded(IEnumerator body)
    {
        while (true)
        {
            object current;
            try { if (!body.MoveNext()) yield break; current = body.Current; }
            catch (Exception error)
            {
                Result = "FAILED: " + error.Message;
                File.WriteAllText(Output + "LiveVerification.json", "{\"passed\":false,\"error\":" + Quote(error.Message) + "}");
                Debug.LogError("Creature reveal live check failed: " + error); yield break;
            }
            yield return current;
        }
    }

    /// <summary>Exercises the real gate: held motion, rejected strikes, skip without striking, restored visuals and captured frames.</summary>
    private static IEnumerator Run()
    {
        var harbor = UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>(); var owner = harbor.Controller;
        owner.TryPrepareSlotTags(new[] { "Armored", "Predator", "Schooling", "Anchored" }); harbor.TryStartNewRun();
        var prep = new SerializedObject(harbor.SlotPreparation);
        ((UnityEngine.UI.Button)prep.FindProperty("confirm").objectReferenceValue).onClick.Invoke(); yield return null;
        var squid = Card("Squid"); Check(squid.Rarity == CardRarity.Rare, "Fixture creature is no longer Rare.");
        typeof(RigBuildingSceneChecks).GetMethod("ForceNextCreature", Fixture).Invoke(null, new object[] { owner, squid });
        yield return AcceptWithoutSkipping(owner);

        var landing = UnityEngine.Object.FindAnyObjectByType<TackleLandingPresentation>();
        var reveal = landing.GetComponent<LandingCreatureReveal>(); var data = new SerializedObject(reveal);
        var card = (RectTransform)data.FindProperty("creatureCard").objectReferenceValue;
        var fade = (CanvasGroup)data.FindProperty("creatureFade").objectReferenceValue;
        var stage = (RectTransform)new SerializedObject(landing.GetComponent<LandingImpactCamera>()).FindProperty("stage").objectReferenceValue;
        var message = (TMPro.TMP_Text)new SerializedObject(landing).FindProperty("message").objectReferenceValue;
        var view = UnityEngine.Object.FindAnyObjectByType<LandingChallengeView>();
        var strike = (UnityEngine.UI.Button)new SerializedObject(view).FindProperty("strike").objectReferenceValue;
        var model = owner.Landing;

        Check(owner.LandingRevealPending && reveal.IsPlaying, "Accepting a creature did not start its entrance.");
        Check(reveal.Timing.Rarity == CardRarity.Rare, "Entrance ignored the creature's rarity.");
        Check(fade.alpha < .01f, "Creature face is visible before the entrance turns it over.");
        Check(message.text == LandingRevealTiming.Caption && !message.text.Contains("RARE"), "Message band does not show only the skip hint.");
        int attempts = model.AttemptsRemaining; long revision = model.Revision;
        Check(!owner.TryStrikeLanding(model.Occurrence, model.Revision) && model.AttemptsRemaining == attempts && model.Revision == revision, "A strike was accepted during the entrance.");
        double fish = model.FishPosition, zone = model.ZonePosition;
        for (int i = 0; i < 8; i++) yield return null;
        Check(model.FishPosition == fish && model.ZonePosition == zone, "Fish or zone moved during the entrance.");

        // Deterministic frames: backdate the presentation's start inside one frame so the controller gate stays open.
        reveal.Finish(); // Restores authored rest values while the controller gate stays open.
        Check(fade.alpha == 1, "Finishing the entrance did not restore the face.");
        Vector2 stageRest = stage.anchoredPosition; Vector3 cardRest = card.localScale;
        var legendary = LandingRevealTiming.For(CardRarity.Legendary);
        foreach (var frame in new[] { ("Anticipation", legendary.Anticipation * .9f), ("Turning", legendary.Anticipation + legendary.Turning * .55f),
                     ("Settle", legendary.SettleAt + .02f), ("Landed", legendary.Duration - .01f) })
        {
            reveal.Begin(CardRarity.Legendary, Time.realtimeSinceStartupAsDouble - frame.Item2); reveal.Tick();
            SlotMachineSceneChecks.Capture(landing.GetComponent<Canvas>(), "Legendary-" + frame.Item1 + ".png", 1920, 1080, Output);
        }
        // Captures take real seconds; re-arm a fresh Rare entrance so the skip below happens before its face lands.
        Check(owner.PreviewLandingReveal(CardRarity.Rare), "Could not re-arm the entrance after captures.");
        reveal.Begin(CardRarity.Rare, owner.LandingRevealStartedAt); reveal.Tick();
        Check(!reveal.Banner.IsShowing, "Banner appeared before the face landed.");

        strike.onClick.Invoke();
        Check(!owner.LandingRevealPending, "Strike button did not skip the entrance.");
        Check(model.AttemptsRemaining == attempts && model.Revision == revision, "Skipping the entrance also struck.");
        yield return null;
        Check(!reveal.IsPlaying && fade.alpha == 1 && card.localScale == cardRest && stage.anchoredPosition == stageRest, "Skip left the panel or stage displaced.");
        Check(!message.text.Contains("RARE") && message.text.StartsWith("Strike when"), "Rarity text leaked into the instruction.");
        var banner = reveal.Banner;
        Check(banner.IsShowing && banner.Rarity == CardRarity.Rare, "Skipping a Rare entrance hid its banner.");
        for (int i = 0; i < 4; i++) yield return null;
        Check(model.FishPosition != fish || model.ZonePosition != zone, "Motion did not resume after the entrance.");
        Check(owner.TryStrikeLanding(model.Occurrence, model.Revision) && model.Revision != revision, "Strike rejected after the entrance.");
        Check(!banner.IsShowing, "Banner stayed over the creature card during scoring.");

        // Reduced motion keeps a stationary fade: no scale, flash or stage motion.
        var rush = landing.GetComponent<LandingScoreRush>();
        var reduced = (UnityEngine.UI.Toggle)new SerializedObject(rush).FindProperty("reducedMotionToggle").objectReferenceValue;
        bool preference = reduced.isOn; reduced.SetIsOnWithoutNotify(true);
        try
        {
            owner.CompleteLandingFeedback(); yield return null;
            Vector2 rest = stage.anchoredPosition; Vector3 scaleRest = card.localScale;
            reveal.Begin(CardRarity.Legendary, Time.realtimeSinceStartupAsDouble - (legendary.Anticipation + legendary.Turning * .5f)); reveal.Tick();
            Check(card.localScale == scaleRest && stage.anchoredPosition == rest && fade.alpha > 0 && fade.alpha < 1, "Reduced motion moved the panel or skipped the fade.");
            reveal.Finish();
            Check(fade.alpha == 1 && card.localScale == scaleRest, "Reduced-motion reveal did not restore the face.");
        }
        finally { reduced.SetIsOnWithoutNotify(preference); }

        Result = "Passed " + checks + " live creature reveal checks";
        File.WriteAllText(Output + "LiveVerification.json", "{\"passed\":true,\"assertions\":" + checks + "}");
        Debug.Log(Result);
    }

    /// <summary>Accepts Discover like the shared fixture but leaves the entrance running for inspection.</summary>
    private static IEnumerator AcceptWithoutSkipping(FishingRunController owner)
    {
        var ready = typeof(FishingRunController).GetProperty("CoreActionInputReady", BindingFlags.Instance | BindingFlags.NonPublic);
        double deadline = Time.realtimeSinceStartupAsDouble + 15;
        while (!(bool)ready.GetValue(owner) && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
        SlotReelAnimationChecks.SettleForControlFixture(UnityEngine.Object.FindAnyObjectByType<SlotMachineView>(), owner);
        yield return null;
        Check(owner.TryAcceptSlotMachine(owner.SlotMachine.Revision), "Could not accept Discover.");
        yield return null;
    }

    /// <summary>Loads installed species without editing their definitions.</summary>
    private static CardDefinition Card(string name) => AssetDatabase.FindAssets("t:CardDefinition", new[] { "Assets/Cards" })
        .Select(g => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).Single(c => c.DisplayName == name);
    private static string Quote(string text) => "\"" + text.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    /// <summary>Retains an actionable status if an invariant fails.</summary>
    private static void Check(bool valid, string message) { if (!valid) { Result = "FAILED: " + message; throw new InvalidOperationException(message); } checks++; }
}
