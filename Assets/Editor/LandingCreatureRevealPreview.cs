using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Review tool for the Landing creature reveal: plays any rarity live in Play mode (with sound), and renders one native
/// 60 fps frame sequence per rarity (encoded by Tools/Rendering/encode_creature_reveal_preview.py) plus a side-by-side page. Presentation only; it never changes scores, attempts or definitions.
/// </summary>
public static class LandingCreatureRevealPreview
{
    public const string Output = "Docs/Validation/CreatureReveal/Preview/";
    private const string Frames = "Temp/CreatureRevealPreview/";
    private const float Lead = .25f, Tail = .75f; private const int Fps = 60;
    private const BindingFlags Hidden = BindingFlags.Static | BindingFlags.NonPublic;
    private static readonly (CardRarity Rarity, string Creature)[] Representatives =
        { (CardRarity.Common, "Sardine"), (CardRarity.Uncommon, "Small Octopus"), (CardRarity.Rare, "Squid"), (CardRarity.Legendary, "Stormjaw Tuna") };
    public static string Status { get; private set; } = "Idle";

    [MenuItem("Fishing Cards/Preview/Creature Reveal/Common")] private static void Common() => Play(CardRarity.Common);
    [MenuItem("Fishing Cards/Preview/Creature Reveal/Uncommon")] private static void Uncommon() => Play(CardRarity.Uncommon);
    [MenuItem("Fishing Cards/Preview/Creature Reveal/Rare")] private static void Rare() => Play(CardRarity.Rare);
    [MenuItem("Fishing Cards/Preview/Creature Reveal/Legendary")] private static void Legendary() => Play(CardRarity.Legendary);
    [MenuItem("Fishing Cards/Preview/Creature Reveal/Render Videos")] public static void RenderVideos() => Start(RenderAll());

    /// <summary>Plays one rarity on the live Landing machine, opening Landing from the Harbor first when needed.</summary>
    public static void Play(CardRarity rarity) => Start(PlayLive(rarity));

    private static void Start(IEnumerator routine)
    {
        if (!Application.isPlaying) { EditorUtility.DisplayDialog("Creature reveal preview", "Enter Play mode in the MidnightFieldJournalComparison scene first.", "OK"); return; }
        var harbor = UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>();
        if (harbor == null) { Debug.LogWarning("Creature reveal preview needs the comparison scene (JournalHarborFlow)."); return; }
        Application.runInBackground = true; Status = "Running"; harbor.Controller.StartCoroutine(Guard(routine));
    }

    private static IEnumerator PlayLive(CardRarity rarity)
    {
        var owner = UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>().Controller;
        bool fresh = false;
        if (!LandingReady(owner)) { yield return OpenLanding(owner, Representative(rarity)); fresh = true; }
        if (fresh) { Status = "Playing " + rarity + " (real encounter)"; yield break; } // The real reveal already started with this creature.
        var stage = Stage.Find(); var identity = stage.Dress(Representative(rarity));
        if (!owner.PreviewLandingReveal(rarity)) { identity.Restore(); throw new InvalidOperationException("Landing is not idle; finish the current strike feedback first."); }
        stage.Reveal.Begin(rarity, owner.LandingRevealStartedAt);
        while (owner.LandingRevealPending || stage.Reveal.IsPlaying) yield return null;
        identity.Restore(); Status = "Played " + rarity;
    }

    private static IEnumerator RenderAll()
    {
        var owner = UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>().Controller;
        if (!LandingReady(owner)) { yield return OpenLanding(owner, Representative(CardRarity.Common)); }
        owner.CompleteLandingReveal(); yield return null;
        Directory.CreateDirectory(Output);
        var entries = new StringBuilder();
        foreach (var (rarity, _) in Representatives)
        {
            Status = "Rendering " + rarity; yield return null;
            entries.Append(entries.Length > 0 ? ",\n" : "").Append(RenderRarity(rarity));
            yield return null;
        }
        File.WriteAllText(Frames + "manifest.json", "{\"fps\":" + Fps + ",\"output\":\"" + Output + "\",\"clips\":[\n" + entries + "\n]}");
        File.WriteAllText(Output + "index.html", Page());
        Status = "Frames ready. Encode with: python -I Tools/Rendering/encode_creature_reveal_preview.py"; Debug.Log(Status);
    }

