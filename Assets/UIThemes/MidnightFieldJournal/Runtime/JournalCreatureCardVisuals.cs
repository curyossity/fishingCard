using TMPro;
using UnityEngine;

/// <summary>Supplies exact theme artwork and masked footer metadata to the shared central card view.</summary>
public sealed class JournalCreatureCardVisuals : CreatureCardVisualExtension
{
    [SerializeField] private MidnightFieldJournalTheme theme;
    [SerializeField] private TMP_Text tagsText;
    [SerializeField] private GameObject rarityRoot;
    [SerializeField] private JournalRarityHook[] rarityHooks;
    [SerializeField] private bool showRarity;
    [SerializeField] private JournalCardOutline catchSelection;
    [SerializeField] private JournalCardOutline baitSelection;
    private bool metadataVisible;

    /// <summary>Returns only this creature's selected new-theme art, never a legacy face overlay or another species.</summary>
    public override Sprite GetArtwork(CardDefinition card)
    {
        if (theme != null && card != null && theme.TryGetArtwork(card.UniqueId, out Sprite sprite)) return sprite;
        return null;
    }

    /// <summary>Shows authored tags and approved rarity presentation only when identity is permitted.</summary>
    public override void Refresh(CardDefinition card, bool informationHidden, bool identityHidden, bool catchLayout)
    {
        metadataVisible = card != null && catchLayout && !identityHidden;
        if (tagsText != null) tagsText.text = metadataVisible
            ? string.Join("  ·  ", CreatureTagVocabulary.Normalize(card.Tags)).ToUpperInvariant() : string.Empty;
        if (rarityRoot != null) rarityRoot.SetActive(metadataVisible && showRarity);
        if (!metadataVisible || rarityHooks == null || theme == null) return;
        int active = Mathf.Clamp((int)card.Rarity + 1, 1, 4);
        for (int i = 0; i < rarityHooks.Length; i++)
        {
            Color color = theme.ParchmentInk; color.a = i < active ? 1 : .22f;
            rarityHooks[i].color = color;
        }
    }

    /// <summary>Enables an approved footer treatment or a temporary review instance without changing gameplay data.</summary>
    public void SetRarityVisible(bool enabled)
    { showRarity = enabled; if (rarityRoot != null) rarityRoot.SetActive(metadataVisible && showRarity); }

    /// <summary>Uses hollow native UI borders so selection never paints over either ability panel.</summary>
    public override void RefreshSelection(EncounterEffectChoice choice, bool active)
    {
        if (catchSelection != null) catchSelection.gameObject.SetActive(active && choice == EncounterEffectChoice.Catch);
        if (baitSelection != null) baitSelection.gameObject.SetActive(active && choice == EncounterEffectChoice.Bait);
    }

    /// <summary>Clears all identity-bearing footer content.</summary>
    public override void Clear()
    {
        metadataVisible = false;
        if (tagsText != null) tagsText.text = string.Empty;
        if (rarityRoot != null) rarityRoot.SetActive(false);
    }
}
