using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>Stages a frozen strike into a growing readout, source kicks, musical steps and a progress payout.</summary>
public sealed class LandingScoreRush : MonoBehaviour
{
    [SerializeField] private CanvasGroup readout;
    [SerializeField] private TMP_Text value, operation, sourceName, caption;
    [SerializeField] private TMP_Text[] coveredLabels;
    [SerializeField] private UnityEngine.UI.Image sourcePortrait, glow;
    [SerializeField] private RectTransform machine, totalLabel;
    [SerializeField] private LandingRushGraphic particles;
    [SerializeField] private LandingStrikeFeedback feedback;
    [SerializeField] private UnityEngine.UI.Image progressLight;
    [SerializeField] private UnityEngine.UI.Image[] marquee = Array.Empty<UnityEngine.UI.Image>();
    [SerializeField] private AudioSource[] voices;
    [SerializeField] private AudioClip hit, addition, multiplier, payoutSmall, payoutBig, miss;
    [SerializeField] private UnityEngine.UI.Toggle soundToggle, reducedMotionToggle;
    [SerializeField, Range(0, 1)] private float soundVolume = .55f;
    private const string SoundKey = "Deepwater.Landing.Sound", MotionKey = "Deepwater.Landing.ReducedMotion";
    private LandingStrikeReceipt receipt;
    private LandingScoreSequence sequence;
    private IReadOnlyList<TackleRigCardView> cards = Array.Empty<TackleRigCardView>();
    private double started;
    private int lastStep = -1, lastImpact = -1, voice;
    private bool paid, running;
    private Vector2 machineRest;
    private Vector3 totalRest;
    private TackleRigCardView kicked;
    private Vector3 kickScale;
    private Quaternion kickRotation;
    private Vector3 portraitScale;
    private Vector2 kickPosition;
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
        soundToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(SoundKey, 1) != 0);
        reducedMotionToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(MotionKey, 0) != 0);
        soundToggle.onValueChanged.AddListener(SetSound); reducedMotionToggle.onValueChanged.AddListener(SetReducedMotion);
    }
    /// <summary>Persists a deliberate sound choice, stopping tails immediately when muted.</summary>
    private void SetSound(bool active) { PlayerPrefs.SetInt(SoundKey, active ? 1 : 0); PlayerPrefs.Save(); if (!active) StopVoices(); }
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
        sequence = new LandingScoreSequence(strike.Score); started = Time.realtimeSinceStartupAsDouble;
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
            var member = receipt.Members.FirstOrDefault(m => m.InstanceId == step.SourceId);
            sourceName.text = member?.Definition.DisplayName.ToUpperInvariant() ?? "ACCURACY BASE";
            sourcePortrait.sprite = member?.Definition.Artwork; sourcePortrait.enabled = sourcePortrait.sprite != null;
            operation.text = index == 0 ? Format(step.Amount) : (step.Multiply ? "×" : "+") + Format(step.Amount);
            operation.color = step.Multiply ? new Color32(166, 240, 223, 255) : new Color32(255, 216, 133, 255);
            feedback.SetScoreSupport(step.SourceId, step.SupportingIds);
            kicked = cards.FirstOrDefault(c => c.BoundInstance?.InstanceId == step.SourceId);
            if (kicked != null)
            {
                kickScale = kicked.transform.localScale; kickRotation = kicked.transform.localRotation;
                kickPosition = ((RectTransform)kicked.transform).anchoredPosition;
                portraitScale = kicked.PortraitTransform.localScale;
            }
        }
        int arrived = sequence.ImpactStepAt(elapsed);
        if (elapsed >= sequence.Steps[arrived].ImpactAt && arrived != lastImpact)
        {
            lastImpact = arrived; var impactStep = sequence.Steps[arrived];
            Play(arrived == 0 ? (receipt.Score.Points == 0 ? miss : hit) : impactStep.Multiply ? multiplier : addition,
                Mathf.Pow(2, Math.Min(arrived, 9) / 12f), arrived == 0 ? .6f : .6f + sequence.Intensity * .12f);
        }
        feedback.SetSequenceProgress(index - 1, arrived - 1, payout);
        if (payout && !paid) { paid = true; RestoreKick(); Play(receipt.Score.Points == 0 ? miss : sequence.Intensity > .45f ? payoutBig : payoutSmall, 1, .85f); }
        float payoutAge = elapsed - sequence.PayoutAt;
        float transfer = payout ? Mathf.Clamp01(payoutAge / .45f) : 0;
        float eased = 1 - Mathf.Pow(1 - transfer, 3);
        DisplayedTotal = transfer >= 1 ? receipt.PointsAfter : receipt.PointsBefore + (receipt.PointsAfter - receipt.PointsBefore) * (decimal)eased;
        DisplayedStrike = payout ? receipt.Score.Points : sequence.ValueAt(elapsed);
        value.text = Format(DisplayedStrike);
        caption.text = payout ? (receipt.Score.Points == 0 ? "MISS" : "LANDING POINTS") : receipt.Score.Accuracy.ToString().ToUpperInvariant();
        if (payout) { operation.text = "+" + Format(receipt.Score.Points); sourceName.text = receipt.ProtectedMiss ? "MISS PROTECTED" : "STRIKE TOTAL"; sourcePortrait.enabled = false; }
        readout.alpha = payout ? 1 - Mathf.Clamp01((payoutAge - .3f) / .24f) : 1;
        float hitAge = elapsed - sequence.Steps[arrived].ImpactAt;
        float bump = hitAge < 0 ? 0 : Mathf.Exp(-hitAge * 16) * Mathf.Sin(hitAge * 32);
        float windup = index == 0 ? 0 : Mathf.Sin(Mathf.Clamp01(age / Mathf.Max(.001f, step.ImpactAt - step.Time)) * Mathf.PI);
        feedback.SetScoreActivation(payout || index == 0 ? -1 : step.SourceId, windup);
        float impact = payout ? Mathf.Exp(-Mathf.Max(0, payoutAge) * 12) * Mathf.Sin(payoutAge * 40) : 0;
        float stretch = ReducedMotion ? 0 : bump * (step.Multiply ? .20f : .11f);
        value.rectTransform.localScale = new Vector3(1 + stretch, 1 + stretch * (step.Multiply ? -.35f : 1), 1);
        sourcePortrait.rectTransform.localScale = Vector3.one * (1 + (ReducedMotion ? 0 : windup * .13f));
        totalLabel.localScale = totalRest * (1 + (ReducedMotion ? 0 : impact * .12f));
        machine.anchoredPosition = machineRest + (ReducedMotion ? Vector2.zero : new Vector2(Mathf.Sin(payoutAge * 43), Mathf.Sin(payoutAge * 31)) * impact * sequence.Intensity * 7);
        glow.color = new Color(1, .69f, .26f, ReducedMotion ? .015f : .015f + Mathf.Max(0, bump) * .035f + Mathf.Abs(impact) * .04f);
        if (kicked != null && !payout && !ReducedMotion)
        {
            kicked.transform.localScale = kickScale * (1 + windup * .025f);
            kicked.transform.localRotation = kickRotation * Quaternion.Euler(0, 0, windup * -1.5f);
            ((RectTransform)kicked.transform).anchoredPosition = kickPosition + Vector2.up * windup * 6;
            kicked.PortraitTransform.localScale = portraitScale * (1 + windup * .10f);
        }
        else if (ReducedMotion) RestoreMotion();
        Vector3 destination = value.rectTransform.TransformPoint(value.rectTransform.rect.center);
        Vector3 origin = sourcePortrait.rectTransform.TransformPoint(sourcePortrait.rectTransform.rect.center);
        float travel = Mathf.Clamp01((elapsed - step.LaunchAt) / Mathf.Max(.001f, step.ImpactAt - step.LaunchAt));
        float ripple = hitAge < 0 || arrived == 0 ? 0 : Mathf.Clamp01(hitAge / .25f);
        particles.Render(origin, destination, ReducedMotion || index == 0 || payout ? 0 : travel,
            ReducedMotion || !payout || receipt.Score.Points == 0 ? 0 : Mathf.Clamp01(payoutAge / .65f), sequence.Intensity,
            ReducedMotion || payout ? 0 : ripple, sequence.Steps[arrived].Multiply, step.Multiply);
        for (int i = 0; i < marquee.Length; i++)
        {
            bool lit = receipt.Score.Points > 0 && i <= Mathf.FloorToInt((marquee.Length - 1) * sequence.Intensity * (arrived + 1) / sequence.Steps.Count);
            marquee[i].enabled = lit;
            marquee[i].color = new Color(1, .85f, .5f, payout ? (1 - transfer) * .9f : ReducedMotion ? .55f : .5f + Mathf.Max(0, bump) * .45f);
        }
        if (progressLight != null) progressLight.color = payout && receipt.Score.Points > 0 ? Color.Lerp(Color.white, new Color(.65f, 1, .88f), Mathf.Max(0, impact)) : Color.white;
    }
    /// <summary>Uses pooled 2D voices routed to the authored feedback mixer, with bounded pitch and gain.</summary>
    private void Play(AudioClip clip, float pitch, float gain)
    {
        if (!soundToggle.isOn || clip == null || voices.Length == 0) return;
        var source = voices[voice++ % voices.Length]; source.Stop(); source.pitch = pitch; source.volume = soundVolume * gain;
        source.clip = clip; source.Play();
    }
    /// <summary>Restores a kicked card before moving to another source or leaving the phase.</summary>
    private void RestoreKick()
    {
        if (kicked != null) { kicked.transform.localScale = kickScale; kicked.transform.localRotation = kickRotation; ((RectTransform)kicked.transform).anchoredPosition = kickPosition; kicked.PortraitTransform.localScale = portraitScale; }
        kicked = null;
    }
    /// <summary>Restores all authored motion targets while leaving the settled score intact.</summary>
    private void RestoreMotion() { RestoreKick(); if (machine != null) machine.anchoredPosition = machineRest; if (totalLabel != null) totalLabel.localScale = totalRest; if (value != null) value.rectTransform.localScale = Vector3.one; if (sourcePortrait != null) sourcePortrait.rectTransform.localScale = Vector3.one; }
    /// <summary>Cancels pending voices without affecting any other game sound.</summary>
    private void StopVoices() { if (voices != null) foreach (var source in voices) if (source != null) source.Stop(); }
    /// <summary>Leaves no light, transform or sound behind when a score finishes or is interrupted.</summary>
    public void Finish()
    {
        running = false; if (receipt != null) DisplayedTotal = receipt.PointsAfter;
        RestoreMotion(); StopVoices();
        if (feedback != null) feedback.SetScoreActivation(-1, 0);
        if (feedback != null) { feedback.SetScoreSupport(-1, null); feedback.SetSequenceProgress(int.MaxValue, int.MaxValue, true); }
        if (progressLight != null) progressLight.color = Color.white;
        foreach (var lamp in marquee) if (lamp != null) lamp.enabled = false;
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
