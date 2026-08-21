using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Fishing Cards/Card Definition")]
public class CardDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string displayName;
    [SerializeField] private CardType cardType;
    [SerializeField] private CardRarity rarity;

    [Header("Presentation")]
    [SerializeField] private Sprite artwork;
    [TextArea(2, 5)]
    [SerializeField] private string rulesText;

    public string DisplayName => displayName;
    public CardType CardType => cardType;
    public CardRarity Rarity => rarity;
    public Sprite Artwork => artwork;
    public string RulesText => rulesText;
}

public enum CardType
{
    Technique,
    Bait,
    Equipment,
    Creature,
    Encounter,
    Location
}

public enum CardRarity
{
    Common,
    Uncommon,
    Rare,
    Legendary
}
