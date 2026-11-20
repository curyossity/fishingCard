using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Authors original damped wood, reel and brass cues, preserving the previous sound set.</summary>
public static class LandingMechanicalAudioSetup
{
    public const string Path = LandingScoreRushSetup.AudioPath + "Mechanical/";
    /// <summary>Creates deterministic PCM assets only when missing and binds them to the existing routed voices.</summary>
    public static void Configure(SerializedObject host)
    {
        Directory.CreateDirectory(Path);
        foreach (string cue in new[] { "hit", "addition", "multiplier", "windup", "payoutSmall", "payoutBig", "miss" })
        {
            string file = Path + cue + ".wav";
            if (!File.Exists(file)) Write(file, cue);
            AssetDatabase.ImportAsset(file, ImportAssetOptions.ForceSynchronousImport);
            var importer = (AudioImporter)AssetImporter.GetAtPath(file); var settings = importer.defaultSampleSettings;
            settings.compressionFormat = AudioCompressionFormat.PCM; settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.preloadAudioData = true; importer.defaultSampleSettings = settings; importer.forceToMono = true; importer.SaveAndReimport();
            host.FindProperty(cue).objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(file);
        }
        host.ApplyModifiedPropertiesWithoutUndo();
    }
    /// <summary>Combines short seeded friction transients with damped resonances instead of a sustained notification beep.</summary>
    private static void Write(string file, string cue)
    {
        const int rate = 44100;
        double duration = cue == "payoutBig" ? .65 : cue == "windup" ? .18 : cue == "payoutSmall" ? .34 : .22;
        var data = new double[(int)(rate * duration)]; uint noise = 731;
        double peak = .001;
        for (int i = 0; i < data.Length; i++)
        {
            double t = (double)i / rate;
            noise = unchecked(noise * 1664525 + 1013904223); double grit = (noise / (double)uint.MaxValue * 2 - 1);
            double wood = Resonance(t, 185, .045) + .45 * Resonance(t, 413, .025);
            double sample;
            if (cue == "windup")
            {
                sample = 0;
                for (int tooth = 0; tooth < 5; tooth++) sample += .22 * Resonance(t - tooth * .026, 820 + tooth * 75, .009);
            }
            else if (cue == "addition") sample = wood * .65 + grit * Math.Exp(-t * 170) * .27;
            else if (cue == "multiplier") sample = wood * .6 + Resonance(t, 294, .085) * .6 + Resonance(t, 735, .04) * .2 + grit * Math.Exp(-t * 210) * .2;
            else if (cue == "miss") sample = Resonance(t, 95, .06) * .5 + grit * Math.Exp(-t * 110) * .12;
            else if (cue.StartsWith("payout"))
            {
                bool big = cue == "payoutBig";
                sample = Resonance(t, 110, .065) * .7 + Resonance(t, 220, .12) * .35;
                for (int tooth = 0; tooth < (big ? 8 : 3); tooth++) sample += .17 * Resonance(t - tooth * .028, 760, .012);
                if (big) sample += .3 * Resonance(t - .08, 440, .16) + .17 * Resonance(t - .12, 660, .16);
            }
            else sample = wood + Resonance(t, 82, .045) * .4 + grit * Math.Exp(-t * 150) * .2;
            sample *= Math.Min(1, t / .002) * Math.Min(1, (duration - t) / .012);
            data[i] = sample; peak = Math.Max(peak, Math.Abs(sample));
        }
        using (var writer = new BinaryWriter(File.Create(file)))
        {
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + data.Length * 2);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16); writer.Write((short)1); writer.Write((short)1);
            writer.Write(rate); writer.Write(rate * 2); writer.Write((short)2); writer.Write((short)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(data.Length * 2);
            foreach (double sample in data) writer.Write((short)(sample / peak * .7 * short.MaxValue));
        }
    }
    /// <summary>Produces a resonating component only after its mechanical contact time.</summary>
    private static double Resonance(double t, double frequency, double decay) => t < 0 ? 0 : Math.Sin(2 * Math.PI * frequency * t) * Math.Exp(-t / decay);
}
