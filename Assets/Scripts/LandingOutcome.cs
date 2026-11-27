using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Closes a Landing challenge after its score feedback. Landed: the creature card is yanked up and slams home under a LANDED
/// stamp before Build opens. Escaped: the creature thrashes and sinks out of its frame under an ESCAPED stamp, then each creature
/// in the already paid haul cashes out into a HAUL PAID readout and the rig column clears. The controller owns the gate (deadline,
/// skip, what waits); this view only samples the shared start time and restores everything it touches.
/// </summary>
public sealed class LandingOutcome : MonoBehaviour
{
    [SerializeField] private RectTransform creatureCard;
    [SerializeField] private CanvasGroup creatureFade;
    [SerializeField] private LandingPanelFlash creatureFlash;
    [SerializeField] private LandingImpactCamera impactCamera;
    [SerializeField] private LandingScoreRush preferences;
    [SerializeField] private DeepwaterSoundPlayer sound;
    [SerializeField] private LandingRarityBanner banner;
    [SerializeField] private CanvasGroup payout;
    [SerializeField] private TMP_Text payoutCaption, payoutValue;
    /// <summary>Layers that sit behind the creature card and must stay hidden while an escaped creature's card is gone.</summary>
    [SerializeField] private CanvasGroup[] behindCard = System.Array.Empty<CanvasGroup>();
    private readonly List<TackleRigCardView> cards = new List<TackleRigCardView>();
    private readonly List<CanvasGroup> cardAlpha = new List<CanvasGroup>();
    private readonly List<Vector3> cardScale = new List<Vector3>();
    private int[] values = System.Array.Empty<int>();
    private int total, lastActive = -1;
    private LandingOutcomeTiming timing;
    private UnityEngine.UI.ScrollRect scroll;
    private double startedAt;
    private string creatureName = "";
    private bool running, captured, stamped, landSounded, yankSounded, punched;
    private Vector2 cardPosition;
    private Quaternion cardRotation;
    private Vector3 cardRestScale, payoutScale;
    private float faceAlpha;
    private double lastTick = -1;
    public bool IsPlaying => running;
    public LandingOutcomeTiming Timing => timing;
    /// <summary>Message-band text while the beat plays.</summary>
    public string Caption => running ? LandingOutcomeTiming.Caption : "";
    /// <summary>Header text for the current moment: names the creature, then states that the expedition is over.</summary>
    public string Header => timing.Kind == LandingOutcomeKind.Landed ? (running && Elapsed < LandingOutcomeTiming.LandAt ? creatureName : creatureName + " LANDED")
        : timing.ShowsExpeditionOver(Elapsed) || !running ? "EXPEDITION OVER" : creatureName + " ESCAPED";
    public int DisplayedPayout { get; private set; }
    private float Elapsed => (float)(Time.realtimeSinceStartupAsDouble - startedAt);
    private bool Reduced => preferences != null && preferences.ReducedMotion;

    /// <summary>
    /// Starts once per ended challenge from the controller's own start time. Escaped receives the rig column's bound haul cards
    /// and their displayed values; the readout always ends on the authoritative paid total.
    /// </summary>
    public void Begin(LandingOutcomeTiming schedule, double outcomeStartedAt, string displayName, IReadOnlyList<TackleRigCardView> haulCards,
        int[] haulValues, int paidTotal, UnityEngine.UI.ScrollRect rigScroll, bool silent = false)
    {
        Clear();
        timing = schedule; startedAt = outcomeStartedAt; creatureName = (displayName ?? "").ToUpperInvariant();
        cardPosition = creatureCard.anchoredPosition; cardRotation = creatureCard.localRotation; cardRestScale = creatureCard.localScale;
        faceAlpha = creatureFade.alpha; payoutScale = payoutValue.rectTransform.localScale; captured = true;
        values = haulValues ?? System.Array.Empty<int>(); total = paidTotal; scroll = rigScroll; lastActive = -1; lastTick = -1;
        cards.Clear(); cardAlpha.Clear(); cardScale.Clear();
        if (haulCards != null)
            for (int i = 0; i < haulCards.Count && i < timing.Cards; i++)
            {
                var card = haulCards[i]; cards.Add(card); cardScale.Add(card.transform.localScale);
                var group = card.GetComponent<CanvasGroup>(); if (group == null) group = card.gameObject.AddComponent<CanvasGroup>();
                cardAlpha.Add(group);
            }
        stamped = false; punched = false; landSounded = yankSounded = silent; running = true;
        payoutCaption.text = "HAUL PAID"; DisplayedPayout = 0; payoutValue.text = "0";
        if (timing.Kind == LandingOutcomeKind.Escaped) Conceal(true);
        if (banner != null) banner.Clear();
        Render(0);
    }

