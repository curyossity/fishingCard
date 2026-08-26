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

    [Header("Run State")]
    // These fields are serialized so the prototype run state can be inspected during Play Mode.
    [SerializeField] private bool runActive;
    [SerializeField] private string currentBiomeId;
    [SerializeField] private int currentDepth;
    [SerializeField] private int lineCapacity;
    [SerializeField] private CardDefinition currentEncounter;
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
        catchChain = Array.Empty<CardDefinition>();
        currentBiomeId = startingBiomeId;
        currentDepth = Mathf.Max(0, startingDepth);
        techniqueDiscardPile = Array.Empty<CardDefinition>();
        techniqueDrawPile = BuildShuffledDeck(startingTechniqueDeck);
        techniqueHand = DrawCards(techniqueDrawPile, startingHandSize, out techniqueDrawPile);

        // Revealing the first encounter gives the player an immediate decision point.
        currentEncounter = RevealFirstEncounter();

        RefreshViews();
        Debug.Log(BuildStartRunSummary(seed), this);
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
            currentEncounterView.SetCard(currentEncounter);
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
}
