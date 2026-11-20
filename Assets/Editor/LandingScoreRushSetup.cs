using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>Authors reusable score-rush visuals and original synthesized cues without modifying supplied artwork.</summary>
public static class LandingScoreRushSetup
{
    public const string AudioPath = TackleDiscoverArtSetup.Root + "Audio/Landing/";
    public const string MixerPath = AudioPath + "LandingFeedback.mixer";
    private static readonly Color Cream = new Color32(233, 220, 177, 255);

    /// <summary>Updates the existing Landing prefab in place; scene instances retain their Harbor route and profile.</summary>
    [MenuItem("Fishing Cards/Landing Feedback/Install Score Rush")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Install outside Play Mode.");
        var root = PrefabUtility.LoadPrefabContents(LandingChallengeSetup.PrefabPath);
        try { Configure(root); PrefabUtility.SaveAsPrefabAsset(root, LandingChallengeSetup.PrefabPath); }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        AssetDatabase.SaveAssets();
    }

    /// <summary>Retains the approved sequence whenever Landing art or source-feedback authoring is regenerated.</summary>
    public static void Configure(GameObject root)
    {
        if (root.GetComponent<TackleLandingPresentation>() == null) return;
        if (root.GetComponent<LandingScoreRush>() != null)
        { TuneReadout(new SerializedObject(root.GetComponent<LandingScoreRush>()), root.transform.Find("1920x1080 Stage")); return; }
        EnsureAudio();
        var stage = root.transform.Find("1920x1080 Stage");
        if (stage == null) throw new InvalidOperationException("Missing Tackle stage.");
        var host = root.AddComponent<LandingScoreRush>(); var data = new SerializedObject(host);
        var presentation = new SerializedObject(root.GetComponent<TackleLandingPresentation>());
        Bind(presentation, "scoreRush", host); presentation.ApplyModifiedPropertiesWithoutUndo();
        Bind(data, "feedback", root.GetComponent<LandingStrikeFeedback>());
        Bind(data, "machine", stage.Find("landing-machine"));
        Bind(data, "totalLabel", ((TMP_Text)presentation.FindProperty("points").objectReferenceValue).rectTransform);

        var popup = Rect(stage, "Score Rush Readout", new Rect(1147, 354, 458, 130));
        var group = popup.gameObject.AddComponent<CanvasGroup>(); group.blocksRaycasts = false; group.interactable = false; Bind(data, "readout", group);
        var background = popup.gameObject.AddComponent<UnityEngine.UI.Image>(); background.raycastTarget = false; background.color = new Color32(15, 23, 20, 255);
        var glow = Rect(popup, "Brass glow", new Rect(2, 2, 454, 126)).gameObject.AddComponent<UnityEngine.UI.Image>(); glow.raycastTarget = false; Bind(data, "glow", glow);
        var number = Text(popup, "Growing strike points", new Rect(8, 28, 442, 78), 72);
        number.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TackleDiscoverArtSetup.Root + "Fonts/PressStart2P.asset");
        number.enableAutoSizing = true; number.fontSizeMin = 24; number.fontSizeMax = 72; Bind(data, "value", number);
        Bind(data, "caption", Text(popup, "Strike caption", new Rect(8, 4, 442, 24), 23));
        Bind(data, "operation", Text(popup, "Current operation", new Rect(8, 104, 442, 26), 27));
        // The source plate stays above the timing glass and below the existing source/assistance displays.
        var plate = Rect(popup, "Active source", new Rect(-30, 163, 518, 26));
        var portrait = Rect(plate, "Source portrait", new Rect(22, -3, 38, 30)).gameObject.AddComponent<UnityEngine.UI.Image>();
        portrait.preserveAspect = true; portrait.raycastTarget = false; Bind(data, "sourcePortrait", portrait);
        var label = Text(plate, "Source name", new Rect(64, 0, 430, 26), 25);
        label.enableAutoSizing = true; label.fontSizeMin = 18; label.fontSizeMax = 25; Bind(data, "sourceName", label);
        var particles = Rect(stage, "Score light trails", new Rect(0, 0, 1920, 1080)).gameObject.AddComponent<LandingRushGraphic>();
        particles.raycastTarget = false; Bind(data, "particles", particles);
        Bind(data, "soundToggle", Toggle(stage, "Sound", "SOUND", new Rect(1385, 156, 150, 40)));
        Bind(data, "reducedMotionToggle", Toggle(stage, "Reduced Motion", "REDUCED MOTION", new Rect(1540, 156, 275, 40)));
        var audioRoot = new GameObject("Landing Feedback Audio"); audioRoot.transform.SetParent(root.transform, false);
        var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath); var bus = mixer.FindMatchingGroups("Master").First();
        var voices = data.FindProperty("voices"); voices.arraySize = 6;
        for (int i = 0; i < voices.arraySize; i++)
        {
            var obj = new GameObject("Score voice " + i); obj.transform.SetParent(audioRoot.transform, false);
            var source = obj.AddComponent<AudioSource>(); source.playOnAwake = false; source.spatialBlend = 0;
            source.outputAudioMixerGroup = bus; source.priority = 100; source.volume = .3f;
            voices.GetArrayElementAtIndex(i).objectReferenceValue = source;
        }
        foreach (string field in new[] { "hit", "addition", "multiplier", "payoutSmall", "payoutBig", "miss" })
            Bind(data, field, AssetDatabase.LoadAssetAtPath<AudioClip>(AudioPath + field + ".wav"));
        data.ApplyModifiedPropertiesWithoutUndo(); TuneReadout(data, stage); popup.gameObject.SetActive(false);
    }

    /// <summary>Keeps the active source inside the glass readout and removes duplicate captions during its count-up.</summary>
    private static void TuneReadout(SerializedObject data, Transform stage)
    {
        var particles = (LandingRushGraphic)data.FindProperty("particles").objectReferenceValue;
        if (particles.GetComponent<CanvasRenderer>() == null) particles.gameObject.AddComponent<CanvasRenderer>();
        var source = (TMP_Text)data.FindProperty("sourceName").objectReferenceValue;
        var plate = (RectTransform)source.transform.parent;
        plate.anchoredPosition = new Vector2(0, -104); plate.sizeDelta = new Vector2(458, 26);
        source.rectTransform.anchoredPosition = new Vector2(208, 0); source.rectTransform.sizeDelta = new Vector2(240, 26);
        source.fontSizeMax = 24;
        var portrait = (UnityEngine.UI.Image)data.FindProperty("sourcePortrait").objectReferenceValue;
        portrait.rectTransform.anchoredPosition = new Vector2(169, 0); portrait.rectTransform.sizeDelta = new Vector2(33, 26);
        var operation = (TMP_Text)data.FindProperty("operation").objectReferenceValue;
        operation.rectTransform.sizeDelta = new Vector2(155, 26);
        var labels = data.FindProperty("coveredLabels"); labels.arraySize = 2;
        labels.GetArrayElementAtIndex(0).objectReferenceValue = stage.Find("PointsCaption").GetComponent<TMP_Text>();
        labels.GetArrayElementAtIndex(1).objectReferenceValue = stage.Find("ResistanceCaption").GetComponent<TMP_Text>();
        data.ApplyModifiedPropertiesWithoutUndo();
        LandingFeedbackPolishSetup.Configure((LandingScoreRush)data.targetObject, stage);
    }

    /// <summary>Creates deterministic original PCM cues once and routes them through a dedicated authored mixer.</summary>
    private static void EnsureAudio()
    {
        Directory.CreateDirectory(AudioPath); AssetDatabase.Refresh();
        if (AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath) == null)
        {
            // Unity's editor-only mixer factory is internal; runtime uses only the public AudioMixer contract.
            var type = typeof(Editor).Assembly.GetType("UnityEditor.Audio.AudioMixerController");
            var factory = type?.GetMethod("CreateMixerControllerAtPath", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (factory == null) throw new InvalidOperationException("Unity mixer authoring factory is unavailable.");
            factory.Invoke(null, new object[] { MixerPath });
        }
        foreach (string cue in new[] { "hit", "addition", "multiplier", "payoutSmall", "payoutBig", "miss" })
        {
            string path = AudioPath + cue + ".wav";
            if (!File.Exists(path)) WriteCue(path, cue);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (AudioImporter)AssetImporter.GetAtPath(path); var settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.DecompressOnLoad; settings.compressionFormat = AudioCompressionFormat.PCM;
            settings.preloadAudioData = true; importer.defaultSampleSettings = settings; importer.forceToMono = true; importer.SaveAndReimport();
        }
    }

    /// <summary>Synthesizes soft harmonics, a short low impact and a resolved chord with a click-free envelope.</summary>
    private static void WriteCue(string path, string cue)
    {
        const int rate = 44100;
        bool big = cue == "payoutBig", payout = big || cue == "payoutSmall", missed = cue == "miss";
        float duration = big ? .85f : payout ? .5f : missed ? .2f : .28f;
        int samples = (int)(duration * rate); var pcm = new short[samples];
        double frequency = missed ? 130.81 : cue == "hit" ? 261.63 : cue == "addition" ? 329.63 : 392;
        for (int i = 0; i < samples; i++)
        {
            double time = (double)i / rate, sample = 0;
            if (payout)
            {
                double[] notes = { 261.63, 329.63, 392, 523.25 };
                for (int n = 0; n < notes.Length; n++) sample += Tone(time, notes[n], duration, .16);
                if (big) for (int n = 0; n < 5; n++) sample += Tone(time - n * .065, 523.25 * Math.Pow(2, n / 4.0), .24, .12);
                sample += Tone(time, 65.41, .2, big ? .34 : .15);
            }
            else sample = Tone(time, frequency, duration, .46) + Tone(time, frequency * 2, duration * .7, .1) + Tone(time, 80, .08, .1);
            pcm[i] = (short)(Math.Max(-.85, Math.Min(.85, sample)) * short.MaxValue);
        }
        using (var writer = new BinaryWriter(File.Create(path)))
        {
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + samples * 2);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16); writer.Write((short)1); writer.Write((short)1);
            writer.Write(rate); writer.Write(rate * 2); writer.Write((short)2); writer.Write((short)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(samples * 2); foreach (short sample in pcm) writer.Write(sample);
        }
    }
    /// <summary>Returns an enveloped harmonic with no discontinuity at either edge.</summary>
    private static double Tone(double time, double frequency, double duration, double gain)
    {
        if (time < 0 || time >= duration) return 0;
        double envelope = Math.Min(1, time / .006) * Math.Exp(-time * 7 / duration) * Math.Min(1, (duration - time) / .02);
        return gain * envelope * (Math.Sin(time * frequency * Math.PI * 2) + .17 * Math.Sin(time * frequency * Math.PI * 6));
    }
    /// <summary>Authors a native keyboard/gamepad toggle with a large pointer target.</summary>
    private static UnityEngine.UI.Toggle Toggle(Transform parent, string name, string label, Rect bounds)
    {
        var rect = Rect(parent, name, bounds); var toggle = rect.gameObject.AddComponent<UnityEngine.UI.Toggle>();
        var background = Rect(rect, "Toggle socket", new Rect(0, 5, 30, 30)).gameObject.AddComponent<UnityEngine.UI.Image>(); background.color = new Color32(50, 62, 53, 255);
        var mark = Rect(background.transform, "Enabled", new Rect(6, 6, 18, 18)).gameObject.AddComponent<UnityEngine.UI.Image>(); mark.color = new Color32(233, 188, 99, 255); mark.raycastTarget = false;
        toggle.targetGraphic = background; toggle.graphic = mark; toggle.isOn = true;
        var text = Text(rect, "Caption", new Rect(39, 4, bounds.width - 39, 32), 23); text.text = label; text.alignment = TextAlignmentOptions.MidlineLeft;
        // The full label remains clickable, not just the square socket.
        var hit = rect.gameObject.AddComponent<UnityEngine.UI.Image>(); hit.color = Color.clear;
        return toggle;
    }
    /// <summary>Creates a top-left anchored native layer within the approved 1920 by 1080 stage.</summary>
    private static RectTransform Rect(Transform parent, string name, Rect bounds)
    {
        var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform; rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1); rect.anchoredPosition = new Vector2(bounds.x, -bounds.y); rect.sizeDelta = bounds.size; return rect;
    }
    /// <summary>Uses independent native text fields so no supplied image is relabeled or replaced.</summary>
    private static TMP_Text Text(Transform parent, string name, Rect bounds, float size)
    {
        var text = Rect(parent, name, bounds).gameObject.AddComponent<TextMeshProUGUI>();
        text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TackleDiscoverArtSetup.Root + "Fonts/VT323.asset");
        text.fontSize = size; text.color = Cream; text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.NoWrap; return text;
    }
    /// <summary>Stores explicit component bindings for the runtime and prefab validation.</summary>
    private static void Bind(SerializedObject data, string name, UnityEngine.Object value) => data.FindProperty(name).objectReferenceValue = value;
}
