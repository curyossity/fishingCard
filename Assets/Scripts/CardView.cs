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
    [SerializeField] private Text encounterStateText;
    [SerializeField] private Text statsText;
    [SerializeField] private Text tagsText;
    [SerializeField] private Text rulesText;

    public CardDefinition CardDefinition => cardDefinition;

    private EncounterState encounterState = EncounterState.None;

    /// <summary>
    /// Refreshes the card UI when the view first becomes active.
    /// </summary>
    private void Awake()
    {
        Refresh();
    }

    /// <summary>
    /// Keeps Inspector-authored card view changes visible while editing.
    /// </summary>
    private void OnValidate()
    {
        Refresh();
    }

    /// <summary>
    /// Assigns a new card definition to this view and redraws the UI fields.
    /// </summary>
    public void SetCard(CardDefinition newCardDefinition)
    {
        cardDefinition = newCardDefinition;
        encounterState = EncounterState.None;
        Refresh();
    }

    /// <summary>
    /// Assigns a new card definition and encounter state to this view, then redraws the UI fields.
    /// </summary>
    public void SetCard(CardDefinition newCardDefinition, EncounterState newEncounterState)
    {
        cardDefinition = newCardDefinition;
        encounterState = newEncounterState;
        Refresh();
    }

    /// <summary>
    /// Copies the current card definition data into the optional UI references.
    /// </summary>
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
        SetText(encounterStateText, BuildEncounterStateText(encounterState));
        SetText(statsText, BuildStatsText(cardDefinition));
        SetText(tagsText, BuildTagsText(cardDefinition));
        SetText(rulesText, cardDefinition.RulesText);

        if (artworkImage != null)
        {
            artworkImage.sprite = cardDefinition.Artwork;
            artworkImage.enabled = cardDefinition.Artwork != null;
        }
    }

    /// <summary>
    /// Clears all UI references when this view has no card assigned.
    /// </summary>
    private void Clear()
    {
        SetText(displayNameText, string.Empty);
        SetText(cardTypeText, string.Empty);
        SetText(rarityText, string.Empty);
        SetText(encounterStateText, string.Empty);
        SetText(statsText, string.Empty);
        SetText(tagsText, string.Empty);
        SetText(rulesText, string.Empty);

        if (artworkImage != null)
        {
            artworkImage.sprite = null;
            artworkImage.enabled = false;
        }
    }

    /// <summary>
    /// Safely assigns text when an optional Text reference has been wired in the Inspector.
    /// </summary>
    private static void SetText(Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }

    /// <summary>
    /// Builds the compact stat line displayed by card views that expose stat text.
    /// </summary>
    private static string BuildStatsText(CardDefinition card)
    {
        if (card == null)
        {
            return string.Empty;
        }

        return $"Weight {card.Weight} / Value {card.Value}";
    }

    /// <summary>
    /// Builds the encounter state label for card views that expose state text.
    /// </summary>
    private static string BuildEncounterStateText(EncounterState state)
    {
        return state == EncounterState.None ? string.Empty : state.ToString();
    }

    /// <summary>
    /// Builds the comma-separated tag line displayed by card views that expose tag text.
    /// </summary>
    private static string BuildTagsText(CardDefinition card)
    {
        if (card == null || card.Tags == null || card.Tags.Length == 0)
        {
            return string.Empty;
        }

        return string.Join(", ", card.Tags);
    }
}
