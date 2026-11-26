using TMPro;
using UnityEngine;

/// <summary>
/// Turns the encounter creature's machine panel face-up when Landing opens, scaled by rarity. The controller owns the gate
/// (clock hold, strike rejection, deadline); this view only samples the shared start time and restores everything it touches.
/// </summary>
public sealed class LandingCreatureReveal : MonoBehaviour
{
    [SerializeField] private RectTransform creatureCard;
    [SerializeField] private CanvasGroup creatureFade;
    [SerializeField] private LandingPanelFlash creatureFlash;
    [SerializeField] private TMP_Text creatureName;
    [SerializeField] private LandingImpactCamera impactCamera;
    [SerializeField] private LandingScoreRush preferences;
    [SerializeField] private DeepwaterSoundPlayer sound;
    [SerializeField] private LandingRarityBanner banner;
    private LandingRevealTiming timing;
    private double startedAt;
    private bool running, sounded, captured, stamped;
    /// <summary>Rare/Legendary tag; it outlives the reveal briefly, so the Landing presentation keeps ticking it.</summary>
    public LandingRarityBanner Banner => banner;
    private Vector3 cardScale;
    private float faceAlpha, nameAlpha;
    public bool IsPlaying => running;
    public LandingRevealTiming Timing => timing;
    /// <summary>Message-band text for the current reveal moment; empty when idle.</summary>
    public string Caption => running ? LandingRevealTiming.Caption : "";
    private float Elapsed => (float)(Time.realtimeSinceStartupAsDouble - startedAt);
    private bool Reduced => preferences != null && preferences.ReducedMotion;

    /// <summary>Starts once per Landing occurrence from the controller's own start time, so view and gate share one clock. Silent suppresses the cue (frame exports).</summary>
    public void Begin(CardRarity rarity, double revealStartedAt, bool silent = false)
    {
        if (running) { sounded = true; stamped = true; Finish(); } // Replacement is silent and never stamps the old banner.
        cardScale = creatureCard.localScale; faceAlpha = creatureFade.alpha; nameAlpha = creatureName.alpha; captured = true;
        timing = LandingRevealTiming.For(rarity); startedAt = revealStartedAt; running = true; sounded = silent; stamped = false;
        if (banner != null) banner.Clear();
        Render();
    }

    /// <summary>Samples the reveal on the unscaled clock; hands control back to the controller's deadline at the end.</summary>
    public void Tick()
    {
        if (!running) return;
        if (Elapsed >= timing.Duration) { Finish(); return; }
        Render();
    }

    /// <summary>Applies the sampled face, name, flash and stage pose; reduced motion keeps only a stationary fade.</summary>
    private void Render()
    {
        float elapsed = Elapsed; bool reduced = Reduced;
        creatureFade.alpha = faceAlpha * timing.FaceOpacity(elapsed, reduced);
        creatureName.alpha = nameAlpha * timing.NameOpacity(elapsed);
        if (reduced)
        {
            creatureCard.localScale = cardScale; creatureFlash.Show(0); impactCamera.ResetMotion();
        }
        else
        {
            float settle = timing.SettleScale(elapsed);
            creatureCard.localScale = new Vector3(cardScale.x * settle, cardScale.y * timing.FaceScale(elapsed) * settle, cardScale.z);
            creatureFlash.Show(timing.FlashOpacity(elapsed));
            impactCamera.RenderOffset(timing.StageOffset(elapsed));
        }
        if (!sounded && elapsed >= timing.SettleAt) PlaySound();
        // The banner stamps with the face landing, on the same absolute clock so exported frames match live play.
        if (!stamped && elapsed >= timing.SettleAt) { stamped = true; if (banner != null) banner.Stamp(timing.Rarity, startedAt + timing.SettleAt); }
    }

    /// <summary>One cue per reveal, at the moment the face lands; a skipped reveal still acknowledges the creature once.</summary>
    private void PlaySound()
    {
        sounded = true;
        if (sound != null) sound.Play(DeepwaterSoundCue.Reveal, timing.Pitch, timing.GainDb);
    }

    /// <summary>Restores the panel, name, flash and stage exactly on completion, skip, replacement, disable or phase exit.</summary>
    public void Finish()
    {
        bool wasRunning = running; running = false;
        if (wasRunning && !sounded) PlaySound();
        // Skipping still announces a Rare/Legendary creature: the banner stamps now instead of at the face landing.
        if (wasRunning && !stamped) { stamped = true; if (banner != null) banner.Stamp(timing.Rarity, Time.realtimeSinceStartupAsDouble); }
        if (!captured) return;
        creatureCard.localScale = cardScale; creatureFade.alpha = faceAlpha; creatureName.alpha = nameAlpha;
        if (creatureFlash != null) creatureFlash.Show(0);
        if (impactCamera != null) impactCamera.Clear();
        captured = false;
    }

    /// <summary>Never leaves a hidden or squashed panel behind when the Landing phase disappears.</summary>
    private void OnDisable() { running = false; sounded = true; Finish(); }
}
