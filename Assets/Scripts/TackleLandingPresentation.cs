using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>Renders live Landing indicators, the current rig and attributed scoring inside the integrated housing without resolving gameplay.</summary>
public sealed class TackleLandingPresentation : MonoBehaviour
{
    [SerializeField] private DeepwaterSoundPlayer sound;
    [SerializeField] private TMP_Text phase, haul, creature, tags, points, resistance, attempts, movement, message, grade, breakdown, emptyRig;
    [SerializeField] private UnityEngine.UI.Image portrait, fishMarker;
    [SerializeField] private RectTransform track, fish, goodZone, perfectZone, progressFill, lampContent, rigContent;
    [SerializeField] private UnityEngine.UI.Image lampTemplate;
    [SerializeField] private Sprite lampOn, lampOff;
    [SerializeField] private TackleRigCardView rigCardPrefab;
    [SerializeField] private UnityEngine.UI.ScrollRect rigScroll, scoreScroll;
    [SerializeField] private TackleButtonFeedback[] buttons;
    [SerializeField] private LandingStrikeFeedback strikeFeedback;
    [SerializeField] private LandingScoreRush scoreRush;
    [SerializeField] private LandingCreatureReveal reveal;
    [SerializeField] private LandingOutcome outcome;
    [SerializeField] private LandingStrikePanel strikePanel;
    private readonly List<TackleRigCardView> cards = new List<TackleRigCardView>();
    private readonly List<UnityEngine.UI.Image> lamps = new List<UnityEngine.UI.Image>();
    private long occurrence = -1, revision = -1, outcomeOccurrence = -1;
    private bool failed;
    private bool failureSounded;
    private bool feedbackPending, revealPending, outcomePending;
    private CardDefinition selectedCreature;
    private string biomeName;
    public bool HasRushPresentation => scoreRush != null && scoreRush.enabled;
    public bool HasRevealPresentation => reveal != null && reveal.enabled;
    public bool HasOutcomePresentation => outcome != null && outcome.enabled;
    public LandingOutcome Outcome => outcome;

    /// <summary>Resets only visual identities so future encounters cannot reuse stale labels or highlights.</summary>
    private void OnDisable() { occurrence = revision = outcomeOccurrence = -1; selectedCreature = null; }

