using UnityEngine;

/// <summary>Synchronizes bounded mechanical audio with resolved reel motion; never rolls results or changes gameplay clocks.</summary>
public sealed class TackleSlotAudio : MonoBehaviour
{
    [SerializeField] private AudioSource rolling;
    [SerializeField] private AudioSource[] voices;
    [SerializeField] private AudioClip start, roll, tick, stop, latch, unlatch, pair, triple;
    [SerializeField] private UnityEngine.UI.Toggle soundToggle;
    private readonly bool[] moving = new bool[3], locks = new bool[3];
    private readonly float[] phases = new float[3];
    private readonly double[] nextTick = new double[3];
    private bool initializedLocks, spinning;
    private int voice;
    private SlotMatchKind match;
    public int Starts { get; private set; }
    public int Stops { get; private set; }
    public int Matches { get; private set; }
    public int Latches { get; private set; }
    public bool IsRolling => rolling != null && rolling.isPlaying;

    /// <summary>Binds the shared preference without treating initial synchronization as a user action.</summary>
    private void Awake() { soundToggle.SetIsOnWithoutNotify(GameAudioPreferences.SoundEnabled); soundToggle.onValueChanged.AddListener(SetSound); }
    /// <summary>Reloads another phase's sound choice on entry.</summary>
    private void OnEnable() { if (soundToggle != null) soundToggle.SetIsOnWithoutNotify(GameAudioPreferences.SoundEnabled); }
    /// <summary>Stops all owned voices immediately at phase exit; no win cue can leak into the next encounter.</summary>
    private void OnDisable() { Cancel(); initializedLocks = false; }
    /// <summary>Releases the setting listener owned by this view.</summary>
    private void OnDestroy() { if (soundToggle != null) soundToggle.onValueChanged.RemoveListener(SetSound); }
    /// <summary>Persists a deliberate choice and immediately silences current motion when muted.</summary>
    private void SetSound(bool enabled) { GameAudioPreferences.SetSoundEnabled(enabled); if (!enabled) StopVoices(); }

    /// <summary>Starts once at a new visual spin revision, retaining the locked reel's silence.</summary>
    public void BeginSpin(TackleReelAnimator[] reels, SlotMatchKind result)
    {
        Cancel(); spinning = true; match = result; Starts++;
        for (int i = 0; i < 3; i++) { moving[i] = reels[i].IsSpinning; phases[i] = reels[i].Phase; nextTick[i] = 0; }
        Play(start, 1, -9);
        if (GameAudioPreferences.SoundEnabled) { rolling.clip = roll; rolling.pitch = 1; rolling.volume = Gain(-23); rolling.Play(); }
    }
    /// <summary>Plays latch changes only after an initial baseline; refreshes and rejected commands stay silent.</summary>
    public void RefreshLocks(TagSlotMachineRuntime machine)
    {
        for (int i = 0; i < 3; i++)
        {
            bool value = machine.IsLocked(i);
            if (initializedLocks && locks[i] != value) { Latches++; Play(value ? latch : unlatch, 1, -12); }
            locks[i] = value;
        }
        initializedLocks = true;
    }
    /// <summary>Samples actual reel movement after animation advances, throttling crossings instead of emitting catch-up bursts.</summary>
    public void Tick(TackleReelAnimator[] reels)
    {
        if (!GameAudioPreferences.SoundEnabled) StopVoices();
        if (!spinning) return;
        int remaining = 0;
        for (int i = 0; i < 3; i++)
        {
            if (!moving[i]) continue;
            if (!reels[i].IsSpinning) { moving[i] = false; Stops++; Play(stop, .94f + i * .07f, -10); continue; }
            remaining++;
            float phase = reels[i].Phase;
            if (Mathf.FloorToInt(phase / (Mathf.PI * .5f)) != Mathf.FloorToInt(phases[i] / (Mathf.PI * .5f))
                && Time.realtimeSinceStartupAsDouble >= nextTick[i])
            { Play(tick, .96f + i * .04f, -24); nextTick[i] = Time.realtimeSinceStartupAsDouble + .055; }
            phases[i] = phase;
        }
        rolling.pitch = .78f + remaining * .075f; rolling.volume = Gain(-26 + remaining);
        if (remaining != 0)
        {
            if (GameAudioPreferences.SoundEnabled && !rolling.isPlaying) rolling.Play();
            return;
        }
        spinning = false; rolling.Stop();
        if (match != SlotMatchKind.ThreeDifferent) { Matches++; Play(match == SlotMatchKind.Triple ? triple : pair, 1, -10); }
    }
    /// <summary>Cancels pending sounds without producing synthetic reel stops or result cues.</summary>
    public void Cancel() { spinning = false; for (int i = 0; i < 3; i++) moving[i] = false; StopVoices(); }
    /// <summary>Uses a fixed pool, 2D routing and decibel gain; visual refreshes never allocate sound objects.</summary>
    private void Play(AudioClip clip, float pitch, float decibels)
    {
        if (!GameAudioPreferences.SoundEnabled || clip == null || voices.Length == 0) return;
        var source = voices[voice++ % voices.Length]; source.Stop(); source.clip = clip; source.pitch = pitch; source.volume = Gain(decibels); source.Play();
    }
    /// <summary>Silences motion and all one-shot tails while preserving sampled animation progress.</summary>
    private void StopVoices() { if (rolling != null) rolling.Stop(); if (voices != null) foreach (var source in voices) if (source != null) source.Stop(); }
    /// <summary>Converts authored mix levels to the linear amplitude required by AudioSource.</summary>
    private static float Gain(float decibels) => Mathf.Pow(10, decibels / 20);
}