    /// <summary>Captures every frame synchronously so the controller never observes the cosmetic state; returns the clip's encode manifest.</summary>
    private static string RenderRarity(CardRarity rarity)
    {
        var stage = Stage.Find(); var creature = Representative(rarity); var timing = LandingRevealTiming.For(rarity);
        string folder = Frames + rarity + "/"; if (Directory.Exists(folder)) Directory.Delete(folder, true); Directory.CreateDirectory(folder);
        var identity = stage.Dress(creature); string message = stage.Message.text, caption = stage.StrikeCaption != null ? stage.StrikeCaption.text : null;
        const string instruction = "Strike when the fish marker and zone align.  SPACE / STRIKE";
        // Long enough to show a Rare/Legendary banner folding away, otherwise the standard tail.
        float tail = LandingRarityBanner.Announces(rarity) ? Mathf.Max(Tail, timing.SettleAt + LandingRarityBanner.Lifetime(rarity) - timing.Duration + .25f) : Tail;
        int count = Mathf.CeilToInt((Lead + timing.Duration + tail) * Fps);
        try
        {
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)Fps - Lead;
                if (t < timing.Duration)
                {
                    stage.Reveal.Begin(rarity, Time.realtimeSinceStartupAsDouble - Mathf.Max(0, t), true); stage.Reveal.Tick();
                    stage.Message.text = LandingRevealTiming.Caption; if (stage.StrikeCaption != null) stage.StrikeCaption.text = "SKIP";
                }
                else
                {
                    stage.Reveal.Finish(); stage.Message.text = instruction; if (stage.StrikeCaption != null) stage.StrikeCaption.text = "STRIKE";
                    var banner = stage.Reveal.Banner; banner.Stamp(rarity, Time.realtimeSinceStartupAsDouble - (t - timing.SettleAt));
                }
                SlotMachineSceneChecks.Capture(stage.Canvas, i.ToString("0000") + ".png", 1920, 1080, folder);
            }
        }
        finally
        {
            stage.Reveal.Finish(); stage.Reveal.Banner.Clear(); identity.Restore(); stage.Message.text = message; if (stage.StrikeCaption != null) stage.StrikeCaption.text = caption;
        }
        // Same take, pitch and gain the runtime player would use; mixer-bus attenuation is not reproduced.
        var definition = stage.Sound.Library.Find(DeepwaterSoundCue.Reveal); var clip = definition.Takes[0];
        float pitch = stage.Sound.CuePitch(DeepwaterSoundCue.Reveal, timing.Pitch), gain = definition.GainDb + timing.GainDb;
        // Encoding runs in Tools/Rendering/encode_creature_reveal_preview.py, so videos can be re-encoded without the Editor.
        return string.Format(CultureInfo.InvariantCulture,
            "{{\"rarity\":\"{0}\",\"frames\":\"{1}\",\"count\":{2},\"audio\":\"{3}\",\"sampleRate\":{4},\"pitch\":{5:0.#####},\"gainDb\":{6:0.##},\"delayMs\":{7}}}",
            rarity, folder, count, AssetDatabase.GetAssetPath(clip), clip.frequency, pitch, gain, Mathf.RoundToInt((Lead + timing.SettleAt) * 1000));
    }

    private static string Page()
    {
        var html = new StringBuilder("<!doctype html><meta charset=utf-8><title>Deepwater creature reveal preview</title>"
            + "<style>body{background:#14181c;color:#e9dcb1;font:15px system-ui;margin:24px}h1{font-size:20px}.grid{display:grid;grid-template-columns:1fr 1fr;gap:18px}"
            + "figure{margin:0}video{width:100%;border:1px solid #3a4148}figcaption{margin:6px 0 0}small{color:#9aa}</style>"
            + "<h1>Landing creature reveal: one preview per rarity</h1><p><small>Native Unity frames at 60 fps with the runtime Reveal cue (same take, pitch and gain; mixer-bus level not reproduced). "
            + "Each clip shows 0.25 s of the hidden panel, the reveal, then the panel at rest (Rare/Legendary until their banner folds away). <a href=\"Reveal-All.mp4\">All four in sequence</a>.</small></p><div class=grid>");
        foreach (var (rarity, creature) in Representatives)
        {
            var t = LandingRevealTiming.For(rarity);
            html.Append("<figure><video src=\"Reveal-").Append(rarity).Append(".mp4\" controls loop preload=auto></video><figcaption><b>")
                .Append(rarity).Append("</b> · ").Append(creature).Append(" · ").Append(t.Duration.ToString("0.00", CultureInfo.InvariantCulture)).Append(" s</figcaption></figure>");
        }
        return html.Append("</div>").ToString();
    }

    private static bool LandingReady(FishingRunController owner) => owner.RunActive && owner.UsesLandingChallenge && !owner.LandingFeedbackPending
        && !owner.AwaitingRigPlacement && owner.Landing.State == LandingChallengeState.Active;

    /// <summary>Starts a preview expedition from the Harbor (saves protected) or accepts the current Discover result, forcing the given creature.</summary>
    private static IEnumerator OpenLanding(FishingRunController owner, CardDefinition creature)
    {
        var harbor = UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>();
        if (!owner.RunActive)
        {
            if (!harbor.HubVisible) throw new InvalidOperationException("Return to the Harbor or open a Landing first.");
            owner.SetDebugSaveResetPending(true);
            owner.TryPrepareSlotTags(new[] { "Armored", "Predator", "Schooling", "Elusive" }); harbor.TryStartNewRun();
            ((UnityEngine.UI.Button)new SerializedObject(harbor.SlotPreparation).FindProperty("confirm").objectReferenceValue).onClick.Invoke();
            yield return null;
        }
        var ready = typeof(FishingRunController).GetProperty("CoreActionInputReady", BindingFlags.Instance | BindingFlags.NonPublic);
        double deadline = Time.realtimeSinceStartupAsDouble + 15;
        while (!(bool)ready.GetValue(owner) && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
        typeof(RigBuildingSceneChecks).GetMethod("ForceNextCreature", Hidden).Invoke(null, new object[] { owner, creature });
        SlotReelAnimationChecks.SettleForControlFixture(UnityEngine.Object.FindAnyObjectByType<SlotMachineView>(), owner);
        yield return null;
        if (!owner.TryAcceptSlotMachine(owner.SlotMachine.Revision)) throw new InvalidOperationException("Could not open Landing from the current phase; return to the Harbor or Discover.");
        yield return null;
    }

    private static CardDefinition Representative(CardRarity rarity)
    {
        string name = Representatives.First(r => r.Rarity == rarity).Creature;
        return AssetDatabase.FindAssets("t:CardDefinition", new[] { "Assets/Cards" })
            .Select(g => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).Single(c => c.DisplayName == name);
    }

    private static IEnumerator Guard(IEnumerator body)
    {
        while (true)
        {
            object current;
            try { if (!body.MoveNext()) yield break; current = body.Current; }
            catch (Exception error) { Status = "FAILED: " + (error.InnerException ?? error).Message; Debug.LogError("Creature reveal preview: " + error); yield break; }
            yield return current;
        }
    }

    /// <summary>Live Landing references, read through the authored serialized bindings.</summary>
    private sealed class Stage
    {
        public LandingCreatureReveal Reveal; public Canvas Canvas; public DeepwaterSoundPlayer Sound;
        public TMP_Text Message, Name, Tags, StrikeCaption; public UnityEngine.UI.Image Portrait;

        public static Stage Find()
        {
            var landing = UnityEngine.Object.FindAnyObjectByType<TackleLandingPresentation>();
            if (landing == null) throw new InvalidOperationException("Landing is not visible.");
            var data = new SerializedObject(landing); var view = UnityEngine.Object.FindAnyObjectByType<LandingChallengeView>();
            var strike = view != null ? (UnityEngine.UI.Button)new SerializedObject(view).FindProperty("strike").objectReferenceValue : null;
            return new Stage
            {
                Reveal = landing.GetComponent<LandingCreatureReveal>(), Canvas = landing.GetComponent<Canvas>(),
                Sound = (DeepwaterSoundPlayer)data.FindProperty("sound").objectReferenceValue,
                Message = (TMP_Text)data.FindProperty("message").objectReferenceValue, Name = (TMP_Text)data.FindProperty("creature").objectReferenceValue,
                Tags = (TMP_Text)data.FindProperty("tags").objectReferenceValue, Portrait = (UnityEngine.UI.Image)data.FindProperty("portrait").objectReferenceValue,
                StrikeCaption = strike != null ? strike.GetComponentInChildren<TMP_Text>() : null,
            };
        }

        /// <summary>Temporarily shows a representative creature's portrait, name and tag; the encounter itself is untouched.</summary>
        public Identity Dress(CardDefinition creature)
        {
            var saved = new Identity(this);
            Portrait.sprite = creature.EncounterArtwork ?? creature.Artwork; Portrait.enabled = Portrait.sprite != null;
            Name.text = creature.DisplayName.ToUpperInvariant(); Tags.text = string.Join(" / ", creature.Tags).ToUpperInvariant();
            return saved;
        }
    }

    private sealed class Identity
    {
        private readonly Stage stage; private readonly Sprite sprite; private readonly bool enabled; private readonly string name, tags;
        public Identity(Stage owner) { stage = owner; sprite = owner.Portrait.sprite; enabled = owner.Portrait.enabled; name = owner.Name.text; tags = owner.Tags.text; }
        public void Restore() { stage.Portrait.sprite = sprite; stage.Portrait.enabled = enabled; stage.Name.text = name; stage.Tags.text = tags; }
    }
}