    /// <summary>Samples the beat on the unscaled clock; holds the final pose once the schedule ends until the gate releases it.</summary>
    public void Tick()
    {
        if (!running) return;
#if UNITY_EDITOR
        if (frozen) return;
#endif
        float age = Elapsed;
        if (age >= timing.Duration) { Finish(); return; }
        Render(age);
    }

    /// <summary>Applies the sampled card, stamp, stage, payout and cash-out poses; reduced motion keeps fades and numbers only.</summary>
    private void Render(float age)
    {
        bool reduced = Reduced;
        float lift = timing.Lift(age), sway = timing.Sway(age);
        Vector2 scale = timing.Scale(age);
        creatureFade.alpha = faceAlpha * timing.FaceOpacity(age, reduced);
        if (reduced)
        {
            creatureCard.anchoredPosition = cardPosition; creatureCard.localRotation = cardRotation; creatureCard.localScale = cardRestScale;
            if (creatureFlash != null) creatureFlash.Show(0);
            if (impactCamera != null) impactCamera.ResetMotion();
        }
        else
        {
            creatureCard.anchoredPosition = cardPosition + new Vector2(sway, lift);
            creatureCard.localRotation = cardRotation * Quaternion.Euler(0, 0, timing.Rotation(age));
            creatureCard.localScale = new Vector3(cardRestScale.x * scale.x, cardRestScale.y * scale.y, cardRestScale.z);
            if (creatureFlash != null) creatureFlash.Show(timing.Flash(age));
            if (impactCamera != null) impactCamera.RenderOffset(timing.StageOffset(age));
        }
        if (timing.Kind == LandingOutcomeKind.Landed)
        {
            if (!yankSounded && age >= LandingOutcomeTiming.DipEnd) { yankSounded = true; Play(DeepwaterSoundCue.Travel, 1.1f, -4); }
            if (!landSounded && age >= LandingOutcomeTiming.LandAt) { landSounded = true; Play(DeepwaterSoundCue.Attach, 1, 0); }
        }
        if (!stamped && age >= timing.StampAt) { stamped = true; if (banner != null) banner.StampOutcome(timing.Kind, startedAt + timing.StampAt, timing.StampHold); }
        if (banner != null) banner.Tick();
        if (timing.Kind == LandingOutcomeKind.Escaped) RenderPayout(age, reduced);
    }

    /// <summary>Cash-out: each card pops and dims as its value rolls into HAUL PAID; the total punches, then the rig column clears.</summary>
    private void RenderPayout(float age, bool reduced)
    {
        payout.alpha = timing.PayoutOpacity(age);
        DisplayedPayout = timing.Payout(age, values, total);
        payoutValue.text = DisplayedPayout.ToString();
        payoutValue.rectTransform.localScale = payoutScale * (reduced ? 1 : timing.PayoutPunch(age));
        for (int i = 0; i < cards.Count; i++)
        {
            cardAlpha[i].alpha = timing.CardOpacity(i, age);
            cards[i].transform.localScale = cardScale[i] * (reduced ? 1 : timing.CardPop(i, age));
            cards[i].SetContributing(timing.ActiveCard(age) == i);
        }
        int active = timing.ActiveCard(age);
        if (active >= 0 && active != lastActive)
        {
            lastActive = active; Reveal(active);
            // Fast tallies tick at most every 50 ms so a long haul rattles rather than buzzes.
            if (Time.realtimeSinceStartupAsDouble - lastTick >= .05) { lastTick = Time.realtimeSinceStartupAsDouble; Play(DeepwaterSoundCue.SlotTick, Mathf.Min(1.5f, 1 + active * .04f), -2); }
        }
        if (!punched && age >= timing.PayoutEnd && timing.Cards > 0) { punched = true; Play(DeepwaterSoundCue.PayoutSmall, .94f, -5); }
    }

