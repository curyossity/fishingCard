using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>Stages a frozen strike into a growing readout, source kicks, musical steps and a progress payout.</summary>
public sealed class LandingScoreRush : MonoBehaviour
{
    [SerializeField] private DeepwaterSoundPlayer sound;
    [SerializeField] private CanvasGroup readout;
    [SerializeField] private TMP_Text value, operation, caption;
    [SerializeField] private TMP_Text[] coveredLabels;
    [SerializeField] private UnityEngine.UI.Image glow;
    [SerializeField] private LandingScoringPortrait scoringPortrait;
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
    private IReadOnlyList<TackleRigCardView> cards = Array.Empty<TackleRigCardView>();
    private double started, dspStarted;
    private int lastStep = -1, lastImpact = -1;
    private bool paid, running;
    private Vector2 machineRest;
    private Vector3 totalRest;
    private TackleRigCardView kicked;
    private Vector3 kickScale;
    private Quaternion kickRotation;
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
        machineRest = machine.anchoredPosition; totalRest = totalLabel.localScale;
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
        Finish(); receipt = strike; cards = rigCards ?? Array.Empty<TackleRigCardView>();
        if (strike?.Score == null || !animate) return;
        sound.StopAll();
        scoringPortrait.CaptureEncounter();
        sequence = new LandingScoreSequence(strike.Score); started = Time.realtimeSinceStartupAsDouble; dspStarted = AudioSettings.dspTime;
        if (strikeContact != null) strikeContact.Begin(strike, sequence.Steps[0].ImpactAt);
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
        lastStep = lastImpact = -1; paid = false; running = true; DisplayedTotal = strike.PointsBefore;
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
    /// <summary>Draws one presentation frame and emits at most one note for newly reached operations.</summary>
    private void Render(float elapsed)
    {
        int index = sequence.StepAt(elapsed); var step = sequence.Steps[index];
        float age = Mathf.Max(0, elapsed - step.Time);
        bool payout = elapsed >= sequence.PayoutAt;
        if (index != lastStep)
        {
            RestoreKick(); lastStep = index;
            if (index == 0) scoringPortrait.RestoreEncounter();
            else scoringPortrait.ShowContributors(receipt.Members.Where(m => step.SourceIds.Contains(m.InstanceId)).ToArray());
            operation.text = index == 0 ? "" : (step.Multiply ? "×" : "+") + Format(step.Amount);
            operation.color = step.Multiply ? new Color32(167, 220, 194, 255) : new Color32(240, 205, 133, 255);
            kicked = cards.FirstOrDefault(c => c.BoundInstance?.InstanceId == step.SourceId);
            if (kicked != null)
            {
                kickScale = kicked.transform.localScale; kickRotation = kicked.transform.localRotation;
            }
        }
        // Small look-ahead uses the audio clock for arrivals; a stalled frame never sprays overdue notes.
        while (soundCursor < soundEvents.Count && soundEvents[soundCursor].Time <= elapsed + .10f)
        {
            var cue = soundEvents[soundCursor++];
            if (elapsed - cue.Time < .08f && soundToggle.isOn)
                sound.PlayFrozen(cue.Cue, cue.Clip, cue.Pitch, cue.Gain, dspStarted + cue.Time);
        }
        int arrived = sequence.ImpactStepAt(elapsed);
        if (elapsed >= sequence.Steps[arrived].ImpactAt && arrived != lastImpact)
        {
            lastImpact = arrived;
        }
        if (payout && !paid) { paid = true; RestoreKick(); scoringPortrait.RestoreEncounter(); }
        float payoutAge = elapsed - sequence.PayoutAt;
        float transfer = payout ? Mathf.Clamp01(payoutAge / (.27f + sequence.Intensity * .12f)) : 0;
        float eased = 1 - Mathf.Pow(1 - transfer, 3);
        DisplayedTotal = transfer >= 1 ? receipt.PointsAfter : receipt.PointsBefore + (receipt.PointsAfter - receipt.PointsBefore) * (decimal)eased;
        DisplayedStrike = payout ? receipt.Score.Points : sequence.ValueAt(elapsed);
        if (strikeContact != null) strikeContact.Render(elapsed, ReducedMotion, DisplayedStrike);
        value.text = Format(DisplayedStrike);
        caption.text = payout ? (receipt.Score.Points == 0 ? "MISS" : "LANDING POINTS") : receipt.Score.Accuracy.ToString().ToUpperInvariant();
        if (payout) operation.text = "+" + Format(receipt.Score.Points);
        readout.alpha = payout ? 1 - Mathf.Clamp01((payoutAge - .3f) / .24f) : 1;
        float impactAge = elapsed - sequence.Steps[arrived].ImpactAt;
        float bump = impactAge < 0 ? 0 : Mathf.Exp(-impactAge * 18) * Mathf.Sin(impactAge * 34);
        float preparation = Mathf.Sin(Mathf.Clamp01(age / Math.Max(.001f, step.ImpactAt - step.Time)) * Mathf.PI);
        feedback.SetScoreSources(payout || index == 0 ? null : step.SourceIds, preparation * .5f);
        feedback.SetScoringFocus(!payout);
        float impact = payout ? Mathf.Exp(-Mathf.Max(0, payoutAge) * 12) * Mathf.Sin(payoutAge * 40) : 0;
        float punch = ReducedMotion ? 0 : bump * (sequence.Steps[arrived].Multiply ? .13f : .065f) + impact * sequence.Intensity * .10f;
        value.rectTransform.localScale = new Vector3(1 + punch, 1 + punch * .65f, 1);
        scoringPortrait.SetPulse(ReducedMotion ? 0 : preparation);
        totalLabel.localScale = totalRest * (1 + (ReducedMotion ? 0 : impact * .12f));
        machine.anchoredPosition = machineRest + (strikeContact != null ? strikeContact.RecoilOffset : Vector2.zero)
            + (ReducedMotion || sequence.Tier == 0 ? Vector2.zero : Vector2.up * impact * sequence.Intensity * 4);
        glow.color = new Color(1, .69f, .26f, ReducedMotion ? .015f : .015f + Mathf.Max(0, bump) * .035f + Mathf.Abs(impact) * .04f);
        if (kicked != null && !payout && !ReducedMotion)
        {
            kicked.transform.localScale = kickScale * (1 + preparation * .025f);
        }
        Vector3 destination = value.rectTransform.TransformPoint(value.rectTransform.rect.center);
        if (ReducedMotion) RestoreMotion();
        Vector3 origin = scoringPortrait.ProjectileOrigin;
        float travel = Mathf.Clamp01((elapsed - step.LaunchAt) / Math.Max(.001f, step.ImpactAt - step.LaunchAt));
        particles.Render(origin, destination, ReducedMotion || index == 0 || payout ? 0 : travel,
            ReducedMotion || !payout || sequence.Tier == 0 ? 0 : Mathf.Clamp01(payoutAge / .55f), sequence.Intensity,
            step.Multiply, ReducedMotion || impactAge < 0 || payout ? 0 : Mathf.Clamp01(impactAge / .18f));
    }
    /// <summary>Freezes the authored take and gain so runtime and review exports share one exact audio schedule.</summary>
    private void AddSound(float at, DeepwaterSoundCue cue, float pitch = 1, float decibels = 0)
    {
        soundEvents.Add(new SoundEvent(at, sound.Select(cue), sound.CuePitch(cue, pitch), sound.CueGain(cue, decibels), cue));
    }
    /// <summary>Restores a kicked card before moving to another source or leaving the phase.</summary>
    private void RestoreKick() { if (kicked != null) { kicked.transform.localScale = kickScale; kicked.transform.localRotation = kickRotation; } kicked = null; }
    /// <summary>Restores all authored motion targets while leaving the settled score intact.</summary>
    private void RestoreMotion() { RestoreKick(); if (machine != null) machine.anchoredPosition = machineRest; if (totalLabel != null) totalLabel.localScale = totalRest; if (value != null) value.rectTransform.localScale = Vector3.one; if (scoringPortrait != null) scoringPortrait.SetPulse(0); }
    /// <summary>Cancels pending voices without affecting any other game sound.</summary>
    private void StopVoices() { if (sound != null) sound.StopAll(); }
    /// <summary>Leaves no light, transform or sound behind when a score finishes or is interrupted.</summary>
    public void Finish()
    {
        bool wasRunning = running; running = false; if (receipt != null) DisplayedTotal = receipt.PointsAfter;
        if (strikeContact != null) strikeContact.Clear();
        RestoreMotion(); if (wasRunning) StopVoices();
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
