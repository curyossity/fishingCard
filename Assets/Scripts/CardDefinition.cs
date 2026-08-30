using System;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Fishing Cards/Card Definition")]
public class CardDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string uniqueId;
    [SerializeField] private string displayName;
    [SerializeField] private CardType cardType;
    [SerializeField] private CardRarity rarity;
    [SerializeField] private string[] tags = Array.Empty<string>();

    [Header("Presentation")]
    [SerializeField] private Sprite artwork;
    [TextArea(2, 5)]
    [SerializeField] private string rulesText;

    [Header("Catch Stats")]
    [Min(0)]
    [SerializeField] private int weight;
    [Min(0)]
    [SerializeField] private int value;

    [Header("Encounter Availability")]
    [SerializeField] private string[] biomeIds = Array.Empty<string>();
    [Min(0)]
    [SerializeField] private int minimumDepth;
    [SerializeField] private int maximumDepth = -1;

    [Header("Effects")]
    [SerializeField] private CardEffectDefinition[] effects = Array.Empty<CardEffectDefinition>();

    public string UniqueId => uniqueId;
    public string DisplayName => displayName;
    public CardType CardType => cardType;
    public CardRarity Rarity => rarity;
    public string[] Tags => tags;
    public Sprite Artwork => artwork;
    public string RulesText => rulesText;
    public int Weight => weight;
    public int Value => value;
    public string[] BiomeIds => biomeIds;
    public int MinimumDepth => minimumDepth;
    public int MaximumDepth => maximumDepth;
    public CardEffectDefinition[] Effects => effects;

    public bool HasWeight => weight > 0;
    public bool HasValue => value > 0;

    /// <summary>
    /// Checks whether this card can appear or be used at the given run depth.
    /// </summary>
    public bool IsAvailableAtDepth(int depth)
    {
        return depth >= minimumDepth && (maximumDepth < 0 || depth <= maximumDepth);
    }

    /// <summary>
    /// Checks whether this card belongs to the requested biome, or to all biomes when no biome IDs are set.
    /// </summary>
    public bool IsAvailableInBiome(string biomeId)
    {
        if (biomeIds == null || biomeIds.Length == 0)
        {
            return true;
        }

        for (int i = 0; i < biomeIds.Length; i++)
        {
            if (string.Equals(biomeIds[i], biomeId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Builds a readable card summary for debug logs and editor-facing diagnostics.
    /// </summary>
    public string BuildDebugSummary()
    {
        StringBuilder summary = new StringBuilder();
        summary.Append(DisplayName);
        summary.Append(" [");
        summary.Append(CardType);
        summary.Append("] ");
        summary.Append("Weight: ");
        summary.Append(weight);
        summary.Append(", Value: ");
        summary.Append(value);

        if (effects != null && effects.Length > 0)
        {
            summary.AppendLine();
            summary.Append("Effects:");
            for (int i = 0; i < effects.Length; i++)
            {
                summary.AppendLine();
                summary.Append("- ");
                summary.Append(effects[i].BuildDebugSummary());
            }
        }

        return summary.ToString();
    }
}

public enum CardType
{
    Technique = 0,
    Bait = 1,
    Equipment = 2,
    Creature = 3,
    Treasure = 4,
    Hazard = 5,
    Environment = 6,
    Opportunity = 7,
    ApexEncounter = 8,
    Encounter = 9,
    Location = 10
}

public enum CardRarity
{
    Common,
    Uncommon,
    Rare,
    Legendary
}

public enum CardEffectType
{
    AddLineLoadModifier,
    RemoveLineLoadModifier,
    ModifyCatchValue,
    ModifyFutureEncounterProperties,
    AffectCreatureTags,
    HideEncounterInformation,
    ReplaceEncounter,
    AvoidEncounter,
    ReleaseCatch,
    ModifyNextDescend,
    ModifyCaughtCard
}

public enum CardEffectTrigger
{
    Manual,
    WhenCaught,
    WhileAttached,
    WhenReleased,
    OnDescend,
    WhenSurfaceBegins
}

public enum CardEffectTarget
{
    None,
    Self,
    HookedEncounter,
    CurrentEncounter,
    CatchChain,
    SpecificCaughtCard,
    FutureEncounters,
    NextDescend,
    SurfaceAttempt
}

public enum CardEffectTone
{
    Neutral,
    Positive,
    Negative
}

public enum EncounterState
{
    None,
    Encountered,
    Hooked,
    Caught
}

[Serializable]
public sealed class CardEffectDefinition
{
    [SerializeField] private string effectId;
    [SerializeField] private CardEffectType effectType;
    [SerializeField] private CardEffectTrigger trigger;
    [SerializeField] private CardEffectTarget target;
    [SerializeField] private CardEffectTone effectTone;
    [SerializeField] private int amount;
    [SerializeField] private string[] requiredTags = Array.Empty<string>();
    [SerializeField] private CardDefinition replacementCard;
    [SerializeField] private bool expiresAfterUse;
    [TextArea(1, 3)]
    [SerializeField] private string reminderText;

    public string EffectId => effectId;
    public CardEffectType EffectType => effectType;
    public CardEffectTrigger Trigger => trigger;
    public CardEffectTarget Target => target;
    public CardEffectTone EffectTone => effectTone;
    public int Amount => amount;
    public string[] RequiredTags => requiredTags;
    public CardDefinition ReplacementCard => replacementCard;
    public bool ExpiresAfterUse => expiresAfterUse;
    public string ReminderText => reminderText;

    /// <summary>
    /// Builds a readable effect summary for debug logs and active-effect inspection.
    /// </summary>
    public string BuildDebugSummary()
    {
        StringBuilder summary = new StringBuilder();
        summary.Append(string.IsNullOrWhiteSpace(effectId) ? effectType.ToString() : effectId);
        summary.Append(" | ");
        summary.Append(trigger);
        summary.Append(" -> ");
        summary.Append(target);

        if (effectTone != CardEffectTone.Neutral)
        {
            summary.Append(" | ");
            summary.Append(effectTone);
        }

        if (amount != 0)
        {
            summary.Append(" | Amount: ");
            summary.Append(amount);
        }

        if (requiredTags != null && requiredTags.Length > 0)
        {
            summary.Append(" | Tags: ");
            summary.Append(string.Join(", ", requiredTags));
        }

        if (replacementCard != null)
        {
            summary.Append(" | Replacement: ");
            summary.Append(replacementCard.DisplayName);
        }

        if (!string.IsNullOrWhiteSpace(reminderText))
        {
            summary.Append(" | ");
            summary.Append(reminderText);
        }

        return summary.ToString();
    }
}

public enum CoreActionType
{
    Descend,
    Release,
    Surface
}
