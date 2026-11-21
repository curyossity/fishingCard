using System;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>Phase-owned, bounded 2D voices with cue-level variation, DSP scheduling and immediate cancellation.</summary>
public sealed class DeepwaterSoundPlayer : MonoBehaviour
{
    [SerializeField] private DeepwaterSoundLibrary library;
    [SerializeField] private AudioSource[] voices = Array.Empty<AudioSource>();
    [SerializeField] private AudioMixerGroup[] buses = Array.Empty<AudioMixerGroup>();
    private readonly int[] takes = new int[Enum.GetValues(typeof(DeepwaterSoundCue)).Length];
    private readonly double[] allowedAt = new double[Enum.GetValues(typeof(DeepwaterSoundCue)).Length];
    private double[] ends = Array.Empty<double>();
    private int[] priorities = Array.Empty<int>();
    public int PlayedCount { get; private set; }
    public int DroppedCount { get; private set; }
    public DeepwaterSoundCue LastCue { get; private set; }
    public DeepwaterSoundLibrary Library => library;

    /// <summary>Allocates voice bookkeeping once, independently of shared authored sound data.</summary>
    private void Awake() { ends = new double[voices.Length]; priorities = new int[voices.Length]; }
    /// <summary>Never carries stale cooldowns or pending scheduled voices into a new phase visit.</summary>
    private void OnEnable() { Array.Clear(allowedAt, 0, allowedAt.Length); GameAudioPreferences.SoundChanged += OnSoundChanged; }
    /// <summary>Stops pending DSP events as well as audible tails at phase exit.</summary>
    private void OnDisable() { GameAudioPreferences.SoundChanged -= OnSoundChanged; StopAll(); }
    /// <summary>Responds to mute changes made by any other presentation owner.</summary>
    private void OnSoundChanged(bool enabled) { if (!enabled) StopAll(); }

    /// <summary>Freezes a take at schedule construction time so exported and live score sequences use the same audio.</summary>
    public AudioClip Select(DeepwaterSoundCue cue)
    {
        var definition = library != null ? library.Find(cue) : null;
        if (definition == null || definition.Takes.Length == 0) return null;
        int index = (int)cue;
        return definition.Takes[takes[index]++ % definition.Takes.Length];
    }

    /// <summary>Plays an accepted immediate event; repeated UI refreshes must never call this method.</summary>
    public void Play(DeepwaterSoundCue cue, float pitch = 1, float gainDb = 0, float pan = 0)
    {
        var definition = library != null ? library.Find(cue) : null;
        if (definition == null || !GameAudioPreferences.SoundEnabled || !isActiveAndEnabled) return;
        double now = AudioSettings.dspTime;
        if (now < allowedAt[(int)cue]) return;
        allowedAt[(int)cue] = now + definition.Cooldown;
        PlayFrozen(cue, Select(cue), CuePitch(cue, pitch), Gain(definition.GainDb + gainDb), now, pan);
    }

    /// <summary>Schedules a frozen score take on the audio clock, protecting important impacts from disposable UI voices.</summary>
    public void PlayFrozen(DeepwaterSoundCue cue, AudioClip clip, float pitch, float gain, double at, float pan = 0)
    {
        if (!GameAudioPreferences.SoundEnabled || !isActiveAndEnabled || clip == null || voices.Length == 0) return;
        var definition = library.Find(cue);
        if (definition == null) return;
        double now = AudioSettings.dspTime;
        int slot = -1;
        for (int i = 0; i < voices.Length; i++) if (ends[i] <= now) { slot = i; break; }
        if (slot < 0)
        {
            // Lower numbers have higher priority, matching Unity's own voice convention.
            for (int i = 0; i < voices.Length; i++)
                if (priorities[i] >= definition.Priority && (slot < 0 || priorities[i] > priorities[slot] || priorities[i] == priorities[slot] && ends[i] < ends[slot])) slot = i;
        }
        if (slot < 0) { DroppedCount++; return; }
        var source = voices[slot]; source.Stop(); source.clip = clip;
        source.pitch = Mathf.Clamp(pitch, .5f, 2); source.volume = Mathf.Clamp01(gain); source.panStereo = Mathf.Clamp(pan, -.2f, .2f);
        source.priority = definition.Priority; source.outputAudioMixerGroup = buses[(int)definition.Bus];
        if (at > now + .002) source.PlayScheduled(at); else source.Play();
        ends[slot] = Math.Max(now, at) + clip.length / source.pitch; priorities[slot] = definition.Priority;
        LastCue = cue; PlayedCount++;
    }

    /// <summary>Converts an authored cue's mix gain once for a reproducible sound event.</summary>
    public float CueGain(DeepwaterSoundCue cue, float extraDb = 0) => Gain(library.Find(cue).GainDb + extraDb);
    /// <summary>Retains each sound family's intended pitch excursion when freezing a score event or playing an immediate cue.</summary>
    public float CuePitch(DeepwaterSoundCue cue, float pitch) => Mathf.Clamp(1 + (pitch - 1) * library.Find(cue).PitchGrowth, .5f, 2);
    /// <summary>Returns authored intensity support independently of gameplay amplification or source timing.</summary>
    public float CueIntensityGainDb(DeepwaterSoundCue cue) => library.Find(cue).IntensityGainDb;
    /// <summary>Silences this phase, including voices scheduled but not yet started.</summary>
    public void StopAll()
    {
        foreach (var source in voices) if (source != null) source.Stop();
        Array.Clear(ends, 0, ends.Length);
    }
    /// <summary>Converts decibels only at the AudioSource boundary.</summary>
    public static float Gain(float decibels) => Mathf.Pow(10, decibels / 20);
}
