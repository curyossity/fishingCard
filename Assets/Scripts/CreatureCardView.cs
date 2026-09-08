using UnityEngine;
using UnityEngine.UI;

public sealed class CreatureCardView : MonoBehaviour
{
    private static readonly Color InkColor = new Color(0.055f, 0.075f, 0.07f, 1f);
    private static readonly Color CreamColor = new Color(0.96f, 0.90f, 0.76f, 1f);

    private Image cardFaceImage;
    private Image fallbackArtworkImage;
    private Text cardTypeText;
    private Text cardNameText;
    private Text weightText;
    private Text valueText;
    private Text effectText;
    private readonly Image[] rarityHooks = new Image[4];
    private Sprite fallbackCardFace;
    private Sprite rarityHookSprite;
    private Font uiFont;

    /// <summary>
    /// Stores shared visual assets and creates the layered card-face UI.
    /// </summary>
    public void Initialize(Sprite fallbackCardFace, Sprite rarityHookSprite)
    {
        this.fallbackCardFace = fallbackCardFace;
        this.rarityHookSprite = rarityHookSprite;
        EnsureLayout();
    }

    /// <summary>
    /// Displays one creature card using resolved runtime stats inside the supplied card artwork.
    /// </summary>
    public void SetCard(
        CardDefinition card,
        int resolvedWeight,
        int resolvedValue,
        bool informationHidden)
    {
        EnsureLayout();
        gameObject.SetActive(card != null);

        if (card == null)
        {
            return;
        }

        bool hasDedicatedCardFace = card.CardFaceArtwork != null;
        cardFaceImage.sprite = hasDedicatedCardFace ? card.CardFaceArtwork : fallbackCardFace;
        cardFaceImage.color = cardFaceImage.sprite == null ? new Color(0.07f, 0.12f, 0.13f, 1f) : Color.white;

        fallbackArtworkImage.sprite = hasDedicatedCardFace ? null : card.Artwork;
        fallbackArtworkImage.enabled = fallbackArtworkImage.sprite != null;
        cardTypeText.text = BuildCardTypeText(card.CardType);
        cardNameText.text = card.DisplayName.ToUpperInvariant();
        cardNameText.gameObject.SetActive(!card.CardFaceIncludesName);
        weightText.text = informationHidden ? "?" : Mathf.Max(0, resolvedWeight).ToString();
        valueText.text = informationHidden ? "?" : Mathf.Max(0, resolvedValue).ToString();
        effectText.text = informationHidden ? string.Empty : card.RulesText;
        RefreshRarityHooks(card.Rarity);
    }

