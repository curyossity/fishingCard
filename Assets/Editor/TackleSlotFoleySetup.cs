using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Edits licensed recorded mechanisms into the Discover cue set without altering gameplay or legacy Build audio.</summary>
public static class TackleSlotFoleySetup
{
    public const string SourcePath = "Assets/Editor/AudioSources/DiscoverFoley/";
    public const string AudioPath = TackleSlotAudioSetup.AudioPath + "Foley/";
    private const int Rate = 44100;
    private static readonly string[] Cues = { "start", "roll", "tick", "stop", "latch", "unlatch", "pair", "triple" };

    /// <summary>Rebuilds the reproducible recorded edits and binds the existing Discover sound owner.</summary>
    [MenuItem("Fishing Cards/Slot Machine/Rebuild Recorded Sound")]
    public static void Rebuild()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Rebuild outside Play mode.");
        foreach (string cue in Cues) Write(cue);
        TackleSlotAudioSetup.Install();
    }

    /// <summary>Retains the selected recorded cues through Discover regeneration, preserving existing edited WAVs.</summary>
    public static void Configure(SerializedObject owner)
    {
        foreach (string cue in Cues)
        {
            string path = AudioPath + cue + ".wav";
            if (!File.Exists(path)) Write(cue);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            var settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.PCM;
            settings.preloadAudioData = true;
            importer.defaultSampleSettings = settings;
            importer.forceToMono = true;
            importer.SaveAndReimport();
            owner.FindProperty(cue).objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
        owner.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>Builds short layered wood/metal contacts, a three-stroke ratchet loop and restrained recorded bell accents.</summary>
    private static void Write(string cue)
    {
        var wood = Read("impactWood_medium_001.ogg");
        var lightWood = Read("impactWood_light_000.ogg");
        var metal = Read("impactMetal_medium_001.ogg");
        var lightMetal = Read("impactMetal_light_002.ogg");
        var bell = Read("impactBell_heavy_000.ogg");
        var ratchet = Read("rotteveel-fast-ratchet-591529.mp3");
        var contact = Contact(Read("romulofs-ratchet-127533.mp3"), 8.05, .075);
        double duration = cue == "roll" ? .60 : cue == "triple" ? .52 : cue == "pair" ? .34 : cue == "tick" ? .055 : .24;
        var result = new double[(int)(duration * Rate)];
        switch (cue)
        {
            case "roll":
                Mix(result, Slice(ratchet, .05, .23), 0, 1.12, .8);
                Mix(result, Slice(ratchet, .69, .23), .20, 1.12, .73);
                Mix(result, Slice(ratchet, 1.29, .23), .40, 1.12, .77);
                break;
            case "tick": Mix(result, contact, 0, 1, 1); break;
            case "start":
                Mix(result, wood, 0, .82, 1); Mix(result, metal, .022, .90, .35);
                Mix(result, Slice(ratchet, .05, .12), .04, 1.1, .27); break;
            case "stop":
                Mix(result, wood, 0, .88, 1); Mix(result, metal, .006, .88, .52); break;
            case "latch":
                Mix(result, lightWood, 0, .92, .6); Mix(result, contact, .012, .88, 1); break;
            case "unlatch":
                Mix(result, lightWood, 0, 1.09, .8); Mix(result, lightMetal, .015, 1.08, .32); break;
            case "pair":
                Mix(result, bell, 0, 1.25, .75); Mix(result, lightWood, 0, .96, .15); break;
            case "triple":
                Mix(result, bell, 0, 1.25, .75); Mix(result, bell, .12, 1.58, .5);
                Mix(result, lightWood, 0, .96, .15); break;
        }
        // Remove DC/low rumble and soften brittle contact highs before normalizing; no oscillators or noise synthesis.
        double low = 0, high = 0, previous = 0;
        double alpha = 1 - Math.Exp(-2 * Math.PI * (cue == "pair" || cue == "triple" ? 6000 : 4200) / Rate);
        for (int i = 0; i < result.Length; i++)
        {
            high = .985 * (high + result[i] - previous); previous = result[i]; low += alpha * (high - low);
            double envelope = Math.Min(1, (double)i / (Rate * .002)) * Math.Min(1, (double)(result.Length - 1 - i) / (Rate * .012));
            result[i] = low * envelope;
        }
        double peak = result.Max(v => Math.Abs(v));
        if (peak < .00001) throw new InvalidOperationException("Empty recorded cue: " + cue);
        Directory.CreateDirectory(AudioPath);
        using (var writer = new BinaryWriter(File.Create(AudioPath + cue + ".wav")))
        {
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + result.Length * 2);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16); writer.Write((short)1); writer.Write((short)1);
            writer.Write(Rate); writer.Write(Rate * 2); writer.Write((short)2); writer.Write((short)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(result.Length * 2);
            foreach (double value in result) writer.Write((short)(value / peak * .62 * short.MaxValue));
        }
    }

    /// <summary>Decodes editor-only licensed source recordings, downmixing before the authored edits.</summary>
    private static double[] Read(string file)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(SourcePath + file);
        if (clip == null || clip.frequency != Rate) throw new InvalidOperationException("Missing 44.1-kHz source: " + file);
        var pcm = new float[clip.samples * clip.channels];
        if (!clip.GetData(pcm, 0)) throw new InvalidOperationException("Cannot decode source: " + file);
        var mono = new double[clip.samples];
        for (int i = 0; i < mono.Length; i++)
            for (int channel = 0; channel < clip.channels; channel++) mono[i] += pcm[i * clip.channels + channel] / clip.channels;
        return mono;
    }

    /// <summary>Extracts one known stroke; source intervals are recorded in the provenance document.</summary>
    private static double[] Slice(double[] source, double start, double duration) => source.Skip((int)(start * Rate)).Take((int)(duration * Rate)).ToArray();

    /// <summary>Preserves the recorded attack of an isolated tooth contact instead of trimming into its transient.</summary>
    private static double[] Contact(double[] source, double start, double duration)
    {
        int first = (int)(start * Rate), last = Math.Min(source.Length, first + (int)(duration * Rate)), peak = first;
        for (int i = first; i < last; i++) if (Math.Abs(source[i]) > Math.Abs(source[peak])) peak = i;
        return Slice(source, Math.Max(start, (double)peak / Rate - .002), .065);
    }

    /// <summary>Layers source recordings with modest pitch changes and click-free attacks, never changing runtime timing.</summary>
    private static void Mix(double[] target, double[] source, double at, double pitch, double gain)
    {
        int offset = (int)(at * Rate);
        for (int i = 0; i + offset < target.Length; i++)
        {
            double position = i * pitch; int sample = (int)position;
            if (sample + 1 >= source.Length) break;
            double value = source[sample] + (source[sample + 1] - source[sample]) * (position - sample);
            double envelope = Math.Min(1, (double)i / (Rate * .001)) * Math.Min(1, (source.Length - 1 - position) / (Rate * .005));
            target[i + offset] += value * gain * envelope;
        }
    }
}
