using System;
using System.Collections.Generic;
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
    [SerializeField] private Sprite encounterArtwork;
    [SerializeField] private CardArtworkLayout artworkLayout;
    [SerializeField] private Sprite cardFaceArtwork;
    [SerializeField] private bool cardFaceIncludesName;
    [TextArea(2, 5)]
    [SerializeField] private string rulesText;
    [SerializeField] private CardTextHighlightDefinition[] rulesTextHighlights =
        Array.Empty<CardTextHighlightDefinition>();

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
    public Sprite EncounterArtwork => encounterArtwork;
    public CardArtworkLayout ArtworkLayout => artworkLayout;
    public Sprite CardFaceArtwork => cardFaceArtwork;
    public bool CardFaceIncludesName => cardFaceIncludesName;
    public string RulesText => rulesText;
    public CardTextHighlightDefinition[] RulesTextHighlights => rulesTextHighlights;
    public int Weight => weight;
    public int Value => value;
    public string[] BiomeIds => biomeIds;
    public int MinimumDepth => minimumDepth;
    public int MaximumDepth => maximumDepth;
    public CardEffectDefinition[] Effects => effects;

    public bool HasWeight => weight > 0;
    public bool HasValue => value > 0;

    /// <summary>Keeps newly added rules-text highlights visible when Unity initializes array entries with zeroed values.</summary>
    private void OnValidate()
    {
        if (rulesTextHighlights == null)
        {
            rulesTextHighlights = Array.Empty<CardTextHighlightDefinition>();
        }
        else
        {
            for (int i = 0; i < rulesTextHighlights.Length; i++)
            {
                rulesTextHighlights[i]?.EnsureValid();
            }
        }

        if (CreatureTagVocabulary.AppliesTo(cardType))
        {
            tags = CreatureTagVocabulary.Normalize(tags);
        }
    }

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

/// <summary>Defines the closed gameplay vocabulary available to creature and creature-like Apex cards.</summary>
public static class CreatureTagVocabulary
{
    public const string Fish = "Fish";
    public const string Predator = "Predator";
    public const string Schooling = "Schooling";
    public const string Small = "Small";
    public const string Heavy = "Heavy";
    public const string Anchored = "Anchored";
    public const string Armored = "Armored";
    public const string Elusive = "Elusive";

    public static readonly string[] All =
    {
        Fish,
        Predator,
        Schooling,
        Small,
        Heavy,
        Anchored,
        Armored,
        Elusive
    };

    /// <summary>Reports whether a card type uses the controlled creature-tag vocabulary.</summary>
    public static bool AppliesTo(CardType cardType)
    {
        return cardType == CardType.Creature || cardType == CardType.ApexEncounter;
    }

    /// <summary>Returns supported tags once each and in canonical display order.</summary>
    public static string[] Normalize(string[] authoredTags)
    {
        if (authoredTags == null || authoredTags.Length == 0)
        {
            return Array.Empty<string>();
        }

        List<string> normalized = new List<string>(All.Length);
        for (int allowedIndex = 0; allowedIndex < All.Length; allowedIndex++)
        {
            for (int authoredIndex = 0; authoredIndex < authoredTags.Length; authoredIndex++)
            {
                if (string.Equals(All[allowedIndex], authoredTags[authoredIndex], StringComparison.OrdinalIgnoreCase))
                {
                    normalized.Add(All[allowedIndex]);
                    break;
                }
            }
        }

        return normalized.ToArray();
    }
}

[Serializable]
public sealed class CardTextHighlightDefinition
{
    private static readonly Color DefaultColor = new Color(0.62f, 0.25f, 0.21f, 1f);
    private const int DefaultSizePercent = 120;

    [SerializeField] private string text;
    [ColorUsage(false)]
    [SerializeField] private Color color = new Color(0.62f, 0.25f, 0.21f, 1f);
    [Range(50, 200)]
    [SerializeField] private int sizePercent = DefaultSizePercent;
    [SerializeField] private bool matchWholeWord = true;

    public string Text => text;
    public Color Color => GetVisibleColor();
    public int SizePercent => sizePercent <= 0 ? DefaultSizePercent : Mathf.Clamp(sizePercent, 50, 200);
    public bool MatchWholeWord => matchWholeWord;

    /// <summary>Repairs zero-initialized serialized values created when a new catalog array entry is added.</summary>
    public void EnsureValid()
    {
        if (color == default)
        {
            color = DefaultColor;
        }
        else
        {
            color.a = 1f;
        }

        sizePercent = SizePercent;
    }

    /// <summary>Returns an opaque highlight color so matched text cannot disappear through alpha.</summary>
    private Color GetVisibleColor()
    {
        Color visibleColor = color == default ? DefaultColor : color;
        visibleColor.a = 1f;
        return visibleColor;
    }
}

public enum CardArtworkLayout
{
    MaskedRegion = 0,
    FullCardOverlay = 1
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
    ModifyCaughtCard,
    RevealEncounterInformation,
    ModifyDescendDistance,
    ModifyNextEncounterDepth,
    ModifyTemporaryCapacity,
    RiskForReward,
    RewardOverloadedDescend
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

public enum CaughtCardTargetMode
{
    PreviousMatching,
    NextMatching,
    FirstMatching,
    LastMatching
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
    [SerializeField] private CaughtCardTargetMode caughtCardTargetMode;
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
    public CaughtCardTargetMode CaughtCardTargetMode => caughtCardTargetMode;
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

        if (target == CardEffectTarget.SpecificCaughtCard)
        {
            summary.Append(" | Select: ");
            summary.Append(caughtCardTargetMode);
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
