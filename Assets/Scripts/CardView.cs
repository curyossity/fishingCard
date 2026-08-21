using UnityEngine;
using UnityEngine.UI;

public sealed class CardView : MonoBehaviour
{
    [Header("Card")]
    [SerializeField] private CardDefinition cardDefinition;

    [Header("UI References")]
    [SerializeField] private Image artworkImage;
    [SerializeField] private Text displayNameText;
    [SerializeField] private Text cardTypeText;
    [SerializeField] private Text rarityText;
    [SerializeField] private Text rulesText;

    public CardDefinition CardDefinition => cardDefinition;

    private void Awake()
    {
        Refresh();
    }

    private void OnValidate()
    {
        Refresh();
    }

    public void SetCard(CardDefinition newCardDefinition)
    {
        cardDefinition = newCardDefinition;
        Refresh();
    }

    public void Refresh()
    {
        if (cardDefinition == null)
        {
            Clear();
            return;
        }

        SetText(displayNameText, cardDefinition.DisplayName);
        SetText(cardTypeText, cardDefinition.CardType.ToString());
        SetText(rarityText, cardDefinition.Rarity.ToString());
        SetText(rulesText, cardDefinition.RulesText);

        if (artworkImage != null)
        {
            artworkImage.sprite = cardDefinition.Artwork;
            artworkImage.enabled = cardDefinition.Artwork != null;
        }
    }

    private void Clear()
    {
        SetText(displayNameText, string.Empty);
        SetText(cardTypeText, string.Empty);
        SetText(rarityText, string.Empty);
        SetText(rulesText, string.Empty);

        if (artworkImage != null)
        {
            artworkImage.sprite = null;
            artworkImage.enabled = false;
        }
    }

    private static void SetText(Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }
}