    /// <summary>Projects normalized model coordinates into the supplied track and refreshes labels once per accepted strike.</summary>
    public void Present(FishingRunController controller)
    {
        var model = controller.Landing;
        bool fresh = occurrence != model.Occurrence;
        bool failure = controller.LandingFailureVisible;
        if (fresh)
        {
            selectedCreature = controller.CurrentEncounter;
            biomeName = controller.CurrentBiome?.GetDepthTier(controller.CurrentDepth)?.DisplayName ?? controller.CurrentBiome?.DisplayName ?? "Waters";
            occurrence = model.Occurrence; revision = -1; failed = false; failureSounded = false;
            PrepareLamps(model.AttemptsRemaining);
            BindCreature();
            if (strikeFeedback != null) strikeFeedback.BindAssistance(model, controller.CatchChain);
            rigScroll.verticalNormalizedPosition = 1;
            if (outcome != null) outcome.Clear();
            if (reveal != null && reveal.Banner != null) reveal.Banner.Clear();
            if (!failure && controller.LandingRevealPending && HasRevealPresentation)
                reveal.Begin(selectedCreature != null ? selectedCreature.Rarity : CardRarity.Common, controller.LandingRevealStartedAt);
            else if (!failure && sound != null) sound.Play(DeepwaterSoundCue.Reveal);
        }
        if (reveal != null && reveal.IsPlaying) { if (controller.LandingRevealPending) reveal.Tick(); else reveal.Finish(); }
        // The banner may linger briefly into play, but a strike's scoring motion or an ended expedition takes the card back.
        if (reveal != null && reveal.Banner != null && reveal.Banner.IsShowing)
        { if (controller.LandingFeedbackPending || failure) reveal.Banner.Clear(); else reveal.Banner.Tick(); }
        Position(fish, model.FishPosition, model.FishWidth);
        Position(goodZone, model.ZonePosition, model.GoodWidth);
        Position(perfectZone, model.ZonePosition, model.PerfectWidth);
        fishMarker.color = model.IsTelegraphingBurst ? new Color32(255, 169, 76, 255) : Color.white;
        movement.text = model.IsTelegraphingBurst ? "BURST SOON" : model.Pattern.ToString().ToUpperInvariant();
        if (revision != model.Revision || failed != failure || feedbackPending != controller.LandingFeedbackPending || revealPending != controller.LandingRevealPending
            || outcomePending != controller.LandingOutcomePending)
        {
            revision = model.Revision; failed = failure; feedbackPending = controller.LandingFeedbackPending; revealPending = controller.LandingRevealPending;
            outcomePending = controller.LandingOutcomePending;
            phase.text = biomeName.ToUpperInvariant() + " / LAND";
            // The failing strike's feedback still names the creature; the outcome beat then tells the player it escaped.
            creature.text = failure && !feedbackPending ? "EXPEDITION OVER" : (selectedCreature?.DisplayName ?? "Landing").ToUpperInvariant();
            haul.text = "HAUL " + (failure ? controller.LastHaulValue.ToString() : controller.UsesLandingChallenge ? controller.Rig.HaulValue.ToString() : VisibleHaul(controller.CatchChain));
            points.text = LandingScoreRush.Format(model.Points); resistance.text = LandingScoreRush.Format(model.Resistance);
            if (model.AttemptsRemaining > lamps.Count) PrepareLamps(model.AttemptsRemaining);
            attempts.text = model.AttemptsRemaining + " ATTEMPTS LEFT";
            for (int i = 0; i < lamps.Count; i++) lamps[i].sprite = i < model.AttemptsRemaining ? lampOn : lampOff;
            message.text = model.Error ?? (failure ? "Full haul paid: " + controller.LastHaulValue + ". Rig and upgrades cleared."
                : controller.AwaitingRigPlacement ? "LANDED! Ready for rig placement."
                : "Strike when the fish marker and zone align.  SPACE / STRIKE");
            grade.text = model.LastScore == null ? "READY TO LAND" : model.LastScore.Accuracy.ToString().ToUpperInvariant() + "  +" + LandingScoreRush.Format(model.LastScore.Points)
                + (model.LastMissProtected ? " / PROTECTED" : "");
            breakdown.text = Breakdown(model.LastScore, failure);
            scoreScroll.verticalNormalizedPosition = 1;
            var receipt = controller.LastLandingStrike?.Occurrence == model.Occurrence ? controller.LastLandingStrike : null;
            // An escaped creature's haul stays in the column until it has been cashed out, then the cleared rig shows.
            RefreshRig(feedbackPending && receipt != null ? receipt.Members.ToArray() : failure && outcomePending ? controller.LastHaul : controller.CatchChain, model.LastScore, false);
            if (strikeFeedback != null) strikeFeedback.Present(receipt, cards, true, HasRushPresentation);
            if (strikePanel != null && feedbackPending) strikePanel.BeginStrike();
            if (scoreRush != null) { scoreRush.Present(receipt, cards, feedbackPending); if (!feedbackPending) scoreRush.Finish(); }
        }
        if (scoreRush != null) scoreRush.Tick();
        PresentOutcome(controller, model, failure);
        if (failure && !controller.LandingFeedbackPending && !failureSounded)
        {
            failureSounded = true;
            if(sound!=null)sound.Play(DeepwaterSoundCue.Failure);
        }
        decimal displayed = feedbackPending && scoreRush != null ? scoreRush.DisplayedTotal : model.Points;
        points.text = LandingScoreRush.Format(displayed);
        // The creature's Resistance reads as health: the bar and number show what is left, draining as Landing Points land.
        decimal remaining = Math.Max(0, model.Resistance - displayed);
        resistance.text = LandingScoreRush.Format(remaining) + " / " + LandingScoreRush.Format(model.Resistance);
        progressFill.anchorMax = new Vector2(model.Resistance > 0 ? Mathf.Clamp01((float)(remaining / model.Resistance)) : 0, 1);
        if (strikePanel != null) strikePanel.Refresh(model);
        if (model.Error != null) message.text = model.Error;
        else if (feedbackPending) message.text = "RIG ACTIVATING  ·  SPACE / FINISH TO COMPLETE";
        else if (controller.LandingRevealPending && reveal != null && reveal.IsPlaying) message.text = reveal.Caption;
        else if (outcome != null && outcome.IsPlaying) message.text = outcome.Caption;
        foreach (var button in buttons) button.RefreshAvailability();
    }

    /// <summary>Starts the Landed/Escaped beat once per ended challenge and keeps the header on its current moment.</summary>
    private void PresentOutcome(FishingRunController controller, LandingChallengeRuntime model, bool failure)
    {
        if (outcome == null) return;
        if (controller.LandingOutcomePending && outcomeOccurrence != model.Occurrence)
        {
            outcomeOccurrence = model.Occurrence;
            var haul = failure ? controller.LastHaul : Array.Empty<CardInstance>();
            outcome.Begin(controller.LandingOutcomeTiming, controller.LandingOutcomeStartedAt, selectedCreature?.DisplayName ?? "Creature", cards,
                haul.Select(card => card.HaulValueContribution).ToArray(), failure ? controller.LastHaulValue : 0, rigScroll);
        }
        if (outcome.IsPlaying) { if (controller.LandingOutcomePending) outcome.Tick(); else outcome.Finish(); }
        if (outcomeOccurrence == model.Occurrence) creature.text = outcome.Header;
    }

