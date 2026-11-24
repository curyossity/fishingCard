using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>Stages a frozen strike into a readable score, weighted card collisions, musical steps and accumulated progress.</summary>
public sealed class LandingScoreRush : MonoBehaviour
{
    [SerializeField] private DeepwaterSoundPlayer sound;
    [SerializeField] private CanvasGroup readout;
    [SerializeField] private TMP_Text value, operation, caption;
    [SerializeField] private TMP_Text[] coveredLabels;
    [SerializeField] private UnityEngine.UI.Image glow;
    [SerializeField] private LandingScoringPortrait scoringPortrait;
    [SerializeField] private LandingCardCollision collision;
    [SerializeField] private RectTransform machine, totalLabel;
    [SerializeField] private LandingRushGraphic particles;
    [SerializeField] private LandingStrikeFeedback feedback;
    [SerializeField] private LandingStrikeContact strikeContact;
    // Serialized aliases retain older prefab/review compatibility; SoundEvents select active takes from the library.
    [SerializeField, HideInInspector] private AudioSource[] voices;
    [SerializeField, HideInInspector] private AudioClip hit, addition, multiplier, payoutSmall, payoutBig, miss;
    [SerializeField, HideInInspector] private AudioClip windup;
    [SerializeField] private UnityEngine.UI.Toggle soundToggle, reducedMotionToggle;
    private const string MotionKey = "Deepwater.Landing.ReducedMotion";
    private LandingStrikeReceipt receipt;
    private LandingScoreSequence sequence;
    private double started, dspStarted;
    private int lastStep = -1;
    private bool paid, running;
    private Vector2 machineRest;
    private Vector3 totalRest;
    private Vector3 operationRest = Vector3.one;
    public readonly struct SoundEvent
    {
        public readonly float Time, Pitch, Gain;
        public readonly AudioClip Clip;
        public readonly DeepwaterSoundCue Cue;
        /// <summary>Freezes one sound's timing and mix for runtime playback and reproducible review exports.</summary>
        public SoundEvent(float time, AudioClip clip, float pitch, float gain, DeepwaterSoundCue cue = DeepwaterSoundCue.Addition) { Time = time; Clip = clip; Pitch = pitch; Gain = gain; Cue = cue; }
    }
    private readonly List<SoundEvent> soundEvents = new List<SoundEvent>();
    private int soundCursor;
    public IReadOnlyList<SoundEvent> SoundEvents => soundEvents.AsReadOnly();
    private static readonly string[] Suffixes = { "M", "B", "T", "Q" };
    public bool IsPlaying => running;
    public LandingScoreSequence Sequence => sequence;
    public decimal DisplayedTotal { get; private set; }
    public decimal DisplayedStrike { get; private set; }
    public bool ReducedMotion => reducedMotionToggle != null && reducedMotionToggle.isOn;

