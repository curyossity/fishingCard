using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>Renders the approved independent Landing artwork, current rig and attributed scoring without resolving gameplay.</summary>
public sealed class TackleLandingPresentation : MonoBehaviour
{
    [SerializeField] private TMP_Text phase, haul, creature, tags, points, resistance, attempts, movement, message, grade, breakdown, emptyRig;
    [SerializeField] private UnityEngine.UI.Image portrait, fishMarker;
    [SerializeField] private RectTransform track, fish, goodZone, perfectZone, progressFill, lampContent, rigContent;
    [SerializeField] private UnityEngine.UI.Image lampTemplate;
    [SerializeField] private Sprite lampOn, lampOff;
    [SerializeField] private TackleRigCardView rigCardPrefab;
    [SerializeField] private UnityEngine.UI.ScrollRect rigScroll, scoreScroll;
    [SerializeField] private TackleButtonFeedback[] buttons;
    private readonly List<TackleRigCardView> cards = new List<TackleRigCardView>();
    private readonly List<UnityEngine.UI.Image> lamps = new List<UnityEngine.UI.Image>();
    private long occurrence = -1, revision = -1;
    private double highlightUntil;
    private bool highlighted, failed;
    private CardDefinition selectedCreature;
    private string biomeName;

    /// <summary>Resets only visual identities so future encounters cannot reuse stale labels or highlights.</summary>
    private void OnDisable() { occurrence = revision = -1; selectedCreature = null; highlighted = false; }

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
            occurrence = model.Occurrence; revision = -1; failed = false;
            PrepareLamps(model.AttemptsRemaining);
            BindCreature();
            rigScroll.verticalNormalizedPosition = 1;
        }
        Position(fish, model.FishPosition, model.FishWidth);
        Position(goodZone, model.ZonePosition, model.GoodWidth);
        Position(perfectZone, model.ZonePosition, model.PerfectWidth);
        fishMarker.color = model.IsTelegraphingBurst ? new Color32(255, 169, 76, 255) : Color.white;
        progressFill.anchorMax = new Vector2(model.Resistance > 0 ? Mathf.Clamp01((float)(model.Points / model.Resistance)) : 0, 1);
        movement.text = model.IsTelegraphingBurst ? "BURST SOON" : model.Pattern.ToString().ToUpperInvariant();
        if (revision != model.Revision || failed != failure)
        {
            revision = model.Revision; failed = failure;
            phase.text = biomeName.ToUpperInvariant() + " / LAND";
            creature.text = failure ? "EXPEDITION ENDED" : (selectedCreature?.DisplayName ?? "Landing").ToUpperInvariant();
            haul.text = "HAUL " + (failure ? controller.LastHaulValue.ToString() : VisibleHaul(controller.CatchChain));
            points.text = Number(model.Points); resistance.text = Number(model.Resistance);
            attempts.text = model.AttemptsRemaining + " ATTEMPTS LEFT";
            for (int i = 0; i < lamps.Count; i++) lamps[i].sprite = i < model.AttemptsRemaining ? lampOn : lampOff;
            message.text = model.Error ?? (failure ? "Full Catch value paid: " + controller.LastHaulValue + ". Bait: 0. Rig cleared."
                : controller.AwaitingRigPlacement ? "LANDED! Ready for rig placement."
                : "Strike when the fish marker and zone align.  SPACE / STRIKE");
            grade.text = model.LastScore == null ? "READY TO LAND" : model.LastScore.Accuracy.ToString().ToUpperInvariant() + "  +" + Number(model.LastScore.Points);
            breakdown.text = Breakdown(model.LastScore, failure);
            scoreScroll.verticalNormalizedPosition = 1;
            highlightUntil = Time.realtimeSinceStartupAsDouble + .35; highlighted = model.LastScore != null && !failure;
            RefreshRig(controller.CatchChain, model.LastScore, highlighted);
        }
        if (highlighted && Time.realtimeSinceStartupAsDouble >= highlightUntil)
        { highlighted = false; foreach (var card in cards) card.SetContributing(false); }
        if (model.Error != null) message.text = model.Error;
        foreach (var button in buttons) button.RefreshAvailability();
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
    /// <summary>Authors no attempt limit in the art: lamps match the current model's configured starting allowance.</summary>
    private void PrepareLamps(int count)
    {
        while (lamps.Count < count) { var lamp = Instantiate(lampTemplate, lampContent); lamp.gameObject.SetActive(true); lamps.Add(lamp); }
        float step = 280f / Mathf.Max(4, count);
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
    /// <summary>Preserves concealed Catch values and zero Bait value in the header.</summary>
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
        }
        text.Append("\nTOTAL +").Append(Number(score.Points));
        if (failure) text.Append("\nFailed creature not acquired");
        return text.ToString();
    }
    /// <summary>Displays exact decimal scores using glyphs in the approved font family.</summary>
    private static string Number(decimal value) { return value.ToString("0.############################", System.Globalization.CultureInfo.InvariantCulture); }
}