    /// <summary>
    /// Creates the card art, runtime text regions, and four rarity-hook overlays once.
    /// </summary>
    private void EnsureLayout()
    {
        if (cardFaceImage != null)
        {
            return;
        }

        uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        cardFaceImage = AddImage(gameObject, fallbackCardFace);
        cardFaceImage.preserveAspect = true;

        GameObject artworkObject = CreateUiObject("Fallback Creature Artwork", transform);
        RectTransform artworkRect = artworkObject.GetComponent<RectTransform>();
        SetAnchoredRect(artworkRect, new Vector2(0.20f, 0.43f), new Vector2(0.80f, 0.76f));
        fallbackArtworkImage = AddImage(artworkObject, null);
        fallbackArtworkImage.preserveAspect = true;

        cardTypeText = CreateText("Card Type", 11, FontStyle.Bold, TextAnchor.MiddleCenter, InkColor);
        SetAnchoredRect(cardTypeText.rectTransform, new Vector2(0.34f, 0.855f), new Vector2(0.66f, 0.90f));

        cardNameText = CreateText("Card Name", 24, FontStyle.Bold, TextAnchor.MiddleCenter, CreamColor);
        SetAnchoredRect(cardNameText.rectTransform, new Vector2(0.20f, 0.765f), new Vector2(0.80f, 0.85f));
        cardNameText.resizeTextForBestFit = true;
        cardNameText.resizeTextMinSize = 12;
        cardNameText.resizeTextMaxSize = 24;

        weightText = CreateText("Weight", 32, FontStyle.Bold, TextAnchor.MiddleCenter, CreamColor);
        SetAnchoredRect(weightText.rectTransform, new Vector2(0.29f, 0.35f), new Vector2(0.49f, 0.45f));
        weightText.resizeTextForBestFit = true;
        weightText.resizeTextMinSize = 12;
        weightText.resizeTextMaxSize = 32;

        valueText = CreateText("Value", 32, FontStyle.Bold, TextAnchor.MiddleCenter, CreamColor);
        SetAnchoredRect(valueText.rectTransform, new Vector2(0.51f, 0.35f), new Vector2(0.71f, 0.45f));
        valueText.resizeTextForBestFit = true;
        valueText.resizeTextMinSize = 12;
        valueText.resizeTextMaxSize = 32;

        effectText = CreateText("Effect", 13, FontStyle.Normal, TextAnchor.MiddleCenter, InkColor);
        SetAnchoredRect(effectText.rectTransform, new Vector2(0.18f, 0.18f), new Vector2(0.82f, 0.34f));
        effectText.resizeTextForBestFit = true;
        effectText.resizeTextMinSize = 8;
        effectText.resizeTextMaxSize = 13;

        float[] hookCenters = { 0.26f, 0.42f, 0.58f, 0.74f };

        for (int i = 0; i < rarityHooks.Length; i++)
        {
            GameObject hookObject = CreateUiObject($"Rarity Hook {i + 1}", transform);
            RectTransform hookRect = hookObject.GetComponent<RectTransform>();
            float center = hookCenters[i];
            SetAnchoredRect(hookRect, new Vector2(center - 0.04f, 0.105f), new Vector2(center + 0.04f, 0.18f));
            rarityHooks[i] = AddImage(hookObject, rarityHookSprite);
            rarityHooks[i].preserveAspect = true;
        }
    }

    /// <summary>
    /// Shows one through four anchor icons for the card's authored rarity.
    /// </summary>
    private void RefreshRarityHooks(CardRarity rarity)
    {
        int activeHookCount = Mathf.Clamp((int)rarity + 1, 1, rarityHooks.Length);

        for (int i = 0; i < rarityHooks.Length; i++)
        {
            rarityHooks[i].gameObject.SetActive(i < activeHookCount && rarityHookSprite != null);
        }
    }

    /// <summary>
    /// Converts gameplay card categories into the compact card-face heading.
    /// </summary>
    private static string BuildCardTypeText(CardType cardType)
    {
        switch (cardType)
        {
            case CardType.Creature:
                return "CATCH CARD";
            case CardType.ApexEncounter:
                return "APEX CATCH";
            default:
                return $"{cardType.ToString().ToUpperInvariant()} CARD";
        }
    }

    /// <summary>
    /// Creates a UI GameObject on Unity's UI layer under this card.
    /// </summary>
    private static GameObject CreateUiObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
        uiObject.layer = 5;
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    /// <summary>
    /// Adds a non-interactive image using the supplied sprite.
    /// </summary>
    private static Image AddImage(GameObject target, Sprite sprite)
    {
        Image image = target.AddComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = false;
        return image;
    }

    /// <summary>
    /// Creates configured legacy UI text under this card.
    /// </summary>
    private Text CreateText(
        string objectName,
        int fontSize,
        FontStyle fontStyle,
        TextAnchor alignment,
        Color color)
    {
        GameObject textObject = CreateUiObject(objectName, transform);
        Text text = textObject.AddComponent<Text>();
        text.font = uiFont;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        return text;
    }

    /// <summary>
    /// Applies normalized anchors without pixel offsets to one card-face element.
    /// </summary>
    private static void SetAnchoredRect(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax)
    {
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
