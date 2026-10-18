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
    [SerializeField] private TMP_Text tierPrototype;
    [SerializeField] private RectTransform tierRoot;
    [SerializeField] private Color activeColor, inactiveColor;
    private readonly List<TMP_Text> tiers = new List<TMP_Text>();
    public override TMP_Text BiomeText => biomeText;
    public override TMP_Text DepthText => depthText;
    public override TMP_Text DeckText => deckText;
    public override UnityEngine.UI.Button DescendButton => descendButton;
    public override UnityEngine.UI.Button SurfaceButton => surfaceButton;
    public override UnityEngine.UI.Button SettingsButton => settingsButton;

    /// <summary>Refreshes labels and pooled rows from actual authored tiers; overlapping tiers can both be highlighted.</summary>
    public override void Refresh(BiomeDefinition biome, int depth, int load, int capacity, int deckCount, int? haulValue)
    {
        biomeText.text = biome == null ? "UNCHARTED WATERS" : biome.DisplayName.ToUpperInvariant();
        depthText.text = "DEPTH  " + Mathf.Max(0, depth) + " m";
        deckText.text = "DECK  " + Mathf.Max(0, deckCount);
        haulText.text = haulValue.HasValue ? Mathf.Max(0, haulValue.Value).ToString() : "?";
        meter.SetLoad(load, capacity);
        BiomeDepthTierDefinition[] data = biome == null ? Array.Empty<BiomeDepthTierDefinition>() : biome.DepthTiers ?? Array.Empty<BiomeDepthTierDefinition>();
        while (tiers.Count < data.Length) { TMP_Text row = Instantiate(tierPrototype, tierRoot); row.name = "Biome Tier " + (tiers.Count + 1); tiers.Add(row); }
        for (int i = 0; i < tiers.Count; i++)
        {
            bool visible = i < data.Length && data[i] != null; tiers[i].gameObject.SetActive(visible); if (!visible) continue;
            var tier = data[i]; string range = tier.MaximumDepth < 0 ? tier.MinimumDepth + "+" : tier.MinimumDepth + "–" + tier.MaximumDepth;
            tiers[i].text = (tier.ContainsDepth(depth) ? ">  " : "·  ") + tier.DisplayName.ToUpperInvariant() + "   " + range + " m";
            tiers[i].color = tier.ContainsDepth(depth) ? activeColor : inactiveColor;
            var rect = tiers[i].rectTransform; rect.anchorMin = new Vector2(0, 1 - (i + 1f) / data.Length); rect.anchorMax = new Vector2(1, 1 - i / (float)data.Length); rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }

    /// <summary>Preserves learned preview information without putting run status on the card face.</summary>
    public override void SetTrailPreview(string preview)
    { trailText.text = preview ?? string.Empty; trailText.gameObject.SetActive(!string.IsNullOrEmpty(preview)); }
}
