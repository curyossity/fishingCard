using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Checks authored coverage, actual score schedules and cancellation of future audio without touching player saves.</summary>
public static class DeepwaterAudioChecks
{
    public const string Output = "Docs/Validation/ChargedCabinet/";
    public static string Result { get; private set; } = "Not run";
    private static int checks;
    [Serializable] private sealed class ExportEvent { public float t, pitch, gain; public string kind, file; }
    [Serializable] private sealed class ExportTimeline { public float duration; public string note; public ExportEvent[] events; }

    /// <summary>Checks every cue, take, native import and phase route rather than trusting a few isolated successful samples.</summary>
    public static int ValidateAssets()
    {
        checks = 0; var library = AssetDatabase.LoadAssetAtPath<DeepwaterSoundLibrary>(DeepwaterAudioSetup.LibraryPath);
        Check(library != null, "Missing sound library.");
        Check(library.Definitions.Count == Enum.GetValues(typeof(DeepwaterSoundCue)).Length, "Cue coverage or uniqueness is incomplete.");
        foreach (DeepwaterSoundCue cue in Enum.GetValues(typeof(DeepwaterSoundCue)))
        {
            var definition = library.Find(cue); Check(definition != null && definition.Takes.Length > 0, "Unassigned cue: " + cue);
            Check(library.Definitions.Count(d => d.Cue == cue) == 1, "Duplicate cue: " + cue);
            Check(definition.GainDb <= 0 && definition.GainDb >= -60 && definition.Priority >= 0 && definition.Priority <= 256, "Unbounded cue mix: " + cue);
            Check(definition.PitchGrowth >= 0 && definition.PitchGrowth <= 1 && definition.IntensityGainDb >= 0 && definition.IntensityGainDb <= 3, "Unbounded cue expression: " + cue);
            foreach (var clip in definition.Takes)
            {
                Check(clip != null && clip.channels == 1 && clip.frequency == 48000 && clip.loadState == AudioDataLoadState.Loaded, "Cue not ready for immediate 2D playback: " + cue);
                var pcm = new float[clip.samples]; Check(clip.GetData(pcm, 0), "PCM unavailable: " + cue);
                Check(pcm.Any(v => Math.Abs(v) > .01) && pcm.All(v => Math.Abs(v) <= .65), "Silent or clipped cue: " + cue);
                Check(cue == DeepwaterSoundCue.SlotRoll ? Math.Abs(pcm[0] - pcm.Last()) < .001 : Math.Abs(pcm[0]) < .001 && Math.Abs(pcm.Last()) < .001, "Non-seamless cue edge: " + cue);
                var importer = (AudioImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(clip));
                Check(importer.defaultSampleSettings.compressionFormat == AudioCompressionFormat.PCM && importer.defaultSampleSettings.loadType == AudioClipLoadType.DecompressOnLoad && importer.defaultSampleSettings.preloadAudioData, "Lossy or deferred short cue: " + cue);
            }
        }
        foreach (string path in new[] { TackleDiscoverArtSetup.PrefabPath, LandingChallengeSetup.PrefabPath, RigBuildingSetup.PrefabPath, SlotMachineSetup.PreparationPath })
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(path); var player = root.GetComponent<DeepwaterSoundPlayer>();
            Check(player != null && player.Library == library, "Phase uses another palette: " + path);
            var data = new SerializedObject(player); var buses = data.FindProperty("buses");
            Check(buses.arraySize == 3 && Enumerable.Range(0, 3).All(i => buses.GetArrayElementAtIndex(i).objectReferenceValue != null), "Incomplete category routing: " + path);
            var voices = data.FindProperty("voices"); Check(voices.arraySize >= 4 && voices.arraySize <= 8, "Unbounded phase voices: " + path);
            foreach (var source in root.GetComponentsInChildren<AudioSource>(true)) Check(source.outputAudioMixerGroup != null && !source.playOnAwake && source.spatialBlend == 0, "Unrouted, automatic or 3D voice: " + path);
        }
        Directory.CreateDirectory(Output); File.WriteAllText(Output + "AssetVerification.json", "{\"passed\":true,\"assertions\":" + checks + "}");
        return checks;
    }

    /// <summary>Starts isolated schedule and voice checks in a disposable prefab, with preferences restored afterward.</summary>
    public static void Begin()
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Enter Play mode for audio schedule checks.");
        checks = 0; Result = "Running";
        UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>().Controller.StartCoroutine(Run());
    }

    /// <summary>Checks complete receipt schedules, two-clock cancellation, cue priority and lack of mute backlog.</summary>
    private static IEnumerator Run()
    {
        bool preference = GameAudioPreferences.SoundEnabled;
        var root = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(LandingChallengeSetup.PrefabPath)); root.SetActive(true);
        try
        {
            GameAudioPreferences.SetSoundEnabled(true);
            var player = root.GetComponent<DeepwaterSoundPlayer>(); var rush = root.GetComponent<LandingScoreRush>();
            ((UnityEngine.UI.Toggle)new SerializedObject(rush).FindProperty("soundToggle").objectReferenceValue).SetIsOnWithoutNotify(true);
            foreach (var item in new[] { (LandingAccuracy.Miss, 0, "RuntimeMiss"), (LandingAccuracy.Good, 0, "RuntimeOrdinary"), (LandingAccuracy.Perfect, 7, "RuntimeStrong"), (LandingAccuracy.Perfect, 30, "RuntimeExceptional") })
            {
                var receipt = Receipt(item.Item1, item.Item2); int played = player.PlayedCount; rush.Present(receipt, Array.Empty<TackleRigCardView>(), true);
                Check(player.PlayedCount > played, "Accepted receipt did not request its initial sound.");
                var events = rush.SoundEvents.ToArray(); var sequence = rush.Sequence;
                Check(events.All(e => e.Clip != null && e.Time >= 0 && e.Time < rush.Timing.Duration), "Cue lies outside its bounded receipt.");
                Check(events.First().Time == 0 && events.First().Cue == (item.Item1 == LandingAccuracy.Miss ? DeepwaterSoundCue.Miss : DeepwaterSoundCue.Strike), "Accepted action lacks immediate contact.");
                if (item.Item1 == LandingAccuracy.Miss)
                    Check(events.Length == 1 && events[0].Cue == DeepwaterSoundCue.Miss, "Zero-point Miss repeated or celebrated.");
                else
                {
                    Check(events.Count(e => Math.Abs(e.Time - rush.Timing.ToReal(sequence.PayoutAt)) < .00001f) == 1, "Payout missing or repeated.");
                    Check(events.Any(e => e.Time == sequence.Steps[0].ImpactAt && e.Cue == (item.Item1 == LandingAccuracy.Perfect ? DeepwaterSoundCue.Perfect : DeepwaterSoundCue.Good)), "Accuracy quality lacks distinct impact sound.");
                    foreach (var step in sequence.Steps.Skip(1))
                    {
                        Check(events.Count(e => Math.Abs(e.Time - rush.Timing.ToReal(step.ImpactAt)) < .00001f && e.Cue == (step.Multiply ? DeepwaterSoundCue.Multiplier : DeepwaterSoundCue.Addition)) == 1, "Rig operation misses or duplicates its exact impact.");
                        var travel = events.Single(e => Math.Abs(e.Time - rush.Timing.ToReal(step.LaunchAt)) < .00001f && e.Cue == DeepwaterSoundCue.Travel);
                        Check(Math.Abs(travel.Time + travel.Clip.length / travel.Pitch - rush.Timing.ToReal(step.ImpactAt)) <= .012, "Projectile approach tail does not meet its arrival.");
                    }
                }
                Check(events.All(e => e.Pitch >= .5f && e.Pitch <= 2 && e.Gain <= 1), "Unbounded pitch/gain in score schedule.");
                foreach (var impact in events.Where(e => e.Cue == DeepwaterSoundCue.Addition || e.Cue == DeepwaterSoundCue.Multiplier))
                {
                    int stepIndex = Enumerable.Range(1, sequence.Steps.Count - 1).Single(i => Math.Abs(rush.Timing.ToReal(sequence.Steps[i].ImpactAt) - impact.Time) < .00001f);
                    Check(Math.Abs(impact.Pitch - player.CuePitch(impact.Cue, LandingScoreSequence.NotePitch(stepIndex - 1))) < .00001f, "Score pitch differs from the selected sound-family treatment.");
                }
                Export(item.Item3, rush);
                var first = events.ToArray(); rush.Present(receipt, Array.Empty<TackleRigCardView>(), true);
                Check(first.SequenceEqual(rush.SoundEvents), "Refreshing rerolled a receipt's sound takes.");
                rush.Finish();
                Check(root.GetComponentsInChildren<AudioSource>(true).All(s => !s.isPlaying), "Finish retained sounding or scheduled score voices.");
            }
            var sourceData = new SerializedObject(player); var voices = sourceData.FindProperty("voices");
            int drops = player.DroppedCount;
            for (int i = 0; i < voices.arraySize; i++) player.PlayFrozen(DeepwaterSoundCue.PayoutBig, player.Select(DeepwaterSoundCue.PayoutBig), 1, .1f, AudioSettings.dspTime + 4);
            player.Play(DeepwaterSoundCue.Preview);
            Check(player.DroppedCount == drops + 1, "Incidental feedback stole a reserved high-priority payout.");
            GameAudioPreferences.SetSoundEnabled(false);
            Check(root.GetComponentsInChildren<AudioSource>(true).All(s => !s.isPlaying), "Mute failed to cancel future DSP voices.");
            GameAudioPreferences.SetSoundEnabled(true);
            player.PlayFrozen(DeepwaterSoundCue.PayoutBig, player.Select(DeepwaterSoundCue.PayoutBig), 1, .1f, AudioSettings.dspTime + 4);
            GameAudioPreferences.SetSoundEnabled(false);
            Check(root.GetComponentsInChildren<AudioSource>(true).All(s => !s.isPlaying), "Same-frame unmute/remute retained a future payout.");
            yield return null;
            int before = player.PlayedCount; player.Play(DeepwaterSoundCue.Upgrade);
            Check(player.PlayedCount == before, "Muted cue was scheduled.");
            GameAudioPreferences.SetSoundEnabled(true); yield return null;
            Check(root.GetComponentsInChildren<AudioSource>(true).All(s => !s.isPlaying), "Unmute replayed a stale celebration.");
            player.PlayFrozen(DeepwaterSoundCue.Triple, player.Select(DeepwaterSoundCue.Triple), 1, .1f, AudioSettings.dspTime + 4);
            root.SetActive(false);
            Check(root.GetComponentsInChildren<AudioSource>(true).All(s => !s.isPlaying), "Phase exit retained future reward audio.");
            Result = "Passed " + checks + " audio schedule/voice checks";
            File.WriteAllText(Output + "ScheduleVerification.json", "{\"passed\":true,\"assertions\":" + checks + "}");
        }
        finally { GameAudioPreferences.SetSoundEnabled(preference); UnityEngine.Object.Destroy(root); }
    }

    /// <summary>Builds review receipts through actual installed abilities and the real Landing model, without changing game content.</summary>
    private static LandingStrikeReceipt Receipt(LandingAccuracy accuracy, int level)
    {
        var rig = new RigRuntime();
        if (level > 0)
            foreach (string name in new[] { "Sardine", "Glass Minnow", "Mackerel", "Cuttlefish", "Stormjaw Tuna" })
            {
                var card = AssetDatabase.FindAssets("t:CardDefinition", new[] { "Assets/Cards" }).Select(g => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).Single(c => c.DisplayName == name);
                for (int i = 0; i < (name == "Sardine" || name == "Cuttlefish" || name == "Stormjaw Tuna" ? level : 1); i++)
                    Check(rig.TryAcquire(rig.Revision, card, rig.Members.Length, out _, out _), "Could not create disposable audio review rig.");
            }
        var rules = rig.SnapshotForLanding(out int extra, out int protect); var model = new LandingChallengeRuntime(); double clock = 0;
        model.Begin(1, 2000000, LandingChallengeChecks.Settings(), rules, new System.Random(12), clock, extra, protect);
        LandingChallengeChecks.Strike(model, accuracy, ref clock);
        return new LandingStrikeReceipt(model, rig.Members);
    }

    /// <summary>Exports the actual frozen event list without normalizing away its contrast or substituting a different sample rate.</summary>
    private static void Export(string name, LandingScoreRush rush)
    {
        Directory.CreateDirectory(Output);
        File.WriteAllBytes(Output + name + ".wav", LandingScoreShowcase.Mix(rush.SoundEvents.ToArray(), rush.Timing.Duration));
        var timeline = new ExportTimeline { duration = rush.Timing.Duration, note = "Frozen runtime cue schedule and rendered mix; not a device loopback recording.",
            events = rush.SoundEvents.Select(e => new ExportEvent { t = e.Time, pitch = e.Pitch, gain = e.Gain, kind = e.Cue.ToString(), file = AssetDatabase.GetAssetPath(e.Clip) }).ToArray() };
        File.WriteAllText(Output + name + ".json", JsonUtility.ToJson(timeline, true));
    }
    /// <summary>Reports a specific failed contract and retains asynchronous test status.</summary>
    private static void Check(bool valid, string message) { if (!valid) { Result = "FAILED: " + message; throw new InvalidOperationException(message); } checks++; }
}
