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
    private readonly bool[] braked = new bool[3];
    private readonly double[] stopDsp = { double.NaN, double.NaN, double.NaN };
    private readonly float[] phases = new float[3];
    private readonly double[] nextTick = new double[3];
    private bool initializedLocks, spinning;
    private float rollTarget;
    private SlotMatchKind match;
    private int movingAtStart;
    private const float AudioLookAhead = .06f, LateTolerance = .08f;
    public int Starts { get; private set; }
    public int Stops { get; private set; }
    public int Matches { get; private set; }
    public int Latches { get; private set; }
    public bool IsRolling => rolling != null && rolling.isPlaying;
    public double MatchDspTime { get; private set; } = double.NaN;

    /// <summary>Exposes the current spin's requested contact deadline for synchronization diagnostics.</summary>
    public double StopDspTime(int reel) => stopDsp[reel];

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
    private void OnSoundChanged(bool enabled)
    {
        if (enabled) return;
        StopVoices();
        double now = AudioSettings.dspTime;
        // A queued cue may already have reached the audio thread before the next visual update.
        // Preserve consumed deadlines so a same-frame unmute cannot repeat that contact/result.
        if (double.IsNaN(MatchDspTime) || MatchDspTime > now) MatchDspTime = double.NaN;
        for (int i = 0; i < 3; i++) if (!braked[i] && (double.IsNaN(stopDsp[i]) || stopDsp[i] > now)) stopDsp[i] = double.NaN;
    }

    /// <summary>Starts once at a new visual spin revision, retaining the locked reel's silence.</summary>
    public void BeginSpin(TackleReelAnimator[] reels, SlotMatchKind result)
    {
        Cancel(); spinning = true; match = result; Starts++;
        movingAtStart = 0;
        for (int i = 0; i < 3; i++) { moving[i] = reels[i].IsSpinning; phases[i] = reels[i].Phase; nextTick[i] = 0; if (moving[i]) movingAtStart++; }
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
    /// <summary>Follows actual forward travel, schedules the brake contact and leaves elastic recovery acoustically quiet.</summary>
    public void Tick(TackleReelAnimator[] reels)
    {
        if (!spinning) return;
        int remaining = 0;
        float velocity = 0, settlement = 0;
        for (int i = 0; i < 3; i++)
        {
            if (!moving[i]) continue;
            var reel = reels[i];
            ScheduleContact(reel, i);
            if (!braked[i] && reel.HasBrakeContact) { braked[i] = true; Stops++; }
            if (!reel.IsSpinning) { moving[i] = false; continue; }
            remaining++;
            velocity += reel.ForwardVelocityFraction;
            settlement = Mathf.Max(settlement, reel.SecondsToSettlement);
            float phase = reel.Phase;
            if (!braked[i] && Mathf.FloorToInt(phase / (Mathf.PI * .5f)) != Mathf.FloorToInt(phases[i] / (Mathf.PI * .5f))
                && Time.realtimeSinceStartupAsDouble >= nextTick[i])
            { sound.Play(DeepwaterSoundCue.SlotTick, pan: (i - 1) * .12f); nextTick[i] = Time.realtimeSinceStartupAsDouble + .055; }
            phases[i] = phase;
        }
        float speed = Mathf.Clamp01(velocity / Mathf.Max(1, movingAtStart));
        rolling.pitch = .78f + .22f * Mathf.Sqrt(speed);
        rollTarget = speed > 0 ? sound.CueGain(DeepwaterSoundCue.SlotRoll) * Mathf.Sqrt(speed) : 0;
        rolling.volume = Mathf.MoveTowards(rolling.volume, rollTarget, Time.unscaledDeltaTime * sound.CueGain(DeepwaterSoundCue.SlotRoll) / .04f);
        if (rollTarget == 0 && rolling.volume < .0001f) rolling.Stop();
        if (match != SlotMatchKind.ThreeDifferent && double.IsNaN(MatchDspTime) && settlement <= AudioLookAhead && GameAudioPreferences.SoundEnabled)
        {
            var cue = match == SlotMatchKind.Triple ? DeepwaterSoundCue.Triple : DeepwaterSoundCue.Pair;
            MatchDspTime = AudioSettings.dspTime + Mathf.Max(0, settlement);
            sound.PlayFrozen(cue, sound.Select(cue), 1, sound.CueGain(cue), MatchDspTime);
        }
        if (remaining != 0)
        {
            if (GameAudioPreferences.SoundEnabled && rollTarget > 0 && !rolling.isPlaying) rolling.Play();
            return;
        }
        spinning = false; rolling.Stop(); rollTarget = 0;
        if (match != SlotMatchKind.ThreeDifferent) Matches++;
    }
    /// <summary>Uses the animator's actual brake deadline; a slow frame drops an old contact instead of bunching missed stops.</summary>
    private void ScheduleContact(TackleReelAnimator reel, int index)
    {
        float delay = reel.SecondsToBrakeContact;
        if (braked[index] || !double.IsNaN(stopDsp[index]) || !GameAudioPreferences.SoundEnabled || delay > AudioLookAhead || delay < -LateTolerance) return;
        var cue = (DeepwaterSoundCue)((int)DeepwaterSoundCue.SlotStop1 + index);
        stopDsp[index] = AudioSettings.dspTime + Mathf.Max(0, delay);
        sound.PlayFrozen(cue, sound.Select(cue), 1, sound.CueGain(cue), stopDsp[index], (index - 1) * .16f);
    }
    /// <summary>Cancels pending sounds without producing synthetic reel stops or result cues.</summary>
    public void Cancel()
    {
        spinning = false; rollTarget = 0; MatchDspTime = double.NaN;
        for (int i = 0; i < 3; i++) { moving[i] = false; braked[i] = false; stopDsp[i] = double.NaN; }
        StopVoices();
    }
    /// <summary>Silences motion and pending result cues without changing sampled animation progress.</summary>
    private void StopVoices() { if (rolling != null) rolling.Stop(); if (sound != null) sound.StopAll(); }
}