    /// <summary>Keeps the card being cashed out inside the scrolling rig column.</summary>
    private void Reveal(int index)
    {
        if (scroll == null || index >= cards.Count) return;
        var content = scroll.content; var viewport = scroll.viewport != null ? scroll.viewport : (RectTransform)scroll.transform;
        float overflow = content.rect.height - viewport.rect.height;
        if (overflow <= 1) return;
        var rect = (RectTransform)cards[index].transform;
        float top = -rect.anchoredPosition.y - (viewport.rect.height - rect.rect.height) * .5f;
        scroll.verticalNormalizedPosition = 1 - Mathf.Clamp01(top / overflow);
    }

    private void Play(DeepwaterSoundCue cue, float pitch, float gainDb) { if (sound != null) sound.Play(cue, pitch, gainDb); }

    private void Conceal(bool hidden) { foreach (var layer in behindCard) if (layer != null) layer.alpha = hidden ? 0 : 1; }

    /// <summary>
    /// Jumps to the final pose (skip or schedule end): Landed rests stamped in its frame; Escaped leaves the frame empty with the
    /// full payout shown and the rig column cleared. Every transform stays restorable through Clear.
    /// </summary>
    public void Finish()
    {
        if (!running) return;
        running = false;
#if UNITY_EDITOR
        frozen = false;
#endif
        if (!stamped) { stamped = true; if (banner != null) banner.StampOutcome(timing.Kind, Time.realtimeSinceStartupAsDouble - LandingRarityBanner.StampSeconds, timing.StampHold); }
        if (!landSounded && timing.Kind == LandingOutcomeKind.Landed) { landSounded = true; Play(DeepwaterSoundCue.Attach, 1, 0); }
        creatureCard.anchoredPosition = cardPosition; creatureCard.localRotation = cardRotation; creatureCard.localScale = cardRestScale;
        if (creatureFlash != null) creatureFlash.Show(0);
        if (impactCamera != null) impactCamera.Clear();
        if (timing.Kind == LandingOutcomeKind.Landed) { creatureFade.alpha = faceAlpha; return; }
        creatureFade.alpha = 0;
        if (banner != null) banner.Clear();
        payout.alpha = 1; DisplayedPayout = total; payoutValue.text = total.ToString(); payoutValue.rectTransform.localScale = payoutScale;
        for (int i = 0; i < cards.Count; i++) { cardAlpha[i].alpha = 0; cards[i].transform.localScale = cardScale[i]; cards[i].SetContributing(false); }
    }

    /// <summary>Restores the panel, stamp, payout readout and every cashed-out card exactly: new encounter, expedition or phase exit.</summary>
    public void Clear()
    {
        running = false;
        Conceal(false);
#if UNITY_EDITOR
        frozen = false;
#endif
        if (banner != null) banner.Clear();
        if (payout != null) payout.alpha = 0;
        if (captured)
        {
            creatureCard.anchoredPosition = cardPosition; creatureCard.localRotation = cardRotation; creatureCard.localScale = cardRestScale;
            creatureFade.alpha = faceAlpha; payoutValue.rectTransform.localScale = payoutScale;
            if (creatureFlash != null) creatureFlash.Show(0);
            if (impactCamera != null) impactCamera.Clear();
            captured = false;
        }
        for (int i = 0; i < cards.Count; i++)
            if (cards[i] != null) { cardAlpha[i].alpha = 1; cards[i].transform.localScale = cardScale[i]; cards[i].SetContributing(false); }
        cards.Clear(); cardAlpha.Clear(); cardScale.Clear();
    }

#if UNITY_EDITOR
    /// <summary>Editor review only: re-bases the shared clock so a slow capture samples an exact age.</summary>
    private bool frozen;
    /// <summary>Editor review only: holds the beat at an exact age until Release, so slow captures sample the intended frame.</summary>
    public void Seek(float age)
    {
        if (!running) return;
        frozen = true; startedAt = Time.realtimeSinceStartupAsDouble - age;
        if (stamped && banner != null) banner.Rebase(startedAt + timing.StampAt);
        Render(age);
    }
    /// <summary>Editor review only: resumes normal sampling.</summary>
    public void Release() { frozen = false; }
#endif

    /// <summary>Never leaves a hidden, moved or dimmed panel behind when the Landing phase disappears.</summary>
    private void OnDisable() { Clear(); }
}
