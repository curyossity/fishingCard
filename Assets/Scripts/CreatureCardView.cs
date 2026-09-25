using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Renders a creature card from supplied visual layers and runtime-owned fields.</summary>
public sealed class CreatureCardView : MonoBehaviour
{
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

    [Header("Rarity Anchors")]
    [SerializeField] private Image[] anchorSockets = new Image[4];
    [SerializeField] private Image[] anchorMarkers = new Image[4];

    [Header("Fallback Assets")]
    [SerializeField] private Sprite fallbackCardFace;
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

    /// <summary>Displays one creature card using resolved runtime values.</summary>
    public void SetCard(CardDefinition card, int resolvedWeight, int resolvedValue, bool informationHidden)
    {
        if (card == null)
        {
            ClearFields();
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        bool usesCompleteSuppliedFace = card.CardFaceArtwork != null;
        SetContent(
            card.DisplayName,
            BuildCardTypeText(card.CardType),
            usesCompleteSuppliedFace ? null : card.Artwork,
            usesCompleteSuppliedFace ? card.CardFaceArtwork : fallbackCardFace,
            informationHidden ? "?" : Mathf.Max(0, resolvedWeight).ToString(),
            informationHidden ? "?" : Mathf.Max(0, resolvedValue).ToString(),
            informationHidden ? string.Empty : card.RulesText,
            card.Rarity,
            card.CardFaceIncludesName,
            usesCompleteSuppliedFace);
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
            fallbackCardFace,
            Mathf.Max(0, weight).ToString(),
            Mathf.Max(0, value).ToString(),
            rules,
            rarity,
            false,
            false);
    }

    /// <summary>Shows the authored blank template with no runtime values or filled rarity anchors.</summary>
    public void SetBlank()
    {
        gameObject.SetActive(true);
        SetImage(cardBackground, fallbackCardFace);
        SetImage(creatureArtwork, null);
        SetText(cardTypeText, string.Empty);
        SetText(cardNameText, string.Empty);
        SetText(weightText, string.Empty);
        SetText(valueText, string.Empty);
        SetText(effectText, string.Empty);
        if (cardFrame != null)
        {
            cardFrame.enabled = true;
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
        Sprite background,
        string weight,
        string value,
        string rules,
        CardRarity rarity,
        bool sourceIncludesName,
        bool usesCompleteSuppliedFace)
    {
        SetImage(cardBackground, background);
        SetImage(creatureArtwork, artwork);

        if (artworkOverflowLayer != null)
        {
            artworkOverflowLayer.sprite = null;
            artworkOverflowLayer.enabled = false;
        }

        if (cardFrame != null)
        {
            cardFrame.enabled = !usesCompleteSuppliedFace;
        }

        SetText(cardTypeText, cardType);
        SetText(cardNameText, sourceIncludesName ? string.Empty : displayName);
        SetText(weightText, weight);
        SetText(valueText, value);
        SetText(effectText, rules);
        RefreshRarityAnchors(rarity);
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

    private static string BuildCardTypeText(CardType cardType)
    {
        return cardType == CardType.ApexEncounter ? "APEX CATCH" : "CATCH CARD";
    }
}
