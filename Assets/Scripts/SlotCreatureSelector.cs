using System;
using System.Collections.Generic;

/// <summary>Separates tag guarantees, requested rarity, nearest-tier fallback and weighted species selection.</summary>
public static class SlotCreatureSelector
{
    public readonly struct Selection
    {
        public CardDefinition Creature { get; }
        public CardRarity RequestedRarity { get; }
        public CardRarity? ResolvedRarity => Creature == null ? (CardRarity?)null : Creature.Rarity;
        public SlotMatch Match { get; }

        /// <summary>Records selection provenance without granting an attachment or landing points.</summary>
        internal Selection(CardDefinition creature, CardRarity requested, SlotMatch match)
        { Creature = creature; RequestedRarity = requested; Match = match; }
    }

    /// <summary>Builds a distinct biome pool, excluding Apex and locked creatures without normal-creature depth limits.</summary>
    public static CardDefinition[] EligiblePool(IEnumerable<CardDefinition> pool, string biomeId, Func<CardDefinition, bool> unlocked)
    {
        var result = new List<CardDefinition>();
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (CardDefinition card in pool ?? Array.Empty<CardDefinition>())
        {
            if (card == null || card.CardType != CardType.Creature || !card.IsAvailableInBiome(biomeId)
                || (unlocked != null ? !unlocked(card) : !string.IsNullOrWhiteSpace(card.RequiredCreatureUnlockId)) || result.Contains(card)
                || (int)card.Rarity < 0 || (int)card.Rarity > 3
                || !string.IsNullOrEmpty(card.UniqueId) && !ids.Add(card.UniqueId)) continue;
            result.Add(card);
        }
        return result.ToArray();
    }

    /// <summary>Selects only within the guaranteed tag and resolves rarity ties toward the lower tier.</summary>
    public static bool TrySelect(CardDefinition[] eligible, SlotMatch match, SlotSelectionSettings settings,
        System.Random random, out Selection selection, out string failure)
    {
        selection = default;
        failure = null;
        var matching = new List<CardDefinition>();
        foreach (CardDefinition card in eligible ?? Array.Empty<CardDefinition>())
            if (card != null && (match.GuaranteedTag == null || HasTag(card, match.GuaranteedTag))) matching.Add(card);
        if (matching.Count == 0)
        {
            failure = match.GuaranteedTag == null ? "No eligible creature in this biome."
                : "No eligible creature has the guaranteed " + match.GuaranteedTag + " tag.";
            return false;
        }
        CardRarity requested = settings.RollRarity(match.Kind, random);
        int nearest = -1, distance = int.MaxValue;
        foreach (CardDefinition card in matching)
        {
            int tier = (int)card.Rarity, currentDistance = Math.Abs(tier - (int)requested);
            if (currentDistance < distance || currentDistance == distance && tier < nearest)
            { nearest = tier; distance = currentDistance; }
        }
        var candidates = matching.FindAll(card => (int)card.Rarity == nearest);
        int total = 0;
        foreach (CardDefinition card in candidates)
            total = checked(total + (match.PreferredTag != null && HasTag(card, match.PreferredTag) ? settings.SecondaryTagWeight : 1));
        int ticket = random.Next(total);
        foreach (CardDefinition card in candidates)
        {
            ticket -= match.PreferredTag != null && HasTag(card, match.PreferredTag) ? settings.SecondaryTagWeight : 1;
            if (ticket < 0) { selection = new Selection(card, requested, match); return true; }
        }
        throw new InvalidOperationException("Species weights did not resolve a ticket.");
    }

    /// <summary>Matches authored tags without borrowing rig tag modifiers.</summary>
    public static bool HasTag(CardDefinition card, string tag)
    { return Array.Exists(card.Tags ?? Array.Empty<string>(), value => string.Equals(value, tag, StringComparison.OrdinalIgnoreCase)); }
}