    /// <summary>Sizes the generic fish to exactly its scoring interval, preserving the icon's pixel aspect ratio.</summary>
    private void Position(RectTransform rectangle, double center, double width)
    {
        float pixels = (float)width * track.rect.width;
        float height = rectangle == fish ? pixels * fishMarker.sprite.rect.height / fishMarker.sprite.rect.width : rectangle.sizeDelta.y;
        rectangle.sizeDelta = new Vector2(pixels, height);
        rectangle.anchoredPosition = new Vector2(((float)center - .5f) * track.rect.width, 0);
    }
    /// <summary>Displays the definition's species portrait independently of the generic moving fish marker.</summary>
    private void BindCreature()
    {
        Sprite image = selectedCreature != null ? selectedCreature.EncounterArtwork ?? selectedCreature.Artwork : null;
        portrait.sprite = image; portrait.enabled = image != null;
        tags.text = selectedCreature == null ? "" : string.Join(" / ", selectedCreature.Tags).ToUpperInvariant();
    }
    /// <summary>Fits the model's variable starting allowance into the integrated fascia without baking an attempt count into its artwork.</summary>
    private void PrepareLamps(int count)
    {
        while (lamps.Count < count) { var lamp = Instantiate(lampTemplate, lampContent); lamp.gameObject.SetActive(true); lamps.Add(lamp); }
        float step = lampContent.rect.width / Mathf.Max(4, count);
        for (int i = 0; i < lamps.Count; i++)
        {
            lamps[i].gameObject.SetActive(i < count);
            var rect = lamps[i].rectTransform; rect.sizeDelta = new Vector2(Mathf.Min(55, step - 4), 55); rect.anchoredPosition = new Vector2(i * step, 0);
        }
    }
    /// <summary>Reuses the shared card prefab in committed order and highlights only contributing stable identities.</summary>
    private void RefreshRig(CardInstance[] rig, LandingScore score, bool highlight)
    {
        while (cards.Count < rig.Length) cards.Add(Instantiate(rigCardPrefab, rigContent));
        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i]; card.gameObject.SetActive(i < rig.Length);
            if (i >= rig.Length) continue;
            card.Bind(rig[i]); card.transform.SetSiblingIndex(i);
            ((RectTransform)card.transform).anchoredPosition = new Vector2(0, -20 - i * 275);
            bool contributes = false;
            if (highlight && score != null) foreach (var item in score.Contributions) contributes |= item.SourceId == rig[i].InstanceId;
            card.SetContributing(contributes);
        }
        rigContent.sizeDelta = new Vector2(315, Mathf.Max(825, 20 + rig.Length * 275));
        emptyRig.text = failed ? "RIG CLEARED\n\nPermanent progression preserved" : "NO CREATURES\nATTACHED";
        emptyRig.gameObject.SetActive(rig.Length == 0);
    }
    /// <summary>Retains legacy concealment only for other presentation callers; redesigned runs use the rig owner's full haul.</summary>
    private static string VisibleHaul(CardInstance[] rig)
    {
        int total = 0;
        foreach (var card in rig) { if (!card.IsBait && card.HidesOwnValueDuringRun) return "?"; total += card.HaulValueContribution; }
        return total.ToString();
    }
    /// <summary>Provides a short scrollable attribution sequence, retaining every source without truncating scoring data.</summary>
    private static string Breakdown(LandingScore score, bool failure)
    {
        if (score == null) return "Good / Perfect timing\nCurrent rig amplifies each strike";
        var text = new StringBuilder("BASE ").Append(Number(score.Base));
        foreach (var item in score.Contributions)
        {
            text.Append('\n').Append(item.Label);
            if (item.Bonus != 0) text.Append(" +").Append(Number(item.Bonus));
            if (item.Multiplier != 1) text.Append(" x").Append(Number(item.Multiplier));
            if (item.Attempts > 0) text.Append(" +").Append(item.Attempts).Append(" attempts");
        }
        text.Append("\nTOTAL +").Append(LandingScoreRush.Format(score.Points));
        if (failure) text.Append("\nFailed creature not acquired");
        return text.ToString();
    }
    /// <summary>Retains exact coefficients in the inspectable arithmetic breakdown.</summary>
    private static string Number(decimal value) { return value.ToString("0.############################", System.Globalization.CultureInfo.InvariantCulture); }
}
