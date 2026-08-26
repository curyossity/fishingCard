using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public sealed class FishingRunController : MonoBehaviour
{
    [Header("Start Run Settings")]
    [SerializeField] private bool startRunOnAwake = true;
    [Min(0)]
    [SerializeField] private int startingLineCapacity = 10;
    [Min(0)]
    [SerializeField] private int startingDepth;
    [SerializeField] private string startingBiomeId = "Coastal";
    [Min(1)]
    [SerializeField] private int startingHandSize = 4;
    [SerializeField] private int randomSeed;
    [SerializeField] private bool useRandomSeed = true;

    [Header("Starting Cards")]
    [SerializeField] private CardDefinition[] startingTechniqueDeck = Array.Empty<CardDefinition>();
    [SerializeField] private CardDefinition[] encounterPool = Array.Empty<CardDefinition>();

    [Header("Optional Views")]
    [SerializeField] private CardView currentEncounterView;
    [SerializeField] private CardView[] techniqueHandViews = Array.Empty<CardView>();

    [Header("Debug Actions")]
    [SerializeField] private int debugTechniqueHandIndex;

    [Header("Run State")]
    // These fields are serialized so the prototype run state can be inspected during Play Mode.
    [SerializeField] private bool runActive;
    [SerializeField] private string currentBiomeId;
    [SerializeField] private int currentDepth;
    [SerializeField] private int lineCapacity;
    [SerializeField] private CardDefinition currentEncounter;
    [SerializeField] private EncounterState currentEncounterState;
    [SerializeField] private CardDefinition hookedEncounter;
    [SerializeField] private HookedEffectRecord[] hookedEffectRecords = Array.Empty<HookedEffectRecord>();
    [SerializeField] private CardDefinition[] catchChain = Array.Empty<CardDefinition>();
    [SerializeField] private CardDefinition[] techniqueHand = Array.Empty<CardDefinition>();
    [SerializeField] private CardDefinition[] techniqueDrawPile = Array.Empty<CardDefinition>();
    [SerializeField] private CardDefinition[] techniqueDiscardPile = Array.Empty<CardDefinition>();

    private System.Random random;

    public bool RunActive => runActive;
    public string CurrentBiomeId => currentBiomeId;
    public int CurrentDepth => currentDepth;
    public int LineCapacity => lineCapacity;
    public CardDefinition CurrentEncounter => currentEncounter;
    public EncounterState CurrentEncounterState => currentEncounterState;
    public CardDefinition HookedEncounter => hookedEncounter;
    public HookedEffectRecord[] HookedEffectRecords => hookedEffectRecords;
    public CardDefinition[] CatchChain => catchChain;
    public CardDefinition[] TechniqueHand => techniqueHand;
    public CardDefinition[] TechniqueDrawPile => techniqueDrawPile;
    public CardDefinition[] TechniqueDiscardPile => techniqueDiscardPile;

    /// <summary>
    /// Calculates the current Line Load from the cards attached to the Catch Chain.
    /// </summary>
    public int CurrentLineLoad
    {
        get
        {
            int load = 0;

            for (int i = 0; i < catchChain.Length; i++)
            {
                if (catchChain[i] != null)
                {
                    load += catchChain[i].Weight;
                }
            }

            return load;
        }
    }

    /// <summary>
    /// Starts the run automatically when the scene loads if configured to do so.
    /// </summary>
    private void Awake()
    {
        if (startRunOnAwake)
        {
            StartRun();
        }
    }

    /// <summary>
    /// Initializes the run state: capacity, deck, hand, biome, depth, and first encounter.
    /// </summary>
    [ContextMenu("Run/Start Run")]
    public void StartRun()
    {
        int seed = useRandomSeed ? Environment.TickCount : randomSeed;
        random = new System.Random(seed);

        // run starts from a clean line: no catches, no discard pile, and a fresh technique deck.
        runActive = true;
        lineCapacity = startingLineCapacity;
        currentEncounterState = EncounterState.None;
        hookedEncounter = null;
        hookedEffectRecords = Array.Empty<HookedEffectRecord>();
        catchChain = Array.Empty<CardDefinition>();
        currentBiomeId = startingBiomeId;
        currentDepth = Mathf.Max(0, startingDepth);
        techniqueDiscardPile = Array.Empty<CardDefinition>();
        techniqueDrawPile = BuildShuffledDeck(startingTechniqueDeck);
        techniqueHand = DrawCards(techniqueDrawPile, startingHandSize, out techniqueDrawPile);

        // Revealing the first encounter gives the player an immediate decision point.
        currentEncounter = RevealFirstEncounter();
        UpdateEncounterReactionState();

        RefreshViews();
        Debug.Log(BuildStartRunSummary(seed), this);
    }

    /// <summary>
    /// Applies a Technique card from the current hand to the Hooked encounter reaction window.
    /// </summary>
    public bool TryUseTechniqueCard(int handIndex)
    {
        if (!runActive)
        {
            Debug.LogWarning("Cannot use a Technique card before a run has started.", this);
            return false;
        }

        if (hookedEncounter == null)
        {
            Debug.LogWarning("There is no Hooked encounter for a Technique card to affect.", this);
            return false;
        }

        if (handIndex < 0 || handIndex >= techniqueHand.Length)
        {
            Debug.LogWarning($"Technique hand index is out of range: {handIndex}.", this);
            return false;
        }

        CardDefinition techniqueCard = techniqueHand[handIndex];

        if (techniqueCard == null)
        {
            Debug.LogWarning($"Technique hand slot {handIndex} is empty.", this);
            return false;
        }

        if (techniqueCard.CardType != CardType.Technique)
        {
            Debug.LogWarning($"Card is not a Technique card: {techniqueCard.DisplayName}.", this);
            return false;
        }

        int addedEffects = AddHookedTechniqueEffects(techniqueCard);
        Debug.Log($"Technique card used on Hooked encounter: {techniqueCard.DisplayName}. Tracked effects added: {addedEffects}.", this);
        return true;
    }

    /// <summary>
    /// Inspector context-menu wrapper for applying a Technique card by hand index.
    /// </summary>
    [ContextMenu("Run/Use Debug Technique Card")]
    private void UseDebugTechniqueCard()
    {
        TryUseTechniqueCard(debugTechniqueHandIndex);
    }

    /// <summary>
    /// Selects a valid first encounter from the configured pool for the current biome and depth.
    /// </summary>
    private CardDefinition RevealFirstEncounter()
    {
        List<CardDefinition> candidates = new List<CardDefinition>();

        for (int i = 0; i < encounterPool.Length; i++)
        {
            CardDefinition card = encounterPool[i];

            // Core actions and Technique cards do not belong in the encounter pool.
            if (card == null || !IsEncounterCard(card))
            {
                continue;
            }

            // Depth and biome filters are data-driven so future pools can change without new scripts.
            if (card.IsAvailableInBiome(currentBiomeId) && card.IsAvailableAtDepth(currentDepth))
            {
                candidates.Add(card);
            }
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning($"No valid encounter found for biome '{currentBiomeId}' at depth {currentDepth}.", this);
            return null;
        }

        return candidates[random.Next(candidates.Count)];
    }

    /// <summary>
    /// Updates encounter state and Hooked tracking after an encounter is revealed or replaced.
    /// </summary>
    private void UpdateEncounterReactionState()
    {
        if (currentEncounter == null)
        {
            hookedEncounter = null;
            currentEncounterState = EncounterState.None;
            RebuildHookedEffectRecords();
            return;
        }

        if (IsCatchableEncounter(currentEncounter))
        {
            hookedEncounter = currentEncounter;
            currentEncounterState = EncounterState.Hooked;
            RebuildHookedEffectRecords();
            return;
        }

        hookedEncounter = null;
        currentEncounterState = EncounterState.Encountered;

        RebuildHookedEffectRecords();
    }

    /// <summary>
    /// Rebuilds the effect records that are currently attached to the Hooked reaction window.
    /// </summary>
    private void RebuildHookedEffectRecords()
    {
        if (hookedEncounter == null)
        {
            hookedEffectRecords = Array.Empty<HookedEffectRecord>();
            return;
        }

        List<HookedEffectRecord> records = new List<HookedEffectRecord>();
        AddRelevantEffectRecords(records, hookedEncounter, HookedEffectSource.HookedEncounter);
        hookedEffectRecords = records.ToArray();
    }

    /// <summary>
    /// Adds relevant Technique effects to the active Hooked reaction records.
    /// </summary>
    private int AddHookedTechniqueEffects(CardDefinition techniqueCard)
    {
        List<HookedEffectRecord> records = new List<HookedEffectRecord>(hookedEffectRecords);
        int originalCount = records.Count;

        AddRelevantEffectRecords(records, techniqueCard, HookedEffectSource.TechniqueCard);
        hookedEffectRecords = records.ToArray();

        return records.Count - originalCount;
    }

    /// <summary>
    /// Adds effects from a source card when they can affect the current Hooked encounter.
    /// </summary>
    private void AddRelevantEffectRecords(List<HookedEffectRecord> records, CardDefinition sourceCard, HookedEffectSource sourceType)
    {
        if (sourceCard == null || sourceCard.Effects == null)
        {
            return;
        }

        for (int i = 0; i < sourceCard.Effects.Length; i++)
        {
            CardEffectDefinition effect = sourceCard.Effects[i];

            if (DoesEffectAffectHookedEncounter(effect, hookedEncounter))
            {
                records.Add(new HookedEffectRecord(sourceCard, effect, sourceType));
            }
        }
    }

    /// <summary>
    /// Checks whether an effect is relevant during the Hooked encounter reaction window.
    /// </summary>
    private static bool DoesEffectAffectHookedEncounter(CardEffectDefinition effect, CardDefinition targetEncounter)
    {
        if (effect == null || targetEncounter == null)
        {
            return false;
        }

        bool targetsHookedCard = effect.Target == CardEffectTarget.HookedEncounter
            || effect.Target == CardEffectTarget.CurrentEncounter
            || effect.Target == CardEffectTarget.NextDescend;

        if (!targetsHookedCard)
        {
            return false;
        }

        return RequiredTagsMatch(effect.RequiredTags, targetEncounter);
    }

    /// <summary>
    /// Checks whether all required effect tags are present on the target card's tags or card type.
    /// </summary>
    private static bool RequiredTagsMatch(string[] requiredTags, CardDefinition targetCard)
    {
        if (requiredTags == null || requiredTags.Length == 0)
        {
            return true;
        }

        if (targetCard == null)
        {
            return false;
        }

        for (int i = 0; i < requiredTags.Length; i++)
        {
            if (string.Equals(requiredTags[i], targetCard.CardType.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!HasTag(targetCard.Tags, requiredTags[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks whether a tag list contains a requested tag, ignoring case.
    /// </summary>
    private static bool HasTag(string[] tags, string requiredTag)
    {
        if (tags == null || tags.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < tags.Length; i++)
        {
            if (string.Equals(requiredTag, tags[i], StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Creates a shuffled runtime draw pile from the configured starting Technique deck.
    /// </summary>
    private CardDefinition[] BuildShuffledDeck(CardDefinition[] sourceDeck)
    {
        List<CardDefinition> cards = new List<CardDefinition>();

        if (sourceDeck != null)
        {
            for (int i = 0; i < sourceDeck.Length; i++)
            {
                CardDefinition card = sourceDeck[i];

                if (card == null)
                {
                    continue;
                }

                // The GDD keeps fundamental fishing actions and encounters out of the player deck.
                if (card.CardType != CardType.Technique)
                {
                    Debug.LogWarning($"Starting deck ignored non-technique card: {card.DisplayName}.", this);
                    continue;
                }

                cards.Add(card);
            }
        }

        // Fisher-Yates keeps the prototype deterministic when a fixed seed is used.
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int swapIndex = random.Next(i + 1);
            CardDefinition temp = cards[i];
            cards[i] = cards[swapIndex];
            cards[swapIndex] = temp;
        }

        return cards.ToArray();
    }

    /// <summary>
    /// Draws cards from the front of the draw pile and returns the remaining pile.
    /// </summary>
    private static CardDefinition[] DrawCards(CardDefinition[] drawPile, int count, out CardDefinition[] remainingDrawPile)
    {
        if (drawPile == null || drawPile.Length == 0 || count <= 0)
        {
            remainingDrawPile = drawPile ?? Array.Empty<CardDefinition>();
            return Array.Empty<CardDefinition>();
        }

        int drawCount = Mathf.Min(count, drawPile.Length);
        CardDefinition[] hand = new CardDefinition[drawCount];
        remainingDrawPile = new CardDefinition[drawPile.Length - drawCount];

        // This is only the starting draw. Discard, refill, and reshuffle rules come later.
        for (int i = 0; i < drawCount; i++)
        {
            hand[i] = drawPile[i];
        }

        for (int i = drawCount; i < drawPile.Length; i++)
        {
            remainingDrawPile[i - drawCount] = drawPile[i];
        }

        return hand;
    }

    /// <summary>
    /// Updates optional card views with the current encounter and starting hand.
    /// </summary>
    private void RefreshViews()
    {
        // View references are optional so the run model can be tested before the full UI exists.
        if (currentEncounterView != null)
        {
            currentEncounterView.SetCard(currentEncounter, currentEncounterState);
        }

        if (techniqueHandViews == null)
        {
            return;
        }

        for (int i = 0; i < techniqueHandViews.Length; i++)
        {
            if (techniqueHandViews[i] == null)
            {
                continue;
            }

            CardDefinition card = i < techniqueHand.Length ? techniqueHand[i] : null;
            techniqueHandViews[i].SetCard(card);
        }
    }

    /// <summary>
    /// Builds the startup log that confirms the initialized run state in the Unity Console.
    /// </summary>
    private string BuildStartRunSummary(int seed)
    {
        StringBuilder summary = new StringBuilder();
        summary.AppendLine("Run started.");
        summary.AppendLine($"Seed: {seed}");
        summary.AppendLine($"Biome: {currentBiomeId}");
        summary.AppendLine($"Depth: {currentDepth}");
        summary.AppendLine($"Line Capacity: {lineCapacity}");
        summary.AppendLine($"Catch Chain: {catchChain.Length} cards");
        summary.AppendLine($"Technique Hand: {techniqueHand.Length} cards");
        summary.AppendLine($"Technique Draw Pile: {techniqueDrawPile.Length} cards");
        summary.Append("First Encounter: ");
        summary.Append(currentEncounter == null ? "none" : currentEncounter.DisplayName);
        summary.AppendLine();
        summary.Append("Encounter State: ");
        summary.Append(currentEncounterState);
        return summary.ToString();
    }

    /// <summary>
    /// Checks whether a card type belongs to the encounter-facing side of the game.
    /// </summary>
    private static bool IsEncounterCard(CardDefinition card)
    {
        // Apex uses the same encounter language, but later systems will keep it out of normal pools.
        return card.CardType == CardType.Creature
            || card.CardType == CardType.Treasure
            || card.CardType == CardType.Hazard
            || card.CardType == CardType.Environment
            || card.CardType == CardType.Opportunity
            || card.CardType == CardType.ApexEncounter
            || card.CardType == CardType.Encounter;
    }

    /// <summary>
    /// Checks whether an encounter should become Hooked during the reaction window.
    /// </summary>
    private static bool IsCatchableEncounter(CardDefinition card)
    {
        return card != null
            && (card.CardType == CardType.Creature || card.CardType == CardType.ApexEncounter);
    }
}

public enum HookedEffectSource
{
    HookedEncounter,
    TechniqueCard
}

[Serializable]
public sealed class HookedEffectRecord
{
    [SerializeField] private CardDefinition sourceCard;
    [SerializeField] private CardEffectDefinition effect;
    [SerializeField] private HookedEffectSource sourceType;

    public CardDefinition SourceCard => sourceCard;
    public CardEffectDefinition Effect => effect;
    public HookedEffectSource SourceType => sourceType;

    /// <summary>
    /// Creates an empty record for Unity serialization.
    /// </summary>
    public HookedEffectRecord()
    {
    }

    /// <summary>
    /// Creates a runtime record for an effect that can influence the Hooked encounter.
    /// </summary>
    public HookedEffectRecord(CardDefinition sourceCard, CardEffectDefinition effect, HookedEffectSource sourceType)
    {
        this.sourceCard = sourceCard;
        this.effect = effect;
        this.sourceType = sourceType;
    }
}
