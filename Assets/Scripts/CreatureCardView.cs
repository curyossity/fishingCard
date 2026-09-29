using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Renders catchable and event encounters from supplied visual layers and runtime-owned fields.</summary>
public sealed class CreatureCardView : MonoBehaviour
{
    public const float ReferenceAspectRatio = 1101f / 1429f;

    private static readonly Color32 CatchNameColor = new Color32(239, 226, 194, 255);
    private static readonly Color32 CatchRulesColor = new Color32(25, 55, 54, 255);
    private static readonly Color32 EventContentColor = new Color32(78, 30, 34, 255);

    [Header("Supplied Visual Layers")]
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image creatureArtwork;
    [SerializeField] private Image artworkOverflowLayer;
    [SerializeField] private Image cardFrame;
    [SerializeField] private Image interactionOverlay;

    [Header("Runtime Fields")]
    [SerializeField] private TMP_Text cardTypeText;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text weightText;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private TMP_Text effectText;

    [Header("Runtime Layout")]
    [SerializeField] private RectTransform rulesRegion;
    [SerializeField] private RectTransform rarityAnchorRoot;

    [Header("Rarity Anchors")]
    [SerializeField] private Image[] anchorSockets = new Image[4];
    [SerializeField] private Image[] anchorMarkers = new Image[4];

    [Header("Fallback Assets")]
    [SerializeField] private Sprite fallbackCardFace;
    [SerializeField] private Sprite eventCardFace;
    [SerializeField] private Sprite rarityHookSprite;

    public int AnchorSlotCount => anchorMarkers == null ? 0 : anchorMarkers.Length;
    public Image InteractionOverlay => interactionOverlay;

    /// <summary>Preserves the existing setup API while prefab references own the card geometry.</summary>
    public void Initialize(Sprite newFallbackCardFace, Sprite newRarityHookSprite)
    {
        if (newFallbackCardFace != null)
        {
            fallbackCardFace = newFallbackCardFace;
        }

        if (newRarityHookSprite != null)
        {
            rarityHookSprite = newRarityHookSprite;
        }

        ApplyAnchorSprites();
    }

