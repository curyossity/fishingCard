using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>Displays journal navigation, authored biome tiers and controller-owned load/haul on reusable surfaces.</summary>
public sealed class JournalGameplayHud : FishingRunHudPresentation
{
    [SerializeField] private TMP_Text biomeText, depthText, deckText, haulText, trailText;
    [SerializeField] private UnityEngine.UI.Button descendButton, surfaceButton, settingsButton;
    [SerializeField] private LineLoadMeterView meter;
    [SerializeField] private JournalDepthGuide depthGuide;
    [SerializeField] private GameObject openingCard;
    public override TMP_Text BiomeText => biomeText;
    public override TMP_Text DepthText => depthText;
    public override TMP_Text DeckText => deckText;
    public override UnityEngine.UI.Button DescendButton => descendButton;
    public override UnityEngine.UI.Button SurfaceButton => surfaceButton;
    public override UnityEngine.UI.Button SettingsButton => settingsButton;
    public override RectTransform OpeningCard => openingCard == null ? null : (RectTransform)openingCard.transform;

    /// <summary>Refreshes labels and pooled rows from actual authored tiers; overlapping tiers can both be highlighted.</summary>
    public override void Refresh(BiomeDefinition biome, int depth, int load, int capacity, int deckCount, int? haulValue)
    {
        biomeText.text = biome == null ? "UNCHARTED WATERS" : biome.DisplayName.ToUpperInvariant();
        depthText.text = "DEPTH  " + Mathf.Max(0, depth) + " m";
        deckText.text = "DECK  " + Mathf.Max(0, deckCount);
        haulText.text = haulValue.HasValue ? Mathf.Max(0, haulValue.Value).ToString() : "?";
        meter.SetLoad(load, capacity);
        depthGuide.Refresh(biome, depth);
    }

    /// <summary>Shows the art-only boat from the explicit controller state, never from an empty encounter alone.</summary>
    public override void SetOpeningVisible(bool visible)
    { if (openingCard == null) return; openingCard.SetActive(visible); if (visible) openingCard.transform.SetAsLastSibling(); }

    /// <summary>Preserves learned preview information without putting run status on the card face.</summary>
    public override void SetTrailPreview(string preview)
    { trailText.text = preview ?? string.Empty; trailText.gameObject.SetActive(!string.IsNullOrEmpty(preview)); }
}
