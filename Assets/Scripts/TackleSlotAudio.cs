using UnityEngine;

/// <summary>Synchronizes bounded mechanical audio with resolved reel motion; never rolls results or changes gameplay clocks.</summary>
public sealed class TackleSlotAudio : MonoBehaviour
{
    [SerializeField] private DeepwaterSoundPlayer sound;
    [SerializeField] private AudioSource rolling;
    // Keep serialized review aliases for older prefab/validation consumers; active one-shots use the library.
    [SerializeField, HideInInspector] private AudioSource[] voices;
    [SerializeField, HideInInspector] private AudioClip start, tick, stop, latch, unlatch, pair, triple;
    [SerializeField] private AudioClip roll;
    [SerializeField] private UnityEngine.UI.Toggle soundToggle;
    private readonly bool[] moving = new bool[3], locks = new bool[3];
    private readonly float[] phases = new float[3];
    private readonly double[] nextTick = new double[3];
    private bool initializedLocks, spinning;
    private float rollTarget;
    private SlotMatchKind match;
    public int Starts { get; private set; }
    public int Stops { get; private set; }
    public int Matches { get; private set; }
    public int Latches { get; private set; }
    public bool IsRolling => rolling != null && rolling.isPlaying;

    /// <summary>Binds the shared preference without treating initial synchronization as a user action.</summary>
    private void Awake() { soundToggle.SetIsOnWithoutNotify(GameAudioPreferences.SoundEnabled); soundToggle.onValueChanged.AddListener(SetSound); }
    /// <summary>Reloads another phase's sound choice on entry.</summary>
    private void OnEnable() { if (soundToggle != null) soundToggle.SetIsOnWithoutNotify(GameAudioPreferences.SoundEnabled); GameAudioPreferences.SoundChanged += OnSoundChanged; }
    /// <summary>Stops all owned voices immediately at phase exit; no win cue can leak into the next encounter.</summary>
    private void OnDisable() { GameAudioPreferences.SoundChanged -= OnSoundChanged; Cancel(); initializedLocks = false; }
    /// <summary>Releases the setting listener owned by this view.</summary>
    private void OnDestroy() { if (soundToggle != null) soundToggle.onValueChanged.RemoveListener(SetSound); }
    /// <summary>Persists a deliberate choice and immediately silences current motion when muted.</summary>
    private void SetSound(bool enabled) { GameAudioPreferences.SetSoundEnabled(enabled); if (!enabled) StopVoices(); }
    /// <summary>Silences the dedicated motion loop immediately when another owner changes the shared preference.</summary>
    private void OnSoundChanged(bool enabled) { if (!enabled) StopVoices(); }

    /// <summary>Starts once at a new visual spin revision, retaining the locked reel's silence.</summary>
    public void BeginSpin(TackleReelAnimator[] reels, SlotMatchKind result)
    {
        Cancel(); spinning = true; match = result; Starts++;
        for (int i = 0; i < 3; i++) { moving[i] = reels[i].IsSpinning; phases[i] = reels[i].Phase; nextTick[i] = 0; }
        sound.Play(DeepwaterSoundCue.SlotStart);
        rollTarget = sound.CueGain(DeepwaterSoundCue.SlotRoll);
        if (GameAudioPreferences.SoundEnabled) { rolling.clip = roll; rolling.pitch = 1; rolling.volume = 0; rolling.Play(); }
    }
    /// <summary>Plays latch changes only after an initial baseline; refreshes and rejected commands stay silent.</summary>
    public void RefreshLocks(TagSlotMachineRuntime machine)
    {
        for (int i = 0; i < 3; i++)
        {
            bool value = machine.IsLocked(i);
            if (initializedLocks && locks[i] != value) { Latches++; sound.Play(value ? DeepwaterSoundCue.SlotLock : DeepwaterSoundCue.SlotUnlock); }
            locks[i] = value;
        }
        initializedLocks = true;
    }
    /// <summary>Samples actual reel movement after animation advances, throttling crossings instead of emitting catch-up bursts.</summary>
    public void Tick(TackleReelAnimator[] reels)
    {
        rolling.volume = Mathf.MoveTowards(rolling.volume, rollTarget, Time.unscaledDeltaTime * sound.CueGain(DeepwaterSoundCue.SlotRoll) / .04f);
        if (rollTarget == 0 && rolling.volume < .0001f) rolling.Stop();
        if (!spinning) return;
        int remaining = 0;
        for (int i = 0; i < 3; i++)
        {
            if (!moving[i]) continue;
            if (!reels[i].IsSpinning) { moving[i] = false; Stops++; sound.Play((DeepwaterSoundCue)((int)DeepwaterSoundCue.SlotStop1 + i), pan: (i - 1) * .16f); continue; }
            remaining++;
            float phase = reels[i].Phase;
            if (Mathf.FloorToInt(phase / (Mathf.PI * .5f)) != Mathf.FloorToInt(phases[i] / (Mathf.PI * .5f))
                && Time.realtimeSinceStartupAsDouble >= nextTick[i])
            { sound.Play(DeepwaterSoundCue.SlotTick, pan: (i - 1) * .12f); nextTick[i] = Time.realtimeSinceStartupAsDouble + .055; }
            phases[i] = phase;
        }
        rolling.pitch = .85f + remaining * .05f; rollTarget = sound.CueGain(DeepwaterSoundCue.SlotRoll, remaining == 1 ? -5 : remaining == 2 ? -2 : 0);
        if (remaining != 0)
        {
            if (GameAudioPreferences.SoundEnabled && !rolling.isPlaying) rolling.Play();
            return;
        }
        spinning = false; rolling.Stop(); rollTarget = 0;
        if (match != SlotMatchKind.ThreeDifferent)
        {
            Matches++;
            var cue = match == SlotMatchKind.Triple ? DeepwaterSoundCue.Triple : DeepwaterSoundCue.Pair;
            // Reserve a short pocket after the final stop; phase exit cancels this scheduled reward.
            sound.PlayFrozen(cue, sound.Select(cue), 1, sound.CueGain(cue), AudioSettings.dspTime + .09);
        }
    }
    /// <summary>Cancels pending sounds without producing synthetic reel stops or result cues.</summary>
    public void Cancel() { spinning = false; rollTarget = 0; for (int i = 0; i < 3; i++) moving[i] = false; StopVoices(); }
    /// <summary>Silences motion and pending result cues without changing sampled animation progress.</summary>
    private void StopVoices() { if (rolling != null) rolling.Stop(); if (sound != null) sound.StopAll(); }
}