    /// <summary>Displays one encounter using its card-type layout and resolved runtime values.</summary>
    public void SetCard(CardDefinition card, int resolvedWeight, int resolvedValue, bool informationHidden)
    {
        if (card == null)
        {
            ClearFields();
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        Sprite encounterArtwork = card.EncounterArtwork != null ? card.EncounterArtwork : card.Artwork;
        bool usesCatchLayout = UsesCatchCardLayout(card.CardType);
        Sprite background = usesCatchLayout || eventCardFace == null ? fallbackCardFace : eventCardFace;

        SetContent(
            card.DisplayName.ToUpperInvariant(),
            BuildCardTypeText(card.CardType),
            encounterArtwork,
            card.ArtworkLayout,
            background,
            informationHidden ? "?" : FormatWeight(Mathf.Max(0, resolvedWeight)),
            informationHidden ? "?" : Mathf.Max(0, resolvedValue).ToString(),
            informationHidden ? string.Empty : card.RulesText,
            card.Rarity,
            usesCatchLayout);
    }

    /// <summary>Populates the visual contract directly for editor previews and UI tests.</summary>
    public void SetPreview(
        string displayName,
        string cardType,
        Sprite artwork,
        int weight,
        int value,
        string rules,
        CardRarity rarity)
    {
        gameObject.SetActive(true);
        SetContent(
            displayName,
            cardType,
            artwork,
            CardArtworkLayout.MaskedRegion,
            fallbackCardFace,
            FormatWeight(Mathf.Max(0, weight)),
            Mathf.Max(0, value).ToString(),
            rules,
            rarity,
            true);
    }

    private static string FormatWeight(int weight)
    {
        return $"{weight}<size=50%>KG</size>";
    }

    /// <summary>Shows the authored blank template with no runtime values or filled rarity anchors.</summary>
    public void SetBlank()
    {
        gameObject.SetActive(true);
        SetImage(cardBackground, fallbackCardFace);
        SetImage(creatureArtwork, null);
        SetImage(artworkOverflowLayer, null);
        ApplyCardLayout(true);
        SetText(cardTypeText, string.Empty);
        SetText(cardNameText, string.Empty);
        SetText(weightText, string.Empty);
        SetText(valueText, string.Empty);
        SetText(effectText, string.Empty);
        if (cardFrame != null)
        {
            cardFrame.enabled = false;
        }

        if (anchorMarkers != null)
        {
            for (int i = 0; i < anchorMarkers.Length; i++)
            {
                if (anchorMarkers[i] != null)
                {
                    anchorMarkers[i].gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>Updates supplied images and runtime text without changing authored geometry.</summary>
    private void SetContent(
        string displayName,
        string cardType,
        Sprite artwork,
        CardArtworkLayout artworkLayout,
        Sprite background,
        string weight,
        string value,
        string rules,
        CardRarity rarity,
        bool usesCatchLayout)
    {
        ApplyCardLayout(usesCatchLayout);
        SetImage(cardBackground, background);
        bool usesFullCardOverlay = artwork != null && artworkLayout == CardArtworkLayout.FullCardOverlay;
        SetImage(creatureArtwork, usesFullCardOverlay ? null : artwork);
        SetImage(artworkOverflowLayer, usesFullCardOverlay ? artwork : null);

        if (cardFrame != null)
        {
            cardFrame.enabled = false;
        }

        SetText(cardTypeText, cardType);
        SetText(cardNameText, displayName);
        SetText(weightText, weight);
        SetText(valueText, value);
        SetText(effectText, rules);
        if (usesCatchLayout)
        {
            RefreshRarityAnchors(rarity);
        }
        else
        {
            HideRarityAnchors();
        }
    }

    /// <summary>Switches between catch-only stats and the expanded event rules region.</summary>
    private void ApplyCardLayout(bool usesCatchLayout)
    {
        SetParentActive(weightText, usesCatchLayout);
        SetParentActive(valueText, usesCatchLayout);

        if (rarityAnchorRoot != null)
        {
            rarityAnchorRoot.gameObject.SetActive(usesCatchLayout);
        }

        if (rulesRegion != null)
        {
            rulesRegion.anchorMin = new Vector2(0.085f, usesCatchLayout ? 0.175f : 0.055f);
            rulesRegion.anchorMax = new Vector2(0.915f, 0.315f);
            rulesRegion.offsetMin = Vector2.zero;
            rulesRegion.offsetMax = Vector2.zero;
        }

        if (cardNameText != null)
        {
            cardNameText.color = usesCatchLayout ? CatchNameColor : EventContentColor;
        }

        if (effectText != null)
        {
            effectText.color = usesCatchLayout ? CatchRulesColor : EventContentColor;
        }
    }

    private void HideRarityAnchors()
    {
        if (anchorMarkers == null)
        {
            return;
        }

        for (int i = 0; i < anchorMarkers.Length; i++)
        {
            if (anchorMarkers[i] != null)
            {
                anchorMarkers[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>Shows one through four filled anchors while keeping all four sockets visible.</summary>
    private void RefreshRarityAnchors(CardRarity rarity)
    {
        ApplyAnchorSprites();
        if (anchorMarkers == null)
        {
            return;
        }

        int activeCount = Mathf.Clamp((int)rarity + 1, 1, 4);
        for (int i = 0; i < anchorMarkers.Length; i++)
        {
            if (anchorMarkers[i] != null)
            {
                anchorMarkers[i].gameObject.SetActive(i < activeCount);
            }
        }
    }

    private void ApplyAnchorSprites()
    {
        if (anchorMarkers == null)
        {
            return;
        }

        for (int i = 0; i < anchorMarkers.Length; i++)
        {
            if (anchorMarkers[i] != null && anchorMarkers[i].sprite == null)
            {
                anchorMarkers[i].sprite = rarityHookSprite;
            }
        }
    }

    private void ClearFields()
    {
        SetBlank();
    }

    private static void SetImage(Image image, Sprite sprite)
    {
        if (image == null)
        {
            return;
        }

        image.sprite = sprite;
        image.enabled = sprite != null;
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
        {
            text.text = value ?? string.Empty;
        }
    }

    private static void SetParentActive(TMP_Text text, bool active)
    {
        if (text != null && text.transform.parent != null)
        {
            text.transform.parent.gameObject.SetActive(active);
        }
    }

    private static bool UsesCatchCardLayout(CardType cardType)
    {
        return cardType == CardType.Creature
            || cardType == CardType.Treasure
            || cardType == CardType.ApexEncounter;
    }

    private static string BuildCardTypeText(CardType cardType)
    {
        switch (cardType)
        {
            case CardType.ApexEncounter:
                return "APEX CATCH";
            case CardType.Creature:
            case CardType.Treasure:
                return "CATCH CARD";
            default:
                return "EVENT CARD";
        }
    }
}
