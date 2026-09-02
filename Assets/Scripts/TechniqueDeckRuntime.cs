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
    public void Refill(int handSize, System.Random random)
    {
        while (hand.Length < handSize)
        {
            if (drawPile.Length == 0)
            {
                if (discardPile.Length == 0)
                {
                    return;
                }

                drawPile = CreateShuffledPile(discardPile, random);
                discardPile = Array.Empty<CardDefinition>();
            }

            CardDefinition[] drawnCards = DrawCards(drawPile, 1, out drawPile);

            if (drawnCards.Length == 0)
            {
                return;
            }

            hand = AppendCard(hand, drawnCards[0]);
        }
    }

    /// <summary>
    /// Returns a Technique card from a hand position without changing any pile.
    /// </summary>
    public bool TryGetHandCard(int handIndex, out CardDefinition card, out string validationMessage)
    {
        card = null;
        validationMessage = string.Empty;

        if (handIndex < 0 || handIndex >= hand.Length)
        {
            validationMessage = $"Technique hand index is out of range: {handIndex}.";
            return false;
        }

        card = hand[handIndex];

        if (card == null)
        {
            validationMessage = $"Technique hand slot {handIndex} is empty.";
            return false;
        }

        if (card.CardType != CardType.Technique)
        {
            validationMessage = $"Card is not a Technique card: {card.DisplayName}.";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Moves a validated Technique card from hand to discard, then refills the hand.
    /// </summary>
    public bool TryUseCard(
        int handIndex,
        int handSize,
        System.Random random,
        out CardDefinition usedCard,
        out string validationMessage)
    {
        if (!TryGetHandCard(handIndex, out usedCard, out validationMessage))
        {
            return false;
        }

        hand = RemoveCardAt(hand, handIndex);
        discardPile = AppendCard(discardPile, usedCard);
        Refill(handSize, random);
        return true;
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

        return CreateShuffledPile(cards.ToArray(), random);
    }

    /// <summary>
    /// Returns a separately shuffled copy of a Technique pile using Fisher-Yates.
    /// </summary>
    private static CardDefinition[] CreateShuffledPile(CardDefinition[] sourcePile, System.Random random)
    {
        CardDefinition[] source = sourcePile ?? Array.Empty<CardDefinition>();
        CardDefinition[] shuffled = new CardDefinition[source.Length];

        for (int i = 0; i < source.Length; i++)
        {
            shuffled[i] = source[i];
        }

        // Fixed seeds remain deterministic while random seeds vary the order between runs.
        for (int i = shuffled.Length - 1; i > 0; i--)
        {
            int swapIndex = random.Next(i + 1);
            CardDefinition temp = shuffled[i];
            shuffled[i] = shuffled[swapIndex];
            shuffled[swapIndex] = temp;
        }

        return shuffled;
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

    /// <summary>
    /// Returns a new card array without the card at the requested hand index.
    /// </summary>
    private static CardDefinition[] RemoveCardAt(CardDefinition[] cards, int removeIndex)
    {
        CardDefinition[] result = new CardDefinition[cards.Length - 1];
        int resultIndex = 0;

        for (int i = 0; i < cards.Length; i++)
        {
            if (i == removeIndex)
            {
                continue;
            }

            result[resultIndex] = cards[i];
            resultIndex++;
        }

        return result;
    }
}
