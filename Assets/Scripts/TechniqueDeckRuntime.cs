using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class TechniqueDeckRuntime
{
    public const int HandLimit = 4;
    [SerializeField] private CardDefinition resolvingCard;
    [SerializeField] private int resolvingOccurrenceId;
    [SerializeField] private int resolvingOriginalHandIndex = -1;
    public int ResolvingOriginalHandIndex => resolvingOriginalHandIndex;
    [SerializeField] private long handVersion;
    public CardDefinition ResolvingCard => resolvingCard;
    public int ResolvingOccurrenceId => resolvingOccurrenceId;
    public long HandVersion => handVersion;
    [SerializeField] private CardDefinition[] hand = Array.Empty<CardDefinition>();
    [SerializeField] private CardDefinition[] drawPile = Array.Empty<CardDefinition>();
    [SerializeField] private CardDefinition[] discardPile = Array.Empty<CardDefinition>();

    [SerializeField] private int[] discardOccurrenceIds = Array.Empty<int>();
    [SerializeField] private int nextDiscardOccurrenceId = 1;
    [SerializeField] private int lastDiscardOccurrenceId;
    public int LastDiscardOccurrenceId => lastDiscardOccurrenceId;
    public CardDefinition[] Hand => hand;
    public CardDefinition[] DrawPile => drawPile;
    public CardDefinition[] DiscardPile => discardPile;

    /// <summary>
    /// Creates, shuffles, and draws from the configured starting Technique deck.
    /// </summary>
    public void Initialize(CardDefinition[] startingDeck, int startingHandSize, System.Random random, Action<string> warningLogger)
    {
        discardPile = Array.Empty<CardDefinition>();
        discardOccurrenceIds = Array.Empty<int>();
        resolvingCard = null; resolvingOccurrenceId = 0; resolvingOriginalHandIndex = -1;
        lastDiscardOccurrenceId = 0;
        drawPile = BuildShuffledDeck(startingDeck, random, warningLogger);
        hand = DrawCards(drawPile, Math.Min(HandLimit, Math.Max(0, startingHandSize)), out drawPile);
        handVersion++;
    }

    /// <summary>
    /// Refills the Technique hand up to the requested hand size when cards are available.
    /// </summary>
    public void Refill(int handSize, System.Random random)
    {
        handSize = Math.Min(HandLimit, Math.Max(0, handSize));
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
                discardOccurrenceIds = Array.Empty<int>();
            }

            CardDefinition[] drawnCards = DrawCards(drawPile, 1, out drawPile);

            if (drawnCards.Length == 0)
            {
                return;
            }

            hand = AppendCard(hand, drawnCards[0]);
            handVersion++;
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

        if (card.CardType != CardType.Technique || card.IsBackupTechnique)
        {
            validationMessage = $"Card is not a Technique card: {card.DisplayName}.";
            return false;
        }

        return true;
    }

    /// <summary>Removes one physical card from hand into resolving ownership, reserving its future discard identity.</summary>
    public bool TryBeginPlay(int handIndex, out CardDefinition usedCard, out string validationMessage)
    {
        usedCard = null;
        validationMessage = "Finish the resolving Technique first";
        if (resolvingCard != null || !TryGetHandCard(handIndex, out usedCard, out validationMessage)) return false;
        EnsureDiscardOccurrenceIds();
        resolvingCard = usedCard;
        resolvingOriginalHandIndex = handIndex;
        resolvingOccurrenceId = nextDiscardOccurrenceId++;
        hand = RemoveCardAt(hand, handIndex);
        handVersion++;
        return true;
    }

    /// <summary>Discards the reserved physical occurrence only after its effect has finished; stale completions do nothing.</summary>
    public bool TryCompletePlay(int expectedOccurrenceId)
    {
        if (resolvingCard == null || resolvingOccurrenceId != expectedOccurrenceId || expectedOccurrenceId <= 0) return false;
        AppendDiscard(resolvingCard, resolvingOccurrenceId);
        resolvingCard = null;
        resolvingOriginalHandIndex = -1;
        resolvingOccurrenceId = 0;
        return true;
    }

    /// <summary>Returns the stable identity at a selected discard index without moving it.</summary>
    public int GetDiscardOccurrenceId(int index)
    {
        EnsureDiscardOccurrenceIds();
        return index >= 0 && index < discardOccurrenceIds.Length ? discardOccurrenceIds[index] : 0;
    }

    /// <summary>Returns one selected discard occurrence only when it fits the four-card hand.</summary>
    public bool TryRecoverDiscardedTechnique(int discardIndex)
    {
        if (hand.Length >= HandLimit || discardIndex < 0 || discardIndex >= discardPile.Length
            || discardPile[discardIndex]?.CardType != CardType.Technique)
        {
            return false;
        }

        EnsureDiscardOccurrenceIds();
        hand = AppendCard(hand, discardPile[discardIndex]);
        handVersion++;
        RemoveDiscardAt(discardIndex);
        return true;
    }

    /// <summary>Discards a selected hand occurrence and draws one replacement without increasing hand size.</summary>
    public bool TryRedrawTechnique(int handIndex, System.Random random)
    {
        if (!TryGetHandCard(handIndex, out CardDefinition chosen, out _)) return false;
        hand = RemoveCardAt(hand, handIndex);
        handVersion++;
        AppendDiscard(chosen);
        if (drawPile.Length == 0)
        {
            drawPile = CreateShuffledPile(discardPile, random);
            discardPile = Array.Empty<CardDefinition>();
            discardOccurrenceIds = Array.Empty<int>();
        }
        CardDefinition[] replacement = DrawCards(drawPile, 1, out drawPile);
        if (replacement.Length > 0) hand = AppendCard(hand, replacement[0]);
        return true;
    }

    /// <summary>Discards one exact hand occurrence without playing it or drawing a replacement.</summary>
    public bool TryDiscardTechnique(int handIndex)
    {
        if (!TryGetHandCard(handIndex, out CardDefinition card, out _)) return false;
        hand = RemoveCardAt(hand, handIndex);
        handVersion++;
        AppendDiscard(card);
        return true;
    }

    /// <summary>Draws only the requested number that fits the supplied capacity, using the normal reshuffle rule.</summary>
    public int DrawTechniques(int count, int capacity, System.Random random)
    {
        int drawn = 0;
        int available = Math.Min(Math.Max(0, count), Math.Max(0, Math.Min(HandLimit, capacity) - hand.Length));
        while (drawn < available)
        {
            if (drawPile.Length == 0)
            {
                if (discardPile.Length == 0) break;
                drawPile = CreateShuffledPile(discardPile, random);
                discardPile = Array.Empty<CardDefinition>();
                discardOccurrenceIds = Array.Empty<int>();
            }
            CardDefinition[] cards = DrawCards(drawPile, 1, out drawPile);
            if (cards.Length == 0) break;
            hand = AppendCard(hand, cards[0]);
            handVersion++;
            drawn++;
        }
        return drawn;
    }

    /// <summary>Validates both existing occurrences before exchanging them without playing, drawing, or changing hand size.</summary>
    public bool TryExchangeTechniques(int handIndex, int discardIndex)
    {
        if (!TryGetHandCard(handIndex, out CardDefinition handCard, out _)
            || discardIndex < 0 || discardIndex >= discardPile.Length
            || discardPile[discardIndex]?.CardType != CardType.Technique) return false;
        EnsureDiscardOccurrenceIds();
        CardDefinition discardedCard = discardPile[discardIndex];
        CardDefinition[] updatedHand = (CardDefinition[])hand.Clone();
        updatedHand[handIndex] = discardedCard;
        RemoveDiscardAt(discardIndex);
        AppendDiscard(handCard);
        hand = updatedHand;
        handVersion++;
        return true;
    }

    /// <summary>Checks that the exact played discard occurrence still exists; identical definitions are insufficient.</summary>
    public bool HasDiscardedTechniqueOccurrence(int occurrenceId, CardDefinition expected)
    {
        EnsureDiscardOccurrenceIds();
        int index = Array.IndexOf(discardOccurrenceIds, occurrenceId);
        return index >= 0 && discardPile[index] == expected && expected?.CardType == CardType.Technique;
    }

    /// <summary>Returns only the played occurrence, subject to available capacity, without recovering another copy.</summary>
    public bool TryRecoverPlayedTechnique(int occurrenceId, CardDefinition expected, int capacity)
    {
        if (hand.Length >= Math.Min(HandLimit, capacity) || !HasDiscardedTechniqueOccurrence(occurrenceId, expected)) return false;
        return TryRecoverDiscardedTechnique(Array.IndexOf(discardOccurrenceIds, occurrenceId));
    }

    /// <summary>Gives previously serialized or fixture discard entries stable IDs before any identity-sensitive operation.</summary>
    private void EnsureDiscardOccurrenceIds()
    {
        if (discardOccurrenceIds.Length == discardPile.Length) return;
        discardOccurrenceIds = new int[discardPile.Length];
        for (int i = 0; i < discardOccurrenceIds.Length; i++) discardOccurrenceIds[i] = nextDiscardOccurrenceId++;
    }

    /// <summary>Assigns a fresh discard occurrence whenever a technique enters the pile.</summary>
    private void AppendDiscard(CardDefinition card, int reservedOccurrenceId = 0)
    {
        EnsureDiscardOccurrenceIds();
        discardPile = AppendCard(discardPile, card);
        int[] updated = new int[discardOccurrenceIds.Length + 1];
        Array.Copy(discardOccurrenceIds, updated, discardOccurrenceIds.Length);
        lastDiscardOccurrenceId = reservedOccurrenceId > 0 ? reservedOccurrenceId : nextDiscardOccurrenceId++;
        updated[updated.Length - 1] = lastDiscardOccurrenceId;
        discardOccurrenceIds = updated;
    }

    /// <summary>Removes one definition and its exact discard identity together.</summary>
    private void RemoveDiscardAt(int index)
    {
        EnsureDiscardOccurrenceIds();
        discardPile = RemoveCardAt(discardPile, index);
        List<int> ids = new List<int>(discardOccurrenceIds);
        ids.RemoveAt(index);
        discardOccurrenceIds = ids.ToArray();
    }

    /// <summary>
    /// Clears all Technique deck state for an inactive run.
    /// </summary>
    public void Reset()
    {
        resolvingCard = null; resolvingOccurrenceId = 0; resolvingOriginalHandIndex = -1; lastDiscardOccurrenceId = 0;
        hand = Array.Empty<CardDefinition>();
        handVersion++;
        drawPile = Array.Empty<CardDefinition>();
        discardPile = Array.Empty<CardDefinition>();
        discardOccurrenceIds = Array.Empty<int>();
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
                if (card.CardType != CardType.Technique || card.IsBackupTechnique)
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