    /// <summary>Loads only user presentation preferences and retains authored transform baselines.</summary>
    private void Awake()
    {
        machineRest = machine.anchoredPosition; totalRest = totalLabel.localScale; operationRest = operation.rectTransform.localScale;
        soundToggle.SetIsOnWithoutNotify(GameAudioPreferences.SoundEnabled);
        reducedMotionToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(MotionKey, 0) != 0);
        soundToggle.onValueChanged.AddListener(SetSound); reducedMotionToggle.onValueChanged.AddListener(SetReducedMotion);
    }
    /// <summary>Reflects a sound preference changed in Discover since Landing last appeared.</summary>
    private void OnEnable() { if (soundToggle != null) soundToggle.SetIsOnWithoutNotify(GameAudioPreferences.SoundEnabled); }
    /// <summary>Persists a deliberate sound choice, stopping tails immediately when muted.</summary>
    private void SetSound(bool active) { GameAudioPreferences.SetSoundEnabled(active); if (!active) StopVoices(); }
    /// <summary>Persists reduced motion and immediately restores moved visuals.</summary>
    private void SetReducedMotion(bool active) { PlayerPrefs.SetInt(MotionKey, active ? 1 : 0); PlayerPrefs.Save(); if (active) RestoreMotion(); }
    /// <summary>Stops owned sound and restores every transformed visual when the phase disappears.</summary>
    private void OnDisable() { Finish(); receipt = null; }

    /// <summary>Starts once per receipt; refreshing or inspecting the score cannot replay audio or animation.</summary>
    public void Present(LandingStrikeReceipt strike, IReadOnlyList<TackleRigCardView> rigCards, bool animate)
    {
        if (ReferenceEquals(receipt, strike)) return;
        Finish(); receipt = strike;
        if (strike?.Score == null || !animate) return;
        sound.StopAll();
        scoringPortrait.CaptureEncounter(); collision.Begin();
        sequence = new LandingScoreSequence(strike.Score); started = Time.realtimeSinceStartupAsDouble; dspStarted = AudioSettings.dspTime;
        if (strikeContact != null) strikeContact.Clear();
        soundEvents.Clear(); soundCursor = 0;
        bool missed = strike.Score.Accuracy == LandingAccuracy.Miss;
        AddSound(0, missed ? DeepwaterSoundCue.Miss : DeepwaterSoundCue.Strike);
        for (int i = 0; i < sequence.Steps.Count; i++)
        {
            var item = sequence.Steps[i];
            if (i == 0)
            {
                if (!missed) AddSound(item.ImpactAt, strike.Score.Accuracy == LandingAccuracy.Perfect ? DeepwaterSoundCue.Perfect : DeepwaterSoundCue.Good);
                continue;
            }
            float flight = item.ImpactAt - item.LaunchAt;
            float travelPitch = Mathf.Clamp(sound.Library.Find(DeepwaterSoundCue.Travel).Takes[0].length / flight, .5f, 2);
            AddSound(item.LaunchAt, DeepwaterSoundCue.Travel, travelPitch);
            var impactCue = item.Multiply ? DeepwaterSoundCue.Multiplier : DeepwaterSoundCue.Addition;
            AddSound(item.ImpactAt, impactCue, LandingScoreSequence.NotePitch(i - 1), sequence.Intensity * sound.CueIntensityGainDb(impactCue));
        }
        if (strike.Score.Points > 0) AddSound(sequence.PayoutAt, sequence.Tier == 2 ? DeepwaterSoundCue.PayoutBig : sequence.Tier == 1 ? DeepwaterSoundCue.PayoutMedium : DeepwaterSoundCue.PayoutSmall);
        soundEvents.Sort((a,b) => a.Time.CompareTo(b.Time));
        lastStep = -1; paid = false; running = true; DisplayedTotal = strike.PointsBefore;
        readout.gameObject.SetActive(true); readout.alpha = 1;
        foreach (var label in coveredLabels) label.gameObject.SetActive(false);
        Render(0);
    }
    /// <summary>Renders score motion on an unscaled clock; model values are never assigned here.</summary>
    public void Tick()
    {
        if (!running) return;
        float elapsed = (float)(Time.realtimeSinceStartupAsDouble - started);
        Render(elapsed);
        if (elapsed >= sequence.Duration) Finish();
    }
    /// <summary>Samples canonical score deliveries and rigid-card impacts on one unscaled clock, scheduling only current sound events.</summary>
    private void Render(float elapsed)
    {
        int index = sequence.StepAt(elapsed); var step = sequence.Steps[index];
        bool payout = elapsed >= sequence.PayoutAt;
        if (index != lastStep)
        {
            lastStep = index;
            if (index == 0) scoringPortrait.RestoreEncounter();
            else scoringPortrait.ShowContributors(receipt.Members.Where(m => step.SourceIds.Contains(m.InstanceId)).ToArray());
            operation.text = index == 0 ? "" : (step.Multiply ? "×" : "+") + Format(step.Amount);
            operation.color = step.Multiply ? new Color32(167, 220, 194, 255) : new Color32(240, 205, 133, 255);
        }
        // Small look-ahead uses the audio clock for arrivals; a stalled frame never sprays overdue notes.
        while (soundCursor < soundEvents.Count && soundEvents[soundCursor].Time <= elapsed + .10f)
        {
            var cue = soundEvents[soundCursor++];
            if (elapsed - cue.Time < .08f && soundToggle.isOn)
                sound.PlayFrozen(cue.Cue, cue.Clip, cue.Pitch, cue.Gain, dspStarted + cue.Time);
        }
        if (payout && !paid) { paid = true; scoringPortrait.RestoreEncounter(); }
        float payoutAge = elapsed - sequence.PayoutAt;
        float transfer = payout ? Mathf.Clamp01(payoutAge / (.27f + sequence.Intensity * .12f)) : 0;
        float eased = 1 - Mathf.Pow(1 - transfer, 3);
        DisplayedTotal = transfer >= 1 ? receipt.PointsAfter : receipt.PointsBefore + (receipt.PointsAfter - receipt.PointsBefore) * (decimal)eased;
        DisplayedStrike = payout ? receipt.Score.Points : sequence.ValueAt(elapsed);

        value.text = Format(DisplayedStrike);
        caption.text = payout ? (receipt.Score.Points == 0 ? "MISS" : "LANDING POINTS") : receipt.Score.Accuracy.ToString().ToUpperInvariant();
        if (payout) operation.text = "+" + Format(receipt.Score.Points);
        readout.alpha = payout ? 1 - Mathf.Clamp01((payoutAge - .3f) / .24f) : 1;
        feedback.SetScoreSources(payout || index == 0 ? null : step.SourceIds, 0);
        feedback.SetScoringFocus(!payout);
        value.rectTransform.localScale = Vector3.one;
        scoringPortrait.SetPulse(0); operation.rectTransform.localScale = operationRest; totalLabel.localScale = totalRest;
        machine.anchoredPosition = machineRest; glow.color = Color.clear;
        collision.Render(sequence, elapsed, ReducedMotion);
    }
    /// <summary>Freezes the authored take and gain so runtime and review exports share one exact audio schedule.</summary>
    private void AddSound(float at, DeepwaterSoundCue cue, float pitch = 1, float decibels = 0)
    {
        soundEvents.Add(new SoundEvent(at, sound.Select(cue), sound.CuePitch(cue, pitch), sound.CueGain(cue, decibels), cue));
    }
    /// <summary>Restores all authored motion targets while leaving the settled score intact.</summary>
    private void RestoreMotion() { if (collision != null) collision.ResetMotion(); if (machine != null) machine.anchoredPosition = machineRest; if (totalLabel != null) totalLabel.localScale = totalRest; if (value != null) value.rectTransform.localScale = Vector3.one; if (operation != null) operation.rectTransform.localScale = operationRest; if (scoringPortrait != null) scoringPortrait.SetPulse(0); }
    /// <summary>Cancels pending voices without affecting any other game sound.</summary>
    private void StopVoices() { if (sound != null) sound.StopAll(); }
    /// <summary>Leaves no light, transform or sound behind when a score finishes or is interrupted.</summary>
    public void Finish()
    {
        bool wasRunning = running; running = false; if (receipt != null) DisplayedTotal = receipt.PointsAfter;
        if (strikeContact != null) strikeContact.Clear();
        RestoreMotion(); if (wasRunning) StopVoices();
        if (collision != null) collision.Clear();
        if (scoringPortrait != null) scoringPortrait.EndEncounter();
        if (feedback != null) { feedback.SetScoreSources(null, 0); feedback.SetScoringFocus(false); }
        if (readout != null) readout.gameObject.SetActive(false);
        if (coveredLabels != null) foreach (var label in coveredLabels) if (label != null) label.gameObject.SetActive(true);
        if (particles != null) particles.Render(Vector3.zero, Vector3.zero, 0, 0, 0);
    }
    /// <summary>Abbreviates only the animated display; exact decimal operations remain in the inspectable breakdown.</summary>
    public static string Format(decimal number)
    {
        if (number < 1000000) return number.ToString("0.##", CultureInfo.InvariantCulture);
        decimal divisor = 1000000;
        foreach (string suffix in Suffixes) { if (number / divisor < 1000) return (number / divisor).ToString("0.##", CultureInfo.InvariantCulture) + suffix; divisor *= 1000; }
        return number.ToString("0.##E+0", CultureInfo.InvariantCulture);
    }
}
