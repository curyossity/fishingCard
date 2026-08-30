using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class TechniqueDeckRuntime
{
    [SerializeField] private CardDefinition[] hand = Array.Empty<CardDefinition>();
    [SerializeField] private CardDefinition[] drawPile = Array.Empty<CardDefinition>();
    [SerializeField] private CardDefinition[] discardPile = Array.Empty<CardDefinition>();

    public CardDefinition[] Hand => hand;
    public CardDefinition[] DrawPile => drawPile;
    public CardDefinition[] DiscardPile => discardPile;

    /// <summary>
    /// Creates, shuffles, and draws from the configured starting Technique deck.
    /// </summary>
    public void Initialize(CardDefinition[] startingDeck, int startingHandSize, System.Random random, Action<string> warningLogger)
    {
        discardPile = Array.Empty<CardDefinition>();
        drawPile = BuildShuffledDeck(startingDeck, random, warningLogger);
        hand = DrawCards(drawPile, startingHandSize, out drawPile);
    }

    /// <summary>
    /// Refills the Technique hand up to the requested hand size when cards are available.
    /// </summary>
    public void Refill(int handSize)
    {
        int missingCards = Math.Max(0, handSize - hand.Length);

        if (missingCards == 0)
        {
            return;
        }

        CardDefinition[] drawnCards = DrawCards(drawPile, missingCards, out drawPile);

        for (int i = 0; i < drawnCards.Length; i++)
        {
            hand = AppendCard(hand, drawnCards[i]);
        }
    }

    /// <summary>
    /// Clears all Technique deck state for an inactive run.
    /// </summary>
    public void Reset()
    {
        hand = Array.Empty<CardDefinition>();
        drawPile = Array.Empty<CardDefinition>();
        discardPile = Array.Empty<CardDefinition>();
    }

    /// <summary>
    /// Creates a shuffled draw pile containing only valid Technique cards.
    /// </summary>
    private static CardDefinition[] BuildShuffledDeck(
        CardDefinition[] sourceDeck,
        System.Random random,
        Action<string> warningLogger)
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

                // Fundamental actions and encounters never enter the player's Technique deck.
                if (card.CardType != CardType.Technique)
                {
                    warningLogger?.Invoke($"Starting deck ignored non-technique card: {card.DisplayName}.");
                    continue;
                }

                cards.Add(card);
            }
        }

        // Fisher-Yates keeps fixed-seed runs deterministic while still varying random-seed runs.
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
    /// Draws cards from the front of a pile and returns the remaining pile.
    /// </summary>
    private static CardDefinition[] DrawCards(CardDefinition[] sourcePile, int count, out CardDefinition[] remainingPile)
    {
        if (sourcePile == null || sourcePile.Length == 0 || count <= 0)
        {
            remainingPile = sourcePile ?? Array.Empty<CardDefinition>();
            return Array.Empty<CardDefinition>();
        }

        int drawCount = Math.Min(count, sourcePile.Length);
        CardDefinition[] drawnCards = new CardDefinition[drawCount];
        remainingPile = new CardDefinition[sourcePile.Length - drawCount];

        // Discard and reshuffle rules are separate from the current front-of-pile draw behavior.
        for (int i = 0; i < drawCount; i++)
        {
            drawnCards[i] = sourcePile[i];
        }

        for (int i = drawCount; i < sourcePile.Length; i++)
        {
            remainingPile[i - drawCount] = sourcePile[i];
        }

        return drawnCards;
    }

    /// <summary>
    /// Returns a new card array with one card appended.
    /// </summary>
    private static CardDefinition[] AppendCard(CardDefinition[] cards, CardDefinition card)
    {
        CardDefinition[] source = cards ?? Array.Empty<CardDefinition>();
        CardDefinition[] result = new CardDefinition[source.Length + 1];

        for (int i = 0; i < source.Length; i++)
        {
            result[i] = source[i];
        }

        result[result.Length - 1] = card;
        return result;
    }
}
